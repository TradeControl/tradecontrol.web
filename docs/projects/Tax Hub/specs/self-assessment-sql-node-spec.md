# Self Assessment SQL Node Implementation Specification

Trade Control Tax Hub Programme  
Revision 3 — 28 August 2026

## 1. Status and Purpose

This document specifies the SQL Node work required to support Sole Trader Self Assessment through Making Tax Digital for Income Tax within Tax Hub.

It supersedes Revision 2.

This is a governing implementation specification, not a general authorisation to edit the repository. Each delivery phase remains separately reviewable and requires explicit approval before implementation.

No commits, pushes, submodule pointer changes, cross-repository edits, or unapproved mappings are authorised by this document.

### 1.1 Product Scope

Trade Control supports Sole Trader Self Assessment submission through **Making Tax Digital for Income Tax only**.

Legacy SA100/SA103F XML submission is not supported.

Users who are not eligible for, are exempt from, or choose not to use MTD Income Tax must use another submission method.

The SQL Node scope is therefore the accounting-to-statutory projection required by the current MTD Income Tax model.

The former dual-path architecture comprising:

- MTD Quarterly Update plus EOPS; and
- legacy SA100/SA103F

is retired.

EOPS is not part of the current MTD filing workflow and must not remain as a live statutory target merely because historical Trade Control code models it.

### 1.2 Programme Objective

This work contributes principally to Tax Hub Programme **Objective 2 — Submission Logic**.

Objective 2 owns:

- extraction from Trade Control accounting classifications;
- Tax Source and Tax Tag projection;
- internal statutory representations;
- structural and numerical validation;
- deterministic traceability back to operational accounting.

Objective 2 does **not** define HMRC wire payloads.

Exact HMRC request and response contracts, API versions, mandatory and optional properties, endpoint semantics, XML/JSON representations, and other externally governed contract details belong to **Objective 3 — HMRC API**.

Transport, authentication, fraud-prevention headers, retry behaviour, envelopes, submission mechanics, and other communication concerns belong to **Objective 4 — HMRC Transport Platform**.

---

## 2. Governing Principles

The implementation shall preserve the following principles.

### 2.1 Accounting Authority

Existing Trade Control accounting calculations remain authoritative for the operational financial result.

The Cash Statement remains the primary operational financial representation.

Tax Hub and the Tax Tag layer consume those results. They do not reinterpret transactions or reproduce the accounting engine.

### 2.2 Deterministic Statutory Projection

Every mapped statutory value must be deterministically traceable to an operational source.

A mapping is permitted only where the accounting classification available to the configured template can supply the statutory concept without semantic ambiguity or double counting.

Similarity of names is not evidence of equivalence.

### 2.3 No Invented Precision

Where the available accounting classification cannot deterministically supply a statutory distinction, the system must not manufacture that distinction.

A coarse MIN classification may therefore support fewer detailed statutory fields than STD.

Unsupported, contextual, externally calculated, derived, or optional values must remain explicitly identified as such.

### 2.4 External Contract Authority

Trade Control code and historical implementations are evidence of previous intent. They are not authoritative where an externally governed statutory or protocol contract is involved.

Current authoritative HMRC specifications govern:

- the statutory concepts that must or may be supplied;
- their semantics;
- the filing lifecycle;
- required, optional, contextual, and derived distinctions;
- the external contract ultimately produced by Objective 3.

Existing SQL Tax Tags, `hmrc_mtd` classes, historical payload builders, test harnesses, and legacy mappings must be corrected where they conflict with the current external contract.

### 2.5 Separation of Concerns

The architecture shall preserve the following boundary:

**Trade Control accounting**  
→ **Tax Source / Tax Tag statutory projection — Objective 2**  
→ **HMRC contract adapter — Objective 3**  
→ **HMRC transport — Objective 4**

A SQL Tax Tag is therefore not automatically an HMRC JSON or XML property.

The Tax Tag layer exists to expose statutory meaning from Trade Control accounting in a deterministic and testable form.

### 2.6 Explicit Absence

The absence of a legitimate source is a valid result.

Unknown, unsupported, not-applicable, contextual, or optional statutory information must not be represented as an artificial zero merely to make a projection appear complete.

