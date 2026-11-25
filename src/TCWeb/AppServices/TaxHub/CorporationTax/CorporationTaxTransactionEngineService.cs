using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.Audit;
using TradeControl.Tax.UK.Adapters.Submission.TransactionEngine;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.Company.ContractInfrastructure;
using TradeControl.Tax.UK.Hmrc.CorporationTax.Submission.V2026;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.Authorization;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal enum CorporationTaxTransactionEngineMode
{
    Disabled,
    DevelopmentFake,
    Production
}

internal sealed class CorporationTaxTransactionEngineOptions
{
    internal const string SectionName = "TaxHub:CorporationTax:TransactionEngine";
    public CorporationTaxTransactionEngineMode Mode { get; set; }
        = CorporationTaxTransactionEngineMode.Disabled;
    public bool FamilyGateEnabled { get; set; }
    public string VendorId { get; set; } = string.Empty;
    public string Product { get; set; } = "Trade Control";
    public string ProductVersion { get; set; } = "1";
    public int TimeoutSeconds { get; set; } = 180;

    internal bool IsProductionEnabled =>
        Mode == CorporationTaxTransactionEngineMode.Production && FamilyGateEnabled;
    internal bool IsDevelopmentFake =>
        Mode == CorporationTaxTransactionEngineMode.DevelopmentFake;
    internal bool IsEnabled => IsDevelopmentFake || IsProductionEnabled;
}

/// <summary>
/// Ephemeral live Government Gateway credentials. The caller supplies them only at
/// submit or explicit recovery time; they are never retained with workflow state.
/// </summary>
internal sealed class CorporationTaxGatewayCredentials(string senderId, string password)
{
    internal string SenderId { get; } = senderId;
    internal string Password { get; } = password;

    public override string ToString() => nameof(CorporationTaxGatewayCredentials);
}

internal sealed record CorporationTaxTransactionStatus(
    string ConversationReference,
    string TransactionId,
    string ServiceArtifactSha256,
    string WireSha256,
    string Stage,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? PollNotBeforeUtc,
    string? Outcome,
    bool OutcomeAvailable,
    bool ReceiptAvailable,
    string? OutcomeSha256,
    string SupportReference,
    string AuthorityMode,
    bool ProductionEnabled);

internal sealed record CorporationTaxTransactionOutcome(
    byte[] Content,
    string Sha256,
    string FileName,
    bool ContainsReceipt);

internal interface ICorporationTaxTransactionEngineService
{
    Task<CorporationTaxTransactionStatus?> GetAsync(ClaimsPrincipal principal,
        string approvalReference, string logicalFilingIdentity,
        CancellationToken cancellationToken = default);

    Task<CorporationTaxTransactionStatus?> GetLatestAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);

    Task<CorporationTaxTransactionOutcome> ReadOutcomeAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken = default);

    Task<CorporationTaxTransactionStatus> SubmitApprovedOnceAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation, string preparationReference,
        string logicalFilingIdentity, CorporationTaxGatewayCredentials credentials,
        CancellationToken cancellationToken = default);

    Task<CorporationTaxTransactionStatus> RecoverOnceAsync(ClaimsPrincipal principal,
        string conversationReference, CorporationTaxGatewayCredentials credentials,
        CancellationToken cancellationToken = default);

    Task<CorporationTaxTransactionStatus> PollOnceAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken = default);

    Task<CorporationTaxTransactionStatus> DeleteOnceAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken = default);
}

