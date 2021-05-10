using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("vwCodeLookup", Schema = "Cash")]
    public partial class Cash_vwCodeLookup
    {
        [Key]
        [Required]
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Description")]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; }
        public short CashModeCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Mode")]
        public string CashMode { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        public short CashTypeCode { get; set; }
        [StringLength(25)]
        [Display(Name = "Type")]
        public string CashType { get; set; }
    }
}
