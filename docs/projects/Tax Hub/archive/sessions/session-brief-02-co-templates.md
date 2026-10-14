# Tax Hub — Company Objective 2 Rewrite

4 August 2026

Proceed with Company Objective 2 implementation.

## Authorities and existing-code evidence

Use these sources in this order.

### Statutory and architectural authority

1. The implemented Company Objective 3 contracts in:
   `src/tax-hub/src/TradeControl.Tax.UK.Company.Contracts`

2. The approved design:
   `docs/projects/Tax Hub/implementation/company-statutory-contract-design.md`

3. The statutory research:
   `docs/projects/Tax Hub/specs/reference/company-field-sets.md`

These define what the current company statutory projection must mean.

### Existing implementation evidence

4. The existing company reconnaissance in:
   `docs/projects/Tax Hub/findings.md`

   Read the Corporation Tax / Limited Company findings before changing the company bootstrap.

   In particular, reuse its established evidence concerning:

   - `App.proc_Template_CO_MICRO_CUR_2026`
   - the COMIN26 and COSTD26 wrapper call graphs
   - the existing `UK-MTD` source
   - the complete AC/CP tag inventory
   - MIN and STD mapping roots and effective contributors
   - depreciation differences between MIN and STD
   - current Tax Tag schema conflicts
   - existing extraction/read paths
   - the historical `SubmitMicro` harness
   - known structural defects and suspicious legacy behaviour

   Do not repeat repository archaeology already established there unless the live code has changed or implementation requires verification of a particular finding.

   `findings.md` is evidence about the existing Trade Control implementation, not statutory authority. Where it conflicts with the implemented Company Objective 3 contracts, approved company design, or authoritative statutory research, the latter govern.

$14. The live SQL schema and current company MIN/STD templates in `src/sqlnode`.

   The live code remains the final authority for what actually exists today. Verify any finding that materially affects a destructive or structural change before acting on it.

The existing AC/CP / `UK-MTD` company tags and mappings are obsolete Government Gateway-era vocabulary and must not be preserved merely for compatibility. The product is unreleased.

## Objective

Rewrite the company Objective 2 statutory projection so that the Trade Control accounting templates project cleanly into the current Company Objective 3 semantic/contracts model for the approved ordinary UK private micro-company profile.

The result should establish the current:

- Company Tax Source(s)
- Tax Tags / statutory semantic projection identifiers
- Tag classes and support/status semantics
- MIN mappings
- STD mappings
- any Category Tree changes genuinely required to make those mappings deterministic
- company template/bootstrap composition
- repeatable validation and isolated runtime verification

Do not derive external statutory semantics from the old AC/CP tags.

## Architectural rules

Keep ordinary accounting bootstrap separate from statutory projection.

The current `proc_Template_CO_MICRO_CUR_2026` mixes accounting bootstrap with obsolete `UK-MTD` Tax Source/Tag/Map creation. Refactor this so that the accounting template remains accounting-oriented and the company statutory projection is composed separately, following the same architectural lesson already established for Sole Trader Objective 2.

Do not force the new company projection into the Sole Trader flat-field shape where the Company Objective 3 contracts require richer semantics.

Use the implemented Company contracts to determine what Trade Control must be able to supply.

Where a statutory fact cannot be deterministically obtained from the current Category Tree, do not invent a mapping. Classify it appropriately as contextual, derived, externally supplied, optional/absent, unsupported, or equivalent according to the existing Tax Hub conventions.

A Trade Control accounting total may map to a writable statutory Component where semantically correct. Do not confuse accounting aggregation with statutory TagClass behavior.

Only directly writable/supplyable statutory Components may appear in `Cash.tbTaxTagMap`.

Preserve explicit zero versus absent semantics where the company contracts distinguish them.

Do not use blanket `ABS()` or sign manipulation merely to resemble historical Government Gateway output. Polarity and magnitude must follow the current statutory contract and Trade Control accounting semantics.

## MIN and STD

Treat MIN and STD as accounting bootstrap profiles, not statutory taxonomies.

MIN should remain genuinely minimal while still supporting the approved company profile as far as its accounting structure deterministically permits.

STD may provide finer deterministic mappings where its richer Category Tree supports them.

Do not add accounting categories merely to mirror every statutory field. Add or restructure Category Tree nodes only where there is a sound accounting/business reason and where doing so enables a deterministic statutory projection.

Avoid parent/descendant double counting.

Preserve useful non-tax accounting behavior and expressions unless a change is required by the new Category Tree or statutory projection.

## Existing material to retire

The existing company projection includes obsolete material such as:

- Tax Source `UK-MTD`
- AC12 / AC405 / AC410 / AC415 / AC420 / AC425 / AC34 / AC435
- CP28 / CP46
- historical depreciation mappings based on those codes

Replace these with the current Objective 2 projection derived from the Company contracts.

Do not retain compatibility aliases unless a live dependency is found and documented.

## Scope boundaries

In scope:

- `src/sqlnode` company template/bootstrap SQL
- Company Tax Source / Tax Tag / Tax Tag Map definitions
- MIN/STD Category Tree changes required for deterministic projection
- company tax projection functions/procedures if required
- validation
- isolated disposable-database runtime verification
- relevant Objective 2 documentation / findings / change log

Out of scope:

- changes to Company Objective 3 contracts unless a genuine contract defect is discovered
- Tax Hub population/application orchestration
- Companies House or HMRC transport
- credentials/authentication
- Objective 4 conformance testing
- unrelated VAT cleanup unless a defect directly blocks company template verification
- Sole Trader Cash Statement repair

## Implementation method

Begin from the existing company reconnaissance in `findings.md`; do not restart Phase 1A archaeology.

Verify the findings against live code where necessary, then reconcile the implemented Company Objective 3 contracts against the existing MIN/STD accounting structures and classify each required company statutory semantic value as:

- directly deterministically supplied by current accounting structure
- supplied after a justified Category Tree refinement
- contextual/workflow
- derived
- external/user supplied
- optional absent
- unsupported for the initial profile

Then implement the approved projection from that reconciliation.

Do not stop at a proposal unless an authoritative ambiguity genuinely blocks correctness. This instruction authorises the Objective 2 rewrite.

Use disposable isolated databases to execute the company bootstrap procedures and verify the resulting accounting tree, Tax Sources, Tax Tags, mappings, rerun behavior and validator output.

Run the SQL project build and any relevant Tax Hub contract/build checks required to prove the boundary remains intact.

Report:

- files changed
- new Company Tax Source and statutory projection
- MIN mapping coverage
- STD mapping coverage
- unsupported/contextual/derived values and why
- Category Tree changes and rationale
- obsolete AC/CP material removed
- runtime/bootstrap validation evidence
- any genuinely authoritative gaps or deferred Objective 4 matters

Do not stage, commit, push, amend, reset, revert, stash, or otherwise alter Git history or index state. Leave all changes unstaged in the working tree for user review. The user will perform Git operations separately.
