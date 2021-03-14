using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwPeriodEndListing
    {
        public short YearNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        [Required]
        [StringLength(50)]
        public string YearInsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime YearInsertedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(10)]
        public string MonthName { get; set; }
        [Required]
        [StringLength(50)]
        public string PeriodInsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime PeriodInsertedOn { get; set; }
        [Required]
        [StringLength(15)]
        public string CashStatus { get; set; }
    }
}
