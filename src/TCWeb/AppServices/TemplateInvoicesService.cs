using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.Data;
using TradeControl.Web.Mail;

namespace TradeControl.Web.AppServices
{
    public interface ITemplateInvoicesService
    {
        Task<TemplateInvoicesState> GetStateAsync(short? invoiceTypeCode = null, int? templateId = null, int? attachmentId = null, string? imageTag = null);
        Task<IReadOnlyList<string>> GetAvailableAttachmentFileNamesAsync(short invoiceTypeCode);
        Task<IReadOnlyList<string>> GetAvailableImageFileNamesAsync(int templateId);
        Task AssignTemplateAsync(short invoiceTypeCode, string templateFileName);
        Task RemoveTemplateAsync(short invoiceTypeCode, int templateId);
        Task AssignAttachmentAsync(short invoiceTypeCode, string attachmentFileName);
        Task RemoveAttachmentAsync(short invoiceTypeCode, int attachmentId);
        Task AssignImageAsync(int templateId, string imageFileName);
        Task RemoveImageAsync(int templateId, string imageTag);
        Task UpdateImageTagAsync(string imageTag, string newImageTag);
        Task<TemplateParseResult> ParseTemplateAsync(int templateId);
        Task<IReadOnlyList<TemplateParseResult>> ParseAllAsync(short invoiceTypeCode);
    }

    public sealed class TemplateInvoicesService : ITemplateInvoicesService
    {
        private readonly NodeContext _nodeContext;
        private readonly IFileProvider _fileProvider;
        private readonly SemaphoreSlim _dbGate = new(initialCount: 1, maxCount: 1);

        public TemplateInvoicesService(NodeContext nodeContext, IFileProvider fileProvider)
        {
            _nodeContext = nodeContext;
            _fileProvider = fileProvider;
        }

