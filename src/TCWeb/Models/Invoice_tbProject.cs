using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbProject", Schema = "Invoice")]
    [Index(nameof(InvoiceNumber), nameof(CashCode), nameof(InvoiceValue), nameof(TaxValue), nameof(TaxCode), Name = "IX_Invoice_tbProject_Full")]
    [Index(nameof(InvoiceNumber), nameof(TaxCode), Name = "IX_Invoice_tbProject_InvoiceNumber_TaxCode")]
    [Index(nameof(ProjectCode), nameof(InvoiceNumber), Name = "IX_Invoice_tbProject_ProjectCode")]
    [Index(nameof(TaxCode), Name = "IX_Invoice_tbProject_TaxCode")]
    public partial class Invoice_tbProject
    {
        [Key]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Key]
        [StringLength(20)]
        public string ProjectCode { get; set; }
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
        [InverseProperty(nameof(Cash_tbCode.TbInvoiceProjects))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(InvoiceNumber))]
        [InverseProperty(nameof(Invoice_tbInvoice.TbProjects))]
        public virtual Invoice_tbInvoice InvoiceNumberNavigation { get; set; }
        [ForeignKey(nameof(ProjectCode))]
        [InverseProperty(nameof(Project_tbProject.TbProjects))]
        public virtual Project_tbProject ProjectCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbInvoiceProjects))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
    }
}
