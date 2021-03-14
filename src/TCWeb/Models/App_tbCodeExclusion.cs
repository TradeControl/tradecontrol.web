using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCodeExclusion", Schema = "App")]
    public partial class App_tbCodeExclusion
    {
        [Key]
        [StringLength(100)]
        public string ExcludedTag { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
