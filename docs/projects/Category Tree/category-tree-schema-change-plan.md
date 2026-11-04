# Category Tree schema change plan

**Status:** Increment 2 complete locally; Steps 0-6 proven and Increment 3 is next
**Governing documents:** `category-tree-change-control-workplan.md` and `category-tree-increment-1-inventory-and-contract.md`
**UI partner:** `category-tree-ui-change-plan.md`
**Scope:** local tcNodeDb4 SQL foundation, bootstrap, templates, synthetic generation and sandbox migration, plus the bounded TCWeb EF alignment required to run ordinary client surfaces after authority-column removal. Category Tree server/JavaScript integration and TCExports changes are deferred to later increments.

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
| `RowVer` | `rowversion` | SQL-managed compatibility field for ODBC/Office consumers; excluded from EF and the web command contract. |

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

Add nullable `CategoryVersionId nvarchar(10)` with a foreign key to `Cash.tbCategoryVersion`. It replaces `NetProfitCode` and `VatCategoryCode` as the SQL authority. Step 5 removes both legacy root columns after the pre/post synthetic differential proves that the active root functions and views return the same Cash Codes and calculation outputs.

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

`CashPolarityCode` and `CashTypeCode` remain `NOT NULL` for every category type. Nominal categories depend on both classifications, totals retain them, and expressions use the established Neutral/Trade values rather than weakening the table invariant with nulls. The EF model and `Cash.proc_CategorySave` must expose both values as required.

- `(VersionId, IsEnabled, Category)`;
- `(VersionId, IsEnabled, CategoryCode)`; and
- `(VersionId, CategoryTypeCode, DisplayOrder, Category)`.

Audit fields and SQL-managed `RowVer` remain row-local. `RowVer` is not mapped through EF. The update trigger must join on both key columns and invoke the version mutation guard before accepting an insert, update or delete.

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
| `RowVer` | `rowversion` | SQL-managed compatibility field; excluded from EF and web concurrency handling. |

Required keys and indexes:

- primary key `(VersionId, CashCode)`, giving at most one assignment per Cash Code per version;
- foreign key `CashCode -> Cash.tbCode.CashCode` without cascade delete;
- composite foreign key `(VersionId, CategoryCode) -> Cash.tbCategory`;
- index `(VersionId, CategoryCode, CashCode)` for expansion; and
- validation that the mapped category has `CategoryTypeCode = 0` (Nominal).

During the Step 2 shadow phase only, the `CashCode -> tbCode` foreign key is deferred so existing physical Cash Code deletion retains its prior behaviour. A compatibility trigger creates, moves and removes the shadow mapping in the same transaction. Step 4 replaces that trigger with the authoritative Cash Code commands and adds the non-cascading foreign key before the mapping becomes operational authority.

The three category types have distinct structural rules:

- **Nominal (`0`)** is the only valid Cash Code mapping target. It is a leaf and cannot be changed to Total or Expression or deleted in a version while that version has a mapping to it. Disabling a Nominal category or Cash Code does not erase the mapping needed for historical classification.
- **Total (`1`)** may own graph children and may be a configured root, but cannot receive a Cash Code mapping. Its removal is governed by same-version edges and roots.
- **Expression (`2`)** cannot receive a Cash Code mapping or own graph children. Its formula row remains governed separately by the global `tbCategoryExp` contract.

The same-version mapping foreign key uses `NO ACTION`, never cascade delete. It therefore provides the final database backstop against deleting a mapped Nominal category. Mapping triggers reject a non-Nominal target, while category triggers reject changing the type of a mapped Nominal row; these checks apply to direct DML as well as commands. Guarded category-delete and type-change commands must check the complete same-version descendant set first and return the blocking Cash Codes; they must not delete mappings implicitly. Moving those Cash Codes to other valid Nominal categories is a separate explicit mutation.

`Cash.tbCode.CategoryCode`, its category foreign key and its category-based index are removed after every SQL dependency reads the mapping table or active mapping view and the pre/post synthetic differential proves equivalent active classification.

