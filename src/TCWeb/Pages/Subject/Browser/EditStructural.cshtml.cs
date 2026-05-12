using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Subject.Browser
{
    public class EditStructuralModel : DI_BasePageModel
    {
        private const short CurrentSubjectClassCode = (short)NodeEnum.SubjectClass.Structural;

        public EditStructuralModel(NodeContext context) : base(context) { }

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
        public string Notes { get; set; } = string.Empty;

        public IReadOnlyList<SelectListItem> SubjectTypeOptions { get; private set; } = Array.Empty<SelectListItem>();

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
                .Include(o => o.TbStructural)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

            if (subject is null || subject.TbStructural is null)
                return NotFound();

            SubjectName = subject.SubjectName ?? string.Empty;
            SubjectTypeCode = subject.SubjectTypeCode;
            Notes = subject.TbStructural.Notes ?? string.Empty;

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
                    .Include(o => o.TbStructural)
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

                if (subject is null || subject.TbStructural is null)
                    return NotFound();

                subject.SubjectName = SubjectName.Trim();
                subject.SubjectTypeCode = SubjectTypeCode;
                subject.TbStructural.Notes = Notes?.Trim();

                await NodeContext.SaveChangesAsync();

                if (IsEmbedded == "1")
                {
                    return RedirectToPage("/Subject/Browser/EditStructural", new {
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
        }
    }
}
