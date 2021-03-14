using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbItem", Schema = "Invoice")]
    [Index(nameof(CashCode), nameof(InvoiceNumber), Name = "IX_Invoice_tbItem_CashCode")]
    [Index(nameof(InvoiceNumber), nameof(CashCode), nameof(InvoiceValue), nameof(TaxValue), nameof(TaxCode), Name = "IX_Invoice_tbItem_Full")]
    [Index(nameof(InvoiceNumber), nameof(TaxCode), Name = "IX_Invoice_tbItem_InvoiceNumber_TaxCode")]
    [Index(nameof(TaxCode), Name = "IX_Invoice_tbItem_TaxCode")]
    public partial class Invoice_tbItem
    {
        [Key]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Key]
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "ntext")]
        public string ItemReference { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }

        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbItems))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(InvoiceNumber))]
        [InverseProperty(nameof(Invoice_tbInvoice.TbItems))]
        public virtual Invoice_tbInvoice InvoiceNumberNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbItems))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
    }
}
