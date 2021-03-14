using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwNetworkChangeLog
    {
        public int LogId { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        public short InvoiceTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        public short InvoiceStatusCode { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        public short TransmitStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        public string TransmitStatus { get; set; }
        public short CashModeCode { get; set; }
        [StringLength(10)]
        public string CashMode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidTaxValue { get; set; }
        [Required]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ChangedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
