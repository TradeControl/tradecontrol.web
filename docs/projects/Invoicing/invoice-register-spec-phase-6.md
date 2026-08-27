# Engineering Work Plan for Invoice Register Section 8, Phase 6

## 1. Objective

Plan the next delivery stage for the Invoice Register behavioural refactor:

- Phase 6 — Submission

The goal is to migrate the legacy invoice email/submission workflow in:

- `Pages/Invoice/Update/EmailConfirm.*`
- `Pages/Invoice/Update/EmailPreview.*`

into the Blazor Invoice Register shell without changing business behaviour, mail host behaviour, template usage rules, printed/despool behaviour, or the established shell ownership model.

This document replaces the earlier Phase 4/5 brief now that planning focus has moved to the submission workflow.

## 2. Authoritative inputs reviewed

The following sources have now been inspected and should be treated as the current planning baseline.

### 2.1 Specification and development contract

- `docs/specs/invoice-register-spec.md`
- `docs/specs/tc-design-principles.md`
- `docs/specs/tc-development-contract.md`

### 2.2 Current Invoice Register implementation

- `Pages/Invoice/Register/*`
- `AppServices/InvoiceRegister/*`

### 2.3 Legacy submission workflow

- `Pages/Invoice/Update/EmailConfirm.cshtml`
- `Pages/Invoice/Update/EmailConfirm.cshtml.cs`
- `Pages/Invoice/Update/EmailPreview.cshtml`
- `Pages/Invoice/Update/EmailPreview.cshtml.cs`

### 2.4 Mail/template subsystem

- `Mail/TemplateManager.cs`
- `Mail/MailInvoice.cs`
- `Mail/MailService.cs`
- `Mail/MailSupport.cs`
- `Mail/TestMailSender.cs`
- `Mail/IdentityEmailSender.cs`

### 2.5 Supporting business/data/model dependencies now available

- `Data/Docs.cs`
- `Data/NodeSettings.cs`
- `Data/Invoices.cs`
- `AppServices/ServiceCollectionExtensions.cs`

and the supplied invoice/web models relevant to template, attachment, image, mail preview and invoice document rendering, including:

- `Models/Invoice_vwDoc.cs`
- `Models/Invoice_vwDocDetail.cs`
- `Models/Invoice_vwTaxSummary.cs`
- `Models/Web_tbTemplate.cs`
- `Models/Web_tbTemplateInvoice.cs`
- `Models/Web_tbTemplateImage.cs`
- `Models/Web_tbTemplateStatus.cs`
- `Models/Web_tbAttachment.cs`
- `Models/Web_tbAttachmentInvoice.cs`
- `Models/Web_tbImage.cs`
- `Models/Web_vwTemplateInvoice.cs`
- `Models/Web_vwTemplateImage.cs`
- `Models/Web_vwAttachmentInvoice.cs`

## 3. Product-launch scope refinement

The full MIS prototype includes a broader Document Manager concept that also supports Projects, Sales Orders, Quotations and other document classes.

That broader Document Manager is not in first-release scope.

For launch, document submission is being integrated directly into the Invoice Register.

This affects interpretation of the existing document flags and submission flow.

### 3.1 Launch scope assumptions now confirmed

For the first product launch:

- submission is handled only from the Invoice Register
- only one submission is handled at a time
- there is no end-user multi-document spooling workflow
- the `Spooled` flag is not needed as a user-facing launch concept
- the `Printed` flag becomes the visible submission status indicator
- users must be able to clearly identify and filter unsent documents (`Printed == false`)

### 3.2 Operational expectation

In practical use, businesses are likely to submit mainly:

- Sales Invoices
- Credit Notes

This means the shell should make submission status obvious and allow fast filtering for work still needing to be sent.

It also introduces the need to consider whether certain non-email/supply-side workflows should be automatically treated as already sent by setting `Printed = true`.

That rule must be handled explicitly in implementation, not left implicit.

## 4. Current state before Phase 6

From the current Blazor shell and supporting workflow service, the following are already in place:

- Register workspace
- enquiry/detail panel
- Raise workspace
- posted invoice edit/cancel workflow
- item add/edit/delete workflow
- pending entry posting workflow
- stub or deferred submission messaging in current post flows

The current codebase therefore has the invoice lifecycle up to and including posting, but the printable/email submission workflow remains outside the shell in legacy Razor Pages.

