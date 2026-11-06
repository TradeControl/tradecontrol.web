# Category Tree change-control work plan

**Status:** Local implementation and hosted B1/B3-B6 build `4.1.20` rollout complete; statutory-seal integration and release evidence remain

**Release boundary:** Required before the Accounts Mode production release

**Later integration:** Statutory seals record the Category Tree version once both contracts exist; the core Category Tree lifecycle does not depend on completion of the Stage 1 seal
**Does not require:** Replacement of the current JavaScript tree or a general version selector across accounts and tax workspaces

## 1. Purpose

The Category Tree determines how Cash Codes are classified and rolled up into the Cash Statement, accounts and statutory projections. Changing the tree can therefore change the interpretation of transactions that already exist, even though the transactions themselves have not changed.

Businesses must remain free to improve a tree while its results are unsubmitted. Changes to the live Published tree should continue to affect current and historical unsubmitted statements immediately. Once a statutory submission has relied on the tree, however, that precise version must remain reproducible and read-only.

This work introduces a small manufacturing-style change-control lifecycle around the existing Category Tree:

| Code | Status | Editable | Operational meaning |
|---:|---|---|---|
| 0 | Draft | Yes | A proposed successor that does not affect ordinary statements. |
| 1 | Published | Yes | The live tree used by statements and statutory calculations. |
| 2 | Submitted | No | The live tree whose exact content has been used by one or more sealed submissions. |
| 3 | Archived | No | A superseded historical version retained for verification and reconstruction. |

There is exactly one active version: the version referenced by the singleton `App.tbOptions.CategoryVersionId`. Its status is either Published or Submitted. Draft versions may be prepared alongside it, while Archived versions remain historical evidence.

## 2. Agreed user model

### 2.1 Published is deliberately editable

A Published tree is the business's live working classification. An authorised user may add, edit, move, enable or disable its permitted contents. Every successful change is immediately visible in active-default Cash Statements and other unsubmitted calculations. There is no requirement to choose a version when opening an ordinary statement.

This is intentional: improving an unsubmitted classification is not rewriting a statutory assertion because no assertion has yet been sealed.

### 2.2 Submission creates the boundary

When a statutory submission succeeds, the active Published version becomes Submitted. Its canonical content hash and version identifier are recorded in the statutory seal. From that point the version and its protected contents are immutable.

The same Submitted version may support further submissions without being cloned when its content remains suitable. Multiple seals may therefore reference one Category Tree version.

### 2.3 Change after submission is deliberate

If the active version is Submitted, the Category Tree is displayed read-only. To change it, an authorised user must:

1. create a successor Draft by cloning the active Submitted version;
2. edit and validate the Draft without affecting ordinary statements;
3. provide a change-control explanation and explicitly publish it; and
4. accept that publication will make the new tree the basis of current and unsubmitted statements.

Publication is atomic: the Submitted predecessor becomes Archived and the selected Draft becomes the sole active Published version in one database transaction.

A successor evolves the predecessor; it is not an empty replacement. Cloning preserves the complete reviewed tree as the starting point. A later version may disable or otherwise exclude a historical Cash Code from its effective membership, but it must not erase the Cash Code identity or the transactions that used it. Any resulting discontinuity remains visible through the Equity Bridge and statutory-readiness checks.

### 2.4 Cash Statement version selection

The generated Cash Statement defaults to the active Category Tree version, but the user may select a Published, Submitted or Archived version when reconstructing or comparing a historical statement. Drafts are never selectable for an ordinary statement. Accounts and tax workspaces continue to use the active projection unless a sealed-filing pathway explicitly reconstructs historical evidence.

The Cash Statement warns when the selected tree's last semantic change occurred after an affected period end. It also warns when the active tree differs from the version that was active at that period end, where lifecycle history can identify one. The warning is explanatory rather than a silent version switch: the user chooses whether to retain the active default or select an eligible historical version.

This is a bounded Cash Statement capability, not a general statement-version selector. A future graphical comparison or Draft-impact preview may still be useful, but it must not complicate the core lifecycle.

### 2.5 Expected Published lifespan

The filing cadence sets the maximum intended lifespan of an editable Published version:

- a non-VAT-registered micro-entity may retain a Published version for at most one accounting year; and
- a VAT-registered or MTD business may retain a Published version for at most one applicable reporting quarter.

