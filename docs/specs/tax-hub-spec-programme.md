# Tax Hub Programme Specification

Trade Control
Accounts Mode Release
Draft 3 – July 2026

## 1. Introduction

Tax Hub is the statutory reporting, reconciliation, and filing workspace of Trade Control. It is an orchestration module, not an accounting module.

It provides a unified interface through which businesses can:

* Review tax liabilities.
* Review statutory accounts.
* Validate tax mappings.
* Reconcile operational and statutory representations of financial data.
* Manage filing obligations.
* Submit returns to HMRC.
* Review submission history and filing status.

Tax Hub forms the final major functional component required for the Accounts Mode release of Trade Control.

## 2.1 Product Context

Trade Control is not an accounting-led application.

The primary financial interface of the system is the Cash Statement, which presents the operational cash position of the business and acts as the principal fiscal surface of the application.

Future MIS releases will extend this operational model through the Company Statement, providing transaction-grained analysis, simulation, and what-if modelling.

Tax Hub serves a different purpose.

It is the compliance layer through which operational financial data is transformed into statutory reporting structures and regulatory submissions.

Users run their business through the Cash Statement.

Users fulfil statutory obligations through Tax Hub.

## 2.2 Business Tax Abstraction

Trade Control treats business taxation as a single conceptual domain.

The distinction between sole trader taxation and company taxation is not represented as separate business processes within Tax Hub.

Both are derived from business profit and both represent statutory taxation of profit.

The applicable filing regime is determined by configuration and influences:

* HMRC mappings.
* HMRC reporting formats.
* HMRC submission protocols.

It does not influence the structure of the Tax Hub workspace.

Tax Hub shall therefore present a single Business Tax workspace that adapts to the configured business tax regime.

## 3. Target Audience

Tax Hub is designed primarily for:

* Sole traders.
* Market traders.
* Self-employed professionals.
* Micro entities.
* Small limited companies.

The target audience typically associates accounting activity with tax compliance rather than financial reporting.

Tax Hub therefore presents statutory information through a compliance-oriented workflow rather than a traditional accounting workflow.

## 4. Programme Objectives

The Tax Hub programme has four objectives.

### Objective 1 – Tax Hub Reporting Workspace

Construct the Tax Hub reporting workspace by refactoring existing statutory reporting functionality into the Reference Implementation and extending the reporting layer to support HMRC-mapped statutory representations derived from Tax Configurator mappings.

### Objective 2 – Submission Logic

Create canonical filing models capable of generating statutory submission payloads from Trade Control accounting data.

### Objective 3 – HMRC Transport Platform

Create a tenant-aware HMRC integration platform capable of authenticating, validating, transmitting, and auditing submissions on behalf of multiple independent businesses.

### Objective 4 – Workflow Integration

Provide a seamless user experience through which users can review, validate, reconcile, and submit statutory returns.

## 5. Architectural Principles

### Behavioural Preservation

Existing accounting calculations are authoritative.

Where functionality already exists within the original Trade Control implementation, business behaviour shall be preserved whilst adopting the reference architecture.

### Separation of Concerns

Accounting presentation, classification, submission modelling, transport services, and user workflows shall remain independent responsibilities.

### Multi-Tenant Design

All filing behaviour shall execute on behalf of the current tenant.

Tenant credentials, obligations, submissions, responses, and audit records shall remain fully isolated.

### Operational First

The Cash Statement remains the primary fiscal interface of Trade Control.

Tax Hub derives statutory reporting and filing information from the underlying accounting model but does not replace the operational role of the Cash Statement.

### Transparent Transformation

Tax Hub shall present both the Trade Control representation and the statutory representation of financial data.

Users shall be able to understand how operational classifications are transformed into statutory classifications through the configured mapping process.

### Deterministic Reconciliation

Trade Control accounting outputs shall remain traceable to their statutory representations.

Tax Hub shall consume existing accounting reconciliation and tax mapping validation services to provide confidence in statutory outputs.

Future implementations may extend these capabilities with additional statutory reconciliation models and submission-level validation services.

Users shall be able to verify the integrity of statutory outputs before submission.

## 6. Tax Classification Layer

Trade Control derives statutory reporting through a deterministic classification model.

Operational transactions are classified through:

