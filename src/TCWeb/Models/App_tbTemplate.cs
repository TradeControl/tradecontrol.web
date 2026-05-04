using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTemplate", Schema = "App")]
    [Index(nameof(TemplateName), Name = "IX_App_tbTemplate_TemplateName", IsUnique = true)]
    public partial class App_tbTemplate
    {
        public App_tbTemplate()
        {
            App_tbTemplateDatasets = new HashSet<App_tbTemplateDataset>();
        }

        [Key]
        [StringLength(10)]
        public string TemplateCode { get; set; }

        [Required]
        [StringLength(100)]
        public string TemplateName { get; set; }

        [Required]
        [StringLength(100)]
        public string StoredProcedure { get; set; }

        public string TemplateDescription { get; set; }

        public bool IsVatRegistered { get; set; }

        [InverseProperty(nameof(App_tbTemplateDataset.TemplateCodeNavigation))]
        public virtual ICollection<App_tbTemplateDataset> App_tbTemplateDatasets { get; set; }
    }
}
