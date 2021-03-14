using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwHistorySale
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
        [StringLength(10)]
        public string AccountCode { get; set; }
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "money")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "money")]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "money")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "money")]
        public decimal PaidTaxValue { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        public string Notes { get; set; }
        public bool Printed { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        public short CashModeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        [Column(TypeName = "money")]
        public decimal UnpaidValue { get; set; }
    }
}
