using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Object_vwUnMirrored
    {
        public int CandidateId { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        public string AllocationCode { get; set; }
        [StringLength(256)]
        public string AllocationDescription { get; set; }
        public short CashModeCode { get; set; }
        [StringLength(10)]
        public string CashMode { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        public int IsObject { get; set; }
    }
}
