using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMirrorReference", Schema = "Invoice")]
    [Index(nameof(InvoiceNumber), Name = "IX_Invoice_tbMirrorReference_InvoiceNumber", IsUnique = true)]
    public partial class Invoice_tbMirrorReference
    {
        [Key]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }

        [ForeignKey(nameof(ContractAddress))]
        [InverseProperty(nameof(Invoice_tbMirror.TbMirrorReference))]
        public virtual Invoice_tbMirror ContractAddressNavigation { get; set; }
        [ForeignKey(nameof(InvoiceNumber))]
        [InverseProperty(nameof(Invoice_tbInvoice.TbMirrorReference))]
        public virtual Invoice_tbInvoice InvoiceNumberNavigation { get; set; }
    }
}
