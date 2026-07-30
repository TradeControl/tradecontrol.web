using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbSubject", Schema = "Subject")]
    [Index(nameof(SubjectName), Name = "IX_Subject_tb_AccountName")]
    [Index(nameof(AreaCode), Name = "IX_Subject_tb_AreaCode")]
    [Index(nameof(ExportTypeCode), Name = "IX_Subject_tbSubject_ExportTypeCode")]
    [Index(nameof(SubjectStatusCode), Name = "IX_Subject_tb_SubjectStatusCode")]
    [Index(nameof(SubjectStatusCode), nameof(SubjectName), Name = "IX_Subject_tb_Status_AccountCode")]
    [Index(nameof(SubjectTypeCode), Name = "IX_Subject_tb_SubjectTypeCode")]
    [Index(nameof(SubjectCode), Name = "IX_tbSubject_tb_AccountCode")]
    public partial class Subject_tbSubject
    {
        public Subject_tbSubject()
        {
            TbAccounts = new HashSet<Subject_tbAccount>();
            TbAddresses = new HashSet<Subject_tbAddress>();
            TbAllocations = new HashSet<Project_tbAllocation>();
            TbChildNamespaces = new HashSet<Subject_tbNamespace>();
            TbDocs = new HashSet<Subject_tbDoc>();
            TbEntries = new HashSet<Invoice_tbEntry>();
            TbInvoiceParentSubjectCodeNavigations = new HashSet<Invoice_tbInvoice>();
            TbInvoices = new HashSet<Invoice_tbInvoice>();
            TbCashMirror = new HashSet<Cash_tbMirror>();
            TbInvoiceMirror = new HashSet<Invoice_tbMirror>();
            TbMirrors = new HashSet<Object_tbMirror>();
            TbOptionSubjectCodeNavigations = new HashSet<App_tbOption>();
            TbOptionMinerAccountCodeNavigations = new HashSet<App_tbOption>();
            TbParentNamespaces = new HashSet<Subject_tbNamespace>();
            TbPaymentParentSubjectCodeNavigations = new HashSet<Cash_tbPayment>();
            TbPayments = new HashSet<Cash_tbPayment>();
            TbProjectParentSubjectCodeNavigations = new HashSet<Project_tbProject>();
            TbProjects = new HashSet<Project_tbProject>();
            TbSectors = new HashSet<Subject_tbSector>();
            TbTaxTypes = new HashSet<Cash_tbTaxType>();
        }

        [Key]
        [StringLength(50)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string SubjectName { get; set; }

        [Display(Name = "Type")]
        public short SubjectTypeCode { get; set; }

        [Display(Name = "Status")]
        public short SubjectStatusCode { get; set; }

        [Display(Name = "Transmit Code")]
        public short TransmitStatusCode { get; set; }

        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }

        [StringLength(15)]
        [Display(Name = "Address Code")]
        public string AddressCode { get; set; }

        [StringLength(100)]
        [Display(Name = "Payment Terms")]
        public string PaymentTerms { get; set; }

        [Display(Name = "Expected Days")]
        public short ExpectedDays { get; set; }

        [Display(Name = "Payment Days")]
        public short PaymentDays { get; set; }

        [Display(Name = "Days From M/E")]
        public bool PayDaysFromMonthEnd { get; set; }

        [Required]
        [Display(Name = "Pay Balance?")]
        public bool PayBalance { get; set; } = true;

        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Opening Balance")]
        [DataType(DataType.Currency)]
        public decimal OpeningBalance { get; set; }

        [StringLength(50)]
        [Display(Name = "Area")]
        public string AreaCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Display(Name = "Export Type Code")]
        public byte ExportTypeCode { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Inserted")]
        public DateTime InsertedOn { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Updated On")]
        public DateTime UpdatedOn { get; set; }

        [ForeignKey(nameof(AddressCode))]
        [InverseProperty(nameof(Subject_tbAddress.TbSubjects))]
        public virtual Subject_tbAddress AddressCodeNavigation { get; set; }

        [ForeignKey(nameof(ExportTypeCode))]
        [InverseProperty(nameof(Subject_tbExportType.TbSubjects))]
        public virtual Subject_tbExportType ExportTypeCodeNavigation { get; set; }

        [ForeignKey(nameof(SubjectStatusCode))]
        [InverseProperty(nameof(Subject_tbStatus.TbSubjects))]
        public virtual Subject_tbStatus SubjectStatusCodeNavigation { get; set; }

        [ForeignKey(nameof(SubjectTypeCode))]
        [InverseProperty(nameof(Subject_tbType.TbSubjects))]
        public virtual Subject_tbType SubjectTypeCodeNavigation { get; set; }

        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbSubjects))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }

        [ForeignKey(nameof(TransmitStatusCode))]
        [InverseProperty(nameof(Subject_tbTransmitStatus.TbSubjects))]
        public virtual Subject_tbTransmitStatus TransmitStatusCodeNavigation { get; set; }

        [InverseProperty(nameof(Subject_tbAccount.SubjectCodeNavigation))]
        public virtual ICollection<Subject_tbAccount> TbAccounts { get; set; }

        [InverseProperty(nameof(Subject_tbAddress.SubjectCodeNavigation))]
        public virtual ICollection<Subject_tbAddress> TbAddresses { get; set; }

        [InverseProperty(nameof(Project_tbAllocation.SubjectCodeNavigation))]
        public virtual ICollection<Project_tbAllocation> TbAllocations { get; set; }

        [InverseProperty(nameof(Subject_tbNamespace.ParentSubjectCodeNavigation))]
        public virtual ICollection<Subject_tbNamespace> TbChildNamespaces { get; set; }

        [InverseProperty(nameof(Subject_tbDoc.SubjectCodeNavigation))]
        public virtual ICollection<Subject_tbDoc> TbDocs { get; set; }

        [InverseProperty(nameof(Invoice_tbEntry.SubjectCodeNavigation))]
        public virtual ICollection<Invoice_tbEntry> TbEntries { get; set; }

        [InverseProperty(nameof(Invoice_tbInvoice.ParentSubjectCodeNavigation))]
        public virtual ICollection<Invoice_tbInvoice> TbInvoiceParentSubjectCodeNavigations { get; set; }

        [InverseProperty(nameof(Invoice_tbInvoice.SubjectCodeNavigation))]
        public virtual ICollection<Invoice_tbInvoice> TbInvoices { get; set; }

        [InverseProperty(nameof(Cash_tbMirror.SubjectCodeNavigation))]
        public virtual ICollection<Cash_tbMirror> TbCashMirror { get; set; }

        [InverseProperty(nameof(Invoice_tbMirror.SubjectCodeNavigation))]
        public virtual ICollection<Invoice_tbMirror> TbInvoiceMirror { get; set; }

        [InverseProperty(nameof(Object_tbMirror.SubjectCodeNavigation))]
        public virtual ICollection<Object_tbMirror> TbMirrors { get; set; }

        [InverseProperty(nameof(App_tbOption.SubjectCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptionSubjectCodeNavigations { get; set; }

        [InverseProperty(nameof(App_tbOption.MinerAccountCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptionMinerAccountCodeNavigations { get; set; }

        [InverseProperty(nameof(Subject_tbNamespace.ChildSubjectCodeNavigation))]
        public virtual ICollection<Subject_tbNamespace> TbParentNamespaces { get; set; }

        [InverseProperty(nameof(Cash_tbPayment.ParentSubjectCodeNavigation))]
        public virtual ICollection<Cash_tbPayment> TbPaymentParentSubjectCodeNavigations { get; set; }

        [InverseProperty(nameof(Cash_tbPayment.SubjectCodeNavigation))]
        public virtual ICollection<Cash_tbPayment> TbPayments { get; set; }

        [InverseProperty(nameof(Project_tbProject.ParentSubjectCodeNavigation))]
        public virtual ICollection<Project_tbProject> TbProjectParentSubjectCodeNavigations { get; set; }

        [InverseProperty(nameof(Project_tbProject.SubjectCodeNavigation))]
        public virtual ICollection<Project_tbProject> TbProjects { get; set; }

        [InverseProperty(nameof(Subject_tbSector.SubjectCodeNavigation))]
        public virtual ICollection<Subject_tbSector> TbSectors { get; set; }

        [InverseProperty(nameof(Cash_tbTaxType.SubjectCodeNavigation))]
        public virtual ICollection<Cash_tbTaxType> TbTaxTypes { get; set; }

        [InverseProperty(nameof(Subject_tbReal.SubjectCodeNavigation))]
        public virtual Subject_tbReal TbReal { get; set; }

        [InverseProperty(nameof(Subject_tbStructural.SubjectCodeNavigation))]
        public virtual Subject_tbStructural TbStructural { get; set; }

        [InverseProperty(nameof(Subject_tbVirtual.SubjectCodeNavigation))]
        public virtual Subject_tbVirtual TbVirtual { get; set; }
    }
}
