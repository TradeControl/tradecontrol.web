# Engineering Work Plan for Phase 2

## 1. Objective

Implement the Phase 2 UI enhancements for Invoice Register without changing established Reference Implementation behaviour:

- move the demo filter and legend out of `InvoiceRegisterGrid.razor`
- introduce a collapsible sidebar for desktop
- introduce a mobile navigation/filter pane
- make financial period the primary filter
- default initial state to current financial year and current period
- preserve shell ownership of state and navigation
- keep grid and child components presentation-only
- retain service-layer-only communication from the shell

## 2. Scope for this phase

In scope:

- desktop collapsible sidebar
- mobile single-pane navigation/filter experience
- financial year and period selectors
- namespace filter input using `NamespaceSelector`
- cash code filter input/select
- move legend into sidebar/filter surface
- move the free-text filter into the sidebar/filter surface
- service-backed period initialisation and register filtering
- preserve register state when navigating between register and detail views
- support the three Invoice Register presentation views:
  - invoice headers from `Invoice.vwRegister`
  - invoice lines from `Invoice.vwRegisterDetails`
  - cash code summaries from `Invoice.vwRegisterCashCode`

Out of scope for now:

- Enquiry enhancements
- Raise/Edit/Submission workflows
- deeper namespace semantics beyond wiring the control
- advanced date filtering unless already needed by the service
- redesign of grid rendering or layout contract
- shared cross-module abstraction between Cash Manager and Invoice Register

## 3. Repository cues and architectural fit

From the supplied files and spec:

- `InvoiceRegisterShell.razor` already owns state and navigation, which is correct.
- `InvoiceRegisterGrid.razor` currently contains:
  - the free-text filter
  - the legend
  - paging state plumbing
- `CashManagerService`, `CashManagerShell.razor`, `CashManagerModels.cs`, and `CashManagerTree.razor` demonstrate the established Trade Control pattern for:
  - loading current year/period
  - excluding archived years
  - supporting `All years`
  - supporting `All months in year`
  - maintaining selected year/period in the shell
  - switching desktop/mobile layouts
- `NamespaceSelector.razor` is explicitly UI-only and must be hosted by a shell that owns filter state.
- `SubjectSearchShell.razor` demonstrates the expected host integration pattern for `NamespaceSelector`.
- `CashPaymentsWorkspaceService.cs` demonstrates the expected namespace/DAG resolution pattern based on `ParentSubjectCode` and `SubjectCode`.
- `InvoiceFilterModel` already supports:
  - `PeriodYear`
  - `PeriodMonth`
  - `Namespace`
  - `CashCode`
  - `SearchText`
- `InvoiceRegisterResult` already supports grid/detail payloads.
- `InvoiceRegisterService` and `InvoiceRegisterQueryBuilder` already provide the correct Phase 1 service/query boundary.
- `Invoice_vwRegister`, `Invoice_vwRegisterDetail`, and `Invoice_vwRegisterCashCode` each expose `StartOn`, which is the accounting period key and corresponds to `tbYearPeriod.StartOn`.
- `Invoice_vwRegister` and `Invoice_vwRegisterDetail` expose the invoice DAG relationship needed for namespace construction through `ParentSubjectCode` and `SubjectCode`.
- `base.css` and theme instances such as `theme-blue.css` define the existing theme contract and should be used for navigation pane styling rather than introducing isolated colour rules.

This means Phase 2 should extend the shell and add focused UI components, not redesign the register and not modify Cash Manager.

## 4. Required implementation approach

### 4.1 Shell remains the single owner

`InvoiceRegisterShell.razor` should own:

- selected workspace mode
- selected invoice
- current year
- current period
- namespace filter text
- cash code filter
- free-text filter
- selected register view
- sidebar open/closed state
- mobile pane state
- current register result
- reference data for year/period/cash code options
- namespace suggestions

Child components should only:

- receive state
- render
- raise events

