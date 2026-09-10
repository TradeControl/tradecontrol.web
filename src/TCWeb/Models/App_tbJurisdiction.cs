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
            TbVirtuals = new HashSet<Subject_tbVirtual>();
            TbOptions = new HashSet<App_tbOption>();
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

        [ForeignKey(nameof(UocCode))]
        public virtual App_tbUoc UocCodeNavigation { get; set; }

        [InverseProperty(nameof(App_tbOption.JurisdictionCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptions { get; set; }

        [InverseProperty(nameof(Subject_tbVirtual.RegistryJurisdictionCodeNavigation))]
        public virtual ICollection<Subject_tbVirtual> TbVirtuals { get; set; }
    }
}
