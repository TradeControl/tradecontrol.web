using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbPaymentStatus", Schema = "Cash")]
    public partial class Cash_tbPaymentStatus
    {
        public Cash_tbPaymentStatus()
        {
            TbPayments = new HashSet<Cash_tbPayment>();
        }

        [Key]
        public short PaymentStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; }

        [InverseProperty(nameof(Cash_tbPayment.PaymentStatusCodeNavigation))]
        public virtual ICollection<Cash_tbPayment> TbPayments { get; set; }
    }
}
