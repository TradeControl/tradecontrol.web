# Category Tree schema change plan

**Date:** 2 November 2026
**Status:** Increment 2 implementation plan; no schema implementation started
**Governing documents:** `category-tree-change-control-workplan.md` and `category-tree-increment-1-inventory-and-contract.md`
**Scope:** local tcNodeDb4, TCWeb and TCExports changes only

## 1. Purpose

This document turns the accepted Category Tree model into an implementation-safe SQL plan. It identifies the target relational design, current physical and logical dependencies, the active-view compatibility boundary, the order in which objects must change, and the tests that must pass before the schema can be treated as authoritative.

Existing local sandbox databases may be regenerated from the supported templates after the schema is implemented.

## 2. Accepted schema boundary

The controlled Category Tree consists of version-owned categories, total edges, Cash Code mappings and roots. Cash Code identity and BI expressions remain global:

```text
Cash.tbCategoryVersionStatus
            |
App.tbOptions.CategoryVersionId
            |
Cash.tbCategoryVersion ---- Cash.tbCategoryVersionEvent
            |
            +---- Cash.tbCategory
            |          |
            |          +---- Cash.tbCategoryTotal
            |          +---- Cash.tbCategoryCodeMap ---- Cash.tbCode
            |          +---- Cash.tbCategoryRoot -------- Cash.tbCategoryRootType
            |
            +---- active views used by ordinary calculations

Cash.tbCategoryExp   current BI formula metadata; not version-owned
Cash.tbCode          persistent Cash Code identity; not version-owned
```

The canonical Category Tree object contains only `tbCategory`, `tbCategoryTotal`, `tbCategoryCodeMap` and `tbCategoryRoot` rows for one `VersionId`.

## 3. Target relational design

### 3.1 `Cash.tbCategoryVersionStatus`

| Column | Proposed SQL type | Rule |
|---|---|---|
| `VersionStatusCode` | `smallint` | Primary key: `0 Draft`, `1 Published`, `2 Submitted`, `3 Archived`. |
| `VersionStatus` | `nvarchar(20)` | Required and unique. |

The four rows are seeded by `App.proc_NodeDataInit`. They are system vocabulary and are not tenant-editable.

### 3.2 `Cash.tbCategoryVersion`

| Column | Proposed SQL type | Rule |
|---|---|---|
| `VersionId` | `nvarchar(10)` | Immutable primary key, system allocated and never reused. A practical allocator is `CV00000001` through a SQL sequence and guarded procedure. |
| `VersionStatusCode` | `smallint` | Foreign key to `tbCategoryVersionStatus`. |
| `Description` | `nvarchar(100)` | Required user-facing description. |
| `Notes` | `nvarchar(max)` | Optional longer explanation; a successor to Submitted requires a meaningful publication note. |
| `PredecessorVersionId` | `nvarchar(10)` | Self-reference; null only for the commissioned first version. |
| `BaseContentSha256` | `binary(32)` | Hash of the active predecessor at clone time; required for every successor Draft. |
| `CanonicalProfile` | `nvarchar(50)` | `trade-control-category-tree-v1`. |
| `CanonicalSha256` | `binary(32)` | Current semantic digest. |
| `CanonicalByteLength` | `bigint` | Non-negative canonical byte length. |
| `ContentChangedOn` | `datetime2(3)` | UTC time of the last accepted semantic edit. |
| `EffectiveFromOn` | `datetime2(3)` | UTC start of the half-open active interval. Null for Draft. |
| `EffectiveToOn` | `datetime2(3)` | UTC end of the half-open active interval. Null for Draft and the active version. |
| `CreatedOn` | `datetime2(3)` | UTC creation time. |
| `CreatedByUserId` | `nvarchar(10)` | Foreign key to `Usr.tbUser`, resolved from `Usr.vwCredentials`. |
| `RowVer` | `rowversion` | Optimistic-concurrency token, not a business timestamp. |

Required constraints and indexes:

- primary key on `VersionId`;
- self-foreign key on `PredecessorVersionId`;
- check that predecessor is not self;
- check that `CanonicalByteLength >= 0`;
- check that Draft has null effective dates;
- check that Published and Submitted have `EffectiveFromOn` and no `EffectiveToOn`;
- check that Archived has both dates and `EffectiveToOn >= EffectiveFromOn`;
- index on `VersionStatusCode, CreatedOn` for history; and
- index on `EffectiveFromOn, EffectiveToOn` for period-end resolution.

