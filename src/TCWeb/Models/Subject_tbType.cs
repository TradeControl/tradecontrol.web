using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbType", Schema = "Subject")]
    public partial class Subject_tbType
    {
        public Subject_tbType()
        {
            TbSubjects = new HashSet<Subject_tbSubject>();
        }

        [Key]
        [Display(Name = "Subject Type Code")]
        public short SubjectTypeCode { get; set; }
        [Display(Name = "Polarity")]
        public short CashPolarityCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string SubjectType { get; set; }

        [ForeignKey(nameof(CashPolarityCode))]
        [InverseProperty(nameof(Cash_tbPolarity.TbSubjectType))]
        public virtual Cash_tbPolarity CashPolarityCodeNavigation { get; set; }
        [InverseProperty(nameof(Subject_tbSubject.SubjectTypeCodeNavigation))]
        public virtual ICollection<Subject_tbSubject> TbSubjects { get; set; }
    }
}
