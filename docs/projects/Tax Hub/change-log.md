# Tax Hub Change Log

## 27 August 2026 — Self Assessment SQL Node Phase 1: Structural Separation

### Authorised objective

Make the MIN and STD Sole Trader accounting bootstrap templates tax-neutral by removing obsolete Self Assessment/MTD tax-source, tax-tag, mapping, and validation material. Correct consequentially stale comments and remove the two reviewed stale `@IsMTD` forwarding arguments. Preserve unrelated accounting behaviour and stop before wrapper composition, mapping, or consumer-contract work.

### Files changed

- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql`
- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_2026.sql`
- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql`
- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_SA_2026.sql`
- `docs/projects/Tax Hub/change-log.md`

### Meaningful changes

- Removed the legacy QU/EOPS tax-source and tag seeds, historical MIN mappings, and QU/EOPS validation calls from the MIN accounting template.
- Removed the historical STD-owned QU/EOPS mappings and validation calls from the STD accounting template.
- Changed the STD base-step comment from saying MIN includes ITSA sources/tags to describing it as the MIN accounting environment.
- Removed `@IsMTD = 1` from the MIN MTD wrapper and `@IsMTD = 0` from the STD SA wrapper because the live MIN/STD accounting procedures do not declare that parameter.
- Historical mappings were deleted from the accounting templates, not relocated or reinterpreted.

### Validation performed

- Reviewed the focused `src/sqlnode` diff: four SQL files changed, comprising one comment replacement, the two tax-block removals, and the two stale-argument removals.
- `git -C src/sqlnode diff --check` completed without whitespace errors; Git reported only line-ending normalization notices.
- Static searches confirmed that the MIN and STD accounting templates contain no `UK-ITSA`/`UK-SA` source codes, `Cash.tbTaxTagSource`, `Cash.tbTaxTag`, `Cash.tbTaxTagMap`, or `Cash.proc_TaxTagMapValidate` references.
- Static wrapper inspection confirmed all four wrappers still call exactly their existing MIN or STD accounting template, no wrapper calls a dedicated tax procedure, and no `@IsMTD` forwarding argument remains.
- Confirmed no diff in either dedicated tax-seeding procedure.
- Rebuilt `src/tcNodeDb4/tcNodeDb4.sqlproj` with Visual Studio MSBuild 18, Debug/AnyCPU. Result: success (exit code 0); `tcNodeDb4.dacpac` was produced. The build emitted `SQL71502` unresolved-reference warnings concerning `#DatasetCodes` in unrelated synthetic dataset procedures; it emitted no Phase 1-file errors.
- Static accounting-anchor checks confirmed the MIN base-template call, owner-capital setup, VAT handling, tax-year alignment, STD MIN call, and STD category/cash-code additions remain present.

No representative isolated database or configured bootstrap fixture was available, so MIN/STD execution was not performed. Static inspection and a successful database-project build are not presented as runtime proof.

### Unexpected observations

- The initial sandboxed build attempt could not read installed Microsoft SDK metadata under the user profile. Re-running the same build with approved local SDK access succeeded.
- The SQL project has unrelated unresolved temporary-table warnings in synthetic dataset procedures. They were not changed because they are outside Phase 1.

### Deliberately unchanged

- Dedicated MTD and SA tax-source/tag procedures and their validation placement.
- All four wrapper call graphs beyond removal of the two invalid forwarding arguments; dedicated tax-procedure composition remains Phase 2.
- Every Tax Tag mapping; no historical mapping was moved and no new mapping was added.
- Canonical QU/EOPS/SA vocabulary decisions and the separate HMRC contract audit.
- `src/hmrc_mtd`, WebHarness, DTOs, serializers, payload handling, and SQL `Scripts` scratch material.
- Category Tree and CashCode classifications, which remain independent business classifications rather than HMRC taxonomies.
- Pre-existing documentation reorganisation, superproject changes, submodule pointers, and repository history.

## 28 August 2026 — Self Assessment SQL Node Phase 2: Wrapper Composition

### Authorised objective

Compose each Sole Trader variant wrapper from exactly one accounting template and exactly one matching dedicated tax-seeding procedure, with accounting initialisation first. Make each wrapper the outer atomic transaction boundary, following the established company-template composition pattern, while preserving child-procedure transaction, error-handling, return-code, and validation conventions. Stop before mappings or HMRC contract changes.

### Files changed

- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql`
- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_SA_2026.sql`
- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_MTD_2026.sql`
- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_SA_2026.sql`
- `docs/projects/Tax Hub/change-log.md`

### Meaningful changes

- MIN MTD now calls `App.proc_Template_ST_SOLE_CUR_MIN_2026` and then `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026`.
- MIN SA now calls `App.proc_Template_ST_SOLE_CUR_MIN_2026` and then `App.proc_Template_ST_SOLE_CUR_TAX_SA_2026`.
- STD MTD now calls `App.proc_Template_ST_SOLE_CUR_STD_2026` and then `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026`.
- STD SA now calls `App.proc_Template_ST_SOLE_CUR_STD_2026` and then `App.proc_Template_ST_SOLE_CUR_TAX_SA_2026`.
- Each tax procedure call occurs after the accounting procedure and before the wrapper returns its existing accounting return code.
- All four wrappers now set `NOCOUNT` and `XACT_ABORT` on, begin a wrapper-owned transaction before accounting initialisation, and commit it only after the matching tax procedure succeeds.
- The outer transactions are named per variant: `SoleTraderMinMtdTemplate`, `SoleTraderMinSaTemplate`, `SoleTraderStdMtdTemplate`, and `SoleTraderStdSaTemplate`.

### Validation performed

- A static call-graph assertion confirmed that every wrapper contains exactly two relevant calls in the required order: one MIN/STD accounting procedure followed by one matching MTD/SA tax procedure.
- The same assertion confirmed no MTD wrapper calls the SA procedure and no SA wrapper calls the MTD procedure.
- Static transaction-order inspection confirmed each wrapper follows `BEGIN TRAN` -> accounting procedure -> matching tax procedure -> `COMMIT TRAN` -> `RETURN`, with the existing `App.proc_ErrorLog` catch retained.
- Reviewed the focused wrapper diff: Phase 2 adds one matching parameterless tax-procedure call to each wrapper and makes no other Phase 2 SQL change.
- `git -C src/sqlnode diff --check` completed without whitespace errors; Git reported only line-ending normalization notices.
- Rebuilt `src/tcNodeDb4/tcNodeDb4.sqlproj` with Visual Studio MSBuild 18, Debug/AnyCPU. Result: success (exit code 0); `tcNodeDb4.dacpac` was produced. As in Phase 1, the build emitted unrelated `SQL71502` warnings concerning `#DatasetCodes` in synthetic dataset procedures and no wrapper errors.
- Confirmed that the dedicated MTD procedure still validates `UK-ITSA-SE-QU` and `UK-ITSA-SE-EOPS`, and the dedicated SA procedure still validates `UK-SA-SE-RETURN`, before their respective commits. Validation was not moved or redesigned.

No representative isolated database or configured bootstrap fixture was available, so the four wrappers were not executed. Static composition checks and a successful project build are not presented as runtime proof of source/tag creation, idempotency, or rollback behaviour.

### Transaction and failure observation

The wrappers now own the outer transaction across both composition calls. SQL Server nested `BEGIN TRAN` statements in the accounting and tax procedures increment `@@TRANCOUNT`; their matching inner `COMMIT` statements decrement it without committing the underlying transaction. The wrapper's final `COMMIT` is therefore the physical commit for the composed setup. If either child fails, `App.proc_ErrorLog` rolls back the active transaction and reraises, so the wrapper does not intentionally leave the accounting stage committed without its tax vocabulary.

This conclusion follows the live transaction and error-handler structure and the successful database-project build. Runtime failure injection was not available, so atomic rollback remains to be demonstrated in isolated database execution rather than treated as empirically proven.

### Deliberately unchanged

- Dedicated MTD and SA procedures, their source/tag vocabularies, mapping placeholders, transactions, and validation calls.
- MIN and STD accounting templates following accepted Phase 1 structural separation.
- All Tax Tag mappings; no mappings were added, relocated, or inferred.
- Canonical vocabulary decisions, HMRC contract audit, `src/hmrc_mtd`, WebHarness, DTOs, serializers, and payload handling.
- Wrapper signatures, accounting argument forwarding, `@RC` handling, and `App.proc_ErrorLog` catches; only the wrapper transaction layer was added.
- SQL `Scripts` scratch material, unrelated source, submodule pointers, commits, and repository history.

---

## 28 August 2026 — Self Assessment Contract Alignment and Architecture Decision

