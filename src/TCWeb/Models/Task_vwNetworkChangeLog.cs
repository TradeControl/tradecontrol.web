using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Task_vwNetworkChangeLog
    {
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        public int LogId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ChangedOn { get; set; }
        public short TransmitStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        public string TransmitStatus { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [StringLength(50)]
        public string AllocationCode { get; set; }
        public short TaskStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string TaskStatus { get; set; }
        public short CashModeCode { get; set; }
        [StringLength(10)]
        public string CashMode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
