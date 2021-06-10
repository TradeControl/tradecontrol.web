using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace TradeControl.Web.Pages.Cash.Totals
{
    [Keyless]
    public class Cash_Total
    {
        [Display(Name = "Category Code")]
        public string CategoryCode { get; set; }
        [Display(Name = "Category")]
        public string Category { get; set; }
        [Display(Name = "Cash Type")]
        public string CashType { get; set; }
        [Display(Name = "Display Order")]
        public short DisplayOrder { get; set; }
        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }
        [Display(Name = "Inserted On")]
        [DataType(DataType.Date)]
        public DateTime InsertedOn { get; set; }
        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }
        [Display(Name = "Updated On")]
        [DataType(DataType.Date)]
        public DateTime UpdatedOn { get; set; }
    }
}
