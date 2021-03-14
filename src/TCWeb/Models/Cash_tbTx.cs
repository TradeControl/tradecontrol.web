using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTx", Schema = "Cash")]
    [Index(nameof(PaymentAddress), nameof(TxId), Name = "IX_Cash_tbTx_PaymentAddress", IsUnique = true)]
    [Index(nameof(TxStatusCode), nameof(TransactedOn), Name = "IX_Cash_tbTx_TxStatusCode")]
    public partial class Cash_tbTx
    {
        public Cash_tbTx()
        {
            TbTxReferences = new HashSet<Cash_tbTxReference>();
        }

        [Key]
        public int TxNumber { get; set; }
        [Required]
        [StringLength(42)]
        public string PaymentAddress { get; set; }
        [Required]
        [StringLength(64)]
        public string TxId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TransactedOn { get; set; }
        public short TxStatusCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal MoneyIn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal MoneyOut { get; set; }
        public int Confirmations { get; set; }
        [StringLength(50)]
        public string TxMessage { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }

        [ForeignKey(nameof(PaymentAddress))]
        [InverseProperty(nameof(Cash_tbChange.TbTxes))]
        public virtual Cash_tbChange PaymentAddressNavigation { get; set; }
        [ForeignKey(nameof(TxStatusCode))]
        [InverseProperty(nameof(Cash_tbTxStatus.TbTxes))]
        public virtual Cash_tbTxStatus TxStatusCodeNavigation { get; set; }
        [InverseProperty(nameof(Cash_tbTxReference.TxNumberNavigation))]
        public virtual ICollection<Cash_tbTxReference> TbTxReferences { get; set; }
    }
}
