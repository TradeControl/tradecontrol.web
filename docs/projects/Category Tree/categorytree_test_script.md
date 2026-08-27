# CategoryTree Test Script

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

## Table K — Expressions (Cash Expressions subtree)

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Desktop | Mobile | Script | Script stub |
|---|---|---|---|---|---|---|---|---|---|---|
| K1 | Anchor | `__EXPRESSIONS__` | Menu hidden in mobile action bar; context menu shows only Create Expression | Right-click `__EXPRESSIONS__` | Long-press `__EXPRESSIONS__` | Mobile footer hidden; menu shows “New Expression” only | ☑ | ☑ | ☐ | category-tree.expr.root.ts |
| K2 | Menu | Expression leaf | Context menu includes View, Edit, Delete, Enable/Disable; Move hidden | Right-click expression | Long-press expression | Actions visible; Move hidden; toggle label reflects current state | ☑ | ☑ | ☐ | category-tree.expr.leaf.menu.ts |
| K3 | Details | ExpressionDetailsCard | Enable/Disable button visible for admin and reflects state | Select expression (desktop) | Tap expression (mobile) | Toggle button appears; label switches Enable/Disable correctly | ☑ | ☑ | ☐ | category-tree.expr.details.toggle.ts |
| K4 | Toggle | Expression leaf | Enable/Disable persists and updates UI without full reload | Context menu > Enable/Disable | Same | Node gets `tc-disabled` when disabled; label flips; state saved | ☑ | ☑ | ☐ | category-tree.expr.toggle.enabled.ts |
| K5 | Action form | Root/Expressions | Create Expression injects node and selects it | Create Expression > Save | Same | New expression injected under `__EXPRESSIONS__`; node selected | ☑ | ☑ | ☐ | category-tree.expr.create.ts |
| K6 | Action form | Expression leaf | Edit Expression saves and returns to Details; selection preserved | Edit > Save | Same | Details card shown; selection unchanged | ☑ | ☑ | ☐ | category-tree.expr.edit.ts |
| K7 | Action form | Expression leaf | Delete Expression removes node and selects root | Delete > Confirm | Same | Node removed; `__EXPRESSIONS__` selected | ☑ | ☑ | ☐ | category-tree.expr.delete.ts |
| K8 | Keyboard | Expressions | Shift+Arrow Up/Down reorders expression siblings | Select expression, Shift+↑/↓ | N/A | Order updated; node moves before/after expression siblings | ☑ | N/A | ☐ | category-tree.expr.keyboard.reorder.ts |
| K9 | DnD | Expressions | Drag/drop before/after reorders expression siblings only | Drag expression before/after sibling | N/A | Order updated; only expressions allowed in subtree | ☑ | N/A | ☐ | category-tree.expr.dnd.reorder.ts |
| K10 | View | Expressions | Selecting expression loads `_ExpressionDetailsCard` | Click expression | Tap expression | Details shows expression name, formula, format, type | ☑ | ☑ | ☐ | category-tree.expr.view.details.ts |
| K11 | Mobile bar | Expressions | Mobile action bar: move hidden; edit/delete/toggle shown per admin | Tap expression | Same | Bar shows view/edit/delete/toggle; move hidden | ☑ | ☑ | ☐ | category-tree.expr.mobile.bar.ts |
| K12 | Persist | Expressions | Selection and expanded state persist across reloads | Select, reload page | Same | Expression stays selected; expanded set retained | ☑ | ☑ | ☐ | category-tree.expr.view.persist.ts |

Notes:
- K2/K4 validate toggle label via DOM text and persisted `tc-disabled` class.
- K6 accepts the current behaviour of returning to Details; selection must remain on the edited expression (no tree reset).
- Keyboard/DnD (K8/K9) are desktop-only; ensure handlers call `ReorderExpression` and update UI order without page reload.

---

### Outstanding Work (Optional Future Refactor – Option B)

- Centralize URL/query construction for actions (selection normalization).
- Consolidate mobile enhancement scripts and remove duplication.
- Consider server-side helpers for consistent embed vs full-page flows.

At present there are no failing scenarios on mobile.
