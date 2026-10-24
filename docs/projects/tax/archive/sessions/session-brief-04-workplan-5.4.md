# Work Plan 5 — Human-readability rewrite

The technical content and phase structure of the revised Work Plan 5 are accepted.

Now perform an editorial rewrite of the complete document:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

This is a readability rewrite, not another design or research exercise.

## Objective

Work Plan 5 is a project document maintained and reviewed by a human developer.

It must remain precise enough to hand individual phases to Codex for implementation, but it should be written for a **human technical reader first**.

The current version is too compressed. It frequently combines requirements, exclusions, dependencies, implementation instructions and acceptance criteria into long compound sentences and paragraphs.

Rewrite the entire Work Plan into clear, conventional technical prose.

## Preserve the engineering content

Do not:

- change the agreed phase order;
- change product priorities;
- change architectural boundaries;
- add or remove implementation requirements;
- weaken acceptance criteria;
- remove repository-specific file/type references;
- remove authoritative references;
- change the VAT, CT, Companies House or SA product scope;
- change the Phase 5.14 programme exit gate;
- change the meaning of the HMRC production-access restriction;
- perform new research;
- implement anything.

This pass should preserve the technical decisions already made.

## Writing style

Prefer:

- short paragraphs;
- reasonably short sentences;
- bullet lists for sets of requirements;
- numbered lists for ordered implementation work;
- subheadings where a phase contains distinct pieces of work;
- explicit dependency statements;
- explicit exclusions;
- scannable acceptance criteria;
- clear human review gates.

Avoid:

- long semicolon-separated sentences;
- paragraphs containing many independent requirements;
- `(a) ... (b) ... (c) ...` sequences buried inside prose;
- repeating architectural invariants in every phase when a concise reference to an earlier invariant is sufficient;
- AI-to-AI instructional language where normal technical documentation would be clearer.

Do not shorten the document merely for the sake of reducing its size. The goal is **clarity**, not brevity.

## Phase format

Where appropriate, use a consistent structure such as:

### Purpose

A short explanation of what the phase achieves and why.

### Implementation

The concrete work required, using bullets or numbered steps where this improves readability.

### Dependencies

What must already exist or be accepted.

### Exclusions

What this phase deliberately does not do.

### Tests and acceptance

Clear, individually readable acceptance requirements.

### Review gate

What evidence the human reviewer must inspect before the next dependent phase.

Do not mechanically add empty headings where they provide no value, but use this structure consistently enough that a reader can navigate the plan quickly.

## Important distinction

This Work Plan serves two audiences:

1. the human developer reviewing, approving and controlling the programme;
2. Codex implementing an explicitly authorised phase.

The first audience has priority.

Codex should be able to obtain its implementation constraints from a clearly structured human document. The human should not have to decode prose optimised for another language model.

## Phase 5.15

Pay particular attention to Phase 5.15.

Retain its newly agreed full end-to-end MTD Income Tax/Self Assessment scope and all of its technical content.

Present:

- product scope;
- contract reconciliation;
- existing prepared building blocks;
- missing Objective 3 work;
- Objective 4 transport work;
- implementation slices;
- exclusions;
- tests;
- external HMRC production-access restriction; and
- review gates

as clearly separated material.

The implementation slices should be an explicit numbered sequence rather than being embedded inside prose.

Retain the exact HMRC quotation concerning new 2026–27 quarterly-update products.

## Cross-phase material

Apply the same readability standard to:

- Objective and authority;
- current baseline;
- invariants;
- phase graph;
- cross-phase verification;
- programme decisions;
- limited-company milestone; and
- full Objective 4 completion.

Where information is genuinely cross-cutting, state it clearly once rather than repeatedly embedding it in individual phase prose.

## Authorised change

Modify only:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

Do not modify source code or any other documentation.

When complete, report:

- that the readability rewrite is complete;
- whether any technical ambiguity was discovered while restructuring the prose; and
- any place where preserving the existing meaning prevented further simplification.

Then stop.
