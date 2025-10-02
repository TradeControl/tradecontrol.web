using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMirror", Schema = "Invoice")]
    [Index(nameof(SubjectCode), nameof(InvoiceNumber), Name = "IX_Invoice_tbMirror_InvoiceNumber", IsUnique = true)]
    public partial class Invoice_tbMirror
    {
        public Invoice_tbMirror()
        {
            TbMirrorEvents = new HashSet<Invoice_tbMirrorEvent>();
            TbMirrorItems = new HashSet<Invoice_tbMirrorItem>();
            TbMirrorProjects = new HashSet<Invoice_tbMirrorProject>();
        }

        [Key]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueOn { get; set; }
        [StringLength(5)]
        public string UnitOfCharge { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceTax { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidTaxValue { get; set; }
        [StringLength(42)]
        public string PaymentAddress { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbInvoiceMirror))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
        [ForeignKey(nameof(InvoiceStatusCode))]
        [InverseProperty(nameof(Invoice_tbStatus.TbMirror))]
        public virtual Invoice_tbStatus InvoiceStatusCodeNavigation { get; set; }
        [ForeignKey(nameof(InvoiceTypeCode))]
        [InverseProperty(nameof(Invoice_tbType.TbMirrors))]
        public virtual Invoice_tbType InvoiceTypeCodeNavigation { get; set; }
        [InverseProperty("ContractAddressNavigation")]
        public virtual Invoice_tbMirrorReference TbMirrorReference { get; set; }
        [InverseProperty(nameof(Invoice_tbMirrorEvent.ContractAddressNavigation))]
        public virtual ICollection<Invoice_tbMirrorEvent> TbMirrorEvents { get; set; }
        [InverseProperty(nameof(Invoice_tbMirrorItem.ContractAddressNavigation))]
        public virtual ICollection<Invoice_tbMirrorItem> TbMirrorItems { get; set; }
        [InverseProperty(nameof(Invoice_tbMirrorProject.ContractAddressNavigation))]
        public virtual ICollection<Invoice_tbMirrorProject> TbMirrorProjects { get; set; }
    }
}