Cash Code creation inserts the global `tbCode` row and its active Nominal mapping atomically through an authoritative database command. Application principals receive execute permission on that command rather than direct insert permission on `tbCode` or unrestricted mapping DML. The command locks the options-selected version, requires editable Published state, validates the Nominal target, inserts both rows in one transaction and asserts exactly one active mapping before commit. Existing Drafts are not silently changed; validation requires a mapping for every Cash Code that exists when the Draft is published, including disabled codes. Archived versions remain immutable and legitimately lack codes created later. A historical Cash Statement encountering transactions for a code absent from the selected version must report the unmapped code rather than silently applying the active mapping.

Physical Cash Code deletion is also an authoritative transaction, not a mapping cascade. It is permitted only when the code has no transactional references and no mapping in a Submitted or Archived version. The command removes mappings from every affected editable version and then the global `tbCode` row in the same transaction; otherwise it rejects deletion and normal retirement uses `IsEnabled = 0`. Deleting a mapping alone from the active version is rejected because it would leave a persistent Cash Code unclassified. A mapping move is an update from one valid Nominal category to another and cannot expose an uncommitted gap.

`Cash.tbCode.IsEnabled` controls operational availability for new work. It must not cause historical transactions to disappear from accounting statements. Statement-classification queries therefore use the version mapping even when the persistent Cash Code has since been disabled; posting and selection lookups may continue to filter `IsEnabled`.

### 3.9 `Cash.tbCategoryRootType` and `Cash.tbCategoryRoot`

`tbCategoryRootType` contains at least:

| `RootTypeCode` | `RootType` |
|---|---|
| `0` | `NET_PROFIT` |
| `1` | `VAT` |

`tbCategoryRoot` has primary key `(VersionId, RootTypeCode)` and a composite foreign key `(VersionId, CategoryCode) -> tbCategory`. Publication requires exactly one valid row for each required root type. The root category must be enabled, have no parent and have at least one child in the same version.

After all SQL consumers are migrated and the differential tests pass, remove `App.tbOptions.NetProfitCode`, `VatCategoryCode`, the Net Profit foreign key and the legacy root-validation section of `App_tbOptions_TriggerUpdate`. The trigger continues to enforce the singleton row and validate `CategoryVersionId` changes.

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

TCWeb also reads/writes the fields through `App_tbOption`, `NodeContext`, `CategoryTreeModel.PrimaryRoot` and the Category Tree details models. Removing the legacy columns is expected to leave the Category Tree interface unavailable until its Increment 3/4 server and JavaScript work. The upgraded-database application smoke test must prove that this exception does not affect other client surfaces.

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

### 5.7 Bounded EF compatibility alignment and deferred TCWeb dependencies

The complete Category Tree EF model still assumes single-column category keys. Those version-aware interface changes remain Increment 3 work:

- composite keys for `Cash_tbCategory` and `Cash_tbCategoryTotal`;
- new entities for version status, version, event, mapping, root and root type;
- removal of `Cash_tbCode.CategoryCodeNavigation`;
- removal of `Cash_tbCategoryExp.CategoryCodeNavigation` and cascade semantics;
- removal of `App_tbOption.NetProfitCode`, `VatCategoryCode` and navigation;
- keyless/read-only entities for the active views where useful; and
- explicit `VersionId` on every Category Tree handler query and command.

Step 5 includes only the compatibility alignment needed by ordinary client surfaces: `App_tbOption` maps `CategoryVersionId` but not the retired root columns; `Cash_tbCode` no longer maps `CategoryCode`; and the obsolete category relationships are removed from the entities and `NodeContext`. Temporary `[NotMapped]` properties retain compile-time compatibility for the deliberately deferred Category Tree handlers without querying missing database columns. The Category Tree page currently performs many direct EF inserts, updates, bulk updates and deletes, so it remains the deliberate client exception after Increment 2 and moves to the guarded command boundary in Increment 3/4. The pre/post SQL differential and upgraded-database application smoke test determine whether any other EF dependency has been missed; schema compatibility is not assumed.

### 5.8 TCExports and historical Cash Statements

**Progress: complete for the Step 5.2 data boundary.** `SqlServerCashFlowRepository.Categories.cs` accepts the resolved eligible `VersionId` for category rows, totals, mappings, token resolution and root traversal. Current BI expressions continue to come from `tbCategoryExp` but are parsed against the selected version.

