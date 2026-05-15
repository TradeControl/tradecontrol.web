using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_vwTypeLookup
    {
        [Display(Name = "Subject Type Code")]
        public short SubjectTypeCode { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string SubjectType { get; set; }

        [Display(Name = "Polarity Code")]
        public short CashPolarityCode { get; set; }

        [StringLength(10)]
        [Display(Name = "Polarity")]
        public string CashPolarity { get; set; }

        [Display(Name = "Subject Class Code")]
        public short SubjectClassCode { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Class")]
        public string SubjectClass { get; set; }
    }
}
