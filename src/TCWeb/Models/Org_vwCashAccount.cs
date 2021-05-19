using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    //CashAccountCode, AccountCode, CashAccountName, AccountName, OrganisationType, OpeningBalance, CurrentBalance, SortCode, AccountNumber, AccountClosed, AccountType
    [Keyless]
    public partial class Org_vwCashAccount
    {
        [StringLength(10)]
        [Display (Name = "Cash A/c")]
        public string CashAccountCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Cash Account")]
        public string CashAccountName { get; set; }
        [StringLength(255)]
        [Display(Name = "Organisation")]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string OrganisationType { get; set; }
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [StringLength(100)]
        [Display(Name = "Cash Desc")]
        public string CashDescription { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Opening Balance")]
        [DataType(DataType.Currency)]
        public decimal OpeningBalance { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Current Balance")]
        [DataType(DataType.Currency)]
        public decimal CurrentBalance { get; set; }
        [Display(Name = "Liquidity")]
        public short LiquidityLevel { get; set; }
        [StringLength(10)]
        [Display(Name = "Sort Code")]
        public string SortCode { get; set; }
        [StringLength(20)]
        [Display(Name = "Account No")]
        public string AccountNumber { get; set; }
        [Display(Name = "Closed?")]
        public bool AccountClosed { get; set; }
        [StringLength(20)]
        [Display(Name = "Account")]
        public string AccountType { get; set; }
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Insert On")]
        [DataType(DataType.Date)]
        public DateTime InsertedOn { get; set; }
    }
}
