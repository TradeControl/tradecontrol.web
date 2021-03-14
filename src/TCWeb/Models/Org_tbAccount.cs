using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAccount", Schema = "Org")]
    [Index(nameof(AccountTypeCode), nameof(LiquidityLevel), nameof(CashAccountCode), Name = "IX_tbAccount_AccountTypeCode")]
    public partial class Org_tbAccount
    {
        public Org_tbAccount()
        {
            TbPayments = new HashSet<Cash_tbPayment>();
        }

        [Key]
        [StringLength(10)]
        public string CashAccountCode { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashAccountName { get; set; }
        [StringLength(10)]
        public string SortCode { get; set; }
        [StringLength(20)]
        public string AccountNumber { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        public bool AccountClosed { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal OpeningBalance { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal CurrentBalance { get; set; }
        public short CoinTypeCode { get; set; }
        public short AccountTypeCode { get; set; }
        public short LiquidityLevel { get; set; }

        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Org_tbOrg.TbAccounts))]
        public virtual Org_tbOrg AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(AccountTypeCode))]
        [InverseProperty(nameof(Org_tbAccountType.TbAccounts))]
        public virtual Org_tbAccountType AccountTypeCodeNavigation { get; set; }
        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbAccounts))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(CoinTypeCode))]
        [InverseProperty(nameof(Cash_tbCoinType.TbAccounts))]
        public virtual Cash_tbCoinType CoinTypeCodeNavigation { get; set; }
        [InverseProperty(nameof(Cash_tbPayment.CashAccountCodeNavigation))]
        public virtual ICollection<Cash_tbPayment> TbPayments { get; set; }
    }
}
