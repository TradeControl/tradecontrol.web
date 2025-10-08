using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbSubject", Schema = "Subject")]
    [Index(nameof(SubjectCode), Name = "IX_Subject_tbSubject_OpeningBalance")]
    [Index(nameof(SubjectCode), Name = "IX_tbSubject_tb_SubjectCode")]
    public partial class Subject_tbSubject
    {
        public Subject_tbSubject()
        {
            TbAccounts = new HashSet<Subject_tbAccount>();
            TbAddresses = new HashSet<Subject_tbAddress>();
            TbAllocations = new HashSet<Project_tbAllocation>();
            TbContacts = new HashSet<Subject_tbContact>();
            TbDocs = new HashSet<Subject_tbDoc>();
            TbEntries = new HashSet<Invoice_tbEntry>();
            TbInvoices = new HashSet<Invoice_tbInvoice>();
            TbCashMirror = new HashSet<Cash_tbMirror>();
            TbInvoiceMirror = new HashSet<Invoice_tbMirror>();
            TbMirrors = new HashSet<Object_tbMirror>();
            TbOptionSubjectCodeNavigations = new HashSet<App_tbOption>();
            TbOptionMinerAccountCodeNavigations = new HashSet<App_tbOption>();
            TbPayments = new HashSet<Cash_tbPayment>();
            TbSectors = new HashSet<Subject_tbSector>();
            TbProjects = new HashSet<Project_tbProject>();
            TbTaxTypes = new HashSet<Cash_tbTaxType>();
        }

        [Key]
        [StringLength(10)]
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
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [StringLength(15)]
        [Display(Name = "Address Code")]
        public string AddressCode { get; set; }
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
        [StringLength(255)]
        [Display(Name = "Web Site")]
        //[DataType(DataType.Url)] //office url format incompatible
        public string WebSite { get; set; }
        [StringLength(100)]
        [Display(Name = "Source")]
        public string SubjectSource { get; set; }
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
        public bool PayBalance { get; set; }
        [Display(Name = "Employees")]
        public int NumberOfEmployees { get; set; }
        [StringLength(20)]
        [Display(Name = "Company Number")]
        public string CompanyNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Vat Number")]
        public string VatNumber { get; set; }
        [Column("EUJurisdiction")]
        [Display(Name = "EU?")]
        public bool Eujurisdiction { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Description")]
        public string BusinessDescription { get; set; }
        [Column(TypeName = "image")]
        [Display(Name = "Logo")]
        public byte[] Logo { get; set; }
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
        public DateTime UpdatedOn { get; set; }
        [Display(Name = "Transmit Code")]
        public short TransmitStatusCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Opening Balance")]
        [DataType(DataType.Currency)]
        public decimal OpeningBalance { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Turnover")]
        public decimal Turnover { get; set; }
        //[Required]
        //public byte[] RowVer { get; set; }

        [ForeignKey(nameof(AddressCode))]
        [InverseProperty(nameof(Subject_tbAddress.TbSubjects))]
        public virtual Subject_tbAddress AddressCodeNavigation { get; set; }
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
        [InverseProperty(nameof(Subject_tbContact.SubjectCodeNavigation))]
        public virtual ICollection<Subject_tbContact> TbContacts { get; set; }
        [InverseProperty(nameof(Subject_tbDoc.SubjectCodeNavigation))]
        public virtual ICollection<Subject_tbDoc> TbDocs { get; set; }
        [InverseProperty(nameof(Invoice_tbEntry.SubjectCodeNavigation))]
        public virtual ICollection<Invoice_tbEntry> TbEntries { get; set; }
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
        [InverseProperty(nameof(Cash_tbPayment.SubjectCodeNavigation))]
        public virtual ICollection<Cash_tbPayment> TbPayments { get; set; }
        [InverseProperty(nameof(Subject_tbSector.SubjectCodeNavigation))]
        public virtual ICollection<Subject_tbSector> TbSectors { get; set; }
        [InverseProperty(nameof(Project_tbProject.SubjectCodeNavigation))]
        public virtual ICollection<Project_tbProject> TbProjects { get; set; }
        [InverseProperty(nameof(Cash_tbTaxType.SubjectCodeNavigation))]
        public virtual ICollection<Cash_tbTaxType> TbTaxTypes { get; set; }
    }
}