### 4.2 Introduce dedicated sidebar/filter UI

Create a new component, likely:

- `src\TCWeb\Pages\Invoice\Register\Components\InvoiceRegisterSidebar.razor`

Responsibilities:

- render financial year selector
- render period selector
- render free-text filter
- render namespace selector
- render cash code selector
- render the three view toggle buttons:
  - Headers
  - Lines
  - Cash Codes
- render legend
- render mobile/desktop appropriate presentation
- use the application theme contract for navigation pane styling
- raise explicit events:
  - `OnYearSelected`
  - `OnPeriodSelected`
  - `OnSearchTextChanged`
  - `OnNamespaceFilterChanged`
  - `OnNamespaceFilterCommitted`
  - `OnCashCodeChanged`
  - `OnViewChanged`
  - optionally `OnClearFilters`

Default input expectation:

- the free-text search box must initialise empty for the user
- the namespace selector must initialise empty for the user
- user-facing guidance may be provided through placeholder/ghost text only
- internal field names such as `_searchText` and `_namespaceFilter` must never be surfaced as default visible values

It must not query data directly.

### 4.3 Keep the grid focused on data presentation

Refactor `InvoiceRegisterGrid.razor` so that it no longer owns:

- legend
- free-text filter
- top-level filtering controls that belong to module state rather than grid display

The grid should retain only:

- tabular invoice display
- row actions
- paging
- column filtering if already part of the baseline
- visible-row total calculations

This keeps the grid presentation-only and aligns with shell-owned state.

### 4.4 Present three workspace views from the same shell

The Invoice Register must present three RHS workspace views:

- `Invoice.vwRegister` for invoice headers
- `Invoice.vwRegisterDetails` for invoice lines
- `Invoice.vwRegisterCashCode` for cash code summarisation by line, for example Sales

Expected behaviour:

- the sidebar/mobile navigation pane exposes three toggle buttons for the active view
- the RHS workspace loads according to the selected view
- namespace filtering applies to invoice headers and invoice lines
- namespace filtering does not apply to the cash code summary view
- period, cash code, invoice type, and free-text filters continue to apply according to the underlying dataset semantics

## 5. Service-layer implementation approach

The current codebase already has a module-specific service structure:

- `IInvoiceRegisterService`
- `InvoiceRegisterService`
- `IInvoiceRegisterQueryBuilder`
- `InvoiceRegisterQueryBuilder`
- `IInvoiceFormattingService`
- `InvoiceFormattingService`

Phase 2 should extend the Invoice Register service layer in the same module, without introducing cross-module abstractions and without editing Cash Manager code.

Recommended additions:

- lookup methods for available years
- lookup methods for periods by year
- lookup method for default/current period
- lookup method for cash code options when the model is supplied
- no data access from UI components

Recommended shape:

- keep `IInvoiceRegisterService` for register data queries
- add a dedicated lookup service, for example `IInvoiceRegisterLookupService`, if doing so keeps the query contract cleaner

Alternative acceptable shape:

- extend `IInvoiceRegisterService` directly with lookup methods if that better matches the current module style

Decision note:
Either option is acceptable, but introducing a small lookup service is cleaner because:

- register query service = result payloads
- lookup service = filter reference data and defaults

## 6. Financial period behaviour

Financial period behaviour is now confirmed and should mirror Cash Manager.

### 6.1 Source of truth

Use:

- `App_Periods` / year-period data for available years and periods
- `vwRegister*.StartOn` for register filtering

`StartOn` in:

- `Invoice_vwRegister`
- `Invoice_vwRegisterDetail`
- `Invoice_vwRegisterCashCode`

corresponds to `tbYearPeriod.StartOn`.

### 6.2 Initial load

On first shell load:

- load available years excluding archived years
- load default/current period using the Cash Manager approach
- set selected year and selected period from that current period
- apply the filter immediately to the active register query

### 6.3 Year selection