Category Tree selection, comparison and coverage SQL is centralised in named functions included in the SSDT project and the single migration. The C# repository is limited to small parameterised calls to those functions or existing views, result mapping, and stored-procedure calls for genuinely procedural operations; it does not embed joins, CTEs, unions or Category Tree business logic. Optional operational calculations are reused only after a database function proves selected-versus-active equivalence for every tree projection they consume. A mismatch fails before an active calculation is read. Historical generation never updates `tbCategoryVersion`, changes `App.tbOptions` or alters the active views. This boundary keeps the planned PostgreSQL port explicit without adding a stored-procedure wrapper around every simple read.

## 6. Mutation boundary and permissions

### 6.1 Authoritative commands

SQL procedures or an equivalent tightly bounded service transaction must own:

- commissioning the first Published version;
- cloning the active version to Draft;
- editing version description and notes;
- category, edge, mapping and root mutations, including type changes and subtree deletion;
- atomic Cash Code creation with its active Nominal mapping, Cash Code moves and guarded physical deletion;
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

**Progress: complete.** The canonical vector, baseline inventory and target invariants are recorded in the Increment 1 contract. The four supported template baselines and the rollback-only version/key fixtures are executable. Extend these fixtures when a later step adds a new invariant; do not reopen the accepted ownership boundary.

- Keep the 585-byte canonical vector and SHA-256 `8B77B246D3DFDB77230B52695747D5D31A135FC5FFB65CD89CE4844A6D032177` passing.
- Capture current SQL build/test baselines from the four writable local sandboxes.
- Add failing SQL tests for the target keys, one-active invariant and immutable statuses before changing production objects.

### Step 1 — Add independent vocabulary and version infrastructure

**Progress: complete.** Status/root vocabularies, `Cash.seqCategoryVersion`, allocator, version header and append-only event shape are in SSDT, destructive reset and `App.proc_NodeDataInit`. Allocation, interval and canonical-evidence fixtures pass.

- Add status/root-type tables, version sequence/allocator, version header and event table.
- Add their SSDT project entries.
- Seed system rows in `proc_NodeDataInit`.
- Add version-allocation and effective-interval tests.

These objects do not yet change current calculations.

### Step 2 — Shadow-version the current graph without changing behaviour

**Progress: complete.** The commissioned initial version owns copied category, edge, mapping and root rows. Explicit-version functions and equivalence fixtures prove the shadow projection before operational cutover. This step remains represented in the migration as the data-preserving backfill from the three legacy authorities.

- Allocate an initial version, add and backfill `VersionId` on `tbCategory` and `tbCategoryTotal`, but temporarily retain the legacy keys and all existing read paths.
- Add `tbCategoryCodeMap` and `tbCategoryRoot`, populated from `Cash.tbCode.CategoryCode` and the two existing options roots; at this stage they are verified shadow data, not operational authorities.
- Add explicit-version inline table-valued functions for category, traversal, root and Cash Code membership queries.
- Compare every function at the initial `VersionId` with its legacy view/query and require row-for-row equivalence.
- Do not expose Draft creation, change any existing view definition or redirect any writer in this step.

This is the deliberately non-disruptive introduction point: version metadata exists and can be tested, while all application behaviour still follows the original keys, root columns and Cash Code relationship.

### Step 3 — Make the options pointer and root table authoritative

**Progress: complete.** The options pointer, singleton assertion, active-status guard, six active projections and explicit-version tax/primary-parent wrappers are implemented. Commissioning assigns the pointer only after the template succeeds. Physical removal of the superseded root columns is deliberately owned by Step 5 rather than keeping this step open.

- Add `App.tbOptions.CategoryVersionId`, its foreign key, initialisation rule and serialised singleton assertion, then point it to the verified initial Published version.
- Make the explicit-version functions the calculation implementations. Existing active views become compatibility wrappers which obtain `CategoryVersionId` from the singleton options row and pass it to those functions.
- For example, `App.vwTaxVatCashCodes` joins the singleton options row and uses `CROSS APPLY App.fnTaxVatCashCodes(options.CategoryVersionId)`; callers remain unchanged, while a historical or Draft validator can call the function with an explicit version.
- Convert `App.vwTaxBizCashCodes`, `App.vwTaxVatCashCodes`, `Cash.vwCategoryPrimaryParent` and the other high-risk statement/tax consumers first, proving equivalence after each group.
- Resolve Net Profit and VAT roots only through `Cash.tbCategoryRoot` for the selected version. Retire the two compatibility columns in Step 5 after its dependency scan is clean.
- Rebuild SSDT after each dependency group to expose missed root, column and key assumptions.

