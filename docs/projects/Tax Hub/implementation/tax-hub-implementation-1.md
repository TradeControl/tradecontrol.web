# Tax Hub - Implementation Specification 1

Trade Control  
Accounts Mode Release  
Draft 2 – July 2026

## 1. Purpose

The purpose of this specification is to instruct the coding assistant to investigate the Trade Control repository and produce an Engineering Work Plan for Objective 1 of the Tax Hub programme.

This is not a coding exercise. It is a repository analysis and implementation planning exercise whose purpose is to establish the minimum safe implementation path for Tax Hub within the Accounts Mode release.

The output of this specification shall be an Engineering Work Plan document.

The output of this specification shall not be code.

### 1.1 Engineering Work Plan Construction

The Engineering Work Plan is expected to be developed incrementally during repository investigation.

You are not required to analyse the entire repository before beginning the document.

The recommended approach is to construct the Engineering Work Plan progressively as understanding develops.

For example:

- Investigate a phase.
- Record findings.
- Identify reusable assets.
- Identify dependencies.
- Identify implementation opportunities.
- Update the Engineering Work Plan.
- Continue to the next phase.

The Engineering Work Plan should evolve throughout the investigation process and be refined as additional understanding is acquired.

A phased and iterative approach is preferred to attempting to fully solve the implementation in a single pass.

## 2. Delivery Process

Trade Control development follows a staged AI-assisted engineering process.

The coding assistant shall work through the following documents in sequence:

1. tc-design-principles.md
2. tc-development-contract.md
3. tax-hub-spec-programme.md
4. tax-hub-implementation-1.md
5. tc-reference-architecture.md

These documents establish:

- Product vision
- Design principles
- Architectural conventions
- Development methodology
- Tax Hub programme objectives

The repository investigation shall be performed only after these documents have been analysed.

### 2.1 Required Output

To fulfil Objective 1, you shall investigate the repository and produce:

```text
tax-hub-workplan-1.md
```

The Engineering Work Plan shall describe:

- Current repository implementation
- Existing reusable assets
- Architectural observations
- SQL work packages
- Service work packages
- Application work packages
- UI work packages
- Dependencies
- Risks
- Assumptions
- Recommended implementation sequence

You are free to organise the investigation in whatever manner best supports compliance with the supplied specifications and repository implementation.

### 2.2 Approval Gate

No implementation is required during this specification.

No code generation is required during this specification.

No file modifications are required during this specification.

Upon approval of:

```text
tax-hub-workplan-1.md
```

subsequent implementation specifications will be issued and the approved work plan will be used to guide implementation.

This specification concludes upon delivery of the Engineering Work Plan.

## 3. Objective

Construct the Tax Hub reporting and validation workspace for the Accounts Mode release.

Tax Hub is an orchestration module.

Tax Hub is not an accounting module.

The repository already contains substantial accounting, taxation, reporting, reconciliation and validation functionality.

The objective is to identify how those existing capabilities can be integrated, refactored and extended to create a unified Tax Hub workspace.

The objective is not to redesign the accounting engine.

The objective is not to redesign the Cash Statement.

The objective is not to redesign the Tax Configurator.

The objective is to unify existing capabilities within a coherent user experience and identify the minimum implementation required to support statutory reporting and filing.

## 4. Accounts Mode Scope

This implementation supports:

- Sole Traders
- Self Employed Businesses
- Micro Entities
- Small Limited Companies

The implementation shall prioritise:

- Simplicity
- Operational readiness
- Reuse of existing functionality
- Minimal implementation risk

Future MIS requirements and ERP extensions are outside the scope of this specification.

## 5. Core Architectural Principles

The Engineering Work Plan shall conform to:

- tc-design-principles.md
- tc-development-contract.md
- tc-reference-architecture.md
- tax-hub-spec-programme.md

The following principles shall guide repository analysis.

### 5.1 Tax Hub Is An Orchestration Module

Tax Hub does not perform accounting calculations.

Tax Hub consumes and presents:

- Accounting outputs
- Reporting outputs
- Validation outputs
- Reconciliation outputs
- Filing outputs

Accounting logic remains within existing accounting services and SQL projections.

### 5.2 Business Tax Is Business Model Neutral

