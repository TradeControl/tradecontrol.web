using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbAuthority", Schema = "App")]
    public partial class App_tbAuthority
    {
        [Key, StringLength(20)]
        public string AuthorityCode { get; set; }

        [Required, StringLength(10)]
        public string JurisdictionCode { get; set; }

        [Required, StringLength(100)]
        public string AuthorityName { get; set; }

        public bool IsEnabled { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        [ForeignKey(nameof(JurisdictionCode))]
        public virtual App_tbJurisdiction JurisdictionCodeNavigation { get; set; }
    }
}
