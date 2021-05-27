using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwRegister
    {
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Required]
        [StringLength(20)]
        [Display (Name = "Invoice No.")]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Invoiced")]
        [DataType(DataType.Date)]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        [Display(Name = "Charge")]
        [DataType(DataType.Currency)]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        [Display(Name = "Tax")]
        [DataType(DataType.Currency)]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        [Display(Name = "Paid Charge")]
        [DataType(DataType.Currency)]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        [Display(Name = "Paid Tax")]
        [DataType(DataType.Currency)]

        public decimal PaidTaxValue { get; set; }
        [StringLength(100)]
        [Display(Name = "Terms")]

        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        [Display(Name = "Printed?")]
        public bool Printed { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "User")]
        public string UserName { get; set; }
        [StringLength(50)]
        [Display(Name = "Status")]
        public string InvoiceStatus { get; set; }
        [Display(Name = "Mode Code")]
        public short CashModeCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
    }
}
