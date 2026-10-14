# Sole Trader MTD Income Tax Contract Reference

Trade Control Tax Hub Programme  
Objective 3 authoritative contract research  
Research cut-off: **1 September 2026**

## 1. Executive conclusion

Trade Control's supported Sole Trader route is the current **Making Tax Digital for Income Tax — Self Employment** route. The authoritative write-side is not SA100, SA103F or EOPS.

The current contract baseline is:

- Business Details API **v2.0** to discover and retain the HMRC `businessId`, choose quarterly type, confirm accounting type, and report periods of account where applicable;
- Obligations API **v3.0** to read business quarterly obligations and the taxpayer-level annual final-declaration obligation;
- Self Employment Business API **v5.0** to PUT/GET the cumulative self-employment update for tax years 2025–26 onward and PUT/GET/DELETE the annual allowances and adjustments submission;
- Business Source Adjustable Summary (BSAS) API **v7.0** to trigger, retrieve and submit category-level accounting corrections after the cumulative data is complete;
- Business Income Source Summary (BISS) API **v3.0** as a material read-side summary, particularly for loss review and non-aligned businesses;
- Individual Losses API **v6.0** for tax years through 2025–26 and **v7.0** for tax years 2026–27 onward;
- Individuals Tax Liability Adjustments API **v1.0**, from 2026–27, when a carry-back claim requires a separately calculated credit against the current liability;
- Individual Calculations API **v8.0** to trigger and retrieve calculations and submit the final declaration or a post-finalisation amendment;
- Self Assessment Accounts API **v4.0** to retrieve the authoritative account balance, charges, payments and allocations after filing.

The quarterly contract is cumulative, not period-only. For a normal mandated self-employment source, every successive PUT replaces the previously effective cumulative position for that tax year and business. The URI contains NINO, business ID and tax year. The body contains the cumulative date range plus income, expenses and optional disallowable-expense totals. It contains no obligation ID and no period key. [S2][S3]

The Objective 2 quarterly starting set is not sufficient for a complete detailed-expense v5 request. Its two income concepts map correctly, and its 13 expense concepts map semantically after property renaming, but the HMRC detailed request has **15** expense properties: `irrecoverableDebts` and `depreciation` are absent from the Objective 2 list. It also exposes `taxTakenOffTradingIncome`, `consolidatedExpenses`, and 15 category-specific disallowable-expense properties. Those additional properties must not be fabricated as Tax Tags; their ownership is classified in section 4.5.

There is no current EOPS filing stage. HMRC removed EOPS submission through software for all tax years. The annual process is instead: finish cumulative updates; submit annual allowances/adjustments; make any BSAS accounting corrections; handle losses and other taxpayer-level data; trigger an `intent-to-finalise` calculation; retrieve and display the calculation; obtain the declaration; then POST the calculation ID as `final-declaration`. [S4][S7][S14]

### 1.1 Contract status legend

| Mark | Meaning |
|---|---|
| **Verified** | Current official HMRC documentation or schema at the research cut-off |
| **Preview** | Official Sandbox/test-only or roadmap behaviour not yet a stable Production annual contract |
| **Inference** | Implementation conclusion drawn from more than one official source |
| **Unresolved** | The official material does not settle the point sufficiently for implementation |

## 2. Supported filing lifecycle

The supported lifecycle is shown below. Reads can be repeated at any point; write order matters.

| Seq. | Level | Action | Authoritative operation | Result/state |
|---:|---|---|---|---|
| 1 | Taxpayer | Confirm MTD status if the product needs it | Self Assessment Individual Details, Retrieve ITSA Status | Mandated, voluntary, annual, latent or exempt context; material but not a Self Employment DTO |
| 2 | Taxpayer/business | Discover income sources | Business Details v2, List All Businesses | HMRC `businessId` for each source; store it rather than repeatedly rediscovering it [S3][S5] |
| 3 | Business | Retrieve details and quarterly choice | Business Details v2, Retrieve Business Details | Type, trading details, commencement/cessation and `quarterlyTypeChoice` |
| 4 | Business | Optionally elect calendar quarters before the first update | Business Details v2, Create and Amend Quarterly Period Type | `quarterlyPeriodType: calendar` or `standard`; choice is blocked after the first update for the year [S3][S5] |
| 5 | Business/taxpayer | Read open quarterly and final obligations | Obligations v3 | Business-specific quarterly dates; taxpayer-level annual final-declaration dates [S6] |
| 6 | Business | PUT cumulative update(s) | Self Employment v5, Create or Amend Cumulative Period Summary | Current YTD accounting position; later PUT invalidates the earlier position [S2][S3] |
| 7 | Taxpayer | Optionally trigger/retrieve an in-year calculation | Individual Calculations v8, `in-year` | Asynchronous HMRC estimate and business-validation messages [S7] |
| 8 | Business | After year end, confirm accounting method and periods of account | Business Details v2 | Required business context before the annual return; LADR election where applicable [S4][S5] |
| 9 | Business | PUT annual allowances and non-BSAS adjustments | Self Employment v5, Annual Submission | Current annual statutory additions/deductions; a replacement PUT must repeat retained values [S2][S4] |
| 10 | Business | If accounting totals need correction, trigger BSAS then submit adjustments | BSAS v7 | HMRC `calculationId`; category deltas; updated business summary [S8] |
| 11 | Business/taxpayer | Review the business result and losses | BSAS/BISS/Individual Calculations | Adjusted profit/loss and available claim context [S4][S8][S12] |
| 12 | Taxpayer/business | Submit losses and claims | Individual Losses v6 or v7; Tax Liability Adjustments v1 where required | HMRC loss state and, from 2026–27, any separately calculated carry-back liability credit [S9][S10][S11] |
| 13 | Taxpayer | Complete all other income, relief and deduction information | Relevant taxpayer-level APIs or HMRC online services | Whole-taxpayer return inputs; outside this Self Employment contract model except as workflow prerequisites [S4] |
| 14 | Taxpayer | Trigger final calculation | Individual Calculations v8 POST with `intent-to-finalise` | `202` plus HMRC `calculationId`; final business validations run [S7] |
| 15 | Taxpayer | Retrieve and display that calculation | Individual Calculations v8 GET by `calculationId` | Calculation result, messages, business profit/loss and authoritative liability calculation [S7] |
| 16 | Taxpayer/agent | Obtain agreement to the prescribed declaration | Product UI | Customer or agent declaration recorded by workflow; declaration text is not a JSON body [S4] |
| 17 | Taxpayer | Submit the final declaration | Individual Calculations v8 POST with `final-declaration` | `204`; annual obligation becomes fulfilled, normally within an hour [S4][S7] |
| 18 | Taxpayer | Read account position | Self Assessment Accounts v4 | Authoritative balance, transactions, payments and allocations [S13] |
| 19 | Taxpayer | Within the amendment window, amend inputs and re-finalise | Write APIs; calculation `intent-to-amend`; declaration `confirm-amendment` | Revised return. HMRC states a 12-month statutory amendment window [S4][S7] |

