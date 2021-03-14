using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwInvoiceTask
    {
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [StringLength(100)]
        public string ContactName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        [StringLength(255)]
        public string TaskNotes { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        public short InvoiceStatusCode { get; set; }
        [StringLength(100)]
        public string TaskTitle { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        public short InvoiceTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        public short CashModeCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidTaxValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidValue { get; set; }
    }
}
