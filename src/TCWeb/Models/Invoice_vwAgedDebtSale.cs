using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwAgedDebtSale
    {
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal UnpaidValue { get; set; }
        public int UnpaidDays { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ExpectedOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        public string Notes { get; set; }
    }
}
