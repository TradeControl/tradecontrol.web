﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwGraphTaskActivity
    {
        [Required]
        [StringLength(111)]
        public string Category { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal SumOfTotalCharge { get; set; }
    }
}
