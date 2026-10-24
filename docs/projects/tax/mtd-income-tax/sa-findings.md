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

## 20. Phase 3 — Contract-Aligned MTD Reconnaissance and Proposal

This section records the Phase 3 reconnaissance performed on 29 August 2026. It is additive to the historical evidence above. Where earlier sections describe obsolete QU/EOPS assumptions, this section applies the reviewed architecture and the current HMRC contracts.

### 20.1 Current-state report

#### SQL Node

The Phase 1/2 structural separation is present and internally consistent:

- `App.proc_Template_ST_SOLE_CUR_MIN_2026` and `App.proc_Template_ST_SOLE_CUR_STD_2026` build accounting classifications only;
- `App.proc_Template_ST_SOLE_CUR_MIN_MTD_2026` and `App.proc_Template_ST_SOLE_CUR_STD_MTD_2026` call their accounting template and then `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026` inside an outer named transaction;
- the MTD tax procedure still seeds `UK-ITSA-SE-QU` with 15 tags and `UK-ITSA-SE-EOPS` with 25 tags, validates both sources, and installs no mappings;
- the four SA wrappers and `App.proc_Template_ST_SOLE_CUR_TAX_SA_2026` remain live build artefacts and node-menu choices, although the governing specification no longer supports that route;
- `App.proc_NodeDataInit` still advertises SA100/SA103F templates and describes MTD templates as containing EOPS adjustments;
- `Cash.vwTagCashPeriodMap` expands category mappings recursively to enabled CashCodes, `Cash.vwTaxBizPayload` joins period values, and `Cash.vwTaxBizSubmission` sums by source, tag and due-date period; and
- `Cash.fnTaxTagMapValidate` detects orphan references and duplicate inclusion of a CashCode within one tag, but does not prove semantic completeness, cross-tag exclusivity, correct polarity, eligibility, or support for an entire statutory field set.

`src/sqlnode/.../Scripts/MTDSoleTraderMappingEnquiry.sql` remains a non-authoritative developer scratch script. Its old mappings are evidence of prior exploration only and are not part of the implementation contract or acceptance suite.

#### `hmrc_mtd`

The executable self-assessment path currently ends in simulation:

`HMRC.WebHarness controller -> HmrcSubmissionRunner -> validator -> QU/EOPS harness builder -> TcBusinessTaxReader -> Cash.vwTaxBizSubmission -> TagMapper -> harness envelope`

The runner returns an object explicitly marked as simulation and states that transport is outside Objective 2. No inspected registration or call site connects the `Hmrc/Sa/v1_0/Submissions/MTDITSA` request/endpoint classes to that runner. The current DTO namespace is therefore not an executable HMRC submission path.

The reader applies `Math.Abs` to every SQL amount. The mapper then emits every expected harness tag and substitutes `0` when no source row exists. Those behaviours lose accounting polarity and conflate “unsupported or absent” with an evidenced zero. They must not be copied into the statutory projection.

### 20.2 `hmrc_mtd` classification

| Area | Classification | Finding |
|---|---|---|
| `Services/TcData/TcBusinessTaxReader` | Trade Control projection reader, currently flawed | Executable from the harness; reads the real SQL projection but destroys sign with `Math.Abs`. |
| `Cash.vwTaxBizSubmission` consumption | Trade Control statutory-projection boundary | Real executable boundary, although its source vocabulary and validation remain incomplete. |
| `Services/Mapping/TagMapper` | Harness/development infrastructure | Converts raw tags to generic items and invents zero values for missing tags. It is not an HMRC adapter. |
| QU/EOPS harness builders, validators, controllers and payload models | Harness/development infrastructure with obsolete assumptions | Executable diagnostics, but their tag lists and EOPS lifecycle do not define the contract. |
| `HmrcSubmissionRunner` self-assessment branches | Harness/development simulation | Builds envelopes and a simulated response; it performs no HMRC serialization or transport. |
| `MTDITSA/QuarterlyUpdate` | Obsolete/legacy contract interpretation | Uses generic category lists and `/income-tax/{mtditid}/periodic-summary` v1 rather than the current cumulative self-employment contract. |
| `MTDITSA/Eops` | Obsolete/legacy | EOPS is not a current filing stage; its models mix adjustments, allowances and losses that now belong to distinct journeys. |
| `MTDITSA/FinalDeclaration` | Obsolete/legacy contract interpretation | The local request aggregates income, deductions and calculation data; current finalisation is driven by the Individual Calculations API after source data has been completed. |
| `MTDITSA/Obligations`, `Payments`, `Liabilities` | Unverified legacy assumptions | Not executed by the runner; enquiry operations return “not implemented”. Each needs a separate current-contract review before Objective 3 use. |
| `SA100` schedules and serializers | Legacy self-assessment implementation | Not the supported MTD sole-trader route. |
| XML canonicalisation, IRmark/RIM-style and generic serialization utilities | Shared capability pending dependency analysis | Do not retire merely because SA100/SA103F is retired; other regimes may depend on the generic mechanisms. |
| VAT models/services | Outside this Phase 3 mapping decision | Retain pending the separate VAT work already identified in section 18. |

### 20.3 Proposed current vocabulary

The current authoritative service is Self Employment Business (MTD) API v5.0, last updated 20 August 2026. From tax year 2025-26 its `PUT /individuals/business/self-employment/{nino}/{businessId}/cumulative/{taxYear}` operation submits cumulative income, expenses and disallowable expenses. HMRC requires income and expense objects even where their values are zero, but that wire rule belongs in the Objective 3 adapter and does not justify zero-seeding unsupported Objective 2 tags.

The proposed source names make the lifecycle explicit:

- `UK-ITSA-SE-CUMULATIVE` — cumulative in-year business projection;
- `UK-ITSA-SE-ANNUAL` — annual adjustments and allowances projection.

These names are proposals, not implemented identifiers. Exact identifier length must be reconciled with the current `NVARCHAR(20)` source table and the validator's narrower `NVARCHAR(10)` parameter before implementation.

#### Cumulative projection

Use canonical concepts aligned with the current contract, with the HMRC object path recorded as adapter metadata rather than treating the Category Tree as an HMRC taxonomy:

| Concept | Current HMRC path | Classification |
|---|---|---|
| turnover | `periodIncome.turnover` | Accounting total |
| otherBusinessIncome | `periodIncome.other` | Accounting total |
| taxTakenOffTradingIncome | `periodIncome.taxTakenOffTradingIncome` | Contextual/external; explicitly excludes CIS deductions |
| consolidatedExpenses | `periodExpenses.consolidatedExpenses` | Conditional alternative to detailed expenses; never alongside them |
| costOfGoods | `periodExpenses.costOfGoods` | Accounting total |
| paymentsToSubcontractors | `periodExpenses.paymentsToSubcontractors` | Accounting total |
| wagesAndStaffCosts | `periodExpenses.wagesAndStaffCosts` | Accounting total |
| carVanTravelExpenses | `periodExpenses.carVanTravelExpenses` | Accounting total |
| premisesRunningCosts | `periodExpenses.premisesRunningCosts` | Accounting total |
| maintenanceCosts | `periodExpenses.maintenanceCosts` | Accounting total |
| adminCosts | `periodExpenses.adminCosts` | Accounting total |
| businessEntertainmentCosts | `periodExpenses.businessEntertainmentCosts` | Accounting total |
| advertisingCosts | `periodExpenses.advertisingCosts` | Accounting total |
| interestOnBankOtherLoans | `periodExpenses.interestOnBankOtherLoans` | Accounting total |
| financeCharges | `periodExpenses.financeCharges` | Accounting total |
| irrecoverableDebts | `periodExpenses.irrecoverableDebts` | Accounting total |
| professionalFees | `periodExpenses.professionalFees` | Accounting total |
| depreciation | `periodExpenses.depreciation` | Accounting total, distinct from capital allowances |
| otherExpenses | `periodExpenses.otherExpenses` | Residual accounting total only when deterministically classified |

Each detailed expense also has a corresponding current `periodDisallowableExpenses` concept. These cannot be derived merely by reusing the gross expense mapping; they need evidence that each transaction or CashCode is allowable/disallowable. The existing templates do not provide that dimension.

The period start/end dates, NINO, business ID and tax year are adapter/context data, not Category Tree mappings. Updates are cumulative from the tax-year start; a later update supersedes the earlier cumulative submission.

#### Annual projection

The proposed annual vocabulary follows the current 2026-27 annual-submission schema, subject to feature/release confirmation at implementation time:

- adjustments: `includedNonTaxableProfits`, `basisAdjustment`, `accountingAdjustment`, deprecated `averagingAdjustment`, `outstandingBusinessIncome`, `balancingChargeBpra`, `balancingChargeOther`, `goodsAndServicesOwnUse`, `transitionProfitAmount`, `transitionProfitAccelerationAmount`, and `adjustmentToProfitsForClass4`;
- allowances: `annualInvestmentAllowance`, `capitalAllowanceMainPool`, `capitalAllowanceSpecialRatePool`, `businessPremisesRenovationAllowance`, `enhancedCapitalAllowance`, `allowanceOnSales`, `capitalAllowanceSingleAssetPool`, `zeroEmissionsCarAllowance`, conditionally released `firstYearAllowanceOnPlantAndMachinery`, `tradingIncomeAllowance`, `structuredBuildingAllowance`, and `enhancedStructuredBuildingAllowance`; and
- non-financial context: `class4NicsExemptionReason`.

Structured-building allowances require per-building identity, postcode and possible first-year qualifying data; they are not scalar Category Tree totals. Trading allowance is mutually exclusive with other allowances. Losses and loss claims are deliberately excluded from this source: brought-forward losses and claims belong to the Individual Losses API, while calculated loss availability is obtained through the BSAS/Individual Calculations journeys. Profit, adjusted-profit and capital-allowance totals calculated elsewhere must not be invented as annual submission fields.

