using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbContact", Schema = "Subject")]
    public partial class Subject_tbContact
    {
        [Key]
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }
        [Key]
        [StringLength(100)]
        [Display (Name = "Contact Name")]
        public string ContactName { get; set; }
        [StringLength(100)]
        [Display(Name = "File As")]
        public string FileAs { get; set; }
        [Required]
        [Display(Name = "On Mail List?")]
        public bool OnMailingList { get; set; }
        [StringLength(25)]
        [Display(Name = "Title")]
        public string NameTitle { get; set; }
        [StringLength(100)]
        [Display(Name = "Nick Name")]
        public string NickName { get; set; }
        [StringLength(100)]
        [Display(Name = "Role")]
        public string JobTitle { get; set; }
        [StringLength(50)]
        [Display(Name = "Direct Line")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }
        [StringLength(50)]
        [Display(Name = "Hobby")]
        public string Hobby { get; set; }
        //[Column(TypeName = "datetime")]
        //public DateTime DateOfBirth { get; set; }
        [StringLength(50)]
        [Display(Name = "Department")]
        public string Department { get; set; }
        [StringLength(50)]
        [Display(Name = "Spouse")]
        public string SpouseName { get; set; }
        [StringLength(50)]
        [Display(Name = "Home Phone No.")]
        public string HomeNumber { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Info")]
        public string Information { get; set; }
        [Column(TypeName = "image")]
        public byte[] Photo { get; set; }
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


        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbContacts))]
        public virtual Subject_tbSubject AccountCodeNavigation { get; set; }
    }
}
