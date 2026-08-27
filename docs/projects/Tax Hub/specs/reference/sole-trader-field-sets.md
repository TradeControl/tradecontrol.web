# SA & MTD Sole Trader Tax Tag Specification (2026 Edition)

*A unified, minimal specification for template bootstrap procedures.*

## 1. Purpose of this Specification

This document defines the **tax tag sets** used when initializing a Sole Trader node for:

- **SA100 / SA103F submissions** (XML regime)
- **MTD ITSA submissions** (REST regime: Quarterly Update + EOPS)

It provides:

- the **tag lists** required for each regime
- the **rules for selecting tag sets**
- the **separation of SA and MTD semantics**
- guidance on category mapping (non‑normative)

It does **not** define:

- category mappings  
- cash‑system behaviour  
- submission formats  
- validation logic  
- UI behaviour  

These belong to other layers of the system.

## 2. Regime Separation (Fundamental Rule)

SA and MTD are **distinct tax interfaces**.

- SA uses **SA100 / SA103F schedules**.
- MTD uses **Quarterly Update (QU)** and **End of Period Statement (EOPS)**.

**No tag set is shared between regimes.**  
**No SA fields appear in MTD EOPS.**  
**No MTD fields appear in SA schedules.**

Bootstrap procedures must select the correct tag set based on the template.

## 3. SA Sole Trader Tag Set (SA100 / SA103F)

This tag set represents the **full SA103F taxonomy**.

### 3.1 Income

- turnover  
- otherIncome  

### 3.2 Allowable Expenses

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

### 3.3 Adjustments & Reliefs

- disallowableExpenses  
- privateUseAdjustments  
- basisPeriod  
- overlapRelief  
- transitionalAdjustments  

### 3.4 Capital Allowances

- capitalAllowances  
- balancingCharges  
- balancingAllowances  

### 3.5 Losses

- lossesBroughtForward  
- lossesUsed  
- lossesCarriedForward  

### 3.6 Notes

- This tag set is used **only** when generating SA100 XML.
- It is the **complete** SA103F field list.
- Category mappings are **implementation‑specific** and not part of this spec.

## 4. MTD ITSA Sole Trader Tag Sets

MTD ITSA consists of **two** submission types:

- **Quarterly Update (QU)**
- **End of Period Statement (EOPS)**

Each has its own tag set.

### 4.1 MTD Quarterly Update Tag Set

Quarterly Update contains **business totals only**.

#### 4.1.1 Income

- turnover  
- otherIncome  

#### 4.1.2 Allowable Expenses (Totals Only)

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

#### 4.1.3 Notes

- QU does **not** include adjustments, disallowables, losses, or capital allowances.
- QU tags represent **totals only**, not detailed SA schedules.
- Category mappings are **implementation‑specific**.

### 4.2 MTD EOPS Tag Set

EOPS contains **year‑end adjustments only**.

#### 4.2.1 Adjustments

- basisPeriod  
- overlapRelief  
- transitionalAdjustments  
- privateUseAdjustments  
- class4NICAdjustments  

#### 4.2.2 Capital Allowances

- capitalAllowances  
- balancingCharges  
- balancingAllowances  

#### 4.2.3 Losses

- lossesBroughtForward  
- lossesUsed  
- lossesCarriedForward  

#### 4.2.4 Notes

- EOPS does **not** include expense totals.
- EOPS does **not** include constructionCosts, wagesSalaries, adminCosts, etc.
- EOPS is **not** SA103F.
- Category mappings are **implementation‑specific**.

## 5. Template & Bootstrap Rules

### 5.1 Templates

Each Sole Trader template must specify:

- **SA Minimal** → SA tag set  
- **SA Standard** → SA tag set  
- **MTD Minimal** → MTD QU + MTD EOPS tag sets  
- **MTD Standard** → MTD QU + MTD EOPS tag sets  

### 5.2 Stored Procedures

Two main procedures exist:

- `proc_Template_BASE_MIN_2026`
- `proc_Template_CO_MICRO_CUR_STD_2026`

Four wrapper procedures are added:

- `proc_Template_ST_SOLE_CUR_MIN_SA_2026`
- `proc_Template_ST_SOLE_CUR_STD_SA_2026`
- `proc_Template_ST_SOLE_CUR_MIN_MTD_2026`
- `proc_Template_ST_SOLE_CUR_STD_MTD_2026`

Wrapper procedures call the main procedure with:

```text
@IsMTD = 1
```

### 5.3 Tag Set Selection

Inside the main procedures:

```text
IF @IsMTD = 1
    INSERT MTD tags
ELSE
    INSERT SA tags
```

### 5.4 Validation

`Cash.proc_TaxTagMapValidate` is used to detect missing category mappings.

This is a **safety net**, not a substitute for correct template design.

## 6. Category Mapping Guidance (Non‑Normative)

Category mappings are **not** part of this specification.

Reason:

- The Cash system is flexible.
- Users may customise category trees.
- Templates represent **suggested defaults**, not mandatory mappings.
- Validation will detect missing mappings at bootstrap time.

This spec defines **tags**, not **categories**.

## 7. Summary

This specification:

- replaces all previous SA/MTD mapping documents  
- removes the SA/MTD hybridisation error  
- defines clean, correct tag sets for both regimes  
- supports minimal and standard templates  
- supports wrapper procedures  
- keeps category mapping out of the spec  
- aligns with HMRC’s actual interfaces  
- is concise, readable, and future‑proof

