using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbClass", Schema = "Subject")]
    public partial class Subject_tbClass
    {
        public Subject_tbClass()
        {
            TbTypes = new HashSet<Subject_tbType>();
        }

        [Key]
        [Display(Name = "Subject Class Code")]
        public short SubjectClassCode { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Subject Class")]
        public string SubjectClass { get; set; }

        [InverseProperty(nameof(Subject_tbType.SubjectClassCodeNavigation))]
        public virtual ICollection<Subject_tbType> TbTypes { get; set; }
    }
}
