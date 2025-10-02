using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwHistorySalesItem
    {
        public short YearNumber { get; set; }
        [Required]
        [StringLength(23)]
        public string PeriodName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string ProjectCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        public double InvoiceValue { get; set; }
        public double TaxValue { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
    }
}