* Cash Codes.
* Category hierarchies.
* Reporting groups.
* Jurisdiction-specific tax mappings.

The Tax Configurator provides the authoritative interface for maintaining these mappings.

Tax Hub and HMRC submission services consume the resulting classifications and shall not independently reinterpret accounting data.

## 7. Repository Boundaries

The programme spans two repositories.

### TCWeb Repository

Owns:

* Tax Hub UI.
* MudBlazor components.
* AppServices.
* User workflows.
* Workspace navigation.
* Tenant context.
* Validation surfaces.
* Filing history presentation.
* Submission initiation.

Consumes:

* hmrc_mtd.

### hmrc_mtd Repository

Owns:

* Canonical filing models.
* Payload generation.
* Payload validation.
* Test API services.
* OAuth workflows.
* HMRC transport.
* Fraud prevention services.
* Submission execution.
* Submission auditing.

Contains no UI concerns and remains independent of the reference architecture.

## 8. Compliance Workspaces

Tax Hub shall organise functionality around compliance obligations rather than accounting artefacts.

The following workspaces describe the completed Tax Hub vision.

Individual implementation specifications may deliver only a subset of the capabilities listed below.

### VAT Workspace

Provides:

* Trade Control VAT view.
* HMRC VAT view.
* Mapping validation.
* Reconciliation validation.
* Filing status.
* Submission history.

### Sole Trader Workspace

Provides:

* Trade Control tax view.
* HMRC Self Assessment view.
* Mapping validation.
* Reconciliation validation.
* Filing status.
* Submission history.

Supports both:

* Annual Self Assessment submissions.
* Periodic income reporting obligations.

### Company Workspace

Provides:

* Trade Control accounts view.
* HMRC-tagged accounts view.
* Corporation tax view.
* Mapping validation.
* Reconciliation validation.
* Filing status.
* Submission history.

## 9. Validation and Reconciliation

Validation is a core Tax Hub responsibility.

### Structural Validation

Validates:

* Category mappings.
* Cash code mappings.
* Reporting group assignments.
* HMRC tag assignments.
* Configuration completeness.

### Numerical Validation

Consumes and presents:

* Existing balance sheet reconciliation outputs.
* Existing profit and loss reconciliation outputs.
* Tax calculation validation outputs.
* Tax mapping validation outputs.

Future implementations may introduce additional validation services that reconcile statutory representations directly to their operational source models.

### Submission Validation

Validates:

* Filing readiness.
* Payload completeness.
* Mapping completeness.
* Reconciliation status.

Submission workflows shall clearly indicate PASS, WARN, and FAIL states prior to filing.

---

## 10. End-to-End Architecture

Operational Transactions

→ Accounting Engine

→ Tax Classification Layer

→ Tax Hub

→ Submission Logic

→ HMRC Transport

→ HMRC

→ Submission Response

→ Tax Hub Submission History

## 11. Programme Structure

### Implementation Specification 1

Tax Hub Refactor and Mapping Presentation

* Tax Hub workspace.
* Trade Control reporting views.
* HMRC reporting views.
* VAT reporting.
* Business tax reporting.
* Consumption of existing validation services.
* Consumption of existing reconciliation outputs.
* Engineering Work Plan generation through repository analysis.

### Implementation Specification 2

Submission Logic

* Canonical filing models.
* Payload generation.
* Payload validation.
* Test API services.

### Implementation Specification 3

HMRC Transport Platform

* OAuth.
* Credential management.
* Fraud prevention.
* Sandbox integration.
* Production integration.
* Submission auditing.

### Implementation Specification 4

TCWeb Integration

* Filing workflows.
* Submission status.
* Obligation management.
* Authentication status.
* Submission history.
* User feedback and error presentation.

## 12. Success Criteria

The Tax Hub programme shall be considered complete when:

* Existing statutory reporting functionality has been migrated.
* Trade Control and statutory representations can be presented side-by-side.
* Reconciliation and validation workflows are operational.
* HMRC-compliant payloads can be generated.
* HMRC submissions can be executed through a tenant-aware platform.
* Submission history is available within Tax Hub.
* Tenant isolation is maintained throughout the filing lifecycle.
* The system supports hosted multi-tenant deployment for independent businesses.
