using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbBalanceConstraint", Schema = "Subject")]
    public partial class Subject_tbBalanceConstraint
    {
        public Subject_tbBalanceConstraint()
        {
            TbAccounts = new HashSet<Subject_tbAccount>();
        }

        [Key]
        public byte BalanceConstraintCode { get; set; }

        [Required]
        [StringLength(50)]
        public string BalanceConstraint { get; set; }

        [InverseProperty(nameof(Subject_tbAccount.BalanceConstraintCodeNavigation))]
        public virtual ICollection<Subject_tbAccount> TbAccounts { get; set; }
    }
}
