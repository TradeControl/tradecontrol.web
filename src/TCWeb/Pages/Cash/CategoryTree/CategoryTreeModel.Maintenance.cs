using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
	/// <summary>
	/// CategoryTree maintenance endpoints and helpers (state-changing logic only).
	/// </summary>
	[Authorize]
	public partial class CategoryTreeModel : DI_BasePageModel
	{
		#region Display Order
		/// <summary>
		/// Returns true if the working set's DisplayOrder is initialised (no zero values).
		/// Evaluates the correct set based on <paramref name="parentKey"/> (root, disconnected, or specific parent).
		/// </summary>
		private async Task<bool> IsDisplayOrderInitialised(string parentKey)
		{
			if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
			{
				var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
				var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

				var orders = await NodeContext.Cash_tbCategories
					.Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
					.Select(c => c.DisplayOrder)
					.ToListAsync();

				return orders.Count == 0 || orders.All(v => v > 0);
			}

			if (IsRootKey(parentKey))
			{
				var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
				var childCodes = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
				var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

				var rootOrders = await NodeContext.Cash_tbCategories
					.Where(c => !childCodes.Contains(c.CategoryCode) && c.IsEnabled != 0 && linkedSet.Contains(c.CategoryCode))
					.Select(c => c.DisplayOrder)
					.ToListAsync();

				return rootOrders.Count == 0 || rootOrders.All(v => v > 0);
			}
			else
			{
				var siblingOrders = await NodeContext.Cash_tbCategoryTotals
					.Where(t => t.ParentCode == parentKey)
					.Select(t => t.DisplayOrder)
					.ToListAsync();

				return siblingOrders.Count == 0 || siblingOrders.All(v => v > 0);
			}
		}

		/// <summary>
		/// Initialises DisplayOrder for the working set matched by <paramref name="parentKey"/>.
		/// Root/disconnected use Cash_tbCategory.DisplayOrder; child sets use Cash_tbCategoryTotal.DisplayOrder.
		/// Fallback ordering groups by CashPolarityCode then by Category.
		/// </summary>
		private async Task DisplayOrderInitialise(string parentKey)
		{
			// Disconnected set initialisation
			if (string.Equals(parentKey, DisconnectedNodeKey, StringComparison.Ordinal))
			{
				var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
				var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

				var list = await NodeContext.Cash_tbCategories
					.Where(c => c.IsEnabled != 0 && !linkedSet.Contains(c.CategoryCode))
					.OrderBy(c => c.CashPolarityCode)
					.ThenBy(c => c.Category)
					.ToListAsync();

				short i = 1;
				foreach (var c in list)
					c.DisplayOrder = i++;

				await NodeContext.SaveChangesAsync();
				return;
			}

			// Root set initialisation
			if (IsRootKey(parentKey))
			{
				var totals = await NodeContext.Cash_tbCategoryTotals.ToListAsync();
				var childCodes = new HashSet<string>(totals.Select(t => t.ChildCode).Where(s => !string.IsNullOrEmpty(s)));
				var linkedSet = new HashSet<string>(totals.SelectMany(t => new[] { t.ParentCode, t.ChildCode }).Where(s => !string.IsNullOrEmpty(s)));

				var roots = await NodeContext.Cash_tbCategories
					.Where(c => !childCodes.Contains(c.CategoryCode) && c.IsEnabled != 0 && linkedSet.Contains(c.CategoryCode))
					.OrderBy(c => c.CashPolarityCode)
					.ThenBy(c => c.Category)
					.ToListAsync();

				short i = 1;
				foreach (var c in roots)
					c.DisplayOrder = i++;

				await NodeContext.SaveChangesAsync();
			}
			else
			{
				// Child set initialisation
				var totalsForParent = await NodeContext.Cash_tbCategoryTotals
					.Where(t => t.ParentCode == parentKey)
					.ToListAsync();

				var childCodes = totalsForParent.Select(t => t.ChildCode).ToArray();
				var childCats = await NodeContext.Cash_tbCategories
					.Where(c => childCodes.Contains(c.CategoryCode))
					.ToDictionaryAsync(c => c.CategoryCode);

				var orderedTotals = totalsForParent
					.OrderBy(t => childCats[t.ChildCode].CashPolarityCode)
					.ThenBy(t => childCats[t.ChildCode].Category)
					.ToList();

				short i = 1;
				foreach (var t in orderedTotals)
					t.DisplayOrder = i++;

				await NodeContext.SaveChangesAsync();
			}
		}
		#endregion

		#region enablement
		/// <summary>
		/// Enable/Disable a node.
		/// - If key refers to a code (key starts with "code:"), toggle only that code.
		/// - If key refers to a category, toggle that category and cascade to all descendant categories only.
		/// Never updates Cash_tbCodes for category cascades.
		/// </summary>
		public async Task<JsonResult> OnPostSetEnabledAsync([FromForm] string key, [FromForm] short enabled)
		{
			try
			{
				if (!User.IsInRole(Constants.AdministratorsRole))
					return new JsonResult(new { success = false, message = "Insufficient privileges" });

				if (string.IsNullOrWhiteSpace(key))
					return new JsonResult(new { success = false, message = "Missing key" });

				// UI: enabled 1=enable, 0=disable. DB: IsEnabled 1=enabled, 0=disabled.
				short newState = enabled != 0 ? (short)1 : (short)0;

				if (key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
				{
					// Toggle a single code only
					var cashCode = key.Substring("code:".Length);
					var affected = await NodeContext.Cash_tbCodes
						.Where(c => c.CashCode == cashCode)
						.ExecuteUpdateAsync(s => s.SetProperty(c => c.IsEnabled, newState));

					return new JsonResult(new { success = affected > 0, isEnabled = newState, nodeType = "code" });
				}

				// Toggle a category and strictly cascade to descendant categories only (no codes)
				var exists = await NodeContext.Cash_tbCategories.AnyAsync(c => c.CategoryCode == key);
				if (!exists)
					return new JsonResult(new { success = false, message = "Category not found" });

				var edges = await NodeContext.Cash_tbCategoryTotals
								.Select(t => new { t.ParentCode, t.ChildCode })
								.ToListAsync();

				var stack = new Stack<string>();
				var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

				stack.Push(key);
				while (stack.Count > 0)
				{
					var cur = stack.Pop();
					if (!seen.Add(cur))
						continue;

					foreach (var ch in edges.Where(e => e.ParentCode == cur && !string.IsNullOrEmpty(e.ChildCode)).Select(e => e.ChildCode))
						stack.Push(ch);
				}

				var affectedCats = await NodeContext.Cash_tbCategories
					.Where(c => seen.Contains(c.CategoryCode))
					.ExecuteUpdateAsync(s => s.SetProperty(c => c.IsEnabled, newState));

				return new JsonResult(new { success = affectedCats > 0, isEnabled = newState, nodeType = "category", affectedCategories = affectedCats });
			}
			catch (Exception e)
			{
				await NodeContext.ErrorLog(e);
				return new JsonResult(new { success = false, message = e.Message });
			}
		}
		#endregion

		private static JsonResult NotImplemented(string action, string key, string parentKey, string nodeType = null) =>
			new(new { success = false, message = $"Not Yet Implemented: {action}", key, parentKey, nodeType });

		// View is allowed for all authenticated users (useful for mobile)
		public Task<JsonResult> OnPostViewAsync([FromForm] string key, [FromForm] string parentKey)
			=> Task.FromResult(NotImplemented("View", key, parentKey, key != null && key.StartsWith("code:", StringComparison.OrdinalIgnoreCase) ? "code" : "category"));

		// Admin-only below
		private bool IsAdmin() => User.IsInRole(Constants.AdministratorsRole);

		public Task<JsonResult> OnPostCreateTotalAsync([FromForm] string parentKey)
			=> Task.FromResult(IsAdmin() ? NotImplemented("CreateTotal", null, parentKey, "category") : new JsonResult(new { success = false, message = "Insufficient privileges" }));

		public Task<JsonResult> OnPostAddExistingTotalAsync([FromForm] string parentKey, [FromForm] string childKey)
			=> Task.FromResult(IsAdmin() ? NotImplemented("AddExistingTotal", childKey, parentKey, "category") : new JsonResult(new { success = false, message = "Insufficient privileges" }));

		public Task<JsonResult> OnPostCreateCodeAsync([FromForm] string parentKey)
			=> Task.FromResult(IsAdmin() ? NotImplemented("CreateCode", null, parentKey, "code") : new JsonResult(new { success = false, message = "Insufficient privileges" }));

		public Task<JsonResult> OnPostAddExistingCodeAsync([FromForm] string parentKey, [FromForm] string codeKey)
			=> Task.FromResult(IsAdmin() ? NotImplemented("AddExistingCode", codeKey, parentKey, "code") : new JsonResult(new { success = false, message = "Insufficient privileges" }));

		public Task<JsonResult> OnPostEditAsync([FromForm] string key)
			=> Task.FromResult(IsAdmin() ? NotImplemented("Edit", key, null) : new JsonResult(new { success = false, message = "Insufficient privileges" }));

		public Task<JsonResult> OnPostDeleteAsync([FromForm] string key, [FromForm] bool recursive = false)
			=> Task.FromResult(IsAdmin() ? NotImplemented("Delete", key, null) : new JsonResult(new { success = false, message = "Insufficient privileges" }));

        public Task<JsonResult> OnPostCreateCodeByCashCodeAsync(
            [FromForm] string siblingCashCode,
            [FromForm] string cashCode,
            [FromForm] string cashDescription)
            => Task.FromResult(
                IsAdmin()
                    ? NotImplemented("CreateCodeByCashCode", cashCode, siblingCashCode, "code")
                    : new JsonResult(new { success = false, message = "Insufficient privileges" })
            );

        public Task<JsonResult> OnPostCreateCodeByCategoryAsync(
            [FromForm] string categoryCode,
            [FromForm] string taxCode,
            [FromForm] string cashCode,
            [FromForm] string cashDescription,
            [FromForm] string templateCode = null)
            => Task.FromResult(
                IsAdmin()
                    ? NotImplemented("CreateCodeByCategory", cashCode, categoryCode, "code")
                    : new JsonResult(new { success = false, message = "Insufficient privileges" })
            );

    }
}