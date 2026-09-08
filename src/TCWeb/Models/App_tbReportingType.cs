using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbReportingType", Schema = "App")]
    public partial class App_tbReportingType
    {
        [Key, StringLength(20)]
        public string ReportingTypeCode { get; set; }

        [Required, StringLength(20)]
        public string AuthorityCode { get; set; }

        [Required, StringLength(100)]
        public string ReportingTypeName { get; set; }

        public bool RequiresTaxSource { get; set; }
        public bool IsEnabled { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(AuthorityCode))]
        public virtual App_tbAuthority AuthorityCodeNavigation { get; set; }
    }
}
