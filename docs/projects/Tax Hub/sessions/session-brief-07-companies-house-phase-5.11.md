# Session brief — Companies House integration, Phase 5.11 prerequisite assurance

3 October 2026

## Session objective

Move the active investigation from blocked Corporation Tax Phase 5.8 to the Companies House filing stream without weakening either authority's fail-closed boundary.

Begin with the prerequisites for **Work Plan 5, Phase 5.11 — Companies House official artifact and eligibility assurance**:

1. verify the current authoritative Companies House filing specifications, schemas, examples and validation rules;
2. determine whether the separately referenced Filing TIS assets needed by the existing preview are officially obtainable and complete;
3. reconcile the official filing artifact, authentication-slot and status-polling contracts against the current Objective 3 preview; and
4. report the precise programme-specification or Objective 3 correction required before implementation, if any.

Do not begin network transport, presenter credential handling or external submissions. Stop at the Phase 5.11 human review gate. If the official assets are unavailable or conflict materially with the current package, stop earlier with an evidence-backed blocker; do not infer or reconstruct them.

## Repository and checkpoint

Workspace:

`C:\Users\Ian\source\repos\tradecontrol.web`

The Tax Hub implementation is the Git submodule at:

`src\tax-hub`

Recorded checkpoints when this hand-over was prepared:

- `tradecontrol.web` `master`: `bfdee13` — `Record Corporation Tax Phase 5.8 asset blocker`
- `tax-hub` `master`: `6253ff8` — `Corporation Tax Objective 4 Phase 5.7`

The parent worktree already contained unrelated user changes in `src/TCWeb/Pages/Admin/Manager/Components/SetupPanel.razor`, `src/tradecontrol.web.sln` and the untracked Phase 5.7 session brief. Preserve them. Recheck both repositories before editing. Do not create a worktree, commit or push unless explicitly requested.

## Programme position

- VAT is technically accepted and its production-access request is with HMRC; external approval remains pending.
- Corporation Tax Phase 5.8 is paused because the authoritative CT computational 2025 taxonomy package has not been located.
- At 11:04 BST on 3 October 2026, the developer emailed HMRC Software Developers Support Team asking for confirmation of the applicable taxonomy and its authoritative package, version or entry point and distribution route.
- The CT Phase 5.7 family gate remains closed. Do not change CT artifacts, status metadata, validation policy or submission eligibility while awaiting HMRC's response.
- No Companies House artifact is currently submission-ready. The present XML is an offline logical preview, not evidence of gateway acceptance.

### Sequencing constraint

Work Plan 5 formally places Phase 5.11 after accepted CT milestone 5.10 and after a separately authorised programme-specification correction assigning Companies House accounts transport to Objective 4. Neither condition is currently satisfied.

The user's decision to switch streams while CT is externally blocked authorises prerequisite investigation and preparation of a narrow correction proposal. It does **not** silently waive those recorded dependencies or approve a readiness promotion. Before changing the filing contract or marking Phase 5.11 implemented, identify and obtain human approval for the smallest work-plan/programme-specification sequencing correction required. Authoritative asset discovery and read-only reconciliation may proceed first.

## Governing documents

Read these before changing code or project records:

1. `docs/projects/Tax Hub/implementation/tax-hub-workplan-5.md`
   - especially the objective boundary, invariants, delivery sequence and Phase 5.11;
2. `docs/projects/Tax Hub/specs/tax-hub-spec-programme.md`
   - its Objective 4 wording currently describes HMRC transport and needs a separately authorised Companies House scope correction;
3. `docs/projects/Tax Hub/implementation/tax-hub-workplan-4.md`
   - especially completed Phases CO5 and CO6 and the Objective 3 fail-closed hand-off;
4. `docs/projects/Tax Hub/implementation/company-statutory-contract-design.md`;
5. `docs/projects/Tax Hub/implementation/tax-hub-implementation-3.md`;
6. `docs/projects/Tax Hub/implementation/tax-hub-repo-structure.md`; and
7. current official Companies House general/accounts TIS, Filing TIS, schema-status, software-filing and developer-testing guidance.

Use primary Companies House or GOV.UK sources as authority. Third-party examples may help locate a question but cannot establish validation or submission readiness. Record acquisition URLs, publication/version dates, file names, sizes and SHA-256 values for every pinned asset.

## Current code facts

The following facts were verified at hand-over:

- `CompanyContractRegistry.CompaniesHouseAccountsTis59` is effective from 1 April 2026 but has `SubmissionReady = false` because the separately referenced Filing TIS envelope schemas are not provisioned.
- `CompanyServiceCoverageCatalog` classifies filing and polling as deferred. Only deterministic full/filleted accounts iXBRL preparation is supported.
- `CompaniesHouseEnvelopeSerializer` explicitly emits a deterministic **logical filing-package preview**. It constructs a simplified GovTalk `CompanyAccounts` body and must not be relabelled as an official filing artifact.
- `CompaniesHouseAccountsPreparer` marks both the transmission and iXBRL document `Preview` and returns `SubmissionPollingMode.PollUntilTerminal` with `submission-status/{envelopeNumber}`. That path is diagnostic metadata and must be corrected if it is not the actual XML gateway status contract.
- `CompaniesHouseEndpointSet` names TIS 5.9 and the GovTalk XML Gateway, but its descriptors and the current acknowledgement record are starting assumptions, not proof of the complete official request/response protocol.
- The full and filleted accounts documents derive from the reviewed Objective 3 statutory-accounts source. Preserve their exact bytes and digests unless official evidence requires a separately reviewed Objective 3 correction.

