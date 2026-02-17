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

        #region template parsing contracts
        public sealed record TemplateParseReport(
            IReadOnlyList<string> InvalidFieldTags,
            IReadOnlyList<string> MissingEmbedDirectives,
            IReadOnlyList<string> MissingEmbedTemplates,
            IReadOnlyList<string> InvalidEmbedTemplates,
            IReadOnlyList<string> MissingRequiredOutputTags,
            IReadOnlyList<string> ImageTagsWithoutAssignment,
            IReadOnlyList<string> AssignedImageTagsWithoutUsage,
            IReadOnlyList<string> AssignedImagesMissingFiles,
            IReadOnlyList<string> UnusedAvailableFields)
        {
            public bool HasErrors =>
                InvalidFieldTags.Count > 0
                || MissingEmbedDirectives.Count > 0
                || MissingEmbedTemplates.Count > 0
                || InvalidEmbedTemplates.Count > 0
                || MissingRequiredOutputTags.Count > 0
                || ImageTagsWithoutAssignment.Count > 0
                || AssignedImagesMissingFiles.Count > 0;

            public bool HasWarnings =>
                AssignedImageTagsWithoutUsage.Count > 0
                || UnusedAvailableFields.Count > 0;
        }

        public interface ITemplateParseProfile
        {
            IReadOnlySet<string> AllowedFieldTags { get; }
            IReadOnlySet<string> RequiredEmbeds { get; } // e.g. { "Details", "Tax" }
            IReadOnlySet<string> RequiredOutputTags { get; } // e.g. { "InvoiceDetailsHtml", "TaxSummaryHtml" }
        }
        #endregion

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

        public async Task<MailDocument> GetUserRegistrationConfirm()
        {
            try
            {
                NodeSettings nodeSettings = new(NodeContext);

                if (!nodeSettings.HasMailHost)
                    throw new Exception("Mail host needs configuring");
                else if (FileProvider == null)
                    throw new Exception("FileProvider not specified");

                var options = await NodeContext.App_tbOptions.FirstOrDefaultAsync();
                if (options == null || options.UserRegistrationConfirmTemplateId == null)
                    throw new Exception("User registration confirmation template not configured.");

                var fileInfo = await GetTemplateFromId(options.UserRegistrationConfirmTemplateId.Value);
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

        public async Task<MailDocument> GetUserRegistrationAdminNotify()
        {
            try
            {
                NodeSettings nodeSettings = new(NodeContext);

                if (!nodeSettings.HasMailHost)
                    throw new Exception("Mail host needs configuring");
                else if (FileProvider == null)
                    throw new Exception("FileProvider not specified");

                var options = await NodeContext.App_tbOptions.FirstOrDefaultAsync();
                if (options == null || options.UserRegistrationAdminNotifyTemplateId == null)
                    throw new Exception("User registration admin notification template not configured.");

                var fileInfo = await GetTemplateFromId(options.UserRegistrationAdminNotifyTemplateId.Value);
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

        public async Task<TemplateParseReport> ParseTemplateAsync(int templateId, ITemplateParseProfile profile)
        {
            if (FileProvider == null)
                throw new Exception("FileProvider must be specified for this action");

            var template = await NodeContext.Web_tbTemplates.SingleAsync(t => t.TemplateId == templateId);
            var fileInfo = FileProvider.GetFileInfo(Path.Combine(TemplatesSubFolder, template.TemplateFileName));
            if (!fileInfo.Exists)
            {
                await UpdateTemplateParseStatusAsync(template, TemplateStatusInvalid, "Template file not found.");
                return new TemplateParseReport(
                    InvalidFieldTags: Array.Empty<string>(),
                    MissingEmbedDirectives: Array.Empty<string>(),
                    MissingEmbedTemplates: Array.Empty<string>(),
                    InvalidEmbedTemplates: Array.Empty<string>(),
                    MissingRequiredOutputTags: Array.Empty<string>(),
                    ImageTagsWithoutAssignment: Array.Empty<string>(),
                    AssignedImageTagsWithoutUsage: Array.Empty<string>(),
                    AssignedImagesMissingFiles: Array.Empty<string>(),
                    UnusedAvailableFields: Array.Empty<string>());
            }

            string html;
            using (var stream = fileInfo.CreateReadStream())
            using (var sr = new StreamReader(stream))
                html = await sr.ReadToEndAsync();

            var fieldTokens = ExtractSquareBracketTokens(html);

            var imgCidTags = ExtractImgCidImageTags(html);
            var imgCidTagSet = new HashSet<string>(imgCidTags, StringComparer.OrdinalIgnoreCase);

            fieldTokens.ExceptWith(imgCidTagSet);

            var embeds = ExtractEmbedDirectives(html); // embedName -> templateKey

            var missingEmbedDirectives = profile.RequiredEmbeds
                .Where(req => !embeds.ContainsKey(req))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var missingEmbedTemplates = new List<string>();
            foreach (var req in profile.RequiredEmbeds)
            {
                if (!embeds.TryGetValue(req, out var key) || string.IsNullOrWhiteSpace(key))
                    continue;

                var embeddedFileName = $"{key}.tpl";
                var embeddedFile = FileProvider.GetFileInfo(Path.Combine(TemplatesSubFolder, embeddedFileName));
                if (!embeddedFile.Exists)
                    missingEmbedTemplates.Add(embeddedFileName);
            }
            missingEmbedTemplates.Sort(StringComparer.OrdinalIgnoreCase);

            var invalidEmbedTemplates = new List<string>();
            foreach (var req in profile.RequiredEmbeds.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                if (!embeds.TryGetValue(req, out var key) || string.IsNullOrWhiteSpace(key))
                    continue;

                var embeddedFileName = $"{key}.tpl";

                if (missingEmbedTemplates.Contains(embeddedFileName))
                    continue;

                var embeddedFile = FileProvider.GetFileInfo(Path.Combine(TemplatesSubFolder, embeddedFileName));
                if (!embeddedFile.Exists)
                    continue;

                string embeddedHtml;
                using (var stream = embeddedFile.CreateReadStream())
                using (var sr = new StreamReader(stream))
                    embeddedHtml = await sr.ReadToEndAsync();

                var failures = ValidateEmbeddedTemplateHtml(embeddedFileName, embeddedHtml);
                if (failures.Count > 0)
                    invalidEmbedTemplates.AddRange(failures);
            }
            invalidEmbedTemplates = invalidEmbedTemplates.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            invalidEmbedTemplates.Sort(StringComparer.OrdinalIgnoreCase);

            var invalidFieldTags = fieldTokens
                .Where(t => !IsEmbedDirectiveToken(t) && !profile.AllowedFieldTags.Contains(t))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var missingRequiredOutputTags = profile.RequiredOutputTags
                .Where(t => !fieldTokens.Contains(t))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var unusedAvailableFields = profile.AllowedFieldTags
                .Where(a => !profile.RequiredOutputTags.Contains(a))
                .Where(a => !fieldTokens.Contains(a))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var assignedImageTags = await NodeContext.Web_tbTemplateImages
                .Where(ti => ti.TemplateId == templateId)
                .Select(ti => ti.ImageTag)
                .ToListAsync();

            var assignedImageTagSet = new HashSet<string>(assignedImageTags, StringComparer.OrdinalIgnoreCase);

            var imageTagsWithoutAssignment = imgCidTagSet
                .Where(t => !assignedImageTagSet.Contains(t))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var assignedImageTagsWithoutUsage = assignedImageTagSet
                .Where(t => !imgCidTagSet.Contains(t))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var assignedImagesMissingFiles = new List<string>();
            foreach (var tag in assignedImageTagSet)
            {
                var imageFile = await GetImageFromTag(tag);
                if (imageFile == null || !imageFile.Exists)
                    assignedImagesMissingFiles.Add(tag);
            }
            assignedImagesMissingFiles.Sort(StringComparer.OrdinalIgnoreCase);

            var report = new TemplateParseReport(
                InvalidFieldTags: invalidFieldTags,
                MissingEmbedDirectives: missingEmbedDirectives,
                MissingEmbedTemplates: missingEmbedTemplates,
                InvalidEmbedTemplates: invalidEmbedTemplates,
                MissingRequiredOutputTags: missingRequiredOutputTags,
                ImageTagsWithoutAssignment: imageTagsWithoutAssignment,
                AssignedImageTagsWithoutUsage: assignedImageTagsWithoutUsage,
                AssignedImagesMissingFiles: assignedImagesMissingFiles,
                UnusedAvailableFields: unusedAvailableFields);

            var status = report.HasErrors ? TemplateStatusInvalid : TemplateStatusValid;
            var message = report.HasErrors
                ? BuildFirstErrorMessage(report)
                : null;

            await UpdateTemplateParseStatusAsync(template, status, message);

            return report;
        }

        private static IReadOnlyList<string> ValidateEmbeddedTemplateHtml(string embeddedFileName, string embeddedHtml)
        {
            var failures = new List<string>();

            if (string.IsNullOrWhiteSpace(embeddedHtml))
            {
                failures.Add($"{embeddedFileName}: empty template");
                return failures;
            }

            var startOk = embeddedHtml.IndexOf("<!--ITEM-->", StringComparison.OrdinalIgnoreCase) >= 0;
            var endOk = embeddedHtml.IndexOf("<!--/ITEM-->", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!startOk || !endOk)
                failures.Add($"{embeddedFileName}: missing repeating block markers <!--ITEM--> and <!--/ITEM-->");

            if (embeddedHtml.IndexOf("[Items]", StringComparison.OrdinalIgnoreCase) < 0)
                failures.Add($"{embeddedFileName}: missing [Items] placeholder");

            return failures;
        }

        private static string BuildFirstErrorMessage(TemplateParseReport report)
        {
            if (report.InvalidFieldTags.Count > 0)
                return $"Unknown field tag: [{report.InvalidFieldTags[0]}]";

            if (report.MissingEmbedDirectives.Count > 0)
                return $"Missing embedded template directive: [Embed:{report.MissingEmbedDirectives[0]}=...]";

            if (report.MissingEmbedTemplates.Count > 0)
                return $"Embedded template not found: {report.MissingEmbedTemplates[0]}";

            if (report.InvalidEmbedTemplates.Count > 0)
                return $"Embedded template invalid: {report.InvalidEmbedTemplates[0]}";

            if (report.MissingRequiredOutputTags.Count > 0)
                return $"Required output tag missing from template: [{report.MissingRequiredOutputTags[0]}]";

            if (report.ImageTagsWithoutAssignment.Count > 0)
                return $"Image tag not assigned to template: [{report.ImageTagsWithoutAssignment[0]}]";

            if (report.AssignedImagesMissingFiles.Count > 0)
                return $"Assigned image file not found for tag: [{report.AssignedImagesMissingFiles[0]}]";

            return "Template parse failed.";
        }

        private static bool IsEmbedDirectiveToken(string token)
        {
            return token.StartsWith("Embed:", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> ExtractEmbedDirectives(string html)
        {
            var embeds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(html))
                return embeds;

            // [Embed:Details=invoice_template_std_details]
            var matches = Regex.Matches(html, @"\[\s*Embed:(?<name>[A-Za-z0-9_]+)\s*=\s*(?<key>[A-Za-z0-9_\-\/]+)\s*\]", RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                var name = m.Groups["name"]?.Value?.Trim();
                var key = m.Groups["key"]?.Value?.Trim();
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(key))
                    embeds[name] = key;
            }

            return embeds;
        }

        private static HashSet<string> ExtractSquareBracketTokens(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var matches = Regex.Matches(html, @"\[(?<tag>[A-Za-z0-9_:]+)(?:=[^\]]+)?\]");
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match m in matches)
            {
                var tag = m.Groups["tag"]?.Value?.Trim();
                if (!string.IsNullOrWhiteSpace(tag))
                    set.Add(tag);
            }

            return set;
        }

        private static List<string> ExtractImgCidImageTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return new List<string>();

            // Only count img tags, because image validity should reflect "will render"
            // Matches: <img ... src="cid:[LOGO]" ...> (single or double quotes)
            var matches = Regex.Matches(
                html,
                @"<img\b[^>]*\bsrc\s*=\s*(?:""cid:\[(?<tag>[A-Za-z0-9_]+)\]""|'cid:\[(?<tag>[A-Za-z0-9_]+)\]')[^>]*>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var list = new List<string>();
            foreach (Match m in matches)
            {
                var tag = m.Groups["tag"]?.Value?.Trim();
                if (!string.IsNullOrWhiteSpace(tag))
                    list.Add(tag);
            }

            return list;
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
