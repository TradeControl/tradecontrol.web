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
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Admin.Setup
{
    [Authorize(Roles = "Administrators")]
    public class ConfigModel : DI_BasePageModel
    {
        [BindProperty]
        public App_Initialisation App_Initialisation { get; set; }

        public SelectList TemplateNames { get; set; }
        public SelectList UocNames { get; set; }
        public SelectList MonthNames { get; set; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public ConfigModel(NodeContext context, UserManager<TradeControlWebUser> userManager) : base(context) 
        {
            UserManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                TemplateNames = new SelectList(await NodeContext.App_tbTemplates
                                        .OrderBy(t => t.TemplateName)
                                        .Select(t => t.TemplateName)
                                        .ToListAsync());                

                MonthNames = new SelectList(await NodeContext.App_tbMonths.OrderBy(m => m.MonthNumber).Select(m => m.MonthName).ToListAsync());

                UocNames = new SelectList(await NodeContext.App_tbUocs.OrderBy(u => u.UocName).Select(u => u.UocName).ToListAsync());

                
                Profile profile = new(NodeContext);

                if (await NodeContext.Usr_Doc.AnyAsync() && await NodeContext.App_tbOptions.AnyAsync())
                {
                    var company = await NodeContext.Usr_Doc.Take(1).SingleOrDefaultAsync();
                    var options = await NodeContext.App_tbOptions.Take(1).SingleOrDefaultAsync();
                    var monthName = await (from y in NodeContext.App_tbYears
                                           join m in NodeContext.App_tbMonths
                                               on y.StartMonth equals m.MonthNumber
                                           orderby m.MonthNumber
                                           select m.MonthName).FirstOrDefaultAsync();

                    string userName = await profile.UserName(UserManager.GetUserId(User));

                    App_Initialisation = new()
                    {
                        TemplateName = TemplateNames.FirstOrDefault().Text,
                        AccountName = company.CompanyName,
                        BusinessAddress = company.CompanyAddress,
                        UserName = await profile.UserName(UserManager.GetUserId(User)),
                        PhoneNumber = company.CompanyPhoneNumber,
                        EmailAddress = company.CompanyEmailAddress,
                        WebSite = company.CompanyWebsite,
                        CompanyNumber = company.CompanyNumber,
                        VatNumber = company.VatNumber,
                        BankName = company.BankName,
                        CurrentAccountName = company.CurrentAccountName,
                        CAAccountNumber = company.BankAccountNumber,
                        CASortCode = company.BankSortCode,
                        CalendarCode = await NodeContext.App_tbCalendars.OrderBy(c => c.CalendarCode).Select(c => c.CalendarCode).SingleOrDefaultAsync(),
                        MonthName = monthName,
                        UocName = await NodeContext.App_tbUocs.Where(u => u.UnitOfCharge == options.UnitOfCharge).Select(u => u.UocName).SingleOrDefaultAsync()
                    };

                    var bankAddr = await (  from ca in NodeContext.Subject_CurrentAccounts
                                            join addr in NodeContext.Subject_tbAddresses
                                            on ca.AccountCode equals addr.AccountCode
                                            select addr.Address).FirstOrDefaultAsync();

                    App_Initialisation.BankAddress = bankAddr != null ? bankAddr : string.Empty;

                    var reserveAccount = await NodeContext.Subject_ReserveAccounts.OrderBy(r => r.CashAccountCode).FirstOrDefaultAsync();

                    if (reserveAccount != null)
                    {
                        App_Initialisation.ReserveAccountName = reserveAccount.CashAccountName;
                        App_Initialisation.RAAccountNumber = reserveAccount.AccountNumber;
                        App_Initialisation.RASortCode = reserveAccount.SortCode;
                    }

                    var gov = await (from t in NodeContext.Cash_tbTaxTypes
                                     join o in NodeContext.Subject_tbSubjects
                                       on t.AccountCode equals o.AccountCode
                                     orderby t.AccountCode
                                     select o.AccountName).FirstOrDefaultAsync();

                    if (gov != null)
                        App_Initialisation.Government = gov;
                }
                else
                    App_Initialisation = new()
                    {
                        TemplateName = TemplateNames.FirstOrDefault().Text,
                        MonthName = await NodeContext.App_tbMonths.Where(m => m.MonthNumber == 4).Select(m => m.MonthName).SingleAsync(),
                        UocName = await NodeContext.App_tbUocs.Where(u => u.UnitOfCharge == "GBP").Select(u => u.UocName).SingleAsync()
                    };
                
                
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
            if (!ModelState.IsValid)
                return Page();

            var user = await UserManager.GetUserAsync(User);

            await NodeContext.ConfigureNode(
                accountCode: App_Initialisation.AccountCode,
                businessName: App_Initialisation.AccountName,
                fullName: App_Initialisation.UserName,
                businessAddress: App_Initialisation.BusinessAddress,
                businessEmailAddress: App_Initialisation.EmailAddress,
                userEmailAddress: user.Email,
                phoneNumber: App_Initialisation.PhoneNumber,
                companyNumber: App_Initialisation.CompanyNumber,
                vatNumber: App_Initialisation.VatNumber,
                calendarCode: App_Initialisation.CalendarCode,
                uocName: App_Initialisation.UocName
                );

            var monthNumber = await NodeContext.App_tbMonths
                                .Where(m => m.MonthName == App_Initialisation.MonthName)
                                .Select(m => m.MonthNumber)
                                .SingleAsync();

            await NodeContext.InstallBasicSetup(
                templateName: App_Initialisation.TemplateName,
                financialMonth: monthNumber,
                govAccountName: App_Initialisation.Government,
                bankName: App_Initialisation.BankName,
                bankAddress: App_Initialisation.BankAddress,
                dummyAccount: App_Initialisation.DummyAccountName,
                currentAccount: App_Initialisation.CurrentAccountName,
                ca_SortCode: App_Initialisation.CASortCode,
                ca_AccountNumber: App_Initialisation.CAAccountNumber,
                reserveAccount: App_Initialisation.ReserveAccountName,
                ra_SortCode: App_Initialisation.RASortCode,
                ra_AccountNumber: App_Initialisation.RAAccountNumber);

            return RedirectToPage("/Index");

        }
    }

    [Keyless]
    public class App_Initialisation
    {
        [Required]
        [Display(Name = "Configuration Template")]
        public string TemplateName { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name ="Unit of Account")]
        public string UocName { get; set; }
        [Required]
        [Display(Name = "Financial Year")]
        public string MonthName { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Government")]
        public string Government { get; set; } = "HM REVENUE AND CUSTOMS";
        [Required]
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; } = "HOME";
        [Required]
        [StringLength(255)]
        [Display(Name = "Business Name")]
        public string AccountName { get; set; }
        [Required]
        [Display(Name = "Business Address")]
        public string BusinessAddress { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Your Name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        [Display(Name = "Web Site")]
        [DataType(DataType.Url)] 
        public string WebSite { get; set; }
        [StringLength(20)]
        [Display(Name = "Company Number")]
        public string CompanyNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Vat Number")]
        public string VatNumber { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Bank Address")]
        public string BankAddress { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Current Account Name")]
        public string CurrentAccountName { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Sort Code")]
        public string CASortCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Account No")]
        public string CAAccountNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Reserve Account Name")]
        public string ReserveAccountName { get; set; }
        [StringLength(10)]
        [Display(Name = "Sort Code")]
        public string RASortCode { get; set; }
        [StringLength(20)]
        [Display(Name = "Account No")]
        public string RAAccountNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Dummy Account")]
        public string DummyAccountName { get; set; } = "ADJUSTMENTS";
        [Required]
        [StringLength(10)]
        [Display(Name = "Calendar Code")]
        public string CalendarCode { get; set; } = "OFFICE";


    }

}
