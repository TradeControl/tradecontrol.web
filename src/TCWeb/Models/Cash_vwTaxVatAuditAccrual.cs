using System;
using System.Collections.Generic;
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
        public string TaskTitle { get; set; }
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        [Required]
        [StringLength(100)]
        public string TaskStatus { get; set; }
        public short TaskStatusCode { get; set; }
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
        public double HomePurchases { get; set; }
        public double ExportSales { get; set; }
        public double ExportPurchases { get; set; }
        public double HomeSalesVat { get; set; }
        public double HomePurchasesVat { get; set; }
        public double ExportSalesVat { get; set; }
        public double ExportPurchasesVat { get; set; }
        public double VatDue { get; set; }
        public double HomeSales { get; set; }
    }
}
