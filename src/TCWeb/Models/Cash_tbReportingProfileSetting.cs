using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbReportingProfileSetting", Schema = "Cash")]
    public partial class Cash_tbReportingProfileSetting
    {
        [Required, StringLength(50)]
        public string SubjectCode { get; set; }

        [Required, StringLength(20)]
        public string ReportingProfileCode { get; set; }

        [Required, StringLength(30)]
        public string SettingCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime EffectiveFrom { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EffectiveTo { get; set; }

        [StringLength(4000)]
        public string TextValue { get; set; }

        public long? IntegerValue { get; set; }

        [Column(TypeName = "decimal(28,9)")]
        public decimal? DecimalValue { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateValue { get; set; }

        public bool? BooleanValue { get; set; }
        public short StatusCode { get; set; }

        [Required, StringLength(10)]
        public string ValueSourceCode { get; set; }

        public bool IsReviewed { get; set; }

        [Required, StringLength(50)]
        public string InsertedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime InsertedOn { get; set; }

        [Required, StringLength(50)]
        public string UpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedOn { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual Cash_tbReportingProfile ReportingProfileNavigation { get; set; }

        [ForeignKey(nameof(SettingCode))]
        public virtual App_tbSettingDefinition SettingCodeNavigation { get; set; }

        [ForeignKey(nameof(StatusCode))]
        public virtual App_tbStatutoryStatus StatusCodeNavigation { get; set; }

        [ForeignKey(nameof(ValueSourceCode))]
        public virtual App_tbValueSource ValueSourceCodeNavigation { get; set; }
    }
}