The wrapper views are the ordinary operational projection. They contain no category logic beyond resolving the active pointer; reusable and historical logic belongs in the parameterised functions.

**Evidence:** the clean-database acceptance run commissioned `COMIN26` as `CV00000001`, matched every active wrapper to its explicit-version result, and rejected a second options row, a Draft pointer, a null pointer on an initialised node and archiving the selected version. The SQL infrastructure and shadow fixtures passed, as did the 20 Cash Statement service assertions and 349 TCWeb/Tax Hub boundary assertions.

### Step 4 — Complete version-owned keys, writers and commissioning

**Progress: complete.** Composite category and edge keys, version-scoped indexes, template commissioning, synthetic mapping creation, canonicalisation, active/explicit-version readers, lifecycle and authoritative mutation procedures, and direct-DML guards are implemented. Existing client contracts remain compatible outside the deliberately deferred Category Tree JavaScript workspace; no TCWeb or TCExports change belongs to this step.

- Replace category primary, unique and foreign keys with version-scoped equivalents and rewrite audit/cycle triggers for composite keys.
- Migrate remaining posting, lookup, project, object, invoice and tax-validation dependencies to active wrappers or explicit-version functions.
- Update templates and synthetic generators to insert version-owned rows and mappings.
- Add commissioning, clone, validate, mutation and publication commands.
- Add root and Cash Code move procedures plus submission/archive transitions at the SQL command boundary.
- Preserve existing view and procedure signatures needed by every current client surface outside the Category Tree JavaScript workspace.
- Add database permissions and direct-DML guards so later server/UI work consumes a complete change-control schema rather than defining its rules.

**Implemented:** commissioning allocates the version before invoking a template and passes `CategoryVersionId` explicitly through all four supported template chains. `proc_Template_BASE_MIN_2026` and each company/sole-trader refinement explicitly write version-owned categories, totals, Cash Code mappings and roots. Step 5 removed the former legacy root and `tbCode.CategoryCode` compatibility outputs. Synthetic Cash Code creation uses one guarded transaction to add the persistent code and its active-version Nominal mapping.

The SQL canonicaliser reproduces the 585-byte v1 conformance vector and SHA-256 `8B77B246D3DFDB77230B52695747D5D31A135FC5FFB65CD89CE4844A6D032177`. Commissioning stores the initial digest before publication; synthetic generation refreshes it after adding its operational codes and records a digest-bound validation event. Semantic writes invalidate the digest, while category, total, mapping and root triggers reject Submitted and Archived mutations.

The blank-database matrix commissioned `COMIN26`, `COSTD26`, `STMIN26` and `STSTD26` with one selected Published `CV00000001`, two roots, complete Cash Code mapping and non-null canonical evidence. The full three-completed-year `COSTD26` run finished with 53 categories, 18 edges and 38/38 Cash Code mappings; its Equity Bridge variances were `0.00`, `0.00` and `0.08` against the `0.10` tolerance. SSDT, canonical/version/active-projection fixtures, the completed-year horizon regression, 20 Cash Statement service assertions and 349 TCWeb/Tax Hub boundary assertions passed. This proves clean rebuild readiness for review; it does not authorise deployment or declare the remaining Step 4/5 dependency cutover complete.

Category and edge primary keys are now `(VersionId, CategoryCode)` and `(VersionId, ParentCode, ChildCode)`, with all supporting uniqueness and traversal indexes version-scoped. The obsolete single-column category foreign keys from `Cash.tbCode`, `Cash.tbCategoryExp` and `App.tbOptions`, and the duplicate single-column edge foreign keys, have been removed. A rollback-only fixture proves identical category codes and edges can exist in predecessor and Draft versions while cross-version edges fail.

