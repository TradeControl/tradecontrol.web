using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTxStatus", Schema = "Cash")]
    public partial class Cash_tbTxStatus
    {
        public Cash_tbTxStatus()
        {
            TbTxReferences = new HashSet<Cash_tbTxReference>();
            TbTxes = new HashSet<Cash_tbTx>();
        }

        [Key]
        public short TxStatusCode { get; set; }
        [Required]
        [StringLength(10)]
        public string TxStatus { get; set; }

        [InverseProperty(nameof(Cash_tbTxReference.TxStatusCodeNavigation))]
        public virtual ICollection<Cash_tbTxReference> TbTxReferences { get; set; }
        [InverseProperty(nameof(Cash_tbTx.TxStatusCodeNavigation))]
        public virtual ICollection<Cash_tbTx> TbTxes { get; set; }
    }
}
