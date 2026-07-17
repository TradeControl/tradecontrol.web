using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Cash_vwEquityReconciliationByYear
    {
        [Required]
        [Display(Name = "Year")]
        public short YearNumber { get; init; }

        [StringLength(10)]
        [Display(Name = "Description")]
        public string Description { get; init; }
        [Column(TypeName = "decimal(38, 5)")]

        [Display(Name = "Opening Capital")]
        [DataType(DataType.Currency)]
        public decimal OpeningCapital { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Closing Capital")]
        [DataType(DataType.Currency)]
        public decimal ClosingCapital { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Profit")]
        [DataType(DataType.Currency)]
        public decimal Profit { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Business Tax")]
        [DataType(DataType.Currency)]
        public decimal BusinessTax { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Profit After Tax")]
        [DataType(DataType.Currency)]
        public decimal ProfitAfterTax { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Tax Carry")]
        [DataType(DataType.Currency)]
        public decimal TaxCarry { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Capital Movement")]
        [DataType(DataType.Currency)]
        public decimal CapitalMovement { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Opening Position")]
        [DataType(DataType.Currency)]
        public decimal OpeningPosition { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Opening Account Position")]
        [DataType(DataType.Currency)]
        public decimal OpeningAccountPosition { get; init; }

        [Column(TypeName = "decimal(38, 6)")]
        [Display(Name = "Opening Losses C/F")]
        [DataType(DataType.Currency)]
        public decimal OpeningLossesCarriedForward { get; init; }

        [Column(TypeName = "decimal(38, 6)")]
        [Display(Name = "Closing Losses C/F")]
        [DataType(DataType.Currency)]
        public decimal ClosingLossesCarriedForward { get; init; }

        [Column(TypeName = "decimal(38, 6)")]
        [Display(Name = "Losses C/F Delta")]
        [DataType(DataType.Currency)]
        public decimal LossesCarriedForwardDelta { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Captial Delta")]
        [DataType(DataType.Currency)]
        public decimal CapitalDelta { get; init; }

        [Column(TypeName = "decimal(38, 5)")]
        [Display(Name = "Variance")]
        [DataType(DataType.Currency)]
        public decimal Variance { get; init; }
    }
}
