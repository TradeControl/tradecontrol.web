using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Subject_fnInvoiceDag
    {
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }

        [Column(TypeName = "decimal(18, 5)")]
        public decimal TotalPaidValue { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
    }
}