Changing year should:

- reload valid periods for that year
- support `All years` using the Cash Manager pattern
- if a selected period is no longer valid, fall back to:
  - no period selected for `All years`
  - or the first valid period for a concrete year, matching the Cash Manager behaviour
- reload the register query

### 6.4 Period selection

Changing period should:

- support `All months in year`
- update `InvoiceFilterModel.PeriodYear`
- update `InvoiceFilterModel.PeriodMonth`
- reload the register query

Expected filter mapping:

- `All years`:
  - no year filter
  - no month filter
- specific year + `All months in year`:
  - year filter only
  - no month filter
- specific year + specific period:
  - year filter
  - month filter derived from the selected period

### 6.5 Query builder impact

`InvoiceRegisterQueryBuilder` currently filters with:

- year + month together
- or `DateFrom`

This will need to be adjusted so Phase 2 supports:

- all years
- year-only
- year + month

while preserving the existing `StartOn`-based accounting period semantics.

## 7. Namespace filter behaviour

Namespace filter behaviour is now defined more explicitly by the invoice DAG requirement.

Use the host integration pattern demonstrated in `SubjectSearchShell.razor`:

- the shell owns namespace text
- the shell loads suggestions
- the shell passes `FilterText`, `Suggestions`, and loading state into `NamespaceSelector`
- `NamespaceSelector` raises:
  - `OnFilterChanged`
  - `OnFilterCommitted`

Default input state:

- the namespace selector must initialise empty for the user
- placeholder/ghost text may be used for guidance
- `_namespaceFilter` must not appear as a visible default value

Invoice namespace semantics:

- invoices are a DAG
- the namespace path can be constructed from `ParentSubjectCode` and `SubjectCode` in `Invoice.vwRegister*`
- the implementation approach should follow the demonstrated pattern in `CashPaymentsWorkspaceService.cs`
- namespace filtering should be applied using the extracted `ParentSubjectCode`, `SubjectCode` pair rather than relying on a simple subject-name text match
- this requirement applies to:
  - `Invoice_vwRegister`
  - `Invoice_vwRegisterDetail`
- it does not apply to `Invoice_vwRegisterCashCode`

Recommended Phase 2 behaviour:

- `OnFilterChanged`
  - update shell state
  - refresh suggestions
  - do not immediately query the register unless clearing the filter should restore results immediately
- `OnFilterCommitted`
  - update shell state
  - reload the register

This keeps namespace behaviour explicit and aligned with the existing selector contract.

## 8. Cash code filter behaviour

Cash code filtering is part of Phase 2. Use `Cash_vwCodeLookup`for the dropdown .

Confirmed constraints:

- this should be implemented through the Invoice Register service layer
- enabled cash codes only
- UI belongs in the sidebar/filter pane
- do not modify Cash Manager code

Recommended interim approach:

- prepare the sidebar and shell contracts now so the cash code selector can be connected with minimal change
- prefer a dropdown/select for Phase 2 unless later requirements demand richer search behaviour

## 9. Mobile interaction model

Mobile behaviour is now confirmed.

Recommended implementation:

- mobile lands on the navigation/filter pane first
- register view is entered from that pane
- details continue to use explicit back navigation
- register state is preserved when moving between filter pane, register, and detail modes
- the navigation/filter pane must include the three view toggle buttons
- the navigation/filter pane must be theme-enabled using the application theme variables

Suggested shell state:

- desktop:
  - sidebar visible/collapsed beside the main workspace
- mobile:
  - `Navigation`
  - `Register`
  - `DetailGrid`
  - `DetailPanel`
  - active dataset view within the register workspace

Long-press note:

- long-press for context actions could be added later if needed
- do not include it in the initial Phase 2 implementation unless a concrete mobile action gap appears during build/testing
- simple explicit buttons are preferable for predictable UX at this stage

## 10. Desktop interaction model

Recommended desktop layout:

- left collapsible sidebar
- main register/detail workspace on the right
- sidebar collapse toggle in shell header
- modern, clean panel styling using the existing MudBlazor and module CSS patterns
- preserve existing embedded grid scroll behaviour
- include three view toggle buttons in the sidebar
- ensure the navigation/sidebar pane is theme-enabled using the existing CSS theme contract in `base.css`

Important constraint:

Do not alter the core register/grid height and scroll contract established by the Reference Implementation. The sidebar should wrap around the existing shell layout rather than forcing a new scroll model.

## 11. Suggested implementation tasks

### Task 1 — Add Phase 2 lookup models

Create focused UI/service models, likely:

- `InvoiceRegisterYearOption`
- `InvoiceRegisterPeriodOption`
- `InvoiceRegisterSelectOption`
- `InvoiceRegisterViewMode` or equivalent module-specific view selector type

These should follow the style used in `CashManagerModels.cs` without introducing shared abstractions.

### Task 2 — Add Invoice Register lookup service support

Implement lookup methods for:

- years excluding archived years
- periods by year
- default/current period
- cash code options when the cash code model is available

Use `CashManagerService` as the behavioural template, but implement this within the Invoice Register module.

### Task 3 — Adjust query builder for All Years / All Months support and DAG namespace filtering

Update `InvoiceRegisterQueryBuilder` so `StartOn` period filtering supports:

- all years
- year only
- year and month

Also update it so namespace filtering for headers and lines uses the extracted `ParentSubjectCode`, `SubjectCode` relationship derived from the invoice DAG, following the `CashPaymentsWorkspaceService.cs` pattern.

This must preserve invoice period semantics already exposed by the views.

### Task 4 — Add sidebar component

Create:

- `src\TCWeb\Pages\Invoice\Register\Components\InvoiceRegisterSidebar.razor`

Responsibilities:

- year selector
- period selector
- free-text filter
- namespace selector
- cash code selector
- three view toggle buttons
- legend
- desktop/mobile presentation
- theme-enabled navigation pane styling
- event-only contract back to the shell
- ensure free-text and namespace inputs initialise empty, with optional placeholder text only

### Task 5 — Refactor shell layout and state

Update `InvoiceRegisterShell.razor` to:

- host sidebar + workspace for desktop
- host navigation/filter pane first on mobile
- manage desktop collapse state
- manage mobile pane/view state
- manage active register dataset view
- initialise current year/current period
- load namespace suggestions
- bind all filter events
- preserve current detail navigation logic
- ensure the free-text box is empty on initial user view
- ensure the namespace selector is empty on initial user view
- remove any behaviour that surfaces internal field names such as `_searchText` or `_namespaceFilter` as visible defaults

### Task 6 — Refactor grid and add the remaining view surfaces

Update `InvoiceRegisterGrid.razor` to remove:

- legend
- free-text filter

Retain:

- data display
- actions
- paging
- totals

Also provide the corresponding RHS presentation surfaces for:

- invoice line view using `vwRegisterDetails`
- cash code summary view using `vwRegisterCashCode`

### Task 7 — Integrate namespace selector

Use `SubjectSearchShell.razor` as the host template.

Shell responsibilities:

- maintain namespace filter text
- request suggestions
- pass selector parameters
- reload register on commit

Query responsibilities:

- apply namespace filtering using `ParentSubjectCode` and `SubjectCode`
- do not apply namespace filtering to the cash code summary view

UI expectation:

- the selector starts empty
- guidance is provided through placeholder/ghost text, not internal variable names

### Task 8 — Integrate cash code selector

When the cash code model is added:

- load enabled cash codes through the Invoice Register service layer
- bind selected value in the shell
- reload register on selection change/commit

### Task 9 — Mobile and themed navigation polish

Verify:

- first mobile surface is navigation/filter
- transition into register is explicit and clear
- back navigation restores the prior mobile state
- detail modes remain predictable
- the three view toggles are present and understandable
- the navigation pane responds correctly to the active application theme

