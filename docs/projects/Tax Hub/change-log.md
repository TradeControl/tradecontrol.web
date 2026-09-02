# Tax Hub Change Log

## 2 September 2026 — Sole Trader Objective 2 contract synchronisation

- Expanded `UK-ITSA-SE-CUM` from 16 to 18 writable Component Tax Tags by adding `irrecoverableDebts` and `depreciation`, both expense polarity and unmapped by default.
- Preserved MIN's intentional `CT-CUMEXP -> consolidatedExpenses` Component mapping and preserved all existing MIN/STD income and expense mappings.
- Removed UK Self Assessment calendar assumptions from `Cash.fnTaxBizCumulative`; supplied ranges now require only `PeriodStart <= PeriodEnd` at the Objective 2 boundary.
- Updated the cumulative projection fixture to verify the 18-tag manifest, Component semantics, default unmapped state, preserved MIN mapping, existing STD mappings, signed reversals, arbitrary chronological ranges and reversed-range rejection.
- Confirmed `CC-MINER` is restricted to Bitcoin Main/TestNet node configurations and made no statutory mapping change.
- Deferred missing-row versus zero provenance pending HMRC Sandbox resolution of OQ-1. No disallowable, tax-deducted, annual or Objective 3 contract fields were introduced.

