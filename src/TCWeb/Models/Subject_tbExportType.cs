using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbExportType", Schema = "Subject")]
    public partial class Subject_tbExportType
    {
        public Subject_tbExportType()
        {
            TbSubjects = new HashSet<Subject_tbSubject>();
        }

        [Key]
        [Display(Name = "Export Type Code")]
        public byte ExportTypeCode { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Export Type")]
        public string ExportType { get; set; }

        [InverseProperty(nameof(Subject_tbSubject.ExportTypeCodeNavigation))]
        public virtual ICollection<Subject_tbSubject> TbSubjects { get; set; }
    }
}
