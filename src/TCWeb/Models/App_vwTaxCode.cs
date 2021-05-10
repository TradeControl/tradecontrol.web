using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("vwTaxCode", Schema = "App")]
    public partial class App_vwTaxCode
    {
        [Key]
        [Required]
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Tax")]
        public string TaxDescription { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Tax Type")]
        public string TaxType { get; set; }
    }
}
