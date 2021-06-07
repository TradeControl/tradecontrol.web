using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxCorpTotal
    {
        [Display(Name ="F/Year No.")]
        public short YearNumber { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Start On")]
        public DateTime StartOn { get; set; }
        [Display(Name = "Period Year")]
        public int PeriodYear { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "F/Year")]
        public string Description { get; set; }
        [StringLength(21)]
        [Display(Name = "Period")]
        public string Period { get; set; }
        [Display(Name = "Tax Rate")]
        [DisplayFormat(DataFormatString = "{0:p}")]
        public float CorporationTaxRate { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Adj.")]
        [DataType(DataType.Currency)]
        public decimal TaxAdjustment { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Net Profit")]
        [DataType(DataType.Currency)]
        public decimal NetProfit { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Company Tax")]
        [DataType(DataType.Currency)]

        public decimal CorporationTax { get; set; }
    }
}
