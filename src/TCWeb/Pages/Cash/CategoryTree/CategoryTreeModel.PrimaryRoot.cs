using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    public partial class CategoryTreeModel
    {
        public async Task<JsonResult> OnPostSetPrimaryRootAsync([FromForm] string kind, [FromForm] string categoryCode)
        {
            try
            {
                if (!IsAdmin())
                    return new JsonResult(new { success = false, message = "Insufficient privileges" });

                if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(categoryCode))
                    return new JsonResult(new { success = false, message = "Missing kind or category code" });

                var normalized = kind.Trim().ToUpperInvariant();
                if (normalized != "PROFIT" && normalized != "VAT")
                    return new JsonResult(new { success = false, message = "Unknown kind (expected Profit or VAT)" });

                // Validate category exists and enabled
                var cat = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode && c.IsEnabled != 0);
                if (cat == null)
                    return new JsonResult(new { success = false, message = "Category not found or disabled" });

                // Must be a true root: no parent edge; must participate as a parent at least once
                var hasParent = await NodeContext.Cash_tbCategoryTotals.AnyAsync(t => t.ChildCode == categoryCode);
                if (hasParent)
                    return new JsonResult(new { success = false, message = "Only root categories (no parents) can be assigned" });

                var isParentSomewhere = await NodeContext.Cash_tbCategoryTotals.AnyAsync(t => t.ParentCode == categoryCode);
                if (!isParentSomewhere)
                    return new JsonResult(new { success = false, message = "Category must be a parent in the totals graph to be a primary root" });

                // Update via EF (portable)
                var options = await NodeContext.App_tbOptions.FirstOrDefaultAsync();
                if (options == null)
                    return new JsonResult(new { success = false, message = "Options not initialised" });

                if (normalized == "PROFIT")
                    options.NetProfitCode = categoryCode;
                else
                    options.VatCategoryCode = categoryCode;

                NodeContext.Attach(options).State = EntityState.Modified;
                await NodeContext.SaveChangesAsync();

                await SetViewData();
                return new JsonResult(new { success = true, message = $"{kind} primary root set to {categoryCode}" });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                return new JsonResult(new { success = false, message = "Server error" });
            }
        }
    }
}