## 3. API and version inventory

All listed APIs are user-restricted OAuth APIs. Read operations use `read:self-assessment`; writes and calculation triggers use `write:self-assessment`. All landing pages require the applicable fraud-prevention headers. Every call requires an `Accept` media type containing the API version; JSON writes also require `Content-Type: application/json`. `Gov-Test-Scenario` is Sandbox-only. [S1][S7][S16]

| API | Current version at cut-off | Environment and tax-year position | Required/material role | Migration state |
|---|---:|---|---|---|
| Business Details | 2.0 | Sandbox + Production | Discover businesses; quarterly type; accounting type; periods of account; LADR | v1 retired 1 May 2026 [S5][S15] |
| Obligations | 3.0 | Sandbox + Production | Quarterly and annual obligation reads | v2 retired 28 October 2025 [S6][S15] |
| Self Employment Business | 5.0 | Sandbox + Production; cumulative endpoint from 2025–26 | Cumulative income/expense PUT/GET; annual PUT/GET/DELETE | v4 retired 9 January 2026; v3 retired 28 October 2025 [S1][S2][S15] |
| BSAS | 7.0 | Sandbox + Production | Trigger business summary; submit/retrieve accounting corrections | v6 retired 9 January 2026; v5 retired 28 October 2025 [S8][S15] |
| BISS | 3.0 | Sandbox + Production | Read calculated business-income summary | v2 retired 28 October 2025 [S12][S15] |
| Individual Losses | 6.0 | Sandbox + Production; through 2025–26 | Legacy-current brought-forward loss and claim resources | v4/v5 retired 9 January 2026; v6 remains required for old tax years [S10][S15] |
| Individual Losses | 7.0 | Sandbox + Production; minimum 2026–27 | Combined per-business, per-year losses and claims resource | New Production version 16 June 2026 [S9][S15] |
| Tax Liability Adjustments | 1.0 | Sandbox + Production; minimum 2026–27 | Carry-back liability decrease amounts | New Production API 16 June 2026 [S11][S15] |
| Individual Calculations | 8.0 | Sandbox + Production | Trigger/list/retrieve calculation; final declaration/amendment | v7 retired 1 May 2026; v6 retired 9 January 2026 [S7][S15] |
| Self Assessment Accounts | 4.0 | Sandbox + Production | Authoritative liabilities, balances, transactions and payments | v3 retired 9 January 2026 [S13][S15] |

### 3.1 Tax-year version gates

1. Use Self Employment v5 cumulative endpoints only for 2025–26 onward. The old period-summary endpoints remain in the v5 documentation only for tax years 2024–25 and earlier; they must not be used for the supported current route. [S2]
2. Use Individual Losses v6 for tax years through 2025–26 and v7 for 2026–27 onward. These are different resource models, not media-type aliases. [S9][S10]
3. Individual Calculations v8 is the only current calculation version. Its request and response schemas are tax-year discriminated. For 2026–27 HMRC removed the old `inputs.lossesBroughtForward`, `inputs.claims`, `calculation.lossesAndClaims`, and several old Class 4 loss result properties because v7 Losses owns the new contract. [S7][S15]
4. The Self Employment v5 **2026–27 annual schema is marked test-only** at the cut-off. It removes `nonFinancials.businessDetailsChangedRecently` and adds `adjustments.adjustmentToProfitsForClass4`. Implement it only behind a preview/version gate until HMRC makes the annual schema Production-stable. [S2][S14][S15]
5. A December 2026 roadmap item proposes a new first-year allowance field. It is future behaviour and is not part of this reference contract. [S14]

## 4. Quarterly cumulative submission contract

### 4.1 Operation

| Item | Verified contract |
|---|---|
| Operation | Create or Amend a Self-Employment Cumulative Period Summary |
| Method/path | `PUT /individuals/business/self-employment/{nino}/{businessId}/cumulative/{taxYear}` |
| Accept | `application/vnd.hmrc.5.0+json` |
| OAuth | `write:self-assessment` |
| Path parameters | `nino`; `businessId` matching `^X[A-Z0-9]{1}IS[0-9]{11}$`; `taxYear` in `YYYY-YY`, minimum `2025-26` |
| Query parameters | None |
| Success | `204 No Content` |
| Retrieve | `GET` on the same path, `read:self-assessment`, returning `200` and the stored cumulative body |
| Sandbox | Available; stateful scenarios include standard, calendar, annual and latent status behaviour |
| Source | [S2] |

There is no client-generated submission ID, obligation ID or period key. The business is identified by the NINO/business-ID pair and the tax year by the path. Dates are body context. [S2]

### 4.2 Request shape

```json
{
  "periodDates": {
    "periodStartDate": "2025-04-06",
    "periodEndDate": "2025-07-05"
  },
  "periodIncome": {
    "turnover": 0,
    "other": 0,
    "taxTakenOffTradingIncome": 0
  },
  "periodExpenses": {
    "costOfGoods": 0,
    "paymentsToSubcontractors": 0,
    "wagesAndStaffCosts": 0,
    "carVanTravelExpenses": 0,
    "premisesRunningCosts": 0,
    "maintenanceCosts": 0,
    "adminCosts": 0,
    "businessEntertainmentCosts": 0,
    "advertisingCosts": 0,
    "interestOnBankOtherLoans": 0,
    "financeCharges": 0,
    "irrecoverableDebts": 0,
    "professionalFees": 0,
    "depreciation": 0,
    "otherExpenses": 0
  },
  "periodDisallowableExpenses": {
    "costOfGoodsDisallowable": 0,
    "paymentsToSubcontractorsDisallowable": 0,
    "wagesAndStaffCostsDisallowable": 0,
    "carVanTravelExpensesDisallowable": 0,
    "premisesRunningCostsDisallowable": 0,
    "maintenanceCostsDisallowable": 0,
    "adminCostsDisallowable": 0,
    "businessEntertainmentCostsDisallowable": 0,
    "advertisingCostsDisallowable": 0,
    "interestOnBankOtherLoansDisallowable": 0,
    "financeChargesDisallowable": 0,
    "irrecoverableDebtsDisallowable": 0,
    "professionalFeesDisallowable": 0,
    "depreciationDisallowable": 0,
    "otherExpensesDisallowable": 0
  }
}
```

