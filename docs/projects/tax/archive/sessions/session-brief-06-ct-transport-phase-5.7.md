# Session brief — Corporation Tax integration, Objective 4 Phase 5.7

2 October 2026

## Session objective

Resume Objective 4 at **Work Plan 5, Phase 5.7 — CT conversation marker and fail-closed package gate**.

Implement only the narrow Phase 5.7 boundary correction:

1. represent HMRC Corporation Tax as a Transaction Engine conversation that requires status polling;
2. introduce the corresponding Application polling marker without inventing a REST-style status path;
3. define a CT-specific typed outcome/port boundary for pending, terminal and uncertain conversation states; and
4. prove that every current diagnostic, preview, invalid, unsupported or mixed-status CT package is rejected before outbound I/O.

Stop at the Phase 5.7 human review gate. Do not begin Phase 5.8 service-artifact assurance or Phase 5.9 Transaction Engine protocol implementation.

## Repository and checkpoint

Workspace:

`C:\Users\Ian\source\repos\tradecontrol.web`

The Tax Hub implementation is the Git submodule at:

`src\tax-hub`

Clean starting checkpoints:

- `tradecontrol.web` `master`: `fd2487c` — `VAT Objective 5 Phase 6.7 application submitted`
- `tax-hub` `master`: `0840a79` — `Vat Objective 5 Phase 6.6`

Confirm both repositories are clean before editing. Do not create a new worktree unless explicitly requested. Do not commit or push without explicit approval. If a Phase 5.7 checkpoint is later approved, commit the submodule first and then the parent repository so the parent records the correct submodule pointer.

## Programme position

VAT is no longer the active engineering stream:

- Objective 5 Phase 6.6 is accepted as technically approval-ready.
- The initial VAT production-access request was sent to HMRC at 15:17 BST on 2 October 2026.
- Phase 6.7 remains externally open while HMRC responds; production access, credentials, live filing and compatible-software listing are not claimed.
- Do not alter the accepted VAT implementation or contact HMRC during this phase.

Work Plan 5 Phase 5.6 has been updated to record the **technical VAT transport milestone as accepted with external approval pending**. That technical acceptance satisfies Phase 5.7's dependency without pretending that HMRC has granted production access.

## Governing documents

Read these before changing code:

1. `docs/projects/Tax Hub/implementation/tax-hub-workplan-5.md`
   - especially the objective boundary, invariants, Phase 5.6 status and all of Phase 5.7;
2. `docs/projects/Tax Hub/specs/tax-hub-spec-programme.md`
   - Objective 3 owns statutory meaning and prepared artifacts; Objective 4 owns transport;
3. `docs/projects/Tax Hub/implementation/tax-hub-workplan-4.md`
   - accepted Objective 3 company/CT preparation boundary;
4. `docs/projects/Tax Hub/implementation/tax-hub-implementation-3.md`;
5. `docs/projects/Tax Hub/specs/reference/hmrc-transport.md`;
6. `docs/projects/Tax Hub/implementation/tax-hub-repo-structure.md`; and
7. current official HMRC Transaction Engine and IRmark guidance where a protocol fact requires verification. Use primary official sources only.

Earlier documents are historical context and do not override the accepted Work Plan 5 boundary.

## Current code facts

The following facts were verified at handover:

- `Company.Contracts/Hmrc/CorporationTax/Submission/V2026/CorporationTaxPackage.cs`
  - `CorporationTaxEndpointSet.Submit` is pinned to `CT600-V3-2026-RIM-1.994` and the `Transaction Engine XML` family;
  - `RequiresStatusPolling` is currently `false` and must become truthful.
- `Application/Preparation/PreparedArtifacts.cs`
  - `SubmissionPollingMode` currently contains only `None` and `PollUntilTerminal`;
  - `IPreparedSubmissionPackageGateway.SendAsync` currently returns `Task` and has no safe typed CT conversation outcome.
- `Application/Preparation/CorporationTaxPreparer.cs`
  - the transmission and all package documents are deliberately `Preview`;
  - the transmission is produced by `CorporationTaxPackageSerializer` and is diagnostic, not an HMRC gateway payload;
  - polling is currently `SubmissionPollingMode.None`.
- `DataProvision.Tests/CorporateHandoffTests.cs`
  - currently asserts Corporation Tax polling is `None`;
  - its capturing gateway proves byte preservation but does not enforce dispatch eligibility or zero-I/O rejection.
- `Company.ContractTests/Program.cs`
  - pins the RIM version and deterministic diagnostic XML but does not yet assert truthful Transaction Engine polling metadata.
- Companies House already uses `SubmissionPollingMode.PollUntilTerminal`; do not change its semantics.

## Required implementation

Follow the accepted Phase 5.7 plan rather than designing the later protocol handler.