### 20.4 MIN support and mapping proposal

MIN is intentionally coarse. Its validity is not measured by resemblance to HMRC terminology. Support exists only where its classification preserves enough information for deterministic, non-overlapping derivation.

| Concept | Candidate MIN source | Status | Rationale / ambiguity |
|---|---|---|---|
| turnover | `CT-TURNOV` | Supported, high confidence | Dedicated sales roll-up. |
| otherBusinessIncome | `CT-OTHRIN` | Supported, medium confidence | Dedicated other-income roll-up; confirm that taxable/non-taxable income is not mixed. |
| wagesAndStaffCosts | `CT-STAFFC` | Supported, medium confidence | Dedicated staff-cost roll-up; verify all enabled descendants are within the statutory concept. |
| costOfGoods | `CT-CSTSAL` / `CA-DIRECT` | Unsupported | “Direct costs” is broader than goods bought for resale or used. No deterministic split exists. |
| consolidatedExpenses | cost/staff/overhead roll-ups | Conditional, not presently supported | HMRC permits this only for qualifying businesses and only for allowable expenses. Current classifications do not prove allowability or private-use exclusion. |
| remaining detailed expenses | coarse `CA-ADMIN` or no source | Unsupported | The required information has not been preserved. Do not infer or emit zero. |
| disallowable expense fields | none | Unsupported | No allowability dimension exists. |
| annual adjustments/allowances | none | Unsupported by Category Tree | Require elections, asset-pool records, tax adjustments, user input or other contextual sources. |

No MIN map rows should be proposed for unsupported concepts. A later consolidated-expense implementation is possible only after eligibility, allowability and exclusion rules have a legitimate source.

### 20.5 STD support and mapping proposal

STD provides useful additional granularity but still does not have to mirror HMRC. Candidate maps below are limited to information actually preserved by the current hierarchy.

| Concept | Candidate STD source(s) | Status | Determinism / overlap / roll-up |
|---|---|---|---|
| turnover | `CT-TURNOV` | Supported, high | Dedicated roll-up. |
| otherBusinessIncome | `CT-OTHRIN` | Supported, medium | Dedicated roll-up; taxable-content check remains. |
| wagesAndStaffCosts | `CT-STAFFC` | Supported, medium | Dedicated roll-up. |
| carVanTravelExpenses | `CA-MOTOR` + `CA-TRAVEL` | Supported, high | Disjoint sibling categories; two rows to one tag produce the required combined roll-up. |
| maintenanceCosts | `CC-REPA` | Supported, medium | General repairs code; motor repairs remain within `CA-MOTOR`, avoiding overlap. Confirm its posting policy excludes motor costs. |
| advertisingCosts | `CC-ADVT` | Supported, high | Dedicated code. |
| interestOnBankOtherLoans | `CC-LOINT` | Supported, high | Dedicated code. |
| financeCharges | `CC-BANKC` + `CC-FINCH` | Supported, medium | Disjoint codes; confirm `CC-BANKC` contains charges only. |
| professionalFees | `CC-PROF` | Supported, medium | Dedicated but broad code; posting guidance must cover accountancy/legal/professional only. |
| premisesRunningCosts | `CA-PREMS` + `CC-INSUR` | Conditional | `CA-PREMS` includes generic premises and cleaning; `CC-INSUR` may include non-premises insurance. Support depends on documented posting semantics. |
| adminCosts | `CC-PHONE` plus an unknown office-cost source | Partially classified; unsupported as a complete tag | `CC-PHONE` alone is incomplete; `CC-ADMIN` is too broad to infer the remainder safely. |
| costOfGoods | `CT-CSTSAL` / `CA-DIRECT` | Unsupported | Direct-cost roll-up is broader than the statutory concept. |
| paymentsToSubcontractors | none | Unsupported | No dedicated classification. |
| businessEntertainmentCosts | none | Unsupported | No dedicated classification. |
| irrecoverableDebts | none | Unsupported | No dedicated classification. |
| depreciation | disabled depreciation codes | Unsupported | Disabled template content is not an evidenced zero and capital allowances are separate. |
| otherExpenses | `CC-ADMIN` or residual roll-up | Unsupported | A residual cannot safely absorb values belonging to unsupported named fields. |
| consolidatedExpenses | all allowable expense sources | Conditional, not presently supported | Same eligibility and allowability gaps as MIN. |
| disallowable expense fields | none | Unsupported | No allowability dimension. |
| annual adjustments/allowances | none | Unsupported by Category Tree | These require non-ledger tax facts or richer asset/tax records. |

The supported candidate rows are a proposal for Phase 4 design, not authorisation to insert mappings. Conditional rows must remain absent until their posting semantics are made deterministic. Mapping both a parent category and one of its descendants to the same tag would double count and remains prohibited.

### 20.6 Retirement proposal

Retire in a bounded later implementation, after dependency checks:

- SQL SA wrappers `App.proc_Template_ST_SOLE_CUR_MIN_SA_2026` and `App.proc_Template_ST_SOLE_CUR_STD_SA_2026`;
- `App.proc_Template_ST_SOLE_CUR_TAX_SA_2026`, its `UK-SA-SE-RETURN` seed, menu entries and SA100/SA103F descriptions;
- the `UK-ITSA-SE-EOPS` SQL source, tags, validator call, descriptions and TODO wording;
- QU/EOPS harness builders, validators, payloads, controllers and runner operations where they embody the obsolete generic/EOPS architecture;
- obsolete `MTDITSA/QuarterlyUpdate`, `MTDITSA/Eops` and `MTDITSA/FinalDeclaration` contract interpretations; and
- old self-assessment obligations/payments/liabilities models only after confirming that no reusable current-contract implementation exists.

Do not automatically remove generic XML serialization, canonicalisation, IRmark/RIM/iXBRL or shared transport utilities. Search all regimes and retain or relocate any independently used capability. Do not treat the developer scratch script as a migration artefact that must be maintained.

### 20.7 Validation proposal

Phase 4 implementation should add repeatable checks at the appropriate boundary:

1. source/tag seed equality against an approved manifest, including exact names, classes and ordering;
2. foreign-key and orphan checks already present;
3. same-tag expansion overlap checks already present;
4. cross-tag CashCode overlap checks across mutually exclusive detailed expense tags;
5. explicit source completeness status (`supported`, `conditional`, `unsupported`) so absence is never silently converted to zero;
6. category/CashCode polarity and sign tests—remove unconditional `Math.Abs` from the production path;
7. cumulative period tests using tax-year start and the selected standard/calendar obligation end date;
8. detailed-versus-consolidated expense mutual-exclusion tests and turnover eligibility evidence;
9. gross-versus-disallowable provenance tests;
10. annual mutual-exclusion and structured-data validation, including trading allowance and building details;
11. fixture tests proving category expansion, multiple disjoint rows per tag, and no double counting; and
12. adapter tests showing that only the HMRC boundary supplies contract-required zero values, and only for a projection proven supported.

SQL project build remains necessary but is insufficient. A database-level fixture or deployed test database is required to exercise recursive category expansion and validation procedures. Harness tests should observe this production projection rather than maintain their own vocabulary.

### 20.8 Issue log

| ID | Issue | Disposition |
|---|---|---|
| P3-01 | Current SQL QU tag names do not equal current HMRC v5 property names. | Replace with an approved cumulative projection vocabulary in Phase 4; adapter owns wire paths. |
| P3-02 | `UK-ITSA-SE-EOPS` represents a retired lifecycle and mixes annual adjustments, allowances and losses. | Retire; introduce an annual source and keep losses separate. |
| P3-03 | SQL source-code widths conflict (`tbTaxTagSource` 20, related tables/validator paths 10 in places) and the proposed descriptive identifiers exceed old limits. | Resolve identifier convention and widen consistently before seeding. |
| P3-04 | MIN and STD lack deterministic support for several detailed expense fields. | Record unsupported; do not infer and do not zero-fill. |
| P3-05 | Neither template proves expense allowability/disallowability. | Design a legitimate classification/context source before supporting those fields or consolidated expenses. |
| P3-06 | Reader destroys sign with `Math.Abs`. | Remove from the production projection and define polarity rules. |
| P3-07 | `TagMapper` zero-fills every expected harness tag. | Keep out of the production path; adapter-only zeros require supported projection evidence. |
| P3-08 | Existing validator misses cross-tag overlap and semantic completeness. | Extend validation and add manifest/fixture tests. |
| P3-09 | Current due-date-driven SQL periods may not express cumulative updates or calendar-quarter election correctly. | Design a cumulative period boundary keyed by tax year and obligation context. |
| P3-10 | Annual 2026-27 schema contains feature-gated/test-only fields. | Pin the production API release at implementation and treat gated fields as conditional. |
| P3-11 | Node template menu text still advertises SA/EOPS behaviour. | Amend with retirement work, not during reconnaissance. |
| P3-12 | No self-assessment HMRC adapter/transport is connected. | Objective 3/4 work; do not disguise the harness as that path. |

### 20.9 Bounded implementation sequence proposed for authorisation

1. Approve the two-source Objective 2 vocabulary and identifier convention, explicitly separating cumulative reporting, annual submission, losses, context and derived calculations.
2. Retire SA/EOPS SQL wrappers, sources and descriptions in a dependency-checked change; retain generic cross-regime utilities.
3. Implement the cumulative and annual source/tag manifests with no maps and add manifest-level tests.
4. Correct the projection period/sign model and extend validation for cross-tag overlap, support state and cumulative periods.
5. Add only the high-confidence MIN/STD candidate mappings approved from sections 20.4 and 20.5; leave conditional/unsupported tags unmapped.
6. Add database fixtures proving recursive expansion, disjoint multi-source roll-ups and absence-not-zero behaviour.
7. Replace obsolete QU/EOPS harness paths with diagnostics that call the real Objective 2 projection; do not introduce HMRC transport.
8. Stop and review the resulting Objective 2 surface before Objective 3 defines current HMRC DTOs and serialization.

