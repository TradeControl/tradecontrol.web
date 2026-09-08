using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbValueSource", Schema = "App")]
    public partial class App_tbValueSource
    {
        [Key, StringLength(10)]
        public string ValueSourceCode { get; set; }

        [Required, StringLength(50)]
        public string ValueSourceName { get; set; }

        public bool RequiresReview { get; set; }
    }
}
