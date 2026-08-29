# Self Assessment SQL Node — Phase 0 Live-State Findings

Date: 26 August 2026  
Scope: read-only reconnaissance under `docs/tmp/session-brief.md` and `docs/tmp/self-assessment-sql-node-spec.md`  
Authoritative checkout revisions: `src/sqlnode` `eafcc683b1e975cbdfaa9bc2a9d65d2ada722813`; `src/hmrc_mtd` `898cf15f7a136f9846e4edde1120ce04036f0a02`

## 1. Executive conclusion

Following review, the evidence **supports proceeding to a tightly bounded Phase 1**. The broad structural diagnosis is correct, and the reviewed clarifications resolve the previously identified Phase 1 gate concerns without changing the underlying repository facts:

1. `@IsMTD` has not been fully removed. It is absent from the MIN and STD accounting-template signatures, but MIN MTD and STD SA still pass it. Those wrapper calls therefore do not match the live callee signatures.
2. All four wrappers call only an accounting template. None calls `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026` or `App.proc_Template_ST_SOLE_CUR_TAX_SA_2026`.
3. The dedicated MTD seed contains 15 QU tags and 25 EOPS tags. The development-only `HMRC.WebHarness` builders currently expect the older 17-tag QU and 78-tag EOPS vocabularies. This is useful evidence of unfinished alignment, but the harness is not authoritative for this wave and does not define or block SQL structural separation.
4. The dedicated SA seed contains 41 tags, while `Sa103F`/`Sa103FSerializer` expose a substantially larger and differently named shape. No live SQL-tag-to-`Sa103F` builder was found.
5. The inspected SQL enquiry script mutates mappings by default, uses obsolete tag names, and selects `IsEnabled` columns that do not exist on `Cash.tbTaxTagSource` or `Cash.tbTaxTag`. It is located in the non-authoritative `Scripts` developer scratch area and is not part of the implementation contract or acceptance suite.

Review has authorised removal of the two stale `@IsMTD` forwarding arguments as minor live-defect cleanup in Phase 1. Canonical QU, EOPS, and SA vocabularies will be settled separately through an HMRC contract audit and alignment exercise before Phase 3 mapping. Phase 1 remains limited to structural separation and this approved cleanup; it must not add mappings or change consumer contracts.

## 2. Repository state and evidence method

- The superproject had a pre-existing modification to `src/tradecontrol.web.sln`. It was not touched.
- Both relevant submodules were clean at the start of reconnaissance and matched the superproject gitlinks listed above.
- No source, project, submodule pointer, commit, database, or external system was changed. This report is the sole write.
- Evidence was gathered from the Appendix entry points and followed through SQL tables, functions, views, bootstrap registration, developer scratch material, C# harness builders/readers/validators, MTD models, SA103F model/serializer, and project definitions. Scratch and harness evidence is contextual rather than authoritative for implementation scope.
- No database instance or representative populated test database was identified or executed. Findings about runtime results are therefore static-code findings, explicitly marked where relevant.

## 3. Procedure signatures and wrapper call graphs

### 3.1 Confirmed signatures

The following six accounting/wrapper procedures all declare the same 12 parameters: `@FinancialMonth SMALLINT = 4`, required `@GovAccountName NVARCHAR(255)`, optional bank fields, required `@DummyAccount NVARCHAR(50)`, optional current/reserve account fields, and `@IsVATRegistered BIT = 0`:

- `src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql`
- `.../proc_Template_ST_SOLE_CUR_STD_2026.sql`
- `.../proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql`
- `.../proc_Template_ST_SOLE_CUR_MIN_SA_2026.sql`
- `.../proc_Template_ST_SOLE_CUR_STD_MTD_2026.sql`
- `.../proc_Template_ST_SOLE_CUR_STD_SA_2026.sql`

Neither accounting template declares `@IsMTD`. Both dedicated tax procedures are parameterless:

- `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026`
- `App.proc_Template_ST_SOLE_CUR_TAX_SA_2026`

### 3.2 Confirmed live call graph

| Entry point | Direct call | Tax-procedure call | Live discrepancy |
|---|---|---|---|
| MIN MTD wrapper | MIN accounting template once | none | passes nonexistent `@IsMTD = 1` |
| MIN SA wrapper | MIN accounting template once | none | no SA source/tags are composed |
| STD MTD wrapper | STD accounting template once | none | gets legacy MTD material only through STD -> MIN |
| STD SA wrapper | STD accounting template once | none | passes nonexistent `@IsMTD = 0`; no SA source/tags are composed |
| STD accounting template | MIN accounting template once | n/a | comment says MIN includes ITSA sources/tags; currently true |
| MIN accounting template | BASE MIN once | n/a | directly creates legacy QU/EOPS sources/tags/maps |

