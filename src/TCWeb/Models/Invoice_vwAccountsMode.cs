using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwAccountsMode
    {
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(10)]
        public string UserId { get; set; }
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        public short InvoiceTypeCode { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "ntext")]
        public string Notes { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "ntext")]
        public string ItemReference { get; set; }
        [Required]
        public byte[] InvoiceRowVer { get; set; }
        [Required]
        public byte[] ItemRowVer { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidTaxValue { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ExpectedOn { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueOn { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        public bool Printed { get; set; }
        public bool Spooled { get; set; }
    }
}
