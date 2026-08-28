# Trade Control - General Principles for Module Refactor

July 2026

These are the reusable invariants, architectural rules, and behavioural contracts that define how any prototype Trade Control module should be refactored or built under the AI to AI system.

## 1. Purpose Principle — Behavioural Refactor, Not Redesign

Every module refactor in Trade Control:

* preserves all existing business behaviour
* replaces only the presentation layer
* adopts the modern Blazor workspace architecture
* must feel like it was always one coherent application

  Completed modules must behave exactly as the existing subsystem while adopting the architecture demonstrated by the Reference Implementation.

This becomes a universal rule.

## 2. Dual Sources of Truth Principle

Every module has exactly two authoritative sources:

### A. Business Behaviour (Legacy Implementation)

Authoritative for:

* business rules
* lifecycle
* posting
* editing
* creation
* aggregation
* email workflow
* SQL interaction

### B. UI Architecture (Reference Implementation)

Authoritative for:

* Shell ownership
* state management
* navigation
* rendering
* workspace layout
* DataGrid behaviour
* desktop/mobile behaviour
* CSS layout contract
* App service layer

This dual authority model is the backbone of your entire refactor strategy.

## 3. Cognitive Invariants (Universal Rules)

These invariants apply to every refactored module:

### Invariant A — Behaviour First

Preserve behaviour. Replace presentation. Never rewrite business rules.

### Invariant B — Reference Implementation is Law

Do not reinterpret or redesign the UI architecture. Extend it only.

### Invariant C — Shell Ownership

The Shell owns:

* state
* navigation
* filters
* workflow

Components remain stateless.

### Invariant D — Business Layer is Untouchable

Business logic stays in the existing classes (e.g., Invoices, Accounts, Vat, etc.).

### Invariant E — Services Only

UI communicates only through the service layer. No direct calls to NodeContext or stored procedures.

### Invariant F — Behavioural Translation

Translate behaviour exactly. Do not optimise. Do not reinterpret.

### Invariant G — Stop Before Guessing

If behaviour cannot be verified from:

* spec
* reference implementation
* legacy implementation

STOP and request clarification.

### Invariant H — Reference Implementation is Executable Specification

If the reference implementation demonstrates a behaviour, it is correct.

### Invariant I — Preserve Proven Behaviour

If a subsystem already works, treat it as a trusted baseline.

These invariants form the “constitution” of Trade Control refactors.

## 4. Architecture Principle — Fixed Responsibility Layers

Every module follows the same architectural layers:

* Business Layer
* EF Models
* Service Layer
* Shell
* Components
* UI Models
* Stylesheet

This is a stable, predictable structure for the coding model.

## 5. User Experience Principle — One Coherent Workspace

Every refactored module must:

* feel unified
* hide its legacy origins
* behave consistently across desktop and mobile
* preserve state
* preserve navigation
* preserve workflow continuity

## 6. Functional Scope Principle — Preserve All Workflows

Each module must preserve all workflows and surfaces such as:

* listings
* enquiry
* creation
* editing
* posting

## 7. Phase Based Destination Principle

This is your evolved methodology.

Phases:

* describe the destination
* do not prescribe implementation
* are capability increments
* are stable anchors for the coding model
* prevent hallucinated steps
* prevent mismatched architecture
* allow the model to generate its own Engineering Work Plan

This is the most important methodological extraction.

## 8. Mandatory Planning Cycle Principle

Before each Phase:

The model must:

1. Inspect the repository
2. Identify required files
3. Identify dependencies
4. Identify implementation risks
5. Produce an implementation plan
6. Estimate tasks
7. Identify missing information
8. Wait for approval

This is the core of your deterministic AI to AI workflow.

## 9. Implementation Conduct Principle

During implementation:

* The model may refine its plan
* The model must stop if architecture changes appear necessary
* The model must explain conflicts
* The model must wait for approval

This prevents drift and protects the architecture.

## 10. Completion Criteria Principle

A module is complete when:

* all workflows exist
* behaviour matches legacy
* reference implementation architecture is preserved
* desktop/mobile behaviour matches baseline
* state preservation works
* paging/sorting/totals work
* enquiry/raise/edit/submission work
* legacy pages are retired

This becomes the universal definition of “done”.

## Summary — The General Principles

Here is the distilled list:

1. Behavioural Refactor, Not Redesign
2. Two Sources of Truth: Legacy Behaviour + Reference Implementation
3. Cognitive Invariants (A–I)
4. Fixed Architectural Layers
5. Unified Workspace UX
6. Preserve All Workflows
7. Phase Based Destination Specification
8. Mandatory Planning Cycle
9. Stop Before Guessing Conduct
10. Completion Criteria

