using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Pages.Admin.FileTransfer
{
    [Authorize(Roles = "Administrators")]
    public class IndexModel : DI_BasePageModel
    {
        private IFileProvider FileProvider { get; }

        [Display(Name = "File Names")]
        public IList<string> FileNames { get; set; } = new List<string>();

        [Display(Name = "Content Type")]
        [BindProperty(SupportsGet = true)]
        public string ContentType { get; set; }

        public SelectList ContentTypes { get; set; }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Page")]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Page Size")]
        public int PageSize { get; set; } = 25;

        public SelectList PageSizes { get; set; }

        public int TotalItems { get; private set; }
        public int TotalPages { get; private set; }

        private static readonly int[] AllowedPageSizes = [10, 25, 50, 100];

        public IndexModel(NodeContext context, IFileProvider fileProvider) : base(context)
        {
            FileProvider = fileProvider;
        }

        public async Task OnGetAsync(string contentType)
        {
            try
            {
                await SetViewData();

                PageSize = AllowedPageSizes.Contains(PageSize) ? PageSize : 25;
                PageSizes = new SelectList(AllowedPageSizes, PageSize);

                // Add "All" option without modifying TemplateManager.ContentTypes
                var contentTypes = new List<string> { "All" };
                contentTypes.AddRange(TemplateManager.ContentTypes);
                ContentTypes = new SelectList(contentTypes);

                ContentType = string.IsNullOrWhiteSpace(contentType) ? ContentTypes.First().Text : contentType;
                if (!contentTypes.Contains(ContentType))
                    ContentType = ContentTypes.First().Text;

                var items = await GetFileNamesAsync(ContentType);

                TotalItems = items.Count;
                TotalPages = TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

                if (PageNumber < 1)
                    PageNumber = 1;
                if (PageNumber > TotalPages)
                    PageNumber = TotalPages;

                FileNames = items
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        // GET: /Admin/FileTransfer/Index?handler=SyncFiles&contentType=Images&embedded=1&returnNode=FileTransfer&pageSize=25
        public async Task<IActionResult> OnGetSyncFilesAsync(string contentType, string embedded, string returnNode, int pageNumber, int pageSize)
        {
            try
            {
                TemplateManager templateManager = new(NodeContext, FileProvider);
                await templateManager.Initialise();

                RouteValueDictionary route = new();

                if (!string.IsNullOrWhiteSpace(contentType))
                    route.Add("contentType", contentType);

                if (pageNumber > 0)
                    route.Add("pageNumber", pageNumber);

                if (pageSize > 0)
                    route.Add("pageSize", pageSize);

                if (!string.IsNullOrWhiteSpace(embedded))
                    route.Add("embedded", embedded);

                if (!string.IsNullOrWhiteSpace(returnNode))
                    route.Add("returnNode", returnNode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostSyncFiles()
        {
            try
            {
                TemplateManager templateManager = new(NodeContext, FileProvider);
                await templateManager.Initialise();

                var embedded = Request?.Form.ContainsKey("embedded") == true
                    && (string.Equals(Request.Form["embedded"], "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Request.Form["embedded"], "true", StringComparison.OrdinalIgnoreCase));

                var returnNode = Request?.Form.ContainsKey("returnNode") == true
                    ? (Request.Form["returnNode"].ToString() ?? string.Empty)
                    : string.Empty;

                RouteValueDictionary route = new();
                route.Add("contentType", ContentType);
                route.Add("pageNumber", PageNumber);
                route.Add("pageSize", PageSize);

                if (embedded)
                    route.Add("embedded", "1");

                if (!string.IsNullOrWhiteSpace(returnNode))
                    route.Add("returnNode", returnNode);

                return RedirectToPage("./Index", route);
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public IActionResult OnGetDownload(string fileName)
        {
            try
            {
                var contentTypeCode = TemplateManager.GetContentTypeFromString(ContentType);
                if (ContentType == "All" || contentTypeCode == NodeEnum.ContentType.Invalid)
                    throw new InvalidOperationException("Download requires a specific content type.");

                TemplateManager templateManager = new(NodeContext, FileProvider);
                return PhysicalFile(templateManager.GetFilePath(contentTypeCode, fileName), MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception e)
            {
                _ = NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task<List<string>> GetFileNamesAsync(string contentType)
        {
            if (string.Equals(contentType, "All", StringComparison.OrdinalIgnoreCase))
            {
                var imagesRaw = await NodeContext.Web_tbImages.Select(i => i.ImageFileName).ToListAsync();
                var docsRaw = await NodeContext.Web_tbAttachments.Select(a => a.AttachmentFileName).ToListAsync();
                var templatesRaw = await NodeContext.Web_tbTemplates.Select(t => t.TemplateFileName).ToListAsync();

                return imagesRaw
                    .Concat(docsRaw)
                    .Concat(templatesRaw)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(WebUtility.HtmlEncode)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            var code = TemplateManager.GetContentTypeFromString(contentType);

            return code switch
            {
                NodeEnum.ContentType.Images => (await NodeContext.Web_tbImages.Select(i => i.ImageFileName).ToListAsync())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(WebUtility.HtmlEncode)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

                NodeEnum.ContentType.Documents => (await NodeContext.Web_tbAttachments.Select(a => a.AttachmentFileName).ToListAsync())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(WebUtility.HtmlEncode)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

                NodeEnum.ContentType.Templates => (await NodeContext.Web_tbTemplates.Select(t => t.TemplateFileName).ToListAsync())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(WebUtility.HtmlEncode)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList(),

                _ => new List<string>()
            };
        }
    }
}
