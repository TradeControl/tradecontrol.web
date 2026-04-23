using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTaxTag", Schema = "Cash")]
    public partial class Cash_tbTaxTag
    {
        public Cash_tbTaxTag()
        {
            TbTaxTagMaps = new HashSet<Cash_tbTaxTagMap>();
        }

        [Key]
        [StringLength(20)]
        public string TaxSourceCode { get; set; }

        [Key]
        [StringLength(64)]
        public string TagCode { get; set; }

        [Required]
        [StringLength(100)]
        public string TagName { get; set; }

        public NodeEnum.TagClassCode TagClassCode { get; set; }

        [StringLength(4000)]
        public string TagDescription { get; set; }

        public short DisplayOrder { get; set; }

        [ForeignKey(nameof(TaxSourceCode))]
        [InverseProperty(nameof(Cash_tbTaxTagSource.TbTaxTags))]
        public virtual Cash_tbTaxTagSource TaxSourceCodeNavigation { get; set; }

        [ForeignKey(nameof(TagClassCode))]
        [InverseProperty(nameof(Cash_tbTaxTagClass.TbTaxTags))]
        public virtual Cash_tbTaxTagClass TagClassCodeNavigation { get; set; }

        [InverseProperty(nameof(Cash_tbTaxTagMap.TagCodeNavigation))]
        public virtual ICollection<Cash_tbTaxTagMap> TbTaxTagMaps { get; set; }
    }
}
