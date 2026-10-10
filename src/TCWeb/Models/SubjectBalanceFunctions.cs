using System;

namespace TradeControl.Web.Models
{
    public sealed class Subject_fnCurrentAgedInvoice
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime AgedOn { get; set; }
        public decimal BusinessBalance { get; set; }
        public short PositionCode { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal Days1To30Amount { get; set; }
        public decimal Days31To60Amount { get; set; }
        public decimal Days61To90Amount { get; set; }
        public decimal Over90Amount { get; set; }
        public decimal StatementBusinessBalance { get; set; }
        public decimal ReconciliationResidual { get; set; }
    }

    public sealed class Subject_fnCurrentAgedInvoiceItem
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public short InvoiceTypeCode { get; set; }
        public string InvoiceType { get; set; } = string.Empty;
        public DateTime InvoicedOn { get; set; }
        public DateTime DueOn { get; set; }
        public int DaysOverdue { get; set; }
        public short AgeBandCode { get; set; }
        public decimal BusinessAmount { get; set; }
    }

    public sealed class Subject_fnDatedBalance
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime AsOfDate { get; set; }
        public decimal NativeStatementBalance { get; set; }
        public decimal BusinessBalance { get; set; }
        public short PositionCode { get; set; }
        public decimal HumanBalance { get; set; }
    }
}
