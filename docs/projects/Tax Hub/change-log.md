# Tax Hub Change Log

## 2 September 2026 — SA Objective 3 contract implementation

- Added independent HMRC SA contract families for Business Details v2, Obligations v3, Self Employment Business v5 cumulative and annual resources, BSAS v7, BISS v3, Individual Losses v6/v7, Tax Liability Adjustments v1, Individual Calculations and Finalisation v8, and Self Assessment Accounts v4.
- Added explicit endpoint descriptors covering HTTP methods, path/query parameters, media/API versions, OAuth scopes, request-body presence, success statuses and request/response types.
- Added offline serialization contract tests and readable JSON fixtures covering detailed/consolidated cumulative submissions, annual data, summaries, losses, liability adjustments, calculations, obligations and accounts.
- Tightened cumulative `periodDates` so the object remains optional for annual/latent sources but, when present, requires both dates; replaced abbreviated read projections with complete supported HMRC wire-response DTOs, including separate current calculation tax-year variants.
- Preserved OQ-1 zero-versus-omission as a population decision and kept the 2026–27 annual Self Employment schema explicitly preview-gated. No Trade Control population, payload harness, HTTP transport, authentication or submission work was performed.

## 2 September 2026 — Sole Trader STD administration mapping correction

- Added `CT-ADMIN` (`Administration Costs`) to the STD accounting hierarchy, with the existing `CA-ADMIN` and `CA-OFFICE` Categories moved beneath it.
- Remapped `adminCosts` from `CA-OFFICE` to `CT-ADMIN`, allowing generic mapping expansion to cover `CC-EXPENSE` as well as the existing office Cash Codes without duplicate contributions.
- Confirmed the corrected STD configuration passes the generic Tax Tag validator with no uncovered enabled business-tax Cash Code warning.

## 2 September 2026 — Sole Trader Objective 2 contract synchronisation

- Expanded `UK-ITSA-SE-CUM` from 16 to 18 writable Component Tax Tags by adding `irrecoverableDebts` and `depreciation`, both expense polarity and unmapped by default.
- Preserved MIN's intentional `CT-CUMEXP -> consolidatedExpenses` Component mapping and preserved all existing MIN/STD income and expense mappings.
- Removed UK Self Assessment calendar assumptions from `Cash.fnTaxBizCumulative`; supplied ranges now require only `PeriodStart <= PeriodEnd` at the Objective 2 boundary.
- Updated the cumulative projection fixture to verify the 18-tag manifest, Component semantics, default unmapped state, preserved MIN mapping, existing STD mappings, signed reversals, arbitrary chronological ranges and reversed-range rejection.
- Confirmed `CC-MINER` is restricted to Bitcoin Main/TestNet node configurations and made no statutory mapping change.
- Deferred missing-row versus zero provenance pending HMRC Sandbox resolution of OQ-1. No disallowable, tax-deducted, annual or Objective 3 contract fields were introduced.

# Company Objective 2 statutory projection

4 August 2026

## Result

The 2026 MIN and STD company templates now keep accounting bootstrap separate from statutory projection. `App.proc_Template_CO_MICRO_CUR_2026` creates the common accounting model; `App.proc_Template_CO_MICRO_CUR_TAX_2026` composes the versioned company semantic sources after the selected accounting profile is complete.

The obsolete mixed `UK-MTD` source and `AC*`/`CP*` vocabulary are deleted. There are no compatibility aliases.

## Sources and manifest

- `UK-CO-ACCTS-2026`: the Company Objective 3 `StatutoryAccounts` surface for the FRS 105 micro-entity/FRC 2026 family.
- `UK-CO-CT-2026`: the supported `CorporationTaxComputation` inputs/results and conditional CT600A surface for CT600 V3 RIM 1.994.
- `UK-CO-CT600-2026`: the CT600 return, attachment indicators, conditional supplementary page and declaration surface.

The accounts source has 28 semantic identifiers, the computation source has 12 and the CT600 source has 13. `TagClassCode = 1` means a directly supplyable accounting Component and is the only class permitted in `Cash.tbTaxTagMap`. `TagClassCode = 2` marks values derived or supplied outside normal Category Tree aggregation. `TagDescription` records whether each non-mapped value is derived, contextual/workflow, external/reviewed, optional when absent, or initially unsupported by accounting structure.

This classification is projection readiness, not an assertion that an authority contract is accepted. Objective 4 retains the external conformance threshold.

## Deterministic accounting mappings

| Semantic identifier | MIN | STD | Mapping |
|---|---:|---:|---|
| `IncomeStatement.Turnover` | 1 contributor | 3 contributors | `CT-TURNOV` |
| `IncomeStatement.OtherIncome` | 1 | 1 | `CT-OTHRIN` |
| `IncomeStatement.CostOfSales` | 1 | 5 | `CT-CSTSAL` |
| `IncomeStatement.AdministrativeExpenses` | 5 | 17 | `CT-STAFFC` plus `CT-OVERHD` |
| `AddBacks.AccountingDepreciation` | 1 | 3 | `CA-DEPREC` |
| `CT600.Turnover` | 1 | 3 | `CT-TURNOV` |

The counts are effective enabled nominal contributors observed in isolated disposable-database executions. Both profiles use the same six non-overlapping mapping roots; STD obtains greater detail from its richer accounting tree.

## Category Tree refinement

`CA-DEPREC` is a dedicated expense-polarity nominal category beneath `CT-OVERHD`. The MIN depreciation charge `CC-DEPRC` and the three STD depreciation charge codes belong to it. `CC-DEPRJ` remains in the neutral asset-movement category because an asset adjustment is not interchangeable with an accounting depreciation charge.

This is an accounting distinction in its own right and makes administrative-expense and depreciation-evidence projection deterministic. It does not classify depreciation as a capital allowance. Capital allowances remain an external or future asset-workflow calculation with no `Cash.tbTaxTagMap` row.

## Unmapped values

The following remain deliberately unmapped:

- balance-sheet facts: derived from period-end account balances, maturity classification and adjustments rather than P&L Category totals;
- profit/loss and statement subtotals: derived and reconciled, avoiding parent/descendant double counting;
- tax on profit: derived from the approved computation, never inferred from the Corporation Tax control/payment account;
- company identity, accounts period, comparative period, approval, signing director and profile statements: contextual/workflow;
- employees, directors' advances, commitments and contingencies: external structured disclosures;
- non-depreciation add-backs, deductions, capital allowances, losses and reliefs: external/reviewed statutory inputs;
- taxable profits, rate application, tax chargeable and tax payable: computation results;
- CT600A: conditional structured input, absent when inapplicable.

An unmapped value therefore stays unsupported/null in the current generic extraction path. It is not fabricated as zero. Mapped values preserve contributor presence, allowing a genuine supported zero to remain distinguishable from absence.

## Verification

- SSDT project build: succeeded.
- Fresh isolated LocalDB MIN bootstrap: succeeded; both source validators returned no errors.
- Fresh isolated LocalDB STD bootstrap: succeeded; both source validators returned no errors.
- Repeat composition: stable at 28 accounts tags, 12 computation tags, 13 CT600 tags and seven mapping roots.
- `Tests/CompanyObjective2Projection.sql`: rollback-only assertions cover obsolete-source removal, manifest cardinality, mapping eligibility, validator output, depreciation/capital-allowance separation and rerun stability.
