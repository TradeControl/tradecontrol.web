using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbStatutoryStatus", Schema = "App")]
    public partial class App_tbStatutoryStatus
    {
        [Key]
        public short StatusCode { get; set; }

        [Required, StringLength(50)]
        public string StatusName { get; set; }

        public bool IsActive { get; set; }
    }
}