`Cash.fnCategoryNamespaceByVersion`, `fnCategoryNamespaceInContextByVersion` and `fnCategoryNamespacePrimaryByVersion` own namespace traversal; the legacy signatures are active wrappers. Calculation-sensitive Cash, App, Project, Object and Invoice readers now resolve classification through `vwCodeActive`, `vwCategoryActive`, `vwCategoryTotalActive` or explicit-version functions. Tax Tag validation has an explicit-version implementation with an active wrapper.

The SQL command boundary now covers clone, refresh/rebase, description/notes, category, edge, mapping, root, atomic Cash Code creation, guarded Cash Code deletion, validate, publish/archive, submit and Draft discard. Each semantic command resolves an application actor and couples its content change and lifecycle event in one transaction; the mapping-to-Cash-Code foreign key is non-cascading. Submitted and Archived rows remain protected by table triggers even if a privileged caller bypasses the procedures. tcNodeDb4 has no source-controlled application principal or role and does not otherwise install object permissions through post-deployment, so Category Tree does not introduce a special `public` `DENY`/`GRANT` scheme. The application command service supplies the ordinary mutation boundary; any instance-specific role hardening belongs to database administration rather than the portable schema.

The source-controlled differential uses bidirectional `EXCEPT` comparisons for category content, edges, Cash Code classification, roots, business/VAT membership, Cash Statement and What-If, budget, profit/loss, tax accrual and Equity reconciliation. The exact `tc-business1` build `4.1.18` BACPAC was imported twice locally as `tcNodeDb4-B1-Baseline` and `tcNodeDb4-B1-Candidate`; the baseline was made read-only and the single migration was applied only to the candidate. The differential passed with 53 categories, 18 edges, 38 mappings and 84 Cash Statement rows. The rollback-only lifecycle fixture passed clone/edit isolation, refresh, publication and predecessor archive, atomic Cash Code create/delete, submission immutability and protected-history deletion. The final production comparison reported no Category Tree schema or programmable-object operations. Its only root differences are deliberately excluded pre-existing/non-product differences: the Azure-managed `Platform.DatabaseIdentity` object, the physical ordinal position of the existing `App.tbYearPeriod` VAT adjustment-note columns, and the VAT polarity check whose `IN` expression SQL Server persists as reordered `OR` predicates. Their dependent `sp_refreshsqlmodule` statements are comparison-plan consequences rather than additional schema differences. The intentional VAT work remains unamended. Build `4.1.19` is the current target.

### Step 5 — Remove competing authorities

**Progress: complete locally.** `App.tbOptions.NetProfitCode`, `VatCategoryCode`, `Cash.tbCode.CategoryCode`, the category-based Cash Code index, the shadow mapping trigger and their compatibility writers are removed from the SSDT model and the single migration. The SQL project builds. Fresh build `4.1.13` to `4.1.19` rehearsal and the company, sole-trader MIN and sole-trader STD differentials pass after physical removal, with 73, 90 and 77 Cash Statement rows respectively. The strengthened lifecycle fixture also proves the retired columns, index, trigger and Access-client menu subsystem are absent on all three migrated databases.

Executable inspection disproved the original unchanged-client assumption: ordinary non-Category-Tree code materialises entities that mapped the retired columns. The owner approved bounded EF alignment in Increment 2. `App_tbOption`, `Cash_tbCode`, `Cash_tbCategory` and `NodeContext` now omit those database mappings and relationships while retaining non-mapped compatibility properties for the deferred Category Tree compilation boundary. TCWeb builds with zero warnings/errors; the Cash Statement service suite passes 20 assertions and the TCWeb boundary suite passes 351 assertions, including explicit EF metadata assertions for `CategoryVersionId` and the retired mappings.

The first reviewed blank publish exposed an unnecessary Category Tree-specific `public` `DENY`/`GRANT` block in post-deployment, including a stale permission on the removed `Cash.tbCode.CategoryCode` column. tcNodeDb4 does not manage comparable object permissions elsewhere, so the whole special permission scheme has been removed from clean publish, migration and its lifecycle assertions. Lifecycle immutability remains enforced by the table triggers and authoritative procedures. The rebuilt DACPAC contains no post-deployment `DENY` or `GRANT` statements; blank publish must be retried with a newly generated publish script rather than the failed `tcNodeDb4_8.publish.sql` artefact.

