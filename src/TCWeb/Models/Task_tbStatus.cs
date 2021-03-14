using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbStatus", Schema = "Task")]
    public partial class Task_tbStatus
    {
        public Task_tbStatus()
        {
            TbAllocationEvents = new HashSet<Task_tbAllocationEvent>();
            TbAllocations = new HashSet<Task_tbAllocation>();
            TbTasks = new HashSet<Task_tbTask>();
        }

        [Key]
        public short TaskStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string TaskStatus { get; set; }

        [InverseProperty(nameof(Task_tbAllocationEvent.TaskStatusCodeNavigation))]
        public virtual ICollection<Task_tbAllocationEvent> TbAllocationEvents { get; set; }
        [InverseProperty(nameof(Task_tbAllocation.TaskStatusCodeNavigation))]
        public virtual ICollection<Task_tbAllocation> TbAllocations { get; set; }
        [InverseProperty(nameof(Task_tbTask.TaskStatusCodeNavigation))]
        public virtual ICollection<Task_tbTask> TbTasks { get; set; }
    }
}
