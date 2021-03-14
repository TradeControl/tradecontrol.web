using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTxReference", Schema = "Cash")]
    [Index(nameof(PaymentCode), nameof(TxNumber), Name = "IX_Cash_tbTxReference_PaymentCode")]
    public partial class Cash_tbTxReference
    {
        [Key]
        public int TxNumber { get; set; }
        [Key]
        public short TxStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        public string PaymentCode { get; set; }

        [ForeignKey(nameof(PaymentCode))]
        [InverseProperty(nameof(Cash_tbPayment.TbTxReferences))]
        public virtual Cash_tbPayment PaymentCodeNavigation { get; set; }
        [ForeignKey(nameof(TxNumber))]
        [InverseProperty(nameof(Cash_tbTx.TbTxReferences))]
        public virtual Cash_tbTx TxNumberNavigation { get; set; }
        [ForeignKey(nameof(TxStatusCode))]
        [InverseProperty(nameof(Cash_tbTxStatus.TbTxReferences))]
        public virtual Cash_tbTxStatus TxStatusCodeNavigation { get; set; }
    }
}
