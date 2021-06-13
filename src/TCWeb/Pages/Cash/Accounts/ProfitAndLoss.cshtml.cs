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

namespace TradeControl.Web.Pages.Cash.Accounts
{
    public class ProfitAndLossModel : DI_BasePageModel
    {
        public ProfitAndLossModel(NodeContext context, IAuthorizationService authorizationService, UserManager<TradeControlWebUser> userManager) : base(context, authorizationService, userManager) { }

        [BindProperty(SupportsGet = true)]
        public string YearName { get; set; }
        public SelectList YearNames { get; set; }

        [BindProperty]
        public string YearNamePrevious { get; set; }

        public partial class ProfitAndLosses
        {
            [StringLength(10)]
            [Display(Name = "Category Code")]
            public string CategoryCode { get; set; }
            [StringLength(50)]
            [Display(Name = "Category")]
            public string Category { get; set; }
            [DataType(DataType.Currency)]
            public decimal CurrentValue { get; set; }
            [DataType(DataType.Currency)]
            public decimal PreviousValue { get; set; }
        }

        public IList<ProfitAndLosses> Cash_ProfitAndLoss { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var yearNames = from tb in NodeContext.App_tbYears
                                where tb.CashStatusCode > 0 && tb.CashStatusCode < 3
                                orderby tb.YearNumber descending
                                select tb.Description;

                YearNames = new SelectList(await yearNames.ToListAsync());
                Cash_ProfitAndLoss = new List<ProfitAndLosses>();

                short yearNumber;

                if (string.IsNullOrEmpty(YearName))
                {
                    FinancialPeriods periods = new(NodeContext);
                    yearNumber = periods.ActiveYear;
                    YearName = await NodeContext.App_tbYears.Where(t => t.YearNumber == yearNumber).Select(t => t.Description).FirstOrDefaultAsync();
                }
                else
                    yearNumber = await NodeContext.App_tbYears.Where(t => t.Description == YearName).Select(t => t.YearNumber).FirstOrDefaultAsync();

                await GenerateProfitAndLoss(yearNumber);

                await SetViewData();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task GenerateProfitAndLoss(short yearNumber)
        {
            var profit_and_loss = await (from tb in NodeContext.Cash_ProfitAndLossByYear
                                       orderby tb.YearNumber descending, tb.DisplayOrder
                                       select tb
                                        ).ToListAsync();

            foreach (var invoice_value in profit_and_loss.Where(b => b.YearNumber == yearNumber))
            {
                Cash_ProfitAndLoss.Add(new ProfitAndLosses()
                {
                    CategoryCode = invoice_value.CategoryCode,
                    Category = invoice_value.Category,
                    CurrentValue = invoice_value.InvoiceValue
                });
            }

            if (profit_and_loss.Where(b => b.YearNumber < yearNumber).Any())
            {
                yearNumber = profit_and_loss.Where(b => b.YearNumber < yearNumber).Max(b => b.YearNumber);

                YearNamePrevious = await NodeContext.App_tbYears
                                                        .Where(t => t.YearNumber == yearNumber)
                                                        .Select(t => t.Description).FirstOrDefaultAsync();

                foreach (var invoice_value in profit_and_loss.Where(b => b.YearNumber == yearNumber))
                {
                    var category = Cash_ProfitAndLoss.Where(a => a.CategoryCode == invoice_value.CategoryCode).First();
                    category.PreviousValue = invoice_value.InvoiceValue;
                }
            }
        }
    }
}
