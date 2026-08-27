# A. Cash Manager — Namespace Rewrite & Cognitive Core (v0.3)

## Context and Purpose

This directive instructs the Model to reorganise the existing Cash subsystem into a single, coherent Blazor module that aligns with the modern Subject‑Namespace architecture. The legacy Razor Pages under `Cash\AssetEntry`, `Cash\PaymentEntry`, `Cash\Statement`, `Cash\Transfer`, and `Subject\CashAccount` provide a functional baseline only; their presentation and structure are deprecated.

The Model must extract and preserve all behavioural logic, spool semantics, posting rules, SvD balancing, CashPolarity behaviour, and stored‑procedure interactions, while discarding the legacy UI. A new namespace, `Cash\Manager`, must be created to contain the unified Cash Manager module.

The Cash Manager must adopt the same UI/UX patterns as the existing Blazor tree modules, using the Namespace Filter to resolve Subject context and DAG parentage for all economic events. Payments and invoices must be DAG‑enabled via `ParentSubjectCode`, ensuring that SvD balancing, FIFO invoice matching, and auto‑invoice creation occur within the correct semantic context.

Posting is session‑based and flushes the spool on demand. The Statement is the primary view and must present a unified, namespace‑aware representation of Posted and Unposted transactions, including running totals and provisional balances.

The purpose of this directive is to provide the Model with the conceptual framework required to reinterpret the legacy Cash subsystem and emit a modern, coherent Cash Manager that is semantically aligned with the DAG‑enabled Subject architecture.

# A. Project Directive

## 1. Namespace Re‑Write

### 1.1 Create New Namespace

Create a new namespace:

```text
Cash\Manager
```

This becomes the **single authoritative module** for all Cash UI/UX.

### 1.2 Absorb and Replace Legacy Namespaces

Extract functional logic from the following namespaces, then delete their contents:

```text
Cash\AssetEntry
Cash\PaymentEntry
Cash\Statement
Cash\Transfer
Subject\CashAccount
```

Rules:

- Preserve **behavioural logic**, **stored procedure calls**, **validation**, **spool semantics**, **posting logic**, **SvD balancing**, **CashPolarity logic**, **security rules**.
- Discard all Razor Pages and legacy UI.
- Rebuild UI using Blazor components under `Cash\Manager`.

---

## 2. Backend Upgrade — DAG Enablement

### 2.1 Add ParentSubjectCode

Add `ParentSubjectCode` to:

```text
Cash.tbPayment
Invoice.tbInvoice
Project.tbProject
```

Rules:

- Nullable.
- FK to `Subject.tbSubject(SubjectCode)`.
- This is the DAG anchor for all economic events.
- `Project.tbProject` is outside the immediate UI scope of `Cash\Manager`, but the schema should still be extended now.
- Where a DAG parent can already be resolved at project creation time, `Project.tbProject.ParentSubjectCode` should be populated and then left available for future workflow development.

### 2.2 DAG Resolution Ordering

DAG parent must be resolved **before** polarity or SvD logic.

Order:

1. Resolve DAG parent  
   - If Subject has one DAG parent → assign automatically.  
   - If multiple → require Namespace Filter selection.  
   - Store in `ParentSubjectCode`.

2. Resolve polarity  
   - If `CashCode` exists → derive polarity via `Category → CashPolarity`.  
   - If `CashCode` null → search invoices FIFO.  
   - If no invoice → auto‑create miscellaneous invoice with same `CashCode`.

3. Apply SvD balancing  
   - Payment settles invoice, or  
   - Payment creates invoice.  
   - Both share the same `ParentSubjectCode`.

### 2.3 Update Procedures

Update all procedures to use `ParentSubjectCode`:

```text
proc_PaymentAdd
proc_PaymentPost
proc_PaymentPostInvoiced
proc_TxPayIn
proc_TxPayOutInvoice
```

Rules:

- FIFO invoice matching must occur **within the same DAG parent**.
- Auto‑invoice creation must inherit the payment’s `ParentSubjectCode`.

---

## 3. Polarity Logic (Existing Behaviour, DAG‑Aware)

