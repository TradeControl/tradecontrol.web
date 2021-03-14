using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxVatDetail
    {
        public short YearNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        [Required]
        [StringLength(23)]
        public string PeriodName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal HomeSales { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal HomePurchases { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal ExportSales { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal ExportPurchases { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal HomeSalesVat { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal HomePurchasesVat { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal ExportSalesVat { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal ExportPurchasesVat { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal VatDue { get; set; }
    }
}
