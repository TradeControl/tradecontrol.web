# Tax Hub Change Log

## 8 October 2026 — Companies House Objective 4 milestone

- Recorded the XML team's confirmation that the completed four-case micro-entity test programme qualifies Trade Control for a live package covering the document types developed to date.
- Closed Companies House Phase 5.13 while preserving the distinction between approval, administrative package issuance and an authorised live filing.
- Released the Companies House workflow into a separate Objective 5 work plan without claiming completion of the externally blocked Corporation Tax stream or deferred MTD Income Tax work.
- Updated the public test-evidence dossier from pending collective review to the confirmed live-package outcome without reproducing protected correspondence, credentials or personal details.

## 7 October 2026 — Companies House submission audit trail

- Added an explicit `regenerate-document` WebHarness route for rebuilding an S13-style Companies House iXBRL document from the reviewed database and filing inputs without sending it externally.
- Exercised that route against the S13 MIN inputs and retained the result under git-ignored `.local/companies-house/S00013-regenerated.xhtml`. Its SHA-256 `EC64CA3FF66F192993DEB0FCC1C22F97BE86E68E53E6833CF8411D4419691FEF` exactly matches the accounts attachment preserved in the accepted S13 request; the response was explicitly marked `REGENERATED LOCALLY - NOT FILED` and made no external request.
- Added post-exchange application event logging through `App.proc_EventLog` in the originating Trade Control database. Each official submission attempt records its identifiers, safe request/response digests, response classification and the exact iXBRL accounts payload; the credentialed GovTalk envelope and all presenter/company authentication values remain excluded.
- Classified acknowledged exchanges as Information and gateway/unclassified/exchange failures as Error. The WebHarness response now exposes the generated event-log code and whether application logging succeeded, while protected filesystem evidence remains the recovery authority if database logging fails.
- Verified the full Tax Hub solution with zero build warnings and extended the WebHarness hardening suite to 70 assertions. No external Companies House request was made.

## 6 October 2026 — Companies House external dependency update

