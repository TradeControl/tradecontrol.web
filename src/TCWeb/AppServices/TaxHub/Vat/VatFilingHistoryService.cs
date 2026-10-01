using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.Audit;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public sealed record VatFilingHistoryRecord(
    string AttemptReference,
    string PeriodKey,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string MaskedVrn,
    string State,
    string OutcomeCode,
    int? HttpStatus,
    string ActorReference,
    string PreparedDigest,
    DateTimeOffset ApprovedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? CorrelationReference,
    SubmissionReceiptEvidence? Receipt,
    SubmissionReconciliationEvidence? Reconciliation,
    IReadOnlyList<VatReturnReviewBox> ApprovedBoxes,
    bool PayloadIntegrityVerified,
    string EvidenceState);

public interface IVatFilingHistoryService
{
    Task<IReadOnlyList<VatFilingHistoryRecord>> GetAsync(DateOnly? periodFrom = null,
        DateOnly? periodTo = null, CancellationToken cancellationToken = default);
    Task<SubmissionReconciliationEvidence> ReconcileAsync(string attemptReference,
        CancellationToken cancellationToken = default);
}

public sealed class VatFilingHistoryService(
    IVatWorkflowIdentityAccessor identities,
    IVatApprovalEvidenceSource approvals,
    IVatFilingAuthorisationPolicy filingPolicy,
    IHttpContextAccessor httpContextAccessor,
    IVatAuthorityReturnSubmission authority,
    NodeContext nodeContext,
    IOptions<VatProductHostOptions> hostOptions) : IVatFilingHistoryService
{
    public async Task<IReadOnlyList<VatFilingHistoryRecord>> GetAsync(DateOnly? periodFrom = null,
        DateOnly? periodTo = null, CancellationToken cancellationToken = default)
    {
        var host = hostOptions.Value;
        if (!host.Enabled || host.PersistenceMode != VatPersistenceMode.DevelopmentFiles
            || string.IsNullOrWhiteSpace(host.DevelopmentStoreRoot))
            return [];

        var identity = await identities.GetRequiredAsync(cancellationToken);
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        if (!filingPolicy.CanManageHmrcConnection(principal))
            throw new UnauthorizedAccessException("VAT filing history requires Administrator or Manager permission.");
        var profile = await nodeContext.Cash_tbReportingProfiles.AsNoTracking()
            .Where(item => item.SubjectCode == identity.ReportingSubjectReference
                && item.ReportingTypeCode == 0 && item.ValidTo == null)
            .OrderByDescending(item => item.ValidFrom)
            .Select(item => new { item.AuthorityReference, item.ValidFrom })
            .FirstOrDefaultAsync(cancellationToken);
        if (profile is null || string.IsNullOrWhiteSpace(profile.AuthorityReference)) return [];
        var currentVrnDigest = Convert.ToHexString(SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(profile.AuthorityReference.Trim())));
        var adoptionDate = DateOnly.FromDateTime(profile.ValidFrom);
        var root = Path.GetFullPath(host.DevelopmentStoreRoot);
        var attempts = new FileSubmissionAttemptStore(
            SubmissionAttemptStoreOptions.SevenYearMetadata(Path.Combine(root, "attempts.json")));
        var content = new FileSubmissionContentStore(new SubmissionContentStoreOptions(
            Path.Combine(root, "protected-content")));
        var writes = (await attempts.ListTenantAsync(identity.TenantReference, 500, cancellationToken))
            .Where(item => item.OperationId == "vat.returns.submit" && item.ApprovalReference is not null)
            .ToArray();
        var evidence = await approvals.GetEvidenceAsync(writes.Select(item => item.ApprovalReference!)
            .Distinct(StringComparer.Ordinal).ToArray(), cancellationToken);
        var result = new List<VatFilingHistoryRecord>(writes.Length);
        foreach (var attempt in writes)
        {
            if (!evidence.TryGetValue(attempt.ApprovalReference!, out var approved))
            {
                if (periodFrom.HasValue || periodTo.HasValue) continue;
                result.Add(new(attempt.AttemptReference, "Unavailable", default, default, "Unavailable",
                    attempt.State.ToString(), attempt.OutcomeCode ?? "Not recorded", attempt.ActualStatusCode,
                    "Unavailable", attempt.PreparedDigest ?? "Unavailable", attempt.CreatedAt,
                    attempt.UpdatedAt, attempt.CorrelationReference, attempt.Receipt,
                    attempt.Reconciliation, [], false,
                    "The approval evidence is missing or inaccessible."));
                continue;
            }
            if (!IsVisible(approved, currentVrnDigest, adoptionDate, periodFrom, periodTo))
                continue;
            var payloadVerified = approved.PayloadIntegrityVerified;
            var evidenceState = approved.EvidenceFailure;
            if (payloadVerified && attempt.SafePayloadReference is not null)
            {
                var bytes = await content.ReadAsync(identity.TenantReference, attempt.PrincipalReference,
                    attempt.SafePayloadReference, cancellationToken);
                payloadVerified = bytes is not null && attempt.PreparedDigest is not null
                    && Convert.ToHexString(SHA256.HashData(bytes)).Equals(attempt.PreparedDigest,
                        StringComparison.Ordinal);
                if (!payloadVerified)
                    evidenceState = bytes is null ? "The dispatched payload is missing."
                        : "The dispatched payload does not match the prepared digest.";
            }
            else if (attempt.SafePayloadReference is null)
            {
                payloadVerified = false;
                evidenceState = "The dispatched payload reference is missing.";
            }
            result.Add(new(attempt.AttemptReference, approved.PeriodKey, approved.PeriodStart,
                approved.PeriodEnd, approved.MaskedVrn, attempt.State.ToString(),
                attempt.OutcomeCode ?? "Not recorded", attempt.ActualStatusCode,
                approved.ActorReference, approved.PreparedSha256, approved.ApprovedAtUtc,
                attempt.UpdatedAt, attempt.CorrelationReference, attempt.Receipt,
                attempt.Reconciliation, approved.Boxes,
                payloadVerified, evidenceState ?? "Approved and dispatched payload digests match."));
        }
        return result.OrderByDescending(item => item.UpdatedAtUtc).ToArray();
    }

    internal static bool IsVisible(VatApprovalEvidence approved, string currentVrnDigest,
        DateOnly adoptionDate, DateOnly? periodFrom, DateOnly? periodTo) =>
        approved.VrnSha256.Equals(currentVrnDigest, StringComparison.Ordinal)
        && approved.PeriodEnd >= adoptionDate
        && (!periodFrom.HasValue || approved.PeriodEnd >= periodFrom.Value)
        && (!periodTo.HasValue || approved.PeriodStart <= periodTo.Value);

    public async Task<SubmissionReconciliationEvidence> ReconcileAsync(string attemptReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(attemptReference) || attemptReference.Length > 64)
            throw new VatReturnReviewException("The VAT attempt reference is invalid.");
        var host = hostOptions.Value;
        if (!host.Enabled || host.PersistenceMode != VatPersistenceMode.DevelopmentFiles
            || string.IsNullOrWhiteSpace(host.DevelopmentStoreRoot))
            throw new VatReturnReviewException("VAT filing reconciliation is unavailable.");
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        if (!filingPolicy.CanManageHmrcConnection(principal))
            throw new UnauthorizedAccessException("VAT reconciliation requires Administrator or Manager permission.");
        var identity = await identities.GetRequiredAsync(cancellationToken);
        var attempts = new FileSubmissionAttemptStore(SubmissionAttemptStoreOptions.SevenYearMetadata(
            Path.Combine(Path.GetFullPath(host.DevelopmentStoreRoot), "attempts.json")));
        var attempt = (await attempts.ListTenantAsync(identity.TenantReference, 500, cancellationToken))
            .SingleOrDefault(item => item.AttemptReference == attemptReference)
            ?? throw new VatReturnReviewException("The VAT filing attempt is unavailable for this tenant.");
        if (attempt.ApprovalReference is null)
            throw new VatReturnReviewException("The VAT filing attempt has no durable approval reference.");
        var vrn = (await nodeContext.Subject_tbVirtuals.AsNoTracking()
            .Where(item => item.SubjectCode == identity.ReportingSubjectReference)
            .Select(item => item.VatNumber).SingleOrDefaultAsync(cancellationToken))?.Trim();
        if (string.IsNullOrWhiteSpace(vrn))
            throw new VatReturnReviewException("The reporting subject VAT registration is unavailable.");
        var approved = await approvals.ResolveForReconciliationAsync(attempt.ApprovalReference, vrn,
            cancellationToken);
        return await authority.ReconcileApprovedAsync(approved, attemptReference, cancellationToken);
    }
}
