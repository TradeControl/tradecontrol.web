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
        [Required]
        [StringLength(10)]
        public string MonthName { get; set; }
        public short CashStatusCode { get; set; }
        public short YearNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