### 3.1 Polarity Determination

Polarity is determined by:

```text
tbPayment.CashCode → tbCategory.CashCode → CashPolarity { Expense, Income, Neutral }
```

### 3.2 Missing CashCode

If `CashCode` is null:

1. Search for invoices FIFO (same DAG parent).  
2. If none found → auto‑create invoice:  
   - Inherit `CashCode` from payment.  
   - Inherit `ParentSubjectCode` from payment.

This maintains SvD balance.

---

## 4. Posting Model — Session‑Based

### 4.1 Posting is a Spool Flush

Posting is **not** tied to:

- App.tbYearPeriod  
- Calendar months  
- Bank statement periods  

Posting is **session‑based**:

- User may post one day, one week, one month, six months, or any arbitrary set of unposted transactions.
- Posting flushes the spool:  
  ```text
  Unposted → Posted
  ```

### 4.2 Period Selector

- Period selector is for **viewing only**.
- Posting is independent of the selected period.

---

## 5. Blazor UI/UX Requirements (Cash\Manager)

### 5.1 Statement as Landing View

The Statement must:

- Display Posted + Unposted transactions.
- Show running totals.
- Show Provisional Balance (Posted + Unposted).
- Group by `ParentSubjectCode` (namespace).
- Use Namespace Filter for context.
- Support reconciliation before posting.

### 5.2 Namespace Filter

Namespace Filter is **mandatory** for all transaction entry.

- Replaces all legacy Subject dropdowns.
- Resolves DAG parent.
- Auto‑assign if unique; otherwise user selects.

### 5.3 UI Adapts to CashAccountType

Rules:

- `Cash` → Payments + Transfers
- `Asset` → Asset Entries
- `Dummy` → Payments

### 5.4 Legacy UI is Discarded

- Do not wrap Razor Pages.
- Do not migrate Razor Pages.
- Rebuild UI using Blazor components consistent with the Subject Browser.

### 5.5 Posting Button

- “Post unposted transactions”.
- Flushes spool.
- Uses existing posting procedures.

---

## 6. Cognitive Invariants

### Invariant A — DAG First

All economic events must be DAG‑anchored **before** polarity or SvD logic.

### Invariant B — Polarity from CashPolarity

Polarity is determined **exclusively** by `CashPolarity`, not entity type.

### Invariant C — Session‑Based Posting

Posting is a spool flush, not a period close.

### Invariant D — Modern Blazor UI

Legacy UI is discarded; behaviour is preserved; presentation is replaced.

---

# Summary Directive

```text

Preserve behaviour.
Replace presentation.
DAG‑anchor everything.
Use Namespace Filter for context.
Flush the spool on demand.
Build a single coherent Cash Manager.
```

# B. Cash Manager UI/UX Design Specification

## 1. Design Intent

The `Cash\Manager` module is the single Blazor surface for all Cash workflows. It replaces the legacy Razor Page split between Payment Entry, Asset Entry, Statement, Transfer, and Cash Account maintenance with one tree-aligned manager experience.

The design must follow the same core interaction model as the existing Blazor tree modules:

- split-pane desktop layout
- embedded/mobile back-navigation pattern
- lightweight action toolbar
- right-hand detail/work surface
- `NamespaceSelector`-driven context resolution
- service-backed state with minimal page reloads

The Statement is the landing view and the operational centre of the module.

## 2. Core UX Principles

- Statement-first: users land on a live cash statement, not on a form.
- Namespace-first: `ParentSubjectCode` is resolved before the user can save or post.
- Session-first: posting flushes the current unposted spool on demand.
- Account-aware: visible actions adapt to `CashAccountType`.
- Blazor-native: no iframe wrappers and no Razor Page reuse.
- Behaviour-preserving: all posting, SvD, FIFO, polarity, invoice settlement, and auto-invoice semantics remain server-owned.

## 3. Shell and Navigation Model

### 3.1 Host Structure

`Cash\Manager` should follow the established shell pattern used by `Admin.Manager` and `Subject.Browser`.

Desktop layout:

