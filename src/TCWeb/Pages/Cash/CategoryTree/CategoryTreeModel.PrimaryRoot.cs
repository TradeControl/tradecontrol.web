using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public partial class CategoryTreeModel
    {
        public async Task<JsonResult> OnPostSetPrimaryRootAsync([FromForm] string kind,
            [FromForm] string categoryCode, [FromForm] string versionId)
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

                if (string.IsNullOrWhiteSpace(versionId))
                    return new JsonResult(new { success = false, message = "Missing Category Tree version context" });

                short rootTypeCode = normalized == "PROFIT"
                    ? (short)NodeEnum.CategoryRootType.NetProfit
                    : (short)NodeEnum.CategoryRootType.Vat;
                var result = await CategoryTreeService.SetRootAsync(versionId, rootTypeCode,
                    categoryCode, $"{kind} root changed to {categoryCode}.",
                    HttpContext.RequestAborted);

                await SetViewData();
                return new JsonResult(new
                {
                    success = true,
                    message = $"{kind} primary root set to {categoryCode}",
                    versionId = result.VersionId,
                    status = result.Status?.ToString()
                });
            }
            catch (Exception ex)
            {
                await NodeContext.ErrorLog(ex);
                return new JsonResult(new {
                    success = false,
                    message = CategoryTreeCommandFeedback.From(ex)
                });
            }
        }
    }
}