The applicable filing workflow must seal the active Published version, so it cannot remain editable after that filing. Time passing is not itself authority to submit, archive or change a tree: the product surfaces an approaching or overdue boundary and requires the authorised filing workflow. An unchanged Submitted version may remain active and support later submissions for as long as the business wishes. A new Published lifespan begins only when the business deliberately publishes a successor.

## 3. Lifecycle and invariants

### 3.1 Permitted transitions

| From | To | Meaning |
|---|---|---|
| none | Published | Commissioning creates the initial live version. |
| Published | Submitted | A successful sealed statutory submission freezes the live version. |
| Published | Archived | A reviewed Draft replaces an unsubmitted live version. |
| Submitted | Archived | A reviewed Draft replaces a submitted live version. |
| Draft | Published | Explicit publication makes the Draft operational. |
| Draft | deleted | An unused proposal is discarded. |

Submitted and Archived versions never return to an editable state. A rejected or abandoned change remains a Draft or is deleted; it is not represented as a historical statutory version.

### 3.2 Mandatory invariants

The database, rather than only the web interface, must enforce these rules:

- an initialised node has exactly one `App.tbOptions` row and a non-null `CategoryVersionId`;
- the version referenced by `App.tbOptions.CategoryVersionId` is the one active Category Tree and has status Published or Submitted;
- no version other than the options-selected version is treated as active, regardless of status;
- every persistent Cash Code has exactly one mapping in the active version, and that mapping targets a Nominal category;
- Total and Expression categories cannot receive Cash Code mappings;
- a mapped Nominal category cannot be deleted or changed to another category type until its same-version Cash Codes are explicitly moved; disabling does not erase its mappings or historical classification;
- physical Cash Code deletion cannot cascade through version history and is allowed only when no transaction or immutable version refers to the code;
- Submitted and Archived contents cannot be inserted, updated or deleted;
- a Draft cannot affect the operational Category Tree or ordinary statements;
- publishing a Draft archives the predecessor and activates the successor atomically;
- a Submitted version's semantic content and content hash never change;
- lifecycle metadata is excluded from the semantic content hash, so Submitted-to-Archived does not invalidate an existing seal;
- the relationship from every seal to its Category Tree version survives archival;
- every successor is cloned from and identifies its active predecessor;
- a Draft records the predecessor content hash from which it was cloned and cannot be published over a changed predecessor without an explicit refresh/rebase; and
- publication of a successor to a Submitted version requires an explicit change note and authorised actor.

The one-active rule is enforced by the existing one-row `App.tbOptions` contract plus its foreign key to `Cash.tbCategoryVersion`. The options trigger must assert the table contains no more than one row under a serialising lock and reject a pointer to Draft or Archived. `@@ROWCOUNT` alone is not sufficient because it reports only the current statement's affected rows. Publication changes the pointer and lifecycle states in one transaction.

## 4. Scope of a Category Tree version

The implementation spike must confirm the complete calculation-sensitive inventory. At minimum a version includes canonical representations of:

- `Cash.tbCategory`: identity, type, polarity, cash type, enabled state and display order;
- `Cash.tbCategoryTotal`: every parent-child edge and its display order;
- `Cash.tbCategoryCodeMap`: the version-specific mapping of persistent Cash Codes to leaf categories;
- the configured Net Profit and VAT roots in `Cash.tbCategoryRoot`; and
- any other root or option demonstrably used to traverse or interpret the tree.

Descriptions and display order should remain in the version even where they do not alter a numerical result because they form part of what the user reviewed. Technical fields such as SQL `rowversion`, insertion timestamps and mutable lifecycle status do not belong in the semantic hash.

`Cash.tbCode` remains persistent operational master data and is not copied into a version. `Cash.tbCategoryExp` remains current user-managed BI formula metadata: expressions, formats and parser diagnostics are outside Category Tree change control. A formula that no longer resolves against the selected tree is reported by the existing parser and can be corrected without changing immutable tree evidence.

Tax Sources, Tax Tags and authority mappings remain separate accounting or statutory configuration. If they can alter a submission they receive their own protected-source evidence and hashes; they must not be silently absorbed into the Category Tree definition.

## 5. SQL design

The implementation-level table, key, dependency, cutover and verification sequence is maintained separately in `category-tree-schema-change-plan.md`. That document is the governing Increment 2 schema plan; this section retains the architectural summary.

### 5.1 Version-owned tables and active views

