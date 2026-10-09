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
| Pre-submission accounting-evidence dependency | **DEFERRED TO SUBJECTS PROJECT** | The approved [polarity-based aged-balances work plan](../../Subjects/subject-aged-balances-workplan.md) must first provide coherent dated debtor/creditor evidence, after which the guided year-end review can be completed. | Phase 7.6 remains unopened; product dispatch stays disabled. |
| 7.6 — Submission and status | **NOT STARTED** | Requires the accepted Subjects evidence contract and guided year-end review in addition to Phase 7.5. | Product dispatch remains disabled. |
| 7.7 — History and recovery | **NOT STARTED** | Requires Phase 7.6 gate acceptance. | No product filing history/recovery workflow exists. |
| 7.8 — Live activation | **EXTERNALLY BLOCKED / NOT STARTED** | Protected live package reference received 9 October 2026; separate live credentials, completed workflow gates and explicit authority for a real filing are still required. Live-service testing is prohibited. | No live exchange is authorised. |

**Current position:** Phase 7.5 is accepted, but Phase 7.6 is deliberately not authorised. Implement and review the polarity-based Subjects ageing framework, then return here to complete the guided year-end evidence review before submission/status work begins.

Status meanings:

- **NOT STARTED** — no phase implementation has begun.
- **IN PROGRESS** — implementation is authorised but its technical evidence and review gate are not yet complete.
- **IMPLEMENTED — AWAITING HUMAN REVIEW** — implementation and technical evidence exist, but the phase has not passed.
- **CORRECTED — AWAITING HUMAN RE-REVIEW** — a gate finding has been corrected, but the corrected product evidence has not yet been accepted.
- **PASSED** — the human review gate has been explicitly accepted and the next phase may begin.
- **EXTERNALLY BLOCKED** — required authority material or permission is unavailable; internal work must not be represented as resolving it.

### Governing records

1. `docs/projects/Tax Hub/specs/tax-hub-spec-programme.md`.
2. `tax-hub-workplan-5.md`, especially Phases 5.11–5.14.
3. `company-statutory-contract-design.md` and the accepted Objective 3 statutory-accounts boundary.
4. `phase-5.11-companies-house-prerequisite-assurance.md`.
5. `../companies-house-approval/companies-house-test-submission-evidence.md`.
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

Phases are implemented in order. Receipt of the protected live package reference does not alter 7.0–7.7 or authorise a live call. Those phases continue using published contracts, accepted official test evidence and send-disabled production composition. Phase 7.8 remains blocked by the remaining workflow, credential and explicit filing-authority gates; the live service must never be used for testing.

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

Detailed debtor/creditor work does not belong in this filing plan. Trade Control has no permanent customer/supplier identity; it has dated Subject-statement polarity. The separate [Subjects aged-balances work plan](../../Subjects/subject-aged-balances-workplan.md) therefore owns one dated calculation and two human operational perspectives: Credit Control for negative positions owed to the business, and Buying/debit control for positive positions owed by the business.

Once that plan supplies an accepted year-end evidence snapshot, this workflow will add a resumable guided review that:

1. shows gross debtor and creditor totals reconciled to the selected year-end accounts and links to the Subject Browser details;
2. reviews native bank/current/reserve balances and fixed-asset cost, write-down and net-book-value evidence;
3. records attributable completion against deterministic source evidence;
4. invalidates completion and all dependent preparation/approval when relevant source evidence changes; and
5. continues clearly into statutory filing inputs and document preparation without accepting free-entry accounting values.

This is an internal product-quality gate, not a Companies House contract requirement and not part of the XML transport. Phase 7.6 remains `NOT STARTED` until the Subjects framework and the resulting guided review have passed human review.

## Phase 7.6 — Controlled submission and status conversation

**Phase status: NOT STARTED. Product dispatch remains disabled.**

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

### Review gate

The reviewer observes acknowledgement, pending, terminal acceptance/rejection and recovery behavior with no duplicate external request. Stop before Phase 7.7.

## Phase 7.7 — Filing history and operational recovery

**Phase status: NOT STARTED.**

### Purpose

Make completed and unresolved Companies House work usable after the original session.

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

