# CategoryTree Test Script

## Current Status (All Previously Reported Mobile Failures Resolved)

The earlier mobile failures (B1, G2, G3, I3, D3, D4) have been fixed by applying the Option A patch (Razor-only changes).  
Key changes implemented:
- Root Set Profit / VAT Root: replaced non‑embedded buttons with POST forms including anti‑forgery and mobile confirmation; redirects now preserve selection (`select`, `key`, `expand`).
- Cash Code Create / Edit / Delete Razor pages: deterministic Cancel anchors with full query parameter set for reselection; hidden normalized keys added.
- “New Cash Code like this…” now reliably passes `siblingCashCode`.
- Disconnected creation flows (D3, D4) now operate correctly on mobile (forms and redirect selection preserved).
- Cancel semantics (I3) on mobile return to the correct node/category every time.

No JavaScript tree logic was refactored beyond necessary form interception for mobile Set Root confirmation. Option B (centralized URL helper / JS consolidation) remains available for future enhancement.

## Validation Summary
1. Mobile Create Cash Code → Cancel: returns with parent category selected. (Pass)
2. Mobile Edit Cash Code → Cancel: returns with original code selected. (Pass)
3. Mobile Delete Cash Code → Cancel: returns with parent category selected; node still present until deletion. (Pass)
4. Mobile Set Profit/VAT Root → Confirm: redirects selecting the same category; glyph updates after refresh. (Pass)
5. Disconnected creates (Total / Category) materialize under `__DISCONNECTED__` and reselection persists. (Pass)

---

## Table A — Top anchors and basics

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Desktop | Mobile | Script | Script stub |
|---|---|---|---|---|---|---|---|---|---|---|
| A1 | Anchor | ROOT | Context menu shows only Expand/Collapse | Right-click ROOT | Long-press ROOT | Only Expand Selected / Collapse Selected visible | ☑ | ☑ | ☐ | category-tree.anchor.root.ts |
| A2 | Anchor | DISCONNECTED | Context menu includes New Total + New Category | Right-click DISCONNECTED | Long-press DISCONNECTED | Shows Create Total, Create Category, Expand/Collapse | ☑ | ☑ | ☐ | category-tree.anchor.disconnected.ts |
| A3 | Anchor | type root | Context menu shows only Expand/Collapse | Right-click type node | Long-press type node | Only Expand/Collapse visible | ☑ | ☑ | ☐ | category-tree.anchor.type.ts |
| A4 | Basics | Any | Expand Selected expands all descendants | Context menu > Expand Selected | Same | Subtree fully expands | ☑ | ☑ | ☐ | category-tree.expand-collapse.ts |
| A5 | Basics | Any | Collapse Selected collapses subtree | Context menu > Collapse Selected | Same | Subtree collapses | ☑ | ☑ | ☐ | category-tree.expand-collapse.ts |

## Table B — Creating and deleting nodes

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Desktop | Mobile | Script | Script stub |
|---|---|---|---|---|---|---|---|---|---|---|
| B1 | Menu | ROOT/Total | Menu includes full action set incl. Set Profit/VAT Root | Right-click | Long-press | Actions visible; Set Root functional | ☑ | ☑ | ☐ | category-tree.menu.root-total.ts |
| B2 | Action form | ROOT/Total | New Total creates child total | Create Total > Save | Same | Child total appears | ☑ | ☑ | ☐ | category-tree.actions.create-total.ts |
| B3 | Action form | ROOT/Total | New Category creates child category | Create Category > Save | Same | Child category appears | ☑ | ☑ | ☐ | category-tree.actions.create-category.ts |
| B4 | Menu | ROOT/Cash Code Category | No Set Root actions | Right-click | Long-press | Correct menu visible | ☑ | ☑ | ☐ | category-tree.menu.root-cashcat.ts |
| B5 | Action form | ROOT/Cash Code Category | New Cash Code creates code | New Cash Code > Save | Same | Code appears | ☑ | ☑ | ☐ | category-tree.actions.create-cashcode.ts |
| B6 | Visibility | ROOT/Total | Set Profit/VAT Root only root-level totals | Right-click | Long-press | Visible | ☑ | ☑ | ☐ | category-tree.visibility.setroot.ts |
| B7 | Visibility | Non-ROOT/Total | Set Root hidden | Right-click | Long-press | Hidden | ☑ | ☑ | ☐ | category-tree.visibility.setroot.ts |

## Table C — Category nodes in Totals context

