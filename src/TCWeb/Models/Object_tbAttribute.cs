using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAttribute", Schema = "Object")]
    public partial class Object_tbAttribute
    {
        [Key]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [Key]
        [StringLength(50)]
        public string Attribute { get; set; }
        public short PrintOrder { get; set; }
        public short AttributeTypeCode { get; set; }
        [StringLength(400)]
        public string DefaultText { get; set; }
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

        [ForeignKey(nameof(ObjectCode))]
        [InverseProperty(nameof(Object_tbObject.TbAttributes))]
        public virtual Object_tbObject ObjectCodeNavigation { get; set; }
        [ForeignKey(nameof(AttributeTypeCode))]
        [InverseProperty(nameof(Object_tbAttributeType.TbAttributes))]
        public virtual Object_tbAttributeType AttributeTypeCodeNavigation { get; set; }
    }
}