Evidence: wrapper files above and `proc_Template_ST_SOLE_CUR_STD_2026.sql` lines 25-37.

`proc_NodeDataInit.sql` registers all four wrappers as selectable templates (`STMIN26-SA`, `STSTD26-SA`, `STMIN26-MTD`, `STSTD26-MTD`). Thus the broken or incomplete wrappers are live bootstrap entry points, not dead files.

### 3.3 `@IsMTD` determination

Repository-wide SQL search found two occurrences:

- `proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql`: forwards `@IsMTD = 1`.
- `proc_Template_ST_SOLE_CUR_STD_SA_2026.sql`: forwards `@IsMTD = 0`.

The corresponding callees do not accept that parameter. This contradicts specification sections 4, 5.4, and the Phase 0 expectation that removal had already occurred. Static inspection indicates these calls cannot bind successfully when compiled/executed in their present form. Review accepts these as minor live defects and explicitly authorises their removal during Phase 1 cleanup.

## 4. Tax-source and dedicated SQL tag inventories

Tag classes are seeded by `proc_NodeDataInit.sql` as 0 Rollup, 1 Component, and 2 Derived. Both dedicated tax procedures use `IF NOT EXISTS` insertion, contain no mappings, run validation immediately, and wrap their work in named transactions with `XACT_ABORT ON`. “Dedicated” identifies their present SQL location and intended separation; it does not pre-judge the contract audit's canonical-vocabulary decision.

### 4.1 `UK-ITSA-SE-QU` — 15 tags

Source: `proc_Template_ST_SOLE_CUR_TAX_MTD_2026.sql`. All are class 1 Component:

`turnover`, `otherBusinessIncome`, `costOfGoods`, `cisPaymentsToSubcontractors`, `wagesSalariesStaffCosts`, `carVanTravelExpenses`, `rentRatesPowerInsurance`, `repairsMaintenance`, `phoneFaxStationeryOtherOffice`, `advertising`, `businessEntertainment`, `interestOnLoans`, `bankFinancialCharges`, `accountancyLegalProfessionalFees`, `otherBusinessExpenses`.

### 4.2 `UK-ITSA-SE-EOPS` — 25 tags

Source: `proc_Template_ST_SOLE_CUR_TAX_MTD_2026.sql`.

- Class 1 Component (23): `basisPeriodStart`, `basisPeriodEnd`, `overlapProfit`, `overlapReliefUsed`, `transitionalProfit`, `transitionalRelief`, `privateUseAdjustment`, `annualInvestmentAllowance`, `writingDownAllowanceMainPool`, `writingDownAllowanceSpecialRate`, `writingDownAllowanceSingleAsset`, `smallPoolsAllowance`, `balancingChargeMainPool`, `balancingChargeSpecialRate`, `balancingChargeSingleAsset`, `balancingAllowanceMainPool`, `balancingAllowanceSpecialRate`, `balancingAllowanceSingleAsset`, `lossBroughtForward`, `lossUsedAgainstProfit`, `lossCarriedForward`, `lossUsedAgainstOtherIncome`, `lossUsedAgainstCapitalGains`.
- Class 2 Derived (2): `adjustedProfitForTax`, `capitalAllowancesTotal`.

Unlike the legacy MIN EOPS seed and development harness builder, this dedicated EOPS vocabulary does not repeat QU income/expense fields.

### 4.3 `UK-SA-SE-RETURN` — 41 tags

Source: `proc_Template_ST_SOLE_CUR_TAX_SA_2026.sql`. All are class 1 Component:

