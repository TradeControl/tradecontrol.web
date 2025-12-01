using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Project_vwNetworkEvent
    {
        [Required]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        public int LogId { get; set; }
        public short EventTypeCode { get; set; }
        [Required]
        [StringLength(15)]
        public string EventType { get; set; }
        public short ProjectStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }
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
    }
}