`periodExpenses` may instead be `{ "consolidatedExpenses": amount }`. It must not combine `consolidatedExpenses` with any detailed expense property. [S2][S3]

### 4.3 Dates, cumulative semantics and replacement

- Values cover the cumulative interval beginning at the applicable tax-year/commencement boundary and ending at `periodEndDate`; they are not the isolated quarter movement. [S2][S3]
- Standard updates normally progress 6 April–5 July, 6 April–5 October, 6 April–5 January and 6 April–5 April. Calendar-quarter choices and the first year of that choice have special boundaries supplied by the Obligations API. Do not hard-code dates; read obligations. [S3][S6]
- For an `annual` ITSA status or latent income source, HMRC says `periodStartDate` and `periodEndDate` are not required. Normal mandated/voluntary quarterly sources require aligned dates. [S2]
- An update ending on or after an obligation end date fulfils that obligation. A later cumulative update can fulfil more than one open obligation, although a late first update can still earn a penalty point. [S3]
- Each PUT represents the complete current cumulative position. HMRC states that each later update invalidates the prior submission. To correct earlier data, include the corrected YTD values in the next PUT or PUT a replacement current cumulative body. [S3]
- No idempotency-key header is defined. Resource idempotency follows from PUT to the same NINO/business/tax-year resource. A transport retry must repeat the identical body.

### 4.4 Value rules

| Rule | Contract |
|---|---|
| Income range | `turnover`, `other`, `taxTakenOffTradingIncome`: 0 to 99,999,999,999.99 |
| Expense range | Every detailed, consolidated and disallowable expense: -99,999,999,999.99 to 99,999,999,999.99 |
| Precision | Maximum 2 decimal places. HMRC defines validation, not a rounding algorithm; Trade Control must round under its approved accounting policy before serialization. |
| Zero/omission | HMRC's endpoint note requires income and expense values even when zero; for no income it expressly requires `turnover: 0` and `other: 0`. Object leaves appear optional in the OAS, so the adapter must not treat arbitrary omission as equivalent to zero. See open question OQ-1. |
| Signs | HMRC accepts positive and negative detailed/disallowable expenses. Do not impose a universal “expense must be negative” DTO rule. Translate from Trade Control's accounting sign convention at the adapter boundary. |
| Consolidated eligibility | A customer with annual turnover below **£90,000** may use `consolidatedExpenses`. Above the threshold HMRC can return the consolidated-expenses-threshold error. |
| Trading allowance | If Trading Income Allowance will be claimed, all expenses must be removed from the final cumulative update before the annual allowance claim. |
| Detailed vs consolidated | `consolidatedExpenses` is the sum of allowable expenses. It is mutually exclusive with detailed `periodExpenses` leaves. |
| Material errors | `RULE_BOTH_EXPENSES_SUPPLIED`, `RULE_TAX_YEAR_NOT_SUPPORTED`, `RULE_OUTSIDE_AMENDMENT_WINDOW`, date-alignment/missing-date rules, early-submission rules, `FORMAT_*`, `CLIENT_OR_AGENT_NOT_AUTHORISED`, `NOT_FOUND`. |

Sources: [S2][S3].

### 4.5 Objective 2 source classification for every quarterly property

The classification below is ownership, not a direction to create Tax Tags.

| HMRC property | Objective 2 source / status | Adapter rule |
|---|---|---|
| `periodDates.periodStartDate`, `periodEndDate` | Tax Hub workflow/context, checked against HMRC obligations | Never a Tax Tag |
| `periodIncome.turnover` | Objective 2 Tax Tag `turnover` | Direct semantic match |
| `periodIncome.other` | Objective 2 Tax Tag `otherBusinessIncome` | Rename at adapter boundary |
| `periodIncome.taxTakenOffTradingIncome` | External/user-supplied; currently absent from Objective 2 | Do not infer from sales or CIS; omit only where lawful, otherwise workflow input |
| `periodExpenses.costOfGoods` | Objective 2 `costOfGoods` | Direct |
| `paymentsToSubcontractors` | Objective 2 `cisPaymentsToSubcontractors` | Rename; this is expense, not CIS tax deducted |
| `wagesAndStaffCosts` | Objective 2 `wagesSalariesStaffCosts` | Rename |
| `carVanTravelExpenses` | Objective 2 `carVanTravelExpenses` | Direct |
| `premisesRunningCosts` | Objective 2 `rentRatesPowerInsurance` | Rename |
| `maintenanceCosts` | Objective 2 `repairsMaintenance` | Rename |
| `adminCosts` | Objective 2 `phoneFaxStationeryOfficeCosts` | Rename |
| `businessEntertainmentCosts` | Objective 2 `businessEntertainment` | Rename |
| `advertisingCosts` | Objective 2 `advertising` | Rename |
| `interestOnBankOtherLoans` | Objective 2 `interestOnBankOtherLoans` | Direct |
| `financeCharges` | Objective 2 `bankCreditCardFinancialCharges` | Rename |
| `professionalFees` | Objective 2 `accountancyLegalProfessionalFees` | Rename |
| `otherExpenses` | Objective 2 `otherBusinessExpenses` | Rename |
| `irrecoverableDebts` | Missing Objective 2 candidate | Business accounting concept; add only after deterministic MIN/STD source analysis |
| `depreciation` | Missing Objective 2 candidate | Business accounting concept; never substitute it for capital allowances |
| `consolidatedExpenses` | Derived Rollup plus workflow choice | Calculate from eligible allowable expense values; not a separate Component mapping |
| `periodDisallowableExpenses.<category>Disallowable` (15 properties corresponding to all detailed expenses) | Optional, currently unsupported Objective 2 candidate family | If Trade Control can deterministically identify the disallowable portion per category, these are Objective 2 statutory projection candidates. Otherwise omit where HMRC permits; never estimate or allocate mechanically. |
| NINO | External/taxpayer configuration | Sensitive identifier; not a Tax Tag |
| `businessId` | HMRC-supplied identifier persisted against the Business Node | Not a Tax Tag |
| `taxYear` | Tax Hub workflow/context | Not a Tax Tag |

The wire property names, not the Objective 2 names, are canonical for DTO serialization.

## 5. Annual Self Employment contract

### 5.1 Annual submission operation

