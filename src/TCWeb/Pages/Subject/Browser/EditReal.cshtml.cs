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
    public class EditRealModel : DI_BasePageModel
    {
        private const short CurrentSubjectClassCode = (short)NodeEnum.SubjectClass.Real;

        public EditRealModel(NodeContext context) : base(context) { }

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
        public string FileAs { get; set; } = string.Empty;

        [BindProperty]
        public bool OnMailingList { get; set; }

        [BindProperty]
        public string NameTitle { get; set; } = string.Empty;

        [BindProperty]
        public string NickName { get; set; } = string.Empty;

        [BindProperty]
        public string JobTitle { get; set; } = string.Empty;

        [BindProperty]
        public string RealPhoneNumber { get; set; } = string.Empty;

        [BindProperty]
        public string MobileNumber { get; set; } = string.Empty;

        [BindProperty]
        public string RealEmailAddress { get; set; } = string.Empty;

        [BindProperty]
        public string Hobby { get; set; } = string.Empty;

        [BindProperty]
        public DateTime? DateOfBirth { get; set; }

        [BindProperty]
        public string Department { get; set; } = string.Empty;

        [BindProperty]
        public string SpouseName { get; set; } = string.Empty;

        [BindProperty]
        public string HomeNumber { get; set; } = string.Empty;

        [BindProperty]
        public string Information { get; set; } = string.Empty;

        public IReadOnlyList<SelectListItem> SubjectTypeOptions { get; private set; } = Array.Empty<SelectListItem>();
        public IReadOnlyList<SelectListItem> SubjectStatusOptions { get; private set; } = Array.Empty<SelectListItem>();
        public IReadOnlyList<SelectListItem> TaxCodeOptions { get; private set; } = Array.Empty<SelectListItem>();
        public IReadOnlyList<string> PaymentTermOptions { get; private set; } = Array.Empty<string>();
        public IReadOnlyList<string> AreaCodeOptions { get; private set; } = Array.Empty<string>();
        public IReadOnlyList<string> NameTitleOptions { get; private set; } = Array.Empty<string>();
        public IReadOnlyList<string> JobTitleOptions { get; private set; } = Array.Empty<string>();
        public IReadOnlyList<string> DepartmentOptions { get; private set; } = Array.Empty<string>();

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
                .Include(o => o.TbReal)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

            if (subject is null || subject.TbReal is null)
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

            FileAs = subject.TbReal.FileAs ?? string.Empty;
            OnMailingList = subject.TbReal.OnMailingList;
            NameTitle = subject.TbReal.NameTitle ?? string.Empty;
            NickName = subject.TbReal.NickName ?? string.Empty;
            JobTitle = subject.TbReal.JobTitle ?? string.Empty;
            RealPhoneNumber = subject.TbReal.PhoneNumber ?? string.Empty;
            MobileNumber = subject.TbReal.MobileNumber ?? string.Empty;
            RealEmailAddress = subject.TbReal.EmailAddress ?? string.Empty;
            Hobby = subject.TbReal.Hobby ?? string.Empty;
            DateOfBirth = subject.TbReal.DateOfBirth;
            Department = subject.TbReal.Department ?? string.Empty;
            SpouseName = subject.TbReal.SpouseName ?? string.Empty;
            HomeNumber = subject.TbReal.HomeNumber ?? string.Empty;
            Information = subject.TbReal.Information ?? string.Empty;

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
                    .Include(o => o.TbReal)
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

                if (subject is null || subject.TbReal is null)
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

                subject.TbReal.FileAs = FileAs?.Trim();
                subject.TbReal.OnMailingList = OnMailingList;
                subject.TbReal.NameTitle = NameTitle?.Trim();
                subject.TbReal.NickName = NickName?.Trim();
                subject.TbReal.JobTitle = JobTitle?.Trim();
                subject.TbReal.PhoneNumber = RealPhoneNumber?.Trim();
                subject.TbReal.MobileNumber = MobileNumber?.Trim();
                subject.TbReal.EmailAddress = RealEmailAddress?.Trim();
                subject.TbReal.Hobby = Hobby?.Trim();
                subject.TbReal.DateOfBirth = DateOfBirth;
                subject.TbReal.Department = Department?.Trim();
                subject.TbReal.SpouseName = SpouseName?.Trim();
                subject.TbReal.HomeNumber = HomeNumber?.Trim();
                subject.TbReal.Information = Information?.Trim();

                await NodeContext.SaveChangesAsync();

                if (IsEmbedded == "1")
                {
                    return RedirectToPage("/Subject/Browser/EditReal", new {
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

            AreaCodeOptions = await NodeContext.Subject_AreaCodes
                .AsNoTracking()
                .OrderBy(o => o.AreaCode)
                .Select(o => o.AreaCode)
                .ToListAsync();

            NameTitleOptions = await NodeContext.Subject_NameTitles
                .AsNoTracking()
                .OrderBy(o => o.NameTitle)
                .Select(o => o.NameTitle)
                .ToListAsync();

            JobTitleOptions = await NodeContext.Subject_JobTitles
                .AsNoTracking()
                .OrderBy(o => o.JobTitle)
                .Select(o => o.JobTitle)
                .ToListAsync();

            DepartmentOptions = await NodeContext.Subject_tbReals
                .AsNoTracking()
                .Select(o => o.Department)
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Distinct()
                .OrderBy(o => o)
                .ToListAsync();
        }
    }
}
