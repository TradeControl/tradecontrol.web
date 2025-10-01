using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMirror", Schema = "Object")]
    [Index(nameof(AccountCode), nameof(AllocationCode), Name = "IX_Object_tbMirror_AllocationCode", IsUnique = true)]
    [Index(nameof(TransmitStatusCode), nameof(AllocationCode), Name = "IX_Object_tbMirror_TransmitStatusCode")]
    public partial class Object_tbMirror
    {
        [Key]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [Key]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Key]
        [StringLength(50)]
        public string AllocationCode { get; set; }
        public short TransmitStatusCode { get; set; }
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

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbMirrors))]
        public virtual Subject_tbSubject AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(ObjectCode))]
        [InverseProperty(nameof(Object_tbObject.TbMirrors))]
        public virtual Object_tbObject ObjectCodeNavigation { get; set; }
        [ForeignKey(nameof(TransmitStatusCode))]
        [InverseProperty(nameof(Subject_tbTransmitStatus.TbObjectMirrors))]
        public virtual Subject_tbTransmitStatus TransmitStatusCodeNavigation { get; set; }
    }
}
