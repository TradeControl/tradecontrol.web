# Engineering Work Plan for Invoice Register Section 8, Phases 4 and 5

## 1. Objective

Plan the next delivery stages for the Invoice Register behavioural refactor:

- Phase 4 — Raise
- Phase 5 — Edit/Cancel

The goal is to migrate the legacy Razor Pages behaviour in:

- `Pages/Invoice/Raise/*`
- `Pages/Invoice/Update/*`
- relevant `Pages/Invoice/Enquiry/*`

into the Blazor Invoice Register shell without changing business behaviour, security, posting flow, or the established shell ownership model.

This document replaces the earlier Phase 4/5 brief now that the requested legacy dependencies have been supplied.

## 2. Authoritative inputs reviewed

The following sources have now been inspected and should be treated as the current planning baseline.

### 2.1 Specification and development contract

- `docs/specs/invoice-register-spec.md`
- `docs/specs/tc-design-principles.md`
- `docs/specs/tc-development-contract.md`

### 2.2 Current Invoice Register implementation

- `Pages/Invoice/Register/*`
- `AppServices/InvoiceRegister/*`
- `wwwroot/css/modules/invoiceRegister.css`

### 2.3 Legacy Raise workflow

- `Pages/Invoice/Raise/Index.*`
- `Pages/Invoice/Raise/Create.*`
- `Pages/Invoice/Raise/Details.*`
- `Pages/Invoice/Raise/Edit.*`
- `Pages/Invoice/Raise/Delete.*`
- `Pages/Invoice/Raise/Post.*`

### 2.4 Legacy Update workflow

- `Pages/Invoice/Update/Index.*`
- `Pages/Invoice/Update/Edit.*`
- `Pages/Invoice/Update/CreateItem.*`
- `Pages/Invoice/Update/EditItem.*`
- `Pages/Invoice/Update/DeleteItem.*`
- `Pages/Invoice/Update/Delete.*`
- `Pages/Invoice/Update/EmailConfirm.*`
- `Pages/Invoice/Update/EmailPreview.*`

### 2.5 Legacy Enquiry workflow relevant to Edit/Cancel continuity

- `Pages/Invoice/Enquiry/Index.*`
- `Pages/Invoice/Enquiry/Details.*`
- `Pages/Invoice/Enquiry/Summary.*`
- `Pages/Invoice/Enquiry/Unpaid.*`
- `Pages/Invoice/Enquiry/UnpaidDetail.*`

### 2.6 Supporting business/data/model dependencies now available

- `Data/Invoices.cs`
- `Data/NodeContext.cs`
- `Data/NodeContextProc.cs`
- `Data/NodeEnum.cs`
- `Data/Profile.cs`
- `Data/CashCodes.cs`
- `Pages/DI_BasePageModel.cs`
- `Models/Invoice_vwEntry.cs`
- `Models/Invoice_tbEntry.cs`
- `Models/Invoice_tbInvoice.cs`
- `Models/Invoice_tbItem.cs`
- `Models/Invoice_tbProject.cs`
- `Models/Invoice_tbStatus.cs`
- `Models/Invoice_tbType.cs`
- `Models/Cash_vwCodeLookup.cs`

## 3. Current state after Phase 3

Phase 3 is complete enough to act as the stable baseline:

- Invoice Register shell owns register state and navigation
- desktop and mobile flows are established
- register header, line, and cash-code views exist
- enquiry panel exists with:
  - summary sections
  - tabs
  - change log
  - notes
  - namespace link to Subject Browser
- detail grid and detail panel modes are already integrated

This means Phase 4 and Phase 5 should extend the current shell rather than reworking the enquiry baseline.

## 4. Legacy behaviour findings

## 4.1 Raise workflow behaviour actually present in legacy code

The supplied Raise files show that "Raise" is not just a create page. It is a pending-entry workspace.

### Raise index

`Pages/Invoice/Raise/Index.*` currently provides:

