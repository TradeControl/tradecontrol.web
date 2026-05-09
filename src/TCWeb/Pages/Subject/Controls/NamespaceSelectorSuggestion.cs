namespace TradeControl.Web.Pages.Subject.Controls
{
    public sealed class NamespaceSelectorSuggestion
    {
        public string Segment { get; init; } = string.Empty;
        public string FullPath { get; init; } = string.Empty;
        public bool HasChildren { get; init; }
        public string? DisplayLabel { get; init; }
    }
}