**Phase status: EXTERNALLY BLOCKED / NOT STARTED — protected package reference received, but no live exchange is authorised and live-service testing is prohibited.**

### Purpose

Materialise the protected live package reference only at transport time and prove one explicitly authorised genuine filing without broadening product scope. A test, probe or rehearsal against the live service is forbidden.

### External and human gates

1. Receive and inspect the live-package instructions and protected values.
2. Record safe provenance and configuration requirements without checking secrets into any repository.
3. Validate the production host, gateway endpoint, certificate/TLS behavior, persistence, backup/restore, monitoring and credential rotation.
4. Perform only non-filing smoke tests permitted by Companies House.
5. Select an eligible Trade Control company and exact accounts period.
6. Obtain explicit contemporaneous human authority for the company, document and one live submission.
7. Submit once, retain the acknowledgement/status evidence and reconcile the terminal result.

No live submission is authorised by this work plan alone. A successful first filing does not add another accounts regime or remove the separate per-company approval requirement.

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

## Phase 7.0 status — 8 October 2026

Phase 7.0 is implemented and accepted at its human review gate. The TCWeb product contract accepts only safe workflow references and a period selection; durable preparation, approval and XML-conversation record shapes preserve exact digests and opaque protected-content handles. A tenant/company-scoped persistence port defines atomic mutation, one-active-conversation and restart-recovery requirements.

TCWeb Companies House options contain no credentials, company authentication or gateway URL. Their only dispatch mode is `SendDisabled`; development files are restricted to the Development host, and the selected Azure-managed composition remains fail-closed until implemented. Presenter/live-package secret references and the company-specific dispatch-time authentication resolver exist only in the Submission adapter.

The reviewer accepted the seven-year operational-evidence policy and selected Azure-managed persistence as the Phase 7.2 implementation; development files remain an isolated test facility rather than a product stepping stone. The initially single-node deployment retains a stable tenant GUID and tenant-partitioned records, content and tests so the design does not collapse into single-tenant global state.

The detailed decision record is `phase-7.0-companies-house-integration-boundary.md`. `TaxHub.slnx` and `TCWeb.csproj` build without warnings, and the TCWeb Tax Hub suite passes 106 boundary assertions. No UI, external request or Phase 7.1 preparation service was added by Phase 7.0.

### Azure accounting-source checkpoint — 8 October 2026

Following Phase 7.0 acceptance, the separate Candidate 4 development database `tcNodeDb4-COSIPFVT1-COSTD26` was created on the existing Azure SQL server while the MIN/VAT databases and deployed bindings remained untouched. The current DACPAC and corrected synthetic generator produced the three-year STD company with the October financial month and `2026-10-07` test anchor. The completed-year regression and Year 3 additive rollback rehearsal passed before the additive change was committed; the final regression also passed. The database was returned from temporary S3 to Basic after validation.

After validation, TCWeb was rebound through a dedicated Key Vault reference to the STD database and restarted; its root, liveness and database-backed readiness checks returned HTTP `200`. The unused Swagger/WebHarness App Service was stopped. An authenticated Dashboard request then proved Basic inadequate for the interactive Tax Hub workload: DTU and CPU both reached 100% for two consecutive minutes before the command timed out. The database is temporarily at Standard S2 for Azure development and must be reviewed/downscaled when that interval ends. This established the Azure accounting source and product host for Phase 7.1. It did not itself implement workflow/evidence stores, enable dispatch or authorise a Companies House submission.

### Azure reference environment replication runbook

This is a non-production reference environment for Objective 5 development. Its purpose is to reproduce the product-hosting and accounting-source conditions under which the Companies House workflow is developed; it is not a template that grants live-filing authority. Resource names below identify the present reference deployment. A replica may use different names, but must preserve the boundaries and checks.

#### Reference topology

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

#### Protected prerequisites

Before replication, obtain through the deployment environment rather than Git:

1. an Azure identity permitted to administer the named resource group, SQL database, App Service configuration and Key Vault references;
2. the SQL deployment identity/secret required by the DACPAC and generator;
3. a configured synthetic source node containing valid application bootstrap and user identity data; and
4. the stable opaque tenant GUID used by the TCWeb Companies House host configuration.

