using System;
using System.Collections.Generic;

namespace TradeControl.Web.Data
{
    public enum SubjectBalancePosition : short
    {
        OwedToUs = 0,
        OwedByUs = 1
    }

    public enum SubjectAgeBand : short
    {
        Current = 0,
        Days1To30 = 1,
        Days31To60 = 2,
        Days61To90 = 3,
        Over90 = 4
    }

    public sealed record SubjectCurrentAgedInvoiceRow(
        string SubjectCode,
        string SubjectName,
        DateOnly AgedOn,
        decimal BusinessBalance,
        SubjectBalancePosition Position,
        decimal CurrentAmount,
        decimal Days1To30Amount,
        decimal Days31To60Amount,
        decimal Days61To90Amount,
        decimal Over90Amount,
        decimal StatementBusinessBalance,
        decimal ReconciliationResidual)
    {
        public decimal HumanBalance => Math.Abs(BusinessBalance);
        public bool IsReconciled => Math.Abs(ReconciliationResidual) < 0.00001m;
    }

    public sealed record SubjectCurrentAgedInvoiceItem(
        string InvoiceNumber,
        short InvoiceTypeCode,
        string InvoiceType,
        DateOnly InvoicedOn,
        DateOnly DueOn,
        int DaysOverdue,
        SubjectAgeBand Band,
        decimal BusinessAmount);

    public sealed record SubjectCurrentAgedInvoiceTotals(
        decimal HumanBalance,
        decimal CurrentAmount,
        decimal Days1To30Amount,
        decimal Days31To60Amount,
        decimal Days61To90Amount,
        decimal Over90Amount,
        decimal StatementEquivalent,
        decimal ReconciliationResidual);

    public sealed class SubjectCurrentAgedInvoicePage
    {
        public DateOnly AgedOn { get; init; }
        public SubjectBalancePosition Position { get; init; }
        public IReadOnlyList<SubjectCurrentAgedInvoiceRow> Items { get; init; } = Array.Empty<SubjectCurrentAgedInvoiceRow>();
        public SubjectCurrentAgedInvoiceTotals Totals { get; init; } = new(0, 0, 0, 0, 0, 0, 0, 0);
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public bool HasMorePages => PageNumber * PageSize < TotalCount;
    }

    public sealed class SubjectCurrentAgedInvoiceDetail
    {
        public required SubjectCurrentAgedInvoiceRow Summary { get; init; }
        public IReadOnlyList<SubjectCurrentAgedInvoiceItem> Items { get; init; } = Array.Empty<SubjectCurrentAgedInvoiceItem>();
        public int TotalItemCount { get; init; }
        public bool HasMoreItems => Items.Count < TotalItemCount;
    }

    public sealed record SubjectDatedBalanceRow(
        string SubjectCode,
        string SubjectName,
        DateOnly AsOfDate,
        decimal NativeStatementBalance,
        decimal BusinessBalance,
        SubjectBalancePosition Position,
        decimal HumanBalance);

    public sealed class SubjectDatedBalancePage
    {
        public DateOnly AsOfDate { get; init; }
        public SubjectBalancePosition Position { get; init; }
        public IReadOnlyList<SubjectDatedBalanceRow> Items { get; init; } = Array.Empty<SubjectDatedBalanceRow>();
        public decimal TotalHumanBalance { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public bool HasMorePages => PageNumber * PageSize < TotalCount;
    }
}
