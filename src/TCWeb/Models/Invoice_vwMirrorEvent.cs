using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwMirrorEvent
    {
        [Required]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }
        public int LogId { get; set; }
        public short EventTypeCode { get; set; }
        [Required]
        [StringLength(15)]
        public string EventType { get; set; }
        public short InvoiceStatusCode { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidTaxValue { get; set; }
        [StringLength(42)]
        public string PaymentAddress { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