Do not print or paste connection strings, SQL credentials, Key Vault values, presenter credentials or company authentication into commands captured as evidence. Secret names and versionless Key Vault references may be recorded; secret values may not.

#### Reproduction sequence

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

#### Recovery and non-filing controls

- The configured source node, VAT database and previous Key Vault secret remain intact. Rollback is a TCWeb rebind to the previously reviewed versionless secret reference followed by restart and all three health checks; never recover by copying a secret value into tracked configuration.
- A failed or partial regeneration is discarded or repeated only in the explicitly identified isolated target. Resolve and verify the absolute database name before deletion or replacement.
- Database backup/restore and deployment evidence are operational records. Application logs may contain safe resource names and digests, but not secret material or unrestricted statutory documents.
- The accounting source and product host are reproduced here. Phase 7.2 separately governs the Azure workflow/evidence facilities and their access controls.
- Companies House dispatch remains `SendDisabled`. Reproducing this environment, generating an accounts document or passing readiness checks makes no external request and grants no filing authority.

## Phase 7.1 status — accounts filing readiness

Phase 7.1 is implemented. It adds a Companies House tab to the existing Accounts workspace without adding a prepare, approve or submit control. For the selected accounting year the server derives and displays:

- the bounded company-number display and exact accounting-period bounds;
- the supported unaudited filleted FRS 105 micro-entity profile;
- first-accounts or subsequent-accounts/comparatives status;
- closed-year and year-end selection eligibility;
- reviewed statutory-context/reporting-profile findings; and
- the selected year's Equity Bridge result against the accepted `0.10` tolerance.

The assessment obtains statutory context through the existing Trade Control adapter and accepts no company identity, statutory values, iXBRL, XML or credentials from the browser. Any missing period, company identity, reporting profile, source context or Equity Bridge evidence blocks readiness. The browser model explicitly reports that no exact document has been prepared and no external request has occurred.

The first Azure product check exposed and corrected a pre-existing calendar assumption in the Accounts workspace: an October-start financial year cannot derive its bounds or year-end marker from calendar `MonthNumber` order or `MonthNumber == 12`. Both now use the actual ordered `StartOn` dates. This prevents a September year end from being presented as December and is a prerequisite to binding an immutable filing candidate to the correct period.

The Azure check also established the necessary temporal distinction between the accounting evidence and operational configuration: accounts figures are bound to the selected period end, whereas statutory identity/reporting-profile eligibility is evaluated at the current preparation date. Evaluating a profile at the historical year end incorrectly hides a profile reviewed after that year end but before filing. Both dates must be retained when the exact candidate is introduced.

The live Azure review of the STD candidate selected `2025-26 SEP` and reported **Ready to prepare document** for company `••3456`, period `1 October 2025` to `30 September 2026`, the supported unaudited filleted FRS 105 micro-entity profile, subsequent accounts with comparatives, and an Equity Bridge pass with variance `0.08`. The screen also stated that no statutory document had been retained, no Companies House request had occurred and submission remained disabled.

The phase builds without warnings, its initial TCWeb Tax Hub suite passed 112 assertions, and the deployed product host remained healthy. The diagnostic WebHarness remained stopped. No Companies House network request was made.

## Phase 7.2 status — Azure workflow and evidence persistence

Phase 7.2 is implemented and deployed without collecting filing-only inputs or generating a statutory artifact. The isolated Basic database `tcTaxHubWorkflow` holds tenant-partitioned preparation, approval and conversation metadata. Its tracked additive DDL enforces composite tenant/reference keys, tenant-scoped evidence-chain foreign keys and one active conversation per logical filing. The three tables contain zero rows at this checkpoint.

Exact statutory and gateway bytes are assigned opaque tenant-scoped references in the private `companies-house-evidence` container of storage account `tctaxhubdb96115e`. Blob public access and shared-key access are disabled, TLS 1.2 is the minimum, and TCWeb's system-assigned managed identity has `Storage Blob Data Contributor` at that storage-account scope. Writes refuse replacement; reads recompute and compare the expected SHA-256 and validate retained tenant/digest metadata.

