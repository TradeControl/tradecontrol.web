using System;
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
