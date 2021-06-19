using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwTaxSummary
    {
        [Required]
        [StringLength(20)]
        [Display(Name ="Invoice No.")]
        public string InvoiceNumber { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Invoice Value")]
        [DataType(DataType.Currency)]
        public decimal InvoiceValueTotal { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax Value")]
        [DataType(DataType.Currency)]
        public decimal TaxValueTotal { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax Rate")]
        [DisplayFormat(DataFormatString = "{2:p}")]
        public decimal TaxRate { get; set; }
    }
}
