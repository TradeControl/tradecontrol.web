using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTemplateImage", Schema = "Web")]
    public partial class Web_tbTemplateImage
    {
        [Key]
        public int TemplateId { get; set; }
        [Key]
        [StringLength(50)]
        public string ImageTag { get; set; }

        [ForeignKey(nameof(ImageTag))]
        [InverseProperty(nameof(Web_tbImage.tbTemplateImages))]
        public virtual Web_tbImage ImageTagNavigation { get; set; }
        [ForeignKey(nameof(TemplateId))]
        [InverseProperty(nameof(Web_tbTemplate.tbTemplateImages))]
        public virtual Web_tbTemplate Template { get; set; }
    }
}
