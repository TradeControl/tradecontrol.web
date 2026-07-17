# Trade Control Reference Architecture

12 July 2026

Module Sources

- Invoice Register
- Subject Browser
- Cash Manager

## Purpose

This document defines the preferred application architecture for Trade Control web modules.

It is the architectural reference for future implementation work.

It explains:

- how modules are structured
- how responsibilities are separated
- how data flows through the application
- how UI workspaces are composed
- how AppServices are used
- how domain and persistence concerns are isolated
- how transactional and reporting workflows should be implemented

This document captures architectural rules and patterns.

It does not prescribe individual screen designs unless those designs are part of an architectural convention.

## 1. Architectural overview

Trade Control is structured as a layered web application with a clear separation between:

- Presentation Layer
- Application Layer
- Domain and Workflow Layer
- Persistence and Integration Layer

The system combines:

- Razor Pages for route ownership and page hosting
- Blazor components for rich interactive workspaces
- shared tree and shell composition patterns
- Bootstrap and custom CSS for responsive workspace layout
- MudBlazor for preferred modern application surfaces and future grid-heavy modules
- Entity Framework Core for database access
- stored procedures and database views for authoritative business behaviour
- application services for orchestration and translation between UI and data

### 1.1 Core architectural principle

The UI must not own business rules.

The UI renders state and raises intents.

Business behaviour remains in:

- AppServices
- domain helper classes
- database procedures
- existing authoritative workflow classes

### 1.2 Architectural style

Trade Control uses a pragmatic layered architecture with service-oriented orchestration.

Trade Control adopts selected Clean Architecture principles while favouring pragmatic workspace-oriented application services over strict domain-centric layering.

It follows these practical rules:

- routes and page composition remain in the web project
- interactive workspaces are component-based
- application services mediate between UI and persistence
- EF models map directly to database tables, views, and function result sets
- legacy business behaviour remains authoritative where already proven
- modules may use either MudBlazor-centric workspaces or bespoke component workspaces depending on module maturity and domain fit
- modules may mix query services and workflow services when behaviour requires orchestration rather than simple CRUD
- mature modules may combine reporting, maintenance, and entry workflows inside one coherent shell where the domain benefits from a single operational workspace

### 1.3 Architectural precedence

Where architectural evidence differs between modules, precedence is given to the prior reference module unless a later module establishes a complementary pattern rather than a conflicting one.

For this document:

- Invoice Register defines the preferred future pattern for transactional workspaces and data grid presentation
- Subject Browser defines the preferred pattern for namespace-driven master-data workspaces, tree navigation, and embedded maintenance flows
- Cash Manager defines additional patterns for operational finance shells, namespace-aware fiscal workflows, mixed reporting-and-entry modules, and spool-based posting orchestration

This means the Cash Manager does not replace either the MudBlazor preference established by the Invoice Register or the tree-workspace conventions established by the Subject Browser.

Instead, it extends the reference architecture with reporting-led operational workspace conventions and additional service segmentation patterns for finance modules.

## 2. Architectural layers

## 2.1 Presentation Layer

The Presentation Layer consists of:

- Razor Pages
- Blazor components
- shared layout files
- CSS modules
- Bootstrap-based workspace composition
- shared tree wrappers and shell components
- MudBlazor UI composition where adopted

Typical responsibilities:

- route entry points
- page hosting
- shell composition
- workspace navigation
- filter input
- rendering trees, tables, forms, panels and dialogs
- forwarding user actions to services
- preserving local UI state
- responsive split-pane behaviour
- embedded editor presentation
- route query-string hydration for initial shell context

The Presentation Layer must not:

- issue direct database queries as a module integration pattern
- implement business calculations
- re-create posting logic
- duplicate stored procedure behaviour
- own authoritative financial settlement logic

### Typical files

- `Pages/.../Index.cshtml`
- `Pages/.../Index.cshtml.cs`
- `Pages/.../*.razor`
- `Pages/Shared/_Layout.cshtml`
- `Pages/Shared/_Navigation.cshtml`
- `wwwroot/css/base.css`
- `wwwroot/css/components/*.css`
- `wwwroot/css/modules/*.css`

### Presentation sub-patterns

Trade Control currently demonstrates three established presentation sub-patterns:

#### Workspace-and-grid pattern

Used by modules such as Invoice Register.

Characteristics:

- shell-hosted transactional workspace
- rich data grids
- toolbar and action surfaces
- strongly task-oriented transitions
- MudBlazor-preferred future presentation style

#### Tree-and-detail pattern

Used by Subject Browser.

Characteristics:

- tree navigation in a left or primary pane
- detail, enquiry, or maintenance surface in a right or embedded pane
- namespace selector driving visibility and context
- stateful shell orchestration over a shared generic tree renderer
- responsive mobile action-bar behaviour

#### Operational finance shell pattern

Used by Cash Manager.

Characteristics:

- left-side account and period navigation
- right-side statement, maintenance, and entry workspaces
- reporting-first landing surface
- in-shell transitions between statement, payment, transfer, asset, and account-maintenance views
- desktop split-pane with mobile action-bar adaptation
- mixed read and write behaviour within one bounded operational module
- filter-driven statement refresh rather than route proliferation

All three patterns are valid architectural references.

## 2.2 Application Layer

The Application Layer is formed by AppServices.

These services coordinate:

- querying
- filtering
- workflow initialisation
- submission actions
- formatting
- lookup loading
- user-authorised operations
- tree loading
- namespace suggestion resolution
- state-refresh support
- payment maintenance orchestration
- workspace-specific posting flows
- period and account selection support

Typical responsibilities:

- expose task-oriented APIs to the shell
- build and execute queries
- prepare editor models
- call authoritative domain or database workflow logic
- enforce application-level authorisation checks
- translate EF or domain data into UI-facing models
- coordinate mixed reporting and entry workflows without leaking persistence details into the UI

