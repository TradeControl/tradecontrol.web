using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbStatus", Schema = "Subject")]
    public partial class Subject_tbStatus
    {
        public Subject_tbStatus()
        {
            TbSubjects = new HashSet<Subject_tbSubject>();
        }

        [Key]
        public short SubjectStatusCode { get; set; }
        [StringLength(255)]
        public string SubjectStatus { get; set; }

        [InverseProperty(nameof(Subject_tbSubject.SubjectStatusCodeNavigation))]
        public virtual ICollection<Subject_tbSubject> TbSubjects { get; set; }
    }
}
