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
        [Display (Name = "Cash A/c")]
        public string CashAccountCode { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Organisation")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Cash A/c Name")]
        public string CashAccountName { get; set; }
        [StringLength(10)]
        [Display(Name = "Sort Code")]
        public string SortCode { get; set; }
        [StringLength(20)]
        [Display(Name = "Bank A/c No")]
        public string AccountNumber { get; set; }
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [Display(Name = "Is Closed?")]
        public bool AccountClosed { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Insert By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Insert On")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Update By")]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Update On")]
        public DateTime UpdatedOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Opening Balance")]
        public decimal OpeningBalance { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Current Balance")]
        public decimal CurrentBalance { get; set; }
        [Display(Name = "Coin Type")]
        public short CoinTypeCode { get; set; }
        [Display(Name = "Account Type")]
        public short AccountTypeCode { get; set; }
        [Display(Name = "Liquidity")]
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
