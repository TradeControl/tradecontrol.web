﻿using System;
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
        [Display(Name ="Start Year")]
        public short YearNumber { get; set; }
        [Display(Name = "Start Month")]
        public short StartMonth { get; set; }
        [Display(Name = "Status")]
        public short CashStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Financial Year")]
        public string Description { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Inserted On")]
        public DateTime InsertedOn { get; set; }

        [ForeignKey(nameof(StartMonth))]
        [InverseProperty(nameof(App_tbMonth.TbYears))]
        public virtual App_tbMonth StartMonthNavigation { get; set; }
        [InverseProperty(nameof(App_tbYearPeriod.YearNumberNavigation))]
        public virtual ICollection<App_tbYearPeriod> TbYearPeriods { get; set; }
    }
}
