# Work Plan Guidelines

24 September 2026

## Purpose and audience

A work plan is a project document maintained and reviewed by a human developer. It should explain the delivery sequence, technical boundaries, acceptance evidence and decision points clearly enough that a single phase can later be authorised for implementation.

Write for the human technical reader first. An implementation agent should be able to follow the same document without requiring prose optimised for machine-to-machine instructions. Approval of a work plan does not, by itself, authorise implementation, production activation or an external submission.

## Establish the authority and scope

At the start of a work plan, identify:

- The objective, its governing specifications and the intended product outcome.
- The current implementation baseline and relevant prior decisions.
- Which sources govern when older documents disagree with newer reviewed decisions.
- The boundaries between the planned work and adjacent objectives or systems.
- Any prerequisite correction to an earlier contract or governing specification.
- Work that is explicitly out of scope.

Use repository evidence to name existing files, types, tests and dependency direction. Cite authoritative external specifications where they determine behaviour. Do not present a historical prototype, preview artifact or unverified assumption as a production contract.

When the plan exposes a conflict in authority, state the conflict and the required approval or correction. Do not silently revise another specification or conceal the inconsistency inside a phase description.

## Show the delivery sequence

Give the reader a short phase map before the detailed phases. Distinguish:

- Technical dependencies: what must exist before a phase can work.
- Delivery priorities: what should be completed first for the product.
- Independent work: what may be prepared in parallel without changing the priority order.
- Human decision gates: where implementation must stop for review or a programme choice.
- External gates: approvals, credentials, test accounts or official assets that the project cannot grant to itself.

Keep phase numbers and cross-references in execution order. If a priority changes, renumber phases and reconcile dependencies, review gates, integration tests and completion criteria. Do not leave the reader to reconstruct the sequence from scattered notes.

## Write phases for review and handoff

Prefer small phases that can be implemented, tested and reviewed independently. A later instruction should be able to name one phase, request its tests and stop at its review gate. If a phase necessarily contains a larger programme, divide it into explicit, separately reviewed slices.

Use a consistent structure where it helps navigation:

### Purpose

State what the phase achieves and why it is needed.

### Implementation

Identify the concrete changes. Name relevant existing files and types, expected new components where known, and the boundary between responsibilities. Use a numbered list for ordered work and bullets for sets of requirements.

### Dependencies and exclusions

State what must already be accepted. Separately identify what this phase does not do, especially work belonging to another objective. Do not obscure an external prerequisite by calling it an implementation detail.

### Tests and acceptance

List independently checkable outcomes. Specify affected test suites, new fixtures, regression or golden protection, failure cases and any controlled external test evidence. A passing local test must not be described as an external approval.

### Review gate

Say what evidence the human reviewer will inspect, what decision is needed and what remains blocked until that decision. Record phase status and execution evidence in the work plan; route durable discoveries and significant implemented changes to the project's designated findings and change-log records where those exist.

Do not add empty headings mechanically. Use additional subheadings when a phase has distinct contract, transport, operational or external-access work.

## Preserve engineering decisions during an editorial rewrite

An editorial pass changes presentation, not the approved design. Preserve:

- Phase order and product priorities.
- Architectural boundaries, dependencies and exclusions.
- Implementation requirements and repository-specific references.
- Authoritative sources, including the exact meaning of external restrictions.
- Tests, acceptance criteria, review gates and programme decisions.
- The distinction between technical readiness, external approval and production access.

Use short paragraphs and reasonably short sentences. Prefer bullets for independent requirements. Do not bury an ordered sequence in `(a)`, `(b)`, `(c)` prose or combine requirements, exclusions and acceptance criteria in a semicolon-heavy paragraph. State cross-cutting invariants once and refer back to them from phases rather than repeating them in full.

Do not shorten a work plan merely to reduce its size. Clarity and completeness matter more than brevity. If preserving an approved requirement prevents simplification, retain it and explain the constraint to the reviewer. If restructuring reveals a technical ambiguity or contradiction, flag it rather than deciding it through editorial wording.

## Describe completion truthfully

Define both intermediate product milestones and final completion. A milestone should say exactly which capabilities and evidence it requires. If the programme permits a partial delivery and a later branch, state the human decision and the status of deferred work. Do not label an objective complete when a required product slice, external approval or credential gate remains open.

Keep unsupported, preview and incomplete paths visibly fail-closed. Separate official test-service results, sandbox readiness, recognition or listing, production credentials and live activation where they are distinct states.

## Session briefs and later chat requests

A session brief describes the task as it stood when written. A later direct request in the same chat may add a requirement or authorise an additional output within that task. Treat a compatible addition as part of the current request; do not discard it merely because the generated brief has not been edited. If the later request clearly replaces the brief, follow the replacement. If the two conflict or the intended scope is unclear, resolve the conflict with the user before making a consequential change.

Always keep the authorised file and action scope clear. A suggestion to create a reusable document does not override an explicit single-file limit until the user authorises that additional document.
