using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Admin.Template
{
    [Authorize(Roles = "Administrators")]
    public class ImageEditModel : DI_BasePageModel
    {
        [BindProperty]
        public Web_tbImage Web_tbImage { get; set; }

        [BindProperty]
        public int TemplateId { get; set; }
        [BindProperty]
        public string ImageTag {get; set; }

        public ImageEditModel(NodeContext nodeContext) : base(nodeContext) { }

        public async Task<IActionResult> OnGetAsync(string imageTag, int templateId)
        {
            try
            {
                if (string.IsNullOrEmpty(imageTag))
                    return NotFound();

                Web_tbImage = await NodeContext.Web_tbImages
                                            .Where(i => i.ImageTag == imageTag)
                                            .SingleOrDefaultAsync();

                TemplateId = templateId;
                ImageTag = imageTag;

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

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                var returnNode = Request?.Form.ContainsKey("returnNode") == true
                    ? (Request.Form["returnNode"].ToString() ?? "Templates")
                    : "Templates";

                TemplateManager manager = new(NodeContext);
                await manager.ImageTag(ImageTag, Web_tbImage.ImageTag);

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
