using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMonth", Schema = "App")]
    public partial class App_tbMonth
    {
        public App_tbMonth()
        {
            TbTaxTypes = new HashSet<Cash_tbTaxType>();
            TbYearPeriods = new HashSet<App_tbYearPeriod>();
            TbYears = new HashSet<App_tbYear>();
        }

        [Key]
        public short MonthNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string MonthName { get; set; }

        [InverseProperty(nameof(Cash_tbTaxType.MonthNumberNavigation))]
        public virtual ICollection<Cash_tbTaxType> TbTaxTypes { get; set; }
        [InverseProperty(nameof(App_tbYearPeriod.MonthNumberNavigation))]
        public virtual ICollection<App_tbYearPeriod> TbYearPeriods { get; set; }
        [InverseProperty(nameof(App_tbYear.StartMonthNavigation))]
        public virtual ICollection<App_tbYear> TbYears { get; set; }
    }
}
