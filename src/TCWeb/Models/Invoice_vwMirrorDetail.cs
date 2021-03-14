using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwMirrorDetail
    {
        [Required]
        [StringLength(42)]
        public string ContractAddress { get; set; }
        [Required]
        [StringLength(50)]
        public string DetailRef { get; set; }
        [Required]
        [StringLength(50)]
        public string DetailCode { get; set; }
        [StringLength(256)]
        public string DetailDescription { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [StringLength(10)]
        public string TaxCode { get; set; }
        public byte[] RowVer { get; set; }
    }
}
