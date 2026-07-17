# Tax Hub - Engineering Work Plan 1

Trade Control  
Accounts Mode Release  
Draft 2 – July 2026

## 1. Purpose

This document defines the Engineering Work Plan for Objective 1 of the Tax Hub programme.

Objective 1 is to construct the Tax Hub reporting and validation workspace for the Accounts Mode release by reusing, integrating, refactoring, and replacing existing Trade Control repository assets where required.

This work plan is based on the supplied repository artefacts and the governing specifications:

- `docs/specs/tc-design-principles.md`
- `docs/specs/tc-development-contract.md`
- `docs/specs/tax-hub-spec-programme.md`
- `docs/specs/tax-hub-spec-implementation-1.md`
- `docs/specs/tc-reference-architecture.md`

This document is both:

- an implementation planning deliverable
- a working instruction set to guide delivery of Objective 1 under review

It is not application code, but it is intended to direct implementation work.

## 2. Delivery Intent

The principal purpose of this work plan is to provide a clear, supervised path to Objective 1 completion.

It is therefore written to support two activities:

1. assistant implementation guidance
2. engineering oversight and approval

This means the document must do more than describe the repository.
It must identify:

- what will be reused
- what will be replaced
- what new module structure will be created
- how deprecated prototypes will be retired
- what sequence of work provides the minimum safe path to completion

## 3. Executive Summary

The repository already contains substantial Tax Hub foundation capability, but the currently visible tax and accounts pages are prototype implementations and are now designated as deprecated for the Accounts Mode release.

The deprecated prototype page groups are:

- `src/TCWeb/Pages/Tax/Vat/*`
- `src/TCWeb/Pages/Tax/Company/*`
- `src/TCWeb/Pages/Cash/Accounts/*`

These pages will be deleted upon completion of the new Tax Hub module.

However, their underlying functionality remains valuable and must be transferred into the new Tax Hub UI and UX.

The repository currently provides:

- VAT totals, period detail, and statement reporting
- business tax totals, statement, and losses carried forward reporting
- tax configuration and tax-tag mapping management
- obligation scheduling primitives via `Cash.tbTaxType` and `Cash.fnTaxTypeDueDates`
- accounting validation and statutory reporting primitives
- data-access integration through `TradeControl.Web.Data.NodeContext` and `NodeContextProc`
- SQL template infrastructure that already embeds jurisdiction-specific tax mapping defaults

The minimum safe implementation path is therefore:

1. preserve the underlying SQL, data access, and validation logic
2. replace the deprecated prototype pages with a new Tax Hub module
3. transfer existing reporting and validation behaviour into the new module UX
4. introduce new Tax Hub services, models, shell, and components in the agreed namespaces
5. retire prototype page routes only after the replacement module reaches behavioural completeness

## 4. Mandated Target Namespaces and Module Skeleton

The new module shall be built in the following namespaces:

- `TradeControl.Web.AppServices.TaxHub`
- `TradeControl.Web.Pages.Tax.Hub`
- `TradeControl.Web.Pages.Tax.Hub.Components`
- `TradeControl.Web.Pages.Tax.Hub.Models`

The skeletal files identified for the module are:

- `src/TCWeb/Pages/Tax/Hub/Index.cshtml`
- `src/TCWeb/Pages/Tax/Hub/TaxHubShell.razor`
- `src/TCWeb/Pages/Tax/Hub/Models/TaxHubResult.cs`
- `src/TCWeb/Pages/Tax/Hub/Models/TaxHubWorkflowModels.cs`
- `src/TCWeb/Pages/Tax/Hub/Components/TaxHubGrid.razor`
- `src/TCWeb/Pages/Tax/Hub/Components/TaxHubSidebar.razor`
- `src/TCWeb/AppServices/TaxHub/ITaxHubService.cs`
- `src/TCWeb/AppServices/TaxHub/TaxHubService.cs`

These files are not expected to be sufficient by themselves.
Additional components, models, and services will be required.

They should be treated as the anchor structure for Objective 1 implementation.

## 5. Current State Assessment

## 5.1 Existing Repository Implementation Overview

The supplied repository evidence shows four major capability areas relevant to Tax Hub:

1. VAT reporting
2. business tax reporting
3. tax mapping and validation
4. accounts and accounting validation reporting

These are currently distributed across:

- Razor Pages in `src/TCWeb/Pages/Tax/*`
- Razor Pages in `src/TCWeb/Pages/Cash/Accounts/*`
- application services in `src/TCWeb/AppServices/*`
- SQL tables, functions, procedures, and templates in `src/sqlnode/src/tcNodeDb4/*`
- EF Core models and mappings in `src/TCWeb/Data/NodeContext.cs`
- procedure wrappers in `src/TCWeb/Data/NodeContextProc.cs`

