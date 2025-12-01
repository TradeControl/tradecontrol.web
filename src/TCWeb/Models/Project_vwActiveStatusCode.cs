using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Project_vwActiveStatusCode
    {
        public short ProjectStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string ProjectStatus { get; set; }
    }
}
