using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTaxTagMapType", Schema = "Cash")]
    public partial class Cash_tbTaxTagMapType
    {
        public Cash_tbTaxTagMapType()
        {
            TbTaxTagMaps = new HashSet<Cash_tbTaxTagMap>();
        }

        [Key]
        public NodeEnum.MapTypeCode MapTypeCode { get; set; }

        [Required]
        [StringLength(20)]
        public string MapType { get; set; }

        [InverseProperty(nameof(Cash_tbTaxTagMap.MapTypeCodeNavigation))]
        public virtual ICollection<Cash_tbTaxTagMap> TbTaxTagMaps { get; set; }
    }
}
