using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.TradeControl.Data;
using TradeControl.Tax.UK.Adapters.TradeControl.Readers;
using TradeControl.Tax.UK.Application.DataProvision;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.CompaniesHouse.Accounts.Tis6_0;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public sealed record CompaniesHouseDirectorAdvanceReview(
    string DirectorName,
    decimal OpeningBalance,
    decimal Advances,
    decimal Repayments,
    decimal ClosingBalance,
    string Terms);

public sealed record CompaniesHousePreparationInput(
    DateOnly PeriodEnd,
    DateOnly ApprovedOn,
    string SigningDirectorName,
    string PrincipalActivity,
    string AccountingPolicies,
    int AverageEmployees,
    bool ConfirmsNoMaterialCommitmentsOrContingencies,
    IReadOnlyList<CompaniesHouseDirectorAdvanceReview> DirectorAdvances);

public sealed record CompaniesHousePreparedReview(
    string Reference,
    string DocumentSha256,
    string PackageSha256,
    DateTimeOffset ExpiresAtUtc,
    string DocumentPath,
    string DownloadPath,
    string PdfPath);

public sealed record CompaniesHouseReviewDocument(string MediaType, byte[] Bytes, string Sha256);

public sealed record CompaniesHouseApprovalReview(
    string Reference,
    string PreparationReference,
    string DeclarationVersion,
    string DeclarationSha256,
    string DocumentSha256,
    string PackageSha256,
    string ApprovedBy,
    DateTimeOffset ApprovedAtUtc);

public static class CompaniesHouseApprovalDeclaration
{
    public const string Version = "CH-ACCOUNTS-APPROVAL-1";
    public const string Text = "I confirm that I have reviewed the exact accounts identified by the document " +
        "and package SHA-256 values. I am authorised by the company to approve these accounts for filing. " +
        "I understand that this approval records an internal decision only: it does not submit the accounts " +
        "or indicate acceptance by Companies House.";

    public static string Sha256 => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(Text)));
}

public interface ICompaniesHousePreparationReviewService
{
    Task<CompaniesHousePreparedReview> PrepareAsync(short yearNumber, DateTime selectedPeriodStart,
        CompaniesHousePreparationInput input, CancellationToken cancellationToken = default);
    Task<CompaniesHouseReviewDocument> ReadDocumentAsync(string preparationReference,
        CancellationToken cancellationToken = default);
    Task<CompaniesHouseApprovalReview> ApproveAsync(string preparationReference,
        string declarationVersion, bool declarationsConfirmed,
        CancellationToken cancellationToken = default);
}

