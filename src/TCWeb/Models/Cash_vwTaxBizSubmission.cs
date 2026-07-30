using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Cash_vwTaxBizSubmission
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Tax Source Code")]
        public string TaxSourceCode { get; init; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Tag Code")]
        public string TagCode { get; init; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Period From")]
        public DateTime PeriodFrom { get; init; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Period To")]
        public DateTime PeriodTo { get; init; }

        [Column(TypeName = "decimal(20, 5)")]
        [Display(Name = "Taxable Amount")]
        [DataType(DataType.Currency)]
        public decimal TaxableAmount { get; init; }
    }
}
