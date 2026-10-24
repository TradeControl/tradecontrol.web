# Tax Hub — Sole Trader Objective 2 Contract Synchronisation

## Purpose

Synchronise the current Sole Trader Objective 2 SQL/bootstrap implementation with the authoritative current Self Employment contract reference.

This is an implementation session.

The objective is to make the existing Sole Trader MIN/STD templates structurally ready to populate the current Objective 3 HMRC Self Employment contracts.

Do not implement Objective 3 C# contract classes in this session.

---

## Authoritative References

Use:

`docs/projects/Tax Hub/specs/reference/sole-trader-contracts.md`

and the completed reconciliation findings in:

`docs/projects/Tax Hub/findings.md`

under:

`## Sole Trader Objective 2 / Objective 3 Contract Reconciliation`

The contract reference takes precedence over historical implementation assumptions.

Apply the reviewed decisions below rather than reopening the entire investigation.

---

## Important Tax Tag Semantics

For this project:

- `Component` means a statutory field that Trade Control may directly supply and therefore may be mapped.
- `Rollup` means a statutory field that is read-only/calculated on the HMRC side and is therefore not directly mapped.
- `Derived` means a statutory value obtained outside the normal Business Node accounting mapping.

Do not infer TagClass from whether the Trade Control source itself is aggregated.

A Trade Control accounting aggregate may still map to a writable HMRC Component.

This distinction is especially important for:

`consolidatedExpenses`

HMRC accepts this as an input field, so it remains a Component.

---

## Primary SQL Scope