Tax Hub shall not treat Sole Trader and Company taxation as separate workspace concepts.

Business Tax represents taxation of profit.

The distinction between:

- Sole Trader
- Self Employed
- Limited Company

is determined by configuration and HMRC submission requirements.

It is not a workspace concern.

Repository investigation shall determine how existing tax-type configuration supports this abstraction.

### 5.3 Reuse Before Replacement

Existing functionality shall be reused wherever practical.

Repository investigation shall identify:

- Existing pages
- Existing services
- Existing SQL views
- Existing validation infrastructure
- Existing reporting infrastructure

before proposing new implementation.

### 5.4 Existing Accounting Validation Remains Authoritative

The Cash Statement already provides:

- Profit and Loss projections
- Balance Sheet projections
- Equity reconciliation
- Accounting validation

Tax Hub shall consume these outputs.

Tax Hub shall not duplicate existing accounting validation logic.

### 5.5 Existing Mapping Validation Remains Authoritative

The Tax Configurator already provides mapping validation functionality.

Tax Hub shall consume and present mapping validation outputs.

Tax Hub shall not duplicate existing mapping validation logic.

### 5.6 TradeControl.Web.Data

TradeControl.Web.Data remains the authoritative data-access layer of the application.

This specification is not a data-layer migration exercise.

Repository investigation shall preserve:

- DbContext configuration
- Entity Framework models
- Existing database infrastructure

Where existing domain-oriented functionality exists within TradeControl.Web.Data, the Engineering Work Plan shall determine whether functionality should:

- Be reused directly
- Be wrapped by App Services
- Be extended where necessary

New Tax Hub functionality should preferentially follow the Reference Architecture.

## 6. Phase-Based Repository Investigation

The Engineering Work Plan shall be developed through the following investigative phases.

These phases are analytical.

They are not implementation phases.

## Phase 1 - Existing Reporting Infrastructure

### Objective

Develop a complete understanding of the existing statutory reporting infrastructure.

### Repository Investigation

Investigate:

#### VAT

- VAT Pages
- VAT Models
- VAT Services
- VAT SQL Views
- VAT Statement generation

#### Accounts

- Profit and Loss reporting
- Balance Sheet reporting
- Losses Carried Forward reporting
- Existing statement construction patterns
- Existing reporting services
- Existing reporting models

#### Tax Reporting

- Existing Company Tax pages
- Existing Company Tax services
- Existing Company Tax SQL dependencies

### Engineering Questions

Determine:

- What reporting assets already exist
- What assets can be reused
- What assets require refactoring
- What assets require replacement
- What assets require extension

## Phase 2 - Tax Mapping Infrastructure

### Objective

Develop a complete understanding of Tax Configurator and mapped statutory reporting.

### Repository Investigation

Investigate:

- tbTaxTag
- tbTaxTagClass
- tbTaxTagMap
- tbTaxTagMapType
- tbTaxTagSource
- tbTaxType
- fnTaxTagMapValidate
- proc_TaxTagMapValidate

Investigate all associated reporting dependencies.

Investigate all supplied account templates.

### Template Investigation

Investigate:

#### Company Templates

- COMIN26
- COSTD26

#### Sole Trader Templates

- STMIN26
- STSTD26

### Engineering Questions

Determine:

- How mappings are constructed
- How mappings are validated
- How mapped totals are derived
- How template-specific outputs are generated
- How HMRC reporting structures can be projected from Trade Control reporting structures

## Phase 3 - Validation And Obligation Infrastructure

### Objective

Develop a complete understanding of validation and filing obligations.

### Repository Investigation

Investigate:

#### Accounting Validation

- Balance Sheet validation
- Equity reconciliation
- Reporting reconciliation

#### Mapping Validation

- Mapping validation services
- Validation outputs
- Validation workflows

#### Tax Types

Investigate:

- Tax Type configuration
- StartMonth
- Recurrence
- OffsetDays

and any associated obligation scheduling infrastructure.

### Engineering Questions

Determine:

- How filing obligations are calculated
- How payment obligations are calculated
- How validation status is determined
- How validation outputs should be surfaced within Tax Hub

## Phase 4 - Target Tax Hub Architecture

### Objective

Develop the target Tax Hub architecture and implementation strategy.

### Workspace Architecture

