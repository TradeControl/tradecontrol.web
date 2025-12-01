using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("vwPaymentsUnposted", Schema = "Cash")]
    public partial class Cash_vwPaymentsUnposted
    {
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
        [DataType(DataType.Currency)]
        [Display(Name = "Paid In")]
        public decimal PaidInValue { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Paid Out")]
        public decimal PaidOutValue { get; set; }

        [StringLength(50)]
        [Display(Name = "Ref.")]
        public string PaymentReference { get; set; }

        [Display(Name = "P&L?")]
        public bool IsProfitAndLoss { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }

        [Display(Name = "Inserted On")]
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Updated On")]
        public DateTime UpdatedOn { get; set; }
    }
}