Only after a repository-wide reference scan and tests are clean:

- prove no SQL calculation or authoritative command reads `Cash.tbCode.CategoryCode` or the two option roots;
- remove `Cash.tbCode.CategoryCode`, its obsolete index and remaining compatibility writes;
- remove `App.tbOptions.NetProfitCode`, `VatCategoryCode` and their legacy trigger logic;
- keep the removed category foreign keys and expression cascade absent so version-owned keys remain authoritative;
- fail the build if the retired column names reappear in current SQL outside the migration and explicit regression checks; and
- require the pre/post differential to pass before accepting the removal.

### Step 6 — Regenerate and verify local databases

**Progress: complete locally.** The target build is `4.1.19`. Recovery BACPACs exist for all four IAM-01 synthetic businesses, the guarded script passed the exact `4.1.18` differential and all three local `4.1.13` shapes, and the bounded EF compatibility gate passes. The clean `tcNodeDb4-COSIPFVT1-COSTD26` publish, commissioning and synthetic generation succeed; owner smoke testing confirmed every ordinary client surface, with only the deliberately deferred Category Tree interface broken. Local validation exposed and corrected the missing trust of ten Category Tree foreign keys, loss of the SQLCMD UTF-8 BOM, and retention of the old `Usr.tbMenu*` subsystem by the `4.1.13` prerequisite. Both sole-trader databases were restored and replayed; company MIN completed the final fresh rehearsal. Exact category, edge, mapping, root, tax, Cash Statement, budget, P&L and Equity Bridge checksums were unchanged; binary-collation scans found zero mojibake; DBCC reported no violations; every Category Tree foreign key is trusted; no legacy menu object remains; all three strengthened lifecycle fixtures pass; and the production comparison contains zero Category Tree operations. The B1–B7 build `4.1.18` migration does not encounter the already-retired menu objects. Its remaining `App.tbYearPeriod` rebuild, normalised VAT check and dependent module operations are the documented comparison exclusions.

- Use tcNodeDb4 build marker `4.1.19` for this Category Tree schema boundary.
- Create one complete pre-update synthetic database and clone it before changing either schema or data. Retain the original as the baseline and apply the single migration script only to the clone.
- Run a source-controlled differential SQL suite against both databases. Compare result counts and `EXCEPT` in both directions for active Cash Code/root membership, Cash Statement and What-If, budget, profit/loss, tax accrual and Equity Bridge inputs/outputs; exclude only documented volatile metadata.
- Treat any difference as a migration failure unless it is an explicitly approved semantic change. In particular, removing `NetProfitCode`, `VatCategoryCode` and `Cash.tbCode.CategoryCode` must leave the active calculation rowsets unchanged.
- Point the application with its bounded EF compatibility alignment at the migrated clone and smoke-test every client surface. The Category Tree interface is the sole expected failure at this boundary; any other failure returns the work to the dependency inventory.
- Rebuild each supported company/sole-trader MIN/STD template database from scratch.
- Assert one Published version, two roots, complete active mappings and canonical digest equivalence.
- Run accounting, tax, Equity Bridge and Cash Statement baselines against the active views.
- Keep Azure and hosted tenants out of the local schema/regeneration steps; section 11 is limited to the four named local IAM-01 synthetic businesses.

### Deferred application boundary

Historical Cash Statement selection and affected-period warnings belong to Increment 3 with the server boundary and TCExports request contract. The Category Tree banner, commands and retained JavaScript workspace integration belong to Increment 4. Neither is a schema-plan step or a prerequisite for the Increment 2 deployment target.

