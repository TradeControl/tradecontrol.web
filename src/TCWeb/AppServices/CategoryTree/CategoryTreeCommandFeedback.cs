using System;
using Microsoft.Data.SqlClient;

namespace TradeControl.Web.AppServices.CategoryTree;

public static class CategoryTreeCommandFeedback
{
    private const string RetryMessage =
        "The Category Tree changed before this request completed. Refresh the page and try again.";

    public static string From(Exception exception)
    {
        var cause = exception.GetBaseException();

        if (cause is InvalidOperationException)
        {
            return cause.Message;
        }

        if (cause is SqlException sqlException)
        {
            if (sqlException.Number is 1205 or 1222 or 3960)
            {
                return RetryMessage;
            }

            if (sqlException.Number is >= 51000 and <= 51999)
            {
                return sqlException.Message;
            }
        }

        return "The Category Tree change could not be completed. Refresh the page and try again.";
    }
}