The existing category tables become the version store rather than being duplicated into parallel snapshot tables. Ordinary calculations continue to have an active projection, but it is exposed through explicit active views which join through `App.tbOptions.CategoryVersionId`. Draft and historical reads use an explicit `VersionId`.

The agreed table shape is:

- `Cash.tbCategoryVersionStatus` (`Draft`, `Published`, `Submitted`, `Archived`);
- `Cash.tbCategoryVersion` (header, effective interval, content evidence and concurrency);
- `Cash.tbCategoryVersionEvent` (append-only lifecycle/change audit);
- `Cash.tbCategory` keyed by `VersionId, CategoryCode`;
- `Cash.tbCategoryTotal` keyed by `VersionId, ParentCode, ChildCode`;
- `Cash.tbCategoryCodeMap` keyed by `VersionId, CashCode`;
- `Cash.tbCategoryRoot` keyed by `VersionId, RootTypeCode`; and
- `Cash.tbCategoryRootType` defining at least `NET_PROFIT` and `VAT`.

`Cash.tbCode` remains the one persistent Cash Code master and no longer owns the authoritative `CategoryCode`; each version owns that relationship through `Cash.tbCategoryCodeMap`. `Cash.tbCategoryExp` remains current BI metadata outside the version lifecycle. `App.tbOptions.CategoryVersionId` replaces the two option root codes and selects the active version; that version's `Cash.tbCategoryRoot` rows provide its Net Profit and VAT roots.

Active views such as `Cash.vwCategoryActive`, `Cash.vwCategoryTotalActive`, `Cash.vwCategoryCodeMapActive` and `Cash.vwCategoryRootActive` form the compatibility boundary. All existing SQL consumers must be migrated deliberately to those views or to an explicit-version query; no unfiltered read of a version-owned table is a valid calculation source.

### 5.2 Version header

`Cash.tbCategoryVersion` should provide at least:

- immutable, system-allocated `nvarchar(10)` version identifier;
- status code;
- predecessor version identifier;
- short description and longer notes/change-control explanation;
- predecessor base-content hash for stale-Draft detection;
- content-schema/canonicalisation version;
- canonical content hash, byte length and last semantic-change time;
- inclusive effective-from and effective-to business dates; the Draft start is a proposal and the predecessor end is system-managed;
- created timestamp and actor;
- SQL-managed `rowversion` retained for ODBC/Office compatibility but excluded from EF and the ASP.NET command contract; and
- the seal or submission association needed to prove why it became Submitted.

`Cash.tbCategoryVersionEvent` records the actor, timestamp, from/to status, event kind and applicable note for create, edit, validate, publish, submit and archive events. Actor fields use the application identity established by `Usr.vwCredentials`, not the SQL login. SQL `timestamp`/`rowversion` is not used as a business time or ASP.NET concurrency token; command serialization and lifecycle guards are enforced in SQL.

### 5.3 Controlled operations

Authoritative stored procedures or an equally narrow transactional service boundary should perform:

- initial-version capture during commissioning or synthetic regeneration;
- clone active version to Draft;
- validate Draft;
- publish Draft;
- discard Draft;
- capture/canonicalise Published content for submission;
- mark Published as Submitted after successful sealed submission; and
- load an immutable historical version for verification.

The existing Category Tree pages currently write through several EF Core handlers and bulk update/delete paths. Every mutation path must be inventoried and routed through one shared status guard. JavaScript-disabled requests, direct handler calls and concurrent requests must receive the same protection as the visible UI.

### 5.4 Versioned mutation and publication

For an editable Published version, an accepted tree mutation updates its version-owned rows directly. Root mutations update `Cash.tbCategoryRoot`; Cash Code moves update only the active version's `Cash.tbCategoryCodeMap`. Draft mutations update only Draft-owned rows. Cash Code master data and BI expressions retain their separate operational mutation rules.

Publishing a Draft must:

1. take an application lock for the tenant tree;
2. verify that the active predecessor still has the Draft's recorded base content hash, then revalidate graph, Cash Code mappings, roots, tax coverage and transactional lifecycle guards;
3. validate that every Cash Code existing at publication has exactly one same-version category mapping;
4. mark the Draft Published and open its effective interval;
5. switch `App.tbOptions.CategoryVersionId` from the predecessor to the successor;
6. archive the predecessor and close its effective interval; and
7. commit all changes together.

If any step fails, neither lifecycle state nor the operational tree changes.

### 5.5 Database protection

Database-side protection should cover:

- uniqueness of version identifiers, the one-row options invariant and the valid active-version pointer;
- legal status transitions;
- immutable Submitted and Archived category, total, mapping and root rows;
- prevention of cycles and invalid parent types;
- valid root categories;
- exactly one valid category mapping for every Cash Code existing at publication, enabled or disabled;
- prevention of direct version-row mutation while the version is Submitted or Archived; and
- rejection of unfiltered version-owned reads in calculation paths.

Deployment and bootstrap procedures must initialise version 1 only after the installed template has produced a valid Category Tree.

## 6. Canonical content and seal integration

The Category Tree canonicalisation profile must use deterministic UTF-8 bytes, invariant values and explicit ordering. Graph rows are sorted by stable keys rather than database retrieval order. The content hash covers semantic and reviewed presentation fields, but not lifecycle status, audit timestamps, SQL row versions or the hash itself.

Once the statutory-seal work is ready to consume Category Tree provenance, the seal records:

- Category Tree version identifier;
- canonicalisation/profile version;
- content hash and byte length;
- exact canonical snapshot object reference;
- whether the version was Published or already Submitted when preparation began; and
- predecessor identity where relevant to the filing narrative.

Preparation and approval must eventually recheck the version identifier and content hash. Editing a Published tree will invalidate any prepared but undispatched seal that depended upon its previous content. This later integration does not block implementation of the Category Tree lifecycle, versioned rows, Cash Statement selection or database guards.

There is an important interval between approval and authority outcome. The four Category Tree statuses should not be expanded merely to represent transport. Instead, the statutory workflow takes a durable submission lock tied to the prepared seal:

- mutations of the active Published tree are rejected while the lock is held;
- successful dispatch/acknowledgement changes Published to Submitted;
- a conclusively failed or cancelled operation releases the lock back to editable Published state; and
- an uncertain authority outcome retains the lock until reconciled rather than permitting a second send or a tree change.

Archived versions and their canonical objects remain verifiable through the standalone seal verifier. A later status change from Submitted to Archived does not change the semantic hash.

## 7. Web and JavaScript user experience

The implementation-level interaction, request-boundary, existing-file and delivery sequence is maintained separately in `category-tree-ui-change-plan.md`. That document is the governing Increment 3/4 UI plan; this section retains the architectural summary.

The existing Fancytree/JavaScript implementation can remain. Change control is a server-owned capability exposed to it; the browser must not determine whether a write is legal.

The existing synthetic `__ROOT__` node becomes the **Category Tree** version workspace. Selecting it shows the active version, Draft proposals, historical versions, effective intervals, warnings and lifecycle commands. Selecting another version changes only the displayed workspace; only atomic Draft publication may change `App.tbOptions.CategoryVersionId`.

### 7.1 Persistent version banner

The Category Tree page should display:

- version code, description and status;
- whether the displayed tree is Live or Draft;
- predecessor version code for a Draft;
- last publication/submission information;
- a concise explanation of the effect of editing; and
- the allowed actions for the current user and state.

Suggested behaviour:

- **Published:** “Live and editable. Changes affect current and unsubmitted statements immediately.”
- **Submitted:** “Live and read-only because this version supports a statutory submission.”
- **Draft:** “Proposal only. Statements continue to use version N until this Draft is published.”
- **Archived:** “Historical read-only version retained as submission evidence.”

### 7.2 Commands

Administrators should receive clearly separated commands:

- Create successor Draft (clone active);
- Open Draft;
- Validate Draft;
- Publish Draft;
- Discard Draft; and
- View change history.

Publishing requires a plain-language change note and an explicit confirmation that current and unsubmitted statements will use the new structure. When replacing a Submitted version, the confirmation also states that the sealed predecessor will be archived but retained.

### 7.3 Write controls

All version-owned create, edit, delete, move, reorder, enable/disable and root-setting actions must be disabled or hidden for Submitted and Archived contexts. BI expression actions remain separately editable and show parser errors against the displayed tree; the server independently enforces the distinction.

The tree's node-loading, selection, mobile action bar, context menu and split-pane behaviour should remain common between live and Draft workspaces. The context/version identifier is supplied on every read and write request; it must never be inferred solely from browser state.

### 7.4 Change review

Before publication, show a deterministic summary such as:

- categories added, changed, enabled/disabled or removed;
- edges and display order changed;
- Cash Code mappings added or reclassified;
- Net Profit or VAT roots changed; and
- validation warnings or statutory mappings affected.

