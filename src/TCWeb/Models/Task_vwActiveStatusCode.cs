using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Task_vwActiveStatusCode
    {
        public short TaskStatusCode { get; set; }
        [Required]
        [StringLength(100)]
        public string TaskStatus { get; set; }
    }
}
