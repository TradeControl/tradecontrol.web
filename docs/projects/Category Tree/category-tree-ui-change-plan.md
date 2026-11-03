# Category Tree user-interface change plan

**Status:** Draft for owner review; root version-workspace direction agreed; implementation has not started

**Governing plan:** `category-tree-change-control-workplan.md`

**Schema partner:** `category-tree-schema-change-plan.md`

**Inventory:** `category-tree-increment-1-inventory-and-contract.md`

## 1. Purpose

This document is the implementation plan for the Category Tree web interface and the bounded Cash Statement version selector. It expands section 7 of the governing workplan in the same way that the schema change plan expands section 5.

The existing Fancytree, split-pane page, detail cards, context menu and mobile action bar remain the user-interface foundation. The change-control interface adds an explicit version workspace and routes the retained controls through the server-owned version boundary; it does not replace the tree with a new JavaScript framework.

The interface must make these facts unmistakable:

- the version referenced by `App.tbOptions.CategoryVersionId` is the sole operational tree;
- Published is live and editable until it is sealed;
- Draft is editable but does not affect ordinary statements;
- Submitted and Archived tree content is read-only;
- viewing another version does not activate it;
- only publishing a validated successor Draft changes the active version;
- Cash Codes remain persistent master records while their Category assignment belongs to each tree version; and
- BI expressions remain current formula metadata outside Category Tree version content.

This plan does not authorise hosted deployment, Azure sandbox migration, accreditation, statutory-seal completion or User Guide publication.

## 2. Agreed interaction model

### 2.1 The root node is the version workspace

The existing synthetic `__ROOT__` node becomes the entry point for Category Tree change control. Its internal key remains unchanged so existing tree navigation and JavaScript can be retained, but its visible title changes from `Σ Root` to **Category Tree**.

Selecting this node replaces the current generic Cash Code list in the right-hand pane with a root-only **Category Tree Versions** workspace. `Disconnected` and `By Cash Type` retain their existing Cash Code browsing behaviour. A compact Cash Code search may be added to the version workspace if testing shows that removing the root-level list makes navigation materially worse, but it is not part of the first implementation.

The root workspace is the home for:

- the active version summary;
- Draft proposals;
- Submitted and Archived history;
- effective intervals and semantic-change warnings;
- clone, refresh, validate, publish and discard commands;
- deterministic change review; and
- lifecycle event history.

### 2.2 Workspace selection is not activation

The page has one explicit `VersionId` workspace context. With no requested version, the server resolves `App.tbOptions.CategoryVersionId` and opens the active tree. Selecting a Draft or historical version reloads the tree and detail pane for that version only.

Version selection must never update `App.tbOptions`. The only interface operation allowed to change the options pointer is publication of a validated successor Draft through `Cash.proc_CategoryVersionPublish` and the application change-control service.

The selected version should be represented in the page URL, for example `?versionId=CV00000002`, so refresh, browser history and a copied local link preserve the workspace. The server resolves and authorises that identifier on every request. Browser state is never the authority for status or editability.

After successful publication, the workspace follows the newly active Published version. After a Draft is discarded, the workspace returns to the active version. An unavailable or deleted Draft link returns to the active version with a concise explanation rather than an empty tree.

### 2.3 Persistent version banner

A version banner appears above the split pane and remains visible when the user selects categories, totals or Cash Codes. It contains:

- version code and description;
- status badge;
- **Live**, **Proposal** or **Historical** context;
- predecessor code for a Draft;
- effective interval where applicable;
- last semantic-change time;
- validation or stale-predecessor state; and
- a short consequence statement.

Required wording is:

| Context | Consequence statement |
|---|---|
| Active Published | **Live and editable. Changes affect current and unsubmitted statements immediately.** |
| Active Submitted | **Live and read-only because this version supports a statutory submission.** |
| Draft | **Proposal only. Statements continue to use {active version} until this Draft is published.** |
| Archived | **Historical and read-only. This version is retained for reconstruction and evidence.** |

The banner must not rely on colour alone. Status text, an icon and accessible description are all required.

### 2.4 Effective dates are lifecycle evidence

`EffectiveFromOn` and `EffectiveToOn` are generated by commissioning and publication. They are displayed but are not user-editable scheduling fields. The first release does not support backdating, future-dated activation or automatic publication.

The root workspace may display broad warnings, such as:

- the active Published tree changed after one or more completed periods;
- the expected annual or quarterly sealing boundary is approaching or overdue;
- a Draft was cloned from an active version that has since changed; or
- a Draft has not yet passed validation.