Work primarily within:

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_BASE_MIN_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_MTD_2026.sql`

`src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_TAX_MTD_2026.sql`

Also inspect and modify directly related current objects where necessary, especially:

`Cash.fnTaxBizCumulative`

and the existing Sole Trader cumulative projection fixtures/tests.

Do not inspect or modify archived prototype SQL unless a live dependency explicitly requires it.

---

# Required Changes

## 1. Preserve `consolidatedExpenses` as a Writable Component

Do not reclassify `consolidatedExpenses` as Rollup.

It is a writable HMRC field and therefore remains:

- a Tax Tag;
- `TagClassCode = 1` (Component);
- `CashPolarityCode = 0` (Expense).

For MIN, preserve the existing mapping from the coarse accounting expense aggregate:

`CT-CUMEXP`

to:

`consolidatedExpenses`

This is intentional.

The MIN template represents the simpler consolidated-expense model suitable for businesses using HMRC's simpler categorisation route.

Do not replace this with artificial detailed expense classifications.

Do not move the aggregate into `otherExpenses`.

Do not add source-specific validator exceptions.

---

## 2. Add Missing Statutory Tax Tags

The current HMRC contract vocabulary includes:

- `irrecoverableDebts`
- `depreciation`

Ensure both Tax Tags exist within the Sole Trader statutory vocabulary.

Requirements:

- create the Tax Tags if absent;
- `TagClassCode = 1` (Component);
- `CashPolarityCode = 0` (Expense);
- include them within the relevant Sole Trader Tax Source definitions;
- ensure validation and projection logic recognise them as legitimate statutory tags.

Do not create default accounting mappings.

Do not create new accounting Categories or Cash Codes solely to support these tags.

Do not infer values from unrelated accounting classifications.

These tags exist because they are valid writable HMRC contract fields.

A business that wants to supply one of these values may later:

- enable an appropriate Cash Code;
- configure the accounting treatment;
- map that Cash Code to the Tax Tag through the Tax Configurator.

The default Sole Trader templates should leave both unmapped.

---

## 3. `irrecoverableDebts`

Treat `irrecoverableDebts` as the statutory Tax Tag name.

Do not force HMRC terminology into the accounting chart.

If a business later chooses to support this value, the accounting-side concept may use conventional Trade Control terminology such as Bad Debts, provided the eventual Tax Configurator mapping targets:

`irrecoverableDebts`

No default mapping is required in this session.

---

## 4. `depreciation`

Treat `depreciation` as a valid statutory Component Tax Tag.

Do not enable or restructure depreciation accounting solely because HMRC exposes the field.

Do not infer capital allowances from depreciation.

Accounting depreciation and capital allowances remain separate concepts.

If a business wishes to report accounting depreciation later, it may:

- enable the appropriate depreciation Cash Code;
- map it through the Tax Configurator;
- supply the statutory `depreciation` field through the normal projection mechanism.

No default Sole Trader mapping is required in this session.

---

## 5. Modify Only the Date Validation in `Cash.fnTaxBizCumulative`

Modify `Cash.fnTaxBizCumulative` for one purpose only in this session:

remove jurisdiction-specific restrictions on the supplied date range.

Remove validation which requires:

- 6 April as `@PeriodStart`;
- alignment with the first `App.tbYearPeriod.StartOn`;
- `@PeriodEnd` to be immediately before another configured accounting period;
- any other UK Self Assessment calendar rule.

The function's responsibility is simply:

> Given a Tax Source and supplied start/end dates, aggregate the mapped accounting values over that interval.

HMRC obligation rules belong to the Objective 3 population/workflow layer.

Users remain responsible for configuring their accounting periods appropriately.

Retain only genuinely generic structural validation if useful, such as:

`@PeriodStart <= @PeriodEnd`

Remove variables and code made redundant by deleting the jurisdiction-specific logic.

### Do Not Change Any Other Behaviour

Do not alter:

- mapped tag handling;
- polarity behaviour;
- reversal behaviour;
- NULL handling;
- unsupported tag handling;
- mapping expansion;
- contributor detection;
- projection semantics.

Only remove the inappropriate jurisdiction-specific date restrictions.

---

# Explicitly Do Not Change

## `CC-MINER`

The reconciliation identified a possible warning involving `CC-MINER`.

Do not alter the Sole Trader statutory projection merely to silence that warning.

Confirm from repository evidence whether the code is confined to cryptocurrency/miner accounting scenarios.

If confirmed, record the finding and make no mapping changes.

If evidence contradicts that assumption, stop and report the evidence.

Do not invent a statutory mapping.

---

## Missing Contribution Versus Explicit Zero

Do not modify projection behaviour merely to distinguish:

- no accounting rows;
- explicit accounting zero.

The HMRC zero/omission question remains unresolved pending Sandbox verification.

Continue current behaviour.

Defer any provenance enhancement.

---

## Disallowable Expenses

Do not create Tax Tags or mappings for the disallowable-expense fields.

The current accounting model has no deterministic allowable/disallowable split.

Do not estimate or infer such values.

---

## `taxTakenOffTradingIncome`

Do not create a Tax Tag or accounting mapping.

This remains external workflow data unless a future authoritative accounting source is identified.

---

## Annual Fields

Do not add annual allowance, adjustment, loss, basis, transition-profit, capital-allowance or Class 4 Tax Tags in this session.

Those remain outside the current Objective 2 accounting projection.

---

# Expected Quarterly Manifest

After the change, `UK-ITSA-SE-CUM` should expose:

- 2 income Components;
- 1 consolidated expense Component;
- 15 detailed expense Components.

Total:

**18 Tax Tags**

Conceptually:

    turnover
    otherBusinessIncome

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

All 18 are writable statutory concepts in this projection.

Existing names should otherwise remain unchanged.

Do not perform optional naming cleanup.

The Objective 3 adapter may later translate projection names to HMRC wire names where necessary.

---

# MIN Expected Behaviour

MIN remains a deliberately coarse accounting template.

It should continue to support:

- `turnover`;
- `otherBusinessIncome`;
- consolidated business expenses through `CT-CUMEXP`.

The intended path is:

    MIN accounting expense aggregate
        CT-CUMEXP
            ↓
        Cash.tbTaxTagMap
            ↓
        consolidatedExpenses
            ↓
        HMRC periodExpenses.consolidatedExpenses

Do not replace the MIN consolidated structure with detailed expense classifications.

Do not route the total through `otherExpenses`.

Do not remove the existing `CT-CUMEXP` mapping unless repository evidence proves it incorrect.

MIN is intended to remain suitable for the simpler consolidated-expense reporting model.

---

# STD Expected Behaviour

STD should continue to support the existing detailed accounting mappings.

After this session:

- existing 13 detailed mappings remain unchanged unless a proven defect is found;
- `irrecoverableDebts` exists as a statutory Component Tax Tag;
- `depreciation` exists as a statutory Component Tax Tag;
- both remain unmapped by default;
- businesses may map them later through the Tax Configurator if required;
- `consolidatedExpenses` remains available as a writable Component but is not the normal STD detailed route.

Do not introduce double counting.

Do not create synthetic accounting structures solely to satisfy the HMRC vocabulary.

---

# Tests / Validation

Update or add fixtures so the implementation proves at least:

1. `UK-ITSA-SE-CUM` contains 18 tags.
2. `consolidatedExpenses` is a Component.
3. MIN retains the `CT-CUMEXP` → `consolidatedExpenses` mapping.
4. `irrecoverableDebts` exists as a valid Component Tax Tag.
5. `depreciation` exists as a valid Component Tax Tag.
6. Both new tags are recognised by validation.
7. Both new tags remain unmapped by default.
8. Existing detailed STD mappings remain valid.
9. Existing income mappings remain valid.
10. MIN continues to produce consolidated expense values through `CT-CUMEXP`.
11. Negative reversals remain signed.
12. `Cash.fnTaxBizCumulative` accepts arbitrary valid supplied date ranges.
13. Reversed date ranges are rejected if generic validation is retained.
14. Generic Tax Tag validation succeeds for MIN and STD.
15. No default mapping exists for either `irrecoverableDebts` or `depreciation`.

Where practical, exercise both bootstrap templates against a test database.

---

# Documentation

Append a concise implementation summary to:

`docs/projects/Tax Hub/change-log.md`

Do not rewrite historical entries.

Update the Sole Trader reconciliation section in:

`docs/projects/Tax Hub/findings.md`

to record the final reviewed decisions:

- `consolidatedExpenses` remains a writable Component;
- MIN retains `CT-CUMEXP` mapped to `consolidatedExpenses`;
- `irrecoverableDebts` exists as a statutory Component but is unmapped by default;
- `depreciation` exists as a statutory Component but is unmapped by default;
- `CC-MINER` is not an Objective 3 blocker if confirmed crypto-only;
- zero/no-row behaviour remains deferred pending OQ-1;
- `Cash.fnTaxBizCumulative` was corrected by removing jurisdiction-specific date validation;
- TagClass describes the statutory field behaviour, not whether the Trade Control source happens to be an aggregate.

Preserve the historical record.

---

# Build and Repository Discipline

Before editing:

- confirm the working tree is clean;
- report if it is not.

After editing:

- build the relevant SQL/database project;
- run the relevant fixtures/tests;
- inspect `git diff`;
- ensure no archived prototype project has been modified;
- ensure no Corporation Tax work has been introduced;
- ensure no Objective 3 C# classes have been created.

Fix only issues caused by this implementation.

Do not perform unrelated cleanup.

---

# Commit

If validation succeeds, commit the completed implementation.

Suggested message:

`Sync Sole Trader projection with current MTD contract`

Do not begin Objective 3 afterwards.

---

# Completion Report

Stop after the commit and report:

- files changed;
- Tax Tags added or modified;
- mappings added, removed or preserved;
- confirmation that MIN retains `CT-CUMEXP` → `consolidatedExpenses`;
- confirmation that `irrecoverableDebts` and `depreciation` are unmapped by default;
- exact change made to `Cash.fnTaxBizCumulative`;
- confirmation of the `CC-MINER` conclusion;
- tests executed and results;
- build result;
- unresolved issues;
- commit hash.

Completion gate:

> The Sole Trader Objective 2 projection is synchronised with the verified Self Employment contract sufficiently for Objective 3 C# contract implementation to begin.

Do not proceed beyond that gate.