| ID | Area | Node context | Scenario | Desktop | Mobile | Script |
|---|---|---|---|---|---|---|
| C1 | Menu | Totals/Total | Full menu without Set Root | ☑ | ☑ | category-tree.menu.totals-total.ts |
| C2 | Menu | Totals/Cash Code Category | Full expected menu | ☑ | ☑ | category-tree.menu.totals-cashcat.ts |
| C3 | Action form | Totals/Total | Add Existing Category mapping | ☑ | ☑ | category-tree.actions.add-existing-category.ts |

## Table D — DISCONNECTED subtree

| ID | Area | Node context | Scenario | Desktop | Mobile | Script |
|---|---|---|---|---|---|---|
| D1 | Menu | DISCONNECTED/Total | New Total, New Category only | ☑ | ☑ | category-tree.menu.disc-total.ts |
| D2 | Menu | DISCONNECTED/Cash Code Category | Cash Code actions listed | ☑ | ☑ | category-tree.menu.disc-cashcat.ts |
| D3 | Action form | DISCONNECTED/Total | New Total (stand-alone) | ☑ | ☑ | category-tree.actions.create-total-disconnected.ts |
| D4 | Action form | DISCONNECTED/Total | New Category (stand-alone) | ☑ | ☑ | category-tree.actions.create-category-disconnected.ts |

## Table E — Type context

| ID | Area | Scenario | Desktop | Mobile | Script |
|---|---|---|---|---|---|
| E1 | Menu | Restricted actions | ☑ | ☑ | category-tree.menu.type.ts |
| E2 | DnD | Reorder within type | ☑ | ☑ | category-tree.dnd.type-reorder.ts |
| E3 | Keyboard | Shift+Up/Down reorder | ☑ | ☑ | category-tree.keyboard.type-reorder.ts |

## Table F — Drag/drop rules (Totals/Disconnected)

| ID | Area | Scenario | Desktop | Mobile | Script |
|---|---|---|---|---|---|
| F1 | DnD | Reorder siblings | ☑ | ☑ | category-tree.dnd.totals-reorder.ts |
| F2 | DnD | Drop over Total -> child | ☑ | ☑ | category-tree.dnd.totals-child.ts |
| F3 | DnD | Reject over non-Total | ☑ | ☑ | category-tree.dnd.totals-child.ts |
| F4 | DnD | Disconnected reorder | ☑ | ☑ | category-tree.dnd.disconnected-reorder.ts |

## Table G — Cash Code nodes

| ID | Area | Scenario | Desktop | Mobile | Script |
|---|---|---|---|---|---|
| G1 | Menu | Code node actions | ☑ | ☑ | category-tree.menu.code.ts |
| G2 | Action | New Cash Code like this… prefill | ☑ | ☑ | category-tree.actions.code-create-like.ts |
| G3 | Action | Edit/Delete cash code | ☑ | ☑ | category-tree.actions.code-edit-delete.ts |

## Table H — Details & mobile action bar

| ID | Area | Scenario | Desktop | Mobile | Script |
|---|---|---|---|---|---|
| H1 | Details | Type context limited controls | ☑ | ☑ | category-tree.details.type.ts |
| H2 | Details | Root Total buttons functional | ☑ | ☑ | category-tree.details.root-total.ts |
| H3 | Details | Root Cash Code Category controls | ☑ | ☑ | category-tree.details.root-cashcat.ts |
| H4 | Mobile bar | Visibility rules | ☑ | ☑ | category-tree.mobile.bar.ts |

## Table I — Enable/Disable and cancel

| ID | Area | Scenario | Desktop | Mobile | Script |
|---|---|---|---|---|---|
| I1 | Toggle | Category toggle persists | ☑ | ☑ | category-tree.toggle.enabled.ts |
| I2 | Toggle | Cash Code toggle persists | ☑ | ☑ | category-tree.toggle.enabled-code.ts |
| I3 | Cancel | Reselect after cancel | ☑ | ☑ | category-tree.actions.cancel.ts |

## Table J — View and navigation

| ID | Area | Scenario | Desktop | Mobile | Script |
|---|---|---|---|---|---|
| J1 | View | Pane vs navigation | ☑ | ☑ | category-tree.view.basic.ts |
| J2 | Persist | Selection persists | ☑ | ☑ | category-tree.view.persist-selection.ts |
| J3 | Expand | Expanded set persists | ☑ | ☑ | category-tree.view.persist-expanded.ts |

---

### Outstanding Work (Optional Future Refactor – Option B)

- Centralize URL/query construction for actions (selection normalization).
- Consolidate mobile enhancement scripts and remove duplication.
- Consider server-side helpers for consistent embed vs full-page flows.

At present there are no failing scenarios on mobile.
