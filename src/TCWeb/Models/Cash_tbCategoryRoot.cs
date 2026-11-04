using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeControl.Web.Models;

[Table("tbCategoryRoot", Schema = "Cash")]
public sealed class Cash_tbCategoryRoot
{
    [StringLength(10)]
    public string VersionId { get; set; } = string.Empty;

    public short RootTypeCode { get; set; }

    [StringLength(10)]
    public string CategoryCode { get; set; } = string.Empty;

}
