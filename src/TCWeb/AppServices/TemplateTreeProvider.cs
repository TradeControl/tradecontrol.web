using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices
{
    public class TemplateTreeProvider : ITemplateTreeProvider
    {
        private NodeContext NodeContext { get; }

        public TemplateTreeProvider(NodeContext nodeContext)
        {
            NodeContext = nodeContext;
        }

        public async Task<IReadOnlyList<TemplateTreeNode>> GetInvoiceTypeNodesAsync(CancellationToken cancellationToken = default)
        {
            var types = await NodeContext.Invoice_tbTypes
                .OrderBy(t => t.InvoiceTypeCode)
                .Select(t => new TemplateTreeNode(
                    $"invoiceType:{t.InvoiceTypeCode}",
                    t.InvoiceType,
                    TemplateTreeNodeKind.InvoiceType))
                .ToListAsync(cancellationToken);

            return types;
        }

        public async Task<IReadOnlyList<TemplateTreeNode>> GetInvoiceTemplateNodesAsync(short invoiceTypeCode, CancellationToken cancellationToken = default)
        {
            var templates = await NodeContext.Web_tbTemplateInvoices
                .Where(ti => ti.InvoiceTypeCode == invoiceTypeCode)
                .OrderByDescending(ti => ti.LastUsedOn)
                .Select(ti => new TemplateTreeNode(
                    $"invoiceType:{invoiceTypeCode}:template:{ti.TemplateId}",
                    ti.Template.TemplateFileName,
                    TemplateTreeNodeKind.Template))
                .ToListAsync(cancellationToken);

            return templates;
        }

        public async Task<IReadOnlyList<TemplateTreeNode>> GetTemplateImageNodesAsync(int templateId, CancellationToken cancellationToken = default)
        {
            var images = await NodeContext.Web_tbTemplateImages
                .Where(ti => ti.TemplateId == templateId)
                .OrderBy(ti => ti.ImageTag)
                .Select(ti => new TemplateTreeNode(
                    $"template:{templateId}:image:{ti.ImageTag}",
                    ti.ImageTag,
                    TemplateTreeNodeKind.Image))
                .ToListAsync(cancellationToken);

            return images;
        }

        public async Task<IReadOnlyList<TemplateTreeNode>> GetInvoiceAttachmentNodesAsync(short invoiceTypeCode, CancellationToken cancellationToken = default)
        {
            var attachments = await NodeContext.Web_tbAttachmentInvoices
                .Where(ai => ai.InvoiceTypeCode == invoiceTypeCode)
                .OrderBy(ai => ai.Attachment.AttachmentFileName)
                .Select(ai => new TemplateTreeNode(
                    $"invoiceType:{invoiceTypeCode}:attachment:{ai.AttachmentId}",
                    ai.Attachment.AttachmentFileName,
                    TemplateTreeNodeKind.Attachment))
                .ToListAsync(cancellationToken);

            return attachments;
        }
    }
}
