# Tax Hub — Sole Trader Objective 2 Contract Reconciliation Reconnaissance

2 September 2026

## Purpose

Reconcile the current Sole Trader MTD Income Tax Objective 2 implementation against the authoritative current Objective 3 contract research before any C# HMRC contract classes are implemented.

This is a **reconnaissance and proposal phase only**.

Do not modify SQL, C#, test harness code, documentation, or project files.

Do not proceed to Objective 3 implementation.

---

## Authoritative Contract Reference

Use as the primary external-contract reference:

`docs/projects/Tax Hub/specs/reference/sole-trader-contracts.md`

This document is the current authoritative Objective 3 contract research for the supported Trade Control route:

**Making Tax Digital for Income Tax — Self Employment**

Its findings take precedence over historical SQL assumptions, historical Tax Tags, removed SA100/SA103F structures, historical EOPS models, old C# classes and old harness payloads.

Do not reconstruct retired SA100, SA103F, EOPS, period-key or crystallisation submission models.

---

## Current Objective 2 SQL Scope

Inspect the current Sole Trader bootstrap chain and its relevant dependencies, beginning with:

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_BASE_MIN_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_MTD_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_TAX_MTD_2026.sql`

Follow any directly relevant dependencies needed to understand:

- Tax Sources;
- Tax Tags;
- Tag classes;
- polarity;
- CategoryCode mappings;
- CashCode mappings;
- cumulative extraction;
- MIN/STD differences;
- current validation assumptions.

Do not broaden the review into unrelated SQL infrastructure.

---

## Architectural Boundary

Keep the programme boundary explicit:

**Trade Control accounting**  
→ **Objective 2 Tax Source / Tax Tag statutory projection**  
→ **population / contract adapter**  
→ **Objective 3 HMRC contract model**  
→ **Objective 4 transport**

Objective 2 must expose only statutory concepts that Trade Control can legitimately and deterministically supply from accounting, approved derivation, or explicitly classified context.

Do not turn every HMRC DTO property into a Tax Tag.

Do not move NINO, business ID, tax year, period dates, calculation IDs, obligation state, declaration state or other workflow identifiers into Objective 2 merely because Objective 3 needs them.

---

## Primary Reconciliation Questions

Determine whether the current Objective 2 implementation is sufficient to populate the current HMRC Self Employment contracts described in `sole-trader-contracts.md`.

In particular, inspect and report on the following.

### 1. Quarterly Detailed Expense Coverage

The authoritative contract research establishes that the current detailed cumulative Self Employment request contains **15 detailed expense properties**, not 13.

The existing Objective 2 projection currently covers 13 detailed expense concepts.

Assess the two missing current HMRC accounting concepts:

- `irrecoverableDebts`
- `depreciation`

For each:

- determine whether MIN has a deterministic accounting source;
- determine whether STD has a deterministic accounting source;
- identify the exact CategoryCode or CashCode candidate if one exists;
- trace category ancestry;
- check whether mapping would overlap an existing mapped parent or child;
- check whether it would double count;
- identify correct `CashPolarityCode`;
- classify as:
  - supported Component;
  - Derived;
  - Contextual;
  - OptionalAbsent;
  - Unsupported.

Do not create mappings merely to obtain a complete 15-field manifest.

For depreciation, preserve the distinction between accounting depreciation and statutory capital allowances.

---

### 2. Consolidated Expenses

The current HMRC contract supports:

`periodExpenses.consolidatedExpenses`

as an alternative to detailed expense properties.

The authoritative contract research classifies this as a **Rollup plus workflow choice**, not a directly mappable Component.

Inspect the current implementation and determine whether this is already represented correctly.

Confirm:

- whether `consolidatedExpenses` is Rollup;
- whether it has any direct Tax Tag map;
- whether MIN uses it correctly;
- whether STD uses detailed categories correctly;
- whether detailed and consolidated modes can remain mutually exclusive without source-specific logic being embedded in the generic validator.

Identify any required correction.

---

### 3. Disallowable Expense Properties

The HMRC cumulative contract exposes 15 category-specific disallowable-expense properties.

Determine whether Trade Control currently contains sufficient accounting classification to project any of these deterministically.

For each relevant MIN/STD branch, assess whether the business accounting model distinguishes:

- total expense;
- allowable portion;
- disallowable portion.

Do not assume that a disallowable split exists because HMRC exposes the property.

Classify the family as:

- deterministically supportable;
- partially supportable;
- OptionalAbsent;
- Unsupported.

If only some categories can be supported, identify exactly which ones and why.

Do not propose artificial allocation or estimation.

---

### 4. `taxTakenOffTradingIncome`

The authoritative contract identifies:

`periodIncome.taxTakenOffTradingIncome`

as current HMRC contract data but not currently an Objective 2 accounting Tax Tag.

Inspect whether Trade Control has any authoritative deterministic source for this value.

Do not confuse this property with:

- subcontractor expense;
- CIS payments to subcontractors;
- ordinary turnover;
- tax deducted from supplier payments.

Classify it as:

- legitimate Objective 2 candidate;
- workflow/context;
- external;
- unsupported.

Do not create a Tax Tag without a legitimate source.

---

### 5. Annual Self Employment Candidates

Use the annual contract sections of `sole-trader-contracts.md` to classify current annual concepts against Trade Control.

The purpose is not to implement the annual contract now, but to determine which annual values belong legitimately in Objective 2.

Assess at minimum:

- `includedNonTaxableProfits`
- `basisAdjustment`
- `accountingAdjustment`
- `outstandingBusinessIncome`
- `balancingChargeBpra`
- `balancingChargeOther`
- `goodsAndServicesOwnUse`
- `transitionProfitAmount`
- `transitionProfitAccelerationAmount`
- current capital-allowance fields
- `tradingIncomeAllowance`
- structured-building allowances
- Class 4 exemption context
- loss-related values where relevant to Business Node projection.

For each classify ownership as one of:

- Component;
- Rollup;
- Derived;
- Contextual;
- External;
- OptionalAbsent;
- Unsupported.

Only Component values may be candidates for `Cash.tbTaxTagMap`.

Do not recreate an EOPS-shaped annual object.

---

### 6. Existing Quarterly Tax Tag Vocabulary

Compare the current SQL-seeded Tax Tags against the authoritative reconciliation in `sole-trader-contracts.md`.

For each current tag classify it as:

- retain unchanged;
- retain but rename for clearer projection semantics;
- retain but change class;
- retain but change mapping;
- obsolete;
- missing required candidate.

Pay particular attention to the current naming relationship between Objective 2 projection names and HMRC wire names.

Do not require Objective 2 names to match HMRC JSON property names where the semantic distinction is already explicit.

---

### 7. MIN / STD Mapping Integrity

For both MIN and STD:

- enumerate the effective current mappings;
- identify which authoritative current quarterly concepts are supported;
- identify which are intentionally unsupported;
- detect overlap or double counting;
- detect parent/child mapping conflicts;
- detect polarity mismatches;
- identify uncovered enabled business P&L CashCodes relevant to the supported projection;
- confirm whether MIN and STD remain semantically valid accounting templates rather than statutory taxonomies.

Do not force MIN to simulate distinctions that only STD can support.

---

### 8. Cumulative Extraction Semantics

Inspect the current cumulative extraction path and confirm whether it remains compatible with the authoritative contract.

Verify:

- cumulative period starts from the applicable tax-year/business commencement boundary;
- period end is supplied by workflow/obligation context;
- the extraction is not quarter-only;
- there is no dependency on period keys or obligation IDs;
- accounting signs are preserved correctly;
- no blanket `Math.Abs` or equivalent sign destruction occurs in the current Objective 2 extraction;
- missing values are not automatically manufactured as zero where absence is meaningful.

Report any mismatch.

Do not implement Objective 3 serialization rules here.

---

## Required Output

Append a new section to:

`docs/projects/Tax Hub/findings.md`

Use a clear heading such as:

`## Sole Trader Objective 2 / Objective 3 Contract Reconciliation`

