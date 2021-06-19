using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Pages.Admin.FileTransfer
{
    [Authorize(Roles = "Administrators")]
    public class IndexModel : DI_BasePageModel
    {
        private IFileProvider FileProvider { get; }
        private IDirectoryContents ContentFiles { get; set;  }

        const string SessionKeyContentType = "_ContentType";
        
        public NodeEnum.ContentType ContentTypeCode
        {
            get
            {
                try
                {
                    var contentType = HttpContext.Session.GetInt32(SessionKeyContentType);
                    return (NodeEnum.ContentType)contentType;
                }
                catch
                {
                    return NodeEnum.ContentType.Invalid;
                }
            }
            set
            {
                int contentType = (int)value;
                HttpContext.Session.SetInt32(SessionKeyContentType, contentType);
            }
        }

        [Display(Name = "File Names")]
        public IList<string> FileNames { get; set; } = new List<string>();

        [Display(Name ="Content Type")]
        [BindProperty(SupportsGet = true)]
        public string ContentType { get; set; }
        public SelectList ContentTypes { get; set; }

        public IndexModel(NodeContext context, IFileProvider fileProvider) : base(context)
        {
            FileProvider = fileProvider;
        }

        public async Task OnGetAsync(string contentType)
        {
            try
            {
                await SetViewData();

                ContentTypes = new SelectList(TemplateManager.ContentTypes);

                if (string.IsNullOrEmpty(contentType))
                    ContentType = ContentTypes.First().Text;
                else
                    ContentType = contentType;

                ContentTypeCode = TemplateManager.GetContentTypeFromString(ContentType);                

                FileNames = ContentTypeCode switch
                {
                    NodeEnum.ContentType.Images => await NodeContext.Web_tbImages.Select(i => WebUtility.HtmlEncode(i.ImageFileName)).ToListAsync(),
                    NodeEnum.ContentType.Documents => await NodeContext.Web_tbAttachments.Select(a => WebUtility.HtmlEncode(a.AttachmentFileName)).ToListAsync(),
                    NodeEnum.ContentType.Templates => await NodeContext.Web_tbTemplates.Select(t => WebUtility.HtmlEncode(t.TemplateFileName)).ToListAsync(),
                    _ => new List<string>()
                };


                ContentFiles = ContentTypeCode switch
                {
                    NodeEnum.ContentType.Images => FileProvider.GetDirectoryContents(TemplateManager.ImagesSubFolder),
                    NodeEnum.ContentType.Documents => FileProvider.GetDirectoryContents(TemplateManager.DocumentsSubFolder),
                    NodeEnum.ContentType.Templates => FileProvider.GetDirectoryContents(TemplateManager.TemplatesSubFolder),
                    _ => null
                };                

            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }


        public async Task<IActionResult> OnPostSyncFiles()
        {
            try
            {
                TemplateManager templateManager = new(NodeContext, FileProvider);
                await templateManager.Initialise();

                RouteValueDictionary route = new();
                route.Add("contentType", ContentType);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }

        public IActionResult OnGetDownload(string fileName)
        {
            try
            {
                TemplateManager templateManager = new(NodeContext, FileProvider);
                return PhysicalFile(templateManager.GetFilePath(ContentTypeCode, fileName), MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception e)
            {
                NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
