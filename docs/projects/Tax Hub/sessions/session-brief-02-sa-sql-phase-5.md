# Tax Hub — Sole Trader Objective 3 Contract Research Brief

## Purpose

Establish the authoritative current HMRC Making Tax Digital for Income Tax contract surface required by Trade Control to support a Sole Trader Self Employment submission workflow.

This research is intended to provide the authoritative Objective 3 reference needed before implementing or rebuilding the C# contract model in `HMRC_MTD`.

The current Trade Control Sole Trader statutory projection is defined separately by:

`docs/projects/Tax Hub/specs/reference/sole-trader-field-sets.md`

That document is an Objective 2 statutory projection reference. It does not define HMRC wire DTOs.

This research must therefore establish the current HMRC-facing contract independently from existing Trade Control SQL, C# classes, test harnesses or historical SA100/EOPS implementations.

---

## Product Scope

Trade Control supports:

**Making Tax Digital for Income Tax — Self Employment**

The supported route is the current MTD Income Tax architecture only.

The following are explicitly outside scope:

- SA100 XML submission;
- SA103F submission;
- legacy Self Assessment filing;
- EOPS as a filing stage;
- property businesses;
- partnerships;
- trusts;
- non-resident specialist regimes;
- unrelated Income Tax APIs;
- Corporation Tax;
- Companies House.

Historical Trade Control classes, SQL, Tax Tags and harness payloads may be inspected as evidence of previous implementation intent, but they are not statutory authority.

---

## Research Cut-Off

Research the authoritative position current at:

**1 September 2026**

Where HMRC publishes multiple API versions, identify:

- the currently supported version;
- any version that is already deprecated or scheduled for withdrawal;
- any migration requirement relevant to implementation.

Do not assume that a repository namespace such as `v1_0` corresponds to an HMRC API version.

---

## Primary Authority

Use current official HMRC sources wherever available, including:

- HMRC Developer Hub API documentation;
- API schemas and examples;
- HMRC MTD Income Tax guidance;
- HMRC technical documentation;
- sandbox/test documentation;
- changelogs and version notices;
- applicable validation and error documentation.

Secondary material may be used only where official material is incomplete, and must be identified clearly as secondary evidence.

Every material technical conclusion must be traceable to a source.

---

## Central Research Question

Determine:

> What exact HMRC APIs, versions, request/response payloads and workflow steps are required for a self-employed individual to submit periodic self-employment information and complete the current MTD Income Tax annual/finalisation process?

The answer must be suitable for direct use as the authoritative basis for implementing the C# Objective 3 contract model.

---

## Required Research

### 1. Current API Surface

Identify the exact current HMRC APIs required for the supported Sole Trader workflow.

For each relevant API or operation record:

- API name;
- API version;
- operation name;
- HTTP method;
- endpoint path;
- OAuth scope;
- path parameters;
- query parameters;
- request body;
- response body;
- required identifiers;
- required headers;
- tax-year/version headers where applicable;
- sandbox/test availability;
- important validation rules;
- material error responses.

Separate APIs that:

- submit business accounting information;
- retrieve business details;
- retrieve obligations;
- retrieve or amend business information;
- retrieve calculations;
- submit adjustments or allowances;
- manage losses where relevant;
- perform finalisation/declaration;
- retrieve liabilities/payments where relevant to the supported workflow.

Do not include APIs merely because they exist. Include only those required or materially relevant to the supported Self Employment workflow.

---

### 2. Quarterly Self Employment Submission

Establish the exact current contract for periodic Self Employment reporting.

Determine:

- whether reporting is cumulative/year-to-date or period-only;
- what identifies the business;
- what identifies the tax year;
- whether a period start/end is supplied;
- whether obligation identifiers or period keys are supplied;
- the exact request payload structure;
- the exact income fields;
- the exact expense fields;
- the detailed-expense versus consolidated-expense rules;
- optional and conditional fields;
- zero versus omission semantics;
- monetary sign conventions;
- precision and rounding rules;
- amendment/update behaviour;
- idempotency or replacement semantics;
- response shape.

Reconcile this contract explicitly against the current Objective 2 quarterly projection in:

`docs/projects/Tax Hub/specs/reference/sole-trader-field-sets.md`

For every quarterly HMRC request property classify the source as one of:

- Objective 2 Tax Tag;
- Business Node configuration;
- Tax Hub workflow/context;
- HMRC-supplied identifier/state;
- derived value;
- external/user-supplied value;
- optional and legitimately absent;
- unsupported.

Do not manufacture Objective 2 Tax Tags merely because HMRC exposes a property.

---

### 3. Annual Self Employment Information

Establish the current annual Self Employment contract independently from historical EOPS or SA103F assumptions.

Identify the current HMRC operations and payload structures for any relevant:

- accounting adjustments;
- private-use adjustments;
- disallowable expenses;
- capital allowances;
- balancing charges;
- goods/services for own use;
- irrecoverable debts;
- basis adjustments;
- losses;
- brought-forward losses;
- loss claims or elections;
- other annual Self Employment statutory information.

