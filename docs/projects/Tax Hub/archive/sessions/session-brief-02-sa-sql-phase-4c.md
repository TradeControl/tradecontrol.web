# Tax Hub Session Brief

## Phase 4C — MIN/STD Cumulative Bootstrap Reconnaissance

### Purpose

Perform a second, read-only reconnaissance of the Sole Trader MTD cumulative projection following Phase 4B.

The objective is to turn the Phase 4B findings and the decisions below into an implementation-ready proposal.

No implementation is authorised.

## Decisions

The following are approved architectural decisions:

- MIN and STD remain bootstrap Category Trees; they are not permanent statutory modes.
- STD continues to build on MIN.
- MIN will support the consolidated-expenses cumulative reporting pattern.
- STD will refine MIN sufficiently to support the detailed-expenses cumulative reporting pattern.
- MIN may map a consolidated expense parent; STD may instead map its appropriate descendants. Parent and descendant mappings must not double count.
- Users remain free to modify their Category Trees. Submission capability depends on the resulting mappings and validation, not the original bootstrap selected.
- The Category Tree remains a Trade Control accounting/business classification, not an HMRC taxonomy.
- Quarterly expense classification is accounting information. Do not duplicate the Category Tree into HMRC disallowable-expense classifications merely because those properties exist in the API.
- Tax-specific adjustments and allowances belong to later annual/finalisation work unless current HMRC authority proves otherwise.
- Objective 2 converts Trade Control economic polarity into statutory polarity. Income retains its sign; expense is multiplied by `-1`. `ABS()` / `Math.Abs()` is not valid.
- Effective polarity comes from contributing leaf Cash Codes, not neutral structural parent categories.

## Reconnaissance

Inspect the live SQL, MIN/STD bootstraps and relevant `hmrc_mtd` code and append a Phase 4C report to `findings.md`.

Determine and propose:

1. the minimum changes required to make MIN a truthful consolidated cumulative bootstrap;
2. the changes required for STD, inherited from MIN, to support the current detailed cumulative expense categories;
3. the proposed cumulative Tax Source and stable Objective 2 Tax Tag vocabulary;
4. complete proposed MIN and STD mapping matrices;
5. how MIN parent mapping and STD descendant mappings remain mutually exclusive;
6. the smallest way to propagate existing leaf `CashPolarityCode` through the SQL projection;
7. the validation required for polarity, overlap, completeness and customised Category Trees;
8. the appropriate cumulative-period query/interface, without repurposing the existing discrete-quarter due-date machinery;
9. the smallest Objective 2 result contract required by Objective 3;
10. any remaining genuine unsupported/contextual information and human decisions.

Reconcile the proposed STD detailed vocabulary against the current authoritative HMRC cumulative-update requirements and API contract. Do not assume every optional HMRC API property requires a Tax Tag.

Treat existing MIN/STD classifications as prototypes. There are no production-user or backwards-compatibility constraints: propose additions, removals, moves or redesign where this produces a better accounting/statutory mapping.

Record authoritative HMRC provenance for contract-dependent conclusions.

## Deliverable

Append the Phase 4C findings to `docs/projects/Tax Hub/findings.md`, including:

- proposed MIN and STD structures;
- Tax Source/Tag vocabulary;
- mapping matrices;
- polarity and validation design;
- cumulative-period design;
- Objective 2/3 seam;
- files likely to change;
- unresolved decisions;
- proposed bounded Phase 4D implementation.

Clearly distinguish current implementation, proposed design, HMRC requirement and Trade Control architectural decision.

## Constraints

Reconnaissance may inspect the complete superproject and submodules.

Do not modify production source, SQL, Test Harness, specifications, historical material or `change-log.md`.

Do not commit or push.

Stop after appending the Phase 4C report to `findings.md`.