Period-specific comparison warnings belong in the Cash Statement, where the user has selected a financial period and eligible tree version.

## 3. Root workspace information architecture

### 3.1 Active version card

The first card shows the one options-selected version, including:

- version code, description and status;
- effective-from time and, for historical display, effective-to time;
- last semantic change and last validation;
- canonical profile, digest availability and byte length without presenting the digest as a user decision;
- Net Profit and VAT root names;
- category, edge and Cash Code mapping counts;
- validation state and warnings;
- expected Published lifespan guidance where applicable; and
- the primary action allowed by the current state and user.

For an active Published version, administrators may edit the tree or clone a successor Draft. For an active Submitted version, the tree is read-only and the principal change action is **Create successor Draft**.

### 3.2 Draft proposals

Drafts are listed separately from operational and historical versions. Each row shows:

- version code and description;
- predecessor version;
- creator and creation time;
- last semantic change;
- validation state;
- whether the predecessor still matches `BaseContentSha256`; and
- available commands.

Multiple Drafts may exist. The interface must not call any Draft “current” or imply that the most recent Draft will be published automatically.

Draft actions are:

- **Open**;
- **Refresh from active** when its predecessor has changed;
- **Validate**;
- **Review changes**;
- **Publish** when eligible; and
- **Discard** with confirmation.

### 3.3 Version history

Published, Submitted and Archived versions are shown as a newest-first lifecycle list or compact table. Each entry contains:

- version code and description;
- status;
- predecessor;
- effective interval;
- creation and transition actors/times;
- last semantic-change time;
- canonical evidence state; and
- **View tree** and **View history** actions.

The active row is visually identified but not inferred from status. Archived and Submitted entries are read-only. A Submitted version that remains active is labelled **Live · Submitted**, not historical.

### 3.4 Lifecycle event history

Event history is an audit view, not an editable activity feed. It lists create, edit, validate, refresh, publish, submit and archive events with actor, UTC/business display time, transition and change note. Technical repeated edit events may be grouped for readability, but the underlying audit remains available.

### 3.5 Responsive layout

On desktop the version workspace uses the existing right-hand pane. On mobile it opens through the established detail-page/navigation pattern rather than attempting to fit a management table beneath the tree.

Mobile actions use the same server capability response as desktop. No lifecycle command may exist only in a desktop context menu.

## 4. State and action matrix

The server returns capabilities for the resolved user and version. JavaScript renders them but never manufactures permission.

| Capability | Active Published | Active Submitted | Draft | Archived |
|---|:---:|:---:|:---:|:---:|
| View tree and details | Yes | Yes | Yes | Yes |
| Edit categories, totals and mappings | Admin | No | Admin | No |
| Reorder, move, enable/disable | Admin | No | Admin | No |
| Change Net Profit or VAT root | Admin | No | Admin | No |
| Clone active to successor Draft | Admin | Admin | No | No |
| Refresh/rebase | No | No | Admin | No |
| Validate | Admin | No | Admin | No |
| Review deterministic changes | Yes | Yes | Yes | Yes |
| Publish | No | No | Admin, when eligible | No |
| Discard | No | No | Admin | No |
| View lifecycle history | Yes | Yes | Yes | Yes |
| Select for ordinary Cash Statement | Yes | Yes | No | Yes |

Published mutation is additionally disabled while a durable submission lock is held. The banner then explains that an approved filing is awaiting a conclusive authority outcome.

BI expression text, formatting and parser diagnostics are not governed by this matrix. The Cash Expressions workspace must label them as current formula metadata and report parsing against the displayed tree. Tree-category fields belonging to an expression node remain version-owned and follow the matrix.

## 5. Cash Code and expression boundaries

### 5.1 Cash Code detail and edit surfaces

The existing Cash Code form combines persistent master fields with Category membership. The revised interface must distinguish them:

- persistent: Cash Code identity, description, tax code and enabled state;
- version-owned: Category assignment in the displayed `VersionId`.

Moving an existing Cash Code changes only `Cash.tbCategoryCodeMap` for the selected editable version. It does not rewrite another version.

Creating or physically deleting a Cash Code is a global operational action, not a Draft-only tree edit. In the first release those commands are available only from the active Published context and use the atomic `proc_CodeAdd`/guarded `proc_CodeDelete` boundary. A Draft cloned before a new Cash Code was created must be refreshed or otherwise supplied with a valid mapping before it can pass publication validation.