Version status does not select the active tree. The sole `App.tbOptions.CategoryVersionId` foreign key is the authority. Published and Submitted are the only statuses eligible to be referenced by that pointer.

### 3.3 `App.tbOptions.CategoryVersionId` and the singleton row

Remove `NetProfitCode` and `VatCategoryCode` and add nullable `CategoryVersionId nvarchar(10)` with a foreign key to `Cash.tbCategoryVersion`.

The options table is already operationally a singleton: `App.proc_NodeBusinessInit` inserts exactly one row with `Identifier = 'TC'`, and `App.proc_NodeDataInit` deletes it during destructive reset. The existing primary key does not by itself prevent a different identifier, so the options trigger must add a structural table-count assertion.

`@@ROWCOUNT` alone is not sufficient: it reports only the number affected by the current statement and would not detect a second one-row insert. The `AFTER INSERT, UPDATE` path must count `App.tbOptions` under a serialising/table lock and throw when the total exceeds one. With that enforcement, a separate `CHECK (Identifier = 'TC')` is redundant.

Additional rules are:

- `CategoryVersionId` may be null while `IsInitialised = 0`;
- `IsInitialised = 1` requires a non-null pointer;
- assigning the pointer requires the target to be Published or Submitted;
- changing a referenced version to Draft or Archived is rejected outside the controlled publication transition;
- the foreign key prevents deletion of the selected version; and
- ordinary options updates do not become Category Tree mutations merely because they touch the singleton row.

The publication procedure locks this row and uses it to resolve the predecessor. No query infers the active version from status.

### 3.4 Version code allocation

Add a SQL sequence and one allocator procedure. The procedure formats the next value into ten characters and serialises allocation within the database. Callers cannot supply arbitrary codes. Rolled-back or abandoned values may leave gaps; reuse is prohibited.

The allocator must fail before the ten-character range is exhausted. The selected `CV` prefix is provisional naming detail, not an architectural dependency.

### 3.5 `Cash.tbCategoryVersionEvent`

| Column | Proposed SQL type | Rule |
|---|---|---|
| `VersionEventId` | `bigint identity` | Primary key. |
| `VersionId` | `nvarchar(10)` | Foreign key to `tbCategoryVersion`. |
| `EventTypeCode` | `smallint` | Create, edit, validate, publish, submit, archive or refresh/rebase. |
| `FromStatusCode` | `smallint null` | Previous status when applicable. |
| `ToStatusCode` | `smallint null` | New status when applicable. |
| `EventOn` | `datetime2(3)` | UTC event time. |
| `UserId` | `nvarchar(10)` | Stable application actor. |
| `UserName` | `nvarchar(50)` | Captured display value for durable audit. |
| `ChangeNote` | `nvarchar(max)` | Required for publication and other policy-defined events. |
| `ContentSha256` | `binary(32) null` | Digest resulting from or verified by the event. |

Events are append-only for every retained version. Update and delete permissions are denied to the application principal. Controlled Draft discard deletes the complete unreferenced Draft aggregate, including its events, in one transaction; it does not leave a misleading retained lifecycle event for a version that no longer exists. A compact event-type lookup may be added if that matches the final tcNodeDb4 naming convention.

### 3.6 `Cash.tbCategory`

Add `VersionId nvarchar(10) NOT NULL` and change the key from `CategoryCode` to `(VersionId, CategoryCode)`.

All existing semantic columns remain. Existing unique indexes become version-scoped, for example:

- `(VersionId, IsEnabled, Category)`;
- `(VersionId, IsEnabled, CategoryCode)`; and
- `(VersionId, CategoryTypeCode, DisplayOrder, Category)`.

Audit fields and `RowVer` remain row-local. The update trigger must join on both key columns and invoke the version mutation guard before accepting an insert, update or delete.

Category codes remain stable logical identifiers across versions. A successor may omit a category, but publication must reject an attempt to reuse a historical code for an unrelated meaning. The initial guard can compare category type and other identity-defining attributes across the predecessor chain; no separate category master table is required for this release.

### 3.7 `Cash.tbCategoryTotal`

Add `VersionId` and change the primary key to `(VersionId, ParentCode, ChildCode)`.

Both ends use composite foreign keys to `Cash.tbCategory(VersionId, CategoryCode)`. The no-self-edge, total-parent and cycle trigger must restrict every recursive step to the inserted row's `VersionId`. Indexes required by traversal are:

- `(VersionId, ParentCode, DisplayOrder, ChildCode)`; and
- `(VersionId, ChildCode, ParentCode)`.

No edge can cross versions.

### 3.8 `Cash.tbCategoryCodeMap`

| Column | Proposed SQL type | Rule |
|---|---|---|
| `VersionId` | `nvarchar(10)` | Version owner. |
| `CashCode` | `nvarchar(50)` | Persistent Cash Code identity. |
| `CategoryCode` | `nvarchar(10)` | Leaf category within the same version. |
| `RowVer` | `rowversion` | Optimistic concurrency. |

Required keys and indexes:

- primary key `(VersionId, CashCode)`, giving at most one assignment per Cash Code per version;
- foreign key `CashCode -> Cash.tbCode.CashCode` without cascade delete;
- composite foreign key `(VersionId, CategoryCode) -> Cash.tbCategory`;
- index `(VersionId, CategoryCode, CashCode)` for expansion; and
- validation that the mapped category is enabled and is an eligible leaf category.

`Cash.tbCode.CategoryCode`, its category foreign key and its category-based index are removed only after every dependent object reads the mapping table or active mapping view.

Cash Code creation inserts the global `tbCode` row and its active mapping atomically. Existing Drafts are not silently changed; validation requires a mapping for every Cash Code that exists when the Draft is published, including disabled codes. Archived versions remain immutable and legitimately lack codes created later. A historical Cash Statement encountering transactions for a code absent from the selected version must report the unmapped code rather than silently applying the active mapping.

`Cash.tbCode.IsEnabled` controls operational availability for new work. It must not cause historical transactions to disappear from accounting statements. Statement-classification queries therefore use the version mapping even when the persistent Cash Code has since been disabled; posting and selection lookups may continue to filter `IsEnabled`.

### 3.9 `Cash.tbCategoryRootType` and `Cash.tbCategoryRoot`

`tbCategoryRootType` contains at least:

| `RootTypeCode` | `RootType` |
|---|---|
| `0` | `NET_PROFIT` |
| `1` | `VAT` |

`tbCategoryRoot` has primary key `(VersionId, RootTypeCode)` and a composite foreign key `(VersionId, CategoryCode) -> tbCategory`. Publication requires exactly one valid row for each required root type. The root category must be enabled, have no parent and have at least one child in the same version.

After all consumers are migrated, remove `App.tbOptions.NetProfitCode`, `VatCategoryCode`, the Net Profit foreign key and the legacy root-validation section of `App_tbOptions_TriggerUpdate`. The same trigger then enforces the singleton row and validates `CategoryVersionId` changes.

### 3.10 `Cash.tbCode`

`tbCode` remains the persistent master referenced by projects, invoices, payments, accounts, periods and tax mappings. Its `CashCode`, description, tax code, enabled state and audit fields are not cloned.

Remove only the authoritative `CategoryCode` column and category-specific index after the mapping cutover. Every existing foreign key from transactional tables to `tbCode(CashCode)` remains unchanged. Deletion is rejected while any version mapping or transaction references the code; normal retirement uses `IsEnabled = 0`.

### 3.11 `Cash.tbCategoryExp`

Expressions remain global and keyed by `CategoryCode`. The current foreign key to `Cash.tbCategory(CategoryCode)` cannot survive the composite version key and must be removed, along with its cascade delete/update behaviour.

Expression validity becomes parser/service validation against the displayed or selected version. Active expression views join `tbCategoryExp` to `vwCategoryActive`. Historical Cash Statements use the selected version's category rows with the current formula table and report parse failures.

An expression edit is a separate global BI operation. Editing a Draft's category structure does not itself edit an expression. The UI must make this distinction explicit because changing a formula for a code also present in the active tree can affect current BI output even while a Draft is open.

## 4. Active and explicit-version read surfaces

### 4.1 Required active views

Add the following narrow views before migrating consumers:

| View | Purpose |
|---|---|
| `Cash.vwCategoryVersionActive` | The version header joined from `App.tbOptions.CategoryVersionId`. |
| `Cash.vwCategoryActive` | Active category rows, including `VersionId`. |
| `Cash.vwCategoryTotalActive` | Active total edges. |
| `Cash.vwCategoryCodeMapActive` | Active Cash Code-to-category assignments. |
| `Cash.vwCategoryRootActive` | Active Net Profit and VAT roots. |
| `Cash.vwCodeActive` | Persistent `tbCode` fields plus the active `CategoryCode`; compatibility source for current accounting queries. |

