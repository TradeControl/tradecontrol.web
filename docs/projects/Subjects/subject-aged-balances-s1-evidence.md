# Subject Aged Balances — Phase S1 Evidence

10 October 2026
**Status:** complete; design accepted and verified on IAM-01.

## Outcome

Phase S1 now separates two accounting questions that the earlier design incorrectly combined:

- **current aged invoices** answer which invoices are unpaid now and how overdue they are; and
- **historical dated balances** answer which Subjects were debtors or creditors at a past date.

The implementation is:

- `Subject.fnCurrentAgedInvoiceItems(@AgedOn)` — current unpaid invoice rows aged from contractual due dates;
- `Subject.fnCurrentAgedInvoices(@AgedOn)` — one current summary row per stable `SubjectCode`;
- `Subject.fnCurrentInvoiceReconciliation()` — current statement balance, open-invoice balance and explicit residual;
- `Subject.fnDatedBalances(@AsOfDate)` — historical statement balances without fictional ageing; and
- `Data.Subjects` — the application intermediary providing bounded current population/detail queries and bounded historical population queries to the UI.

The superseded `Invoice.vwAgedDebtSales` and `Invoice.vwAgedDebtPurchases` views and the first-pass `Subject.fnAgedBalanceItems` and `Subject.fnAgedBalances` functions have been removed from the database project and the COSTD26 sandbox.

## Native-source inventory

### Current unpaid invoices

The current source is `Invoice.tbInvoice`. An item is open when `InvoiceStatusCode < 3` and its present balance is non-zero:

`(InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue)`

`Invoice.tbType.CashPolarityCode` converts the invoice balance to business polarity. The modern functions include invoices and credit notes, preserve their signed effect and group by stable `SubjectCode`.

### Due date and ageing

`Invoice.tbInvoice.DueOn` is the contractual due date. The modern schedule uses these bands at the supplied current-date anchor:

- due on or after the date: current;
- 1–30 days overdue;
- 31–60 days overdue;
- 61–90 days overdue; and
- more than 90 days overdue.

The retired views used `InvoicedOn` and a reversed `DATEDIFF`, producing negative `UnpaidDays` values. All 14 live legacy rows were negative, from -603 to -10. Their monetary totals were useful as a transition comparator, but their age measure was not retained.

### Current statement reconciliation

The native Subject statement combines:

- `Subject.tbSubject.OpeningBalance`;
- invoice headers signed to native statement polarity; and
- posted `Cash.tbPayment` rows for non-asset Subject accounts.

The modern reconciliation converts that statement to business polarity and exposes:

`StatementBusinessBalance = OpenInvoiceBusinessBalance + ReconciliationResidual`

The residual is not inserted into an age band. It identifies opening balances or other statement movement that has no contractual invoice due date.

### Historical balances

`Subject.fnDatedBalances(@AsOfDate)` applies the selected cutoff to invoice and posted-payment movements and converts the resulting native statement balance by multiplying it by `-1`. It deliberately returns no ageing fields.

Invoice values and dates remain mutable, and `Invoice.tbChangeLog` has limited retention. A historical balance is therefore reproducible from unchanged accounting facts, not a temporal reconstruction of values later edited in place. Phase S3 must use immutable evidence if it needs that stronger guarantee.

## Stable identity

All populations group directly by `SubjectCode`. Namespace parents and paths are navigation context, so a Subject reachable through two parents is counted once. The rollback fixture explicitly proves this case.

## COSTD26 sandbox evidence

The replacement functions were deployed to the IAM-01 sandbox database `tcNodeDb4-COSIPFVT1-COSTD26`. No Azure service or Azure SQL resource was started or changed.

### Legacy-to-modern transition comparison

Before removing the old views, their monetary totals were compared with the modern current schedule at 10 October 2026:

| Position | Legacy total | Modern total | Variance |
|---|---:|---:|---:|
| Owed to us | 42,139.76 | 42,139.76 | 0.00 |
| Owed by us | 731.98 | 731.98 | 0.00 |

No database object depended on either legacy view. Both views were then dropped from COSTD26 and removed from the database project, together with their unused Entity Framework mappings.

### Current ageing totals

| Position | Subjects | Open invoices | Current | 1–30 | 31–60 | 61–90 | Over 90 |
|---|---:|---:|---:|---:|---:|---:|---:|
| Owed to us | 5 | 42,139.76 | 8,855.72 | 21,425.79 | 0.00 | 9,133.23 | 2,725.02 |
| Owed by us | 3 | 731.98 | 731.98 | 0.00 | 0.00 | 0.00 | 0.00 |

### Reconciliation to the current statement

The current statement totals are 43,339.76 owed to the business and 1,531.98 owed by the business. The difference from current unpaid invoices is fully explained by two opening balances:

| Subject | Statement business balance | Open invoices | Residual |
|---|---:|---:|---:|
| `DatasetMouldingCustomerUk` | 27,320.25 | 26,120.25 | 1,200.00 |
| `DatasetPlasticSupplier` | -1,412.44 | -612.44 | -800.00 |

Every other current Subject has a zero residual. This proves why the invoice schedule and statement total need not be identical while still reconciling exactly.

## Executable coverage

`Tests/SubjectAgedBalances.sql` creates and rolls back an isolated fixture proving:

- all five contractual due-date bands;
- sales and purchase polarity;
- credit-note reduction of debt;
- current paid values and posted-payment reconciliation;
- zero-net omission from the active current schedule;
- an opening balance retained as a visible residual rather than aged;
- exact statement/open-invoice/residual reconciliation;
- a historical Subject crossing from debtor to creditor between two dates; and
- one stable Subject identity across two namespace parents.

The fixture leaves no test Subjects behind. The TCWeb and database projects compile with zero warnings and zero errors.

## Accepted Phase S1 contract

Phase S2 may now build three distinct Subject Browser experiences:

1. current aged debt owed to us;
2. current aged liabilities owed by us; and
3. historical dated debtor and creditor balances without ageing.

The UI must show reconciliation residuals where relevant and must not offer a historical date selector on the current unpaid-invoice schedules.

## Phase S2 implementation note — 10 October 2026

The Subject Browser now exposes these experiences under its **Balances** mode. `SubjectBalanceReports.razor` obtains every report and drill-down through `Data.Subjects`; Razor components do not query the functions directly. `Data.Subjects` composes LINQ over table-valued functions mapped once in `NodeContext.SubjectBalances.cs`, so it contains no embedded report SQL and remains independent of the active relational provider. The earlier standalone aged-balance application service was removed so that Subject data continues to enter the UI through the established Subject intermediary.
