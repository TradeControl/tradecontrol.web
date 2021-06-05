using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwRegisterOverdue
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Invoice No.")]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "A/c")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Name")]
        public string AccountName { get; set; }
        [Display(Name = "Type Code")]
        public short InvoiceTypeCode { get; set; }
        [Display(Name = "Status Code")]
        public short InvoiceStatusCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Status")]
        public string InvoiceStatus { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
        [Display(Name = "Days Unpaid")]
        public int UnpaidDays { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Invoiced")]
        [DataType(DataType.Date)]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Due")]
        [DataType(DataType.Date)]
        public DateTime DueOn { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Expected")]
        [DataType(DataType.Date)]
        public DateTime ExpectedOn { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        [Display(Name = "Invoice Value")]
        [DataType(DataType.Currency)]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        [Display(Name = "Tax Value")]
        [DataType(DataType.Currency)]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "decimal(22, 5)")]
        [Display(Name = "Unpaid")]
        [DataType(DataType.Currency)]
        public decimal UnpaidValue { get; set; }
        [StringLength(100)]
        [Display(Name = "Terms")]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Note")]
        public string Notes { get; set; }
    }
}
