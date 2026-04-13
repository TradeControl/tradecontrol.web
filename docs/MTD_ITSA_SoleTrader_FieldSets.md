# Request: Canonical ITSA Self‑Employment field sets for QU and Annual (EOPS)

We are building an MTD ITSA integration for **UK Self‑Employment (SA103F)**.

We need **two separate canonical field sets** to store as configuration:

- `UK-ITSA-SE-QU` = Quarterly Update (QUs) field set (short list)
- `UK-ITSA-SE-EOPS` = Annual business return layer (EOPS) field set (the main filing semantics for Sole Trader)

Constraints:

- Use **SA103F (full)** as the baseline.
- QU fields are **totals only**.
- EOPS fields include **adjustments + capital allowances + losses + basis period items**.
- We will implement mapping later; for now we only need canonical field names and meanings.

## Deliverable A (QU)

Confirm the canonical QU list (field names must be exact), returning a table:

| FieldName | Meaning |

QU fields we currently have:
turnover, otherIncome,
costOfGoods, constructionCosts, wagesSalaries, carVanExpenses, travelExpenses,
premisesRunningCosts, maintenanceCosts, adminCosts, advertisingMarketing,
interestOnLoans, financialCharges, badDebts, professionalFees, depreciation, otherExpenses

If any names are wrong, replace and note the correction.

## Deliverable B (EOPS) — RETURN THIS IN SLICES

EOPS is large. Return it in **three chunks**, each as a table:

| FieldName | Meaning | Notes (optional) |

### Chunk 1: Accounts totals + adjustments (non-capital-allowances)

Include (at minimum):

- turnover/otherIncome + expense headings where reused
- goods taken for own use / personal use adjustments
- disallowable expenses categories (if HMRC expects explicit fields)
- accounting profit → adjusted profit concepts

### Chunk 2: Losses + basis period (and overlap) items

Include:

- loss brought forward / used / carried forward
- basis period start/end, overlap profit, overlap relief used
- transitional profit / basis period reform items (if expected)

### Chunk 3: Capital allowances (SA103F / EOPS level)

Include:

- AIA, WDA, small pools, balancing charges/allowances
- private use adjustments
- cars/pools distinctions if they exist as separate fields

## Naming rules

- Field names should be **JSON-style** (`camelCase`), stable, and unique.
- If HMRC publishes official names, use those. If not official, propose a pragmatic canonical set and state “vendor-canonical”.

## Non-goals (do not include yet)

- Final Declaration / SA100 personal aggregation fields
- Property income schemas (UK property / foreign property)

Return only the tables and brief correction notes.

---

# Deliverable A — Canonical Quarterly Update (QU) Field Set

Canonical field names + meanings  
(Aligned to HMRC MTD ITSA Self‑Employment QU schema)

| FieldName            | Meaning |
|----------------------|---------|
| turnover             | Total sales / takings / receipts for the period. |
| otherIncome          | Any other business income not included in turnover. |
| costOfGoods          | Cost of goods bought for resale or used in providing services. |
| constructionCosts    | Payments to subcontractors (CIS‑related). |
| wagesSalaries        | Staff wages, salaries, bonuses, employer pension contributions. |
| carVanExpenses       | Motor expenses (fuel, repairs, insurance, servicing, etc.). |
| travelExpenses       | Travel costs not covered by motor expenses. |
| premisesRunningCosts | Rent, utilities, repairs, maintenance of business premises. |
| maintenanceCosts     | General repairs and maintenance not tied to premises. |
| adminCosts           | Office/admin expenses (postage, stationery, phone, software). |
| advertisingMarketing | Advertising, marketing, website, promotion. |
| interestOnLoans      | Interest on business loans and finance charges. |
| financialCharges     | Bank charges and similar financial costs. |
| badDebts             | Allowable bad debt write‑offs. |
| professionalFees     | Accountancy, legal, consultancy fees. |
| depreciation         | Depreciation (disallowed at EOPS; included here for completeness). |
| otherExpenses        | Any allowable business expenses not covered above. |

## Notes on corrections

- All names above are **vendor‑canonical** but aligned to HMRC’s published QU categories.
- No corrections required to the provided list; all names are acceptable and stable.

---

# Deliverable B — EOPS (Annual) Field Set  

## Chunk 1: Accounts Totals + Adjustments (Non‑Capital‑Allowances)

