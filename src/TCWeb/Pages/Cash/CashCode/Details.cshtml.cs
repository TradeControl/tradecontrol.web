using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CashCode
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) { }

        public Cash_vwCode Cash_Code { get; set; }

        public async Task<IActionResult> OnGetAsync(string cashCode)
        {
            if (cashCode == null)
                return NotFound();            

            Cash_Code = await NodeContext.Cash_Codes.Where(c => c.CashCode == cashCode).FirstOrDefaultAsync();

            if (Cash_Code == null)
                return NotFound();
            else
            {                
                await SetViewData();
                return Page();
            }
        }
    }
}
