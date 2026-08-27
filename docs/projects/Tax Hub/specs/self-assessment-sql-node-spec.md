# Self Assessment SQL Node Implementation Specification

Trade Control Tax Hub Programme  
26 August 2026

## 1. Status and purpose

This document specifies the next SQL Node work required to support the Sole Trader Self Assessment submission models used by Tax Hub.

It is a review draft, not an instruction to implement. No SQL, mappings, commits, or submodule pointer changes are authorised by this document. Implementation may begin only after the proposed mapping matrix and the relevant phase have been reviewed and approved.

The immediate scope is the SQL bootstrap and classification layer for two distinct Self Assessment representations:

- MTD ITSA Self-Employment, comprising Quarterly Update and EOPS tax sources.
- Legacy annual Self Assessment, represented by the SA100/SA103F self-employment tax source.

This work contributes to Tax Hub Programme Objective 2: generation of internal raw tag-set test payloads from Trade Control accounting data. It does not specify HMRC-ready payloads, endpoint behaviour, transport, authentication, filing workflow, or submission history.

## 2. Governing principles

The implementation shall preserve the following programme principles:

- Existing accounting calculations remain authoritative.
- The Cash Statement remains the primary operational financial representation.
- Tax Hub consumes the tax classification layer; it does not reinterpret transactions or reproduce accounting calculations.
- Every mapped statutory value must be deterministically traceable to its operational source.
- Structural, numerical, and submission concerns remain separate.
- MTD and SA100/SA103F are separate statutory vocabularies even where they describe similar accounting concepts.
- Absence of a legitimate source is represented explicitly. It is not resolved through a speculative or approximate mapping.

## 3. Repository and submodule boundaries

The working checkout is the live `tradecontrol.web` superproject, including its populated submodules. That complete checkout is authoritative for implementation. Its relevant submodules are independent repositories with independent histories:

- `src/sqlnode` owns the SQL schema, accounting bootstrap, tax sources, tax tags, and mappings addressed by this specification.
- `src/hmrc_mtd` owns the Self Assessment submission models, serializers, payload-building logic, and later HMRC API and transport concerns. For this phase it is an authoritative reference for the consumer-facing tag vocabulary and shape.
- `tradecontrol.web` owns the Tax Hub UI and workflow and is outside the immediate implementation scope.
- `src/TCExports` is not relevant to this specification.

Submodule boundaries are repository boundaries. Work inside `src/sqlnode` must not be treated as an ordinary edit to the superproject. Reconnaissance may inspect the complete `tradecontrol.web` checkout, including every populated submodule, wherever necessary to establish contracts, dependencies, tests, and current behaviour. No implementation phase may modify `src/hmrc_mtd`, `tradecontrol.web`, or `src/TCExports` unless a separately reviewed and approved cross-repository requirement demonstrates that the contract itself must change.

The implementation write scope is `src/sqlnode` only. The reconnaissance scope is the complete `tradecontrol.web` checkout, including all populated submodules. Advancing a submodule pointer in `tradecontrol.web`, creating commits, coordinating releases, or making any cross-repository change are separate, explicit actions and must never occur as a side effect.

## 4. Evidence base and confidence

This specification was prepared with reference to:

- `tax-hub-spec-programme.md`, which defines the programme objectives, separation of concerns, classification layer, validation duties, and deterministic reconciliation requirement.
- `session-brief.md`, which identifies the intended template/tax-procedure split and the unfinished structural and mapping work.
- Known current-state context that `@IsMTD` has already been removed from the repository. Live reconnaissance must verify that state; it must not treat the parameter as a presumed current defect or implementation task.

The live `tradecontrol.web` checkout, including its populated submodules, is authoritative for implementation. At the start of the work, Codex must inspect the relevant live files identified in the Appendix of `session-brief.md`, then follow dependencies and references anywhere in the checkout as necessary. The Appendix is an entry map, not a fence. Procedure names, signatures, wrapper call graphs, tag inventories, transaction boundaries, schema constraints, validation procedures, category hierarchy objects, `hmrc_mtd` builders and models, tests, and other dependencies must be established from live repository evidence.

## 5. Established current state

### 5.1 Accounting templates

The Sole Trader template family includes:

- `App.proc_Template_ST_SOLE_CUR_MIN_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_2026`

