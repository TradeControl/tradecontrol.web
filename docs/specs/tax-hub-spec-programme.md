# Tax Hub Programme Specification  

Trade Control  
Accounts Mode Release  

Draft 4 — August 2026  

## 1. Introduction

Tax Hub is the statutory reporting, reconciliation, and filing workspace of Trade Control.  
It is an orchestration module, not an accounting module.

Tax Hub provides a unified interface through which businesses can:

- Review tax liabilities  
- Review statutory accounts  
- Validate tax mappings  
- Reconcile operational and statutory representations  
- Manage filing obligations  
- Generate HMRC submission payloads  
- Submit returns to HMRC  
- Review submission history  

Tax Hub forms the final major functional component required for the Accounts Mode release.

## 2.1 Product Context

Trade Control is not an accounting-led application.

The primary financial interface is the **Cash Statement**, which presents the operational cash position of the business.

Tax Hub transforms operational financial data into statutory reporting structures and regulatory submissions.

Users run their business through the Cash Statement.  
Users fulfil statutory obligations through Tax Hub.

## 2.2 Business Tax Abstraction

Trade Control treats business taxation as a single conceptual domain.

The distinction between sole trader taxation and company taxation is represented only through configuration and HMRC endpoint selection.

Tax Hub presents a single Business Tax workspace that adapts to the configured regime.

## 3. Target Audience

Tax Hub is designed for:

- Sole traders  
- Market traders  
- Self-employed professionals  
- Micro entities  
- Small limited companies  

The target audience associates accounting activity with tax compliance rather than financial reporting.

Tax Hub therefore presents statutory information through a compliance-oriented workflow.

## 4. Programme Objectives (Updated)

The Tax Hub programme now has **five** objectives.

### Objective 1 — Tax Hub UI (Complete)

Construct the reporting workspace and HMRC-aligned statutory views.

### Objective 2 — Submission Logic (Updated)

Generate **internal test harness payloads** from Trade Control accounting data.  
These payloads are **not HMRC payloads**.  
They are raw tag sets used for development and testing.

### Objective 3 — HMRC API (New)

Define the HMRC API suite:

- Endpoint catalogue  
- Versioning  
- Payload schemas (JSON + XML)  
- CT600 XML schema  
- iXBRL attachment rules  
- Sandbox vs production behaviour  

This objective produces the **HMRC payload specification**.

### Objective 4 — HMRC Transport Platform (Updated)

Implement the HMRC transport layer:

- OAuth  
- Fraud headers  
- JSON/XML transmission  
- Attachment handling  
- Logging  
- Error semantics  
- Environment selection  

### Objective 5 — Workflow Integration (Updated)

Integrate Objectives 2–4 into the Tax Hub UI:

- Filing workflows  
- Submission status  
- Obligation management  
- Authentication status  
- Submission history  
- User feedback and error presentation  

## 5. Architectural Principles

### Behavioural Preservation

Existing accounting calculations remain authoritative.

### Separation of Concerns

UI, submission logic, HMRC API, transport, and workflow integration remain independent.

### Multi-Tenant Design

All filing behaviour executes on behalf of the current tenant.

### Operational First

The Cash Statement remains the primary fiscal interface.

### Transparent Transformation

Tax Hub presents both operational and statutory representations.

### Deterministic Reconciliation

Statutory outputs remain traceable to operational data.

## 6. Tax Classification Layer

Operational transactions are classified through:

- Cash Codes  
- Category hierarchies  
- Reporting groups  
- Jurisdiction-specific tax mappings  

Tax Hub consumes these classifications and does not reinterpret accounting data.

## 7. Repository Boundaries

### TCWeb Repository

Owns:

- Tax Hub UI  
- MudBlazor components  
- User workflows  
- Filing history  
- Submission initiation  

Consumes:

- hmrc_mtd

### hmrc_mtd Repository

Owns:

- Test harness payload models  
- Test harness payload builders  
- Test API services  
- HMRC payload builders (Objective 3)  
- HMRC transport (Objective 4)  
- Fraud prevention  
- Submission execution  
- Submission auditing  

Contains no UI concerns.

## 8. Compliance Workspaces

Tax Hub shall organise functionality around compliance obligations rather than accounting artefacts.

The following workspaces describe the completed Tax Hub vision.

Individual implementation specifications may deliver only a subset of the capabilities listed below.

### VAT Workspace

Provides:

- Trade Control VAT view.
- HMRC VAT view.
- Mapping validation.
- Reconciliation validation.
- Filing status.
- Submission history.

### Sole Trader Workspace

Provides:

- Trade Control tax view.
- HMRC Self Assessment view.
- Mapping validation.
- Reconciliation validation.
- Filing status.
- Submission history.

Supports both:

- Annual Self Assessment submissions.
- Periodic income reporting obligations.

### Company Workspace

Provides:

- Trade Control accounts view.
- HMRC-tagged accounts view.
- Corporation tax view.
- Mapping validation.
- Reconciliation validation.
- Filing status.
- Submission history.

## 9. Validation and Reconciliation

Validation is a core Tax Hub responsibility.

### Structural Validation

Validates:

- Category mappings.
- Cash code mappings.
- Reporting group assignments.
- HMRC tag assignments.
- Configuration completeness.

### Numerical Validation

Consumes and presents:

- Existing balance sheet reconciliation outputs.
- Existing profit and loss reconciliation outputs.
- Tax calculation validation outputs.
- Tax mapping validation outputs.

Future implementations may introduce additional validation services that reconcile statutory representations directly to their operational source models.

### Submission Validation

Validates:

- Filing readiness.
- Payload completeness.
- Mapping completeness.
- Reconciliation status.

Submission workflows shall clearly indicate PASS, WARN, and FAIL states prior to filing.

## 10. End-to-End Architecture

Operational Transactions  
→ Accounting Engine  
→ Tax Classification Layer  
→ Tax Hub  
→ Submission Logic (Objective 2)  
→ HMRC API (Objective 3)  
→ HMRC Transport (Objective 4)  
→ HMRC  
→ Submission Response  
→ Tax Hub Submission History  

## 11. Programme Structure (Updated)

### Implementation Specification 1  

Tax Hub Refactor and Mapping Presentation

### Implementation Specification 2  

Submission Logic (Test Harness Payloads)

### Implementation Specification 3  

HMRC API (Endpoint + Payload Specification)

### Implementation Specification 4  

HMRC Transport Platform

### Implementation Specification 5  

TCWeb Workflow Integration

## 12. Success Criteria

The programme is complete when:

- Statutory reporting is migrated  
- Operational and statutory representations can be compared  
- Validation and reconciliation workflows are operational  
- HMRC-compliant payloads can be generated (Objective 3)  
- HMRC submissions can be executed (Objective 4)  
- Submission history is available  
- Tenant isolation is maintained  
- Multi-tenant deployment is supported  

**End of document.**
