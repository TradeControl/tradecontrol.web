# CategoryTree Test Script

## Table A — Top anchors and basics

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub |
|---|---|---|---|---|---|---|---|---|
| A1 | Anchor | ROOT | Context menu shows only Expand/Collapse | Right-click ROOT | Long-press ROOT | Only Expand Selected / Collapse Selected visible |  | category-tree.anchor.root.ts |
| A2 | Anchor | DISCONNECTED | Context menu includes New Total + New Category | Right-click DISCONNECTED | Long-press DISCONNECTED | Shows Create Total, Create Category, Expand/Collapse |  | category-tree.anchor.disconnected.ts |
| A3 | Anchor | type root (e.g., TYPE_ROOT/type:0) | Context menu shows only Expand/Collapse | Right-click type node | Long-press type node | Only Expand/Collapse visible |  | category-tree.anchor.type.ts |
| A4 | Basics | Any | Expand Selected expands all descendants | Context menu > Expand Selected | Same | The subtree under node fully expands |  | category-tree.expand-collapse.ts |
| A5 | Basics | Any | Collapse Selected collapses subtree | Context menu > Collapse Selected | Same | Node and all descendants collapsed |  | category-tree.expand-collapse.ts |

## Table B — Creating and deleting nodes

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub |
|---|---|---|---|---|---|---|---|---|
| B1 | Menu | ROOT/Total | Menu includes: View, New Total, New Category, Edit, Delete, Toggle, Add Total (Add Existing Category), Move, Move Up/Down, Expand/Collapse, Set Profit Root, Set VAT Root | Right-click Total under ROOT | Long-press same | All listed actions visible; Set Profit/VAT Root present |  | category-tree.menu.root-total.ts |
| B2 | Action form | ROOT/Total | New Total creates child total | Choose Create Total; complete form; Save | Same | A new Total child appears under the node |  | category-tree.actions.create-total.ts |
| B3 | Action form | ROOT/Total | New Category creates child category | Choose Create Category; Save | Same | New Category appears under node |  | category-tree.actions.create-category.ts |
| B4 | Menu | ROOT/Cash Code Category | Menu includes: View, New Cash Code, Edit, Delete, Toggle, Add Existing Cash Code, Move, Move Up/Down, Expand/Collapse (no Profit/VAT Root) | Right-click Cash Code Category | Long-press | Actions listed visible; no Set Profit/VAT Root |  | category-tree.menu.root-cashcat.ts |
| B5 | Action form | ROOT/Cash Code Category | New Cash Code creates code under category | Choose New Cash Code; Save | Same | New code node appears under the category |  | category-tree.actions.create-cashcode.ts |
| B6 | Visibility | ROOT/Total | Set Profit Root / Set VAT Root visible only at ROOT-level totals | Right-click same Total | Long-press | Both Set Root actions visible |  | category-tree.visibility.setroot.ts |
| B7 | Visibility | Non-ROOT/Total | Set Profit/VAT Root hidden when parent != ROOT | Right-click Total with parent ≠ ROOT | Long-press | Set Root actions absent |  | category-tree.visibility.setroot.ts |

## Table C — Category nodes in Totals context (non-ROOT, non-type, non-DISCONNECTED)

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub | 
|---|---|---|---|---|---|---|---|---| 
| C1 | Menu | Totals context/Total | Menu includes: View, New Total, New Category, Edit, Delete, Toggle, Add Total (Add Existing Category), Move, Move Up/Down, Expand/Collapse | Right-click | Long-press | Listed actions visible; no Set Root |  | category-tree.menu.totals-total.ts |
| C2 | Menu | Totals context/Cash Code Category | Menu includes: View, New Cash Code, Edit, Delete, Toggle, Add Existing Cash Code, Move, Move Up/Down, Expand/Collapse | Right-click | Long-press | Listed actions visible |  | category-tree.menu.totals-cashcat.ts | 
| C3 | Action form | Totals context/Total | Add Existing Category maps child total | Choose Add Total; pick an existing category; Save | Same | Selected category appears as a child total |  | category-tree.actions.add-existing-category.ts |

## Table D — DISCONNECTED subtree (staging area)

 | ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub | 
|---|---|---|---|---|---|---|---|---| 
| D1 | Menu | DISCONNECTED/Total | Menu shows: New Total, New Category; no Cash Code actions | Right-click | Long-press | New Total, New Category visible; no Add/New Cash Code |  | category-tree.menu.disc-total.ts | 
| D2 | Menu | DISCONNECTED/Cash Code Category | Menu shows: New Cash Code, Add Existing Cash Code, Edit, Delete, Toggle, Move | Right-click | Long-press | Listed actions visible |  | category-tree.menu.disc-cashcat.ts | 
| D3 | Action form | DISCONNECTED/Total | New Total (disconnected) creates stand-alone total | Choose Create Total; Save | Same | New Total appears under DISCONNECTED |  | category-tree.actions.create-total-disconnected.ts | 
| D4 | Action form | DISCONNECTED/Total | New Category (disconnected) creates stand-alone category | Choose Create Category; Save | Same | New Category appears under DISCONNECTED |  | category-tree.actions.create-category-disconnected.ts |

