using System;
using System.Collections.Generic;
using System.Linq;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices
{
    public enum TaxConfiguratorNodeKind
    {
        Jurisdiction,
        Source,
        TagClass,
        Tag
    }

    public sealed class TaxConfiguratorLookupRow
    {
        public string Code { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
    }

    public sealed class TaxConfiguratorMappingRow
    {
        public NodeEnum.MapTypeCode MapTypeCode { get; init; }
        public string MapType { get; init; } = string.Empty;
        public string TargetCode { get; init; } = string.Empty;
        public string TargetName { get; init; } = string.Empty;
        public bool IsEnabled { get; init; }
    }

    public sealed class TaxConfiguratorTagRow
    {
        public string TagCode { get; init; } = string.Empty;
        public string TagName { get; init; } = string.Empty;
        public short DisplayOrder { get; init; }
        public bool IsEnabled { get; init; }
        public bool IsMapped { get; init; }
    }

    public sealed class TaxConfiguratorValidationRow
    {
        public bool IsError { get; init; }
        public string? TagCode { get; init; }
        public string? TagName { get; init; }
        public string? CashCode { get; init; }
        public string? CategoryCode { get; init; }
        public int? HitCount { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    public sealed class TaxConfiguratorNodeDetails
    {
        public TaxConfiguratorNodeKind Kind { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }

        public string? ParentCode { get; init; }
        public string? ParentName { get; init; }

        public string? JurisdictionCode { get; init; }
        public string? JurisdictionName { get; init; }

        public string? TaxSourceCode { get; init; }
        public string? TaxSourceName { get; init; }

        public string? TagClassCode { get; init; }
        public string? TagClassName { get; init; }

        public bool CanMap { get; init; }
        public short? DisplayOrder { get; init; }

        public IReadOnlyList<TaxConfiguratorValidationRow> ValidationIssues { get; init; } = Array.Empty<TaxConfiguratorValidationRow>();
        public int ValidationIssueCount => ValidationIssues.Count;
        public int ValidationErrorCount => ValidationIssues.Count(v => v.IsError);
        public bool HasValidationErrors => ValidationErrorCount > 0;

        public IReadOnlyList<TaxConfiguratorLookupRow> CategoryOptions { get; init; } = Array.Empty<TaxConfiguratorLookupRow>();
        public IReadOnlyList<TaxConfiguratorLookupRow> CashCodeOptions { get; init; } = Array.Empty<TaxConfiguratorLookupRow>();
        public IReadOnlyList<TaxConfiguratorTagRow> Tags { get; init; } = Array.Empty<TaxConfiguratorTagRow>();
        public IReadOnlyList<TaxConfiguratorMappingRow> Mappings { get; init; } = Array.Empty<TaxConfiguratorMappingRow>();
    }
}
