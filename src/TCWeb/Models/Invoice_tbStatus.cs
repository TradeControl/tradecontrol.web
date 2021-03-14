using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbStatus", Schema = "Invoice")]
    public partial class Invoice_tbStatus
    {
        public Invoice_tbStatus()
        {
            TbInvoices = new HashSet<Invoice_tbInvoice>();
            TbMirror = new HashSet<Invoice_tbMirror>();
        }

        [Key]
        public short InvoiceStatusCode { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }

        [InverseProperty(nameof(Invoice_tbInvoice.InvoiceStatusCodeNavigation))]
        public virtual ICollection<Invoice_tbInvoice> TbInvoices { get; set; }
        [InverseProperty(nameof(Invoice_tbMirror.InvoiceStatusCodeNavigation))]
        public virtual ICollection<Invoice_tbMirror> TbMirror { get; set; }
    }
}