The views must join through the singleton options row and its `CategoryVersionId`; they must not scan status, use `TOP (1)` or choose the newest Published row. A missing pointer after initialisation or an ineligible referenced status is an error.

Ordinary accounts and tax calculations migrate to these views. They do not accept a browser-supplied version.

### 4.2 Version-parameterised functions

Add explicit-version functions for tree traversal and Cash Statement generation, while retaining active wrappers for current calculations:

- `Cash.fnCategoryCashCodesByVersion(@VersionId, @CategoryCode)`;
- `Cash.fnCategoryNamespaceByVersion(@VersionId, ...)`;
- `Cash.fnCategoryPrimaryParentByVersion(@VersionId, ...)`; and
- the smallest version-aware flow/category result surfaces required by TCExports.

Existing functions such as `Cash.fnCategoryCashCodes` become active wrappers over the versioned implementation. They must not duplicate traversal logic.

The historical Cash Statement path passes an eligible `VersionId` to the version-aware functions. This is deliberately separate from ordinary statement views and does not create a general version selector across the product.

## 5. Current dependency inventory

### 5.1 Physical constraints and triggers that must change

| Current object | Current dependency | Required change |
|---|---|---|
| `Cash.tbCategoryTotal` | Parent and child FKs reference `tbCategory(CategoryCode)`. | Replace with same-version composite FKs and version-scoped cycle trigger. |
| `Cash.tbCode` | `CategoryCode` FK and index. | Remove after `tbCategoryCodeMap` cutover. |
| `Cash.tbCategoryExp` | FK with cascade delete/update to `tbCategory`. | Remove; expression lifecycle becomes independent. |
| `App.tbOptions` | `NetProfitCode` FK, two root columns and root-validation trigger. | Replace both roots with `CategoryVersionId`; enforce the singleton row and resolve roots from the referenced version's `tbCategoryRoot`. |
| `Cash.tbTaxTagMap` trigger | Tests category existence without a version. | Validate category mappings against the active version and revalidate a Draft during publication. |
| `Cash.tbPayment` insert trigger | Joins `tbCode.CategoryCode` to `tbCategory`. | Join `vwCodeActive`/`vwCategoryActive`. |
| `Cash.tbPeriod` trigger | Joins through `tbCode.CategoryCode`. | Join active mapping/category views. |

`Cash.tbTaxTagMap.CategoryCode` remains a logical mapping code, not a foreign key to one version. Tax-tag validation is rerun against the active tree and against a Draft before publication.

### 5.2 Static SQL reference fan-out

A repository scan finds at least 88 SQL files that directly reference at least one of `Cash.tbCategory`, `Cash.tbCategoryTotal`, `Cash.tbCode.CategoryCode`, `App.tbOptions.NetProfitCode` or `VatCategoryCode`:

| Area | Stored procedures | Functions | Views | Tables | Scripts/tests | Total |
|---|---:|---:|---:|---:|---:|---:|
| App | 13 | 0 | 4 | 1 | 0 | 18 |
| Cash | 5 | 8 | 29 | 7 | 0 | 49 |
| Invoice | 1 | 0 | 0 | 0 | 0 | 1 |
| Object | 3 | 0 | 1 | 0 | 0 | 4 |
| Project | 2 | 1 | 8 | 1 | 0 | 12 |
| Scripts/tests | 0 | 0 | 0 | 0 | 4 | 4 |
| **Total** | **24** | **9** | **42** | **9** | **4** | **88** |

This is a direct-text inventory, not the full transitive dependency graph. Objects that consume an affected view or function must also be regression-tested even where their own SQL contains no table name.

### 5.3 Highest-risk calculation dependencies

These objects directly determine classification or statutory output and must migrate before the legacy columns are removed:

- `Cash.fnCategoryCashCodes`, `fnCategoryNamespace`, `fnCategoryNamespaceInContext`, `fnCategoryNamespacePrimary`, `fnFlowCategory`, `fnFlowCategoriesByType` and `fnFlowCategoryTotalCodes`;
- `Cash.vwStatementBase`, `vwStatementWhatIf`, `vwCashCodePeriodValues`, `vwProfitAndLossData`, `vwProfitAndLossByPeriod`, `vwProfitAndLossByYear`, `vwBudget`, `vwCategoryBudget`, `vwCategoryCapital` and `vwFlowCategoryByPeriod`;
- `Cash.vwTaxTagCashCode`, `fnTaxTagMapValidate`, `vwTaxBizAccruals`, `vwTaxVatAccruals` and their downstream tax views;
- `App.vwTaxBizCashCodes`, `App.vwTaxVatCashCodes` and `Cash.vwCategoryPrimaryParent`; and
- `Cash.tbPayment` and `Cash.tbPeriod` triggers, whose current decisions depend on `tbCode.CategoryCode`.

