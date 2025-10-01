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

namespace TradeControl.Web.Pages.Subject.Update
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
            public string SubjectType { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "Status")]
            public string SubjectStatus { get; set; }
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

        public SelectList SubjectTypes { get; set; }
        public SelectList SubjectStatuses { get; set; }
        public SelectList TaxCodes { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public CreateModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            UserManager = userManager;
        }

        public async Task OnGetAsync(string returnUrl, string SubjectType)
        {
            try
            {
                await SetViewData();

                SubjectTypes = new SelectList(await NodeContext.Subject_tbTypes.OrderBy(t => t.SubjectTypeCode).Select(t => t.SubjectType).ToListAsync());
                SubjectStatuses = new SelectList(await NodeContext.Subject_tbStatuses.OrderBy(t => t.SubjectStatusCode).Select(t => t.SubjectStatus).ToListAsync());
                TaxCodes = new SelectList(await NodeContext.App_TaxCodes.OrderBy(t => t.TaxCode).Select(t => t.TaxCode).ToListAsync());

                Subjects orgs = new(NodeContext);

                string organisatonType = string.IsNullOrEmpty(SubjectType) ? SubjectTypes.First().ToString()
                                        : await NodeContext.Subject_tbTypes.Where(t => t.SubjectType == SubjectType).Select(t => t.SubjectType).FirstOrDefaultAsync();

                OrgEntry = new OrgData()
                {
                    AccountName = string.Empty,
                    SubjectType = SubjectType,
                    SubjectStatus = NodeContext.Subject_tbStatuses.Where(t => t.SubjectStatusCode == (short)NodeEnum.SubjectStatus.Active).Select(t => t.SubjectStatus).First(),
                    TaxCode = await orgs.DefaultTaxCode()
                };

                ReturnUrl = string.IsNullOrEmpty(returnUrl) ? "./Index" : returnUrl;
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

                Profile profile = new(NodeContext);
                var userName = await profile.UserName(UserManager.GetUserId(User));
                Subjects orgs = new(NodeContext);
                string accountCode = await orgs.DefaultAccountCode(OrgEntry.AccountName);

                Subject_tbSubject subject = new()
                {
                    AccountCode = accountCode,
                    AccountName = OrgEntry.AccountName,
                    SubjectStatusCode = NodeContext.Subject_tbStatuses.Where(t => t.SubjectStatus == OrgEntry.SubjectStatus).Select(t => t.SubjectStatusCode).First(),
                    SubjectTypeCode = NodeContext.Subject_tbTypes.Where(t => t.SubjectType == OrgEntry.SubjectType).Select(t => t.SubjectTypeCode).First(),
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

                NodeContext.Subject_tbSubjects.Add(subject);
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
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
