using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Invoice_vwDoc
    {
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Display(Name = "Owner")]
        public string UserName { get; set; }
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }
        [Display(Name = "Account Name")]
        public string SubjectName { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Address")]
        public string InvoiceAddress { get; set; }
        [Display(Name = "Invoice No.")]
        public string InvoiceNumber { get; set; }
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
        [Display(Name = "Status")]
        public string InvoiceStatus { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Invoiced")]
        [DataType(DataType.Date)]
        public DateTime InvoicedOn { get; set; }
        [Display(Name = "Due")]
        [DataType(DataType.Date)]
        public DateTime DueOn { get; set; }
        [Display(Name = "Charge")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Display(Name = "Tax")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalValue { get; set; }
        [Display(Name = "Terms")]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }

}
