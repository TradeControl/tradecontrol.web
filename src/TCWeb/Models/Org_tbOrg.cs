using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbOrg", Schema = "Org")]
    [Index(nameof(AccountCode), Name = "IX_Org_tbOrg_OpeningBalance")]
    [Index(nameof(AccountCode), Name = "IX_tbOrg_tb_AccountCode")]
    public partial class Org_tbOrg
    {
        public Org_tbOrg()
        {
            TbAccounts = new HashSet<Org_tbAccount>();
            TbAddresses = new HashSet<Org_tbAddress>();
            TbAllocations = new HashSet<Task_tbAllocation>();
            TbContacts = new HashSet<Org_tbContact>();
            TbDocs = new HashSet<Org_tbDoc>();
            TbEntries = new HashSet<Invoice_tbEntry>();
            TbInvoices = new HashSet<Invoice_tbInvoice>();
            TbCashMirror = new HashSet<Cash_tbMirror>();
            TbInvoiceMirror = new HashSet<Invoice_tbMirror>();
            TbMirrors = new HashSet<Activity_tbMirror>();
            TbOptionAccountCodeNavigations = new HashSet<App_tbOption>();
            TbOptionMinerAccountCodeNavigations = new HashSet<App_tbOption>();
            TbPayments = new HashSet<Cash_tbPayment>();
            TbSectors = new HashSet<Org_tbSector>();
            TbTasks = new HashSet<Task_tbTask>();
            TbTaxTypes = new HashSet<Cash_tbTaxType>();
        }

        [Key]
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        [Display(Name = "Type")]
        public short OrganisationTypeCode { get; set; }
        [Display(Name = "Status")]
        public short OrganisationStatusCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [StringLength(15)]
        [Display(Name = "Address Code")]
        public string AddressCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Area Code")]
        public string AreaCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [StringLength(255)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        [Display(Name = "Web Site")]
        public string WebSite { get; set; }
        [StringLength(100)]
        [Display(Name = "Source")]
        public string AccountSource { get; set; }
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
        public decimal OpeningBalance { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Turnover")]
        public decimal Turnover { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(AddressCode))]
        [InverseProperty(nameof(Org_tbAddress.TbOrgs))]
        public virtual Org_tbAddress AddressCodeNavigation { get; set; }
        [ForeignKey(nameof(OrganisationStatusCode))]
        [InverseProperty(nameof(Org_tbStatus.TbOrgs))]
        public virtual Org_tbStatus OrganisationStatusCodeNavigation { get; set; }
        [ForeignKey(nameof(OrganisationTypeCode))]
        [InverseProperty(nameof(Org_tbType.TbOrgs))]
        public virtual Org_tbType OrganisationTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbOrgs))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
        [ForeignKey(nameof(TransmitStatusCode))]
        [InverseProperty(nameof(Org_tbTransmitStatus.TbOrgs))]
        public virtual Org_tbTransmitStatus TransmitStatusCodeNavigation { get; set; }
        [InverseProperty(nameof(Org_tbAccount.AccountCodeNavigation))]
        public virtual ICollection<Org_tbAccount> TbAccounts { get; set; }
        [InverseProperty(nameof(Org_tbAddress.AccountCodeNavigation))]
        public virtual ICollection<Org_tbAddress> TbAddresses { get; set; }
        [InverseProperty(nameof(Task_tbAllocation.AccountCodeNavigation))]
        public virtual ICollection<Task_tbAllocation> TbAllocations { get; set; }
        [InverseProperty(nameof(Org_tbContact.AccountCodeNavigation))]
        public virtual ICollection<Org_tbContact> TbContacts { get; set; }
        [InverseProperty(nameof(Org_tbDoc.AccountCodeNavigation))]
        public virtual ICollection<Org_tbDoc> TbDocs { get; set; }
        [InverseProperty(nameof(Invoice_tbEntry.AccountCodeNavigation))]
        public virtual ICollection<Invoice_tbEntry> TbEntries { get; set; }
        [InverseProperty(nameof(Invoice_tbInvoice.AccountCodeNavigation))]
        public virtual ICollection<Invoice_tbInvoice> TbInvoices { get; set; }
        [InverseProperty(nameof(Cash_tbMirror.AccountCodeNavigation))]
        public virtual ICollection<Cash_tbMirror> TbCashMirror { get; set; }
        [InverseProperty(nameof(Invoice_tbMirror.AccountCodeNavigation))]
        public virtual ICollection<Invoice_tbMirror> TbInvoiceMirror { get; set; }
        [InverseProperty(nameof(Activity_tbMirror.AccountCodeNavigation))]
        public virtual ICollection<Activity_tbMirror> TbMirrors { get; set; }
        [InverseProperty(nameof(App_tbOption.AccountCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptionAccountCodeNavigations { get; set; }
        [InverseProperty(nameof(App_tbOption.MinerAccountCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptionMinerAccountCodeNavigations { get; set; }
        [InverseProperty(nameof(Cash_tbPayment.AccountCodeNavigation))]
        public virtual ICollection<Cash_tbPayment> TbPayments { get; set; }
        [InverseProperty(nameof(Org_tbSector.AccountCodeNavigation))]
        public virtual ICollection<Org_tbSector> TbSectors { get; set; }
        [InverseProperty(nameof(Task_tbTask.AccountCodeNavigation))]
        public virtual ICollection<Task_tbTask> TbTasks { get; set; }
        [InverseProperty(nameof(Cash_tbTaxType.AccountCodeNavigation))]
        public virtual ICollection<Cash_tbTaxType> TbTaxTypes { get; set; }
    }
}
