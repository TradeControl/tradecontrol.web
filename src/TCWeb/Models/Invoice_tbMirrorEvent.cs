using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbMirrorEvent", Schema = "Invoice")]
    [Index(nameof(EventTypeCode), nameof(InvoiceStatusCode), nameof(InsertedOn), Name = "IX_Invoice_tbMirrorEvent_EventTypeCode", IsUnique = true)]
    public partial class Invoice_tbMirrorEvent
    {
        [Key]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Key]
        public int LogId { get; set; }
        public short EventTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidTaxValue { get; set; }
        [StringLength(42)]
        public string PaymentAddress { get; set; }

        [ForeignKey(nameof(ContractAddress))]
        [InverseProperty(nameof(Invoice_tbMirror.TbMirrorEvents))]
        public virtual Invoice_tbMirror ContractAddressNavigation { get; set; }
        [ForeignKey(nameof(EventTypeCode))]
        [InverseProperty(nameof(App_tbEventType.TbMirrorEvents))]
        public virtual App_tbEventType EventTypeCodeNavigation { get; set; }
    }
}
