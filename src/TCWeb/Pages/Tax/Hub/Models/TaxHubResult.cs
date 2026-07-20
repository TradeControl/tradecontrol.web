using System;
using System.Collections.Generic;

namespace TradeControl.Web.Pages.Tax.Hub.Models
{
    public sealed class TaxHubResult
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = string.Empty;
        public TaxHubWorkflowState? State { get; init; }
    }

    public sealed class TaxHubDashboardModel
    {
        public string BusinessType { get; init; } = string.Empty;
        public IReadOnlyList<string> ActiveRegimes { get; init; } = Array.Empty<string>();
        public IReadOnlyList<TaxHubObligationSummary> Obligations { get; init; } = Array.Empty<TaxHubObligationSummary>();
        public IReadOnlyList<TaxHubDashboardCard> Cards { get; init; } = Array.Empty<TaxHubDashboardCard>();
    }

    public sealed class TaxHubDashboardCard
    {
        public TaxHubWorkspace Workspace { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string PrimaryValue { get; init; } = string.Empty;
        public string SecondaryValue { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
    }

    public sealed class TaxHubObligationSummary
    {
        public short TaxTypeCode { get; init; }
        public string TaxType { get; init; } = string.Empty;
        public string CashCode { get; init; } = string.Empty;
        public string CashDescription { get; init; } = string.Empty;
        public string SubjectCode { get; init; } = string.Empty;
        public string SubjectName { get; init; } = string.Empty;
        public DateTime? NextFilingDueOn { get; init; }
        public DateTime? NextPaymentDueOn { get; init; }
        public DateTime? FilingPeriodFrom { get; init; }
        public DateTime? FilingPeriodTo { get; init; }
        public DateTime? PaymentPeriodFrom { get; init; }
        public DateTime? PaymentPeriodTo { get; init; }
    }

    public sealed class TaxHubMappingHealthSummary
    {
        public int SourceCount { get; init; }
        public int ErrorCount { get; init; }
        public int WarningCount { get; init; }
        public IReadOnlyList<TaxHubMappingHealthItem> Sources { get; init; } = Array.Empty<TaxHubMappingHealthItem>();
    }

    public sealed class TaxHubMappingHealthItem
    {
        public string TaxSourceCode { get; init; } = string.Empty;
        public string SourceName { get; init; } = string.Empty;
        public int ErrorCount { get; init; }
        public int WarningCount { get; init; }
    }

    public sealed class TaxHubValidationIssue
    {
        public bool IsError { get; init; }
        public string? TagCode { get; init; }
        public string? TagName { get; init; }
        public string? CashCode { get; init; }
        public string? CategoryCode { get; init; }
        public int? HitCount { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    public sealed record TaxHubProjectedTaxDue
    {
        public decimal VatDue { get; init; }
        public decimal BusinessTaxDue { get; init; }
    }

    public sealed class TaxHubVatWorkspaceModel
    {
        public string? SelectedPeriodName { get; init; }
        public DateTime? SelectedPeriodStartOn { get; init; }
        public short? SelectedYearNumber { get; init; }
        public string? SelectedYearDescription { get; init; }
        public bool IsAllYears { get; init; }
        public bool IsAllPeriodsInYear { get; init; }
        public DateTime? ActiveStatementStartOn { get; init; }
        public IReadOnlyList<TaxHubVatTotalRow> Totals { get; init; } = Array.Empty<TaxHubVatTotalRow>();
        public IReadOnlyList<TaxHubVatStatementRow> Statement { get; init; } = Array.Empty<TaxHubVatStatementRow>();
        public IReadOnlyList<TaxHubVatPeriodRow> Periods { get; init; } = Array.Empty<TaxHubVatPeriodRow>();
    }

    public sealed class TaxHubVatTotalRow
    {
        public short YearNumber { get; init; }
        public string Description { get; init; } = string.Empty;
        public string Period { get; init; } = string.Empty;
        public DateTime StartOn { get; init; }
        public double HomeSales { get; init; }
        public double HomePurchases { get; init; }
        public double ExportSales { get; init; }
        public double ExportPurchases { get; init; }
        public double HomeSalesVat { get; init; }
        public double HomePurchasesVat { get; init; }
        public double ExportSalesVat { get; init; }
        public double ExportPurchasesVat { get; init; }
        public double VatAdjustment { get; init; }
        public double VatDue { get; init; }
    }

    public sealed class TaxHubVatStatementRow
    {
        public long RowNumber { get; init; }
        public DateTime StartOn { get; init; }
        public double VatDue { get; init; }
        public double VatPaid { get; init; }
        public decimal Balance { get; init; }
    }

    public sealed class TaxHubVatPeriodRow
    {
        public DateTime StartOn { get; init; }
        public string TaxCode { get; init; } = string.Empty;
        public double HomeSales { get; init; }
        public double HomePurchases { get; init; }
        public double ExportSales { get; init; }
        public double ExportPurchases { get; init; }
        public double HomeSalesVat { get; init; }
        public double HomePurchasesVat { get; init; }
        public double ExportSalesVat { get; init; }
        public double ExportPurchasesVat { get; init; }
        public double VatDue { get; init; }
    }

    public sealed class TaxHubAccountsWorkspaceModel
    {
        public short? SelectedYearNumber { get; init; }
        public string SelectedYearName { get; init; } = string.Empty;
        public string SelectedPreviousYearName { get; init; } = string.Empty;
        public DateTime? SelectedPeriodStartOn { get; init; }
        public string SelectedPeriodName { get; init; } = string.Empty;
        public string SelectedPreviousPeriodName { get; init; } = string.Empty;
        public bool IsYearEndBalanceSheet { get; init; }
        public IReadOnlyList<TaxHubProfitAndLossRow> AnnualProfitAndLoss { get; init; } = Array.Empty<TaxHubProfitAndLossRow>();
        public IReadOnlyList<TaxHubProfitAndLossRow> AnnualTaxTotals { get; init; } = Array.Empty<TaxHubProfitAndLossRow>();
        public IReadOnlyList<TaxHubProfitAndLossRow> MonthlyProfitAndLoss { get; init; } = Array.Empty<TaxHubProfitAndLossRow>();
        public IReadOnlyList<TaxHubProfitAndLossRow> MonthlyTaxTotals { get; init; } = Array.Empty<TaxHubProfitAndLossRow>();
        public IReadOnlyList<TaxHubProfitAndLossDetailSection> AnnualDetails { get; init; } = Array.Empty<TaxHubProfitAndLossDetailSection>();
        public IReadOnlyList<TaxHubProfitAndLossDetailSection> MonthlyDetails { get; init; } = Array.Empty<TaxHubProfitAndLossDetailSection>();
        public IReadOnlyList<TaxHubBalanceSheetRow> BalanceSheet { get; init; } = Array.Empty<TaxHubBalanceSheetRow>();
        public TaxHubAccountsValidationSummary ValidationSummary { get; init; } = new();
        public IReadOnlyList<TaxHubEquityReconciliationRow> EquityReconciliation { get; init; } = Array.Empty<TaxHubEquityReconciliationRow>();
    }

    public sealed class TaxHubProfitAndLossRow
    {
        public string CategoryCode { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public decimal CurrentValue { get; init; }
        public decimal PreviousValue { get; init; }
    }

    public sealed class TaxHubProfitAndLossDetailSection
    {
        public string CategoryCode { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public IReadOnlyList<TaxHubProfitAndLossDetailRow> Rows { get; init; } = Array.Empty<TaxHubProfitAndLossDetailRow>();
        public decimal CurrentTotal { get; init; }
        public decimal PreviousTotal { get; init; }
    }

    public sealed class TaxHubProfitAndLossDetailRow
    {
        public string CashCode { get; init; } = string.Empty;
        public string CashDescription { get; init; } = string.Empty;
        public decimal CurrentValue { get; init; }
        public decimal PreviousValue { get; init; }
    }

    public sealed class TaxHubBalanceSheetRow
    {
        public string AssetCode { get; init; } = string.Empty;
        public string AssetName { get; init; } = string.Empty;
        public decimal CurrentBalance { get; init; }
        public decimal PreviousBalance { get; init; }
        public bool IsCapital { get; init; }
    }

    public sealed class TaxHubEquityReconciliationRow
    {
        public short YearNumber { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal OpeningCapital { get; init; }
        public decimal ClosingCapital { get; init; }
        public decimal Profit { get; init; }
        public decimal BusinessTax { get; init; }
        public decimal ProfitAfterTax { get; init; }
        public decimal TaxCarry { get; init; }
        public decimal CapitalMovement { get; init; }
        public decimal OpeningSubjectPosition { get; init; }
        public decimal OpeningAccountPosition { get; init; }
        public decimal OpeningLossesCarriedForward { get; init; }
        public decimal ClosingLossesCarriedForward { get; init; }
        public decimal LossesCarriedForwardDelta { get; init; }
        public decimal CapitalDelta { get; init; }
        public decimal Variance { get; init; }
        public decimal BridgeTotal { get; init; }
        public string Status { get; init; } = string.Empty;
    }

    public sealed class TaxHubAccountsValidationSummary
    {
        public decimal Tolerance { get; init; } = 0.10m;
        public int TotalRows { get; init; }
        public int PassCount { get; init; }
        public int WarnCount { get; init; }
        public int FailCount { get; init; }
        public string Status { get; init; } = "PENDING";
    }
}
