using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.Authorization;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal sealed record CorporationTaxApprovalView(
    CorporationTaxApprovalRecord Approval,
    string DeclarationText);

internal static class CorporationTaxApprovalDeclaration
{
    internal const string Version = "CT600-RETURN-APPROVAL-1";
    internal const string Text = "The information I have given on this form and the accompanying " +
        "Supplementary pages is correct and complete to the best of my knowledge and belief. " +
        "I confirm that I have reviewed and am authorised by the company to approve the exact " +
        "retained accounts, computation and CT600 service documents identified below. This " +
        "approval records an internal decision only: it does not submit the return or indicate " +
        "acceptance by HMRC.";

    internal static string Sha256 { get; } = Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(Text)));
}

internal interface ICorporationTaxApprovalService
{
    Task<CorporationTaxApprovalView?> GetAsync(ClaimsPrincipal principal,
        string preparationReference, CancellationToken cancellationToken = default);

    Task<CorporationTaxApprovalView> ApproveAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        string preparationReference,
        string declarationVersion,
        bool declarationConfirmed,
        CancellationToken cancellationToken = default);
}

internal sealed class CorporationTaxApprovalService(
    ICorporationTaxPreparationService preparations,
    ICorporationTaxPreparationStore preparationStore,
    ICorporationTaxApprovalStore approvalStore,
    ICompaniesHouseProtectedContentStore contentStore,
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    TimeProvider timeProvider) : ICorporationTaxApprovalService
{
    public async Task<CorporationTaxApprovalView?> GetAsync(ClaimsPrincipal principal,
        string preparationReference, CancellationToken cancellationToken = default)
    {
        Authorize(principal);
        ValidateReference(preparationReference);
        var identity = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var approval = await approvalStore.GetForPreparationAsync(identity.TenantReference,
            preparationReference, cancellationToken);
        if (approval is null) return null;
        var preparation = await preparationStore.GetAsync(identity.TenantReference,
            preparationReference, cancellationToken)
            ?? throw new KeyNotFoundException("The approved Corporation Tax preparation was not found.");
        ValidateApproval(approval, preparation, DocumentManifestSha256(preparation));
        return new(approval, CorporationTaxApprovalDeclaration.Text);
    }

    public async Task<CorporationTaxApprovalView> ApproveAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        string preparationReference,
        string declarationVersion,
        bool declarationConfirmed,
        CancellationToken cancellationToken = default)
    {
        Authorize(principal);
        ArgumentNullException.ThrowIfNull(continuation);
        ValidateReference(preparationReference);
        if (!string.Equals(declarationVersion, CorporationTaxApprovalDeclaration.Version,
                StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Corporation Tax approval declaration changed. Review it again.");
        if (!declarationConfirmed)
            throw new InvalidOperationException(
                "Confirm the Corporation Tax approval declaration before continuing.");

        var identity = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        var preparation = await preparationStore.GetAsync(identity.TenantReference,
            preparationReference, cancellationToken)
            ?? throw new KeyNotFoundException("The Corporation Tax preparation was not found.");
        if (!string.Equals(preparation.AccountsSnapshotSha256,
                continuation.AccountsSnapshotSha256, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Corporation Tax preparation belongs to different accepted accounts.");

        var manifestSha256 = DocumentManifestSha256(preparation);
        var existing = await approvalStore.GetForPreparationAsync(identity.TenantReference,
            preparationReference, cancellationToken);
        if (existing is not null)
        {
            ValidateApproval(existing, preparation, manifestSha256);
            return new(existing, CorporationTaxApprovalDeclaration.Text);
        }

        await RequireCurrentAsync(principal, continuation, preparation, cancellationToken);
        await VerifyRetainedContentAsync(identity.TenantReference, preparation, cancellationToken);
        // Re-read every bound identity and calculation after protected-content verification so the
        // approval is made against the current source, not merely the state observed on page load.
        await RequireCurrentAsync(principal, continuation, preparation, cancellationToken);

        var approvedAtUtc = timeProvider.GetUtcNow();
        var reference = $"cta1-{Guid.NewGuid():N}";
        var approvalSha256 = ApprovalSha256(reference, preparation, manifestSha256,
            identity.ActorReference, approvedAtUtc);
        var proposed = new CorporationTaxApprovalRecord(
            identity.TenantReference,
            reference,
            preparation.Reference,
            preparation.AccountsSnapshotSha256,
            preparation.SourceSha256,
            preparation.ReviewedInputSha256,
            preparation.ReturnDetailsSha256,
            manifestSha256,
            CorporationTaxApprovalDeclaration.Version,
            CorporationTaxApprovalDeclaration.Sha256,
            true,
            identity.ActorReference,
            approvedAtUtc,
            approvalSha256);
        var retained = await approvalStore.AddOrGetAsync(proposed, cancellationToken);
        ValidateApproval(retained, preparation, manifestSha256);
        return new(retained, CorporationTaxApprovalDeclaration.Text);
    }

    private async Task RequireCurrentAsync(ClaimsPrincipal principal,
        TaxHubCorporationTaxContinuation continuation,
        CorporationTaxPreparationRecord expected,
        CancellationToken cancellationToken)
    {
        var current = await preparations.GetLatestAsync(principal, continuation, cancellationToken);
        if (current is null
            || !current.IsCurrent
            || !string.Equals(current.Preparation.Reference, expected.Reference,
                StringComparison.Ordinal)
            || !string.Equals(current.Preparation.SourceSha256, expected.SourceSha256,
                StringComparison.Ordinal)
            || !string.Equals(current.Preparation.ReviewedInputSha256,
                expected.ReviewedInputSha256, StringComparison.Ordinal)
            || !string.Equals(current.Preparation.ReturnDetailsSha256,
                expected.ReturnDetailsSha256, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Corporation Tax preview is no longer current. Prepare and review new documents before approval.");
    }

    private async Task VerifyRetainedContentAsync(string tenantReference,
        CorporationTaxPreparationRecord preparation,
        CancellationToken cancellationToken)
    {
        foreach (var artifact in Artifacts(preparation))
        {
            if (artifact.Status != PreparedArtifactStatus.Preview
                || artifact.Findings.Any(finding =>
                    finding.Severity == PreparedFindingSeverity.Error))
                throw new InvalidOperationException(
                    "Only validated Corporation Tax preview documents can cross the approval boundary.");
            _ = await contentStore.ReadAsync(tenantReference,
                artifact.ProtectedContentReference, artifact.Sha256, cancellationToken);
        }
    }

    private static IReadOnlyList<CorporationTaxPreparedArtifactRecord> Artifacts(
        CorporationTaxPreparationRecord preparation) =>
        preparation.Periods.OrderBy(period => period.PeriodStart)
            .SelectMany(period => new[] { period.Computation, period.ServiceRoot })
            .Prepend(preparation.Accounts)
            .ToArray();

    private static string DocumentManifestSha256(CorporationTaxPreparationRecord preparation)
    {
        var lines = new List<string>
        {
            ArtifactLine("accounts", "", preparation.Accounts)
        };
        foreach (var period in preparation.Periods.OrderBy(value => value.PeriodStart))
        {
            var scope = string.Join('|',
                period.LogicalFilingIdentity,
                period.PeriodStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                period.PeriodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            lines.Add(ArtifactLine("computation", scope, period.Computation));
            lines.Add(ArtifactLine("service-root", scope, period.ServiceRoot));
        }
        return CorporationTaxEvidenceDigests.Hash(string.Join("\n", lines));
    }

    private static string ArtifactLine(string kind, string scope,
        CorporationTaxPreparedArtifactRecord artifact) => string.Join('|',
        kind,
        scope,
        artifact.FileName,
        artifact.MediaType,
        artifact.Status.ToString(),
        artifact.Sha256,
        artifact.Length.ToString(CultureInfo.InvariantCulture));

    private static string ApprovalSha256(string reference,
        CorporationTaxPreparationRecord preparation,
        string manifestSha256,
        string approvedBy,
        DateTimeOffset approvedAtUtc) => CorporationTaxEvidenceDigests.Hash(string.Join("\n",
            reference,
            preparation.Reference,
            preparation.AccountsSnapshotSha256,
            preparation.SourceSha256,
            preparation.ReviewedInputSha256,
            preparation.ReturnDetailsSha256,
            manifestSha256,
            CorporationTaxApprovalDeclaration.Version,
            CorporationTaxApprovalDeclaration.Sha256,
            approvedBy,
            approvedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));

    private static void ValidateApproval(CorporationTaxApprovalRecord approval,
        CorporationTaxPreparationRecord preparation,
        string manifestSha256)
    {
        var expectedSha256 = ApprovalSha256(approval.Reference, preparation, manifestSha256,
            approval.ApprovedBy, approval.ApprovedAtUtc);
        if (!approval.DeclarationConfirmed
            || !string.Equals(approval.PreparationReference, preparation.Reference,
                StringComparison.Ordinal)
            || !string.Equals(approval.AccountsSnapshotSha256,
                preparation.AccountsSnapshotSha256, StringComparison.Ordinal)
            || !string.Equals(approval.SourceSha256, preparation.SourceSha256,
                StringComparison.Ordinal)
            || !string.Equals(approval.ReviewedInputSha256,
                preparation.ReviewedInputSha256, StringComparison.Ordinal)
            || !string.Equals(approval.ReturnDetailsSha256,
                preparation.ReturnDetailsSha256, StringComparison.Ordinal)
            || !string.Equals(approval.DocumentManifestSha256, manifestSha256,
                StringComparison.Ordinal)
            || !string.Equals(approval.DeclarationVersion,
                CorporationTaxApprovalDeclaration.Version, StringComparison.Ordinal)
            || !string.Equals(approval.DeclarationSha256,
                CorporationTaxApprovalDeclaration.Sha256, StringComparison.Ordinal)
            || !string.Equals(approval.ApprovalSha256, expectedSha256,
                StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The retained Corporation Tax approval failed its immutable evidence check.");
    }

    private static void ValidateReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 64)
            throw new ArgumentException("A valid Corporation Tax preparation reference is required.",
                nameof(value));
    }

    private static void Authorize(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (principal.Identity?.IsAuthenticated != true
            || !(principal.IsInRole(Constants.AdministratorsRole)
                || principal.IsInRole(Constants.ManagersRole)))
            throw new UnauthorizedAccessException(
                "Corporation Tax approval requires Administrator or Manager permission.");
    }
}