BI expression edits are reported separately by the formula workspace and parser because `Cash.tbCategoryExp` is not Category Tree version content.

A full graphical diff is not required for the first release. The initial review must nevertheless make material classification changes visible before confirmation.

## 8. Pre-launch regeneration and commissioning

### 8.1 Pre-launch adoption

There are no customer databases to migrate. Implementation therefore updates the templates, bootstrap and synthetic generator first so that new databases receive change control during commissioning. The B1-B7 Azure accounting sandboxes are the sole adoption exception: `tcNodeDb4-COSIPFVT1-COSTD26` is wiped and recreated to prove the upgraded bootstrap and synthetic generator, while the company MIN and both sole-trader databases are converted by the bounded migration in section 13 so their accumulated test data remains available.

Regeneration must prove that each template produces one valid first Published version, that its canonical representation matches the version-owned rows, that the active views expose exactly that version, and that the expected statements remain correct. Together with the B1-B7 sandbox conversion, this is the complete migration process for the pre-launch programme; do not generalise it into customer conversion machinery that has no users to serve.

The underlying schema and lifecycle must nevertheless remain forward-upgradeable after launch. A future customer upgrade path will require its own plan and evidence rather than being inferred from this synthetic regeneration exercise.

### 8.2 New tenants

Tenant commissioning installs the selected template, validates it, allocates the first `nvarchar(10)` version code and marks it Published. No Draft is required for the initial template. A commissioned tenant must never be left without one active version.

### 8.3 Build and distribution

Implementation includes the corresponding tcNodeDb4 build increment, installer/upgrade scripts, EF Core models and solution artifacts. SQL Server is the first implementation, but the lifecycle, canonical content and seal contract must remain portable to PostgreSQL.

## 9. Validation programme

### 9.1 SQL and lifecycle tests

- Initial commissioning produces exactly one Published version.
- Multiple Drafts may exist without affecting the active tree.
- A Published edit changes the Cash Statement and canonical content as expected.
- Draft edits do not change any ordinary statement.
- Submission freezes the active version and preserves its hash.
- Every direct and application mutation path is rejected for Submitted and Archived versions.
- Clone produces byte-equivalent semantic content before editing.
- Publishing atomically archives the predecessor and creates exactly one active Published version.
- Failure during publication rolls back effective intervals and status changes.
- Concurrent publication cannot produce two active versions.
- Referenced Cash Codes cannot be destructively removed.
- Graph cycles, invalid roots, incomplete Cash Code mappings and invalid Tax Tag coverage block publication.
- Lifecycle metadata changes do not alter the semantic hash.
- One-bit semantic changes do alter the hash.

### 9.2 Accounting and statutory tests

- A Published reclassification immediately changes the expected unsubmitted Cash Statement.
- The same change invalidates a prepared but undispatched seal.
- Filing workflow guidance identifies the one-year Published limit for a non-VAT micro-entity and the applicable quarterly limit for a VAT-registered or MTD business without changing state automatically.
- A Submitted tree reconstructs its sealed classification exactly; statement evidence separately records any non-versioned BI expression output it relies upon.
- A successor Published version may change current/unsubmitted statements without changing predecessor reconstruction.
- Multiple seals can reference one unchanged Submitted version.
- Archived predecessor evidence remains independently verifiable.
- Net Profit, VAT, account and tax projections use the expected active roots and memberships.
- Tax Tag validation is rerun after every material tree change and before publication/submission.

### 9.3 Web tests

Extend the existing Playwright Category Tree suite and `categorytree_test_script.md` to cover desktop and mobile:

- correct banner, description, status and version code;
- state-dependent command visibility;
- Published editing;
- Submitted/Archived read-only behaviour;
- successor Draft clone, edit, validation, discard and publication;
- explicit publish confirmation and required change note;
- active-by-default Cash Statement version selection excluding Drafts;
- warnings when a selected tree changed after an included period end or differs from the version active at that period end;
- stale-tab and optimistic-concurrency handling;
- JavaScript-disabled/direct-request write rejection;
- selection and expanded-state isolation between active and Draft trees; and
- accessible keyboard and screen-reader treatment of status and disabled actions.

### 9.4 Recovery and performance tests

- Restart or connection loss during clone, publication, submission lock and archival.
- Backup restoration behind or ahead of externally retained seal evidence.
- Detection of active-view/version inconsistencies without rewriting sealed evidence.
- Canonicalisation and publication time on the largest supported tree.
- No material regression to Cash Statement, Accounts workspace or Tax Hub reads because they continue to use the active projection.
- Historical Cash Statement generation can select an eligible immutable version without changing the active projection.

