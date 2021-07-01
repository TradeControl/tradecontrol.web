using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Usr_vwDoc
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Address")]
        public string CompanyAddress { get; set; }
        [StringLength(50)]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string CompanyPhoneNumber { get; set; }
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string CompanyEmailAddress { get; set; }
        [StringLength(255)]
        [Display(Name = "Web Site")]
        public string CompanyWebsite { get; set; }
        [StringLength(20)]
        [Display(Name = "Company Number")]
        public string CompanyNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Vat Number")]
        public string VatNumber { get; set; }
        //[Column(TypeName = "image")]
        //public byte[] Logo { get; set; }
        [StringLength(255)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [StringLength(50)]
        [Display(Name = "Current Account")]
        public string CurrentAccountName { get; set; }
        [StringLength(306)]
        [Display(Name = "Bank Account")]
        public string BankAccount { get; set; }
        [StringLength(20)]
        [Display(Name = "Bank A/c No")]
        public string BankAccountNumber { get; set; }
        [StringLength(10)]
        [Display(Name = "Sort Code")]
        public string BankSortCode { get; set; }
    }

}
