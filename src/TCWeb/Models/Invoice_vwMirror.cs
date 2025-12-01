using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwMirror
    {
        [Required]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        public bool IsMirrored { get; set; }
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string MirrorNumber { get; set; }
        public short InvoiceTypeCode { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        public short CashPolarityCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueOn { get; set; }
        [StringLength(5)]
        public string UnitOfCharge { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal InvoiceTax { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(20, 5)")]
        public decimal PaidTaxValue { get; set; }
        [StringLength(42)]
        public string PaymentAddress { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
