# Tax Hub Work Plan 7 — Objective 5: Companies House Accounts Workflow Integration

8 October 2026

## Objective and authority

This work plan defines the Companies House delivery slice of **Tax Hub Objective 5 — Workflow Integration**. It turns the accepted Objective 2–4 company-accounts path into an authenticated TCWeb journey for the initial approved filing scope:

> **Unaudited filleted FRS 105 micro-entity accounts.**

The product outcome is a workflow through which an authorised company user can:

1. review the accounting period, company identity and filing readiness;
2. inspect the exact statutory accounts document prepared from Trade Control;
3. understand validation and reconciliation findings;
4. make the required declarations and approve one immutable filing candidate;
5. submit that exact candidate once through the Companies House XML gateway;
6. see acknowledgement, pending, accepted, rejected, parked or recovery-required status truthfully; and
7. review an attributable filing history with safe evidence references.

Objective 4 established the published TIS 6.0 contract, exact-byte document and GovTalk preparation, protected transport inputs, official test exchange, status query and external test-programme approval. Objective 5 presents and orchestrates those capabilities. It does not construct a second accounts document, recalculate balance-sheet values or expose presenter/company authentication values to the browser.

### Programme position

Companies House confirmed on 8 October 2026 that it was content to issue a live package for the document types developed to date. On 9 October 2026 the XML team confirmed that the live package reference had been created. The reference remains protected external configuration and is not reproduced in tracked documentation. Live presenter credentials have not been supplied through the product configuration, and no live filing is authorised.

Companies House expressly prohibits testing against the live service and may withdraw the package if it is used for testing. All development and conformance work must continue against the official test service using the retained test account. The live reference must not be installed in a test harness, used for probing, or selected by diagnostics.

The human reviewer authorised this Companies House Objective 5 slice independently of Corporation Tax. CT Phases 5.8–5.10 remain externally blocked, and MTD Income Tax Phases 5.15–5.16 remain deferred. This plan neither completes Objective 4 as a whole nor changes those gates.

### Current phase and review gate

This register is the authoritative statement of progress. Implementation evidence does **not** by itself pass a phase: a phase becomes **PASSED** only when the stated human review gate has been explicitly accepted. Work on the next phase must not begin while the current gate is awaiting review.

| Phase | Status | Gate decision/evidence | Consequence |
|---|---|---|---|
| 7.0 — Integration boundary | **PASSED** | Human architecture/security review accepted on 8 October 2026. | 7.1 authorised. |
| 7.1 — Filing readiness | **PASSED** | Human review accepted the server-derived Azure readiness result and no-send boundary on 8 October 2026. | 7.2 authorised. |
| 7.2 — Azure persistence | **PASSED** | Human review accepted the tenant, Blob, Key Vault and non-VAT boundary and authorised continuation on 8 October 2026. | 7.3 authorised. |
| 7.3 — Reviewed filing inputs | **PASSED** | Human review accepted the corrected source-derived balance-sheet boundary, Accounts Mode eligibility treatment and clarified director-note schedule on 9 October 2026. | 7.4 authorised. |
| 7.4 — Exact-document preparation | **PASSED** | Human review accepted the corrected retained XHTML, source-derived employees, filing classifications and the paged `DRAFT — NOT FILED` PDF derivative on 9 October 2026. | 7.5 authorised. |
| 7.5 — Immutable approval | **PASSED** | Human review exercised the Azure preparation-to-approval journey and accepted its attributable, digest-bound `Approved — not filed` state on 9 October 2026. | The approval mechanism is accepted; submission is not yet authorised. |
| Pre-submission accounting-evidence dependency | **PASSED** | Human review on 12 October 2026 exercised the entire wizard, including all accounting-evidence drill-throughs, and accepted the continuous journey. | Phase 7.6 authorised; this internal gate does not itself enable dispatch. |
| 7.6 — Submission and status | **IMPLEMENTED — AWAITING HUMAN REVIEW** | Official-test submission `S00017` was acknowledged on its first and only send. A specific-submission `GetSubmissionStatus` continuation is now deployed against that same durable conversation; no status enquiry has yet been made. | The reviewer may use the single status action for `S00017` and inspect the retained authority result. No second filing is permitted. Production/live remains disabled. |
| 7.7 — History and recovery | **IN PROGRESS — INCREMENT 0 COMPLETE** | Restart continuation and duplicate prevention were exercised against acknowledged, non-terminal `S00017`. | Complete company filing history, safe recovery actions, Event Log evidence and the 7.7 human gate after 7.6 passes. |
| 7.8 — Live activation | **DEFERRED** | The protected live package reference has been received, but there is no live multi-tenant product instance on which to establish the production isolation and operational evidence required by this phase. Live-service testing is prohibited. | Retain the live material as protected configuration. Do not install, probe or use it until a live multi-tenant deployment and explicit filing authority exist. |

**Current position:** Phase 7.5 and the complete guided year-end evidence journey are accepted. Human review exercised the complete wizard and its accounting-detail drill-throughs on 12 October 2026. Phase 7.6's official-test-only path produced first-send acknowledgement `S00017`; that acknowledged but non-terminal conversation is already the required half-completed/restart case. A manual, specific-submission `GetSubmissionStatus` continuation is deployed for that same conversation and awaits its first human-observed use. A specific-submission query cannot require `StatusAck`; the conditional acknowledgement branch remains proven by the published-contract adapter and deterministic simulator rather than being manufactured against Companies House. Phase 7.7 Increment 0 prevents `S00017` being presented as a fresh filing journey after restart; the remaining history/recovery work follows the Phase 7.6 gate. Phase 7.8 is deferred until a live multi-tenant product deployment exists.

Status meanings:

- **NOT STARTED** — no phase implementation has begun.
- **IN PROGRESS** — implementation is authorised but its technical evidence and review gate are not yet complete.
- **IMPLEMENTED — AWAITING HUMAN REVIEW** — implementation and technical evidence exist, but the phase has not passed.
- **CORRECTED — AWAITING HUMAN RE-REVIEW** — a gate finding has been corrected, but the corrected product evidence has not yet been accepted.
- **PASSED** — the human review gate has been explicitly accepted and the next phase may begin.
- **EXTERNALLY BLOCKED** — required authority material or permission is unavailable; internal work must not be represented as resolving it.
- **DEFERRED** — the phase is deliberately postponed because its intended deployment context does not yet exist; it is neither failed nor implicitly authorised.

### Governing records

1. `docs/projects/Tax Hub/programme/tax-hub-spec-programme.md`.
2. `tax-hub-workplan-5.md`, especially Phases 5.11–5.14.
3. `company-statutory-contract-design.md` and the accepted Objective 3 statutory-accounts boundary.
4. `phase-5.11-companies-house-prerequisite-assurance.md`.
5. `approval/companies-house-test-submission-evidence.md`.
6. Current Companies House TIS 6.0, Filing TIS schemas, developer guidance and the protected XML-team correspondence.
7. The implemented source under `src/TCWeb` and `src/tax-hub`.

Official Companies House material governs the external contract. Protected correspondence may resolve operational requirements but must not be reproduced in source, fixtures, tracked documentation or ordinary diagnostics when it contains credentials or personal information.

## Accepted truth chain

The workflow must preserve this chain:

```text
Trade Control accounting records
    → statutory company source and Equity Bridge
    → FRS 105 micro-entity projection
    → exact filleted iXBRL document and SHA-256
    → TIS 6.0 Companies House package
    → explicit approval bound to the document/package digests
    → protected transport-time credential materialisation
    → one GovTalk submission identity and durable attempt
    → acknowledgement and status conversation
    → terminal outcome and safe filing history
```

