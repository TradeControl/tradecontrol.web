# Subjects Work Plan — Polarity-Based Aged Balances

9 October 2026
**Status:** approved for implementation; not started.

## Purpose

Replace the legacy combined Debtors and Creditors report with a reusable, dated statement-and-ageing capability in the Subject Browser.

Trade Control does not assign permanent customer and supplier identities. A Subject's accounting position is determined by its statement balance at the selected effective date:

- a negative balance is owed to the business;
- a positive balance is owed by the business; and
- a zero balance is absent from both active populations.

That single mathematical model serves different human functions. Credit control needs an aged-debt view of amounts owed to the business. Buyers and debit-control users need an aged-liability view of amounts the business owes. The implementation must preserve one calculation while giving each audience appropriate language and actions.

This is a Subjects project. Companies House and Corporation Tax may later consume a year-end evidence snapshot, but neither tax workflow owns the operational calculation or Subject Browser interface.

## Human decision — 9 October 2026

The following direction is approved:

1. keep the domain model polarity-based; do not introduce artificial customer or supplier classifications;
2. implement one dated statement-and-ageing engine;
3. expose two role-oriented Subject Browser experiences over that engine;
4. keep detailed operational investigation in the Subject Browser;
5. let later accounts/tax reviews consume reconciled summaries and immutable evidence rather than duplicate the operational interface; and
6. defer the Tax Hub year-end evidence wizard until this framework is coherent and reviewed.

## Superseded prototype

`/Subject/Reports/DebtorsAndCreditors` and its current implementation in:

- `src/TCWeb/Pages/Subject/Reports/DebtorsAndCreditors.cshtml`; and
- `src/TCWeb/Pages/Subject/Reports/DebtorsAndCreditors.cshtml.cs`

are **deprecated prototypes**. They remain available until the replacement passes its review gate, but they are not an implementation target and must receive no new product behaviour.

The prototype usefully demonstrates period selection, polarity filtering and links to subject details, invoices, payments and statements. Its limitations are that it combines distinct operational roles, is separate from the modern Subject Browser, and does not prove that every drill-down explains the balance at the same effective date.

After acceptance of the replacement, update navigation and bookmarks to the Subject Browser. Remove or redirect the old route only after its useful behaviour and access rules are covered by the new implementation and tests.

## Accounting contract

### Effective-date balance

For each real Subject, calculate the statement closing balance at `AsOfDate`. The same cutoff applies to the population, statement drill-down, open items and reconciliation totals.

Do not use today's balance to explain a historical report. A Subject may legitimately move between negative, zero and positive positions over time and may consequently appear in different operational populations at different dates.

### Classification and gross presentation

Classify each Subject independently from its dated closing balance:

| Native closing balance | Accounting position | Human presentation |
|---:|---|---|
| `< 0` | Owed to the business | Credit control — aged debt |
| `> 0` | Owed by the business | Buying/debit control — aged liabilities |
| `= 0` | Settled | Omit from active schedules |

Do not net different Subjects. For example, 10,000 owed to the business and 7,000 owed by the business remain gross populations of 10,000 and 7,000, not a net balance of 3,000.

Namespace paths and multiple-parent appearances are navigation context, not additional accounting identities. A Subject balance must be counted once by stable Subject identity even when it can be reached through more than one namespace path.

### Ageing

Age open items from their contractual due dates at `AsOfDate`, using reviewed bands initially proposed as:

- not yet due/current;
- 1–30 days overdue;
- 31–60 days overdue;
- 61–90 days overdue; and
- more than 90 days overdue.

The first implementation increment must inspect the native invoice, payment-allocation and statement SQL before fixing these bands or an open-item algorithm. It must not infer settlement merely from a transaction's age or silently allocate payments.

The aged items plus any explicitly identified unapplied, unallocated or non-invoice statement movement must reconcile to the Subject's dated statement balance. A mismatch is visible evidence requiring investigation; it is never hidden in an invented balancing item.

## Product experience

### Credit-control view

Present negative dated positions as positive human amounts under **Aged debt — owed to us**. The view is intended for collection work and should lead naturally to the Subject's dated invoices, payments and statement.

### Buying/debit-control view

Present positive dated positions under **Aged liabilities — amounts we owe**. The view is intended for payment planning, invoice queries and supplier relationships, with the same dated drill-down expressed from the business's payable perspective.

