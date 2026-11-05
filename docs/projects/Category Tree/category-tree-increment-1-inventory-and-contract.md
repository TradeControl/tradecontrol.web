# Category Tree Increment 1 inventory and executable contract

**Date:** 2 November 2026
**Scope:** local repository evidence only
**Outcome:** inventory and proposed executable contract complete; core schema implementation may proceed independently of later statutory-seal integration

## 1. Increment decision

The agreed Category Tree architecture is internally consistent. The existing category tables become version-owned, explicit active views provide the ordinary calculation projection, and Cash Code identity and BI expressions remain outside the lifecycle. Category Tree change control supplies an independently useful evolution boundary; the unfinished statutory-seal work is a later consumer and does not block schema implementation.

The current VAT implementation provides useful proving components: deterministic canonical VAT bytes, SHA-256 verification, exact prepared payload retention, re-preparation checks before approval and dispatch, and an attempt store that prevents replay of an active or successful authority operation. It does not yet provide the Category Tree prerequisite:

- `VatStatutorySeal` is a hashable candidate rather than a signed canonical manifest and has no Category Tree version code, profile, digest, byte length or canonical-object reference;
- `App.proc_VatSourceEvidence` labels `@@DBTS` as `AccountingEventId`, while the statutory-seal contract expressly says that SQL row versions are not a sufficient source boundary;
- the VAT source inventory includes mutable SQL `rowversion` values and is a broad source-state digest, not a canonical Category Tree object;
- no durable submission lock is acquired against the active Category Tree at preparation or approval;
- the submission attempt store prevents authority replay but cannot reject SQL or EF Category Tree writes;
- success does not transactionally change a Published Category Tree version to Submitted; and
- the Stage 1 contract still lists canonicalisation, preparation-lock semantics, signatures, the tax-balance schema and standalone verification as open implementation decisions.

Consequently this increment records an executable target contract. The next bounded increment can implement the SQL lifecycle without waiting for seal completion.

## 2. Calculation-sensitive inventory

### 2.1 Versioned Category Tree content

| Source | Canonical fields | Reason |
|---|---|---|
| `Cash.tbCategory` | `CategoryCode`, `Category`, `CategoryTypeCode`, nullable `CashPolarityCode`, nullable `CashTypeCode`, `DisplayOrder`, logical `IsEnabled` | Identity, reviewed label, node kind, sign, cash classification, presentation order and participation all affect the reviewed or calculated tree. `Cash.fnCategoryCashCodes`, statement views, tax-tag expansion and namespace selection consume these values. |
| `Cash.tbCategoryTotal` | `ParentCode`, `ChildCode`, `DisplayOrder` | This is the complete directed graph and reviewed sibling order. The existing trigger already rejects self-edges, non-total parents and cycles. |
| `Cash.tbCategoryCodeMap` | `CashCode`, `CategoryCode` within `VersionId` | Cash Code identity remains persistent in `Cash.tbCode`, while the leaf assignment is part of each tree version. Moving a Cash Code changes the selected version's mapping without cloning or deleting the Cash Code. |
| `Cash.tbCategoryRoot` | version-owned `NET_PROFIT` and `VAT` root rows | This is the authoritative replacement for `App.tbOptions.NetProfitCode` and `VatCategoryCode`. The three current consumers, `App.vwTaxBizCashCodes`, `App.vwTaxVatCashCodes` and `Cash.vwCategoryPrimaryParent`, must resolve roots from the active version. |

`Cash.tbCode.CategoryCode` is currently mandatory, but the agreed design moves that authority to `Cash.tbCategoryCodeMap`. `Cash.tbCode` remains the persistent master referenced by transactions. A successor can map the same Cash Code differently without changing its identity or transaction history. The resulting change to historical classification remains visible through `Cash.vwEquityReconciliationByYear`, which is consumed by both Tax Hub and Companies House readiness.

`Cash.tbCategoryExp` is current user-managed BI formula metadata, not Category Tree version content. Expression text, format, syntax and parser diagnostics remain editable independently. The parser reports expressions that no longer resolve against the selected classification; correcting such a formula does not mutate Submitted or Archived tree evidence.

The following technical/audit fields are excluded from semantic content: `InsertedBy`, `InsertedOn`, `UpdatedBy`, `UpdatedOn`, every `RowVer`, lifecycle status, lifecycle actors/timestamps, predecessor, description, notes, version identifier, canonical byte length and the digest itself.

