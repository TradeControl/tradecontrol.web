using System;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    // Single authoritative partial for expression keys
    public partial class CategoryTreeModel
    {
        // Synthetic expressions root (single level list)
        public const string ExpressionsNodeKey = "__EXPRESSIONS__";

        // Node key prefix for individual expression entries
        public const string ExpressionKeyPrefix = "expr:";

        // Helper to build expression node key from category code
        public static string MakeExpressionKey(string categoryCode) =>
            string.IsNullOrWhiteSpace(categoryCode) ? string.Empty : $"{ExpressionKeyPrefix}{categoryCode}";

        // Helper to test if a node key is an expression node
        public static bool IsExpressionKey(string key) =>
            !string.IsNullOrWhiteSpace(key) && key.StartsWith(ExpressionKeyPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
