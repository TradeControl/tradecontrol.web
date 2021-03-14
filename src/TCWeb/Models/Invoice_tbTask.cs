using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTask", Schema = "Invoice")]
    [Index(nameof(InvoiceNumber), nameof(CashCode), nameof(InvoiceValue), nameof(TaxValue), nameof(TaxCode), Name = "IX_Invoice_tbTask_Full")]
    [Index(nameof(InvoiceNumber), nameof(TaxCode), Name = "IX_Invoice_tbTask_InvoiceNumber_TaxCode")]
    [Index(nameof(TaskCode), nameof(InvoiceNumber), Name = "IX_Invoice_tbTask_TaskCode")]
    [Index(nameof(TaxCode), Name = "IX_Invoice_tbTask_TaxCode")]
    public partial class Invoice_tbTask
    {
        [Key]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Key]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }

        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbInvoiceTasks))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(InvoiceNumber))]
        [InverseProperty(nameof(Invoice_tbInvoice.TbTasks))]
        public virtual Invoice_tbInvoice InvoiceNumberNavigation { get; set; }
        [ForeignKey(nameof(TaskCode))]
        [InverseProperty(nameof(Task_tbTask.TbTasks))]
        public virtual Task_tbTask TaskCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbInvoiceTasks))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
    }
}
