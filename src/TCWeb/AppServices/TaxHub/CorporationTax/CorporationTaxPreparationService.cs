using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Tax.UK.Adapters.TradeControl.Data;
using TradeControl.Tax.UK.Adapters.TradeControl.Readers;
using TradeControl.Tax.UK.Application.DataProvision;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.Company.ContractInfrastructure;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal sealed record CorporationTaxPreparationInput(
    string DeclarantName,
    DateOnly DeclarationDate);

internal sealed record CorporationTaxPreparationView(
    CorporationTaxPreparationRecord Preparation,
    bool IsCurrent,
    string? StaleReason);

internal interface ICorporationTaxPreparationService
{
    Task<CorporationTaxPreparationView?> GetLatestAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        CancellationToken cancellationToken = default);

    Task<CorporationTaxPreparationView> PrepareAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        CorporationTaxPreparationInput input,
        CancellationToken cancellationToken = default);

    Task<(CompaniesHouseProtectedContent Content, CorporationTaxPreparedArtifactRecord Artifact)>
        ReadArtifactAsync(ClaimsPrincipal principal, string preparationReference,
        string logicalFilingIdentity, string kind,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Freezes Objective 3 preview artifacts only. It neither promotes them to submission-ready nor
/// exposes a dispatch path; CT-D5 must approve these exact retained bytes before any later transport.
/// </summary>
internal sealed class CorporationTaxPreparationService(
    NodeContext nodeContext,
    ICompaniesHouseAccountsSnapshotService accountsSnapshots,
    ICompaniesHouseProtectedContentStore contentStore,
    ICorporationTaxPreparationStore preparationStore,
    ICorporationTaxCalculationReviewService calculationReviews,
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    TimeProvider timeProvider) : ICorporationTaxPreparationService
{
    public async Task<CorporationTaxPreparationView?> GetLatestAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        CancellationToken cancellationToken = default)
    {
        Authorize(principal);
        ArgumentNullException.ThrowIfNull(continuation);
        var identity = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var preparation = await preparationStore.GetLatestAsync(identity.TenantReference,
            continuation.AccountsSnapshotSha256, cancellationToken);
        if (preparation is null) return null;

        try
        {
            var calculation = await calculationReviews.BuildAsync(continuation, cancellationToken);
            if (!calculation.IsReady)
                return new(preparation, false,
                    "The accepted accounts or reviewed calculation is no longer ready.");
            var reviewedInputSha256 = ReviewedInputSha256(continuation, calculation);
            if (!string.Equals(preparation.SourceSha256, calculation.SourceSha256,
                    StringComparison.Ordinal)
                || !string.Equals(preparation.ReviewedInputSha256, reviewedInputSha256,
                    StringComparison.Ordinal))
                return new(preparation, false,
                    "The transaction source or reviewed Corporation Tax inputs have changed.");

            var utr = await ReadReviewedUtrAsync(identity.ReportingSubjectReference,
                continuation.AccountsPeriodEnd, cancellationToken);
            var companyNumber = await ReadCompanyNumberAsync(continuation, calculation, utr,
                preparation.DeclarantName, preparation.DeclarationDate, cancellationToken);
            var returnDetailsSha256 = ReturnDetailsSha256(utr, companyNumber,
                preparation.DeclarantName, preparation.DeclarationDate);
            return string.Equals(preparation.ReturnDetailsSha256, returnDetailsSha256,
                    StringComparison.Ordinal)
                ? new(preparation, true, null)
                : new(preparation, false,
                    "The statutory identity, UTR or return preparation details have changed.");
        }
        catch (Exception exception) when (exception is InvalidOperationException
            or ArgumentException or KeyNotFoundException or FormatException)
        {
            return new(preparation, false, exception.Message);
        }
    }

    public async Task<CorporationTaxPreparationView> PrepareAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        CorporationTaxPreparationInput input,
        CancellationToken cancellationToken = default)
    {
        Authorize(principal);
        ArgumentNullException.ThrowIfNull(continuation);
        ArgumentNullException.ThrowIfNull(input);
        var declarantName = input.DeclarantName.Trim();
        if (declarantName.Length is < 2 or > 160 || declarantName.Any(char.IsControl))
            throw new InvalidOperationException("Enter the full name of the person preparing the CT600.");
        if (input.DeclarationDate < continuation.AccountsPeriodEnd
            || input.DeclarationDate > DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).AddDays(1))
            throw new InvalidOperationException(
                "The CT600 declaration date must be on or after the period end and cannot be in the future.");

        var identity = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var calculation = await calculationReviews.BuildAsync(continuation, cancellationToken);
        if (!calculation.IsReady)
            throw new InvalidOperationException(
                "Corporation Tax documents cannot be prepared until the accepted accounts and reviewed calculation are ready.");

        var accepted = await accountsSnapshots.ReadAcceptedForCorporationTaxAsync(
            continuation.AccountsApprovalReference, continuation.AcceptanceSimulated,
            cancellationToken);
        if (!string.Equals(accepted.Accounts.AccountsSnapshotSha256,
                continuation.AccountsSnapshotSha256, StringComparison.Ordinal)
            || !string.Equals(accepted.Accounts.FullAccountsSha256,
                continuation.FullAccountsSha256, StringComparison.Ordinal)
            || accepted.Accounts.PeriodStart != continuation.AccountsPeriodStart
            || accepted.Accounts.PeriodEnd != continuation.AccountsPeriodEnd)
            throw new InvalidOperationException(
                "The accepted accounts changed before Corporation Tax document preparation.");
        var utr = await ReadReviewedUtrAsync(identity.ReportingSubjectReference,
            continuation.AccountsPeriodEnd, cancellationToken);
        var source = await ReadSourceAsync(continuation, calculation, utr, declarantName,
            input.DeclarationDate, cancellationToken);
        var sourceSha256 = CorporationTaxEvidenceDigests.SourceSha256(source.Versions);
        if (!string.Equals(sourceSha256, calculation.SourceSha256, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Corporation Tax transaction source changed during document preparation. Review the calculation again.");

        var accountsArtifact = PreparedStatutoryArtifact.Create(
            "GB", "HMRC", "STATUTORY-ACCOUNTS", CompanyContractCatalog.Frc2026.Version,
            PreparedArtifactStatus.Preview, accepted.Accounts.FullAccountsMediaType,
            accepted.Accounts.FullAccountsBytes, source.Versions);
        if (!string.Equals(accountsArtifact.Sha256, continuation.FullAccountsSha256,
                StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The exact accepted full accounts failed their Corporation Tax digest check.");
        var accounts = new PreparedCompanyAccounts(
            "full", "full-accounts.xhtml", CompanyContractCatalog.Frc2026.Version, 0,
            accountsArtifact);
        var populated = new CorporationTaxPopulator().Populate(source);
        var prepared = populated.Select((value, index) =>
        {
            var filingPeriod = continuation.Periods?.Single(period =>
                period.PeriodStart == value.Return.Period.Start
                && period.PeriodEnd == value.Return.Period.End)
                ?? throw new InvalidOperationException(
                    "The prepared CT600 period is outside the reviewed filing scope.");
            var result = new CorporationTaxPreparer().Prepare(value, accounts,
                $"{continuation.AccountsSnapshotSha256[..12]}-{index + 1}", source.Versions);
            var artifacts = result.Package.Documents.Select(document => document.Artifact)
                .Prepend(result.Package.Transmission).ToArray();
            if (artifacts.Any(artifact => artifact.HasErrors))
                throw new InvalidOperationException(
                    "The prepared Corporation Tax documents contain blocking validation findings.");
            if (artifacts.Any(artifact => artifact.Status != PreparedArtifactStatus.Preview))
                throw new InvalidOperationException(
                    "CT-D4 may retain preview artifacts only; submission-ready promotion is a later approval boundary.");
            return (filingPeriod, result);
        }).ToArray();

        await RequireUnchangedAsync(continuation, calculation, identity.ReportingSubjectReference,
            utr, cancellationToken);
        var retainedAccounts = await contentStore.WriteAsync(identity.TenantReference,
            "ct-accounts-preview", accountsArtifact.Content.ToArray(), cancellationToken);
        var accountRecord = Artifact("accounts", accounts.FileName, accountsArtifact,
            retainedAccounts);
        var retainedPeriods = new List<CorporationTaxPreparedPeriodRecord>(prepared.Length);
        foreach (var item in prepared)
        {
            var computationDocument = item.result.Package.Documents.Single(document =>
                !string.Equals(document.Name, accounts.FileName, StringComparison.OrdinalIgnoreCase));
            var serviceContent = await contentStore.WriteAsync(identity.TenantReference,
                "ct-service-preview", item.result.Package.Transmission.Content.ToArray(),
                cancellationToken);
            var computationContent = await contentStore.WriteAsync(identity.TenantReference,
                "ct-computation-preview", computationDocument.Artifact.Content.ToArray(),
                cancellationToken);
            retainedPeriods.Add(new(
                item.filingPeriod.LogicalIdentity,
                item.result.PeriodStart,
                item.result.PeriodEnd,
                Artifact("service-root", $"ct600-{item.result.PeriodEnd:yyyyMMdd}.xml",
                    item.result.Package.Transmission, serviceContent),
                Artifact("computation", computationDocument.Name,
                    computationDocument.Artifact, computationContent)));
        }

        await RequireUnchangedAsync(continuation, calculation, identity.ReportingSubjectReference,
            utr, cancellationToken);
        var preparation = new CorporationTaxPreparationRecord(
            identity.TenantReference,
            $"ctp1-{Guid.NewGuid():N}",
            continuation.AccountsSnapshotSha256,
            sourceSha256,
            ReviewedInputSha256(continuation, calculation),
            ReturnDetailsSha256(utr, source.CompanyNumber, declarantName, input.DeclarationDate),
            declarantName,
            input.DeclarationDate,
            identity.ActorReference,
            timeProvider.GetUtcNow(),
            accountRecord,
            retainedPeriods);
        await preparationStore.InsertAsync(preparation, cancellationToken);
        return new(preparation, true, null);
    }

    public async Task<(CompaniesHouseProtectedContent Content, CorporationTaxPreparedArtifactRecord Artifact)>
        ReadArtifactAsync(ClaimsPrincipal principal, string preparationReference,
        string logicalFilingIdentity, string kind,
        CancellationToken cancellationToken = default)
    {
        Authorize(principal);
        var identity = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var preparation = await preparationStore.GetAsync(identity.TenantReference,
            preparationReference, cancellationToken)
            ?? throw new KeyNotFoundException("The Corporation Tax preparation was not found.");
        var artifact = kind switch
        {
            "accounts" => preparation.Accounts,
            "service-root" => preparation.Periods.Single(period =>
                period.LogicalFilingIdentity == logicalFilingIdentity).ServiceRoot,
            "computation" => preparation.Periods.Single(period =>
                period.LogicalFilingIdentity == logicalFilingIdentity).Computation,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        var content = await contentStore.ReadAsync(identity.TenantReference,
            artifact.ProtectedContentReference, artifact.Sha256, cancellationToken);
        return (content, artifact);
    }

    private async Task<CorporationTaxSource> ReadSourceAsync(
        TaxHubCorporationTaxContinuation continuation,
        CorporationTaxCalculationReview calculation,
        string utr,
        string declarantName,
        DateOnly declarationDate,
        CancellationToken cancellationToken)
    {
        var connectionString = nodeContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The Corporation Tax accounting source is unavailable.");
        var reviewed = calculation.Periods.Select(period => period.Review.Input).ToArray();
        return await new TcCorporationTaxSourceReader(new ConnectionFactory(), connectionString)
            .ReadAsync(new(
                continuation.AccountsPeriodEnd,
                continuation.AccountsPeriodEnd,
                new(continuation.AccountsPeriodStart, continuation.AccountsPeriodEnd),
                new(null, utr, reviewed, declarantName, declarationDate)), cancellationToken);
    }

    private async Task<string> ReadCompanyNumberAsync(
        TaxHubCorporationTaxContinuation continuation,
        CorporationTaxCalculationReview calculation,
        string utr,
        string declarantName,
        DateOnly declarationDate,
        CancellationToken cancellationToken) =>
        (await ReadSourceAsync(continuation, calculation, utr, declarantName,
            declarationDate, cancellationToken)).CompanyNumber;

    private async Task RequireUnchangedAsync(
        TaxHubCorporationTaxContinuation continuation,
        CorporationTaxCalculationReview original,
        string reportingSubject,
        string expectedUtr,
        CancellationToken cancellationToken)
    {
        var current = await calculationReviews.BuildAsync(continuation, cancellationToken);
        if (!current.IsReady
            || !string.Equals(current.SourceSha256, original.SourceSha256, StringComparison.Ordinal)
            || !string.Equals(ReviewedInputSha256(continuation, current),
                ReviewedInputSha256(continuation, original), StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Corporation Tax calculation changed during document preparation. No preparation was recorded.");
        var currentUtr = await ReadReviewedUtrAsync(reportingSubject,
            continuation.AccountsPeriodEnd, cancellationToken);
        if (!CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(currentUtr),
                System.Text.Encoding.UTF8.GetBytes(expectedUtr)))
            throw new InvalidOperationException(
                "The Corporation Tax registration changed during document preparation. No preparation was recorded.");
    }

    private async Task<string> ReadReviewedUtrAsync(string reportingSubject,
        DateOnly asOfDate, CancellationToken cancellationToken)
    {
        var date = asOfDate.ToDateTime(TimeOnly.MinValue);
        var values = await (
            from registration in nodeContext.Subject_tbRegistrations.AsNoTracking()
            join status in nodeContext.App_tbStatutoryStatuses.AsNoTracking()
                on registration.StatusCode equals status.StatusCode
            where registration.SubjectCode == reportingSubject
                && registration.RegistrationSchemeCode == "GB-UTR"
                && registration.IsReviewed
                && status.IsActive
                && registration.ValidFrom <= date
                && (registration.ValidTo == null || registration.ValidTo >= date)
            select registration.RegistrationValue).ToListAsync(cancellationToken);
        if (values.Count != 1 || string.IsNullOrWhiteSpace(values[0]))
            throw new InvalidOperationException(
                "One active reviewed Corporation Tax UTR is required for the accepted accounts period.");
        return values[0].Trim();
    }

    private static CorporationTaxPreparedArtifactRecord Artifact(string kind, string fileName,
        PreparedStatutoryArtifact artifact, CompaniesHouseProtectedContent content)
    {
        if (!string.Equals(artifact.Sha256, content.Sha256, StringComparison.Ordinal))
            throw new InvalidOperationException("Retained Corporation Tax content failed its digest check.");
        return new(kind, fileName, artifact.MediaType, artifact.Status, artifact.Sha256,
            content.Reference, content.Length, artifact.Findings);
    }

    private static string ReviewedInputSha256(TaxHubCorporationTaxContinuation continuation,
        CorporationTaxCalculationReview calculation)
    {
        var canonical = string.Join("\n", calculation.Periods
            .OrderBy(period => period.PeriodStart)
            .Select(period =>
            {
                var logical = continuation.Periods?.Single(value =>
                    value.PeriodStart == period.PeriodStart
                    && value.PeriodEnd == period.PeriodEnd).LogicalIdentity
                    ?? throw new InvalidOperationException(
                        "A reviewed Corporation Tax period has no filing identity.");
                return $"{logical}|{period.Review.InputSha256}|{period.Review.Revision}";
            }));
        return CorporationTaxEvidenceDigests.Hash(canonical);
    }

    private static string ReturnDetailsSha256(string utr, string companyNumber,
        string declarantName, DateOnly declarationDate) =>
        CorporationTaxEvidenceDigests.Hash(string.Join("\n",
            utr.Trim(), companyNumber.Trim(), declarantName.Trim(),
            declarationDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)));

    private static void Authorize(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (principal.Identity?.IsAuthenticated != true
            || !(principal.IsInRole(Constants.AdministratorsRole)
                || principal.IsInRole(Constants.ManagersRole)))
            throw new UnauthorizedAccessException(
                "Corporation Tax document preparation requires Administrator or Manager permission.");
    }
}
