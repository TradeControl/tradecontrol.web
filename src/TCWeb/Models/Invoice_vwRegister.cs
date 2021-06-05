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
        [StringLength(20)]
        [Display (Name = "Invoice No.")]
        public string InvoiceNumber { get; set; }
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
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
        [Display(Name = "Charge")]
        [DataType(DataType.Currency)]
        public double InvoiceValue { get; set; }
        [Display(Name = "Tax")]
        [DataType(DataType.Currency)]
        public double TaxValue { get; set; }
        [Display(Name = "Total Charged")]
        [DataType(DataType.Currency)]
        public double TotalInvoiceValue { get; set; }

        [Display(Name = "Paid Charge")]
        [DataType(DataType.Currency)]
        public double PaidValue { get; set; }
        [Display(Name = "Paid Tax")]
        [DataType(DataType.Currency)]

        public double PaidTaxValue { get; set; }
        [Display(Name = "Total Paid")]
        [DataType(DataType.Currency)]
        public double TotalPaidValue { get; set; }
        [StringLength(100)]
        [Display(Name = "Terms")]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        [Display(Name = "Sent?")]
        public bool Printed { get; set; }
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        [StringLength(50)]
        [Display(Name = "Owner")]
        public string UserName { get; set; }
        [StringLength(10)]
        [Display(Name = "User Id")]
        public string UserId { get; set; }
        [StringLength(50)]
        [Display(Name = "Status")]
        public string InvoiceStatus { get; set; }
        [Display(Name = "Mode Code")]
        public short CashModeCode { get; set; }
        [StringLength(20)]
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
    }
}
