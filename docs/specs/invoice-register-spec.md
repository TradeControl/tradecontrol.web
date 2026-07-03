# Invoice Register — Behavioural Refactor Specification (Version 5)

**Trade Control Web — June 2026**

## Overview

### 1.1 Purpose

The Invoice Register is the single Blazor workspace for all invoice activity.

It replaces the legacy Razor Pages under `Pages/Invoice/*` with a modern Blazor implementation while preserving every existing business rule.

This project is a behavioural refactor.

It is **not** a redesign.

The completed module must behave exactly as the existing invoice subsystem while adopting the architecture demonstrated by the Reference Implementation.

### 1.2 Instructions

The following documents communicate the design principles and development contract that underpin the project.

docs/specs/tc-design-principles.md  
docs/specs/tc-development-contract.md

## 2. Design Intent

The completed Invoice Register provides a single coherent workspace for:

- Register
- Raise
- Edit
- Enquiry
- Submission

Desktop users work within a multi-pane workspace.

Mobile users work within a single-pane navigation model.

The Register is always the primary landing surface.

## 3. Behavioural Sources of Truth

There are exactly two authoritative sources.

### 3.1 Business Behaviour

The legacy Razor Pages define all functional behaviour.

They are authoritative for:

- business rules
- lifecycle
- posting
- editing
- creation
- cash-code aggregation
- email workflow
- security
- SQL interaction

Their presentation is deprecated.

Their behaviour is not.

### 3.2 UI Architecture

The Reference Implementation defines the UI architecture.

It is authoritative for:

- Shell ownership
- state management
- navigation
- rendering
- workspace layout
- DataGrid rendering
- scrolling behaviour
- desktop/mobile behaviour
- CSS layout contract

It is an executable specification.

Do not reinterpret it.

Extend it.

## 4. Cognitive Invariants

These invariants apply throughout the entire implementation.

### Invariant A — Behaviour First

- Preserve business behaviour.
- Replace presentation.
- Never rewrite business rules.

### Invariant B — Reference Implementation

The Reference Implementation is a verified baseline.

Assume it is already correct.

Do not redesign:

- DataGrid rendering
- workspace layout
- scrolling model
- flex layout
- CSS height contract
- embedded/mobile navigation

If a requested feature appears to require changing these behaviours:

STOP.

Explain the conflict.

### Invariant C — Shell Ownership

The Shell owns:

- state
- navigation
- filters
- selected invoice
- workflow

Components remain stateless.

### Invariant D — Business Layer

- The existing `Invoices` class remains authoritative.
- Business logic must not migrate into UI components.

### Invariant E — Services

The Shell communicates only through the service layer.

Components never communicate directly with:

- NodeContext
- Invoices
- stored procedures

### Invariant F — Behavioural Translation

- Translate behaviour.
- Do not reinterpret behaviour.
- Do not optimise behaviour.

### Invariant G — Stop Before Guessing

If implementation requires assumptions that cannot be verified from:

- the specification,
- the Reference Implementation,
- or the legacy implementation,

STOP.

Request clarification.

Never infer missing architecture.

### Invariant H — The Reference Implementation is executable specification

Any behaviour already demonstrated by the Reference Implementation (layout contract, DataGrid rendering, workspace sizing, scrolling, navigation, CSS relationships) is considered correct. Extend it; do not replace or reinterpret it. If a change appears necessary, treat it as a specification conflict and stop for guidance rather than modifying the underlying pattern.

### Invariant I — Preserve proven behaviour

If a subsystem is already working and satisfies the specification, treat it as a trusted baseline. Do not rewrite or optimise it in order to support another feature. If the requested feature appears to require changing the trusted baseline, stop and explain why before making any edits.

## 5. Architecture

(Existing architecture sections retained almost unchanged.)

- Business Layer
- EF Models
- Service Layer
- Shell
- Components
- UI Models
- Stylesheet

These define where responsibilities belong.

## 6. Expected User Experience

The completed module should feel like one coherent application.

Users should never be aware that the module originated from multiple Razor Pages.

