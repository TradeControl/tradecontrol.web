using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Template
{
    [Authorize(Roles = "Administrators")]
    public class ImagesModel : DI_BasePageModel
    {
        const string SessionKeyTemplateId= "_TemplateId";

        public int TemplateId
        {
            get
            {
                try
                {
                    int templateId = (int)HttpContext.Session.GetInt32(SessionKeyTemplateId);
                    return templateId;
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                HttpContext.Session.SetInt32(SessionKeyTemplateId, value);
            }
        }
        public IList<Web_vwTemplateImage> Web_TemplateImages { get; set; }
        public string TemplateFileName { get; set; }

        [BindProperty]
        [Display(Name = "Available Images")]
        public string ImageFileName { get; set; }
        public SelectList ImageFileNames { get; set; }

        public ImagesModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(int? templateId)
        {
            try
            {
                if (templateId == null)
                    return NotFound();

                TemplateId = (int)templateId;
                TemplateFileName = await NodeContext.Web_tbTemplates
                                        .Where(t => t.TemplateId == templateId)
                                        .Select(t => t.TemplateFileName)
                                        .SingleOrDefaultAsync();

                Web_TemplateImages = await NodeContext.Web_TemplateImages
                                        .Where(i => i.TemplateId == templateId)
                                        .OrderBy(i => i.ImageFileName)
                                        .ToListAsync();                

                var imageFileNames = NodeContext.Web_tbImages
                                        .OrderBy(t => t.ImageFileName)
                                        .Select(t => t.ImageFileName)
                                        .Except(NodeContext.Web_TemplateImages
                                            .Where(i => i.TemplateId == templateId)
                                            .Select(i => i.ImageFileName));

                ImageFileNames = new SelectList(await imageFileNames.ToListAsync());

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync(string imageFileName)
        {
            try
            {
                TemplateManager templateManager = new TemplateManager(NodeContext);

                await templateManager.AssignImageToTemplate(TemplateId, imageFileName);

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                var returnNode = Request?.Form.ContainsKey("returnNode") == true
                    ? (Request.Form["returnNode"].ToString() ?? "Templates")
                    : "Templates";

                var embeddedQs = embedded ? "embedded=1&" : string.Empty;

                return Redirect($"/Admin/Template/Images?{embeddedQs}returnNode={Uri.EscapeDataString(returnNode)}&templateId={TemplateId}");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
