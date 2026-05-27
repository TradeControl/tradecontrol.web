using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TradeControl.Web.Pages.Cash.Manager.Components
{
    public sealed record CashManagerTransferEntrySummary(
        decimal CurrentBalance,
        decimal UnpostedNet,
        int UnpostedCount);

    public sealed record CashManagerTransferCodeOption(
        string CashCode,
        string CashDescription,
        short CashPolarityCode);

    public sealed record CashManagerTransferAccountOption(
        string AccountCode,
        string AccountName);

    public sealed class CashManagerTransferDraftModel
    {
        [StringLength(10)]
        public string DestinationAccountCode { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PaidOn { get; set; } = DateTime.Today;

        [StringLength(255)]
        public string PaymentReference { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999999999.99999")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string PaidOutCashCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string PaidInCashCode { get; set; } = string.Empty;
    }

    public sealed record CashManagerTransferRow(
        string PaymentCode,
        string UserName,
        DateTime PaidOn,
        string PaymentReference,
        string CashCode,
        string CashDescription,
        decimal PaidOutValue,
        decimal PaidInValue);

    public sealed record CashManagerTransfersWorkspaceState(
        CashManagerTransferEntrySummary Summary,
        CashManagerTransferDraftModel Draft,
        IReadOnlyList<CashManagerTransferAccountOption> DestinationAccounts,
        IReadOnlyList<CashManagerTransferCodeOption> PaidOutTransferCodes,
        IReadOnlyList<CashManagerTransferCodeOption> PaidInTransferCodes,
        IReadOnlyList<CashManagerTransferRow> Rows)
    {
        public static CashManagerTransfersWorkspaceState Empty { get; } = new(
            new CashManagerTransferEntrySummary(0m, 0m, 0),
            new CashManagerTransferDraftModel(),
            Array.Empty<CashManagerTransferAccountOption>(),
            Array.Empty<CashManagerTransferCodeOption>(),
            Array.Empty<CashManagerTransferCodeOption>(),
            Array.Empty<CashManagerTransferRow>());
    }
}
