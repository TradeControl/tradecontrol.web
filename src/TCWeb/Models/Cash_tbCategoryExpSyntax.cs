using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeControl.Web.Models
{
    [Table("tbCategoryExpSyntax", Schema = "Cash")]
    public partial class Cash_tbCategoryExpSyntax
    {
        [Key]
        public short SyntaxTypeCode { get; set; }

        [Required]
        [StringLength(100)]
        public string SyntaxType { get; set; } = string.Empty;

        [InverseProperty(nameof(Cash_tbCategoryExp.SyntaxTypeCodeNavigation))]
        public virtual ICollection<Cash_tbCategoryExp> TbCategoryExps { get; set; } = new List<Cash_tbCategoryExp>();
    }
}
