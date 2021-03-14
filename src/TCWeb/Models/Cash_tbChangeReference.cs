using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbChangeReference", Schema = "Cash")]
    [Index(nameof(InvoiceNumber), Name = "IX_Cash_tbChangeReference_InvoiceNumber", IsUnique = true)]
    public partial class Cash_tbChangeReference
    {
        [Key]
        [StringLength(42)]
        public string PaymentAddress { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }

        [ForeignKey(nameof(InvoiceNumber))]
        [InverseProperty(nameof(Invoice_tbInvoice.TbChangeReference))]
        public virtual Invoice_tbInvoice InvoiceNumberNavigation { get; set; }
        [ForeignKey(nameof(PaymentAddress))]
        [InverseProperty(nameof(Cash_tbChange.TbChangeReference))]
        public virtual Cash_tbChange PaymentAddressNavigation { get; set; }
    }
}
