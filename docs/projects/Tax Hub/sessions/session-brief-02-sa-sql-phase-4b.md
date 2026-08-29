# Tax Hub — Session Brief

## Phase 4B: Cumulative Sole Trader Projection Design

### Purpose

Design the first constructive Sole Trader MTD slice from the clean Phase 4A baseline.

This session is reconnaissance and proposal only.

The target is the current HMRC cumulative self-employment submission journey and the minimum truthful Trade Control statutory projection required to supply it.

Do not implement the proposed design.

---

## Governing Material

Read:

- the current Tax Hub programme specification;
- `specs/self-assessment-sql-node-spec.md`;
- `specs/reference/sole-trader-field-sets.md`;
- `specs/tax-hub-test-payloads.md`;
- `findings.md`, especially Phase 3;
- `change-log.md`, including Phase 4A.

Treat:

1. current authoritative HMRC contracts as authority for Objective 3;
2. current Trade Control accounting semantics as authority for Trade Control data;
3. the governing specifications as authority for the Objective 2 / Objective 3 boundary;
4. historical implementation as evidence only where it remains consistent with those authorities.

For HMRC-dependent findings, record the authoritative HMRC sources used and their current version/date where available.

---

## Current Position

Phase 4A has retired the positively obsolete Sole Trader SA/EOPS architecture.

The surviving implementation is intentionally incomplete.

Do not restore or replace retired architecture merely to fill gaps.

In particular:

- legacy SA100/SA103F submission is unsupported;
- EOPS is not a supported filing stage;
- the surviving `UK-ITSA-SE-QU` vocabulary is historical pending replacement;
- no replacement cumulative or annual Tax Source has yet been approved;
- no current Objective 3 cumulative contract has yet been implemented;
- the Test Harness is diagnostic infrastructure, not an alternative contract layer.

This session concerns the cumulative self-employment journey only.

Annual adjustments, allowances, losses and finalisation remain separate later work except where their boundaries must be identified to prevent them contaminating the cumulative design.

---

## 1. Establish the Current HMRC Cumulative Contract

Verify the exact current HMRC Self Employment Business MTD cumulative submission contract that Trade Control intends to support.

Record:

- service and API version;
- endpoint and HTTP method;
- supported tax-year applicability;
- exact request structure;
- required and optional properties;
- income fields;
- expense fields;
- disallowable-expense fields;
- detailed versus consolidated-expense rules;
- period/date semantics;
- contextual identifiers required by the endpoint;
- relevant validation and mutual-exclusion rules.

Distinguish explicitly between:

- fields that require an Objective 2 statutory projection;
- fields supplied as endpoint/context metadata;
- optional HMRC properties which do not justify creating mandatory Trade Control Tax Tags;
- fields which Trade Control cannot currently derive deterministically.

Do not assume that every HMRC request property must become a Tax Tag.

---

## 2. Define the Minimum Objective 2 Cumulative Projection

Using the verified HMRC contract and the existing Trade Control accounting model, propose the minimum statutory projection required to truthfully support cumulative submission.

For each proposed concept classify it as appropriate, for example:

- deterministic accounting projection;
- contextual;
- conditional;
- externally supplied;
- unsupported;
- derived outside Tax Tag mapping.

Do not invent mappings or substitute zero for information that Trade Control does not possess.

Reconcile the proposal with the existing documented core cumulative accounting concepts and explain any proposed addition or omission.

Do not design the annual statutory projection in this session.

---

## 3. Inspect Trade Control Polarity

`Cash.vwTaxBizSubmission` is intentionally a Trade Control projection.

Its monetary values therefore retain Trade Control economic polarity.

For example, an expense cash outgoing may appear as:

`-1000`

while a credit/reversal against the same expense classification may appear as:

`+100`

The statutory representation must preserve this distinction.

For an expense-oriented statutory field the intended behaviour is conceptually:

`-1000 -> +1000`

`+100 -> -100`

and therefore:

`-1000 + 100 -> statutory expense 900`

`Math.Abs` is not an acceptable general polarity transformation because it can convert a credit/reversal into additional expenditure.

Inspect the live SQL path:

`Cash.tbTaxTag`
→ `Cash.tbTaxTagMap`
→ Cash Code and/or Category mapping
→ Category
→ Polarity
→ tax-business projection views
→ `Cash.vwTaxBizSubmission`
→ `TcBusinessTaxReader`

Determine:

- where polarity is currently encountered;
- whether the polarity governing a Tax Tag can be derived unambiguously from its mappings;
- how direct Category mappings differ from Cash Code mappings;
- whether multiple contributors to one Tax Tag can resolve to conflicting polarities;
- how recursive Category expansion affects polarity;
- whether `Cash.vwTaxBizSubmission` can expose the applicable Trade Control polarity as part of its existing projection without a second database traversal;
- whether that is preferable to a separate polarity lookup function;
- where the Trade Control value/polarity should be converted into the Objective 2 statutory value.

Treat exposing polarity through `Cash.vwTaxBizSubmission` as a candidate design, not a pre-authorised implementation.

Do not modify the view or remove `Math.Abs` in this session.

Propose validation rules required to prevent ambiguous Tax Tag polarity.

---

## 4. Inspect Cumulative Period Semantics

Trace how the current SQL projection determines:

- tax periods;
- start/end boundaries;
- due-date periods;
- aggregation boundaries.

Compare this with the current HMRC cumulative-update semantics.

Determine what must change, if anything, so that Objective 2 can produce a cumulative tax-year-to-update-period projection without corrupting the existing accounting-period model.

Do not implement period changes.

---

## 5. Reassess MIN and STD Mapping Feasibility

Against the proposed minimum cumulative projection, reassess the Phase 3 MIN and STD mapping candidates.

For each cumulative accounting concept classify the bootstrap position as:

- supported deterministically;
- conditional / requires further classification;
- unsupported.

Identify the exact CategoryCode and/or CashCode evidence for any proposed deterministic mapping.

Check:

- recursive Category expansion;
- parent/descendant overlap;
- multiple disjoint contributors;
- cross-tag overlap;
- polarity consistency;
- possible double counting.

Do not add mappings.

Absence of deterministic evidence must remain absence.

---

## 6. Define the Objective 2 / Objective 3 Seam

Propose the smallest clean boundary between the Trade Control statutory projection and the HMRC contract adapter.

The proposal should make clear which layer owns:

- Trade Control accounting values;
- Trade Control polarity;
- statutory polarity conversion;
- cumulative aggregation;
- Tax Tag vocabulary;
- contextual endpoint data;
- HMRC-required property names;
- HMRC-required zero values where the external contract genuinely requires them;
- serialization.

Objective 2 must not become an HMRC wire DTO.

Objective 3 must not reinterpret raw Trade Control accounting semantics.

---

## 7. Propose the First Constructive Implementation Slice

Based on the reconnaissance, propose one bounded implementation slice that would establish:

`Trade Control accounting`
→ `Objective 2 cumulative statutory projection`
→ `Objective 3 cumulative HMRC request`
→ `exact serialization`
→ `Test Harness inspection`

Transport remains out of scope.

The proposal must identify:

- SQL objects to change or create;
- C# projection/reader changes;
- Objective 3 contract classes required;
- serializer requirements;
- Test Harness observation point;
- synthetic test cases;
- validation required before implementation is accepted.

Include synthetic polarity cases covering at least:

1. ordinary income;
2. ordinary expense;
3. expense credit/reversal;
4. net expense after a partial credit;
5. expense classification whose credits exceed expenditure.

The Test Harness must exercise the production projection and Objective 3 classes. It must not create a parallel payload vocabulary.

---

## Constraints

- Reconnaissance and proposal only.
- No SQL changes.
- No C# changes.
- No Tax Source or Tax Tag changes.
- No mappings.
- No DTO implementation.
- No serializer implementation.
- No Test Harness implementation.
- No transport work.
- No annual implementation.
- No loss implementation.
- No finalisation implementation.
- No VAT or Corporation Tax work.
- No commits or pushes.

Record out-of-scope discoveries rather than fixing them.

---

## Required Output

Append durable findings to `findings.md`.

Report:

1. verified current HMRC cumulative contract;
2. HMRC source/version provenance;
3. proposed minimum Objective 2 cumulative field set;
4. field classification and Objective 2 / Objective 3 ownership;
5. polarity-path findings and proposed polarity rule;
6. cumulative-period findings;
7. MIN mapping assessment;
8. STD mapping assessment;
9. validation requirements;
10. proposed Objective 2 / Objective 3 seam;
11. exact bounded implementation proposal for the first cumulative vertical slice;
12. unresolved questions requiring human decision.

Do not rewrite previous findings or implementation history.

---

## Completion Gate

Phase 4B is complete when there is enough evidence to decide whether to authorise the first cumulative Sole Trader implementation slice.

Stop there.

Do not implement the proposal without explicit approval.
