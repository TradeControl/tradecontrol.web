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

## Phase 7.0 status — 8 October 2026

Phase 7.0 is implemented and accepted at its human review gate. The TCWeb product contract accepts only safe workflow references and a period selection; durable preparation, approval and XML-conversation record shapes preserve exact digests and opaque protected-content handles. A tenant/company-scoped persistence port defines atomic mutation, one-active-conversation and restart-recovery requirements.

TCWeb Companies House options contain no credentials, company authentication or gateway URL. Their only dispatch mode is `SendDisabled`; development files are restricted to the Development host, and the selected Azure-managed composition remains fail-closed until implemented. Presenter/live-package secret references and the company-specific dispatch-time authentication resolver exist only in the Submission adapter.

The reviewer accepted the seven-year operational-evidence policy and selected Azure-managed persistence as the first real Phase 7.1 implementation; development files remain an isolated test facility rather than a product stepping stone. The initially single-node deployment retains a stable tenant GUID and tenant-partitioned records, content and tests so the design does not collapse into single-tenant global state.

The detailed decision record is `phase-7.0-companies-house-integration-boundary.md`. `TaxHub.slnx` and `TCWeb.csproj` build without warnings, and the TCWeb Tax Hub suite passes 106 boundary assertions. No UI, external request or Phase 7.1 preparation service was added by Phase 7.0.

### Azure accounting-source checkpoint — 8 October 2026

Following Phase 7.0 acceptance, the separate Candidate 4 development database `tcNodeDb4-COSIPFVT1-COSTD26` was created on the existing Azure SQL server while the MIN/VAT databases and deployed bindings remained untouched. The current DACPAC and corrected synthetic generator produced the three-year STD company with the October financial month and `2026-10-07` test anchor. The completed-year regression and Year 3 additive rollback rehearsal passed before the additive change was committed; the final regression also passed. The database was returned from temporary S3 to Basic after validation.

After validation, TCWeb was rebound through a dedicated Key Vault reference to the STD database and restarted; its root, liveness and database-backed readiness checks returned HTTP `200`. The unused Swagger/WebHarness App Service was stopped. An authenticated Dashboard request then proved Basic inadequate for the interactive Tax Hub workload: DTU and CPU both reached 100% for two consecutive minutes before the command timed out. The database is temporarily at Standard S2 for Azure development and must be reviewed/downscaled when that interval ends. This establishes the Azure accounting source and product host for the next manageable Phase 7.1 increment. It does not implement the Azure workflow/evidence stores, enable dispatch or authorise a Companies House submission.

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
- This environment currently establishes only the accounting source and product host. Azure workflow/evidence persistence must be implemented and reviewed in Phase 7.1 before it is relied upon.
- Companies House dispatch remains `SendDisabled`. Reproducing this environment, generating an accounts document or passing readiness checks makes no external request and grants no filing authority.

## Phase 7.1 progress — increment A: server-derived readiness

Phase 7.1 is now in progress. Its first increment adds a Companies House tab to the existing Accounts workspace without adding a prepare, approve or submit control. For the selected accounting year the server derives and displays:

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

Increment B will establish the tenant-partitioned Azure workflow metadata and protected evidence facilities and collect the reviewed filing-only inputs without generating an exact artifact prematurely. Increment C will prepare through `CompaniesHouseAccountsPreparer`, atomically retain and digest-verify the exact document/package, and render the authorised safe review. This sequencing keeps exact preparation and protected retention together. Phase 7.1 remains open, and the Phase 7.2 approval boundary is not authorised by this increment.

The increment builds without warnings, the TCWeb Tax Hub suite passes 112 assertions, and the deployed product host remains healthy. The diagnostic WebHarness remains stopped. No Companies House network request was made.
