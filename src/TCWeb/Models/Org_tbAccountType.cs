using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAccountType", Schema = "Org")]
    public partial class Org_tbAccountType
    {
        public Org_tbAccountType()
        {
            TbAccounts = new HashSet<Org_tbAccount>();
        }

        [Key]
        public short AccountTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string AccountType { get; set; }

        [InverseProperty(nameof(Org_tbAccount.AccountTypeCodeNavigation))]
        public virtual ICollection<Org_tbAccount> TbAccounts { get; set; }
    }
}
