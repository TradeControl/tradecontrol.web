using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Task_vwNetworkAllocation
    {
        [Required]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        [Required]
        [StringLength(50)]
        public string AllocationCode { get; set; }
        [StringLength(256)]
        public string AllocationDescription { get; set; }
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [StringLength(100)]
        public string TaskTitle { get; set; }
        public short CashModeCode { get; set; }
        [StringLength(10)]
        public string CashMode { get; set; }
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [StringLength(5)]
        public string UnitOfCharge { get; set; }
        public short TaskStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string TaskStatus { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal TaxRate { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal QuantityOrdered { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal QuantityDelivered { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
