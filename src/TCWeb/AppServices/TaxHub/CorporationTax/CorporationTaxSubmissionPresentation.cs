using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal sealed record CorporationTaxSubmissionView(
    string StatusLabel,
    string Heading,
    string Message,
    string BadgeClass,
    string AlertClass,
    string AlertRole,
    string? DownloadLabel);

public sealed record CorporationTaxSubmissionSummary(
    string StatusLabel,
    string BadgeClass,
    string Heading,
    string Message,
    string AlertClass,
    string AlertRole);

internal static class CorporationTaxSubmissionPresentation
{
    internal static CorporationTaxSubmissionSummary Summarize(
        IReadOnlyCollection<CorporationTaxTransactionStatus> statuses,
        int expectedPeriods,
        bool isFake)
    {
        ArgumentNullException.ThrowIfNull(statuses);
        if (expectedPeriods < 1)
            throw new ArgumentOutOfRangeException(nameof(expectedPeriods));

        if (statuses.Count == 0)
            return new("Approved", "bg-primary", "Approval complete — submission is next",
                "The approved return is safely retained. Continue in the controlled submission panel.",
                "alert-info", "status");

        var recovery = statuses.FirstOrDefault(status => status.Stage == "RecoveryRequired");
        if (recovery is not null)
            return new("Support required", "bg-danger", "Submission needs support",
                $"Automatic processing stopped safely. Do not submit another return. Contact support and quote {recovery.SupportReference}.",
                "alert-danger", "alert");

        if (statuses.Any(status => status.Stage == "SubmissionUncertain"))
            return new("Confirmation required", "bg-warning text-dark",
                "Submission confirmation required",
                "HMRC may already have the return. Do not submit it again; recover the existing submission status.",
                "alert-warning", "alert");

        if (statuses.Any(status => status.Stage == "Complete"
                && status.Outcome != "BusinessResponse"))
            return new("Rejected", "bg-danger", "Return rejected",
                "Review the retained authority response and resolve the reported findings. The approved return cannot simply be submitted again.",
                "alert-danger", "alert");

        if (statuses.Count == expectedPeriods
            && statuses.All(status => status.Stage == "Complete"
                && status.Outcome == "BusinessResponse"))
            return new("Accepted", "bg-success", "Return accepted",
                isFake
                    ? "The fake authority accepted every return in this approved filing. The retained response evidence is available above."
                    : "HMRC accepted every return in this approved filing. Retain the receipts with the company records.",
                "alert-success", "alert");

        if (statuses.Any(status => status.Stage == "Complete"))
            return new("Action required", "bg-warning text-dark",
                "Continue the existing filing",
                "One return is complete and another has not finished. Continue this approved journey; do not repeat a completed return.",
                "alert-warning", "alert");

        if (statuses.Any(status => status.Stage is "TerminalPersisted" or "DeletePending"))
            return new("Outcome received", "bg-info text-dark", "Outcome received",
                "The authority outcome is safely retained while the conversation is closed in the background.",
                "alert-info", "status");

        if (statuses.Any(status => status.Stage == "AwaitingPoll"))
            return new("Received", "bg-info text-dark", "Return received",
                isFake
                    ? "The fake authority received the return. Trade Control is continuing in the background."
                    : "HMRC received the return. Trade Control is continuing to check the outcome in the background.",
                "alert-info", "status");

        return new("Prepared", "bg-secondary", "Return prepared",
            "The exact approved request is retained and has not yet completed submission.",
            "alert-secondary", "status");
    }

    internal static CorporationTaxSubmissionView For(
        CorporationTaxTransactionStatus? status, bool isFake)
    {
        var accepted = status?.Stage == "Complete"
            && status.Outcome == "BusinessResponse";
        var rejected = status?.Stage == "Complete" && !accepted;
        var statusLabel = status?.Stage switch
        {
            null => "Approved — not submitted",
            "Prepared" => "Prepared — not sent",
            "SubmissionUncertain" => "Send outcome uncertain — recovery required",
            "AwaitingPoll" => isFake
                ? "Received — awaiting fake-authority outcome"
                : "Received — awaiting HMRC outcome",
            "TerminalPersisted" => "Outcome received — completing conversation",
            "DeletePending" => "Outcome retained — completing conversation",
            "Complete" when accepted => "Accepted",
            "Complete" => "Rejected",
            "RecoveryRequired" => "Support required",
            _ => status.Stage
        };
        var heading = status?.Stage switch
        {
            null => "Approved return ready",
            "Prepared" => "Return prepared",
            "SubmissionUncertain" => "Submission confirmation required",
            "AwaitingPoll" => "Return received",
            "TerminalPersisted" or "DeletePending" => "Outcome received",
            "Complete" when accepted => "Return accepted",
            "Complete" => "Return rejected",
            "RecoveryRequired" => "Support required",
            _ => "Submission status"
        };
        var message = status?.Stage switch
        {
            null => "The exact return is approved and ready for its one permitted submission.",
            "Prepared" => "The exact request is safely prepared but has not been sent.",
            "SubmissionUncertain" => "We cannot yet confirm whether the authority received the return. Do not submit it again. Use Recover submission status with the same credentials.",
            "AwaitingPoll" when isFake => "The fake authority received the return. You can leave this page while Trade Control continues checking in the background.",
            "AwaitingPoll" => "HMRC received the return. You can leave this page while Trade Control continues checking in the background.",
            "TerminalPersisted" or "DeletePending" => "The authority outcome is safely retained and the conversation is being closed in the background.",
            "Complete" when accepted && isFake => "The fake authority accepted the return. The retained response and its digest are available below.",
            "Complete" when accepted => "HMRC accepted the Company Tax Return. Keep the receipt with the company records.",
            "Complete" => "The authority rejected the return. Download the response for the reported errors; this approved return cannot be altered or submitted again.",
            "RecoveryRequired" => $"Automatic processing stopped safely. Do not submit another return. Contact support and quote {status.SupportReference}.",
            _ => "The latest durable submission status is shown here."
        };
        var badgeClass = accepted ? "bg-success"
            : rejected || status?.Stage == "RecoveryRequired" ? "bg-danger"
            : status?.Stage == "SubmissionUncertain" ? "bg-warning text-dark"
            : status?.Stage is "AwaitingPoll" or "TerminalPersisted" or "DeletePending"
                ? "bg-info text-dark"
                : "bg-secondary";
        var alertClass = accepted ? "alert-success"
            : rejected || status?.Stage == "RecoveryRequired" ? "alert-danger"
            : status?.Stage == "SubmissionUncertain" ? "alert-warning"
            : status?.Stage is "AwaitingPoll" or "TerminalPersisted" or "DeletePending"
                ? "alert-info"
                : "alert-secondary";
        var alertRole = status?.Stage is "Complete" or "RecoveryRequired" or "SubmissionUncertain"
            ? "alert"
            : "status";
        var downloadLabel = status?.OutcomeAvailable == true
            ? status.ReceiptAvailable ? "Download receipt" : "Download authority response"
            : null;
        return new(statusLabel, heading, message, badgeClass, alertClass, alertRole,
            downloadLabel);
    }
}
