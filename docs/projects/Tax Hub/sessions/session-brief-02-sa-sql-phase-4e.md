# Codex Session Brief — Tax Tag Validator Correction and Reconnaissance

## Objective

Correct the current Tax Tag mapping validator only within the explicitly authorised generic scope. Do not continue into Objective 3 or unrelated Tax Hub implementation.

## Start Here

Before editing anything:

1. Read the revised `sole-trader-field-sets.md`
2. Review the latest `findings.md` and `change-log.md` in full.
3. Inspect the current `Cash.fnTaxTagMapValidate` implementation and its caller/procedure pair.
4. Compare current behaviour with the original validator evidence, including the supplied original function spool where available.
5. Inspect the schema and category-tree semantics used by Tax Tag mappings, especially `Cash.tbTaxTag`, `Cash.tbTaxTagMap`, `Cash.tbCategoryTotal`, relevant views, enablement rules, polarity fields and nominal-leaf resolution.

Record concise findings before making a behavioural change. Treat the revised field-set reference as authoritative for Sole Trader Tax Tag semantics and classification. Existing SQL is implementation evidence, not authority where it conflicts with that reference.

## Agreed Tax Tag Class Contract

`TagClassCode` is a mapping eligibility and statutory-value classification:

| Code | Class | Contract |
|---:|---|---|
| `0` | Rollup | Calculated/aggregate statutory field; informational from the mapping perspective; cannot be mapped. |
| `1` | Component | Business accounting value; the only class permitted in `Cash.tbTaxTagMap`. |
| `2` | Derived | Sole-trader statutory/finalisation value outside the individual Business Node/business balance-sheet accounting projection; may involve taxpayer-level context, multiple businesses, or a Tax Hub workflow/interface; cannot be mapped. |

Do not use `Derived` as a generic source classification. Keep Tag Class separate from source/support classification. Do not force Derived into limited-company taxonomy without a demonstrated requirement. Identify any Derived tags only where they are **necessary to complete a valid supported MTD submission path**. Otherwise leave the mechanism unused.

## Authorised Validator Scope

Preserve useful generic validator behaviour and implement only what evidence supports:

- Reject any mapping to a Rollup or Derived tag.
- Validate Component mappings generically: references, enabled/eligible roots and resolved nominal contributors, overlap/double-counting invariants, and other established generic integrity rules.
- Validate configured `StatutoryPolarityCode` against actual Component contributors wherever the schema contract makes that applicable.
- Keep the validator source-agnostic and data-driven.

Remove or avoid:

- hard-coded HMRC tag names or manifests;
- exact HMRC tag-count assertions;
- hard-coded consolidated-versus-detailed readiness rules;
- source-specific bootstrap correctness tests inside the generic validator.

Exact HMRC bootstrap/tag-set correctness is a development/bootstrap test concern. If such tests are missing, identify that gap and propose the appropriate test location; do not reproduce the manifest in the generic validator.

## Required Reconnaissance: Lost Original Invariants

Identify every original generic invariant lost during the current rewrite. Pay particular attention to the original warning for enabled CashCodes not mapped to any tag for the selected source.

Do not restore or rewrite that warning mechanically. First determine from the actual category schema:

- how CategoryCode mappings expand through `Cash.tbCategoryTotal`;
- which direction represents ancestry and descendant nominal leaves;
- what enabled, eligible and neutral CashCodes mean;
- whether an unmapped CashCode is genuinely disconnected or is covered indirectly through a mapped category subtree;
- whether disconnected accounting branches are intentionally outside the selected tax source;
- whether current views already encode effective coverage.

`TagClassCode` gates the tax-tag side: only Component tags participate in mapping. It does not justify inventing a second Tax Tag recursion unless the schema explicitly contains such a relationship.

After inspecting the tree semantics, preserve or correct the warning so it reports genuinely relevant disconnected nominal leaves, not merely CashCodes lacking a direct mapping. If the intended definition of “relevant” or “disconnected” cannot be proved from schema, code and existing specifications, document the ambiguity and stop before making that behavioural change.

## Constraints

- Preserve unrelated existing work and useful generic validation.
- Do not alter the canonical quarterly 15 accounting tags.
- Do not redesign Tax Tag classes, source/support storage, bootstrap manifests or limited-company taxonomy in this session.
- Do not infer new readiness rules.
- Make the smallest evidence-backed correction.
- Update `findings.md` and `change-log.md` with the inspected evidence, decisions, exact edits and verification results.

## Verification and Deliverable

Run the relevant build/tests and focused validator checks. At minimum demonstrate:

- Component mappings remain valid when their references, coverage and configured polarity are valid;
- Rollup and Derived mappings are rejected;
- no HMRC literal manifest/count/readiness knowledge remains in the generic validator;
- overlap/double-counting and other retained generic invariants still behave as intended;
- unmapped/disconnected CashCode warnings match the proven category-tree semantics, or are explicitly left unchanged pending authority.

Report:

1. evidence reviewed;
2. original invariants found, retained, lost or corrected;
3. files changed;
4. tests and results;
5. any narrowly scoped unresolved question requiring approval.
