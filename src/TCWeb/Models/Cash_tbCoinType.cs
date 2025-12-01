using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbCoinType", Schema = "Cash")]
    public partial class Cash_tbCoinType
    {
        public Cash_tbCoinType()
        {
            TbAccounts = new HashSet<Subject_tbAccount>();
            TbOptions = new HashSet<App_tbOption>();
        }

        [Key]
        public short CoinTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string CoinType { get; set; }

        [InverseProperty(nameof(Subject_tbAccount.CoinTypeCodeNavigation))]
        public virtual ICollection<Subject_tbAccount> TbAccounts { get; set; }
        [InverseProperty(nameof(App_tbOption.CoinTypeCodeNavigation))]
        public virtual ICollection<App_tbOption> TbOptions { get; set; }
    }
}
