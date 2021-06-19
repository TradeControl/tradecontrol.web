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

namespace TradeControl.Web.Pages.Invoice.Template
{
    [Authorize(Roles = "Administrators")]
    public class ImageEditModel : DI_BasePageModel
    {
        /*
        const string SessionKeyTemplateId = "_TemplateId";

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
        */
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
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                TemplateManager manager = new(NodeContext);
                await manager.ImageTag(ImageTag, Web_tbImage.ImageTag);

                RouteValueDictionary route = new();
                route.Add("templateId", TemplateId);

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
