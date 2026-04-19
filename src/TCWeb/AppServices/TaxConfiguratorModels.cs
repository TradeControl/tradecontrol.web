using System;
using System.Collections.Generic;
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

    public sealed class TaxConfiguratorMappingRow
    {
        public NodeEnum.MapTypeCode MapTypeCode { get; init; }
        public string MapType { get; init; } = string.Empty;
        public string TargetCode { get; init; } = string.Empty;
        public string TargetName { get; init; } = string.Empty;
        public bool IsEnabled { get; init; }
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

        public bool IsEnabled { get; init; }
        public bool CanMap { get; init; }
        public short? DisplayOrder { get; init; }

        public IReadOnlyList<TaxConfiguratorMappingRow> Mappings { get; init; } = Array.Empty<TaxConfiguratorMappingRow>();
    }
}