The repository therefore already contains the domain assets required to build Tax Hub as an orchestration module, but not yet in the final required module structure.

## 5.2 Prototype Page Deprecation Decision

The following page groups are now explicitly deprecated prototypes:

- `src/TCWeb/Pages/Tax/Vat/*`
- `src/TCWeb/Pages/Tax/Company/*`
- `src/TCWeb/Pages/Cash/Accounts/*`

This decision changes the implementation posture.

These page groups are no longer to be treated as long-term UI assets to keep and wrap.
Instead they must be treated as:

- behavioural references
- UX prototypes
- evidence of existing flows
- sources of reusable queries, models, and presentation logic

The final Tax Hub implementation must replace them.

This means the work plan must distinguish between:

- reusable underlying behaviour
- deprecated route/page implementations

## 5.3 Existing VAT Reporting Infrastructure

### Evidence

VAT reporting currently exists in:

- `src/TCWeb/Pages/Tax/Vat/Index.cshtml`
- `src/TCWeb/Pages/Tax/Vat/Index.cshtml.cs`
- `src/TCWeb/Pages/Tax/Vat/Details.cshtml`
- `src/TCWeb/Pages/Tax/Vat/Details.cshtml.cs`
- `src/TCWeb/Pages/Tax/Vat/Statement.cshtml`
- `src/TCWeb/Pages/Tax/Vat/Statement.cshtml.cs`
- `src/TCWeb/Pages/Tax/Vat/Periods/Index.cshtml`
- `src/TCWeb/Pages/Tax/Vat/Periods/Index.mobile.cshtml`
- `src/TCWeb/Pages/Tax/Vat/Periods/Index.cshtml.cs`
- `src/TCWeb/Pages/Tax/Vat/Periods/Details.cshtml`
- `src/TCWeb/Pages/Tax/Vat/Periods/Details.cshtml.cs`

Supporting EF models include:

- `Cash_vwTaxVatTotal`
- `Cash_vwTaxVatSummary`
- `Cash_vwTaxVatDetail`
- `Cash_vwTaxVatStatement`
- `Cash_vwTaxVatAuditAccrual`
- `Cash_vwTaxVatAuditInvoice`

Supporting `NodeContext` DbSets include:

- `Cash_TaxVatTotals`
- `Cash_TaxVatSummary`
- `Cash_TaxVatDetails`
- `Cash_TaxVatStatement`
- `Cash_TaxVatAuditAccruals`
- `Cash_TaxVatAuditInvoices`

### Current behaviour

The current VAT prototype provides:

- VAT totals list with pagination
- statement access
- period drill-down access
- per-period tax-code detail
- adjustment entry points on totals pages for privileged users
- mobile-aware presentation using device detection
- statement view showing due, paid, and balance
- period summary totals by tax code

### Architectural observations

The current VAT pages are deprecated, but the underlying VAT reporting capability is mature and reusable.

Implications:

- the page implementation will be removed
- the VAT queries, view models, data sets, and user flows remain valid
- the new Tax Hub VAT workspace must absorb this behaviour

### Reuse assessment

Reusable:

- VAT SQL views
- EF models
- `NodeContext` datasets
- pagination patterns
- totals/statement/period behavioural flows
- period filtering logic
- mobile behaviour principles

Not retained:

- existing `/Tax/Vat/*` page routes
- current Razor Page composition

Required replacement:

- Tax Hub VAT workspace inside the new module

## 5.4 Existing Business Tax Reporting Infrastructure

### Evidence

Business tax reporting currently exists in:

- `src/TCWeb/Pages/Tax/Company/Index.cshtml`
- `src/TCWeb/Pages/Tax/Company/Index.cshtml.cs`
- `src/TCWeb/Pages/Tax/Company/Details.cshtml`
- `src/TCWeb/Pages/Tax/Company/Details.cshtml.cs`
- `src/TCWeb/Pages/Tax/Company/Statement.cshtml`
- `src/TCWeb/Pages/Tax/Company/Statement.cshtml.cs`
- `src/TCWeb/Pages/Tax/Company/LossesCarriedForward.cshtml`
- `src/TCWeb/Pages/Tax/Company/LossesCarriedForward.cshtml.cs`

Supporting EF models include:

- `Cash_vwTaxBizTotal`
- `Cash_vwTaxBizStatement`
- `Cash_vwTaxBizAuditAccrual`
- `Cash_vwTaxLossesCarriedForward`

Supporting `NodeContext` DbSets include:

- `Cash_TaxBizTotals`
- `Cash_TaxBizStatement`
- `Cash_TaxBizAuditAccruals`
- `Cash_TaxLossesCarriedForward`

