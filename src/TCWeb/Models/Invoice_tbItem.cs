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
        [Display(Name = "Invoice No.")]
        public string InvoiceNumber { get; set; }
        [Key]
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Ref.")]
        public string ItemReference { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Total Value")]
        [DataType(DataType.Currency)]
        public decimal TotalValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Invoice Value")]
        [DataType(DataType.Currency)]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax Value")]
        [DataType(DataType.Currency)]

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
