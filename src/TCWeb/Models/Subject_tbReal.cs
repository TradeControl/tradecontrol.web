using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbReal", Schema = "Subject")]
    [Index(nameof(Department), Name = "IX_Subject_tbRealDepartment")]
    [Index(nameof(JobTitle), Name = "IX_Subject_tbRealJobTitle")]
    [Index(nameof(NameTitle), Name = "IX_Subject_tbRealNameTitle")]
    [Index(nameof(SubjectCode), Name = "IX_Subject_tbReal_AccountCode")]
    public partial class Subject_tbReal
    {
        [Key]
        [StringLength(50)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }

        [StringLength(100)]
        [Display(Name = "File As")]
        public string FileAs { get; set; }

        [Required]
        [Display(Name = "On Mail List?")]
        public bool OnMailingList { get; set; } = false;

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

        //public byte[] RowVer { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbReal))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
    }
}