### 2.2 Related protected sources that remain separate

`Cash.tbCode.TaxCode`, `App.tbTaxCode`, `Cash.tbTaxType`, `Cash.tbTaxTagSource`, `Cash.tbTaxTag`, `Cash.tbTaxTagMap`, reporting profiles and authority mappings can change statutory output. They are not Category Tree membership and must retain their own protected-source evidence and hashes. Publication validation must nevertheless rerun `Cash.fnTaxTagMapValidate` for every applicable Tax Source.

`Cash.tbCategoryType`, `Cash.tbPolarity` and `Cash.tbType` are schema/template vocabularies rather than tenant version rows. Their stable numeric values are recorded in versioned category rows. `Cash.tbCategoryExpSyntax` and `Cash.tbCategoryExprFormat` belong to the separate BI-expression system.

No other Category Tree root was found. `SubjectCode`, `TaxHorizon`, jurisdiction, unit of charge, host and coin fields in `App.tbOptions` affect other calculations or identity. The two legacy root fields are replaced by `CategoryVersionId`: the options row selects the active version, whose `Cash.tbCategoryRoot` rows select the Net Profit and VAT roots.

## 3. Current mutation-path inventory

There is no shared Category Tree service or database status guard. Current writes go directly to the active projection.

### 3.1 EF Core JSON handlers on `/Cash/CategoryTree`

| Handler | Current mutation |
|---|---|
| `SetEnabled` | Bulk-updates category, expression-category or Cash Code `IsEnabled`; category changes can cascade to descendant categories. |
| `Move` | Deletes one `Cash.tbCategoryTotal` edge and inserts another. |
| `MoveUp`, `MoveDown` | Bulk-swaps category or edge `DisplayOrder`. |
| `ReorderSiblings` | Rewrites category or edge `DisplayOrder` for root, disconnected or child contexts. |
| `ReorderExpression` | Normalises and rewrites expression-category `DisplayOrder`. |
| `UpgradeTypeOrdering`, `ReorderType` | Normalises or rewrites nominal-category `DisplayOrder` by cash type. |
| `CreateTotal` | Inserts a total category and optionally a parent edge. |
| `CreateCategory` | Inserts a nominal category and optionally a parent edge. |
| `CreateCodeByCategory` | Inserts a `Cash.tbCode` row under a category. |
| `AddExistingCategory` | Inserts a parent-child edge. |
| `AddExistingCode` | Reassigns `Cash.tbCode.CategoryCode`. |
| `DeleteCashCode` | Deletes a Cash Code. |
| `DeleteCategory` | Bulk-deletes affected edges and the category; FK cascades may delete its expression. |
| `DeleteTotal` | Deletes an edge or a total subtree, its membership edges and categories according to the current handler rules. |
| `Unlink` | Deletes one parent-child edge and resequences siblings. |
| `SetPrimaryRoot` | Currently updates `App.tbOptions.NetProfitCode` or `VatCategoryCode`; it must instead update the selected version's `Cash.tbCategoryRoot` row. |
| `ResetExpressionError` | Updates only non-semantic expression diagnostics. |

The navigation-only `View`, `CreateCode` and `Edit` handlers do not mutate data.

### 3.2 Full-page Razor handlers

| Page | Current mutation |
|---|---|
| `AddCategory` | Adds an existing category edge and can remove an old edge during reparenting. |
| `AddCashCode` | Reassigns an existing Cash Code to another category. |
| `CreateCategory` | Inserts a nominal category and optional edge. |
| `CreateTotal` | Inserts a total category and optional edge. |
| `CreateExpression` | Inserts an expression category and `Cash.tbCategoryExp`. |
| `CreateCashCode` | Inserts a Cash Code. |
| `EditCategory`, `EditTotal` | Updates category label, polarity/type/order/enabled fields exposed by the forms. |
| `EditExpression` | Updates the expression category and inserts or updates its expression row. |
| `EditCashCode` | Updates description, category, tax code and enabled state. The Category Tree service must distinguish membership fields from separately protected tax treatment. |
| `Move` | Deletes and inserts graph edges. |
| `DeleteCashCode` | Deletes a Cash Code. |
| `DeleteExpression` | Deletes the expression category and cascades the expression row. |

