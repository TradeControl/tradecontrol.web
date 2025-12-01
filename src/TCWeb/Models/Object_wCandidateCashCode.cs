using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Object_wCandidateCashCode
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
        public short CashPolarityCode { get; set; }
        public short CashTypeCode { get; set; }
    }
}
