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

