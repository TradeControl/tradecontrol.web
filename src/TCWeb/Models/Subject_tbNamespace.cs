using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbNamespace", Schema = "Subject")]
    [PrimaryKey(nameof(ParentSubjectCode), nameof(ChildSubjectCode))]
    [Index(nameof(ChildSubjectCode), Name = "IX_Subject_tbNamespace_Child")]
    [Index(nameof(ParentSubjectCode), Name = "IX_Subject_tbNamespace_Parent")]
    [Index(nameof(ParentSubjectCode), nameof(IsDefault), Name = "IX_Subject_tbNamespace_Parent_IsDefault")]
    public partial class Subject_tbNamespace
    {
        [StringLength(50)]
        [Display(Name = "Parent Subject Code")]
        public string ParentSubjectCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Child Subject Code")]
        public string ChildSubjectCode { get; set; }

        [Display(Name = "Ordinal")]
        public int Ordinal { get; set; }

        [Required]
        [Display(Name = "Default")]
        public bool IsDefault { get; set; }

        [ForeignKey(nameof(ChildSubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbParentNamespaces))]
        public virtual Subject_tbSubject ChildSubjectCodeNavigation { get; set; }

        [ForeignKey(nameof(ParentSubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbChildNamespaces))]
        public virtual Subject_tbSubject ParentSubjectCodeNavigation { get; set; }
    }
}
