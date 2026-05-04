using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbStructural", Schema = "Subject")]
    public partial class Subject_tbStructural
    {
        [Key]
        [StringLength(50)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbStructural))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
    }
}
