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

namespace TradeControl.Web.Pages.Org.Update
{
    [Authorize(Roles = "Administrators, Managers")]
    public class CreateModel : DI_BasePageModel
    {
        const string SessionKeyReturnUrl = "_returnUrlOrgCreate";
        public string ReturnUrl
        {
            get { return HttpContext.Session.GetString(SessionKeyReturnUrl); }
            set { HttpContext.Session.SetString(SessionKeyReturnUrl, value); }
        }

        #region entry data
        public class OrgData
        {
            [Required]
            [StringLength(255)]
            [Display(Name = "Account Name")]
            public string AccountName { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "Type")]
            public string OrganisationType { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "Status")]
            public string OrganisationStatus { get; set; }
            [StringLength(10)]
            [Display(Name = "Tax Code")]
            public string TaxCode { get; set; }
            [StringLength(100)]
            [Display(Name = "Payment Terms")]
            public string PaymentTerms { get; set; }
            [Display(Name = "Payment Days")]
            public short PaymentDays { get; set; } = 0;
            [Display(Name = "Expected Days")]
            public short ExpectedDays { get; set; } = 0;
            [Display(Name = "From M/End?")]
            public bool PayDaysFromMonthEnd { get; set; } = false;
            [Required]
            [Display(Name = "Pay Balance?")]
            public bool PayBalance { get; set; } = true;
            [Display(Name = "Opening Balance")]
            [DataType(DataType.Currency)]
            public decimal OpeningBalance { get; set; } = 0;
            [StringLength(100)]
            [Display(Name = "Contact Name")]
            public string ContactName { get; set; }
            [StringLength(255)]
            [Display(Name = "Email Address")]
            [DataType(DataType.EmailAddress)]
            public string EmailAddress { get; set; }
            [StringLength(255)]
            [Display(Name = "Business Address")]
            public string BusinessAddress { get; set; }
            [StringLength(50)]
            [Display(Name = "Phone Number")]
            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }
        }

        [BindProperty]
        public OrgData OrgEntry { get; set; }

        #endregion

        public SelectList OrganisationTypes { get; set; }
        public SelectList OrganisationStatuses { get; set; }
        public SelectList TaxCodes { get; set; }

        public CreateModel(NodeContext context,
            IAuthorizationService authorizationService,
            UserManager<TradeControlWebUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public async Task OnGetAsync(string returnUrl, string organisationType)
        {
            try
            {
                await SetViewData();

                OrganisationTypes = new SelectList(await NodeContext.Org_tbTypes.OrderBy(t => t.OrganisationTypeCode).Select(t => t.OrganisationType).ToListAsync());
                OrganisationStatuses = new SelectList(await NodeContext.Org_tbStatuses.OrderBy(t => t.OrganisationStatusCode).Select(t => t.OrganisationStatus).ToListAsync());
                TaxCodes = new SelectList(await NodeContext.App_TaxCodes.OrderBy(t => t.TaxCode).Select(t => t.TaxCode).ToListAsync());

                Orgs orgs = new(NodeContext);

                string organisatonType = string.IsNullOrEmpty(organisationType) ? OrganisationTypes.First().ToString()
                                        : await NodeContext.Org_tbTypes.Where(t => t.OrganisationType == organisationType).Select(t => t.OrganisationType).FirstOrDefaultAsync();

                OrgEntry = new OrgData()
                {
                    AccountName = string.Empty,
                    OrganisationType = organisationType,
                    OrganisationStatus = NodeContext.Org_tbStatuses.Where(t => t.OrganisationStatusCode == (short)NodeEnum.OrgStatus.Active).Select(t => t.OrganisationStatus).First(),
                    TaxCode = await orgs.DefaultTaxCode()
                };

                ReturnUrl = string.IsNullOrEmpty(returnUrl) ? "./Index" : returnUrl;
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                Profile profile = new(NodeContext);
                var userName = await profile.UserName(UserManager.GetUserId(User));
                Orgs orgs = new(NodeContext);
                string accountCode = await orgs.DefaultAccountCode(OrgEntry.AccountName);

                Org_tbOrg org = new()
                {
                    AccountCode = accountCode,
                    AccountName = OrgEntry.AccountName,
                    OrganisationStatusCode = NodeContext.Org_tbStatuses.Where(t => t.OrganisationStatus == OrgEntry.OrganisationStatus).Select(t => t.OrganisationStatusCode).First(),
                    OrganisationTypeCode = NodeContext.Org_tbTypes.Where(t => t.OrganisationType == OrgEntry.OrganisationType).Select(t => t.OrganisationTypeCode).First(),
                    TaxCode = OrgEntry.TaxCode,
                    PaymentTerms = OrgEntry.PaymentTerms,
                    PaymentDays = OrgEntry.PaymentDays,
                    ExpectedDays = OrgEntry.ExpectedDays,
                    PayDaysFromMonthEnd = OrgEntry.PayDaysFromMonthEnd,
                    PayBalance = OrgEntry.PayBalance,
                    OpeningBalance = OrgEntry.OpeningBalance,
                    EmailAddress = OrgEntry.EmailAddress,
                    PhoneNumber = OrgEntry.PhoneNumber,
                    InsertedBy = userName,
                    UpdatedBy = userName,
                    InsertedOn = DateTime.Now,
                    UpdatedOn = DateTime.Now
                };

                NodeContext.Org_tbOrgs.Add(org);
                await NodeContext.SaveChangesAsync();

                orgs = new(NodeContext, accountCode);

                if (!string.IsNullOrEmpty(OrgEntry.BusinessAddress))
                    await orgs.AddAddress(OrgEntry.BusinessAddress);

                if (!string.IsNullOrEmpty(OrgEntry.ContactName))
                    await orgs.AddContact(OrgEntry.ContactName);

                RouteValueDictionary route = new();
                route.Add("AccountCode", orgs.AccountCode);

                return RedirectToPage(ReturnUrl, route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