These objects require value-level before/after tests, not merely successful compilation.

### 5.4 Lookup and operational dependencies

The following classes primarily need the active compatibility views:

- Cash Code lookup views: `Cash.vwCode`, `vwCodeLookup`, `vwBankCashCodes`, `vwCurrentAccount`, `vwExternalCodesLookup` and `vwTransferCodeLookup`;
- project and object views/procedures including `Project.vwCashMode`, `vwProjects`, `vwQuotes`, `proc_Pay`, `proc_Cost`, `Object.proc_Mode`, `proc_WorkFlow` and `proc_WorkFlowMultiLevel`;
- application candidate and graph views such as `App.vwCandidateCategoryCodes` and `vwGraphTaskActivity`; and
- posting/default procedures including `Cash.proc_CodeDefaults`, `proc_PaymentPostById`, `proc_PaymentPostMisc` and `proc_FlowCategoryCodeFromName`.

Where an object needs only current classification, replace `tbCode.CategoryCode` with `vwCodeActive.CategoryCode` and `tbCategory` with `vwCategoryActive`. Do not add optional version parameters to ordinary operational procedures.

### 5.5 Root dependencies

Only the following repository SQL objects currently read or write the two option roots:

- `App.proc_Template_BASE_MIN_2026`;
- `App.proc_VatSourceEvidence`;
- `App.vwTaxBizCashCodes`;
- `App.vwTaxVatCashCodes`;
- `Cash.vwCategoryPrimaryParent`;
- `Scripts/MTDSoleTraderMappingEnquiry.sql`; and
- `App.tbOptions` itself.

TCWeb also reads/writes the fields through `App_tbOption`, `NodeContext`, `CategoryTreeModel.PrimaryRoot` and the Category Tree details models. The EF options model gains `CategoryVersionId`; root actions move to `tbCategoryRoot`/`vwCategoryRootActive` before the legacy columns are dropped.

### 5.6 Templates, reset and synthetic generation

The following procedures currently manufacture or reset unversioned rows and must be treated as a single commissioning dependency group:

- `App.proc_NodeDataInit`;
- `App.proc_NodeBusinessInit` and `App.proc_BasicSetup`;
- `App.proc_Template_BASE_MIN_2026`;
- all company and sole-trader MIN/STD template variants and wrappers;
- `App.proc_Template_DisableVAT`;
- `App.proc_DatasetSyntheticMIS` and its bootstrap/assets/expenses helpers; and
- `Scripts/EXEC_DatasetSyntheticMIS_Year3_Additive.sql`.

The template procedures should accept or resolve a commissioning `VersionId` and insert category, edge, mapping and root rows under it. `proc_BasicSetup` validates the completed tree, computes its canonical digest, marks it Published, opens its effective interval and assigns `App.tbOptions.CategoryVersionId`. It must not initialise the options row or assign a pointer before the selected template succeeds.

`proc_NodeDataInit` deletes version events and version-owned rows in child-to-parent order, reseeds system vocabulary and resets the version-code sequence for a newly generated database. It must not attempt to preserve history during destructive synthetic regeneration.

### 5.7 EF Core and TCWeb dependencies

The current EF model assumes single-column category keys and `Cash_tbCode.CategoryCode`. Required model changes include:

- composite keys for `Cash_tbCategory` and `Cash_tbCategoryTotal`;
- new entities for version status, version, event, mapping, root and root type;
- removal of `Cash_tbCode.CategoryCodeNavigation`;
- removal of `Cash_tbCategoryExp.CategoryCodeNavigation` and cascade semantics;
- removal of `App_tbOption.NetProfitCode`, `VatCategoryCode` and navigation;
- keyless/read-only entities for the active views where useful; and
- explicit `VersionId` on every Category Tree handler query and command.

