﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwItem
    {
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        public string CashDescription { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "ntext")]
        public string ItemReference { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
    }
}
