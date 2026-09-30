using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbReportingTypeRegistrationScheme", Schema = "App")]
    public partial class App_tbReportingTypeRegistrationScheme
    {
        public short ReportingTypeCode { get; set; }

        [Required, StringLength(20)]
        public string RegistrationSchemeCode { get; set; }

        [ForeignKey(nameof(ReportingTypeCode))]
        public virtual App_tbReportingType ReportingTypeCodeNavigation { get; set; }

        [ForeignKey(nameof(RegistrationSchemeCode))]
        public virtual App_tbRegistrationScheme RegistrationSchemeCodeNavigation { get; set; }
    }
}
