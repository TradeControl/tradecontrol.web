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
        [Display(Name = "Id")]
        public long EntryNumber { get; set; }
        [StringLength(10)]
        [Display(Name = "Asset Code")]
        public string AssetCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Asset Class")]
        public string AssetName { get; set; }
        [Display(Name = "Cash Mode")]
        public int CashModeCode { get; set; }
        [Display(Name = "Liquidity")]
        public short LiquidityLevel { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Period")]
        public DateTime StartOn { get; set; }
        [Display(Name = "Year")]
        public short YearNumber { get; set; }
        [Display(Name = "Month")]
        public short MonthNumber { get; set; }
        [Display(Name = "Is Entry")]
        public bool IsEntry { get; set; }
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        public double Balance { get; set; }
    }
}
