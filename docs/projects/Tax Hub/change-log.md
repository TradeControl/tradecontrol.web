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