### Reason for review

Self Assessment SQL Node Phases 1 and 2 were completed and accepted under the then-current architecture.

Before Phase 3 Tax Tag mapping began, the deliberately deferred HMRC contract audit was performed.

The audit established that several historical Trade Control Self Assessment assumptions no longer represented the current MTD Income Tax architecture and that the Test Harness had acquired architectural responsibilities it was never intended to own.

No Phase 1 or Phase 2 implementation is reclassified as erroneous. The required end state changed following subsequent external-contract verification and product decisions.

### Decisions

- Trade Control Sole Trader Self Assessment submission is **MTD Income Tax only**.
- Legacy SA100 / SA103F submission is outside the supported product scope.
- EOPS is not a current MTD Income Tax filing stage and will not remain a supported statutory target.
- Historical SA100, SA103F, EOPS, Tax Tag, C# model, serializer, and harness structures have no continuing authority merely because they already exist.
- Current authoritative external HMRC specifications govern Objective 3 contracts.
- Objective 2 owns the Trade Control statutory projection required to truthfully supply those contracts.
- Objective 3 owns exact HMRC-facing request and response contracts.
- Objective 4 owns transport and communication mechanics.
- The Test Harness is development and verification infrastructure outside the production integration architecture.
- Harness endpoints may observe and exercise Objectives 2, 3, and 4 but do not define alternative canonical payloads or statutory vocabularies.
- Existing QU/EOPS harness behaviour is not protected and may be removed where obsolete.
- Classes and supporting infrastructure within `src/hmrc_mtd` require evidence-based classification rather than being assumed to represent HMRC contracts from repository location.
- Sole Trader personal Income Tax liability is not assumed to be deterministically calculable from the Business Node alone. Trade Control may forecast an estimated liability and reconcile it through the existing period-adjustment mechanism when an authoritative result becomes available.
- Corporation Tax and other legitimately deterministic business-tax calculations remain eligible for direct integration into Trade Control forecasting and scheduling.

### Documentation consequences

The governing documentation was revised to reflect these decisions:

- `specs/tax-hub-spec-programme.md`
- `specs/self-assessment-sql-node-spec.md`
- `specs/reference/sole-trader-field-sets.md`
- `specs/reference/sole-trader-capital.md`
- `specs/tax-hub-test-payloads.md`

The former Objective 2 implementation instructions and associated work plan describe the architecture under which that earlier work was performed. They are retained as historical implementation material but are superseded for current Self Assessment work by the revised governing specifications and subsequent authorised session briefs.

### Current implementation position

Self Assessment SQL Node Phase 1 remains complete and accepted.

Self Assessment SQL Node Phase 2 remains complete and accepted.

The MTD-only decision makes the legacy SA wrappers and SA tax-seeding procedure retirement candidates.

The removal of EOPS from the current filing lifecycle makes the existing EOPS Tax Source and associated structures retirement/reconciliation candidates.

No retirement, replacement vocabulary, mapping, `hmrc_mtd` refactor, harness refactor, or other implementation change was performed as part of this architectural review.

The next implementation-related stage remains gated by **Phase 3 — Contract-Aligned MTD Reconnaissance and Proposal**.

## 29 August 2026 — Self Assessment Phase 4A: Structural Retirement

### Authorised objective

Remove the Sole Trader SA bootstrap, EOPS bootstrap content, obsolete QU/EOPS harness paths, and the obsolete local Quarterly Update, EOPS and Final Declaration contract interpretations. Preserve the supported MIN/STD MTD wrappers and potentially shared or unverified cross-regime capabilities. Stop before introducing replacement cumulative or annual architecture.

### SQL components retired

- Deleted `App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_SA_2026.sql`.
- Deleted `App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_SA_2026.sql`.
- Deleted `App/Stored Procedures/proc_Template_ST_SOLE_CUR_TAX_SA_2026.sql`, removing the live bootstrap seed for `UK-SA-SE-RETURN` and its SA100/SA103F tags.
- Removed all three deleted procedures from `tcNodeDb4.sqlproj`.
- Removed the `STMIN26-SA` and `STSTD26-SA` registrations, stored-procedure references and SA100/SA103F descriptions from `App.proc_NodeDataInit`.
- Removed the `UK-ITSA-SE-EOPS` source seed, its 25 tag seeds and its validation call from `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026`.
- Reworded the remaining MTD mapping placeholder so it no longer promises QU/EOPS mappings.
- Reworded the two live MTD template descriptions so they no longer advertise EOPS or claim completed mappings.

