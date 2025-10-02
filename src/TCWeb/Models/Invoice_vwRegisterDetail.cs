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
        [DataType(DataType.Date)]
        [Display(Name = "Period")]
        public DateTime StartOn { get; set; }
        [StringLength(20)]
        [Display(Name = "Invoice No.")]
        public string InvoiceNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Item Code")]
        public string ProjectCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [StringLength(100)]
        [Display(Name = "Cash Desc.")]
        public string CashDescription { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Tax Desc.")]
        public string TaxDescription { get; set; }
        [StringLength(10)]
        [Display(Name = "A/c")]
        public string SubjectCode { get; set; }
        [Display(Name = "Type Code")]
        public short InvoiceTypeCode { get; set; }
        [Display(Name = "Status Code")]
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        [DataType(DataType.Date)]
        [Display(Name = "Invoiced")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Due")]
        [DataType(DataType.Date)]
        public DateTime DueOn { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Expected")]
        [DataType(DataType.Date)]
        public DateTime ExpectedOn { get; set; }
        [StringLength(100)]
        [Display(Name = "Terms")]
        public string PaymentTerms { get; set; }
        [Display(Name = "Emailed?")]
        public bool Printed { get; set; }
        [StringLength(255)]
        [Display(Name = "Name")]
        public string SubjectName { get; set; }
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
        public short CashPolarityCode { get; set; }
        [StringLength(20)]
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
        [Display(Name = "Quantity")]
        public double Quantity { get; set; }
        [DataType(DataType.Currency)]
        [Display(Name = "Invoice Value")]
        public double InvoiceValue { get; set; }
        [DataType(DataType.Currency)]
        [Display(Name = "Tax Value")]
        public double TaxValue { get; set; }
        [DataType(DataType.Currency)]
        [Display(Name = "Total Charge")]
        public double TotalValue { get; set; }
        [Display(Name = "Is Project?")]
        public bool IsProject { get; set; }
        [Display(Name = "Ref.")]
        public string ItemReference { get; set; }
    }
}
