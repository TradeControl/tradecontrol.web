# Tax Hub Programme Specification

Trade Control  
Accounts Mode Release

**Draft 5 — August 2026**

## 1. Introduction

Tax Hub is the statutory reporting, reconciliation, and filing workspace of Trade Control.  
It is an orchestration module, not an accounting module.

Tax Hub provides a unified interface through which businesses can:

- Review tax liabilities
- Review statutory accounts
- Validate tax mappings
- Reconcile operational and statutory representations
- Manage filing obligations
- Generate statutory submission data
- Generate HMRC submission payloads
- Submit returns to HMRC
- Review submission history

Tax Hub forms the final major functional component required for the Accounts Mode release.

---

## 2.1 Product Context

Trade Control is not an accounting-led application.

The primary financial interface is the **Cash Statement**, which presents the operational cash position of the business.

Tax Hub transforms operational financial data into statutory reporting structures and regulatory submissions.

Users run their business through the Cash Statement.  
Users fulfil statutory obligations through Tax Hub.

Tax compliance is therefore a projection from the operational accounting model. It must not redefine or duplicate the underlying accounting calculations.

---

## 2.2 Business Tax Abstraction

Trade Control treats business taxation as a single conceptual domain while recognising that different statutory regimes expose materially different reporting and submission contracts.

The distinction between sole trader taxation and company taxation is represented through configuration, statutory projection, submission contract, and endpoint selection.

Tax Hub presents a single Business Tax workspace that adapts to the configured regime.

Shared accounting concepts may be reused where their semantics are genuinely equivalent. Statutory fields, Tax Tags, payload structures, workflows, and transport mechanisms must not be treated as interchangeable merely because they describe similar financial concepts.

---

## 2.3 Self Assessment Product Policy

Trade Control supports Self Assessment submission through **Making Tax Digital for Income Tax (MTD ITSA)**.

Legacy SA100/XML Self Assessment submission is not supported.

Trade Control does not maintain a parallel legacy Self Assessment submission implementation for users who are exempt from, ineligible for, or unwilling to use MTD ITSA. Such users must use another submission method.

This is a deliberate product boundary.

It does not alter the underlying accounting capabilities of Trade Control and does not imply that all HMRC legacy XML services are deprecated or unsupported. Other statutory regimes may continue to require XML-based submission mechanisms.

---

## 3. Target Audience

Tax Hub is designed for:

- Sole traders
- Market traders
- Self-employed professionals
- Micro entities
- Small limited companies

The target audience associates accounting activity with tax compliance rather than financial reporting.

Tax Hub therefore presents statutory information through a compliance-oriented workflow.

The initial Sole Trader implementation also establishes the statutory projection and submission architecture that will subsequently support company taxation and statutory accounts.

---

## 4. Programme Objectives

The Tax Hub programme has **five** objectives.

### Objective 1 — Tax Hub UI (Complete)

Construct the reporting workspace and HMRC-aligned statutory views.

### Objective 2 — Submission Logic (Reopened: Contract Alignment)

Generate **internal statutory projection and test-harness payloads** from Trade Control accounting data.

These payloads are **not HMRC wire payloads**.

Objective 2 owns:

- Extraction of authoritative accounting values
- Tax classification and Tax Tag projection
- Deterministic mapping from operational classifications
- Internal submission-domain representations
- Test-harness payload generation
- Structural and numerical validation of those representations
- Traceability from projected statutory values back to their accounting sources

Objective 2 must expose sufficient truthful information to satisfy the statutory contracts defined by Objective 3.

Objective 2 must not invent accounting precision or statutory values that cannot be deterministically obtained from Trade Control data or legitimate workflow/contextual input.

The original Objective 2 implementation was completed against an earlier Self Assessment model. The objective has been reopened where necessary to align its Self Assessment projections with the current MTD ITSA contract and the MTD-only product policy.

### Objective 3 — HMRC API (In Progress)

Define the authoritative HMRC-facing contract suite.

