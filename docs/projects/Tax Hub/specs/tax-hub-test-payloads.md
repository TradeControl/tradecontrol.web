# Tax Hub — Test Harness Payload Specification  

August 2026  
Version: Objective 2.2  

Author: TradeControl / Tax Hub  
Status: Implementation Specification (Internal Harness)

## 1. Overview

This document defines the **internal test harness payload schemas** used by the Tax Hub Submission Logic (Objective 2).

These payloads are **not HMRC payloads**.  
They are **internal tag-based data structures** generated from Trade Control accounting data and returned by the WebHarness API for development, validation, and mapping verification.

The harness supports four tax sources:

- **UK‑ITSA‑SE‑QU** — Quarterly Update (Self‑Employment)  
- **UK‑ITSA‑SE‑EOPS** — End of Period Statement (Self‑Employment)  
- **UK‑MTD‑MICRO** — Micro‑entity accounting tags (FRS105-derived)  
- **VAT‑MTD** — VAT Return tag set  

Harness payloads are derived from:

- Internal SQL templates  
- Tag seeds  
- Category mappings  
- Accounting engine outputs  
- Tax classification layer  

These payloads serve as **raw tag sets** from which HMRC submission payloads (Objective 3) will later be constructed.

## 2. Transport Envelope (Harness)

All harness payloads use a simple JSON envelope:

```json
{
    "payloadVersion": "2026.1",
    "taxSourceCode": "UK-ITSA-SE-QU",
    "periodStart": "2026-04-06",
    "periodEnd": "2026-07-05",
    "subjectCode": "SUB123",
    "items": [
        { "tag": "turnover", "value": 12345 }
    ],
    "meta": {
        "generatedAt": "2026-07-21T10:00:00Z"
    }
}
```

## 3. UK‑ITSA‑SE‑QU Payload

Quarterly Update field set (tags defined in SQL seed: )

### 3.1 Tag List

All tags are numeric except where noted.

- turnover  
- otherIncome  
- costOfGoods  
- constructionCosts  
- wagesSalaries  
- carVanExpenses  
- travelExpenses  
- premisesRunningCosts  
- maintenanceCosts  
- adminCosts  
- advertisingMarketing  
- interestOnLoans  
- financialCharges  
- badDebts  
- professionalFees  
- depreciation  
- otherExpenses  

### 3.2 JSON Schema

``` json
{
    "payloadVersion": "2026.1",
    "taxSourceCode": "UK-ITSA-SE-QU",
    "periodStart": "...",
    "periodEnd": "...",
    "subjectCode": "...",
    "items": [
    { "tag": "turnover", "value": 0 },
    { "tag": "otherIncome", "value": 0 },
    { "tag": "costOfGoods", "value": 0 },
    { "tag": "constructionCosts", "value": 0 },
    { "tag": "wagesSalaries", "value": 0 },
    { "tag": "carVanExpenses", "value": 0 },
    { "tag": "travelExpenses", "value": 0 },
    { "tag": "premisesRunningCosts", "value": 0 },
    { "tag": "maintenanceCosts", "value": 0 },
    { "tag": "adminCosts", "value": 0 },
    { "tag": "advertisingMarketing", "value": 0 },
    { "tag": "interestOnLoans", "value": 0 },
    { "tag": "financialCharges", "value": 0 },
    { "tag": "badDebts", "value": 0 },
    { "tag": "professionalFees", "value": 0 },
    { "tag": "depreciation", "value": 0 },
    { "tag": "otherExpenses", "value": 0 }
    ]
}
```

## 4. UK‑ITSA‑SE‑EOPS Payload

Annual business return tag set (defined in SQL seed).

### 4.1 Tag Groups

EOPS includes all QU tags plus:

#### Adjustments

- goodsForOwnUse  
- disallowableCostOfGoods  
- disallowableWages  
- disallowableMotor  
- disallowableTravel  
- disallowablePremises  
- disallowableMaintenance  
- disallowableAdmin  
- disallowableAdvertising  
- disallowableInterest  
- disallowableFinancial  
- disallowableBadDebts  
- disallowableProfessional  
- disallowableOther  

#### Derived totals

- accountingProfit  
- totalDisallowables  
- adjustedProfit  

#### Losses

- lossBroughtForward  
- lossUsedAgainstProfit  
- lossCarriedForward  
- lossUsedAgainstOtherIncome  
- lossUsedAgainstCapitalGains  
- postCessationReceipts  
- postCessationExpenses  

#### Basis period

- basisPeriodStart  
- basisPeriodEnd  
- basisPeriodAdjustedProfit  
- basisPeriodDisallowables  
- overlapProfit  
- overlapReliefUsed  
- transitionalProfit  
- transitionalRelief  
- transitionalProfitSpread  
- adjustedProfitForTax  

#### Capital allowances

