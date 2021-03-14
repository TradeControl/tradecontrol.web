using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwGraphBankBalance
    {
        [StringLength(4000)]
        public string PeriodOn { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal SumOfClosingBalance { get; set; }
    }
}