`turnover`, `otherIncome`, `costOfGoods`, `cisPaymentsToSubcontractors`, `wagesSalariesStaffCosts`, `carVanExpenses`, `travelExpenses`, `rentRatesPowerInsurance`, `repairsMaintenance`, `phoneFaxStationeryOtherOffice`, `advertising`, `businessEntertainment`, `interestOnLoans`, `bankFinancialCharges`, `accountancyLegalProfessionalFees`, `depreciationDisallowable`, `otherBusinessExpenses`, `goodsForOwnUse`, `privateUseAdjustment`, `basisPeriodStart`, `basisPeriodEnd`, `overlapProfit`, `overlapReliefUsed`, `transitionalProfit`, `transitionalRelief`, `annualInvestmentAllowance`, `writingDownAllowanceMainPool`, `writingDownAllowanceSpecialRate`, `writingDownAllowanceSingleAsset`, `smallPoolsAllowance`, `balancingChargeMainPool`, `balancingChargeSpecialRate`, `balancingChargeSingleAsset`, `balancingAllowanceMainPool`, `balancingAllowanceSpecialRate`, `balancingAllowanceSingleAsset`, `lossBroughtForward`, `lossUsedAgainstProfit`, `lossCarriedForward`, `lossUsedAgainstOtherIncome`, `lossUsedAgainstCapitalGains`.

The SA procedure's comment `TODO: Add QU + EOPS mappings here` is confirmed stale.

## 5. Legacy accounting-template tax material

### 5.1 MIN

`proc_Template_ST_SOLE_CUR_MIN_2026.sql` directly creates QU and EOPS sources, seeds the old 17-tag QU vocabulary and old 78-tag EOPS vocabulary, inserts eight category mappings (four per source), and validates both sources.

Legacy MIN mappings per source are:

- `turnover -> CT-TURNOV`
- `otherIncome -> CT-OTHRIN`
- `costOfGoods -> CT-CSTSAL`
- `wagesSalaries -> CT-STAFFC`

The last two code names differ from the dedicated vocabulary (`wagesSalariesStaffCosts`; `otherBusinessIncome` for QU), confirming that the historical material cannot be mechanically relocated.

### 5.2 STD

`proc_Template_ST_SOLE_CUR_STD_2026.sql` calls MIN, adds detailed classifications, adds 16 mappings (eight per MTD source), and validates both sources. Per source it maps:

- category: `carVanExpenses -> CA-MOTOR`, `travelExpenses -> CA-TRAVEL`, `premisesRunningCosts -> CA-PREMS`, `adminCosts -> CA-ADMIN`;
- cash code: `interestOnLoans -> CC-LOINT`, `financialCharges -> CC-FINCH`, `professionalFees -> CC-PROF`, `advertisingMarketing -> CC-ADVT`.

Most of these tag codes are absent from the dedicated QU vocabulary; all income/expense mappings are absent from the dedicated EOPS vocabulary. They are historical intent only.

## 6. Alignment evidence from live `hmrc_mtd`

### 6.1 Development raw-tag harness path and authority

The inspected development harness path is:

`QuTestController`/`EopsTestController` -> `HmrcSubmissionRunner` -> `QuHarnessPayloadBuilder`/`EopsHarnessPayloadBuilder` -> `TcBusinessTaxReader` -> `Cash.vwTaxBizSubmission` -> `Cash.vwTaxBizPayload` -> `Cash.vwTagCashPeriodMap` -> `Cash.tbTaxTagMap` and category/cash-code data.

`HMRC.WebHarness` is a development test harness used to pass JSON into the `HMRC_MTD` assembly and inspect results. It is not yet the authoritative HMRC payload path for this wave. Its builders and documentation therefore provide evidence of unfinished alignment, not a canonical vocabulary or a prerequisite for Phase 1 structural work.

Within that limited context, `TagMapper` emits every builder-defined expected tag, defaulting absent database values to zero. Consequently, a harness result can appear structurally complete even when SQL contains no tag seed or mapping for many expected tags. The C# validators only require request keys and at least one SQL result row; they do not verify the requested source code is QU/EOPS, tag-set completeness, duplicates, derived arithmetic, or SQL/C# vocabulary alignment.

### 6.2 QU mismatch

`QuHarnessPayloadBuilder.cs` and `docs/tax-hub-test-payloads.md` expect the old 17-tag set:

`turnover`, `otherIncome`, `costOfGoods`, `constructionCosts`, `wagesSalaries`, `carVanExpenses`, `travelExpenses`, `premisesRunningCosts`, `maintenanceCosts`, `adminCosts`, `advertisingMarketing`, `interestOnLoans`, `financialCharges`, `badDebts`, `professionalFees`, `depreciation`, `otherExpenses`.

Only `turnover`, `costOfGoods`, and `interestOnLoans` exactly match the dedicated 15-tag seed. The dedicated-only names/concepts are `otherBusinessIncome`, `cisPaymentsToSubcontractors`, `wagesSalariesStaffCosts`, `carVanTravelExpenses`, `rentRatesPowerInsurance`, `repairsMaintenance`, `phoneFaxStationeryOtherOffice`, `advertising`, `businessEntertainment`, `bankFinancialCharges`, `accountancyLegalProfessionalFees`, and `otherBusinessExpenses`. This is material unfinished alignment rather than casing variation, but it does not establish that the harness list is canonical.