Objective 3 owns:

- Endpoint catalogue
- HMRC API/service versioning
- Request and response schemas
- Exact JSON and XML wire contracts
- Mapping from internal statutory projections to HMRC payloads
- Required, optional, conditional, contextual, and derived field semantics
- Corporation Tax XML schemas
- CT600 requirements
- iXBRL attachment rules
- Sandbox and production contract differences

Objective 3 produces the **HMRC payload specification**.

Current authoritative HMRC specifications and associated statutory artefacts govern this boundary.

Existing Trade Control classes, serializers, test harnesses, SQL tags, mappings, and historical implementations are evidence of prior implementation intent. They are not authoritative where they conflict with the current external statutory contract.

### Objective 4 — HMRC Transport Platform

Implement the machinery required to transmit and receive the contracts defined by Objective 3.

#### Modern Transport

Where required by the relevant HMRC service:

- OAuth
- Fraud-prevention headers
- REST submission
- JSON transmission
- Response handling

#### XML-Based Transport

Where required by the relevant HMRC service:

- XML envelope construction
- XML canonicalisation
- IRmark generation
- Transaction Engine submission
- XML receipt parsing
- Attachment handling, including iXBRL where applicable

#### Shared Transport Concerns

- Logging
- Error semantics
- Environment selection
- Submission auditing
- Authentication state
- Retry and transport-failure handling where applicable

Objective 4 does not define statutory accounting semantics or HMRC payload contents. It transports the contracts established by Objective 3.

Legacy SA100/XML Self Assessment is not a supported Objective 4 workflow.

### Objective 5 — Workflow Integration

Integrate Objectives 2–4 into the Tax Hub UI:

- Filing workflows
- Submission status
- Obligation management
- Authentication status
- Submission history
- User feedback and error presentation

---

## 5. Architectural Principles

### Behavioural Preservation

Existing Trade Control accounting calculations remain authoritative for Trade Control accounting behaviour.

### External Contract Authority

For statutory schemas, filing requirements, API contracts, transport protocols, and other externally governed interfaces, the current authoritative specification published by the responsible authority takes precedence over existing Trade Control implementation.

Existing code is not evidence that an external contract remains valid.

Where an external contract has changed, Trade Control shall adapt at the statutory projection, contract, or transport boundary without rewriting authoritative accounting behaviour unless a separately identified accounting defect requires correction.

### Separation of Concerns

Accounting, UI, statutory projection, HMRC contract definition, transport, and workflow integration remain independent concerns.

### Multi-Tenant Design

All filing behaviour executes on behalf of the current tenant.

### Operational First

The Cash Statement remains the primary fiscal interface.

### Transparent Transformation

Tax Hub presents operational and statutory representations without obscuring the transformation between them.

### Deterministic Reconciliation

Every statutory value derived from Trade Control accounting data must remain traceable to its operational source.

### No Invented Precision

A statutory field must not be populated by inference merely because Trade Control contains a broader accounting value with a similar description.

Where the accounting classification cannot deterministically provide the required statutory distinction, that distinction is unsupported until supplied through a legitimate source or the accounting model is deliberately extended.

### Contract Isolation

HMRC wire-format concerns must not leak backwards into the accounting model.

Tax Tags and internal statutory projections represent stable business/statutory semantics. Exact endpoint payload shape, protocol metadata, authentication, and transport concerns belong to later boundaries.

---

## 6. Tax Classification and Statutory Projection Layer

Operational transactions are classified through:

- Cash Codes
- Category hierarchies
- Reporting groups
- Jurisdiction-specific tax mappings

Tax Hub consumes these classifications and does not reinterpret accounting transactions.

The statutory projection layer transforms available accounting classifications into Tax Tags or equivalent internal statutory values.

The Category Tree is a Trade Control business classification structure. It is not an HMRC taxonomy.

Businesses may customise their classification structures. Bootstrap templates provide useful reference classifications but do not define statutory truth.

