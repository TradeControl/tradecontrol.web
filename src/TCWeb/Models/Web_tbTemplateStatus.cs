using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTemplateStatus", Schema = "Web")]
    public partial class Web_tbTemplateStatus
    {
        public Web_tbTemplateStatus()
        {
            tbTemplates = new HashSet<Web_tbTemplate>();
        }

        [Key]
        public short TemplateStatusCode { get; set; }

        [Required]
        [StringLength(50)]
        public string TemplateStatus { get; set; }

        [InverseProperty(nameof(Web_tbTemplate.TemplateStatusCodeNavigation))]
        public virtual ICollection<Web_tbTemplate> tbTemplates { get; set; }
    }
}
