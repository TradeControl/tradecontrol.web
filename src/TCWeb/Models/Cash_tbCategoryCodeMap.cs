using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeControl.Web.Models;

[Table("tbCategoryCodeMap", Schema = "Cash")]
public sealed class Cash_tbCategoryCodeMap
{
    [StringLength(10)]
    public string VersionId { get; set; } = string.Empty;

    [StringLength(50)]
    public string CashCode { get; set; } = string.Empty;

    [StringLength(10)]
    public string CategoryCode { get; set; } = string.Empty;

}
