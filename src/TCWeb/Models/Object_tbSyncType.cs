using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbSyncType", Schema = "Object")]
    public partial class Object_tbSyncType
    {
        public Object_tbSyncType()
        {
            TbTaskFlows = new HashSet<Task_tbFlow>();
            TbObjectFlows = new HashSet<Object_tbFlow>();
            TbTaskOps = new HashSet<Task_tbOp>();
            TbObjectOps = new HashSet<Object_tbOp>();
        }

        [Key]
        public short SyncTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        public string SyncType { get; set; }

        [InverseProperty(nameof(Task_tbFlow.SyncTypeCodeNavigation))]
        public virtual ICollection<Task_tbFlow> TbTaskFlows { get; set; }
        [InverseProperty(nameof(Object_tbFlow.SyncTypeCodeNavigation))]
        public virtual ICollection<Object_tbFlow> TbObjectFlows { get; set; }
        [InverseProperty(nameof(Task_tbOp.SyncTypeCodeNavigation))]
        public virtual ICollection<Task_tbOp> TbTaskOps { get; set; }
        [InverseProperty(nameof(Object_tbOp.SyncTypeCodeNavigation))]
        public virtual ICollection<Object_tbOp> TbObjectOps { get; set; }
    }
}