### Current behaviour

The current business tax prototype provides:

- totals list with pagination
- period detail
- statement
- losses carried forward reporting
- adjustment entry points on totals list for privileged users
- mobile-aware totals presentation

### Architectural observations

The current business tax pages are deprecated and will be deleted.

Their underlying business behaviour is still required and must be transferred into Tax Hub under the business-model-neutral concept of Business Tax.

Implications:

- “Company” page identity is not part of the final architecture
- business tax reporting flows remain valid
- the new Tax Hub module must expose these flows under Business Tax
- the new module must support future source-driven adaptation for sole trader and company reporting without using separate workspace concepts

### Reuse assessment

Reusable:

- SQL views
- EF models
- totals logic
- statement logic
- losses carried forward logic
- pagination behaviour
- adjustment workflow dependency patterns

Not retained:

- `/Tax/Company/*` routes
- current page composition

Required replacement:

- Business Tax workspace in Tax Hub

## 5.5 Existing Tax Configurator and Mapping Infrastructure

### Evidence

Tax configurator UI and service assets exist in:

- `src/TCWeb/Pages/Tax/Configurator/Index.cshtml`
- `src/TCWeb/Pages/Tax/Configurator/Index.cshtml.cs`
- `src/TCWeb/Pages/Tax/Configurator/TaxConfigurator.razor`
- `src/TCWeb/Pages/Tax/Configurator/TaxConfiguratorTree.razor`
- `src/TCWeb/Pages/Tax/Configurator/TaxTagDetails.razor`
- `src/TCWeb/AppServices/ITaxConfiguratorService.cs`
- `src/TCWeb/AppServices/TaxConfiguratorService.cs`
- `src/TCWeb/AppServices/TaxConfiguratorModels.cs`
- `src/TCWeb/AppServices/ServiceCollectionExtensions.cs`
- `src/TCWeb/wwwroot/css/modules/taxConfigurator.css`

Supporting SQL assets exist in:

- `Cash.tbTaxTag`
- `Cash.tbTaxTagClass`
- `Cash.tbTaxTagMap`
- `Cash.tbTaxTagMapType`
- `Cash.tbTaxTagSource`
- `Cash.fnTaxTagMapValidate`
- `Cash.proc_TaxTagMapValidate`

### Current behaviour

The configurator already provides:

- jurisdiction → source → tag class → tag tree navigation
- source-level validation display
- tag-level category mapping
- tag-level cash code mapping
- mapping enable/disable
- mapping removal
- validation refresh via `Cash.proc_TaxTagMapValidate`
- responsive and embedded workspace behaviour

### Architectural observations

This module is not designated for replacement.
It remains the authoritative mapping maintenance module.

Tax Hub should consume and integrate it, not reproduce it.

### Reuse assessment

Reusable directly:

- configurator route
- service layer
- validation function and procedure
- mapping maintenance UI
- source validation workflow

Needs extension:

- Tax Hub links into configurator
- dashboard mapping-health summaries
- workspace validation cross-navigation

## 5.6 Existing Accounts and Accounting Validation Reporting

### Evidence

Accounts reporting currently exists in:

- `src/TCWeb/Pages/Cash/Accounts/ProfitAndLoss.cshtml`
- `src/TCWeb/Pages/Cash/Accounts/ProfitAndLoss.cshtml.cs`
- `src/TCWeb/Pages/Cash/Accounts/ProfitAndLossByPeriod.cshtml`
- `src/TCWeb/Pages/Cash/Accounts/ProfitAndLossByPeriod.cshtml.cs`
- `src/TCWeb/Pages/Cash/Accounts/BalanceSheet.cshtml`
- `src/TCWeb/Pages/Cash/Accounts/BalanceSheet.cshtml.cs`
- `src/TCWeb/Pages/Cash/Accounts/CashStatement.cshtml`
- `src/TCWeb/Pages/Cash/Accounts/CashStatement.cshtml.cs`

Supporting models include:

- `Cash_vwProfitAndLossByYear`
- `Cash_vwProfitAndLossByPeriod`
- `Cash_vwBalanceSheet`

Supporting `NodeContext` views include:

- `Cash_ProfitAndLossByYear`
- `Cash_ProfitAndLossByMonth`
- `Cash_BalanceSheet`
- `Cash_FlowCategories`
- `Cash_FlowCategoryByPeriods`
- `Cash_FlowCategoryByYears`

### Current behaviour

The current accounts prototypes provide:

- annual profit and loss comparison
- monthly profit and loss comparison
- tax totals within accounts pages
- balance sheet comparison
- category and cash-code drill-down in P&L pages
- export-oriented cash statement generation

### Architectural observations

These pages are also deprecated and will be deleted from the Accounts Mode release.

