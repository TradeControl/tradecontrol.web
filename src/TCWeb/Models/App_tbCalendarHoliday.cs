using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCalendarHoliday", Schema = "App")]
    public partial class App_tbCalendarHoliday
    {
        [Key]
        [StringLength(10)]
        public string CalendarCode { get; set; }
        [Key]
        [Column(TypeName = "datetime")]
        public DateTime UnavailableOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(CalendarCode))]
        [InverseProperty(nameof(App_tbCalendar.TbCalendarHolidays))]
        public virtual App_tbCalendar CalendarCodeNavigation { get; set; }
    }
}
