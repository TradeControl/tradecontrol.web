# Slice 1 Identify ITSA Quarterly Update “field list”

“Provide the list of MTD ITSA Quarterly Update JSON fields/categories for self-employment income/expenses (SA103-style). Include names and what each represents. No mapping yet.

Output: a simple table of FieldName + Meaning”

--

# MTD ITSA – Quarterly Update Fields (Self‑Employment)

**Source:** HMRC guidance on MTD ITSA quarterly updates (income + expenses).  
**Purpose:** FieldName + Meaning only. No mapping.

## Income Fields

| FieldName        | Meaning |
|------------------|---------|
| turnover         | Total sales / takings / receipts from the trade for the quarter. |
| otherIncome      | Any other business income not included in turnover (e.g., incidental earnings). |

## Expense Fields

| FieldName            | Meaning |
|----------------------|---------|
| costOfGoods          | Cost of goods bought for resale or used in providing services. |
| constructionCosts    | Payments to subcontractors (CIS‑related). |
| wagesSalaries        | Staff wages, salaries, bonuses, pensions (not drawings). |
| carVanExpenses       | Motor expenses (fuel, repairs, insurance, etc.). |
| travelExpenses       | Travel costs not covered by motor expenses. |
| premisesRunningCosts | Rent, utilities, repairs, maintenance of business premises. |
| maintenanceCosts     | General repairs and maintenance not tied to premises. |
| adminCosts           | Office/admin expenses (postage, stationery, phone, software). |
| advertisingMarketing | Advertising, marketing, website, promotion. |
| interestOnLoans      | Interest on business loans and finance charges. |
| financialCharges     | Bank charges and similar financial costs. |
| badDebts             | Allowable bad debt write‑offs. |
| professionalFees     | Accountancy, legal, consultancy fees. |
| otherExpenses        | Any allowable business expenses not covered by the above categories. |

## Notes

- These categories reflect HMRC’s requirement that quarterly updates contain **summaries**, not detailed transactions.  
- They correspond to the **SA103F/S expense headings**, but expressed as **JSON field names** rather than SA103 box numbers.  
- Quarterly updates are cumulative and can be corrected in later submissions.  

If The Model wants the following just say the word:

- the **EOPS (End of Period Statement) fields**,
- the **full SA103F/S category list**,
- or a **proposed mapping skeleton** from your semantic CT/CA/CC/CE categories → MTD categories,

---

# Slice 1b — Confirm ITSA Quarterly Update field list (self-employment)

Task:

1) Confirm the canonical JSON field names HMRC expects for MTD ITSA Quarterly Updates for self-employment.

2) Provide a 2-column table: `FieldName` + `Meaning`.
3) If any names below are non-canonical, replace them and note the replacement.

Draft list to validate (may be incomplete):

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
- otherExpenses

---

# Slice 1c

" confirm whether HMRC’s quarterly update schema differentiates between:
self-employment vs UK property vs foreign property, and whether that changes field names."

# Does HMRC’s MTD ITSA Quarterly Update Schema Differ by Income Type?

## Short Answer

Yes. HMRC defines **different quarterly update schemas** for:

- Self-employment
- UK property
- Foreign property

Each has its own field list and JSON structure.

## Why This Matters

The Model must:

- Maintain **separate mapping tables** for each income type.
- Not assume that self-employment fields apply to property.
- Select the correct schema based on the “business type” identifier.

## Why This Is Relevant Even If the MIS Only Handles Self-Employment

The question is structural, not domain-specific.

The Model is checking:

- Whether the schema is *unified* (one field list for all income types)
- Or *branched* (different field lists per income type)

HMRC uses a **branched schema**, so The Model must not generalise incorrectly.

## Conclusion

Even if the MIS only supports self-employment, The Model is correct to verify schema branching. It prevents incorrect assumptions about field reuse or schema uniformity.

If you want, I can produce:

- the full field list for UK property
- the full field list for foreign property
- a comparison table showing differences
- or a schema‑selection decision tree for The Model

## Questions

1. Are you mapping to SA103F (full) or SA103S (short)?
Most trades require SA103F.
2. Do you want a 1:1 mapping (each CA → one SA103 box)
or a grouped mapping (multiple CA → one SA103 box)?
3. Are you treating capital allowances inside the semantic engine
or in a separate tax computation layer?

---

Answers (to unblock Slice 1 – canonical self‑employment QU field list):

1) SA103 variant:

- Use **SA103F (full)** as the default target.  
- We can add a “SA103S-short” profile later if needed.

2) Mapping shape:

- For **Quarterly Updates (QUs)** we want a **grouped mapping** (many of our CA/CC roll up into one HMRC QU field).
- We do not need a strict 1:1 mapping at this stage.

3) Capital allowances:

- Treat capital allowances and year-end adjustments as **EOPS / tax computation layer**, not part of the QU semantic mapping.
- Slice 1 should therefore cover **income + expense totals only** (no capital allowances / private use adjustments yet).

Extra (re your offer):

- Please also provide a brief **schema-selection decision rule** for picking the correct ITSA schema (Self-employment vs UK property vs Foreign property), but keep it short.
- We do **not** need the full property field lists yet; just confirm they are separate schemas and name them if possible.

---

# Slice 1 – Confirmation + Schema‑Selection Rule

## 1. SA103 Variant

Use **SA103F (full)** as the default target for self‑employment.
A “SA103S-short” profile can be added later if required.

## 2. Mapping Shape

For **Quarterly Updates (QUs)** use a **grouped mapping**:

- Multiple internal CA/CC categories may roll up into one HMRC QU field.
- A strict 1:1 mapping is not required for QUs.

## 3. Capital Allowances

