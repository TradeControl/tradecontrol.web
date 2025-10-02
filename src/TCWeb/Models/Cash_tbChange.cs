using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbChange", Schema = "Cash")]
    [Index(nameof(AccountCode), nameof(ChangeStatusCode), nameof(AddressIndex), Name = "IX_Cash_tbChange_ChangeStatusCode")]
    public partial class Cash_tbChange
    {
        public Cash_tbChange()
        {
            TbTxes = new HashSet<Cash_tbTx>();
        }

        [Key]
        [StringLength(42)]
        public string PaymentAddress { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        public short ChangeTypeCode { get; set; }
        public short ChangeStatusCode { get; set; }
        public int AddressIndex { get; set; }
        [StringLength(256)]
        public string Note { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        [StringLength(50)]
        public string InsertedBy { get; set; }
        [Required]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(ChangeTypeCode))]
        [InverseProperty(nameof(Cash_tbChangeType.TbChanges))]
        public virtual Cash_tbChangeType ChangeTypeCodeNavigation { get; set; }
        [InverseProperty("PaymentAddressNavigation")]
        public virtual Cash_tbChangeReference TbChangeReference { get; set; }
        [InverseProperty(nameof(Cash_tbTx.PaymentAddressNavigation))]
        public virtual ICollection<Cash_tbTx> TbTxes { get; set; }
    }
}