When viewing Submitted, Archived or Draft content, global master-data fields must not appear to be versioned. Mixed forms should be split into a persistent-data action and a version-assignment action, or label the distinction with equal clarity.

### 5.2 BI expressions

Expression text and diagnostics remain in current `Cash.tbCategoryExp`. They are not cloned, submitted or archived with a tree. The interface must state this when an expression is opened against a Draft or historical tree.

Parser failures are shown against the displayed version. Correcting the formula updates current BI metadata and does not alter that version's canonical digest. Creating, deleting or changing the version-owned expression category still follows the selected version's editability.

## 6. User workflows

### 6.1 Open the Category Tree

1. Resolve the active version from the singleton options row.
2. Render its banner and capability set.
3. Load all nodes and details with that explicit `VersionId`.
4. Select the Category Tree root and show the versions workspace by default on a first visit.

The existing `select` and `expand` navigation parameters continue to work and are combined with the resolved version context.

### 6.2 View another version

1. Select **Open** or **View tree** in the root workspace.
2. Navigate to the explicit version URL.
3. Reload the node data, expansion state and details for that version.
4. Display a persistent Proposal or Historical banner.

Tree expansion/selection storage must be namespaced by tenant and `VersionId` so a Draft does not inherit misleading expanded nodes from the active tree.

### 6.3 Clone the active version

1. Choose **Create successor Draft** from the active card.
2. Enter a required short description and optional notes.
3. Confirm that the Draft is isolated from ordinary statements.
4. Execute the authoritative clone command.
5. Open the returned Draft and show clone-equivalence/validation state.

The client does not construct or copy tree rows.

### 6.4 Edit a Published version or Draft

Every read and mutation carries the explicit workspace `VersionId`. Mutation requests also carry the latest version/item concurrency token required by the server contract.

After success, the server returns the current status and replacement token. The interface reloads affected nodes and the banner without assuming the version remained editable during the request.

### 6.5 Resolve a stale Draft

When the active predecessor no longer matches the Draft's base digest:

- publication is unavailable;
- the Draft banner and root row show **Predecessor changed**;
- **Refresh from active** explains that the Draft content will be replaced with a fresh clone; and
- refresh requires explicit confirmation and a change note.

The first release does not attempt an automatic graph merge. Users must review and reapply their proposed changes after refresh.

### 6.6 Validate and review

Validation runs on the server and reports:

- graph/root validity;
- complete one-to-one Cash Code mapping to Nominal categories;
- stale predecessor state;
- applicable tax/statutory validation outcomes; and
- canonical digest/byte-length availability.

A successful validation does not publish or lock the Draft. Any subsequent semantic edit makes the earlier result stale.

The change review is generated deterministically by the server against the recorded predecessor and groups:

- categories added, removed or changed;
- enablement, polarity, type and order changes;
- graph edges added, removed or reordered;
- Cash Code mappings added or reclassified;
- root changes; and
- validation warnings.

BI expression edits and persistent Cash Code master-field changes are identified separately and are not presented as version content.

### 6.7 Publish a Draft

1. Open **Review changes**.
2. Resolve validation and stale-predecessor failures.
3. Enter a required plain-language change note.
4. Confirm that the new tree will immediately drive current and unsubmitted statements.
5. When replacing Submitted content, confirm that the predecessor remains retained as Archived evidence.
6. Execute the atomic publication command once.
7. Follow the newly active version and refresh the entire workspace.

The confirmation contains the exact Draft and predecessor codes. It must not use a generic “Are you sure?” message.

### 6.8 Discard a Draft

Discard identifies the exact Draft code and explains that no operational statement used it. On success the interface returns to the active version. Submitted, Published and Archived versions never expose this command.

## 7. Server and request contract

Increment 3 establishes a single Category Tree change-control service. Pages and handlers may not query status and then perform independent direct EF writes; the authoritative command must recheck the state transactionally.

### 7.1 Read context

The service returns a workspace context containing at least:

- requested and resolved `VersionId`;
- active `VersionId`;
- status and active flag;
- description, notes and predecessor;
- effective interval and semantic-change time;
- row-version/concurrency token;
- validation and canonical-evidence state;
- stale-predecessor/submission-lock state; and
- server-calculated capabilities for the current actor.

All Category Tree reads use an explicit version. Active-default resolution happens once at the request boundary, not as an unfiltered read of version-owned tables.

### 7.2 Read handlers

The retained handlers gain `versionId`:

- `Nodes(id, versionId)`;
- `Details(key, parentKey, versionId)`;
- create/edit/delete/move form GETs; and
- candidate lists and lookup endpoints.

