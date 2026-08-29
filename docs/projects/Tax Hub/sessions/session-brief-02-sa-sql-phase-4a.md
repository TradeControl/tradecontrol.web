# Tax Hub — Session Brief

## Phase 4A: Structural Retirement

### Purpose

Remove implementation that Phase 3 has positively classified as obsolete or unsupported under the current Sole Trader MTD architecture.

This is a cleanup and structural-retirement session only.

Do not introduce replacement architecture.

---

## Governing Evidence

Read:

- the current Tax Hub programme specification;
- `specs/self-assessment-sql-node-spec.md`;
- `specs/sole-trader-field-sets.md`;
- `specs/tax-hub-test-payloads.md`;
- `findings.md`, especially Phase 3 from section 20;
- `change-log.md`.

Treat current specifications and current HMRC contract evidence as authoritative over historical implementation.

---

## Authorised Work

### 1. Retire legacy Sole Trader SA bootstrap

Remove the unsupported legacy SA route from the live SQL bootstrap surface, including:

- `App.proc_Template_ST_SOLE_CUR_MIN_SA_2026`;
- `App.proc_Template_ST_SOLE_CUR_STD_SA_2026`;
- `App.proc_Template_ST_SOLE_CUR_TAX_SA_2026`;
- `UK-SA-SE-RETURN`;
- corresponding Node template/menu registrations and descriptions;
- stale SA100/SA103F bootstrap comments or references made obsolete by that removal.

Do not alter unrelated accounting bootstrap behaviour.

### 2. Retire EOPS from the Sole Trader MTD bootstrap

Remove:

- `UK-ITSA-SE-EOPS`;
- its seeded tags;
- its validation call;
- related bootstrap descriptions and stale TODO/comments.

Do not create a replacement annual Tax Source in this session.

### 3. Retire obsolete Self Assessment harness paths

Remove QU/EOPS harness components only where Phase 3 established that they embody the obsolete generic/EOPS architecture.

This may include obsolete:

- builders;
- validators;
- payload models;
- controllers;
- runner operations;
- associated registrations.

Do not redesign the Test Harness.

Do not create replacement cumulative/annual harness endpoints yet.

### 4. Retire obsolete Self Assessment contract interpretations

Remove or isolate the obsolete Sole Trader implementations identified in Phase 3 for:

- `MTDITSA/QuarterlyUpdate`;
- `MTDITSA/Eops`;
- `MTDITSA/FinalDeclaration`.

Do not replace them with new Objective 3 contracts in this session.

For Obligations, Payments, Liabilities and similar unverified areas, do not remove them merely by association unless dependency analysis establishes that they are obsolete and unused.

### 5. Preserve shared capability

Do not remove generic/shared:

- XML serialization;
- canonicalisation;
- IRmark;
- RIM;
- iXBRL;
- transport utilities;

merely because they currently sit beneath historical Self Assessment code.

Search dependencies first and preserve anything potentially required by Corporation Tax or other regimes.

---

## Constraints

- No new Tax Sources.
- No new Tax Tags.
- No new mappings.
- No cumulative or annual replacement vocabulary.
- No HMRC endpoint modelling.
- No serializer redesign.
- No transport implementation.
- No VAT or Corporation Tax changes except where required to preserve shared dependencies.
- No commits or pushes.

Record out-of-scope discoveries rather than fixing them.

---

## Validation

Confirm:

- SQL project builds;
- `hmrc_mtd` builds;
- WebHarness builds if still present;
- supported MIN/STD MTD wrappers remain intact;
- no live SA template registrations remain;
- no live EOPS Tax Source remains;
- shared cross-regime utilities required elsewhere have not been removed.

Report the focused diff and any surviving references that were intentionally retained.

---

## Required Output

Report:

1. exact files/components retired;
2. exact live references removed;
3. shared components retained and why;
4. build/validation results;
5. surviving ambiguous references requiring later review;
6. any gaps intentionally left for the next constructive phase.

Append implementation observations to `change-log.md`.

Do not rewrite historical findings.

---

## Completion Gate

Phase 4A is complete when the unsupported SA/EOPS architecture has been removed from the live Sole Trader path and the remaining solution builds from a cleaner, intentionally incomplete baseline.

Stop there.

Do not begin replacement cumulative/annual implementation without explicit approval.