| Item | Verified contract |
|---|---|
| Create/amend | `PUT /individuals/business/self-employment/{nino}/{businessId}/annual/{taxYear}` |
| Retrieve | `GET` same path |
| Delete | `DELETE` same path |
| Version/scopes | v5; PUT/DELETE `write:self-assessment`, GET `read:self-assessment` |
| Success | PUT/DELETE `204`; GET `200` with the annual shape |
| Body groups | Optional `adjustments`, `allowances`, `nonFinancials` |
| Replacement | An amendment must repeat all retained previously submitted figures; the new submission removes omitted previous entries [S4] |
| Source | [S2][S4] |

### 5.2 Annual adjustments

Unless stated otherwise, numeric fields allow 0 to 99,999,999,999.99 with at most 2 decimals.

| HMRC property | TY position | Meaning/sign | Trade Control ownership decision |
|---|---|---|---|
| `includedNonTaxableProfits` | Current | Non-taxable amounts included in business income/expenses | Objective 2 candidate only if deterministically projected; otherwise reviewed workflow input |
| `basisAdjustment` | Current | Basis-period adjustment; may be positive or negative | Derived/external Tax Hub value. For non-aligned businesses it depends on profit calculated outside MTD, not a category mapping [S4] |
| `overlapReliefUsed` | Deprecated from 2024–25; absent from 2026–27 preview | Nonnegative historic overlap relief | Do not add to the new Objective 2 canonical set |
| `accountingAdjustment` | Current | Change-of-accounting-practice adjustment, nonnegative | External/reviewed statutory input; not ordinary ledger depreciation |
| `averagingAdjustment` | Deprecated; HMRC says it will be reinstated later | Positive or negative specialist adjustment | Unsupported until HMRC restores a stable contract |
| `outstandingBusinessIncome` | Current | Other annual business income not included elsewhere | Objective 2 candidate only where Trade Control has a deterministic source; otherwise workflow input |
| `balancingChargeBpra` | Current | BPRA balancing charge | Capital-allowance workflow/external; no accounting-depreciation substitution |
| `balancingChargeOther` | Current | Other balancing charge on disposal/cessation | Capital-allowance workflow/external |
| `goodsAndServicesOwnUse` | Current | Normal sale value of goods/stock taken from business | Objective 2 Component candidate if owner-use transactions are deterministic; otherwise workflow input |
| `transitionProfitAmount` | 2024–25 onward | Transition profit arising for this source | Basis-period workflow/external |
| `transitionProfitAccelerationAmount` | 2024–25 onward; conditional on transition profit | Additional elected transition profit | User election/workflow; `RULE_WRONG_TPA_AMOUNT_SUBMITTED` if supplied without the base amount |
| `adjustmentToProfitsForClass4` | **Preview**, 2026–27 test-only annual schema | Total Class 4 profit adjustments | Do not freeze into Objective 2 until Production-stable; likely Derived/workflow value |

There is no current generic `privateUseAdjustment` annual property. Private-use or disallowable amounts belong in the category-specific periodic disallowable/BSAS additions model where applicable. `goodsAndServicesOwnUse` is narrower and must not be used as a generic private-use bucket. [S2][S8]

### 5.3 Annual allowances

All scalar amounts are nonnegative, maximum 99,999,999,999.99 and 2 decimals, except `tradingIncomeAllowance` which is capped at 1,000.

| Property | TY 2025–26 Production contract | Objective ownership |
|---|---|---|
| `annualInvestmentAllowance` | Current | Capital-allowance calculation/workflow; potential Derived Tax Tag, never mapped from depreciation |
| `capitalAllowanceMainPool` | Current | Same |
| `capitalAllowanceSpecialRatePool` | Current | Same |
| `businessPremisesRenovationAllowance` | Current | Same; specialist/possibly unsupported |
| `enhancedCapitalAllowance` | Current | Same; HMRC guidance says use this for eligible 2025–26 electric charge-point allowance reporting |
| `allowanceOnSales` | Current | Capital-allowance disposal workflow |
| `capitalAllowanceSingleAssetPool` | Current | Capital-allowance workflow |
| `zeroEmissionsCarAllowance` | Current | Capital-allowance workflow |
| `tradingIncomeAllowance` | Current; mutually exclusive with every other allowance | User election/workflow; also requires removal of all expenses from final cumulative update |
| `structuredBuildingAllowance[]` | Current | Workflow/external asset data |
| `enhancedStructuredBuildingAllowance[]` | Current | Workflow/external asset data |

Each structured-building item contains required `amount`; optional `firstYear` with required `qualifyingDate` and `qualifyingAmountExpenditure`; and required `building`, where `postcode` is required and at least one of `name` or `number` must be supplied. These address/date properties are Objective 3/workflow context, not Tax Tags. [S2]

Fields present in older schemas but removed by 2025–26 (`zeroEmissionsGoodsVehicleAllowance`, `electricChargePointAllowance`) must not be put in the current 2025–26 DTO variant. The December 2026 first-year allowance roadmap proposal is not current. [S2][S14]

### 5.4 Annual non-financials

| Property | Position | Ownership |
|---|---|---|
| `businessDetailsChangedRecently` | Deprecated for 2025–26 and removed from the 2026–27 preview. HMRC says it should no longer be sent. | Retired; no Tax Tag or new DTO write property |
| `class4NicsExemptionReason` | Optional; enum `non-resident`, `trustee`, `diver`, `ITTOIA-2005`, `over-state-pension-age`, `under-16` | Taxpayer/user/HMRC context; not business accounting |

### 5.5 BSAS accounting adjustments are a separate contract

The annual submission above is not the category-correction mechanism. For corrected income/expense totals the current workflow is:

1. `POST /individuals/self-assessment/adjustable-summary/{nino}/trigger` with `accountingPeriod { startDate, endDate }`, `typeOfBusiness: self-employment`, and `businessId`.
2. Receive `{ "calculationId": "..." }`.
3. Optionally GET the self-employment BSAS.
4. `POST /individuals/self-assessment/adjustable-summary/{nino}/self-employment/{calculationId}/adjust/{taxYear}` with non-zero deltas.

For 2025–26 onward the trigger accounting period starts 6 April and ends 5 April. The adjustment body has optional:

- `income`: `turnover`, `other`;
- `expenses`: the 15 detailed expense properties plus `consolidatedExpenses`;
- `additions`: the 15 matching `...Disallowable` properties;
- `zeroAdjustments: true` to state that all income, expense and addition adjustments are zero.

Every numeric adjustment is -99,999,999,999.99 to 99,999,999,999.99, maximum 2 decimals, and **zero is not accepted** as an individual adjustment leaf. A submitted delta is relative to the original cumulative total. A later adjustment set replaces the prior adjustment position. If a periodic or annual update is written after BSAS, the BSAS adjustment becomes invalid and must be triggered and submitted again. [S4][S8]

