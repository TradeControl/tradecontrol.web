using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Web_vwTemplateImage
    {
        public int TemplateId { get; set; }
        [StringLength(256)]
        [Display(Name = "Template Name")]
        public string TemplateFileName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Image Tag")]
        public string ImageTag { get; set; }
        [Required]
        [StringLength(256)]
        [Display(Name = "Image")]
        public string ImageFileName { get; set; }
    }
}
