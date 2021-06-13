using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbType", Schema = "Org")]
    public partial class Org_tbType
    {
        public Org_tbType()
        {
            TbOrgs = new HashSet<Org_tbOrg>();
        }

        [Key]
        [Display(Name = "Org Type Code")]
        public short OrganisationTypeCode { get; set; }
        [Display(Name = "Mode")]
        public short CashModeCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string OrganisationType { get; set; }

        [ForeignKey(nameof(CashModeCode))]
        [InverseProperty(nameof(Cash_tbMode.TbOrgType))]
        public virtual Cash_tbMode CashModeCodeNavigation { get; set; }
        [InverseProperty(nameof(Org_tbOrg.OrganisationTypeCodeNavigation))]
        public virtual ICollection<Org_tbOrg> TbOrgs { get; set; }
    }
}
