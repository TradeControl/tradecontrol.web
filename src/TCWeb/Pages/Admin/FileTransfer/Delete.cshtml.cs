using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;

using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Pages.Admin.FileTransfer
{
    [Authorize(Roles = "Administrators")]
    public class DeleteModel : DI_BasePageModel
    {
        private IFileProvider FileProvider { get; }

        [Display(Name = "Content Type")]
        [BindProperty]
        public string ContentType { get; set; }

        public IFileInfo RemoveFile { get; private set; }

        public DeleteModel(NodeContext context, IFileProvider fileProvider) : base(context)
        {
            FileProvider = fileProvider;
        }

        public async Task<IActionResult> OnGetAsync(string contentType, string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(fileName))
                    return NotFound();

                ContentType = contentType;

                var contentTypeCode = TemplateManager.GetContentTypeFromString(ContentType);

                if (contentTypeCode == NodeEnum.ContentType.Invalid)
                    return NotFound();

                TemplateManager templateManager = new(NodeContext, FileProvider);
                RemoveFile = templateManager.GetFileInfo(contentTypeCode, fileName);

                if (!RemoveFile.Exists)
                    return NotFound();

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPost(string contentType, string fileName)
        {
            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(fileName))
                return Page();

            var contentTypeCode = TemplateManager.GetContentTypeFromString(ContentType);
            
            TemplateManager templateManager = new(NodeContext, FileProvider);
            RemoveFile = templateManager.GetFileInfo(contentTypeCode, fileName);

            if (RemoveFile.Exists)
            {
                System.IO.File.Delete(RemoveFile.PhysicalPath);
                await templateManager.RemoveFile(contentTypeCode, fileName);
            }
            RouteValueDictionary route = new();
            route.Add("contentType", ContentType);

            return RedirectToPage("./Index", route);
        }
    }
}
