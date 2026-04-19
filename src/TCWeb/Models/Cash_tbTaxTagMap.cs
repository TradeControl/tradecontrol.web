using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTaxTagMap", Schema = "Cash")]
    public partial class Cash_tbTaxTagMap
    {
        [Key]
        [StringLength(20)]
        public string TaxSourceCode { get; set; }

        [Key]
        [StringLength(64)]
        public string TagCode { get; set; }

        [Key]
        public NodeEnum.MapTypeCode MapTypeCode { get; set; }

        [Key]
        [StringLength(10)]
        public string CategoryCode { get; set; }

        [Key]
        [StringLength(50)]
        public string CashCode { get; set; }

        public bool IsEnabled { get; set; }

        [ForeignKey(nameof(MapTypeCode))]
        [InverseProperty(nameof(Cash_tbTaxTagMapType.TbTaxTagMaps))]
        public virtual Cash_tbTaxTagMapType MapTypeCodeNavigation { get; set; }

        public virtual Cash_tbTaxTag TagCodeNavigation { get; set; }
    }
}
