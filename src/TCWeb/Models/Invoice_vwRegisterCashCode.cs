using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public partial class Invoice_vwRegisterCashCode
    {
        [Column(TypeName = "datetime")]
        [DataType(DataType.Date)]
        public DateTime StartOn { get; set; }
        [Display(Name = "Period")]
        public string PeriodName { get; set; }
        [StringLength(10)]
        [Display(Name = "Polarity")]
        public string CashPolarity { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Cash Desc.")]
        public string CashDescription { get; set; }
        [Display(Name = "Invoice Value")]
        [DataType(DataType.Currency)]
        public double TotalInvoiceValue { get; set; }
        [Display(Name = "Tax Value")]
        [DataType(DataType.Currency)]
        public double TotalTaxValue { get; set; }
        [Display(Name = "Total Value")]
        [DataType(DataType.Currency)]
        public double TotalValue { get; set; }
    }
}
