using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxVatAuditInvoice
    {
        [Required]
        [StringLength(21)]
        public string YearPeriod { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }

        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }

        public short InvoiceTypeCode { get; set; }

        [StringLength(10)]
        public string TaxCode { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal TaxRate { get; set; }

        public byte ExportTypeCode { get; set; }

        [Required]
        [StringLength(50)]
        public string IdentityCode { get; set; }

        [StringLength(100)]
        public string ItemDescription { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal vatDueSales { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal vatDueAcquisitions { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal totalVatDue { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal vatReclaimedCurrPeriod { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal netVatDue { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal totalValueSalesExVAT { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal totalValuePurchasesExVAT { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal totalValueGoodsSuppliedExVAT { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal totalValueGoodsReceivedExVAT { get; set; }
    }
}
