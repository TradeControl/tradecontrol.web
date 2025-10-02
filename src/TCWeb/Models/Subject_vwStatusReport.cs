using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwStatusReport
    {
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [Required]
        [StringLength(50)]
        public string SubjectType { get; set; }
        [StringLength(255)]
        public string SubjectStatus { get; set; }
        [StringLength(50)]
        public string TaxDescription { get; set; }
        [Column(TypeName = "ntext")]
        public string Address { get; set; }
        [StringLength(50)]
        public string AreaCode { get; set; }
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        [StringLength(255)]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        public string WebSite { get; set; }
        [StringLength(50)]
        public string IndustrySector { get; set; }
        [StringLength(100)]
        public string SubjectSource { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        public short PaymentDays { get; set; }
        public short ExpectedDays { get; set; }
        public int NumberOfEmployees { get; set; }
        [StringLength(20)]
        public string CompanyNumber { get; set; }
        [StringLength(50)]
        public string VatNumber { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal Turnover { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal OpeningBalance { get; set; }
        [Column("EUJurisdiction")]
        public bool Eujurisdiction { get; set; }
        [Column(TypeName = "ntext")]
        public string BusinessDescription { get; set; }
        [Required]
        [StringLength(20)]
        public string PaymentCode { get; set; }
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        [StringLength(50)]
        public string PaymentTaxDescription { get; set; }
        [Required]
        [StringLength(50)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime PaidOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidInValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidOutValue { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [StringLength(50)]
        public string PaymentReference { get; set; }
    }
}
