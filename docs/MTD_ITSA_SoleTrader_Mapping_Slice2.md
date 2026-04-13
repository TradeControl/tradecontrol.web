# MTD ITSA — Sole Trader mapping (Slice 2)

This document proposes a provisional mapping from Trade Control categories/cash codes to the ITSA Self-Employment tags:

- `UK-ITSA-SE-QU` (Quarterly Update)
- `UK-ITSA-SE-EOPS` (Annual / EOPS)

Goal: provide a draft mapping for external (general AI) review to confirm we have not misunderstood SA103F/ITSA semantics and our TagClassCode classifications.

## Assumptions / constraints in this repo

- Prefer category mappings (`MapTypeCode = 0`) so mappings survive cash-code customization.
- Categories expand via `Cash.tbCategoryTotal` (tree rollups).
- Validate using `Cash.proc_TaxTagMapValidate @TaxSourceCode = ...`.
- Owner movements (`CA-OWNER`, `CC-OWNCAP`) are balance-sheet movements and should not map to ITSA P&L expense headings.

## Category-to-tag mapping (provisional)

These categories are created/enabled in `App.proc_Template_ST_SOLE_CUR_STD_2026`:

| Trade Control CategoryCode | Purpose | ITSA tag(s) |
|---|---:|---|
| `CA-MOTOR` | Motor running costs | `carVanExpenses` |
| `CA-TRAVEL` | Travel & transport | `travelExpenses` |
| `CA-PREMS` | Premises running costs | `premisesRunningCosts` |
| `CA-FINANCE` | Finance costs | `interestOnLoans`, `financialCharges` (see note) |
| `CA-ADMIN` | Admin/office overhead | `adminCosts` |
| `CT-OVERHD` (residual only) | Any overhead not classified above | `otherExpenses` |

### Finance split note (needs review)

`CA-FINANCE` currently contains both:

- `CC-LOINT` (loan interest) -> `interestOnLoans`
- `CC-FINCH` (other finance charges) -> `financialCharges`

So we should map by **CashCode** for finance headings, not by category.

## Proposed SQL inserts (Slice 2)

### 1) Quarterly Update (`UK-ITSA-SE-QU`)

``` SQL
INSERT INTO Cash.tbTaxTagMap (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled) VALUES -- Expenses (category mappings) ('UK-ITSA-SE-QU', 'carVanExpenses',       0, 'CA-MOTOR',  '', 1), ('UK-ITSA-SE-QU', 'travelExpenses',       0, 'CA-TRAVEL', '', 1), ('UK-ITSA-SE-QU', 'premisesRunningCosts', 0, 'CA-PREMS',  '', 1), ('UK-ITSA-SE-QU', 'adminCosts',           0, 'CA-ADMIN',  '', 1),-- Finance (cash-code mappings)
('UK-ITSA-SE-QU', 'interestOnLoans',      1, '', 'CC-LOINT', 1),
('UK-ITSA-SE-QU', 'financialCharges',     1, '', 'CC-FINCH', 1),
-- Residual overhead (catch-all)
-- NOTE: reviewer to confirm whether mapping CT-OVERHD to otherExpenses causes double count.
('UK-ITSA-SE-QU', 'otherExpenses',        0, 'CT-OVERHD', '', 1);
```

### 2) EOPS (`UK-ITSA-SE-EOPS`)

``` SQL
INSERT INTO Cash.tbTaxTagMap (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled) VALUES -- Expenses (category mappings) ('UK-ITSA-SE-EOPS', 'carVanExpenses',       0, 'CA-MOTOR',  '', 1), ('UK-ITSA-SE-EOPS', 'travelExpenses',       0, 'CA-TRAVEL', '', 1), ('UK-ITSA-SE-EOPS', 'premisesRunningCosts', 0, 'CA-PREMS',  '', 1), ('UK-ITSA-SE-EOPS', 'adminCosts',           0, 'CA-ADMIN',  '', 1),
-- Finance (cash-code mappings)
('UK-ITSA-SE-EOPS', 'interestOnLoans',      1, '', 'CC-LOINT', 1),
('UK-ITSA-SE-EOPS', 'financialCharges',     1, '', 'CC-FINCH', 1),

-- Depreciation (note: this is accounts depreciation; disallowable handling is separate)
-- TODO: confirm which depreciation code(s) exist in sole trader STD. If none, omit.
-- ('UK-ITSA-SE-EOPS', 'depreciation',       1, '', 'CC-DEPRC', 1),

-- Residual overhead (catch-all)
('UK-ITSA-SE-EOPS', 'otherExpenses',        0, 'CT-OVERHD', '', 1);
```

## Open items for reviewer

1) Confirm that mapping `CT-OVERHD -> otherExpenses` does not double-count when more specific overhead categories also sit under `CT-OVERHD`.

2) Confirm that `interestOnLoans` and `financialCharges` should be split by code (as drafted) vs rolled into one heading.
3) Confirm TagClassCode interpretation:

- Component: the headings above
- Derived: adjusted profits, disallowables totals, etc.
- Rollup: overall P&L rollups (if exposed as tags)
