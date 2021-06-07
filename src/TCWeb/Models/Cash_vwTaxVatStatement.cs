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
        [Display(Name = "Row No.")]
        public long RowNumber { get; set; }
        [Column(TypeName = "datetime")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime StartOn { get; set; }
        [Display(Name = "Vat Due")]
        [DataType(DataType.Currency)]
        public double VatDue { get; set; }
        [Display(Name = "Vat Paid")]
        [DataType(DataType.Currency)]
        public double VatPaid { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }
    }
}
