using System;
using System.Collections.Generic;
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
        [Column("EUJurisdiction")]
        public bool Eujurisdiction { get; set; }
        [Required]
        [StringLength(50)]
        public string IdentityCode { get; set; }
        [StringLength(100)]
        public string ItemDescription { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal HomeSales { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal HomePurchases { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal ExportSales { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal ExportPurchases { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal HomeSalesVat { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal HomePurchasesVat { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal ExportSalesVat { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal ExportPurchasesVat { get; set; }
        [Column(TypeName = "decimal(22, 5)")]
        public decimal VatDue { get; set; }
    }
}
