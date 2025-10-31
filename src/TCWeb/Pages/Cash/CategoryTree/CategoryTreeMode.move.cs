using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    public partial class CategoryTreeModel
    {
        public async Task<JsonResult> OnPostMoveAsync([FromForm] string key, [FromForm] string targetParentKey)
        {
            if (!User.IsInRole(Constants.AdministratorsRole))
            {
                return new JsonResult(new { success = false, message = "Insufficient privileges" });
            }
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(targetParentKey))
            {
                return new JsonResult(new { success = false, message = "Missing parameters." });
            }
            if (string.Equals(key, targetParentKey, System.StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { success = false, message = "Invalid target." });
            }

            try
            {
                // Block moves into Cash Type subtree (UI already prevents this)
                if (IsTypeParent(targetParentKey))
                {
                    return new JsonResult(new { success = false, message = "Move not allowed in Cash Type view." });
                }

                // Validate source and target exist and are enabled
                var cats = await NodeContext.Cash_tbCategories
                    .Where(c => (c.CategoryCode == key || c.CategoryCode == targetParentKey) && c.IsEnabled != 0)
                    .Select(c => c.CategoryCode)
                    .ToListAsync();

                if (!cats.Contains(key) || !cats.Contains(targetParentKey))
                {
                    return new JsonResult(new { success = false, message = "Category not found or disabled." });
                }

                // Current parent (if any)
                var oldParentKey = await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == key)
                    .Select(t => t.ParentCode)
                    .FirstOrDefaultAsync();

                await using var tx = await NodeContext.Database.BeginTransactionAsync();

                // Remove existing mapping(s)
                await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ChildCode == key)
                    .ExecuteDeleteAsync();

                // Insert under target with next display order
                short nextOrder = (short)(((await NodeContext.Cash_tbCategoryTotals
                    .Where(t => t.ParentCode == targetParentKey)
                    .MaxAsync(t => (short?)t.DisplayOrder)) ?? (short)0) + 1);

                NodeContext.Cash_tbCategoryTotals.Add(new Cash_tbCategoryTotal
                {
                    ParentCode = targetParentKey,
                    ChildCode = key,
                    DisplayOrder = nextOrder
                });

                await NodeContext.SaveChangesAsync();
                await tx.CommitAsync();

                return new JsonResult(new
                {
                    success = true,
                    oldParentKey = oldParentKey ?? string.Empty,
                    newParentKey = targetParentKey
                });
            }
            catch (System.Exception e)
            {
                await NodeContext.ErrorLog(e);
                return new JsonResult(new { success = false, message = "Server error." });
            }
        }
    }
}