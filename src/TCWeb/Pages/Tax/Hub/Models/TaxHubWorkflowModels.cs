using System;
using System.Collections.Generic;

namespace TradeControl.Web.Pages.Tax.Hub.Models
{
    public enum TaxHubWorkspace
    {
        Dashboard,
        Vat,
        BusinessTax,
        Accounts
    }

    public sealed record TaxHubWorkflowState
    {
        public TaxHubWorkspace SelectedWorkspace { get; init; } = TaxHubWorkspace.Dashboard;
        public string Title { get; init; } = string.Empty;
        public string? Subtitle { get; init; }
    }

    public sealed class TaxHubDueDateWindow
    {
        public DateTime PayOn { get; init; }
        public DateTime? PayFrom { get; init; }
        public DateTime? PayTo { get; init; }
    }

    public sealed class TaxHubYearOption
    {
        public short YearNumber { get; init; }
        public string Description { get; init; } = string.Empty;
    }

    public sealed class TaxHubPeriodOption
    {
        public short YearNumber { get; init; }
        public short MonthNumber { get; init; }
        public DateTime StartOn { get; init; }
        public string Description { get; init; } = string.Empty;
    }
}
