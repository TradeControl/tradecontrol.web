# Tax Hub Objective 4 — Revise Work Plan 5

Update:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

This is an authorised documentation edit to the Work Plan only.

Do not modify source code, tests, project files, specifications, `findings.md`, `change-log.md`, or any other document.

Do not implement any phase.

Use the repository and research context already available in this Work session to make the Work Plan internally consistent.

## Required revision

The existing Work Plan is technically detailed and should be preserved wherever possible, but its product priorities and Objective 4 scope need correcting.

The required delivery priority is:

1. common transport foundations;
2. VAT;
3. VAT HMRC recognition/production readiness;
4. Corporation Tax;
5. Corporation Tax HMRC recognition/production readiness;
6. Company Accounts / Companies House submission;
7. limited-company completion gate;
8. MTD Income Tax / Self Assessment.

The remaining SA work must come last.

---

## 1. Preserve the common foundations

Retain the existing Objective 3 → Objective 4 boundary corrections and common transport infrastructure wherever they remain valid.

In particular preserve the existing work concerning:

- exact prepared-byte invariants;
- REST contract metadata;
- trusted configuration;
- secret boundaries;
- durable attempt/audit handling;
- tenant/principal isolation;
- OAuth/token lifecycle;
- fraud-prevention headers;
- response/error classification;
- retry/ambiguity handling;
- fail-closed preview/submission-readiness gates;
- regression/golden protection.

Do not redesign these simply because the product sequence is changing.

---

## 2. VAT is the first complete product milestone

Keep VAT ahead of the other tax families.

The VAT sequence must result in a complete usable transport capability covering:

- OAuth;
- fraud-prevention headers;
- VAT enquiries;
- VAT return submission;
- durable attempts/audit;
- authority responses/errors;
- ambiguous-write protection;
- sandbox evidence; and
- HMRC recognition/production readiness.

Add an explicit VAT recognition/production-readiness phase or gate after the existing VAT submission work.

HMRC recognition is a planned Objective 4 deliverable, not merely a note attached to eventual production deployment.

The intended outcome is that Trade Control can pursue the applicable HMRC process for recognition/listing as compatible VAT software without waiting for Corporation Tax, Company Accounts or SA to be completed.

---

## 3. Corporation Tax follows VAT

Move the Corporation Tax stream immediately after the VAT recognition milestone.

Preserve the existing detailed CT work concerning:

- correction of the current diagnostic `Transmission` boundary;
- Transaction Engine conversation metadata;
- Objective 3 CT service-artifact assurance;
- official RIM/taxonomy/XSD/Schematron validation;
- GovTalk;
- IRmark;
- acknowledgement/polling/recovery/delete;
- durable terminal responses and receipts;
- test-service/recognition evidence; and
- fail-closed submission-readiness gates.

Take Corporation Tax through an explicit HMRC recognition/production-readiness milestone before moving to the Company Accounts submission phase.

Do not put MTD Income Tax between VAT and Corporation Tax merely because MTD Income Tax reuses the REST infrastructure.

---

## 4. Include Company Accounts / Companies House submission in Objective 4

The present Work Plan explicitly excludes Companies House transport.

Remove that exclusion.

Objective 4 is to provide the external statutory submission pipeline required by the initial Trade Control product. For limited companies that pipeline is incomplete without Company Accounts submission to Companies House.

Use the existing repository contracts, preparation artifacts, polling semantics and company-account infrastructure as the starting point.

Add the necessary Objective 4 phases to transport an approved immutable Objective 3 Company Accounts artifact through the real Companies House submission process.

The Work Plan must cover, as applicable from the existing repository and authoritative Companies House requirements:

- submission eligibility/readiness;
- presenter/authentication requirements;
- production configuration and secrets;
- submission protocol;
- exact prepared artifact transmission;
- acknowledgement;
- status polling;
- terminal success/rejection;
- error handling;
- retry/recovery behaviour;
- durable submission/audit state;
- receipt/reference preservation;
- sandbox/test or equivalent external validation;
- presenter/software approval or production-readiness requirements.

Preserve the same architectural boundary used elsewhere:

**Objective 3 prepares and validates the statutory artifact. Objective 4 transports that approved artifact and records the authority outcome.**

Objective 4 must not regenerate or reinterpret the accounting meaning of Company Accounts.

Do not force Companies House through the HMRC REST or Transaction Engine implementation if its protocol differs materially.

Reuse common Objective 4 infrastructure only where the semantics genuinely match.