- left pane: account tree, namespace filter, workflow shortcuts, session summary
- right pane: active workspace
- persistent top header within the right pane for title, account badge, namespace badge, and primary actions

Mobile/embedded layout:

- single-surface detail view
- back button at top
- compact namespace selector
- bottom action bar for the current account/workflow
- statement list and forms rendered as stacked cards rather than wide tables

### 3.2 Navigation Nodes

The left-side navigation is account-centric, not task-centric.

Recommended tree:

- `Cash Manager`
  - `Statements`
  - `Accounts`
    - `<Cash Account>`
      - `Statement`
      - `Payments`
      - `Assets`
      - `Transfers`
      - `Posting`

Rules:

- selecting an account opens its Statement by default
- the right pane changes mode without full navigation
- deep links may open directly into `Statement`, `Payment`, `Asset`, `Transfer`, or `Posting`
- the current namespace filter is preserved while switching between workflows for the same account

## 4. Component Hierarchy

- `Cash\Manager`
  - `ManagerShell`
    - `CashManagerTree`
    - `CashManagerHeader`
    - `NamespaceContextBar`
    - `CashManagerActionBar`
    - `CashManagerDetailHost`
      - `StatementWorkspace`
        - `StatementToolbar`
        - `StatementSummaryCards`
        - `StatementGroupList`
          - `StatementTransactionGrid`
        - `PostingPanel`
      - `PaymentEntryWorkspace`
        - `PaymentEntryForm`
        - `InvoiceResolutionPanel`
        - `DraftPaymentsList`
      - `AssetEntryWorkspace`
        - `AssetEntryForm`
        - `AssetCapitalisationAssist`
        - `DraftAssetsList`
      - `TransferWorkspace`
        - `TransferForm`
        - `DraftTransfersList`
      - `CashAccountWorkspace`
        - `AccountSummaryPanel`
        - `AccountCapabilityPanel`
  - `Services`
    - `CashManagerState`
    - `CashManagerService`
    - `CashStatementQueryService`
    - `CashEntryCommandService`
    - `CashNamespaceResolver`
  - `Models`
    - `CashManagerViewState`
    - `NamespaceResolutionResult`
    - `StatementGroupModel`
    - `StatementRowModel`
    - `PostingPreviewModel`
    - `EntryDraftModel`

## 5. Layout Structure

### 5.1 Left Pane

The left pane mirrors the tree-module convention and contains:

- financial year selector derived from `App.tbYear`
- period selector derived from `App.tbYearPeriod`
- only periods whose cash status is not `Archived`
- current month selected by default
- account tree filtered to cash accounts relevant to the selected visible period
- account type badge: `Cash`, `Asset`, or `Dummy`
- namespace filter input using the shared `NamespaceSelector`
- quick counters:
  - unposted item count
  - posted item count for the selected visible period
  - provisional balance delta
- workflow shortcuts enabled by account type

Workflow visibility by account type:

- `Cash`:
  - Statement
  - Payment Entry
  - Transfers
  - Posting
- `Asset`:
  - Statement
  - Asset Entry
  - Posting
- `Dummy`:
  - Statement

The left-hand filters are for view scoping and navigation only. They do not redefine session-based posting.

### 5.2 Right Pane

The right pane is composed of:

- header row
- context bar
- workspace body
- optional slide-in confirmation panel for posting and destructive actions

Header content:

- workspace title
- selected cash account
- current period view
- applied namespace path or namespace scope label
- primary action buttons

Primary actions:

- `New payment`
- `New asset entry`
- `New transfer`
- `Post unposted transactions`

Action visibility follows account-type rules.

## 6. Namespace Filter Usage

The Namespace Filter is mandatory in all transaction-entry workflows.

### 6.1 Shared Behaviour

The filter uses the existing `NamespaceSelector` contract:

- `OnFilterChanged` for suggestions and partial scope changes
- `OnFilterCommitted` for committed namespace context
- path-specific suggestions, not simple subject-name search
- support for multi-parent DAG semantics

### 6.2 Resolution Rules

For all payment-like events:

1. user selects subject or source business context
2. module resolves valid DAG parent paths
3. if one valid parent exists, assign automatically
4. if multiple valid parents exist, require explicit selection
5. save `ParentSubjectCode`
6. only then allow polarity/SvD/invoice resolution

### 6.3 Workflow-Specific Use

Payment Entry:

- namespace filter resolves the semantic parent for the counterparty event
- mandatory before save
- when paying invoices, invoice lookup is restricted to the resolved DAG parent
- when entering a miscellaneous payment, auto-invoice creation inherits the resolved `ParentSubjectCode`

Asset Entry:

- namespace filter resolves the semantic owner/context of the asset event
- default namespace may derive from the selected asset account if unique
- save is blocked until `ParentSubjectCode` is known

Transfers:

- internal transfer still requires namespace context when the transfer has semantic ownership beyond the account pair
- default to the account’s unique namespace where deterministic
- expose namespace override only when multiple valid DAG parents exist

Statement:

- namespace filter acts as a scope reducer and grouping controller
- the view may show:
  - all namespaces for the selected account
  - one committed namespace path
  - one DAG parent across all descendants

## 7. Workspace Designs

### 7.1 Statement Workspace

This is the default landing workspace.

Toolbar:

- financial year selector from `App.tbYear`
- period selector from `App.tbYearPeriod`
- archived periods hidden
- current month loaded by default
- namespace filter
- toggle chips:
  - `All`
  - `Posted`
  - `Unposted`
  - `Transfers`
- density toggle: `Comfortable` / `Compact`

Summary cards:

- opening posted balance
- total posted in visible period
- total unposted in session
- provisional balance
- unposted item count

Main content:

- grouped by `ParentSubjectCode`
- each group shows:
  - namespace label/path
  - group posted total
  - group unposted total
  - group provisional balance
- rows ordered by:
  - group
  - `PaidOn`
  - unposted before posted when same date
  - stable payment code ordering

Row content:

- status badge: `Posted`, `Unposted`, `Transfer`
- `PaidOn`
- subject
- reference
- cash description
- paid out
- paid in
- running balance
- actions

Desktop rows are grid-like.
Mobile rows are card summaries with expandable detail.

### 7.2 Payment Entry Workspace

Two entry modes are required:

- `Pay invoices`
- `Miscellaneous payment`

Shared form fields:

- cash account
- counterparty subject
- namespace context
- paid on
- reference
- amount
- direction
- optional notes if already supported by the backing model

Mode-specific behaviour:

`Pay invoices`

- no manual cash code unless explicitly required by an exception path
- preview open invoices within the resolved DAG parent
- FIFO settlement preview is displayed read-only
- user sees which invoices will be settled before save/post

`Miscellaneous payment`

- requires cash code and tax code as today
- polarity is derived from `CashCode -> Category -> CashPolarity`
- if `CashCode` is null, the workflow moves into invoice-driven resolution and uses FIFO within the same DAG parent

Draft list below the form:

- unposted payments for current account and namespace scope
- inline actions:
  - edit
  - delete
  - inspect resolution

### 7.3 Asset Entry Workspace

Asset entry is a double-entry support workflow, not a simple isolated cash form.

Purpose:

- record the asset-side event after the originating cash outflow has occurred
- support capital balance increase and future depreciation workflows
- preserve the semantic link between the originating cash transaction and the resulting asset entry

Fields:

- asset account
- namespace context
- paid on
- amount
- reference
- optional originating payment link
- optional asset classification if already supported by the backing model

Defaults inherited from the selected account where valid:

- subject
- cash code
- tax code

Workflow:

1. user identifies or selects the originating cash payment
2. user resolves namespace context
3. user confirms the asset-side entry
4. system stores the unposted asset transaction
5. statement refreshes and shows the updated provisional position

#### 7.3.1 Asset Capitalisation Assist

A non-blocking assistive feature should be added.

Recommended behaviour:

- define a configurable capitalisation threshold, for example `£1,000`
- when a qualifying cash payment exceeds the threshold, the Statement and Payment detail surfaces show a suggestion such as `Consider adding as asset`
- the suggestion opens `AssetEntryWorkspace` with the originating payment preselected
- this is advisory only
- it must not auto-create the asset entry
- users remain free to ignore the suggestion
- the threshold should be configurable so that organisations can adopt their own capitalisation policy

The draft list shows only unposted asset entries for the selected account and namespace scope.

### 7.4 Transfer Workspace

The transfer workflow is restricted to `Cash` account types only.

Fields:

- source account
- destination account
- transfer code
- paid on
- reference
- amount

Rules:

- same account cannot be source and destination
- transfer code set is limited to valid transfer cash codes
- only `Cash` accounts may act as transfer endpoints in this workflow
- transfer rows are shown in both source and destination statements with transfer styling
- `Asset` and `Dummy` accounts do not expose transfer entry in `Cash\Manager`

## 8. Interaction Model

### 8.1 Standard Flow

1. User selects a cash account.
2. Statement loads for that account.
3. User commits a namespace filter or keeps all namespaces in scope.
4. User opens an entry workflow.
5. User completes the form.
6. The UI resolves DAG parentage.
7. The server validates polarity, SvD, FIFO, and invoice consequences.
8. Entry is saved as unposted.
9. Statement refreshes immediately.
10. User posts when ready.

### 8.2 Editing and Deleting Unposted Rows

Only unposted rows are editable or deletable.

Posted rows:

- remain visible
- open in read-only detail
- may expose drill-through to related invoice or subject detail
- cannot be mutated from Cash Manager unless an existing supported reversal path already exists

### 8.3 Error and Conflict Handling

Inline validation is used for:

- missing namespace resolution
- invalid amount
- dual-sided amount entry
- missing account
- missing required cash/tax code

Server-originated errors are displayed in a non-modal alert region at the top of the workspace.

## 9. State Management

`CashManagerState` should own the right-pane state and follow the lightweight service-driven pattern already used in the tree modules.

State slices:

- selected account
- selected account type
- selected workspace
- current period view
- current namespace filter text
- committed namespace path
- resolved `ParentSubjectCode`
- draft entry model
- statement rows
- grouped summaries
- posting preview
- busy/error/success state

Rules:

- account selection survives workspace changes
- namespace scope survives workspace changes for the same account
- draft forms are reset only after successful save or explicit cancel
- statement refresh happens after save, delete, edit, transfer, and post

## 10. Backend Integration Points

The Blazor module remains thin. Business behaviour stays in procedures and data-access services.

### 10.1 Statement/Data Queries

Use existing statement and cash-account data shapes as the base:

- `Cash_vwAccountStatement`
- `Cash_vwPaymentsUnposted`
- `Cash_vwTransfersUnposted`
- `Subject_vwCashAccount`

These need namespace-aware expansion so rows include:

- `ParentSubjectCode`
- namespace display path
- posted/unposted grouping metadata

### 10.2 Commands

The module orchestrates existing procedure families:

- `Cash.proc_PaymentAdd`
- `Cash.proc_PaymentPost`
- `Cash.proc_PaymentPostById`
- `Cash.proc_PaymentPostInvoiced`
- `Cash.proc_TxPayIn`
- `Cash.proc_TxPayOutInvoice`

Support actions already present in the current data layer may remain available through the new service surface:

- payment delete
- payment move
- asset posting

### 10.3 Required Behavioural Upgrades

All affected commands must accept or derive `ParentSubjectCode`.

The UI must assume these server guarantees:

- FIFO invoice matching is restricted to the same DAG parent
- auto-created invoices inherit `ParentSubjectCode`
- polarity is resolved after DAG anchoring
- SvD balancing operates within the same namespace context

### 10.4 Required Schema Alignment

The DAG upgrade must extend beyond the immediate Cash Manager UI boundary.

Required schema changes:

- add `ParentSubjectCode` to `Cash.tbPayment`
- add `ParentSubjectCode` to `Invoice.tbInvoice`
- add `ParentSubjectCode` to `Project.tbProject`

Notes:

- `Project.tbProject` is not fully implemented in this phase, but the schema should be aligned now
- when a project is created in a context where DAG parentage is already known, `ParentSubjectCode` should be stamped at creation time
- this allows future project, invoice, and payment workflows to share one namespace-aware semantic anchor

### 10.5 Procedure Impact

The following procedures are directly impacted by DAG anchoring and must be revised accordingly:

- `Cash.proc_PaymentAdd`
- `Cash.proc_PaymentPost`
- `Cash.proc_PaymentPostInvoiced`
- `Cash.proc_TxPayIn`
- `Cash.proc_TxPayOutInvoice`

The following behavioural updates are required:

- `ParentSubjectCode` must be written at payment creation time
- invoice selection and settlement must operate within the same DAG parent
- miscellaneous auto-invoice creation must inherit the payment DAG anchor
- any project-derived invoice flow must preserve the same parent context when available

### 10.6 Supplementary Dependency — Subject Browser and Subject Enquiry

A supplementary update is required in the Subject Browser stack.

Reason:

- once `Cash\Manager` becomes DAG-aware, the existing Subject Enquiry surfaces become semantically incomplete because they currently query by `SubjectCode` alone

Required follow-on changes:

- Subject Enquiry must become namespace-aware
- enquiry links from `Cash\Manager` should be capable of carrying namespace context
- payments, invoices, and statements shown from Subject Enquiry must be filterable by `ParentSubjectCode` or namespace path instance
- where a Subject appears in multiple DAG paths, the enquiry UI must distinguish the active semantic instance instead of collapsing all activity into a single undifferentiated subject view

This is a dependency for semantic consistency across the Blazor application, even if the full Subject Enquiry rewrite lands after the first `Cash\Manager` delivery.

## 11. Statement Rendering and Refresh Rules

The Statement must refresh after:

- save payment
- save asset entry
- save transfer
- edit unposted row
- delete unposted row
- posting completion
- namespace filter commit
- account change
- period change

Refresh strategy:

- preserve current account
- preserve namespace scope
- preserve period view
- preserve expanded namespace groups where possible
- recompute running totals after every refresh, not incrementally in the UI

Running balance rules:

- posted balance comes from posted ledger sequence
- provisional balance = posted balance + unposted delta in current scope
- unposted rows must be visually distinct and included in provisional totals

## 12. Posting Confirmation Model

When the user selects `Post unposted transactions`:

1. open a confirmation panel/dialog
2. show unposted count
3. show totals by namespace
4. show final provisional balance impact
5. warn if any row lacks namespace resolution or server eligibility
6. require explicit confirmation

On success:

- close confirmation surface
- refresh statement
- show success banner with posted count

On failure:

- keep the user in context
- show server error
- leave the unposted spool unchanged in the UI until a successful refresh confirms otherwise

## 13. Visual Language

The visual design should match the existing Blazor tree modules:

- Bootstrap-first layout
- restrained chrome
- compact filters
- small muted metadata
- strong section headers
- card summaries above dense data
- badges for status and account type
- no modal-heavy workflow unless confirmation is required

Status colours:

- posted: neutral/success
- unposted: warning/emphasis
- transfer: info
- error/blocked: danger

## 14. Delivery Sequence

Recommended implementation order:

1. `ManagerShell` and account navigation
2. Statement workspace and grouped rendering
3. namespace-aware state/service layer
4. Payment Entry workspace
5. Asset Entry workspace
6. Transfer workspace
7. Posting panel and confirmation flow
8. final mobile/embedded refinement

## 15. Acceptance Criteria

The Cash Manager design is complete when:

- all legacy cash workflows are reachable from one Blazor shell
- Statement is the default landing experience
- every economic event is namespace-anchored before save/post
- grouped statement rendering works by `ParentSubjectCode`
- posted and unposted rows are visible together
- provisional balance is always visible
- posting is explicitly user-triggered and session-based
- UI actions adapt correctly to `CashAccountType`
- server integration preserves existing financial behaviour while adding DAG awareness
- legacy Razor Pages are no longer part of the Cash user experience