The root detail handler returns the version workspace rather than the generic Cash Code list when `key == __ROOT__`.

### 7.3 Mutation handlers

Every Category Tree mutation posts:

- antiforgery token;
- explicit `versionId`;
- expected concurrency token;
- semantic command fields; and
- change note where required.

The service routes commands to the authoritative SQL procedures for category, edge, mapping, root, Cash Code and lifecycle changes. Existing direct `SaveChanges`, `ExecuteUpdate`, `ExecuteDelete` and bulk mutation paths are removed from page handlers.

Common responses identify success/failure, resolved version/status, replacement concurrency token, affected nodes and an actionable message. Stale requests produce a conflict result and reload option; immutable or locked state produces a state-specific rejection. Raw SQL exceptions are translated consistently without hiding the server audit entry.

### 7.4 Authorisation

Read access remains available to authenticated users. All tree and lifecycle mutations require the Administrators role at both page/service entry and authoritative database boundary. Expression pages must no longer bypass the consistent administrator rule for version-owned category changes.

JavaScript-disabled posts and direct handler requests receive the same authorisation, status, lock and concurrency checks.

## 8. Existing page and JavaScript change inventory

### 8.1 Razor pages and partials

| Surface | Required change |
|---|---|
| `Index.cshtml` / `Index.cshtml.cs` | Resolve workspace, render persistent banner/configuration and carry `versionId`. |
| `Details.cshtml` / `Details.cshtml.cs` | Add root version-workspace detail type and explicit-version reads. |
| New root partial(s) | Render active card, Draft list, history, warnings and lifecycle dialogs. |
| `_CategoryDetailsCard` | Render server capabilities and immutable-state explanation. |
| `_CashCodeDetailsCard` | Separate persistent master fields from version mapping. |
| `_ExpressionDetailsCard` | Explain current formula metadata versus version-owned category fields. |
| `_TreeContextMenu` | Render only server-authorised actions for the selected context. |
| `_MobileActionBar` | Match desktop capabilities and include access to version workspace. |
| Add/create/edit/delete/move pages | Carry version and concurrency tokens and use the shared service. |

### 8.2 JavaScript

| Script | Required change |
|---|---|
| `categoryTree.js` | Maintain workspace context, capability-aware actions and version-scoped selection/expansion. |
| `categoryTree.shared.js` | Include version/concurrency data in shared mutations and navigation. |
| `categoryTree.page.js` | Preserve explicit version in desktop/mobile navigation. |
| `categoryTree.move.js` | Post the selected version and conflict token. |
| `categoryTree.embedded.js` | Carry version through embedded forms and reload only the resolved workspace. |
| `categoryTree.delete.js` | Use state-specific confirmation and version-aware delete commands. |
| `categoryTree.cancel.js` | Restore details in the same version context. |

The JavaScript must consume server capabilities rather than duplicate the lifecycle matrix. Hiding a control is usability behaviour, not the security boundary.

## 9. Cash Statement version selector

The generated Cash Statement gains an optional Category Tree version selector separate from the Category Tree workspace.

- Default: the active options-selected version.
- Eligible explicit choices: Published, Submitted and Archived.
- Excluded: Draft.
- Selection affects only that generated Cash Statement request and never updates `App.tbOptions`.
- Each choice shows version code, description, status and effective interval.

For every included period end, the service reports:

- whether the selected version's last semantic change occurred after that period end; and
- whether lifecycle history identifies a different eligible version as active at that date.

Warnings are explanatory and list the affected periods. Where an eligible historical version is identifiable, the interface may offer **Select that version**, but it must not switch silently. Missing Cash Code mappings or BI-expression parse failures are errors/warnings against the selected version, never a reason to fall back to active classification.

No equivalent general selector is added to Accounts, Equity Bridge, Tax Hub or other operational workspaces.

## 10. Accessibility, errors and safety

- Version status and Live/Proposal/Historical meaning are exposed as text to screen readers.
- Root workspace cards, version rows and commands are keyboard reachable in a logical order.
- Disabled commands explain why; they are not conveyed only through a disabled HTML button.
- Confirmation dialogs return focus to their invoking control on cancellation.
- Long notes and validation errors wrap without breaking the split pane or mobile viewport.
- Network retry never automatically repeats publish, discard, Cash Code delete or other non-idempotent commands.
- A post whose server state changed is rejected and refreshed; the client does not overwrite newer work.
- Errors distinguish validation, authorisation, immutable state, submission lock, stale predecessor, optimistic concurrency and unexpected failure.

## 11. Implementation sequence and progress