### Task 10 — Validation

Verify:

- initial load defaults to current year and current period
- free-text search box is empty on first user view
- namespace selector is empty on first user view
- any guidance for those inputs is placeholder/ghost text only
- `_searchText` is not surfaced as a visible default
- `_namespaceFilter` is not surfaced as a visible default
- `All years` works
- `All months in year` works
- invoice DAG namespace filtering works for headers and lines
- cash code summary ignores namespace filtering
- three-view switching works correctly
- grid reflects period filter immediately
- paging still works
- detail navigation preserves filter state
- desktop collapse works
- mobile navigation/filter-first workflow works
- navigation pane styling follows theme variables
- no component queries services directly

## 12. Estimated number of tasks

10 implementation tasks, as listed above.

Suggested delivery sequence:

- Part A: lookup models and service support
- Part B: shell state and period filtering
- Part C: sidebar and grid refactor
- Part D: three-view integration, theme enablement, mobile behaviour, and final validation

## 13. Implementation risks

### Risk 1 — Period filter regression

Effect:

- invoices may appear outside the selected accounting period
- `All years` / `All months in year` may behave differently from Cash Manager

Mitigation:

- mirror the Cash Manager lookup/default logic
- use `vwRegister*.StartOn`
- validate all three filter modes explicitly

### Risk 2 — Namespace DAG filtering regression

Effect:

- invoice headers or lines may be included/excluded incorrectly
- namespace filtering may not reflect the real `ParentSubjectCode`/`SubjectCode` path

Mitigation:

- derive namespace semantics from the invoice DAG rather than text search
- follow the proven `CashPaymentsWorkspaceService.cs` resolution pattern
- validate headers and lines independently from cash code summaries

### Risk 3 — Sidebar layout could break scroll contract

Effect:

- nested scrolling or clipped grids

Mitigation:

- preserve existing shell/grid container structure and adapt around it rather than replacing it

### Risk 4 — Namespace reload semantics could feel noisy

Effect:

- unnecessary register reloads while the user types

Mitigation:

- load suggestions on change
- reload register on commit
- only reload on clear if needed for expected UX

### Risk 5 — Mobile mode transitions could become confusing

Effect:

- unclear transitions between navigation/filter, register, detail grid, detail panel, and dataset views

Mitigation:

- model mobile pane/view state explicitly in the shell
- keep back actions explicit and mode-specific
- keep the three view toggles consistently located in the navigation surface

### Risk 6 — Theme enablement drift

Effect:

- the navigation pane may not follow the active site theme
- module-specific CSS may diverge from the application theme contract

Mitigation:

- use existing variables from `base.css`
- validate against a themed instance such as `theme-blue.css`
- avoid hard-coded colours for navigation pane surfaces

## 14. Remaining information intentionally deferred

The main remaining deferred item is:

1. Cash code option model

- when the `Cash_tbCode` model is supplied, add enabled cash code lookup support in the Invoice Register service layer

Everything else needed to begin Phase 2 planning is now sufficiently defined.

## 15. Recommended implementation decisions

Based on the confirmed requirements, proceed with these decisions:

- default load = current year + current period
- desktop = collapsible left sidebar
- mobile = navigation/filter pane first
- free-text filter moved from grid into sidebar and initialised empty for the user
- namespace selector hosted by the shell using the `SubjectSearchShell` pattern and initialised empty for the user
- placeholder/ghost text may be used for both inputs
- internal names such as `_searchText` and `_namespaceFilter` must not be shown to users
- period handling mirrors Cash Manager
- archived years excluded
- `All years` supported
- `All months in year` supported
- invoice namespace filtering uses the DAG relationship from `ParentSubjectCode` and `SubjectCode`
- cash code summary view is not namespace-filtered
- the RHS workspace supports three presentation views:
  - headers
  - lines
  - cash codes
