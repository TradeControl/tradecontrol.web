using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Project_vwNetworkEventLog
    {
        [Required]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        public int LogId { get; set; }
        public short EventTypeCode { get; set; }
        public short ProjectStatusCode { get; set; }
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
        [Required]
        [StringLength(15)]
        public string EventType { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [Required]
        [StringLength(50)]
        public string AllocationCode { get; set; }
        [StringLength(256)]
        public string AllocationDescription { get; set; }
        [Required]
        [StringLength(20)]
        public string ProjectCode { get; set; }
        public short CashPolarityCode { get; set; }
        [StringLength(10)]
        public string CashPolarity { get; set; }
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [StringLength(5)]
        public string UnitOfCharge { get; set; }
    }
}
