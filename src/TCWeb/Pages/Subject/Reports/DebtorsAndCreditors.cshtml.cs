using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;


namespace TradeControl.Web.Pages.Subject.Reports
{
    public class DebtorsAndCreditorsModel : DI_BasePageModel
    {
        [BindProperty]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty]
        public string AssetType { get; set; }
        public SelectList AssetTypes { get; set; }

        [BindProperty]
        public IList<Subject_vwBalanceSheetAudit> Subject_BalanceSheetAudit { get; set; }

        public DebtorsAndCreditorsModel(NodeContext context) : base(context) { }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public SelectList PageSizeOptions { get; set; }

        public async Task OnGetAsync(string periodName, string assetType)
        {
            try
            {
                await SetViewData();

                DateTime startOn = DateTime.Today;

                var periodNames = from tb in NodeContext.App_Periods
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());

                if (string.IsNullOrEmpty(periodName))
                {                    
                    var periods = new Data.FinancialPeriods(NodeContext);
                    startOn = periods.ActiveStartOn;
                    PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                    startOn = await NodeContext.App_Periods.Where(t => t.Description == periodName).Select(t => t.StartOn).FirstOrDefaultAsync();

                var assetTypes = from tb in NodeContext.Cash_tbAssetTypes
                                 where tb.AssetTypeCode <= (short)NodeEnum.AssetType.Creditors
                                 orderby tb.AssetTypeCode
                                 select tb.AssetType;

                AssetTypes = new SelectList(await assetTypes.ToListAsync());

                // Base query for audit (filtered by period)
                IQueryable<Subject_vwBalanceSheetAudit> audit = from tb in NodeContext.Subject_BalanceSheetAudits
                                                               where tb.StartOn == startOn
                                                               select tb;

                // apply assetType filter if provided
                if (!string.IsNullOrEmpty(assetType))
                {
                    short assetTypeCode = await NodeContext.Cash_tbAssetTypes.Where(t => t.AssetType == assetType).Select(t => t.AssetTypeCode).FirstOrDefaultAsync();
                    audit = audit.Where(t => t.AssetTypeCode == assetTypeCode);
                }

                // Page size options (10, 20, 50)
                PageSizeOptions = new SelectList(new[] { "10", "20", "50" }, PageSize.ToString());

                // protect PageSize
                if (PageSize <= 0) PageSize = 10;

                // compute totals BEFORE paging
                TotalItems = await audit.CountAsync();

                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (PageNumber < 1) PageNumber = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;

                Subject_BalanceSheetAudit = await audit
                    .OrderBy(t => t.AssetTypeCode).ThenBy(t => t.SubjectName)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
