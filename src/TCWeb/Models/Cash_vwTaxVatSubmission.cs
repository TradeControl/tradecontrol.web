using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxVatSubmission
    {
        [Display(Name = "Year")]
        public short YearNumber { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "F/Year")]
        public string Description { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Period")]
        public string Period { get; set; }

        [Column(TypeName = "datetime")]
        [DataType(DataType.Date)]
        [Display(Name = "Start On")]
        public DateTime StartOn { get; set; }

        [Display(Name = "VAT Due on Sales")]
        [DataType(DataType.Currency)]
        public decimal vatDueSales { get; set; }

        [Display(Name = "VAT Due on Acquisitions")]
        [DataType(DataType.Currency)]
        public decimal vatDueAcquisitions { get; set; }

        [Display(Name = "Total VAT Due")]
        [DataType(DataType.Currency)]
        public decimal totalVatDue { get; set; }

        [Display(Name = "VAT Reclaimed Current Period")]
        [DataType(DataType.Currency)]
        public decimal vatReclaimedCurrPeriod { get; set; }

        [Display(Name = "Net VAT Due")]
        [DataType(DataType.Currency)]
        public decimal netVatDue { get; set; }

        [Display(Name = "Sales ex VAT")]
        [DataType(DataType.Currency)]
        public decimal totalValueSalesExVAT { get; set; }

        [Display(Name = "Purchases ex VAT")]
        [DataType(DataType.Currency)]
        public decimal totalValuePurchasesExVAT { get; set; }

        [Display(Name = "Goods Supplied ex VAT")]
        [DataType(DataType.Currency)]
        public decimal totalValueGoodsSuppliedExVAT { get; set; }

        [Display(Name = "Goods Received ex VAT")]
        [DataType(DataType.Currency)]
        public decimal totalValueGoodsReceivedExVAT { get; set; }

    }
}
