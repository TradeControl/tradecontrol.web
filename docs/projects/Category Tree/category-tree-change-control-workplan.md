# Category Tree change-control work plan

**Status:** Architecture agreed; implementation not started

**Release boundary:** Required before the Accounts Mode production release

**Depends on:** The Stage 1 statutory-seal contract and its canonical evidence rules
**Does not require:** General statement version selection or replacement of the current JavaScript tree

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

There is exactly one active version. Its status is either Published or Submitted. Draft versions may be prepared alongside it, while Archived versions remain historical evidence.

## 2. Agreed user model

### 2.1 Published is deliberately editable

A Published tree is the business's live working classification. An authorised user may add, edit, move, enable or disable its permitted contents. Every successful change is immediately visible in the Cash Statement and other unsubmitted calculations. There is no requirement to choose a version when opening an ordinary statement.

This is intentional: improving an unsubmitted classification is not rewriting a statutory assertion because no assertion has yet been sealed.

### 2.2 Submission creates the boundary

When a statutory submission succeeds, the active Published version becomes Submitted. Its canonical content hash and version identifier are recorded in the statutory seal. From that point the version and its protected contents are immutable.

The same Submitted version may support further submissions without being cloned when its content remains suitable. Multiple seals may therefore reference one Category Tree version.

### 2.3 Change after submission is deliberate

If the active version is Submitted, the Category Tree is displayed read-only. To change it, an authorised user must:

1. create a Draft, optionally by cloning the Submitted version;
2. edit and validate the Draft without affecting ordinary statements;
3. provide a change-control explanation and explicitly publish it; and
4. accept that publication will make the new tree the basis of current and unsubmitted statements.

Publication is atomic: the Submitted predecessor becomes Archived and the selected Draft becomes the sole active Published version in one database transaction.

### 2.4 No general statement version selector

Ordinary Cash Statements, accounts and tax workspaces always use the active projection. Only a sealed-submission or filing-history pathway reconstructs a historical version, using the Category Tree version and hash recorded in its seal.

A future comparison or Draft-impact preview may be useful, but it is not required for the first release and must not complicate the core lifecycle.

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

- exactly one active Category Tree version exists after commissioning;
- the active version has status Published or Submitted;
- no more than one version has active status at any time;
- Submitted and Archived contents cannot be inserted, updated or deleted;
- a Draft cannot affect the operational Category Tree or ordinary statements;
- publishing a Draft archives the predecessor and activates the successor atomically;
- a Submitted version's semantic content and content hash never change;
- lifecycle metadata is excluded from the semantic content hash, so Submitted-to-Archived does not invalidate an existing seal;
- the relationship from every seal to its Category Tree version survives archival;
- every successor identifies its predecessor; and
- publication of a successor to a Submitted version requires an explicit change note and authorised actor.

The one-active rule should be protected with a database constraint or unique filtered/computed key as well as transactional stored procedures. Application checks alone are insufficient under concurrent requests.

## 4. Scope of a Category Tree version

The implementation spike must confirm the complete calculation-sensitive inventory. At minimum a version includes canonical representations of:

- `Cash.tbCategory`: identity, type, polarity, cash type, enabled state and display order;
- `Cash.tbCategoryTotal`: every parent-child edge and its display order;
- `Cash.tbCategoryExp`: expression text, format, syntax and calculation-relevant state;
- Cash Code membership of categories, including enabled state where it changes tree expansion;
- the configured Net Profit and VAT roots from `App.tbOptions`; and
- any other root or option demonstrably used to traverse or interpret the tree.

Descriptions and display order should remain in the version even where they do not alter a numerical result because they form part of what the user reviewed. Technical fields such as SQL `rowversion`, insertion timestamps and mutable lifecycle status do not belong in the semantic hash.

Tax Sources, Tax Tags and authority mappings remain separate accounting or statutory configuration. If they can alter a submission they receive their own protected-source evidence and hashes; they must not be silently absorbed into the Category Tree definition.

## 5. SQL design

