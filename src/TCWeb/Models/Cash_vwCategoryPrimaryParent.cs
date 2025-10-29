using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Models
{
    [Keyless]
    [Table("vwCategoryPrimaryParent", Schema = "Cash")]
    public class Cash_vwCategoryPrimaryParent
    {
        public string ParentCode { get; set; } = string.Empty;
        public string ChildCode { get; set; } = string.Empty;
        public string RootCode { get; set; } = string.Empty;
        public string PrimaryKind { get; set; } = string.Empty; // "Profit" or "VAT"
        public int Depth { get; set; }
        public int rn { get; set; }
        public int ParentCount { get; set; }
    }
}
