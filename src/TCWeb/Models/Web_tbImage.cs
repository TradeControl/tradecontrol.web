using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbImage", Schema = "Web")]
    [Index(nameof(ImageFileName), Name = "IX_Web_tbImage_ImageFileName", IsUnique = true)]
    public partial class Web_tbImage
    {
        public Web_tbImage()
        {
            tbTemplateImages = new HashSet<Web_tbTemplateImage>();
        }

        [Key]
        [StringLength(50)]
        [Display(Name = "Tag")]
        public string ImageTag { get; set; }
        [Required]
        [StringLength(256)]
        [Display(Name ="File Name")]
        public string ImageFileName { get; set; }

        [InverseProperty(nameof(Web_tbTemplateImage.ImageTagNavigation))]
        public virtual ICollection<Web_tbTemplateImage> tbTemplateImages { get; set; }
    }
}