### 5.1 Compatibility architecture

The first release should retain the existing operational tables as the active projection:

- `Cash.tbCategory`
- `Cash.tbCategoryTotal`
- `Cash.tbCategoryExp`
- `Cash.tbCode`
- the applicable root fields in `App.tbOptions`

Existing statements and calculations can therefore continue to read the active tree without a version parameter. Version control is added around this projection rather than changing every financial query.

Draft and immutable historical contents are held in version-owned snapshot tables. Provisional names are:

- `Cash.tbCategoryTreeStatus`
- `Cash.tbCategoryTreeVersion`
- `Cash.tbCategoryTreeCategory`
- `Cash.tbCategoryTreeTotal`
- `Cash.tbCategoryTreeExpression`
- `Cash.tbCategoryTreeCashCode`
- `Cash.tbCategoryTreeRoot`

Names and column types must follow the tcNodeDb4 conventions during implementation. A numeric or UUID internal key may be used, but each version also needs a stable human-readable revision.

### 5.2 Version header

`Cash.tbCategoryTreeVersion` should provide at least:

- immutable version identifier and revision number;
- status code;
- predecessor version identifier;
- change-control explanation;
- content-schema/canonicalisation version;
- canonical content hash and byte length once frozen;
- created, published, submitted and archived actor identities and business/UTC timestamps;
- optimistic concurrency token; and
- the seal or submission association needed to prove why it became Submitted.

Actor fields use the application identity established by `Usr.vwCredentials`, not the SQL login.

### 5.3 Controlled operations

Authoritative stored procedures or an equally narrow transactional service boundary should perform:

- initial-version capture during commissioning or synthetic regeneration;
- create empty Draft;
- clone active version to Draft;
- validate Draft;
- publish Draft;
- discard Draft;
- capture/canonicalise Published content for submission;
- mark Published as Submitted after successful sealed submission; and
- load an immutable historical snapshot for verification.

The existing Category Tree pages currently write through several EF Core handlers and bulk update/delete paths. Every mutation path must be inventoried and routed through one shared status guard. JavaScript-disabled requests, direct handler calls and concurrent requests must receive the same protection as the visible UI.

### 5.4 Active projection synchronisation

For an editable Published version, an accepted mutation must update both the active operational projection and its version-owned representation within one transaction. Draft mutations update only Draft rows.

Publishing a Draft must:

1. take an application lock for the tenant tree;
2. revalidate graph, codes, roots, expressions, tax mappings and concurrency tokens;
3. apply a safe delta to the operational tables, preserving referential integrity and preventing deletion of Cash Codes referenced by transactions;
4. verify that the resulting operational projection exactly matches the Draft's canonical representation;
5. archive the previous active version;
6. mark the Draft Published; and
7. commit all changes together.

If any step fails, neither lifecycle state nor the operational tree changes.

### 5.5 Database protection

Database-side protection should cover:

- uniqueness of revision numbers and the active slot;
- legal status transitions;
- immutable Submitted and Archived snapshot rows;
- prevention of cycles and invalid parent types;
- valid expression references and formats;
- valid root categories;
- preservation or safe disabling of referenced Cash Codes;
- prevention of direct active-projection mutation while the active version is Submitted; and
- detection of active-projection drift from its version-owned representation.

Deployment and bootstrap procedures must initialise version 1 only after the installed template has produced a valid Category Tree.

## 6. Canonical content and seal integration

The Category Tree canonicalisation profile must use deterministic UTF-8 bytes, invariant values and explicit ordering. Graph rows are sorted by stable keys rather than database retrieval order. The content hash covers semantic and reviewed presentation fields, but not lifecycle status, audit timestamps, SQL row versions or the hash itself.

The statutory seal records:

- Category Tree version identifier and revision;
- canonicalisation/profile version;
- content hash and byte length;
- exact canonical snapshot object reference;
- whether the version was Published or already Submitted when preparation began; and
- predecessor identity where relevant to the filing narrative.

