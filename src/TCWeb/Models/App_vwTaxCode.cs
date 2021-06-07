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
        [StringLength(50)]
        [Display(Name = "Tax")]
        public string TaxDescription { get; set; }
        [StringLength(20)]
        [Display(Name = "Tax Type")]
        public string TaxType { get; set; }
        public short TaxTypeCode { get; set; }
        public short RoundingCode { get; set; }
        [StringLength(20)]
        [Display(Name = "Rounding")]
        public string Rounding { get; set; }
        [Display(Name = "Decimal Places")]
        public short Decimals { get; set; }
        [Column(TypeName = "percent")]
        [Display(Name = "Tax Rate")]
        [DisplayFormat(DataFormatString = "{0:p}")]
        public decimal TaxRate { get; set; }
        [StringLength(50)]
        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Updated On")]
        [DataType(DataType.Date)]
        public DateTime UpdatedOn { get; set; }
    }
}
