using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Web_vwAttachmentInvoice
    {
        public short InvoiceTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Invoice Type")]
        public string InvoiceType { get; set; }
        public int AttachmentId { get; set; }
        [Required]
        [StringLength(256)]
        [Display(Name = "Attachment")]
        public string AttachmentFileName { get; set; }
    }
}
