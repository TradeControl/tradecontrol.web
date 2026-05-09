using System;
using System.Collections.Generic;
using System.Globalization;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Subject.Browser
{
    public sealed class SubjectBrowserDetailModel
    {
        public string SubjectCode { get; init; } = string.Empty;
        public short SubjectTypeCode { get; init; }
        public string SubjectType { get; init; } = string.Empty;
        public NodeEnum.SubjectClass SubjectClass { get; init; }
        public string Name { get; init; } = string.Empty;
        public string DisplayLabel { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public bool IsDefaultInNamespace { get; init; }
        public IReadOnlyList<string> NamespacePaths { get; init; } = Array.Empty<string>();
        public IReadOnlyList<SubjectBrowserDetailField> IdentityFields { get; init; } = Array.Empty<SubjectBrowserDetailField>();
        public IReadOnlyList<SubjectBrowserAddressItem> Addresses { get; init; } = Array.Empty<SubjectBrowserAddressItem>();

        public bool IsStructural => SubjectClass == NodeEnum.SubjectClass.Structural;
        public bool IsReal => SubjectClass == NodeEnum.SubjectClass.Real;
        public bool IsVirtual => SubjectClass == NodeEnum.SubjectClass.Virtual;
        public string SubjectClassCode => ((short)SubjectClass).ToString(CultureInfo.InvariantCulture);
        public string SubjectClassName => SubjectClass.ToString();

        public string DetailSectionTitle => SubjectClass switch {
            NodeEnum.SubjectClass.Real => "Person Details",
            NodeEnum.SubjectClass.Virtual => "Organisation Details",
            NodeEnum.SubjectClass.Structural => "Structural Details",
            _ => "Details"
        };

        public string DefaultBadgeCssClass => IsReal
            ? "text-bg-success"
            : "text-bg-light border text-secondary";
    }

    public sealed record SubjectBrowserDetailField(string Label, string Value);

    public sealed record SubjectBrowserAddressItem(string AddressCode, string Address, bool IsDefault);
}