The MIN template builds the core Sole Trader environment after invoking the base minimum template. The evidence shows core reporting totals including:

- `CT-TURNOV` — Turnover
- `CT-OTHRIN` — Other Income
- `CT-CSTSAL` — Cost of Sales
- `CT-STAFFC` — Staff Costs
- `CT-OVERHD` — Overheads
- `CT-GROSSP` — Gross Profit
- `CT-PANDL` — Profit and Loss

These totals roll up nominal categories such as `CA-SALES`, `CA-INCOME`, `CA-DIRECT`, `CA-WAGES`, and `CA-ADMIN`. The base model also contains individual cash codes within those nominal categories.

The STD template extends the accounting model. The review-draft evidence indicates additional expense classification through cash codes and nominal categories, including:

- travel and transport through `CA-TRAVEL`;
- motor expenses through `CA-MOTOR`;
- finance costs through `CA-FINANCE`;
- premises running costs through `CA-PREMS`;
- more specific administrative cash codes for phone, insurance, bank charges, professional fees, advertising, and repairs.

These STD additions roll into `CT-OVERHD`. Consequently, mapping both an overhead parent and one or more of its descendants to additive statutory fields may double count the same operational value.

### 5.2 Legacy tax material in accounting templates

The review-draft evidence identifies tax concerns inside the accounting templates, to be confirmed against the live checkout:

- MIN section 9 creates MTD tax sources and obsolete tag seeds.
- MIN section 10 maps a small set of MTD tags to category totals and invokes validation.
- STD section 7 contains additional MTD mappings and invokes validation.

The old tag names do not consistently match the dedicated tax-procedure vocabularies. Examples include `otherIncome` versus `otherBusinessIncome`, `wagesSalaries` versus `wagesSalariesStaffCosts`, and several STD mappings to tags that do not occur in the current dedicated QU/EOPS seeds. This material is historical evidence of mapping intent only. It is not an approved mapping matrix and must not be mechanically relocated.

### 5.3 Dedicated tax procedures

The following procedures exist:

- `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026`
- `App.proc_Template_ST_SOLE_CUR_TAX_SA_2026`

The MTD procedure defines:

- `UK-ITSA-SE-QU`, with Quarterly Update income and expense tags;
- `UK-ITSA-SE-EOPS`, with basis-period, adjustment, capital-allowance, loss, and derived-total tags.

The SA procedure defines:

- `UK-SA-SE-RETURN`, with the canonical SA103F self-employment tag set used by the submission layer.

Both dedicated procedures contain mapping placeholders and invoke `Cash.proc_TaxTagMapValidate`. Their tax-source and tag-seed definitions are treated as the intended current vocabularies, subject to verification against the live `hmrc_mtd` consumers.

The SA procedure's placeholder comment refers to “QU + EOPS mappings”; that wording is stale and should not be treated as design intent for the SA source.

### 5.4 Composition wrappers

Four wrapper procedures exist:

- `App.proc_Template_ST_SOLE_CUR_MIN_MTD_2026`
- `App.proc_Template_ST_SOLE_CUR_MIN_SA_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_MTD_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_SA_2026`

The review-draft evidence indicates that they call their corresponding MIN or STD accounting template but do not call the dedicated tax procedure. Known current-state context is that the obsolete `@IsMTD` forwarding argument has already been removed. Phase 0 must verify both points in the live checkout; unless live reconnaissance shows otherwise, no work concerning that parameter is required.

### 5.5 What is not yet established

The evidence does not establish an approved, complete mapping matrix. In particular, it does not prove:

- that every statutory tag should be mapped;
- that a similar name means equivalent accounting semantics;
- that MIN and STD should map the same statutory fields at different levels of detail;
- that adjustment, loss, allowance, date, or derived tags have legitimate sources in the current Category Tree;
- that `Cash.proc_TaxTagMapValidate` alone detects completeness, double counting, or semantic errors;
- that all dedicated tag seeds and `hmrc_mtd` model properties remain perfectly aligned in the live repositories.

These are questions for reconnaissance and review, not assumptions for implementation.

## 6. Required end state

The completed design shall observe this composition model:

1. The MIN and STD accounting templates create only the accounting environment appropriate to their variant.
2. The dedicated MTD and SA procedures create only their respective tax sources and canonical tag vocabularies.
3. Each wrapper composes exactly one accounting template with exactly one tax regime.
4. Variant-specific mappings are owned by the wrapper because the wrapper is the point at which an accounting model and a statutory vocabulary are combined.
5. Validation runs only after the relevant tax source, tags, and approved mappings all exist.

The required wrapper outcomes are:

| Wrapper | Accounting template | Tax procedure | Mapping scope |
|---|---|---|---|
| MIN MTD | MIN | MTD | Approved mappings supported by the MIN classification model |
| MIN SA | MIN | SA | Approved mappings supported by the MIN classification model |
| STD MTD | STD | MTD | Approved mappings supported by the STD classification model, including valid inherited MIN coverage |
| STD SA | STD | SA | Approved mappings supported by the STD classification model, including valid inherited MIN coverage |

“Inherited MIN coverage” describes semantic reuse, not a requirement to duplicate or call another wrapper. Each wrapper must remain a coherent entry point and must not initialise an accounting template or tax vocabulary more than once.

The accounting templates shall contain no creation of Self Assessment tax sources, tax tags, tax-tag mappings, or validation calls. The dedicated tax procedures shall contain no mappings whose validity depends on choosing MIN or STD.

## 7. Mapping policy

### 7.1 Mapping is an accounting decision

Mappings determine the statutory meaning of operational values. They must be proposed from evidence and approved before insertion. Implementation must never infer a mapping solely from similar labels, an obsolete mapping, or apparent convenience.

For every tag in each of the three sources, the mapping analysis shall record:

- tax source code;
- tag code and description;
- tag class and whether it is raw, contextual, adjustment, or derived;
- MIN disposition;
- STD disposition;
- proposed source type: CategoryCode, CashCode, derived elsewhere, contextual input, or unmapped;
- proposed source code, where applicable;
- accounting rationale;
- roll-up and double-counting assessment;
- evidence reference;
- confidence and unresolved questions;
- explicit review decision.

### 7.2 Category versus cash-code mappings

A CategoryCode is appropriate only when the complete category total has the same statutory meaning as the tag. A CashCode is appropriate when the statutory field requires a narrower amount than its containing category supplies.

Preference for a category or cash code must be driven by semantics, not by a general hierarchy rule. For example, STD-specific expense cash codes may legitimately separate professional fees or loan interest from a broader administrative or finance category. Conversely, mapping all of `CT-OVERHD` to one tag while mapping descendants to other tags would require proof that the extraction and aggregation rules prevent duplication.

### 7.3 Roll-ups and double counting

Before approving any mapping, the analysis must trace the relevant `Cash.tbCategoryTotal` ancestry and the cash codes assigned beneath each category.

The matrix must identify overlapping mappings within a tax source. No set of additive statutory fields may receive both a parent total and its included child values unless the target model explicitly requires that relationship and the payload builder treats the parent as non-additive or derived.

MIN-to-STD differences must also be explicit. A coarse MIN classification may support fewer detailed fields than STD. The implementation must not manufacture precision by splitting a broad MIN total without a deterministic source.

### 7.4 Unmapped, contextual, and derived tags

Unmapped is a valid and sometimes required result. Tags representing basis-period dates, transitional adjustments, losses, private-use adjustments, capital allowances, or other values outside the Category Tree must remain unmapped unless a verified operational source exists.

The matrix must distinguish:

- genuinely unsupported values;
- values supplied by user or workflow context;
- values calculated by an existing authoritative service;
- derived totals that should be calculated from mapped components;
- optional fields that may correctly be absent.

A derived tag must not be mapped to an accounting total merely to make validation appear complete. Likewise, depreciation and capital allowances must not be treated as interchangeable without explicit accounting approval.

### 7.5 Cross-regime reuse

MTD QU, MTD EOPS, and SA103F may share concepts, but mappings shall be approved per tax source and tag code. A mapping may be reused only after confirming equivalent semantics in both statutory models.

Historical mappings in MIN section 10 and STD section 7 may be cited as evidence of earlier intent. They have no presumptive authority.

### 7.6 Validation semantics

`Cash.proc_TaxTagMapValidate` shall be invoked for every configured source after mappings have been inserted. Its actual guarantees must be inspected and documented.

