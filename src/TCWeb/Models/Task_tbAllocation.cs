using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAllocation", Schema = "Task")]
    [Index(nameof(AccountCode), nameof(AllocationCode), Name = "IX_Task_tbAllocation_ObjectCode")]
    [Index(nameof(AccountCode), nameof(TaskCode), Name = "IX_Task_tbAllocation_TaskCode")]
    [Index(nameof(TaskStatusCode), nameof(AccountCode), nameof(AllocationCode), nameof(ActionOn), Name = "IX_Task_tbAllocation_TaskStatusCode")]
    public partial class Task_tbAllocation
    {
        public Task_tbAllocation()
        {
            TbAllocationEvents = new HashSet<Task_tbAllocationEvent>();
        }

        [Key]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
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
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [StringLength(5)]
        public string UnitOfCharge { get; set; }
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

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbAllocations))]
        public virtual Subject_tbSubject AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(CashModeCode))]
        [InverseProperty(nameof(Cash_tbMode.TbAllocations))]
        public virtual Cash_tbMode CashModeCodeNavigation { get; set; }
        [ForeignKey(nameof(TaskStatusCode))]
        [InverseProperty(nameof(Task_tbStatus.TbAllocations))]
        public virtual Task_tbStatus TaskStatusCodeNavigation { get; set; }
        [InverseProperty(nameof(Task_tbAllocationEvent.ContractAddressNavigation))]
        public virtual ICollection<Task_tbAllocationEvent> TbAllocationEvents { get; set; }
    }
}
