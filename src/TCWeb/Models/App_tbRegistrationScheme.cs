using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbRegistrationScheme", Schema = "App")]
    public partial class App_tbRegistrationScheme
    {
        [Key, StringLength(20)]
        public string RegistrationSchemeCode { get; set; }

        [Required, StringLength(20)]
        public string AuthorityCode { get; set; }

        [Required, StringLength(100)]
        public string SchemeName { get; set; }

        [Required, StringLength(10)]
        public string ValueTypeCode { get; set; }

        [StringLength(255)]
        public string ValidationPattern { get; set; }

        public bool IsSensitive { get; set; }
        public bool IsSingleValue { get; set; }
        public bool IsEnabled { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(AuthorityCode))]
        public virtual App_tbAuthority AuthorityCodeNavigation { get; set; }

        [ForeignKey(nameof(ValueTypeCode))]
        public virtual App_tbValueType ValueTypeCodeNavigation { get; set; }
    }
}
