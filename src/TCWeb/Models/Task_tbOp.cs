using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbOp", Schema = "Task")]
    public partial class Task_tbOp
    {
        [Key]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Key]
        public short OperationNumber { get; set; }
        public short SyncTypeCode { get; set; }
        public short OpStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        [Required]
        [StringLength(50)]
        public string Operation { get; set; }
        [Column(TypeName = "ntext")]
        public string Note { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime EndOn { get; set; }
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

        [ForeignKey(nameof(OpStatusCode))]
        [InverseProperty(nameof(Task_tbOpStatus.TbOps))]
        public virtual Task_tbOpStatus OpStatusCodeNavigation { get; set; }
        [ForeignKey(nameof(SyncTypeCode))]
        [InverseProperty(nameof(Object_tbSyncType.TbTaskOps))]
        public virtual Object_tbSyncType SyncTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(TaskCode))]
        [InverseProperty(nameof(Task_tbTask.TbOps))]
        public virtual Task_tbTask TaskCodeNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbOps))]
        public virtual Usr_tbUser User { get; set; }
    }
}
