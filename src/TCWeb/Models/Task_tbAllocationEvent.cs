using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAllocationEvent", Schema = "Task")]
    [Index(nameof(EventTypeCode), nameof(TaskStatusCode), nameof(InsertedOn), Name = "IX_Task_tbAllocationEvent_EventTypeCide")]
    public partial class Task_tbAllocationEvent
    {
        [Key]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Key]
        public int LogId { get; set; }
        public short EventTypeCode { get; set; }
        public short TaskStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionOn { get; set; }
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
        [Column(TypeName = "decimal(18, 7)")]
        public decimal UnitCharge { get; set; }

        [ForeignKey(nameof(ContractAddress))]
        [InverseProperty(nameof(Task_tbAllocation.TbAllocationEvents))]
        public virtual Task_tbAllocation ContractAddressNavigation { get; set; }
        [ForeignKey(nameof(EventTypeCode))]
        [InverseProperty(nameof(App_tbEventType.TbAllocationEvents))]
        public virtual App_tbEventType EventTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(TaskStatusCode))]
        [InverseProperty(nameof(Task_tbStatus.TbAllocationEvents))]
        public virtual Task_tbStatus TaskStatusCodeNavigation { get; set; }
    }
}
