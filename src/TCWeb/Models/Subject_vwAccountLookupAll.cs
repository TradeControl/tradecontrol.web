using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("vwSubjectLookupAll", Schema = "Subject")]
    public partial class Subject_vwSubjectLookupAll
    {
        [Key]
        [Required]
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Name")]
        public string SubjectName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string SubjectType { get; set; }
        public short SubjectTypeCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string SubjectStatus { get; set; }
        public short SubjectStatusCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Cash Mode")]
        public string CashPolarity { get; set; }
        public short CashPolarityCode { get; set; }
    }
}
