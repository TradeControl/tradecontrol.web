using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Manager.Components
{
    public enum CashManagerSection
    {
        Statement,
        CashAccounts,
        Payments,
        Assets,
        Transfers
    }

    public enum CashManagerRowStatus
    {
        Posted,
        Unposted,
        Transfer
    }

    public enum CashManagerStatementSortColumn
    {
        EntryNumber,
        PaidOn,
        Subject,
        Reference,
        Cash,
        PaidOut,
        PaidIn,
        RunningBalance,
        Status
    }

    public enum CashManagerStatementPaymentEditMode
    {
        Edit,
        Move,
        Payment,
        Delete
    }

    public sealed record CashManagerAccountSummary(
        string AccountCode,
        string AccountName,
        string SubjectCode,
        string SubjectName,
        NodeEnum.CashAccountType AccountType,
        decimal CurrentBalance,
        short LiquidityLevel,
        bool AccountClosed);

    public sealed record CashManagerYearOption(
        short YearNumber,
        string Description);

    public sealed record CashManagerPeriodOption(
        short YearNumber,
        short MonthNumber,
        DateTime StartOn,
        string Description,
        NodeEnum.CashStatus CashStatus);

    public sealed record CashManagerStatementRow(
        long? RowNumber,
        string PaymentCode,
        DateTime PaidOn,
        string SubjectCode,
        string SubjectName,
        string? ParentSubjectCode,
        string NamespacePath,
        string PaymentReference,
        string CashCode,
        string CashDescription,
        string TaxCode,
        string TaxDescription,
        string UserName,
        decimal PaidInValue,
        decimal PaidOutValue,
        decimal DeltaValue,
        decimal RunningBalance,
        CashManagerRowStatus Status,
        bool IsEditable);

    public sealed record CashManagerStatementGroup(
        string GroupKey,
        string Title,
        string NamespacePath,
        decimal PostedNet,
        decimal UnpostedNet,
        decimal ProvisionalNet,
        IReadOnlyList<CashManagerStatementRow> Rows);

    public sealed record CashManagerStatementSummary(
        decimal OpeningPostedBalance,
        decimal PostedNet,
        decimal UnpostedNet,
        decimal ProvisionalBalance,
        int PostedCount,
        int UnpostedCount);

    public sealed record CashManagerStatementResult(
        CashManagerStatementSummary Summary,
        IReadOnlyList<CashManagerStatementRow> Rows,
        IReadOnlyList<CashManagerStatementGroup> Groups)
    {
        public static CashManagerStatementResult Empty { get; } = new(
            new CashManagerStatementSummary(0m, 0m, 0m, 0m, 0, 0),
            Array.Empty<CashManagerStatementRow>(),
            Array.Empty<CashManagerStatementGroup>());
    }

    public sealed record CashManagerSelectOption(
        string Value,
        string Label);

    public sealed class CashManagerStatementPaymentEditorModel
    {
        [Required]
        [StringLength(20)]
        public string PaymentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CurrentAccountCode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string UserId { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PaidOn { get; set; }

        [StringLength(50)]
        public string PaymentReference { get; set; } = string.Empty;

        [StringLength(50)]
        public string CashCode { get; set; } = string.Empty;

        [StringLength(10)]
        public string TaxCode { get; set; } = string.Empty;

        public decimal PaidInValue { get; set; }

        public decimal PaidOutValue { get; set; }

        public bool AllowClosedPeriodOverride { get; set; }

        public bool IsClosedPeriod { get; set; }

        public NodeEnum.CashStatus PeriodStatus { get; set; }
    }

    public sealed record CashManagerStatementPaymentEditorState(
        CashManagerStatementPaymentEditorModel Model,
        IReadOnlyList<CashManagerSelectOption> Users,
        IReadOnlyList<CashManagerSelectOption> CashCodes,
        IReadOnlyList<CashManagerSelectOption> TaxCodes,
        IReadOnlyList<CashManagerSelectOption> Accounts,
        bool CanEditCashCode);

    public sealed record CashAccountLookupOption(
        string Value,
        string Label);

    public sealed record CashAccountBalanceConstraintOption(
        byte Value,
        string Label);

    public sealed record CashAccountEditorOptions(
        IReadOnlyList<CashAccountLookupOption> Organisations,
        IReadOnlyList<CashAccountLookupOption> AccountTypes,
        IReadOnlyList<CashAccountLookupOption> CashCodes,
        IReadOnlyList<CashAccountBalanceConstraintOption> BalanceConstraints);

    public sealed class CashAccountEditorModel
    {
        public bool IsNew { get; set; }

        [StringLength(10)]
        public string OriginalAccountCode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string OrganisationName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AccountName { get; set; } = string.Empty;

        [StringLength(100)]
        public string CashDescription { get; set; } = string.Empty;

        [StringLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [StringLength(10)]
        public string SortCode { get; set; } = string.Empty;

        [Range(typeof(byte), "0", "255")]
        public byte BalanceConstraintCode { get; set; }

        public short LiquidityLevel { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal CurrentBalance { get; set; }

        public bool AccountClosed { get; set; }

        public short CoinTypeCode { get; set; }

        public short AccountTypeCode { get; set; }

        public string SubjectCode { get; set; } = string.Empty;

        public bool ShowBankFields =>
            string.Equals(AccountType, nameof(NodeEnum.CashAccountType.Cash), StringComparison.OrdinalIgnoreCase);
    }

    public static class CashManagerSectionCatalog
    {
        public static IReadOnlyList<CashManagerSection> GetSections(NodeEnum.CashAccountType accountType)
        {
            return accountType switch {
                NodeEnum.CashAccountType.Cash => new[]
                {
                    CashManagerSection.Statement,
                    CashManagerSection.CashAccounts,
                    CashManagerSection.Payments,
                    CashManagerSection.Transfers
                },
                NodeEnum.CashAccountType.Asset => new[]
                {
                    CashManagerSection.Statement,
                    CashManagerSection.CashAccounts,
                    CashManagerSection.Assets
                },
                NodeEnum.CashAccountType.Dummy => new[]
                {
                    CashManagerSection.Statement,
                    CashManagerSection.CashAccounts,
                    CashManagerSection.Payments
                },
                _ => new[]
                {
                    CashManagerSection.Statement,
                    CashManagerSection.CashAccounts
                }
            };
        }

        public static string GetLabel(CashManagerSection section)
        {
            return section switch {
                CashManagerSection.Statement => "Statement",
                CashManagerSection.CashAccounts => "Cash Accounts",
                CashManagerSection.Payments => "Payments",
                CashManagerSection.Assets => "Assets",
                CashManagerSection.Transfers => "Transfers",
                _ => section.ToString()
            };
        }

        public static string GetDescription(CashManagerSection section)
        {
            return section switch {
                CashManagerSection.Statement => "Unified statement workspace for posted and unposted activity.",
                CashManagerSection.CashAccounts => "Create and maintain cash accounts within Cash Manager.",
                CashManagerSection.Payments => "Payment entry architecture for invoice settlement and miscellaneous payments.",
                CashManagerSection.Assets => "Asset entry architecture for capitalisation and follow-on depreciation workflows.",
                CashManagerSection.Transfers => "Cash-only transfer architecture between eligible account endpoints.",
                _ => string.Empty
            };
        }

        public static string GetRowStatusLabel(CashManagerRowStatus status)
        {
            return status switch {
                CashManagerRowStatus.Posted => "Posted",
                CashManagerRowStatus.Unposted => "Unposted",
                CashManagerRowStatus.Transfer => "Transfer",
                _ => status.ToString()
            };
        }

        public static string GetRowStatusBadgeClass(CashManagerRowStatus status)
        {
            return status switch {
                CashManagerRowStatus.Posted => "text-bg-success",
                CashManagerRowStatus.Unposted => "text-bg-warning",
                CashManagerRowStatus.Transfer => "text-bg-info",
                _ => "text-bg-secondary"
            };
        }

        public static string GetYearLabel(
            IReadOnlyList<CashManagerYearOption> years,
            short? selectedYearNumber)
        {
            if (!selectedYearNumber.HasValue || selectedYearNumber.Value <= 0)
            {
                return "All years";
            }

            return years.FirstOrDefault(year => year.YearNumber == selectedYearNumber.Value)?.Description
                ?? selectedYearNumber.Value.ToString();
        }

        public static string GetPeriodScopeLabel(
            IReadOnlyList<CashManagerPeriodOption> periods,
            DateTime? selectedPeriodStartOn)
        {
            if (!selectedPeriodStartOn.HasValue)
            {
                return "All months in year";
            }

            return periods.FirstOrDefault(period => period.StartOn == selectedPeriodStartOn.Value)?.Description
                ?? selectedPeriodStartOn.Value.ToString("d");
        }

        public static string GetSortLabel(CashManagerStatementSortColumn column)
        {
            return column switch {
                CashManagerStatementSortColumn.EntryNumber => "Entry",
                CashManagerStatementSortColumn.PaidOn => "Paid On",
                CashManagerStatementSortColumn.Subject => "Subject",
                CashManagerStatementSortColumn.Reference => "Reference",
                CashManagerStatementSortColumn.Cash => "Cash",
                CashManagerStatementSortColumn.PaidOut => "Paid Out",
                CashManagerStatementSortColumn.PaidIn => "Paid In",
                CashManagerStatementSortColumn.RunningBalance => "Balance",
                CashManagerStatementSortColumn.Status => "Status",
                _ => column.ToString()
            };
        }

        public static CashAccountEditorModel CreateEditor(Subject_vwCashAccount account)
        {
            return new CashAccountEditorModel {
                IsNew = false,
                OriginalAccountCode = account.AccountCode ?? string.Empty,
                AccountCode = account.AccountCode ?? string.Empty,
                OrganisationName = account.SubjectName ?? string.Empty,
                AccountType = account.AccountType ?? string.Empty,
                AccountName = account.AccountName ?? string.Empty,
                CashDescription = account.CashDescription ?? string.Empty,
                AccountNumber = account.AccountNumber ?? string.Empty,
                SortCode = account.SortCode ?? string.Empty,
                BalanceConstraintCode = account.BalanceConstraintCode,
                LiquidityLevel = account.LiquidityLevel,
                OpeningBalance = account.OpeningBalance,
                CurrentBalance = account.CurrentBalance,
                AccountClosed = account.AccountClosed,
                AccountTypeCode = account.AccountTypeCode,
                SubjectCode = account.SubjectCode ?? string.Empty
            };
        }
    }
}