For each concept determine:

- whether it is current;
- whether it is mandatory, optional or conditional;
- whether it belongs to the Self Employment Business API or another HMRC API;
- whether Trade Control accounting can legitimately supply it;
- whether it belongs in Objective 2, Objective 3 workflow/context, or an external HMRC process.

This section must provide the evidence needed to decide whether the currently unfrozen annual section of `sole-trader-field-sets.md` can now be completed.

Do not preserve historical EOPS groupings unless independently supported by the current MTD architecture.

---

### 4. Finalisation Workflow

Document the complete current workflow required to move from periodic Self Employment reporting to completion of the taxpayer's MTD Income Tax filing obligations.

Determine the authoritative sequence involving, where applicable:

- business updates;
- annual adjustments;
- allowances;
- losses;
- crystallisation/calculation;
- calculation retrieval;
- corrections;
- final declaration;
- submission confirmation;
- authoritative tax liability.

Identify:

- which steps are business-specific;
- which steps are taxpayer-level;
- which operations require a business ID;
- which require NINO or other taxpayer identity;
- which require calculation IDs;
- which identifiers are created by HMRC;
- which operations are reads versus writes;
- sequencing constraints;
- repeat/amend behaviour;
- final submission/declaration semantics.

Explicitly confirm whether EOPS exists as a current filing stage.

---

### 5. Contract Families

Produce a proposed logical Objective 3 contract structure suitable for implementation under:

`TradeControl.Tax.UK.Hmrc.Sa.v1_0`

Do not write code.

Identify the natural contract families suggested by the authoritative APIs, for example only where supported:

- Business Details;
- Self Employment;
- Obligations;
- Calculations;
- Losses;
- Liabilities;
- Payments;
- Finalisation;
- Shared.

Do not force the existing repository folder structure to survive if the current HMRC API surface demonstrates a better structure.

Do not reconstruct SA100, SA103F, EOPS or any removed historical classes.

---

### 6. Existing `HMRC_MTD` Classes

Review the current surviving Sole Trader classes only to classify them against the authoritative current API contracts.

For each current class/folder classify it as:

- current and substantially correct;
- current but incomplete;
- current but structurally incorrect;
- obsolete;
- unrelated/read-side utility;
- requires replacement;
- requires authoritative re-versioning.

Pay particular attention to:

- `Liabilities`;
- `Obligations`;
- `Payments`;
- request/response shapes;
- endpoint paths;
- API versions;
- OAuth scopes;
- date handling;
- JSON serialization;
- identifier handling.

Do not modify the repository.

---

### 7. Objective 2 Reconciliation

Compare the authoritative HMRC contract with:

`docs/projects/Tax Hub/specs/reference/sole-trader-field-sets.md`

Identify:

- Objective 2 concepts already correctly represented;
- Objective 2 concepts requiring rename or semantic clarification;
- missing annual concepts that should become Objective 2 candidates;
- HMRC properties that must remain Objective 3/context rather than Tax Tags;
- obsolete concepts that must remain retired;
- any assumptions in the current SQL projection that contradict the authoritative contract.

Do not propose SQL implementation changes at this stage.

The purpose is to establish whether the Objective 2 reference is sufficient for Objective 3 implementation and, where it is not, precisely what must be corrected first.

---

## Deliverable

Create:

`docs/projects/Tax Hub/specs/reference/sole-trader-contracts.md`

The document must be technical and implementation-oriented rather than explanatory or academic.

It should contain at minimum:

1. Executive conclusion.
2. Supported MTD Income Tax Self Employment filing lifecycle.
3. API/version inventory.
4. Quarterly submission contract.
5. Annual Self Employment contract.
6. Finalisation/calculation/declaration workflow.
7. Identifier and context model.
8. Request/response contract inventory.
9. Existing `HMRC_MTD` class assessment.
10. Objective 2 reconciliation.
11. Proposed Objective 3 C# contract families.
12. Open questions or unsupported areas.
13. Authoritative source register.

Where practical, include compact tables of fields and operations.

For each field or operation, retain authoritative HMRC terminology and machine property names where known.

Clearly distinguish:

- verified current contract;
- interpretation/inference;
- future/deprecated behaviour;
- unresolved matters.

---

## Non-Goals

Do not:

- modify C#;
- modify SQL;
- modify Tax Tags;
- modify the test harness;
- implement transport;
- implement OAuth;
- implement serializers;
- create DTOs;
- redesign the Category Tree;
- design Corporation Tax;
- restore SA100 or EOPS structures;
- infer contracts from historical Trade Control code;
- broaden the product beyond Self Employment.

---

## Completion Criteria

This research is complete when it is possible to give Codex an implementation instruction equivalent to:

> Implement the current HMRC MTD Income Tax Self Employment Objective 3 contract model from `sole-trader-contracts.md`, using `sole-trader-field-sets.md` as the approved Objective 2 accounting projection, without needing to invent HMRC fields, endpoints, workflow or statutory semantics.

If the research cannot support that instruction, identify exactly what remains unresolved rather than filling gaps by assumption.
