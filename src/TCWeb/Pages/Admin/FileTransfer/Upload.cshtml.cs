using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Pages.Admin.FileTransfer
{
    [Authorize(Roles = "Administrators")]
    public class UploadModel : DI_BasePageModel
    {
        private IFileProvider FileProvider { get; }

        private readonly long maxFileSize;

        [Display(Name = "Maximum Bytes")]
        [BindProperty]
        public string MaxFileSizeMb
        {
            get
            {
                var megabyteSizeLimit = maxFileSize / 1048576;
                return $"{megabyteSizeLimit:N1} MB";
            }
        }

        [Display(Name = "Permitted Extensions")]
        [BindProperty]
        public string FileExtensions
        {
            get
            {
                string extTag = string.Empty;
                foreach (var ext in PermittedExtensions(TemplateManager.GetContentTypeFromString(ContentType)))
                    if (extTag.Length == 0)
                        extTag += ext;
                    else
                        extTag += $", {ext}";

                return extTag;
            }
        }

        [Required]
        [Display(Name = "File")]
        public List<IFormFile> FormFiles { get; set; }

        [Display(Name = "Content Type")]
        [BindProperty]
        public string ContentType { get; set; }

        public string Result { get; private set; }

        public UploadModel(NodeContext context, IFileProvider fileProvider, IConfiguration config) : base(context)
        {
            FileProvider = fileProvider;
            maxFileSize = int.Parse(config.GetSection("Settings")["MaxFileSize"]);
        }

        public async Task<IActionResult> OnGetAsync(string contentType)
        {
            try
            {
                ContentType = contentType;

                var contentTypeCode = TemplateManager.GetContentTypeFromString(ContentType);

                if (contentTypeCode == NodeEnum.ContentType.Invalid)
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

        List<string> PermittedExtensions(NodeEnum.ContentType contentType)
        {
            List<string> permittedExtensions = new List<string>();

            switch (contentType)
            {
                case NodeEnum.ContentType.Images:
                    permittedExtensions.Add(".jpg");
                    permittedExtensions.Add(".png");
                    permittedExtensions.Add(".gif");
                    break;
                case NodeEnum.ContentType.Documents:
                    permittedExtensions.Add(".pdf");
                    break;
                case NodeEnum.ContentType.Templates:
                    permittedExtensions.Add(".html");
                    break;
            }
            ;

            return permittedExtensions;
        }

        public async Task<IActionResult> OnPostUploadAsync(string contentType)
        {
            var embedded = Request?.Form.ContainsKey("embedded") == true
                && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

            var returnNode = Request?.Form.ContainsKey("returnNode") == true
                ? (Request.Form["returnNode"].ToString() ?? string.Empty)
                : string.Empty;

            if (!ModelState.IsValid)
            {
                Result = "Invalid upload";
                await SetViewData();
                ContentType = contentType;
                return Page();
            }

            TemplateManager templateManager = new(NodeContext, FileProvider);
            NodeEnum.ContentType contentTypeCode = TemplateManager.GetContentTypeFromString(contentType);
            var permittedExtensions = PermittedExtensions(contentTypeCode).ToArray();

            foreach (var formFile in FormFiles)
            {
                var formFileContent = await FileHelper.ProcessFormFile(formFile, ModelState, permittedExtensions, maxFileSize);

                if (!ModelState.IsValid)
                {
                    Result = "Invalid upload";
                    await SetViewData();
                    ContentType = contentType;
                    return Page();
                }

                string filePath = templateManager.GetFilePath(contentTypeCode, formFile.FileName);

                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await fileStream.WriteAsync(formFileContent);
                }

                FileInfo fileInfo = new(filePath);
                await templateManager.AddFile(contentTypeCode, fileInfo.Name);
            }

            RouteValueDictionary route = new();
            route.Add("contentType", ContentType);

            if (embedded)
                route.Add("embedded", "1");

            if (!string.IsNullOrWhiteSpace(returnNode))
                route.Add("returnNode", returnNode);

            return RedirectToPage("./Index", route);
        }
    }
}
