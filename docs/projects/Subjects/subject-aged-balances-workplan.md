# Subjects Work Plan — Current Aged Invoices and Historical Balances

10 October 2026
**Status:** COMPLETE — Phases S1–S4 accepted, including retirement of the superseded Razor Pages.

## Purpose

Replace the legacy combined Debtors and Creditors report with two deliberately different Subject Browser capabilities:

1. a current, operational aged-invoice schedule sourced from genuinely unpaid invoices; and
2. historical debtor and creditor balances sourced from the Subject statement at a selected date.

Trade Control does not assign permanent customer and supplier identities. A Subject's accounting position is determined by the relevant balance:

- a positive business-polarity balance is owed to the business;
- a negative business-polarity balance is owed by the business; and
- a zero balance is absent from the active population.

This is a Subjects project. Companies House and Corporation Tax may later consume a year-end balance snapshot, but neither tax workflow owns the operational calculation or Subject Browser interface.

## Human decisions

### 9 October 2026

The domain model remains polarity-based, without artificial customer or supplier classifications. Credit-control and buying/debit-control users receive distinct language and entry points over shared accounting services.

### 10 October 2026

The source and meaning of the two reports are now explicitly separated:

1. **Aged debt and liabilities are current only.** They are derived from the present unpaid balances on `Invoice.tbInvoice` and aged from contractual `DueOn` dates.
2. **Historical debtors and creditors are balances, not aged debt.** They are derived from the Subject statement at the selected date and multiplied by `-1` to convert native statement polarity to business polarity.
3. **Reconciliation remains visible.** At the current date, the statement balance equals the open-invoice balance plus an explicit residual for opening balances or other non-invoice statement movement.
4. The superseded `[Invoice].[vwAgedDebtSales]` and `[Invoice].[vwAgedDebtPurchases]` views were removed after their current totals were reproduced by the modern contract.

## Retired prototype

`/Subject/Reports/DebtorsAndCreditors` and its current implementation in:

- `src/TCWeb/Pages/Subject/Reports/DebtorsAndCreditors.cshtml`; and
- `src/TCWeb/Pages/Subject/Reports/DebtorsAndCreditors.cshtml.cs`

were deprecated prototypes. They were removed on 10 October 2026 after the Subject Browser replacement passed its desktop and mobile review gates. The Subject Browser is now the sole UI for current aged invoices and historical debtor/creditor balances.

## Accounting contract

### Current aged invoices

The current schedule uses invoices whose status is not closed and whose present unpaid balance is non-zero:

`(InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue)`

Invoice type polarity converts that balance to the business perspective. Sales invoices and sales credit notes contribute to amounts owed to the business; purchase invoices and purchase credit notes contribute to amounts owed by the business. Credits remain in their own contractual age bands and are not silently reallocated.

Age each item from `Invoice.tbInvoice.DueOn` at today's local business date:

- not yet due/current;
- 1–30 days overdue;
- 31–60 days overdue;
- 61–90 days overdue; and
- more than 90 days overdue.

The date parameter anchors the age calculation; it is not a historical transaction cutoff. The report must be labelled as a current unpaid-invoice schedule and must not imply that it reconstructs the open invoices that existed at an earlier date.

### Historical dated balances

For each real Subject, calculate the native statement closing balance at `AsOfDate` from:

- the Subject opening balance;
- invoice movements posted on or before the date; and
- posted Subject-account payments on or before the date.

Convert it to business polarity:

`BusinessBalance = NativeStatementBalance * -1`

Historical results contain no ageing bands. A Subject may legitimately cross through zero and move between debtor and creditor populations at different dates.

### Classification and gross presentation

Classify each Subject independently. Do not net different Subjects, and group by stable `SubjectCode` rather than namespace path.

| Business balance | Accounting position | Human presentation |
|---:|---|---|
| `> 0` | Owed to the business | Debtors / aged debt |
| `< 0` | Owed by the business | Creditors / aged liabilities |
| `= 0` | Settled | Omit from active schedules |

### Current reconciliation

For every Subject:

`StatementBusinessBalance = CurrentOpenInvoiceBusinessBalance + ReconciliationResidual`

The residual is evidence, not an invented aged item. Opening balances and non-invoice statement movement remain visible but do not acquire a fictional due date or age band.

## Product experience

### Credit control

Present the positive current invoice population as **Aged debt — owed to us**, with positive human amounts, ageing totals and invoice drill-down.

### Buying/debit control

Present the negative current invoice population as **Aged liabilities — amounts we owe**, oriented as positive human amounts for the user, with the same current invoice source and due-date bands.

### Historical balances

Provide a separate dated **Debtors and creditors** view. It has an effective-date selector, gross debtor/creditor totals and Subject statement drill-down, but no ageing columns or aged-debt label.

### Shared Subject Browser behaviour

The Subject Browser supplies bounded paging, sorting, accessible totals, empty/error states and links into existing Subject enquiries. Role-oriented wording is presentation, not persisted Subject classification.

## Delivery sequence

### Phase S1 — Source and reconciliation proof

**Status:** COMPLETE — DESIGN ACCEPTED AND SANDBOX VERIFIED.

1. Inventory the Subject statement, invoice status and legacy aged-debt sources.
2. Define separate current-invoice and historical-statement result contracts.
3. Prove signs, due-date bands, credits, payments, zero balances and stable Subject identity.
4. Reconcile current unpaid invoices to the current statement through an explicit residual.
5. Reproduce the legacy current totals, then retire the two legacy views.

