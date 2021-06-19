using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAttachment", Schema = "Web")]
    [Index(nameof(AttachmentFileName), Name = "IX_Web_tbAttachment_AttachmentFileName", IsUnique = true)]
    public partial class Web_tbAttachment
    {
        public Web_tbAttachment()
        {
            TbAttachmentInvoices = new HashSet<Web_tbAttachmentInvoice>();
        }

        [Key]
        public int AttachmentId { get; set; }
        [Required]
        [StringLength(256)]
        public string AttachmentFileName { get; set; }

        [InverseProperty(nameof(Web_tbAttachmentInvoice.Attachment))]
        public virtual ICollection<Web_tbAttachmentInvoice> TbAttachmentInvoices { get; set; }
    }
}
