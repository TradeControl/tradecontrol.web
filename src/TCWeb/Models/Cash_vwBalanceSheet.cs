using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwBalanceSheet
    {
        public long EntryNumber { get; set; }
        [StringLength(10)]
        public string AssetCode { get; set; }
        [StringLength(50)]
        public string AssetName { get; set; }
        public int CashModeCode { get; set; }
        public short LiquidityLevel { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        public bool IsEntry { get; set; }
        public double Balance { get; set; }
    }
}
