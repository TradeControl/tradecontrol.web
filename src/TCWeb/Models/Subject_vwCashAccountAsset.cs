using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwCashAccountAsset
    {
        [Required]
        [StringLength(10)]
        public string CashAccountCode { get; set; }
        public short LiquidityLevel { get; set; }
        [Required]
        [StringLength(50)]
        public string CashAccountName { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Display(Name = "Closed?")]
        public bool AccountClosed { get; set; }
    }
}