At the end of Increment 2, deploying the consolidated database build must leave existing client features operational through the active compatibility projection. The known exception is the Category Tree JavaScript interface: it has not yet been adapted to choose a version or invoke the new lifecycle/mutation command boundary and is not an acceptance surface for this increment.

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
- Every active mapping targets a Nominal category; Total and Expression categories reject mappings, while disabled Nominal mappings remain available for historical classification.
- Deleting or changing the type of a mapped Nominal category is rejected without deleting its mapping.
- Direct Cash Code insertion cannot commit without its active mapping, and deleting an active mapping alone is rejected.
- Physical Cash Code deletion is rejected when any transaction or immutable version refers to it; otherwise code and editable mappings are removed atomically.
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
- Current Cash Statements continue to use the active version with unchanged client behaviour. Explicit historical selection is deferred to Increment 3.

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
- the legacy category/root authority columns are physically removed after the pre/post synthetic differential passes;
- all directly scanned SQL dependencies, including the current minimum set of 88 files, are either migrated or proven unaffected, and transitive accounting/statutory tests pass;
- canonical SQL and C# vectors agree;
- direct DML cannot mutate Submitted or Archived content;
- active calculations have no version selector and use the active views;
- clone, validate, publish, submit/archive and guarded Category/Cash Code/root mutations are complete SQL commands;
- the single sandbox migration script elevates build `4.1.18` to the complete target schema and a production comparison reports no Category Tree differences, with only the three documented pre-existing/environment exclusions;
- existing client integration tests pass with only the bounded TCWeb EF compatibility alignment and no TCExports changes, excluding the deferred Category Tree JavaScript workspace; and
- all work remains local and uncommitted for review.

## 10. Known implementation details, not owner decisions

The remaining choices are bounded engineering details to settle in the Increment 2 branch of work:

- exact version-code prefix and sequence name;
- exact index and constraint names;
- whether event types use a lookup table or a constrained code;
- stored-procedure names and result contracts; and
- the precise split between SQL triggers and permission grants for defence in depth.

None changes the accepted ownership boundary: category graph, Cash Code mapping and roots are versioned; Cash Code identity and BI expressions are not.

## 11. IAM-01 synthetic-business migration

After the local schema, templates and executable contracts pass review, rebuild `tcNodeDb4-COSIPFVT1-COSTD26` from scratch and add one source-controlled migration script for the other three accounting sandbox databases in the local IAM-01 estate. This is bounded development-data adoption, not customer migration machinery and not a hosted-tenant deployment process.

### 11.1 Script and execution boundary

- Keep the migration in the tcNodeDb4 SQL source tree and include it in the SSDT project as a non-build deployment script.
- Maintain it at `Scripts/DB4 cat change control migration script.sql`; until every conversion, recovery and acceptance section is present it must fail in its first batch and cannot be used against any database.
- The one script must elevate an allowed build `4.1.18` database to the complete target build, including every changed table, constraint, function, view, procedure and trigger plus data conversion, canonical publication and build evidence. There is no follow-up schema-difference deployment step.
- Make the script target one explicitly supplied database per execution. Its exact allow-list is `tcNodeDb4-COMIPFVT1-COMIN26`, `tcNodeDb4-STMIPFVT1-STMIN26` and `tcNodeDb4-STSIPFVT1-STSTD26`; do not embed credentials, server names or a cross-database loop.
- Require an expected pre-migration build/version and refuse an unknown, partially migrated or already newer schema.
- Make a completed migration safely detectable and non-repeatable. A rerun may report success only after verifying the installed objects and data; it must not allocate another initial version.
- Emit a compact evidence result containing database identity, previous/new build, allocated `VersionId`, row counts, roots, mapping coverage and canonical digest.

### 11.2 Preflight and write access

Before changing a sandbox database:

1. capture its build, options row, category/category-total/Cash Code counts, legacy roots and existing validation results;
2. take or identify the agreed recoverable backup/export point;
3. remove the previous development read-only lock using the same supported mechanism by which it was applied and prove a rollback transaction can write; and
4. abort without schema changes if the database is not one of the three allow-listed migration targets, has more or fewer than one options row, contains invalid roots, orphaned graph rows, unmapped Cash Codes or an unexpected build.

The runner records the lock state and restores the intended sandbox access policy after verification. The migration contains no Azure credentials or environment-wide permission changes.

### 11.3 Transactional conversion