Zero may be supplied only where it is the correct accounting value or where the governing statutory contract explicitly requires it.

---

## 3. Repository and Submodule Boundaries

The working checkout is the live `tradecontrol.web` superproject, including its populated submodules.

The relevant repository responsibilities are:

- `src/sqlnode` owns the SQL schema, accounting bootstrap, Tax Sources, Tax Tags, mappings, extraction, and SQL validation addressed by this specification.
- `src/hmrc_mtd` owns the HMRC-facing model and adapter implementation associated with Objectives 3 and 4.
- `tradecontrol.web` owns the Tax Hub UI and workflow.
- `src/TCExports` is outside the present scope.

The live repositories are authoritative for establishing **current implementation state**.

They are not independently authoritative for externally governed HMRC semantics.

Reconnaissance may inspect the complete populated superproject wherever necessary to establish dependencies and contracts.

Unless separately approved, implementation write scope remains:

`src/sqlnode`

No implementation phase may modify `src/hmrc_mtd`, `tradecontrol.web`, `src/TCExports`, or a superproject submodule pointer as a side effect.

Commits, releases, pointer advancement, and cross-repository changes are distinct review actions.

---

## 4. Revision 2 Work Already Completed

Revision 3 does not rewrite implementation history.

The following work was correctly performed under the then-current Revision 2 architecture and has already been reviewed and accepted.

### 4.1 Phase 1 — Structural Separation — COMPLETE

The MIN and STD Sole Trader accounting templates were made tax-neutral.

Obsolete Self Assessment Tax Source, Tax Tag, mapping, and validation material was removed from the accounting templates.

Stale `@IsMTD` arguments and associated misleading comments were removed.

Unrelated accounting behaviour was preserved.

### 4.2 Phase 2 — Wrapper Composition — COMPLETE

The four then-defined wrapper variants were changed so that each composed:

1. its accounting template; and
2. its corresponding dedicated tax-seeding procedure.

The accounting procedure executes before the tax procedure.

An outer wrapper transaction provides atomic composition.

No mappings, canonical vocabulary changes, or HMRC model changes were introduced.

### 4.3 Effect of the Revised Product Decision

The subsequent decision to support MTD Income Tax only changes the required end state.

It does not make the completed Phase 1 or Phase 2 work erroneous.

The legacy SA wrappers and SA tax-seeding procedure now become deliberate retirement candidates because the product requirement they served has been withdrawn.

Likewise, the EOPS source and vocabulary must be replaced because the current MTD filing lifecycle no longer uses EOPS.

Implementation history must record this as an architectural change after Phase 2, not as correction of an implementation defect.

---

## 5. Established Current State Requiring Reconciliation

### 5.1 Accounting Templates

The Sole Trader accounting variants are:

- `App.proc_Template_ST_SOLE_CUR_MIN_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_2026`

They are now tax-neutral.

MIN provides a deliberately coarse accounting model.

STD extends that model with more detailed CategoryCode and CashCode classification.

Neither template is an HMRC taxonomy.

The Category Tree remains an operational accounting structure configurable by the business.

### 5.2 Existing MTD Tax Procedure

The existing procedure:

`App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026`

currently contains historical MTD Tax Source and Tax Tag definitions including:

- Quarterly Update; and
- EOPS.

The Quarterly Update concept remains relevant but its exact Tax Tag vocabulary must be reconciled with the current MTD statutory requirements.

The EOPS source is obsolete as a filing-stage model and must not survive into the required end state.

Individual adjustment, allowance, loss, or other concepts formerly grouped beneath EOPS may still be relevant to the current annual MTD process. Their continued statutory relevance must be established individually rather than preserving the EOPS container.

### 5.3 Existing Legacy SA Procedure

The procedure:

`App.proc_Template_ST_SOLE_CUR_TAX_SA_2026`

creates the legacy:

`UK-SA-SE-RETURN`

SA100/SA103F vocabulary.

That submission path is outside the revised product scope.

The procedure, source, tags, mappings, and references are therefore candidates for retirement.

### 5.4 Existing Wrappers

The current wrapper family contains:

- `App.proc_Template_ST_SOLE_CUR_MIN_MTD_2026`
- `App.proc_Template_ST_SOLE_CUR_MIN_SA_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_MTD_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_SA_2026`

