# Tax Hub Session Brief

## Phase 4D — Sole Trader Cumulative Projection Foundation

### Purpose

Implement the approved SQL and Objective 2 foundation for the Sole Trader MTD cumulative projection described by the Phase 4C findings.

This is the first constructive implementation after reconnaissance.

Do not implement HMRC transport.

## Approved decisions

The following Phase 4C proposals are approved for this phase:

- use `UK-ITSA-SE-CUM` as the cumulative Sole Trader Tax Source;
- use the proposed sixteen-tag Objective 2 vocabulary:
  - `turnover`
  - `otherBusinessIncome`
  - `consolidatedExpenses`
  - the thirteen directed detailed expense tags;
- MIN supports the consolidated-expenses pattern;
- STD continues to inherit MIN's accounting base and supports the detailed-expenses pattern;
- add the proposed `CT-CUMEXP` structural total to MIN;
- refine STD using the proposed detailed accounting categories and Cash Codes from Phase 4C;
- disable the ambiguous STD posting choices `CC-DIRCT` and `CC-ADMIN` once their replacements are installed;
- retain/re-enable `CC-EMPNI` as a Sole Trader staff-cost code;
- Tax Tag statutory orientation must be explicit metadata;
- effective polarity comes from contributing leaf Cash Codes;
- Income statutory amount retains Trade Control economic sign;
- Expense statutory amount is Trade Control economic amount multiplied by `-1`;
- `ABS()` / `Math.Abs()` is not valid statutory conversion;
- MIN and STD mappings are installed by their respective MTD wrappers;
- consolidated and detailed expense mappings are mutually exclusive;
- submission capability is determined by current mappings and validation, not bootstrap identity;
- no quarterly disallowable-expense taxonomy is to be introduced.

For MIN consolidated reporting, `CT-CUMEXP` is an accounting/reporting roll-up. Keep owner capital/drawings, tax, asset movements, transfers and personal/non-business movements outside it. Do not create parallel allowable/disallowable Category Trees for quarterly reporting.

## Financial-period rule

Trade Control financial periods are configurable metadata over dated economic activity.

For Sole Trader MTD, the configured Trade Control financial-year boundary should agree with the HMRC tax-year boundary. Intermediate Trade Control period boundaries may remain operationally flexible.

The statutory projection must use explicit start/end dates rather than repurposing the existing discrete-quarter due-date machinery.

Where relevant, expose enough information for a later UI to warn:

> Ensure that the HMRC submission dates agree with the Trade Control Financial Periods. If not transactions may be reported in the wrong years. It will take you 30 seconds to fix.

Do not implement Tax Hub UI in this phase.

## Implementation

Implement the following bounded work:

1. Replace the historical `UK-ITSA-SE-QU` Tax Source/tag seed with `UK-ITSA-SE-CUM` and the approved sixteen-tag manifest.

2. Add explicit statutory orientation for cumulative Tax Tags.

3. Implement the approved MIN `CT-CUMEXP` accounting structure.

4. Implement the Phase 4C STD detailed Category/CashCode structure, including the approved moves, additions and disabling of ambiguous coarse posting codes.

5. Install wrapper-owned mappings:
   - MIN MTD: income plus `consolidatedExpenses`;
   - STD MTD: income plus all thirteen detailed expense mappings and no consolidated mapping.

6. Propagate existing leaf `CashPolarityCode` through a reusable effective Tax Tag/CashCode projection without a second category traversal.

7. Extend/add validation so that:
   - mappings resolve to enabled non-neutral leaves;
   - contributor polarity matches Tax Tag orientation;
   - parent/descendant or multiple-root duplication fails;
   - a CashCode cannot contribute to mutually exclusive cumulative tags;
   - consolidated and detailed patterns cannot coexist;
   - detailed submission readiness requires all thirteen directed expense tags mapped;
   - consolidated readiness requires the consolidated mapping and no detailed mappings;
   - a supported genuine zero remains distinct from an unmapped/unsupported concept;
   - customised Category Trees are assessed from effective mappings rather than bootstrap identity.

8. Add a parameterised cumulative Objective 2 SQL interface taking explicit start/end dates.

Do not alter or repurpose `Cash.fnTaxTypeDueDates` or the existing generic discrete-period machinery.

Use the most direct dated accounting surface that preserves correct boundary semantics. Trade Control period boundaries must not be approximated as calendar months.

9. Add repeatable SQL/database fixtures covering:
   - MIN consolidated projection;
   - STD detailed projection;
   - parent/descendant overlap;
   - cross-tag overlap;
   - mixed/neutral polarity failure;
   - ordinary income;
   - ordinary expense;
   - expense credit/reversal;
   - credits exceeding expenditure;
   - genuine zero versus unsupported/unmapped;
   - customised submission-capable and non-capable Category Trees;
   - financial-year boundary/date handling.

$19. Add or adjust the minimal Objective 2 C# projection model and reader as required to consume the new cumulative projection.

Remove `Math.Abs` only as part of this verified replacement path.

## Exclusions

Do not implement in this phase:

- Objective 3 HMRC cumulative request DTOs;
- HMRC JSON serialization;
- HMRC required-zero/default behaviour;
- Test Harness cumulative endpoints;
- Sandbox calls;
- OAuth, fraud headers or transport;
- annual adjustments or allowances;
- losses or finalisation;
- VAT;
- Corporation Tax;
- Tax Hub UI;
- autonomous source-control operations.

Do not perform deployed-data cleanup/migration unless required to make the source-tree implementation internally consistent. Record any such migration requirement instead.

## Validation

At minimum:

- build the SQL project;
- build `HMRC_MTD`;
- execute the new database fixtures where the available environment permits;
- demonstrate exact MIN and STD mapping inventories;
- demonstrate polarity conversion numerically;
- demonstrate consolidated/detailed mutual exclusion;
- demonstrate customised-tree readiness is mapping-driven;
- confirm no legacy `UK-ITSA-SE-QU` constructive path remains;
- confirm no Objective 3 or transport implementation has been introduced.

Append implementation evidence, decisions and any out-of-scope discoveries to `change-log.md`.

Do not rewrite historical findings.

## Constraints

Work may inspect the complete superproject and submodules.

Modify only the files required for this bounded implementation.

Do not commit or push.

Stop after Phase 4D implementation and validation.