`DeleteCategory` and `DeleteTotal` pages are confirmation surfaces; they post to the JSON handlers. Expression create/edit/delete pages currently rely only on the global authenticated-user fallback policy, unlike the other full-page mutation models which require the Administrators role. The future shared command boundary must apply both lifecycle and authorisation rules consistently.

### 3.3 JavaScript-triggered paths

The retained Fancytree implementation calls the handlers through several separate client surfaces:

- `categoryTree.js`: context menu, details pane, mobile bar, keyboard reorder and drag/drop call `SetEnabled`, `ResetExpressionError`, `MoveUp`, `MoveDown`, `ReorderExpression`, `ReorderType`, `ReorderSiblings`, `Unlink`, `SetPrimaryRoot`, full-page create/edit/delete forms and the move flow;
- `categoryTree.shared.js`: the details-card enable/disable action posts directly to `SetEnabled` and navigates to full-page create/edit/delete flows;
- `categoryTree.move.js`: posts the full-page move form;
- `categoryTree.delete.js`: posts confirmation forms, including the Category Tree delete handlers;
- `categoryTree.embedded.js`: posts embedded create/edit forms; and
- `categoryTree.cancel.js`: reloads details after cancelling but is read-only.

The browser currently supplies only node and parent keys. Every future request must also carry an explicit version/workspace identifier, while the server independently resolves and verifies the version status.

### 3.4 SQL, template and synthetic paths

Direct writes that must be commissioned before guards become active, or routed through privileged controlled operations afterward, are:

- destructive reset: `App.proc_NodeDataInit` deletes `App.tbOptions`, `Cash.tbCategoryTotal`, `Cash.tbCategoryExp`, `Cash.tbCode` and `Cash.tbCategory`;
- initial/template manufacture: `App.proc_Template_BASE_MIN_2026`, `proc_Template_CO_MICRO_CUR_2026`, `proc_Template_CO_MICRO_CUR_MIN_2026`, `proc_Template_CO_MICRO_CUR_STD_2026`, `proc_Template_CO_MICRO_CUR_STD_EXP_2026`, `proc_Template_ST_SOLE_CUR_MIN_2026`, `proc_Template_ST_SOLE_CUR_STD_2026` and `proc_Template_DisableVAT` insert/update/delete projection rows and roots;
- template wrappers: `proc_Template_ST_SOLE_CUR_MIN_MTD_2026`, `proc_Template_ST_SOLE_CUR_STD_MTD_2026`, `proc_Template_CO_MICRO_CUR_TAX_2026` and `proc_Template_ST_SOLE_CUR_TAX_MTD_2026` compose the preceding templates or statutory mappings;
- commissioning: `App.proc_NodeBusinessInit` inserts `App.tbOptions`; `App.proc_BasicSetup` dispatches the selected `App.tbTemplate.StoredProcedure` and updates options;
- synthetic regeneration: `App.proc_DatasetSyntheticMIS` calls `proc_DatasetSyntheticMIS_Bootstrap`, which executes `proc_NodeDataInit`, `proc_NodeBusinessInit` and `proc_BasicSetup`; `proc_DatasetSyntheticMIS_Assets` and `proc_DatasetSyntheticMIS_Expenses` insert Cash Codes; and `Scripts/EXEC_DatasetSyntheticMIS_Year3_Additive.sql` inserts categories, edges and Cash Codes;
- expression diagnostics: `Cash.proc_CategoryExprStatusSet` changes only `IsError` and `ErrorMessage`;
- audit triggers on `Cash.tbCategory`, `Cash.tbCode` and `App.tbOptions` update audit columns; and
- SQL test fixtures `Tests/TaxTagMapValidator.sql` and `Tests/Phase4D_CumulativeProjection.sql` deliberately mutate category polarity/enabled state or insert a Cash Code and then restore it.

`App.proc_RotateSymmetricKeys`, `proc_Initialised` and unrelated options maintenance update other `App.tbOptions` columns. The options trigger must distinguish ordinary options maintenance from a `CategoryVersionId` change while always enforcing the table's singleton invariant.

No database permissions currently restrict direct DML on the existing category/Cash Code tables or the two option root columns, and no trigger checks version status or submission locks.

## 4. Existing tests and assets requiring change

### 4.1 Category Tree tests

The Playwright suite currently contains five files:

- `category-tree.reconciliation.spec.ts`: index/anchor smoke and create disconnected/child totals;
- `category-tree.cashcode.spec.ts`: create a category and Cash Code;
- `category-tree.delete.spec.ts`: delete disconnected and parented totals/categories;
- `category-tree.duplicate.spec.ts`: duplicate category rejection; and
- `category-tree.disabled.spec.ts`: disabled creation behaviour.

These tests call handlers directly and are therefore valuable guard-bypass fixtures. They need workspace/version parameters and Published, Submitted, Draft, Archived, stale-context and JavaScript-disabled cases. The manual `categorytree_test_script.md` covers current desktop/mobile tree behaviour but has no lifecycle, version banner, Draft, publish, history, concurrency, submission-lock or immutable-state scenarios.

The SQL tests `TaxTagMapValidator.sql` and `Phase4D_CumulativeProjection.sql` must run inside an editable Published version or a controlled fixture mode. New SQL tests must cover commissioning, the singleton options row and active pointer, immutability, clone equivalence, Draft isolation, publication rollback/concurrency, root validity, referenced Cash Codes and digest stability/sensitivity.

The application contract tests already prove a fixed VAT canonical byte vector, replay-sensitive source hashing and one-bit tamper detection. Category Tree vectors should follow that style once the Stage 1 manifest can bind the new object.

### 4.2 Templates, bootstrap and generated artifacts

All template procedures listed in section 3.4 must finish by validating the installed tree and commissioning its first Published version. `proc_NodeDataInit` must seed lifecycle/status and root-type vocabularies before a template runs but must not create an empty active version. `proc_BasicSetup` is the natural commissioning boundary after its selected template succeeds. It stores the template roots in `Cash.tbCategoryRoot`, assigns `App.tbOptions.CategoryVersionId`, and removes the two legacy root columns from the steady-state design.

`proc_DatasetSyntheticMIS_Bootstrap` deliberately destroys and rebuilds the accounting dataset; it must rebuild the first version after `proc_BasicSetup` rather than attempt to migrate discarded version history. `proc_DatasetSyntheticMIS` and `Scripts/EXEC_DatasetSyntheticMIS.sql` must assert one Published active version and canonical byte equivalence. The Year 3 additive script is a post-commissioning direct writer and must be converted to a controlled Published mutation or applied before commissioning.

The SSDT project, EF Core models/`NodeContext`, solution artifacts and SQL build currently at `4.1.17` will require the Increment 2 additions and build increment. No customer migration utility is required.

## 5. Proposed executable contract

This section is the precise target for SQL and application tests. Names remain subject to tcNodeDb4 naming review, but semantics do not.

### 5.1 Identity and table boundary

- `VersionId`: immutable, system-allocated `nvarchar(10)` code, unique within the database. It is not a GUID and is never reused.
- `VersionStatusCode`: `0 Draft`, `1 Published`, `2 Submitted`, `3 Archived`.
- `Description`: required short user-facing version description.
- `Notes`: optional longer change-control explanation; publication still requires a meaningful note when succeeding a Submitted version.
- `PredecessorVersionId`: null only for commissioning; required for later versions and immutable. Every successor is cloned from the active version; there is no empty Draft command.
- `BaseContentSha256`: the active predecessor hash at clone time; publication fails if the predecessor has changed unless the Draft is explicitly refreshed/rebased.
- `CanonicalProfile`: `trade-control-category-tree-v1`.
- `CanonicalSha256`: 32 binary bytes in SQL and 64 uppercase hexadecimal characters at API/evidence boundaries.
- `CanonicalByteLength`: non-negative 64-bit integer.
- category, total, Cash Code mapping and root rows are owned by `VersionId`; their primary keys combine `VersionId` with the stable key(s).
- lifecycle actor identifiers use `Usr.vwCredentials.UserId`; captured `UserName` is retained beside the stable identifier. Technical timestamps are UTC; business timestamp/zone fields follow the accepted Stage 1 convention.

`Cash.tbCategoryVersionEvent` is an append-only audit of create, edit, validate, publish, submit and archive events, including actor, event time, status transition and applicable note. `EffectiveFromOn` and `EffectiveToOn` are inclusive business dates. A Draft may carry a proposed start date but has no end date. Publication is manual and is permitted only on the proposed start date; it automatically closes the predecessor on the preceding date. Submission leaves the active interval open.