The accepted test programme proved first accounts, genuine comparatives, successive-period continuity and richer ordinary Year 3 activity. Production code must reuse the same preparation boundary; the WebHarness test payload is not a product API.

## Product and security decisions

- The supported filing is the tested unaudited filleted FRS 105 micro-entity profile only.
- Company authentication is company-specific protected input. Presenter credentials and live-package data are deployment configuration. Neither belongs in the prepared statutory artifact or browser model.
- The company user approves immutable document and package digests. Any source, identity, period, declaration or document change invalidates the approval.
- One company, accounting period and filing profile identifies one active logical filing. Actor changes cannot create a duplicate send.
- A synchronous acknowledgement is receipt, not acceptance. Pending, parked and recovery-required are not terminal failure states.
- An uncertain send is never automatically repeated. Recovery continues the original filing conversation using retained evidence and fresh permitted transaction identities.
- TCWeb hosts the product workflow in-process through Application and adapter boundaries. It does not call Swagger or depend on `TradeControl.Tax.UK.WebHarness`.
- The Block A simulator remains deterministic regression/failure infrastructure. It is not production evidence and does not replace official behavior.
- The existing Accounts workspace and Equity Bridge remain the accounting-reconciliation surface. Before submission, a guided year-end evidence review will also require users to inspect source-derived debtor/creditor, bank/cash and fixed-asset evidence. The operational debtor/creditor calculation and drill-down belong to the Subjects project; this plan consumes its reviewed year-end evidence and does not duplicate it.

## Delivery sequence

| Phase | Deliverable | Principal gate |
|---|---|---|
| 7.0 | Product boundary, persistence and live-configuration decision | Architecture/security review |
| 7.1 | Accounts filing readiness | Accounting-source readiness review |
| 7.2 | Azure workflow and evidence persistence | Tenant/isolation/digest review |
| 7.3 | Reviewed filing inputs | Filing-input provenance and validation review |
| 7.4 | Exact-document preparation and review | Source/document identity review |
| 7.5 | Declarations and immutable filing approval | Human approval-model review |
| 7.6 | Controlled submission and status conversation | Official test/live-disabled lifecycle review |
| 7.7 | Filing history, recovery and evidence presentation | Audit/retention/access review |
| 7.8 | Live-package activation and controlled first live filing | External package and explicit live-enable gates |

Phases are implemented in order. The only completed overlap is Phase 7.7 Increment 0: read-only conversation rehydration and duplicate prevention were necessary to continue Phase 7.6's acknowledged submission safely after a browser restart. That safety dependency does not open filing history, recovery decisions or the Phase 7.7 review gate. Receipt of the protected live package reference does not authorise a live call. Work continues using published contracts, accepted official-test evidence and send-disabled production composition. Phase 7.8 is deliberately deferred until a live multi-tenant product deployment exists; the live service must never be used for testing.

## Phase 7.0 — Integration boundary and persistence

**Phase status: PASSED — human gate accepted 8 October 2026.**

### Purpose

Define the TCWeb composition, durable workflow records and protected configuration boundary before adding filing controls.

### Implementation

1. Reuse the tenant, actor, authorisation-policy, protected-content and durable-attempt principles accepted for VAT where their semantics match.
2. Define Companies House-specific preparation, approval, filing-conversation and history records. Do not force the XML acknowledgement/status lifecycle into the VAT REST model.
3. Bind presenter/package configuration only inside the Submission adapter. Company authentication must be resolved for the selected company at dispatch time through a protected host boundary.
4. Provide a production send-disabled composition until the issued live package is installed and explicitly enabled.
5. Define retention, digest verification, restart recovery, access policy and safe Event Log/support references.
6. Keep WebHarness-only one-shot authorisation, database connection strings and diagnostic payload DTOs outside TCWeb.

### Review gate

Human review accepts the storage/configuration design and verifies that no browser or UI model can obtain credentials, raw protected envelopes or unrestricted authority responses. Stop before Phase 7.1.

### Accepted implementation evidence — 8 October 2026

The human architecture/security gate accepted the product contract, durable record shapes, tenant/company-scoped persistence port, atomic mutation and restart-recovery requirements. TCWeb Companies House options contain no credentials, company authentication or gateway URL. `SendDisabled` is the production default; the separately named `OfficialTestSingleSend` mode is restricted to the official-test authority and Azure-managed persistence. Development files remain restricted to the Development host, and presenter/live-package values remain transport-time inputs.

The reviewer accepted the seven-year operational-evidence policy, Azure-managed product persistence and the stable tenant GUID/tenant-partitioning boundary. `TaxHub.slnx` and `TCWeb.csproj` built without warnings, and the TCWeb Tax Hub suite passed 106 boundary assertions. Phase 7.0 added no UI or external request.

