using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbInvoice", Schema = "Invoice")]
    [Index(nameof(AccountCode), nameof(InvoiceTypeCode), nameof(DueOn), Name = "IX_Invoice_tbInvoice_AccountCode_DueOn")]
    [Index(nameof(AccountCode), nameof(InvoiceStatusCode), nameof(InvoiceNumber), Name = "IX_Invoice_tbInvoice_AccountCode_Status")]
    [Index(nameof(AccountCode), nameof(InvoiceNumber), nameof(InvoiceTypeCode), Name = "IX_Invoice_tbInvoice_AccountCode_Type")]
    [Index(nameof(AccountCode), nameof(InvoiceStatusCode), nameof(InvoiceNumber), Name = "IX_Invoice_tbInvoice_AccountValues")]
    [Index(nameof(ExpectedOn), nameof(InvoiceTypeCode), nameof(InvoiceStatusCode), Name = "IX_Invoice_tbInvoice_ExpectedOn")]
    [Index(nameof(InvoiceTypeCode), nameof(UserId), nameof(InvoiceStatusCode), nameof(AccountCode), nameof(InvoiceNumber), nameof(InvoicedOn), nameof(PaymentTerms), nameof(Printed), Name = "IX_Invoice_tbInvoice_FlowInitialise")]
    public partial class Invoice_tbInvoice
    {
        public Invoice_tbInvoice()
        {
            TbChangeLogs = new HashSet<Invoice_tbChangeLog>();
            TbItems = new HashSet<Invoice_tbItem>();
            TbTasks = new HashSet<Invoice_tbTask>();
        }

        [Key]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Display(Name = "Type")]
        public short InvoiceTypeCode { get; set; }
        [Display(Name = "Status")]
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Invoice Date")]
        [DataType(DataType.Date)]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Expected")]
        [DataType(DataType.Date)]
        public DateTime ExpectedOn { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Due")]
        [DataType(DataType.Date)]
        public DateTime DueOn { get; set; }
        [StringLength(100)]
        [Display(Name = "Terms")]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        [Display(Name = "Sent?")]
        public bool Printed { get; set; }
        [Display(Name = "Spooled?")]
        public bool Spooled { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Invoice Value")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Tax Value")]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Paid Value")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Paid Tax")]
        public decimal PaidTaxValue { get; set; }

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbInvoices))]
        public virtual Subject_tbSubject AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(InvoiceStatusCode))]
        [InverseProperty(nameof(Invoice_tbStatus.TbInvoices))]
        public virtual Invoice_tbStatus InvoiceStatusCodeNavigation { get; set; }
        [ForeignKey(nameof(InvoiceTypeCode))]
        [InverseProperty(nameof(Invoice_tbType.TbInvoices))]
        public virtual Invoice_tbType InvoiceTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbInvoices))]
        public virtual Usr_tbUser User { get; set; }
        [InverseProperty("InvoiceNumberNavigation")]
        public virtual Cash_tbChangeReference TbChangeReference { get; set; }
        [InverseProperty("InvoiceNumberNavigation")]
        public virtual Invoice_tbMirrorReference TbMirrorReference { get; set; }
        [InverseProperty(nameof(Invoice_tbChangeLog.InvoiceNumberNavigation))]
        public virtual ICollection<Invoice_tbChangeLog> TbChangeLogs { get; set; }
        [InverseProperty(nameof(Invoice_tbItem.InvoiceNumberNavigation))]
        public virtual ICollection<Invoice_tbItem> TbItems { get; set; }
        [InverseProperty(nameof(Invoice_tbTask.InvoiceNumberNavigation))]
        public virtual ICollection<Invoice_tbTask> TbTasks { get; set; }
    }
}
