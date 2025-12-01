using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwAccountStatement
    {
        [Required]
        [StringLength(10)]
        [Display(Name = "Cash Account")]
        public string AccountCode { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Start On")]
        public DateTime StartOn { get; set; }
        [Display(Name = "Entry")]
        public long EntryNumber { get; set; }
        [StringLength(30)]
        [Display(Name = "Payment Code")]
        public string PaymentCode { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Paid On")]
        [DataType(DataType.Date)]
        public DateTime PaidOn { get; set; }
        [StringLength(255)]
        [Display(Name = "Name")]
        public string SubjectName { get; set; }
        [StringLength(50)]
        [Display(Name = "Ref.")]
        public string PaymentReference { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Paid In")]
        [DataType(DataType.Currency)]
        public decimal PaidInValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Paid Out")]
        [DataType(DataType.Currency)]
        public decimal PaidOutValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        public decimal PaidBalance { get; set; }
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [StringLength(100)]
        [Display(Name = "Cash Desc.")]
        public string CashDescription { get; set; }
        [StringLength(50)]
        [Display(Name = "Tax Desc.")]
        public string TaxDescription { get; set; }
        [StringLength(50)]
        [Display(Name = "User")]
        public string UserName { get; set; }
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
    }
}
