using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwTaxCode
    {
        [Required]
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Required]
        [StringLength(50)]
        public string TaxDescription { get; set; }
        [Required]
        [StringLength(20)]
        public string TaxType { get; set; }
    }
}
