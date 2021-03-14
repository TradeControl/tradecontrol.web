using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwVersion
    {
        [Required]
        [StringLength(36)]
        public string VersionString { get; set; }
        [Column("SQLDataVersion")]
        public double SqldataVersion { get; set; }
        [Column("SQLRelease")]
        public int Sqlrelease { get; set; }
    }
}