The Category Tree page currently performs many direct EF inserts, updates, bulk updates and deletes. Schema deployment must not be considered complete until those writes use the guarded command boundary. Ordinary non-tree services reading `Cash_tbCodes` can continue to use the persistent table if they do not need category data; any category-aware query must join the active mapping view.

### 5.8 TCExports and historical Cash Statements

`SqlServerCashFlowRepository.Categories.cs` currently reads active functions/views and directly probes `Cash.tbCategory`. It must accept the resolved eligible `VersionId` for category rows, totals, mappings, token resolution and root traversal. Current BI expressions continue to come from `tbCategoryExp` but are parsed against the selected version.

Other Cash Statement datasets that depend transitively on active classification must gain a version-aware SQL path. The implementation should centralise this in parameterised SQL functions or repository queries rather than temporarily changing the active database version. Historical generation must never update `tbCategoryVersion` or alter the active views.

## 6. Mutation boundary and permissions

### 6.1 Authoritative commands

SQL procedures or an equivalent tightly bounded service transaction must own:

- commissioning the first Published version;
- cloning the active version to Draft;
- editing version description and notes;
- category, edge, mapping and root mutations;
- validation and canonical digest refresh;
- Draft refresh/rebase;
- publishing or discarding a Draft;
- acquiring/releasing the later submission lock; and
- Published-to-Submitted and active-to-Archived transitions.

Every command resolves the application actor, locks the singleton options row when active state is involved, verifies status and concurrency, writes the version event and refreshes the digest in one transaction where applicable.

### 6.2 Defence in depth

- Application principals receive execute permission on commands, not unrestricted DML on version-owned tables.
- Table triggers reject semantic writes to Submitted or Archived versions and reject writes to a locked Published version.
- Direct DML tests prove that disabling JavaScript or bypassing handlers does not bypass status guards.
- `SESSION_CONTEXT` may identify an authorised internal command, but it must not be the sole status check and must be set/cleared inside the command boundary.
- Canonical hashes are recomputed from stored rows; callers cannot supply a trusted digest.

## 7. SSDT implementation and cutover sequence

### Step 0 — Freeze the executable contract

- Keep the 585-byte canonical vector and SHA-256 `8B77B246D3DFDB77230B52695747D5D31A135FC5FFB65CD89CE4844A6D032177` passing.
- Capture current SQL build/test baselines from the four writable local sandboxes.
- Add failing SQL tests for the target keys, one-active invariant and immutable statuses before changing production objects.

### Step 1 — Add independent vocabulary and version infrastructure

- Add status/root-type tables, version sequence/allocator, version header and event table.
- Add `App.tbOptions.CategoryVersionId`, its foreign key, the initialisation rule and the serialised singleton trigger assertion.
- Add their SSDT project entries.
- Seed system rows in `proc_NodeDataInit`.
- Add effective-interval, pointer-status and singleton tests.

These objects do not yet change current calculations.

### Step 2 — Convert the category graph to version-owned keys

- Add `VersionId` to `tbCategory` and `tbCategoryTotal` definitions.
- Replace primary, unique and foreign keys with version-scoped equivalents.
- Rewrite audit and cycle triggers to use composite keys.
- Add `tbCategoryCodeMap`, root tables and their constraints.
- Add active views and version-aware traversal functions.

Because there are no customer databases, validation occurs on freshly regenerated databases. Do not build a general in-place conversion framework. Any developer-only transitional deployment script must use an exact database allow-list and is discarded after regeneration is proven.

### Step 3 — Cut over dependent SQL reads

- Migrate high-risk statement/tax objects first.
- Migrate posting triggers and operational procedures.
- Migrate lookup, project, object and invoice objects.
- Update `tbTaxTagMap` validation to active/Draft-aware rules.
- Rebuild SSDT after each dependency group to expose missed column and key assumptions.

Legacy columns remain during this step only as temporary compatibility data. No new code may write them.

### Step 4 — Cut over writers and commissioning

- Update templates and synthetic generators to insert version-owned rows and mappings.
- Add commissioning, clone, validate, mutation and publication commands.
- Route TCWeb Category Tree handlers through the command boundary.
- Update root and Cash Code move operations.
- Add version context to retained JavaScript requests while keeping enforcement server-side.

### Step 5 — Remove competing authorities

Only after a repository-wide reference scan and tests are clean:

- remove `Cash.tbCode.CategoryCode`, its FK and index;
- remove `App.tbOptions.NetProfitCode`, `VatCategoryCode`, their FK and trigger logic;
- remove the category FK/cascade from `Cash.tbCategoryExp`;
- remove temporary compatibility writes or transition objects; and
- fail the build if the retired column names reappear outside historical documentation/tests.

