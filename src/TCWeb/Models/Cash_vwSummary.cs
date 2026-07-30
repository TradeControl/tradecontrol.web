using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwSummary
    {
        [Column(TypeName = "datetime")]
        public DateTime Timestamp { get; set; }

        [Column(TypeName = "decimal(38, 5)")]
        public decimal Collect { get; set; }

        [Column(TypeName = "decimal(38, 5)")]
        public decimal Pay { get; set; }

        [Column(TypeName = "decimal(38, 5)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(38, 5)")]
        public decimal Cash { get; set; }

        [Column(TypeName = "decimal(38, 5)")]
        public decimal Balance { get; set; }
    }
}