This family is workflow-derived. It must not duplicate the quarterly Tax Tags as a second set of category mappings. The adapter should calculate deltas between the approved corrected statutory projection and the HMRC baseline.

### 5.6 Loss contracts

Loss creation/use is not an annual Self Employment payload group and is not EOPS.

#### Tax years through 2025–26 — Individual Losses v6

- Create brought-forward loss: `POST /individuals/losses/{nino}/brought-forward-losses/tax-year/brought-forward-from/{taxYear}` with required `taxYearBroughtForwardFrom`, `typeOfLoss`, `businessId`, `lossAmount`; returns `201` and an HMRC `lossId`.
- Create claim: `POST /individuals/losses/{nino}/loss-claims` with `taxYearClaimedFor`, `typeOfLoss`, `typeOfClaim`, `businessId`; returns `201` and HMRC claim identity.
- Dedicated list/retrieve/amend/delete resources use `lossId` or `claimId`.
- Self-employment claim types are `carry-forward` and `carry-sideways` in v6. [S10]

#### Tax years 2026–27 onward — Individual Losses v7

The per-business per-tax-year resource is:

`PUT /individuals/losses/{nino}/businesses/{businessId}/loss-claims/{taxYear}`

with optional groups:

| Group/property | Meaning |
|---|---|
| `claims.carryBack.previousYearGeneralIncome` | Section 64 carry-back to prior-year general income |
| `claims.carryBack.earlyYearLosses` | Section 72 early-trade loss relief |
| `claims.carryBack.terminalLosses` | Section 89 terminal loss relief |
| `claims.carrySideways.currentYearGeneralIncome` | Section 64 current-year general-income relief |
| `claims.preferenceOrder.applyFirst` | `carry-sideways` or `carry-back` |
| `claims.carryForward.currentYearLosses` | Current-year loss carried forward under Section 83 |
| `claims.carryForward.previousYearsLosses` | Prior-year unused losses carried forward |
| `losses.broughtForwardLosses` | Brought-forward loss applied against current-year profits |

All amounts are 0 to 99,999,999,999.99, maximum 2 decimals. PUT/DELETE return `204`; GET returns the same logical resource. The operation is end-of-year only in Production. [S9]

For carry-back from 2026–27, the separately calculated credit is PUT through Tax Liability Adjustments v1:

```json
{
  "carryBackLossesDecrease": {
    "incomeTax": 0,
    "class4": 0,
    "capitalGainsTax": 0
  }
}
```

The matching v7 loss claim must be submitted before final declaration. These liability amounts are external/professional tax calculations, not Trade Control accounting Tax Tags. [S11]

## 6. Calculation and final-declaration workflow

### 6.1 In-year and final calculation

`POST /individuals/calculations/{nino}/self-assessment/{taxYear}/trigger/{calculationType}`

| `calculationType` | Use |
|---|---|
| `in-year` | Non-final estimate after periodic/annual data changes |
| `intent-to-finalise` | Starts the annual return process and turns business-validation warnings into blocking errors |
| `intent-to-amend` | From 2025–26, calculate a post-finalisation amendment |

Success is `202` with `{ "calculationId": "uuid" }`. Calculation is asynchronous; HMRC recommends waiting at least 5 seconds before retrieval. Relevant errors include no income submissions, final declaration already received, changed income sources/residency, recent submissions, calculation in progress, tax year not ended, and business-validation failure. [S7]

### 6.2 Retrieve calculation

`GET /individuals/calculations/{nino}/self-assessment/{taxYear}/{calculationId}` returns `200` with the tax-year-specific v8 envelope:

| Group | Relevant contract |
|---|---|
| `metadata` | `calculationId`, `taxYear`, requester/timestamps/reason/type, final-declaration flags/timestamp, period bounds |
| `inputs` | Personal/income-source context and HMRC-recorded annual adjustment/loss/claim references for tax-year variants where present |
| `calculation.businessProfitAndLoss[]` | Business source identity and HMRC totals such as income, expenses, net profit/loss, additions/deductions, accounting adjustments, taxable profit and loss-use outcomes |
| `calculation.taxDeductedAtSource` | Includes `taxTakenOffTradingIncome` and other taxpayer-level deductions where present |
| `calculation.taxCalculation` | Income Tax, NIC, CGT and aggregate due outputs. The minimum authoritative aggregate is `totalIncomeTaxAndNicsAndCgt`; also retain `totalIncomeTaxAndNicsDue`, total deducted and NIC detail when present. |
| `messages` | `info`, `warnings`, `errors`; finalisation must stop on errors |

The retrieve response is large and tax-year discriminated. Objective 3 must model the v8 OAS variant selected by tax year and may expose a focused read model for Trade Control. It must not copy fields from v6/v7 examples into the 2026–27 DTO. [S7][S15]

### 6.3 Declaration submission

After displaying the calculation that corresponds to the exact `calculationId` and obtaining agreement to HMRC's prescribed customer or agent declaration:

`POST /individuals/calculations/{nino}/self-assessment/{taxYear}/{calculationId}/final-declaration`

There is no request body. Success is `204`. The ID must match the calculation the customer approved. If data changes, trigger and retrieve a new calculation and obtain confirmation against the new result. [S4][S7]

For an amendment, use a new `intent-to-amend` calculation and then:

`POST .../{calculationId}/confirm-amendment`

The final-declaration POST is not treated as a freely retryable resource PUT. Handle duplicate/in-progress/received errors explicitly and confirm state through calculation/obligation reads.

### 6.4 EOPS and crystallisation terminology

- **EOPS does not exist as a current software filing stage.** Do not create an EOPS contract family. [S14]
- The Obligations v3 final-declaration read retains the legacy wire path `/obligations/details/{nino}/crystallisation`. This path name does not restore crystallisation or EOPS as a product stage; the operation is officially named “Retrieve ... Final Declaration Obligations”. [S6]
- “Crystallisation” is not a separate current trigger. The current trigger is `intent-to-finalise`. [S4][S7]

## 7. Identifier and context model

