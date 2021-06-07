﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxVatSummary
    {
        [Column(TypeName = "datetime")]
        [DataType(DataType.Date)]
        [Display(Name = "Start On")]
        public DateTime StartOn { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Display(Name = "Home Sales")]
        [DataType(DataType.Currency)]
        public double HomeSales { get; set; }
        [Display(Name = "Home Purchases")]
        [DataType(DataType.Currency)]
        public double HomePurchases { get; set; }
        [Display(Name = "Export Sales")]
        [DataType(DataType.Currency)]
        public double ExportSales { get; set; }
        [Display(Name = "Export Purchases")]
        [DataType(DataType.Currency)]
        public double ExportPurchases { get; set; }
        [Display(Name = "Home Sales Vat")]
        [DataType(DataType.Currency)]
        public double HomeSalesVat { get; set; }
        [Display(Name = "Home Purchases Vat")]
        [DataType(DataType.Currency)]
        public double HomePurchasesVat { get; set; }
        [Display(Name = "Export Sales Vat")]
        [DataType(DataType.Currency)]
        public double ExportSalesVat { get; set; }
        [Display(Name = "Export Purchases Vat")]
        [DataType(DataType.Currency)]
        public double ExportPurchasesVat { get; set; }
        [Display(Name = "Vat Due")]
        [DataType(DataType.Currency)]
        public double VatDue { get; set; }
    }
}