The MTD wrappers remain valid composition entry points subject to the corrected MTD statutory projection.

The SA wrappers no longer represent supported product variants and are candidates for retirement.

### 5.5 Existing Validation

`Cash.proc_TaxTagMapValidate` provides structural validation of configured Tax Tag mappings.

Existing reconnaissance established that it does not by itself prove:

- statutory completeness;
- semantic correctness;
- cross-tag overlap;
- absence of double counting through category ancestry;
- appropriate treatment of required versus optional values;
- correct derivation semantics;
- conformity with current HMRC contracts.

Passing this procedure is therefore necessary where applicable but not sufficient for acceptance.

---

## 6. Current MTD Statutory Direction

### 6.1 Quarterly Updates

The current MTD Income Tax model requires cumulative quarterly reporting of self-employment accounting information from the start of the tax year to the end of the relevant update period.

The core statutory accounting concepts comprise two income totals and thirteen expense totals.

The SQL projection must support these concepts where the configured accounting model can supply them deterministically.

The existence of additional optional properties in an HMRC API request schema does not automatically create additional mandatory Tax Tags.

The Tax Tag model must distinguish between:

- statutory accounting information required from Trade Control;
- optional information that Trade Control can legitimately supply;
- information supplied elsewhere by workflow context;
- information derived by Objective 3;
- information not supported by the configured accounting model.

### 6.2 Annual and Finalisation Information

Current MTD Income Tax finalisation is not an EOPS workflow.

Annual self-employment information may nevertheless require statutory concepts including adjustments and allowances.

Losses are governed separately and must not remain embedded in an EOPS model merely because the historical implementation placed them there.

The SQL projection must therefore model the current statutory concepts required from Trade Control without reproducing obsolete filing-stage terminology.

### 6.3 Final Declaration and Personal Tax Calculation

Trade Control's SQL accounting layer does not calculate the individual's definitive Income Tax liability.

It provides the deterministic business accounting and statutory business information required by the MTD process.

The definitive personal tax calculation may depend upon information outside the Business Node.

Where an authoritative liability becomes available through the submission process, it may subsequently be reconciled with Trade Control's estimated tax provision through the existing period-adjustment mechanism.

That liability-feedback mechanism belongs to later Tax Hub workflow integration and is not a Tax Tag mapping responsibility.

---

## 7. Required End State

The completed SQL Node design shall observe the following composition model.

### 7.1 Accounting Variants

MIN and STD remain alternative accounting classifications.

They remain tax-neutral.

### 7.2 Supported Sole Trader Submission Variant

MTD Income Tax is the only supported Sole Trader Self Assessment submission architecture.

The supported bootstrap compositions are therefore:

| Wrapper | Accounting template | Tax projection |
|---|---|---|
| MIN MTD | MIN | Current MTD Income Tax statutory projection |
| STD MTD | STD | Current MTD Income Tax statutory projection |

Legacy MIN SA and STD SA are not supported end-state variants.

### 7.3 Tax Source Ownership

Dedicated tax-seeding logic creates only Tax Sources and Tax Tags that represent the approved current MTD statutory projection.

It shall not contain variant-dependent mappings whose correctness depends upon choosing MIN or STD.

### 7.4 Mapping Ownership

Variant-dependent mappings belong at the composition boundary where an accounting variant and the statutory projection meet.

The MIN MTD and STD MTD wrappers are therefore responsible for installing only their approved mappings after both accounting classifications and Tax Tags exist.

### 7.5 Validation Position

Validation occurs after:

1. accounting classifications exist;
2. the approved Tax Source and Tax Tag vocabulary exists;
3. the approved mappings have been installed.

### 7.6 Retirement

The required end state contains no live Sole Trader submission dependency upon:

- SA100;
- SA103F;
- `UK-SA-SE-RETURN`;
- MIN SA wrapper;
- STD SA wrapper;
- the dedicated legacy SA tax-seeding procedure;
- EOPS as an MTD filing source;
- legacy EOPS-specific workflow assumptions.

Removal must be evidence-led.

A reconnaissance phase must identify every dependency before deletion is authorised.

