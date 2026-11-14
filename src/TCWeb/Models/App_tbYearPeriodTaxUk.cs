using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeControl.Web.Models
{
    [Table("tbYearPeriodTaxUk", Schema = "App")]
    public class App_tbYearPeriodTaxUk
    {
        [Key]
        public short YearNumber { get; set; }

        [Key]
        [Column(TypeName = "datetime")]
        public DateTime StartOn { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal CorporationTaxMainRate { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal CorporationTaxSmallProfitsRate { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal CorporationTaxMarginalReliefFraction { get; set; }

        [Column(TypeName = "decimal(28, 5)")]
        public decimal CorporationTaxLowerLimit { get; set; }

        [Column(TypeName = "decimal(28, 5)")]
        public decimal CorporationTaxUpperLimit { get; set; }

        [Required, StringLength(50)]
        public string InsertedBy { get; set; } = string.Empty;

        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }

        [Required, StringLength(50)]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }
    }
}