The Application Layer should be the only layer that the shell communicates with.

### Typical files

- `AppServices/ServiceCollectionExtensions.cs`
- `AppServices/<Module>/*.cs`
- `AppServices/<Module>/I*.cs`

### Common service categories

Observed patterns include:

- lookup services
- query builders
- query services
- formatting services
- workflow services
- navigation-supporting services
- tree browser services
- enquiry services
- maintenance services
- workspace services
- execution and background services

## 2.3 Domain and Workflow Layer

Trade Control retains important business behaviour in authoritative domain-support classes and database-backed workflows.

Examples include:

- `Data/Invoices.cs`
- `Data/Profile.cs`
- `Data/NodeSettings.cs`
- `Data/Subjects.cs`
- `Data/FinancialPeriods.cs`
- `Data/NodeAdmin.cs`
- `Data/CashAccounts.cs`
- `Data/CashCodes.cs`
- database stored procedures invoked through `NodeContext` or raw SQL

Typical responsibilities:

- executing posting workflows
- determining defaults
- generating identifiers
- handling payment and invoice lifecycle operations
- integrating with host configuration and encryption
- exposing business operations already proven in legacy behaviour
- enforcing namespace operations and relationship semantics
- handling subject and address maintenance actions
- controlling account rebuilds and financial reprocessing
- resolving tax and code defaults
- executing cash posting, transfer, and asset procedures

This layer is authoritative for behaviour.

Where existing domain helpers or stored procedures already implement working behaviour, they must be reused rather than rewritten in UI code.

## 2.4 Persistence and Integration Layer

This layer consists of:

- `NodeContext`
- EF Core table models
- EF Core view models
- EF Core function result models
- stored procedure wrappers
- file system abstractions
- mail and template integration

Typical responsibilities:

- mapping tables, views and functions
- running stored procedures
- loading persisted entities
- saving state
- integrating with the file system and SMTP infrastructure
- exposing read-only reporting views
- providing access to table-backed correction workflows where mutation is permitted

### Typical files

- `Data/NodeContext.cs`
- `Data/NodeContextProc.cs`
- `Models/*.cs`
- `Mail/*.cs`

## 3. Module structure

A Trade Control module should generally be organised around a single cohesive workspace.

## 3.1 Standard module shape

A mature module typically contains:

- page entry point
- shell component
- child components
- UI models
- AppServices
- CSS module
- EF models or reused shared models
- optional domain helpers or workflow adapters

### Preferred structure

- `Pages/<Area>/<Module>/Index.cshtml`
- `Pages/<Area>/<Module>/Index.cshtml.cs`
- `Pages/<Area>/<Module>/<Module>Shell.razor`
- `Pages/<Area>/<Module>/Components/*`
- `Pages/<Area>/<Module>/Models/*`
- `AppServices/<Module>/*`
- `wwwroot/css/modules/<module>.css`

### Additional shared-UI structure

Where a module is built on generic reusable UI primitives, an additional shared wrapper structure is preferred.

Examples:

- `Pages/Shared/Tree/*`
- `Pages/<Area>/Controls/*`
- `Pages/<Area>/<Module>/<Module>Branch.razor`
- `Pages/<Area>/<Module>/<Module>Node.cs`

This allows domain-specific wrappers to sit on top of generic renderers without modifying the shared primitives.

### Operational workspace structure

For hybrid operational modules, a more segmented component set is preferred.

Typical examples include:

- shell
- navigation tree
- detail host
- reporting workspace
- transaction-entry workspace
- maintenance workspace
- workspace-specific model files

This pattern is visible in Cash Manager and is useful where one business area requires:

- read-only reporting
- corrective editing
- maintenance
- new-entry workflows

within one coherent module.

## 3.2 Shell ownership pattern

The shell is the centre of the module.

The shell owns:

- current workspace mode
- filters
- current selection
- mobile/desktop routing within the module
- messages
- loaded result sets
- orchestration of service calls
- branch refresh tokens where applicable
- hosted embedded workflow state
- context menu and action-bar state where applicable
- selected account and period context where applicable
- initial query-string hydration where applicable

Child components should remain as stateless as practical.

They receive:

- data
- selected values
- callbacks

They emit:

- user intents
- changed selections
- requests to navigate or save

This pattern is explicit in the Invoice Register shell, the Subject Browser shell, and the Cash Manager shell.

## 3.3 Component responsibility pattern

Components should be narrow and focused.

Examples of component roles:

- sidebar / filter host
- register grid
- detail grid
- detail panel
- workflow editor
- confirmation panel
- preview surface
- tree branch wrapper
- namespace selector
- enquiry shell
- embedded maintenance panel
- statement workspace
- asset entry workspace
- transfer workspace
- payment workspace
- account maintenance workspace

Components should not:

- query `NodeContext` as a general architectural pattern
- call stored procedures directly
- infer cross-workspace navigation
- own business lifecycle state beyond their local temporary form state
- own namespace mutation rules
- own posting semantics or settlement algorithms

## 3.4 Shared primitive wrapper pattern

If a reusable generic UI primitive exists, it should remain generic.

Domain-specific behaviour should be introduced through wrappers rather than modifications to the shared primitive.

The Subject Browser demonstrates this clearly:

- shared generic tree components remain unaware of Subjects and namespaces
- subject-specific wrappers load data, apply filtering, map node models, and handle mode-sensitive actions

This is a preferred architectural pattern for reusable UI infrastructure.

## 4. Presentation architecture

## 4.1 Route hosting pattern

Razor Pages remain responsible for route entry.

A page typically:

- sets title and view data
- loads shared layout context
- references module CSS
- hosts the Blazor shell component

Example pattern:

- `Index.cshtml` hosts the workspace
- `Index.cshtml.cs` sets view data and page dependencies