### Shared Subject Browser behaviour

Both views use the same service and result contract. The Subject Browser supplies:

- an effective-date selector with a clear default;
- separate role-oriented entry points or filters rather than one ambiguous combined button;
- population totals and ageing-band totals;
- sorting and bounded paging suitable for operational use;
- drill-down to the selected Subject at the same `AsOfDate`;
- clear native/accounting sign semantics in diagnostics while presenting ordinary positive amounts to users; and
- no free-entry balance or filing adjustment.

Role-oriented wording is presentation, not persisted Subject classification. Access control may later permit different roles to see the two experiences without duplicating their accounting logic.

## Delivery sequence

### Phase S1 — Source and reconciliation proof

**Status:** NOT STARTED.

1. Inventory the SQL views/functions behind `Subject.vwBalanceSheetAudit`, the Subject statement, invoices and payment allocation.
2. Define one bounded service result for a Subject's balance and open-item ageing at `AsOfDate`.
3. Prove sign orientation, due-date semantics, payments/credits, zero balances, historical cutoffs and reconciliation to the statement.
4. Prove stable Subject identity prevents namespace double counting.

**Gate:** human review of representative negative, positive, settled, crossed-period and exceptional subjects. No UI replacement proceeds until the dated totals reconcile.

### Phase S2 — Subject Browser operational views

**Status:** NOT STARTED.

1. Add the separate Credit Control and Buying/debit-control experiences to the modern Subject Browser.
2. Reuse the existing enquiry components for invoices, payments and statements, extending them to preserve `AsOfDate` where necessary.
3. Add accessible totals, ageing columns, empty/error states, paging and links without reintroducing standalone Razor Page logic.
4. Verify that changing the date can legitimately move a Subject between views.

**Gate:** credit-control and buying perspectives are independently understandable while producing the same underlying reconciled accounting result.

### Phase S3 — Evidence-consumer contract

**Status:** NOT STARTED.

1. Expose a read-only year-end snapshot containing the effective date, gross polarity populations, constituent Subject balances, reconciliation state and deterministic digest.
2. Keep the contract authority-neutral and free of Companies House, HMRC, filing credentials or declarations.
3. Define invalidation when relevant source transactions or allocations change.
4. Demonstrate how Tax Hub can show summary totals and deep-link to the Subject Browser without cloning its operational UI.

**Gate:** the snapshot is sufficient for a future guided year-end evidence review and remains reproducible from unchanged accounting records.

### Phase S4 — Prototype retirement

**Status:** NOT STARTED.

1. Replace navigation to `/Subject/Reports/DebtorsAndCreditors` with the reviewed Subject Browser entry points.
2. Redirect or remove the prototype only after route usage and bookmarks have been considered.
3. Remove duplicated page-model queries and retain one authoritative service.
4. Update user documentation from reviewed screenshots and terminology.

**Gate:** no user journey or test depends on the prototype and no second debtor/creditor calculation remains.

## Tests and acceptance

At minimum, executable coverage must prove:

- negative, positive and zero classification at an explicit date;
- a Subject changing polarity between two dates;
- gross population totals without cross-Subject netting;
- due-date ageing at band boundaries;
- invoices, credits, payments and exceptional/unallocated movements;
- aged/open-item reconciliation to each dated statement and to the population total;
- no namespace double counting;
- consistent results across paging and role-oriented presentation;
- authorisation and bounded queries; and
- deterministic year-end evidence for unchanged source data and invalidation after a relevant change.

## Exclusions

This work does not:

- add customer or supplier flags to the accounting model;
- change the sign convention of Subject statements;
- permit users to type or override accounting balances;
- implement debt collection, automated reminders, payment runs or credit limits;
- implement the Tax Hub year-end wizard itself;
- change the Equity Bridge or statutory filing calculations; or
- authorise a Companies House or HMRC submission.

## Return to the Tax Hub wizard

After Phase S3 is accepted, resume the deferred guided year-end evidence review. Its debtor/creditor step should display the gross year-end totals, reconciliation status and review completion, then deep-link into these Subject Browser views. Bank/cash and fixed-asset evidence remain separate native-system steps. Completion of that wizard becomes a pre-submission product gate; it does not alter the external filing contract.
