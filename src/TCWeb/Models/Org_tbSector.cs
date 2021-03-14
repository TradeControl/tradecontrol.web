using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbSector", Schema = "Org")]
    public partial class Org_tbSector
    {
        [Key]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Key]
        [StringLength(50)]
        public string IndustrySector { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Org_tbOrg.TbSectors))]
        public virtual Org_tbOrg AccountCodeNavigation { get; set; }
    }
}
