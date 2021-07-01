using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwFlowCategoryByPeriod
    {
        [Required]
        [StringLength(10)]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        public short YearNumber { get; set; }
        public short MonthNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal? InvoiceValue { get; set; }
    }
}