This retirement applies to the Sole Trader Self Assessment path only.

It must not be generalised into removal of XML, RIM, iXBRL, IRmark, or other legacy transport machinery required by unrelated current statutory regimes such as Corporation Tax.

---

## 8. Tax Tag and Mapping Policy

### 8.1 Tax Tags Are a Statutory Projection

Tax Tags form a statutory projection over Trade Control accounting classifications.

They are not the Category Tree itself and they are not necessarily one-to-one HMRC wire properties.

A business may use MIN, STD, or future custom classifications.

Support for a statutory concept depends upon whether those classifications provide a deterministic source.

### 8.2 Mapping Is an Accounting Decision

Mappings determine the statutory meaning assigned to operational values.

They must be proposed from evidence and approved before insertion.

A mapping must never be inferred solely from:

- similar names;
- historical mappings;
- current SQL seed names;
- current `hmrc_mtd` property names;
- apparent convenience.

### 8.3 Mapping Matrix

For every proposed current MTD Tax Tag, the mapping analysis shall record:

- Tax Source;
- Tax Tag code;
- statutory description;
- statutory provenance;
- classification as required, optional, contextual, adjustment, allowance, derived, externally supplied, or unsupported;
- MIN disposition;
- STD disposition;
- proposed source type;
- proposed CategoryCode or CashCode where applicable;
- accounting rationale;
- roll-up and double-counting assessment;
- Objective 3 contract relationship where known;
- evidence reference;
- confidence and unresolved questions;
- explicit review decision.

### 8.4 CategoryCode Versus CashCode

A CategoryCode is appropriate only where the complete category total has the same statutory meaning as the Tax Tag.

A CashCode is appropriate where the statutory concept requires a narrower amount that can be deterministically identified from a cash classification.

The choice must be driven by semantics rather than a general preference for either level.

### 8.5 Roll-Ups and Double Counting

Before approving a mapping, the analysis must trace relevant category ancestry and the Cash Codes allocated beneath it.

No additive statutory projection may consume both a parent total and amounts already included beneath that parent unless the target semantics explicitly require the relationship and aggregation treats the parent appropriately.

MIN-to-STD differences must be explicit.

STD detail must not be retrospectively invented for MIN.

### 8.6 Unsupported, Contextual and Derived Values

Unmapped is a legitimate result.

The matrix must distinguish:

- genuinely unsupported statutory concepts;
- workflow or user-supplied context;
- values supplied from another authoritative subsystem;
- statutory adjustments;
- allowances;
- derived totals;
- externally calculated values;
- optional values that may legitimately be absent.

A derived value must not be mapped to an accounting total merely to achieve apparent completeness.

Accounting depreciation and statutory capital allowances must not be treated as interchangeable.

### 8.7 Historical Mapping Evidence

Historical mappings may be examined to understand prior intent.

They have no presumptive authority.

Every retained mapping must survive the current semantic and statutory review independently.

---

## 9. Bounded Delivery Phases

Phases 1 and 2 are historical completed phases described in section 4.

Work under Revision 3 begins with Phase 3.

A later phase must not be folded into an earlier phase merely because relevant files are already open.

### Phase 3 — Contract-Aligned MTD Reconnaissance and Proposal

Read-only reconnaissance shall establish the exact difference between the live SQL implementation and the revised MTD-only architecture.

It shall:

- confirm the current MIN and STD accounting-template state;
- confirm the current four wrapper call graphs and transaction boundaries;
- inventory all MTD and SA Tax Sources and Tax Tags;
- locate every reference to the legacy SA procedure, SA wrappers, `UK-SA-SE-RETURN`, SA100, and SA103F;
- locate every EOPS source, tag, builder, test, comment, and dependency relevant to SQL Node;
- inspect the current MTD Quarterly Update vocabulary;
- reconcile proposed statutory concepts against the current authoritative Objective 3 HMRC contract evidence;
- distinguish the core quarterly accounting totals from additional optional API properties;
- identify current annual adjustment and allowance concepts that legitimately require Trade Control projection;
- identify loss information that belongs to the current losses architecture rather than EOPS;
- trace MIN and STD CategoryCode/CashCode coverage for every proposed Tax Tag;
- inspect `Cash.proc_TaxTagMapValidate` and document precisely what it does and does not prove;
- identify all repeatable bootstrap or integration-test facilities;
- identify any dependency that would make retirement of the SA or EOPS paths unsafe.