Preparation and approval must recheck the version identifier and content hash. Editing a Published tree invalidates any prepared but undispatched seal that depended upon its previous content.

There is an important interval between approval and authority outcome. The four Category Tree statuses should not be expanded merely to represent transport. Instead, the statutory workflow takes a durable submission lock tied to the prepared seal:

- mutations of the active Published tree are rejected while the lock is held;
- successful dispatch/acknowledgement changes Published to Submitted;
- a conclusively failed or cancelled operation releases the lock back to editable Published state; and
- an uncertain authority outcome retains the lock until reconciled rather than permitting a second send or a tree change.

Archived snapshots and their canonical objects remain verifiable through the standalone seal verifier. A later status change from Submitted to Archived does not change the semantic hash.

## 7. Web and JavaScript user experience

The existing Fancytree/JavaScript implementation can remain. Change control is a server-owned capability exposed to it; the browser must not determine whether a write is legal.

### 7.1 Persistent version banner

The Category Tree page should display:

- revision and status;
- whether the displayed tree is Live or Draft;
- predecessor revision for a Draft;
- last publication/submission information;
- a concise explanation of the effect of editing; and
- the allowed actions for the current user and state.

Suggested behaviour:

- **Published:** “Live and editable. Changes affect current and unsubmitted statements immediately.”
- **Submitted:** “Live and read-only because this version supports a statutory submission.”
- **Draft:** “Proposal only. Statements continue to use revision N until this Draft is published.”
- **Archived:** “Historical read-only version retained as submission evidence.”

### 7.2 Commands

Administrators should receive clearly separated commands:

- Create Draft;
- Clone active version;
- Open Draft;
- Validate Draft;
- Publish Draft;
- Discard Draft; and
- View change history.

Publishing requires a plain-language change note and an explicit confirmation that current and unsubmitted statements will use the new structure. When replacing a Submitted version, the confirmation also states that the sealed predecessor will be archived but retained.

### 7.3 Write controls

All create, edit, delete, move, reorder, enable/disable, root-setting and expression actions must be disabled or hidden for Submitted and Archived contexts. The server independently rejects the same operations with a useful, non-technical explanation.

The tree's node-loading, selection, mobile action bar, context menu and split-pane behaviour should remain common between live and Draft workspaces. The context/version identifier is supplied on every read and write request; it must never be inferred solely from browser state.

### 7.4 Change review

Before publication, show a deterministic summary such as:

- categories added, changed, enabled/disabled or removed;
- edges and display order changed;
- Cash Codes added or reclassified;
- expressions changed;
- Net Profit or VAT roots changed; and
- validation warnings or statutory mappings affected.

A full graphical diff is not required for the first release. The initial review must nevertheless make material classification changes visible before confirmation.

## 8. Pre-launch regeneration and commissioning

### 8.1 Pre-launch adoption

There are no customer databases to migrate. Implementation therefore updates the templates, bootstrap and synthetic generator first, then recreates every disposable local and hosted test tenant so that change control is present from initial commissioning. Existing B1/B2 and other synthetic databases may be retained as BACPAC evidence or troubleshooting references, but they are not upgraded into the accepted test population.

Regeneration must prove that each template produces one valid Published revision 1, that its version-owned representation round-trips exactly to the operational projection, and that the expected statements remain correct. This is the complete migration process for the pre-launch programme; do not build customer conversion machinery that has no users to serve.

The underlying schema and lifecycle must nevertheless remain forward-upgradeable after launch. A future customer upgrade path will require its own plan and evidence rather than being inferred from this synthetic regeneration exercise.

### 8.2 New tenants

Tenant commissioning installs the selected template, validates it, captures revision 1 and marks it Published. No Draft is required for the initial template. A commissioned tenant must never be left without one active version.

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
- Failure during publication rolls back projection and status changes.
- Concurrent publication cannot produce two active versions.
- Referenced Cash Codes cannot be destructively removed.
- Graph cycles, invalid roots, invalid expressions and invalid Tax Tag coverage block publication.
- Lifecycle metadata changes do not alter the semantic hash.
- One-bit semantic changes do alter the hash.

