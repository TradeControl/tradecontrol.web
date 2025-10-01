using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAccountType", Schema = "Subject")]
    public partial class Subject_tbAccountType
    {
        public Subject_tbAccountType()
        {
            TbAccounts = new HashSet<Subject_tbAccount>();
        }

        [Key]
        public short AccountTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string AccountType { get; set; }

        [InverseProperty(nameof(Subject_tbAccount.AccountTypeCodeNavigation))]
        public virtual ICollection<Subject_tbAccount> TbAccounts { get; set; }
    }
}
