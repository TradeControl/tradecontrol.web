using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TradeControl.Web.Models
{
    [Table("tbDocOpenMode", Schema = "App")]
    public partial class App_tbDocOpenMode
    {
        public App_tbDocOpenMode()
        {
            TbDocs = new HashSet<App_tbDoc>();
        }

        [Key]
        public short OpenMode { get; set; }
        [StringLength(20)]
        public string OpenModeDescription { get; set; }

        [InverseProperty(nameof(App_tbDoc.OpenModeNavigation))]
        public virtual ICollection<App_tbDoc> TbDocs { get; set; }
    }
}