- pending entry list from `Invoice_Entries`
- optional invoice-type filtering
- role-based visibility:
  - managers/admins can see all entries
  - other users see only entries matching their resolved internal `UserId`
- actions per row:
  - details
  - edit
  - post
  - delete
- action above list:
  - create new entry
- action below list:
  - post all

Post-all behaviour:
- non-manager/admin users call `Invoices.Post(userId)` for their own entries
- managers/admins iterate distinct entry users and call `Invoices.Post(userId)` for each
- if any posted entry type is:
  - `SalesInvoice`
  - `CreditNote`
  then redirect to `../Update/Index?Printed=false`
- otherwise remain in Raise

### Raise create

`Pages/Invoice/Raise/Create.*` currently provides:

- creation of pending `Invoice_tbEntry`
- defaults resolved from:
  - selected/current subject
  - selected/current cash code
  - selected/current tax code
  - selected/current invoice type
  - current internal user id
  - current date
- session-backed selection persistence for:
  - subject code
  - cash code
  - tax code
  - invoice type code
- selector escape routes to other modules:
  - subject select/new
  - cash-code select/new
  - tax-code select/new
- amount normalisation:
  - if both `TotalValue` and `InvoiceValue` are supplied, `InvoiceValue` is zeroed
- validation:
  - invalid model or zero total combined value returns page
- persistence:
  - add row to `Invoice_tbEntries`

### Raise edit

`Pages/Invoice/Raise/Edit.*` currently provides:

- load header from `Invoice_Entries`
- load editable row from `Invoice_tbEntries`
- security:
  - managers/admins unrestricted
  - others restricted to matching internal `UserId`
- tax selection by tax description
- direct modification of `Invoice_tbEntries`

### Raise details

`Pages/Invoice/Raise/Details.*` currently provides:

- read-only inspection of one pending entry
- lookup by composite key:
  - `accountCode`
  - `cashCode`

### Raise delete

`Pages/Invoice/Raise/Delete.*` currently provides:

- confirmation and deletion of one pending `Invoice_tbEntry`
- security:
  - managers/admins unrestricted
  - others restricted to own entry

### Raise post

`Pages/Invoice/Raise/Post.*` currently provides:

- confirmation page for one pending entry
- two post modes:
  - post one entry by subject + cash code via `Invoices.PostByEntry`
  - post all entries for that account via `Invoices.PostByAccount`
- email-capable invoice types:
  - `SalesInvoice`
  - `CreditNote`
  redirect to `../Update/Index?Printed=false`
- all other posted types return to Raise

## 4.2 Update workflow behaviour actually present in legacy code

The supplied Update files confirm that Phase 5 is broader than a single "edit header" action.

### Update index

`Pages/Invoice/Update/Index.*` currently provides:

- invoice header list from `Invoice_Register`
- filters:
  - account code
  - printed flag
  - invoice number
  - invoice type
  - period
- page size and page number
- paging
- row actions:
  - subject details
  - edit
  - delete
  - email confirm
- bulk action:
  - Mark all as sent via `Docs.DespoolAll()`

Important behavioural detail:
- if `AccountCode` supplied, period filter is suppressed
- if `InvoiceNumber` supplied, period is derived from the invoice
- otherwise default period is active period

### Update edit header

`Pages/Invoice/Update/Edit.*` currently provides:

- load header from `Invoice_Register`
- load editable row from `Invoice_tbInvoices`
- load invoice details from `Invoice_RegisterDetails`
- security:
  - managers/admins unrestricted
  - others restricted to matching internal `UserId`
- editable header fields:
  - invoice type
  - invoice status
  - invoiced on
  - due on
  - expected on
  - payment terms
  - printed
  - notes
- after save:
  - invoice type/status mapped from description back to code
  - detect period rebuild requirement if invoiced date crosses active period boundary
  - detect organisation rebuild requirement if invoice type/status/date meaningfully changes
  - save `Invoice_tbInvoice`
  - call `Invoices.Accept()`
  - regenerate periods if needed
  - rebuild subject totals if needed
  - redirect to Update index with invoice number filter

