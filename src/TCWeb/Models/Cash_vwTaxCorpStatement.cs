using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxCorpStatement
    {
        [Column(TypeName = "datetime")]
        [Display(Name = "Start On")]
        [DataType(DataType.Date)]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax Due")]
        [DataType(DataType.Currency)]
        public decimal TaxDue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax Paid")]
        [DataType(DataType.Currency)]
        public decimal TaxPaid { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }
    }
}
