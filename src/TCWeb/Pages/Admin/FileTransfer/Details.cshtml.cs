using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Pages.Admin.FileTransfer
{
    [Authorize(Roles = "Administrators")]
    public class DetailsModel : DI_BasePageModel
    {
        private IFileProvider FileProvider { get; }

        [Display(Name = "Content Type")]
        [BindProperty(SupportsGet = true)]
        public string ContentType { get; set; }

        [Display(Name = "File Name")]
        [BindProperty(SupportsGet = true)]
        public string FileName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 25;

        public IFileInfo FileInfo { get; private set; }

        public DetailsModel(NodeContext context, IFileProvider fileProvider) : base(context)
        {
            FileProvider = fileProvider;
        }

        public async Task<IActionResult> OnGetAsync(string contentType, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contentType) || string.IsNullOrWhiteSpace(fileName))
                    return NotFound();

                ContentType = contentType;
                FileName = fileName;

                var contentTypeCode = TemplateManager.GetContentTypeFromString(ContentType);
                if (contentTypeCode == NodeEnum.ContentType.Invalid)
                    return NotFound();

                TemplateManager templateManager = new(NodeContext, FileProvider);
                FileInfo = templateManager.GetFileInfo(contentTypeCode, FileName);

                if (FileInfo == null || !FileInfo.Exists)
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
    }
}