This preserves compatibility with the wider application while enabling modern component-based interaction.

## 4.2 Shared layout pattern

Global layout concerns remain in shared Razor files.

These include:

- branding
- navigation
- authentication links
- theme CSS selection
- antiforgery token emission
- shared script and stylesheet registration
- device-aware navbar behaviour

The layout is not module-specific.

It provides the host environment into which modules render.

## 4.3 Mobile and desktop workspace pattern

Modules should support responsive behaviour without splitting business workflows into separate implementations.

The preferred pattern is:

- desktop: multi-pane embedded workspace
- mobile: single-pane navigation model or action-driven condensed workspace

The same shell owns both.

The shell may switch between:

- navigation/filter pane
- workspace pane
- detail pane
- embedded maintenance pane
- statement pane
- action-bar driven mobile detail panes

State must be preserved when moving between mobile views.

This is a major architectural pattern and should be reused.

## 4.4 Embedded maintenance host pattern

The Subject Browser establishes a reusable embedded maintenance pattern.

A module may host editing or maintenance content in an embedded detail context, while still allowing a full route-based page host.

Typical behaviour:

- desktop can host embedded maintenance within the workspace
- mobile can navigate to an embedded or focused maintenance route
- the shell remains responsible for restoring selection and state on return

This pattern is particularly suitable for master-data and tree-driven workspaces.

Cash Manager demonstrates that the same host pattern can also be used for operational maintenance surfaces inside a financial shell.

## 4.5 Device-aware shell behaviour

Device-aware behaviour belongs in the shell and route host, not in duplicated business workflows.

Observed responsibilities include:

- deciding whether to use split panes
- switching between embedded and full-screen detail experiences
- adjusting mobile actions and back behaviour
- preserving selected nodes and filters between contexts
- driving mobile action bars from selected business context
- opening selected detail workspaces from compact mobile launch actions

## 4.6 Query-string shell hydration pattern

The route host may pass selected query-string state into the shell as initial parameters.

Observed examples include:

- selected account code
- selected payment code
- embedded return context

This pattern is useful when:

- deep-linking into a module
- opening a correction workflow directly
- reconnecting a browser session to a meaningful operational context

## 5. MudBlazor usage pattern

MudBlazor is the preferred component library for rich workspaces and especially for future data-grid-oriented modules.

The Subject Browser and Cash Manager do not use MudBlazor, but this does not displace the prior pattern.

## 5.1 Preferred MudBlazor roles

Use MudBlazor for:

- papers and panels
- buttons and icon buttons
- stacks and grids
- tabs
- alerts
- chips
- tooltips
- data grids
- date pickers
- check boxes
- select controls
- text fields

## 5.2 Workspace composition pattern

Common layout pieces include:

- `MudPaper` for bounded work surfaces
- `MudStack` for toolbar and action alignment
- `MudGrid` and `MudItem` for form and summary layout
- `MudTabs` for related secondary datasets
- `MudAlert` for status and readiness messages

## 5.3 DataGrid pattern

MudDataGrid is the preferred grid surface for interactive datasets in future modules and remains the reference pattern established by the Invoice Register.

Typical conventions:

- fixed header for large data sets
- pager enabled
- sortable columns
- filterable columns
- dense row styling
- striped rows
- bordered layout
- footer summaries where appropriate

Grids may use:

- `PropertyColumn` for simple fields
- `TemplateColumn` for icons, actions, links, badges and custom display

Totals are usually calculated in the hosting component from visible or filtered items.

## 5.4 Navigation and actions pattern

Actions are usually presented as:

- top-level buttons in headers or toolbars
- icon buttons within grid rows
- back buttons at workflow boundaries
- mobile action bars where appropriate
- contextual menus for tree or node actions where appropriate

The back action is architecturally significant.

It restores the prior workspace mode rather than redirecting the browser away from the module.

## 5.5 Non-MudBlazor workspace principle

A module may use Bootstrap and bespoke Blazor components where that better matches an existing mature workspace.

However:

- this should not be taken as a rejection of MudBlazor
- future grid-heavy modules should still prefer MudBlazor
- reusable behaviour should be extracted at the shell and service boundaries regardless of UI library

## 5.6 Operational table principle

Where a module uses Bootstrap tables rather than MudBlazor grids, the same architectural expectations still apply:

- clear sorting ownership
- explicit filter ownership
- service-backed reloads
- visible summary context
- local presentation-only calculations where appropriate
- no migration of authoritative business rules into the table component

## 6. AppService pattern

AppServices are the boundary between the shell and the rest of the application.

## 6.1 Dependency injection pattern

Services are registered centrally in:

- `AppServices/ServiceCollectionExtensions.cs`

This keeps module service wiring in one place and avoids scattering registrations.

## 6.2 Service segmentation pattern

A mature module may use several focused services rather than one large service.

Observed examples include:

- `IInvoiceRegisterLookupService`
- `IInvoiceRegisterQueryBuilder`
- `IInvoiceRegisterService`
- `IInvoiceRegisterWorkflowService`
- `IInvoiceFormattingService`
- `ISubjectBrowserService`
- `ISubjectEnquiryService`
- `ICashManagerService`
- `ICashStatementQueryService`
- `ICashStatementPaymentMaintenanceService`
- `ICashPaymentsWorkspaceService`
- `ICashAssetsWorkspaceService`
- `ICashTransfersWorkspaceService`
- `ICashAccountMaintenanceService`
- `ICashNamespaceResolver`

### Preferred responsibilities

#### Lookup service

Loads static or slowly changing options.

Examples:

- years
- periods
- cash codes
- type lists

#### Query builder

Builds composable filtered query expressions against `NodeContext`.

#### Query service

Executes queries and returns aggregated result models for the shell.

#### Formatting service

Applies display-oriented transformations without altering underlying business behaviour.

#### Workflow service

Coordinates multi-step user actions such as:

- create
- edit
- delete
- post
- submit
- preview
- mark-sent

#### Browser service

Loads hierarchical node datasets, detail models, and filter suggestions for tree-driven modules.

#### Enquiry service

Loads read-only related datasets, usually paged, for contextual detail views.

#### Workspace service

Owns the state-loading and task APIs for a specific workspace such as payments, transfers, or assets.

#### Maintenance service

Owns corrective editing and maintenance workflows over existing persisted records, especially where reporting and correction coexist.

#### Namespace resolver service

Maps persistence-level subject and parent keys into displayable or filterable namespace paths for reporting and drill-through scenarios.

## 6.3 Scope and lifetime pattern

Services that need a fresh `NodeContext` for each operation often use:

- `IServiceScopeFactory`

This supports:

- explicit operation scoping
- independent service calls from long-lived Blazor components
- avoiding leaking DbContext lifetime into the shell

Where a scoped `NodeContext` is injected directly into a scoped service, the service must still avoid leaking persistence concerns into the UI boundary.

Cash Manager demonstrates that both patterns can coexist:

- direct scoped-context query and maintenance services
- scope-factory-based workspace services for longer-lived interactive components

## 6.4 Snapshot and projection service pattern

The Subject Browser introduces a useful application-service pattern for master-data browsing:

- build a service-owned snapshot or projection of the relevant domain graph
- serve filtered, paged, and mapped results from that projection
- invalidate and rebuild the snapshot after mutations

This pattern is suitable when:

- read interactions are frequent
- a hierarchical or multi-parent structure must be navigated repeatedly
- the shell requires stable repeated reads without re-querying all relationships on every interaction

## 6.5 Hybrid reporting-and-maintenance service pattern

Cash Manager establishes a reusable service boundary pattern for operational modules:

- one query service owns the read model for the main reporting surface
- one or more workspace services own entry workflows
- one maintenance service owns corrective editing over existing rows
- one shell-level service owns common account, period, and lookup loading

This split is preferable to a monolithic service where a module contains:

- a reporting landing page
- multiple entry workflows
- corrective maintenance surfaces
- account or entity maintenance sub-workspaces

## 7. Data transfer and UI model pattern

Trade Control distinguishes between persistence models and workspace models.

## 7.1 Persistence models

Persistence models map directly to:

- tables
- views
- keyless database projections
- stored procedure result shapes
- function result shapes

These belong in `Models`.

They reflect database structure and are not designed around UI needs.

## 7.2 Workspace models

Workspace models belong in module-local `Pages/.../Models` or module-local shell files where appropriate.

They represent:

- filter state
- workflow editor state
- action result messages
- select options
- result aggregates
- confirmation payloads
- browser node state
- detail panel projections
- workspace summary cards
- row projections for entry and reporting tables

Examples include:

- `InvoiceFilterModel`
- `InvoiceRegisterResult`
- `InvoiceRaiseEditModel`
- `InvoiceSubmitModel`
- `InvoiceWorkflowActionResult`
- `SubjectBrowserDetailModel`
- `SubjectBrowserNode`
- `CashManagerStatementResult`
- `CashManagerPaymentsWorkspaceState`
- `CashManagerTransfersWorkspaceState`
- `CashManagerAssetsWorkspaceState`

This separation is important.

Do not overload EF models to carry UI-only state.

## 7.3 Record and DTO conventions

Simple read-only options are well suited to records.

Examples:

- year options
- select options
- key models
- browser action descriptors
- namespace suggestions
- detail field pairs
- workspace summary cards
- statement rows and grouped projections

Mutable workflow editors are represented as classes.

This allows in-place updates during component editing.

## 7.4 Mapping responsibility

Mapping should happen in the Application Layer.

Prefer:

- projection in queries
- service-level translation
- assembly of workflow models in workflow services
- explicit mapping from domain-aware node models to generic shared UI node models
- row-shaping from SQL-oriented views into shell-oriented reporting models

Avoid pushing mapping into components.

## 7.5 Shared-generic to domain-specific mapping rule

If a shared component consumes a generic model, the module layer must map its domain model into that generic model.

The shared component should not know about:

- subject classes
- invoice semantics
- cash polarity
- namespace identity

This preserves shared-component reusability.

## 8. Query architecture

## 8.1 Query source pattern

Trade Control often queries database views rather than tables for read models.

This is preferred where:

- the database already encapsulates domain joins
- read models are mature
- reporting surfaces depend on SQL-defined calculations

Examples include:

- invoice register views
- detail views
- change log views
- cash statement views
- tax views
- subject statement and virtual/real compatibility views
- transfer lookup and listing views
- cash account summary views

## 8.2 Query builder pattern

Query logic should be centralised and reusable.

A query builder should:

- accept a filter model
- start from the authoritative DbSet or view
- apply conditional filters
- apply sorting
- return `IQueryable<T>` where appropriate

This allows query composition and reduces duplication between summary and detail queries.

## 8.3 Filtering conventions

Filters should be represented explicitly in a filter model or explicit shell state.

Typical filter types:

- period year and month
- date range
- status
- type
- namespace
- text search
- specific selected entity
- cash code
- selected account

The shell owns the active filter state.

The query builder or browser service applies it.

## 8.4 Summary and totals pattern

Where the UI requires totals:

- execute summary queries using the same active filters
- calculate totals in the service layer or component depending on scope
- ensure totals respect filters

For in-grid footer totals over filtered rows, component-local recalculation is acceptable.

For overall workspace totals, service-level querying is preferred.

## 8.5 Tree query pattern

For hierarchical modules, do not load the full structure into the UI tree at once.

Preferred pattern:

- root query
- child query by parent
- filter-aware branch pruning
- paging for large child collections
- suggestion queries separate from tree-node queries

This pattern is established by the Subject Browser.

## 8.6 Reporting-and-entry split query pattern

