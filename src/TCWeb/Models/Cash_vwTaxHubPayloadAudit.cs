using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Cash_vwTaxHubPayloadAudit
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
        [Display(Name = "Cash Code")]
        public string CashCode { get; init; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Period From")]
        public DateTime PeriodFrom { get; init; }

        [Column(TypeName = "datetime")]
        [Display(Name = "Period To")]
        public DateTime PeriodTo { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Raw Total")]
        [DataType(DataType.Currency)]
        public decimal RawTotal { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Payload Total")]
        [DataType(DataType.Currency)]
        public decimal PayloadTotal { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Difference")]
        [DataType(DataType.Currency)]
        public decimal Difference { get; init; }

    }
}
