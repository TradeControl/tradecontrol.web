using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Tax.UK.Application.Preparation;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public enum VatConnectionState
{
    NotConnected,
    Connected,
    ReauthorisationRequired,
    Disconnected,
    Unavailable
}

public sealed record VatConnectionStatus(VatConnectionState State, string? SafeReason = null);
public sealed record VatAuthorityObligation(string PeriodKey, DateOnly Start, DateOnly End,
    DateOnly Due, string Status, DateOnly? Received = null);
public sealed record VatPreparationReference(string Value, string Sha256, DateTimeOffset ExpiresAtUtc);
public sealed record VatApprovalReference(string Value, string PreparationReference,
    string DeclarationVersion, DateTimeOffset ApprovedAtUtc);
public sealed record VatFilingHistoryItem(string AttemptReference, string PeriodKey,
    string State, string PreparedDigest, DateTimeOffset UpdatedAtUtc);

/// <summary>
/// API-shaped, transport-neutral product VAT workflow. Razor/Blazor and any future authenticated
/// HTTP API are adapters over this boundary. Implementations resolve tenant and actor identity
/// internally; callers never supply tenant references, authority payloads or nine VAT boxes.
/// </summary>
public interface IVatProductWorkflow
{
    Task<VatConnectionStatus> GetConnectionStatusAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VatAuthorityObligation>> RetrieveObligationsAsync(
        CancellationToken cancellationToken = default);
    Task<VatPreparationReference> PrepareReturnAsync(string periodKey,
        CancellationToken cancellationToken = default);
    Task<VatApprovalReference> RecordApprovalAsync(string preparationReference, string declarationVersion,
        CancellationToken cancellationToken = default);
    Task<PreparedApiOutcome> SubmitApprovedAsync(string approvalReference,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VatFilingHistoryItem>> GetFilingHistoryAsync(
        CancellationToken cancellationToken = default);
    Task<VatReturnReconciliationResult> ReconcileAsync(string attemptReference,
        CancellationToken cancellationToken = default);
}
