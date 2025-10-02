using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Data;

namespace TradeControl.Web.Models
{
    [Keyless]
    public class Object_proc_WorkFlow
    {
        public string ObjectCode { get; set; } = string.Empty;
        public string ProjectStatus { get; set; } = string.Empty;
        [Column(TypeName = "smallint")]
        public NodeEnum.CashPolarity CashPolarityCode { get; set; } = NodeEnum.CashPolarity.Neutral;
        public string UnitOfMeasure { get; set; } = "each";
        public short OffsetDays { get; set; } = 0;
        [Column(TypeName = "decimal(18, 6)")]
        public decimal UsedOnQuantity { get; set; } = 1;
    }
}