Cash Manager demonstrates a useful split between:

- authoritative reporting queries over views and persisted rows
- workspace-specific lookups and searches for entry workflows
- targeted corrective loading of one editable record into an editor state

This pattern is preferable where a module combines:

- statement or register-style reporting
- searchable source-item selection
- mutation over a narrow subset of records

## 8.7 Namespace path enrichment pattern

When persisted data stores subject and parent keys separately, reporting services may enrich rows with a displayable namespace path through a dedicated resolver service.

This allows:

- display grouping
- namespace filter matching
- drill-through URL composition

without requiring UI components to reconstruct namespace semantics.

## 9. Workflow architecture

## 9.1 Workflow service orchestration

Multi-step actions belong in workflow services or domain helper orchestration.

Typical flow:

1. shell requests initial model
2. workflow service loads authoritative data
3. shell displays editor or confirmation surface
4. user submits intent
5. workflow service executes authoritative action
6. shell reloads the appropriate workspace and shows result message

## 9.2 Authoritative business behaviour rule

If a workflow already exists in:

- `Invoices`
- `NodeSettings`
- `Profile`
- `Subjects`
- `CashAccounts`
- `CashCodes`
- stored procedures
- legacy proven logic

that behaviour must be called, not reimplemented.

## 9.3 Transaction workflow categories

Observed transaction categories include:

- pending entry creation
- pending entry editing
- posting by entry
- posting by account
- posting all
- invoice header update
- item create/update/delete
- invoice cancellation
- submission preview
- submission send
- mark as sent
- subject creation
- subject deletion
- namespace reparenting
- namespace addition/removal
- address maintenance
- default relationship maintenance
- payment add/update/delete
- posted payment correction
- payment move
- transfer add/post
- asset entry add/post/delete
- account create/update/delete

These should remain task-oriented APIs rather than generic CRUD methods.

## 9.4 Result pattern

Workflow actions should return a standard result object containing:

- success flag
- user-facing message
- optional selected or created key where required

This provides a predictable shell integration pattern.

## 9.5 Preview-before-mutate pattern

The Subject Browser establishes a reusable preview-before-mutate pattern for sensitive master-data operations.

Typical sequence:

1. request preview or plan
2. inspect action code, counts, and user-facing message
3. confirm or cancel
4. execute the actual mutation
5. refresh only the affected workspace state

This is especially suitable for:

- relationship removal
- reparenting
- deletion
- potentially destructive maintenance operations

## 9.6 Spool-and-flush workflow pattern

Cash Manager adds an important financial workflow pattern:

- new financial entries accumulate in an unposted or provisional state
- reporting surfaces may show posted and unposted activity together
- an explicit posting action flushes the provisional spool into authoritative posted state
- posting scope is task-driven, not necessarily tied to route changes

This pattern is suitable for modules where:

- users prepare multiple provisional records
- later confirmation or posting is a distinct business step
- the reporting surface must reflect both current and provisional state

The UI must not implement posting semantics itself.

It should call the authoritative posting workflow.

## 9.7 Correction workflow pattern

Cash Manager shows a reusable correction pattern for posted financial rows:

- the reporting surface opens a dedicated editor state
- corrective actions are segmented by intent, such as edit, move, payment-value update, or delete
- authorisation and period checks are enforced in the service layer
- closed-period corrections trigger more restrictive behaviour and may require rebuilds or elevated confirmation

This is a specialist workflow pattern and should be used carefully for modules with audited financial semantics.

## 10. State management pattern

## 10.1 Shell-local state

State is primarily shell-local.

The shell holds:

- selected workspace mode
- selected entity key
- paging state
- filters
- loaded result sets
- temporary workflow messages
- branch expansion targets
- refresh tokens
- embedded workflow flags
- context menu state
- selected account
- selected period
- mobile action state

This is sufficient for most module interactions.

## 10.2 Workspace mode pattern

A dedicated mode enum should describe the currently visible workspace.

Examples from the Invoice Register include:

- register
- detail grid
- detail panel
- raise list
- raise create
- raise edit
- raise details
- raise delete
- raise post
- update edit
- item create/edit/delete
- submit edit
- submit preview

Examples from Subject Browser include:

- enquiry
- namespace
- subject

Examples from Cash Manager include section-based modes such as:

- statement
- cash accounts
- payments
- assets
- transfers

This makes navigation explicit and testable.

## 10.3 State preservation principle

When moving between workflow surfaces, preserve:

- active filters
- selected entity
- current mobile/desktop context where applicable
- tree focus and detail context where applicable
- selected account and period where applicable

Returning from a child workflow should restore the previous meaningful workspace context.

## 10.4 Refresh-token pattern

For interactive component trees, a monotonically changing refresh token is an effective shell-controlled invalidation mechanism.

Use this when:

- a shared component should re-evaluate branch state
- the shell wants to force a lightweight UI refresh without rebuilding all state manually

## 10.5 URL-backed shell state

The Subject Browser shows that complex shell state can be selectively mirrored into query-string parameters.

Suitable state to mirror includes:

- mode
- selected key
- return key
- embedded state
- filter text
- pending workflow flags

This is useful when:

- restoring context after route transitions
- supporting embedded maintenance navigation
- enabling device-specific transitions without losing shell intent

Cash Manager adds a narrower form of this pattern for initial selection context such as selected account or payment.

## 10.6 Child-workspace local state pattern

In complex operational modules, child workspaces may own temporary state such as:

- editor tabs
- selected source item
- collapsed/expanded entry panels
- split-height adjustments
- confirmation flags
- sort state over already-loaded rows

This is acceptable provided:

- shell-owned business context remains outside the child
- authoritative data reloads still come from services
- local state is presentation-focused rather than business-authoritative

## 11. Validation pattern

Validation is multi-layered.

## 11.1 UI validation

UI validation should cover:

- required user selections
- readiness states
- empty inputs
- basic data type constraints
- mutual exclusivity of local form inputs where obvious

This improves usability but is not authoritative.

## 11.2 Application validation

Application services and workflow helpers should validate:

- required parameters for a workflow
- existence of target entities
- readiness for external actions
- safe fallback behaviour
- operation eligibility under current user context
- namespace path or relationship eligibility where relevant
- same-account and same-type restrictions where relevant
- correction eligibility for posted or period-sensitive rows

## 11.3 Domain and database validation

Authoritative validation belongs in:

- stored procedures
- database constraints
- legacy business helper classes
- domain-level orchestration logic

This is especially important for:

- posting
- numbering
- invoice transitions
- payment generation
- namespace and relationship logic
- tax behaviour
- subject deletion and reparenting semantics
- closed-period financial corrections
- transfer and asset posting workflows
- financial rebuild semantics

## 11.4 Validation principle

Never rely solely on component validation for business correctness.

## 11.5 EditContext and local validation pattern

For Blazor editor components, local `EditContext`-driven validation with explicit message stores is an accepted pattern where the component owns a temporary editor model.

This is appropriate for:

- inline maintenance panels
- embedded detail editors
- component-local input validation before delegating to authoritative workflow logic

## 11.6 Temporal-period validation pattern

Cash Manager adds an important validation category:

- a workflow may need to validate the business period implied by a date field
- date changes can affect whether a row is mutable, postable, or requires administrative override

This validation belongs in the service or deeper, with the component merely surfacing the resulting warning or failure.

## 12. Authorisation pattern

## 12.1 UI restriction

The UI may hide or disable controls based on role or readiness, but this is not sufficient on its own.

## 12.2 Service enforcement

Sensitive actions must be authorised in the Application Layer or deeper.

Example workflow checks include:

- authenticated user required
- managers or administrators may override
- otherwise the acting user must match the invoice owner

## 12.3 Role-aware workflow pattern

The preferred pattern is:

- inspect current principal
- permit privileged roles
- otherwise map external identity to internal user identity
- compare against the authoritative owner of the record

This prevents UI-only security gaps.

## 12.4 Restricted correction pattern

Cash Manager establishes a specific authorisation pattern for sensitive financial corrections:

- ordinary users may be restricted to their own records
- managers may perform broader operational actions
- administrators may override closed-period restrictions or perform destructive corrections

This is a reusable pattern for audited financial workflows.

## 13. Reporting pattern

The Cash Statement and related financial views indicate the preferred reporting architecture.

## 13.1 Read-only reporting principle

Reporting surfaces should prefer:

- read-only projections
- database-defined views
- application-level filtering and formatting
- stable presentation components

Avoid mutating data from reporting surfaces unless the workflow explicitly requires reconciliation or correction.

## 13.2 Reporting workspace pattern

A reporting workspace should typically provide:

- a clear summary header
- period-aware filtering
- data grid or tabular representation
- derived totals
- drill-through or contextual navigation where useful

## 13.3 Financial correctness principle

Calculated financial outputs should come from authoritative SQL views or established business logic rather than re-implemented client-side arithmetic, except for presentational totals over already-loaded rows.

## 13.4 Reporting-led shell pattern

Cash Manager demonstrates that some modules should land on a reporting or statement surface first, with entry and maintenance workflows hanging off that reporting context.

This is suitable where the user’s primary mental model is:

- inspect current position
- filter by operational scope
- then create, correct, or post entries

rather than starting from a blank data-entry form.

## 13.5 Mixed posted-and-provisional reporting pattern

A reporting surface may legitimately combine:

- authoritative posted rows
- provisional or unposted rows
- visible summary totals over both scopes

provided that:

- statuses are visually distinct
- the user understands which rows are provisional
- the authoritative posting workflow remains separate
- the query service defines the composition, not the UI alone

## 13.6 Grouped reporting pattern

Cash Manager adds a useful grouped reporting pattern:

- group rows by a meaningful domain key such as namespace parent, account, or status grouping
- provide subgroup totals
- keep overall summary visible separately

This pattern is suitable for operational review workspaces where raw flat lists are insufficient.

## 14. Transaction pattern

The Invoice Register represents the preferred architecture for transactional workspaces.

## 14.1 Single workspace principle

A transactional module should feel like one coherent workspace even when it contains multiple lifecycle stages.

Examples:

- list
- detail
- create
- edit
- delete
- post
- submit
- preview

## 14.2 Intent-driven component pattern

Transactional components should ask for actions such as:

- save
- post
- delete
- preview
- submit

They should not decide how those actions are fulfilled.

## 14.3 Shell-controlled navigation

The shell decides:

- when to move from grid to detail
- when to open edit mode
- when to return to register mode
- when to reload datasets after actions

## 14.4 Post-action refresh pattern

After any mutating workflow:

- reload the relevant workspace data
- restore the correct mode
- surface a user-facing message

## 14.5 Hybrid transaction pattern

Cash Manager shows that transactional behaviour may be split across multiple task-focused workspaces inside one shell rather than one monolithic editor.

This is suitable where the domain naturally separates into distinct intents such as:

- payment entry
- transfer entry
- asset entry
- account maintenance

while still sharing one operational context.

## 15. Master data pattern

The Subject Browser is the primary master-data architectural reference.

## 15.1 Namespace-centric navigation

Trade Control uses namespace-style subject navigation.

Relevant conventions include:

- parent and child subject relationships
- namespace path display
- namespace suggestion lookups
- browser deep-link construction
- path-specific selection
- support for multiple appearances of the same subject in a DAG-style structure

## 15.2 Hierarchical identity principle

Master data is not treated as flat lists only.

Modules should support:

- hierarchical navigation
- direct lookup
- contextual linking
- namespace-aware filtering

## 15.3 Suggestion and commit pattern

