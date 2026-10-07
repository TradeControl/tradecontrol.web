# Companies House XML filing — test submission evidence dossier

7 October 2026

**Status:** Living evidence record. `S00013` has been manually accepted by Companies House. STD Year 1 (`S00014`), STD Year 2 (`S00015`) and STD Year 3 (`S00016`) have been acknowledged without errors by the official test gateway and await the collective Companies House review.

## Purpose

This document records the evidence supporting Trade Control's Companies House XML software-filing test submissions for its initial product scope:

> **Unaudited filleted FRS 105 micro-entity accounts.**

It is intended to provide a single repository document that can be linked when reporting the completed test programme to the Companies House XML team. It records:

- the accounting purpose of each submission;
- the relationship between the successive synthetic accounts;
- the database and document tests applied;
- the Equity Bridge and comparative-continuity results;
- safe request and response identifiers and SHA-256 digests; and
- the distinction between local evidence, gateway acknowledgement and Companies House manual acceptance.

This document must not contain presenter credentials, company authentication values, secrets, complete credentialed GovTalk envelopes or protected correspondence. Exact protected evidence remains in the git-ignored `.local/companies-house` area.

## Evidence rules

1. A locally generated document is not a Companies House submission.
2. A synchronous gateway acknowledgement is not a terminal filing decision or manual acceptance.
3. The three remaining cases are expected to be assessed collectively by Companies House after submission. A case may be recorded as gateway-successful before that review, but the four-case programme is not complete until Companies House confirms the collective result.
4. Submission and transaction identifiers are unique, incremental and never reused.
5. Every external request is separately reviewed and human-authorised. Automatic retry remains prohibited.
6. A planned result is never recorded as passed. Pending rows below must be replaced only with retained evidence from an executed test.
7. Presenter credentials and company authentication remain protected external configuration and are excluded from tracked evidence.

## Test programme

The accepted `S00013` case remains an independent MIN-template baseline. The remaining three cases form a coherent accounting journey for one synthetic STD-template company. Their values must arise from the company's accounting transactions rather than independently manufactured filing figures.

| Case | Accounting period | Filing narrative | Material evidence | Programme status |
|---|---|---|---|---|
| `S00013` | Year ended 30 September 2026, with year ended 30 September 2025 comparatives | Established MIN synthetic company | Proves the baseline database-to-Equity-Bridge-to-filleted-iXBRL route and official manual acceptance. | **Accepted by Companies House** |
| STD Year 1 | 1 October 2023 to 30 September 2024 | First accounts; no comparative period | Proves the first-accounts branch, absence of comparative contexts and a transaction-derived opening trading year. | `S00014` acknowledged without errors; collective review pending |
| STD Year 2 | 1 October 2024 to 30 September 2025 | Subsequent accounts with genuine Year 1 comparatives | Proves exact comparative continuity, retained-profit movement and a second reconciled accounting year. | `S00015` acknowledged without errors; collective review pending |
| STD Year 3 | 1 October 2025 to 30 September 2026 | Further developed ordinary trading following a controlled additive Category Tree evolution | Proves richer FRS 105 balance-sheet activity without changing the previously established Year 1 or Year 2 statutory history. | `S00016` acknowledged without errors; collective review pending |

All four cases remain within the same supported account type. The programme does not introduce full accounts, dormant accounts, audited accounts, FRS 102 or another accounting regime merely to manufacture test variety.

## What the synthetic dataset represents

In this programme, “synthetic dataset” does not mean a collection of independently chosen ledger balances or filing values. It is a deterministic, MIS-ready simulation of a trading business built through the same operational and accounting structures used by Trade Control.

The installed business template supplies the Category Tree, Cash Codes, financial roll-ups, banking structure and tax configuration. The dataset then creates and exercises a connected operating model that includes, as applicable:

- customers, suppliers and their commercial relationships;
- multi-level Objects and bills of materials;
- Projects representing sales and purchase orders;
- project dependencies, operations and scheduling;
- recurring orders, work in progress and delivery activity;
- customer and supplier invoicing, receivables and payables;
- receipts, payments, wages, expenses and bank transfers;
- fixed assets, depreciation, financing and loan repayment; and
- VAT and Corporation Tax consequences across accounting periods.