The supported `App.proc_Template_ST_SOLE_CUR_MIN_MTD_2026` and `App.proc_Template_ST_SOLE_CUR_STD_MTD_2026` wrappers were not changed. Each still owns its outer transaction and composes its matching accounting template followed by `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026`.

### `hmrc_mtd` components retired

- Deleted the complete obsolete contract folders:
  - `Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate`;
  - `Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops`;
  - `Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration`.
- Deleted the `MTDITSA/Shared` generic list/category models whose only consumers were those three obsolete interpretations.
- Deleted `QuHarnessPayloadBuilder`, `EopsHarnessPayloadBuilder`, `QuValidator`, `EopsValidator`, `QuHarnessPayload`, and `EopsHarnessPayload`.
- Deleted `QuTestController` and `EopsTestController`.
- Removed `SubmitQu` and `SubmitEops` from `OperationType`, runner construction, validation dispatch and execution dispatch.
- Removed the corresponding dependency-injection registrations and `.http` requests.
- Removed the QU and EOPS capability claims from the `hmrc_mtd` README.

### Shared and ambiguous components deliberately retained

- The historical `SA100` schedule, envelope, serializer, canonicalisation and IRmark code remains. Although the Sole Trader SA bootstrap is retired, the session explicitly prohibited deleting generic XML/canonicalisation/IRmark capability merely by association. Its internal dependency cluster and possible cross-regime reuse require a separate decision.
- `MTDITSA/Obligations`, `MTDITSA/Payments` and `MTDITSA/Liabilities` remain because Phase 3 classified them as unverified rather than positively obsolete. The runner enquiry branches still return not-implemented results.
- VAT and Micro harness paths, shared SQL readers/mappers, transport placeholders, and all Corporation Tax code remain unchanged.
- `tcNodeDb4/Scripts/MTDSoleTraderMappingEnquiry.sql` still contains EOPS exploration. It is intentionally retained as the previously classified non-authoritative developer scratch script and is neither built nor an acceptance asset.
- Archived SQL and historical programme findings retain SA/EOPS references as implementation history.

### Validation performed

- Static searches found no live SQL project or bootstrap reference to the deleted SA wrapper/tax procedures, `UK-SA-SE-RETURN`, or `UK-ITSA-SE-EOPS`. The only non-archived SQL EOPS matches are in the intentionally retained scratch script.
- Static searches found no surviving QU/EOPS controller, builder, validator, payload, runner operation, dependency-injection registration, `.http` request, or obsolete Quarterly Update/EOPS/Final Declaration endpoint path in live `hmrc_mtd` source.
- Static wrapper inspection confirmed the MIN and STD MTD call graphs and outer transaction boundaries remain intact.
- Rebuilt `src/tcNodeDb4/tcNodeDb4.sqlproj` with Visual Studio MSBuild 18, Debug/AnyCPU: success. Existing unrelated `SQL71502` warnings concerning `#DatasetCodes` remain.
- Built `HMRC_MTD.csproj` with `dotnet build --no-restore`: success, zero warnings and zero errors.
- Built `HMRC.WebHarness.csproj` with `dotnet build --no-restore`: success, zero warnings and zero errors. An initial parallel build collided on the shared `HMRC_MTD.dll` output; the required sequential rerun succeeded.
- `git diff --check` completed without whitespace errors; only line-ending normalization warnings were reported.

### Intentionally incomplete baseline

- No cumulative or annual Tax Source, Tax Tag, mapping, DTO, endpoint, serializer, harness endpoint or transport was introduced.
- The surviving `UK-ITSA-SE-QU` seed remains historical vocabulary pending the separately authorised constructive phase.
- This source retirement prevents future bootstrap creation of SA/EOPS objects. It does not contain a data migration that deletes already-seeded `App.tbTemplate`, `Cash.tbTaxTagSource`, `Cash.tbTaxTag` or mapping rows from an existing deployed database; deployment cleanup must be designed and authorised with the normal upgrade path.
- No configured database fixture was available, so wrapper execution and deployed-object cleanup were not tested.
- Existing `TcBusinessTaxReader` sign handling and generic `TagMapper` zero filling remain because their retained Micro consumer prevents deletion and redesign is outside Phase 4A.

