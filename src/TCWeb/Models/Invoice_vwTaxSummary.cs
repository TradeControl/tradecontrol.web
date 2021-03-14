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
        public string InvoiceNumber { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValueTotal { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValueTotal { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxRate { get; set; }
    }
}