internal sealed class CompaniesHousePreparationReviewService(
    NodeContext nodeContext,
    ICompaniesHouseReadinessService readinessService,
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    ICompaniesHouseWorkflowStore workflowStore,
    ICompaniesHouseProtectedContentStore contentStore,
    ICompaniesHouseFilingAuthorisationPolicy filingAuthorisationPolicy,
    IHttpContextAccessor httpContextAccessor,
    IOptions<CompaniesHouseProductHostOptions> hostOptions,
    TimeProvider timeProvider) : ICompaniesHousePreparationReviewService
{
    private const string DatasetKey = "UK-CO-ACCTS-2026";
    private const string ReviewSubmissionNumber = "000000";
    private static readonly JsonSerializerOptions ReviewJson = new(JsonSerializerDefaults.Web);

    public async Task<CompaniesHousePreparedReview> PrepareAsync(short yearNumber,
        DateTime selectedPeriodStart, CompaniesHousePreparationInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!hostOptions.Value.Enabled || hostOptions.Value.DispatchMode != CompaniesHouseDispatchMode.SendDisabled)
            throw new InvalidOperationException("Companies House exact-document preparation is unavailable.");

        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var readiness = await readinessService.AssessAsync(yearNumber, selectedPeriodStart, cancellationToken);
        if (!readiness.IsEligible)
            throw new InvalidOperationException("The selected accounting year no longer passes Companies House readiness.");

        var draft = ToDraft(input);
        var errors = CompaniesHouseFilingInputReview.Validate(draft,
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
        if (errors.Count != 0)
            throw new InvalidOperationException(string.Join(" ", errors));

        var period = await ResolvePeriodAsync(yearNumber, cancellationToken);
        if (period.Current.End != input.PeriodEnd
            || selectedPeriodStart.Date != period.LastPeriodStart.ToDateTime(TimeOnly.MinValue))
            throw new InvalidOperationException("The reviewed accounting period no longer matches the selected closed year end.");

        var connectionString = nodeContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The statutory accounting source is unavailable.");

        var asOfDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var context = await new TcStatutoryContextReader(new ConnectionFactory(), connectionString)
            .ReadAsync(asOfDate, cancellationToken);
        var companyNumber = CompaniesHouseReadinessService.NormalizeCompanyNumber(
            context.Identity.CompanyNumber);
        if (companyNumber.Length != 8 || !companyNumber.All(char.IsAsciiLetterOrDigit))
            throw new InvalidOperationException("The server-derived Companies House registration cannot be normalised to the required eight-character form.");

        var request = new CompanyProjectionRequest(
            asOfDate,
            period.Current,
            period.Comparative,
            period.Comparative is null,
            new(companyNumber, true, true, 0m, period.Comparative is null ? null : 0m,
                input.PrincipalActivity, input.AccountingPolicies, input.AverageEmployees,
                input.DirectorAdvances.Select(item => new DirectorAdvanceDraft(
                    item.DirectorName, item.OpeningBalance, item.Advances, item.Repayments,
                    item.ClosingBalance, item.Terms)).ToArray(),
                [], input.ApprovedOn, identity.ActorReference, input.SigningDirectorName));

        var source = await new TcCompanyStatutorySourceReader(new ConnectionFactory(), connectionString)
            .ReadAsync(request, cancellationToken);
        var accounts = new CompanyAccountsPopulator().Populate(source, new(true, true));
        var preparedAccounts = new CompanyAccountsPreparer().Prepare(accounts, true,
            $"{source.Identity.SubjectName} statutory accounts",
            $"{source.Identity.SubjectCode}-filleted-accounts.xhtml", source.Versions);
        if (preparedAccounts.Artifact.HasErrors)
            throw new InvalidOperationException(BlockingFindings(
                "The exact statutory accounts document contains blocking findings",
                preparedAccounts.Artifact.Findings));

        var package = new CompaniesHouseAccountsPreparer().Prepare(accounts, preparedAccounts,
            ReviewSubmissionNumber, new(true, true, true), "TIS-6.0",
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime), false, source.Versions);
        if (package.Transmission.HasErrors)
            throw new InvalidOperationException(BlockingFindings(
                "The credential-free Companies House review package contains blocking findings",
                package.Transmission.Findings));

        var document = package.Documents.Single().Artifact;
        var reviewedInputBytes = JsonSerializer.SerializeToUtf8Bytes(input, ReviewJson);
        var retainedDocument = await contentStore.WriteAsync(identity.TenantReference,
            "accounts-document", document.Content.ToArray(), cancellationToken);
        var retainedPackage = await contentStore.WriteAsync(identity.TenantReference,
            "accounts-package-preview", package.Transmission.Content.ToArray(), cancellationToken);
        var retainedInput = await contentStore.WriteAsync(identity.TenantReference,
            "reviewed-input", reviewedInputBytes, cancellationToken);
        if (!string.Equals(retainedDocument.Sha256, document.Sha256, StringComparison.Ordinal)
            || !string.Equals(retainedPackage.Sha256, package.Transmission.Sha256, StringComparison.Ordinal)
            || !string.Equals(retainedInput.Sha256,
                Convert.ToHexString(SHA256.HashData(reviewedInputBytes)), StringComparison.Ordinal))
            throw new InvalidOperationException("Protected Companies House evidence changed during retention.");

        var now = timeProvider.GetUtcNow();
        var reference = $"chp1-{Guid.NewGuid():N}";
        var snapshot = Snapshot(source.Versions);
        await workflowStore.AddPreparationAsync(new(
            reference,
            identity.TenantReference,
            identity.ReportingSubjectReference,
            identity.ActorReference,
            Sha256($"{identity.TenantReference}|{companyNumber.Trim().ToUpperInvariant()}"),
            CompaniesHouseReadinessService.MaskCompanyNumber(companyNumber),
            period.Current.Start,
            period.Current.End,
            CompaniesHouseProductPolicy.SupportedFilingProfile,
            "TradeControl",
            DatasetKey,
            snapshot,
            document.Sha256,
            package.Transmission.Sha256,
            retainedDocument.Reference,
            retainedPackage.Reference,
            now,
            now.Add(CompaniesHouseProductPolicy.PreparationLifetime),
            yearNumber,
            DateOnly.FromDateTime(selectedPeriodStart),
            retainedInput.Sha256,
            retainedInput.Reference), cancellationToken);

        return new(reference, document.Sha256, package.Transmission.Sha256,
            now.Add(CompaniesHouseProductPolicy.PreparationLifetime),
            $"/TaxHub/CompaniesHouse/Preparation/{reference}/Document",
            $"/TaxHub/CompaniesHouse/Preparation/{reference}/Download",
            $"/TaxHub/CompaniesHouse/Preparation/{reference}/Draft.pdf");
    }

    public async Task<CompaniesHouseReviewDocument> ReadDocumentAsync(string preparationReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(preparationReference))
            throw new ArgumentException("A preparation reference is required.", nameof(preparationReference));
        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var preparation = await workflowStore.GetPreparationAsync(identity.TenantReference,
            preparationReference, cancellationToken)
            ?? throw new KeyNotFoundException("The Companies House preparation was not found.");
        if (!string.Equals(preparation.PrincipalReference, identity.ReportingSubjectReference,
                StringComparison.Ordinal))
            throw new UnauthorizedAccessException("The Companies House preparation belongs to another reporting subject.");
        if (preparation.ExpiresAtUtc <= timeProvider.GetUtcNow())
            throw new InvalidOperationException("The Companies House preparation has expired and must be regenerated.");

        var document = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedDocumentReference, preparation.DocumentSha256, cancellationToken);
        // Preserve the exact retained iXBRL bytes and digest, while presenting them through the
        // sandboxed browser review surface as HTML. Chromium treats application/xhtml+xml as a
        // downloadable/blocked resource in this authenticated iframe configuration.
        return new("text/html; charset=utf-8", document.Bytes, document.Sha256);
    }

    public async Task<CompaniesHouseApprovalReview> ApproveAsync(string preparationReference,
        string declarationVersion, bool declarationsConfirmed,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(preparationReference) || preparationReference.Length > 64)
            throw new ArgumentException("A valid preparation reference is required.", nameof(preparationReference));
        if (!string.Equals(declarationVersion, CompaniesHouseApprovalDeclaration.Version,
                StringComparison.Ordinal))
            throw new InvalidOperationException("The approval declaration changed. Review it again.");
        if (!declarationsConfirmed)
            throw new InvalidOperationException("Confirm the approval declaration before continuing.");

        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        if (!filingAuthorisationPolicy.CanApproveAccounts(principal))
            throw new UnauthorizedAccessException(
                "Companies House accounts approval requires Administrator or Manager permission.");

        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var preparation = await workflowStore.GetPreparationAsync(identity.TenantReference,
            preparationReference, cancellationToken)
            ?? throw new KeyNotFoundException("The Companies House preparation was not found.");
        EnsurePreparationOwnerAndLifetime(preparation, identity);

        var existing = await workflowStore.GetApprovalForPreparationAsync(identity.TenantReference,
            preparationReference, cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.DeclarationVersion, CompaniesHouseApprovalDeclaration.Version,
                    StringComparison.Ordinal)
                || !string.Equals(existing.DeclarationSha256, CompaniesHouseApprovalDeclaration.Sha256,
                    StringComparison.Ordinal)
                || !string.Equals(existing.DocumentSha256, preparation.DocumentSha256,
                    StringComparison.Ordinal)
                || !string.Equals(existing.PackageSha256, preparation.PackageSha256,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The existing approval does not match the current declaration or retained evidence.");
            return ToApprovalReview(existing);
        }

        if (!preparation.YearNumber.HasValue || !preparation.SelectedPeriodStart.HasValue
            || string.IsNullOrWhiteSpace(preparation.ReviewedInputSha256)
            || string.IsNullOrWhiteSpace(preparation.ProtectedInputReference))
            throw new InvalidOperationException(
                "This preparation predates immutable approval evidence. Prepare and review the accounts again.");

        var retainedInput = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedInputReference, preparation.ReviewedInputSha256, cancellationToken);
        _ = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedDocumentReference, preparation.DocumentSha256, cancellationToken);
        _ = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedPackageReference, preparation.PackageSha256, cancellationToken);
        var input = JsonSerializer.Deserialize<CompaniesHousePreparationInput>(retainedInput.Bytes, ReviewJson)
            ?? throw new InvalidOperationException("The retained reviewed filing inputs are invalid.");
        await VerifyCurrentCandidateAsync(preparation, input, identity, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var approval = new CompaniesHouseApprovalRecord(
            $"cha1-{Guid.NewGuid():N}",
            preparation.Reference,
            identity.TenantReference,
            identity.AspNetSubjectReference,
            identity.ActorReference,
            Sha256($"{identity.TenantReference}|{preparation.CompanyIdentitySha256}|" +
                $"{preparation.PeriodStart:yyyy-MM-dd}|{preparation.PeriodEnd:yyyy-MM-dd}|" +
                preparation.FilingProfile),
            preparation.CompanyIdentitySha256,
            preparation.PeriodStart,
            preparation.PeriodEnd,
            preparation.FilingProfile,
            CompaniesHouseApprovalDeclaration.Version,
            CompaniesHouseApprovalDeclaration.Sha256,
            preparation.SourceSystem,
            preparation.DatasetKey,
            preparation.SnapshotToken,
            preparation.DocumentSha256,
            preparation.PackageSha256,
            true,
            now);
        await workflowStore.AddApprovalAsync(approval, cancellationToken);
        return ToApprovalReview(approval);
    }

    private async Task VerifyCurrentCandidateAsync(CompaniesHousePreparationRecord preparation,
        CompaniesHousePreparationInput input, CompaniesHouseWorkflowIdentity identity,
        CancellationToken cancellationToken)
    {
        var errors = CompaniesHouseFilingInputReview.Validate(ToDraft(input),
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
        if (errors.Count != 0)
            throw new InvalidOperationException("The reviewed filing inputs are no longer valid. Prepare the accounts again.");

        var period = await ResolvePeriodAsync(preparation.YearNumber!.Value, cancellationToken);
        if (period.Current.Start != preparation.PeriodStart
            || period.Current.End != preparation.PeriodEnd
            || period.LastPeriodStart != preparation.SelectedPeriodStart!.Value
            || input.PeriodEnd != preparation.PeriodEnd)
            throw new InvalidOperationException("The selected accounting period changed after preparation.");

        var connectionString = nodeContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The statutory accounting source is unavailable.");
        var asOfDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var context = await new TcStatutoryContextReader(new ConnectionFactory(), connectionString)
            .ReadAsync(asOfDate, cancellationToken);
        var companyNumber = CompaniesHouseReadinessService.NormalizeCompanyNumber(context.Identity.CompanyNumber);
        var currentCompanyIdentity = Sha256(
            $"{identity.TenantReference}|{companyNumber.Trim().ToUpperInvariant()}");
        if (!string.Equals(currentCompanyIdentity, preparation.CompanyIdentitySha256, StringComparison.Ordinal))
            throw new InvalidOperationException("The Companies House identity changed after preparation.");

        var request = new CompanyProjectionRequest(
            asOfDate,
            period.Current,
            period.Comparative,
            period.Comparative is null,
            new(companyNumber, true, true, 0m, period.Comparative is null ? null : 0m,
                input.PrincipalActivity, input.AccountingPolicies, input.AverageEmployees,
                input.DirectorAdvances.Select(item => new DirectorAdvanceDraft(
                    item.DirectorName, item.OpeningBalance, item.Advances, item.Repayments,
                    item.ClosingBalance, item.Terms)).ToArray(),
                [], input.ApprovedOn, identity.ActorReference, input.SigningDirectorName));
        var source = await new TcCompanyStatutorySourceReader(new ConnectionFactory(), connectionString)
            .ReadAsync(request, cancellationToken);
        var accounts = new CompanyAccountsPopulator().Populate(source, new(true, true));
        var currentDocument = new CompanyAccountsPreparer().Prepare(accounts, true,
            $"{source.Identity.SubjectName} statutory accounts",
            $"{source.Identity.SubjectCode}-filleted-accounts.xhtml", source.Versions);
        if (currentDocument.Artifact.HasErrors
            || !string.Equals(Snapshot(source.Versions), preparation.SnapshotToken, StringComparison.Ordinal)
            || !string.Equals(currentDocument.Artifact.Sha256, preparation.DocumentSha256,
                StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The accounting source or reviewed inputs changed after preparation. Prepare and review new accounts.");
    }

    private void EnsurePreparationOwnerAndLifetime(CompaniesHousePreparationRecord preparation,
        CompaniesHouseWorkflowIdentity identity)
    {
        if (!string.Equals(preparation.PrincipalReference, identity.ReportingSubjectReference,
                StringComparison.Ordinal))
            throw new UnauthorizedAccessException(
                "The Companies House preparation belongs to another reporting subject.");
        if (preparation.ExpiresAtUtc <= timeProvider.GetUtcNow())
            throw new InvalidOperationException("The Companies House preparation has expired and must be regenerated.");
    }

    private static CompaniesHouseApprovalReview ToApprovalReview(CompaniesHouseApprovalRecord approval) => new(
        approval.Reference,
        approval.PreparationReference,
        approval.DeclarationVersion,
        approval.DeclarationSha256,
        approval.DocumentSha256,
        approval.PackageSha256,
        approval.ActorReference,
        approval.ApprovedAtUtc);

    private async Task<(ReportingWindow Current, ReportingWindow? Comparative, DateOnly LastPeriodStart)>
        ResolvePeriodAsync(short yearNumber, CancellationToken cancellationToken)
    {
        var currentStarts = await nodeContext.App_tbYearPeriods.AsNoTracking()
            .Where(item => item.YearNumber == yearNumber)
            .OrderBy(item => item.StartOn)
            .Select(item => item.StartOn)
            .ToArrayAsync(cancellationToken);
        if (currentStarts.Length == 0)
            throw new InvalidOperationException("The selected accounting year was not found.");
        var currentBounds = CompaniesHouseReadinessService.ResolvePeriodBounds(currentStarts);

        var priorYear = await nodeContext.App_tbYears.AsNoTracking()
            .Where(item => item.YearNumber < yearNumber && item.CashStatusCode == 2)
            .OrderByDescending(item => item.YearNumber)
            .Select(item => (short?)item.YearNumber)
            .FirstOrDefaultAsync(cancellationToken);
        ReportingWindow? comparative = null;
        if (priorYear.HasValue)
        {
            var priorStarts = await nodeContext.App_tbYearPeriods.AsNoTracking()
                .Where(item => item.YearNumber == priorYear.Value)
                .OrderBy(item => item.StartOn)
                .Select(item => item.StartOn)
                .ToArrayAsync(cancellationToken);
            if (priorStarts.Length == 0)
                throw new InvalidOperationException("The comparative accounting year was not found.");
            var priorBounds = CompaniesHouseReadinessService.ResolvePeriodBounds(priorStarts);
            comparative = new(DateOnly.FromDateTime(priorBounds.PeriodStart),
                DateOnly.FromDateTime(priorBounds.PeriodEnd));
        }

        return (new(DateOnly.FromDateTime(currentBounds.PeriodStart),
                DateOnly.FromDateTime(currentBounds.PeriodEnd)),
            comparative, DateOnly.FromDateTime(currentBounds.LastPeriodStart));
    }

    private static TaxHubCompaniesHouseFilingInputDraft ToDraft(CompaniesHousePreparationInput input) => new()
    {
        PeriodEnd = input.PeriodEnd,
        ApprovedOn = input.ApprovedOn.ToDateTime(TimeOnly.MinValue),
        SigningDirectorName = input.SigningDirectorName,
        PrincipalActivity = input.PrincipalActivity,
        AccountingPolicies = input.AccountingPolicies,
        AverageEmployees = input.AverageEmployees,
        ConfirmsNoMaterialCommitmentsOrContingencies = input.ConfirmsNoMaterialCommitmentsOrContingencies,
        DirectorAdvances = input.DirectorAdvances.Select(item => new TaxHubCompaniesHouseDirectorAdvanceInput
        {
            DirectorName = item.DirectorName,
            OpeningBalance = item.OpeningBalance,
            Advances = item.Advances,
            Repayments = item.Repayments,
            ClosingBalance = item.ClosingBalance,
            Terms = item.Terms
        }).ToList()
    };

    private static string Snapshot(IEnumerable<SourceVersion> versions) => Sha256(string.Join("\n",
        versions.OrderBy(item => item.SourceCode, StringComparer.Ordinal)
            .ThenBy(item => item.RowVersion, StringComparer.Ordinal)
            .ThenBy(item => item.UpdatedOn)
            .Select(item => $"{item.SourceCode}|{item.RowVersion}|{item.UpdatedOn:O}")));

    private static string BlockingFindings(string prefix, IEnumerable<PreparedArtifactFinding> findings)
    {
        var blocking = findings.Where(item => item.Severity == PreparedFindingSeverity.Error)
            .Select(item => $"{item.Code}: {item.Message}").ToArray();
        return blocking.Length == 0 ? $"{prefix}." : $"{prefix}: {string.Join("; ", blocking)}";
    }

    private static string Sha256(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal sealed class DisabledCompaniesHousePreparationReviewService
    : ICompaniesHousePreparationReviewService
{
    public Task<CompaniesHousePreparedReview> PrepareAsync(short yearNumber, DateTime selectedPeriodStart,
        CompaniesHousePreparationInput input, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Companies House exact-document preparation is unavailable in this host.");

    public Task<CompaniesHouseReviewDocument> ReadDocumentAsync(string preparationReference,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Companies House exact-document review is unavailable in this host.");

    public Task<CompaniesHouseApprovalReview> ApproveAsync(string preparationReference,
        string declarationVersion, bool declarationsConfirmed,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Companies House accounts approval is unavailable in this host.");
}
