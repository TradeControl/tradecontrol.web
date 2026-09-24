# Tax Hub Change Log

## 24 September 2026 — Objective 4 Phase 5.3 fraud-prevention boundary

- Added typed browser, trusted-ingress, deployment-topology and vendor fact models for HMRC `WEB_APP_VIA_SERVER` specification 3.3.
- Added direct and explicitly trusted-proxy capture policies, including public IP/port validation, ordered multi-hop construction, spoofed-forwarding rejection and capture-time freshness checks.
- Added all 16 required fraud-prevention header formatters with bounded inputs, structured percent encoding and US-ASCII output validation.
- Added tenant/principal/actor-bound AES-256-GCM evidence sealing with opaque references, atomic file persistence, topology fingerprinting, tamper detection, 15-minute usability and 30-day development/reference retention.
- Added offline coverage for required facts, fractional screen scaling, IPv6 forwarded-hop encoding, multi-hop topology, isolation, persistence, freshness, tampering and redaction, and strengthened architecture guards. No WebHarness submission route or VAT/SA body handling was added.
- Recorded a live specification 3.3 validator result from the ignored historical `.local` client: three errors and three warnings expose that incomplete client's non-public IPs, invalid scaling value, forwarded encoding and missing MFA/licence facts. The result is diagnostic only and does not satisfy Tax Hub's clean-validator acceptance gate.
- Added a development-only WebHarness sandbox flow centred on bodyless, parameterless `GET /diagnostics/hmrc/fraud-prevention/validate`. An injected Swagger script automatically captures browser-only device/session facts through a separate same-origin POST into a 15-minute HTTP-only session; validation then combines those facts with principal/MFA and trusted socket observations, resumes authorization through `/VatMTD`, and returns HMRC's bounded response verbatim. All routes are published through Swagger, and no caller can supply network, vendor, licence, bearer-token or credential facts.
- Changed Swagger authorization from a repeated manual prerequisite to the normal on-demand OAuth behavior: an HMRC operation without a usable grant returns a narrowly marked same-origin `401`, Swagger starts the top-level sign-in/consent journey, and subsequent operations reuse the stored access/refresh grant.
- Corrected WebHarness composition so its encrypted development OAuth grant and local encryption key use the git-ignored `.local/tax-hub/web-harness` runtime root rather than a process-specific temporary directory, allowing an existing grant to survive application restarts.
- Integrated the development WebHarness with the existing Trade Control ASP.NET Identity session. An unauthenticated validation now redirects through the Trade Control login and returns to Swagger; HMRC OAuth runs only when that authenticated Identity principal lacks a usable grant. `POST /diagnostics/hmrc/sign-out` clears only the shared Identity cookie and retains the principal-bound HMRC grant. The cross-host development cookie uses a git-ignored Windows-DPAPI-protected key ring and is not a production hosting decision.
- Added the distinct authenticated `POST /diagnostics/hmrc/disconnect` operation. It retires and overwrites only the calling Identity principal's local HMRC VAT grant, leaves the Trade Control session active, and causes the next validation to start the full HMRC sign-in and consent journey.
- Completed the first live Tax Hub specification 3.3 validator run through Swagger. Browser/device facts, fractional scaling and structured encoding produced no findings; localhost public-IP facts and the absent Tax Hub application identity produced four expected errors, while absent application MFA and licence evidence produced two warnings. HMRC sandbox credentials remain authority-consent credentials and are not reused as Tax Hub user/MFA evidence.

## 24 September 2026 — Objective 4 Phase 5.2 OAuth lifecycle

- Added sandbox Authorization Code Grant initiation with CSPRNG state, `S256` PKCE, exact registered callback composition and tenant/principal/actor-bound single-use callback validation.
- Added the bounded HMRC token endpoint adapter, protected client-secret resolution and typed available/reauthorization-required access outcomes without adding a VAT resource call.
- Added AES-256-GCM file-backed pending/grant persistence keyed by tenant, principal and scope, with atomic replacement, expiry skew, local revocation and closed production composition.
- Added cross-thread/process refresh leases so HMRC single-use refresh tokens rotate once under concurrency; rejected, non-rotating, expired, revoked and wrong-scope grants require reauthorization.
- Kept MTD Income Tax OAuth scopes represented but disabled until Phase 5.15, and added offline fake-clock/fake-endpoint coverage plus architecture guards for the dedicated OAuth boundary.

## 24 September 2026 — Objective 4 Phase 5.1 trusted foundations

- Replaced the Submission adapter placeholders with closed HMRC sandbox/production profiles, a fixed HTTPS host allow-list and a sandbox-only public selector; production remains disabled.
- Added an allow-listed JSON secret provider for the existing ignored VAT sandbox settings, protected in-memory secret values and explicit HMRC client secret references. No credential values or settings-derived host values entered the repositories.
- Added an atomic file-backed attempt store with tenant/principal access checks, one-active-write enforcement, restart recovery, unknown-outcome protection, bounded metadata and seven-year terminal metadata retention.
- Added a separate host-access-controlled content store with 1 MiB payload and 256 KiB response bounds and 30-day raw-content retention. Attempt records accept only opaque relative content references, not authority URLs.
- Replaced the no-op logger with structured diagnostics that hash identifiers and redact token, secret, credential and fraud-related values.
- Added the offline `TradeControl.Tax.UK.Adapters.Submission.Tests` project and strengthened architecture guards without implementing OAuth, fraud headers or HTTP transport.

## 24 September 2026 — Objective 4 Phase 5.0 REST boundary correction

- Pinned VAT obligations and view-return successes to `200` and VAT return submission to `201`, retaining their existing response DTOs and current VAT API v1.0 scopes/media type.
- Carried catalogue eligibility, required OAuth scope, exact success status and response-body/DTO expectations through `PreparedApiContract` into immutable `PreparedApiRequest` values, with fail-closed metadata validation.
- Added the attributable `AuthorityDispatchContext`, typed `PreparedApiOutcome` and a guarded gateway base that rejects deferred, unsupported, preview and error-bearing requests before handler dispatch.
- Corrected VAT special-period handling so `#001` is valid and resolves to `%23001` through the existing path encoder.
- Preserved the approved VAT, MIN and STD canonical bytes/digests and narrowed architecture/reflection bans without permitting credentials, tokens or received response state in prepared requests.

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
