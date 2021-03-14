using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwHistoryCashCode
    {
        public short YearNumber { get; set; }
        [Required]
        [StringLength(23)]
        public string Period { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal TotalInvoiceValue { get; set; }
        [Column(TypeName = "decimal(38, 5)")]
        public decimal TotalTaxValue { get; set; }
    }
}
