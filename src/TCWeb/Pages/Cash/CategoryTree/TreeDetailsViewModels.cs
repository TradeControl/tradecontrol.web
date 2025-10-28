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
    }
}