However, the underlying accounting validation and reporting behaviour remains authoritative and must be consumed by Tax Hub.

Implications:

- the existing accounts pages are behavioural prototypes only
- Tax Hub should absorb the required validation and reporting surfaces
- accounting SQL views and read models remain authoritative
- no accounting logic redesign is required

### Reuse assessment

Reusable:

- accounting SQL views
- comparison logic
- category/cash-code detail logic
- tax-total inclusion logic
- reconciliation/reporting patterns

Not retained:

- `/Cash/Accounts/*` routes
- current page composition

Required replacement:

- Tax Hub validation and reporting surfaces

## 5.7 Existing Tax Type and Obligation Scheduling Infrastructure

### Evidence

Tax type infrastructure exists in:

- `src/TCWeb/Models/Cash_vwTaxType.cs`
- `src/TCWeb/Pages/Admin/Manager/Components/TaxSettingsPanel.razor`
- `src/sqlnode/src/tcNodeDb4/Cash/Functions/fnTaxTypeDueDates.sql`

Supporting data includes:

- `Cash_tbTaxTypes`
- `App_TaxTypes`
- `Cash_vwTaxType`

### Current behaviour

The current implementation provides tax type data containing:

- tax type code
- enabled state
- cash code
- month number
- recurrence
- subject
- offset days

The SQL function `Cash.fnTaxTypeDueDates` calculates due dates from:

- `MonthNumber`
- `RecurrenceCode`
- `OffsetDays`

### Architectural observations

This remains suitable as the underlying obligation scheduling primitive for Tax Hub.

No UI from the current tax settings area is being promoted into Tax Hub directly, but its data contract remains relevant.

### Reuse assessment

Reusable:

- tax type data
- due-date function
- admin configuration dependency

Needs extension:

- obligation summary read models
- Tax Hub dashboard summary service
- filing/payment distinction at service level

## 5.8 Existing Template and Mapped Reporting Infrastructure

### Evidence

Template procedures exist in:

- `proc_Template_BASE_MIN_2026.sql`
- `proc_Template_CO_MICRO_CUR_2026.sql`
- `proc_Template_CO_MICRO_CUR_MIN_2026.sql`
- `proc_Template_CO_MICRO_CUR_STD_2026.sql`
- `proc_Template_CO_MICRO_CUR_STD_EXP_2026.sql`
- `proc_Template_ST_SOLE_CUR_MIN_2026.sql`
- `proc_Template_ST_SOLE_CUR_STD_2026.sql`
- `proc_Template_DisableVAT.sql`

### Current behaviour

The templates already:

- create categories, cash codes, and tax codes
- seed UK MTD source and tags
- seed UK ITSA self-employment sources and tags
- seed default tag mappings
- validate tag mappings via `Cash.proc_TaxTagMapValidate`
- configure VAT enabled or disabled behaviour
- configure business tax behaviour
- adapt outputs by business template

### Architectural observations

This infrastructure remains a major strength and should inform Tax Hub’s statutory projection layer.

Tax Hub should not infer regimes from UI conventions.
It should derive reporting behaviour from configured sources, mappings, and templates.

## 6. Phase Findings

## 6.1 Phase 1 - Existing Reporting Infrastructure Findings

### VAT

Reusable underlying assets:

- VAT SQL views
- EF models
- totals, statement, and period flows
- period filtering and pagination patterns

Deprecated implementation:

- `Pages/Tax/Vat/*`

Replacement target:

- Tax Hub VAT workspace

### Business Tax

Reusable underlying assets:

- business tax totals
- statement
- losses carried forward
- tax audit accrual datasets

Deprecated implementation:

- `Pages/Tax/Company/*`

Replacement target:

- Tax Hub Business Tax workspace

### Accounts and Validation

Reusable underlying assets:

- annual and monthly P&L
- balance sheet
- category detail reporting
- tax totals in accounts views

Deprecated implementation:

- `Pages/Cash/Accounts/*`

Replacement target:

- Tax Hub validation and statutory reporting surfaces

## 6.2 Phase 2 - Tax Mapping Infrastructure Findings

Structures already present:

- `tbTaxTag`
- `tbTaxTagClass`
- `tbTaxTagMap`
- `tbTaxTagMapType`
- `tbTaxTagSource`
- `tbTaxType`
- `fnTaxTagMapValidate`
- `proc_TaxTagMapValidate`

Key conclusions:

- mapping infrastructure is authoritative
- Tax Hub must consume it
- Tax Configurator remains the maintenance module
- Tax Hub should surface mapping health and provide navigation into the configurator

## 6.3 Phase 3 - Validation and Obligation Infrastructure Findings

### Validation

