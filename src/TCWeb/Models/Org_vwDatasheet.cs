﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("vwDatasheet", Schema = "Org")]
    public partial class Org_vwDatasheet
    {
        [Key]
        [StringLength(10)]
        [Display (Name = "Account Code")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        [Display(Name = "Tasks")]
        public int Tasks { get; set; }
        [Display(Name = "Org Type Code")]
        public short OrganisationTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string OrganisationType { get; set; }
        [Display(Name = "Polarity Code")]
        public short CashModeCode { get; set; }
        [Display(Name = "Status Code")]
        public short OrganisationStatusCode { get; set; }
        [StringLength(255)]
        [Display(Name = "Status")]
        public string OrganisationStatus { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Transmit Status")]
        public string TransmitStatus { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Address")]
        public string Address { get; set; }
        [StringLength(50)]
        [Display(Name = "Tax Rate")]
        public string TaxDescription { get; set; }
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
        [Display(Name = "Phone No.")]
        public string PhoneNumber { get; set; }
        [StringLength(255)]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        [Display(Name = "Website")]
        public string WebSite { get; set; }
        [StringLength(50)]
        [Display(Name = "Sector")]
        public string IndustrySector { get; set; }
        [StringLength(100)]
        [Display(Name = "Source")]
        public string AccountSource { get; set; }
        [StringLength(100)]
        [Display(Name = "Payment Terms")]
        public string PaymentTerms { get; set; }
        [Display(Name = "Payment Days")]
        public short PaymentDays { get; set; }
        [Display(Name = "Expected Days")]
        public short ExpectedDays { get; set; }
        [Display(Name = "Pay at M/E?")]
        public bool PayDaysFromMonthEnd { get; set; }
        [Display(Name = "Pay Balance?")]
        public bool PayBalance { get; set; }
        [Display(Name = "Employees")]
        public int NumberOfEmployees { get; set; }
        [StringLength(20)]
        [Display(Name = "Company No.")]
        public string CompanyNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Vat No.")]
        public string VatNumber { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "T/O")]
        public decimal Turnover { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Opening Balance")]
        public decimal OpeningBalance { get; set; }
        [Column("EUJurisdiction")]
        [Display(Name = "EU?")]
        public bool Eujurisdiction { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Description")]
        public string BusinessDescription { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Insert By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Insert On")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Update By")]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Update On")]
        public DateTime UpdatedOn { get; set; }
    }
}
