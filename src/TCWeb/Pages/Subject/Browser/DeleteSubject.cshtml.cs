using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Browser
{
    public class DeleteSubjectModel : DI_BasePageModel
    {
        public DeleteSubjectModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string SubjectCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ReturnNode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string IsEmbedded { get; set; } = "0";

        [BindProperty(SupportsGet = true)]
        public string Done { get; set; } = string.Empty;

        [BindProperty]
        public Subject_tbSubject? Subject { get; set; }

        public string SubjectType { get; private set; } = string.Empty;
        public string SubjectClass { get; private set; } = string.Empty;
        public string? ActionMessage { get; private set; }
        public SubjectRemovalPlan? RemovalPlan { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(SubjectCode))
                return RedirectToPage("/Subject/Browser/Index");

            if (IsEmbedded == "1" && !string.IsNullOrWhiteSpace(Done))
            {
                await SetViewData();
                return Page();
            }

            if (!await LoadSubjectAsync())
                return NotFound();

            await LoadRemovalPlanAsync();
            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(SubjectCode))
                return RedirectToPage("/Subject/Browser/Index");

            try
            {
                var parentSubjectCode = ExtractSubjectCode(ReturnNode);
                var subjects = new Subjects(NodeContext, SubjectCode);
                var result = await subjects.DeleteAsync(parentSubjectCode);

                if (result.Succeeded)
                {
                    if (IsEmbedded == "1")
                    {
                        return RedirectToPage("/Subject/Browser/DeleteSubject", new {
                            subjectCode = SubjectCode,
                            returnNode = ReturnNode,
                            isEmbedded = "1",
                            done = "delete"
                        });
                    }

                    var routeValues = new Dictionary<string, object?> {
                        ["mode"] = "Subject"
                    };

                    if (!string.IsNullOrWhiteSpace(ReturnNode))
                        routeValues["select"] = ReturnNode;

                    return RedirectToPage("/Subject/Browser/Index", routeValues);
                }

                ActionMessage = result.Message;

                if (!await LoadSubjectAsync())
                    return NotFound();

                await LoadRemovalPlanAsync();
                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                ActionMessage = "Unable to delete the selected Subject.";

                if (!await LoadSubjectAsync())
                    return NotFound();

                await LoadRemovalPlanAsync();
                await SetViewData();
                return Page();
            }
        }

        private async Task<bool> LoadSubjectAsync()
        {
            Subject = await NodeContext.Subject_tbSubjects
                .Include(o => o.SubjectTypeCodeNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

            if (Subject is null)
                return false;

            SubjectType = Subject.SubjectTypeCodeNavigation?.SubjectType ?? string.Empty;
            SubjectClass = Subject.SubjectTypeCodeNavigation?.SubjectClassCode switch {
                1 => NodeEnum.SubjectClass.Real.ToString(),
                0 => NodeEnum.SubjectClass.Virtual.ToString(),
                2 => NodeEnum.SubjectClass.Structural.ToString(),
                _ => string.Empty
            };

            return true;
        }

        private async Task LoadRemovalPlanAsync()
        {
            var parentSubjectCode = ExtractSubjectCode(ReturnNode);

            var subjects = new Subjects(NodeContext, SubjectCode);
            RemovalPlan = await subjects.PreviewRemoveFromNamespaceAsync(parentSubjectCode);
        }

        private static string ExtractSubjectCode(string? branchKey)
        {
            if (string.IsNullOrWhiteSpace(branchKey))
                return string.Empty;

            var segments = branchKey.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return segments.Length == 0 ? string.Empty : segments[^1];
        }
    }
}