It also includes ordinary non-MIS transactions because a real business does not route every expense or adjustment through a production order. The resulting statutory figures are therefore downstream consequences of one connected operational history, not numbers inserted to satisfy Companies House tags.

The evidence chain is:

```text
Objects and multi-level BOMs
    → sales, purchase and operational Projects
    → scheduling, delivery, invoicing and payment
    → receivables, payables, cash, assets, tax and retained profit
    → Profit and Loss, Balance Sheet and Equity Bridge
    → statutory facts and Companies House iXBRL
```

This is why the successive-period comparison is valuable: it tests whether a realistic operating history remains mathematically coherent when projected into statutory accounts. It does not claim that synthetic data is equivalent to audited customer evidence, nor that the dataset covers every possible commercial event.

The public [Synthetic Datasets](https://tradecontrol.github.io/admin/admin-manager-dataset/) page shows how an administrator selects and installs a dataset. It is supporting product context; this section records the materially important point for Companies House—the dataset is an operational MIS simulation whose accounting consequences are generated and reconciled by the application.

## STD accounting journey

### Year 1 — formation and first accounts

The STD company begins its accounting history on 1 October 2023. There is no earlier trading period and no comparative column.

The transaction history should include ordinary activity sufficient to establish a real opening year:

- called-up share capital;
- customer sales and receipts;
- supplier purchases and payments;
- trade debtors and creditors at the year end;
- VAT activity and settlement;
- an ordinary fixed-asset acquisition and accounting depreciation;
- longer-term financing where produced by the existing dataset; and
- Corporation Tax arising from the generated profit.

The first-period filing must set `IsFirstAccountsPeriod` and must not supply a comparative period. Comparative contexts, facts, headings and values must be absent from the resulting iXBRL document.

### Year 2 — genuine comparative continuity

The same company continues from 1 October 2024 without resetting its accounting history. Year 2 contains continued trading, asset depreciation, financing movements, debtor and creditor settlement, VAT and Corporation Tax activity, and the resulting retained-profit movement.

The current figures filed for Year 1 must become the comparative figures in the Year 2 filing. They are not a separately generated approximation.

### Year 3 — richer ordinary activity after a controlled tree evolution

The same company continues from 1 October 2025. At the Year 3 boundary, the synthetic exercise introduces three additional ordinary classifications: digital-services turnover, cloud infrastructure and automation equipment. The automation-equipment activity also uses the established Plant & Tools capital-account and depreciation path, making the Year 3 balance sheet materially richer without changing the earlier years.

This is deliberately not the future production Category Tree issue/change-control implementation. Until that lifecycle is designed, the test transition is constrained as follows:

- retain every Cash Code used by Years 1 and 2;
- do not delete or retrospectively remap historical Cash Codes;
- do not change historical statutory roll-up results;
- add new categories and Cash Codes rather than replacing used structures;
- use new codes only for transactions dated on or after 1 October 2025;
- retain the existing FRS 105 statutory projection destinations; and
- prove that the Year 1 and Year 2 canonical statutory facts are unchanged before and after the evolution.

If those invariants cannot be met, the Category Tree evolution fails the test and the Year 3 filing must not be presented. A destructive or retrospectively reclassifying transition is outside this test programme and remains deferred to the planned product-style Category Tree change-control work.

## Generation and filing method

The three STD cases should be generated once as a single deterministic accounting history. The database must then remain unchanged while each reporting horizon is prepared and examined.

The proposed minimal generator extension is a completed-year horizon, preferably expressed as `@CompletedYearCount = 3`. The existing default should preserve the present two-completed-year behaviour. The extension must add the earlier completed year without changing the generic application setup or manufacturing a future reporting period.

For each case:

1. select the completed current period and, where applicable, its immediately preceding comparative period;
2. read the statutory values from the Trade Control database through the production data-provision boundary;
3. run the accounting and source-readiness gates;
4. construct the unaudited filleted FRS 105 iXBRL document;
5. retain the exact local document and its SHA-256 digest;
6. run the no-send Companies House preflight and review the exact materialised request digest;
7. stop for human approval before the one-shot official test exchange;
8. retain the exact protected response and its digest;
9. query status only when justified, using a fresh transaction identifier; and
10. after all three remaining cases have produced successful retained gateway evidence, notify Companies House with the complete matrix and record its collective review outcome.

## Mathematical evidence

### Annual Equity Bridge

For each accounting year, the database evidence must satisfy:

```text
ProfitAfterTax = Profit - BusinessTax

CapitalDelta = ClosingCapital - OpeningCapital

Variance = CapitalDelta
           - (ProfitAfterTax
              + CapitalMovement
              + OpeningAccountPosition)
```

The accepted tolerance is:

```text
ABS(Variance) <= 0.10
```

The result is obtained from `Cash.vwEquityReconciliationByYear` and checked by the Equity Bridge regression. A zero or in-tolerance variance demonstrates that the movement in the company's balance-sheet capital is explained by its accounting result and capital movements; it does not merely assert that the iXBRL arithmetic adds up.

The conceptual basis for this reconciliation is documented in [DEBK — Double-Entry Bookkeeping is a Category Error](https://github.com/iamonnox/papers/blob/main/DEBK/debk.md).

### Successive-period continuity

For every statutory monetary concept present in the supported projection:

```text
Year2.Comparative[tag] = Year1.Current[tag]
Year3.Comparative[tag] = Year2.Current[tag]
```

The comparison must use canonical concept/value pairs rather than whole-document hashes. The same amount legitimately appears under different XBRL context identifiers when it moves from a current column to a comparative column.

The following cross-year relationships must also hold:

```text
Year2.OpeningCapital = Year1.ClosingCapital
Year3.OpeningCapital = Year2.ClosingCapital
```

Any difference must be explained by an explicit, reviewed accounting treatment. Unexplained drift fails the candidate.

### Category Tree preservation

Canonical statutory facts for the completed earlier years must be captured immediately before and after the Year 3 additive evolution:

```text
CanonicalFactsBefore(Year1) = CanonicalFactsAfter(Year1)
CanonicalFactsBefore(Year2) = CanonicalFactsAfter(Year2)
```

The Equity Bridge must also remain in tolerance for all three years after the change. Passing the bridge alone is not sufficient if a previously reported statutory classification has changed.

## Required test record

The following results must be retained for each candidate before it can be reported as successful.

| Evidence area | Required result |
|---|---|
| Dataset identity | Same STD company and one continuous deterministic transaction history across Years 1–3 |
| Source readiness | All database projection rows ready; no unsupported required value silently treated as zero |
| Equity Bridge | Per-year values and variance, with every variance within `0.10` |
| Comparative continuity | Canonical equality report for Year 1 → Year 2 and Year 2 → Year 3 |
| First-accounts structure | No comparative period, contexts, headings or facts in STD Year 1 |
| Tree preservation | Year 1 and Year 2 canonical statutory facts unchanged by the Year 3 additive evolution |
| Document validation | Relevant local contract, presentation and schema assertions pass |
| Local artifact | Exact iXBRL filename, byte length and SHA-256 |
| Official request | Unique submission/transaction identifiers and safe request SHA-256 |
| Official response | Response classification, safe response SHA-256 and bounded error details |
| Completion outcome | The collective Companies House review date and accepted/rejected result covering the remaining matrix |

## Recorded official evidence — S00013

`S00013` is the completed first case and must not be resubmitted.

| Item | Recorded evidence |
|---|---|
| Scope | Unaudited filleted FRS 105 micro-entity accounts; MIN synthetic business |
| Company number | `01234567` |
| Current period end | 30 September 2026 |
| Comparative period end | 30 September 2025 |
| Submission number | `S00013` |
| Submission transaction ID | `2026100600000013` |
| Gateway acknowledgement timestamp | 6 October 2026 at 19:55:08 UTC |
| Request SHA-256 | `CF5CCF3284742868D858130E1E0A3BAECE2254C0EA1C6A38903B0BD2A969F0FA` |
| Response SHA-256 | `7EE6CD0A7CDF03495D65BB1D1714888D4FDCD5B13D78BFF81ED7A89B5DE65C5D` |
| Status transaction ID | `2026100700000014` |
| Status request SHA-256 | `EB16AF1BEFA74DAFC91B82288AD5D187B06AE2B8DD444AF831A22F03AECDB013` |
| Status response SHA-256 | `EEFD6F8F12DE8425CF59CF98818FDB93B5792A0F869AB1192939FE00021326D8` |
| Observed gateway status | `Pending`, with manual review indicated |
| Locally regenerated iXBRL SHA-256 | `EC64CA3FF66F192993DEB0FCC1C22F97BE86E68E53E6833CF8411D4419691FEF` |
| Regeneration comparison | Exact match to the accounts attachment retained with the accepted request |
| Companies House review | Manually reviewed and accepted by the XML team on 7 October 2026 at 12:33 BST |
| Programme result | Successful case 1 of 4 |

The exact request, response, regenerated document and correspondence are retained in the protected local evidence area. Their locations are recorded operationally but are not GitHub evidence links and do not expose authentication material.

## STD candidate result sheets

These sections are intentionally incomplete. They must be populated from retained evidence, not expected results.

### Internal three-year dataset evidence — 7 October 2026

The VAT-registered STD profit scenario was rebuilt with `@CompletedYearCount = 3` and an as-of date of 7 October 2026. The ordinary two-year default was first exercised inside a rolled-back transaction and still produced exactly the original completed years `2024–25` and `2025–26`.

The committed three-year run produced the following read-only regression result:

| Year | Period | Projects | Invoices | Payments | Opening capital | Closing capital | Profit | Business Tax | Profit after tax | Capital movement | Capital delta | Variance |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| STD Year 1 | 1 October 2023–30 September 2024 | 233 | 233 | 169 | 0.00 | 148,095.13 | 142,390.28 | 26,294.15 | 116,096.13 | (3,001.00) | 148,095.13 | 0.00 |
| STD Year 2 | 1 October 2024–30 September 2025 | 252 | 255 | 190 | 148,095.13 | 318,405.91 | 210,494.79 | 40,184.01 | 170,310.78 | 0.00 | 170,310.78 | 0.00 |
| STD Year 3 | 1 October 2025–30 September 2026 | 296 | 274 | 185 | 318,405.91 | 452,742.35 | 166,081.93 | 31,745.57 | 134,336.36 | 0.00 | 134,336.44 | 0.08 |

All three annual variances pass the `0.10` Equity Bridge tolerance. Each year contains Project, invoice and payment activity, and each year-end `Cash.fnTaxBizBalanceSheetUK` projection reports `Ready` for fixed assets, current assets, creditors due within one year and creditors due after one year.

The shared operating model contains 52 Object/BOM flows with 27 multi-level links and 872 Project flows with 754 multi-level links. This confirms that the added historical year exercises the MIS graph rather than adding empty financial periods.

The SQL database project compiled successfully, and the read-only `SyntheticDatasetCompletedYearHorizon.sql` regression passed. This is internal accounting/data-provision evidence only. Local Year 1 and Year 2 iXBRL preparation and comparative-continuity results are recorded below. These figures were retained as the pre-transition baseline before the separately controlled Year 3 additive script was applied. No external request was made for the Year 3 candidate.

The corresponding pre-transition cash statement is retained in this approval package as [STD cash statement — Years 1 and 2 baseline](STD-cash-statement-years-1-and-2-baseline.xlsx).

### Local Year 1 and Year 2 payload investigation — 7 October 2026

The WebHarness `regenerate-document` endpoint prepared both candidates from the unchanged STD database through the Tax Hub's production statutory data-provision and document-preparation path. Both responses were HTTP `200` and were explicitly marked `REGENERATED LOCALLY - NOT FILED`; the official submission endpoint was not invoked.

The locally retained documents are:

- `.local/companies-house/STD-Year1-2024-regenerated.xhtml` — board approval 7 October 2024; SHA-256 `A149F53C76DEB2B73C78AA5612E4CFC28CD5A20D68704DEF815F3C0AB7C1BDC8`;
- `.local/companies-house/STD-Year2-2025-regenerated.xhtml` — board approval 7 October 2025; SHA-256 `8E66F6296418AF66B2C58AA25992758438F2102AB73B398ED253952DD19181FA`.

The Year 1 document contains one balance-sheet heading (`2024`), one instant context (`2024-09-30`) and no comparative facts. The Year 2 document contains the `2025` current and `2024` comparative headings and the corresponding two instant contexts. Every Year 2 comparative statutory monetary fact exactly equals the matching Year 1 current fact.

The statutory values were independently reconstructed from the retained cash statement and compared with the generated iXBRL facts:

| Statutory concept | STD Year 1 current / Year 2 comparative | STD Year 2 current | Difference from cash statement |
|---|---:|---:|---:|
| Fixed assets | 4,000.00000 | 3,000.00000 | 0.00000 |
| Current assets | 184,963.99000 | 372,494.69639 | 0.00000 |
| Creditors due within one year | 37,867.86145 | 54,087.78447 | 0.00000 |
| Net current assets | 147,096.12855 | 318,406.91192 | 0.00000 |
| Total assets less current liabilities | 151,096.12855 | 321,406.91192 | 0.00000 |
| Creditors due after one year | 3,000.00000 | 3,000.00000 | 0.00000 |
| Net assets / statutory equity | 148,096.12855 | 318,406.91192 | 0.00000 |

Prepayments and accrued income, provisions, and accrued liabilities/deferred income are zero in both periods and also match exactly. The Equity Bridge `ClosingCapital` figures are one pound lower than statutory equity because that bridge figure represents accumulated reserves; the iXBRL equity figure also includes the company's £1 called-up share capital. This is an explained classification relationship, not a reconciliation difference.

### STD Year 1 — first accounts

| Item | Result |
|---|---|
| Dataset/build identity | Three-year MIS dataset generated; Year 1 contains 233 Projects, 233 invoices and 169 payments |
| Equity Bridge result | Passed: variance `0.00` |
| First-accounts/no-comparative assertions | Passed locally: one `2024` heading and one `2024-09-30` instant context; no comparative facts |
| Cash-statement reconciliation | Passed: every supported statutory balance-sheet value matched exactly |
| iXBRL SHA-256 | `A149F53C76DEB2B73C78AA5612E4CFC28CD5A20D68704DEF815F3C0AB7C1BDC8` |
| Submission and transaction IDs | `S00014`; `2026100700000015` |
| Gateway evidence | Acknowledgement at 17:19:19 UTC on 7 October 2026; no errors; event log `SA14` |
| Request SHA-256 | `A4D89BD31C18B0A4B38CF066417C378C40AFE0374FFFB937DB88D88C68D6DC0F` |
| Response SHA-256 | `106A08EE62FA31708ED2C8B10FD6672EF96EA651643BA97E9DADB7ECB54F29B6` |
| Companies House collective review | Four-case submission matrix complete; collective review pending |

### STD Year 2 — first genuine comparatives

| Item | Result |
|---|---|
| Dataset/build identity | Same MIS dataset; Year 2 contains 252 Projects, 255 invoices and 190 payments |
| Equity Bridge result | Passed: variance `0.00` |
| Year 1 current → Year 2 comparative equality | Passed locally for every supported statutory monetary concept |
| Cash-statement reconciliation | Passed: every current and comparative statutory balance-sheet value matched exactly |
| iXBRL SHA-256 | `8E66F6296418AF66B2C58AA25992758438F2102AB73B398ED253952DD19181FA` |
| Submission and transaction IDs | `S00015`; `2026100700000016` |
| Gateway evidence | Acknowledgement at 17:19:39 UTC on 7 October 2026; no errors; event log `SA15` |
| Request SHA-256 | `A3C8E1C38E89D7B5BE992F057CA0EE4AA6A01A5AA0D7AF1B2609E36C2F83CC1D` |
| Response SHA-256 | `2442E16AEF7B0AEE07B13A126EAFB3BEB419A079B21B6425404813DCC3619B5D` |
| Companies House collective review | Four-case submission matrix complete; collective review pending |

### STD Year 3 — additive Category Tree evolution

| Item | Result |
|---|---|
| Dataset/build identity | Same three-year MIS dataset plus the tracked `EXEC_DatasetSyntheticMIS_Year3_Additive.sql` transition; Year 3 now contains 299 Projects, 277 invoices and 190 payments |
| Additive classifications | `CA-DIGSV` / `CC-DIGSV` (digital-services turnover), `CA-CLOUD` / `CC-CLOUD` (cloud infrastructure), and `CA-AUTO` / `CC-AUTO` (automation equipment); no existing Category, Cash Code or edge was updated or deleted |
| Activity introduced | One ordinary digital-services sale, one cloud-infrastructure purchase and one automation-equipment purchase, with the equipment capitalised to Plant & Tools and depreciated through the established asset-account path |
| Script safety | Dry-run by default, rerunnable, exact-definition checks, transaction rollback on failure, and fail-closed historical-fact/projection/Equity-Bridge gates |
| Pre/post tree historical-fact equality | Passed: every canonical Year 1 and Year 2 `Cash.fnTaxBizBalanceSheetUK` fact and both earlier Equity Bridges remained unchanged |
| Three-year Equity Bridge result | Passed after transition: variances `0.00`, `0.00` and `0.08` |
| Year 3 supported statutory values | Fixed assets `6,800.00000`; current assets `495,788.80048`; creditors due within one year `40,101.44853`; creditors due after one year `3,000.00000`; all report `Ready` |
| Cash-statement reconciliation | Passed against the tracked [STD cash statement — Year 3 additive transition](STD-cash-statement-year-3-additive-transition.xlsx): all eleven Year 2 current, Year 3 comparative and Year 3 current statutory monetary concepts agree at the source five-decimal precision |
| Local iXBRL preparation | HTTP `200`; evidence class `REGENERATED LOCALLY - NOT FILED`; external send `false`; retained as `.local/companies-house/STD-Year3-2026-regenerated.xhtml` |
| Year 2 current → Year 3 comparative equality | Passed exactly for all eleven supported statutory monetary concepts |
| iXBRL SHA-256 | `F41FC36D982812A95CA2BEB6BC79083286B07686333B9A81BE40E39A60A674B9` |
| Submission and transaction IDs | `S00016`; `2026100700000017` |
| Gateway evidence | Acknowledgement at 18:10:08 UTC on 7 October 2026; no errors; event log `SA17` |
| Request SHA-256 | `15637634A931031ACAFEDD32333B0AE501C5DA65F2D71E8556775C43F2640A7A` |
| Response SHA-256 | `B2529DCF4F75ABB9EA7FF22036047565EE8D7344DEB5140D7252DCA79C88C2D1` |
| Companies House collective review | Four-case submission matrix complete; collective review pending |

## Completion gate and report to Companies House

All four planned submissions now have reproducible evidence and official gateway acknowledgements, with `S00013` also manually accepted. This dossier is complete only when Companies House has applied the collective completion gate to `S00014`–`S00016` and confirmed the overall result. The gateway-acknowledged candidates are not assumed to receive or require separate case-by-case approval messages.

The final report to the Companies House XML team should link to this document and summarise:

- the single supported account type;
- the four accepted submission numbers;
- the first/subsequent-accounts coverage;
- the coherent STD three-year accounting history;
- the Equity Bridge and comparative-continuity results;
- the Category Tree preservation result; and
- the safe request and response digests.

It must not link to or reproduce protected `.local` evidence. Any exact protected artifact requested by Companies House should be supplied through an explicitly reviewed secure route.

## Related project records

- [Tax Hub Work Plan 5](../implementation/tax-hub-workplan-5.md)
- [Company statutory contract design](../implementation/company-statutory-contract-design.md)
- [Companies House development simulator work plan](../implementation/companies-house-development-simulator-workplan.md)
- [Tax Hub findings](../findings.md)
- [Tax Hub change log](../change-log.md)