Only one UI step should normally be in progress at a time. Each step must build and pass its bounded tests before the next begins.

### UI Step 0 — Server prerequisite

- Complete EF mappings for version headers, events and version-owned composite keys.
- Implement the one Category Tree workspace/change-control service.
- Expose explicit-version reads and server-calculated capabilities.
- Route every existing mutation path through authoritative commands and concurrency checks.
- Add service and direct-handler tests before changing visible JavaScript behaviour.

**Exit:** the existing UI can call the guarded boundary; Submitted/Archived, stale and locked writes fail even without JavaScript.

### UI Step 1 — Root version workspace

- Rename the visible root to Category Tree without changing `__ROOT__`.
- Add the root-only detail model and responsive versions workspace.
- Add active, Draft and history summaries plus broad warnings.
- Add explicit version selection and URL persistence.

**Exit:** users can inspect and switch among versions without changing the active pointer; all trees are initially read-only through this new selection path.

### UI Step 2 — Version-aware tree reads and controls

- Pass `versionId` through every node, detail, candidate and form read.
- Namespace expansion/selection state by tenant and version.
- Add the persistent version banner.
- Apply server capability results to desktop and mobile controls.
- Preserve the current tree, search, split-pane and embedded-form behaviour.

**Exit:** active, Draft and historical trees render correctly and immutable contexts expose no apparent tree mutation route.

### UI Step 3 — Guarded editing

- Cut create, edit, move, reorder, enable/disable, root and mapping actions over to the service.
- Split persistent Cash Code master actions from version membership.
- Carry and refresh concurrency tokens.
- Standardise conflict, immutable-state and lock feedback.

**Exit:** Published and Draft changes affect only their intended version; every existing handler bypass test is guarded.

### UI Step 4 — Lifecycle commands and review

- Add clone, refresh, validate, deterministic review, publish and discard flows.
- Add exact version/change-note confirmations.
- Add lifecycle event history.
- Follow the new active version after publication.

**Exit:** an administrator can complete the Submitted → Draft → Published workflow and can replace an editable Published predecessor without ambiguity.

### UI Step 5 — Cash Statement selection and warnings

- Add the eligible version source and active default.
- Pass the selected version through TCExports generation.
- Add affected-period, historical-version, missing-mapping and formula-parser warnings.
- Prove that selection never changes the active options pointer.

**Exit:** current statements remain active-by-default and deliberate historical reconstruction works without a general product-wide selector.

### UI Step 6 — Acceptance and evidence

- Extend contract, handler, Playwright and manual desktop/mobile tests.
- Run accessibility and keyboard checks.
- Prove JavaScript-disabled rejection, stale-tab handling and publication retry safety.
- Re-run ordinary Cash Statement, Accounts, Equity Bridge and Tax Hub regressions.

**Exit:** Increment 4 web acceptance evidence is complete for owner review. Hosted deployment and public documentation remain separate authorised work.

## 12. Test matrix

At minimum the automated and manual suites cover:

- default active resolution and explicit URL selection;
- root workspace information and status wording;
- Published editing and immediate active-statement effect;
- Draft isolation, validation, refresh, discard and publication;
- Submitted and Archived rendering and write rejection;
- multiple Drafts without an implied automatic choice;
- root changes and Cash Code reclassification;
- global Cash Code create/delete boundaries;
- current BI expression parsing against active, Draft and historical trees;
- stale version/item tokens and changed predecessor digest;
- submission-lock rejection;
- direct handlers and JavaScript-disabled forms;
- desktop context menu, split pane, keyboard reorder and drag/drop;
- mobile action bar, detail navigation and lifecycle commands;
- version-scoped selection/expansion state;
- active-default and historical Cash Statement generation;
- affected-period and different-active-version warnings;
- absence of Drafts from the Cash Statement selector; and
- no active-pointer mutation from viewing a version or generating a statement.

## 13. Completion criteria

The UI change is complete when:

- the Category Tree root provides the discoverable version-control workspace;
- users can always tell which version they are viewing and whether it is operational;
- switching the workspace never activates a version;
- the retained tree behaves consistently across editable and read-only versions;
- every mutation is authorised, version-explicit, concurrency-checked and server-guarded;
- clone, refresh, validate, review, publish, discard and history flows are complete on desktop and mobile;
- Cash Code master data, version membership and BI expressions are not misrepresented as the same lifecycle content;
- Cash Statement historical selection and warnings meet the bounded contract; and
- the full local acceptance suite passes before any Azure or hosted rollout is considered.