The deliverable shall contain:

1. current-state evidence;
2. proposed current MTD Tax Source and Tax Tag vocabulary;
3. MIN and STD mapping matrices;
4. explicit retirement list;
5. dependency and issue log;
6. validation proposal;
7. unresolved questions.

No SQL or C# edits are authorised in Phase 3.

**Gate:** explicit human approval of the Tax Source vocabulary, Tax Tags, mappings, retirement list, and any prerequisite accounting change.

### Phase 4 — Structural Retirement and Approved Vocabulary

After Phase 3 approval:

- retire the legacy SA bootstrap path from SQL Node;
- retire EOPS as an MTD Tax Source;
- remove obsolete Tax Tags and stale references within the authorised SQL scope;
- establish only the approved current MTD Tax Sources and Tax Tags;
- preserve MIN and STD accounting behaviour;
- preserve the MTD wrapper transaction/composition behaviour except where an explicitly approved structural change requires otherwise.

No unapproved mappings or accounting classifications may be introduced.

**Deliverable:** focused SQL diff corresponding to the approved structural retirement and vocabulary proposal.

**Gate:** review confirms that only supported MTD statutory structures remain and unrelated accounting behaviour is unchanged.

### Phase 5 — Approved Mapping Implementation

Install only the mappings explicitly approved in Phase 3.

Mappings must be inserted at the approved composition boundary.

Do not add Categories, Cash Codes, Tax Sources, or Tax Tags except where separately authorised as an approved prerequisite.

Invoke structural validation after the complete approved mapping set exists.

**Deliverable:** reviewed SQL changes corresponding one-for-one with the approved mapping matrix.

**Gate:** structural validation passes and the live mappings reconcile exactly to the approved matrix without undocumented exceptions.

### Phase 6 — Variant and Numerical Validation

Exercise MIN MTD and STD MTD independently in isolated, repeatable test databases where available.

Verify:

- object creation;
- Tax Source and Tax Tag inventories;
- absence of retired SA/EOPS structures;
- mapping integrity;
- rerun/idempotency behaviour;
- atomic failure behaviour;
- category roll-up behaviour;
- absence of double counting;
- representative numerical extraction;
- expected unsupported or absent values;
- traceability from Tax Tag to CategoryCode/CashCode and underlying operational classification.

Where useful, exercise the Tax Hub Test Harness to inspect and verify the resulting Objective 2 statutory projections.

The Test Harness is development and verification infrastructure and does not define an alternative statutory projection or submission contract.

**Deliverable:** validation report containing the commands or test cases used, expected values, actual results, reconciliation evidence, known limitations, and any accepted unmapped statutory concepts.

**Gate:** all acceptance criteria are satisfied or every exception is explicitly reviewed and accepted.

### Phase 7 — Repository Integration

Only after SQL Node implementation has been approved may repository integration be considered.

Commit creation within `sqlnode`, advancement of the `src/sqlnode` submodule pointer, coordinated `hmrc_mtd` work, and superproject integration are separate actions.

**Deliverable:** integration proposal identifying exact repositories and revisions.

No implicit commits or pointer advancement are authorised.

---

## 10. Acceptance Criteria

### 10.1 Product Scope

- Sole Trader Self Assessment SQL support is MTD Income Tax only.
- No supported SQL bootstrap variant depends upon SA100 or SA103F.
- EOPS is not represented as a current MTD filing stage.
- Retirement of legacy Sole Trader submission code has not removed transport machinery required by other statutory regimes.

### 10.2 Structural

- MIN and STD accounting templates remain tax-neutral.
- MIN MTD and STD MTD each compose exactly one accounting template with the approved current MTD tax projection.
- Legacy SA wrappers and their dedicated tax procedure are absent from the supported bootstrap architecture.
- The MTD Tax Source/Tag vocabulary contains only approved current statutory concepts.
- Variant-dependent mappings do not reside in the accounting templates or variant-neutral tax-seeding logic.
- No obsolete `@IsMTD` mechanism is reintroduced.