Important implication:
Phase 5 must preserve the rebuild side effects and not treat header save as a simple data write.

### Update create item

`Pages/Invoice/Update/CreateItem.*` currently provides:

- add a new invoice item to an existing posted invoice
- security same as header edit
- default cash codes constrained by invoice cash polarity inferred from invoice type:
  - sales/credit => income
  - purchase/debit => expense
- default VAT tax code
- amount normalisation:
  - if both `TotalValue` and `InvoiceValue` are supplied, zero `InvoiceValue`
- save item to `Invoice_tbItems`
- then:
  - `Invoices.Accept()`
  - regenerate financial periods if historical
  - rebuild subject totals
  - redirect to header edit

### Update edit item

`Pages/Invoice/Update/EditItem.*` currently provides:

- edit one existing item in `Invoice_tbItems`
- header source from `Invoice_RegisterDetails`
- security same as header edit
- tax description selector
- save item
- then:
  - compare previous and new item values
  - detect organisation rebuild if value changed
  - detect period rebuild if invoice is historical
  - `Invoices.Accept()`
  - rebuild subject totals if required
  - regenerate periods if required
  - redirect to header edit

### Update delete item

`Pages/Invoice/Update/DeleteItem.*` currently provides:

- delete one invoice item from `Invoice_tbItems`
- security same as header edit
- after delete:
  - `Invoices.Accept()`
  - regenerate periods if historical
  - rebuild subject totals
  - redirect to header edit

### Update delete invoice

`Pages/Invoice/Update/Delete.*` currently provides:

- delete an invoice header from `Invoice_tbInvoices`
- security same as header edit
- after delete:
  - call `Invoices.CancelPending(invoice.UserId)`
  - rebuild subject totals
  - redirect to Update index

Important implication:
The visible "delete" operation is logically a cancellation workflow and must preserve `CancelPending` behaviour.

### Update email confirm / preview

`Pages/Invoice/Update/EmailConfirm.*` and `EmailPreview.*` currently provide:

- email workflow for printable invoices
- recipient selection from subject email addresses
- template selection from template usage by invoice type
- preview via `TemplateManager` and `MailInvoice`
- send via `MailInvoice.Send`
- template usage registration
- contact creation escape route
- return to Update index

This is Phase 6 territory functionally, but it affects Phase 4 and 5 navigation because Raise posting redirects into Update for unprinted emailed invoices.

## 4.3 Enquiry workflow findings relevant to continuity

The legacy Enquiry files confirm the broader enquiry ecosystem:

- detail enquiry for invoice lines
- unpaid enquiry with payment creation launch
- cash-code summary enquiry with drill-through
- edit links from enquiry into update pages

Implication:
the new shell already supersedes much of the display layer, but Phase 5 must preserve the ability to enter edit flows from enquiry context without losing register state.

## 5. Interpretation of specification phases after inspection

## 5.1 Phase 4 — Raise

Based on actual legacy behaviour, Phase 4 should be interpreted as:

- pending entry list
- pending entry create
- pending entry details
- pending entry edit
- pending entry delete
- pending entry post
- post-all pending entries
- preserve redirect into post-raise invoice workflow for email-capable invoice types

This is a full pending-entry workspace.

## 5.2 Phase 5 — Edit/Cancel

Based on actual legacy behaviour, Phase 5 should be interpreted as:

- posted invoice header editing
- invoice item creation
- invoice item editing
- invoice item deletion
- invoice cancellation/delete workflow
- preservation of `Invoices.Accept()`
- preservation of period regeneration logic
- preservation of subject rebuild logic
- preservation of access control

The spec wording says:
- "Header editing migrated"
- "Item editing migrated"
- "Existing posting behaviour preserved"

The legacy code confirms those are real behavioural requirements, not optional enhancements.