The repository contains the ingredients for Tax Hub validation, but not yet a single module-level Tax Hub validation model.

Validation must aggregate:

- mapping validation
- VAT reporting readiness
- business tax readiness
- accounting/reporting validation

### Obligations

The repository already contains the underlying scheduling primitive:

- `Cash.fnTaxTypeDueDates`

Tax Hub must wrap this in a user-facing obligation model.

## 6.4 Phase 4 - Target Tax Hub Architecture Findings

The repository supports a low-risk Tax Hub implementation using the Reference Architecture.

Recommended architectural style:

- Razor Page route host in `TradeControl.Web.Pages.Tax.Hub`
- Blazor shell in `TradeControl.Web.Pages.Tax.Hub`
- child components in `TradeControl.Web.Pages.Tax.Hub.Components`
- UI models in `TradeControl.Web.Pages.Tax.Hub.Models`
- orchestration services in `TradeControl.Web.AppServices.TaxHub`
- `NodeContext` and `NodeContextProc` retained as authoritative data access
- Tax Configurator integrated rather than replaced
- deprecated prototype pages removed after parity is achieved

## 7. Target Tax Hub Architecture

## 7.1 Route and Namespace Structure

The Tax Hub implementation shall be centred on the new module structure:

- `src/TCWeb/Pages/Tax/Hub/Index.cshtml`
- `src/TCWeb/Pages/Tax/Hub/TaxHubShell.razor`
- `src/TCWeb/Pages/Tax/Hub/Components/*`
- `src/TCWeb/Pages/Tax/Hub/Models/*`
- `src/TCWeb/AppServices/TaxHub/*`

This structure conforms to the Reference Architecture by separating:

- route ownership
- shell composition
- component presentation
- UI-facing models
- application-service orchestration

## 7.2 Dashboard Architecture

The dashboard should provide:

- active tax regimes
- filing obligations
- payment obligations
- validation status
- submission readiness indicators
- navigation into VAT, Business Tax, Validation, and Configurator flows

Recommended cards:

1. VAT
2. Business Tax
3. Mapping Health
4. Accounts Validation

The dashboard is new UI and replaces prototype landing patterns.

## 7.3 VAT Workspace Architecture

The Tax Hub VAT workspace should transfer the prototype VAT behaviour into the new module.

Required sections:

- Summary
- Statement
- Periods
- Validation
- HMRC View

Source behaviour to transfer:

- VAT totals
- VAT statement
- VAT periods
- VAT detail drill-down
- pagination
- adjustment actions
- mobile-aware layout behaviour

The old VAT pages should not survive completion.

## 7.4 Business Tax Workspace Architecture

The Tax Hub Business Tax workspace should transfer the current company-tax prototype behaviour into the new module.

Required sections:

- Summary
- Statement
- Losses Carried Forward
- Validation
- HMRC View

Source behaviour to transfer:

- totals
- statement
- detail
- losses carried forward
- adjustment actions
- pagination

The final workspace must present this as Business Tax, not Company Tax.

## 7.5 Validation Workspace Architecture

The Validation workspace should transfer and unify the prototype accounts-validation and tax-readiness behaviour.

Required sections:

- Mapping validation summary
- VAT readiness
- Business Tax readiness
- Accounting validation
- Reconciliation drill-through

Source behaviour to transfer:

- P&L comparison concepts
- monthly P&L concepts
- balance sheet comparison concepts
- tax totals within accounts views
- category/cash-code detail drill-down patterns where useful

The existing accounts pages should not remain as final UI assets.

## 7.6 Tax Configuration Architecture

The Tax Configurator remains the authoritative maintenance area.

Tax Hub should integrate it by:

- surfacing mapping health on the dashboard
- linking validation issues to the configurator
- preserving configurator as the place where mapping corrections are made

## 7.7 HMRC Reporting Architecture

Tax Hub must support distinct statutory representations.

Recommended SQL approach:

- new HMRC projection views
- source-driven and template-aware
- built from existing tax totals, mappings, and configured tax sources
- read-model oriented only

Tax Hub should present both:

- Trade Control view
- HMRC view

for VAT and Business Tax where supported by the resulting projections.

## 8. Required Module Structure

## 8.1 Required Starting Files

The following files form the agreed starting structure:

- `src/TCWeb/Pages/Tax/Hub/Index.cshtml`
- `src/TCWeb/Pages/Tax/Hub/TaxHubShell.razor`
- `src/TCWeb/Pages/Tax/Hub/Models/TaxHubResult.cs`
- `src/TCWeb/Pages/Tax/Hub/Models/TaxHubWorkflowModels.cs`
- `src/TCWeb/Pages/Tax/Hub/Components/TaxHubGrid.razor`
- `src/TCWeb/Pages/Tax/Hub/Components/TaxHubSidebar.razor`
- `src/TCWeb/AppServices/TaxHub/ITaxHubService.cs`
- `src/TCWeb/AppServices/TaxHub/TaxHubService.cs`