| Value | Issuer/owner | Scope and format | Persistence rule |
|---|---|---|---|
| NINO | HMRC/taxpayer | Taxpayer; `AA999999A` format | Secure taxpayer configuration; never serialize as body data unless schema says so |
| `businessId` | HMRC | One income source for one taxpayer; `^X[A-Z0-9]{1}IS[0-9]{11}$` | Persist against the Business Node; obtained from Business Details |
| `taxYear` | Tax Hub workflow | `YYYY-YY`; path or query depending operation | Explicit value object; never infer API version from it |
| Period dates | HMRC obligations plus business commencement/quarter choice | ISO `YYYY-MM-DD` | Context; validate against current obligations |
| Quarterly obligation | HMRC | Business-specific nested obligation detail; no period key in v3 | Read state; not supplied to cumulative PUT |
| Final obligation | HMRC | Taxpayer-level annual period/due/status | Read state; current wire path retains `crystallisation` |
| Self Employment cumulative resource | Client/HMRC | NINO + business ID + tax year | PUT replacement; no submission ID |
| Annual resource | Client/HMRC | NINO + business ID + tax year | PUT replacement; no submission ID |
| BSAS `calculationId` | HMRC | One generated business adjustable summary | Use only for matching business/type; regenerate after invalidating writes |
| Tax calculation `calculationId` | HMRC | One asynchronous whole-taxpayer calculation | Bind displayed calculation and declaration to the same ID |
| v6 `lossId` / `claimId` | HMRC | Individual loss or claim resource | Retain only for v6 tax-year route |
| v7 loss resource | HMRC/client | NINO + business ID + tax year | PUT replacement; no client claim ID in path |
| Account document/transaction/charge references | HMRC | Account read-side identities | Do not reuse as filing identifiers |

Repository namespace `TradeControl.Tax.UK.Hmrc.Sa.v1_0` is a Trade Control model namespace. It is **not** an HMRC media-type version. Each endpoint family must carry its actual HMRC version and `Accept` value.

## 8. Request/response contract inventory

| Family/operation | Method and path | Request | Success response |
|---|---|---|---|
| Business list | `GET /individuals/business/details/{nino}/list` | No body | `listOfBusinesses[] { typeOfBusiness, businessId, tradingType?, tradingName? }` |
| Business detail | `GET /individuals/business/details/{nino}/{businessId}` | No body | Business identity, dates, address, `quarterlyTypeChoice`, optional accounting periods |
| Quarterly type | `PUT /individuals/business/details/{nino}/{businessId}/{taxYear}` | `{ quarterlyPeriodType: "calendar" | "standard" }` | `204` |
| Accounting type | `GET/PUT .../{taxYear}/accounting-type` | PUT `{ accountingType: "CASH" | "ACCRUAL" }` | GET body / PUT `204` |
| Periods of account | `GET/PUT .../{taxYear}/periods-of-account` | `{ periodsOfAccount: boolean, periodsOfAccountDates?: [{startDate,endDate}] }` | GET includes `submittedOn`; PUT `204` |
| LADR | `GET/POST/DELETE .../{taxYear}/late-accounting-date-rule-election...` | No ordinary accounting body | Read or `204` |
| Quarterly obligations | `GET /obligations/details/{nino}/income-and-expenditure` | Queries `typeOfBusiness?`, `businessId?`, paired `fromDate?`/`toDate?`, `status?` | `{ obligations: [{typeOfBusiness,businessId,obligationDetails:[{periodStartDate,periodEndDate,dueDate,receivedDate?,status}]}] }` |
| Final obligation | `GET /obligations/details/{nino}/crystallisation` | `taxYear?`, `status?` | `{ obligations: [{periodStartDate,periodEndDate,dueDate,receivedDate?,status}] }` |
| Cumulative update | `PUT/GET .../self-employment/{nino}/{businessId}/cumulative/{taxYear}` | Section 4 body | PUT `204`; GET same logical body |
| Annual | `PUT/GET/DELETE .../self-employment/{nino}/{businessId}/annual/{taxYear}` | Section 5 body | PUT/DELETE `204`; GET annual body |
| BSAS trigger | `POST /individuals/self-assessment/adjustable-summary/{nino}/trigger` | `{ accountingPeriod:{startDate,endDate}, typeOfBusiness, businessId }` | `{ calculationId }` |
| BSAS retrieve | `GET .../adjustable-summary/{nino}/self-employment/{calculationId}/{taxYear}` | No body | Adjustable and adjusted summary calculation |
| BSAS adjust | `POST .../self-employment/{calculationId}/adjust/{taxYear}` | Income/expense/addition deltas or `zeroAdjustments:true` | `200` adjustment identity/result per v7 schema |
| BISS retrieve | `GET /individuals/self-assessment/income-summary/{nino}/{typeOfBusiness}/{taxYear}/{businessId}` | No body | Business source summary including profit/loss/adjusted values |
| Loss v7 | `GET/PUT/DELETE /individuals/losses/{nino}/businesses/{businessId}/loss-claims/{taxYear}` | Section 5.6 | GET resource; PUT/DELETE `204` |
| Liability adjustment v1 | `GET/PUT/DELETE /individuals/tax-liability/adjustments/{nino}/{taxYear}` | `carryBackLossesDecrease` | GET resource; PUT/DELETE `204` |
| Calculation trigger | `POST /individuals/calculations/{nino}/self-assessment/{taxYear}/trigger/{calculationType}` | No body | `202 { calculationId }` |
| Calculation list | `GET /individuals/calculations/{nino}/self-assessment/{taxYear}` | No body | `calculations[]` metadata/status summaries |
| Calculation retrieve | `GET .../{taxYear}/{calculationId}` | No body | v8 tax-year-specific calculation envelope |
| Declaration | `POST .../{taxYear}/{calculationId}/{final-declaration|confirm-amendment}` | No body | `204` |
| Account balance/transactions | `GET /accounts/self-assessment/{nino}/balance-and-transactions` | Query filters; no body | `balanceDetails`, `codingDetails[]`, `documentDetails[]`, `financialDetails[]` |
| Payments/allocations | `GET /accounts/self-assessment/{nino}/payments-and-allocations` | Date or payment-lot filters | `payments[]` with transaction and `allocations[]` |

Sources: [S2][S5][S6][S7][S8][S9][S11][S12][S13].

## 9. Existing `HMRC_MTD` Sole Trader class assessment

Assessed scope: `src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA` and the shared JSON extractor it uses. No files were modified.

