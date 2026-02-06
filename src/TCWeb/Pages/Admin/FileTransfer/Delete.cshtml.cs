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
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPost(string contentType, string fileName, int pageNumber, int pageSize)
        {
            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(fileName))
                return Page();

            var embedded = Request?.Form.ContainsKey("embedded") == true
                && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

            var returnNode = Request?.Form.ContainsKey("returnNode") == true
                ? (Request.Form["returnNode"].ToString() ?? string.Empty)
                : string.Empty;

            ContentType = contentType;

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

            if (pageNumber > 0)
                route.Add("pageNumber", pageNumber);

            if (pageSize > 0)
                route.Add("pageSize", pageSize);

            if (embedded)
                route.Add("embedded", "1");

            if (!string.IsNullOrWhiteSpace(returnNode))
                route.Add("returnNode", returnNode);

            return RedirectToPage("./Index", route);
        }
    }
}
