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

namespace TradeControl.Web.Pages.Org.Address
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(NodeContext context) : base(context) {}

        [BindProperty]
        public Org_vwAddressList Address { get; set; }

        public async Task<IActionResult> OnGetAsync(string addressCode)
        {
            if (addressCode == null)
                return NotFound();

            Address = await NodeContext.Org_AddressList.FirstOrDefaultAsync(m => m.AddressCode== addressCode);

            if (Address == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }

        }
    }
}