Draft rows are mutable only through Draft commands. Published category, total, mapping and root rows are the active data and remain editable through the guarded boundary. Submitted and Archived version-owned rows are immutable. `Cash.tbCode` and `Cash.tbCategoryExp` are not version-owned.

Successor creation always starts with a byte-identical clone. Evolution may subsequently disable or exclude Cash Code membership, including codes used historically, but cannot remove the operational Cash Code identity while transactions reference it. Publication exposes any accounting discontinuity through the Equity Bridge rather than treating versioning as permission to rewrite the past.

### 5.2 Active-version invariant

The active version is the row referenced by the sole `App.tbOptions.CategoryVersionId`, not a version inferred by scanning status. `App.proc_NodeBusinessInit` already inserts the operational singleton with `Identifier = 'TC'`; the options trigger adds a serialised table-count assertion so a second row is rejected. `CategoryVersionId` may be null only before initialisation, has a foreign key to `Cash.tbCategoryVersion`, and must reference Published or Submitted once assigned. The foreign key prevents deletion of the active version.

The existing `Cash.tbCategory` and `Cash.tbCategoryTotal` become version-owned tables rather than being paired with duplicate snapshot tables. Ordinary calculations read explicit active views joined through the options pointer. Draft and historical reads require a `VersionId`. The generated Cash Statement alone exposes an eligible version selector, defaulting to the options-selected version and excluding Drafts; accounts and tax workspaces remain active-only outside sealed-history reconstruction.

### 5.3 Lifecycle vector

| Current | Command | Required conditions | Result |
|---|---|---|---|
| none | commission | valid installed tree; uninitialised singleton options row; no version rows | first system-allocated version Published and assigned to `App.tbOptions.CategoryVersionId` |
| Published | mutate live | authorised actor; no durable submission lock; current concurrency token | same-version category, total, mapping or root rows change; prepared seals for the former hash become stale |
| Published | acquire submission lock | prepared seal binds current version and hash | content remains Published but all live mutations/publish operations are rejected |
| Published + lock | authority success | exact locked seal/operation; acknowledged success | same version becomes Submitted; hash unchanged; lock becomes completed evidence |
| Published + lock | conclusive failure/cancel | terminal outcome is proven | lock released; version remains editable Published |
| Published + lock | uncertain outcome | outcome unresolved | lock retained; status remains Published; dispatch and tree mutation remain blocked |
| Published/Submitted | clone active | authorised actor | new Draft with predecessor and byte-identical semantic content; active version unchanged |
| Draft | mutate/validate | authorised actor; current concurrency token | Draft only; ordinary statements unchanged |
| Draft | publish | options-selected predecessor still matches `BaseContentSha256`; valid graph/mappings/roots/tax coverage; required change note and actor; no lock | Draft Published, options pointer switched, and predecessor Archived with matched effective intervals in one transaction |
| Draft | discard | no seal/reference and authorised actor | Draft deleted |
| Submitted/Archived | any content mutation or delete | always | reject |
| Submitted | additional submission | exact same version/hash and new accepted seal operation | remain Submitted; another seal may reference it |

There is no `Submitted -> Published`, `Archived -> *`, or automatic time-based transition.

### 5.4 Canonicalisation profile v1

Canonical bytes are UTF-8 without BOM, emitted as compact JSON with the exact property order below. Strings are Unicode NFC, JSON escaped by the canonical writer, and otherwise preserve stored content (no culture-sensitive case conversion or whitespace trimming). Integers use base-10 JSON numbers, logical enabled fields use JSON booleans, and nullable category polarity/type values use JSON `null`.

Top-level property order is:

1. `profile`
2. `categories`
3. `totals`
4. `codeMap`
5. `roots`

Row property and ordering rules are:

- category: `categoryCode`, `category`, `categoryTypeCode`, `cashPolarityCode`, `cashTypeCode`, `displayOrder`, `isEnabled`; sort by `categoryCode` ordinal;
- total: `parentCode`, `childCode`, `displayOrder`; sort by `parentCode`, then `childCode` ordinal (not display order);
- Cash Code mapping: `cashCode`, `categoryCode`; sort by `cashCode` ordinal; and
- root: `rootType`, `categoryCode`; emit `NET_PROFIT` then `VAT` (and any future profile-defined root in profile order).

The object deliberately excludes version/lifecycle metadata, Cash Code master fields, tax codes and mappings, all BI-expression content and diagnostics, and every SQL audit/concurrency field.

