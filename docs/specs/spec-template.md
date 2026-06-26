# <Module Name> — Cognitive Specification (v0.x)

## A. Context and Purpose

Describe the legacy subsystem being replaced, the architectural goals, and the conceptual model the new module must adopt.  
State what behaviour must be preserved and what presentation/UI must be discarded.  
Define the semantic centre of the module (e.g., “The **Entity** is the conceptual core”).

## A. Project Directive

### 1. Module Structure

- Create new namespace: `<Namespace>\<Module>`
- Identify legacy namespaces to absorb.
- Preserve: behavioural logic, stored procedures, validation, posting rules, balancing logic.
- Discard: legacy UI, Razor Pages, obsolete presentation.

### 2. Backend / Schema Requirements

- Add or extend fields required for module semantics.
- Define resolution ordering (e.g., DAG → polarity → settlement).
- Specify required schema changes across related tables.
- Identify procedures requiring updates.

### 3. Behavioural Logic

- Define polarity/behavioural rules.
- Define fallback or auto‑creation rules (e.g., auto‑invoice, auto‑context).
- Define settlement or matching rules (e.g., FIFO within namespace).

### 4. Posting Model

- Define posting semantics (session‑based, period‑independent, spool flush).
- Clarify what posting does and does not depend on.
- Define period selector behaviour (view‑only vs operational).

### 5. UI/UX Requirements

- Define landing view.
- Define grouping, filtering, namespace resolution.
- Define required components and their responsibilities.
- Define desktop vs mobile behaviour.
- Define which workflows are exposed for which account/entity types.

### 6. Cognitive Invariants

List the non‑negotiable conceptual rules, e.g.:

- **Invariant A — DAG First**  
- **Invariant B — Polarity from Category**  
- **Invariant C — Session‑Based Posting**  
- **Invariant D — Modern Blazor UI**  

These anchor the entire module.

## B. UI/UX Design Specification

### 1. Design Intent

Describe the purpose of the module as a unified Blazor surface replacing multiple legacy workflows.

### 2. Core UX Principles

Define the “X‑first” principles (e.g., statement‑first, namespace‑first, session‑first).

### 3. Shell and Navigation Model

- Left pane: tree, filters, selectors.  
- Right pane: workspace host, header, action bar.  
- Mobile: single‑surface with back navigation.

### 4. Component Hierarchy

List all components, grouped by:

- Shell  
- Workspaces  
- Forms  
- Panels  
- Services  
- Models  

### 5. Layout Structure

Define left‑pane and right‑pane responsibilities.  
Define action visibility rules based on entity/account type.

### 6. Filter Usage

Define:

- shared behaviour  
- resolution rules  
- workflow‑specific behaviour  
- how namespace affects grouping, filtering, and settlement  

### 7. Workspace Designs

For each workspace (e.g., Statement, Entry, Transfer):

- Toolbar  
- Summary cards  
- Main content  
- Row structure  
- Draft lists  
- Mode‑specific behaviour  
- Mobile adaptations  

### 8. Interaction Model

Define the standard user flow:

- select entity  
- resolve namespace  
- open workflow  
- save draft  
- refresh statement  
- post  

Define editing/deleting rules for unposted vs posted rows.

### 9. State Management

Define state slices and persistence rules:

- selected entity  
- selected workspace  
- namespace scope  
- draft models  
- statement rows  
- posting preview  
- busy/error states  

### 10. Backend Integration Points

Define:

- queries  
- commands  
- required behavioural guarantees  
- schema alignment  
- cross‑module dependencies  

### 11. Rendering and Refresh Rules

Define when the statement or main view must refresh.  
Define running‑balance rules and grouping rules.

### 12. Posting Confirmation Model

Define confirmation UI, success/failure behaviour, and refresh strategy.

### 13. Visual Language

Define styling conventions, badges, density, colour semantics via themes stylesheets.

### 14. Delivery Sequence

Define recommended implementation order.

### 15. Acceptance Criteria

Define the conditions under which the module is considered complete.