- the sidebar/mobile navigation pane exposes three view toggle buttons
- the navigation pane is theme-enabled using the existing `base.css` theme contract
- cash code filter prepared now and completed when the model is added
- grid remains client-paged over the full filtered dataset
- no Cash Manager module files are modified

## 16. Approval gate

Phase 2 planning is now updated and aligned with the supplied repository files and clarifications.

Next step after approval:

- implement Part A:
  - lookup models
  - lookup service support
  - period-filter query updates
  - DAG-aware namespace filtering
  - shell state initialisation

## Appendix 1 - Phase 2 Continuation

2026-07-04

Summary of where we are

1. Phase 1 is complete enough for the current baseline:
   - Invoice Register now queries through the service layer.
   - Shell owns state and navigation.
   - Detail views are integrated through the shell.

2. Phase 2 is largely in place:
   - desktop sidebar exists
   - mobile navigation/filter-first flow exists
   - current year/current period defaulting exists
   - `All years` / `All months in year` support exists
   - namespace selector is hosted by the shell
   - cash code dropdown exists
   - invoice type filter exists
   - clear filters action exists
   - free-text filter and legend have been moved out of the grid

3. The current implementation compiles and executes successfully, so we now have a working Phase 2 foundation rather than a planning state.

Where we are in the task list

1. Task 1 — lookup models: done
2. Task 2 — lookup service support: done
3. Task 3 — query builder period support: partially done
4. Task 4 — sidebar component: done
5. Task 5 — shell layout and state: mostly done
6. Task 6 — grid refactor: partially done
7. Task 7 — namespace selector integration: partially done
8. Task 8 — cash code selector integration: done for current lookup source
9. Task 9 — mobile behaviour polish: partially done
10. Task 10 — validation and refinement: still open

Most likely remaining Phase 2 work tomorrow
These are the areas I would expect us to refine next:

1. Desktop layout polish
   - verify sidebar/grid sizing against the Reference Implementation
   - ensure no wrapping/stacking regressions under all container sizes
   - confirm theme variables are being used consistently in the navigation pane

2. Filter behaviour refinement
   - confirm invoice type filtering semantics are exactly right
   - confirm cash code filter is filtering the intended datasets
   - confirm free-text input is empty on first render and uses only appropriate user-facing ghost text if needed
   - confirm namespace selector is empty on first render and uses only appropriate user-facing ghost text if needed

3. Data/query correctness
   - implement real DAG-aware namespace filtering from `ParentSubjectCode` and `SubjectCode`
   - check whether header-level cash code filtering is currently too weak
   - verify detail and summary totals always match the visible filtered dataset
   - verify selected header loading should or should not respect the active filter context
   - ensure cash code summary remains independent of namespace filtering

4. Three-view integration
   - add explicit toggle controls for Headers / Lines / Cash Codes
   - load the RHS according to the selected view
   - decide whether detail navigation semantics need small adjustments when the active base view is Lines or Cash Codes

5. Mobile polish
   - review navigation/back behaviour between:
     - navigation pane
     - register
     - detail grid
     - detail panel
     - active dataset view

6. Final Phase 2 validation
   - paging
   - sorting
   - totals
   - all years/months behaviour
   - default period reset
   - sidebar collapse behaviour
   - empty-state behaviour
   - namespace suggestion behaviour
   - theme compliance for the navigation pane (see base.css and theme-blue.css example)

Important likely issues to review first tomorrow

1. `InvoiceRegisterQueryBuilder.ApplyHeaderFilters()`
   - current cash code filtering at header level is only:
     - `query = query.Where(x => x.InvoiceNumber != null && x.InvoiceNumber.Length > 0);`
   - that is clearly only a placeholder-style filter and probably not the final intended behaviour

2. Namespace filtering
   - current namespace filtering appears to rely on simple subject matching
   - this should be replaced with DAG-aware filtering based on `ParentSubjectCode` and `SubjectCode`, using the `CashPaymentsWorkspaceService.cs` approach as the reference

