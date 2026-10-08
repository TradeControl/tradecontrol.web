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

Companies House confirmed on 8 October 2026 that it was content to issue a live package for the document types developed to date. Administrative package provisioning is in progress. The live package and live presenter configuration have not yet been issued, and no live filing is authorised.

The human reviewer authorised this Companies House Objective 5 slice independently of Corporation Tax. CT Phases 5.8–5.10 remain externally blocked, and MTD Income Tax Phases 5.15–5.16 remain deferred. This plan neither completes Objective 4 as a whole nor changes those gates.

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
- The existing Accounts workspace and Equity Bridge remain the accounting-review surface. Objective 5 adds filing review and lifecycle views without changing accounting calculations.

## Delivery sequence

| Phase | Deliverable | Principal gate |
|---|---|---|
| 7.0 | Product boundary, persistence and live-configuration decision | Architecture/security review |
| 7.1 | Accounts filing readiness and exact-document review | Source/document identity review |
| 7.2 | Declarations and immutable filing approval | Human approval-model review |
| 7.3 | Controlled submission and status conversation | Official test/live-disabled lifecycle review |
| 7.4 | Filing history, recovery and evidence presentation | Audit/retention/access review |
| 7.5 | Live-package activation and controlled first live filing | External package and explicit live-enable gates |

Phases are implemented in order. The pending live package does not block 7.0–7.4 work that can be proved with published contracts, accepted official test evidence and send-disabled production composition. It blocks live activation and any live exchange in 7.5.

## Phase 7.0 — Integration boundary and persistence

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

## Phase 7.1 — Filing readiness and exact-document review

### Purpose

Make the accepted company-accounts preparation visible and understandable without permitting submission.

### Implementation

1. Add a Companies House section to the Accounts workspace for eligible year-end periods.
2. Show company number in an appropriately masked or bounded form, accounting period, filing profile, first/subsequent-accounts status and readiness findings.
3. Prepare through `CompaniesHouseAccountsPreparer` and retain the exact iXBRL/package digests and source evidence.
4. Render a safe review of the exact prepared document. Do not rebuild iXBRL in TCWeb or accept statutory values from the browser.
5. Present the Equity Bridge and comparative-continuity relationship as supporting accounting evidence, not as Companies House acceptance.
6. Block unsupported regimes and any package with missing, stale or error-bearing evidence.

### Review gate

The reviewer traces the displayed figures and document digest to the accepted preparation path and confirms that review performs zero external sends. Stop before Phase 7.2.

## Phase 7.2 — Declarations and immutable approval

### Purpose

Record an attributable human decision for one exact filing candidate.

### Implementation

1. Present the current Companies House filing declarations and warnings required for the supported profile.
2. Require explicit confirmation and an authorised Trade Control actor.
3. Bind approval to tenant, company identity, period, profile, declaration version, source evidence, document digest and package digest.
4. Expire approval when any bound input changes or the approved candidate becomes stale.
5. Distinguish **approved — not filed** prominently from authority receipt or acceptance.

### Review gate

Human review confirms the declaration text, permissions, accessibility and digest binding. No send control is enabled before acceptance.

## Phase 7.3 — Controlled submission and status conversation

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

The reviewer observes acknowledgement, pending, terminal acceptance/rejection and recovery behavior with no duplicate external request. Stop before Phase 7.4.

## Phase 7.4 — Filing history and operational recovery

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

## Phase 7.5 — Live-package activation and first controlled filing

### Purpose

Install the issued live package safely and prove one explicitly authorised live filing without broadening product scope.

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

## Initial status — 8 October 2026

The work plan and Objective 4 → Objective 5 boundary are authorised. No Objective 5 filing code has been added yet. Phase 7.0 is the next implementation gate. The user guide may now record the approved filing scope and current product boundary, while live filing controls remain forthcoming until their corresponding phases are implemented and reviewed.
