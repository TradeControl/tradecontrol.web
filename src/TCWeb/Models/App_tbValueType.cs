using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbValueType", Schema = "App")]
    public partial class App_tbValueType
    {
        [Key, StringLength(10)]
        public string ValueTypeCode { get; set; }

        [Required, StringLength(50)]
        public string ValueTypeName { get; set; }
    }
}