Phase 6 is the stage where the Blazor shell must absorb that remaining workflow.

In addition, because launch does not include a separate Document Manager, Phase 6 must also provide the visible submission-status surface within the Invoice Register itself.

## 5. Legacy submission workflow findings

## 5.1 Email confirmation page behaviour

`Pages/Invoice/Update/EmailConfirm.*` currently provides:

- invoice lookup by `invoiceNumber`
- optional recipient hint via `emailAddress`
- authorisation rules:
  - managers/admins unrestricted
  - other users restricted to invoices matching their resolved internal `UserId`
- template selection from templates assigned to the invoice type:
  - joins `Web_tbTemplateInvoices` to `Web_tbTemplates`
  - ordered by `LastUsedOn` descending
- default template selection:
  - first available template in the ordered list
- recipient selection from subject email addresses:
  - all distinct addresses for the invoice subject
  - if requested `emailAddress` exists, use it
  - else if an admin contact exists, use the admin address
  - else use first address
- navigation/actions:
  - preview document
  - submit document
  - open subject contact creation page
  - return to Update index

Important implication:

Phase 6 is not only “send email”.
It includes:

- template selection
- recipient selection
- preview
- send
- return navigation
- subject contact creation escape route

## 5.2 Preview behaviour

`Pages/Invoice/Update/EmailPreview.*` currently provides:

- invoice lookup by `invoiceNumber`
- template lookup by `templateId`
- retrieval of `MailDocument` via `TemplateManager.GetInvoice`
- HTML preview generation via `MailInvoice.PreviewInvoice()`
- template usage registration via `TemplateManager.RegisterTemplateUsage`
- raw rendered HTML display
- return to index navigation

Important implication:

Preview is not passive.
It updates template usage metadata.

This behaviour must be preserved unless explicitly changed.

## 5.3 Submit behaviour

`EmailConfirm.cshtml.cs` submit handler currently provides:

- template resolution from selected template filename
- invoice lookup from `Invoice_tbInvoices`
- document creation via `TemplateManager.GetInvoice`
- invoice mail workflow via `MailInvoice.Send(emailAddress)`
- template usage registration
- return to Update index

The actual email send path therefore depends on:

- mail host settings
- selected template
- invoice type
- invoice number
- recipient address
- all document build logic encapsulated in `MailInvoice`

## 5.4 Printed/despool behaviour

Within `MailInvoice.Send(...)`:

- invoice recipient details are resolved from subject email addresses
- the invoice document is built and sent
- `Invoices.SetToPrinted()` is called after a successful send

Elsewhere, `Docs.DespoolAll()` exists for mark-all-as-sent behaviour in the broader document-spooling model.

For the launch product:

- `Spooled` is not required as a user-facing workflow concept
- `Printed` must be treated as the authoritative visible submission state
- Invoice Register surfaces must expose and filter this state clearly

## 5.5 Mail host / template availability observations

The legacy pages are confirmed to preview and send correctly when configuration is present.

However, they do not appear to strongly front-load validation of:

- active mail host availability
- required template configuration

This is worth improving in the Blazor workflow.

Important implication:

Phase 6 should add explicit, user-visible readiness handling for:

- no configured mail host
- no valid template assignment for the invoice type
- no available recipient addresses

without changing the underlying send path.

## 6. Mail/template subsystem findings

## 6.1 TemplateManager responsibilities

The supplied `TemplateManager` currently owns:

- template/image/document file discovery
- template initialisation and file/database synchronisation
- attachment assignment to invoice types
- template assignment to invoice types
- image assignment to templates
- image tag maintenance
- retrieval of invoice mail document
- retrieval of generic mail text
- retrieval of support/user registration templates
- template usage registration
- content-type file helpers
- template parse/validation support

Important implication:

Phase 6 should use `TemplateManager` as-is.
It must not replicate template resolution logic inside the Blazor shell.

## 6.2 MailInvoice responsibilities

`MailInvoice` currently owns:

- invoice header argument population
- company detail population
- embedded template resolution
- detail table rendering from `Invoice_vwDocDetail`
- tax summary rendering from `Invoice_vwTaxSummary`
- preview HTML rendering
- actual send workflow
- printed flag update after successful send

Important implication:

Phase 6 should not migrate document rendering into UI components.
The shell should orchestrate preview/send, while `MailInvoice` remains authoritative for document build behaviour.

