using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbFlow", Schema = "Project")]
    public partial class Project_tbFlow
    {
        [Key]
        [StringLength(20)]
        public string ParentProjectCode { get; set; }
        [Key]
        public short StepNumber { get; set; }
        [StringLength(20)]
        public string ChildProjectCode { get; set; }
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

        [ForeignKey(nameof(ChildProjectCode))]
        [InverseProperty(nameof(Project_tbProject.TbFlowChildProjectCodeNavigations))]
        public virtual Project_tbProject ChildProjectCodeNavigation { get; set; }
        [ForeignKey(nameof(ParentProjectCode))]
        [InverseProperty(nameof(Project_tbProject.TbFlowParentProjectCodeNavigations))]
        public virtual Project_tbProject ParentProjectCodeNavigation { get; set; }
        [ForeignKey(nameof(SyncTypeCode))]
        [InverseProperty(nameof(Object_tbSyncType.TbProjectFlows))]
        public virtual Object_tbSyncType SyncTypeCodeNavigation { get; set; }
    }
}