For namespace selection:

- shell requests suggestions from a service
- selector component remains UI-focused
- committed namespace values are translated into subject and parent codes by the service or shell workflow logic

This pattern should be reused for other hierarchical selectors.

## 15.4 Shared tree wrapper pattern

Master-data modules may use a shared generic tree renderer, but domain behaviour must sit in module-specific wrappers.

Preferred split:

- shared tree renders and emits events
- module-specific shell loads nodes and orchestrates workflows
- module-specific node model carries domain semantics
- mapping occurs before the shared component boundary

## 15.5 Embedded maintenance pattern

Master-data modules often need a mixed experience of:

- browse
- inspect
- edit
- create
- delete
- manage relationships

The Subject Browser shows that this can be done successfully inside one coherent shell using:

- embedded panels on desktop
- focused detail flows on mobile
- state-driven transitions rather than route sprawl
- explicit return-node behaviour

## 15.6 Enquiry-in-detail pattern

A master-data detail pane may host related enquiries as subordinate read-only workspaces.

Examples:

- invoices
- payments
- statement

This allows a subject-centric workspace without duplicating reporting modules.

## 15.7 Namespace-aware operational reuse

Cash Manager demonstrates that namespace and subject selection controls should be reusable outside pure master-data modules.

The Subject Browser’s namespace selector pattern can be successfully reused in finance and transaction modules for:

- subject resolution
- contextual grouping
- filter scoping
- deep-link drill-through

## 16. Persistence model conventions

## 16.1 Table model conventions

Table models:

- map closely to SQL schema
- use EF data annotations
- expose navigation properties
- are not used as UI editor models unless there is a very narrow reason

## 16.2 View model conventions

Keyless view models are common.

They are preferred for:

- reporting surfaces
- register grids
- detail grids
- change logs
- projections with pre-joined data
- compatibility views during schema transition

## 16.3 Stored procedure access conventions

Stored procedure access appears in two forms:

- wrappers on `NodeContext`
- direct execution from domain helper classes using `SqlConnection` and `SqlCommand`
- `ExecuteSqlRawAsync` or interpolated SQL for concise cases

This is acceptable where the procedure is authoritative and stable.

## 16.4 Persistence principle

Trade Control does not require all behaviour to be expressed as EF entity manipulation.

Direct stored procedure execution is part of the architecture.

## 16.5 Function-result model pattern

Table-valued-function outputs mapped to keyless models are an established pattern.

Use this when:

- the database defines an authoritative read model
- the result is contextual and read-only
- the UI needs filtered or paged enquiry data without materialising larger entity graphs

## 16.6 Mixed view-and-table mutation pattern

Cash Manager shows an accepted persistence split:

- use SQL views and keyless projections for reporting
- use table-backed entities for corrective mutation
- use domain helpers or stored procedures for posting and destructive workflow operations

This pattern is appropriate where reporting and mutation must coexist without collapsing into a single anemic CRUD model.

## 17. File and template integration pattern

The mail and template subsystem reveals a reusable integration pattern.

## 17.1 Template-driven document generation

Generated communications use:

- database metadata
- file-based HTML templates
- embedded partial templates
- image assignments
- token replacement
- preview and send workflows

## 17.2 Template management responsibilities

Template management is responsible for:

- template discovery
- file existence checks
- assignment lookup
- parse validation
- image binding
- attachment resolution
- usage registration

## 17.3 Mail workflow pattern

Mail sending is layered:

- settings come from `NodeSettings`
- a document or text object is built
- a mail service composes MimeKit content
- SMTP transport is executed centrally

This should remain infrastructure-owned rather than UI-owned.

## 18. Configuration and environment pattern

## 18.1 Runtime configuration access

Runtime options are centralised in helpers such as:

- `NodeSettings`
- `Profile`

These wrap database-backed configuration and user preferences.

## 18.2 Theme selection pattern

Themes are selected in the shared layout through user-aware profile lookup.

This means:

- theming is cross-cutting
- modules should rely on shared theme variables
- module CSS should extend rather than replace the base theme contract

## 18.3 File provider pattern

File-based module features should use `IFileProvider` rather than hard-coded physical assumptions where possible.

## 18.4 Device detection pattern

Device detection is an accepted cross-cutting concern at the page and layout level.

Use it to adapt presentation and shell behaviour, not to fork business logic.

## 19. CSS and layout contract

## 19.1 Base theme contract

Shared visual variables live in:

- `wwwroot/css/base.css`
- theme files such as `wwwroot/css/themes/theme-blue.css`

Modules should consume these variables.

## 19.2 Module CSS contract

Each substantial module may define its own CSS file under:

- `wwwroot/css/modules`

Shared behavioural component CSS may live under:

- `wwwroot/css/components`

Module CSS should focus on:

- layout structure
- sizing rules
- overflow behaviour
- module-specific visual elements

It should not duplicate global styling.

## 19.3 Height and scroll architecture

Workspace modules commonly depend on explicit flex and min-height contracts.

Important patterns include:

- `min-height: 0`
- nested flex containers
- dedicated scroll hosts
- fixed headers with scrolling content bodies
- viewport and header offset CSS variables where required by shell layout

These are architectural, not incidental.

Do not replace them casually.

## 19.4 Split-pane and workspace geometry pattern

The Subject Browser demonstrates an accepted split-pane layout pattern for dense administrative or master-data workspaces.

Typical characteristics:

- left navigation/tree pane
- right detail pane
- drag-resizable gutter on desktop
- alternate mobile layout with stacked navigation and action bar

This is a reusable pattern for non-grid-centric workspaces.

Cash Manager extends this with:

- account-and-period navigation in the left pane
- operational statement and editor workspaces in the right pane
- nested resizable work regions inside a child workspace where justified

## 19.5 CSS variable operational layout pattern

