using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwFlowCategoryByYear
    {
        [Required]
        [StringLength(10)]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        public short YearNumber { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal? InvoiceValue { get; set; }
    }
}