        public async Task<TemplateInvoicesState> GetStateAsync(short? invoiceTypeCode = null, int? templateId = null, int? attachmentId = null, string? imageTag = null)
        {
            await _dbGate.WaitAsync();
            try
            {
                var invoiceTypes = await _nodeContext.Invoice_tbTypes
                    .AsNoTracking()
                    .OrderBy(item => item.InvoiceTypeCode)
                    .Select(item => new TemplateInvoiceTypeOption {
                        InvoiceTypeCode = item.InvoiceTypeCode,
                        InvoiceType = item.InvoiceType
                    })
                    .ToListAsync();

                if (invoiceTypes.Count == 0)
                {
                    return new TemplateInvoicesState {
                        InvoiceTypeCode = 0,
                        InvoiceType = string.Empty,
                        InvoiceTypes = [],
                        AssignedTemplates = [],
                        AvailableTemplateFileNames = [],
                        Attachments = [],
                        Images = []
                    };
                }

                var resolvedInvoiceTypeCode = await ResolveInvoiceTypeCodeAsync(invoiceTypeCode, templateId, attachmentId) ?? invoiceTypes[0].InvoiceTypeCode;
                var resolvedInvoiceType = invoiceTypes.First(item => item.InvoiceTypeCode == resolvedInvoiceTypeCode).InvoiceType;

                var assignedTemplates = await _nodeContext.Web_tbTemplateInvoices
                    .AsNoTracking()
                    .Where(item => item.InvoiceTypeCode == resolvedInvoiceTypeCode)
                    .OrderByDescending(item => item.LastUsedOn)
                    .ThenBy(item => item.Template.TemplateFileName)
                    .Select(item => new TemplateInvoiceAssignment {
                        TemplateId = item.TemplateId,
                        TemplateFileName = item.Template.TemplateFileName,
                        LastUsedOn = item.LastUsedOn,
                        TemplateStatusCode = item.Template.TemplateStatusCode,
                        TemplateStatus = item.Template.TemplateStatusCodeNavigation.TemplateStatus,
                        ParsedOn = item.Template.ParsedOn,
                        ParseMessage = item.Template.ParseMessage
                    })
                    .ToListAsync();

                var availableTemplateFileNames = await _nodeContext.Web_tbTemplates
                    .AsNoTracking()
                    .OrderBy(item => item.TemplateFileName)
                    .Where(item => !_nodeContext.Web_tbTemplateInvoices
                        .Any(assigned => assigned.InvoiceTypeCode == resolvedInvoiceTypeCode && assigned.TemplateId == item.TemplateId))
                    .Select(item => item.TemplateFileName)
                    .ToListAsync();

                var attachments = await _nodeContext.Web_AttachmentInvoices
                    .AsNoTracking()
                    .Where(item => item.InvoiceTypeCode == resolvedInvoiceTypeCode)
                    .OrderBy(item => item.AttachmentFileName)
                    .Select(item => new TemplateInvoiceAttachment {
                        AttachmentId = item.AttachmentId,
                        AttachmentFileName = item.AttachmentFileName
                    })
                    .ToListAsync();

                var selectedTemplateId = templateId.HasValue && assignedTemplates.Any(item => item.TemplateId == templateId.Value)
                    ? templateId
                    : assignedTemplates.FirstOrDefault()?.TemplateId;

                var images = selectedTemplateId.HasValue
                    ? await _nodeContext.Web_TemplateImages
                        .AsNoTracking()
                        .Where(item => item.TemplateId == selectedTemplateId.Value)
                        .OrderBy(item => item.ImageTag)
                        .Select(item => new TemplateInvoiceImage {
                            TemplateId = item.TemplateId,
                            TemplateFileName = item.TemplateFileName,
                            ImageTag = item.ImageTag,
                            ImageFileName = item.ImageFileName
                        })
                        .ToListAsync()
                    : [];

                var selectedImageTag = !string.IsNullOrWhiteSpace(imageTag) && images.Any(item => string.Equals(item.ImageTag, imageTag, StringComparison.OrdinalIgnoreCase))
                    ? imageTag
                    : images.FirstOrDefault()?.ImageTag;

                var selectedAttachmentId = attachmentId.HasValue && attachments.Any(item => item.AttachmentId == attachmentId.Value)
                    ? attachmentId
                    : attachments.FirstOrDefault()?.AttachmentId;

                return new TemplateInvoicesState {
                    InvoiceTypeCode = resolvedInvoiceTypeCode,
                    InvoiceType = resolvedInvoiceType,
                    InvoiceTypes = invoiceTypes,
                    AssignedTemplates = assignedTemplates,
                    AvailableTemplateFileNames = availableTemplateFileNames,
                    Attachments = attachments,
                    Images = images,
                    SelectedTemplateId = selectedTemplateId,
                    SelectedAttachmentId = selectedAttachmentId,
                    SelectedImageTag = selectedImageTag
                };
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task<IReadOnlyList<string>> GetAvailableAttachmentFileNamesAsync(short invoiceTypeCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                return await _nodeContext.Web_tbAttachments
                    .AsNoTracking()
                    .OrderBy(item => item.AttachmentFileName)
                    .Select(item => item.AttachmentFileName)
                    .Except(_nodeContext.Web_AttachmentInvoices
                        .Where(item => item.InvoiceTypeCode == invoiceTypeCode)
                        .Select(item => item.AttachmentFileName))
                    .ToListAsync();
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task<IReadOnlyList<string>> GetAvailableImageFileNamesAsync(int templateId)
        {
            await _dbGate.WaitAsync();
            try
            {
                return await _nodeContext.Web_tbImages
                    .AsNoTracking()
                    .OrderBy(item => item.ImageFileName)
                    .Select(item => item.ImageFileName)
                    .Except(_nodeContext.Web_TemplateImages
                        .Where(item => item.TemplateId == templateId)
                        .Select(item => item.ImageFileName))
                    .ToListAsync();
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task AssignTemplateAsync(short invoiceTypeCode, string templateFileName)
        {
            await _dbGate.WaitAsync();
            try
            {
                var manager = new TemplateManager(_nodeContext);
                await manager.AssignTemplateToInvoice((NodeEnum.InvoiceType)invoiceTypeCode, templateFileName);
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task RemoveTemplateAsync(short invoiceTypeCode, int templateId)
        {
            await _dbGate.WaitAsync();
            try
            {
                var manager = new TemplateManager(_nodeContext);
                await manager.UnassignTemplateToInvoice((NodeEnum.InvoiceType)invoiceTypeCode, templateId);
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task AssignAttachmentAsync(short invoiceTypeCode, string attachmentFileName)
        {
            await _dbGate.WaitAsync();
            try
            {
                var manager = new TemplateManager(_nodeContext);
                await manager.AssignAttatchmentToInvoice((NodeEnum.InvoiceType)invoiceTypeCode, attachmentFileName);
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task RemoveAttachmentAsync(short invoiceTypeCode, int attachmentId)
        {
            await _dbGate.WaitAsync();
            try
            {
                var manager = new TemplateManager(_nodeContext);
                await manager.UnassignAttatchmentToInvoice((NodeEnum.InvoiceType)invoiceTypeCode, attachmentId);
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task AssignImageAsync(int templateId, string imageFileName)
        {
            await _dbGate.WaitAsync();
            try
            {
                var manager = new TemplateManager(_nodeContext);
                await manager.AssignImageToTemplate(templateId, imageFileName);
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task RemoveImageAsync(int templateId, string imageTag)
        {
            await _dbGate.WaitAsync();
            try
            {
                var manager = new TemplateManager(_nodeContext);
                await manager.UnassignImageToTemplate(templateId, imageTag);
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task UpdateImageTagAsync(string imageTag, string newImageTag)
        {
            await _dbGate.WaitAsync();
            try
            {
                var manager = new TemplateManager(_nodeContext);
                await manager.ImageTag(imageTag, newImageTag);
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task<TemplateParseResult> ParseTemplateAsync(int templateId)
        {
            await _dbGate.WaitAsync();
            try
            {
                return await ParseTemplateCoreAsync(templateId);
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task<IReadOnlyList<TemplateParseResult>> ParseAllAsync(short invoiceTypeCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                var templateIds = await _nodeContext.Web_tbTemplateInvoices
                    .AsNoTracking()
                    .Where(item => item.InvoiceTypeCode == invoiceTypeCode)
                    .OrderByDescending(item => item.LastUsedOn)
                    .ThenBy(item => item.Template.TemplateFileName)
                    .Select(item => item.TemplateId)
                    .ToListAsync();

                var results = new List<TemplateParseResult>(templateIds.Count);

                foreach (var templateId in templateIds)
                    results.Add(await ParseTemplateCoreAsync(templateId));

                return results;
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<TemplateParseResult> ParseTemplateCoreAsync(int templateId)
        {
            var manager = new TemplateManager(_nodeContext, _fileProvider);
            var report = await manager.ParseTemplateAsync(templateId, MailInvoice.ParseProfile);

            var template = await _nodeContext.Web_tbTemplates
                .AsNoTracking()
                .Include(item => item.TemplateStatusCodeNavigation)
                .SingleAsync(item => item.TemplateId == templateId);

            return new TemplateParseResult {
                TemplateId = template.TemplateId,
                TemplateFileName = template.TemplateFileName,
                TemplateStatusCode = template.TemplateStatusCode,
                TemplateStatus = template.TemplateStatusCodeNavigation.TemplateStatus,
                ParsedOn = template.ParsedOn,
                ParseMessage = template.ParseMessage,
                InvalidFieldTags = report.InvalidFieldTags.ToList(),
                MissingEmbedDirectives = report.MissingEmbedDirectives.ToList(),
                MissingEmbedTemplates = report.MissingEmbedTemplates.ToList(),
                InvalidEmbedTemplates = report.InvalidEmbedTemplates.ToList(),
                MissingRequiredOutputTags = report.MissingRequiredOutputTags.ToList(),
                ImageTagsWithoutAssignment = report.ImageTagsWithoutAssignment.ToList(),
                AssignedImageTagsWithoutUsage = report.AssignedImageTagsWithoutUsage.ToList(),
                AssignedImagesMissingFiles = report.AssignedImagesMissingFiles.ToList(),
                UnusedAvailableFields = report.UnusedAvailableFields.ToList()
            };
        }

        private async Task<short?> ResolveInvoiceTypeCodeAsync(short? invoiceTypeCode, int? templateId, int? attachmentId)
        {
            if (invoiceTypeCode.HasValue)
                return invoiceTypeCode.Value;

            if (templateId.HasValue)
            {
                return await _nodeContext.Web_tbTemplateInvoices
                    .AsNoTracking()
                    .Where(item => item.TemplateId == templateId.Value)
                    .OrderBy(item => item.InvoiceTypeCode)
                    .Select(item => (short?)item.InvoiceTypeCode)
                    .FirstOrDefaultAsync();
            }

            if (attachmentId.HasValue)
            {
                return await _nodeContext.Web_tbAttachmentInvoices
                    .AsNoTracking()
                    .Where(item => item.AttachmentId == attachmentId.Value)
                    .OrderBy(item => item.InvoiceTypeCode)
                    .Select(item => (short?)item.InvoiceTypeCode)
                    .FirstOrDefaultAsync();
            }

            return null;
        }
    }

    public sealed class TemplateInvoicesState
    {
        public short InvoiceTypeCode { get; init; }
        public string InvoiceType { get; init; } = string.Empty;
        public IReadOnlyList<TemplateInvoiceTypeOption> InvoiceTypes { get; init; } = [];
        public IReadOnlyList<TemplateInvoiceAssignment> AssignedTemplates { get; init; } = [];
        public IReadOnlyList<string> AvailableTemplateFileNames { get; init; } = [];
        public IReadOnlyList<TemplateInvoiceAttachment> Attachments { get; init; } = [];
        public IReadOnlyList<TemplateInvoiceImage> Images { get; init; } = [];
        public int? SelectedTemplateId { get; init; }
        public int? SelectedAttachmentId { get; init; }
        public string? SelectedImageTag { get; init; }
    }

    public sealed class TemplateInvoiceTypeOption
    {
        public short InvoiceTypeCode { get; init; }
        public string InvoiceType { get; init; } = string.Empty;
    }

    public sealed class TemplateInvoiceAssignment
    {
        public int TemplateId { get; init; }
        public string TemplateFileName { get; init; } = string.Empty;
        public DateTime? LastUsedOn { get; init; }
        public short TemplateStatusCode { get; init; }
        public string TemplateStatus { get; init; } = string.Empty;
        public DateTime? ParsedOn { get; init; }
        public string? ParseMessage { get; init; }
    }

    public sealed class TemplateInvoiceAttachment
    {
        public int AttachmentId { get; init; }
        public string AttachmentFileName { get; init; } = string.Empty;
    }

    public sealed class TemplateInvoiceImage
    {
        public int TemplateId { get; init; }
        public string TemplateFileName { get; init; } = string.Empty;
        public string ImageTag { get; init; } = string.Empty;
        public string ImageFileName { get; init; } = string.Empty;
    }

    public sealed class TemplateParseResult
    {
        public int TemplateId { get; init; }
        public string TemplateFileName { get; init; } = string.Empty;
        public short TemplateStatusCode { get; init; }
        public string TemplateStatus { get; init; } = string.Empty;
        public DateTime? ParsedOn { get; init; }
        public string? ParseMessage { get; init; }
        public IReadOnlyList<string> InvalidFieldTags { get; init; } = [];
        public IReadOnlyList<string> MissingEmbedDirectives { get; init; } = [];
        public IReadOnlyList<string> MissingEmbedTemplates { get; init; } = [];
        public IReadOnlyList<string> InvalidEmbedTemplates { get; init; } = [];
        public IReadOnlyList<string> MissingRequiredOutputTags { get; init; } = [];
        public IReadOnlyList<string> ImageTagsWithoutAssignment { get; init; } = [];
        public IReadOnlyList<string> AssignedImageTagsWithoutUsage { get; init; } = [];
        public IReadOnlyList<string> AssignedImagesMissingFiles { get; init; } = [];
        public IReadOnlyList<string> UnusedAvailableFields { get; init; } = [];

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
}
