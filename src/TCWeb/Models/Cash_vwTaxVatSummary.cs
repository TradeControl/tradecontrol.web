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
        public DateTime StartOn { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        public double HomeSales { get; set; }
        public double HomePurchases { get; set; }
        public double ExportSales { get; set; }
        public double ExportPurchases { get; set; }
        public double HomeSalesVat { get; set; }
        public double HomePurchasesVat { get; set; }
        public double ExportSalesVat { get; set; }
        public double ExportPurchasesVat { get; set; }
        public double VatDue { get; set; }
    }
}
