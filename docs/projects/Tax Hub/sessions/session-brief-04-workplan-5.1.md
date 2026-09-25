# Tax Hub — Objective 4: HMRC Transport Platform

22 September 2026

## Codex Work Plan Session Brief

### Purpose

Prepare the implementation work plan for **Tax Hub Objective 4 — HMRC Transport Platform**.

This session is planning only.

Do **not** implement Objective 4.

Do **not** modify source code, tests, schemas, project files or existing project documentation other than the single authorised output document specified below.

The required output is:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

The work-plan number is deliberately independent of the programme Objective number:

- Objective 4 is the fourth Tax Hub programme objective.
- Work Plan 5 is the fifth implementation work plan in the project history.
- Existing `tax-hub-workplan-4.md` records completed Objective 3 implementation and must not be renamed merely to align numbering.

---

# 1. Governing programme specification

Read first:

`docs\projects\Tax Hub\specs\tax-hub-spec-programme.md`

This is the governing programme specification.

Objective 4 is the HMRC Transport Platform.

The work plan must remain consistent with the programme architecture and especially the separation between:

- accounting/statutory meaning;
- Objective 2 Tax Sources/Tags;
- Objective 3 HMRC API/Payload Contracts;
- Objective 4 transport;
- Objective 5 application workflow/UI.

Objective 4 must transport Objective 3 outputs without reinterpreting their accounting or tax meaning.

---

# 2. Completed Objective 3 implementation record

Read:

`docs\projects\Tax Hub\implementation\tax-hub-implementation-3.md`

This describes the Objective 3 design and implemented contract/preparation boundary.