## 10. Delivery increments

### Increment 1 — Inventory and executable contract

- Confirm the exact protected tree inventory and current mutation paths.
- Add lifecycle transition and canonicalisation test vectors.
- Resolve the version identifier, version-owned table and active-view schema.
- Specify the later seal-consumer contract without blocking the Category Tree lifecycle.

### Increment 2 — SQL foundation and regeneration

**Progress:** Increment 2 is complete locally. The SSDT model builds; authoritative clone, refresh, metadata, category, edge, mapping, root, Cash Code create/delete, validate, publish, submit/archive and discard commands are present; and Submitted/Archived direct DML is rejected. The exact `tc-business1` build `4.1.18` BACPAC differential passed, followed by the three local build `4.1.13` shapes. The clean `tcNodeDb4-COSIPFVT1-COSTD26` publish and synthetic initialisation succeed, and owner smoke testing confirmed the Cash Statement, Equity Bridge and Tax Hub while the deliberately deferred Category Tree interface remains broken. Migration validation exposed and corrected three packaging omissions: ten generated Category Tree foreign keys were not re-trusted, the SQLCMD input had lost its required UTF-8 BOM, and the local `4.1.13` prerequisite retained the deliberately retired `Usr.tbMenu*` subsystem. The script now re-trusts the constraints, retains a BOM protected by an executable boundary assertion, and transactionally removes the legacy menu tables/views/procedures only on the allow-listed build `4.1.13` path. Both sole-trader databases were restored from their recovery BACPACs and replayed; the company MIN database then completed the final fresh end-to-end rehearsal. All three retain exact pre/post calculation checksums, contain no mojibake or legacy menu objects, pass DBCC and the strengthened lifecycle fixture, and report zero Category Tree DACPAC operations. The B1–B7 Azure sandboxes begin at build `4.1.18` and are therefore unaffected by the deprecated-menu cleanup. The VAT work remains unamended. TCExports and the Category Tree interface have not been changed.

- Add status, version-owned tables, mapping, roots and active views.
- Add the singleton options pointer and immutability enforcement.
- Implement clone, validate, publish and submission-transition procedures.
- Update templates, synthetic generation and bootstrap, and increment the tcNodeDb4 build.
- Prove active-view selection, canonical equivalence and rollback.
- Deliver one consolidated schema and sandbox migration that contain every amended database object; use the production schema comparison only to validate zero remaining differences.
- Preserve existing client behaviour through compatibility views and procedure contracts, apart from the Category Tree JavaScript workspace which is deliberately deferred to Increment 4.
- Build one pre-update synthetic database, clone it, apply the migration only to the clone and compare calculation-sensitive SQL outputs in both directions before application testing.
- Remove the superseded Cash Code/category and options/root authority columns only after the differential proves equivalent active Cash Codes, roots and accounting outputs.
- Point the application with its bounded EF compatibility alignment at the migrated clone and require every client surface except the deliberately deferred Category Tree interface to work.
- Make no TCExports or Category Tree interface changes in this increment; permit only the bounded TCWeb EF compatibility alignment required after physical authority-column removal.

### Increment 3 — Server boundary

**Progress:** The version-aware EF alignment, workspace/capability contract and single Category Tree change-control service are implemented locally. Every retained tree mutation handler, including JavaScript-disabled page posts, carries the selected `VersionId` and routes category, edge, mapping, root and Cash Code changes through serializable guarded commands. Composite mutations are atomic. Submitted/Archived and inactive-Published rejection remains authoritative in SQL. SQL-managed `RowVer` columns are deliberately absent from EF and web contracts; executable boundary tests prohibit direct handler writes, except for current non-versioned BI expression parse status. The root workspace and lifecycle controls are operational. Cash Statement selection, inclusive period attribution, warnings and durable workbook evidence are operational. The controlled local changeover now proves active/default and historical Cash Flow plus reconciliation through one version-parameterised Equity Bridge function hierarchy. `Cash.vwEquityReconciliationByYear` is the active compatibility wrapper supplying `App.tbOptions.CategoryVersionId`; historical generation supplies the selected version directly to the same head function. Seal integration remains the agreed later dependency.

