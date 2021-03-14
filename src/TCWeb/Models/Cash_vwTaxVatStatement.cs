using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Cash_vwTaxVatStatement
    {
        public long RowNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }
        public double VatDue { get; set; }
        public double VatPaid { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal Balance { get; set; }
    }
}
