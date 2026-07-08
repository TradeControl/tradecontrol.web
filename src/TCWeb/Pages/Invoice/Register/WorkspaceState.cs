namespace TradeControl.Web.Pages.Invoice.Register;

public enum WorkspaceMode
{
    Register,
    DetailGrid,
    DetailPanel,
    RaiseList,
    RaiseCreate,
    RaiseEdit,
    RaiseDetails,
    RaiseDelete,
    RaisePost,
    UpdateEdit,
    UpdateCreateItem,
    UpdateEditItem,
    UpdateDeleteItem,
    UpdateDeleteInvoice
}

public sealed class WorkspaceState<TKey>
{
    public WorkspaceMode Mode { get; set; }
        = WorkspaceMode.Register;

    public TKey? SelectedKey { get; set; }

    public string? SearchText { get; set; }

    public int CurrentPage { get; set; }

    public int RowsPerPage { get; set; } = 10;

    public bool HasSelection => SelectedKey is not null;
}