### 1. Correct CT metadata

- Set `CorporationTaxEndpointSet.Submit.RequiresStatusPolling` to `true`.
- Keep the protocol family explicitly pinned as HMRC Transaction Engine XML.
- Add `SubmissionPollingMode.TransactionEngine`.
- Use that marker in `CorporationTaxPreparer` with no relative status path. Transaction Engine supplies correlation, response endpoint, interval and deletion behaviour later; it is not a REST polling URL.

### 2. Add the narrow CT outcome boundary

Define the minimum CT-specific outcome and port return needed to distinguish:

- pending conversation with a safe correlation reference;
- terminal acceptance with safe receipt evidence;
- terminal business rejection with bounded error evidence;
- unknown/recovery-required state; and
- deletion state separately from tax acceptance.

Do not force HMRC Transaction Engine and Companies House into one generic outcome model merely because both poll. Prefer a CT-specific port/result or another equally narrow design justified by the existing dependency direction.

This phase defines the boundary only. It does not implement submit/poll/delete HTTP, GovTalk, IRmark, `DATA_REQUEST`, endpoint validation or receipt verification; those belong to Phase 5.9.

### 3. Enforce a fail-closed package gate

At the package port or first adapter entry, reject before outbound I/O when any of the following is true:

- transmission or any constituent document is `Preview`;
- any artifact has an error finding;
- any artifact is unsupported or not `SubmissionReady`;
- package statuses are mixed or inconsistent;
- the service-artifact identity is not the specifically approved CT service artifact; or
- the operation is diagnostic rather than eligible for submission.

The CT family remains disabled regardless of individual labels until Phase 5.8 establishes the genuine validated service artifact and an explicit eligibility marker. Merely relabelling current diagnostic XML must never make it sendable.

### 4. Strengthen regression evidence

Update the existing company contract and corporate handoff tests and add focused gate tests covering:

- the corrected Transaction Engine metadata;
- every current CT preview artifact;
- a forged mixed-ready package;
- a diagnostic operation;
- blocking findings and unsupported status; and
- a recording handler that proves **zero sends** for every rejected case.

Preserve the existing company document bytes, diagnostic transmission bytes and SHA-256 values. Do not update a golden merely to accommodate an unintended byte change.

## Non-negotiable boundaries

- Do not change Corporation Tax calculations, CT600 field population, accounts semantics, SQL source mappings or the equity bridge.
- Do not change `CorporationTaxPackageSerializer` bytes.
- Do not generate or insert an IRmark.
- Do not provision official XSD, Schematron, taxonomies or samples; that is Phase 5.8.
- Do not implement or call GovTalk/Transaction Engine endpoints; that is Phase 5.9.
- Do not make any real or sandbox HMRC submission.
- Do not alter Companies House polling or begin Phases 5.11–5.13.
- Do not wire modern transport into the legacy diagnostic runner or make a preview identifier sufficient authority to submit.
- Preserve dependency direction: contracts → Application ← adapters, with WebHarness as a host/diagnostic client only.

## Verification

At minimum run:

- `TradeControl.Tax.UK.Company.ContractTests`;
- `TradeControl.Tax.UK.Application.Tests` where the public preparation boundary changes;
- `TradeControl.Tax.UK.DataProvision.Tests` corporate handoff coverage;
- `TradeControl.Tax.UK.Architecture.Tests`;
- any new focused Submission adapter tests; and
- a Release build of `src/tax-hub/src/TaxHub.slnx`.

If the Data Provision executable requires an unavailable secret-backed `TC_NODE_CONTEXT`, do not fabricate it. Run all offline coverage that does not require the secret, prove that the project compiles, and report the qualification explicitly.

Run `git diff --check` in both the submodule and parent repository. Review all changed files and confirm that no unrelated VAT or Phase 5.8/5.9 work entered the diff.

## Documentation and review gate

Record the Phase 5.7 implementation evidence in `tax-hub-workplan-5.md` and add a concise durable finding only if the implementation uncovers a generally reusable architectural fact.

The final report must state:

- which metadata and port contracts changed;
- how the fail-closed gate prevents every current CT preview from reaching I/O;
- tests/builds run and their results;
- whether any byte/digest changed; and
- remaining Phase 5.8/5.9 exclusions.

Stop for human review. Phase 5.7 may be accepted only when the reviewer confirms that no present company preview can reach an authority.

## Working style

- Inspect and reason from the current source before editing.
- Prefer the smallest coherent change; do not introduce a generic workflow framework.
- Preserve unrelated user changes if the worktree is unexpectedly dirty and report them before proceeding.
- Do not create Azure resources or deploy anything for this phase.
- Do not commit at every edit. Commits are made only at an approved phase/session checkpoint.