Desktop:

- embedded workspace
- collapsible sidebar
- embedded detail
- summary always visible

Mobile:

- single-pane workflow
- back navigation
- card layout
- preserved Register state

## 7. Functional Scope

The completed module supports:

- Register
- Raise
- Edit
- Enquiry
- Submission

Each workflow preserves existing behaviour.

## 8. Implementation Phases

The following phases describe capability increments.

They do not prescribe implementation.

The implementation strategy is determined after repository inspection.

### Phase 1

Register Query

Expected outcome:

- Register queries through the service layer.
- Existing behaviour preserved.
- No visual regressions.

### Phase 2

UI Enhancements

- Desktop Collapsible Sidebar
    - Period Selector - App.tbYear, App.tbYearPeriod
    - Filters by CashCode, Date, Namespace
    - Legend
- Mobile
    - Single Pane Navigation
    - Back buttons

### Phase 3

Enquiry

Evolve InvoiceDetailPanel.razor

Expected outcome:

- Enquiry workflow integrated into the Shell.
- Navigation preserved.
- Register context restored on return.
- Change Log (Invoice_vwChangeLog)

### Phase 4

Raise

Expected outcome:

- Raise workflow migrated.
- Existing creation behaviour preserved.

### Phase 5

Edit

Expected outcome:

- Header editing migrated.
- Item editing migrated.
- Existing posting behaviour preserved.

### Phase 6

Submission

Expected outcome:

- HTML preview.
- Email workflow.
- Printed flag.
- Existing Mail Host behaviour preserved.

### Phase 7

Legacy Retirement

Expected outcome:

Legacy Razor Pages removed.

## 9. Mandatory Planning Cycle

Before every implementation phase:

The Model shall:

1. Inspect the repository.
2. Identify required files.
3. Identify dependencies.
4. Identify implementation risks.
5. Produce an implementation plan.
6. Estimate the number of tasks.
7. Identify missing information.
8. Wait for approval.

No code shall be modified before approval.

## 10. During Implementation

The Model may refine its implementation plan after repository inspection.

Changing the plan is encouraged when better information becomes available.

However:

If additional architectural changes become necessary:

STOP.

Explain why.

Wait for approval.

## 11. Completion Criteria

The implementation is complete when:

- all workflows exist
- business behaviour matches the legacy implementation
- Reference Implementation architecture is preserved
- desktop behaviour matches the executable baseline
- mobile behaviour matches the executable baseline
- state preservation works
- paging works
- sorting works
- summary totals work
- enquiry works
- raise works
- edit works
- submission works
- legacy Razor Pages have been retired

The completed module should appear to users as though it had always been designed as a single Blazor application.

## Appendix - Aider Files

aider --no-show-model-warnings --no-git

/add docs/specs/invoice-register-spec.md  
/add docs/specs/tc-design-principles.md  
/add docs/specs/tc-development-contract.md

/add src/TCWeb/Pages/Invoice/Enquiry/*  
/add src/TCWeb/Pages/Invoice/Update/*  
/add src/TCWeb/Pages/Invoice/Raise/*  

/add src/TCWeb/Pages/DI_BasePageModel.cs  
/add src/TCWeb/Data/Invoices.cs  
/add src/TCWeb/Data/NodeContext.cs  
/add src/TCWeb/Data/NodeContextProc.cs  
/add src/TCWeb/Data/NodeEnum.cs  
/add src/TCWeb/AppServices/ServiceCollectionExtensions.cs  

/add src/TCWeb/Models/Invoice_vwChangeLog.cs  
/add src/TCWeb/Models/Invoice_vwEntry.cs  
/add src/TCWeb/Models/Invoice_vwRegister.cs  
/add src/TCWeb/Models/Invoice_vwRegisterDetail.cs

/add src/TCWeb/wwwroot/css/base.css

/add src/TCWeb/Pages/Invoice/Register/*  
/add src/TCWeb/wwwroot/css/modules/invoiceRegister.css
/add src/TCWeb/AppServices/InvoiceRegister/*
