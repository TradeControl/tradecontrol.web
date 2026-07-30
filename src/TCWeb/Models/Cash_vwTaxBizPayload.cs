using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Cash_vwTaxBizPayload
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Tax Source Code")]
        public string TaxSourceCode { get; init; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Tag Code")]
        public string TagCode { get; init; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Parent Code")]
        public string ParentCode { get; init; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Cash Code")]
        public string CashCode { get; init; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Category Code")]
        public string CategoryCode { get; init; }

        [Required]
        [Display(Name = "Cash Type Code")]
        public short CashTypeCode { get; init; }

        [Required]
        [Column(TypeName = "datetime")]
        [Display(Name = "Start On")]
        public DateTime PeriodStartOn { get; init; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Period From")]
        public DateTime PeriodFrom { get; init; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Period To")]
        public DateTime PeriodTo { get; init; }

        [Column(TypeName = "decimal(20, 5)")]
        [Display(Name = "Invoice Value")]
        [DataType(DataType.Currency)]
        public decimal PeriodInvoiceValue { get; init; }

    }
}
