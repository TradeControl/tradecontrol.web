using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwAccountStatementListing
    {
        public short YearNumber { get; set; }
        [Required]
        [StringLength(255)]
        public string Bank { get; set; }
        [Required]
        [StringLength(10)]
        public string CashAccountCode { get; set; }
        [Required]
        [StringLength(50)]
        public string CashAccountName { get; set; }
        [StringLength(10)]
        public string SortCode { get; set; }
        [StringLength(20)]
        public string AccountNumber { get; set; }
        [Required]
        [StringLength(21)]
        public string PeriodName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        public int EntryNumber { get; set; }
        [StringLength(30)]
        public string PaymentCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime PaidOn { get; set; }
        [StringLength(255)]
        public string AccountName { get; set; }
        [StringLength(50)]
        public string PaymentReference { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidInValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidOutValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidBalance { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(100)]
        public string CashDescription { get; set; }
        [StringLength(50)]
        public string TaxDescription { get; set; }
        [StringLength(50)]
        public string UserName { get; set; }
        [StringLength(10)]
        public string AccountCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
    }
}