If the governing programme specification currently excludes Companies House transport from Objective 4, update Work Plan 5 to identify the required programme-spec correction explicitly as a prerequisite/documentation action. Do not silently pretend the contradiction does not exist.

Do not modify the programme specification in this session.

---

## 5. Limited-company completion milestone

After VAT, Corporation Tax and Company Accounts submission are complete, add an explicit limited-company completion milestone.

At this point the Objective 4 transport layer should provide the external statutory submission capabilities needed by the initial limited-company Trade Control product:

- VAT submission operational and HMRC recognition/production-ready;
- Corporation Tax submission operational and HMRC recognition/production-ready;
- Company Accounts submission operational and Companies House production-ready/approved as applicable;
- durable authority outcomes exposed through the Application boundary;
- all unsupported, preview and incomplete paths fail closed.

This is the principal initial product milestone for Objective 4.

---

## 6. Add a human programme exit gate

Immediately after the limited-company completion milestone, add a formal human decision gate.

The decision is:

### Continue Objective 4

Proceed with the remaining MTD Income Tax / Self Assessment transport work.

or:

### Proceed to Objective 5

Defer the remaining SA/MTD Income Tax work and proceed directly to Objective 5 — ASP.NET Core Tax Hub integration.

This is an intentional programme checkpoint.

The likely initial Trade Control adopters are limited companies, and completing the hosted/UI product may have greater value at that point than immediately completing the remaining sole-trader transport.

The Work Plan must therefore permit Objective 5 to begin after the limited-company milestone without deleting or pretending that the remaining SA work has been completed.

---

## 7. Move MTD Income Tax / Self Assessment to the end

Move the existing MTD Income Tax phase(s) behind the limited-company completion and exit gate.

Preserve the existing technical content wherever still valid, including:

- reuse of the REST/OAuth/fraud infrastructure established for VAT;
- immutable MIN/STD prepared bytes and golden digests;
- endpoint contract metadata;
- `204` success handling;
- retry/concurrency/restart behaviour;
- obligation-backed period rules;
- sandbox evidence; and
- current HMRC production-access caveats.

Do not redesign the SA contracts because their implementation priority has changed.

If the human exit decision is to defer SA, mark these phases as planned/deferred work that can be resumed later.

---

## 8. Findings and change log

Retain the Work Plan's use of:

`docs\projects\Tax Hub\findings.md`

and:

`docs\projects\Tax Hub\change-log.md`

These are to resume as forward-going project records during implementation.

Older entries may remain historical and need not be rewritten.

Use:

- `findings.md` for durable discoveries, decisions and external evidence;
- `change-log.md` for significant implemented changes;
- Work Plan 5 for phase status, acceptance and execution evidence.

Do not edit either file during this Work Plan revision.

---

## 9. Renumber and reconcile the plan

Reorder and renumber the phases so that the document reads naturally in execution order.

Update all consequential references, including:

- phase graph;
- phase dependencies;
- review gates;
- references such as “before Phase 5.x”;
- integration-test gates;
- cross-phase verification;
- human decision points;
- operational hardening;
- Objective 5 handoff; and
- completion condition.

Do not retain obsolete phase numbers merely to minimise textual edits.

The revised Work Plan should make the execution sequence obvious without requiring the reader to reconstruct it from dependency notes.

---

## 10. Completion semantics

Revise the current completion condition.

The plan must distinguish between:

### Limited-company Objective 4 milestone

VAT + Corporation Tax + Company Accounts submission capabilities are complete and appropriately recognition/production-ready, allowing a human decision to proceed to Objective 5.

and:

### Full Objective 4 completion

The above plus the planned MTD Income Tax / Self Assessment transport capability.

If SA is deliberately deferred at the programme exit gate, record Objective 4 as partially complete/deferred rather than falsely complete.

---

## 11. Preserve the quality of the existing plan

Do not replace Work Plan 5 with a shorter generic plan.

Its detailed repository-specific implementation guidance, tests, exclusions, acceptance criteria and review gates are valuable for later Codex implementation.

Modify and extend the existing document rather than regenerating it from first principles.

Where new Companies House phases require repository or authoritative protocol facts, use the evidence already available to this repo-aware Work session and perform any necessary authoritative research needed to make those phases implementation-ready.

Do not introduce speculative details where the evidence does not support them; identify any genuinely unresolved implementation prerequisite explicitly.

---

## Authorised output

Modify only:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

Do not implement anything.

Do not modify any other file.

When the revised Work Plan is complete, report:

- the new phase sequence;
- the principal changes made;
- any programme-spec inconsistency identified for later correction; and
- any unresolved Companies House prerequisite that remains before implementation.

Then stop.
