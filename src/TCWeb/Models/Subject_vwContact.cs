using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwContact
    {
        [Required]
        [StringLength(100)]
        [Display (Name = "Contact")]
        public string ContactName { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "A/C")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Subject")]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string SubjectType { get; set; }
        [StringLength(255)]
        [Display(Name = "Status")]
        public string SubjectStatus { get; set; }
        [Display(Name = "Live Tasks")]
        public int Tasks { get; set; }
        [StringLength(50)]
        [Display(Name = "Direct Line")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Home Tel")]
        [DataType(DataType.PhoneNumber)]
        public string HomeNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Mobile")]
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }
        [StringLength(255)]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [StringLength(25)]
        [Display(Name = "Title")]
        public string NameTitle { get; set; }
        [StringLength(100)]
        [Display(Name = "Nickname")]
        public string NickName { get; set; }
        [StringLength(100)]
        [Display(Name = "Job")]
        public string JobTitle { get; set; }
        [StringLength(50)]
        [Display(Name = "Dept.")]
        public string Department { get; set; }
        [Column(TypeName = "ntext")]
        [Display(Name = "Info")]
        public string Information { get; set; }
        [StringLength(50)]
        [Display(Name = "Insert By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Insert On")]
        public DateTime InsertedOn { get; set; }
    }
}