- Recorded receipt of the official XML software-filing test account and protected test presenter/configuration values without exposing any value in tracked material.
- Recorded the official unique-and-incremental submission-number rule and post-submission notification/manual-review workflow.
- Reclassified account issuance and presenter-credential availability as resolved while retaining fail-closed gates for testing criteria, company authentication, credentialed-envelope review and accepted developer-test evidence.
- Preserved accepted simulator Block A and withdrew the prematurely started Block B-only regression increment. Future simulator work is evidence-led rather than sequence-led.
- Recommended a no-send official-test preflight as the smallest next implementation step; no external Companies House submission or Phase 5.12 transport activation occurred.
- Implemented the reviewed adapter hop through protected filing materialisation, a pinned internal/unregistered HTTP component with offline response parsing, and a Development-only digest preflight that cannot send.
- Extended protected materialisation to `GetSubmissionStatus` and conditional empty-body `StatusAck`, keeping filing-only company authentication and package reference out of polling.
- Aligned both the published contract serializer and preserved Block A simulator with the official control-message examples: `Function=submit` remains on accounts submissions but is absent from status and acknowledgement requests. No external request was made by that increment.
- Added a doubly gated Development-only one-shot official-test path with protected pre-send evidence, no redirects/retries and process-lifetime duplicate prevention.
- Following explicit human authorisation, sent `S00001`/`2026100600000001` once to the pinned official test gateway using synthetic company authentication. Companies House returned synchronous GovTalk error `501`, `Invalid Gateway Target (Class) supplied [AA]`; no filing acknowledgement, authentication verdict, accounts parsing or status outcome occurred.
- Preserved the exact protected request/response evidence, stopped the temporary send-enabled process and marked both identifiers consumed.
- Reconciled the 501 against more specific Companies House evidence: the live accounts wrapper example and recent successful-routing test requests use `Class=Accounts` and `FormIdentifier=Accounts`, and Companies House staff confirmed that Class in March 2026. Corrected the filing contract, simulator and offline transport fixtures accordingly. A fresh `S00002` preflight remains no-send and requires review before any separately authorised exchange.
- Completed the corrected-route `S00002`/`2026100600000002` Development preflight. Database preparation and protected-envelope materialisation succeeded with wire SHA-256 `8EE86CF7D12DB6162C19661BEF44457ADBEC8E6A2153830C1648F84FD8BAC44C`; external sending remained disabled.
- Following separate explicit human authorisation, sent that exact `S00002` request once. The corrected `Accounts`/`Accounts` route passed and Companies House returned synchronous iXBRL validation error `9999`, proving that document validation had been reached without establishing authentication or filing acceptance.
- Corrected `IxbrlDocumentBuilder` to place `ix:resources` directly under the required `ix:header`, added a structural regression assertion and passed the relevant offline suites. Preserved the protected second-exchange evidence, stopped the send-enabled process and marked both identifiers consumed; no retry, poll or third exchange occurred.
- Following a fresh preflight and explicit user instruction, sent `S00003`/`2026100600000003` once. Companies House progressed to the next embedded iXBRL rule and returned error `9999` because `link:schemaRef` was incorrectly inside `ix:resources`; no authentication or filing verdict was reached.
- Reconciled that response with normative Inline XBRL 1.1, then changed the builder to emit a hidden `ix:header` with ordered `ix:references/link:schemaRef` and `ix:resources` contexts/units. Added hierarchy/order regression assertions, retained protected evidence, stopped the send-enabled process and consumed both identifiers. No retry, poll or fourth exchange occurred.
- Following explicit approval of its exact preflight digest, sent `S00004`/`2026100600000004` once. Companies House accepted the corrected header hierarchy and reported an unobtainable HTTP FRC 2026 entry point, five invalid `xml:lang` attributes on fixed facts and one potentially cascading concept-content error. No authentication or filing decision was reached.
- Inspected the official FRC 2026 v1.0.0 ZIP only under git-ignored `.local`, changed the taxonomy entry point to directly retrievable HTTPS and introduced explicit zero-length fixed values that do not emit language metadata. Added regression assertions, retained protected evidence, stopped the harness and consumed both identifiers. No retry, poll or fifth exchange occurred.
- Following approval of its exact digest, sent `S00005`/`2026100600000005` once. All prior diagnostics were absent; Companies House returned only the undeclared `core:AccrualsDeferredIncome` QName error.
- Corrected that authority mapping to the official FRC 2026 monetary item `core:AccruedLiabilitiesDeferredIncome`, added a regression assertion, preserved protected evidence and consumed both identifiers. No retry, poll or sixth exchange occurred.
- Sent `S00010` once after the XHTML compatibility corrections. Companies House returned a well-formed but then-unclassified reply; the one-shot gate failed closed, but this exposed that the raw reply was not retained when classification failed.
- Corrected that evidence gap so any unclassified official response is retained exactly in the protected evidence area with its SHA-256, while remaining fail-closed and non-retryable.
- Sent `S00011` once and retained the official 1,085-byte response. It is the successful non-terminal receipt shape actually emitted by the test gateway: `Class=Accounts`, `Qualifier=response`, an empty body and gateway timestamp `2026-10-06T19:09:58Z`. The classifier now accepts this observed `response` form as an Accounts acknowledgement without broadening any other operation.
- Separated the accounts reporting date from the contract-selection date in the WebHarness. The database has a ready 31 March 2026 balance sheet; TIS 6.0 is selected using the current preparation/submission date, so future-dated accounts are no longer needed.
- Regenerated the MIN and STD synthetic businesses for a completed 30 September 2026 year end with 30 September 2025 comparatives, and verified both through the database-to-envelope preflight.
- Sent `S00012` once and received the precise synchronous requirement for `DateAuthorisationFinancialStatementsForIssue`; added the explicit authorisation date to the filing/document contract and its regression coverage.
- Sent corrected `S00013` once and received an error-free official gateway acknowledgement. Retained protected exact evidence and safe request/response SHA-256 values. This proves synchronous receipt and validation progress, not a terminal filing decision, manual-review acceptance or production approval.
- Reconciled Work Plan 5 through `S00013`. Phase 5.12 remains unsigned pending one narrow restart-safe, specific-submission `GetSubmissionStatus` host path for the existing filing; the terminal result and Companies House review remain Phase 5.13 evidence.
- Added the Development-only `official-test-status-once` WebHarness endpoint for a separately authorised, specific-submission query. It recovers the acknowledged filing from protected evidence, uses only presenter authentication, preserves request/response evidence beside that filing, requires a unique status transaction ID and cannot resubmit accounts or issue `StatusAck`.
- Added restart and failure-gate coverage: a completed status result is returned after restart without another gateway call, while unknown submissions and ambiguous retained attempts fail closed. The endpoint is locally verified but has not yet queried Companies House; one controlled S00013 status request remains before Phase 5.12 sign-off.
- Queried `S00013` once using status transaction `2026100700000014`. Companies House returned `PENDING` with the examiner comment `Pending review of attachment`; the exact response was retained. Local outcome construction initially failed closed because the live response inherited the GovTalk namespace for its status tree rather than using the separately published status namespace. Narrowed the parser to accept both authoritative shapes. Repeating the same local request now recovers the retained response without another external call; the transaction ID remains externally consumed.
- Recovered the retained S00013 status successfully with zero additional gateway calls and exposed its safe evidence, hashes, gateway timestamp and examiner metadata. Signed off Phase 5.12 transport while retaining the pending terminal decision and Companies House manual review under Phase 5.13. Changed the diagnostic status field from its numeric enum representation to the human-readable value `Pending`.
- Began Phase 5.13 with the smallest evidence-led step: notify the XML team that filleted micro-entity test submission `S00013` is pending, request its manual review and ask it to confirm the remaining approval test matrix. Deferred further submissions and polling rather than interpreting the provisional “full and filleted” work-plan wording as an official testing requirement.
- Sent that review request to the Companies House XML team at 12:09 on 7 October 2026, including the safe S00013 status summary and hashes. Phase 5.13 now awaits its manual review and confirmed test criteria; no further filing or poll was made.
- Recorded the XML team's 12:33 manual acceptance of `S00013` and its requirement for normally at least four successful submissions covering the account types intended for support before live approval can be considered.
- Refined the four-case matrix entirely within unaudited filleted FRS 105 micro-entity accounts: accepted MIN baseline plus one coherent three-year STD-company journey comprising first accounts, genuine successive comparatives and richer ordinary Year 3 activity after a constrained additive Category Tree evolution.
- Added a dedicated Companies House test-submission evidence dossier for the accounting narrative, MIS-generated operational provenance, Equity Bridge mathematics, comparative-continuity tests, Category Tree preservation proof, safe gateway digests and manual-review results. Future cases are explicitly pending and no external submission was made.
- Recorded the expected Companies House completion gate as a collective review of the three remaining submissions. Each exchange still has its own human send gate and retained evidence, but the programme does not assume individual approval after every candidate.
- Added the bounded optional `@CompletedYearCount` to the synthetic MIS generator and retained the original two-year default. Rebuilt the STD sandbox with three completed consecutive accounting years through 30 September 2026.
- Added and passed a read-only completed-year horizon regression covering consecutive periods, per-year MIS activity, multi-level Object/BOM and Project flows, statutory balance-sheet readiness and the Equity Bridge. The three annual variances are `0.00`, `0.00` and `0.08` against a `0.10` tolerance. No iXBRL candidate or external request was produced.
- Froze further simulator expansion. Block A remains deterministic regression/failure infrastructure, while Blocks B–D, loopback hosting and persistent simulator storage are unjustified unless a concrete official-test or recovery need emerges.

