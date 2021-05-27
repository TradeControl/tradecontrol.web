using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwIdentity
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }
        [Required]
        [Column(TypeName = "ntext")]
        [Display(Name = "Address")]
        public string Address { get; set; }
        [StringLength(50)]
        [Display(Name = "Direct Line")]
        public string PhoneNumber { get; set; }
        [StringLength(255)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [StringLength(255)]
        [Display(Name = "Website")]
        public string WebSite { get; set; }
        [Column(TypeName = "image")]
        public byte[] Logo { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Logon Name")]
        public string LogonName { get; set; }
        [Column(TypeName = "image")]
        [Display(Name = "Avatar")]
        public byte[] Avatar { get; set; }
        [StringLength(20)]
        [Display(Name = "Company No.")]
        public string CompanyNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Vat No.")]
        public string VatNumber { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Unit of Account")]
        public string UocName { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "UOC Symbol")]
        public string UocSymbol { get; set; }
    }
}
