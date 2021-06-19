using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAttachmentInvoice", Schema = "Web")]
    [Index(nameof(AttachmentId), nameof(InvoiceTypeCode), Name = "IX_tbAttachmentInvoice", IsUnique = true)]
    public partial class Web_tbAttachmentInvoice
    {
        [Key]
        public short InvoiceTypeCode { get; set; }
        [Key]
        public int AttachmentId { get; set; }

        [ForeignKey(nameof(AttachmentId))]
        [InverseProperty(nameof(Web_tbAttachment.TbAttachmentInvoices))]
        public virtual Web_tbAttachment Attachment { get; set; }
        [ForeignKey(nameof(InvoiceTypeCode))]
        [InverseProperty(nameof(Invoice_tbType.TbAttachmentInvoices))]
        public virtual Invoice_tbType InvoiceTypeCodeNavigation { get; set; }
    }
}
