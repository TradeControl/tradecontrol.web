using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_fnPaymentDag
    {
        [Required]
        [StringLength(20)]
        public string PaymentCode { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime PaidOn { get; set; }

        [StringLength(50)]
        public string PaymentReference { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidOutValue { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidInValue { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountName { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
    }
}
