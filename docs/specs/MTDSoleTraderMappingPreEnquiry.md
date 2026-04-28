## Pre‑inquiry: confirm protocol and tag set for Sole Trader submissions

Before we finalize a mapping request, we need to confirm the correct HMRC protocol and schema/tag set.

### Questions

1) For a UK Sole Trader under Making Tax Digital, what is the current/expected submission route?

- Is it MTD for Income Tax Self Assessment (ITSA)?
- Is it delivered via API (JSON) rather than XML (like CT600)?
- Are there any published schemas, tag lists, or “boxes” that align to sole trader income/expenses?

2) Is there an HMRC-defined tag taxonomy comparable to the Company Accounts/CT600 tag codes (e.g., `ACxxx`, `CPxx`)?

- If yes, please provide the authoritative reference and example payload.

3) If multiple payloads exist (periodic updates vs end-of-period statement), which one should drive our default mapping?

### Outcome

Based on the answers:

- If the tag set/schema differs from company micro-entity MTD, we will create a separate `Cash.tbTaxTagSource` (e.g., `UK-ITSA`), rather than reusing `UK-MTD`.

---

# MTD ITSA – Sole Trader Protocol & Tag‑Set Clarification

**For: The Model**  
**Purpose: Establish correct mapping targets before implementing Sole Trader submission logic**

## 1. Submission Route & Protocol

### 1.1 Correct regime

Yes — Sole Traders fall under:

- **Making Tax Digital for Income Tax Self Assessment (MTD ITSA)**
- Applies to self‑employment and property income
- Mandatory from April 2026 for qualifying individuals

### 1.2 Transport format

MTD ITSA uses:

- **API‑based submissions**
- **JSON payloads** over REST‑style endpoints
- No CT600‑style XML, no XBRL, no AC/CP tag sets

### 1.3 Published schemas / categories

HMRC provides:

- **Defined income & expense categories** for:
  - Quarterly Updates (QUs)
  - End of Period Statement (EOPS)

These categories **mirror SA103F/S headings**, not CT600 tags.

There is **no numeric tag taxonomy** like `AC12`, `CP17`, etc.

---

## 2. Tag Taxonomy (or lack thereof)

### 2.1 No CT600‑style tag system

For Sole Traders:

- There is **no equivalent** to Company Accounts tags (`ACxxx`)
- There is **no equivalent** to CT600 computation tags (`CPxx`)
- There is **no XBRL schema** for SA103 under MTD

Instead, HMRC defines:

- **Named JSON fields**
- **Category totals**
- **SA103‑aligned headings**

These are functional labels, not a formal taxonomy.

### 2.2 Example (conceptual)

A quarterly update might contain fields like:

```json
{
  "turnover": 42000,
  "otherIncome": 1200,
  "costOfGoods": 8000,
  "carVanExpenses": 2100,
  "adminCosts": 5400
}
```

These are *not* tag codes — they are category names.

## Which Payload Should Drive Mapping?

MTD ITSA has **three** submission types:

### 3.1 Quarterly Updates (QUs)

- Category totals only
- No adjustments
- Must be submitted four times per year

**This is the primary mapping target**

### 3.2 End of Period Statement (EOPS)

- Year‑end adjustments
- Capital allowances
- Private use adjustments
- Losses
- Finalised business profit

This is the **secondary mapping layer**.

### 3.3 Final Declaration

- Replaces SA100
- Aggregates all income sources
- Not where your semantic categories live

This is **not** the mapping driver.

## Summary for The Model

Sole Traders use **MTD ITSA**, not CT600
Submissions are **JSON**, not XML
Categories are **SA103‑aligned**, not AC/CP‑tagged
There is **no HMRC tag taxonomy** for sole traders
The **Quarterly Update** schema should drive your default mapping
**EOPS** adds adjustments and capital allowances
**Final Declaratio**n is aggregation only

### Suggested Nerxt Stage

mapping your semantic CT/CA/CC categories to the SA103‑aligned MTD categories.