The metadata connection is a versionless Key Vault reference named by `ConnectionStrings:TaxHubWorkflow`; its value is absent from source, tracked settings and diagnostics. The deployed host uses `AzureManaged`, an opaque stable tenant GUID, the private container and `SendDisabled`. Readiness now probes the accounting database, workflow schema and Blob container and returns HTTP `200` only when all required facilities are reachable.

The deployment builds without warnings and the expanded TCWeb Tax Hub suite passes 117 assertions. Phase 7.3 now defines and reviews the minimal filing-only input model before Phase 7.4 connects it to exact preparation. No document, Blob evidence object, approval, conversation or Companies House request was created.

## Phase 7.3 status — reviewed filing inputs

Phase 7.3 was initially implemented and deployed as a review-only product step, but human review correctly rejected its free-entry fields for current/comparative prepayments, provisions and accruals/deferred income. Those fields confused statutory presentation with accounting measurement: the native balance sheet already includes those accounting effects, and adding independently typed values would double count them and invalidate the Equity Bridge.

The corrected implementation removes all six fields from the browser draft and core reviewed-input contract. The Accounts workspace now collects only non-ledger filing facts that cannot be derived reliably from the accounting source: accounts-approval date, signing director, principal activity, accounting policies, average employees, and the separately reviewable director-advance schedule. Legacy diagnostic DTOs retain zero-valued balance-sheet members for compatibility with accepted test fixtures, but validation rejects every non-zero override and the source reader does not consume them.

Commitments and contingencies are present in the wider MIS schema but are not accessible to Accounts Mode in the first release. The product therefore does not expose a blank disclosure editor or treat an empty list as evidence of absence. Its plain-language eligibility question defaults to none for the ordinary supported case; that answer becomes attributable only when the user reviews the form. Yes, unsure or no answer blocks the automated filing route. The authority-neutral structured disclosure remains available for a later MIS-backed implementation.

Each director-advance row now identifies one director and one accounting-period movement schedule. The four labelled amounts are opening balance, advances or credits during the period, repayments during the period and closing balance; they must satisfy opening plus advances/credits less repayments equals closing. “Add director” creates another director schedule, not another advance, and the statutory summary does not ask for individual transaction dates.

The current SQL balance-sheet projection provides four mutually exclusive native buckets: non-current assets, current assets, current liabilities and non-current liabilities. Prepayments, accruals and provisions are already accounted for within those balances. The present supported filing therefore emits zero for the *additional* statutory presentation lines and leaves the native buckets untouched. This is a bridge-preserving presentation decision, not an assertion that the underlying accounting items do not exist. Any later requirement to show a non-zero separate line needs a source-backed decomposition which removes the identical amount from its containing bucket; it must never be an operator override.

The review is fail-closed. It requires a closed year-end that has passed Phase 7.1 readiness; validates approval date, required text, employee and director-advance bounds, disclosure cardinality, director-advance arithmetic and the Accounts Mode eligibility declaration; and invalidates the review when the period or an input changes. Fixed supported-profile facts and every balance-sheet value remain read-only. The browser receives no presenter credential, company authentication, transport setting or authority endpoint.

Companies House readiness is now scoped to Companies House evidence. A company that is not VAT registered is not blocked merely because no VAT registration exists, and unrelated unreviewed VAT or Corporation Tax settings do not contaminate this filing gate. Required company identity, address, jurisdiction, currency, provenance and the reviewed Companies House profile remain mandatory.

This phase deliberately retains no draft or reviewed inputs in the workflow database and creates no statutory document or Blob object. A successful review means only that the in-memory inputs are internally coherent; exact-document preparation and durable candidate creation begin in Phase 7.4. Dispatch remains `SendDisabled`.

The corrected implementation builds without warnings. The company-contract suite passes 87 assertions, WebHarness hardening passes 73 assertions, and the TCWeb Tax Hub suite passes 126 assertions. Azure deployment `320a90bd-d323-4cab-9a3c-7903dfbe7e12` includes the director labelling and eligibility-default correction; liveness and readiness return HTTP `200`. The corrected form still requires authenticated human review before this gate can pass. No Companies House request was made.
