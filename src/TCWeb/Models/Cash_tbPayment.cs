using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbPayment", Schema = "Cash")]
    [Index(nameof(PaymentReference), Name = "IX_Cash_tbPayment")]
    [Index(nameof(SubjectCode), nameof(PaidOn), Name = "IX_Cash_tbPayment_SubjectCode")]
    [Index(nameof(AccountCode), nameof(PaidOn), Name = "IX_Cash_tbPayment_AccountCode")]
    [Index(nameof(CashCode), nameof(PaidOn), Name = "IX_Cash_tbPayment_CashCode")]
    [Index(nameof(SubjectCode), nameof(PaymentStatusCode), nameof(PaymentCode), Name = "IX_Cash_tbPayment_PaymentCode_Status")]
    [Index(nameof(SubjectCode), nameof(PaymentCode), nameof(TaxCode), Name = "IX_Cash_tbPayment_PaymentCode_TaxCode")]
    [Index(nameof(PaymentStatusCode), Name = "IX_Cash_tbPayment_Status")]
    [Index(nameof(PaymentStatusCode), nameof(SubjectCode), Name = "IX_Cash_tbPayment_Status_SubjectCode")]
    [Index(nameof(PaymentStatusCode), nameof(AccountCode), nameof(PaidOn), Name = "IX_Cash_tbPayment_Status_CashAccount_PaidOn")]
    [Index(nameof(TaxCode), Name = "IX_tbPayment_TaxCode")]
    public partial class Cash_tbPayment
    {
        public Cash_tbPayment()
        {
            TbTxReferences = new HashSet<Cash_tbTxReference>();
        }

        [Key]
        [StringLength(20)]
        [Display(Name = "Pay Code")]
        public string PaymentCode { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "User")]
        public string UserId { get; set; }
        [Display(Name = "Status")]
        public short PaymentStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Account Code")]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(10)]
        [Display(Name = "Cash Account")]
        public string AccountCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [StringLength(10)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Paid On")]
        [DataType(DataType.Date)]
        public DateTime PaidOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Paid In")]
        public decimal PaidInValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Paid Out")]
        public decimal PaidOutValue { get; set; }
        [StringLength(50)]
        [Display(Name = "Ref.")]
        public string PaymentReference { get; set; }
        [Required]
        [Display(Name = "P&L?")]
        public bool IsProfitAndLoss { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Insert By")]
        public string InsertedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Insert On")]
        [DataType(DataType.Date)]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Update By")]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Update On")]
        [DataType(DataType.Date)]
        public DateTime UpdatedOn { get; set; }

        [ForeignKey(nameof(SubjectCode))]
        [InverseProperty(nameof(Subject_tbSubject.TbPayments))]
        public virtual Subject_tbSubject SubjectCodeNavigation { get; set; }
        [ForeignKey(nameof(AccountCode))]
        [InverseProperty(nameof(Subject_tbAccount.TbPayments))]
        public virtual Subject_tbAccount AccountCodeNavigation { get; set; }
        [ForeignKey(nameof(CashCode))]
        [InverseProperty(nameof(Cash_tbCode.TbPayments))]
        public virtual Cash_tbCode CashCodeNavigation { get; set; }
        [ForeignKey(nameof(PaymentStatusCode))]
        [InverseProperty(nameof(Cash_tbPaymentStatus.TbPayments))]
        public virtual Cash_tbPaymentStatus PaymentStatusCodeNavigation { get; set; }
        [ForeignKey(nameof(TaxCode))]
        [InverseProperty(nameof(App_tbTaxCode.TbPayments))]
        public virtual App_tbTaxCode TaxCodeNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Usr_tbUser.TbPayments))]
        public virtual Usr_tbUser User { get; set; }
        [InverseProperty(nameof(Cash_tbTxReference.PaymentCodeNavigation))]
        public virtual ICollection<Cash_tbTxReference> TbTxReferences { get; set; }
    }
}
