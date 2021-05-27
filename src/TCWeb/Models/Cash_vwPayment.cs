using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwPayment
    {
        [Required]
        [StringLength(10)]
        [Display (Name = "Account Code")]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Payment Code")]
        public string PaymentCode { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "User Id")]
        public string UserId { get; set; }
        public short PaymentStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Cash Account Code")]
        public string CashAccountCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Column(TypeName = "datetime")]
        [DataType (DataType.Date)]
        [Display(Name = "Paid On")]
        public DateTime PaidOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Paid In")]
        public decimal PaidInValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Paid Out")]
        public decimal PaidOutValue { get; set; }
        [StringLength(50)]
        [Display(Name = "Reference")]

        public string PaymentReference { get; set; }
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
        [Required]
        [StringLength(50)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [StringLength(50)]
        [Display(Name = "Tax")]
        public string TaxDescription { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Cash Account Name")]
        public string CashAccountName { get; set; }
        [StringLength(100)]
        [Display(Name = "Cash Description")]
        public string CashDescription { get; set; }
    }
}
