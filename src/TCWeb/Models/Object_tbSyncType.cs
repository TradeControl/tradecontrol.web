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
            TbProjectFlows = new HashSet<Project_tbFlow>();
            TbObjectFlows = new HashSet<Object_tbFlow>();
            TbProjectOps = new HashSet<Project_tbOp>();
            TbObjectOps = new HashSet<Object_tbOp>();
        }

        [Key]
        public short SyncTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        public string SyncType { get; set; }

        [InverseProperty(nameof(Project_tbFlow.SyncTypeCodeNavigation))]
        public virtual ICollection<Project_tbFlow> TbProjectFlows { get; set; }
        [InverseProperty(nameof(Object_tbFlow.SyncTypeCodeNavigation))]
        public virtual ICollection<Object_tbFlow> TbObjectFlows { get; set; }
        [InverseProperty(nameof(Project_tbOp.SyncTypeCodeNavigation))]
        public virtual ICollection<Project_tbOp> TbProjectOps { get; set; }
        [InverseProperty(nameof(Object_tbOp.SyncTypeCodeNavigation))]
        public virtual ICollection<Object_tbOp> TbObjectOps { get; set; }
    }
}
