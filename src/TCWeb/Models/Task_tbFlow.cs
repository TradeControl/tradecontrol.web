using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbFlow", Schema = "Task")]
    public partial class Task_tbFlow
    {
        [Key]
        [StringLength(20)]
        public string ParentTaskCode { get; set; }
        [Key]
        public short StepNumber { get; set; }
        [StringLength(20)]
        public string ChildTaskCode { get; set; }
        public short SyncTypeCode { get; set; }
        public float OffsetDays { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 6)")]
        public decimal UsedOnQuantity { get; set; }

        [ForeignKey(nameof(ChildTaskCode))]
        [InverseProperty(nameof(Task_tbTask.TbFlowChildTaskCodeNavigations))]
        public virtual Task_tbTask ChildTaskCodeNavigation { get; set; }
        [ForeignKey(nameof(ParentTaskCode))]
        [InverseProperty(nameof(Task_tbTask.TbFlowParentTaskCodeNavigations))]
        public virtual Task_tbTask ParentTaskCodeNavigation { get; set; }
        [ForeignKey(nameof(SyncTypeCode))]
        [InverseProperty(nameof(Activity_tbSyncType.TbTaskFlows))]
        public virtual Activity_tbSyncType SyncTypeCodeNavigation { get; set; }
    }
}