Phase 4A stops at this cleaner, intentionally incomplete baseline. Replacement cumulative and annual implementation has not begun.

## 29 August 2026 — Self Assessment Phase 4D: Cumulative Projection Foundation

### Authorised objective

Implement the approved Sole Trader cumulative SQL projection and the minimum Objective 2 C# consumption boundary. This phase deliberately excludes HMRC request DTOs, serialization, transport, harness endpoints and UI.

### SQL implementation

- Replaced the constructive `UK-ITSA-SE-QU` seed with `UK-ITSA-SE-CUM` and the approved sixteen-tag manifest: two income tags, the consolidated-expense alternative and thirteen directed detailed expense tags.
- Initially added nullable `Cash.tbTaxTag.StatutoryPolarityCode`, constrained to Income or Expense and linked to `Cash.tbPolarity`. This initial design was superseded on 30 August 2026 by the mandatory `CashPolarityCode` correction recorded below.
- Added `CT-CUMEXP` to MIN as a neutral accounting/reporting roll-up of cost of sales, staff costs and overheads. Owner movements, tax and asset movements remain outside it.
- Re-enabled `CC-EMPNI` for Sole Traders.
- Reworked STD into the approved detailed expense categories and CashCodes, including the approved code moves. `CC-DIRCT` and `CC-ADMIN` are disabled after their deterministic replacements are installed.
- MIN MTD now replaces this source's mappings with turnover, other business income and consolidated expenses. STD MTD replaces them with turnover, other business income and all thirteen detailed expense mappings, with no consolidated mapping. Mapping installation remains inside each wrapper's outer transaction.
- Added `Cash.vwTaxTagCashCode`, a reusable expansion of effective enabled mapping roots to enabled nominal leaf CashCodes. It preserves separate routes so parent/descendant and multiple-root duplication remain detectable and exposes the existing leaf `CashPolarityCode` without another traversal.
- Extended mapping validation to cover the exact cumulative manifest, empty or invalid mapping roots, non-neutral leaf resolution, statutory-orientation mismatch, duplicate inclusion within a tag, cross-tag CashCode overlap, consolidated/detailed coexistence, both required income mappings and complete thirteen-tag detailed readiness. Validation is based on effective mappings, not bootstrap identifiers.
- Added `Cash.fnTaxBizCumulative(@TaxSourceCode, @PeriodStart, @PeriodEnd)`. It returns the complete manifest with global validation status, per-tag support status, Trade Control economic amount and statutory amount. Income retains economic sign; Expense multiplies economic sign by `-1`. Unsupported values remain null, while mapped genuine zeros remain supported zeros.
- The cumulative interface accepts only exact configured boundaries: the start must be the configured April 6 financial-year start and the inclusive end must immediately precede another configured Trade Control period start. It does not use or alter `Cash.fnTaxTypeDueDates`, and it does not assume calendar-month boundaries.

### Objective 2 C# implementation

- Added the minimum cumulative projection/value models and explicit orientation, support and validation enums.
- Added `TcBusinessTaxReader.ReadCumulativeAsync`, which calls `Cash.fnTaxBizCumulative` with explicit start/end dates and consumes its validated statutory amounts without applying `Math.Abs` or inventing zeros.
- Kept the older `ReadAsync` absolute-value behaviour isolated for its retained Corporation Tax Micro harness consumers. That method is not used by the Sole Trader cumulative statutory path; changing the unrelated Micro contract was outside Phase 4D.

### Acceptance fixture

- Added `Tests/Phase4D_CumulativeProjection.sql` as a project-listed, repeatable, rollback-only database acceptance fixture rather than extending the non-authoritative scratch enquiry.
- The fixture checks exact MIN/STD mapping inventories, complete manifest and bootstrap validation, ordinary income, ordinary expense, expense credits/reversals, credits exceeding expenditure, genuine zero versus unsupported, parent/descendant overlap, cross-tag overlap, neutral polarity failure, consolidated/detailed coexistence, mapping-driven customised-tree capability, incomplete customised-tree failure and configured financial-boundary rejection.

### Validation performed

