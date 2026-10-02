using System;
using System.Linq;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public static class VatAuthorityUserMessages
{
    public static string Obligations(string safeCode) => safeCode switch
    {
        "HMRC-CLIENT_OR_AGENT_NOT_AUTHORISED" =>
            "The connected Government Gateway organisation account is not authorised for this VAT registration number. Disconnect HMRC, then connect the correct organisation account that owns this VAT registration.",
        "HMRC-TOO_MANY_REQUESTS" or "HMRC-HTTP-429" =>
            "HMRC's request limit has been reached. Wait before refreshing the obligations again.",
        "HMRC-SERVER_ERROR" or "HMRC-SERVICE_UNAVAILABLE" or "HMRC-SCHEDULED_MAINTENANCE"
            or "HMRC-HTTP-500" or "HMRC-HTTP-502" or "HMRC-HTTP-503" or "HMRC-HTTP-504" =>
            "HMRC's VAT service is temporarily unavailable. The local VAT records are unchanged; try again later.",
        "FRAUD-CONTEXT-REQUIRED" or "FRAUD-CONTEXT-REJECTED" =>
            "Fresh browser and session security information is required. Reload the Tax Hub before trying again.",
        "HMRC-VRN_INVALID" or "HMRC-INVALID_VRN" =>
            "The reporting subject does not contain a valid VAT registration number. Review the indirect-tax reporting profile.",
        "HMRC-INVALID_DATE_FROM" or "HMRC-INVALID_DATE_TO" =>
            "Trade Control could not create a valid HMRC obligation search window. Retain the support reference for investigation.",
        _ when safeCode.StartsWith("OAUTH-REAUTHORISATION-", StringComparison.Ordinal) =>
            "HMRC requires the business to reconnect before obligations can be retrieved.",
        _ => "HMRC VAT obligations could not be refreshed. Try again later."
    };

    public static string Submission(VatSubmissionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.State == VatSubmissionResultState.Accepted)
            return "HMRC accepted the approved VAT return and returned a receipt.";
        if (result.State == VatSubmissionResultState.OutcomeUnknown)
            return $"Sending may have begun ({result.OutcomeCode}). Do not submit again until the outcome is reconciled.";

        var codes = (result.AuthorityErrors ?? []).Select(error => error.Code)
            .Append(result.OutcomeCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (codes.Contains("DUPLICATE_SUBMISSION") || codes.Contains("HMRC-DUPLICATE_SUBMISSION"))
            return "HMRC reports that this period has already been filed. Do not submit it again; refresh obligations and use Filing History or HMRC readback to reconcile the return.";
        if (codes.Contains("CLIENT_OR_AGENT_NOT_AUTHORISED")
            || codes.Contains("HMRC-CLIENT_OR_AGENT_NOT_AUTHORISED"))
            return "The connected Government Gateway organisation account is not authorised for this VAT registration number. Disconnect HMRC and connect the correct organisation account.";
        if (codes.Contains("TOO_MANY_REQUESTS") || codes.Contains("HMRC-TOO_MANY_REQUESTS")
            || codes.Contains("HMRC-HTTP-429"))
            return "HMRC's request limit has been reached. The return was not accepted; wait before taking another action.";
        if (codes.Any(code => code is "SERVER_ERROR" or "SERVICE_UNAVAILABLE" or "SCHEDULED_MAINTENANCE"
                or "HMRC-SERVER_ERROR" or "HMRC-SERVICE_UNAVAILABLE" or "HMRC-SCHEDULED_MAINTENANCE"
                or "HMRC-HTTP-500" or "HMRC-HTTP-502" or "HMRC-HTTP-503" or "HMRC-HTTP-504"))
            return "HMRC's VAT service is temporarily unavailable. The approved return remains protected; do not assume it was filed.";
        return result.State == VatSubmissionResultState.Rejected
            ? $"HMRC rejected the return ({result.OutcomeCode}). Review the authority response before preparing another return."
            : $"The return was not sent ({result.OutcomeCode}). Correct the issue before trying again.";
    }
}
