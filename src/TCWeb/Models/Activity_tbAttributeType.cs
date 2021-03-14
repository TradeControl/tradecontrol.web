using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAttributeType", Schema = "Activity")]
    public partial class Activity_tbAttributeType
    {
        public Activity_tbAttributeType()
        {
            TbAttribute1s = new HashSet<Task_tbAttribute>();
            TbAttributes = new HashSet<Activity_tbAttribute>();
        }

        [Key]
        public short AttributeTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string AttributeType { get; set; }

        [InverseProperty(nameof(Task_tbAttribute.AttributeTypeCodeNavigation))]
        public virtual ICollection<Task_tbAttribute> TbAttribute1s { get; set; }
        [InverseProperty(nameof(Activity_tbAttribute.AttributeTypeCodeNavigation))]
        public virtual ICollection<Activity_tbAttribute> TbAttributes { get; set; }
    }
}