## Table E — Type context (Cash Type ordering view)

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub | 
|---|---|---|---|---|---|---|---|---| 
| E1 | Menu | type:…/Category | Menu shows: View, Move Up, Move Down, Toggle Enabled only | Right-click category inside type:… | Long-press | Only those actions visible (no create/delete/move) |  | category-tree.menu.type.ts | 
| E2 | DnD | type:…/Category | Drag before/after sibling reorders within type | Drag category before/after a sibling | N/A (desktop only) | Sibling order changes; persists after refresh |  | category-tree.dnd.type-reorder.ts | 
| E3 | Keyboard | type:…/Category | Shift+Up/Down reorders sibling | Select node; Shift+ArrowUp/Down | N/A | Order changes accordingly |  | category-tree.keyboard.type-reorder.ts |

## Table F — Drag/drop rules (Totals/Disconnected)

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub |
|---|---|---|---|---|---|---|---|---| 
| F1 | DnD | Totals context | Reorder siblings before/after | Drag category before/after sibling | N/A | Order changes; persists after refresh |  | category-tree.dnd.totals-reorder.ts | 
| F2 | DnD | Totals context | Drop “over” allowed only into Total-type categories | Drag a category over a Total | N/A | Node becomes child of Total |  | category-tree.dnd.totals-child.ts | 
| F3 | DnD | Totals context | Drop “over” rejected into non-Total | Drag a category over non-Total | N/A | Operation rejected with message |  | category-tree.dnd.totals-child.ts | 
| F4 | DnD | DISCONNECTED | Reorder siblings | Drag before/after | N/A | Order changes; persists |  | category-tree.dnd.disconnected-reorder.ts |

## Table G — Leaf: Cash Code nodes

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub | 
|---|---|---|---|---|---|---|---|---| 
| G1 | Menu | Any/Cash Code | Menu shows: View, New Cash Code like this…, Edit, Delete, Toggle Enabled | Right-click code | Long-press code | Listed actions visible |  | category-tree.menu.code.ts | 
| G2 | Action | Any/Cash Code | “New Cash Code like this…” pre-fills form | Choose action; Save | Same | New code created; form prefilled (where applicable) |  | category-tree.actions.code-create-like.ts | 
| G3 | Action | Any/Cash Code | Edit saves, Delete removes | Edit > Save; Delete > Confirm | Same | Edits persist; delete removes node |  | category-tree.actions.code-edit-delete.ts |


## Table H — Details pane and mobile action bar visibility

 | ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub | 
|---|---|---|---|---|---|---|---|---| 
| H1 | Details | type:… | Details shows limited controls in type context | Select category inside type:… | Tap node | Buttons: View, Move Up/Down, Toggle Enabled only |  | category-tree.details.type.ts | 
| H2 | Details | ROOT/Total | Buttons include: New Total, New Category, Add Total, Move, Set Profit/VAT Root | Select node | Tap node | Listed buttons visible; function correctly |  | category-tree.details.root-total.ts | 
| H3 | Details | ROOT/Cash Code Category | Buttons include: New Cash Code, Add Existing Cash Code, Move | Select node | Tap node | Listed buttons visible; function correctly |  | category-tree.details.root-cashcat.ts | 
| H4 | Mobile bar | Any/non-synthetic | Mobile action bar shows View/Edit; hides Move/Delete in type context | N/A | Tap node | Bar visible; Move/Delete hidden in type: context |  | category-tree.mobile.bar.ts |

## Table I — Enable/Disable and cancel paths

| ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub | 
|---|---|---|---|---|---|---|---|---| 
| I1 | Toggle | Any category | Toggle Enabled flips state and updates styling | Menu > Toggle Enabled | Same | Node gains/loses disabled styling; persists |  | category-tree.toggle.enabled.ts | 
| I2 | Toggle | Cash Code | Toggle Enabled flips state (no cascade) | Menu > Toggle Enabled | Same | Code state flips; styling updates; persists |  | category-tree.toggle.enabled-code.ts | 
| I3 | Cancel | Any action | Cancel returns to previous selection (desktop RHS) | Open form; click Cancel | Open form; hit back | Pane returns; tree selection unchanged |  | category-tree.actions.cancel.ts |

## Table J — View and navigation 

 | ID | Area | Node context | Scenario | Desktop steps | Mobile steps | Expected result | Result | Script stub | 
|---|---|---|---|---|---|---|---|---| 
| J1 | View | Any | View opens details pane (desktop) or navigates (mobile) | Menu > View | Tap View | Desktop: RHS shows details; Mobile: navigates |  | category-tree.view.basic.ts | 
| J2 | Persist | Any | Selection persists across refresh | Select node; F5 | Same | Same node becomes active after reload |  | category-tree.view.persist-selection.ts | 
| J3 | Expand | Any | Expanded set persists across refresh | Expand nodes; F5 | Same | Same folders expanded |  | category-tree.view.persist-expanded.ts |