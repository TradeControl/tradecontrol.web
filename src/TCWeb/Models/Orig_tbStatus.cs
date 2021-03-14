using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbStatus", Schema = "Org")]
    public partial class Orig_tbStatus
    {
        public Orig_tbStatus()
        {
            TbOrgs = new HashSet<Org_tbOrg>();
        }

        [Key]
        public short OrganisationStatusCode { get; set; }
        [StringLength(255)]
        public string OrganisationStatus { get; set; }

        [InverseProperty(nameof(Org_tbOrg.OrganisationStatusCodeNavigation))]
        public virtual ICollection<Org_tbOrg> TbOrgs { get; set; }
    }
}
