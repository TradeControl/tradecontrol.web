using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Browser
{
    public class EditVirtualModel : DI_BasePageModel
    {
        private const short CurrentSubjectClassCode = (short)NodeEnum.SubjectClass.Virtual;

        public EditVirtualModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string SubjectCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ReturnNode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string IsEmbedded { get; set; } = "0";

        [BindProperty(SupportsGet = true)]
        public string Done { get; set; } = string.Empty;

        [BindProperty]
        public string SubjectName { get; set; } = string.Empty;

        [BindProperty]
        public short SubjectTypeCode { get; set; }

        [BindProperty]
        public short SubjectStatusCode { get; set; }

        [BindProperty]
        public string TaxCode { get; set; } = string.Empty;

        [BindProperty]
        public string PaymentTerms { get; set; } = string.Empty;

        [BindProperty]
        public short ExpectedDays { get; set; }

        [BindProperty]
        public short PaymentDays { get; set; }

        [BindProperty]
        public bool PayDaysFromMonthEnd { get; set; }

        [BindProperty]
        public bool PayBalance { get; set; }

        [BindProperty]
        public string AreaCode { get; set; } = string.Empty;

        [BindProperty]
        public string SubjectPhoneNumber { get; set; } = string.Empty;

        [BindProperty]
        public string SubjectEmailAddress { get; set; } = string.Empty;

        [BindProperty]
        public int NumberOfEmployees { get; set; }

        [BindProperty]
        public string CompanyNumber { get; set; } = string.Empty;

        [BindProperty]
        public string VatNumber { get; set; } = string.Empty;

        [BindProperty]
        public bool Eujurisdiction { get; set; }

        [BindProperty]
        public string BusinessDescription { get; set; } = string.Empty;

        [BindProperty]
        public decimal Turnover { get; set; }

        [BindProperty]
        public string WebSite { get; set; } = string.Empty;

        [BindProperty]
        public string SubjectSource { get; set; } = string.Empty;

        public IReadOnlyList<SelectListItem> SubjectTypeOptions { get; private set; } = Array.Empty<SelectListItem>();
        public IReadOnlyList<SelectListItem> SubjectStatusOptions { get; private set; } = Array.Empty<SelectListItem>();
        public IReadOnlyList<SelectListItem> TaxCodeOptions { get; private set; } = Array.Empty<SelectListItem>();
        public IReadOnlyList<string> PaymentTermOptions { get; private set; } = Array.Empty<string>();
        public IReadOnlyList<string> SubjectSourceOptions { get; private set; } = Array.Empty<string>();
        public IReadOnlyList<string> AreaCodeOptions { get; private set; } = Array.Empty<string>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(SubjectCode))
                return RedirectToPage("/Subject/Browser/Index");

            await LoadLookupsAsync();

            if (IsEmbedded == "1" && !string.IsNullOrWhiteSpace(Done))
            {
                await SetViewData();
                return Page();
            }

            var subject = await NodeContext.Subject_tbSubjects
                .Include(o => o.TbVirtual)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

            if (subject is null || subject.TbVirtual is null)
                return NotFound();

            SubjectName = subject.SubjectName ?? string.Empty;
            SubjectTypeCode = subject.SubjectTypeCode;
            SubjectStatusCode = subject.SubjectStatusCode;
            TaxCode = subject.TaxCode ?? string.Empty;
            PaymentTerms = subject.PaymentTerms ?? string.Empty;
            ExpectedDays = subject.ExpectedDays;
            PaymentDays = subject.PaymentDays;
            PayDaysFromMonthEnd = subject.PayDaysFromMonthEnd;
            PayBalance = subject.PayBalance;
            AreaCode = subject.AreaCode ?? string.Empty;
            SubjectPhoneNumber = subject.PhoneNumber ?? string.Empty;
            SubjectEmailAddress = subject.EmailAddress ?? string.Empty;

            NumberOfEmployees = subject.TbVirtual.NumberOfEmployees;
            CompanyNumber = subject.TbVirtual.CompanyNumber ?? string.Empty;
            VatNumber = subject.TbVirtual.VatNumber ?? string.Empty;
            Eujurisdiction = subject.TbVirtual.Eujurisdiction;
            BusinessDescription = subject.TbVirtual.BusinessDescription ?? string.Empty;
            Turnover = subject.TbVirtual.Turnover;
            WebSite = subject.TbVirtual.WebSite ?? string.Empty;
            SubjectSource = subject.TbVirtual.SubjectSource ?? string.Empty;

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(SubjectCode))
                return RedirectToPage("/Subject/Browser/Index");

            await LoadLookupsAsync();

            if (string.IsNullOrWhiteSpace(SubjectName))
                ModelState.AddModelError(nameof(SubjectName), "Name is required.");

            if (!await IsValidSubjectTypeAsync())
                ModelState.AddModelError(nameof(SubjectTypeCode), "Select a valid type for this Subject class.");

            if (!ModelState.IsValid)
            {
                await SetViewData();
                return Page();
            }

            try
            {
                var subject = await NodeContext.Subject_tbSubjects
                    .Include(o => o.TbVirtual)
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

                if (subject is null || subject.TbVirtual is null)
                    return NotFound();

                subject.SubjectName = SubjectName.Trim();
                subject.SubjectTypeCode = SubjectTypeCode;
                subject.SubjectStatusCode = SubjectStatusCode;
                subject.TaxCode = TaxCode?.Trim();
                subject.PaymentTerms = PaymentTerms?.Trim();
                subject.ExpectedDays = ExpectedDays;
                subject.PaymentDays = PaymentDays;
                subject.PayDaysFromMonthEnd = PayDaysFromMonthEnd;
                subject.PayBalance = PayBalance;
                subject.AreaCode = AreaCode?.Trim();
                subject.PhoneNumber = SubjectPhoneNumber?.Trim();
                subject.EmailAddress = SubjectEmailAddress?.Trim();

                subject.TbVirtual.NumberOfEmployees = NumberOfEmployees;
                subject.TbVirtual.CompanyNumber = CompanyNumber?.Trim();
                subject.TbVirtual.VatNumber = VatNumber?.Trim();
                subject.TbVirtual.Eujurisdiction = Eujurisdiction;
                subject.TbVirtual.BusinessDescription = BusinessDescription?.Trim();
                subject.TbVirtual.Turnover = Turnover;
                subject.TbVirtual.WebSite = WebSite?.Trim();
                subject.TbVirtual.SubjectSource = SubjectSource?.Trim();

                await NodeContext.SaveChangesAsync();

                if (IsEmbedded == "1")
                {
                    return RedirectToPage("/Subject/Browser/EditVirtual", new {
                        subjectCode = SubjectCode,
                        returnNode = ReturnNode,
                        isEmbedded = "1",
                        done = "save"
                    });
                }

                return RedirectToPage("/Subject/Browser/Index", new {
                    mode = "Subject",
                    select = string.IsNullOrWhiteSpace(ReturnNode) ? SubjectCode : ReturnNode
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                ModelState.AddModelError(string.Empty, "Unable to save changes.");
                await SetViewData();
                return Page();
            }
        }

        private async Task<bool> IsValidSubjectTypeAsync()
        {
            return await NodeContext.Subject_tbTypes
                .AsNoTracking()
                .AnyAsync(o => o.SubjectTypeCode == SubjectTypeCode && o.SubjectClassCode == CurrentSubjectClassCode);
        }

        private async Task LoadLookupsAsync()
        {
            SubjectTypeOptions = await NodeContext.Subject_tbTypes
                .AsNoTracking()
                .Where(o => o.SubjectClassCode == CurrentSubjectClassCode)
                .OrderBy(o => o.SubjectType)
                .Select(o => new SelectListItem(o.SubjectType, o.SubjectTypeCode.ToString(CultureInfo.InvariantCulture)))
                .ToListAsync();

            SubjectStatusOptions = await NodeContext.Subject_tbStatuses
                .AsNoTracking()
                .OrderBy(o => o.SubjectStatus)
                .Select(o => new SelectListItem(o.SubjectStatus, o.SubjectStatusCode.ToString(CultureInfo.InvariantCulture)))
                .ToListAsync();

            TaxCodeOptions = await NodeContext.App_tbTaxCodes
                .AsNoTracking()
                .OrderBy(o => o.TaxCode)
                .Select(o => new SelectListItem($"{o.TaxCode} — {o.TaxDescription}", o.TaxCode))
                .ToListAsync();

            PaymentTermOptions = await NodeContext.Subject_PaymentTerms
                .AsNoTracking()
                .OrderBy(o => o.PaymentTerms)
                .Select(o => o.PaymentTerms)
                .ToListAsync();

            SubjectSourceOptions = await NodeContext.Set<Subject_vwSubjectSource>()
                .AsNoTracking()
                .OrderBy(o => o.SubjectSource)
                .Select(o => o.SubjectSource)
                .ToListAsync();

            AreaCodeOptions = await NodeContext.Subject_AreaCodes
                .AsNoTracking()
                .OrderBy(o => o.AreaCode)
                .Select(o => o.AreaCode)
                .ToListAsync();
        }
    }
}