A mapping is valid only where the available accounting classification deterministically supplies the statutory meaning of the target value.

Mappings must therefore account for:

- Category roll-ups
- Cash Code granularity
- Overlap and double counting
- Optional statutory fields
- Contextual values
- Derived values
- Unsupported distinctions

Absence of a legitimate source must remain explicit. It must not be converted into a speculative value or an artificial zero unless the governing statutory contract specifically requires zero.

---

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
- SQL Node statutory/accounting surfaces

### sqlnode Repository

Owns:

- Accounting schema and bootstrap
- Cash Codes and Category structures
- Tax Sources
- Tax Tags
- Deterministic accounting-to-tax mappings
- SQL statutory projection surfaces
- SQL-side mapping and reconciliation support

### hmrc_mtd Repository

Owns:

- Internal test-harness payload models and builders
- HMRC request and response contracts
- HMRC payload builders
- HMRC API definitions
- HMRC transport
- Fraud-prevention support
- Submission execution
- Submission auditing

Contains no UI concerns.

Repository ownership does not establish statutory authority. HMRC-facing implementation within `hmrc_mtd` must conform to the authoritative contracts established under Objective 3.

---

## 8. Tax Hub Workspace

Tax Hub is a single compliance workspace.

It does not provide separate application workspaces for sole traders, companies, VAT, Corporation Tax, or other tax regimes.

The available Tax Hub surfaces are determined by the configuration of the current node, including:

- Business type
- Tax Types
- Tax Sources
- Cash Codes
- Category structures
- Tax Tags
- Statutory mappings
- Filing obligations

The same Tax Hub workspace therefore adapts to the statutory obligations of the configured business.

For example, a limited company may expose:

- VAT
- Corporation Tax
- Statutory accounts

A sole trader may expose:

- VAT, where registered
- MTD Income Tax Self Assessment

These differences are configuration and statutory-projection differences, not separate workspace architectures.

### Business Tax

Trade Control treats business tax as a financial consequence of trading activity.

The accounting engine remains responsible for establishing the operational financial result. Tax Sources and Tax Tags identify the statutory components required by the configured tax regime and map them deterministically to the available accounting classifications.

Where the applicable tax regime can be deterministically calculated from information available to Trade Control, the resulting tax liability forms part of the normal forward financial calculation.

Corporation Tax is treated in this way. The calculated liability may be projected through the accounting periods and incorporated into cash and production scheduling.

Where the definitive tax liability depends upon information or calculations outside the Trade Control business model, Trade Control may instead maintain an estimated liability.

Sole Trader Income Tax is treated in this way. Trade Control deterministically calculates the business accounting result and the statutory business information required for MTD ITSA, while the resulting personal Income Tax liability may remain an estimate until an authoritative calculation becomes available.

### Period Adjustments

Tax calculations may be reconciled by period adjustment when the definitive liability becomes known.

This is an existing Trade Control accounting mechanism and is not specific to Self Assessment.

VAT and Corporation Tax calculations may already produce small period adjustments where the final statutory liability differs from the accumulated calculated value.

The same mechanism applies to Sole Trader Income Tax:

**Business activity**  
→ **estimated tax liability**  
→ **cash and production forecast**  
→ **MTD submission and authoritative calculation**  
→ **period adjustment**  
→ **revised actual liability**

The adjustment represents the difference between the liability already provided for by Trade Control and the authoritative liability established through the statutory process.

The resulting adjustment feeds back into the normal accounting, cash forecasting, and production-scheduling mechanisms.

Trade Control therefore does not need to reproduce a personal Income Tax calculation that cannot be derived deterministically from the business model. It needs to maintain a useful estimate during the period and reconcile that estimate when the authoritative liability becomes available.

### Tax Hub Responsibilities

Tax Hub:

- presents the applicable statutory representation;
- validates mappings and reconciliations;
- manages filing obligations;
- generates submission data;
- receives and records authoritative submission outcomes;
- initiates any required reconciliation between estimated and actual tax liabilities.

