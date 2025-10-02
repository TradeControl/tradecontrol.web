using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Address
{
    [Authorize(Roles = "Administrators, Managers")]
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Subject_vwAddressList Address { get; set; }

        public async Task<IActionResult> OnGetAsync(string addressCode)
        {
            if (addressCode == null)
                return NotFound();

            Address = await NodeContext.Subject_AddressList.FirstOrDefaultAsync(m => m.AddressCode == addressCode);

            if (Address == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }

        }

        public async Task<IActionResult> OnPostAsync(string addressCode)
        {
            try
            {
                if (addressCode == null)
                    return NotFound();

                Subject_tbAddress tbAddress = await NodeContext.Subject_tbAddresses.FindAsync(addressCode);

                if (tbAddress != null)
                {

                    NodeContext.Subject_tbAddresses.Remove(tbAddress);
                    await NodeContext.SaveChangesAsync();
                }

                RouteValueDictionary route = new();
                route.Add("accountCode", tbAddress.SubjectCode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