- Rebuilt `src/tcNodeDb4/tcNodeDb4.sqlproj` with Visual Studio MSBuild 18, Debug/AnyCPU: success. The dacpac was produced. Existing unrelated `SQL71502` warnings concerning synthetic-dataset `#DatasetCodes` references remain.
- Built `HMRC_MTD.csproj` with `dotnet build --no-restore`: success, zero warnings and zero errors.
- Static inventory inspection confirms MIN has exactly three mapping roots and no detailed mapping; STD has thirteen distinct detailed expense tags (fourteen expense roots because travel has two disjoint roots), two income roots and no consolidated mapping.
- Static source inspection confirms no constructive `UK-ITSA-SE-QU` path remains. Historical findings and the explicitly non-authoritative `Scripts/MTDSoleTraderMappingEnquiry.sql` retain references as evidence only.
- Static change inspection confirms Phase 4D added no HMRC request DTO, serializer, endpoint, OAuth, transport, harness endpoint or UI implementation.
- `git diff --check` reported no whitespace errors; only repository line-ending normalization notices were emitted.

The local SQL Server service was present, but `sqlcmd` could not establish an integrated connection because the installed ODBC client reported an encryption/credential-provider failure. The database fixture was therefore authored and included but could not be executed in the available environment. Its numerical and mutation assertions must be run against freshly bootstrapped MIN and STD MTD nodes when a working database connection is available.

### Deployment note

The source-tree bootstrap no longer constructs `UK-ITSA-SE-QU`. Existing deployed nodes may still contain that historical source and its tag rows. Phase 4D does not delete deployed business data; removal or migration of already-seeded legacy rows remains an explicitly authorised upgrade-path task.

Phase 4D stops at the validated Objective 2 foundation. No Objective 3 or transport implementation has begun.

## 30 August 2026 — Generic Tax Tag Validator Correction

### Authorised objective

Correct `Cash.fnTaxTagMapValidate` so it enforces the shared Tax Tag mapping contract without embedding Sole Trader/HMRC manifest or submission-readiness knowledge. No Objective 3 or unrelated Tax Hub work was authorised or performed.

### Evidence reviewed

- Revised authoritative `specs/reference/sole-trader-field-sets.md`, including the agreed Rollup/Component/Derived mapping contract.
- Complete current `findings.md` and `change-log.md`.
- Current validator function/procedure, effective mapping view, tag/map/category/code tables, category relationship direction, enablement fields, polarity fields and existing business-tax CashCode view.
- The pre-Phase-4D validator from the parent of SQL Node commit `60bbbc7`, used as the original function spool.

### Original invariants and decisions

- Retained: enabled selected-source map processing, Category descendant expansion, direct CashCode mapping and same-tag duplicate-route failure.
- Corrected: missing/disabled/ineligible roots now fail with specific generic messages; configured tag orientation is compared with every effective Component leaf for every source rather than one named source.
- Added from the agreed class contract: any map row attached to Rollup or Derived fails; only Component tags are mappable.
- Restored with corrected semantics: uncovered enabled CashCodes now produce warnings only when they are enabled nominal leaves in `App.vwTaxBizCashCodes`, the configured net-profit/P&L universe, and are absent from the selected source's effective Component coverage. A CashCode covered through a mapped ancestor is not warned; disconnected owner/capital and other non-P&L branches are outside this warning boundary.
- Kept source-specific: cross-tag exclusivity, exact manifest, tag count, named required income fields, consolidated/detailed alternatives and thirteen-field readiness. Different Component fields can only be declared mutually exclusive by their approved source contract; the generic validator must not assume that relationship.

### Exact edits

- `Cash/Functions/fnTaxTagMapValidate.sql`: removed all source/tag literals and readiness manifests; added data-driven source existence, tag-class eligibility, root eligibility, effective contributor, same-tag overlap, configured-polarity and corrected uncovered-P&L warning checks.
- `Cash/Stored Procedures/proc_TaxTagMapValidate.sql`: changed the hard-coded `MTD` error heading to generic `Tax Tag` wording.
- `Cash/Views/vwTaxTagCashCode.sql`: disabled or ineligible Category roots and descendants no longer enter effective coverage.
- `Tests/TaxTagMapValidator.sql`: added a repeatable rollback-only generic fixture covering valid Component mappings, Rollup/Derived rejection, disabled roots, same-tag parent/descendant overlap, configured polarity mismatch and indirect-coverage-aware warnings.
- `Tests/Phase4D_CumulativeProjection.sql`: moved Sole Trader cross-tag and consolidated/detailed assertions out of the generic validator and retained them as source-specific fixture checks.
- `tcNodeDb4.sqlproj`: listed the new generic fixture as a non-build acceptance asset.
- `findings.md`: appended the pre-edit reconnaissance, lost-invariant assessment and corrected warning rationale.