Cash Manager reinforces the use of CSS custom properties for shell geometry and responsive adjustments.

Examples include:

- viewport height alignment
- header offset alignment
- theme-driven action-bar and tree colours

This is a preferred approach for complex responsive workspaces because it keeps layout contracts explicit and composable.

## 20. Navigation architecture

## 20.1 Application navigation

Top-level navigation remains in shared Razor navigation markup.

This means:

- modules register through menu and route structure
- navigation wording remains consistent with domain concepts
- route ownership is stable

## 20.2 In-module navigation

Within a module, navigation is state-driven rather than route-driven for complex workspaces.

That is the preferred pattern for:

- transactional workspaces
- tree-and-detail workspaces
- embedded maintenance modules
- operational finance shells

Use:

- mode enums
- selected keys
- callback-based transitions
- back actions that restore prior context
- selective query-string persistence where useful

rather than many separate pages for each sub-step.

## 20.3 Return-context pattern

When a module opens an embedded or subordinate workflow, it should preserve a return context.

Examples:

- return node key
- selected invoice or subject key
- parent branch key
- previously active mode
- selected account and period

This enables coherent restore behaviour after save, cancel, or delete actions.

## 20.4 Drill-through pattern

Reporting rows may provide direct drill-through into another module where that module is the authoritative home for the entity.

Cash Manager demonstrates this through statement-to-subject drill-through links.

This is preferred over duplicating large volumes of foreign detail inside the current module.

## 21. Error handling and resilience

## 21.1 Logging pattern

Lower layers commonly log through `NodeContext.ErrorLog`.

This is an accepted platform pattern.

## 21.2 Service resilience pattern

Services often:

- catch exceptions
- log them
- return safe fallback values or failure results

This is especially common in configuration and utility services.

## 21.3 UX principle

Where an operation can fail in normal business use, prefer returning a user-readable result message rather than allowing raw exceptions to surface into the shell.

## 21.4 Non-destructive failure principle

For interactive workspaces, failures should avoid collapsing or resetting the user’s current context unless the entity itself is gone.

This is especially important for:

- tree workspaces
- detail panes
- embedded maintenance flows
- paged enquiry views
- operational statement workspaces

## 21.5 Corrective-workflow resilience pattern

In corrective financial workflows:

- keep the current editor state visible where safe
- surface the failure without losing operational context
- only clear the editor after confirmed success or confirmed deletion

This pattern is established by the Cash Manager maintenance surfaces.

## 22. Development rules for future modules

Future modules should follow these rules.

### Rule 1 — Shell owns state

Keep primary workspace state in the shell.

### Rule 2 — Components render and emit intent

Components should not own business workflows.

### Rule 3 — Services mediate all module behaviour

The shell should communicate through AppServices only.

### Rule 4 — Reuse authoritative business behaviour

Do not reimplement proven workflow logic already expressed in stored procedures or domain helpers.

### Rule 5 — Prefer database views for rich read models

Where SQL already defines the read model correctly, consume that view.

### Rule 6 — Use module-local UI models

Do not overload EF entities with UI-only state.

### Rule 7 — Keep route hosting thin

Use Razor Pages for hosting and shell entry, not for workflow sprawl.

### Rule 8 — Preserve mobile and desktop through one shell

Do not fork business behaviour into separate device-specific implementations.

### Rule 9 — Authorise mutating workflows in the service layer

Do not rely on hidden buttons alone.

### Rule 10 — Keep CSS layered

Use base theme variables, module CSS for layout, and avoid duplicating shared design rules.

### Rule 11 — Keep shared UI primitives generic

Wrap shared infrastructure with module-specific adapters rather than injecting domain logic into the shared primitive.

### Rule 12 — Use MudBlazor as the preferred future grid standard

Even where an existing mature module uses bespoke components, future modules needing interactive data grids should prefer MudBlazor unless there is a strong architectural reason not to.

### Rule 13 — Keep reporting, correction, and entry concerns separated in services

If a module combines a reporting landing surface with mutating workflows, split read, correction, and entry responsibilities across focused services.

### Rule 14 — Keep authoritative financial behaviour below the UI

Posting, rebuilds, settlement, temporal validation, and financial corrections must remain service-, domain-, or database-owned.

## 23. Preferred implementation checklist

Before implementing a new Trade Control module, confirm:

- the route host is defined
- the shell is identified
- workspace modes are enumerated
- filter state is modelled
- UI models are separated from EF models
- required AppServices are defined
- authoritative business behaviour has been identified
- database views or procedures have been located
- mobile and desktop workspace behaviour has been planned
- authorisation boundaries are explicit
- module CSS is isolated and based on the shared theme contract
- shared UI primitives, if used, remain generic
- the module presentation pattern is chosen deliberately:
  - MudBlazor workspace-and-grid
  - tree-and-detail
  - operational reporting shell
  - another justified variant consistent with this reference architecture
- if the module mixes reporting and mutation, the service boundary split is explicit
- if the module performs financial posting or correction, authoritative lower-layer workflows are identified up front

## 24. Summary

Trade Control modules should be built as coherent, stateful workspaces hosted in Razor Pages and implemented through Blazor components.

They should:

- use MudBlazor as the preferred standard for rich interactive data-grid-oriented application surfaces
- keep state and navigation in the shell
- delegate all business behaviour to AppServices and authoritative domain/database workflows
- use EF Core table, view, and function models appropriately
- preserve proven business logic
- support both reporting and transactional workflows within a consistent architecture
- rely on shared theming, layout and navigation conventions
- use wrapper layers when building on generic shared UI infrastructure
- support master-data, transactional, reporting, and operational finance modules through consistent shell-driven patterns
- separate reporting, entry, and correction concerns when a module combines them

This reference architecture is intended to be stable enough that future specifications can say:

> Implement this module in accordance with the Trade Control Reference Architecture.

without restating the foundational architectural rules.