### Step 6 — Regenerate and verify local databases

- Increment the tcNodeDb4 build marker from its current repository value `4.1.17`.
- Rebuild each supported company/sole-trader MIN/STD template database from scratch.
- Assert one Published version, two roots, complete active mappings and canonical digest equivalence.
- Run accounting, tax, Equity Bridge and Cash Statement baselines against the active views.
- Keep Azure and hosted tenants out of this increment.

### Step 7 — Add historical Cash Statement SQL surface

- Add the version-aware TCExports repository path and request parameter.
- Reject Draft identifiers and unknown identifiers.
- Prove an explicit historical selection does not change `App.tbOptions.CategoryVersionId` or any database row.
- Prove unmapped newer Cash Codes and incompatible current expressions produce explicit warnings/errors.

## 8. Verification matrix

### 8.1 Schema tests

- Composite keys prevent cross-version category, edge, mapping and root references.
- A second `App.tbOptions` row is rejected, including a concurrent one-row insert; a multi-row insert is also rejected.
- An initialised options row cannot have a null pointer or reference Draft/Archived.
- Commissioning cannot finish without assigning the first Published version to `CategoryVersionId`.
- Effective intervals are valid and successor publication has no gap or overlap at the transition instant.
- Version code allocation is unique, bounded and non-reusing.
- Every required root exists exactly once and satisfies graph rules.
- Every Cash Code existing at publication, enabled or disabled, has exactly one mapping.
- Submitted and Archived semantic rows reject insert, update and delete.
- Expression rows remain editable without mutating the Category Tree hash.

### 8.2 Dependency tests

- A repository scan finds no calculation read of unfiltered `Cash.tbCategory` or `tbCategoryTotal`.
- No current object reads `Cash.tbCode.CategoryCode` or the two retired option roots.
- Every active wrapper returns the same values as its explicit-version implementation for the active `VersionId`.
- Payment and period triggers preserve their prior decisions through active mapping joins.
- Tax Tag validation resolves the active/Draft category graph correctly.
- Direct and transitive statement/tax view baselines remain stable when the active tree is unchanged.

### 8.3 Lifecycle and accounting tests

- Draft cloning is byte-equivalent and does not change ordinary statements.
- Moving a Cash Code changes only the selected editable version's mapping.
- Publishing atomically closes the predecessor interval and opens the successor interval.
- A stale `BaseContentSha256` prevents publication.
- Disabled Cash Codes cannot be selected for new postings but historical transactions remain classified by the selected version's mapping.
- Equity Bridge and statutory readiness expose classification discontinuities.
- Current Cash Statements default to active; eligible historical versions reproduce their mappings and roots.

### 8.4 Failure and recovery tests

- Failure during clone/publication rolls back rows, status, interval and event changes.
- Concurrent publication cannot produce two successful pointer swaps or lose the selected predecessor.
- Restart/connection loss cannot leave a partially published graph.
- Canonical recomputation detects tampering with any category, edge, mapping or root.
- Restoring a database preserves version/event relationships and the singleton active pointer.

## 9. Completion gate for Increment 2

Increment 2 schema work is complete only when:

- the SSDT project builds with the new objects and no unresolved references;
- all four supported templates commission a valid first Published version;
- the legacy category/root authorities are removed;
- all directly scanned SQL dependencies, including the current minimum set of 88 files, are either migrated or proven unaffected, and transitive accounting/statutory tests pass;
- EF models match the deployed schema;
- canonical SQL and C# vectors agree;
- direct DML cannot mutate Submitted or Archived content;
- active calculations have no version selector and use the active views;
- the historical Cash Statement SQL path uses an explicit eligible `VersionId` without changing active state; and
- all work remains local and uncommitted for review.

## 10. Known implementation details, not owner decisions

The remaining choices are bounded engineering details to settle in the Increment 2 branch of work:

- exact version-code prefix and sequence name;
- exact index and constraint names;
- whether event types use a lookup table or a constrained code;
- the minimum set of version-aware Cash Statement functions versus repository queries;
- stored-procedure names and result contracts; and
- the precise split between SQL triggers and permission grants for defence in depth.

None changes the accepted ownership boundary: category graph, Cash Code mapping and roots are versioned; Cash Code identity and BI expressions are not.
