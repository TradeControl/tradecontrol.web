using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public IList<Subject_vwBalanceSheetAudit> Subject_BalanceSheetAudit { get; set; } = new List<Subject_vwBalanceSheetAudit>();

        public DebtorsAndCreditorsModel(NodeContext context) : base(context) { }

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

                var periodNames = from tb in NodeContext.App_Periods
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());

                DateTime startOn;

                if (string.IsNullOrWhiteSpace(periodName))
                {
                    var periods = new FinancialPeriods(NodeContext);
                    startOn = periods.ActiveStartOn;
                    PeriodName = await NodeContext.App_Periods
                        .Where(t => t.StartOn == startOn)
                        .Select(t => t.Description)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    PeriodName = periodName;
                    startOn = await NodeContext.App_Periods
                        .Where(t => t.Description == periodName)
                        .Select(t => t.StartOn)
                        .FirstOrDefaultAsync();
                }

                var assetTypes = from tb in NodeContext.Cash_tbAssetTypes
                                 where tb.AssetTypeCode <= (short)NodeEnum.AssetType.Creditors
                                 orderby tb.AssetTypeCode
                                 select tb.AssetType;

                AssetTypes = new SelectList(await assetTypes.ToListAsync());
                AssetType = assetType;

                IQueryable<Subject_vwBalanceSheetAudit> audit = NodeContext.Subject_BalanceSheetAudits
                    .Where(t => t.StartOn == startOn);

                if (!string.IsNullOrWhiteSpace(assetType))
                {
                    var assetTypeCode = await NodeContext.Cash_tbAssetTypes
                        .Where(t => t.AssetType == assetType)
                        .Select(t => t.AssetTypeCode)
                        .FirstOrDefaultAsync();

                    audit = audit.Where(t => t.AssetTypeCode == assetTypeCode);
                }

                PageSizeOptions = new SelectList(new[] { "10", "20", "50" }, PageSize.ToString());

                if (PageSize <= 0)
                {
                    PageSize = 10;
                }

                TotalItems = await audit.CountAsync();
                TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

                if (TotalPages == 0)
                {
                    TotalPages = 1;
                }

                if (PageNumber < 1)
                {
                    PageNumber = 1;
                }

                if (PageNumber > TotalPages)
                {
                    PageNumber = TotalPages;
                }

                Subject_BalanceSheetAudit = await audit
                    .OrderBy(t => t.AssetTypeCode)
                    .ThenBy(t => t.ParentSubjectName)
                    .ThenBy(t => t.ParentSubjectCode)
                    .ThenBy(t => t.SubjectName)
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
