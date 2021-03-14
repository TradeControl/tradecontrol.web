using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwPeriod
    {
        public short YearNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [StringLength(21)]
        public string Description { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