Phase 3 is complete as reconnaissance and proposal only. No production SQL or C# implementation was changed, and Phase 4 has not begun.

### 20.10 HMRC sources

The external contract findings in this section were checked on 29 August 2026 against the following current, HMRC-controlled sources. The linked API versions and schemas should be pinned or rechecked when implementation begins because HMRC may revise beta contracts and feature-gated fields.

| Source | Phase 3 use |
|---|---|
| [Self Employment Business (MTD) API v5.0 — overview](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-employment-business-api/5.0) | Establishes the current API and its quarterly and annual self-employment summary responsibilities. The page identified v5.0 beta as the latest version and was last updated 20 August 2026 when checked. |
| [Self Employment Business API v5.0 — HMRC-maintained OpenAPI root](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/application.yaml) | Establishes the current cumulative endpoint path, annual endpoint path, OAuth scopes and separation between cumulative period summaries and annual submissions. |
| [Create or Amend a Self-Employment Cumulative Period Summary — endpoint definition](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/create_amend_cumulative_period_summary.yaml) | Establishes cumulative submission semantics from 2025-26, the requirement to supply income and expense values at the wire boundary, and the rule prohibiting detailed and consolidated expenses together. |
| [Cumulative period summary request schema](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/schemas/createAmendCumulativePeriodSummary/request.json) | Authoritative source for current `periodDates`, `periodIncome`, `periodExpenses` and `periodDisallowableExpenses` property names and value constraints. |
| [Create and Amend Self-Employment Annual Submission — endpoint definition](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/create_and_amend_annual_submission.yaml) | Establishes the distinct annual-summary journey and its tax-year-specific request definitions. |
| [2026-27 annual submission request schema](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/schemas/createAmendAnnualSubmission/def4/request.json) | Source for the proposed annual adjustments, allowances and non-financial vocabulary. The schema contains conditional/test-only documentation markers, so release status must be rechecked before implementation. |
| [Making updates during the tax year](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/documentation/make-updates-during-tax-year.html) | Establishes four quarterly obligations plus an annual return, cumulative updates after tax year 2025, supersession of earlier updates, calendar-quarter handling, and the under-£90,000 consolidated-expenses option. |
| [Making updates at the end of a tax year](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/documentation/make-updates-at-tax-year-end.html) | Establishes the current year-end sequence and separation of business adjustments, allowances, loss treatment, calculation and annual tax-return finalisation. |
| [Business Source Adjustable Summary (MTD) API v7.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-assessment-bsas-api/7.0) | Establishes BSAS as the service for generating business-source adjustable summaries and submitting accounting adjustments, rather than an EOPS payload. |
| [Individual Losses (MTD) API v7.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/individual-losses-api/7.0) | Establishes brought-forward losses and loss claims as a separate contract journey rather than Category Tree totals or annual-summary fields. |
| [Individual Calculations (MTD) API v8.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/individual-calculations-api/8.0) | Establishes the calculation/finalisation service boundary, including triggering or retrieving calculations and submitting the final declaration/tax return. |
| [Making Tax Digital for Income Tax update notice](https://www.gov.uk/government/publications/update-notice-for-making-tax-digital-for-income-tax/making-tax-digital-for-income-tax-update-notice) | Statutory-direction context for reporting update periods and summary totals of income and expense categories. |

The HMRC-owned GitHub repository is linked for exact OpenAPI and JSON Schema evidence because the interactive Developer Hub endpoint viewer loads those same contract assets but does not expose stable line-addressable content. Historical Trade Control DTOs, harness tag lists, SA100/SA103F models and scratch SQL were not used as authority for the external contract.

## 21. Phase 4B — Cumulative Sole Trader Projection Design

This section records the Phase 4B reconnaissance performed on 29 August 2026 against the Phase 4A baseline. It is a design proposal only. No SQL, C#, Tax Source, Tax Tag, mapping, DTO, serializer or harness implementation was changed.

### 21.1 Verified current HMRC cumulative contract

The intended contract is **Self Employment Business (MTD) API v5.0 beta**, shown by HMRC as the latest version and last updated 20 August 2026 when rechecked on 29 August 2026.

The operation is:

```text
PUT /individuals/business/self-employment/{nino}/{businessId}/cumulative/{taxYear}
Accept: application/vnd.hmrc.5.0+json
Content-Type: application/json
OAuth scope: write:self-assessment
```

It is available for tax years starting with 2025-26. `nino`, `businessId` and `taxYear` are path/context values and are not Tax Tags.

The exact request shape is:

```text
periodDates
  periodStartDate
  periodEndDate
periodIncome
  turnover
  other
  taxTakenOffTradingIncome
periodExpenses
  consolidatedExpenses
  costOfGoods
  paymentsToSubcontractors
  wagesAndStaffCosts
  carVanTravelExpenses
  premisesRunningCosts
  maintenanceCosts
  adminCosts
  businessEntertainmentCosts
  advertisingCosts
  interestOnBankOtherLoans
  financeCharges
  irrecoverableDebts
  professionalFees
  depreciation
  otherExpenses
periodDisallowableExpenses
  costOfGoodsDisallowable
  paymentsToSubcontractorsDisallowable
  wagesAndStaffCostsDisallowable
  carVanTravelExpensesDisallowable
  premisesRunningCostsDisallowable
  maintenanceCostsDisallowable
  adminCostsDisallowable
  businessEntertainmentCostsDisallowable
  advertisingCostsDisallowable
  interestOnBankOtherLoansDisallowable
  financeChargesDisallowable
  irrecoverableDebtsDisallowable
  professionalFeesDisallowable
  depreciationDisallowable
  otherExpensesDisallowable
```

The schema prohibits additional properties at each object level. Income values are non-negative and limited to two decimal places. Expense and disallowable-expense values may be positive or negative and are limited to two decimal places. This supports faithful reporting of credits and reversals rather than absolute-value normalization.

HMRC's endpoint prose says submissions must include income and expense values even when zero and gives zero `turnover` and `other` as its example. The JSON Schema does not declare top-level `required` properties and does not declare individual income or expense numbers required; it declares only that `periodStartDate` and `periodEndDate` are required if `periodDates` is present. The endpoint also documents date exceptions for annual-status or latent sources. The apparent difference between prose/business rules and structural schema must be resolved through HMRC Sandbox/contract tests before production readiness rules are frozen. It must not be resolved by zero-filling unsupported Objective 2 concepts.

Detailed expenses and `consolidatedExpenses` are mutually exclusive. HMRC's service guide says the consolidated route is available where annual turnover is below £90,000. `periodDisallowableExpenses` is a separate optional object and is not present in HMRC's consolidated example.

For ordinary mandated cumulative reporting, dates describe the cumulative span from the applicable tax-year start to the submitted update end. Standard and calendar reporting types are supported. Later submissions start at the same tax-year start and supersede earlier cumulative submissions. End dates interact with obligations, commencement date, reporting type, early-submission rules, amendment windows and the prohibition on moving a submission end date backwards.

### 21.2 HMRC source and version provenance

| Authority | Evidence used |
|---|---|
| [Self Employment Business (MTD) API v5.0 overview](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-employment-business-api/5.0) | Latest version, beta status, last-updated date, REST service purpose and environments. |
| [HMRC OpenAPI operation definition](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/create_amend_cumulative_period_summary.yaml) | Method, path parameters, scope, tax-year applicability, business-rule errors, required-zero prose and detailed/consolidated mutual exclusion. |
| [HMRC cumulative request JSON Schema](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/schemas/createAmendCumulativePeriodSummary/request.json) | Exact object/property names, structural requiredness, numeric ranges, date format and additional-property rules. |
| [HMRC detailed request example](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/examples/createAmendCumulativePeriodSummary/non_consolidated_request.json) | Confirmed detailed income, expense and disallowable-expense nesting and demonstrated that negative expense values are valid. |
| [HMRC consolidated request example](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/examples/createAmendCumulativePeriodSummary/consolidated_request.json) | Confirmed consolidated-expense nesting and omission of detailed/disallowable fields in that example. |
| [Making updates during the tax year](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/documentation/make-updates-during-tax-year.html) | Cumulative update semantics, supersession, obligation handling, standard/calendar periods and the under-£90,000 consolidated option. |

The exact contract must be rechecked immediately before implementation because v5.0 remains beta.

### 21.3 Proposed minimum Objective 2 cumulative field set

The governing Trade Control reference defines two core income concepts and thirteen core expense concepts. That remains the minimum stable Objective 2 vocabulary. It is deliberately not a copy of every optional HMRC property.

| Objective 2 concept | Classification | Objective 3 relationship |
|---|---|---|
| `turnover` | Deterministic accounting projection where mapped | `periodIncome.turnover` |
| `otherBusinessIncome` | Deterministic accounting projection where mapped | `periodIncome.other` |
| `costOfGoods` | Accounting projection; unsupported by current MIN/STD precision | `periodExpenses.costOfGoods` |
| `paymentsToSubcontractors` | Accounting projection; unsupported by current MIN/STD | `periodExpenses.paymentsToSubcontractors` |
| `wagesAndStaffCosts` | Deterministic accounting projection where mapped | `periodExpenses.wagesAndStaffCosts` |
| `carVanTravelExpenses` | Deterministic in STD; unsupported in MIN | `periodExpenses.carVanTravelExpenses` |
| `premisesRunningCosts` | Conditional pending posting semantics | `periodExpenses.premisesRunningCosts` |
| `maintenanceCosts` | Deterministic in STD subject to posting policy | `periodExpenses.maintenanceCosts` |
| `adminCosts` | Incompletely classified in both templates | `periodExpenses.adminCosts` |
| `advertisingCosts` | Deterministic in STD; unsupported in MIN | `periodExpenses.advertisingCosts` |
| `businessEntertainmentCosts` | Unsupported | `periodExpenses.businessEntertainmentCosts` |
| `interestOnBankOtherLoans` | Deterministic in STD; unsupported in MIN | `periodExpenses.interestOnBankOtherLoans` |
| `financeCharges` | Deterministic in STD subject to posting policy | `periodExpenses.financeCharges` |
| `professionalFees` | Deterministic in STD subject to posting policy | `periodExpenses.professionalFees` |
| `otherExpenses` | Unsupported as a safe residual | `periodExpenses.otherExpenses` |

The following current HMRC properties do not expand that minimum mandatory Tax Tag set:

| HMRC concept | Proposed ownership/status |
|---|---|
| `taxTakenOffTradingIncome` | Optional contextual/external value; excludes CIS deductions and has no current deterministic Category Tree source. |
| `consolidatedExpenses` | Conditional alternate projection, not an additional detailed tag. It requires eligibility plus a proven allowable-expense total. Currently unsupported. |
| `irrecoverableDebts` | Optional detailed accounting concept, currently unsupported by MIN/STD; consider only if the accounting classification is extended. |
| `depreciation` | Optional detailed accounting concept, currently unsupported by the Sole Trader templates because depreciation codes are disabled. It is not a capital allowance. |
| all disallowable-expense properties | Optional statutory information, currently unsupported because no allowability dimension exists. |
| dates and endpoint identifiers | Workflow/context, not Tax Tags. |
| HMRC-required zeros | Objective 3 wire completion, but only for a concept whose support/applicability is known. |

An approved Tax Source may contain the stable core concepts even when a template leaves some unmapped, provided readiness validation exposes those absences. It must not imply that every bootstrap can submit the detailed route.

### 21.4 Trade Control polarity path and proposed rule

The live path is:

```text
Cash.tbTaxTag
  -> Cash.tbTaxTagMap
  -> category expansion or direct CashCode
  -> Cash.tbCode.CategoryCode
  -> leaf Cash.tbCategory.CashPolarityCode
  -> Cash.vwCashCodePeriodValues
  -> Cash.vwTaxBizPayload
  -> Cash.vwTaxBizSubmission
  -> TcBusinessTaxReader
```

`Cash.tbPolarity` defines `0 = Expense`, `1 = Income`, `2 = Neutral`.

`Cash.vwCashCodePeriodValues` is where polarity is currently encountered. It joins each CashCode to its leaf Category and applies:

```text
Expense leaf (0): InvoiceValue * -1
Income/other leaf: InvoiceValue unchanged
```

Consequently `Cash.vwTaxBizSubmission.TaxableAmount` already retains Trade Control economic polarity. An ordinary expense is negative and an expense credit/reversal is positive. `TcBusinessTaxReader` then incorrectly applies `Math.Abs`, destroying that distinction.

The proposed Objective 2 conversion is based on the statutory concept's expected polarity after all Trade Control contributors have been aggregated:

```text
income-oriented statutory value  = Trade Control economic value
expense-oriented statutory value = Trade Control economic value * -1
```

Therefore:

```text
ordinary expense       -1000 ->  1000
expense credit           100 ->  -100
net (-1000 + 100)       -900 ->   900
credits exceed expense   200 ->  -200
```

This is not `ABS`. The conversion belongs in Objective 2 after economic aggregation and before the Objective 3 adapter. Objective 3 may round/validate for the HMRC contract but must not reinterpret Trade Control polarity.

For a direct CashCode mapping, the applicable polarity is the polarity of that CashCode's leaf Category. For a Category mapping, `Cash.fnCategoryCashCodes` recursively expands enabled descendants and returns leaf CashCodes, but does not return polarity. The mapped parent cannot be trusted for polarity: MIN/STD total categories such as `CT-TURNOV`, `CT-STAFFC` and `CT-OVERHD` deliberately use neutral polarity while their nominal descendants are income or expense.

Multiple contributors to one tag are valid only if every effective leaf CashCode has the same economic polarity and that polarity matches the tag's declared statutory orientation. Multiple category roots may be disjoint yet still have conflicting polarities. Recursive expansion can also reach a CashCode from more than one mapped ancestor; that is a duplicate-source error independent of polarity.

The existing projection can expose polarity without a second database traversal. `vwCashCodePeriodValues` already joins the leaf Category; adding its `CashPolarityCode` to that view and carrying it through `vwTaxBizPayload` permits `vwTaxBizSubmission` either to group by validated polarity or expose a single polarity after rejecting conflicts. This is preferable to a separate polarity lookup function, which would repeat category/CashCode traversal and risk diverging from the actual expanded contributors. This is a candidate design only.

### 21.5 Cumulative-period findings

The current period path is not cumulative:

- `Cash.tbTaxTagSource.TaxTypeCode` selects a tax type;
- `UK-ITSA-SE-QU` uses Tax Type 5 (`Quarterly Return`);
- Tax Type 5 has recurrence code 2, which `Cash.fnTaxTypeDueDates` translates to a three-month interval;
- `vwTagCashPeriodMap` cross-applies those due-date windows to every mapped tag;
- `vwTaxBizPayload` includes accounting periods where `StartOn >= PeriodFrom AND StartOn < PeriodTo`;
- `vwTaxBizSubmission` aggregates each separate window; and
- `TcBusinessTaxReader` selects a row set by exact `PeriodTo`.

This produces discrete three-month accounting windows, not tax-year-to-date cumulative totals. It also uses an exclusive SQL `PeriodTo`, whereas HMRC's `periodEndDate` is an inclusive calendar date. The Sole Trader template moves the first accounting period start to 6 April, but subsequent accounting month starts and the generic recurrence calculation do not by themselves establish HMRC's standard 5 July/5 October/5 January/5 April boundaries or an elected calendar-quarter sequence.

The accounting-period model should remain intact. The proposed cumulative projection should accept or join an explicit workflow period context containing tax year, cumulative start and inclusive update end. It should aggregate the existing period values from the effective tax-year start through `< DATEADD(day, 1, updateEnd)` (or an equivalent inclusive rule), rather than altering `App.tbYearPeriod` or repurposing `fnTaxTypeDueDates`.

The authoritative update end should ultimately come from the applicable obligation/reporting-type context. Commencement, annual/latent status, standard/calendar election, first-year exceptions and the final 1-5 April calendar gap cannot be inferred safely from Tax Type 5 recurrence alone.

### 21.6 MIN mapping reassessment

| Concept | Exact MIN evidence | Status | Polarity / overlap finding |
|---|---|---|---|
| turnover | `CT-TURNOV -> CA-SALES -> CC-SALES` | Supported deterministically | Leaf `CA-SALES` is Income (1). |
| otherBusinessIncome | `CT-OTHRIN -> CA-INCOME -> CC-INCME` | Conditional | Leaf is Income (1), but confirm the code contains taxable business income only. |
| wagesAndStaffCosts | `CT-STAFFC -> CA-WAGES -> CC-WAGES, CC-PENSN`; `CC-EMPNI` disabled by Sole Trader MIN | Supported, subject to scope confirmation | All effective leaves are Expense (0); no internal overlap. |
| costOfGoods | `CT-CSTSAL -> CA-DIRECT -> CC-DIRCT` and conditional `CC-MINER` | Unsupported | “Direct costs” is broader than goods bought for resale/used. Polarity is consistent but semantics are not. |
| all other core detailed expenses | only coarse `CT-OVERHD -> CA-ADMIN -> CC-ADMIN`, or no code | Unsupported | Expense polarity is consistent; statutory distinctions are absent. |
| consolidatedExpenses | possible cost/staff/overhead roll-ups | Conditional and currently unsupported | Polarity is consistently expense for trade leaves, but allowability/private-use and £90,000 eligibility are not established. |
| irrecoverable debts, depreciation and disallowables | no enabled deterministic source | Unsupported | Disabled depreciation is absence, not zero. |

The supported MIN candidates are disjoint at the leaf CashCode level. Mapping a higher ancestor such as `CT-PANDL` would cross income and expense polarity and would also overlap the dedicated mappings; it must not be used.

### 21.7 STD mapping reassessment

STD inherits MIN and adds expense classifications. All listed STD nominal expense categories have Expense polarity (0).

| Concept | Exact STD evidence | Status | Overlap / roll-up finding |
|---|---|---|---|
| turnover | `CT-TURNOV` | Supported | Expands only to `CC-SALES`; Income polarity. |
| otherBusinessIncome | `CT-OTHRIN` | Conditional | Expands only to `CC-INCME`; confirm taxable content. |
| wagesAndStaffCosts | `CT-STAFFC` | Supported subject to scope | `CC-WAGES` and `CC-PENSN`; disjoint from other candidates. |
| carVanTravelExpenses | `CA-MOTOR` + `CA-TRAVEL` | Supported | Disjoint sibling categories under `CT-OVERHD`; all ten leaf codes are Expense. Do not also map `CT-OVERHD`. |
| premisesRunningCosts | `CA-PREMS` + possible `CC-INSUR` | Conditional | Sources are disjoint, but `CC-INSUR` may contain non-premises insurance and `CA-PREMS` includes cleaning/generic premises costs. |
| maintenanceCosts | `CC-REPA` | Supported subject to posting policy | Expense polarity. Excludes motor repair `CC-MREPA`, correctly retained in car/van/travel. |
| adminCosts | `CC-PHONE` plus no deterministic stationery/office source | Unsupported as complete | Mapping `CC-PHONE` alone would be partial; `CC-ADMIN` is too broad. |
| advertisingCosts | `CC-ADVT` | Supported | Dedicated, disjoint Expense code. |
| interestOnBankOtherLoans | `CC-LOINT` | Supported | Dedicated Expense code. |
| financeCharges | `CC-BANKC` + `CC-FINCH` | Supported subject to posting policy | Disjoint Expense codes in different categories. |
| professionalFees | `CC-PROF` | Supported subject to posting policy | Dedicated Expense code, semantically broad but aligned if posting is controlled. |
| costOfGoods | `CT-CSTSAL` / `CA-DIRECT` | Unsupported | Same semantic breadth as MIN. |
| paymentsToSubcontractors | none | Unsupported | No classification. |
| businessEntertainmentCosts | none | Unsupported | No classification. |
| otherExpenses | `CC-ADMIN` or residual | Unsupported | A residual would absorb amounts belonging to named but unsupported concepts. |
| consolidatedExpenses | all allowable expense sources | Conditional and currently unsupported | Uniform expense polarity is insufficient; eligibility and allowability are missing. |
| irrecoverable debts, depreciation and disallowables | no enabled deterministic source | Unsupported | No legitimate mapping. |

The proposed multi-row tags (`CA-MOTOR` + `CA-TRAVEL`, `CC-BANKC` + `CC-FINCH`) are disjoint. Existing same-tag validation must still prove their expanded CashCode sets do not intersect after user customisation. Cross-tag validation must ensure, for example, motor repairs are not also included in general maintenance.

### 21.8 Validation requirements

Before the cumulative slice can be accepted, validation must cover:

1. an approved source/tag manifest and explicit tag orientation (`Income` or `Expense`);
2. every enabled mapping resolving to at least one existing enabled CashCode;
3. every effective CashCode for a tag resolving to exactly one non-neutral leaf polarity;
4. all effective contributors to a tag sharing one polarity and matching the tag orientation;
5. no effective CashCode appearing twice within a tag through parent/descendant or multi-root expansion;
6. no effective CashCode appearing in mutually exclusive tags across the same projection;
7. supported/conditional/unsupported state remaining explicit and independent from an accounting value of zero;
8. reconciliation from effective CashCodes to Trade Control economic total and then to statutory total;
9. cumulative start/end validation, inclusive end treatment, tax-year consistency and non-decreasing submission end dates;
10. standard/calendar/commencement context supplied by an authoritative workflow source;
11. detailed/consolidated mutual exclusion, plus evidence for consolidated-route eligibility and allowability;
12. two-decimal range validation without absolute-value conversion;
13. HMRC income non-negativity and a defined failure path if an income classification nets negative;
14. Objective 3 required-zero completion only after Objective 2 support/applicability is known; and
15. exact JSON contract tests rejecting incorrect names, casing, nesting, extra properties and unintended defaults.

### 21.9 Proposed Objective 2 / Objective 3 seam

| Responsibility | Owner |
|---|---|
| Operational period values and economic sign | Trade Control accounting |
| Category/CashCode expansion and traceability | Objective 2 SQL projection |
| Leaf polarity evidence and ambiguity validation | Objective 2 SQL projection |
| Cumulative aggregation using supplied tax-year/update context | Objective 2 |
| Conversion from Trade Control economic sign to statutory income/expense orientation | Objective 2 |
| Stable cumulative Tax Tag vocabulary and support state | Objective 2 |
| NINO, HMRC business ID, tax-year path value, obligation/reporting type and chosen update end | Workflow/context passed to Objective 3 and, where needed, Objective 2 period selection |
| Mapping stable statutory concepts to exact HMRC property paths | Objective 3 |
| Detailed/consolidated request choice after eligibility/readiness checks | Objective 3 consuming Objective 2 status plus workflow policy |
| HMRC-required zeros for supported/applicable fields | Objective 3 |
| Exact request classes, casing, omission policy, serialization and contract validation | Objective 3 |
| HTTP/OAuth/fraud headers and submission | Objective 4, outside this slice |

The smallest Objective 2 record should expose source, stable concept code, statutory amount, orientation, cumulative start/end, support status and traceability/audit contributors. It should not contain HMRC path parameter names or JSON object nesting. Objective 3 must receive already converted statutory values; it must not see a raw negative Trade Control expense and decide independently how to flip it.

### 21.10 Proposed first constructive cumulative vertical slice

The first implementation should be one bounded, transport-free vertical slice:

1. **SQL manifest and polarity evidence**
   - replace the surviving historical QU seed with one approved cumulative Tax Source whose identifier fits the schema consistently;
   - seed only the approved core cumulative tags with explicit orientation metadata;
   - extend the effective mapping/projection path to carry leaf `CashPolarityCode` without a second traversal;
   - add cumulative projection input/query semantics separate from `fnTaxTypeDueDates`;
   - extend validation for same-tag duplication, cross-tag overlap, neutral/mixed polarity and orientation mismatch;
   - install only separately approved deterministic MIN/STD mappings.
2. **C# Objective 2 reader/projection**
   - replace `Math.Abs` with consumption of the validated statutory amount produced by Objective 2;
   - return support status, period and traceability rather than a harness-shaped item list;
   - fail on mixed/neutral polarity or ambiguous period context.
3. **Objective 3 contract**
   - implement API v5 cumulative request types with exact `periodDates`, `periodIncome`, `periodExpenses` and optional `periodDisallowableExpenses` JSON names;
   - use nullable/optional members so unsupported properties are not silently emitted;
   - implement detailed/consolidated mutual exclusion and contract limits;
   - keep path/context identifiers outside the body DTO.
4. **Exact serializer**
   - use explicit JSON property naming and omission rules;
   - serialize the production Objective 3 object directly;
   - add golden JSON/schema tests including negative expense credits and rejection of extra properties.
5. **Test Harness observation**
   - add one diagnostic endpoint that calls the production Objective 2 projection and production Objective 3 builder/serializer;
   - return the actual projection plus exact serialized JSON as observation output;
   - create no harness vocabulary or zero-filling mapper.
6. **Synthetic acceptance cases**
   - income `+1000` -> statutory income `1000`;
   - ordinary expense Trade Control `-1000` -> statutory expense `1000`;
   - expense credit Trade Control `+100` -> statutory expense `-100`;
   - expense `-1000` plus credit `+100` -> statutory expense `900`;
   - credits exceeding expense, for example `-100 + 300 = +200`, -> statutory expense `-200`;
   - mixed-polarity contributors to one tag -> validation failure;
   - parent/descendant duplicated CashCode -> validation failure;
   - unsupported concept -> absent Objective 2 value, never automatic zero;
   - exact standard and calendar cumulative boundaries -> expected year-to-date totals; and
   - detailed plus consolidated fields -> Objective 3 validation failure.

Acceptance requires SQL and both .NET projects to build, database fixtures to exercise real recursive expansion and period aggregation, exact serialized JSON comparison, and the harness to expose the same production output. No HTTP transport is part of this slice.

### 21.11 Unresolved questions requiring human decision

1. Approve the cumulative Tax Source identifier and the stable Objective 2 tag naming convention. The Phase 3 descriptive proposal may exceed historical identifier widths; a short identifier such as `UK-ITSA-SE-CUM` is possible but not pre-approved.
2. Decide whether the first product route targets detailed expenses only, consolidated expenses only for eligible businesses, or supports both. Current MIN/STD cannot truthfully populate a complete detailed breakdown and cannot yet prove an allowable consolidated total.
3. Decide whether the bootstrap classifications should be extended later to support cost of goods, subcontractors, office costs, entertainment, other expenses, irrecoverable debts and disallowable amounts, or whether those remain unsupported/user-supplied.
4. Confirm posting-policy semantics for other income, staff costs, premises/insurance, repairs, bank charges and professional fees before approving conditional mappings.
5. Select the authoritative workflow source for business ID, reporting type, commencement/latent status, obligation end and calendar-quarter election.
6. Confirm through current HMRC Sandbox tests which income/expense members must be present as zero, because endpoint prose is stricter than the published JSON Schema's `required` arrays.
7. Decide the readiness behaviour when a mapped income concept nets negative, since HMRC income fields prohibit negative values while Trade Control must preserve the economic result.
8. Decide whether cumulative querying is exposed through a parameterised stored procedure/table-valued function or a context table plus view. A plain global view cannot safely accept tenant-specific update context.
9. Approve whether tag orientation is new explicit metadata or a constrained classification derived from an existing tag-class mechanism. It must not be inferred from neutral parent categories.
10. Confirm that existing deployed SA/EOPS rows will be handled by a separate upgrade migration before the constructive source replacement is deployed.

### 21.12 Completion position

Phase 4B provides enough evidence to decide whether to authorise a first cumulative vertical slice, but it also shows that a truthful end-to-end submission cannot be declared generally ready from the current MIN/STD templates alone. The safe implementation can establish the projection, polarity, period, contract and serialization architecture while leaving unsupported fields absent and submission readiness blocked where the selected HMRC route requires unavailable information.

No constructive implementation has begun.

## 22. Phase 4C — MIN/STD Cumulative Bootstrap Reconnaissance

This section records the Phase 4C read-only reconnaissance performed on 29 August 2026. It applies the architectural decisions in the Phase 4C session brief to the Phase 4A baseline and Phase 4B design. Everything described as a structure, source, tag, mapping, interface or validation change below is proposed; no implementation was made.

### 22.1 Authority and current implementation

#### HMRC requirement

The statutory quarterly update direction, updated 27 March 2026, requires a self-employed person to provide the update-period start/end dates, two income totals and thirteen expense-category totals. The thirteen directed expense categories are cost of goods; subcontractor payments; staff costs; car/van/travel; premises; repairs/maintenance; office costs; advertising; business entertainment; loan interest; financial charges; professional fees; and other business expenses.

Self Employment Business (MTD) API v5.0 beta remains the latest published API, last updated 20 August 2026 when rechecked. Its cumulative request provides the corresponding detailed properties plus `consolidatedExpenses`, `irrecoverableDebts`, `depreciation`, `taxTakenOffTradingIncome`, and a parallel optional disallowable-expense object. The additional API properties are not all requirements of the quarterly direction and do not all require Objective 2 Tax Tags.

HMRC's service guide permits businesses with annual turnover below £90,000 to use consolidated expenses instead of a detailed expense breakdown. The API prohibits consolidated and detailed expenses in the same request.

#### Trade Control decisions

- MIN is a bootstrap for the consolidated pattern.
- STD inherits MIN's accounting base and refines it for all thirteen directed detailed expense categories.
- The Category Tree remains a business/accounting classification, not an HMRC tree.
- Gross quarterly expense classification remains accounting information. No parallel disallowable Category Tree is proposed.
- Objective 2 converts economic polarity; Objective 3 does not reinterpret it.
- Submission capability is validated from the configured mappings after user customisation, not inferred from the original template name.

#### Current implementation

MIN currently has dedicated income, direct-cost and staff branches but only a generic admin branch for overheads. It has no single all-expense parent. STD adds travel, motor, finance and premises categories plus several CashCodes under the inherited admin branch, but it lacks complete deterministic separation for cost of goods, subcontractors, office costs, entertainment and other expenses. Phase 4A left the historical `UK-ITSA-SE-QU` source and its old tag vocabulary in place without mappings. The current SQL projection uses discrete due-date windows and the C# reader still applies `Math.Abs`.

### 22.2 Authoritative HMRC provenance

| Source | Contract-dependent conclusion |
|---|---|
| [MTD Income Tax quarterly update direction](https://www.gov.uk/government/publications/update-notice-for-making-tax-digital-for-income-tax/making-tax-digital-for-income-tax-update-notice), updated 27 March 2026 | Legal update information: dates, two income totals and thirteen expense totals. |
| [Self Employment Business (MTD) API v5.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-employment-business-api/5.0), last updated 20 August 2026 | Current REST service/version and cumulative/annual service scope. |
| [HMRC cumulative request schema](https://github.com/hmrc/self-employment-business-api/blob/main/resources/public/api/conf/5.0/schemas/createAmendCumulativePeriodSummary/request.json) | Exact API properties, numeric signs/ranges, optional additional fields and object structure. |
| [Making updates during the tax year](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/documentation/make-updates-during-tax-year.html) | Cumulative periods, supersession, standard/calendar reporting and under-£90,000 consolidated option. |

API v5.0 remains beta and must be rechecked immediately before Objective 3 implementation.

### 22.3 Proposed cumulative Tax Source and Tax Tags

Propose one source:

```text
TaxSourceCode: UK-ITSA-SE-CUM
Meaning:       Sole Trader cumulative statutory accounting projection
```

`UK-ITSA-SE-CUM` fits the current `NVARCHAR(20)` schema. It replaces, rather than aliases, historical `UK-ITSA-SE-QU`. It represents Objective 2 statutory meaning and is not an HMRC DTO name.

Propose the following stable vocabulary:

| TagCode | Orientation | Purpose |
|---|---:|---|
| `turnover` | Income | Directed business-income total. |
| `otherBusinessIncome` | Income | Directed other-business-income total. |
| `consolidatedExpenses` | Expense | Conditional alternative used by MIN-style consolidated configurations. |
| `costOfGoods` | Expense | Directed detailed category. |
| `paymentsToSubcontractors` | Expense | Directed detailed category. |
| `wagesAndStaffCosts` | Expense | Directed detailed category. |
| `carVanTravelExpenses` | Expense | Directed detailed category. |
| `premisesRunningCosts` | Expense | Directed detailed category. |
| `maintenanceCosts` | Expense | Directed detailed category. |
| `adminCosts` | Expense | Directed phone/fax/stationery/office category. |
| `advertisingCosts` | Expense | Directed detailed category. |
| `businessEntertainmentCosts` | Expense | Directed detailed category. |
| `interestOnBankOtherLoans` | Expense | Directed detailed category. |
| `financeCharges` | Expense | Directed detailed category. |
| `professionalFees` | Expense | Directed detailed category. |
| `otherExpenses` | Expense | Directed residual category, populated only by explicitly classified other-expense codes. |

Orientation should be explicit tag metadata, not inferred from `TagClassCode` labels or neutral total categories. An explicit `CashPolarityCode` column linked to `Cash.tbPolarity` is preferable to overloading presentation-oriented tag classes.

Do not seed cumulative tags for `taxTakenOffTradingIncome`, `irrecoverableDebts`, `depreciation` or any disallowable-expense property in Phase 4D. The first is contextual/external; the next two are optional API properties outside the thirteen directed categories; and the last group has no Trade Control source. They may be introduced later only from verified need and deterministic data.

### 22.4 Proposed MIN consolidated bootstrap

MIN needs one new structural expense total, proposed as:

```text
CT-CUMEXP  Cumulative reportable expenses (Total, Neutral)
```

It should roll up the complete set of MIN expense branches that are eligible for consolidated reporting:

```text
CT-CUMEXP
├─ CT-CSTSAL -> CA-DIRECT -> CC-DIRCT [coarse reportable direct expense]
├─ CT-STAFFC -> CA-WAGES  -> CC-WAGES, CC-PENSN, CC-EMPNI
└─ CT-OVERHD -> CA-ADMIN  -> CC-ADMIN [coarse reportable overhead]
```

The existing branches may remain simultaneously under their profit-and-loss parents because the Category Tree is a directed acyclic reporting graph. A Tax Tag mapping to `CT-CUMEXP` expands its leaves once; it must never be combined with mappings to those descendants for the same submission configuration.

Minimum accounting changes proposed:

- add `CT-CUMEXP` and its three child relationships;
- stop disabling `CC-EMPNI`: a sole trader may employ staff and employer National Insurance is a staff cost;
- define `CC-DIRCT` and `CC-ADMIN` explicitly as coarse **reportable business expense** buckets in MIN posting guidance;
- keep owner drawings/capital, tax, asset movements, transfers, personal expenditure and any known non-reportable/non-business amounts outside `CT-CUMEXP`; and
- document that using a coarse bucket for mixed reportable and excluded amounts invalidates consolidated capability until the business refines its classification.

HMRC's API describes `consolidatedExpenses` as allowable expenses, while the architectural decision correctly rejects building a duplicate disallowable taxonomy for quarterly accounting. The practical bootstrap rule is therefore containment, not duplication: the mapped MIN parent may contain only amounts the business is entitled to include in the consolidated update. Separate non-reportable accounting codes may exist outside it. Validation can prove structural containment but cannot determine tax allowability from a CashCode name; posting policy and user review remain necessary.

#### Proposed MIN mapping matrix

| Tax Tag | Map type | Proposed source | Status and rationale |
|---|---:|---|---|
| `turnover` | Category | `CT-TURNOV` | Deterministic; expands to income leaf `CC-SALES`. |
| `otherBusinessIncome` | Category | `CT-OTHRIN` | Deterministic if bootstrap posting definition is restricted to reportable other business income. |
| `consolidatedExpenses` | Category | `CT-CUMEXP` | Deterministic for the approved consolidated pattern when containment and eligibility validation pass. |
| all thirteen detailed expense tags | none | — | Deliberately unmapped in MIN; consolidated and detailed patterns are exclusive. |

No zero-valued map rows or placeholder mappings are proposed.

### 22.5 Proposed STD detailed bootstrap

STD should retain MIN's overall accounting base but replace its two ambiguous coarse expense CashCodes with a complete detailed classification. Because these are bootstrap prototypes with no backwards-compatibility constraint, preserving ambiguous `CC-DIRCT` and `CC-ADMIN` as enabled posting choices is counterproductive. STD should disable them and provide explicit replacements.

Proposed structure beneath the existing P&L expense area:

```text
CT-CSTSAL
├─ CA-COGS       Cost of goods
└─ CA-SUBCON     Subcontractor payments

CT-STAFFC
└─ CA-WAGES      Wages and staff costs

CT-OVERHD
├─ CA-TRAVEL     Travel and subsistence
├─ CA-MOTOR      Motor expenses
├─ CA-PREMS      Premises running costs
├─ CA-REPAIR     Repairs and maintenance
├─ CA-OFFICE     Phone, stationery and office costs
├─ CA-ADVERT     Advertising
├─ CA-ENTERT     Business entertainment
├─ CA-LOANINT    Bank and loan interest
├─ CA-FINANCE    Other financial charges
├─ CA-PROF       Accountancy, legal and professional fees
└─ CA-OTHER      Other business expenses
```

All nominal categories above are Expense polarity. Structural totals remain Neutral. The precise CashCode inventory can remain useful business detail beneath each nominal category. Proposed treatment of current/new codes is:

| Detailed branch | CashCodes |
|---|---|
| `CA-COGS` | new `CC-COGS` generic goods/materials code; add further business-specific goods codes later. Disable inherited ambiguous `CC-DIRCT`. |
| `CA-SUBCON` | new `CC-SUBCON` for construction-industry subcontractor payments. |
| `CA-WAGES` | existing `CC-WAGES`, `CC-PENSN`, and retained/re-enabled `CC-EMPNI`. |
| `CA-TRAVEL` | existing `CC-PARK`, `CC-PUBTR`, `CC-HOTEL`, `CC-MEALS`. |
| `CA-MOTOR` | existing `CC-MFUEL`, `CC-MREPA`, `CC-MINSR`, `CC-MLICN`, `CC-MLEASE`. |
| `CA-PREMS` | existing `CC-RENT`, `CC-UTILS`, `CC-CLEAN`, `CC-PREMS`; move `CC-INSUR` here and define it as reportable premises/business insurance within this directed category. |
| `CA-REPAIR` | move existing `CC-REPA`; motor repairs stay in `CA-MOTOR`. |
| `CA-OFFICE` | move existing `CC-PHONE`; add `CC-OFFICE` for stationery and other office costs. Disable inherited ambiguous `CC-ADMIN`. |
| `CA-ADVERT` | move existing `CC-ADVT`. |
| `CA-ENTERT` | new `CC-ENTERT`. This is an accounting total required by the direction; no disallowable duplicate is created. |
| `CA-LOANINT` | move existing `CC-LOINT`. |
| `CA-FINANCE` | existing `CC-FINCH`; move `CC-BANKC` here. |
| `CA-PROF` | move existing `CC-PROF`. |
| `CA-OTHER` | new `CC-OTHER`; use only for expenses affirmatively classified as the directed residual, never as an automatic remainder. |

STD may retain `CT-CUMEXP` as inherited accounting structure, but the STD MTD wrapper must not map it. Its descendants are instead mapped to detailed tags. Keeping the structural parent is useful for accounting roll-up and does not itself double count because only map rows, not Category Tree membership, select projection contributors.

#### Proposed STD mapping matrix

| Tax Tag | Map type | Proposed source(s) | Polarity / overlap |
|---|---:|---|---|
| `turnover` | Category | `CT-TURNOV` | Income leaves only. |
| `otherBusinessIncome` | Category | `CT-OTHRIN` | Income leaves only; posting definition restricted to reportable other income. |
| `consolidatedExpenses` | none | — | Deliberately absent in detailed configuration. |
| `costOfGoods` | Category | `CA-COGS` | Expense leaves; disjoint from subcontractors. |
| `paymentsToSubcontractors` | Category | `CA-SUBCON` | Expense leaves; dedicated code. |
| `wagesAndStaffCosts` | Category | `CT-STAFFC` | Expense leaves `CC-WAGES`, `CC-PENSN`, `CC-EMPNI`. |
| `carVanTravelExpenses` | Category (two rows) | `CA-MOTOR`, `CA-TRAVEL` | Disjoint sibling branches combined into the directed concept. |
| `premisesRunningCosts` | Category | `CA-PREMS` | Includes the deliberately moved insurance code. |
| `maintenanceCosts` | Category | `CA-REPAIR` | Excludes motor repairs, preventing overlap with car/van/travel. |
| `adminCosts` | Category | `CA-OFFICE` | Complete office branch; ambiguous inherited admin code disabled. |
| `advertisingCosts` | Category | `CA-ADVERT` | Dedicated branch. |
| `businessEntertainmentCosts` | Category | `CA-ENTERT` | Dedicated accounting branch. |
| `interestOnBankOtherLoans` | Category | `CA-LOANINT` | Dedicated interest branch. |
| `financeCharges` | Category | `CA-FINANCE` | Financial charges and moved bank-charge code. |
| `professionalFees` | Category | `CA-PROF` | Dedicated professional-fee branch. |
| `otherExpenses` | Category | `CA-OTHER` | Explicit residual postings only. |

This matrix covers all two-plus-thirteen concepts in the statutory direction. It does not map optional API-only properties.

### 22.6 Mutual exclusivity and wrapper ownership

The common tax-seeding procedure should create the source and full alternative vocabulary but no mappings. Variant mapping remains at the composition boundary:

- MIN MTD wrapper installs income plus `consolidatedExpenses` mappings.
- STD MTD wrapper installs income plus all thirteen detailed mappings and no consolidated mapping.

STD accounting may inherit `CT-CUMEXP`, but it does not inherit MIN MTD wrapper mappings because `App.proc_Template_ST_SOLE_CUR_STD_MTD_2026` calls the STD accounting procedure directly, not the MIN MTD wrapper. This existing call graph provides clean variant separation.

Configuration validation must enforce exactly one expense pattern for a submission-capable source:

```text
Consolidated pattern:
  consolidatedExpenses mapped
  AND no detailed expense tag mapped

Detailed pattern:
  consolidatedExpenses unmapped
  AND every one of the thirteen directed detailed tags mapped
```

Any partial detailed configuration is valid as saved custom configuration but is not submission-ready. Any simultaneous consolidated/detailed map is an error. Effective CashCode overlap must also be checked; deleting only the parent map is insufficient if another consolidated-style root remains mapped.

### 22.7 Smallest polarity propagation design

Current economic sign is established in `Cash.vwCashCodePeriodValues` from each CashCode's nominal leaf Category. The smallest non-duplicative change is:

1. expose the already joined leaf `CashPolarityCode` from `vwCashCodePeriodValues`;
2. carry it through a reusable effective Tax Tag/CashCode projection and the cumulative aggregation;
3. group or return by tag and polarity so mixed contributors remain visible rather than silently summed; and
4. apply orientation conversion only after validation proves one effective polarity per tag.

Do not use a mapped total category's neutral polarity. Do not perform a separate reader-side category traversal. Do not use `ABS()` or `Math.Abs()`.

Proposed conversion:

```text
Income tag:  StatutoryAmount = TradeControlAmount
Expense tag: StatutoryAmount = TradeControlAmount * -1
```

If a tag has zero contributors it is unsupported/unmapped, not an accounting zero. If it has contributors whose net is zero, it is supported with a genuine zero. These states must remain distinct.

### 22.8 Validation for bootstrap and customised trees

Validation must operate on effective expanded CashCodes, so the same rules apply after a user changes the Category Tree:

The mapping UI deliberately permits a user to select any Category or CashCode. Consequently `Cash.proc_TaxTagMapValidate` (through its effective-map validation function) is the authoritative enforcement boundary rather than the UI selection list. The selected mapping determines the **actual contributor polarity**, derived from each effective leaf through `Cash.tbCode -> Cash.tbCategory -> Cash.tbPolarity`; the Tax Tag's explicit statutory orientation declares the **required polarity** independently. Validation must compare those two values and reject every enabled mapping whose effective leaves are neutral, mixed, or contrary to the tag orientation. It must not infer or overwrite the tag orientation from the user's selection, because doing so would allow an erroneous mapping to validate itself. This constraint applies equally to direct CashCode mappings, Category mappings, multiple roots and customised Category Trees. A future UI may warn or filter opportunistically, but server-side validation remains mandatory because the Category Tree can change after mapping and must not be constrained to bootstrap or HMRC-shaped structures.

- source manifest contains exactly the approved tags and orientations;
- map references exist and resolve to enabled leaves;
- no effective contributor has Neutral or null polarity;
- every contributor to an Income tag is Income and every contributor to an Expense tag is Expense;
- all contributors to one tag share the same polarity;
- no CashCode is reached twice within one tag through parent/descendant or multiple-root mappings;
- no CashCode is mapped across two mutually exclusive cumulative tags;
- consolidated and detailed expense patterns never coexist;
- consolidated configuration maps all intended reportable expense leaves and excludes known non-reportable leaves;
- detailed configuration maps all thirteen directed concepts;
- each detailed tag has at least one legitimate mapping, even when its current period amount is zero;
- disabled coarse STD codes (`CC-DIRCT`, `CC-ADMIN`) cannot silently remain effective contributors;
- mapping coverage reconciles to the relevant configured accounting expense universe;
- period bounds are valid, cumulative and tax-year consistent; and
- warnings distinguish custom but submission-capable structures from unsupported/invalid structures without requiring bootstrap code names.

Completeness is pattern-specific. MIN readiness does not require thirteen detailed mappings; STD readiness does. Template identity itself is not a validation input.

### 22.9 Cumulative-period interface

Do not alter or repurpose `Cash.fnTaxTypeDueDates` or the existing discrete `vwTagCashPeriodMap` windows. They express generic tax due-date recurrence and may have other consumers.

Propose two reusable pieces:

1. `Cash.vwTaxTagCashCode` (name provisional): expands enabled Category/CashCode mappings once and exposes source, tag, mapping root, effective CashCode, leaf Category and leaf polarity, without attaching due-date windows.
2. `Cash.fnTaxBizCumulative(@TaxSourceCode, @PeriodStart, @PeriodEnd)` (name provisional): joins that effective map to `vwCashCodePeriodValues`, filters `StartOn >= @PeriodStart` and `StartOn < DATEADD(DAY, 1, @PeriodEnd)`, aggregates by tag and effective polarity, and returns Trade Control economic totals plus statutory totals after orientation validation.

An inline table-valued function is the smallest composable interface because a view cannot safely accept the business-specific start/end context. If SQL Server date-range and monthly-period semantics show that `StartOn < DATEADD(DAY, 1, @PeriodEnd)` is insufficient for part-month boundaries, the implementation must use the authoritative underlying transaction/accrual surface rather than pretend monthly buckets provide day precision. This must be proven with fixtures before acceptance.

Period start/end are supplied from workflow/obligation context. Standard/calendar election, commencement, latent/annual status and HMRC business identity remain outside the Category Tree and Tax Tags.

### 22.10 Smallest Objective 2 result contract and Objective 3 seam

The minimum production result consumed by Objective 3 is:

```text
CumulativeProjection
  TaxSourceCode
  PeriodStart
  PeriodEnd
  ValidationStatus
  Values[]

CumulativeProjectionValue
  TagCode
  Orientation
  SupportStatus       // Supported, Unsupported, Invalid
  StatutoryAmount?    // null unless Supported
```

Contributor-level CashCode, mapping-root, Trade Control amount and polarity evidence should be available through an audit result or diagnostic companion, but need not inflate the minimal Objective 3 input.

Objective 2 owns mapping expansion, cumulative aggregation, polarity validation/conversion, support status and statutory values. Objective 3 owns mapping `TagCode` to exact HMRC property names, request-shape selection, contract-required zeros for supported/applicable concepts, path/context binding, exact JSON types/names/omission and serialization. Objective 3 must reject an invalid/not-ready projection; it must not repair it.

For the consolidated route, Objective 3 consumes the two income values and `consolidatedExpenses`. For the detailed route, it consumes the two incomes and thirteen detailed expense values. Optional API-only values are added from separately authoritative context only when present; they are not manufactured from missing Tax Tags.

### 22.11 Files likely to change in Phase 4D

#### SQL Node

- `App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql` — add consolidated structural parent, relationships, posting semantics and retain employer NI.
- `App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_2026.sql` — replace ambiguous coarse STD choices with complete detailed branches/CashCodes and moves.
- `App/Stored Procedures/proc_Template_ST_SOLE_CUR_TAX_MTD_2026.sql` — replace historical source/tags with the approved cumulative manifest and orientation.
- `App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql` — install MIN consolidated mappings then validate.
- `App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_MTD_2026.sql` — install STD detailed mappings then validate.
- `Cash/Tables/tbTaxTag.sql` plus a small reference table if required — explicit statutory orientation.
- `Cash/Views/vwCashCodePeriodValues.sql` — expose existing leaf polarity.
- new reusable effective mapping view and cumulative inline table-valued function; corresponding `.sqlproj` entries.
- `Cash/Functions/fnTaxTagMapValidate.sql` and/or a cumulative-specific validator — pattern, polarity, coverage and cross-tag validation.
- database fixture/test scripts promoted as acceptance assets, not the non-authoritative scratch enquiry.

#### `hmrc_mtd`

- `Models/Tc/TcBusinessTaxView.cs` or a replacement Objective 2 model — support/orientation/period/statutory result contract.
- `Services/TcData/TcBusinessTaxReader.cs` — call the cumulative interface and remove absolute-value transformation.
- new Objective 3 API v5 cumulative request classes and explicit serializer configuration.
- new production cumulative builder/adapter separated from harness infrastructure.
- a minimal WebHarness controller/registration that invokes the production reader, adapter and serializer and exposes their actual outputs.
- contract and polarity tests in an appropriate test project; none currently exists in the inspected solution.

No transport, OAuth, fraud-header, annual, loss, finalisation, VAT or Corporation Tax file is part of Phase 4D.

### 22.12 Remaining unsupported/contextual information and decisions

#### Remains contextual or unsupported

- NINO, HMRC business ID, tax year, obligation/reporting type and chosen cumulative end are contextual.
- `taxTakenOffTradingIncome` remains externally/contextually supplied and excludes CIS deductions.
- disallowable-expense API properties remain unsupported; no duplicate quarterly taxonomy is proposed.
- `irrecoverableDebts` and `depreciation` remain optional API-only concepts outside the first source manifest.
- annual adjustments, allowances, losses and finalisation remain later work.
- a customised Category Tree that mixes concepts or polarities remains non-submission-capable until remapped/refined.

#### Human decisions still required before Phase 4D

1. Approve `UK-ITSA-SE-CUM` and the sixteen-tag vocabulary.
2. Approve the proposed MIN `CT-CUMEXP` containment rule and decide how UI/posting guidance identifies amounts excluded from consolidated expenses without creating a disallowable taxonomy.
3. Approve the proposed STD category/CashCode names, disabling of `CC-DIRCT`/`CC-ADMIN`, movement of existing codes, and retention of `CC-EMPNI`.
4. Approve the explicit tag-orientation schema design.
5. Approve wrapper-owned alternative mappings and the submission-readiness pattern rules.
6. Choose the authoritative source for cumulative date/reporting context.
7. Confirm whether monthly `Cash.tbPeriod` values provide sufficient accuracy for HMRC update ends that fall inside an accounting month; otherwise select the existing transaction/accrual surface for day-accurate aggregation.
8. Confirm HMRC required-zero behaviour through Sandbox/contract testing before fixing Objective 3 omission/default rules.
9. Decide whether deployed obsolete source/template data cleanup is included in Phase 4D or remains a separate upgrade migration.

### 22.13 Proposed bounded Phase 4D implementation

Implement one transport-free cumulative vertical slice in this order:

1. Add tag orientation and the approved cumulative source/tag manifest; remove historical QU vocabulary.
2. Refine MIN with `CT-CUMEXP` and STD with the complete detailed accounting branches/codes.
3. Add wrapper-owned mutually exclusive MIN consolidated and STD detailed mappings.
4. Add effective-leaf mapping/polarity projection and pattern-aware validation.
5. Add the parameterised cumulative query with explicit inclusive-period semantics.
6. Add SQL fixtures for MIN, STD, custom trees, overlap, mixed polarity, true zero, reversals and standard/calendar periods.
7. Add the minimal Objective 2 C# result and reader, removing `Math.Abs` only as part of this verified replacement path.
8. Add exact API v5 Objective 3 request classes, builder and serialization tests for consolidated and detailed requests.
9. Add one harness observation endpoint over the production projection/builder/serializer.
10. Build SQL Node, `HMRC_MTD` and WebHarness; run database, polarity, mapping, contract and golden-JSON tests; stop before transport.

Phase 4D acceptance must demonstrate both bootstrap patterns, prove that customised mappings—not template identity—govern capability, preserve credits exceeding expenses as negative statutory expense values, and show exact serialized HMRC requests without a parallel harness vocabulary.

Phase 4C is complete as an implementation-ready proposal only. Phase 4D has not begun.

## 23. Tax Tag Validator Correction Reconnaissance

This section records the focused validator review performed on 30 August 2026 before corrective editing.

### 23.1 Authority and defect

The revised `specs/reference/sole-trader-field-sets.md` establishes `TagClassCode` as mapping eligibility: only Component (`1`) tags may have `Cash.tbTaxTagMap` rows; Rollup (`0`) and Derived (`2`) tags cannot be mapped. The Phase 4D rewrite of `Cash.fnTaxTagMapValidate` incorrectly embedded the `UK-ITSA-SE-CUM` manifest, exact tag count, named income tags, consolidated/detailed alternatives and thirteen-tag readiness. Those are bootstrap or source-specific acceptance concerns, not generic mapping integrity, and must be removed from the shared validator.

### 23.2 Original generic invariants

The pre-Phase-4D function spool shows four behaviours:

- it collected enabled map rows for the selected source;
- it warned about missing Category/CashCode references;
- it expanded Category mappings down `Cash.tbCategoryTotal` and failed when the same CashCode reached the same tag through more than one mapping route; and
- it warned about enabled, connected CashCodes not covered by any effective mapping for the selected source.

Phase 4D retained and strengthened reference/root resolution and same-tag overlap detection through `Cash.vwTaxTagCashCode`, but lost the unmapped-CashCode warning. It also added useful leaf-polarity evidence while incorrectly restricting polarity comparison to one named source. Its cross-tag exclusivity rule is valid for the approved Sole Trader cumulative alternatives, but that rule is source-specific and belongs in the Sole Trader acceptance fixture rather than the generic validator.

### 23.3 Proven Category Tree direction and relevance boundary

`Cash.tbCategoryTotal.ParentCode -> ChildCode` is the ancestry-to-descendant direction. A Category mapping therefore expands from its selected root through descendant totals to enabled nominal categories (`CategoryTypeCode = 0`) and then enabled `Cash.tbCode` rows. `Cash.vwTaxTagCashCode` preserves each mapping root and route, so it already represents effective indirect coverage; a CashCode covered by a mapped ancestor is not unmapped.

The original warning defined relevance negatively by excluding only categories with no parent. That means any enabled CashCode attached anywhere inside any Category Tree could be warned about, including branches irrelevant to a particular business-tax projection. The schema provides stronger positive evidence: `App.vwTaxBizCashCodes` traverses from `App.tbOptions.NetProfitCode` and identifies the CashCodes in the configured business profit-and-loss accounting universe. Disconnected owner/capital and other non-profit branches are intentionally capable of remaining outside that universe. The corrected generic warning should therefore consider enabled nominal leaves in `App.vwTaxBizCashCodes`, subtract CashCodes already present in the selected source's effective mapping, and describe them as uncovered business-tax CashCodes without naming MTD or HMRC.

### 23.4 Smallest authorised correction

The generic validator should:

- reject enabled mappings whose Tax Tag is not Component;
- validate enabled Category/CashCode roots and their effective enabled nominal contributors;
- compare mandatory `CashPolarityCode` with every actual effective leaf;
- retain same-tag duplicate-route detection;
- retain generic same-tag overlap/double-counting detection, while leaving cross-tag exclusivity to source-specific acceptance rules because separate Component fields may legitimately reuse accounting evidence under a separately approved statutory design;
- restore the corrected uncovered-business-tax-CashCode warning based on `App.vwTaxBizCashCodes`; and
- contain no source-code literal, statutory tag literal, manifest count or submission-readiness rule.

Exact Sole Trader manifest, MIN/STD mapping inventories and consolidated/detailed readiness remain in the Phase 4D database acceptance fixture. The fixture is the correct location for bootstrap-specific assertions, although runtime execution remains outstanding until a usable database connection is available.

## 24. Mandatory Tax Tag Cash Polarity Correction

The tag orientation field is accounting-domain metadata and is therefore named `Cash.tbTaxTag.CashPolarityCode`, consistently with `Cash.tbCategory.CashPolarityCode` and `Cash.tbPolarity`. It is mandatory and restricted to Income (`1`) or Expense (`0`). Neutral (`2`) remains invalid for a Tax Tag.

The mapping remains user-selectable and the actual contributor polarity remains derived from the effective Category/CashCode relationships. The mandatory tag value is the independent expected polarity against which those contributors are validated; it is not inferred from the selected mapping. `Cash.fnTaxTagMapValidate` must therefore compare every effective leaf with the tag's `CashPolarityCode` unconditionally and reject null, neutral, mixed or contrary contributor polarity.

No database migration is required because there are no deployed instances of this development schema. Older templates are not granted a nullable compatibility path. In particular, the current Corporation Tax template does not supply `CashPolarityCode` and is intentionally expected to fail until that template is updated in its later scheduled work. The Sole Trader cumulative seed supplies the field explicitly and remains the compliant bootstrap path for this wave.
