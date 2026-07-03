namespace TradeControl.Web.Pages.Invoice.MudBlazorEval;

public enum WorkspaceMode
{
    Register,
    DetailGrid,
    DetailPanel
}

public sealed class WorkspaceState<TKey>
{
    public WorkspaceMode Mode { get; set; }
        = WorkspaceMode.Register;

    public TKey? SelectedKey { get; set; }

    public string? SearchText { get; set; }

    public int CurrentPage { get; set; }

    public int RowsPerPage { get; set; } = 50;

    public bool HasSelection => SelectedKey is not null;
}