Also read:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-4.md`

Despite its filename, this is the completed work-plan record for Objective 3.

Treat that numbering as historical chronology. Do not rename or rewrite it.

Use these documents to understand what Objective 3 deliberately owns and what was deliberately deferred to Objective 4.

---

# 3. Objective 4 reconnaissance and boundary review

Read in full:

`docs\projects\Tax Hub\specs\reference\hmrc-transport.md`

This is the principal research input for the new work plan.

It contains:

1. the original Objective 4 transport reconnaissance;
2. current HMRC protocol research;
3. analysis of the existing repository;
4. assessment of the historical VAT implementation;
5. proposed architecture and implementation phases; and
6. the later **Boundary Resolution Review**.

The **Boundary Resolution Review takes precedence over earlier recommendations in the same document where the two differ.**

Do not mechanically convert the original reconnaissance phases into a work plan.

The later review deliberately narrows and qualifies several earlier architectural recommendations.

Its purpose was to establish the smallest clean Objective 3 → Objective 4 boundary before implementation.

---

# 4. Tax Hub project history and decisions

Read:

`docs\projects\Tax Hub\implementation\tax-hub-implementation-*.md`

`docs\projects\Tax Hub\implementation\tax-hub-workplan-*.md`

Use these to understand earlier decisions, corrections and completed work.

Do not reopen settled earlier-objective decisions unless the Objective 4 evidence demonstrates a concrete incompatibility.

Where older material conflicts with a later reviewed decision, use the later decision.

---

# 5. Repository architecture reference

Read:

`docs\projects\Tax Hub\implementation\tax-hub-repo-structure.md`

Use this together with the actual repository.

Do not rely on documentation where the current source establishes otherwise.

---

# 6. Current implementation

Inspect the actual implementation beneath:

`src\tax-hub\src`

Inspect the relevant tests beneath:

`src\tax-hub\tests`

In particular verify the concrete types, files and dependency direction discussed by `hmrc-transport.md`, including:

- `PreparedApiContract`;
- `PreparedApiRequest`;
- `PreparedApiRequestPipeline`;
- `IPreparedApiRequestGateway`;
- `PreparedSubmissionPackage`;
- `PreparedPollingSemantics`;
- `IPreparedSubmissionPackageGateway`;
- VAT operation descriptors;
- MTD Income Tax endpoint descriptors;
- Corporation Tax package/preparer/serializer types;
- company contract readiness metadata;
- current Submission adapter placeholders;
- WebHarness composition and diagnostic transport code; and
- existing handoff, contract, golden and architecture tests.

The work plan must name actual files/types where practical rather than inventing an architecture detached from the repository.

---

# 7. Historical VAT MTD reference

Read as historical evidence only:

`.local\vat_mtd_client_test-master`

This is a pre-COVID VAT MTD sandbox application retained beneath `.local` and excluded from source control.

It was re-tested against the HMRC sandbox on 22 September 2026 and demonstrated the broad external sequence:

- HMRC OAuth authorisation;
- access-token retrieval;
- VAT obligations GET;
- fraud-prevention-header validation;
- VAT return POST;
- successful `201 Created` response.

Its architecture is **not** the target architecture.

Do not copy its:

- .NET 5 hosting structure;
- controller-built VAT payloads;
- authentication-ticket token storage;
- per-action `HttpClient` construction;
- fixed deployment assumptions;
- old fraud-header implementation; or
- sandbox-specific behaviour.

Use it only as empirical evidence of the HMRC interaction sequence and as historical context.

Do not reproduce credentials, secrets or sensitive configuration found beneath `.local`.

---

# 8. Authoritative HMRC evidence

The current authoritative sources used during reconnaissance are enumerated in:

`docs\projects\Tax Hub\specs\reference\hmrc-transport.md`

Use those references where protocol details affect the work plan.

If repository evidence and the reconnaissance are insufficient for a planning decision, verify the point against current authoritative HMRC/GOV.UK documentation.

Do not substitute assumptions or historical behaviour for current HMRC requirements.

Record any materially new authoritative evidence in the work plan where it changes implementation planning.

---

# 9. Boundary conclusions to preserve

The Boundary Resolution Review identifies four issues.

Treat them according to its final conclusions rather than simply repeating the original description of four equally serious “conflicts”.

## 9.1 REST dispatch metadata

This is a narrow contract/plumbing correction.

Objective 3 descriptors already contain most of the required transport contract information, but not all of it currently crosses through `PreparedApiRequest`.

The work plan should provide a small independently reviewable boundary phase that carries the necessary existing contract facts without introducing a duplicate Objective 4 operation catalogue.

Prepared request body bytes must remain unchanged.

## 9.2 Corporation Tax `Transmission`

This is the substantive Objective 3 → Objective 4 boundary problem.

The current diagnostic Corporation Tax XML must not be treated as an HMRC submission payload merely because it currently occupies `PreparedSubmissionPackage.Transmission`.

Plan the correction required to establish the genuine immutable CT600/service artifact boundary.

Objective 4 may subsequently add only the transport/protocol machinery HMRC requires, including the GovTalk envelope and protocol-defined IRmark operation.

Do not allow Objective 4 to reconstruct Corporation Tax statutory meaning.

## 9.3 Corporation Tax polling

The existing `None` metadata is inconsistent with the Transaction Engine conversation.

Plan the smallest correction required to identify Transaction Engine semantics.

Do not turn the Transaction Engine into a REST-shaped generic polling mechanism.

## 9.4 Corporation Tax submission readiness

Current company/Corporation Tax artifacts are deliberately preview-only.

This is principally an incomplete submission-readiness gate rather than a payload defect.

Objective 4 must reject non-submission-ready packages.

Do not make completion of the entire Corporation Tax submission-readiness programme a prerequisite for beginning REST transport implementation.

Corporation Tax protocol components may be developed/tested against approved synthetic or official fixtures while the real Objective 3 CT submission gate remains closed.

---

# 10. Architectural constraints

The work plan must preserve these rules.

### Exact prepared REST bytes

Objective 4 must send Objective 3 canonical JSON body bytes directly.

It must not deserialize and reserialize them.

### No duplicated endpoint catalogue

Transport must receive sufficient versioned contract metadata from Objective 3.

It must not recreate VAT/MTD operation definitions in an adapter-side lookup table.

### Protocol separation

HMRC REST/JSON and Corporation Tax Transaction Engine/GovTalk are materially different transports.

Share infrastructure only where there is a concrete common requirement.

Do not force them through an artificial universal transport abstraction.

### Narrow boundary correction

Any changes required to completed Objective 3 contracts must be:

- explicit;
- minimal;
- independently testable; and
- incapable of changing existing canonical JSON/iXBRL semantic values accidentally.

Existing golden bytes/digests should remain unchanged unless a specifically approved Corporation Tax submission-readiness correction requires a new artifact.

### Preview safety

No `Preview`, unsupported or error-bearing artifact may reach live/sandbox submission merely because a gateway exists.

### Secrets

Credentials, OAuth tokens and Government Gateway credentials must never become part of prepared statutory artifacts.

### Existing dependency direction

Preserve the established project dependency direction unless the work plan identifies a concrete architectural reason that it cannot be preserved.

---

# 11. Work-plan structure

Create:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

Use the established Tax Hub work-plan style where appropriate.

The work plan should begin with:

# Tax Hub Work Plan 5 — Objective 4: HMRC Transport Platform

It must describe a phased implementation that can be handed to Codex incrementally, with explicit review gates between phases.

At minimum determine whether the implementation should contain phases covering:

1. Objective 3 → Objective 4 boundary corrections and safety gates;
2. transport configuration, durable attempt/audit foundations and safe secret boundaries;
3. OAuth/token lifecycle and fraud-prevention-header infrastructure;
4. REST read/enquiry transport;
5. VAT and MTD Income Tax write transport;
6. Corporation Tax submission-readiness prerequisites;
7. Transaction Engine/GovTalk/IRmark transport; and
8. final hardening and Objective 5 handoff.

These are planning inputs, not mandatory phase names or architecture.

Simplify, combine, split or reorder them where repository dependencies justify doing so.

In particular, distinguish work that genuinely blocks the first REST implementation from work that can safely be deferred until the Corporation Tax phase.

---

# 12. Requirements for each implementation phase

For every proposed phase state:

- purpose;
- exact scope;
- relevant existing files/types;
- new files/types expected, where sufficiently known;
- dependencies on previous phases;
- explicitly excluded work;
- tests to add/change;
- acceptance criteria;
- documentation/history updates required after successful completion; and
- whether human review/approval is required before proceeding.

Prefer small reviewable phases.

Do not produce enormous speculative implementation batches.

The work plan should be suitable for later instructions of the form:

> Implement Phase N only, run the defined tests, report results and stop.

---

# 13. Tests and regression protection

The work plan must explicitly protect the completed Objective 3 work.

Plan regression tests proving, where applicable:

- existing canonical REST body bytes remain identical;
- existing SHA-256 values remain identical;
- Objective 4 has no JSON serializer path capable of rebuilding prepared REST bodies;
- required scope/status/response metadata reaches transport correctly;
- unsupported/preview/error-bearing artifacts are rejected;
- credentials never enter prepared artifacts;
- VAT/MTD success and error responses are classified without losing authority information;
- unsafe retries cannot duplicate ambiguous VAT POST submissions;
- Transaction Engine state survives acknowledgement/poll/recovery boundaries; and
- Corporation Tax transport cannot accidentally submit the existing diagnostic preview XML.

Reuse existing tests where they already establish an invariant rather than duplicating them.

---

# 14. Scope discipline

This work plan is for Objective 4.

Do not redesign:

- Trade Control accounting;
- Tax Sources/Tags;
- VAT calculations;
- MTD Income Tax calculations;
- Corporation Tax calculations;
- company accounts semantics;
- Objective 5 UI/workflow;
- Companies House transport; or
- unrelated Tax Hub architecture.

If Objective 4 exposes a genuine earlier-objective defect, identify the smallest prerequisite correction and explain why it is required.

Do not use Objective 4 as an opportunity for general cleanup.

---

# 15. Sole authorised output

The only file this session may create or modify is:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

Do not modify:

- source;
- tests;
- project files;
- `hmrc-transport.md`;
- programme specifications;
- findings/change-log;
- previous implementation/work-plan records; or
- `.local` historical material.

Do not commit changes.

---

# Completion condition

The work plan is complete when it provides a repository-grounded, phased and testable route from the completed Objective 3 implementation to the completed Objective 4 HMRC Transport Platform.

It must make clear:

- which narrow boundary corrections happen first;
- which existing types change and why;
- what new transport capabilities are introduced in each phase;
- what remains unchanged;
- how every phase proves it has preserved Objective 3 semantic outputs;
- which Corporation Tax work is blocked by submission-readiness evidence;
- which REST work can proceed independently;
- and where Codex must stop for human review before continuing.

Do not implement the plan.

Produce the plan and stop.