## 6.3 MailService responsibilities

`MailService` currently owns:

- document HTML generation
- HTML preview file support
- attachment inclusion
- linked image handling
- SMTP delivery via MailKit
- plain text send support

Important implication:

No UI or service-layer rewrite should bypass this subsystem unless explicitly required.

## 6.4 NodeSettings responsibilities

`NodeSettings` currently owns mail host configuration access, including:

- host existence checks
- encrypted credential retrieval
- symmetric key management
- mail host settings materialisation

Important implication:

The specification requirement to preserve existing Mail Host behaviour means Phase 6 must continue to rely on `NodeSettings` and existing `MailService` behaviour.

## 7. Interpretation of Phase 6 after inspection

Phase 6 should be interpreted as the migration of the full printable/email submission workflow into the Blazor Invoice Register shell.

That includes:

1. selecting an invoice for submission from the shell
2. showing visible sent/unsent state using `Printed`
3. filtering unsent invoices (`Printed == false`)
4. selecting a recipient
5. selecting a template
6. previewing rendered HTML
7. submitting/sending the email
8. registering template usage
9. marking the invoice printed/sent
10. preserving return navigation into the current invoice/register context
11. preserving authorisation rules
12. preserving contact-management escape routing where practical

Because launch does not include a separate Document Manager, the submission-status surface is part of Phase 6, not a later concern.

## 8. Architecture constraints for implementation

These constraints remain mandatory.

### 8.1 Shell ownership

The shell must own:

- submission workflow mode
- selected invoice
- selected template
- selected recipient
- preview state
- return navigation
- mobile/desktop workflow transitions
- submission-status filter state

Child components may:

- render forms
- render preview content
- render status indicators
- raise events

Child components must not:

- call `TemplateManager`
- call `MailInvoice`
- call `NodeContext`
- decide workflow navigation

### 8.2 Business behaviour preservation

Business behaviour must remain in:

- `TemplateManager`
- `MailInvoice`
- `MailService`
- `NodeSettings`
- `Docs`
- `Invoices`

Do not move:

- template resolution
- invoice rendering
- SMTP send
- printed update
- template usage registration
- host decryption/configuration

into UI components.

### 8.3 Preserve current shell baseline

The current Invoice Register shell is a trusted baseline.

Do not redesign:

- register workspace
- detail panel ownership
- mobile navigation model
- rendering patterns
- existing workflow mode pattern

Phase 6 must extend the shell.

## 9. Recommended implementation plan — Phase 6 Submission

## 9.1 Target user outcome

Users should be able to submit/post-send printable invoices from within the Invoice Register shell without returning to legacy Razor Pages.

For invoice types that participate in the email workflow, users should be able to:

- clearly see whether an invoice has been sent
- filter the register to show unsent invoices
- open submission from the current invoice context
- choose recipient
- choose template
- preview the rendered invoice
- send the invoice
- return cleanly to the register/detail workspace

## 9.2 Submission status surface to provide early

Before or as part of the main submission workflow implementation, the Invoice Register should expose:

- visible sent/unsent status in the invoice grid
- visible sent/unsent status in the detail panel
- filtering for unsent invoices (`Printed == false`)

This should be treated as an early Phase 6 surface, not a later polish item.

Reason:

It supports launch readiness and gives the user the operational view needed to manage submissions.

## 9.3 Suggested shell additions

Add one or more new shell workflow modes for submission, likely:

- SubmitConfirm
- SubmitPreview

or equivalent explicit submission sub-modes.

These should be launched from the existing detail panel `Submit` action and from any post-raise redirect path for email-capable invoice types.

## 9.4 Required service-layer capabilities

Add submission-focused service methods for:

1. load submission context for one invoice
2. resolve allowed templates for invoice type
3. resolve recipient addresses for the invoice subject
4. determine default template and recipient using legacy rules
5. report whether submission is currently possible:
   - mail host configured
   - template available
   - recipient available
6. generate HTML preview
7. send invoice email
8. register template usage
9. update invoice printed state through existing business path
10. optionally expose “mark all sent” behaviour if it is to be incorporated into the shell during this phase

These methods should wrap existing behaviour rather than reinterpret it.

## 9.5 Behaviour to preserve exactly