- Add EF models and a single Category Tree version/change-control service.
- Route every Category Tree mutation through the shared guard.
- Add canonical content/digest and seal-provider integration.
- Add concurrency, submission-lock and invalidation handling.
- Add Cash Statement version selection and affected-period warnings.

### Increment 4 — Existing JavaScript UI integration

**Progress: complete locally and owner-accepted.** The retained JavaScript tree carries version context through editing, preserves selection during moves, supports lifecycle-aware read-only behaviour, and integrates the root version workspace on desktop and mobile. Browser review covered Published, Draft and Submitted behaviour. Cash Statement selection, period warnings, Settings evidence, the controlled local changeover, cross-shape build `4.1.20` Equity Bridge equivalence and the loss-making company fixture are complete. Hosted acceptance remains an Increment 5 release-evidence activity rather than unfinished local UI implementation.

- Add status/version banner and workspace context.
- Add successor Draft clone, validate, publish, discard and history flows.
- Apply read-only behaviour consistently to context menu, detail cards and mobile controls.
- Add deterministic change summary and publication confirmation.

### Increment 5 — Accreditation and release evidence

**Status: in progress.** The guarded build `4.1.18` to `4.1.20` migration is complete on hosted B1 and B3-B6; B2 and B7 remain excluded development artefacts. B3-B6 each retained identical pre/post Profit and Loss, business-tax, VAT-statement, Equity Bridge and Cash Statement fingerprints, one active Published version, trusted Category Tree constraints and a clean `DBCC CHECKDB`. B1 and B3 DACPAC validation reported no unexplained build-20 differences. Shared Category Tree provenance is integrated into the VAT seal v2 and the CH/CT accounts snapshot, including preparation bracketing, approval/dispatch revalidation, retained evidence and review-surface disclosure. The database-authoritative submission lock and authority-outcome transitions were published as build `4.1.21` to hosted B3-B6 and B8, followed by a runtime-successful shared-app deployment. B8's hosted SQL acceptance passed stale-evidence, immutable-lock, competing-owner, conclusive-release, manual-bypass and rollback-only completion checks while restoring the exact Published baseline. The remaining Increment 5 boundary is deployed workflow/recovery evidence, authority receipts and hosted reconstruction evidence.

The living Business 8 authority/changeover sequence and its recorded baseline are maintained in [Category Tree hosted statutory changeover test](category-tree-hosted-statutory-changeover-test.md). It exercises VAT on the initial version, Companies House accounts on a successor, and the post-submission plant-purchase/Equity Bridge scenario on a further successor.

- Execute SQL, accounting, Playwright, tamper and recovery tests.
- Prove a Published change affects unsubmitted statements.
- Seal a submission against the tree and prove immutability/reconstruction.
- Publish a successor and prove the predecessor remains verifiable.
- Record evidence in the VAT/CT accreditation dossier and Accounts Mode release checklist.

### Increment 6 — User Guide and release communication

**Status: pending by design.** Do not publish until the hosted release candidate, statutory-seal integration and Increment 5 evidence are accepted.

- Update the public User Guide only after the complete workflow and reviewed interface exist.
- Explain the four statuses, the one-active-version rule and the immediate effect of editing Published content.
- Explain the maximum one-year Published lifespan for non-VAT micro-entities and the maximum applicable-quarter lifespan for VAT-registered and MTD businesses, with sealing performed through the authorised filing workflow.
- Make clear that the system never submits, archives or publishes a version merely because time has elapsed.
- Illustrate cloning a Submitted version, reviewing a Draft, publishing deliberately and reopening historical sealed evidence.
- Use synthetic, non-secret screenshots from the accepted release candidate.

## 11. Exit criteria

The change-control system is complete when:

- the lifecycle and one-active invariant are enforced in SQL;
- users can understand and operate Published, Submitted, Draft and Archived states on desktop and mobile;
- current Cash Statements default to the active version while allowing deliberate eligible historical-version selection;
- Published changes affect unsubmitted statements immediately;
- Submitted and Archived trees are immutable through every supported path;
- successor publication is atomic, explicit and auditable;
- every relevant statutory seal identifies and verifies the precise Category Tree version;
- historical sealed results reconstruct independently of the active tree;
- commissioning and synthetic database regeneration create a valid initial Published version;
- the released User Guide accurately explains annual/quarterly cadence and the complete Draft-to-Published workflow; and
- the complete validation programme passes on local, hosted and restored tenant databases.

## 12. Programme order

The recommended sequence for the three current programmes is:

