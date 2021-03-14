using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Task_vwAttributesForOrder
    {
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [Required]
        [StringLength(50)]
        public string Attribute { get; set; }
        public short PrintOrder { get; set; }
        [StringLength(400)]
        public string AttributeDescription { get; set; }
    }
}
