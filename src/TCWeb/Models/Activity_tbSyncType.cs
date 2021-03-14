using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbSyncType", Schema = "Activity")]
    public partial class Activity_tbSyncType
    {
        public Activity_tbSyncType()
        {
            TbTaskFlows = new HashSet<Task_tbFlow>();
            TbActivityFlows = new HashSet<Activity_tbFlow>();
            TbTaskOps = new HashSet<Task_tbOp>();
            TbActivityOps = new HashSet<Activity_tbOp>();
        }

        [Key]
        public short SyncTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        public string SyncType { get; set; }

        [InverseProperty(nameof(Task_tbFlow.SyncTypeCodeNavigation))]
        public virtual ICollection<Task_tbFlow> TbTaskFlows { get; set; }
        [InverseProperty(nameof(Activity_tbFlow.SyncTypeCodeNavigation))]
        public virtual ICollection<Activity_tbFlow> TbActivityFlows { get; set; }
        [InverseProperty(nameof(Task_tbOp.SyncTypeCodeNavigation))]
        public virtual ICollection<Task_tbOp> TbTaskOps { get; set; }
        [InverseProperty(nameof(Activity_tbOp.SyncTypeCodeNavigation))]
        public virtual ICollection<Activity_tbOp> TbActivityOps { get; set; }
    }
}
