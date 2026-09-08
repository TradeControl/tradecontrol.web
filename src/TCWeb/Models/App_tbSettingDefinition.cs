using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbSettingDefinition", Schema = "App")]
    public partial class App_tbSettingDefinition
    {
        [Key, StringLength(30)]
        public string SettingCode { get; set; }

        [Required, StringLength(100)]
        public string SettingName { get; set; }

        [StringLength(10)]
        public string JurisdictionCode { get; set; }

        [StringLength(20)]
        public string AuthorityCode { get; set; }

        [StringLength(20)]
        public string ReportingTypeCode { get; set; }

        [Required, StringLength(10)]
        public string ValueTypeCode { get; set; }

        public string AllowedValues { get; set; }

        [StringLength(255)]
        public string ValidationPattern { get; set; }

        public bool IsSensitive { get; set; }
        public bool IsEnabled { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(JurisdictionCode))]
        public virtual App_tbJurisdiction JurisdictionCodeNavigation { get; set; }

        [ForeignKey(nameof(AuthorityCode))]
        public virtual App_tbAuthority AuthorityCodeNavigation { get; set; }

        [ForeignKey(nameof(ReportingTypeCode))]
        public virtual App_tbReportingType ReportingTypeCodeNavigation { get; set; }

        [ForeignKey(nameof(ValueTypeCode))]
        public virtual App_tbValueType ValueTypeCodeNavigation { get; set; }
    }
}