Implementation and IAM-01 sandbox evidence are recorded in [`subject-aged-balances-s1-evidence.md`](subject-aged-balances-s1-evidence.md).

### Phase S2 — Subject Browser operational views

**Status:** COMPLETE — MOBILE AND DESKTOP EXPERIENCE ACCEPTED.

1. Add separate Credit Control and Buying/debit-control current ageing experiences.
2. Add a distinct historical Debtors and Creditors balance experience.
3. Reuse existing enquiry components for invoices, payments and statements.
4. Add accessible totals, ageing columns, empty/error states, bounded paging and links without standalone Razor Page query logic.

Implementation note: all report and drill-down data reaches the components through `src/TCWeb/Data/Subjects.cs`, preserving the established Subject data intermediary. That class contains provider-translated LINQ rather than embedded SQL; the database functions are mapped centrally in `NodeContext.SubjectBalances.cs` in preparation for the later PostgreSQL provider migration. The Subject Browser exposes a **Balances** mode with three clearly separated views.

Responsive note: wide layouts retain the compact ageing and enquiry tables. Below the large breakpoint, current schedules use one Subject card per position with the total balance first and labelled age-band tiles beneath it. Invoice, payment and statement enquiries likewise use labelled record cards with their existing drill-down links, while the Browser mode selector becomes a two-column grid on narrow phones. This avoids compressing monetary values and Subject identities into unreadable table columns. The current Bootstrap/Blazor interaction model remains in place for this release; a future MudBlazor conversion may replace the presentation components without changing the data contract.

**Gate:** all three perspectives are independently understandable; current schedules reconcile visibly and historical balances are never described as aged debt.

### Phase S3 — Evidence-consumer contract

**Status:** COMPLETE — AUTOMATED AND IAM-01 RUNTIME VERIFICATION PASSED.

1. Expose a read-only year-end balance snapshot containing the effective date, gross polarity populations, constituent Subject balances, reconciliation state and deterministic digest.
2. Keep the contract authority-neutral and free of Companies House, HMRC, filing credentials or declarations.
3. Define invalidation when relevant source transactions change.
4. Demonstrate how Tax Hub can show summary totals and deep-link to the Subject Browser without cloning its operational UI.

Implementation note: the authority-neutral `subject-balances/v1` contract lives in the Tax UK Application layer and records an immutable, canonically ordered Subject population, gross debtor and creditor totals, reconciliation state and a deterministic SHA-256 snapshot token. `Data.Subjects` remains the TCWeb intermediary and builds the snapshot from the mapped dated-balance function without embedded SQL. Separate aggregate and constituent reads make a concurrent source change visible as an unreconciled snapshot rather than silently presenting mixed evidence.

The Tax Hub year-end Balance Sheet displays only the evidence summary and token, then deep-links to the Subject Browser's Historical view with the accounting period end preselected. Automated contract, provider-translation and IAM-01 browser evidence is recorded in [`subject-aged-balances-s3-evidence.md`](subject-aged-balances-s3-evidence.md). No Azure resource was started or changed.

**Gate:** the snapshot is sufficient for a future guided year-end evidence review and remains reproducible from unchanged accounting records.

### Phase S4 — Prototype retirement

**Status:** COMPLETE — LEGACY ROUTE AND DUPLICATED PAGE MODEL REMOVED.

1. Replace navigation to `/Subject/Reports/DebtorsAndCreditors` with the reviewed Subject Browser entry points.
2. Redirect or remove the prototype only after route usage and bookmarks have been considered.
3. Remove duplicated page-model queries and retain the authoritative services.
4. Update user documentation from reviewed screenshots and terminology.

Implementation note: the obsolete `/Subject/Reports/DebtorsAndCreditors` route, its page model and its Finance-menu entry were removed after the Subject Browser replacement was accepted. Automated source-boundary coverage now prevents the retired route or page files from returning.

**Gate:** no user journey or test depends on the prototype and no second debtor/creditor calculation remains.

## Tests and acceptance

Executable coverage must prove:

- current unpaid-invoice signs and zero-net omission;
- contractual due-date ageing at all band boundaries;
- sales, purchases, credits and paid values;
- reconciliation of current invoices to the current statement through an explicit residual;
- dated statement classification and a Subject changing polarity between dates;
- gross population totals without cross-Subject netting;
- no namespace double counting;
- bounded queries and consistent paging; and
- deterministic year-end evidence for unchanged source data in Phase S3.

## Exclusions

This work does not:

- assign historical age bands to a closed-period statement balance;
- add customer or supplier flags to the accounting model;
- change the native sign convention of Subject statements;
- permit users to type or override accounting balances;
- implement debt collection, automated reminders, payment runs or credit limits;
- implement the Tax Hub year-end wizard itself;
- change the Equity Bridge or statutory filing calculations; or
- authorise a Companies House or HMRC submission.

## Return to the Tax Hub wizard

With Phases S1–S4 accepted, resume the deferred guided year-end evidence review. Its debtor/creditor step should display gross year-end balance totals, reconciliation status and review completion, then deep-link into the Subject Browser's historical balance view. Current aged invoices remain an operational control rather than statutory year-end evidence.
