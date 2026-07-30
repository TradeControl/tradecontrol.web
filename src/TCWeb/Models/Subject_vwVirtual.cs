using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwVirtual
    {
        [Required]
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
        public bool PayBalance { get; set; }

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

        [Display(Name = "Jurisdiction")]
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

        [Display(Name = "Employees")]
        public int NumberOfEmployees { get; set; }

        [StringLength(20)]
        [Display(Name = "Company Number")]
        public string CompanyNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Vat Number")]
        public string VatNumber { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "Description")]
        public string BusinessDescription { get; set; }

        [Column(TypeName = "varbinary(max)")]
        [Display(Name = "Logo")]
        public byte[] Logo { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Turnover")]
        public decimal Turnover { get; set; }

        [StringLength(255)]
        [Display(Name = "Web Site")]
        public string WebSite { get; set; }

        [StringLength(100)]
        [Display(Name = "Source")]
        public string SubjectSource { get; set; }

        [Display(Name = "Namespace")]
        public string SubjectNamespace { get; set; }
    }
}
