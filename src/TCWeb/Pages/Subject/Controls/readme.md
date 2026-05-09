# Subject Controls

## NamespaceSelector

`NamespaceSelector.razor` is a Blazor input control for namespace-aware filtering and dot-notation completion of Subject namespaces.

It is intended to be hosted by a page or component that already knows how to:

- load namespace suggestions
- apply a namespace filter to a tree or result set
- react to committed selections

The control does **not** load data directly.

## Files

- `NamespaceSelector.razor` — UI control
- `NamespaceSelectorSuggestion.cs` — suggestion DTO passed in by the host

## Required host responsibilities

The host component must:

1. Provide the current filter text
2. Provide the current suggestion list
3. Provide loading state for suggestions
4. Handle `OnFilterChanged`
5. Handle `OnFilterCommitted`

Typical host logic:

- `OnFilterChanged`
  - update filter text
  - request suggestions
  - refresh filtered tree/results

- `OnFilterCommitted`
  - resolve the committed namespace path
  - select the nearest matching node
  - expand the tree to the selected path

See `Subject/Browser/SubjectBrowserShell.razor` for the current reference implementation.

## Parameters

- `FilterText`
- `Suggestions`
- `IsLoadingSuggestions`
- `OnFilterChanged`
- `OnFilterCommitted`
- `MultiSelect` (reserved for future use)

## Suggestion model

Each suggestion currently provides:

- `Segment`
- `FullPath`
- `HasChildren`
- `DisplayLabel`

`FullPath` is the structural identity used by the host.

## Behaviour summary

The control supports:

- debounced input
- keyboard navigation
- click selection
- dot-notation completion
- trailing-dot branch commit
- browser spellcheck disabled on the input

Examples:

- `BellMaker`
- `BellMaker.`
- `BellMaker.Foundry`
- `BellMaker.Foundry.`

When the input ends with `.`, the host may treat the text as a committed namespace branch rather than a leaf selection.

## Minimal usage example

``` html
<NamespaceSelector
    FilterText="@_namespaceFilter"
    Suggestions="@_namespaceSuggestions"
    IsLoadingSuggestions="@_isLoadingNamespaceSuggestions" 
    OnFilterChanged="HandleFilterChangedAsync" 
    OnFilterCommitted="HandleFilterCommittedAsync" 
/>
```

## Notes

- Namespace identity is based on `SubjectCode`
- Namespace paths use dot notation
- The control is UI-only; namespace semantics live in the host/service layer
- The current production usage is in `SubjectBrowserShell.razor`
