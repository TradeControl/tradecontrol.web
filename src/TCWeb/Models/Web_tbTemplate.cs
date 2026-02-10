using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTemplate", Schema = "Web")]
    [Index(nameof(TemplateFileName), Name = "IX_Web_tbTemplate_TemplateFileName", IsUnique = true)]
    public partial class Web_tbTemplate
    {
        public Web_tbTemplate()
        {
            tbTemplateImages = new HashSet<Web_tbTemplateImage>();
            tbTemplateInvoices = new HashSet<Web_tbTemplateInvoice>();
        }

        [Key]
        public int TemplateId { get; set; }

        [StringLength(256)]
        public string TemplateFileName { get; set; }

        public short TemplateStatusCode { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ParsedOn { get; set; }

        [StringLength(512)]
        public string ParseMessage { get; set; }

        [ForeignKey(nameof(TemplateStatusCode))]
        [InverseProperty(nameof(Web_tbTemplateStatus.tbTemplates))]
        public virtual Web_tbTemplateStatus TemplateStatusCodeNavigation { get; set; }

        [InverseProperty(nameof(Web_tbTemplateImage.Template))]
        public virtual ICollection<Web_tbTemplateImage> tbTemplateImages { get; set; }

        [InverseProperty(nameof(Web_tbTemplateInvoice.Template))]
        public virtual ICollection<Web_tbTemplateInvoice> tbTemplateInvoices { get; set; }
    }
}
