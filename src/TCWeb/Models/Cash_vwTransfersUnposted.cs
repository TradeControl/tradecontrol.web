using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTransfersUnposted
    {
        [Required]
        [StringLength(20)]
        public string PaymentCode { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        public short PaymentStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(10)]
        public string CashAccountCode { get; set; }
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime PaidOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidInValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidOutValue { get; set; }
        [StringLength(50)]
        public string PaymentReference { get; set; }
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
    }
}