The workspace must not duplicate the underlying accounting engine merely because different statutory regimes consume its outputs differently.

---

## 9. Validation and Reconciliation

Validation is a core Tax Hub responsibility.

### Structural Validation

Validates:

- Category mappings
- Cash Code mappings
- Reporting group assignments
- Tax Tag assignments
- Configuration completeness
- Mapping overlap and double counting
- Compatibility between internal statutory projections and Objective 3 contracts

### Numerical Validation

Consumes and presents:

- Existing balance sheet reconciliation outputs
- Existing profit and loss reconciliation outputs
- Tax calculation validation outputs
- Tax mapping validation outputs

Additional validation services may reconcile statutory representations directly to their operational source models.

### Submission Validation

Validates:

- Filing readiness
- Payload completeness
- Mapping completeness
- Reconciliation status
- Required contextual information
- Contract-specific prerequisites

Submission workflows shall clearly indicate PASS, WARN, and FAIL states prior to filing.

Passing structural mapping validation does not by itself prove statutory correctness. Semantic mapping, numerical reconciliation, and Objective 3 contract validation remain independently required.

---

## 10. End-to-End Architecture

Operational Transactions  
→ Accounting Engine  
→ Tax Classification Layer  
→ Statutory Projection / Tax Tags  
→ Tax Hub  
→ Submission Logic (Objective 2)  
→ HMRC Contract Adapter (Objective 3)  
→ HMRC Transport (Objective 4)  
→ HMRC  
→ Submission Response  
→ Tax Hub Submission History

The principal authority boundaries are:

**Trade Control accounting**  
governs operational financial truth.

**Objective 2 statutory projection**  
governs deterministic transformation of that truth into statutory concepts.

**Objective 3 HMRC contract**  
governs exact HMRC-facing representations.

**Objective 4 transport**  
governs transmission and receipt.

No later layer may silently reinterpret the financial meaning established by an earlier layer.

---

## 11. Programme Structure

### Implementation Specification 1

Tax Hub Refactor and Mapping Presentation

### Implementation Specification 2

Submission Logic and Internal Statutory Projections

### Implementation Specification 3

HMRC API and Payload Contracts

### Implementation Specification 4

HMRC Transport Platform

### Implementation Specification 5

TCWeb Workflow Integration

Implementation specifications and historical work plans record the requirements applicable when they were produced.

Where the programme specification or a later reviewed specification changes an architectural decision, completed implementation history remains historically valid but must not override the current governing specification.

---

## 12. Current Programme Position

### Objective 1

Complete.

### Objective 2

Previously completed against an earlier statutory model.

Reopened for bounded alignment of Self Assessment submission logic with:

- MTD-only Self Assessment product policy
- Current MTD ITSA statutory concepts
- The authoritative HMRC contracts established under Objective 3

Existing infrastructure should be preserved where valid. Obsolete SA100 and EOPS assumptions are not preservation requirements.

### Objective 3

In progress.

The current priority is to establish truthful HMRC contracts before further statutory mapping or transport implementation.

The Self Assessment contract audit establishes the pattern to be used for subsequent Corporation Tax and statutory-accounts work: authoritative external contracts shall be established before internal submission models are treated as canonical.

### Objectives 4 and 5

Not yet complete.

They must consume the validated outputs of the preceding objectives rather than compensating for defects in those outputs.

---

## 13. Success Criteria

The programme is complete when:

- Statutory reporting is migrated
- Operational and statutory representations can be compared
- Statutory values remain deterministically traceable to operational sources
- Validation and reconciliation workflows are operational
- Internal submission logic conforms to the statutory information required by current external contracts
- HMRC-compliant payloads can be generated under Objective 3
- HMRC submissions can be executed under Objective 4
- Submission history is available
- Tenant isolation is maintained
- Multi-tenant deployment is supported
- Supported statutory regimes are explicitly defined
- Unsupported or obsolete filing regimes are not accidentally preserved through legacy implementation

**End of document.**
