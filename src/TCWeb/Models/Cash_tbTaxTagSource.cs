using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTaxTagSource", Schema = "Cash")]
    public partial class Cash_tbTaxTagSource
    {
        public Cash_tbTaxTagSource()
        {
            TbTaxTags = new HashSet<Cash_tbTaxTag>();
        }

        [Key]
        [StringLength(20)]
        public string TaxSourceCode { get; set; }

        [Required]
        [StringLength(50)]
        public string SourceName { get; set; }

        [StringLength(255)]
        public string SourceDescription { get; set; }

        [Required]
        public short TaxTypeCode { get; set; }

        [InverseProperty(nameof(Cash_tbTaxTag.TaxSourceCodeNavigation))]
        public virtual ICollection<Cash_tbTaxTag> TbTaxTags { get; set; }

        [ForeignKey(nameof(TaxTypeCode))]
        [InverseProperty(nameof(Cash_tbTaxType.TaxTagSourceNavigation))]
        public virtual Cash_tbTaxType TaxTypeCodeNavigation { get; set; }

    }
}
