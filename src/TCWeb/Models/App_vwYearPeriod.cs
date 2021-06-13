using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwYearPeriod
    {
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        public short MonthNumber { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name ="Month")]
        public string MonthName { get; set; }
        [Display(Name = "Status")]
        public string CashStatus { get; set; }
        public short CashStatusCode { get; set; }
        public short YearNumber { get; set; }
        [Column(TypeName = "datetime")]
        [DataType(DataType.Date)]
        [Display(Name ="Start Date")]
        public DateTime StartOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
