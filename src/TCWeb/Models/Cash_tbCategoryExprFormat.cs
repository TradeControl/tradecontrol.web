using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeControl.Web.Models
{
    [Table("tbCategoryExprFormat", Schema = "Cash")]
    public partial class Cash_tbCategoryExprFormat
    {
        [Key]
        [Required]
        [StringLength(10)]
        [Display(Name = "Template Code")]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Template")]
        public string Template { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "TemplateDescription")]
        public string TemplateDescription { get; set; } = string.Empty;

    }
}
