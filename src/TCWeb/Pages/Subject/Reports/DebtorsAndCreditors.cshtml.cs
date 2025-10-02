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
                    FinancialPeriods periods = new(NodeContext);
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

                var audit = from tb in NodeContext.Subject_BalanceSheetAudits
                            where tb.StartOn == startOn
                            orderby tb.AssetTypeCode, tb.SubjectName
                            select tb;

                if (!string.IsNullOrEmpty(assetType))
                {
                    short assetTypeCode = await NodeContext.Cash_tbAssetTypes.Where(t => t.AssetType == assetType).Select(t => t.AssetTypeCode).FirstOrDefaultAsync();
                    Subject_BalanceSheetAudit = await audit.Where(t => t.AssetTypeCode == assetTypeCode).ToListAsync();
                }
                else
                    Subject_BalanceSheetAudit = await audit.ToListAsync();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
