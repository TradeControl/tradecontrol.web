using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwReal
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Contact Name")]
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
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "Hobby")]
        public string Hobby { get; set; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Date Of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(50)]
        [Display(Name = "Department")]
        public string Department { get; set; }

        [StringLength(50)]
        [Display(Name = "Spouse")]
        public string SpouseName { get; set; }

        [StringLength(50)]
        [Display(Name = "Home Phone No.")]
        public string HomeNumber { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "Info")]
        public string Information { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] Photo { get; set; }

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

        //public byte[] RowVer { get; set; }

        [Display(Name = "Namespace")]
        public string SubjectNamespace { get; set; }
    }
}
