using System;
using System.Collections.Generic;
using TradeControl.Web.AppServices.CategoryTree;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public class CategoryDetailsVm
    {
        public string VersionId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public string CashType { get; set; } = string.Empty;
        public string CashPolarity { get; set; } = string.Empty;
        public short DisplayOrder { get; set; }
        public bool IsEnabled { get; set; }
        public int ChildTotalsCount { get; set; }
        public int CodesCount { get; set; }
        public string Namespace { get; set; } = string.Empty;

        public int ParentCount { get; set; }
        public int PrimaryParentCount { get; set; }

        public bool IsCategoryInPrimary { get; set; }
        public bool IsContextInPrimary { get; set; }
        public string PrimaryKind { get; set; } = string.Empty; // "Profit" | "VAT" | ""
        public bool ShowWarning => IsCategoryInPrimary && !IsContextInPrimary;

        public bool IsRootNode { get; set; }      // true when ParentCount == 0
        public bool IsProfitRoot { get; set; }    // true when App.tbOptions.NetProfitCode == CategoryCode
        public bool IsVatRoot { get; set; }       // true when App.tbOptions.VatCategoryCode == CategoryCode

        public bool IsRootContext { get; set; }
        public bool IsDisconnectedContext { get; set; }
        public bool IsCashCodeCategory { get; set; }
        public bool CanEditTree { get; set; }
        public bool CanCreateCashCode { get; set; }
    }

    public class CashCodeDetailsVm
    {
        public string VersionId { get; set; } = string.Empty;
        public string CashCode { get; set; } = string.Empty;
        public string CashDescription { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CashPolarity { get; set; } = string.Empty;
        public string CashType { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsCategoryEnabled { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public bool CanEditMaster { get; set; }
    }

    public class ExpressionDetailsVm
    {
        public string VersionId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Expression { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string SyntaxType { get; set; } = string.Empty;
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool CanEditCategory { get; set; }
        public bool CanManageMetadata { get; set; }
        public string ContextNotice { get; set; } = string.Empty;
    }

    public class ExpressionReferenceItemVm
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public bool UsesCode { get; set; }
    }

    public class CashCodeListItemVm
    {
        public string CashCode { get; set; } = string.Empty;
        public string CashDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public bool IsCashEnabled { get; set; }
        public bool IsCategoryEnabled { get; set; }

        public bool IsEnabled => IsCashEnabled && IsCategoryEnabled;
    }

    public class CashCodeListByPolarityVm
    {
        public List<CashCodeListItemVm> Expense { get; set; } = new();
        public List<CashCodeListItemVm> Income { get; set; } = new();
        public List<CashCodeListItemVm> Neutral { get; set; } = new();

        public int TotalCount => (Expense?.Count ?? 0) + (Income?.Count ?? 0) + (Neutral?.Count ?? 0);
    }

    public class CategoryTreeVersionWorkspaceVm
    {
        public CategoryTreeWorkspaceContext Workspace { get; set; }
        public CategoryTreeVersionReview Review { get; set; }
        public IReadOnlyDictionary<string, CategoryTreeVersionSnapshotVm> Snapshots { get; set; }
            = new Dictionary<string, CategoryTreeVersionSnapshotVm>();
    }

    public class CategoryTreeVersionSnapshotVm
    {
        public int CategoryCount { get; set; }
        public int EdgeCount { get; set; }
        public int CashCodeCount { get; set; }
        public string ProfitRoot { get; set; } = string.Empty;
        public string VatRoot { get; set; } = string.Empty;
    }
}
