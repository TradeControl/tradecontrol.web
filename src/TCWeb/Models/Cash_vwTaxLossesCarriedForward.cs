using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxLossesCarriedForward
    {
        [Display(Name = "Period")]
        public string YearEndDescription { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Start On")]
        [DataType(DataType.Date)]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax Due")]
        [DataType(DataType.Currency)]
        public decimal TaxDue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax Balance")]
        [DataType(DataType.Currency)]
        public decimal TaxBalance { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Carried Forward")]
        [DataType(DataType.Currency)]
        public decimal LossesCarriedForward { get; set; }
    }
}
