using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbUom", Schema = "App")]
    public partial class App_tbUom
    {
        public App_tbUom()
        {
            TbActivities = new HashSet<Object_tbObject>();
        }

        [Key]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }

        [InverseProperty(nameof(Object_tbObject.UnitOfMeasureNavigation))]
        public virtual ICollection<Object_tbObject> TbActivities { get; set; }
    }
}