Relevant implementation files are:

- `src/tax-hub/src/TradeControl.Tax.UK.Company.Contracts/CompaniesHouse/Accounts/Tis5_9/CompaniesHouseContracts.cs`
- `src/tax-hub/src/TradeControl.Tax.UK.Company.Contracts/CompaniesHouse/Accounts/Tis5_9/CompaniesHouseEnvelopeSerializer.cs`
- `src/tax-hub/src/TradeControl.Tax.UK.Company.Contracts/ContractInfrastructure/CompanyContractRegistry.cs`
- `src/tax-hub/src/TradeControl.Tax.UK.Company.Contracts/ContractInfrastructure/CompanyServiceCoverage.cs`
- `src/tax-hub/src/TradeControl.Tax.UK.Application/Preparation/CompaniesHouseAccountsPreparer.cs`
- `src/tax-hub/tests/TradeControl.Tax.UK.Company.ContractTests/Program.cs`
- `src/tax-hub/tests/TradeControl.Tax.UK.DataProvision.Tests/CorporateHandoffTests.cs`

## Required first investigation

### 1. Establish the authoritative asset set

Locate and pin, from official sources only:

- the current general and accounts TIS applicable to the target filing date and micro-entity profile;
- every separately referenced Filing TIS base, envelope, form or response/status schema;
- schema-status entries, downloadable examples and validation/business rules;
- the supported full and filleted micro-entity accounts profiles and applicable taxonomy constraints; and
- the official developer-test and production-approval route.

Follow references inside the TIS rather than guessing schema URLs. Determine whether access requires a Companies House developer account or a direct request to its XML/software-filing team. If a required official asset cannot be obtained, document the exact reference, attempted official route and response, then stop fail-closed.

### 2. Reconcile the current preview

Produce a field-by-field and lifecycle comparison covering:

- outer GovTalk/envelope namespaces, class, qualifier and function;
- unique envelope number and correlation semantics;
- company and presenter authentication placement;
- accounts document representation and whether exact iXBRL bytes are embedded, encoded or otherwise transported;
- registrar declarations and full/filleted filing choices;
- acknowledgement, pending, accepted and rejected response shapes; and
- the actual status request/polling mechanism and timing.

Treat differences as evidence for a narrow contract correction. Do not retrofit protocol credentials into `PreparedStatutoryArtifact` or use a fabricated REST-like status path.

### 3. Decide the authorised next step

At the end of reconnaissance, report one of these outcomes:

- the complete official asset set is available and the current Objective 3 boundary can support a Phase 5.11 implementation after the recorded scope/sequence correction is approved;
- official evidence requires a narrow Objective 3 filing-contract correction, stated precisely for human approval; or
- an authoritative external asset or developer-access route is missing, so Companies House also remains blocked.

Only after explicit approval of the scope/sequence correction may the session implement the Phase 5.11 artifact, validation goldens and readiness change. Phase 5.12 remains a separate future phase.

## Non-negotiable boundaries

- Do not change or work around the CT Phase 5.8 blocker.
- Do not alter VAT, Corporation Tax calculations, statutory-accounting semantics, SQL mappings or the accounts equity bridge.
- Do not mark the existing logical envelope or any constituent document `SubmissionReady` without complete official validation evidence and human approval.
- Do not implement Companies House network I/O, presenter secrets, authentication-code storage, polling transport or durable authority attempts; those belong to Phase 5.12.
- Do not send a test or live Companies House filing.
- Do not implement the future replacement filing API.
- Preserve dependency direction: contracts → Application ← adapters, with WebHarness remaining a diagnostic host.

## Verification if implementation is later authorised

At minimum run:

- `TradeControl.Tax.UK.Company.ContractTests`;
- `TradeControl.Tax.UK.Application.Tests` where the public preparation boundary changes;
- `TradeControl.Tax.UK.DataProvision.Tests` corporate handoff coverage in offline mode;
- `TradeControl.Tax.UK.Architecture.Tests`;
- any focused Submission adapter gate tests required to prove zero outbound I/O; and
- a Release build of `src/tax-hub/src/TaxHub.slnx`.

Validate full and filleted artifacts against the pinned official XSDs and examples. Preserve exact iXBRL attachment bytes and SHA-256 values. Run `git diff --check` in both repositories and review every changed file for unrelated VAT or CT work.

## Documentation and review gate

Record authoritative asset provenance, checksums, validation results, the document/authentication-slot map, status lifecycle and any required correction in Work Plan 5 and the appropriate durable assurance record. Do not commit downloaded validation packages unless their licensing, provenance and repository role have been reviewed.

The final report must distinguish:

- investigation from implementation;
- a valid accounts iXBRL document from a valid Companies House filing artifact;
- local schema validation from external developer testing or production approval; and
- technical readiness from any pending Companies House action.

Stop for human review. Do not begin Phase 5.12.

## Working style

- Inspect current official material and source before editing.
- Prefer the smallest coherent correction over a generic cross-authority abstraction.
- Preserve all unrelated worktree changes.
- Do not create external resources, contact Companies House, commit or push without explicit approval.