## 8.2 Expected Additional Files

Additional files will likely be required, including:

### Additional components

- `TaxHubDashboard.razor`
- `TaxHubVatWorkspace.razor`
- `TaxHubBusinessTaxWorkspace.razor`
- `TaxHubValidationWorkspace.razor`
- `TaxHubSummaryCard.razor`
- `TaxHubPeriodSelector.razor`
- `TaxHubObligationsPanel.razor`

### Additional models

- dashboard card models
- validation summary models
- obligation summary models
- workspace state models
- HMRC comparison models
- row models for unified grid rendering

### Additional services

- VAT query/orchestration service
- Business Tax query/orchestration service
- Validation summary service
- Obligation summary service
- dashboard service
- navigation or shell-state support service if needed

## 9. Recommended Work Packages

## 9.1 SQL Work Packages

### SQL-1 Obligation summary projections

Create read models for:

- next filing obligation per tax type
- next payment obligation per tax type
- due-date summaries derived from `Cash.fnTaxTypeDueDates`

### SQL-2 Validation summary projections

Create read models that summarise:

- mapping issues by source
- VAT readiness
- Business Tax readiness
- accounting validation indicators

### SQL-3 HMRC VAT projection views

Create HMRC-facing VAT read models derived from:

- `Cash_vwTaxVatTotal`
- `Cash_vwTaxVatSummary`
- mappings and tax sources where required

### SQL-4 HMRC Business Tax projection views

Create HMRC-facing business tax read models for:

- company-style mapped outputs
- sole trader/self-employed mapped outputs
- template-specific source-driven outputs

### SQL-5 Tax source health views

Create summary views for:

- source health
- issue counts
- mapping completeness
- active source indicators

## 9.2 Service Work Packages

### SVC-1 Core Tax Hub service contract

Create:

- `TradeControl.Web.AppServices.TaxHub.ITaxHubService`
- `TradeControl.Web.AppServices.TaxHub.TaxHubService`

This should act as the primary shell-facing orchestration boundary.

### SVC-2 VAT orchestration services

Add services to transfer prototype VAT flows into the new module:

- totals
- statement
- periods
- details
- validation summary
- HMRC comparison

### SVC-3 Business Tax orchestration services

Add services to transfer prototype Company-tax flows into the new module:

- totals
- statement
- losses carried forward
- detail
- validation summary
- HMRC comparison

### SVC-4 Validation summary services

Add services that aggregate:

- mapping validation
- accounting validation
- VAT readiness
- Business Tax readiness

### SVC-5 Obligation summary services

Add services that return:

- next filing obligation
- next payment obligation
- tax-type due context
- active tax regime indicators

### SVC-6 Shell support services

Add additional shell support services only if they simplify:

- workspace selection
- period selection
- summary refresh
- mobile/desktop navigation handling

## 9.3 Application Work Packages

### APP-1 Tax Hub route host

Create:

- `src/TCWeb/Pages/Tax/Hub/Index.cshtml`

Responsibilities:

- route ownership
- title and layout integration
- CSS inclusion
- shell hosting

### APP-2 Tax Hub shell

Create:

- `src/TCWeb/Pages/Tax/Hub/TaxHubShell.razor`

Responsibilities:

- own workspace state
- manage sidebar navigation
- orchestrate dashboard/workspace composition
- manage mobile and desktop transitions
- coordinate service calls

### APP-3 Tax Hub shared UI models

Create:

- `src/TCWeb/Pages/Tax/Hub/Models/TaxHubResult.cs`
- `src/TCWeb/Pages/Tax/Hub/Models/TaxHubWorkflowModels.cs`

These should define the base result and workflow state contracts for the module, with additional models added as needed.

### APP-4 Tax Hub components

Create:

- `src/TCWeb/Pages/Tax/Hub/Components/TaxHubGrid.razor`
- `src/TCWeb/Pages/Tax/Hub/Components/TaxHubSidebar.razor`

and additional child components required to host:

- dashboard content
- VAT content
- Business Tax content
- validation content
- obligation content

### APP-5 Service registration

Update service registration so the new Tax Hub services are wired through the established application-service registration pattern.

### APP-6 Prototype retirement gate

Do not delete deprecated prototype pages until:

- equivalent Tax Hub flows are implemented
- navigation replacement is complete
- critical reporting behaviour is transferred
- validation drill-through routes are replaced or redirected as required

## 9.4 UI Work Packages

### UI-1 Tax Hub sidebar and shell layout

Build a shell-led workspace with:

- sidebar navigation
- dashboard landing
- responsive content panel
- mobile-aware transitions

### UI-2 VAT workspace replacement

Replace the VAT prototype pages with:

- Tax Hub VAT summary
- Tax Hub VAT statement
- Tax Hub VAT period drill-down
- Tax Hub VAT detail presentation

### UI-3 Business Tax workspace replacement

Replace the Company-tax prototype pages with:

- Tax Hub Business Tax summary
- Tax Hub Business Tax statement
- losses carried forward
- period/detail presentation

### UI-4 Validation workspace replacement

Replace the Accounts prototype reporting UI, where needed for Tax Hub scope, with:

- validation summary surfaces
- comparison panels
- readiness states
- drill-through detail surfaces

### UI-5 Configurator integration

Integrate the existing Tax Configurator through:

- links
- status cards
- source validation actions

## 10. Existing Reusable Assets

## 10.1 Assets Reusable Without Change

- `TradeControl.Web.Data.NodeContext`
- `TradeControl.Web.Data.NodeContextProc`
- VAT and business tax EF models
- accounting EF models
- tax mapping tables
- tax validation function and procedure
- tax due-date function
- Tax Configurator service and UI
- tax template procedures
- admin tax settings panel
- SQL TRY/CATCH patterns and existing data-layer behaviour

## 10.2 Assets Reusable With Refactoring or Transfer

- VAT reporting flows from `Pages/Tax/Vat/*`
- business tax reporting flows from `Pages/Tax/Company/*`
- accounts validation/reporting flows from `Pages/Cash/Accounts/*`
- mobile presentation patterns from prototype pages
- pagination logic from prototype pages

These assets are reusable as behaviour, not as final UI structure.

## 10.3 Assets To Be Replaced

The following page groups are to be replaced and later deleted:

- `src/TCWeb/Pages/Tax/Vat/*`
- `src/TCWeb/Pages/Tax/Company/*`
- `src/TCWeb/Pages/Cash/Accounts/*`

## 10.4 Assets Not Recommended For Replacement

- `NodeContext`
- `NodeContextProc`
- tax SQL views
- accounting SQL views
- tax mapping validation procedure
- tax source and mapping structures
- template setup procedures

## 11. Dependencies

## 11.1 Repository Dependencies

Tax Hub depends on:

- `TradeControl.Web.Data`
- tax SQL views and procedures
- accounting SQL views
- tax template setup procedures
- Tax Configurator services and UI

## 11.2 SQL Dependencies

Key SQL dependencies:

- `Cash.tbTaxType`
- `Cash.fnTaxTypeDueDates`
- `Cash.fnTaxTagMapValidate`
- `Cash.proc_TaxTagMapValidate`
- `Cash.tbTaxTag*`
- tax total and statement views
- business tax views
- accounting views
- template procedures

## 11.3 Service Dependencies

Tax Hub depends on:

- existing `TaxConfiguratorService`
- new `TradeControl.Web.AppServices.TaxHub` services
- the established DI registration pattern in `ServiceCollectionExtensions`

## 11.4 Architectural Dependencies

The target implementation depends on:

- Razor Page route hosting
- Blazor shell composition
- shell-owned state
- application-service orchestration
- reuse of authoritative SQL/data-layer behaviour
- controlled replacement of deprecated prototype pages

## 12. Risks

## 12.1 Prototype replacement scope risk

Because three prototype page groups are now to be replaced, the implementation scope is wider than a simple shell wrapper.

Mitigation:

- treat prototype pages as behavioural references only
- transfer behaviour incrementally into the new module
- delete prototypes only at the retirement gate

## 12.2 Business Tax naming and abstraction risk

Current implementation uses “Company”.
Target implementation requires “Business Tax”.

Mitigation:

- keep underlying datasets intact
- change shell and workspace language in the new module
- drive regime-specific behaviour from mappings and configuration

## 12.3 HMRC projection ambiguity risk

The repository provides mapping infrastructure but not final HMRC Tax Hub projections.

Mitigation:

- keep HMRC views as explicit SQL work packages
- avoid hard-coding final statutory shapes into the shell prematurely

## 12.4 Validation-state definition risk

There is no single current Tax Hub validation model.

Mitigation:

- create validation summary models in the new module
- aggregate existing authoritative validation sources rather than redefining them

## 12.5 Retirement timing risk

Deleting prototype pages too early could strand behaviours that have not yet been transferred.

Mitigation:

- enforce an explicit prototype retirement gate
- delete only after parity review

## 13. Assumptions

