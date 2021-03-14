using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxCorpStatement
    {
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxDue { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal TaxPaid { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal Balance { get; set; }
    }
}
