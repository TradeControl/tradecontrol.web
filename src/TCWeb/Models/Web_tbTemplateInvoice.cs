using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTemplateInvoice", Schema = "Web")]
    [Index(nameof(TemplateId), nameof(InvoiceTypeCode), Name = "IX_tbTemplateInvoice", IsUnique = true)]
    [Index(nameof(InvoiceTypeCode), nameof(LastUsedOn), Name = "IX_tbTemplateInvoice_LastUsedOn")]
    public partial class Web_tbTemplateInvoice   
    {
        [Key]
        public short InvoiceTypeCode { get; set; }
        [Key]
        public int TemplateId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? LastUsedOn { get; set; }

        [ForeignKey(nameof(InvoiceTypeCode))]
        [InverseProperty(nameof(Invoice_tbType.TbTemplateInvoices))]
        public virtual Invoice_tbType InvoiceTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(TemplateId))]
        [InverseProperty(nameof(Web_tbTemplate.tbTemplateInvoices))]
        public virtual Web_tbTemplate Template { get; set; }
    }
}