The separate `QuarterlyUpdateRequest` model does not define canonical codes; it carries free-form `MtdIncomeCategory.CategoryName`, `MtdExpenseCategory.CategoryName`, and adjustment reason strings. No builder connecting SQL tag rows to that request model was found.

### 6.3 EOPS mismatch

`EopsHarnessPayloadBuilder.cs` expects 78 tags: the legacy 17 QU tags plus the legacy adjustment, derived-total, loss, basis-period, transition, and detailed allowance tags documented in `docs/tax-hub-test-payloads.md`. The dedicated SQL EOPS source contains only 25 tags.

All 25 dedicated EOPS codes occur in the 78-tag harness list, but 53 harness codes have no dedicated EOPS seed, including all QU income/expense codes, disallowables, `goodsForOwnUse`, accounting totals, post-cessation fields, detailed car/enhanced allowances, and pool values. The builder special-cases basis-period dates from row periods; all other absent values become numeric zero.

The separate `EopsRequest` model is narrower and differently structured: 5 allowance fields, 4 loss fields, and 4 adjustment fields. Several names have no exact dedicated tag (`CapitalAllowances`, `BalancingCharge`, `OtherAllowances`, `UsedThisYear`, `Class4LossesUsed`, `BasisPeriodAdjustment`, `AccountingAdjustment`, `TransitionalAdjustment`). No SQL-tag-to-`EopsRequest` builder or serializer was found.

### 6.4 SA103F mismatch

`Sa103F.cs` and `Sa103FSerializer.cs` contain business identity, accounting-period dates, 17 old-style income/expense properties, 13 disallowables, four adjustment/totals, five losses, four basis-period fields, six overlap/transitional fields, extensive allowance/pool fields, cessation values, and two Boolean flags.

The 41-tag dedicated SA seed uses the newer combined headings (`wagesSalariesStaffCosts`, `rentRatesPowerInsurance`, etc.) rather than many `Sa103F` property names (`WagesSalaries`, `PremisesRunningCosts`, etc.), omits many model fields, and classifies every tag—including dates—as Component. Conversely it contains `cisPaymentsToSubcontractors` and `businessEntertainment`, for which `Sa103F` has no exact property. `SaSubmissionBuilder` accepts a populated `Sa103F` and serializes it; it does not consume SQL tax tags. No live bridge from `UK-SA-SE-RETURN` raw rows to `Sa103F` was found.

### 6.5 Source-of-truth consequence

The dedicated SQL procedures, legacy SQL templates, development harness builders/document, and submission DTOs represent at least three currently unaligned vocabularies/shapes. No inspected representation should be promoted to canonical solely from this Phase 0 comparison. The canonical QU/EOPS/SA contracts will instead be settled by the separately reviewed HMRC contract audit and alignment exercise before Phase 3 mapping.

This unresolved alignment does not block Phase 1 because Phase 1 removes historical tax material from the accounting templates and performs approved cleanup only. It must not select a vocabulary, add mappings, or modify `hmrc_mtd` consumers.

## 7. Mapping schema, extraction, and validation behaviour

### 7.1 Enforced database constraints

Evidence: `Cash/Tables/tbTaxTagMap.sql`, `tbTaxTag.sql`, `tbTaxTagSource.sql`.

- Composite map primary key: `(TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode)` prevents an identical duplicate row, but permits multiple distinct mappings per tag.
- Check constraint enforces exactly one source shape: category (`MapTypeCode=0`, non-empty category, empty cash code) or cash code (`MapTypeCode=1`, inverse).
- Foreign key enforces an existing `(TaxSourceCode, TagCode)` and a valid map type.
- An INSERT/UPDATE trigger rejects nonexistent category/cash codes. There are no direct FKs from the two optional source columns because of their dual shape.
- Tag and source primary/FK constraints enforce source/tag identity. `ON DELETE CASCADE` removes tags/maps when sources/tags are deleted.

### 7.2 Extraction semantics

`Cash.fnCategoryCashCodes` recursively expands a mapped category through `Cash.tbCategoryTotal` to enabled nominal categories and enabled cash codes. `Cash.vwTagCashPeriodMap` unions category-derived and explicit cash-code mappings, and `Cash.vwTaxBizSubmission` sums period invoice values by source/tag/period. `TcBusinessTaxReader` then applies `Math.Abs` to each aggregate row before the harness mapper sums/rounds it. This removes accounting sign and is a material numerical-semantics point for later reconciliation.

