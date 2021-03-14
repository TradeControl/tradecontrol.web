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
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        public int PeriodYear { get; set; }
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        [StringLength(21)]
        public string Period { get; set; }
        public float CorporationTaxRate { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxAdjustment { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal NetProfit { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal CorporationTax { get; set; }
    }
}