Passing that procedure is necessary but not sufficient. Acceptance also requires checks for canonical tag existence, valid CategoryCode/CashCode references, duplicate mappings, overlap through category roll-ups, expected unmapped tags, and numerical traceability using representative data.

## 8. Bounded delivery phases

Each phase is separately reviewable. A later phase must not be folded into an earlier one merely because the files are already open.

### Phase 0 — Live-state verification

Read-only reconnaissance shall:

- begin with the relevant live files identified in the Appendix of `session-brief.md`, treating that Appendix as an entry map rather than a limit;
- follow dependencies and references anywhere in the complete `tradecontrol.web` checkout, including populated submodules, as necessary to establish the implementation contracts;
- confirm all procedure signatures and the removal of `@IsMTD`;
- confirm the four wrapper call graphs;
- inventory all three tax-source tag sets;
- compare those inventories with the live `hmrc_mtd` models and builders;
- inspect the tax mapping table constraints and validation procedure;
- trace the MIN and STD category/cash-code hierarchies;
- identify existing automated or repeatable bootstrap tests and follow any other repository trail relevant to the findings.

Deliverable: an evidence report listing confirmed facts, discrepancies, and open questions. No edits.

Gate: review confirms that this specification still matches the live repositories or approves amendments.

### Phase 1 — Structural separation

After approval, remove obsolete Self Assessment tax-source, tag, mapping, and validation material from the MIN and STD accounting templates. Correct misleading comments associated with the removed material. Preserve all unrelated accounting behaviour and transaction/error-handling conventions.

Deliverable: a mechanical structural change only. No new mappings and no speculative redesign.

Gate: review of the focused diff and successful creation of the tax-neutral MIN and STD accounting environments.

### Phase 2 — Wrapper composition

Update each wrapper so that it calls its accounting template and the matching dedicated tax procedure exactly once, in the order required for mappings to reference both accounting classifications and tax tags. Establish the intended validation position without adding unapproved mappings.

Transaction and failure semantics must be investigated before editing. The implementation must not assume that nested procedure transactions and caught errors provide atomic wrapper behaviour.

Deliverable: four correctly composed wrappers with no mapping decisions beyond approved structural scaffolding.

Gate: each wrapper independently produces only its intended accounting variant and tax-source vocabulary, with no cross-regime leakage or duplicate initialisation.

### Phase 3 — Mapping reconnaissance and proposal

Produce the complete mapping matrix defined in section 7 for:

- MIN MTD QU;
- MIN MTD EOPS;
- MIN SA103F;
- STD MTD QU;
- STD MTD EOPS;
- STD SA103F.

Where several tags compete for a broad accounting category, show the alternatives and their numerical consequences. Identify schema or classification gaps rather than silently extending the accounting model.

Deliverable: mapping matrix and issue log. No SQL mapping edits.

Gate: explicit human approval of each proposed mapping, each intentional unmapped disposition, and any prerequisite accounting-model change.

### Phase 4 — Approved mapping implementation

Implement only approved mappings in the appropriate wrappers. Do not add tags, categories, or cash codes unless they were separately approved as prerequisites. Ensure mapping insertion is deterministic and compatible with the bootstrap's expected rerun behaviour.

Invoke validation for the source or sources configured by each wrapper after mapping insertion.

Deliverable: reviewed SQL changes corresponding one-for-one with the approved matrix.

Gate: structural validation passes and the live mappings can be reconciled back to the approved matrix without undocumented exceptions.

### Phase 5 — Variant and reconciliation validation

Exercise all four wrappers in isolated, repeatable test databases. Verify object creation, tag inventories, mapping integrity, idempotency expectations, transaction behaviour, and representative numerical extraction.

Generate internal raw tag-set test payloads through the existing Objective 2 path where available. These are test-harness payloads, not HMRC submissions.

Deliverable: validation report containing commands or test cases used, results for every wrapper/source combination, expected unmapped fields, and any residual limitations.

Gate: all acceptance criteria below are met or each exception is explicitly accepted.

### Phase 6 — Repository integration

Only after the SQL Node work is approved should repository integration be considered. Commit creation in `sqlnode`, advancement of the `src/sqlnode` pointer in `tradecontrol.web`, and any coordinated `hmrc_mtd` versioning are distinct review actions.

