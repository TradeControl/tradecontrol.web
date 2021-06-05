using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbType", Schema = "Invoice")]
    public partial class Invoice_tbType
    {
        public Invoice_tbType()
        {
            TbEntries = new HashSet<Invoice_tbEntry>();
            TbInvoices = new HashSet<Invoice_tbInvoice>();
            TbMirrors = new HashSet<Invoice_tbMirror>();
        }

        [Key]
        public short InvoiceTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
        public short CashModeCode { get; set; }
        public int NextNumber { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(CashModeCode))]
        [InverseProperty(nameof(Cash_tbMode.TbInvoiceType))]
        public virtual Cash_tbMode CashModeCodeNavigation { get; set; }
        [InverseProperty(nameof(Invoice_tbEntry.InvoiceTypeCodeNavigation))]
        public virtual ICollection<Invoice_tbEntry> TbEntries { get; set; }
        [InverseProperty(nameof(Invoice_tbInvoice.InvoiceTypeCodeNavigation))]
        public virtual ICollection<Invoice_tbInvoice> TbInvoices { get; set; }
        [InverseProperty(nameof(Invoice_tbMirror.InvoiceTypeCodeNavigation))]
        public virtual ICollection<Invoice_tbMirror> TbMirrors { get; set; }
    }
}
