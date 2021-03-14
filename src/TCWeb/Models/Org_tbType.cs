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
        public short OrganisationTypeCode { get; set; }
        public short CashModeCode { get; set; }
        [Required]
        [StringLength(50)]
        public string OrganisationType { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(CashModeCode))]
        [InverseProperty(nameof(Cash_tbMode.TbOrgType))]
        public virtual Cash_tbMode CashModeCodeNavigation { get; set; }
        [InverseProperty(nameof(Org_tbOrg.OrganisationTypeCodeNavigation))]
        public virtual ICollection<Org_tbOrg> TbOrgs { get; set; }
    }
}
