using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMirrorItem", Schema = "Invoice")]
    [Index(nameof(ChargeCode), nameof(ContractAddress), Name = "IX_Invoice_tbMirrorItem_InvoiceNumber")]
    public partial class Invoice_tbMirrorItem
    {
        [Key]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Key]
        [StringLength(50)]
        public string ChargeCode { get; set; }
        [StringLength(100)]
        public string ChargeDescription { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }

        [ForeignKey(nameof(ContractAddress))]
        [InverseProperty(nameof(Invoice_tbMirror.TbMirrorItems))]
        public virtual Invoice_tbMirror ContractAddressNavigation { get; set; }
    }
}