The separate Candidate 4 Azure accounting source was then generated and validated without altering the MIN/VAT databases. TCWeb was rebound through Key Vault, all three health checks passed, and an authenticated workload established that Basic was inadequate while Standard S2 supported interactive development. The diagnostic WebHarness was stopped. The reproducible environment record is retained in [Appendix A](#appendix-a--azure-reference-environment-replication-runbook).

## Phase 7.1 — Accounts filing readiness

**Phase status: PASSED — human gate accepted 8 October 2026.**

### Purpose

Establish whether the selected accounting year is eligible to enter the Companies House workflow without preparing a document or permitting submission.

### Implementation

1. Add a Companies House section to the Accounts workspace for eligible year-end periods.
2. Show company number in an appropriately masked or bounded form, accounting period, filing profile, first/subsequent-accounts status and readiness findings.
3. Present the Equity Bridge and comparative-continuity relationship as supporting accounting evidence, not as Companies House acceptance.
4. Block unsupported regimes and missing, stale or error-bearing accounting evidence.
5. Keep unrelated VAT registration and tax profiles outside Companies House eligibility.

### Review gate

The reviewer confirms the selected year, statutory identity, supported profile and Equity Bridge are server-derived and that no exact document or external request exists. Stop before Phase 7.2.

### Accepted implementation evidence — 8 October 2026

The Companies House tab derives the bounded company number, exact accounting-period bounds, supported unaudited filleted FRS 105 profile, first/subsequent status, statutory-context findings and Equity Bridge result from the server. Missing or error-bearing source evidence fails closed; no exact document or external request exists at this stage.

Azure review corrected a pre-existing calendar assumption: financial-year bounds and year-end selection now follow ordered native `StartOn` dates rather than calendar month order. It also established that accounting figures are bound to the selected period end while current statutory identity/profile eligibility is evaluated at preparation time.

The STD candidate selected `2025-26 SEP` and reported **Ready to prepare document** for company `••3456`, period 1 October 2025 to 30 September 2026, subsequent accounts with comparatives, and an Equity Bridge variance of `0.08`. The initial TCWeb suite passed 112 assertions; the product host remained healthy, WebHarness remained stopped and no Companies House request was made.

## Phase 7.2 — Azure workflow and evidence persistence

**Phase status: PASSED — human gate accepted 8 October 2026.**

### Purpose

Provide durable tenant-partitioned metadata and protected exact-content storage before any filing artifact is created.

### Implementation

1. Provision an isolated Azure SQL workflow database and private Blob evidence container.
2. Enforce tenant-scoped preparation, approval and conversation records and one active conversation per logical filing.
3. Use opaque tenant-scoped Blob references, refuse replacement and verify expected SHA-256 plus retained metadata on every read.
4. Supply the SQL connection through Key Vault and Blob access through TCWeb managed identity; disable public and shared-key Blob access.
5. Include both facilities in readiness while dispatch remains `SendDisabled`.

### Review gate

Human review confirms isolation, role scope, protected configuration, additive schema deployment, empty initial state and healthy fail-closed composition. Stop before Phase 7.3.

### Accepted implementation evidence — 8 October 2026

The isolated `tcTaxHubWorkflow` database holds tenant-partitioned preparation, approval and conversation metadata with tenant-scoped evidence-chain keys and one active conversation per logical filing. Exact statutory and gateway bytes use opaque tenant-scoped references in the private `companies-house-evidence` Blob container. Public/shared-key access is disabled, writes refuse replacement, and reads verify SHA-256 plus retained tenant/digest metadata.

The metadata connection is supplied by a versionless Key Vault reference and Blob access uses TCWeb managed identity. Readiness probes the accounting database, workflow schema and Blob container. At the gate the three workflow tables were empty, the TCWeb Tax Hub suite passed 117 assertions and dispatch remained `SendDisabled`; no filing input, document, approval, conversation or Companies House request existed.

## Phase 7.3 — Reviewed filing inputs

**Phase status: PASSED — corrected human gate accepted 9 October 2026.**

### Purpose

Collect only facts that cannot be derived reliably from the selected Trade Control accounting source and make their provenance explicit before document preparation.

### Implementation

1. Derive company identity, period, first/subsequent status, accounting values and supported profile on the server.
2. Present server-derived defaults for principal activity, accounting policies and average employees with explicit reviewed overrides.
3. Collect the accounts approval date and signing director plus applicable director advances. A director row is one period movement schedule—not four transactions—and must label opening balance, advances/credits, repayments and closing balance. Accounts Mode does not expose the MIS commitment/contingency model in the first release, so collect only an eligibility declaration: default to none for the ordinary case, bind that answer to the user's review, and stop on yes, unsure or unanswered.
4. Treat the supported micro-entity/audit statements as fixed filing-profile facts here; their attributable human declaration remains Phase 7.5.
5. Validate dates, bounded text, director-advance amounts, repeating disclosures and the first-release eligibility declaration server-side. Do not accept company number, accounting periods, source balances, balance-sheet allocations, commitment/contingency schedules, credentials or transport identifiers from the browser.
6. Keep prepayments/accrued income, provisions and accruals/deferred income within the native source-derived balance-sheet presentation. The present four-bucket SQL projection already includes their accounting effect; until a source-backed bridge-neutral decomposition is introduced, their additional statutory presentation lines are system-derived zero. A future non-zero split must subtract the same amount from its containing native bucket so total assets, liabilities and the Equity Bridge remain unchanged.
7. Produce a reviewable filing-input draft only. Do not generate iXBRL or create a preparation record.

### Accepted implementation evidence — 9 October 2026

Human review rejected the initial free-entry fields for prepayments, provisions and accruals/deferred income because they duplicated native accounting measurements and could invalidate the Equity Bridge. The corrected implementation removed them from the browser and core contract. Legacy diagnostic members remain zero-only for fixture compatibility and validation rejects every non-zero override.

The browser now collects only non-ledger facts that cannot be derived reliably: approval date, signing director, principal activity, accounting policies, average employees and separately reviewable director-advance movement schedules. Accounts Mode does not expose the wider MIS commitments/contingencies model in the first release; no, yes/unsure and unanswered are handled explicitly and fail closed where appropriate.

The source projection retains its four native balance-sheet buckets. Additional statutory presentation lines remain system-derived zero until a source-backed, bridge-neutral decomposition exists. Companies House readiness is scoped to Companies House evidence, so a non-VAT-registered company is not blocked by absent VAT registration or unrelated VAT/Corporation Tax review state.

The company-contract suite passed 87 assertions, WebHarness hardening 73 and the TCWeb Tax Hub suite 126. Azure deployment `320a90bd-d323-4cab-9a3c-7903dfbe7e12` was healthy. This phase created no document, durable preparation, approval, conversation or external request.

### Review gate

Evidence already established:

- [x] The corrected warning-free build, company-contract suite, WebHarness hardening suite and 126-assertion TCWeb suite pass.
- [x] Azure liveness/readiness and the authenticated closed-year UI check pass.
- [x] Open-year and non-year-end selections remain blocked.
- [x] Review creates no document, durable preparation, approval or external request.
- [x] The browser and core reviewed-input models contain no prepayment, provision or accrual balance-sheet override.
- [x] Legacy diagnostic callers are rejected if they supply a non-zero value for those allocations.
- [x] Accounts Mode exposes no commitment/contingency entry schedule; unanswered and yes/unsure eligibility decisions fail closed.
- [x] Corrected Azure deployment `320a90bd-d323-4cab-9a3c-7903dfbe7e12` succeeded and liveness/readiness return HTTP `200`.

Human decisions recorded 9 October 2026:

- [x] Every browser-supplied value is a genuine non-ledger filing input rather than a replaceable accounting or identity fact.
- [x] The corrected Azure form contains no balance-sheet allocation fields and the native source/Equity Bridge remains authoritative.
- [x] The validation, non-VAT scoping and first-release Accounts Mode eligibility treatment are acceptable for the supported filing profile.
- [x] Director advances remain a note which does not alter the accounts. The first release retains the reviewed movement schedule; transaction linkage is a desirable later enhancement rather than a condition of this filing slice.

**Gate verdict: PASSED — Phase 7.4 authorised.**

## Phase 7.4 — Exact-document preparation and review

**Phase status: PASSED — Phase 7.5 authorised.**

### Purpose

Prepare, retain and render the exact supported filing candidate without permitting submission.

### Implementation

1. Prepare through `CompaniesHouseAccountsPreparer` using the selected accounting source and reviewed Phase 7.3 input draft.
2. Atomically retain the exact iXBRL/package, digests and source evidence in the Phase 7.2 facilities.
3. Render a safe review of the retained exact document. Do not rebuild iXBRL in TCWeb.
4. Verify protected-content digests on retrieval and block missing, stale or error-bearing evidence.

### Increment 1 — preparation-state correction

The core preparer no longer emits the obsolete `CH-PENDING-DEVELOPER-TEST` error: official developer testing and approval of the supported document scope are established evidence. The credential-free GovTalk/FormSubmission artifact remains a `Preview` and now carries the warning `CH-TRANSPORT-MATERIALISATION-REQUIRED`. That warning accurately records that presenter credentials, company authentication and the protected live package reference are absent by design and must be introduced only by the transport materialiser. It therefore permits exact preparation and retention without making the preview dispatchable or exposing protected values.

This is an enabling increment only. No TCWeb prepare action, protected Blob, workflow row, approval or external request has yet been created. Phase 7.4 remains in progress.

### Increment 2 — retained exact-document review

The authenticated Accounts workspace now converts the reviewed Phase 7.3 facts and server-derived accounting source into the exact filleted iXBRL through `CompaniesHouseAccountsPreparer`. It retains the document and credential-free GovTalk package as immutable protected Blob objects, stores tenant/principal-scoped preparation metadata, verifies the document digest on every retrieval and expires the review after 24 hours. Numeric company registrations stored without their leading zero are normalised to the required eight characters before projection; prefixed registrations remain unchanged.

The browser receives only a time-limited preparation reference, document/package digests, expiry and a protected same-origin document route. It cannot supply statutory values, identity, credentials or transport configuration. The retained bytes are presented as sandboxed HTML with no referrer, `no-store`, restrictive content security policy and same-origin framing; serving the browser representation as HTML does not alter the retained iXBRL bytes or digest.

The Azure STD review prepared the 30 September 2026 subsequent-accounts candidate with genuine 2025 comparatives and displayed it under **Prepared — not approved or filed**. Repeated preparation produced identical evidence:

- document SHA-256 `FE0F9778140ACD4F441588415EF2BCA888FC78F512DD0C2C9ECFC0F55CE8251F`;
- credential-free package SHA-256 `510AB139AEE860C6A97410946BBC090EF265E33F861CF9E4EDE2DD6E6FB56E0A`.

Deployment `5dd2a057-93d9-40f9-b3af-d13bd7f8afc3` completed successfully, `/health/live` and `/health/ready` both returned HTTP `200`, and the TCWeb Tax Hub boundary suite passed 132 assertions with a warning-free build. No approval, conversation, gateway request or Companies House submission was created. Phase 7.4 stops here for human review.

### Increment 3 — statutory number presentation correction

The first human document review found that the balance-sheet headings did not state the presentation currency and the monetary facts exposed raw decimal strings. The iXBRL builder now renders headings such as `2026 £'s`, formats monetary facts with thousands separators and two decimal places, and displays negative amounts in brackets.

This is implemented at the iXBRL fact boundary rather than as a TCWeb-only overlay. Formatted facts declare the published 2020 `ixt:num-dot-decimal` transformation; a negative fact retains the required `sign="-"` metadata while its visible table cell supplies the brackets. Regression evidence verifies the currency headings, `35,000.00` positive presentation and `(35,000.50)` negative presentation. The company-contract suite passes 90 assertions and the TCWeb boundary suite passes 132 assertions, both with warning-free builds.

Azure deployment `1282caa3-9dbd-40a1-8b17-3a257ccc7601` completed successfully and both health endpoints returned HTTP `200`. The deployment restart invalidated the authenticated browser session before a new retained candidate could be generated; the earlier digests above therefore describe the pre-format review artifact and are superseded for approval purposes. Phase 7.4 remains fail-closed pending authenticated regeneration and human review of the corrected document.

### Increment 4 — accounting-period coherence and hard-copy download

Human review identified that the year and period selectors could appear inconsistent. The cause was not the statutory period resolver: when Accounts received a selected year with no selected month, its workspace service substituted the system-wide active period. A historical year could therefore be paired with a period from the active year.

The Accounts interaction now selects the final period belonging to the chosen financial year whenever the year changes or the user enters Accounts without a valid period. The service independently enforces the same invariant: an omitted period resolves to that year's final period, while an explicit period from another year fails closed. Live Azure verification confirmed that selecting `2025-26` produces `2025-26 • 2025-26 SEP` and readiness `01 Oct 2025 – 30 Sept 2026`.

The retained review card now also offers **Download accounts draft**. The endpoint does not invoke preparation. It uses the existing authenticated read operation, including server-derived tenant/principal ownership, 24-hour expiry and SHA-256 verification, and returns the same retained bytes as a non-cacheable XHTML attachment suitable for saving or printing. Regression coverage constructs the controller around known retained bytes and verifies exact byte identity, attachment filename, media type and cache policy.

The TCWeb boundary suite passes 135 assertions and the Company contract suite passes 90 assertions with warning-free builds. Azure deployment `3e721a5a-11dc-4983-a699-f306bf1205e3` completed successfully and both health endpoints returned HTTP `200`. No approval, dispatch or Companies House request was created. Phase 7.4 remains fail-closed pending regeneration and human review of the corrected retained document.

### Increment 5 — filing-classification presentation

The retained document review exposed four blank values under **Accounts information**. The XBRL was not missing those classifications: FRC `fixedItemType` facts deliberately have an empty body. Accounts status, accounts type and accounting standard are conveyed by context dimensions, and ordinary trading is conveyed by the published default member of the entity-trading-status dimension. The presentation renderer had shown the empty machine fact without its contextual interpretation.

The human document now labels the same unchanged facts as **Unaudited — audit exempt, no accountants' report**, **Filleted accounts**, **FRS 105 micro-entities** and **Trading**. It also uses human labels for the dates and **Dormant company** and explains that the classifications come from the supported filing profile and XBRL contexts. Tests verify both the visible wording and that every accepted fixed-item fact remains zero-length and carries no inappropriate `xml:lang` value. These are profile-derived classifications, not editable fields.

A paged PDF bearing `DRAFT — NOT FILED` was recorded here as a useful hard-copy derivative. Human review subsequently authorised its inclusion in Phase 7.4; Increment 6 implements it without changing the exact retained XHTML filing artifact.

The Company contract suite passes 92 assertions and the TCWeb boundary suite passes 135 assertions with warning-free builds. Azure deployment `3c8399a6-a09b-4173-8a59-f8757afed0d1` completed successfully and both health endpoints returned HTTP `200`. No approval, dispatch or Companies House request was created. The corrected document still requires regeneration and human review before Phase 7.4 can pass.

### Increment 6 — paged draft PDF and staged User Guide evidence

Human review accepted the corrected XHTML presentation, including the employee count sourced through the Subject Browser, and authorised a paged hard-copy review document within this phase. The retained review card now separates **Download draft PDF** from **Download filing XHTML** so a user cannot reasonably mistake the derivative for the exact filing artifact.

The authenticated PDF route first performs the same tenant/principal ownership, 24-hour expiry and SHA-256 verification as the exact-document viewer. Only then does it render the retained iXBRL into A4 pages. Every page bears `DRAFT — NOT FILED`; the complete source iXBRL digest is included in the PDF metadata and footer. The renderer does not query accounting data, mutate the retained evidence or create a filing package. Its cross-platform font boundary is explicit and fails closed when a reviewed host font is unavailable.

The TCWeb boundary suite passes 137 assertions. In addition to route and source-binding checks, the renderer produced a substantive PDF from a test document. The previously downloaded retained STD XHTML was also rendered as a two-page A4 review copy, converted to page images and visually inspected; the final filing-information block is kept together rather than orphaning individual lines. The NuGet vulnerability service was unavailable and emitted `NU1900`, so its automated audit must be repeated when reachable; compilation and executable tests succeeded.

The public User Guide will record this filing journey incrementally. Screenshots must come from a reviewed, non-secret Azure product state after the relevant phase gate; planned controls must not be presented as available. The first guide increment should cover Accounts selection, readiness/input review, exact preparation, on-screen review and the two clearly distinguished downloads after this PDF gate is accepted.

Azure deployment `33d1d509-7508-4ecd-83ab-88beedf4351b` completed successfully with one healthy instance. `/health/live` and the database-backed `/health/ready` both return HTTP `200`. Product dispatch remained disabled and no Companies House request was made. Human review subsequently accepted the complete XHTML/PDF journey and passed Phase 7.4.

### Review gate

The reviewer traces the displayed figures and document digest to the accepted preparation path and confirms that review performs zero external sends. Stop before Phase 7.5.

## Phase 7.5 — Declarations and immutable approval

**Phase status: PASSED — human gate accepted 9 October 2026. Product dispatch remains disabled.**

### Purpose

Record an attributable human decision for one exact filing candidate.

### Implementation

1. Present the current Companies House filing declarations and warnings required for the supported profile.
2. Require explicit confirmation and an authorised Trade Control actor.
3. Bind approval to tenant, company identity, period, profile, declaration version, source evidence, document digest and package digest.
4. Expire approval when any bound input changes or the approved candidate becomes stale.
5. Distinguish **approved — not filed** prominently from authority receipt or acceptance.

### Implemented increment

The retained reviewed-input model is now protected alongside the exact iXBRL and credential-free package. A preparation records opaque references and SHA-256 values for all three artifacts, plus the selected accounting year/period needed for deterministic revalidation. Preparations created before this evidence extension are deliberately ineligible for approval and must be regenerated.

Only an authenticated Administrator or Manager may approve. The approval service verifies the versioned declaration, resolves tenant, authenticated ASP.NET principal, internal actor and reporting subject on the server, checks the preparation's subject ownership and 24-hour lifetime, and rereads all three protected artifacts through their digest-verifying Blob boundary.

Before writing approval, the service rebuilds the current accounts document from the retained reviewed inputs and live accounting source. Company identity, selected period, source-version snapshot and document SHA-256 must still match the reviewed preparation. Any change requires a new preparation; the browser cannot approve stale bytes. A unique tenant/preparation index makes approval idempotent and prevents competing approvals for one candidate.

The stored approval binds the tenant, authenticated principal, internal actor, company identity, period, supported filing profile, declaration version/hash, source evidence, document digest and package digest. The UI presents the exact declaration with an unchecked confirmation and labels the outcome **Approved — not filed**. It exposes no submit control and dispatch remains `SendDisabled`.

The public Companies House User Guide remains deliberately deferred until the whole review/approval/submission/status journey has passed its gates. Before live release, star the PDFsharp repository as a project courtesy; this reminder is not a technical or release gate.

The tracked additive index was applied to the isolated Azure workflow database while it contained seven historical preparations, zero approvals and zero conversations. The TCWeb boundary suite passes 140 assertions. Deployment `bc7ea676-3397-4fb4-a368-8a1e31c6f5eb` completed successfully and both health probes return HTTP `200`. No approval was created by deployment and no Companies House request was made.

Human review subsequently exercised the deployed journey and accepted the exact-document approval, attribution and prominent **Approved — not filed** result. That acceptance passes the Phase 7.5 mechanism; it does not authorise submission.

### Review gate

Human review confirms the declaration text, permissions, accessibility and digest binding. No send control is enabled before acceptance.

## Pre-submission accounting-evidence dependency

The review identified a valuable control that is broader than Companies House: before approving statutory accounts, a user should review the economic evidence behind material balances rather than beginning with secondary filing facts. The resulting **guided year-end review** will cover dated debtor/creditor positions, bank/cash balances and fixed assets/write-downs before statutory input review and exact-document approval.

Detailed debtor/creditor work does not belong in this filing plan. Trade Control has no permanent customer/supplier identity; it has dated Subject-statement polarity. The separate [Subjects aged-balances work plan](../../../Subjects/subject-aged-balances-workplan.md) therefore owns one dated calculation and two human operational perspectives: Credit Control for negative positions owed to the business, and Buying/debit control for positive positions owed by the business.

Once that plan supplies an accepted year-end evidence snapshot, this workflow will add a resumable guided review that:

1. shows gross debtor and creditor totals reconciled to the selected year-end accounts and links to the Subject Browser details;
2. reviews native bank/current/reserve balances and fixed-asset cost, write-down and net-book-value evidence;
3. records attributable completion against deterministic source evidence;
4. invalidates completion and all dependent preparation/approval when relevant source evidence changes; and
5. continues clearly into statutory filing inputs and document preparation without accepting free-entry accounting values.

This is an internal product-quality gate, not a Companies House contract requirement and not part of the XML transport. Phase 7.6 remains `NOT STARTED` until the Subjects framework and the resulting guided review have passed human review.

### Guided review increment 1 — scope and Subject balances, 10 October 2026

The accepted Subjects project now supplies an authority-neutral, deterministic `subject-balances/v1` snapshot for the selected accounting year end. Human usability review rejected a separate **Year-end review** tab because it exposed an internal control concept as a competing destination: a user naturally selected **Companies House** and then could not see how to continue. The Accounts workspace navigation now presents the action **File Your Accounts**, rather than requiring the user to infer the journey from an authority name. It opens one destination headed **File your accounts**, with an explicit **Start filing journey** action. That continuous journey begins with the wizard-shaped evidence sequence—scope, debtors/creditors, bank/cash, fixed assets and completion—then unlocks filing inputs, exact-document review and approval in place.

The first increment implements the scope and debtor/creditor steps. It shows the gross year-end populations and reconciliation status, identifies the exact snapshot digest, states explicitly that the evidence is a historical balance rather than reconstructed aged debt, and opens the Subject Browser's Historical view at the same effective date in a separate tab. The wizard consumes only the bounded summary; operational constituent detail remains in the Subject Browser.

Confirmation is bound in the component to the selected effective date and snapshot token and resets when either changes. The overall review therefore cannot complete, and the Companies House filing-input/preparation component now fails closed behind the incomplete review. Existing dispatch remains `SendDisabled`; this increment creates no preparation, approval, submission or external request.

### Guided review increment 2 — native bank and cash evidence, 10 October 2026

The second increment reads the last native statement balance for every open cash account at the selected year end. It applies the accounting system's established classification—an account with a cash code is **Current**, while an account without one is **Reserve**—rather than introducing filing allocations or user-entered values. Each account is shown with a Cash Manager drill-through.

The ordered account population, dated balances, current/reserve totals and corresponding native balance-sheet totals are bound into a deterministic `bank-cash/v1` SHA-256 digest. Confirmation is enabled only when the individual account total exactly reconciles to the `CASH + BANK` balance-sheet total and the earlier Subject step has been confirmed. On the STD sandbox at 30 September 2026, current £27,880.00 plus reserve £425,769.05 reconciled to the balance sheet with a £0.00000 difference.

The evidence confirmation resets if either the Subject or bank/cash digest changes. Fixed assets and final digest-bound completion remain visibly pending, so filing inputs, preparation and approval remain inaccessible and no external request can be made.

### Guided review increment 3 — mapped fixed assets and completion, 10 October 2026

The third increment uses the same reviewed Category Tree roots as the statutory accounts projection (`CA-ASSET` and `CA-DEPREC`) to select fixed-asset accounts. It does not infer fixed assets from `AccountTypeCode` alone because that native account type also contains capital and long-term-liability accounts. For every mapped asset it presents opening carrying value, posted increases, posted reductions/write-downs and closing carrying value, with a Cash Manager drill-through for transaction-level review.

The closing asset population reconciles to the native mapped balance-sheet accounts and was initially bound into `fixed-assets/v1`. On the STD sandbox at 30 September 2026, the mapped asset accounts and balance sheet both total £6,800.00; the Companies House `BalanceSheet.FixedAssets` projection independently returns £6,800.00. The evidence UI explicitly states that a reduction may be a write-down, disposal or another reviewed adjustment and requires the user to inspect the underlying account rather than inventing a classification.

The final review page lists all three reviewed evidence contracts and binds their source digests, company display identity and period into `year-end-evidence-review/v1`. A deliberate final confirmation raises the completion event that unlocks the existing filing-input, exact-document and immutable-approval journey below. Any source digest or selected period change resets every confirmation and relocks the journey. This review completion is transient workflow state; the later accepted immutable accounts approval remains the durable authority for the prepared document and package.

No external submission is enabled or made. The complete guided review is **READY FOR HUMAN REVIEW**; Phase 7.6 remains unopened.

### Guided review increment 4 — scheduled asset write-offs, 10 October 2026

Trade Control asset accounts use the same native `Cash.tbPayment` mechanics as bank accounts. The existing `Cash.proc_AssetReversal` workflow already expresses a depreciation/write-off plan as dated Unposted reductions; posting an instalment changes its native status and lets the ordinary accounting views, Equity Bridge and statutory projection consume it. The filing journey therefore does not introduce a depreciation ledger or a browser-entered balance-sheet adjustment.

The fixed-assets step now reads recognised Unposted `Write off…` reductions from the same mapped asset accounts and presents the existing schedule alongside the posted carrying-value evidence. Instalments after the selected accounting date remain visibly Unposted. An instalment due on or before that date can be selected and posted only after an explicit confirmation that identifies its payment code, date and amount.

The server action revalidates the transaction at use time: it must still be Unposted, reduction-only, due by the selected accounting date, use the established write-off reference convention and belong to a native asset account. Existing ownership rules apply, with Manager/Administrator privilege matching Cash Manager. The action uses the existing asset-posting path rather than a direct balance override.

Any due scheduled write-off keeps fixed-asset confirmation disabled. After posting, the whole Accounts workspace is reloaded; the revised native balances, Equity Bridge, statutory projection and evidence digest are recomputed, while the changed `fixed-assets/v2` snapshot invalidates the fixed-asset and final confirmations. Earlier Subject and bank/cash confirmations survive only when their own evidence digests are unchanged.

This increment deliberately consumes a pre-existing schedule; it does not create or infer useful lives, methods or residual values inside the filing journey. Schedule creation and maintenance remain Cash asset-account responsibilities. No Companies House request is enabled or made.

Human review on 12 October 2026 stepped through the entire wizard and successfully used its review-detail drill-throughs. The reviewer accepted the continuous evidence journey. This passes the pre-submission accounting-evidence dependency and authorises Phase 7.6; it does not constitute filing approval, enable transport or make a Companies House request.

## Phase 7.6 — Controlled submission and status conversation

**Phase status: IMPLEMENTED — AWAITING HUMAN REVIEW. Official-test-only status continuation deployed; production/live dispatch remains disabled.**

### Purpose

Submit one approved package safely and continue its Companies House conversation without duplicate filing.

### Implementation

1. Dispatch the exact approved artifact once through the Companies House gateway port.
2. Allocate and persist unique submission/envelope/transaction identities according to the live package rules.
3. Present acknowledgement separately from terminal status.
4. Continue `GetSubmissionStatus` only when due and permitted. Send conditional `StatusAck` only where the returned contract requires it.
5. Preserve errors, examiner comments and safe authority references without exposing raw protected content.
6. Treat lost/ambiguous replies as recovery-required and prohibit ordinary resubmission.
7. Exercise the lifecycle first through recording handlers, Block A failure scenarios and the official test environment where still available. Production remains send-disabled.

### Increment 1 — durable approved-filing staging

Implemented on 12 October 2026. An authorised Administrator or Manager can now pass an immutable approval into a server-only staging boundary. The boundary reloads the tenant-scoped approval and preparation, verifies both protected document and package digests, and persists one active `Prepared` conversation for the logical company/period/profile filing. Repeating the action resolves the same durable conversation; the database's filtered unique index remains the final concurrent-activation guard.

This increment is deliberately not exposed as a filing action. It allocates neither a Companies House submission number nor a gateway transaction identity, has no gateway dependency and cannot make a network request. The TCWeb boundary suite passes 149 assertions. The next increment must compose authority identities and a recording gateway around this already-persisted conversation, then demonstrate acknowledgement and ambiguous-send behavior before any external dispatch can be considered.

### Increment 2 — persist-before-dispatch recording lifecycle

Implemented on 12 October 2026. An internal, deliberately unregistered coordinator now continues the staged conversation through a transport-shaped recording boundary. It reloads the exact approval chain, verifies the protected preview and accounts document, materialises and retains the exact proposed request, then atomically changes `Prepared` to `Dispatching` before invoking the exchange. Only the winning state transition may exchange; repeat or concurrent activation resolves the durable conversation without replay.

A classified acknowledgement is retained separately and advances the conversation to `Acknowledged`, not `Accepted`. Gateway errors advance to `GatewayRejected` with bounded typed error evidence. An exception, mismatched response, malformed response or unclassified response retains all safely available bytes and leaves the original conversation `Dispatching` with `RecoveryRequired = true` and a safe support reference. It never resets to `Prepared` and cannot be ordinarily resubmitted.

The recording tests prove one acknowledgement and one deliberately ambiguous exchange, including repeat activation after each outcome. The suite passes 151 assertions, including an explicit composition check that the dispatcher is not registered. Authority-identity allocation, materialisation and exchange remain injected ports; the product host registers none of them, exposes no submit control and remains `SendDisabled`. Consequently this increment makes no external request and cannot reach either the official-test or protected live endpoint.

### Increment 3 — deployed official-test-only single send

Implemented and deployed on 12 October 2026. A distinct `OfficialTestSingleSend` composition is valid only with the `OfficialTest` authority environment and durable Azure-managed persistence. Production has no send-enabled mode. The UI exposes its action only after immutable approval, labels it **official test only — not a live filing**, requires an explicit confirmation and disables ordinary replay after the conversation leaves `Prepared`.

The isolated workflow database now owns a presenter-scope authority sequence. Allocation occurs immediately before materialisation and begins at `S00017`, following the retained WebHarness test series `S00001`–`S00016`. The final materialiser digest-verifies the retained credential-free GovTalk preview and iXBRL document, replaces only the `000000` review marker with the allocated submission number, injects the transaction identity and five protected test values, then verifies that the embedded accounts bytes still match the approved document digest. The exact request is retained before the atomic `Prepared` → `Dispatching` claim.

The five official-test values are held in the existing Key Vault and exposed to the adapter only through resolved App Service references. No live presenter or package value is configured. The endpoint is fixed in code to the published Companies House test gateway, with no redirects or automatic HTTP retry. The filing journey carries a persistent red **OFFICIAL TEST — NOT A LIVE FILING** banner from its overview through approval, states that the live package reference is absent, and repeats the label beside the one-send control. The TCWeb suite passes 154 assertions, the Submission adapter suite 137 assertions, the Company contract suite 92 assertions and Block A 31 assertions. Azure liveness and readiness both return HTTP `200` after deployment, and all five test references report `Resolved`.

The first authenticated deployment check also detected that the Candidate 4 database predated the completed Subjects project and lacked `Subject.fnDatedBalances`. No filing action was possible in that state. A reviewed DACPAC deployment report identified the four new Subject reporting functions, one same-name check-constraint refresh and the current template procedure refreshes. The current sqlnode DACPAC was then published to the synthetic S2 database with data-loss blocking and source-external object deletion disabled. The Accounts workspace and the closed 2025–26 filing journey subsequently loaded successfully. This schema alignment made no Companies House request.

After the reviewer inspected the generated draft PDF, the authenticated journey approved and submitted that exact package once. The official test gateway acknowledged `S00017` at `2026-10-12T12:32:13Z` with no gateway errors. The durable record binds document SHA-256 `B8F9F592BEFE3789E72BA93493EC506F42FCE37D07290796D103E109E5D43461`, package SHA-256 `D976E6284232B2D530C0493FDF0394362CE496496DED06AEA51E2ECA255155F1`, request SHA-256 `412AA80F8FE537363A408E89A6DC7F727A73038C94F48DC7ECAEFBA01E8DB354` and response SHA-256 `C93EF3CF2458D7436FBCC6FF7622C4B22F693299B6804AB784D4B14B9219C024`. No retry occurred or is permitted.

An authenticated submitted-copy PDF is now derived from the same retained document only after the server reopens the preparation, approval and active conversation and verifies an acknowledged official-test state, matching document/package digests and retained request/response hashes. It has no diagonal draft watermark, but its metadata and every-page footer identify `OFFICIAL TEST — NOT LIVE FILED`, `S00017`, the acknowledgement time and source-document digest. It is human evidence only and does not replace the exact retained iXBRL. Both A4 pages were rendered to PNG and visually inspected; the TCWeb suite passes 156 assertions. Azure deployment `cb6fe8b7-f913-4d11-8770-d2f2cc62eea1` is runtime-successful with liveness/readiness HTTP `200`.

### Increment 4 — durable official-test status continuation

Implemented and deployed on 12 October 2026. The acknowledged `S00017` conversation now exposes one explicit **Check Companies House status** action. It constructs the published specific-submission `GetSubmissionStatus` request from the retained submission number, injects only the protected official-test presenter credentials at transport time, retains the exact request before an atomic status-check claim, performs one non-retrying exchange against the fixed official-test gateway, and retains the exact response before classification. Status evidence is appended separately from the original filing request and acknowledgement, so polling can neither overwrite filing provenance nor make the filing replayable.

The continuation accepts only one correlated status result for `S00017`, masks and validates any returned company identity, bounds examiner and error text, and maps the authority outcome to `Pending`, `Parked`, `Accepted`, `Rejected` or `InternalFailure`. Malformed, ambiguous or uncorrelated evidence becomes a status-check recovery condition without changing the original filing into a resubmittable state. Terminal conversations no longer expose the action. Because this is the published **specific-submission** request, TIS 6.0 does not require a `StatusAck`; that acknowledgement is conditional only on an all-submissions response and remains covered by the adapter contract and deterministic simulator.

The deployed Accounts journey rehydrates `S00017`, identifies it as **EXISTING FILING — DO NOT RESUBMIT**, provides its official-test submitted PDF and shows the status action beneath the durable acknowledgement. The TCWeb boundary suite passes 159 assertions, the Submission adapter suite 137 assertions and the Company contract suite 92 assertions. Azure deployment `a50e7179-2b17-40a5-995f-f36dabe90804` is runtime-successful and both health endpoints return HTTP `200`. Browser verification confirmed the control against the 2025–26 `S00017` conversation. No status request was made during implementation, deployment or verification; its first use remains the human review event.

`S00017` is already the half-completed submission case: it was acknowledged without a terminal decision, then recovered after an application restart without presenting a second filing path. No additional official submission is needed merely to reproduce that state.

### Review gate

The reviewer observes `S00017` continue from acknowledgement through the actual Companies House status sequence, including conditional `StatusAck` only if the authority requests it, with no duplicate submission request. Deterministic contract/Block A tests continue to prove rejection, parked, malformed/ambiguous response and recovery-required paths; the official service need not be manipulated into every failure state. Phase 7.7 Increment 0 already proves restart continuation of the unresolved conversation. Broader filing history and operational recovery stop until Phase 7.6 is accepted.

## Phase 7.7 — Filing history and operational recovery

**Phase status: IN PROGRESS — Increment 0 implemented and exercised; remaining work follows the Phase 7.6 gate.**

### Purpose

Make completed and unresolved Companies House work usable after the original session.

### Increment 0 — existing-conversation continuation guard

Implemented as a narrow Phase 7.6 safety dependency on 12 October 2026. It is recorded here, under the phase whose history/restart responsibility it fulfils. When the filing workspace opens, the server derives the authenticated tenant and reporting subject, resolves the statutory company number, and searches durable company-scoped conversation metadata for the selected period and supported filing profile. Matching preparation and conversation document/package digests must agree before anything is shown.

An existing conversation replaces the fresh **Start filing journey** presentation with an explicit **EXISTING FILING — DO NOT RESUBMIT** continuation card containing its safe state, submission number, update time and evidence digests. The preparation boundary independently rejects creation of another preparation for the same logical company/period/profile even if the UI is bypassed. Any inability to verify continuation state disables the new journey. Terminal or failed conversations remain visible and blocked from silent replacement; a later, explicit recovery policy must decide whether a corrected successor is permitted.

This increment does not provide a general history list, status polling, resubmission, rejection correction or operator recovery. It exists so Phase 7.6 can continue `S00017` after restart without presenting a duplicate-filing path.

The TCWeb boundary suite passes 158 assertions. Final Azure deployment `c03174d2-30ce-4f03-9683-e5ae38b33002` completed `RuntimeSuccessful`; after the Basic-tier warm-up, liveness and database readiness both returned HTTP `200`. An authenticated restart check selected the 2025–26 period and confirmed that the workspace rehydrated `S00017` as `Acknowledged`, displayed the retained document and package SHA-256 values, offered the submitted-copy PDF and did not present the fresh filing journey.

### Implementation

1. Add a company-scoped filing history sourced from durable approval, attempt, status and outcome records.
2. Show the accounting period, profile, actor, submission number, timestamps, current authority state and safe evidence references.
3. Expose a controlled status/recovery action only where the retained conversation permits it.
4. Verify protected-content digests on every retrieval and fail closed on missing or corrupt evidence.
5. Record safe application Event Log entries for preparation, approval, dispatch, status and exceptional recovery.
6. Provide operational guidance for rejection, parked/internal-failure, unknown outcome, credential rotation and Companies House support escalation.

### Review gate

Human review confirms tenant/company isolation, retention, restart recovery, status truthfulness and absence of secrets or raw protected envelopes from the UI and ordinary logs.

## Phase 7.8 — Live-package activation and first controlled filing

**Phase status: DEFERRED — protected package reference received, but the live multi-tenant product deployment does not yet exist. No live exchange is authorised and live-service testing is prohibited.**

### Purpose

When a live multi-tenant product instance exists, materialise the protected live package reference only at transport time and prove one explicitly authorised genuine filing without broadening product scope. A test, probe or rehearsal against the live service is forbidden.

### External and human gates

1. Receive and inspect the live-package instructions and protected values.
2. Record safe provenance and configuration requirements without checking secrets into any repository.
3. Validate the production host, gateway endpoint, certificate/TLS behavior, persistence, backup/restore, monitoring and credential rotation.
4. Perform only non-filing smoke tests permitted by Companies House.
5. Select an eligible Trade Control company and exact accounts period.
6. Obtain explicit contemporaneous human authority for the company, document and one live submission.
7. Submit once, retain the acknowledgement/status evidence and reconcile the terminal result.

No live submission is authorised by this work plan alone. These gates are intentionally deferred rather than simulated against the present single-instance development deployment. A successful first filing does not add another accounts regime or remove the separate per-company approval requirement.

## Test and regression strategy

- Preserve all existing Company Contract, Application, Data Provision, Submission adapter, simulator and architecture tests.
- Add TCWeb host tests proving server-derived tenant/company identity, authorisation policy, antiforgery protection and no browser-supplied statutory values or credentials.
- Prove exact document/package digest continuity from preparation through approval and dispatch.
- Cover first/subsequent accounts, stale source, unsupported profile, duplicate attempt, ambiguous send, restart, pending, parked, accepted, rejected and conditional acknowledgement.
- Keep official network tests separately gated and non-repeatable. Offline and simulator tests must never be labelled authority evidence.
- Build `TaxHub.slnx`, `TCWeb.csproj` and the user guide at each accepted phase.

## Documentation and completion

The user guide describes only controls that exist in the released product. Before a phase is implemented it may explain the supported filing scope and current availability, but must not present planned controls as usable.

Record implementation evidence in this plan, durable external decisions in `findings.md`, significant code changes in `change-log.md` and protected operational evidence outside Git. Update screenshots only from reviewed, non-secret product states.

This Companies House Objective 5 slice is complete when an authorised user can review, approve, file and follow one supported accounts submission through a durable terminal outcome in TCWeb, with recovery/history and production evidence accepted. Administrative live-package approval alone does not satisfy that product completion condition.

## Appendix A — Azure reference environment replication runbook

This is a non-production reference environment for Objective 5 development. Its purpose is to reproduce the product-hosting and accounting-source conditions under which the Companies House workflow is developed; it is not a template that grants live-filing authority. Resource names below identify the present reference deployment. A replica may use different names, but must preserve the boundaries and checks.

### Reference topology

| Role | Reference resource | Required property |
|---|---|---|
| Subscription | `dad8f85c-390e-4797-ab8d-88d5db96115e` | Operator is authenticated to the intended subscription before any mutation. |
| Resource group / region | `tradecontrol-taxhub-sandbox` / UK West | Development resources remain isolated from production. |
| Linux App Service plan | `tc-taxhub-sandbox-plan` | Hosts TCWeb and the separately stoppable WebHarness. |
| Product host | `tcweb-payg-db96115e` | Authenticated TCWeb; its database binding is a Key Vault reference. |
| Diagnostic host | `taxhub-payg-db96115e` | WebHarness/Swagger; stopped when it is not explicitly required. |
| Azure SQL logical server | `tradecontrol-db96115e` | Source and target databases coexist so the configured-node copy is server-side. |
| Configured source node | `tcNodeDb4-COMIPFVT1-COMIN26` | Supplies node/user/bootstrap configuration only; it remains unchanged. |
| Candidate 4 STD node | `tcNodeDb4-COSIPFVT1-COSTD26` | Three completed September year ends plus the controlled Year 3 additive evolution. |
| Existing VAT node | `tcNodeDb4-HMRC62-2017` | Remains unchanged and is not a regeneration source. |
| Secret store | `tcsecretsdb96115e` | Holds connection strings and future protected configuration; values are never tracked. |

The current STD database is temporarily Standard S2 for interactive development. Basic was measured, not guessed, to be inadequate for the authenticated Tax Hub Dashboard. Synthetic regeneration used temporary S3 capacity and was downscaled after completion. These are operational settings, not product requirements: measure a replica and select the cheapest tier that completes the workload reliably.

### Protected prerequisites

Before replication, obtain through the deployment environment rather than Git:

1. an Azure identity permitted to administer the named resource group, SQL database, App Service configuration and Key Vault references;
2. the SQL deployment identity/secret required by the DACPAC and generator;
3. a configured synthetic source node containing valid application bootstrap and user identity data; and
4. the stable opaque tenant GUID used by the TCWeb Companies House host configuration.

Do not print or paste connection strings, SQL credentials, Key Vault values, presenter credentials or company authentication into commands captured as evidence. Secret names and versionless Key Vault references may be recorded; secret values may not.

### Reproduction sequence

1. **Inventory before mutation.** Select the intended Azure subscription and record the resource group, SQL database names/service objectives, App Service plan, current TCWeb database-setting reference, Key Vault name and application running states. Confirm the source node and VAT database are not the target.
2. **Create an isolated target.** Use an Azure SQL server-side database copy of the configured MIN source node to create the explicitly named STD target. Do not manually manufacture application-user or node-identity rows. Wait for the copy to become `Online`, then verify source and target names again before deployment.
3. **Build and deploy the current schema.** Build `src/sqlnode/src/tcNodeDb4/tcNodeDb4.sqlproj` and deploy that DACPAC to the isolated target with the guarded deployment settings. Review every potential data-loss or type-conversion warning. The reference copy contained obsolete reporting-profile/classification data from an earlier schema: those rows were removed in foreign-key order only in the disposable target, after inspection, so the current typed classifications could be deployed and reseeded. A replica must assess its actual migration; this observation is not authority for indiscriminate deletion.
4. **Temporarily scale for generation.** Raise only the isolated target to a tier capable of completing the synthetic generator (S3 was used for the reference run). Record the original tier so restoration is explicit.
5. **Generate the coherent STD history.** Invoke `App.proc_DatasetSyntheticMIS` with the standard-company template, three completed years, financial month `10` (a September year end), and test anchor `2026-10-07`. Retain the generator's normal full accounting surface—projects, multi-level object/BOM flows, invoices, payments, wages, expenses, assets, tax, transfers and opening balance. The decisive parameters are:

   ```sql
   @IsCompany = 1,
   @IsVatRegistered = 1,
   @UseStdCompanyTemplate = 1,
   @CompletedYearCount = 3,
   @FinancialMonth = 10,
   @AsOfDate = '2026-10-07'
   ```

   Other generation ratios and switches must use the reviewed STD scenario in `Scripts/EXEC_DatasetSyntheticMIS.sql`; if that script's defaults later change, record the committed revision used rather than silently relying on new defaults.
6. **Prove the base history.** Run `Tests/SyntheticDatasetCompletedYearHorizon.sql`. It must report three consecutive completed years, every Equity Bridge variance within `0.10`, statutory balance-sheet projections `Ready`, and both multi-level Object and Project flows present.
7. **Exercise the Year 3 additive change safely.** Run `Scripts/EXEC_DatasetSyntheticMIS_Year3_Additive.sql` first with its default `@ApplyChanges = 0`. Inspect the rolled-back results. Set `@ApplyChanges = 1` only against the intended STD target after the rehearsal passes, then rerun the completed-year regression. The script is additive and fail-closed; it must not rewrite Years 1 or 2.
8. **Check the reference accounting outcome.** The reference run produced year ends `2024-09-30`, `2025-09-30` and `2026-09-30`, with Equity Bridge variances `0.00`, `0.00` and `0.08`. Its final statutory balance sheet reported fixed assets `6,800.00000`, current assets `495,943.40048`, creditors within one year `40,125.60613` and creditors after one year `3,000.00000`, all `Ready`. Exact monetary values are evidence for this deterministic reference revision, not universal deployment constants; the structural checks and reconciliation gate are mandatory.
9. **Create a dedicated protected binding.** Add a new Key Vault secret containing the STD connection string under the reference name `tcnodecontext-costd26` (or an equivalently scoped name). Bind TCWeb `ConnectionStrings__TCNodeContext` to a versionless Key Vault reference for that secret. Do not replace or expose the older secret, and do not bind the diagnostic App Service by accident.
10. **Restart and verify the product host.** Restart TCWeb and require HTTP `200` from the application root, `/health/live` and the database-backed `/health/ready`. Sign in through the normal application identity path, open the Tax Hub Dashboard and Accounts workspace, and confirm the business is the STD synthetic company with the expected September year ends. A liveness response alone is not sufficient.
11. **Control auxiliary services and capacity.** Stop `taxhub-payg-db96115e` unless a separately authorised diagnostic exercise requires it. During interactive development monitor Azure SQL DTU, CPU, reads and log writes. The reference Basic trial reached 100% DTU and CPU for two consecutive minutes and timed out; S2 restored an acceptable interactive response. At the end of the development interval, stop unused hosts and review/downscale the database rather than leaving temporary capacity in place.

### Recovery and non-filing controls

- The configured source node, VAT database and previous Key Vault secret remain intact. Rollback is a TCWeb rebind to the previously reviewed versionless secret reference followed by restart and all three health checks; never recover by copying a secret value into tracked configuration.
- A failed or partial regeneration is discarded or repeated only in the explicitly identified isolated target. Resolve and verify the absolute database name before deletion or replacement.
- Database backup/restore and deployment evidence are operational records. Application logs may contain safe resource names and digests, but not secret material or unrestricted statutory documents.
- The accounting source and product host are reproduced here. Phase 7.2 separately governs the Azure workflow/evidence facilities and their access controls.
- Companies House dispatch remains `SendDisabled`. Reproducing this environment, generating an accounts document or passing readiness checks makes no external request and grants no filing authority.
