using System;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public class CategoryDetailsVm
    {
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

        // Keep if you also use it elsewhere; otherwise optional
        public bool IsRootContext { get; set; }
    }

    public class CodeDetailsVm
    {
        public string CashCode { get; set; } = string.Empty;
        public string CashDescription { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CashPolarity { get; set; } = string.Empty;
        public string CashType { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsCategoryEnabled { get; set; }
        public string Namespace { get; set; } = string.Empty;
    }
}