Determine the implementation requirements for:

#### Dashboard

Including:

- Filing obligations
- Payment obligations
- Validation status
- Submission status
- Tax type indicators

#### VAT Workspace

Including:

- Summary
- Details
- Statement
- Validation

#### Business Tax Workspace

Including:

- Trade Control View
- HMRC View
- Validation

### HMRC Reporting

Determine the SQL views required to generate:

- HMRC company reporting outputs
- HMRC sole trader reporting outputs
- Template-specific projections

### Service Architecture

Determine:

- Existing services that can be reused
- Existing services that require extension
- New services that require creation

### UI Architecture

Determine:

- Workspace structure
- Navigation structure
- Period-selection behaviour
- Component structure

## 7. Repository Scope

The Engineering Work Plan shall be based upon analysis of repository artefacts supplied to the coding assistant.

The repository remains the primary source of truth.

Where specification assumptions conflict with repository implementation:

- Repository implementation shall take precedence.
- The discrepancy shall be documented.
- Recommendations shall be included within the Engineering Work Plan.

## 8. Required Deliverable

The sole deliverable for this specification is:

```text
tax-hub-workplan-1.md
```

The Engineering Work Plan shall include:

## Current State Assessment

Identify:

- Existing assets
- Existing reporting flows
- Existing validation flows
- Existing tax mapping flows
- Existing obligation-calculation flows

### Target Architecture

Define:

- Dashboard architecture
- VAT workspace architecture
- Business Tax workspace architecture
- Service architecture
- SQL architecture

### Work Packages

Define:

- SQL work packages
- Service work packages
- Application work packages
- UI work packages

### Dependencies

Identify:

- Repository dependencies
- SQL dependencies
- Service dependencies
- Architectural dependencies

### Implementation Sequence

Define a recommended implementation order.

### Risks And Assumptions

Document:

- Unknowns
- Assumptions
- Clarifications required
- Potential implementation risks

## 9. Acceptance Criteria

The Engineering Work Plan shall:

- Demonstrate understanding of existing repository assets.
- Demonstrate understanding of existing reporting infrastructure.
- Demonstrate understanding of Tax Configurator infrastructure.
- Demonstrate understanding of validation infrastructure.
- Demonstrate understanding of obligation scheduling infrastructure.
- Maximise reuse of existing functionality.
- Minimise unnecessary implementation.
- Preserve existing accounting and validation behaviour.
- Conform to the Trade Control Reference Architecture.
- Conform to the Development Contract.
- Conform to the Design Principles.
- Produce a practical implementation path for the Accounts Mode release.

The Engineering Work Plan shall be approved before implementation begins.

## Appendix 1 - Aider files

