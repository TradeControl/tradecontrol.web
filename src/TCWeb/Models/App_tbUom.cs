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
            TbActivities = new HashSet<Activity_tbActivity>();
        }

        [Key]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [InverseProperty(nameof(Activity_tbActivity.UnitOfMeasureNavigation))]
        public virtual ICollection<Activity_tbActivity> TbActivities { get; set; }
    }
}
