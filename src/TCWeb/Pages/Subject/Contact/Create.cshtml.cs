using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Contact
{
    [Authorize(Roles = "Administrators, Managers")]
    public class CreateModel : DI_BasePageModel
    {
        #region session
        const string SessionKeyReturnUrl = "_returnUrlContact";
        const string SessionKeyInvoiceNumber = "_invoiceNumberContact";

        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        public string InvoiceNumber
        {
            get { return HttpContext.Session.GetString(SessionKeyInvoiceNumber); }
            set { HttpContext.Session.SetString(SessionKeyInvoiceNumber, value); }
        }
        #endregion

        [BindProperty]
        public Subject_tbContact Subject_tbContact { get; set; }

        [BindProperty]
        public string AccountName { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string accountCode, string returnUrl, string invoiceNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(accountCode))
                    return NotFound();

                var subject = await NodeContext.Subject_tbSubjects.FirstOrDefaultAsync(t => t.AccountCode == accountCode);

                if (subject == null)
                    return NotFound();
                else
                    AccountName = subject.AccountName;

                Profile profile = new(NodeContext);
                Subject_tbContact = new()
                {
                    AccountCode = subject.AccountCode,
                    InsertedBy = await profile.UserName(UserManager.GetUserId(User)),
                    InsertedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };

                Subject_tbContact.UpdatedBy = Subject_tbContact.InsertedBy;

                ReturnUrl = string.IsNullOrEmpty(returnUrl) ? string.Empty : returnUrl;
                InvoiceNumber = string.IsNullOrEmpty(invoiceNumber) ? string.Empty : invoiceNumber;

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                NodeContext.Subject_tbContacts.Add(Subject_tbContact);

                await NodeContext.SaveChangesAsync();

                RouteValueDictionary route = new();

                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    if (!string.IsNullOrEmpty(InvoiceNumber)) route.Add("InvoiceNumber", InvoiceNumber);
                    if (!string.IsNullOrEmpty(Subject_tbContact.EmailAddress)) route.Add("EmailAddress", Subject_tbContact.EmailAddress);

                    return RedirectToPage(string.Concat("/", ReturnUrl), route);
                }
                else
                {
                    route.Add("AccountCode", Subject_tbContact.AccountCode);

                    return RedirectToPage("./Index", route);
                }
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