The SQL `UNION` in `vwTagCashPeriodMap` deduplicates identical `(source, tag, parent, cash)` rows but does not prevent a cash code entering different additive tags. A parent-category tag plus child-category tag therefore produces the same operational value under multiple target tags.

### 7.3 What `Cash.proc_TaxTagMapValidate` actually guarantees

The procedure delegates to `Cash.fnTaxTagMapValidate`, logs warnings, and raises on errors. The function:

- warns for enabled mappings whose category/cash code no longer exists (normally prevented for new writes by the trigger);
- recursively expands category mappings;
- errors when the same cash code reaches the **same tag** more than once through overlapping mappings;
- warns for enabled, connected cash codes not mapped anywhere in the requested source.

It does **not**:

- check that `@TaxSourceCode` exists;
- require any tag or any map, so a newly seeded source with zero mappings passes except for broad unmapped-cash warnings;
- report tags with no mappings;
- reject one cash code mapped to multiple different tags;
- detect additive overlap across different tags;
- validate tag semantics, required/contextual/optional status, tag class correctness, derived formulas, dates, non-negativity, or numerical reconciliation;
- limit warnings to self-employment-relevant cash codes;
- fail on orphan warnings (and the insert trigger already prevents most new orphans).

The hard-coded error prefix says `MTD tag mapping errors` even for the SA source.

### 7.4 Transaction observation

MIN, STD, and both tax procedures each begin/commit named transactions. `App.proc_ErrorLog` rolls back the entire active transaction when `@@TRANCOUNT > 0` and reraises. Nested names do not create independent SQL Server transactions. This supports atomic rollback when errors reach these catches, but wrapper composition still requires explicit testing: wrappers themselves do not start transactions, and caught/reraised error behaviour plus return-code handling must be exercised against a database.

## 8. MIN and STD classification hierarchy

### 8.1 Confirmed inherited MIN hierarchy

Evidence: `proc_Template_BASE_MIN_2026.sql`, called by MIN.

- `CT-PANDL` contains `CT-GROSSP`, `CT-STAFFC`, `CT-OVERHD`, and `CA-ASSET`.
- `CT-GROSSP` contains `CT-TURNOV`, `CT-OTHRIN`, and `CT-CSTSAL`.
- `CT-TURNOV -> CA-SALES -> CC-SALES`.
- `CT-OTHRIN -> CA-INCOME -> CC-INCME`.
- `CT-CSTSAL -> CA-DIRECT -> CC-DIRCT` (and conditionally `CC-MINER`).
- `CT-STAFFC -> CA-WAGES -> CC-WAGES`, `CC-PENSN`; MIN disables `CC-EMPNI`.
- `CT-OVERHD -> CA-ADMIN -> CC-ADMIN`.
- MIN disables company-only depreciation codes `CC-DEPRC` and `CC-DEPRJ`, adds disconnected `CA-OWNER -> CC-OWNCAP`, and otherwise preserves base accounting behaviour.

Thus the four legacy MIN mappings are deterministic coarse totals, but their statutory semantics and dedicated tag names remain unapproved.

### 8.2 Confirmed STD additions

STD adds six enabled codes beneath existing `CA-ADMIN`: `CC-PHONE`, `CC-INSUR`, `CC-BANKC`, `CC-PROF`, `CC-ADVT`, `CC-REPA`.

It also adds four nominal children of `CT-OVERHD`:

- `CA-TRAVEL`: `CC-PARK`, `CC-PUBTR`, `CC-HOTEL`, `CC-MEALS`.
- `CA-MOTOR`: `CC-MFUEL`, `CC-MREPA`, `CC-MINSR`, `CC-MLICN`, `CC-MLEASE`.
- `CA-FINANCE`: `CC-LOINT`, `CC-FINCH`.
- `CA-PREMS`: `CC-RENT`, `CC-UTILS`, `CC-CLEAN`, `CC-PREMS`.

This confirms the specification's double-counting concern. For example, `CA-ADMIN` includes `CC-PROF` and `CC-ADVT`; mapping the category to one additive tag and those cash codes to other additive tags counts those values in multiple statutory fields. Likewise `CT-OVERHD` contains every listed STD category, so it must not be combined with descendant mappings without explicit non-additive semantics.

