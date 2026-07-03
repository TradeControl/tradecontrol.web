# MudRazorEval – Reference Implementation Fix Plan

This document defines the required corrections to the MudRazorEval prototype.  
Coding assistants must follow these instructions exactly and without reinterpretation.

## General Principles and Contracts

Read the following documents before proceeding to code:

/add docs/specs/tc-design-principles.md  
/add docs/specs/tc-development-contract.md  

## 1. Add Prototype Files

Entire prototype folder:

/add src/TCWeb/Pages/Invoice/MudRazorEval/*

This is the reference implementation.  
All fixes below apply directly to these files.

## 2. Problems to Fix

1. Totals do not refresh when filters change (InvoiceRegisterGrid, InvoiceRegisterDetailsGrid).  
2. Details grid navigation bar is off‑screen due to fixed header height.  
3. Back button does not render using the active theme (InvoiceDetailPanel, InvoiceRegisterDetailsGrid).  
4. Back navigation returns to the full dataset instead of the preserved register state.  
5. Selected record is not visible when returning to the grids.  
6. Icons have no visible meaning (no tooltips or legend).  
7. Details grid shows a Back button even when embedded in the panel.

## 3. Tasks

### Phase 1 – Correctness

#### **Task 1 – Move state into the Shell**

Goal: Shell becomes the single source of truth.  

- Move selected invoice, filters, sort order, paging, and navigation state into `InvoiceRegisterShell`.  
- `InvoiceRegisterGrid` and `InvoiceRegisterDetailsGrid` become stateless view components.  
- Components receive state via parameters and raise events only.

#### **Task 2 – Preserve Register state**

Goal: Back returns to exactly where the user left.  
Preserve and restore:

- Selected row  
- Current filter  
- Sort order  
- Current page  
- (Optional) scroll position  

Fixes problems **4** and **5**.

#### **Task 3 – Refresh component lifecycle**  

Goal: Components reload correctly when parameters change.

- Replace `OnInitialized()` with `OnParametersSet()` where appropriate.  
- Extract `LoadData()` methods.  
- Recalculate totals after every data load.  

Fixes problem **1**.

### Phase 2 – Layout

#### **Task 4 – Responsive workspace layout**

Goal: Remove hard‑coded heights.

- Replace `calc(100vh - xxx)` with a flex layout.  
- Header auto‑sizes.  
- Grid fills remaining space with `flex: 1 1 auto; overflow: auto;`.

Fixes problem **2**.

#### **Task 5 – Navigation ownership**

Goal: Navigation belongs to the Shell.

- Add `ShowNavigation` parameter to details grid.  
- Embedded grids must not render Back buttons.  
- Shell decides when navigation controls appear.

Fixes problem **7**.

### Phase 3 – UI Consistency

#### **Task 6 – Standardise controls**

Goal: Use MudBlazor consistently.

- Replace Bootstrap buttons with `MudButton`.  
- Ensure theme classes apply correctly.  
- Standardise icons and create a shared icon helper.

Fixes problem **3**.

#### **Task 7 – Icon usability**

Goal: Users understand the icons.

- Add tooltips.  
- Optional legend component.  
- Consider MudChips later.

Fixes problem **6**.

### Phase 4 – Framework Extraction

#### **Task 8 – Create shared `RegisterState<T>` class**

Goal: Reusable register state container.

Includes:

- Filters  
- Sort  
- Paging  
- Selected row  
- (Optional) scroll position  

#### **Task 9 – Create shared Workspace Shell**

Goal: Generalise `InvoiceRegisterShell` into a reusable shell supporting:

- Register only  
- Register + Detail Grid  
- Register + Detail Panel  
- Split views  
- Future tabbed interface  

#### **Task 10 – Shared rendering helpers**

Goal: Extract common UI helpers.  
Includes:

- Invoice icons  
- Status icons  
- Common toolbar  
- Common totals  
- Common grid configuration  

## Architectural Principle

**Components should be stateless views wherever possible.  
The Shell owns application state; child components render it and raise events.  
No component should maintain navigation or business state that cannot be reconstructed by the Shell.**

## Appendix A - Engineering Clarifications

Use MudBlazor 9.5.0.

### 1. State ownership

Implement a shell-owned explicit state object.

Do not use MudDataGrid internal state APIs, even if they are available.

The Reference Implementation is intended to demonstrate architecture rather than MudBlazor-specific techniques.

The shell owns:

current filter
selected invoice
current mode
navigation history required for back navigation

Do not attempt to preserve sort order, page number or scroll position in this implementation.

### 2. Selected record

When returning from the detail view, restore the previously selected invoice.

Visual row highlighting is sufficient.

Do not attempt to restore scroll position or page position.

Those are future enhancements and outside the scope of this Reference Implementation.

### 3. Scope

Implement Phases 1–3 only.

Do not create shared infrastructure during this pass.

Do not create a shared workspace shell.

Do not extract common rendering helpers.

Those belong to a later refactoring once the Reference Implementation is complete.

### 4. Shared helpers

None required.

Keep all implementation inside the current Invoice Register module.

No shared framework should be introduced in this pass.

### 5. Overall principle

Prefer explicit implementation over abstraction.

Do not redesign.

Do not introduce generic infrastructure.

Do not optimise for future modules.

Produce a clean, working Reference Implementation.

Once the implementation is complete, refactoring opportunities will be reviewed separately.

## Appendix B – Acceptance Criteria

The following acceptance criteria define the required observable behaviour of the MudRazorEval Reference Implementation.

The implementation is complete when all criteria are satisfied.

### B1. Register State

The Invoice Register Shell is the single owner of application state.

The Shell preserves:

- current filter
- selected invoice
- current workspace mode

Child components act as views and do not own application state.

### B2. Filtering

Changing the register filter immediately updates:

- the visible rows
- the displayed totals
- the displayed row count

No additional user action is required.

PASS when all three values update simultaneously.

### B3. Details Navigation

Selecting a register row displays the associated details.

The details shown always correspond to the currently selected invoice.

Changing the selected invoice immediately refreshes the details.

### B4. Back Navigation

Given:

- a filtered register
- a selected invoice
- the details view is open

When the user selects **Back**:

- the workspace returns to the Register view
- the current filter is preserved
- the filtered register remains displayed
- the previously selected invoice remains selected
- the details view is closed

The user should perceive no loss of context.

### B5. Selected Row

The currently selected invoice is visually distinguishable from all other rows.

When returning from the details view, the same invoice remains visibly selected.

The implementation is not required to restore scrolling or pager position.

### B6. Register Totals

Register totals are calculated from the rows currently displayed after filtering.

Totals always correspond to the visible dataset.

### B7. Details Totals

Details totals are calculated from the rows currently displayed after filtering.

Totals always correspond to the visible dataset.

### B8. Layout

The register workspace occupies the available page height.

The grid resizes with the workspace.

The pager/navigation controls remain visually attached to the bottom edge of the grid and remain visible without overlapping other controls.

### B9. Navigation Controls

The standalone Details view displays a Back button.

The embedded Details panel does not display a Back button.

Navigation behaviour is identical regardless of how the Details component is hosted.

### B10. Icons

Every action icon has a visible meaning.

Users can determine the purpose of each icon without referring to external documentation.

### B11. Scope

This implementation remains entirely within the existing `TradeControl.Web.Pages.Invoice.MudBlazorEval` namespace.

No shared framework, generic workspace, or reusable helper infrastructure is introduced during this phase.

Such refactoring belongs to the subsequent Reference Implementation.

*End of mudrazor-eval.md*