Conformance vector 1 is exactly 585 bytes and has SHA-256 `8B77B246D3DFDB77230B52695747D5D31A135FC5FFB65CD89CE4844A6D032177`:

```json
{"profile":"trade-control-category-tree-v1","categories":[{"categoryCode":"CAT-1","category":"Sales & services","categoryTypeCode":0,"cashPolarityCode":1,"cashTypeCode":0,"displayOrder":10,"isEnabled":true},{"categoryCode":"ROOT-1","category":"Net profit","categoryTypeCode":1,"cashPolarityCode":2,"cashTypeCode":0,"displayOrder":1,"isEnabled":true}],"totals":[{"parentCode":"ROOT-1","childCode":"CAT-1","displayOrder":1}],"codeMap":[{"cashCode":"CC-SALES","categoryCode":"CAT-1"}],"roots":[{"rootType":"NET_PROFIT","categoryCode":"ROOT-1"},{"rootType":"VAT","categoryCode":"ROOT-1"}]}
```

Required vector assertions are: repeat is byte-identical; input row order is irrelevant; lifecycle changes do not alter bytes; one-bit or one-field semantic changes alter the hash; NFC-equivalent category strings produce the same bytes; and Cash Code master, tax-code or BI-expression-only changes do not change this object but must change their applicable separate evidence where required.

### 5.5 Cash Statement version contract

The generated Cash Statement request gains an optional `CategoryTreeVersionId`. Null resolves to `App.tbOptions.CategoryVersionId`. An explicit identifier must resolve to Published, Submitted or Archived; Draft is rejected. Generating against a historical version reads version-owned categories, edges, Cash Code mappings and roots. Current BI expressions are parsed against the selected version and any incompatibility is reported; expressions are not reconstructed from tree history.

Each version records `ContentChangedOn` in business time as well as UTC whenever its semantic content changes. For every period end included by the generated statement:

- warn when the selected version's `ContentChangedOn` is later than that period end; and
- where publication history identifies a different version as active at that period end, warn that the selected version differs and offer that eligible historical version.

The warning never silently changes the selection. A mutable Published version cannot reconstruct intermediate states within the same version code; its last semantic-change timestamp makes that limitation visible. The Equity Bridge remains an accounting control and may block statutory readiness independently of this warning.

### 5.6 Later statutory-seal integration

The accepted Stage 1 manifest/provider interfaces must add a protected-object reference with at least:

- object kind `category-tree`;
- the `VersionId` code;
- profile `trade-control-category-tree-v1`;
- canonical SHA-256 and byte length;
- immutable content-addressed snapshot reference;
- preparation-time Category Tree status (`Published` or `Submitted`); and
- predecessor identity when present.

Preparation stores the canonical object before review. Approval re-resolves the active version and compares identifier, profile, digest and length. Acquiring the durable lock and final approval binding must be atomic from the application's perspective. The lock key is tenant/database instance plus Category Tree `VersionId`, and it references the seal and durable authority operation.

Every Published version-row write guard checks that lock in the same authoritative store/transaction boundary. Conclusive failure releases it; uncertainty retains it; acknowledged success records the seal relationship and changes Published to Submitted. A Submitted version can be referenced by later seals without becoming editable. Archival changes lifecycle metadata only and cannot change canonical bytes.

The VAT-wide source inventory may include the Category Tree protected-object digest as a named item, but it must not substitute a broad, rowversion-bearing reserialization of the projection for the versioned canonical object.

## 6. Resolved clarifications and remaining decisions

Owner clarification removes the Stage 1 gate: Category Tree change control proceeds first and the finished seal later records the then-current version. Successors evolve the active tree by cloning it; they are not empty replacements. The existing category tables become version-owned, while active views provide the ordinary calculation boundary. `Cash.tbCategoryCodeMap` versions only the mapping of persistent Cash Codes. `Cash.tbCategoryRoot` replaces the two `App.tbOptions` roots. BI expressions remain current, independently editable formula metadata. The generated Cash Statement defaults to active but may select eligible historical versions and displays affected-period warnings.

No further owner decision blocks the SQL lifecycle design. The accepted header uses a system-allocated `nvarchar(10)` `VersionId`, required description and optional notes. The remaining implementation choices are bounded engineering details such as the generated code pattern, index names, multiple cloned Draft handling and exact stored-procedure signatures.
