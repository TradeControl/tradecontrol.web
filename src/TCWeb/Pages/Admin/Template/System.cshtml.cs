using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Admin.Template
{
    [Authorize(Roles = "Administrators")]
    public class SystemModel : DI_BasePageModel
    {
        public SelectList TemplateOptions { get; private set; }

        [BindProperty]
        [Display(Name = "Support request template")]
        public int? SupportRequestTemplateId { get; set; }

        [BindProperty]
        [Display(Name = "User registration template")]
        public int? UserRegistrationTemplateId { get; set; }

        public SystemModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadAsync();
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
                {
                    await LoadAsync();
                    await SetViewData();
                    return Page();
                }

                var options = await NodeContext.App_tbOptions.FirstOrDefaultAsync();
                if (options == null)
                    return NotFound();

                options.SupportRequestTemplateId = SupportRequestTemplateId;
                options.UserRegistrationTemplateId = UserRegistrationTemplateId;

                NodeContext.Attach(options).State = EntityState.Modified;
                await NodeContext.SaveChangesAsync();

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                if (embedded)
                    return Redirect("/Admin/Template/System?embedded=1&done=1");

                return Redirect("/Admin/Manager/Index?node=Templates");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task LoadAsync()
        {
            var templates = await NodeContext.Web_tbTemplates
                .OrderBy(t => t.TemplateFileName)
                .Select(t => new { t.TemplateId, t.TemplateFileName })
                .ToListAsync();

            TemplateOptions = new SelectList(templates, "TemplateId", "TemplateFileName");

            var options = await NodeContext.App_tbOptions.FirstOrDefaultAsync();
            if (options != null)
            {
                SupportRequestTemplateId = options.SupportRequestTemplateId;
                UserRegistrationTemplateId = options.UserRegistrationTemplateId;
            }
        }
    }
}