- The supplied repository artefacts are authoritative for this planning exercise.
- The listed prototype pages are deprecated and will be deleted after successful Tax Hub replacement.
- The underlying SQL, EF models, and validation behaviour remain authoritative and must be preserved.
- Tax Hub for Objective 1 is a reporting and validation orchestration module.
- Submission transport and full filing workflows are outside this specification.
- The mandated namespaces and skeletal files are the approved starting structure for implementation.
- Additional files beyond the skeleton will be required.

## 14. Implementation Sequence

## Step 1 - Establish Tax Hub module structure

Create the new namespaces and skeletal files:

- `TradeControl.Web.AppServices.TaxHub`
- `TradeControl.Web.Pages.Tax.Hub`
- `TradeControl.Web.Pages.Tax.Hub.Components`
- `TradeControl.Web.Pages.Tax.Hub.Models`

Create the agreed shell, route host, base models, and core service contract.

## Step 2 - Build shell, sidebar, and dashboard

Implement:

- `Index.cshtml`
- `TaxHubShell.razor`
- `TaxHubSidebar.razor`
- dashboard layout and shell state

This provides the new module host before transferring prototype functionality.

## Step 3 - Implement core Tax Hub services

Implement:

- `ITaxHubService`
- `TaxHubService`

Then add focused supporting services for:

- VAT
- Business Tax
- validation
- obligations

## Step 4 - Transfer VAT prototype behaviour

Move the current VAT prototype behaviour into the new Tax Hub VAT workspace:

- totals
- statement
- periods
- detail drill-down
- adjustment (link to /Admin/Manager/Index?node=TaxRates)
- pagination
- mobile-aware interaction

At this point the old VAT pages remain present but are functionally superseded.

## Step 5 - Transfer Business Tax prototype behaviour

Move the current Company-tax prototype behaviour into the new Business Tax workspace:

- totals
- statement
- losses carried forward
- detail
- adjustment entry points
- pagination

Rename in UX to Business Tax.

## Step 6 - Transfer validation and accounts prototype behaviour

Move required accounts and validation behaviour into the new Validation workspace:

- P&L comparison concepts
- monthly comparison concepts
- balance sheet concepts
- readiness and reconciliation summaries
- drill-through detail where needed

## Step 7 - Integrate Tax Configurator and mapping health

Expose:

- mapping health on dashboard
- source validation summaries
- links into Tax Configurator

## Step 8 - Add obligation summaries

Implement:

- filing obligation summary
- payment obligation summary
- dashboard due-date surfaces

using `Cash.fnTaxTypeDueDates` and any supporting SQL summaries.

## Step 9 - Add HMRC projection views and comparison panels

Add:

- HMRC VAT view
- HMRC Business Tax view

as read-model-backed comparison panels within Tax Hub.

## Step 10 - Prototype retirement review

Verify that Tax Hub has replaced the required flows.

Then retire:

- `Pages/Tax/Vat/*`
- `Pages/Tax/Company/*`
- `Pages/Cash/Accounts/*`

## 15. Minimum Safe Implementation Path

The minimum safe path for Objective 1 is:

1. create the new Tax Hub module in the agreed namespaces
2. preserve all underlying SQL and data-access behaviour
3. transfer VAT prototype behaviour into Tax Hub
4. transfer Business Tax prototype behaviour into Tax Hub
5. transfer required accounts-validation behaviour into Tax Hub
6. integrate Tax Configurator validation
7. add obligation summaries
8. retire prototype pages only after replacement completeness is achieved

This path maximises reuse while still respecting the explicit decision to replace the prototype pages.

## 16. Acceptance Alignment

This amended work plan satisfies the clarified implementation direction by:

- recognising the prototype pages as deprecated and replaceable
- preserving their underlying functionality as reusable assets
- defining the new Tax Hub namespaces
- anchoring the implementation around the agreed skeletal file structure
- describing the work plan as a practical instruction path to Objective 1 completion
- preserving existing accounting, mapping, and validation behaviour
- following the Reference Architecture module structure

## 17. Final Recommendation

Proceed with Tax Hub as a new replacement module, not a wrapper around the deprecated prototype pages.

Preserve:

- SQL behaviour
- `TradeControl.Web.Data`
- mapping validation infrastructure
- tax type obligation infrastructure
- tax template infrastructure

Replace:

- VAT prototype pages
- Company-tax prototype pages
- Accounts prototype pages

Implement the new module in:

- `TradeControl.Web.AppServices.TaxHub`
- `TradeControl.Web.Pages.Tax.Hub`
- `TradeControl.Web.Pages.Tax.Hub.Components`
- `TradeControl.Web.Pages.Tax.Hub.Models`

Use the agreed skeletal files as the starting frame, then add the additional services and components required to transfer all necessary behaviour into the final Tax Hub UX.

That is the minimum safe path to Objective 1 completion for the Accounts Mode release.
