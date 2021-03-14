using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbRegister", Schema = "App")]
    public partial class App_tbRegister
    {
        public App_tbRegister()
        {
            TbActivities = new HashSet<Activity_tbActivity>();
            TbOptions = new HashSet<App_tbOption>();
        }

        [Key]
        [StringLength(50)]
        public string RegisterName { get; set; }
        public int NextNumber { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [InverseProperty(nameof(Activity_tbActivity.RegisterNameNavigation))]
        public virtual ICollection<Activity_tbActivity> TbActivities { get; set; }
        [InverseProperty(nameof(App_tbOption.RegisterNameNavigation))]
        public virtual ICollection<App_tbOption> TbOptions { get; set; }
    }
}