### 10.3 Mapping Integrity

- Every implemented mapping appears in the approved matrix.
- Every mapped Tax Tag references a valid operational source.
- No mapping relies solely upon label similarity or historical precedent.
- Category roll-ups have been examined for overlap and double counting.
- MIN mappings do not depend on STD-only classification.
- STD detail is used only where semantically correct and deterministic.
- Required, optional, contextual, adjustment, allowance, derived, externally supplied, and unsupported concepts are distinguished explicitly.
- Artificial zero values are not used to conceal absence or unsupported information.

### 10.4 Behavioural Preservation

- Existing non-tax MIN and STD accounting behaviour remains unchanged.
- Existing accounting calculations remain the source of operational financial values.
- VAT, owner capital, account creation, tax-year alignment, and unrelated accounting behaviour are unaffected.
- Wrapper failure handling does not leave a partially configured accounting/tax environment under tested execution paths.

### 10.5 Reconciliation and Projection Readiness

- Representative Tax Tag values can be traced back to their CategoryCode or CashCode and onward to underlying operational classification.
- Aggregated statutory values reconcile with approved accounting sources.
- Unsupported or absent statutory concepts are visible and explained.
- MIN MTD and STD MTD can produce their approved internal Objective 2 statutory projection without invoking HMRC transport.
- Objective 2 projections contain sufficient truthful information for Objective 3 to build supported HMRC contracts without inventing missing accounting detail.

### 10.6 Repository Discipline

- Changes remain within authorised repository scope.
- No unrelated user changes are overwritten.
- No commits, pushes, pull requests, releases, or submodule pointer updates occur without explicit instruction.
- Final handoff identifies changes by repository and revision.

---

## 11. Out of Scope

This specification does not authorise:

- legacy SA100/SA103F submission support;
- definitive personal Income Tax calculation;
- HMRC endpoint implementation;
- exact HMRC JSON/XML DTO implementation;
- serializers;
- OAuth;
- fraud-prevention headers;
- Government Gateway transport;
- retry policy;
- filing workflow;
- Tax Hub UI changes;
- liability-feedback or period-adjustment workflow implementation;
- changes to underlying accounting calculations;
- invention of accounting classifications merely to achieve statutory completeness;
- VAT or Corporation Tax work;
- removal of XML/RIM/iXBRL/IRmark machinery required by other statutory regimes;
- autonomous commits, pushes, releases, pull requests, or submodule pointer updates.

---

## 12. Review Decisions Required Before Phase 4

Phase 3 must produce sufficient evidence for explicit approval of:

1. the current MTD Tax Source structure;
2. the canonical Objective 2 Tax Tag vocabulary;
3. the distinction between core quarterly accounting totals and optional HMRC API properties;
4. the current treatment of annual adjustments and allowances;
5. the treatment and location of losses;
6. the MIN mapping matrix;
7. the STD mapping matrix;
8. every intentional unsupported or unmapped statutory concept;
9. the complete SA100/SA103F retirement set;
10. the complete EOPS retirement/replacement set;
11. the required validation behaviour;
12. bootstrap rerun/idempotency expectations;
13. any requirement that genuinely crosses the `sqlnode` repository boundary.

No Phase 4 implementation may begin until these decisions are approved.

---

## 13. Definition of Completion

The Self Assessment SQL Node work is complete when:

- Sole Trader Self Assessment is represented exclusively through the supported current MTD Income Tax architecture;
- MIN and STD accounting templates remain unchanged in their accounting purpose and tax-neutral in their construction;
- the current statutory Tax Source and Tax Tag projection has been established from authoritative requirements rather than historical implementation;
- every implemented mapping is deterministic, approved, traceable, and free from unintended double counting;
- unsupported or externally supplied information is represented explicitly rather than guessed;
- legacy SA100/SA103F and EOPS filing structures no longer participate in the supported Sole Trader bootstrap;
- MIN MTD and STD MTD produce reconciled internal Objective 2 statutory projections;
- repository boundaries and approval controls have been preserved.

Completion of this SQL Node work does not imply completion of HMRC wire contracts, HMRC transport, personal tax calculation, Tax Hub filing workflow, or the wider Tax Hub Programme.