MIN has no deterministic subdivision matching the combined dedicated expense headings. STD has more detail, but several combined headings span categories/codes (for example car/van/travel and rent/rates/power/insurance), so similar labels alone are insufficient.

## 9. Automated and repeatable validation assets

### 9.1 Authoritative or promotable validation mechanisms

- `Cash.fnTaxTagMapValidate` is directly queryable and `Cash.proc_TaxTagMapValidate` is repeatable per source, subject to the limitations above.
- SQL project definitions include all four wrappers, both tax procedures, and validation objects, allowing a database-project build to catch binding/schema errors.
- Isolated database bootstrap assertions can be defined for Phase 1 to verify tax-neutral MIN/STD accounting creation and preservation of unrelated accounting behaviour.

### 9.2 Contextual development aids, not acceptance assets

- `src/sqlnode/src/tcNodeDb4/Scripts` is a non-authoritative developer scratch area. `MTDSoleTraderMappingEnquiry.sql` may be useful as reconnaissance material, but it is not part of the implementation contract or acceptance suite unless explicitly repaired, reviewed, and promoted later.
- `src/hmrc_mtd/src/HMRC.WebHarness` exposes development endpoints for QU and EOPS. It can support exploratory alignment work but is not the authoritative payload path or a Phase 1 acceptance dependency.
- `src/hmrc_mtd/docs/tax-hub-test-payloads.md` describes the harness's manual raw-tag expectations; it is evidence of the current harness design, not the canonical tax contract for this wave.

### 9.3 Gaps and observed defects

- No unit/integration test project was found in `src/sqlnode` or `src/hmrc_mtd`.
- No automated test was found that executes all four wrappers against isolated databases, asserts tax-source/tag inventories, validates reruns, or reconciles representative numbers.
- No test fixture/database connection was identified in scope, so no bootstrap was executed during Phase 0.
- For completeness, the scratch `MTDSoleTraderMappingEnquiry.sql` defaults `@SeedMappings = 1`, deletes/reinserts mappings, uses obsolete tags, and references absent `IsEnabled` columns. These are scratch-script observations, not implementation-contract or acceptance-suite defects.
- Static SQL-project inspection exposes the two invalid `@IsMTD` calls. A clean build should be made an explicit Phase 1 pre/post check, but was not run because builds may write generated files and this session authorised only the findings report.

## 10. Confirmed discrepancies against the governing specification

| Specification statement/expectation | Live finding |
|---|---|
| `@IsMTD` already removed; no task expected | Two wrappers still forward it to callees that do not declare it. |
| Four wrappers call accounting template but not tax procedure | Confirmed. |
| Dedicated seeds are intended current vocabularies, subject to consumer verification | They materially disagree with development QU/EOPS harness builders and with SA103F shape; the separate contract audit will determine canonical vocabularies before Phase 3. |
| Dedicated EOPS defines basis-period, adjustment, allowance, loss, derived-total tags | It has basis/transition, private-use, allowances, losses and two derived tags, but no general accounting/disallowable adjustments or QU totals. |
| Dedicated SA is canonical SA103F set used by submission layer | Not confirmed; the live `Sa103F` model/serializer is larger and differently named, and no SQL-to-SA103F builder exists. |
| Validation is necessary but limited | Confirmed, with the additional fact that an unmapped source does not fail. |
| Existing repeatable tests may establish bootstrap behaviour | No four-wrapper automated suite was found. Scratch scripts and WebHarness are contextual development aids, not the acceptance suite. |

## 11. Open questions, reviewed decisions, and deferrals

### 11.1 Resolved for Phase 1

1. Removal of the two stale `@IsMTD` forwarding arguments is authorised as minor live-defect cleanup in Phase 1.
2. WebHarness vocabulary differences do not define or block SQL structural separation.
3. `MTDSoleTraderMappingEnquiry.sql` is non-authoritative scratch material and is excluded from the acceptance suite unless explicitly promoted later.
4. Phase 1 is limited to removing obsolete Self Assessment source/tag/map/validation material from MIN and STD, correcting associated stale comments, and removing the two authorised forwarding arguments. It includes no mappings, vocabulary redesign, wrapper composition, or consumer changes.

### 11.2 Deferred to HMRC contract audit before Phase 3

