using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwDebitNoteSpool
    {
        public bool Printed { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValueTotal { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValueTotal { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        public string Notes { get; set; }
        [StringLength(255)]
        public string EmailAddress { get; set; }
        [Column(TypeName = "ntext")]
        public string InvoiceAddress { get; set; }
        [Required]
        [StringLength(20)]
        public string TaskCode { get; set; }
        [StringLength(100)]
        public string TaskTitle { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ActionedOn { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Required]
        [StringLength(15)]
        public string UnitOfMeasure { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
    }
}