/add docs/specs/tax-hub-spec-implementation-1.md  
/add docs/specs/tax-hub-spec-programme.md  
/add docs/specs/tax-hub-workplan-1.md  
/add docs/specs/tc-design-principles.md  
/add docs/specs/tc-development-contract.md  
/add docs/specs/tc-reference-architecture.md  
/add src/TCWeb/Pages/Tax/Company/Details.cshtml  
/add src/TCWeb/Pages/Tax/Company/Details.cshtml.cs  
/add src/TCWeb/Pages/Tax/Company/Index.cshtml  
/add src/TCWeb/Pages/Tax/Company/Index.cshtml.cs  
/add src/TCWeb/Pages/Tax/Company/LossesCarriedForward.cshtml  
/add src/TCWeb/Pages/Tax/Company/LossesCarriedForward.cshtml.cs  
/add src/TCWeb/Pages/Tax/Company/Statement.cshtml  
/add src/TCWeb/Pages/Tax/Company/Statement.cshtml.cs  
/add src/TCWeb/Pages/Tax/Configurator/Index.cshtml  
/add src/TCWeb/Pages/Tax/Configurator/Index.cshtml.cs  
/add src/TCWeb/Pages/Tax/Configurator/TaxConfigurator.razor  
/add src/TCWeb/Pages/Tax/Configurator/TaxConfiguratorTree.razor  
/add src/TCWeb/Pages/Tax/Configurator/TaxTagDetails.razor  
/add src/TCWeb/Pages/Tax/Vat/Details.cshtml  
/add src/TCWeb/Pages/Tax/Vat/Details.cshtml.cs  
/add src/TCWeb/Pages/Tax/Vat/Index.cshtml  
/add src/TCWeb/Pages/Tax/Vat/Index.cshtml.cs  
/add src/TCWeb/Pages/Tax/Vat/Statement.cshtml  
/add src/TCWeb/Pages/Tax/Vat/Statement.cshtml.cs  
/add src/TCWeb/Pages/Tax/Vat/Periods/Details.cshtml  
/add src/TCWeb/Pages/Tax/Vat/Periods/Details.cshtml.cs  
/add src/TCWeb/Pages/Tax/Vat/Periods/Index.cshtml  
/add src/TCWeb/Pages/Tax/Vat/Periods/Index.cshtml.cs  
/add src/TCWeb/Pages/Tax/Vat/Periods/Index.mobile.cshtml  
/add src/TCWeb/AppServices/ITaxConfiguratorService.cs  
/add src/TCWeb/AppServices/ServiceCollectionExtensions.cs  
/add src/TCWeb/AppServices/TaxConfiguratorModels.cs  
/add src/TCWeb/AppServices/TaxConfiguratorService.cs  
/add src/TCWeb/Pages/Cash/Accounts/BalanceSheet.cshtml  
/add src/TCWeb/Pages/Cash/Accounts/BalanceSheet.cshtml.cs  
/add src/TCWeb/Pages/Cash/Accounts/ProfitAndLoss.cshtml  
/add src/TCWeb/Pages/Cash/Accounts/ProfitAndLoss.cshtml.cs  
/add src/TCWeb/Pages/Cash/Accounts/ProfitAndLossByPeriod.cshtml  
/add src/TCWeb/Pages/Cash/Accounts/ProfitAndLossByPeriod.cshtml.cs  
/add src/TCWeb/Data/NodeContext.cs  
/add src/TCWeb/Data/NodeContextProc.cs  
/add src/TCWeb/Data/NodeEnum.cs  
/add src/TCWeb/Data/NodeSettings.cs  
/add src/TCWeb/wwwroot/css/base.css  
/add src/TCWeb/wwwroot/css/modules/taxConfigurator.css  
/add src/TCWeb/wwwroot/css/themes/theme-blue.css  
/add src/TCWeb/Pages/Admin/Manager/Components/TaxSettingsPanel.razor  
/add ""src/sqlnode/src/tcNodeDb4/Cash/Stored Procedures/proc_TaxTagMapValidate.sql"  
/add src/sqlnode/src/tcNodeDb4/Cash/Functions/fnTaxTagMapValidate.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Functions/fnTaxTypeDueDates.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Tables/tbTaxTag.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Tables/tbTaxTagClass.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Tables/tbTaxTagMap.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Tables/tbTaxTagMapType.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Tables/tbTaxTagSource.sql  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_TaxRates.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_BASE_MIN_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_CO_MICRO_CUR_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_CO_MICRO_CUR_MIN_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_CO_MICRO_CUR_STD_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_CO_MICRO_CUR_STD_EXP_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_DisableVAT.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_YearPeriods.sql"  
/add src/TCWeb/Models/Cash_vwProfitAndLossByPeriod.cs  
/add src/TCWeb/Models/Cash_vwProfitAndLossByYear.cs  
/add src/TCWeb/Models/Cash_vwBalanceSheet.cs  
/add src/TCWeb/Models/Cash_vwTaxBizAuditAccrual.cs  
/add src/TCWeb/Models/Cash_vwTaxBizStatement.cs  
/add src/TCWeb/Models/Cash_vwTaxBizTotal.cs  
/add src/TCWeb/Models/Cash_vwTaxLossesCarriedForward.cs  
/add src/TCWeb/Models/Cash_vwTaxType.cs  
/add src/TCWeb/Models/Cash_vwTaxVatAuditAccrual.cs  
/add src/TCWeb/Models/Cash_vwTaxVatAuditInvoice.cs  
/add src/TCWeb/Models/Cash_vwTaxVatDetail.cs  
/add src/TCWeb/Models/Cash_vwTaxVatStatement.cs  
/add src/TCWeb/Models/Cash_vwTaxVatSummary.cs  
/add src/TCWeb/Models/Cash_vwTaxVatTotal.cs  