## 3 October 2026 — Companies House development simulator Block A

- Added the standalone, memory-only and network-free Companies House development simulator plus a dedicated offline scenario suite.
- Modelled deterministic simulated acknowledgement, all published asynchronous status values, specific/presenter polling and conditional `StatusAck` redelivery while preserving exact attachment digests.
- Enforced synthetic-only authentication, duplicate refusal, presenter isolation, unsupported-input rejection and production-project isolation. Every result is classified `SIMULATED — NOT FILED`.
- Added the first transport-neutral Application lifecycle consumer: immutable prepared/acknowledged/pending/parked/terminal transitions, exact transmission/accounts digests, bounded rejection evidence and conditional status acknowledgement, all exercised directly against the simulator.
- Closed the literal Block A evidence and isolation requirements by retaining immutable exact request/decoded-accounts bytes, injecting transaction-reference generation and scoping outstanding general-poll acknowledgements and evidence access by synthetic presenter.
- Added a Development-only WebHarness simulation endpoint that follows the established integration structure: read Trade Control, build statutory accounts and iXBRL through the existing preparation path, pass the exact filing package to the in-process simulator, poll through the Application lifecycle and return safe `SIMULATED — NOT FILED` evidence.
- Prevented opportunistic HMRC browser-fact capture from redirecting an otherwise anonymous local Swagger session into the TCWeb identity bridge; explicit HMRC operations retain their authentication behavior while Companies House simulation remains usable when the local identity application is not running.
- Corrected the earlier AI-derived balance-sheet boundary after its year-end query exceeded SQL Server optimiser resources: generic `Cash.fnTaxBizBalanceSheet` now exposes only non-current/current asset and liability source classifications, while `Cash.fnTaxBizBalanceSheetUK` performs the explicit UK fixed-assets and creditors-due extrapolation. Reviewed adjustments and derived statutory totals remain at the existing Application reconciliation boundary.
- Kept Blocks B–C, the remaining transport-backed development journey, persistence, official XSD commitment and external submission out of scope pending separate review.

