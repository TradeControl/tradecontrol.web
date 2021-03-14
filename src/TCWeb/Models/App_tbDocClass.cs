using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbDocClass", Schema = "App")]
    public partial class App_tbDocClass
    {
        public App_tbDocClass()
        {
            TbDocTypes = new HashSet<App_tbDocType>();
        }

        [Key]
        public short DocClassCode { get; set; }
        [Required]
        [StringLength(50)]
        public string DocClass { get; set; }

        [InverseProperty(nameof(App_tbDocType.DocClassCodeNavigation))]
        public virtual ICollection<App_tbDocType> TbDocTypes { get; set; }
    }
}