The report must contain:

1. current Objective 2 state;
2. authoritative contract mismatches;
3. quarterly field reconciliation;
4. MIN mapping assessment;
5. STD mapping assessment;
6. `irrecoverableDebts` assessment;
7. `depreciation` assessment;
8. disallowable-expense assessment;
9. `taxTakenOffTradingIncome` ownership assessment;
10. annual Objective 2 candidate classification;
11. cumulative extraction assessment;
12. exact proposed changes required before Objective 3 implementation;
13. exact items that require no change;
14. unresolved questions, if any.

Where possible, identify exact:

- TaxSourceCode;
- TaxTagCode;
- CategoryCode;
- CashCode;
- TagClassCode;
- CashPolarityCode;
- stored procedure/view/function involved.

---

## Proposal Classification

Every proposed change must be classified as one of:

- **Required before Objective 3**
- **Recommended cleanup**
- **Deferred**
- **No change required**

Do not mix cleanup with contract-blocking work.

The purpose of this reconnaissance is to produce the smallest safe implementation phase needed to make Objective 2 sufficient for Objective 3.

---

## Non-Goals

Do not:

- modify any repository file except appending the reconnaissance result to `findings.md`;
- implement SQL changes;
- implement C# DTOs;
- modify `HMRC_MTD`;
- modify the test harness;
- implement population/adapters;
- implement Objective 4 transport;
- add OAuth;
- add HMRC HTTP clients;
- redesign the Category Tree;
- invent unsupported annual Tax Tags;
- reconstruct SA100, SA103F or EOPS;
- review Corporation Tax.

---

## Completion Gate

Stop after reconnaissance and proposal.

Do not proceed to implementation.

The phase is complete when we can answer:

> What is the smallest evidence-based set of Objective 2 corrections required so that the current Trade Control Sole Trader projection can safely populate the authoritative current HMRC MTD Income Tax Self Employment Objective 3 contracts?

If any contract field cannot be supported deterministically, classify it explicitly rather than manufacturing a mapping.
