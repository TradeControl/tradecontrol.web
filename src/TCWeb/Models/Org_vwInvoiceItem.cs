﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Org_vwInvoiceItem
    {
        [Required]
        [StringLength(10)]
        public string AccountCode { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        public short InvoiceTypeCode { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal PaidTaxValue { get; set; }
        [Column(TypeName = "ntext")]
        public string ItemReference { get; set; }
    }
}
