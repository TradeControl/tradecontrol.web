namespace TradeControl.Web.Pages.Invoice.Register.Models
{
    public sealed class InvoiceFilterModel
    {
        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }
        public bool ShowAll { get; set; }

        public string? InvoiceType { get; set; }
        public string? InvoiceStatus { get; set; }
        public string? Namespace { get; set; }
        public string? CashCode { get; set; }
        public string? SearchText { get; set; }
        public string? SelectedInvoiceNumber { get; set; }

        public System.DateTime? DateFrom { get; set; }
        public System.DateTime? DateTo { get; set; }

        public bool StatusDraft { get; set; }
        public bool StatusPosted { get; set; }
        public bool StatusUnsent { get; set; }
        public bool StatusUnpaid { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortField { get; set; }
        public string? SortDirection { get; set; }
    }
}
