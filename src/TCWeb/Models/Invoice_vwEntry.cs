using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwEntry
    {
        [StringLength(10)]
        [Display(Name = "UID")]
        public string UserId { get; set; }
        [StringLength(50)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [StringLength(10)]
        [Display(Name = "A/c")]
        public string SubjectCode { get; set; }
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string SubjectName { get; set; }
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [StringLength(100)]        
        [Display(Name = "Description")]
        public string CashDescription { get; set; }
        [StringLength(20)]
        [Display(Name = "Type")]
        public string InvoiceType { get; set; }
        public short InvoiceTypeCode { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Invoiced")]
        [DataType(DataType.Date)]
        public DateTime InvoicedOn { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Description")]
        public string TaxDescription { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Ref.")]
        public string ItemReference { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Total Value")]
        [DataType(DataType.Currency)]
        public decimal TotalValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Invoice Value")]
        [DataType(DataType.Currency)]
        public decimal InvoiceValue { get; set; }
        [Display(Name = "Value")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 5)")]
        public decimal EntryValue { get; set; }
    }
}