1. Whether the dedicated 15-tag QU list, 17-tag harness list, or another audited list is canonical.
2. Whether EOPS is annual-only or includes the broader raw annual set currently expected by the development harness.
3. The canonical SA source shape and its relationship to `Sa103F`, including contextual, optional, and derived fields.
4. Any required `hmrc_mtd` model, builder, serializer, harness, or documentation changes. Such changes remain outside the present SQL structural scope unless separately authorised.

### 11.3 Still required before later phases or final acceptance

1. Confirm validation placement during Phase 2 design; current dedicated-procedure calls accept zero mappings and do not establish mapping readiness.
2. Define and test first-run/rerun and transaction behaviour before wrapper composition is accepted.
3. Decide completeness severity and representation for unmapped, derived, contextual, and optional tags before Phase 3 approval.
4. Audit numerical semantics including `TcBusinessTaxReader`'s `Math.Abs` before reconciliation acceptance.
5. Confirm tax-type/due-period semantics for SA and EOPS before payload reconciliation.

## 12. Recommendation and Phase 1 gate

Recommendation: **proceed to Phase 1 when separately instructed**, under the reviewed narrow scope. Vocabulary and WebHarness alignment are not Phase 1 prerequisites because Phase 1 makes the accounting templates tax-neutral; a temporarily tax-neutral wrapper result is an expected bounded-phase state until Phase 2 composition.

Authorised Phase 1 scope:

1. Remove obsolete Self Assessment tax-source, tag, mapping, and validation material from MIN and STD accounting templates.
2. Correct comments made stale or misleading by that removal.
3. Remove the two stale `@IsMTD` forwarding arguments from MIN MTD and STD SA wrappers.
4. Preserve all unrelated accounting behaviour, signatures, transaction/error conventions, and repository boundaries.

Phase 1 exclusions remain explicit:

- no new mappings or relocation of historical mappings;
- no canonical vocabulary decisions or tag-seed redesign;
- no calls to dedicated tax procedures (wrapper composition remains Phase 2);
- no `hmrc_mtd`, WebHarness, serializer, payload, or consumer-contract changes;
- no promotion or repair of scratch scripts as acceptance tooling.

Recommended Phase 1 evidence is a focused SQL diff, a SQL project build, and isolated MIN/STD bootstrap assertions showing preserved accounting behaviour with no Self Assessment sources, tags, mappings, or validation calls created by those accounting templates. The Phase 3 mapping gate and pre-Phase-3 HMRC contract audit remain mandatory.

No Phase 1 work was performed in this session.

---

# HMRC Contract Audit — Self Assessment and VAT

Date: 28 August 2026  
Scope: current HMRC contract verification and consequential review of the Tax Hub Self Assessment architecture.

## 13. Purpose

Following completion of Self Assessment SQL Node Phases 1 and 2, the previously deferred HMRC contract audit was performed before authorising Tax Tag mapping work.

The audit compared the current Trade Control Self Assessment and VAT assumptions with current authoritative HMRC specifications.

The purpose was to establish the external statutory truth before existing SQL Tax Tags, C# models, harness payloads, serializers, or historical implementation were allowed to become canonical.

## 14. Self Assessment Contract Findings

### 14.1 Product submission route

The existing implementation contained two Sole Trader submission architectures:

- Making Tax Digital for Income Tax; and
- legacy SA100 / SA103F XML submission.

Current product review concluded that Trade Control should support Sole Trader Self Assessment submission through **Making Tax Digital for Income Tax only**.

Legacy SA100 / SA103F submission therefore has no continuing product requirement.

This does not imply that XML, RIM, iXBRL, IRmark, or related transport mechanisms are obsolete generally. Other statutory regimes, particularly Corporation Tax, may continue to require them.

### 14.2 EOPS

The historical Trade Control MTD implementation models:

- Quarterly Updates; and
- End of Period Statement (EOPS).

The contract audit established that EOPS is no longer a current MTD Income Tax filing stage.

The existing:

`UK-ITSA-SE-EOPS`

Tax Source and its associated SQL and C# structures are therefore historical implementation rather than a current statutory contract.

Individual adjustment, allowance, loss, or other statutory concepts previously grouped beneath EOPS may still be relevant, but each must be independently verified against the current MTD architecture before reuse.

### 14.3 Quarterly reporting

Current MTD Self Employment quarterly reporting is cumulative from the beginning of the tax year to the end of the update period.

The statutory accounting starting point comprises two income concepts and thirteen expense concepts.

This produces a 15-value core quarterly accounting projection.

The existing dedicated SQL Quarterly Update seed also contains 15 tags, but equality of count does not establish semantic equivalence. Names, meanings, mappings, and support by MIN and STD remain subject to the contract-aligned Phase 3 reconnaissance.

