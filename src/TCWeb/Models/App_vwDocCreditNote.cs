using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class App_vwDocCreditNote
    {
        [Required]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }
        public bool Printed { get; set; }
        public bool Spooled { get; set; }
        public short InvoiceStatusCode { get; set; }
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; }
        [Required]
        [StringLength(255)]
        public string SubjectName { get; set; }
        [StringLength(50)]
        public string InvoiceStatus { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime InvoicedOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxValue { get; set; }
        [StringLength(100)]
        public string PaymentTerms { get; set; }
        [Column(TypeName = "ntext")]
        public string Notes { get; set; }
        [StringLength(255)]
        public string EmailAddress { get; set; }
        [Required]
        public byte[] RowVer { get; set; }
    }
}
