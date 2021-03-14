using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbFlow", Schema = "Activity")]
    public partial class Activity_tbFlow
    {
        [Key]
        [StringLength(50)]
        public string ParentCode { get; set; }
        [Key]
        public short StepNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string ChildCode { get; set; }
        public short SyncTypeCode { get; set; }
        public short OffsetDays { get; set; }
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

        [ForeignKey(nameof(ChildCode))]
        [InverseProperty(nameof(Activity_tbActivity.TbFlowChildCodeNavigations))]
        public virtual Activity_tbActivity ChildCodeNavigation { get; set; }
        [ForeignKey(nameof(ParentCode))]
        [InverseProperty(nameof(Activity_tbActivity.TbFlowParentCodeNavigations))]
        public virtual Activity_tbActivity ParentCodeNavigation { get; set; }
        [ForeignKey(nameof(SyncTypeCode))]
        [InverseProperty(nameof(Activity_tbSyncType.TbActivityFlows))]
        public virtual Activity_tbSyncType SyncTypeCodeNavigation { get; set; }
    }
}