1. **Complete the Hosting Plan accreditation baseline.** Finish the environment and commissioning work needed to exercise VAT and CT reliably without expanding into launch-scale hosting features.
2. **Implement Category Tree change control.** Establish versioned evolution, authoritative roots, Cash Statement selection and the database mutation boundary independently of the unfinished seal.
3. **Complete the Stage 1 pre-optimisation seal update and integrate it.** Add the then-current Category Tree version and hash to statutory evidence and bind its submission lock to the established Category Tree mutation boundary.

The later tax calculation optimisation remains a separate Stage 2 programme. It may consume sealed balances and Category Tree provenance, but it is not a prerequisite for this change-control lifecycle.

**Current position, 6 November 2026:** the local Category Tree implementation and UI/reporting acceptance are complete at build `4.1.20`. Hosted B1 and B3-B6 now run the same boundary; B2 and B7 were deliberately excluded as development artefacts. The B3-B6 one-off migration retained exact accounting/reporting fingerprints and passed structural, constraint and DBCC validation, while B1 and B3 production schema comparisons contain only the documented Azure-managed principal/`Platform`, physical `App.tbYearPeriod` column-order and equivalent check-constraint-text exclusions. A failed first B3 attempt was recovered from the verified pre-migration Azure point-in-time backup before the corrected artefact was replayed; its preserved partial database remains pending explicit cleanup approval. The Stage 1 seal update is intentionally next: bind the selected Category Tree version and canonical digest into statutory evidence, then prove submission immutability and historical reconstruction. Accreditation evidence follows that integration; public User Guide publication remains last.

## 13. IAM-01 synthetic-business migration

**Status: complete locally at build `4.1.20`.** The three former build `4.1.19` migrated databases passed a transactional reporting-only uplift whose commit was conditional on exact comparison of the complete old Equity Bridge body, active wrapper and version-explicit function. `tcNodeDb4-COSIPFVT1-COSTD26` already held the full current implementation. The company MIN database was subsequently used as the backed-up disposable Standard-company loss fixture and remains intentionally populated with that accepted dataset.

`tcNodeDb4-COSIPFVT1-COSTD26` is the accepted clean bootstrap and synthetic-generation target and remains excluded from the migration allow-list. Maintain the single-database migration artefact alongside the implementation at `src/sqlnode/src/tcNodeDb4/Scripts/DB4 cat change control migration script.sql`; its source copy remains guarded and UTF-8 BOM encoded. With explicit owner authorisation, both sole-trader databases were restored and replayed after the trust/encoding corrections, then `tcNodeDb4-COMIPFVT1-COMIN26` completed the final fresh rehearsal including retirement of the old Access-client menu subsystem. All four local databases reached the completed Category Tree schema boundary at build `4.1.19`; the three migrated businesses passed their shape-specific accounting and lifecycle evidence. The current implementation target is build `4.1.20`, adding version-explicit Cash Statement and Equity Bridge reporting while retaining `4.1.19` as the recorded schema-migration milestone.

The migration must:

- preflight the expected database build, singleton options row, graph, Cash Code mappings and legacy roots;
- use an agreed backup/export recovery point and temporarily remove the earlier development read-only lock by its supported mechanism;
- convert the current graph into one initial Published version, derive versioned Cash Code mappings and roots, and assign `App.tbOptions.CategoryVersionId` atomically;
- migrate dependent objects and remove the legacy category/root authority columns only after the pre/post differential proves equivalent outputs;
- prove canonical equivalence and unchanged accounting, Cash Statement, Equity Bridge and applicable tax baselines;
- emit per-database, secret-free evidence and restore the intended sandbox access state; and
- reject unexpected builds, partial prior attempts, out-of-scope databases and unsafe reruns.

The rebuilt Standard Company database must prove clean commissioning, version creation and full synthetic generation without transitional compatibility state. The detailed object order, transaction boundary, recovery behaviour and acceptance checks are governed by section 11 of `category-tree-schema-change-plan.md`. This exception is limited to the four named IAM-01 synthetic businesses: it does not authorise customer migration tooling, general Azure rollout, hosted-tenant deployment, accreditation or User Guide publication.

Before changing B1–B7, restore or clone an exact build `4.1.18` sandbox locally, apply the current consolidated `4.1.20` migration once, rerun the accounting/lifecycle/Equity Bridge suites and use the production schema comparison only as a validator. This is the hosted-deployment gate for the amended artefact; it does not reopen the completed IAM-01 or UI acceptance steps.
