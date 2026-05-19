using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.Manager.Components
{
    public sealed record CashManagerPaymentEntrySummary(
        decimal UnpostedBalance,
        int EntryCount);

    public sealed class CashManagerPaymentLineModel
    {
        public bool IsExisting { get; set; }

        [StringLength(20)]
        public string PaymentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string UserId { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PaidOn { get; set; } = DateTime.Today;

        [StringLength(50)]
        public string SubjectCode { get; set; } = string.Empty;

        [StringLength(255)]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(255)]
        public string NamespaceFilter { get; set; } = string.Empty;

        [StringLength(50)]
        public string PaymentReference { get; set; } = string.Empty;

        public decimal PaidOutValue { get; set; }

        public decimal PaidInValue { get; set; }

        [StringLength(50)]
        public string CashCode { get; set; } = string.Empty;

        [StringLength(10)]
        public string TaxCode { get; set; } = string.Empty;

        public decimal OutstandingBalance { get; set; }

        public NodeEnum.CashStatus PeriodStatus { get; set; } = NodeEnum.CashStatus.Current;
    }

    public sealed class CashManagerOrganisationDraftModel
    {
        [StringLength(255)]
        public string NamespaceFilter { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; } = string.Empty;

        public short? SubjectTypeCode { get; set; }
    }

    public sealed record CashManagerOrganisationCreationResult(
        string SubjectCode,
        string SubjectName,
        string NamespaceFilter,
        decimal OutstandingBalance,
        string DefaultTaxCode);

    public sealed record CashManagerPaymentsWorkspaceState(
        CashManagerPaymentEntrySummary Summary,
        CashManagerPaymentLineModel Draft,
        IReadOnlyList<CashManagerPaymentLineModel> Rows,
        IReadOnlyList<CashManagerSelectOption> Users,
        IReadOnlyList<CashManagerSelectOption> CashCodes,
        IReadOnlyList<CashManagerSelectOption> TaxCodes,
        IReadOnlyList<CashManagerSelectOption> SubjectTypes)
    {
        public static CashManagerPaymentsWorkspaceState Empty { get; } = new(
            new CashManagerPaymentEntrySummary(0m, 0),
            new CashManagerPaymentLineModel(),
            Array.Empty<CashManagerPaymentLineModel>(),
            Array.Empty<CashManagerSelectOption>(),
            Array.Empty<CashManagerSelectOption>(),
            Array.Empty<CashManagerSelectOption>(),
            Array.Empty<CashManagerSelectOption>());
    }
}
