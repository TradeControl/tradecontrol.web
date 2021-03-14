using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwRegisterDetail
    {
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string TaskCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [StringLength(50)]
        public string TaxDescription { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
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
        public double Quantity { get; set; }
        public double InvoiceValue { get; set; }
        public double TaxValue { get; set; }
    }
}