| FieldName                 | Meaning | Notes |
|---------------------------|---------|-------|
| turnover                  | Total business income for the year. | Reused from QU. |
| otherIncome               | Other business income not in turnover. | Reused from QU. |
| costOfGoods               | Cost of goods bought for resale or used in services. | Reused. |
| constructionCosts         | CIS subcontractor costs. | Reused. |
| wagesSalaries             | Staff wages, salaries, pensions. | Reused. |
| carVanExpenses            | Motor expenses. | Reused. |
| travelExpenses            | Travel costs. | Reused. |
| premisesRunningCosts      | Rent, utilities, premises repairs. | Reused. |
| maintenanceCosts          | General repairs not tied to premises. | Reused. |
| adminCosts                | Office/admin expenses. | Reused. |
| advertisingMarketing      | Advertising, marketing, promotion. | Reused. |
| interestOnLoans           | Interest on business loans. | Reused. |
| financialCharges          | Bank charges and similar costs. | Reused. |
| badDebts                  | Allowable bad debts. | Reused. |
| professionalFees          | Accountancy, legal, consultancy. | Reused. |
| depreciation              | Depreciation charged in accounts. | Disallowed later. |
| otherExpenses             | Any allowable expenses not listed. | Reused. |
| goodsForOwnUse           | Value of goods taken from the business for personal use. | Required for SA103F adjustments. |
| disallowableCostOfGoods  | Non‑allowable portion of costOfGoods. | Vendor‑canonical. |
| disallowableWages        | Non‑allowable portion of wagesSalaries. | Vendor‑canonical. |
| disallowableMotor        | Non‑allowable portion of carVanExpenses. | Vendor‑canonical. |
| disallowableTravel       | Non‑allowable portion of travelExpenses. | Vendor‑canonical. |
| disallowablePremises     | Non‑allowable portion of premisesRunningCosts. | Vendor‑canonical. |
| disallowableMaintenance  | Non‑allowable portion of maintenanceCosts. | Vendor‑canonical. |
| disallowableAdmin        | Non‑allowable portion of adminCosts. | Vendor‑canonical. |
| disallowableAdvertising  | Non‑allowable portion of advertisingMarketing. | Vendor‑canonical. |
| disallowableInterest     | Non‑allowable portion of interestOnLoans. | Vendor‑canonical. |
| disallowableFinancial    | Non‑allowable portion of financialCharges. | Vendor‑canonical. |
| disallowableBadDebts     | Non‑allowable portion of badDebts. | Vendor‑canonical. |
| disallowableProfessional | Non‑allowable portion of professionalFees. | Vendor‑canonical. |
| disallowableOther        | Non‑allowable portion of otherExpenses. | Vendor‑canonical. |
| accountingProfit          | Profit per accounts before adjustments. | SA103F concept. |
| totalDisallowables        | Sum of all disallowable expenses. | Derived. |
| adjustedProfit            | Profit after disallowables and goodsForOwnUse adjustments. | Feeds losses + CA. |

## Notes

- HMRC does **not** publish a formal JSON schema for EOPS; these names are **vendor‑canonical** but aligned to SA103F semantics.
- Disallowable categories are required because SA103F expects explicit adjustments.

# Deliverable B — EOPS Field Set

## Chunk 2: Losses + Basis Period + Overlap Items

| FieldName                     | Meaning | Notes |
|-------------------------------|---------|-------|
| lossBroughtForward            | Loss from earlier years brought into this year. | SA103F box 33 equivalent. |
| lossUsedAgainstProfit         | Loss used to reduce this year’s adjusted profit. | Vendor‑canonical. |
| lossCarriedForward            | Remaining loss carried forward to future years. | SA103F box 35 equivalent. |
| lossUsedAgainstOtherIncome    | Loss set against other income (sideways relief). | Only if applicable. |
| lossUsedAgainstCapitalGains   | Loss set against capital gains (rare). | Vendor‑canonical. |
| postCessationReceipts         | Receipts after business ceased. | SA103F concept. |
| postCessationExpenses         | Allowable expenses after cessation. | SA103F concept. |

---

## Basis Period Items

| FieldName                     | Meaning | Notes |
|-------------------------------|---------|-------|
| basisPeriodStart              | Start date of basis period for the tax year. | Required for EOPS. |
| basisPeriodEnd                | End date of basis period for the tax year. | Required for EOPS. |
| basisPeriodAdjustedProfit     | Profit allocated to the basis period after adjustments. | Feeds Class 4 NI. |
| basisPeriodDisallowables      | Total disallowable expenses for basis period. | Derived. |

