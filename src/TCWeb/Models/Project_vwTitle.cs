using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Project_vwTitle
    {
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [StringLength(100)]
        public string ProjectTitle { get; set; }
    }
}
