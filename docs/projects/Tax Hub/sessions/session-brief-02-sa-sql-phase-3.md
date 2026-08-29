# Tax Hub — Session Brief

## Phase 3: Contract-Aligned MTD Reconnaissance and Proposal

### Purpose

Perform reconnaissance of the current Sole Trader MTD implementation across the live SQL Node and `hmrc_mtd` codebase.

The purpose of this session is to establish the evidence required for the next implementation phase.

This is a reconnaissance and proposal session only.

**Do not implement changes.**

---

## Governing Documents

Read the current Tax Hub documentation before beginning, in particular:

- `docs/projects/Tax Hub/specs/self-assessment-sql-node-spec.md`
- `docs/projects/Tax Hub/specs/sole-trader-field-sets.md`
- `docs/projects/Tax Hub/specs/tax-hub-test-payloads.md`
- the current Tax Hub programme specification
- `docs/projects/Tax Hub/findings.md`
- `docs/projects/Tax Hub/change-log.md`

Treat the current governing specifications as authoritative over superseded implementation plans and historical code.

For external HMRC contracts, current authoritative HMRC specifications take precedence over historical Trade Control implementation.

---

## Current Position

Earlier SQL work completed two valid structural phases:

1. MIN and STD accounting bootstrap procedures were made tax-neutral.
2. The four then-supported Sole Trader wrappers were changed to compose accounting setup with dedicated tax setup.

Subsequent HMRC contract verification and an architectural decision changed the required end state.

Trade Control now supports **Making Tax Digital for Income Tax only** for Self Assessment submission.

Legacy SA100/SA103F submission is not supported.

The former EOPS filing stage is not part of the current MTD Income Tax architecture.

The existing implementation therefore contains a mixture of:

1. current or potentially reusable HMRC contract code;
2. Trade Control statutory-projection code;
3. Test Harness/development infrastructure;
4. obsolete or legacy implementation.

Do not assume that an existing class, Tax Source, Tax Tag, endpoint, payload builder or service represents a current HMRC contract merely because it exists.

---

## Authorised Reconnaissance

### 1. SQL Node

Inspect the live Sole Trader tax bootstrap implementation in `src/sqlnode`.

Confirm:

- the current MIN and STD accounting templates;
- the current Sole Trader MTD wrappers;
- existing Tax Sources;
- existing Tax Tags;
- existing Tax Tag mappings;
- the current validation procedure and its actual guarantees;
- remaining dependencies on legacy SA100/SA103F or EOPS concepts.

Reconcile the existing MTD Tax Tag vocabulary against the current statutory projection requirements in the governing specifications.

In particular, distinguish:

- the core cumulative quarterly accounting totals;
- optional or contextual HMRC API properties;
- annual adjustments and allowances;
- losses and loss claims;
- derived information;
- external information which Trade Control cannot deterministically obtain from its accounting data.

Do not treat matching field counts as proof of semantic equivalence.

---

### 2. `hmrc_mtd`

Inspect the complete current `hmrc_mtd` implementation, including models, services, builders, readers, runners, serializers and supporting infrastructure.

Classify significant components as:

1. current HMRC contract;
2. Trade Control statutory projection;
3. Test Harness/development infrastructure;
4. obsolete/legacy implementation.

Identify code dependent upon:

- legacy SA100/SA103F submission;
- EOPS as a filing stage;
- superseded Quarterly Update contracts;
- superseded Final Declaration contracts;
- historical obligations, payments, liabilities or other Self Assessment API assumptions.

Do not repair these components during this session.

Where apparently current HMRC contract classes exist, compare them with the governing specifications and current authoritative HMRC contracts rather than accepting their names or namespaces as evidence of correctness.

The Trade Control namespace version `v1_0` is not an HMRC API version.

---

### 3. Operational Paths

Trace the existing operational paths through the implementation.

Where applicable, follow:

`SQL/statutory data -> reader -> builder -> runner -> Test Harness`

Establish which existing components actually participate in executable paths and which are disconnected models, historical experiments or unused infrastructure.

Pay particular attention to places where:

- missing statutory values are converted to zero;
- generic Tax Tag collections are transformed into supposed HMRC payloads;
- Test Harness structures have become de facto production contracts;
- Objective 2 statutory projection and Objective 3 HMRC wire contracts are conflated.

The Test Harness is development and diagnostic infrastructure only. It does not define an alternative Tax Hub or HMRC contract.

---

### 4. MIN / STD Mapping Feasibility

For both MIN and STD, inspect whether the accounting classifications can deterministically supply the proposed current MTD statutory projection.

Produce a mapping assessment showing, where evidence permits:

- statutory concept;
- source CategoryCode and/or CashCode;
- proposed Tax Tag;
- mapping rationale;
- whether mapping is deterministic;
- possible overlap or double counting;
- required roll-up;
- unsupported/contextual/derived/external status;
- unresolved ambiguity.

Do not invent mappings where the accounting model does not contain sufficient information.

Absence of information must remain absence rather than being represented as zero.

---

### 5. Retirement Candidates

Identify, but do not remove, implementation made obsolete by the current architecture.

This includes investigation of:

- legacy Sole Trader SA wrappers and tax-seeding procedures;
- `UK-SA-SE-RETURN`;
- `UK-ITSA-SE-EOPS`;
- EOPS payload/build/runner/harness paths;
- the `SA100` namespace and related Sole Trader XML submission machinery;
- other classes whose apparent HMRC contracts are no longer current.

Do not remove generic XML, canonicalisation, IRmark, RIM or iXBRL capabilities merely because they occur within historical Self Assessment implementation. Those mechanisms may be required by other tax regimes, particularly Corporation Tax.

---

## Required Output

At the end of the session, report:

1. **Current-state evidence**
   - relevant SQL procedures and call graph;
   - relevant `hmrc_mtd` operational paths;
   - significant dependencies and disconnected components.

2. **Component classification**
   - current HMRC contract;
   - statutory projection;
   - Test Harness/development infrastructure;
   - obsolete/legacy.

3. **Proposed MTD Tax Source and Tax Tag vocabulary**
   - quarterly projection;
   - annual/finalisation requirements where currently established;
   - explicit unresolved areas where the governing material does not yet justify a frozen vocabulary.

4. **MIN mapping assessment**

5. **STD mapping assessment**

6. **Retirement candidate list**

7. **Validation assessment**
   - what current validation proves;
   - what it does not prove;
   - validation required for the proposed end state.

8. **Issue and uncertainty log**
   - conflicting evidence;
   - unsupported mappings;
   - external information requirements;
   - questions requiring a design or statutory decision.

9. **Proposed bounded implementation sequence**
   - for review only;
   - no implementation in this session.

Append durable reconnaissance findings to `findings.md` only where appropriate under the existing documentation rules.

Do not rewrite historical findings.

---

## Constraints

- Reconnaissance and proposal only.
- No production source changes.
- No SQL implementation changes.
- No Tax Tag or Tax Source changes.
- No removal of legacy code.
- No payload or serializer repairs.
- No Test Harness redesign.
- No transport implementation.
- No commits or pushes.
- Do not broaden scope into VAT, Corporation Tax, Tax Hub UI or unrelated repository work.
- Record relevant out-of-scope discoveries rather than fixing them.

---

## Completion Gate

Phase 3 is complete when the evidence, classification, mapping assessment, retirement candidates and proposed implementation sequence are sufficient for human review.

**Stop at that point.**

Do not begin Phase 4 without explicit approval.
