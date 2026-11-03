using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeControl.Web.Models;

[Table("tbCategoryVersionStatus", Schema = "Cash")]
public sealed class Cash_tbCategoryVersionStatus
{
    [Key]
    public short VersionStatusCode { get; set; }

    [Required, StringLength(20)]
    public string VersionStatus { get; set; } = string.Empty;
}

[Table("tbCategoryVersion", Schema = "Cash")]
public sealed class Cash_tbCategoryVersion
{
    [Key, StringLength(10)]
    public string VersionId { get; set; } = string.Empty;

    public short VersionStatusCode { get; set; }

    [Required, StringLength(100)]
    public string Description { get; set; } = string.Empty;

    public string? Notes { get; set; }

    [StringLength(10)]
    public string? PredecessorVersionId { get; set; }

    [Column(TypeName = "binary(32)")]
    public byte[]? BaseContentSha256 { get; set; }

    [Required, StringLength(50)]
    public string CanonicalProfile { get; set; } = string.Empty;

    [Column(TypeName = "binary(32)")]
    public byte[]? CanonicalSha256 { get; set; }

    public long? CanonicalByteLength { get; set; }
    public DateTime ContentChangedOn { get; set; }
    public DateTime? EffectiveFromOn { get; set; }
    public DateTime? EffectiveToOn { get; set; }
    public DateTime CreatedOn { get; set; }

    [Required, StringLength(10)]
    public string CreatedByUserId { get; set; } = string.Empty;

    [Timestamp]
    public byte[] RowVer { get; set; } = [];
}

[Table("tbCategoryVersionEvent", Schema = "Cash")]
public sealed class Cash_tbCategoryVersionEvent
{
    [Key]
    public long VersionEventId { get; set; }

    [Required, StringLength(10)]
    public string VersionId { get; set; } = string.Empty;

    public short EventTypeCode { get; set; }
    public short? FromStatusCode { get; set; }
    public short? ToStatusCode { get; set; }
    public DateTime EventOn { get; set; }

    [Required, StringLength(10)]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    public string? ChangeNote { get; set; }

    [Column(TypeName = "binary(32)")]
    public byte[]? ContentSha256 { get; set; }
}

[Table("tbCategoryRootType", Schema = "Cash")]
public sealed class Cash_tbCategoryRootType
{
    [Key]
    public short RootTypeCode { get; set; }

    [Required, StringLength(20)]
    public string RootType { get; set; } = string.Empty;
}

