using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TradeControl.Web.Pages.Cash.Manager.Components
{
    public sealed record CashManagerAssetEntrySummary(
        decimal CurrentBalance,
        decimal UnpostedNet,
        int UnpostedCount);

    public sealed record CashManagerAssetCodeOption(
        string CashCode,
        string CashDescription,
        string CashPolarity,
        string CashType,
        short CashPolarityCode);

    public sealed record CashManagerAssetSubjectSelection(
        string SubjectCode,
        string SubjectName,
        string NamespaceFilter);

    public sealed record CashManagerPostedPaymentSearchResult(
        string PaymentCode,
        DateTime PaidOn,
        string AccountCode,
        string AccountName,
        string SubjectCode,
        string SubjectName,
        string PaymentReference,
        string CashCode,
        string CashDescription,
        decimal PaidOutValue,
        decimal PaidInValue,
        decimal DeltaValue);

    public sealed class CashManagerAssetDraftModel
    {
        [StringLength(255)]
        public string NamespaceFilter { get; set; } = string.Empty;

        [StringLength(50)]
        public string SubjectCode { get; set; } = string.Empty;

        [StringLength(255)]
        public string SubjectName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PaidOn { get; set; } = DateTime.Today;

        [StringLength(50)]
        public string CashCode { get; set; } = string.Empty;

        [StringLength(10)]
        public string TaxCode { get; set; } = string.Empty;

        [StringLength(255)]
        public string PaymentReference { get; set; } = string.Empty;

        public decimal PaidOutValue { get; set; }

        public decimal PaidInValue { get; set; }

        public bool GenerateReversalSeries { get; set; }

        public int ReversalPeriods { get; set; } = 12;

        public int ReversalIntervalMonths { get; set; } = 1;

        [DataType(DataType.Date)]
        public DateTime ReversalStartOn { get; set; } = DateTime.Today;
    }

    public sealed record CashManagerAssetRow(
        string PaymentCode,
        string UserName,
        DateTime PaidOn,
        string SubjectCode,
        string SubjectName,
        string PaymentReference,
        string CashCode,
        string CashDescription,
        decimal PaidOutValue,
        decimal PaidInValue);

    public sealed record CashManagerAssetsWorkspaceState(
        CashManagerAssetEntrySummary Summary,
        CashManagerAssetDraftModel Draft,
        IReadOnlyList<CashManagerAssetCodeOption> CashCodes,
        IReadOnlyList<CashManagerAssetRow> Rows)
    {
        public static CashManagerAssetsWorkspaceState Empty { get; } = new(
            new CashManagerAssetEntrySummary(0m, 0m, 0),
            new CashManagerAssetDraftModel(),
            Array.Empty<CashManagerAssetCodeOption>(),
            Array.Empty<CashManagerAssetRow>());
    }
}
