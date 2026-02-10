using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TradeControl.Web.AppServices
{
    public interface ITemplateTreeProvider
    {
        Task<IReadOnlyList<TemplateTreeNode>> GetInvoiceTypeNodesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TemplateTreeNode>> GetInvoiceTemplateNodesAsync(short invoiceTypeCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TemplateTreeNode>> GetTemplateImageNodesAsync(int templateId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TemplateTreeNode>> GetInvoiceAttachmentNodesAsync(short invoiceTypeCode, CancellationToken cancellationToken = default);
    }

    public enum TemplateTreeNodeKind
    {
        InvoiceType,
        TemplatesFolder,
        Template,
        ImagesFolder,
        Image,
        AttachmentsFolder,
        Attachment
    }

    public record TemplateTreeNode(string Key, string Text, TemplateTreeNodeKind Kind);
}
