using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTemplate", Schema = "App")]
    public partial class App_tbTemplate
    {
        [Key]
        [StringLength(100)]
        public string TemplateName { get; set; }

        [Required]
        [StringLength(100)]
        public string StoredProcedure { get; set; }

        public string TemplateDescription { get; set; }
    }
}