## 3 October 2026 — Companies House Phase 5.11 prerequisite assurance

- Acquired the current official general/accounts TIS, live filing/status/acknowledgement schemas and official examples into a temporary evidence directory; recorded URLs, byte sizes and SHA-256 values without committing the downloaded assets.
- Established that the current accounts supplement is TIS 6.0, that the official filing body is `FormSubmission` with a base64 iXBRL `ACCOUNTS` document, and that status polling is a GovTalk XML operation followed conditionally by `StatusAck`, not a REST path.
- Recorded the field-by-field preview mismatch and the narrow Objective 3 correction required for the mandatory iXBRL facts/statements, official filing header, authentication slots, transaction/submission identity and lifecycle models.
- Kept the existing preview, iXBRL bytes, eligibility and all VAT/CT artifacts unchanged. Phase 5.11 remains at human review; no account, credential, network transport, test filing or Phase 5.12 work was created.
- Following explicit approval, corrected Objective 4 scope/sequencing, added the TIS 6.0 mandatory micro-entity account facts/statements, replaced the obsolete logical body with a credential-free GovTalk/FormSubmission preview, modelled GetSubmissionStatus and conditional StatusAck, and added a fail-closed Companies House package gate.
- Recorded the 12:22 XML software-filing test-account application as pending. No approval, testing acceptance, credential receipt, external filing or Phase 5.12 activation is claimed.

## 28 September 2026 — Objective 5 Phase 6.0 product boundary

- Added direct TCWeb references to the Tax Hub Application, Trade Control adapter and Submission adapter while architecture checks prohibit a WebHarness dependency and reverse adapter/contract dependencies on TCWeb.
- Added an API-shaped VAT product workflow contract, server-derived tenant/ASP.NET subject/internal actor/reporting-subject identity and tenant-matched authority dispatch context without accepting VAT boxes, request bodies or tenant identity from browser callers.
- Added fail-closed host options: disabled by default, absolute development stores only in Development, and no production HMRC or Azure-managed composition before the selected facilities are implemented and reviewed.
- Selected a single-tenant-current/multi-tenant-by-design production direction using durable opaque tenant identity, Key Vault application secrets/keys, versioned encrypted Azure SQL grant/workflow records, digest-verified private Blob content and privacy-safe tenant-attribution telemetry.
- Kept Administrators/Managers as an initial replaceable host filing policy rather than a Tax Hub role architecture; actual protected HMRC connection state drives connection presentation.
- Added 20 TCWeb boundary assertions. Both solutions build cleanly and all established Tax Hub suites pass, with the secret-backed data-provision suite using its supported offline path for this boundary-only phase.

## 27 September 2026 — Objective 4 Phases 5.4–5.6 VAT transport milestone

- Added the closed HMRC VAT REST gateway for obligations, view-return and return submission, preserving prepared paths, ordered queries, headers, canonical body bytes and SHA-256.
- Added durable pre-send attempt/payload evidence, exact `201` receipt capture, bounded raw responses, HMRC error-code fidelity, safe enquiry retries and no automatic replay after an ambiguous VAT write.
- Added an authenticated Swagger reconciliation endpoint that retrieves HMRC's return for a short-lived prepared artifact and compares its period and nine boxes with the exact `Cash.vwTaxVatSubmission`-derived body. Matching returns produce `200`; differences produce `409` with field-level evidence.
- Added a reusable reconciliation result carrying the prepared digest, authoritative dataset and snapshot token without recalculating or normalising VAT values.
- Corrected the VAT write's duplicate-protection identity to use the prepared VRN and period key rather than the terminal `returns` path segment.
- Corrected the four HMRC `ExVAT` JSON member names and updated the approved VAT canonical-body digest.
- Added offline coverage for exact reconciliation, mismatch reporting, prepared-source provenance, VRN/period duplicate identity and the Swagger reconciliation surface. The full solution builds without warnings and all nine Tax Hub suites pass.
- Deployed the Phase 5.6 harness to the Azure sandbox and verified the new reconciliation endpoint end to end: HTTP `200`, `matches: true`, no field differences, and intact prepared digest/source provenance.
- Recorded the redacted Azure sandbox evidence: obligations/view `200`, controlled submission `201`, retained receipt fields and an exact nine-box post-submission match. No credential, VRN, bundle or charge-reference value was committed.

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