| Existing area | Classification | Evidence and required disposition |
|---|---|---|
| `Obligations/SaObligationsEndpoint` | Obsolete; requires replacement and authoritative re-versioning | Path `/self-assessment/obligations` does not exist in current v3. Current paths are `/obligations/details/{nino}/income-and-expenditure` and `/.../crystallisation`; version is 3.0. |
| `SaObligationsRequest` | Structurally incorrect | Uses UTR, serializes `From`/`To` as a GET JSON body and does not model NINO, business filters or `status`. Current filters are query parameters. Default `System.Text.Json` property casing also does not match HMRC query names. |
| `SaObligation` / `SaObligationsResponse` | Structurally incorrect | Assumes flat `obligations[]` with `periodKey`, `start`, `end`, `due`, `received`. Current quarterly response nests `obligationDetails[]` beneath business/type and uses `periodStartDate`, `periodEndDate`, `dueDate`, `receivedDate`; no `periodKey`. Final obligations are a separate response shape. |
| `Liabilities/SaLiabilitiesEndpoint` | Obsolete; requires replacement | Path `/self-assessment/liabilities` and v1.0 are not current. Liability reads are Self Assessment Accounts v4 `balance-and-transactions`. |
| `SaLiabilitiesRequest` | Structurally incorrect | Uses UTR and GET body. Current identity is NINO and current filters are `docNumber`, dates, open-item/lock/interest/payment flags. |
| `SaBalanceDetails` | Current concept, substantially incorrect shape | Current v4 has `payableAmount`, pending/overdue dates and amounts, BCD balances, coded-out and credit fields. The local four-field model loses material account state and defaults missing money to zero. |
| `SaChargeDetail` / `SaLiabilitiesResponse` | Obsolete shape | Current response separates `documentDetails[]` and `financialDetails[]`, with nested charge and item data. Local `chargeDetails[]` is not the current wire contract. |
| `Payments/SaPaymentsEndpoint` | Obsolete; requires replacement | Path `/self-assessment/payments` and v1.0 are not current. Current v4 path is `/accounts/self-assessment/{nino}/payments-and-allocations`. |
| `SaPaymentsRequest` | Structurally incorrect | Uses UTR and a GET JSON body. Current query uses dates and/or `paymentLot` plus `paymentLotItem`. |
| `SaPayment` / `SaPaymentsResponse` | Structurally incorrect | Current properties are `paymentLot`, `paymentLotItem`, `paymentReference`, `paymentAmount`, `paymentMethod`, `transactionDate`, and rich `allocations[]`. Local `amount`, `received`, `method`, `allocatedTo[]` does not match. |
| `Hmrc/Shared/JsonExtract` | Unrelated read-side utility; unsafe for authoritative DTO validation | It silently converts missing/invalid required decimals and dates to zero/default and uses culture-sensitive `DateOnly.TryParse`. Contract DTO deserialization should preserve absence, use ISO date parsing, and surface required-field errors. |
| Whole `Submissions/MTDITSA` folder naming | Requires replacement | It combines three read-side account/obligation concepts beneath a historical “Submissions” folder and has no current Self Employment, Business Details, BSAS, Losses, Calculations or Finalisation families. |

No surviving Sole Trader class is “current and substantially correct”. The concepts of obligations, balance and payments remain relevant, but their endpoint metadata, identity model and wire shapes require replacement.

## 10. Objective 2 reconciliation

### 10.1 Correctly represented concepts

The following Objective 2 concepts are semantically valid and should remain: `turnover`, `otherBusinessIncome`, and the 13 existing expense concepts listed in section 4.5. Their names are projection names, not wire names.

### 10.2 Required clarification or correction before Objective 3 implementation

1. Change the statement that the “core statutory quarterly field set” is 2 income + 13 expenses. The current detailed v5 wire contract contains 2 ordinary income totals plus optional tax deducted and **15 detailed expense totals**. The existing 13 expense concepts omit irrecoverable debts and depreciation.
2. State explicitly that Objective 2 expense names map to the different HMRC machine names shown in section 4.5.
3. Decide, through the approved MIN/STD mapping reconnaissance, whether `irrecoverableDebts` and `depreciation` can be deterministic Component Tax Tags. Depreciation remains an accounting expense and must never feed an annual capital-allowance field directly.
4. Decide whether category-specific disallowable amounts can be projected deterministically. They are legitimate Objective 2 candidates only if Trade Control holds the tax-allowability split; otherwise the optional HMRC properties remain absent/unsupported.
5. Record `consolidatedExpenses` as a Rollup plus workflow election, not a mappable Component.
6. Keep `taxTakenOffTradingIncome` outside the Business Node accounting projection unless an authoritative Trade Control source is identified. It is not CIS subcontractor expense.
7. Add an annual-candidate review using sections 5.2 and 5.3. Do not freeze every annual DTO scalar as a Tax Tag. Business-derived concepts (`includedNonTaxableProfits`, `outstandingBusinessIncome`, `goodsAndServicesOwnUse`) may be candidates; basis/elections/capital allowances generally require Derived or external workflow treatment.
8. Keep NINO, business ID, tax year, period dates, calculation IDs, declaration type, accounting type, periods of account, exemption reason and structured-building identity/address data in Objective 3/workflow context.

### 10.3 Concepts that remain retired

`SA100`, `SA103F`, `UK-SA-SE-RETURN`, `EOPS`, `UK-ITSA-SE-EOPS`, period-key-based quarterly submission, and a separate crystallisation submission must remain retired.

### 10.4 SQL assumptions contradicted by the current contract

Without proposing SQL changes, the current contract contradicts any projection assumption that:

- a quarterly write is period-only;
- a current write is identified by `periodKey` or obligation ID;
- 13 expense categories are sufficient for a fully detailed current request;
- all absent values should be serialized as zero;
- expenses must always have one sign;
- annual adjustments, accounting corrections and losses belong in one EOPS-shaped object;
- UTR is the identity used by these APIs;
- a repository namespace `v1_0` selects HMRC API v1.0.

## 11. Proposed Objective 3 C# contract families

Do not write SA100/SA103F/EOPS compatibility classes. Under `TradeControl.Tax.UK.Hmrc.Sa.v1_0`, use the following logical families; folder/API version suffixes are illustrative but the HMRC version metadata is mandatory.

| Family | Contracts |
|---|---|
| `Shared` | `Nino`, `BusinessId`, `TaxYear`, ISO dates, HMRC decimal validation, error envelope, common authorization/version/fraud-header metadata |
| `BusinessDetails.V2` | Business list/detail; quarterly-type PUT; accounting-type GET/PUT; periods-of-account; LADR operations |
| `Obligations.V3` | Separate income/expenditure and final-declaration request/response models; nested business obligations |
| `SelfEmployment.V5.Cumulative` | Tax-year route, date/income/expense/disallowable groups, detailed-vs-consolidated union, PUT/GET endpoint metadata |
| `SelfEmployment.V5.Annual` | Explicit TY2025–26 Production DTO and separately gated TY2026–27 preview DTO; annual GET/PUT/DELETE |
| `BusinessAdjustments.V7` | BSAS trigger ID, adjustable-summary response, self-employment delta submission |
| `BusinessIncomeSummary.V3` | BISS read response needed for non-aligned/loss review |
| `Losses.V6` | Brought-forward loss and claim resources for tax years through 2025–26 |
| `Losses.V7` | Per-business per-tax-year loss/claim resource from 2026–27 |
| `TaxLiabilityAdjustments.V1` | Carry-back liability decrease resource from 2026–27 |
| `Calculations.V8` | Trigger/list/retrieve; tax-year-specific response variants; messages and focused Trade Control liability read model |
| `Finalisation.V8` | Declaration endpoint requests composed only from path/context; prescribed declaration workflow state; no fabricated body DTO |
| `Accounts.V4` | Balance/transactions, payment/allocation and charge history read contracts |

