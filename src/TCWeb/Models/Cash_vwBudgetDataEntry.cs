using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwBudgetDataEntry
    {
        public short YearNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(10)]
        public string MonthName { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal ForecastValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal ForecastTax { get; set; }
        [Column(TypeName = "ntext")]
        public string Note { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
