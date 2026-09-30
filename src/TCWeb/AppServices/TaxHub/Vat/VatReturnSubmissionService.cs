using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public enum VatSubmissionResultState
{
    Accepted,
    Rejected,
    NotSent,
    OutcomeUnknown
}

public sealed record VatSubmissionResult(
    VatSubmissionResultState State,
    string OutcomeCode,
    string? AttemptReference,
    int? HttpStatus,
    DateTimeOffset? ProcessingDate = null,
    string? PaymentIndicator = null,
    string? FormBundleNumber = null,
    string? ChargeReference = null,
    bool? ReadbackMatches = null,
    bool ObligationRefreshSucceeded = false,
    string? ReadbackOutcomeCode = null,
    string? SupportReference = null,
    IReadOnlyList<VatAuthorityError>? AuthorityErrors = null);

public interface IVatReturnSubmissionService
{
    Task<VatSubmissionResult> SubmitAsync(string approvalReference,
        CancellationToken cancellationToken = default);
}

public sealed class VatReturnSubmissionService(
    IVatApprovedReturnResolver approvals,
    IVatAuthorityReturnSubmission authority,
    IVatFilingAuthorisationPolicy filingPolicy,
    IHttpContextAccessor httpContextAccessor,
    NodeContext nodeContext,
    ILogger<VatReturnSubmissionService> logger) : IVatReturnSubmissionService
{
    public async Task<VatSubmissionResult> SubmitAsync(string approvalReference,
        CancellationToken cancellationToken = default)
    {
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        if (!filingPolicy.CanManageHmrcConnection(principal))
            throw new UnauthorizedAccessException("VAT submission requires Administrator or Manager permission.");
        try
        {
            var approved = await approvals.ResolveAsync(approvalReference, cancellationToken);
            var dispatched = await authority.DispatchApprovedAsync(approved, cancellationToken);
            var outcome = dispatched.Outcome;
            var state = outcome.Kind switch
            {
                PreparedApiOutcomeKind.Succeeded when dispatched.Receipt is not null => VatSubmissionResultState.Accepted,
                PreparedApiOutcomeKind.Succeeded => VatSubmissionResultState.OutcomeUnknown,
                PreparedApiOutcomeKind.Rejected => VatSubmissionResultState.Rejected,
                PreparedApiOutcomeKind.Unknown => VatSubmissionResultState.OutcomeUnknown,
                _ => VatSubmissionResultState.NotSent
            };
            string? supportReference = null;
            if (state == VatSubmissionResultState.Rejected)
            {
                var detail = dispatched.Errors is { Count: > 0 }
                    ? string.Join(" | ", dispatched.Errors.Select(error => $"{error.Code}: {error.Message}"))
                    : "HMRC did not supply a readable business-error detail.";
                supportReference = await nodeContext.EventLog(NodeEnum.EventType.IsError,
                    $"HMRC VAT submission rejected. Attempt {outcome.AttemptReference ?? "not supplied"}; " +
                    $"HTTP {outcome.ActualStatusCode?.ToString() ?? "not supplied"}; " +
                    $"outcome {outcome.OutcomeCode}; {detail}");
            }
            else if (state == VatSubmissionResultState.OutcomeUnknown)
            {
                supportReference = await nodeContext.EventLog(NodeEnum.EventType.IsError,
                    $"HMRC VAT submission outcome is unknown. Attempt {outcome.AttemptReference ?? "not supplied"}; " +
                    $"HTTP {outcome.ActualStatusCode?.ToString() ?? "not supplied"}; " +
                    $"outcome {outcome.OutcomeCode}. Automatic replay remains blocked pending reconciliation.");
            }
            return new(state, outcome.OutcomeCode, outcome.AttemptReference, outcome.ActualStatusCode,
                dispatched.Receipt?.ProcessingDate == default
                    ? null : new DateTimeOffset(dispatched.Receipt!.ProcessingDate, TimeSpan.Zero),
                dispatched.Receipt?.PaymentIndicator, dispatched.Receipt?.FormBundleNumber,
                dispatched.Receipt?.ChargeRefNumber, dispatched.Reconciliation?.Matches,
                dispatched.ObligationRefreshSucceeded, dispatched.ReadbackOutcomeCode,
                supportReference, dispatched.Errors);
        }
        catch (VatReturnReviewException) { throw; }
        catch (UnauthorizedAccessException) { throw; }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected failure during approved VAT submission.");
            string? reference = null;
            try { reference = await nodeContext.ErrorLog(exception); }
            catch (Exception logException)
            {
                logger.LogError(logException, "Failed to write a VAT submission failure to the node Event Log.");
            }
            return new(VatSubmissionResultState.NotSent, "VAT-SUBMISSION-UNAVAILABLE", null, null,
                SupportReference: reference);
        }
    }
}