## 6. Dependencies now satisfied

The previously requested dependencies are now present in chat.

No further mandatory source dependencies are currently missing for planning Phases 4 and 5.

There are still some implementation-time checks that may be required later, but they are not blockers for planning:
- `Docs` helper used by Update index mark-all-as-sent
- `FinancialPeriods`
- `Subjects`
- mail/template classes used by submission pages

Those are relevant to later implementation details, especially Phase 6, but not required to update this planning brief.

## 7. Architecture constraints for implementation

These constraints remain mandatory.

### 7.1 Shell ownership

The shell must own:
- workflow mode
- current register filters
- selected invoice
- selected pending entry
- current mobile/desktop surface
- return navigation

Components may:
- render grids
- render forms
- raise events

Components must not:
- call `NodeContext`
- call `Invoices`
- implement business logic

### 7.2 Business behaviour preservation

Business behaviour must remain in:
- `Invoices`
- existing EF entities/views
- existing data/helper classes
- existing stored procedures

Do not move:
- posting logic
- accept/recalculate logic
- cancellation logic
- period rebuild logic
- subject rebuild logic

into UI components.

### 7.3 Preserve current register baseline

The current Register/Enquiry shell is a trusted baseline.
Do not redesign:
- register layout
- shell ownership
- detail panel mode
- mobile navigation model
- DataGrid pattern
- CSS layout contract

Raise and Edit/Cancel must extend this baseline.

## 8. Recommended implementation plan — Phase 4 Raise

## 8.1 Target user outcome

Users should be able to work through the complete pending-entry workflow inside the Invoice Register shell without returning to legacy Razor Pages.

## 8.2 Suggested shell additions

Add one or more new shell workflow modes for Raise, likely:

- RaiseList
- RaiseCreate
- RaiseEdit
- RaiseDetails
- RaisePost

Whether these are separate `WorkspaceMode` values or an enclosing Raise mode with sub-state should be decided during implementation, but shell ownership must remain explicit.

## 8.3 Required service-layer capabilities

Add Raise-focused service methods for:

1. query pending entries
2. load pending entry by composite key
3. load create defaults and lookup sets
4. create pending entry
5. update pending entry
6. delete pending entry
7. post one entry
8. post account entries
9. post all visible/authorised entries

These should wrap existing behaviour, not reinterpret it.

## 8.4 Behaviour to preserve exactly

- security rules by role/internal user id
- selector defaults
- amount normalisation
- internal user id resolution using `Profile`
- email-capable redirect semantics after posting
- manager/admin post-all scope

## 8.5 Likely UI surfaces

1. pending-entry grid
2. pending-entry details panel/page
3. pending-entry edit/create form
4. posting confirmation view

## 8.6 Phase 4 estimated task count

Estimated tasks: 10 to 14

Likely task outline:
1. extend shell workflow state
2. add Raise result models
3. add Raise lookup/result service contracts
4. implement pending-entry query service
5. implement create/edit/delete/post commands
6. add pending-entry list component
7. add pending-entry editor component
8. add pending-entry details component
9. wire mobile navigation
10. wire desktop workflow
11. preserve redirect-to-update flow
12. validate permissions and side effects

## 9. Recommended implementation plan — Phase 5 Edit/Cancel

## 9.1 Target user outcome

Users should be able to maintain posted invoices from within the Invoice Register shell, including header and item maintenance, while preserving all legacy side effects.

## 9.2 Suggested shell additions

Add edit/cancel workflow modes launched from the current enquiry panel stub buttons:

- Edit header
- Add item
- Edit item
- Delete item
- Cancel invoice

The existing Phase 3 stub buttons in `InvoiceDetailPanel.razor` should become entry points for the Phase 5 workflow.

## 9.3 Required service-layer capabilities

Add Update-focused service methods for:

1. load editable invoice header
2. save editable invoice header
3. load invoice item create defaults/lookups
4. add invoice item
5. load editable invoice item
6. save invoice item
7. delete invoice item
8. cancel invoice
9. possibly query invoice list in update semantics if a dedicated maintenance view is needed

## 9.4 Behaviour to preserve exactly

- role/user authorisation
- invoice type/status mapping from labels to codes
- `Invoices.Accept()` after header/item changes
- period regeneration logic
- subject rebuild logic
- invoice cancellation via `CancelPending(userId)`
- redirect/return into the current invoice context

## 9.5 Important design decision already implied by Phase 3

The enquiry panel already contains stub buttons:
- Edit
- Cancel
- Submit

For Phase 5:
- Edit should launch posted-invoice maintenance
- Cancel should launch cancellation confirmation
- Submit should remain for later submission work unless spec says otherwise

## 9.6 Item/project scope note

The supplied legacy Update pages clearly include item creation/edit/delete.
They do not show equivalent project-line edit pages in the supplied set.

Therefore Phase 5 planning should commit to:
- header editing
- item editing
- item add/delete
- invoice cancellation

Project-line editing should only be added if further legacy files show it exists as separate behaviour.

## 9.7 Phase 5 estimated task count

Estimated tasks: 12 to 16

Likely task outline:
1. extend shell workflow state for maintenance
2. add editable invoice header model
3. add editable item model
4. implement update query/command services
5. add header edit form component
6. add item create form component
7. add item edit form component
8. add delete item confirmation
9. add cancel invoice confirmation
10. wire detail panel stub buttons
11. preserve navigation return state
12. validate side effects and authorisation
13. validate mobile behaviour
14. validate desktop behaviour

## 10. Suggested sequencing

## 10.1 Recommended order

Implement in this order:

### Part A — Phase 4 Raise
1. service models and command/query services
2. Raise list
3. Raise create
4. Raise details/edit/delete
5. Raise posting actions
6. redirect handling to subsequent invoice workflow

### Part B — Phase 5 Edit/Cancel
1. editable header flow
2. item add/edit/delete flows
3. cancellation flow
4. wire enquiry panel buttons
5. preserve state restoration

This order matches the legacy lifecycle:
- pending entries are raised first
- posted invoices are maintained afterwards

## 11. Risks

### Risk 1 — Conflating pending-entry edit with posted-invoice edit
Mitigation:
- keep Phase 4 pending-entry workflows separate from Phase 5 posted-invoice workflows

### Risk 2 — Breaking trusted shell navigation
Mitigation:
- add explicit workflow modes rather than embedding ad hoc navigation logic in child components

### Risk 3 — Accidentally dropping rebuild side effects
Mitigation:
- wrap existing helper/business calls in services, do not replace them

### Risk 4 — Overextending Phase 5 into submission
Mitigation:
- keep email/template/send functionality out of Phase 5 except where navigation must preserve the future route into submission workflow

### Risk 5 — Session-backed Raise selectors
Mitigation:
- translate session persistence into shell-owned workflow state without changing the visible behaviour

## 12. Proposed approval-gate implementation plan

Before code changes for Phase 4 and 5:

1. decide final shell workflow state shape for Raise and Edit/Cancel
2. define new models and service interfaces
3. identify exact files to change
4. implement Phase 4 first
5. then implement Phase 5

## 13. Summary

What is now established after inspection:

- all previously requested planning dependencies are present
- Phase 4 is a full pending-entry Raise workspace, not just a create form
- Phase 5 includes:
  - header editing
  - item add/edit/delete
  - invoice cancellation
  - business side-effect preservation
- the current Blazor Invoice Register shell is the trusted baseline and should be extended rather than redesigned

## 14. Assumptions made

- project-line editing is not part of Phase 5 unless further legacy files show a separate project maintenance workflow
- `Submit` in the enquiry panel remains primarily a later submission-phase entry point
- Raise-to-Update redirect semantics for emailed invoice types must be preserved until Phase 6 subsumes that route more fully
