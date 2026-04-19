using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTaxTagClass", Schema = "Cash")]
    public partial class Cash_tbTaxTagClass
    {
        public Cash_tbTaxTagClass()
        {
            TbTaxTags = new HashSet<Cash_tbTaxTag>();
        }

        [Key]
        public NodeEnum.TagClassCode TagClassCode { get; set; }

        [Required]
        [StringLength(20)]
        public string TagClass { get; set; }

        [InverseProperty(nameof(Cash_tbTaxTag.TagClassCodeNavigation))]
        public virtual ICollection<Cash_tbTaxTag> TbTaxTags { get; set; }
    }
}
