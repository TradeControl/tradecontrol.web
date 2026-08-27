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