internal sealed class CorporationTaxTransactionEngineService(
    ICorporationTaxApprovalService approvals,
    ICorporationTaxApprovalStore approvalStore,
    ICorporationTaxPreparationService preparations,
    ICorporationTaxPreparationStore preparationStore,
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    ICompaniesHouseProtectedContentStore protectedContent,
    ITransactionEngineConversationStore conversationStore,
    ISubmissionContentStore transactionContent,
    IHttpClientFactory httpClientFactory,
    IOptions<CorporationTaxTransactionEngineOptions> options,
    TimeProvider timeProvider) : ICorporationTaxTransactionEngineService
{
    internal const string HttpClientName = "CorporationTaxTransactionEngine";
    private readonly CorporationTaxTransactionEngineOptions _options = options.Value;

    public async Task<CorporationTaxTransactionStatus?> GetAsync(ClaimsPrincipal principal,
        string approvalReference, string logicalFilingIdentity,
        CancellationToken cancellationToken = default)
    {
        Authorize(principal);
        ValidateReference(approvalReference, nameof(approvalReference));
        ValidateReference(logicalFilingIdentity, nameof(logicalFilingIdentity));
        var owner = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var record = await conversationStore.GetForLogicalSubmissionAsync(owner.TenantReference,
            LogicalSubmission(approvalReference, logicalFilingIdentity), cancellationToken);
        return record is null ? null : Safe(record);
    }

    public async Task<CorporationTaxTransactionStatus?> GetLatestAsync(
        ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        Authorize(principal);
        var owner = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var records = await conversationStore.ListForOwnerAsync(owner.TenantReference,
            owner.ReportingSubjectReference, 1, cancellationToken);
        return records.Count == 0 ? null : Safe(records[0]);
    }

    public async Task<CorporationTaxTransactionOutcome> ReadOutcomeAsync(
        ClaimsPrincipal principal, string conversationReference,
        CancellationToken cancellationToken = default)
    {
        Authorize(principal);
        var (owner, record) = await OwnedAsync(principal, conversationReference,
            cancellationToken);
        if (record.TerminalResponseReference is null
            || record.TerminalResponseSha256 is null)
            throw new InvalidOperationException(
                "The Corporation Tax authority outcome is not yet available.");
        var content = await transactionContent.ReadAsync(owner.TenantReference,
            owner.ReportingSubjectReference, record.TerminalResponseReference,
            cancellationToken) ?? throw new InvalidDataException(
                "The Corporation Tax authority outcome could not be read.");
        var digest = Convert.ToHexString(SHA256.HashData(content));
        if (!string.Equals(digest, record.TerminalResponseSha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                "The Corporation Tax authority outcome failed its integrity check.");
        return new(content, digest,
            $"corporation-tax-outcome-{record.TransactionId}.xml",
            record.ReceiptReference is not null);
    }

    public async Task<CorporationTaxTransactionStatus> SubmitApprovedOnceAsync(
        ClaimsPrincipal principal, TaxHubCorporationTaxContinuation continuation,
        string preparationReference, string logicalFilingIdentity,
        CorporationTaxGatewayCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        RequireEnabled(principal);
        ArgumentNullException.ThrowIfNull(continuation);
        ValidateReference(preparationReference, nameof(preparationReference));
        ValidateReference(logicalFilingIdentity, nameof(logicalFilingIdentity));
        ValidateCredentials(credentials);

        var owner = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var approvalView = await approvals.GetAsync(principal, preparationReference,
            cancellationToken) ?? throw new InvalidOperationException(
            "The Corporation Tax return has not been approved.");
        var preparation = await preparationStore.GetAsync(owner.TenantReference,
            preparationReference, cancellationToken) ?? throw new KeyNotFoundException(
            "The approved Corporation Tax preparation was not found.");
        var current = await preparations.GetLatestAsync(principal, continuation, cancellationToken);
        if (current is null || !current.IsCurrent
            || current.Preparation.Reference != preparation.Reference)
            throw new InvalidOperationException(
                "The approved Corporation Tax return is no longer current and cannot be submitted.");
        var period = preparation.Periods.SingleOrDefault(value =>
                value.LogicalFilingIdentity == logicalFilingIdentity)
            ?? throw new InvalidOperationException(
                "The approved Corporation Tax period was not found.");
        var logical = LogicalSubmission(approvalView.Approval.Reference,
            period.LogicalFilingIdentity);
        var existing = await conversationStore.GetForLogicalSubmissionAsync(owner.TenantReference,
            logical, cancellationToken);
        if (existing is not null)
        {
            if (existing.Stage != TransactionEngineConversationStage.Prepared)
                return Safe(existing);
        }
        else
        {
            var package = await RestoreSubmissionReadyPackageAsync(owner.TenantReference,
                preparation, period, cancellationToken);
            var identity = Identity(credentials, serviceRoot: package.Transmission.Content.AsSpan());
            var protocol = TransactionEngineProtocolOptions.CorporationTaxProduction();
            var coordinator = new TransactionEngineConversationCoordinator(conversationStore,
                transactionContent, protocol, timeProvider);
            existing = await coordinator.PrepareAsync(package, identity,
                Guid.NewGuid().ToString("N").ToUpperInvariant(), owner.TenantReference,
                owner.ReportingSubjectReference, logical, cancellationToken);
        }

        var exchange = Exchanges();
        try
        {
            return Safe(await exchange.SubmitOnceAsync(owner.TenantReference,
                owner.ReportingSubjectReference, existing.ConversationReference,
                cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // BeginSubmission persists SubmissionUncertain before the first HTTP byte.
            // The caller receives durable state and must recover by transaction ID;
            // this method never performs a second submit.
            return Safe(await RequiredAsync(owner.TenantReference,
                owner.ReportingSubjectReference, existing.ConversationReference,
                CancellationToken.None));
        }
    }

    public async Task<CorporationTaxTransactionStatus> RecoverOnceAsync(
        ClaimsPrincipal principal, string conversationReference,
        CorporationTaxGatewayCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        RequireEnabled(principal);
        ValidateCredentials(credentials);
        var (owner, record) = await OwnedAsync(principal, conversationReference,
            cancellationToken);
        var preparation = await PreparationForLogicalAsync(owner.TenantReference,
            record.LogicalSubmissionReference, cancellationToken);
        var period = PeriodForLogical(preparation, record.LogicalSubmissionReference);
        var serviceRoot = await ReadAsync(owner.TenantReference, period.ServiceRoot,
            cancellationToken);
        var identity = Identity(credentials, serviceRoot);
        return Safe(await Exchanges().RecoverOnceAsync(owner.TenantReference,
            owner.ReportingSubjectReference, record.ConversationReference, identity,
            cancellationToken));
    }

    public async Task<CorporationTaxTransactionStatus> PollOnceAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken = default)
    {
        RequireEnabled(principal);
        var (owner, record) = await OwnedAsync(principal, conversationReference,
            cancellationToken);
        return Safe(await Exchanges().PollOnceAsync(owner.TenantReference,
            owner.ReportingSubjectReference, record.ConversationReference, cancellationToken));
    }

    public async Task<CorporationTaxTransactionStatus> DeleteOnceAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken = default)
    {
        RequireEnabled(principal);
        var (owner, record) = await OwnedAsync(principal, conversationReference,
            cancellationToken);
        return Safe(await Exchanges().DeleteOnceAsync(owner.TenantReference,
            owner.ReportingSubjectReference, record.ConversationReference, cancellationToken));
    }

    private TransactionEngineExchangeCoordinator Exchanges()
    {
        var protocol = TransactionEngineProtocolOptions.CorporationTaxProduction();
        var coordinator = new TransactionEngineConversationCoordinator(conversationStore,
            transactionContent, protocol, timeProvider);
        return new(coordinator, new TransactionEngineHttpGateway(
            httpClientFactory.CreateClient(HttpClientName), protocol,
            new(TimeoutSeconds: _options.TimeoutSeconds)));
    }

    private async Task<PreparedSubmissionPackage> RestoreSubmissionReadyPackageAsync(
        string tenantReference, CorporationTaxPreparationRecord preparation,
        CorporationTaxPreparedPeriodRecord period, CancellationToken cancellationToken)
    {
        var service = await ReadAsync(tenantReference, period.ServiceRoot, cancellationToken);
        var accounts = await ReadAsync(tenantReference, preparation.Accounts, cancellationToken);
        var computation = await ReadAsync(tenantReference, period.Computation, cancellationToken);
        return new("HMRC-CORPORATION-TAX",
            Artifact("CORPORATION-TAX-RETURN",
                CorporationTaxServiceRootSerializer.ContractVersion, period.ServiceRoot, service),
            [
                new(preparation.Accounts.FileName,
                    Artifact("COMPANY-ACCOUNTS", "accepted-snapshot",
                        preparation.Accounts, accounts)),
                new(period.Computation.FileName,
                    Artifact("CORPORATION-TAX-COMPUTATION",
                        CompanyContractRegistry.HmrcComputationTaxonomy2025.Version,
                        period.Computation, computation))
            ],
            new(SubmissionPollingMode.TransactionEngine));
    }

    private async Task<byte[]> ReadAsync(string tenantReference,
        CorporationTaxPreparedArtifactRecord artifact,
        CancellationToken cancellationToken) =>
        (await protectedContent.ReadAsync(tenantReference, artifact.ProtectedContentReference,
            artifact.Sha256, cancellationToken)).Bytes;

    private static PreparedStatutoryArtifact Artifact(string operation, string contractVersion,
        CorporationTaxPreparedArtifactRecord retained, ReadOnlySpan<byte> bytes) =>
        PreparedStatutoryArtifact.Create("GB", "HMRC", operation, contractVersion,
            PreparedArtifactStatus.SubmissionReady, retained.MediaType, bytes, findings: retained.Findings);

    private TransactionEngineSubmissionIdentity Identity(
        CorporationTaxGatewayCredentials credentials, ReadOnlySpan<byte> serviceRoot)
    {
        var utr = ReadUtr(serviceRoot);
        return new(_options.VendorId.Trim(), credentials.SenderId.Trim(), credentials.Password,
            utr, _options.Product.Trim(), _options.ProductVersion.Trim(), IsExternalTest: false);
    }

    private static string ReadUtr(ReadOnlySpan<byte> serviceRoot)
    {
        using var stream = new MemoryStream(serviceRoot.ToArray(), writable: false);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 8 * 1024 * 1024
        });
        var document = XDocument.Load(reader, LoadOptions.None);
        XNamespace ct = "http://www.govtalk.gov.uk/taxation/CT/5";
        var values = document.Descendants(ct + "Key")
            .Where(element => (string?)element.Attribute("Type") == "UTR")
            .Where(element => element.Parent?.Name == ct + "Keys"
                && element.Parent.Parent?.Name == ct + "IRheader")
            .Select(element => element.Value.Trim()).ToArray();
        if (values.Length != 1 || values[0].Length is < 1 or > 32
            || values[0].Any(character => !char.IsAsciiDigit(character)))
            throw new InvalidDataException(
                "The approved Corporation Tax service root does not contain one valid UTR.");
        return values[0];
    }

    private static void ValidateCredentials(CorporationTaxGatewayCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        if (string.IsNullOrWhiteSpace(credentials.SenderId) || credentials.SenderId.Length > 128
            || credentials.SenderId.Any(char.IsControl)
            || string.IsNullOrEmpty(credentials.Password) || credentials.Password.Length > 256
            || credentials.Password.Any(char.IsControl))
            throw new ArgumentException(
                "Valid live Government Gateway credentials are required.", nameof(credentials));
    }

    private async Task<(CompaniesHouseWorkflowIdentity Owner,
        TransactionEngineConversationRecord Record)> OwnedAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken)
    {
        ValidateReference(conversationReference, nameof(conversationReference));
        var owner = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var record = await RequiredAsync(owner.TenantReference,
            owner.ReportingSubjectReference, conversationReference, cancellationToken);
        return (owner, record);
    }

    private async Task<TransactionEngineConversationRecord> RequiredAsync(string tenant,
        string principal, string reference, CancellationToken cancellationToken) =>
        await conversationStore.GetAsync(tenant, principal, reference, cancellationToken)
        ?? throw new KeyNotFoundException(
            "The Corporation Tax submission conversation was not found.");

    private async Task<CorporationTaxPreparationRecord> PreparationForLogicalAsync(
        string tenantReference, string logicalSubmissionReference,
        CancellationToken cancellationToken)
    {
        var separator = logicalSubmissionReference.IndexOf(':');
        if (separator < 1)
            throw new InvalidOperationException(
                "The Corporation Tax logical submission identity is invalid.");
        var approvalReference = logicalSubmissionReference[..separator];
        var approval = await approvalStore.GetAsync(tenantReference, approvalReference,
            cancellationToken) ?? throw new InvalidOperationException(
            "The Corporation Tax conversation approval was not found.");
        return await preparationStore.GetAsync(tenantReference, approval.PreparationReference,
            cancellationToken) ?? throw new InvalidOperationException(
            "The Corporation Tax conversation preparation was not found.");
    }

    private static CorporationTaxPreparedPeriodRecord PeriodForLogical(
        CorporationTaxPreparationRecord preparation, string logicalSubmissionReference) =>
        preparation.Periods.Single(period => logicalSubmissionReference.EndsWith(
            period.LogicalFilingIdentity, StringComparison.Ordinal));

    private static string LogicalSubmission(string approvalReference,
        string logicalFilingIdentity)
    {
        ValidateReference(approvalReference, nameof(approvalReference));
        ValidateReference(logicalFilingIdentity, nameof(logicalFilingIdentity));
        return $"{approvalReference}:{logicalFilingIdentity}";
    }

    private CorporationTaxTransactionStatus Safe(TransactionEngineConversationRecord record) =>
        new(record.ConversationReference, record.TransactionId, record.ServiceRootSha256,
            record.WireSha256, record.Stage.ToString(), record.UpdatedAt,
            record.PollNotBefore, record.Outcome,
            record.TerminalResponseReference is not null,
            record.ReceiptReference is not null,
            record.TerminalResponseSha256,
            $"CT-{record.ConversationReference[..Math.Min(12, record.ConversationReference.Length)].ToUpperInvariant()}",
            _options.Mode.ToString(),
            _options.IsProductionEnabled);

    private void RequireEnabled(ClaimsPrincipal principal)
    {
        Authorize(principal);
        if (!_options.IsEnabled)
            throw new InvalidOperationException(
                "Corporation Tax submission remains disabled by the family gate.");
    }

    private static void Authorize(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (principal.Identity?.IsAuthenticated != true
            || !(principal.IsInRole(Constants.AdministratorsRole)
                || principal.IsInRole(Constants.ManagersRole)))
            throw new UnauthorizedAccessException(
                "Corporation Tax submission requires Administrator or Manager permission.");
    }

    private static void ValidateReference(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 256 || value.Any(char.IsControl))
            throw new ArgumentException("A bounded reference is required.", parameterName);
    }
}

