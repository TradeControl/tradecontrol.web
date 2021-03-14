using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbContact", Schema = "Org")]
    public partial class Org_tbContact
    {
        [Key]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Key]
        [StringLength(100)]
        public string ContactName { get; set; }
        [StringLength(100)]
        public string FileAs { get; set; }
        [Required]
        public bool OnMailingList { get; set; }
        [StringLength(25)]
        public string NameTitle { get; set; }
        [StringLength(100)]
        public string NickName { get; set; }
        [StringLength(100)]
        public string JobTitle { get; set; }
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        public string MobileNumber { get; set; }
        [StringLength(255)]
        public string EmailAddress { get; set; }
        [StringLength(50)]
        public string Hobby { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateOfBirth { get; set; }
        [StringLength(50)]
        public string Department { get; set; }
        [StringLength(50)]
        public string SpouseName { get; set; }
        [StringLength(50)]
        public string HomeNumber { get; set; }
        [Column(TypeName = "ntext")]
        public string Information { get; set; }
        [Column(TypeName = "image")]
        public byte[] Photo { get; set; }
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
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Org_tbOrg.TbContacts))]
        public virtual Org_tbOrg AccountCodeNavigation { get; set; }
    }
}
