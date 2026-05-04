using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbTemplateDataset", Schema = "App")]
    [Index(nameof(TemplateCode), Name = "IX_App_tbTemplateDataset_TemplateCode")]
    [Index(nameof(TemplateCode), nameof(DatasetTitle), Name = "IX_App_tbTemplateDataset_TemplateCode_DatasetTitle", IsUnique = true)]
    public partial class App_tbTemplateDataset
    {
        [Key]
        [StringLength(10)]
        public string DatasetCode { get; set; }

        [Required]
        [StringLength(10)]
        public string TemplateCode { get; set; }

        [Required]
        [StringLength(100)]
        public string DatasetTitle { get; set; }

        public string Notes { get; set; }

        public bool IsCompany { get; set; }

        public bool? IsVatRegistered { get; set; }

        public bool UseStdCompanyTemplate { get; set; }

        public int MisOrdersPerMonth { get; set; }

        public int MonthsForward { get; set; }

        [Column(TypeName = "decimal(18, 7)")]
        public decimal PriceRatio { get; set; }

        [Column(TypeName = "decimal(18, 7)")]
        public decimal QuantityRatio { get; set; }

        [Column(TypeName = "decimal(18, 7)")]
        public decimal FloatRatio { get; set; }

        [ForeignKey(nameof(TemplateCode))]
        [InverseProperty(nameof(App_tbTemplate.App_tbTemplateDatasets))]
        public virtual App_tbTemplate TemplateCodeNavigation { get; set; }
    }
}
