namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal sealed record CorporationTaxSubmissionView(
    string StatusLabel,
    string Heading,
    string Message,
    string BadgeClass,
    string AlertClass,
    string AlertRole,
    string? DownloadLabel);

internal static class CorporationTaxSubmissionPresentation
{
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
