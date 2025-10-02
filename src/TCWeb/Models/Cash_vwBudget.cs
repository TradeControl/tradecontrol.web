using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwBudget
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
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        public short YearNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string MonthName { get; set; }
        [StringLength(4000)]
        public string Period { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal ForecastValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal ForecastTax { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceTax { get; set; }
        [Column(TypeName = "ntext")]
        public string Note { get; set; }
        [StringLength(10)]
        public string CashPolarity { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TaxRate { get; set; }
    }
}