The canonical quarterly accounting tag seed and `Cash.tbTaxTag` schema were not changed in this correction.

### Verification

- Static search found no source code, HMRC/MTD term, statutory tag name, manifest count, consolidated/detailed term or readiness literal in the generic function/procedure pair.
- SQL project rebuild with Visual Studio MSBuild 18, Debug/AnyCPU: success; the dacpac was produced. Existing unrelated `SQL71502` warnings for synthetic-dataset `#DatasetCodes` references remain.
- `git diff --check` reported no whitespace defects; only repository line-ending notices.
- The repeatable database fixtures could not be executed because the available local SQL Server still cannot establish an integrated connection in this environment. The generic behavioural assertions are therefore present but remain runtime-pending.

### Narrow unresolved point

The validator's corrected uncovered-CashCode warning uses the existing configured net-profit/P&L universe as the strongest schema-backed definition of business-tax relevance. If a future Tax Source intentionally projects accounting values outside that universe, its coverage-warning boundary will need explicit source metadata rather than another hard-coded exception.

## 30 August 2026 — Mandatory Tax Tag Cash Polarity

### Authorised correction

- Renamed `Cash.tbTaxTag.StatutoryPolarityCode` to `CashPolarityCode` and made it `NOT NULL`, retaining the Income/Expense check and foreign key to `Cash.tbPolarity`.
- Renamed the related constraints and propagated the column name through the generic validator, cumulative SQL projection, Sole Trader cumulative tag seed and Objective 2 .NET reader.
- Made polarity comparison unconditional in the generic validator: every Tax Tag now declares its required accounting cash polarity, and every effective contributor must match it.
- Updated the rollback-only validator fixture terminology.

No migration was added because the product has no deployed database requiring preservation at this development stage. The existing Corporation Tax tag seed deliberately remains without `CashPolarityCode`; it will now fail against the current schema and thereby expose that non-compliant template until its later scheduled update.

## 30 August 2026 — Template Bootstrap and Synthetic Dataset Key Correction

- Corrected the Sole Trader template keys in `App.proc_NodeDataInit` from over-width `STMIN26-MTD` / `STSTD26-MTD` values to the existing ten-character-compatible `STMIN26` / `STSTD26` keys used by `App.tbTemplateDataset`.
- Changed `App.proc_DatasetSyntheticMIS` and `App.proc_DatasetSyntheticMIS_Bootstrap` to select, validate and pass templates by `TemplateCode`, not mutable `TemplateName` text.
- Corrected synthetic Sole Trader selection so `UseStdCompanyTemplate` chooses `STMIN26` or `STSTD26`, matching the dataset configuration rows; the obsolete hard-coded SA title was removed.
- Changed `App.proc_BasicSetup` to require `@TemplateCode` as its first parameter and removed the title-based parameter and lookup entirely.
- Updated `TradeControl.Web.Data.NodeContext.InstallBasicSetup` to pass `@TemplateCode`.
- Updated `SetupPanel.razor` so the selection value and editor state are `TemplateCode`, while the user continues to see `TemplateName`; descriptions and VAT defaults are also keyed by code.
- Rebuilt `tcNodeDb4.sqlproj` successfully and produced the dacpac. `TCWeb.csproj` also builds successfully; its existing unrelated MudBlazor analyzer warnings remain.

## 30 August 2026 — Sole Trader STD Code-Width Correction

- Audited code literals across every 2026 template procedure against their target schema widths.
- Replaced the sole over-width Category identifier, `CA-INTEREST` (11 characters), with `CA-LOANINT` (10 characters) in the STD Category seed, hierarchy, CashCode assignments and cumulative Tax Tag mapping.
- Updated the design record to use the executable identifier consistently.

## 30 August 2026 — STD Synthetic Miscellaneous Payment Correction

- Corrected `App.proc_DatasetSyntheticMIS_PayMisc`, which continued to post energy and provisions payments to `CC-ADMIN` after the STD template disabled that coarse aggregate code.
- Energy payments now select enabled `CC-UTILS`; provisions select enabled `CC-OTHER`. Each falls back to enabled `CC-ADMIN` for MIN templates, preserving the intentionally coarse MIN classification without relying on a disabled code in STD.
- Added an explicit failure when neither the semantic code nor the MIN fallback is available.
