using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Task_vwNetworkQuotation
    {
        public int AllocationId { get; set; }
        [Required]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        [StringLength(100)]
        public string ActivityDescription { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        public bool IsAllocation { get; set; }
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        public int CashModeCode { get; set; }
        [StringLength(10)]
        public string CashMode { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "decimal(21, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }
    }
}
