using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Mail
{
    public class TemplateManager
    {
        private NodeContext NodeContext { get; }
        private IFileProvider FileProvider { get; }

        private const short TemplateStatusUnknown = 0;
        private const short TemplateStatusValid = 1;
        private const short TemplateStatusInvalid = 2;

        #region initialise
        public TemplateManager(NodeContext nodeContext) : this(nodeContext, null) { }

        public TemplateManager(NodeContext nodeContext, IFileProvider fileProvider)
        {
            NodeContext = nodeContext;
            FileProvider = fileProvider;
        }

        public async Task Initialise()
        {
            bool updated = false;

            if (FileProvider == null)
                throw new Exception("FileProvider must be specified for this action");

            var templates = GetTemplates();

            foreach (var template in templates)
            {
                if (!await NodeContext.Web_tbTemplates.Where(t => t.TemplateFileName == template.Name).AnyAsync())
                {
                    Web_tbTemplate tbTemplate = new() { TemplateFileName = template.Name };
                    NodeContext.Web_tbTemplates.Add(tbTemplate);
                    updated = true;
                }
            }

            var tbTemplates = await NodeContext.Web_tbTemplates.ToListAsync();

            foreach (var tbTemplate in tbTemplates)
            {
                if (!templates.Any(t => t.Name == tbTemplate.TemplateFileName))
                {
                    NodeContext.Web_tbTemplates.Remove(tbTemplate);
                    updated = true;
                }
            }

            var documents = GetDocuments();

            foreach (var document in documents)
            {
                if (!await NodeContext.Web_tbAttachments.Where(t => t.AttachmentFileName == document.Name).AnyAsync())
                {
                    Web_tbAttachment tbAttachment = new() { AttachmentFileName = document.Name };
                    NodeContext.Web_tbAttachments.Add(tbAttachment);
                    updated = true;
                }
            }

            var tbAttachments = await NodeContext.Web_tbAttachments.ToListAsync();

            foreach (var tbAttachment in tbAttachments)
            {
                if (!documents.Any(t => t.Name == tbAttachment.AttachmentFileName))
                {
                    NodeContext.Web_tbAttachments.Remove(tbAttachment);
                    updated = true;
                }
            }

            var images = GetImages();

            foreach (var image in images)
            {
                if (!await NodeContext.Web_tbImages.Where(t => t.ImageFileName == image.Name).AnyAsync())
                {
                    Web_tbImage tbImage = new() {
                        ImageTag = await DefaultImageTag(),
                        ImageFileName = image.Name
                    };

                    NodeContext.Add(tbImage);
                    updated = true;
                }
            }

            var tbImages = await NodeContext.Web_tbImages.ToListAsync();

            foreach (var tbImage in tbImages)
            {
                if (!images.Any(t => t.Name == tbImage.ImageFileName))
                {
                    NodeContext.Web_tbImages.Remove(tbImage);
                    updated = true;
                }
            }

            if (updated)
                await NodeContext.SaveChangesAsync();

        }

        async Task<string> DefaultImageTag()
        {
            await NodeContext.SaveChangesAsync();
            int tagId = await NodeContext.Web_tbImages.CountAsync();
            string tag = string.Concat("TAG", tagId);

            while (await NodeContext.Web_tbImages.Where(t => t.ImageTag == tag).AnyAsync())
                tag = string.Concat("TAG", ++tagId);

            return tag;
        }
        #endregion


        #region get files
        public static string ImagesSubFolder { get; } = @"content\\images";
        public static string TemplatesSubFolder { get; } = @"content\\templates";
        public static string DocumentsSubFolder { get; } = @"content\\documents";


        IList<IFileInfo> GetTemplates()
        {
            return GetFiles(TemplatesSubFolder);
        }

        async Task<IFileInfo> GetTemplateFromId(int templateId)
        {
            var template = await NodeContext.Web_tbTemplates.FirstOrDefaultAsync(t => t.TemplateId == templateId);
            return FileProvider.GetFileInfo(Path.Combine(TemplatesSubFolder, template.TemplateFileName));
        }


        IList<IFileInfo> GetImages()
        {
            return GetFiles(ImagesSubFolder);
        }

        async Task<IFileInfo> GetImageFromTag(string imageTag)
        {
            var image = await NodeContext.Web_tbImages.FirstOrDefaultAsync(t => t.ImageTag == imageTag);
            return FileProvider.GetFileInfo(Path.Combine(ImagesSubFolder, image.ImageFileName));
        }

        IList<IFileInfo> GetDocuments()
        {
            return GetFiles(DocumentsSubFolder);
        }

        async Task<string> GetDocumentFromId(int attachmentId)
        {
            var attachment = await NodeContext.Web_tbAttachments.FirstOrDefaultAsync(t => t.AttachmentId == attachmentId);
            var fileInfo = FileProvider.GetFileInfo(Path.Combine(DocumentsSubFolder, attachment.AttachmentFileName));
            return fileInfo.Exists ? fileInfo.PhysicalPath : string.Empty;
        }

        IList<IFileInfo> GetFiles(string folderName)
        {
            IDirectoryContents files = FileProvider.GetDirectoryContents(folderName);

            return files.OrderBy(f => f.Name).ToList();
        }
        #endregion

        #region template images and attachments
        public async Task AssignAttatchmentToInvoice(NodeEnum.InvoiceType invoiceType, string attachmentFileName)
        {
            if (await NodeContext.Web_tbAttachments.Where(t => t.AttachmentFileName == attachmentFileName).AnyAsync())
            {
                Web_tbAttachmentInvoice attachmentInvoice = new() {
                    InvoiceTypeCode = (short)invoiceType,
                    AttachmentId = await NodeContext.Web_tbAttachments.Where(t => t.AttachmentFileName == attachmentFileName).Select(t => t.AttachmentId).SingleAsync()
                };

                NodeContext.Web_tbAttachmentInvoices.Add(attachmentInvoice);
                await NodeContext.SaveChangesAsync();
            }
        }

        public async Task UnassignAttatchmentToInvoice(NodeEnum.InvoiceType invoiceType, int attachmentId)
        {
            var attachmentInvoice = await NodeContext.Web_tbAttachmentInvoices
                                                            .Where(t => t.AttachmentId == attachmentId && t.InvoiceTypeCode == (short)invoiceType)
                                                            .FirstOrDefaultAsync();

            if (attachmentInvoice != null)
            {
                NodeContext.Web_tbAttachmentInvoices.Remove(attachmentInvoice);
                await NodeContext.SaveChangesAsync();
            }
        }

        public async Task AssignTemplateToInvoice(NodeEnum.InvoiceType invoiceType, string templateFileName)
        {
            if (await NodeContext.Web_tbTemplates.Where(t => t.TemplateFileName == templateFileName).AnyAsync())
            {
                Web_tbTemplateInvoice templateInvoice = new() {
                    InvoiceTypeCode = (short)invoiceType,
                    TemplateId = await NodeContext.Web_tbTemplates.Where(t => t.TemplateFileName == templateFileName).Select(t => t.TemplateId).SingleAsync()
                };

                NodeContext.Web_tbTemplateInvoices.Add(templateInvoice);
                await NodeContext.SaveChangesAsync();
            }
        }

        public async Task UnassignTemplateToInvoice(NodeEnum.InvoiceType invoiceType, int templateId)
        {
            var templateInvoice = await NodeContext.Web_tbTemplateInvoices
                                                .Where(t => t.TemplateId == templateId && t.InvoiceTypeCode == (short)invoiceType)
                                                .FirstOrDefaultAsync();

            if (templateInvoice != null)
            {
                NodeContext.Web_tbTemplateInvoices.Remove(templateInvoice);
                await NodeContext.SaveChangesAsync();
            }
        }

        public async Task AssignImageToTemplate(int templateId, string imageFileName)
        {
            string imageTag = await NodeContext.Web_tbImages.Where(i => i.ImageFileName == imageFileName).Select(i => i.ImageTag).SingleOrDefaultAsync();

            if (!await NodeContext.Web_tbTemplateImages.Where(t => t.TemplateId == templateId && t.ImageTag == imageTag).AnyAsync())
            {
                Web_tbTemplateImage templateImage = new() {
                    TemplateId = templateId,
                    ImageTag = imageTag
                };

                NodeContext.Web_tbTemplateImages.Add(templateImage);
                await NodeContext.SaveChangesAsync();
            }
        }

        public async Task UnassignImageToTemplate(int templateId, string imageTag)
        {
            var templateImage = await NodeContext.Web_tbTemplateImages
                                            .Where(t => t.TemplateId == templateId && t.ImageTag == imageTag)
                                            .FirstOrDefaultAsync();
            if (templateImage != null)
            {
                NodeContext.Web_tbTemplateImages.Remove(templateImage);
                await NodeContext.SaveChangesAsync();
            }
        }

        public async Task ImageTag(string imageTag, string newImageTag)
        {
            await NodeContext.Database.ExecuteSqlRawAsync("Web.proc_ImageTag @p0, @p1", parameters: new[] { imageTag, newImageTag });
        }
        #endregion

        #region get mail document and settings
        public async Task<MailDocument> GetInvoice(NodeEnum.InvoiceType invoiceType, int templateId)
        {
            NodeSettings nodeSettings = new(NodeContext);

            if (!nodeSettings.HasMailHost)
                throw new Exception("Mail host needs configuring");
            else if (FileProvider == null)
                throw new Exception("FileProvider not specified");

            var templateFile = await GetTemplateFromId(templateId);

            MailDocument mailDocument = new() { TemplateFileName = templateFile.PhysicalPath };

            mailDocument.Settings = await nodeSettings.MailHost();

            var attachments = await NodeContext.Web_tbAttachmentInvoices
                                    .Where(t => t.InvoiceTypeCode == (short)invoiceType)
                                    .Select(t => t.AttachmentId).ToListAsync();

            foreach (int attachmentId in attachments)
                mailDocument.Attachments.Add(await GetDocumentFromId(attachmentId));

            var images = await NodeContext.Web_tbTemplateImages
                                    .Where(t => t.TemplateId == templateId)
                                    .Select(t => t.ImageTag).ToListAsync();

            foreach (string imageTag in images)
            {
                var fileInfo = await GetImageFromTag(imageTag);

                MailImage mailImage = new() {
                    Tag = imageTag,
                    FileName = fileInfo.PhysicalPath
                };

                mailDocument.Images.Add(mailImage);
            }

            return mailDocument;
        }

        public async Task<MailText> GetText(string name, string emailTo, string subject, string body)
        {
            NodeSettings nodeSettings = new(NodeContext);

            if (!nodeSettings.HasMailHost)
                return null;

            MailText mailText = new() {
                Name = name,
                EmailTo = emailTo,
                Subject = subject,
                Body = body,
                Settings = await nodeSettings.MailHost()
            };

            return mailText;
        }

        public async Task<MailDocument> GetSupportRequest()
        {
            try
            {
                NodeSettings nodeSettings = new(NodeContext);

                if (!nodeSettings.HasMailHost)
                    throw new Exception("Mail host needs configuring");
                else if (FileProvider == null)
                    throw new Exception("FileProvider not specified");

                var options = await NodeContext.App_tbOptions.FirstOrDefaultAsync();
                if (options == null || options.SupportRequestTemplateId == null)
                    throw new Exception("Support request template not configured.");

                var fileInfo = await GetTemplateFromId(options.SupportRequestTemplateId.Value);
                MailDocument mailDocument = new() { TemplateFileName = fileInfo.PhysicalPath };
                mailDocument.Settings = await nodeSettings.MailHost();
                return mailDocument;
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<MailDocument> GetUserRegistration()
        {
            try
            {
                NodeSettings nodeSettings = new(NodeContext);

                if (!nodeSettings.HasMailHost)
                    throw new Exception("Mail host needs configuring");
                else if (FileProvider == null)
                    throw new Exception("FileProvider not specified");

                var options = await NodeContext.App_tbOptions.FirstOrDefaultAsync();
                if (options == null || options.UserRegistrationTemplateId == null)
                    throw new Exception("User registration template not configured.");

                var fileInfo = await GetTemplateFromId(options.UserRegistrationTemplateId.Value);
                MailDocument mailDocument = new() { TemplateFileName = fileInfo.PhysicalPath };
                mailDocument.Settings = await nodeSettings.MailHost();
                return mailDocument;
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        #endregion

        #region usage
        public async Task RegisterTemplateUsage(int templateId, NodeEnum.InvoiceType invoiceType)
        {
            var templateInvoice = await NodeContext.Web_tbTemplateInvoices
                                    .Where(t => t.TemplateId == templateId && t.InvoiceTypeCode == (short)invoiceType)
                                    .SingleAsync();

            templateInvoice.LastUsedOn = DateTime.Now;
            NodeContext.Attach(templateInvoice).State = EntityState.Modified;
            await NodeContext.SaveChangesAsync();
        }

        #endregion

        #region content types
        public static List<string> ContentTypes
        {
            get
            {
                return new() { "Documents", "Images", "Templates" };
            }
        }

        public static NodeEnum.ContentType GetContentTypeFromString(string contentType)
        {
            return contentType switch {
                "Documents" => NodeEnum.ContentType.Documents,
                "Images" => NodeEnum.ContentType.Images,
                "Templates" => NodeEnum.ContentType.Templates,
                _ => NodeEnum.ContentType.Invalid
            };
        }

        #endregion

        #region maintenance
        public string GetFilePath(NodeEnum.ContentType contentType, string fileName)
        {
            return contentType switch {
                NodeEnum.ContentType.Documents => FileProvider.GetFileInfo(Path.Combine(DocumentsSubFolder, fileName)).PhysicalPath,
                NodeEnum.ContentType.Images => FileProvider.GetFileInfo(Path.Combine(ImagesSubFolder, fileName)).PhysicalPath,
                NodeEnum.ContentType.Templates => FileProvider.GetFileInfo(Path.Combine(TemplatesSubFolder, fileName)).PhysicalPath,
                _ => string.Empty
            };
        }

        public IFileInfo GetFileInfo(NodeEnum.ContentType contentType, string fileName)
        {
            return contentType switch {
                NodeEnum.ContentType.Documents => FileProvider.GetFileInfo(Path.Combine(DocumentsSubFolder, fileName)),
                NodeEnum.ContentType.Images => FileProvider.GetFileInfo(Path.Combine(ImagesSubFolder, fileName)),
                NodeEnum.ContentType.Templates => FileProvider.GetFileInfo(Path.Combine(TemplatesSubFolder, fileName)),
                _ => null
            };
        }

        public async Task AddFile(NodeEnum.ContentType contentType, string fileName)
        {
            switch (contentType)
            {
                case NodeEnum.ContentType.Templates:
                    Web_tbTemplate tbTemplate = new() { TemplateFileName = fileName };
                    NodeContext.Web_tbTemplates.Add(tbTemplate);
                    break;
                case NodeEnum.ContentType.Documents:
                    Web_tbAttachment tbAttachment = new() { AttachmentFileName = fileName };
                    NodeContext.Web_tbAttachments.Add(tbAttachment);
                    break;
                case NodeEnum.ContentType.Images:
                    Web_tbImage tbImage = new() { ImageTag = await DefaultImageTag(), ImageFileName = fileName };
                    NodeContext.Add(tbImage);
                    break;
            }

            await NodeContext.SaveChangesAsync();
        }

        public async Task RemoveFile(NodeEnum.ContentType contentType, string fileName)
        {
            switch (contentType)
            {
                case NodeEnum.ContentType.Templates:
                    Web_tbTemplate tbTemplate = await NodeContext.Web_tbTemplates.SingleAsync(t => t.TemplateFileName == fileName);
                    NodeContext.Web_tbTemplates.Remove(tbTemplate);
                    break;
                case NodeEnum.ContentType.Documents:
                    Web_tbAttachment tbAttachment = await NodeContext.Web_tbAttachments.SingleAsync(t => t.AttachmentFileName == fileName);
                    NodeContext.Web_tbAttachments.Remove(tbAttachment);
                    break;
                case NodeEnum.ContentType.Images:
                    Web_tbImage tbImage = await NodeContext.Web_tbImages.SingleAsync(t => t.ImageFileName == fileName);
                    NodeContext.Remove(tbImage);
                    break;
            }

            await NodeContext.SaveChangesAsync();
        }

        public async Task ParseInvoiceTemplateAsync(int templateId)
        {
            if (FileProvider == null)
                throw new Exception("FileProvider must be specified for this action");

            var template = await NodeContext.Web_tbTemplates.SingleAsync(t => t.TemplateId == templateId);
            var fileInfo = FileProvider.GetFileInfo(Path.Combine(TemplatesSubFolder, template.TemplateFileName));
            if (!fileInfo.Exists)
            {
                await UpdateTemplateParseStatusAsync(template, TemplateStatusInvalid, "Template file not found.");
                return;
            }

            string html;
            using (var stream = fileInfo.CreateReadStream())
            using (var sr = new StreamReader(stream))
                html = await sr.ReadToEndAsync();

            var tokens = ExtractSquareBracketTokens(html);
            var cidTokens = ExtractCidImageTokens(html);

            var imageTags = await NodeContext.Web_tbImages.Select(i => i.ImageTag).ToListAsync();
            var imageTagSet = new HashSet<string>(imageTags, StringComparer.OrdinalIgnoreCase);

            foreach (var cid in cidTokens)
            {
                if (!imageTagSet.Contains(cid))
                {
                    await UpdateTemplateParseStatusAsync(template, TemplateStatusInvalid, $"Unknown image tag: [{cid}]");
                    return;
                }
            }

            foreach (var token in tokens)
            {
                if (cidTokens.Contains(token))
                    continue;

                if (!MailInvoice.AllowedTemplateTags.Contains(token))
                {
                    await UpdateTemplateParseStatusAsync(template, TemplateStatusInvalid, $"Unknown field tag: [{token}]");
                    return;
                }
            }

            await UpdateTemplateParseStatusAsync(template, TemplateStatusValid, null);
        }

        private static HashSet<string> ExtractSquareBracketTokens(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var matches = Regex.Matches(html, @"\[(?<tag>[A-Za-z0-9_]+)\]");
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match m in matches)
            {
                var tag = m.Groups["tag"]?.Value?.Trim();
                if (!string.IsNullOrWhiteSpace(tag))
                    set.Add(tag);
            }

            return set;
        }

        private static HashSet<string> ExtractCidImageTokens(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var matches = Regex.Matches(html, @"cid:\[(?<tag>[A-Za-z0-9_]+)\]", RegexOptions.IgnoreCase);
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match m in matches)
            {
                var tag = m.Groups["tag"]?.Value?.Trim();
                if (!string.IsNullOrWhiteSpace(tag))
                    set.Add(tag);
            }

            return set;
        }

        private async Task UpdateTemplateParseStatusAsync(Web_tbTemplate template, short statusCode, string message)
        {
            template.TemplateStatusCode = statusCode;
            template.ParsedOn = DateTime.Now;
            template.ParseMessage = string.IsNullOrWhiteSpace(message) ? null : message;

            NodeContext.Attach(template).State = EntityState.Modified;
            await NodeContext.SaveChangesAsync();
        }

        #endregion
    }
}
