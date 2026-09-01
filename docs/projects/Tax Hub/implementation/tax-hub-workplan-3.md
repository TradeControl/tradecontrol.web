# Corporation Tax — HMRC_MTD Contract Model Work Plan

## Objective

Add the current HMRC Corporation Tax submission model to the `HMRC_MTD` repository under a new:

`Hmrc.CorporationTax.v1_0`

namespace.

The implementation must be derived from the authoritative Corporation Tax contracts documented in:

`docs/projects/Tax Hub/specs/reference/company-field-sets.md`

It must not be derived from the historical Trade Control `AC` / `CP` Tax Tags or existing Corporation Tax SQL bootstrap.

The purpose of this work is to establish the authoritative C# contract model that can subsequently be used to design and verify the Trade Control Corporation Tax statutory projection.

---

## Architectural Boundary

The current HMRC Company Tax Return submission consists of distinct concerns:

1. CT600 return data;
2. conditional CT600 supplementary pages;
3. Corporation Tax computations;
4. full statutory accounts;
5. supporting documents where applicable;
6. submission packaging, validation and transport.

These concerns must remain distinct in the C# model.

Do not collapse them into a single flat payload.

The initial namespace structure should follow:

``` text
Hmrc
└── CorporationTax
    └── v1_0
        └── Submissions
            ├── CT600
            │   ├── Return
            │   ├── SupplementaryPages
            │   └── Shared
            ├── Computations
            ├── Accounts
            └── Shared
```

The precise class/file decomposition should follow the authoritative contract rather than being forced to match this proposed tree where that would distort the external model.

---

## Phase 1 — Repository Reconnaissance

Inspect the existing `HMRC_MTD` repository and document:

- namespace and folder conventions;
- DTO/model conventions;
- serialization conventions;
- validation conventions;
- shared primitive/value types;
- versioning conventions;
- how existing SA and VAT models distinguish contracts from transport;
- reusable infrastructure under `Hmrc.Shared`.

Do not modify code.

Return a proposed Corporation Tax namespace/file structure before implementation.

### Acceptance criteria

- Existing conventions are understood.
- Reusable infrastructure is identified.
- No Corporation Tax model is invented where an existing generic type is appropriate.
- No SA or VAT-specific assumption is imported into Corporation Tax.

---

## Phase 2 — CT600 Core Return Model

Implement the current CT600 core return contract.

Source authority must be the current:

- CT600 form;
- CT600 guide;
- CT600 Return Information Model (RIM);
- generic validation rules.

Model the CT600 by coherent statutory groups rather than accounting headings.

These include:

- company identity;
- return context;
- attached-document and supplementary-page indicators;
- income and gains;
- deductions and reliefs;
- associated-company/rate information;
- tax calculation;
- credits and reconciliation;
- capital allowances;
- losses;
- repayment information;
- declaration.

Preserve authoritative identifiers such as CT600 box numbers and RIM element identities in documentation/metadata where useful.

Do not map ledger values or Tax Tags during this phase.

### Acceptance criteria

- The current CT600 can be represented without relying on historical `AC`/`CP` fields.
- Required, optional and conditional fields are distinguishable.
- Monetary sign and precision semantics follow the current HMRC contract.
- Period fields represent one Corporation Tax accounting period of no more than 12 months.
- Model provenance identifies the authoritative CT600/RIM release used.

---

## Phase 3 — CT600 Supplementary Pages

Model supplementary pages as separate conditional contracts.

Current pages include:

- CT600A;
- CT600B;
- CT600C;
- CT600D;
- CT600E;
- CT600F;
- CT600G;
- CT600H;
- CT600I;
- CT600J;
- CT600K;
- CT600L;
- CT600M;
- CT600N;
- CT600P.

Do not implement every page blindly.

For the initial micro-entity target, first classify each page as:

- plausibly applicable to an ordinary trading micro-company;
- specialist but architecturally supported;
- outside initial implementation scope.

Implement the common/in-scope pages first.

The CT600 core model must nevertheless be capable of representing which supplementary pages accompany a return.

### Acceptance criteria

- Supplementary pages remain separate from the core CT600.
- Applicability is explicit.
- Micro-entity status does not incorrectly suppress a page whose underlying transaction/status makes it applicable.

---

## Phase 4 — Corporation Tax Computation Model

Implement the semantic model required to represent the current HMRC Corporation Tax computation.

Start with the currently specified computation sections, including:

- trade identity and period;
- accounting profit/loss;
- disallowable expenditure;
- taxable income not credited in accounts;
- allowable deductions;
- deductions not present in accounts;
- adjusted trade profit/loss;
- capital allowances;
- gains;
- non-trading income;
- losses and claims.

Keep accounting facts and tax adjustments distinct.

In particular:

- book depreciation must remain separate from capital allowances;
- accounting profit/loss must remain separate from taxable profit/loss;
- a negative accounting result must not automatically become a Corporation Tax loss;
- each computation belongs to one Corporation Tax accounting period.

The model must allow one statutory accounts period to support more than one Corporation Tax computation/return.

### Acceptance criteria

- A normal trading micro-company computation can be represented.
- A long first accounts period can support two CT accounting-period computations.
- Accounting values remain traceable to their source.
- Tax adjustments do not overwrite accounting values.

---

## Phase 5 — HMRC Accounts Attachment Model

Determine the correct boundary for the full statutory accounts attached to the Company Tax Return.

Do not duplicate the entire accounting domain unnecessarily inside `HMRC_MTD`.

Establish the minimum contract representation needed for:

- current and comparative statutory accounts facts;
- iXBRL contexts;
- taxonomy concept bindings;
- dimensions;
- units;
- precision/scaling;
- required notes and disclosures.

The authoritative accounts facts are based on the applicable FRC taxonomy rather than historical HMRC `AC` identifiers.

The model should distinguish:

1. semantic accounts facts supplied by Trade Control; and
2. HMRC-specific iXBRL rendering/binding requirements.

### Acceptance criteria

- Accounts are not represented as a flat `tag -> value` dictionary.
- Current and comparative facts remain distinguishable.
- instant and duration contexts are represented correctly.
- dimensional facts can be represented.
- taxonomy release/version is external contract metadata rather than a permanent internal semantic key.

---

## Phase 6 — Submission Package Model

Implement the model representing one complete HMRC Company Tax Return submission package.

It must be capable of associating:

- exactly one CT600;
- zero or more applicable supplementary pages;
- full statutory accounts;
- exactly one computation for the CT accounting period;
- permitted supporting documents;
- submission/envelope metadata.

Do not implement live transport yet.

### Acceptance criteria

- One package corresponds to one Corporation Tax accounting period.
- One accounts set may participate in multiple packages where the accounts period exceeds 12 months.
- Accounts and computations remain separate documents.
- Package validation can establish internal completeness before serialization or transport.

---

## Phase 7 — Serialization and Contract Fixtures

Using authoritative HMRC schemas and technical specifications, establish deterministic serialization fixtures.

Create representative fixtures for at least:

1. a straightforward first-year micro-company;
2. a company with current and comparative accounts;
3. a long first accounts period producing two CT returns;
4. depreciation add-back with capital allowances;
5. a loss/relief case;
6. at least one applicable supplementary-page case.

Validate generated representations against the available HMRC schemas, validation rules and Local Test Service where appropriate.

Do not connect Trade Control accounting data yet.

### Acceptance criteria

- C# models serialize deterministically to the expected contract representation.
- Fixtures pass available schema/contract validation.
- Failures identify the contract field/rule responsible.
- Test fixtures are independent of Trade Control SQL.

---

## Phase 8 — Contract Review

Before any Trade Control mapping work begins, review the resulting Corporation Tax model against:

`docs/projects/Tax Hub/specs/reference/company-field-sets.md`

Confirm:

- CT600 coverage;
- computation coverage;
- accounts attachment boundary;
- period handling;
- supplementary-page architecture;
- contract/version provenance;
- unresolved authoritative gaps.

Record omissions explicitly.

Do not invent fields merely to complete an abstraction.

---

## Phase 9 — Trade Control Reconciliation

Only after the authoritative C# contract model is approved should the existing Trade Control Corporation Tax SQL bootstrap be revisited.

That later phase will compare:

1. authoritative C# Corporation Tax model;
2. authoritative company field-set reference;
3. historical SQL bootstrap;
4. current Category Tree / Tax Source / Tax Tag architecture.

It will classify existing SQL concepts as:

- retain;
- rename/rebind;
- derive;
- move outside Tax Tags;
- obsolete;
- unsupported;
- new requirement.

Historical `AC` and `CP` identifiers are evidence only and must not be treated as current contract authority.

---

## Explicit Non-Goals

This work plan does not authorise:

- modification of Trade Control SQL;
- modification of company bootstrap templates;
- creation of new Tax Tags or mappings;
- Companies House implementation;
- live HMRC submission;
- credentials or authentication work;
- production transport;
- redesign of the Category Tree;
- support for every specialist Corporation Tax regime.

The immediate goal is the authoritative HMRC Corporation Tax contract model.

---

## End State

At completion, `HMRC_MTD` should contain a current, independently verifiable Corporation Tax contract model under:

`Hmrc.CorporationTax.v1_0`

That model becomes the authority against which the Trade Control Corporation Tax statutory projection can be designed and tested.

The sequence is therefore:

Authoritative statutory research
→ authoritative C# contract model
→ Trade Control projection reconciliation
→ Tax Source / Tax Tag implementation
→ test harness
→ serialization
→ transport