Implementation rules:

- endpoint descriptors must declare exact method, path template, media-type version, scope and success codes;
- GET filters are query models, never JSON bodies;
- required JSON fields must not silently default; optional money must remain nullable/absent;
- use explicit `JsonPropertyName` or a verified global camel-case policy;
- parse HMRC dates/timestamps as ISO formats, not current culture;
- model mutually exclusive detailed/consolidated expenses so both cannot be serialized;
- select DTO schema by tax year and endpoint version, not by repository namespace;
- preserve unknown response fields where forward compatibility or audit requires it, while exposing a stable internal read model;
- log HMRC identifiers and response status without logging NINO or full payloads indiscriminately.

## 12. Open questions and unsupported areas

| ID | Status | Question/decision |
|---|---|---|
| OQ-1 | **Unresolved official inconsistency** | The cumulative endpoint narrative requires income and expenses even when zero, while OAS leaves are not all marked `required`. Before production serialization tests are frozen, confirm through HMRC stateful Sandbox whether every detailed expense leaf must be present or whether a present object with selected leaves/zeros is accepted. Until then, do not equate omission with zero. |
| OQ-2 | Objective 2 decision | Can MIN and STD deterministically source `irrecoverableDebts`, `depreciation`, and any category-specific disallowable amount without artificial allocation? The contract is known; Trade Control support is not. |
| OQ-3 | Product decision | Will Trade Control support consolidated-expense mode, and can it reliably test the under-£90,000 annual-turnover eligibility across all relevant business records? |
| OQ-4 | Product decision | Which annual capital-allowance calculations are owned by Trade Control, an asset subsystem, an accountant/user workflow, or left unsupported? The DTO fields are known, but an accounting ledger does not itself establish statutory allowance values. |
| OQ-5 | **Preview** | HMRC's 2026–27 annual Self Employment schema is test-only at the cut-off. Recheck Production status before enabling `adjustmentToProfitsForClass4` or treating the removed fields as final for a filed 2026–27 return. |
| OQ-6 | **Future** | The roadmap proposes new first-year allowance data in December 2026 and Individual Calculations v9 changes. These must trigger a new contract review; they are not implemented from this document. |
| OQ-7 | Product boundary | The final declaration requires the taxpayer's whole return. If Trade Control does not support every other income/relief API, the UI must clearly hand off unsupported data to HMRC online services and verify that it is complete before `intent-to-finalise`. |
| OQ-8 | Sandbox/Production access | HMRC states that the market window for new 2026–27 quarterly-update Production credential requests is closed. This is an onboarding constraint, not a DTO change, but must be resolved outside implementation if Trade Control lacks approved Production access. |

Except for OQ-1 and the explicitly preview/future schemas, the current Self Employment Objective 3 contract can be implemented without inventing endpoints, identifiers, workflow stages or statutory properties. Objective 2 must first resolve OQ-2 and correct the 13-versus-15 detailed-expense statement.

## 13. Authoritative source register

All sources are official HMRC Developer Hub, HMRC-maintained GitHub, or GOV.UK material. No secondary source was needed.

| ID | Official source | Used for |
|---|---|---|
| S1 | [Self Employment Business API v5 landing page](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-employment-business-api/5.0) | Current version, environments, API purpose, mandatory fraud headers |
| S2 | [Self Employment Business API v5 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-employment-business-api/5.0/oas/page) | Exact cumulative/annual endpoints, schemas, validation, responses and test scenarios |
| S3 | [Service guide — making updates during the tax year](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/documentation/make-updates-during-tax-year.html) | Obligations, cumulative semantics, calendar quarters, replacement behaviour, £90,000 consolidated-expense threshold |
| S4 | [Service guide — making updates at the end of a tax year](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/documentation/make-updates-at-tax-year-end.html) | Annual, BSAS, non-aligned, losses, calculation, declaration and amendment sequence |
| S5 | [Business Details API v2 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/business-details-api/2.0/oas/page) | Business ID, business detail, quarterly choice, accounting type, periods of account, LADR |
| S6 | [Obligations API v3 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/obligations-api/3.0/oas/page) | Quarterly and final-declaration paths, filters and response shapes |
| S7 | [Individual Calculations API v8 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/individual-calculations-api/8.0/oas/page) | Calculation trigger/list/retrieve, final declaration, versions, errors and response envelope |
| S8 | [Business Source Adjustable Summary API v7 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-assessment-bsas-api/7.0/oas/page) | BSAS trigger, adjustments, validation and response identifiers |
| S9 | [Individual Losses API v7 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/individual-losses-api/7.0/oas/page) | 2026–27 losses and claims contract |
| S10 | [Individual Losses API v6 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/individual-losses-api/6.0/oas/page) | Through-2025–26 brought-forward loss and claim resources |
| S11 | [Individuals Tax Liability Adjustments API v1 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/individuals-tax-liability-adjustments-api/1.0/oas/page) | Carry-back liability adjustments from 2026–27 |
| S12 | [Business Income Source Summary API v3 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-assessment-biss-api/3.0/oas/page) | Read-side business profit/loss summary |
| S13 | [Self Assessment Accounts API v4 OpenAPI reference](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-assessment-accounts-api/4.0/oas/page) | Current balances, liabilities, payments and allocations |
| S14 | [Making Tax Digital for Income Tax API roadmap](https://developer.service.hmrc.gov.uk/roadmaps/mtd-itsa-vendors-roadmap/documentation/apis.html) | EOPS removal, cumulative updates, 2026–27 and future contract status |
| S15 | [HMRC Income Tax MTD API changelog](https://github.com/hmrc/income-tax-mtd-changelog/blob/main/README.md) | Release dates, retirements, version migration and 1 September 2026 state |
| S16 | [MTD Income Tax end-to-end service guide](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/) | Whole-product boundary, official terminology, Production-access warning and shared integration requirements |

