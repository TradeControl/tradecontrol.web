using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbUoc", Schema = "App")]
    public partial class App_tbUoc
    {
        public App_tbUoc()
        {
            TbOptions = new HashSet<App_tbOption>();
        }

        [Key]
        [StringLength(5)]
        public string UnitOfCharge { get; set; }
        [Required]
        [StringLength(10)]
        public string UocSymbol { get; set; }
        [Required]
        [StringLength(100)]
        public string UocName { get; set; }

        [InverseProperty(nameof(App_tbOption.UnitOfChargeNavigation))]
        public virtual ICollection<App_tbOption> TbOptions { get; set; }
    }
}