**Supporting artefact after completed Step 5; still guarded in source.** The migration artefact contains the allow-listed build `4.1.13` preflight and ordinary `4.1.13`-to-`4.1.18` prerequisite, including retirement of the old `Usr.tbMenu*` subsystem, version vocabularies/header/event ledger, sequence, explicit in-place conversion of both Category Tree primary keys, version-scoped indexes and composite foreign keys, mapping/root/pointer backfill, every amended current-build programmable object, canonicalisation/publication, options selection, physical removal of the three retired authorities, build evidence and constraint validation. It must remain UTF-8 with a BOM so default `sqlcmd` preserves Unicode template text; the executable boundary suite now fails if the three BOM bytes are absent. The exact `tc-business1` BACPAC was restored twice locally; the read-only baseline remained untouched and a pristine candidate completed the single script to `4.1.19`. The accounting differential and lifecycle fixture passed. All three backed-up IAM-01 build `4.1.13` businesses completed the corrected artefact under explicit owner authorisation; both sole traders were restored/replayed after the encoding correction and company MIN provided the final fresh end-to-end rehearsal. The source guard was enabled only in temporary BOM-preserving execution copies and remains locked in source. Recovery BACPACs exist for all four local IAM-01 synthetic businesses.

The generated build `4.1.18`-to-current DACPAC comparison is a dependency inventory, not the migration: it proposes automatic rebuilds without the required Category Tree commissioning data and also reports the three documented non-Category-Tree differences. The reviewed migration pre-aligns the two version-owned keys explicitly, commissions the first version and includes all amended objects itself. After the complete script has run on a disposable source-build database, run the production schema comparison only as a validator. It must report no remaining Category Tree target-schema differences, table rebuilds or possible-data-loss operations; never execute that difference to finish an incomplete migration.

For each database, perform the following as one controlled migration transaction wherever SQL Server DDL permits:

1. add the status/root vocabularies, version header/event infrastructure and the nullable options pointer;
2. allocate one initial version and copy the current category graph into the new version-owned key shape;
3. create `Cash.tbCategoryCodeMap` from every persistent `Cash.tbCode.CategoryCode` relationship, failing on null, orphaned or duplicate coverage;
4. create the Net Profit and VAT root rows from the two legacy `App.tbOptions` root values;
5. validate graph integrity, complete Cash Code coverage, roots, tax-tag compatibility and the canonical digest;
6. mark the initial version Published, set its effective start and point the singleton options row to it;
7. migrate all dependent SQL objects to active views or explicit-version implementations;
8. remove the obsolete single-column foreign keys and legacy category/root authority columns only after dependency and pre/post differential checks succeed; and
9. update the database build marker and commit.

Any error rolls back the database to its pre-migration schema and data. If a DDL operation cannot participate in the transaction, the script must stop before that boundary or provide a separately tested recovery step; it must never continue from an unrecognised partial state.

### 11.4 Rebuild proof and per-database acceptance

First wipe and recreate `tcNodeDb4-COSIPFVT1-COSTD26` through the upgraded bootstrap, Standard Company template and synthetic dataset generator. Do not run the migration script against it. This proves that a newly commissioned versioned database and the full synthetic accounting surface can be produced without transitional compatibility data. Preserve the required scenario parameters in the evidence rather than relying silently on future script defaults.

Then run the migration script against the three allow-listed databases. The sandbox adoption is accepted only when the rebuilt database and every migrated database independently prove:

- exactly one options row points to exactly one Published initial version;
- active categories, edges, Cash Code mappings and roots are semantically equivalent to the preflight snapshot;
- the initial canonical digest recomputes identically in SQL and C#;
- accounting, Cash Statement, Equity Bridge and applicable tax baselines are unchanged;
- no current target-schema object refers to `Cash.tbCode.CategoryCode`, `App.tbOptions.NetProfitCode` or `VatCategoryCode`; their only remaining references are the migration source reads and explicit regression assertions;
- clone/edit/publish and immutable Submitted/Archived smoke tests pass; and
- the final access state and evidence output are recorded without secrets.

Review the recreated Standard Company evidence before migrating anything. Use `tcNodeDb4-COMIPFVT1-COMIN26` as the migration canary, review it, then migrate the sole-trader MIN and STD databases one at a time. Stop on the first failure, preserve the pre-migration recovery point and do not treat success on one sandbox as permission to modify any database outside the named set.
