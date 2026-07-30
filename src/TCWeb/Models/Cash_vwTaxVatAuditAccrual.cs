using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxVatAuditAccrual
    {
        public short YearNumber { get; set; }

        [Required]
        [StringLength(21)]
        public string YearPeriod { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }

        [StringLength(100)]
        public string ProjectTitle { get; set; }

        [Required]
        [StringLength(20)]
        public string ProjectCode { get; set; }

        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }

        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }

        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }

        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }

        public short ProjectStatusCode { get; set; }

        [StringLength(10)]
        public string TaxCode { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal TaxRate { get; set; }

        [Column(TypeName = "decimal(38, 6)")]
        public decimal TotalValue { get; set; }

        [Column(TypeName = "decimal(38, 6)")]
        public decimal TaxValue { get; set; }

        [Column(TypeName = "decimal(38, 4)")]
        public decimal QuantityRemaining { get; set; }

        [Required]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }

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
