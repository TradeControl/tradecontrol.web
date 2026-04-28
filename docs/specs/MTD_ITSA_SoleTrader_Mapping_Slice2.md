# MTD ITSA — Sole Trader mapping (Slice 2)

This document proposes a provisional mapping from Trade Control categories/cash codes to the ITSA Self-Employment tags:

- `UK-ITSA-SE-QU` (Quarterly Update)
- `UK-ITSA-SE-EOPS` (Annual / EOPS)

Goal: provide a draft mapping for external review to confirm we have not misunderstood SA103F/ITSA semantics and our TagClassCode classifications.

## Assumptions / constraints in this repo

- Prefer category mappings (`MapTypeCode = 0`) so mappings survive cash-code customization.
- Categories expand via `Cash.tbCategoryTotal` (tree rollups).
- Validate using `Cash.proc_TaxTagMapValidate @TaxSourceCode = ...`.
- Owner movements (`CA-OWNER`, `CC-OWNCAP`) are balance-sheet movements and must not map to ITSA P&L expense headings.

## CT-OVERHD (important Slice 2 rule)

`CT-OVERHD` is a **structural rollup** (a Total category) whose children include:

- `CA-ADMIN`
- `CA-FINANCE`
- `CA-MOTOR`
- `CA-PREMS`
- `CA-TRAVEL`

Because it is a true rollup, it must **not** be mapped to any ITSA tag.

Mapping `CT-OVERHD` would double-count all overheads already mapped via its child categories.

## Category-to-tag mapping (provisional)

These categories are created/enabled in `App.proc_Template_ST_SOLE_CUR_STD_2026`:

| Trade Control CategoryCode | Purpose | ITSA tag(s) |
|---|---|---|
| `CA-MOTOR` | Motor running costs | `carVanExpenses` |
| `CA-TRAVEL` | Travel & transport | `travelExpenses` |
| `CA-PREMS` | Premises running costs | `premisesRunningCosts` |
| `CA-ADMIN` | Admin/office overhead | `adminCosts` |

### Finance split note (needs review)

`CA-FINANCE` contains both loan interest and other finance charges.

For finance headings, map by **CashCode** (MapTypeCode = 1), not by category:

- `CC-LOINT` (loan interest) -> `interestOnLoans`
- `CC-FINCH` (other finance charges) -> `financialCharges`

## `otherExpenses` policy (Slice 2)

Do not map `CT-OVERHD` to `otherExpenses`.

Instead, populate `otherExpenses` using **CashCode mappings only** (MapTypeCode = 1), and only for CashCodes that:

- are overhead-type costs, and
- are not already mapped to a more specific ITSA heading.

This avoids double-counting and remains stable if the category tree evolves.

## Proposed SQL inserts (Slice 2)

### 1) Quarterly Update (`UK-ITSA-SE-QU`)

``` SQL
-- Quarterly Update (UK-ITSA-SE-QU) — Slice 2 (no CT-OVERHD mapping)
INSERT INTO Cash.tbTaxTagMap
    (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled)
VALUES
    -- Income + core costs
    ('UK-ITSA-SE-QU', 'turnover',        0, 'CT-TURNOV', '', 1),
    ('UK-ITSA-SE-QU', 'otherIncome',     0, 'CT-OTHRIN', '', 1),
    ('UK-ITSA-SE-QU', 'costOfGoods',     0, 'CT-CSTSAL', '', 1),
    ('UK-ITSA-SE-QU', 'wagesSalaries',   0, 'CT-STAFFC', '', 1),

    -- Overheads (category mappings)
    ('UK-ITSA-SE-QU', 'carVanExpenses',       0, 'CA-MOTOR',  '', 1),
    ('UK-ITSA-SE-QU', 'travelExpenses',       0, 'CA-TRAVEL', '', 1),
    ('UK-ITSA-SE-QU', 'premisesRunningCosts', 0, 'CA-PREMS',  '', 1),
    ('UK-ITSA-SE-QU', 'adminCosts',           0, 'CA-ADMIN',  '', 1),

    -- Finance (CashCode mappings)
    ('UK-ITSA-SE-QU', 'interestOnLoans',      1, '', 'CC-LOINT', 1),
    ('UK-ITSA-SE-QU', 'financialCharges',     1, '', 'CC-FINCH', 1),

    -- Additional headings currently mapped by CashCode (per enquiry script)
    ('UK-ITSA-SE-QU', 'professionalFees',     1, '', 'CC-PROF', 1),
    ('UK-ITSA-SE-QU', 'advertisingMarketing', 1, '', 'CC-ADVT', 1);

/*
otherExpenses (Slice 2):
Do NOT map CT-OVERHD.
If required, add MapTypeCode = 1 rows here for specific overhead CashCodes that are not mapped above.
*/
```

### 2) EOPS (`UK-ITSA-SE-EOPS`)

``` SQL
-- EOPS (UK-ITSA-SE-EOPS) — Slice 2 (no CT-OVERHD mapping)
INSERT INTO Cash.tbTaxTagMap
    (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled)
VALUES
    -- Income + core costs
    ('UK-ITSA-SE-EOPS', 'turnover',        0, 'CT-TURNOV', '', 1),
    ('UK-ITSA-SE-EOPS', 'otherIncome',     0, 'CT-OTHRIN', '', 1),
    ('UK-ITSA-SE-EOPS', 'costOfGoods',     0, 'CT-CSTSAL', '', 1),
    ('UK-ITSA-SE-EOPS', 'wagesSalaries',   0, 'CT-STAFFC', '', 1),

    -- Overheads (category mappings)
    ('UK-ITSA-SE-EOPS', 'carVanExpenses',       0, 'CA-MOTOR',  '', 1),
    ('UK-ITSA-SE-EOPS', 'travelExpenses',       0, 'CA-TRAVEL', '', 1),
    ('UK-ITSA-SE-EOPS', 'premisesRunningCosts', 0, 'CA-PREMS',  '', 1),
    ('UK-ITSA-SE-EOPS', 'adminCosts',           0, 'CA-ADMIN',  '', 1),

    -- Finance (CashCode mappings)
    ('UK-ITSA-SE-EOPS', 'interestOnLoans',      1, '', 'CC-LOINT', 1),
    ('UK-ITSA-SE-EOPS', 'financialCharges',     1, '', 'CC-FINCH', 1),

    -- Additional headings currently mapped by CashCode (per enquiry script)
    ('UK-ITSA-SE-EOPS', 'professionalFees',     1, '', 'CC-PROF', 1),
    ('UK-ITSA-SE-EOPS', 'advertisingMarketing', 1, '', 'CC-ADVT', 1);

/*
otherExpenses (Slice 2):
Do NOT map CT-OVERHD.
If required, add MapTypeCode = 1 rows here for specific overhead CashCodes that are not mapped above.
*/
```

## Open items for reviewer

1) Confirm that the ITSA “adminCosts” heading is the appropriate destination for `CA-ADMIN`.

2) Confirm that `interestOnLoans` and `financialCharges` should be split by CashCode (as drafted) vs rolled into one heading.

3) Confirm TagClassCode interpretation:

- Component: the headings above
- Derived: adjusted profits, disallowables totals, etc.
- Rollup: overall P&L rollups (if exposed as tags)