- role/user authorisation from `EmailConfirm`
- template ordering by `LastUsedOn` descending
- default template = first available ordered template
- recipient default precedence:
  - requested email if valid
  - else admin contact if present
  - else first address
- preview registration of template usage
- send registration of template usage
- send path through `MailInvoice.Send`
- printed flag update after successful send
- existing mail host behaviour via `NodeSettings`
- contact-creation escape route or equivalent recoverable path

## 9.6 New launch-specific behaviour to define explicitly

The following must now be treated as explicit Phase 6 rules to implement:

1. `Printed` is the visible submission-status flag.
2. The user must be able to filter on unsent invoices (`Printed == false`).
3. `Spooled` is not required for the first-release Invoice Register submission workflow.
4. It should be considered whether non-submitted supply-side invoice types should automatically be marked `Printed = true`.

That fourth rule is not yet fully specified and should be confirmed before implementation.

## 9.7 Open rule requiring confirmation

This needs explicit product confirmation before coding:

Should invoices that are not expected to go through the email submission flow be automatically set to `Printed = true` at creation/posting time?

Possible interpretations:

- only Sales Invoices and Credit Notes participate in submission, everything else auto-printed
- all invoice types keep manual printed state
- some subset auto-printed based on polarity or type

This is a genuine business rule and should not be guessed.

## 9.8 Likely UI surfaces

1. register grid enhancements
   - sent/unsent indicator
   - unsent filter
2. submission confirmation/editor surface
   - invoice summary
   - template select
   - recipient select
   - configuration readiness warnings
   - action buttons
3. preview surface
   - rendered HTML preview
   - back/send actions
4. possible shell-level hand-off from Raise post to Submission for emailed invoice types

## 9.9 Suggested workflow model additions

Likely additions to Invoice Register workflow models:

- submission context model
- template option model
- recipient option model
- preview model/result
- submission result model
- submission readiness state model

These should remain focused and explicit.

## 9.10 Suggested service structure

Likely additions to `IInvoiceRegisterWorkflowService` / `InvoiceRegisterWorkflowService`:

- `GetSubmitConfirmAsync(invoiceNumber, emailAddress?)`
- `PreviewSubmitAsync(model)`
- `SendSubmitAsync(model)`

Naming can vary, but behaviour should stay explicit rather than generic.

## 9.11 Navigation expectations

### Entry points

Submission should be reachable from:

- `InvoiceDetailPanel` `Submit` button
- unsent invoices visible in the register
- any post-raise workflow that currently implies email-capable next steps

### Returns

Return should preserve:

- selected invoice where possible
- current detail panel context
- register filter state
- mobile back navigation expectations

## 10. Suggested sequencing

Implement in this order:

1. confirm the rule for automatic `Printed = true` handling on non-submitted invoice types
2. expose `Printed` clearly in the register/detail surfaces
3. add unsent filtering (`Printed == false`)
4. define submission workflow models
5. define service interface additions
6. implement submission query/load service methods
7. implement configuration-readiness checks
8. implement preview service method
9. implement send service method
10. add submission confirm component
11. add preview component
12. wire detail panel `Submit` button
13. wire raise-post redirection for email-capable invoice types
14. verify authorisation and printed/template-usage side effects

## 11. Risks

### Risk 1 — Reimplementing template/mail logic in the shell

Mitigation:

- shell orchestrates only
- existing mail/template classes remain authoritative

### Risk 2 — Losing legacy default-selection behaviour

Mitigation:

- reproduce exact template/recipient resolution rules in service layer using the existing data sources

### Risk 3 — Breaking printed flag semantics

Mitigation:

- preserve `MailInvoice.Send` / `Invoices.SetToPrinted()` path
- explicitly separate launch submission-status requirements from the broader Document Manager spool model

### Risk 4 — Preview/send side effects diverging

Mitigation:

- preserve legacy template usage registration behaviour for both preview and send unless explicitly changed

### Risk 5 — Submission workflow colliding with current shell navigation

Mitigation:

- use explicit workflow modes with shell-owned return transitions

### Risk 6 — Contact management escape route becoming orphaned

Mitigation:

- if not migrated into Blazor in this phase, provide a controlled route out and back that preserves invoice context

### Risk 7 — Ambiguous auto-printed behaviour for non-email invoice types

Mitigation:

- do not guess
- confirm business rule before coding

## 12. Estimated task count

