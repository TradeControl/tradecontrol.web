using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwActivePeriod
    {
        public short YearNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(10)]
        public string Description { get; set; }
        public short MonthNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string MonthName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime EndOn { get; set; }
    }
}
