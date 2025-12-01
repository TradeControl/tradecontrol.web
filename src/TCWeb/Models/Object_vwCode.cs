using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Object_vwCode
    {
        [Required]
        [StringLength(50)]
        public string ObjectCode { get; set; }
        [Required]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
    }
}