### 9.2 Accounting and statutory tests

- A Published reclassification immediately changes the expected unsubmitted Cash Statement.
- The same change invalidates a prepared but undispatched seal.
- Filing workflow guidance identifies the one-year Published limit for a non-VAT micro-entity and the applicable quarterly limit for a VAT-registered or MTD business without changing state automatically.
- A Submitted tree reconstructs the sealed VAT, CT or accounts projection exactly.
- A successor Published version may change current/unsubmitted statements without changing predecessor reconstruction.
- Multiple seals can reference one unchanged Submitted version.
- Archived predecessor evidence remains independently verifiable.
- Net Profit, VAT, account and tax projections use the expected active roots and memberships.
- Tax Tag validation is rerun after every material tree change and before publication/submission.

### 9.3 Web tests

Extend the existing Playwright Category Tree suite and `categorytree_test_script.md` to cover desktop and mobile:

- correct banner, status and revision;
- state-dependent command visibility;
- Published editing;
- Submitted/Archived read-only behaviour;
- Draft creation, clone, edit, validation, discard and publication;
- explicit publish confirmation and required change note;
- stale-tab and optimistic-concurrency handling;
- JavaScript-disabled/direct-request write rejection;
- selection and expanded-state isolation between active and Draft trees; and
- accessible keyboard and screen-reader treatment of status and disabled actions.

### 9.4 Recovery and performance tests

- Restart or connection loss during clone, publication, submission lock and archival.
- Backup restoration behind or ahead of externally retained seal evidence.
- Detection and repair workflow for projection/snapshot drift without rewriting sealed evidence.
- Canonicalisation and publication time on the largest supported tree.
- No material regression to Cash Statement, Accounts workspace or Tax Hub reads because they continue to use the active projection.

## 10. Delivery increments

### Increment 1 — Inventory and executable contract

- Confirm the exact protected tree inventory and current mutation paths.
- Add lifecycle transition and canonicalisation test vectors.
- Resolve version identifiers, revision numbering and snapshot schema.
- Specify integration with the Stage 1 seal and submission lock.

### Increment 2 — SQL foundation and regeneration

- Add status, version and snapshot schema.
- Add active-slot and immutability enforcement.
- Implement clone, validate, publish and submission-transition procedures.
- Update templates, synthetic generation and bootstrap, and increment the tcNodeDb4 build.
- Prove active projection round-trip and rollback.

### Increment 3 — Server boundary

- Add EF models and a single Category Tree version/change-control service.
- Route every Category Tree mutation through the shared guard.
- Add canonical snapshot/digest and seal-provider integration.
- Add concurrency, submission-lock and invalidation handling.

### Increment 4 — Existing JavaScript UI integration

- Add status/version banner and workspace context.
- Add Draft, clone, validate, publish, discard and history flows.
- Apply read-only behaviour consistently to context menu, detail cards and mobile controls.
- Add deterministic change summary and publication confirmation.

### Increment 5 — Accreditation and release evidence

- Execute SQL, accounting, Playwright, tamper and recovery tests.
- Prove a Published change affects unsubmitted statements.
- Seal a submission against the tree and prove immutability/reconstruction.
- Publish a successor and prove the predecessor remains verifiable.
- Record evidence in the VAT/CT accreditation dossier and Accounts Mode release checklist.

### Increment 6 — User Guide and release communication

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
- current statements require no version selection;
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
2. **Implement the Stage 1 pre-optimisation seal update.** Establish the canonical evidence, statement balance and submission-lock contracts on which Category Tree sealing depends.
3. **Implement Category Tree change control.** Build this work plan on the accepted seal foundation before final accreditation evidence and the Accounts Mode production release are declared complete.

The later tax calculation optimisation remains a separate Stage 2 programme. It may consume sealed balances and Category Tree provenance, but it is not a prerequisite for this change-control lifecycle.
