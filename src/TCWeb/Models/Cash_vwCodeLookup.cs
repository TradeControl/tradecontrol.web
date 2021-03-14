using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwCodeLookup
    {
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        public short CashModeCode { get; set; }
        [StringLength(10)]
        public string CashMode { get; set; }
        [Required]
        [StringLength(10)]
        public string TaxCode { get; set; }
        public short CashTypeCode { get; set; }
        [StringLength(25)]
        public string CashType { get; set; }
    }
}