The current HMRC Self Employment API exposes additional optional properties beyond this core accounting set.

An HMRC API property is not automatically a Trade Control Tax Tag.

### 14.4 Annual and finalisation information

The historical EOPS and SA103F structures must not be used as canonical annual Tax Tag vocabularies.

Current annual Self Employment requirements include statutory concepts such as adjustments and capital allowances.

Losses are handled through dedicated HMRC processes and must not be treated as an EOPS field set.

No canonical annual Objective 2 Tax Tag vocabulary has yet been approved.

Establishing that vocabulary is deliberately deferred to contract-aligned Phase 3 reconnaissance.

### 14.5 Final Declaration

The inspected historical Final Declaration model does not represent the current HMRC contract.

Current Final Declaration processing is tied to the HMRC calculation/finalisation workflow rather than the historical Trade Control request-body representation.

Exact endpoint, request, response, identifier, and version semantics remain Objective 3 concerns.

### 14.6 Obligations, calculations, liabilities and losses

Existing Trade Control C# representations of MTD obligations, calculations, liabilities, losses, and related structures cannot be assumed to represent current HMRC wire contracts.

They require classification and verification against their respective current HMRC services before being retained as Objective 3 contracts.

## 15. Objective Boundary Findings

The audit confirmed that the project had allowed several architectural concerns to become conflated.

The corrected boundary is:

**Trade Control accounting**  
→ **Tax Source / Tax Tag statutory projection — Objective 2**  
→ **HMRC contract adapter — Objective 3**  
→ **HMRC transport — Objective 4**

Objective 2 establishes truthful statutory information from Trade Control accounting and legitimate contextual sources.

Objective 3 owns exact HMRC-facing contracts.

Objective 4 owns transmission and receipt.

Existing implementation is authoritative evidence of current repository state.

It is not authority for externally governed HMRC semantics.

## 16. Test Harness Findings

The existing WebHarness had evolved around an interpretation in which harness-specific raw-tag payloads were treated as part of Objective 2 submission architecture.

That interpretation is rejected.

The Test Harness is development and verification infrastructure.

Its purpose is to allow developers and coding agents to exercise real production components and inspect useful observation points including:

- SQL/accounting output;
- Objective 2 statutory projections;
- Objective 3 serialized HMRC payloads;
- Objective 4 transport behaviour; and
- HMRC Sandbox responses.

The harness must observe actual implementation output rather than define a parallel canonical representation.

Existing QU and EOPS harness endpoints are not protected behaviour and may be removed rather than repaired where they represent obsolete architecture.

The existing VAT-MTD harness path may remain useful as an implementation template, subject to reconnaissance confirming that it exercises the real production path.

## 17. `hmrc_mtd` Classification Requirement

The audit established that physical location beneath `src/hmrc_mtd` does not prove that a class represents an HMRC contract.

Current models, services, builders, serializers, transport components, and harness infrastructure must therefore be classified during subsequent reconnaissance as one of:

1. actual HMRC contract;
2. Trade Control statutory projection;
3. harness/development infrastructure; or
4. obsolete/legacy implementation.

Repository and namespace disposition should follow that evidence rather than be inferred from current location.

This classification must include supporting services and infrastructure as well as the previously reviewed model classes.

## 18. VAT Findings

The existing VAT implementation is materially closer to the current HMRC contract than the Self Assessment implementation.

The nine VAT accounting values remain valid.

The principal identified contract defect is serialization: the current implementation must be verified to emit the exact HMRC JSON property names and casing rather than relying upon inappropriate default .NET serialization behaviour.

`Cash.vwTaxVatSubmission` remains the authoritative Trade Control VAT accounting surface.

The older pre-Brexit VAT surface is not authoritative.

VAT does not block the current Self Assessment SQL mapping reconnaissance.

## 19. Consequence for Self Assessment SQL Node Work

The HMRC audit satisfies the reason that canonical vocabulary selection was deliberately deferred after Phase 2.

However, it does not itself authorise mapping implementation.

Self Assessment SQL Node Revision 3 therefore reopens the next stage as:

**Phase 3 — Contract-Aligned MTD Reconnaissance and Proposal**

Phase 3 must establish the proposed current Objective 2 Tax Source and Tax Tag vocabulary, MIN and STD support matrices, retirement set, validation requirements, and any genuine cross-repository consequences before implementation is authorised.

No Phase 3 implementation has yet been performed.
