using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbOp", Schema = "Activity")]
    public partial class Activity_tbOp
    {
        [Key]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        [Key]
        public short OperationNumber { get; set; }
        public short SyncTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        public string Operation { get; set; }
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
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Duration { get; set; }

        [ForeignKey(nameof(ActivityCode))]
        [InverseProperty(nameof(Activity_tbActivity.TbOps))]
        public virtual Activity_tbActivity ActivityCodeNavigation { get; set; }
        [ForeignKey(nameof(SyncTypeCode))]
        [InverseProperty(nameof(Activity_tbSyncType.TbActivityOps))]
        public virtual Activity_tbSyncType SyncTypeCodeNavigation { get; set; }
    }
}
