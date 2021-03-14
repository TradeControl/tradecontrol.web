﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbChangeStatus", Schema = "Cash")]
    public partial class Cash_tbChangeStatus
    {
        [Key]
        public short ChangeStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        public string ChangeStatus { get; set; }
    }
}
