using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCalendar", Schema = "App")]
    public partial class App_tbCalendar
    {
        public App_tbCalendar()
        {
            TbCalendarHolidays = new HashSet<App_tbCalendarHoliday>();
            TbUsers = new HashSet<Usr_tbUser>();
        }

        [Key]
        [StringLength(10)]
        [Display(Name = "Calendar Code")]
        public string CalendarCode { get; set; }
        [Required]
        public bool Monday { get; set; }
        [Required]
        public bool Tuesday { get; set; }
        [Required]
        public bool Wednesday { get; set; }
        [Required]
        public bool Thursday { get; set; }
        [Required]
        public bool Friday { get; set; }
        [Required]
        public bool Saturday { get; set; }
        [Required]
        public bool Sunday { get; set; }
        //[Required]
        //public byte[] RowVer { get; set; }

        [InverseProperty(nameof(App_tbCalendarHoliday.CalendarCodeNavigation))]
        public virtual ICollection<App_tbCalendarHoliday> TbCalendarHolidays { get; set; }
        [InverseProperty(nameof(Usr_tbUser.CalendarCodeNavigation))]
        public virtual ICollection<Usr_tbUser> TbUsers { get; set; }
    }
}
