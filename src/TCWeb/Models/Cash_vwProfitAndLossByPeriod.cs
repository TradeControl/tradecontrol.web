using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Cash_vwProfitAndLossByPeriod
    {
        [Display(Name = "Category Code")]
        public string CategoryCode { get; set; }
        [Display(Name = "Category")] 
        public string Category { get; set; }
        [Display(Name = "Cash Type")]
        public short CashTypeCode { get; set; }

        [Display(Name = "Year No.")]
        public short YearNumber { get; set; }
        [Display(Name = "Month No.")]
        public short MonthNumber { get; set; }
        [Display(Name = "Order")]
        public short DisplayOrder { get; set; }
        [Display(Name = "Year")]
        public string Description { get; set; }
        [Display(Name = "Month")]
        public string MonthName { get; set; }
        [Column(TypeName = "datetime")]
        [Display(Name = "Period")]
        public DateTime StartOn { get; set; }
        [Display(Name = "Total Value")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 5)")]
        public decimal InvoiceValue { get; set; }
    }
}
