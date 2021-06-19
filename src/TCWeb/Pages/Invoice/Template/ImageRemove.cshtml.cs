using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;
using TradeControl.Web.Mail;
using TradeControl.Web.Models;


namespace TradeControl.Web.Pages.Invoice.Template
{
    [Authorize(Roles = "Administrators")]
    public class ImageRemoveModel : DI_BasePageModel
    {
        [BindProperty]
        public Web_vwTemplateImage Web_TemplateImage { get; set; }

        public ImageRemoveModel(NodeContext context) : base(context) { }

        public async Task<IActionResult> OnGetAsync(string imageTag, int? templateId)
        {
            if (string.IsNullOrEmpty(imageTag) || templateId == null)
                return NotFound();

            Web_TemplateImage = await NodeContext.Web_TemplateImages.FirstOrDefaultAsync(t => t.ImageTag == imageTag && t.TemplateId == templateId);

            if (Web_TemplateImage == null)
                return NotFound();
            else
            {
                await SetViewData();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                TemplateManager templateManager = new TemplateManager(NodeContext);
                await templateManager.UnassignImageToTemplate(Web_TemplateImage.TemplateId, Web_TemplateImage.ImageTag);

                RouteValueDictionary route = new();
                route.Add("templateId", Web_TemplateImage.TemplateId);

                return RedirectToPage("./Images", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