3. View surface completeness
   - headers are present
   - lines/details are present
   - cash code summary view and view toggles still need to be completed as first-class workspace behaviour

Files you should add to the chat tomorrow
To let me pick up seamlessly, please add these files again as the authoritative current versions.

Highest priority files

1. `src/TCWeb/Pages/Invoice/Register/InvoiceRegisterShell.razor`
2. `src/TCWeb/Pages/Invoice/Register/Components/InvoiceRegisterSidebar.razor`
3. `src/TCWeb/Pages/Invoice/Register/Components/InvoiceRegisterGrid.razor`
4. `src/TCWeb/AppServices/InvoiceRegister/InvoiceRegisterQueryBuilder.cs`
5. `src/TCWeb/AppServices/InvoiceRegister/InvoiceRegisterService.cs`
6. `src/TCWeb/AppServices/InvoiceRegister/InvoiceRegisterLookupService.cs`
7. `src/TCWeb/AppServices/InvoiceRegister/IInvoiceRegisterLookupService.cs`
8. `src/TCWeb/Pages/Invoice/Register/Models/InvoiceRegisterLookupModels.cs`
9. `src/TCWeb/Pages/Invoice/Register/Models/InvoiceFilterModel.cs`
10. `src/TCWeb/Pages/Invoice/Register/Models/InvoiceRegisterResult.cs`

Also useful
11. `src/TCWeb/AppServices/InvoiceRegister/IInvoiceRegisterQueryBuilder.cs`
12. `src/TCWeb/AppServices/InvoiceRegister/IInvoiceRegisterService.cs`
13. `src/TCWeb/AppServices/InvoiceRegister/IInvoiceFormattingService.cs`
14. `src/TCWeb/AppServices/InvoiceRegister/InvoiceFormattingService.cs`
15. `src/TCWeb/AppServices/ServiceCollectionExtensions.cs`

Models likely needed again
16. `src/TCWeb/Models/Invoice_vwRegister.cs`
17. `src/TCWeb/Models/Invoice_vwRegisterDetail.cs`
18. `src/TCWeb/Models/Invoice_vwRegisterCashCode.cs`
19. `src/TCWeb/Models/Cash_vwCodeLookup.cs`

UI/reference files worth adding
20. `src/TCWeb/Pages/Invoice/Register/Components/InvoiceDetailPanel.razor`
21. `src/TCWeb/Pages/Invoice/Register/Components/InvoiceRegisterDetailsGrid.razor`
22. `src/TCWeb/Pages/Invoice/Register/WorkspaceState.cs`
23. `src/TCWeb/Pages/Invoice/Register/Index.cshtml`
24. `src/TCWeb/Pages/Invoice/Register/Index.cshtml.cs`

Reference/support files that help me avoid guessing
25. `src/TCWeb/Pages/Subject/Controls/NamespaceSelector.razor`
26. `src/TCWeb/Pages/Subject/Controls/NamespaceSelectorSuggestion.cs`
27. `src/TCWeb/Pages/Subject/Controls/readme.md`
28. `src/TCWeb/Pages/Subject/Components/SubjectSearchShell.razor`
29. `src/TCWeb/Pages/Cash/Manager/CashManagerShell.razor`
30. `src/TCWeb/Pages/Cash/Manager/Components/CashManagerTree.razor`
31. `src/TCWeb/Pages/Cash/Manager/Components/CashManagerModels.cs`
32. `src/TCWeb/AppServices/CashManagerService.cs`
33. `src/TCWeb/AppServices/ICashManagerService.cs`

Specs/docs to re-add
34. `docs/specs/invoice-register-spec.md`
35. `docs/specs/tc-design-principles.md`
36. `docs/specs/tc-development-contract.md`
37. `docs/tmp/session-brief.md`

