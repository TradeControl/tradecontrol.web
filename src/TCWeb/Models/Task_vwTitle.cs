using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Task_vwTitle
    {
        [Required]
        [StringLength(50)]
        public string ActivityCode { get; set; }
        [StringLength(100)]
        public string TaskTitle { get; set; }
    }
}