Estimated tasks: 12 to 16

Likely task outline:

1. confirm launch printed-status rules
2. add register sent/unsent indicator
3. add unsent filter support
4. inspect and confirm latest shell workflow mode structure
5. add submission workflow models
6. add service interface methods
7. implement invoice submission context loading
8. implement template/recipient defaulting
9. implement readiness checks for host/templates/recipients
10. implement HTML preview service path
11. implement send service path
12. add submission confirm component
13. add preview component
14. wire detail panel submit action
15. wire raise-post hand-off for email-capable invoices
16. validate authorisation and printed/template usage side effects

## 13. Approval-gate implementation plan

Before code changes for Phase 6:

1. confirm the product rule for which invoice types participate in manual submission
2. confirm whether non-submitted types should auto-set `Printed = true`
3. decide whether the sent/unsent surface is implemented as the first Phase 6 task or bundled with the submission editor
4. decide final shell workflow mode shape for submission
5. define new workflow models and service contracts
6. identify exact Blazor components to add
7. confirm whether “mark all sent” is in Phase 6 scope or deferred
8. confirm whether subject contact creation remains a Razor Pages escape route in this phase
9. implement submission-status surface first
10. then submission confirmation
11. then preview
12. then send
13. then raise/detail-panel navigation integration

## 14. Summary

What is now established after inspection:

- the legacy submission workflow is fully understood
- Phase 6 is broader than a send button; it includes template selection, recipient selection, preview, send, printed state, and usage tracking
- the existing mail/template subsystem is already the correct business layer and should be preserved
- the current Blazor Invoice Register shell is the trusted baseline and should be extended rather than redesigned
- because there is no separate Document Manager at launch, the Invoice Register must visibly own submission status using `Printed`

## 15. Assumptions made

- “Submission” in the specification corresponds to the current email confirm / email preview workflow for invoice delivery
- `Submit` on the detail panel should become the main shell entry point for Phase 6
- the contact creation route may remain an external Razor Pages escape route during Phase 6 unless explicitly required to be absorbed into Blazor
- “Mark all as sent” may be deferred unless you want it explicitly included in Phase 6 delivery
- launch users need sent/unsent visibility in the Invoice Register before or as part of the full submission workflow

## Appendix 1 - Aider files

/add docs/specs/invoice-register-spec.md  
/add docs/specs/tc-design-principles.md  
/add docs/specs/tc-development-contract.md  
/add docs/tmp/session-brief.md  
/add src/TCWeb/Pages/Invoice/Register/*  
/add src/TCWeb/AppServices/InvoiceRegister/*  
/add src/TCWeb/AppServices/ServiceCollectionExtensions.cs  
/add src/TCWeb/Pages/Invoice/Update/EmailConfirm.*  
/add src/TCWeb/Pages/Invoice/Update/EmailPreview.*  
/add src/TCWeb/Mail/TemplateManager.cs  
/add src/TCWeb/Mail/MailInvoice.cs  
/add src/TCWeb/Mail/MailService.cs  
/add src/TCWeb/Mail/MailSupport.cs  
/add src/TCWeb/Mail/TestMailSender.cs  
/add src/TCWeb/Mail/IdentityEmailSender.cs  
/add src/TCWeb/Data/Docs.cs  
/add src/TCWeb/Data/NodeSettings.cs  
/add src/TCWeb/Data/Invoices.cs  
/add src/TCWeb/Models/Invoice_vwDoc.cs  
/add src/TCWeb/Models/Invoice_vwDocDetail.cs  
/add src/TCWeb/Models/Invoice_vwTaxSummary.cs  
/add src/TCWeb/Models/Web_tbTemplate.cs  
/add src/TCWeb/Models/Web_tbTemplateInvoice.cs  
/add src/TCWeb/Models/Web_tbTemplateImage.cs  
/add src/TCWeb/Models/Web_tbTemplateStatus.cs  
/add src/TCWeb/Models/Web_tbAttachment.cs  
/add src/TCWeb/Models/Web_tbAttachmentInvoice.cs  
/add src/TCWeb/Models/Web_tbImage.cs  
/add src/TCWeb/Models/Web_vwTemplateInvoice.cs  
/add src/TCWeb/Models/Web_vwTemplateImage.cs  
/add src/TCWeb/Models/Web_vwAttachmentInvoice.cs
