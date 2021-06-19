using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Invoice.Register
{
    public class SummaryModel : DI_BasePageModel
    {
        public SummaryModel(NodeContext context) : base(context) { }

        public IList<Invoice_vwRegisterCashCode> Invoice_CashCodes { get; set; }

        [BindProperty]
        public string PeriodName { get; set; }
        public SelectList PeriodNames { get; set; }

        [BindProperty]
        public string CashMode { get; set; }
        public SelectList CashModes { get; set; }

        [BindProperty]
        public string CashCode { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "Invoice Total")]
        public double TotalInvoiceValue { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Display(Name = "Tax Total")]
        public double TotalTaxValue { get; set; }

        public async Task OnGetAsync(string periodName, string cashCode, string cashMode)
        {
            try
            {
                await SetViewData();

                var periodNames = from tb in NodeContext.App_Periods
                                  orderby tb.StartOn descending
                                  select tb.Description;

                PeriodNames = new SelectList(await periodNames.ToListAsync());

                CashModes = new SelectList(await NodeContext.Cash_tbModes.Where(m => m.CashModeCode < (short)NodeEnum.CashMode.Neutral)
                                                        .OrderBy(m => m.CashModeCode).Select(m => m.CashMode).ToListAsync());

                var summary = from tb in NodeContext.Invoice_RegisterCashCodes select tb;

                if (!string.IsNullOrEmpty(cashCode))
                {
                    summary = summary.Where(i => i.CashCode == cashCode);
                    CashMode = null;
                    CashCode = cashCode;
                }
                else
                {
                    DateTime startOn = DateTime.Today;
                    CashCode = null;

                    if (string.IsNullOrEmpty(periodName))
                    {
                        FinancialPeriods periods = new(NodeContext);
                        startOn = periods.ActiveStartOn;
                    }
                    else
                        startOn = await NodeContext.App_Periods.Where(t => t.Description == periodName).Select(t => t.StartOn).FirstOrDefaultAsync();

                    PeriodName = await NodeContext.App_Periods.Where(t => t.StartOn == startOn).Select(t => t.Description).FirstOrDefaultAsync();
                    summary = summary.Where(i => i.StartOn == startOn);

                    if (!string.IsNullOrEmpty(cashMode))
                        summary = summary.Where(i => i.CashMode == cashMode);
                }

                summary = from tb in summary
                          orderby tb.StartOn, tb.CashCode
                          select tb;

                Invoice_CashCodes = await summary.ToListAsync();

                TotalInvoiceValue = await summary.SumAsync(i => i.TotalInvoiceValue);
                TotalTaxValue = await summary.SumAsync(i => i.TotalTaxValue);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