internal sealed class DisabledCorporationTaxTransactionEngineService
    : ICorporationTaxTransactionEngineService
{
    public Task<CorporationTaxTransactionStatus?> GetAsync(ClaimsPrincipal principal,
        string approvalReference, string logicalFilingIdentity,
        CancellationToken cancellationToken = default) => Task.FromResult<CorporationTaxTransactionStatus?>(null);
    public Task<CorporationTaxTransactionStatus?> GetLatestAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default) => Task.FromResult<CorporationTaxTransactionStatus?>(null);
    public Task<CorporationTaxTransactionOutcome> ReadOutcomeAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken = default) => Disabled<CorporationTaxTransactionOutcome>();

    public Task<CorporationTaxTransactionStatus> SubmitApprovedOnceAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation, string preparationReference,
        string logicalFilingIdentity, CorporationTaxGatewayCredentials credentials,
        CancellationToken cancellationToken = default) => Disabled();
    public Task<CorporationTaxTransactionStatus> RecoverOnceAsync(ClaimsPrincipal principal,
        string conversationReference, CorporationTaxGatewayCredentials credentials,
        CancellationToken cancellationToken = default) => Disabled();
    public Task<CorporationTaxTransactionStatus> PollOnceAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken = default) => Disabled();
    public Task<CorporationTaxTransactionStatus> DeleteOnceAsync(ClaimsPrincipal principal,
        string conversationReference, CancellationToken cancellationToken = default) => Disabled();

    private static Task<CorporationTaxTransactionStatus> Disabled() =>
        throw new InvalidOperationException(
            "Corporation Tax production submission remains disabled by the family gate.");
    private static Task<T> Disabled<T>() =>
        throw new InvalidOperationException(
            "Corporation Tax production submission remains disabled by the family gate.");
}
