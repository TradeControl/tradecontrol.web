using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwSummary
    {
        [Required]
        [StringLength(4000)]
        public string PeriodOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        public int InvoiceTypeCode { get; set; }
        [StringLength(20)]
        public string InvoiceType { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal TotalInvoiceValue { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal TotalTaxValue { get; set; }
    }
}
