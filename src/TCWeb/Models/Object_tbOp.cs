using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbOp", Schema = "Object")]
    public partial class Object_tbOp
    {
        [Key]
        [StringLength(50)]
        public string ObjectCode { get; set; }
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

        [ForeignKey(nameof(ObjectCode))]
        [InverseProperty(nameof(Object_tbObject.TbOps))]
        public virtual Object_tbObject ObjectCodeNavigation { get; set; }
        [ForeignKey(nameof(SyncTypeCode))]
        [InverseProperty(nameof(Object_tbSyncType.TbObjectOps))]
        public virtual Object_tbSyncType SyncTypeCodeNavigation { get; set; }
    }
}