Deliverable: an integration proposal identifying exact repositories and revisions. No implicit pointer update.

## 9. Acceptance criteria

### 9.1 Structural

- MIN and STD accounting templates contain no Self Assessment tax sources, tax tags, mappings, or tax-tag validation calls.
- The MTD procedure defines only `UK-ITSA-SE-QU` and `UK-ITSA-SE-EOPS` and their canonical vocabularies.
- The SA procedure defines only `UK-SA-SE-RETURN` and its canonical vocabulary.
- Every wrapper calls exactly one accounting template and exactly one matching tax procedure.
- MTD wrappers do not create SA tags; SA wrappers do not create MTD tags.
- No obsolete `@IsMTD` parameter or forwarding argument is reintroduced.
- Variant-dependent mappings reside in wrappers, not in accounting templates or variant-neutral tax procedures.

### 9.2 Mapping integrity

- Every implemented mapping appears in the approved matrix.
- Every mapping references an existing tag and exactly one valid operational source type.
- No mapping uses an obsolete or near-match tag name.
- Category roll-ups have been checked for overlap and double counting.
- MIN mappings do not depend on STD-only categories or cash codes.
- STD mappings use detailed sources only where that detail is semantically correct and deterministic.
- Unsupported, contextual, optional, and derived tags are explicitly classified rather than guessed.
- Validation passes for every configured tax source, and the limits of validation are documented.

### 9.3 Behavioural preservation

- Existing non-tax accounting bootstrap behaviour remains unchanged for both MIN and STD.
- Existing accounting calculations remain the source of reported values.
- VAT, owner capital, account creation, tax-year alignment, and other unrelated template behaviour are not altered by this work.
- Failure handling does not leave a partially configured accounting/tax environment under the tested execution paths.

### 9.4 Reconciliation and payload readiness

- Representative mapped values can be traced from raw tag output to CategoryCode or CashCode and onward to the underlying operational classification.
- Aggregate raw tag values reconcile with their approved accounting sources.
- Expected unmapped fields are visible and explained.
- All four wrapper variants can generate the intended internal tag-set shape without attempting HMRC transport.

### 9.5 Repository discipline

- Changes are confined to the approved `sqlnode` scope unless a separate cross-repository change is authorised.
- No unrelated user changes are overwritten.
- No submodule pointer changes or commits occur without explicit instruction.
- The final handoff identifies changes by repository, not merely by paths within the superproject.

## 10. Out of scope

This specification does not authorise:

- changes to HMRC endpoints, JSON/XML payload specifications, serializers, or transport;
- OAuth, fraud-prevention headers, Government Gateway envelopes, IRmark generation, or filing;
- Tax Hub UI or workflow changes;
- changes to the underlying accounting calculations;
- invention of new accounting classifications to obtain apparent mapping completeness;
- company-tax, VAT, or non-self-employment SA schedule work;
- automatic commits, pushes, pull requests, releases, or submodule pointer updates.

## 11. Review decisions required before implementation

Review should resolve the following:

1. Confirm wrapper ownership of variant-specific mappings, with dedicated tax procedures limited to source/tag vocabulary.
2. Confirm whether validation belongs in wrappers after mappings, or whether the validation calls in dedicated tax procedures should be moved or otherwise reorganised.
3. Confirm the canonical source of truth when SQL tag seeds and `hmrc_mtd` model properties disagree.
4. Approve the format and required evidence fields of the Phase 3 mapping matrix.
5. Decide whether any mapping incompleteness is acceptable for Objective 2 and how it should surface as PASS, WARN, or FAIL.
6. Confirm the required bootstrap rerun/idempotency behaviour and the transaction boundary expected across composed procedures.

Until these decisions and the Phase 3 matrix are approved, the work remains specification and reconnaissance only.

## 12. Definition of completion

This implementation is complete when the four Sole Trader wrappers deterministically compose the correct accounting variant with the correct Self Assessment vocabulary; all and only approved mappings are installed at the composition boundary; mapping and numerical validation demonstrate traceability without double counting; internal Objective 2 tag-set payloads can be produced for the supported variants; and repository boundaries have been preserved.

Completion of this SQL Node work does not imply completion of HMRC payload specification, transport, or Tax Hub filing workflow.