- capitalAllowancesClaimed  
- annualInvestmentAllowance  
- writingDownAllowanceMainPool  
- writingDownAllowanceSpecialRate  
- writingDownAllowanceSingleAsset  
- smallPoolsAllowance  
- balancingChargeMainPool  
- balancingChargeSpecialRate  
- balancingChargeSingleAsset  
- balancingAllowanceMainPool  
- balancingAllowanceSpecialRate  
- balancingAllowanceSingleAsset  
- privateUseAdjustment  
- carMainRateAllowance  
- carSpecialRateAllowance  
- carBalancingCharge  
- carBalancingAllowance  
- enhancedCapitalAllowance  
- superDeductionAllowance  
- fullExpensingAllowance  
- specialRateFirstYearAllowance  
- poolOpeningValueMainPool  
- poolOpeningValueSpecialRate  
- poolOpeningValueSingleAsset  
- poolClosingValueMainPool  
- poolClosingValueSpecialRate  
- poolClosingValueSingleAsset  
- capitalAllowancesTotal  

### 4.2 JSON Schema

``` json
{
    "payloadVersion": "2026.1",
    "taxSourceCode": "UK-ITSA-SE-EOPS",
    "periodStart": "...",
    "periodEnd": "...",
    "subjectCode": "...",
    "items": [
    { "tag": "turnover", "value": 0 },
    { "tag": "otherIncome", "value": 0 },
    { "tag": "costOfGoods", "value": 0 },
    { "tag": "goodsForOwnUse", "value": 0 },
    { "tag": "disallowableCostOfGoods", "value": 0 },
    { "tag": "accountingProfit", "value": 0 },
    { "tag": "adjustedProfit", "value": 0 },
    { "tag": "lossBroughtForward", "value": 0 },
    { "tag": "basisPeriodStart", "value": "2026-04-06" },
    { "tag": "capitalAllowancesClaimed", "value": 0 },
    { "tag": "capitalAllowancesTotal", "value": 0 }
    ]
}
```

## 5. UK‑MTD‑MICRO Harness Payload

Micro‑entity accounting tags (FRS105-derived).
These are **internal accounting tags**, not CT600 payloads.

Tags defined in SQL template:
AC12, AC405, AC410, AC415, AC420, AC425, AC34, AC435, CP28, CP46

### 5.1 Tag List

- AC12 — Turnover  
- AC405 — Other Income  
- AC410 — Cost of Sales  
- AC415 — Staff Costs  
- AC420 — Depreciation Total  
- AC425 — Other Charges  
- AC34 — Tax on Profit  
- AC435 — Profit and Loss  
- CP28 — Depreciation charge  
- CP46 — Depreciation adjustment  

### 5.2 JSON Schema

``` json
{
    "payloadVersion": "2026.1",
    "taxSourceCode": "UK-MTD",
    "periodStart": "...",
    "periodEnd": "...",
    "subjectCode": "...",
    "items": [
    { "tag": "AC12", "value": 0 },
    { "tag": "AC405", "value": 0 },
    { "tag": "AC410", "value": 0 },
    { "tag": "AC415", "value": 0 },
    { "tag": "AC420", "value": 0 },
    { "tag": "AC425", "value": 0 },
    { "tag": "AC34", "value": 0 },
    { "tag": "AC435", "value": 0 },
    { "tag": "CP28", "value": 0 },
    { "tag": "CP46", "value": 0 }
    ]
}
```

## 6. VAT Harness Payload

Fields defined by HMRC VAT API, but used here only as **internal tag values**.

### 6.1 Tag List

- vatDueSales  
- vatDueAcquisitions  
- totalVatDue  
- vatReclaimedCurrPeriod  
- netVatDue  
- totalValueSalesExVAT  
- totalValuePurchasesExVAT  
- totalValueGoodsSuppliedExVAT  
- totalValueGoodsReceivedExVAT  

### 6.2 JSON Schema

``` json
{
    "payloadVersion": "2026.1",
    "taxSourceCode": "VAT",
    "periodStart": "...",
    "periodEnd": "...",
    "subjectCode": "...",
    "items": [
    { "tag": "vatDueSales", "value": 0 },
    { "tag": "vatDueAcquisitions", "value": 0 },
    { "tag": "totalVatDue", "value": 0 },
    { "tag": "vatReclaimedCurrPeriod", "value": 0 },
    { "tag": "netVatDue", "value": 0 },
    { "tag": "totalValueSalesExVAT", "value": 0 },
    { "tag": "totalValuePurchasesExVAT", "value": 0 },
    { "tag": "totalValueGoodsSuppliedExVAT", "value": 0 },
    { "tag": "totalValueGoodsReceivedExVAT", "value": 0 }
    ]
}
```

## 7. Validation Rules (all payloads)

- All numeric fields must be non‑negative.  
- Dates must be ISO‑8601.  
- Tag codes must match the tax source.  
- Items must not contain duplicates.  
- Derived totals (EOPS) must be internally consistent  
- VAT fields must satisfy basic arithmetic constraints.

## 8. Implementation Notes

- QU and EOPS tags are created in the Sole Trader template (see: `App.proc_Template_ST_SOLE_CUR_MIN_2026`).  
- Category mappings for QU/EOPS are defined in section 80 (see: `App.proc_Template_ST_SOLE_CUR_MIN_2026`).  
- Micro‑entity tags are created in the MICRO template (see: `App.proc_Template_CO_MICRO_CUR_2026`).  
- VAT values are sourced from the VAT Submission reader (`Cash.vwTaxVatSubmission`).

## 9. Appendix — Tag Classes

TagClassCode meanings (from SQL seeds):

- 0 - Rollup
- 1 - Component
- 2 - Derived

**End of document.**
