using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbJurisdiction", Schema = "App")]
    public partial class App_tbJurisdiction
    {
        public App_tbJurisdiction()
        {
            TbTaxTagSources = new HashSet<Cash_tbTaxTagSource>();
        }

        [Key]
        [StringLength(10)]
        public string JurisdictionCode { get; set; }

        [Required]
        [StringLength(50)]
        public string JurisdictionName { get; set; }

        [Required]
        [StringLength(5)]
        public string UocCode { get; set; }

        public bool IsEnabled { get; set; }

        [ForeignKey(nameof(UocCode))]
        public virtual App_tbUoc UocCodeNavigation { get; set; }

        [InverseProperty(nameof(Cash_tbTaxTagSource.JurisdictionCodeNavigation))]
        public virtual ICollection<Cash_tbTaxTagSource> TbTaxTagSources { get; set; }
    }
}
