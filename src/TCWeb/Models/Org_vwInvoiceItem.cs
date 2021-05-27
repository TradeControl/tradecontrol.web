using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwInvoiceItem
    {
        [Required]
        [StringLength(10)]
        [Display (Name = "Account Code")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Invoice No.")]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [Column(TypeName = "datetime")]
        [DataType (DataType.Date)]
        [Display(Name = "Invoiced")]
        public DateTime InvoicedOn { get; set; }
        [Display(Name = "Type Code")]
        public short InvoiceTypeCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Status")]
        public string InvoiceStatus { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Cash Description")]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        [Display(Name = "Status Code")]
        public short InvoiceStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax")]
        [DataType (DataType.Currency)]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Charge")]
        [DataType(DataType.Currency)]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Paid")]
        [DataType(DataType.Currency)]

        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Tax")]
        [DataType(DataType.Currency)]
        public decimal PaidTaxValue
        {
            get; set;
        } 
        [Column(TypeName = "ntext")]
        [Display(Name = "Reference")]
        public string ItemReference { get; set; }
    }
}