Capital allowances and year‑end adjustments belong to the **EOPS / tax computation layer**.
They are **not** part of the Quarterly Update semantic mapping.
Slice 1 should therefore include **income + expense totals only**.

---

# Schema‑Selection Decision Rule (Requested)

HMRC’s MTD ITSA uses **separate schemas** for each income source.  
The Model must select the correct schema based on the “business type” identifier.

## Decision Rule

1. If `businessType == "self-employment"`  
   → Use **Self‑Employment Quarterly Update Schema** (SA103‑aligned).

2. If `businessType == "uk-property"`  
   → Use **UK Property Quarterly Update Schema** (property income + property expenses).

3. If `businessType == "foreign-property"`  
   → Use **Foreign Property Quarterly Update Schema** (similar to UK property but foreign‑specific).

## Notes

- These are **three distinct HMRC schemas**.  
- Field names differ between them.  
- Only the **self‑employment** schema is relevant for the current MIS, but The Model must still recognise that the schema family is branched, not unified.

# MTD ITSA – Full Self‑Employment Quarterly Update Field List

Canonical field names + meanings  
(Aligned to SA103F categories; Quarterly Update version)

---

## Income

| FieldName   | Meaning |
|-------------|---------|
| turnover    | Total sales / takings / receipts for the period. |
| otherIncome | Any other business income not included in turnover. |

---

## Expenses (SA103F‑aligned)

| FieldName              | Meaning |
|------------------------|---------|
| costOfGoods            | Cost of goods bought for resale or used in providing services. |
| constructionCosts      | Payments to subcontractors (CIS‑related). |
| wagesSalaries          | Staff wages, salaries, bonuses, employer pension contributions. |
| carVanExpenses         | Motor expenses (fuel, repairs, insurance, servicing, etc.). |
| travelExpenses         | Travel costs not covered by motor expenses. |
| premisesRunningCosts   | Rent, utilities, repairs, maintenance of business premises. |
| maintenanceCosts       | General repairs and maintenance not tied to premises. |
| adminCosts             | Office/admin expenses (postage, stationery, phone, software). |
| advertisingMarketing   | Advertising, marketing, website, promotion. |
| interestOnLoans        | Interest on business loans and finance charges. |
| financialCharges       | Bank charges and similar financial costs. |
| badDebts               | Allowable bad debt write‑offs. |
| professionalFees       | Accountancy, legal, consultancy fees. |
| depreciation           | Depreciation (disallowed; included only for completeness). |
| otherExpenses          | Any allowable business expenses not covered above. |

---

## Notes

- This list reflects the **full SA103F expense taxonomy**, expressed in the **Quarterly Update JSON field style**.  
- Quarterly Updates require **totals only**, not adjustments.  
- Capital allowances, private use adjustments, and disallowables are handled in **EOPS**, not QUs.  
- HMRC does not publish numeric tags for these fields; these names represent the canonical category set used by MTD software vendors.

# Annual Self Assessment (SA103F) – What The Model Must Support

## 1. What the “January return” actually is

The annual Self Assessment for a sole trader consists of:

1. **SA103F (Self‑Employment Full)** – business accounts + adjustments  
2. **SA100 (Main Return)** – personal tax  
3. **Supplementary pages** if needed (SA105 property, SA102 employment, etc.)

Under MTD ITSA, these are replaced by:

- **EOPS (End of Period Statement)** – replaces SA103F adjustments  
- **Final Declaration** – replaces SA100  

The **Quarterly Updates** are *not* the tax return.  
The **EOPS + Final Declaration** *are*.

---

## 2. What the annual schema contains

The annual SA103F/EOPS layer includes:

### A. Business Accounts Data

- Turnover  
- Other income  
- All expense categories (full SA103F list)  
- Goods taken for personal use  
- Disallowable expenses  
- Adjusted profit  
- Loss brought forward  
- Loss used  
- Loss carried forward  

### B. Capital Allowances

- Annual Investment Allowance  
- Writing Down Allowances  
- Small Pools Allowance  
- Balancing charges  
- Balancing allowances  
- Private use adjustments  
- Short‑life assets  
- Cars (CO₂‑based pools)  

### C. Basis Period Information

- Start/end dates  
- Overlap profit  
- Overlap relief used  
- Transitional profit (basis period reform)  

### D. Class 4 National Insurance

- Profit for Class 4  
- Adjustments  
- Exemptions  

### E. Other Adjustments

- Averaging (for farmers, creators, etc.)  
- Foreign tax credit relief  
- Post‑cessation receipts  
- Post‑cessation expenses  

This is **far larger** than the Quarterly Update schema.

---

## 3. Why this is the “main one” for the MIS

Quarterly Updates are just **summaries**.

The **EOPS** is where:

- capital allowances are applied  
- disallowables are removed  
- private use is handled  
- losses are computed  
- basis period rules are applied  
- the *actual taxable profit* is finalised  

The **Final Declaration** then aggregates all income sources and computes the final tax liability.

---

## 4. Field Count

Quarterly Update: ~16 fields  
SA103F/EOPS: **well over 100 conceptual fields**  
(though HMRC does not expose them as a single flat JSON schema)

---

## 5. What The Model should do next

- Treat **Quarterly Update** as the “P&L totals only” layer  
- Treat **EOPS** as the “full SA103F adjustments + capital allowances” layer  
- Treat **Final Declaration** as the “SA100 aggregation” layer  

The MIS ultimately needs to support **EOPS**, not just QUs.

## Next Step

I can now produce:

- The full SA103F category list (all boxes, all adjustments)
- A canonical EOPS field list reconstructed from HMRC’s published logic
- A mapping skeleton from your semantic categories → SA103F/EOPS
- Or a unified MIS‑friendly schema that abstracts away HMRC’s chaos

Which one do you want to select?

---