---

## Overlap & Transitional Items

| FieldName                     | Meaning | Notes |
|-------------------------------|---------|-------|
| overlapProfit                 | Overlap profit carried from earlier years. | SA103F box 69 equivalent. |
| overlapReliefUsed             | Overlap relief used in this tax year. | Required for basis period reform. |
| transitionalProfit            | Transitional profit arising from basis period reform. | 2023–2027 transitional rules. |
| transitionalRelief            | Relief applied to transitional profit. | Vendor‑canonical. |
| transitionalProfitSpread      | Amount of transitional profit spread over future years. | 5‑year spreading rule. |
| adjustedProfitForTax          | Final profit after overlap relief + transitional adjustments. | Feeds capital allowances + losses. |

---

## Notes

- HMRC does **not** publish an official JSON schema for EOPS; these names are **vendor‑canonical** but aligned to SA103F semantics and basis period reform legislation.
- These fields represent the **minimum viable set** required to compute the correct taxable profit under MTD ITSA.

# Deliverable B — EOPS Field Set  

## Chunk 3: Capital Allowances (SA103F / EOPS Layer)

| FieldName                        | Meaning | Notes |
|----------------------------------|---------|-------|
| capitalAllowancesClaimed         | Total capital allowances claimed for the year. | High-level summary. |
| annualInvestmentAllowance        | AIA claimed on qualifying plant and machinery. | SA103F box 49. |
| writingDownAllowanceMainPool     | WDA for main pool assets. | Based on remaining pool value. |
| writingDownAllowanceSpecialRate  | WDA for special rate pool (e.g., integral features). | Vendor-canonical. |
| writingDownAllowanceSingleAsset  | WDA for single asset pools (short-life assets). | Vendor-canonical. |
| smallPoolsAllowance              | Small pools allowance (if pool < £1,000). | SA103F concept. |
| balancingChargeMainPool          | Balancing charge for main pool disposals. | Increases taxable profit. |
| balancingChargeSpecialRate       | Balancing charge for special rate pool. | Vendor-canonical. |
| balancingChargeSingleAsset       | Balancing charge for single asset pools. | Vendor-canonical. |
| balancingAllowanceMainPool       | Balancing allowance for main pool. | Reduces taxable profit. |
| balancingAllowanceSpecialRate    | Balancing allowance for special rate pool. | Vendor-canonical. |
| balancingAllowanceSingleAsset    | Balancing allowance for single asset pools. | Vendor-canonical. |
| privateUseAdjustment             | Adjustment for private use of assets. | Required for SA103F. |
| carMainRateAllowance             | Allowance for cars qualifying for main rate. | CO₂-based rules. |
| carSpecialRateAllowance          | Allowance for cars qualifying for special rate. | CO₂-based rules. |
| carBalancingCharge               | Balancing charge for car disposals. | Vendor-canonical. |
| carBalancingAllowance            | Balancing allowance for car disposals. | Vendor-canonical. |
| enhancedCapitalAllowance         | First-year allowances for qualifying assets (e.g., energy-saving). | If applicable. |
| superDeductionAllowance          | Super-deduction (130%) for qualifying assets (historic). | Only for relevant years. |
| fullExpensingAllowance           | 100% full expensing for qualifying plant (post-2023). | Vendor-canonical. |
| specialRateFirstYearAllowance    | First-year allowance for special rate assets (50%). | Vendor-canonical. |
| poolOpeningValueMainPool         | Opening written-down value of main pool. | Needed for computation. |
| poolOpeningValueSpecialRate      | Opening written-down value of special rate pool. | Vendor-canonical. |
| poolOpeningValueSingleAsset      | Opening written-down value of single asset pools. | Vendor-canonical. |
| poolClosingValueMainPool         | Closing written-down value of main pool. | Derived. |
| poolClosingValueSpecialRate      | Closing written-down value of special rate pool. | Derived. |
| poolClosingValueSingleAsset      | Closing written-down value of single asset pools. | Derived. |
| capitalAllowancesTotal           | Sum of all allowances after private use adjustments. | Feeds adjusted profit. |

## Notes

- HMRC does **not** publish an official EOPS JSON schema; these names are **vendor‑canonical** but aligned to SA103F and capital allowances legislation.
- This set covers all major pools, allowances, and adjustments required to compute taxable profit for self‑employment.
- Cars are separated because HMRC treats them differently from plant & machinery (CO₂‑based pools, no AIA).
