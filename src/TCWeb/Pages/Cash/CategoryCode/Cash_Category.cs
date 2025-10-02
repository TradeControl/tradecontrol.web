using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TradeControl.Web.Pages.Cash.CategoryCode
{
    [Keyless]

    public class Cash_Category
    {
        [Required]
        [StringLength(10)]
        [Display(Name = "Category Code")]
        public string CategoryCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; }
        [StringLength(20)]
        [Display(Name = "Cat. Type")]
        public string CategoryType { get; set; }
        [Display(Name = "Cash Type Code")]
        public short CashTypeCode { get; set; }
        [Display(Name = "Display Order")]
        public short DisplayOrder { get; set; }
        [StringLength(25)]
        [Display(Name = "Cash Type")]
        public string CashType { get; set; }
        [StringLength(10)]
        [Display(Name = "Mode")]
        public string CashPolarity { get; set; }
        [Display(Name = "Enabled?")]
        public bool IsEnabled { get; set; }
    }
}
