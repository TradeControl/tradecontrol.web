using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbYear", Schema = "App")]
    public partial class App_tbYear
    {
        public App_tbYear()
        {
            TbYearPeriods = new HashSet<App_tbYearPeriod>();
        }

        [Key]
        public short YearNumber { get; set; }
        public short StartMonth { get; set; }
        public short CashStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(StartMonth))]
        [InverseProperty(nameof(App_tbMonth.TbYears))]
        public virtual App_tbMonth StartMonthNavigation { get; set; }
        [InverseProperty(nameof(App_tbYearPeriod.YearNumberNavigation))]
        public virtual ICollection<App_tbYearPeriod> TbYearPeriods { get; set; }
    }
}
