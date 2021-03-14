using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwCandidateCategoryCode
    {
        [Required]
        [StringLength(10)]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
    }
}
