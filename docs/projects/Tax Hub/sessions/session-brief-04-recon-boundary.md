# Tax Hub — Objective 4 Boundary Resolution

22 September 2026

## Session purpose

Perform a focused follow-up to the completed Objective 4 HMRC Transport Platform reconnaissance.

The previous reconnaissance is recorded at:

`docs\projects\Tax Hub\specs\reference\hmrc-transport.md`

Treat that document as **research and proposed implementation-design evidence, not an approved implementation specification**.

Its executive summary identifies four apparent conflicts at the completed Objective 3 → Objective 4 boundary. This session must independently verify those findings against the current repository, the governing Tax Hub documentation and, where necessary, current authoritative HMRC documentation.

The purpose is to determine **exactly how the boundary should be corrected before Objective 4 implementation begins**.

This is reconnaissance and design work only.

Do not implement Objective 4.

Do not modify application source code.

Do not commit or refactor code.

---

## Governing material

The programme specification is:

`docs\projects\Tax Hub\specs\tax-hub-spec-programme.md`

Supporting project documentation is beneath:

`docs\projects\Tax Hub`

The current implementation, including the completed Objective 3 contracts and payload preparation, is beneath:

`src\tax-hub\src`

Tests are beneath:

`src\tax-hub\tests`

The previous Objective 4 reconnaissance is:

`docs\projects\Tax Hub\specs\reference\hmrc-transport.md`

The historical working VAT MTD application remains available as read-only evidence at:

`.local\vat_mtd_client_test-master`

Do not expose or reproduce credentials or secrets found beneath `.local`.

---

## Architectural invariant

Objective 3 owns the semantic preparation of HMRC requests/packages.

Objective 4 owns authority authentication, transport, transport-specific protocol machinery, responses, retry/recovery and durable transport audit.

Where Objective 3 has prepared canonical JSON or statutory document bytes, Objective 4 must not reconstruct, reinterpret, deserialize/re-serialize, re-round, rename or otherwise alter their tax/accounting meaning.

At the same time, Objective 4 must receive enough contract metadata to transport and interpret those artefacts correctly without maintaining a second, divergent copy of Objective 3's endpoint catalogue.

The purpose of this session is to determine the smallest clean boundary satisfying both requirements.

---

# Primary investigation

The previous reconnaissance identified four apparent boundary conflicts.

Verify each independently.

Do not assume the proposed remedy in `hmrc-transport.md` is correct merely because the underlying problem is real.

## Conflict 1 — REST dispatch metadata

The reconnaissance reports that `PreparedApiRequest` does not carry all metadata Objective 4 requires, while the underlying VAT and MTD Income Tax descriptors already know some or all of:

- required OAuth scope;
- accepted success status;
- response/body contract.

Investigate the concrete descriptor → preparation → gateway path.

Determine:

1. Exactly what metadata exists today.
2. Where each item currently lives.
3. Which metadata Objective 4 genuinely requires at dispatch time.
4. Which metadata Objective 4 requires when interpreting the authority response.
5. Whether that information should:
   - become immutable metadata on the prepared request;
   - be represented by another existing contract;
   - be resolved through an existing catalogue without duplication; or
   - cross the gateway by some other narrow mechanism.
6. Whether VAT descriptors themselves are incomplete and require authoritative contract correction.
7. Whether any proposed change alters Objective 3 semantics or only makes already-known contract metadata explicit.

Prefer the smallest solution.

Do not create a generic metadata framework unless repository evidence requires one.

### Required conclusion

Specify the exact type/file-level boundary change, if any, required before REST transport can safely be implemented.

---

## Conflict 2 — Corporation Tax `Transmission` bytes

The reconnaissance reports an apparent contradiction:

- `CorporationTaxPackageSerializer` describes its output as diagnostic and “not a gateway payload”; but
- `CorporationTaxPreparer` places those bytes into `PreparedSubmissionPackage.Transmission`.

Trace this path from source data through preparation to the gateway.

Determine exactly what each current artifact represents:

- accounts iXBRL;
- computation iXBRL;
- CT XML;
- `Transmission`;
- package documents;
- preview/submission-ready status.

Then reconcile this with the current HMRC Corporation Tax submission protocol.

In particular establish the correct ownership boundary between:

**Objective 3 statutory/service artifacts**

and

**Objective 4 GovTalk/Transaction Engine transport envelope and IRmark machinery.**

Do not simply rename diagnostic bytes as submission-ready.

Do not move statutory/tax meaning into Objective 4.

### Required conclusion

Define precisely what immutable artifact or artifacts Objective 3 should eventually hand to Objective 4 for Corporation Tax and what Objective 4 is permitted to construct around or within them for transport.

If current official Corporation Tax assets are insufficient to make that boundary final, state exactly what remains unknown and what evidence is required.

---

## Conflict 3 — Corporation Tax polling semantics

The reconnaissance reports that the repository currently declares:

`SubmissionPollingMode.None`

and:

`RequiresStatusPolling = false`

for Corporation Tax, while current Transaction Engine evidence requires an acknowledgement containing a correlation ID followed by polling until a terminal response and subsequent deletion.

Verify this against:

- current repository types and tests;
- programme documentation;
- current authoritative HMRC Transaction Engine documentation.

Determine whether the existing `PreparedPollingSemantics` abstraction is conceptually appropriate for Transaction Engine at all.

Do not force Transaction Engine into a REST-shaped polling abstraction merely to preserve an existing type.

Conversely, do not replace the abstraction if a narrow correction accurately expresses the protocol.

### Required conclusion

Specify the smallest correct contract change required to represent the Corporation Tax submission conversation without leaking transport implementation into Objective 3.

---

## Conflict 4 — Corporation Tax preview/submission readiness

The reconnaissance reports that current Corporation Tax/company artifacts remain `Preview` because official validation/taxonomy assets required for submission assurance are absent.

Verify this against the actual preparers, serializers, findings and tests.

Identify:

- which artifacts are currently preview-only;
- why;
- exactly which authoritative assets/evidence are missing;
- whether those missing assets belong to Objective 3 contract assurance, Objective 4 transport work, or both;
- what must become true before Objective 4 may legally/architecturally transmit a Corporation Tax package.

The correct answer may be that Corporation Tax transport can be designed and tested independently while actual live/sandbox submission remains gated.

### Required conclusion

Define the explicit submission-readiness gate and identify which objective owns satisfying each prerequisite.

---

# Secondary design questions

After resolving the four conflicts, review the main design recommendations in `hmrc-transport.md`.

These are **not yet approved architecture**.

## Dispatch context

The reconnaissance proposes a typed `AuthorityDispatchContext` carrying information such as tenant, authority principal, approval and fraud-prevention context.

Determine what Objective 4 genuinely requires that cannot safely be inferred from `PreparedApiRequest` or `PreparedSubmissionPackage`.

Keep tax payload and transport/session identity separate.

Recommend the smallest explicit context required.

## Fraud-prevention ownership

The reconnaissance suggests that browser/device/network facts must originate from the initiating web request and may therefore involve Objective 5.

Clarify responsibility.

Distinguish:

- who collects the raw client facts;
- who validates/seals them;
- who formats HMRC fraud-prevention headers;
- who owns trusted proxy/network topology;
- who persists compliance evidence;
- what Objective 5 merely orchestrates.

Avoid moving transport responsibility into the UI/application workflow merely because some evidence originates there.

## Durable attempt model

Assess whether durable attempt state is genuinely required for the first Objective 4 implementation and, if so, establish the minimum state needed for:

- REST enquiries;
- VAT POST ambiguity;
- MTD Income Tax writes;
- Transaction Engine acknowledgement/poll/recovery/delete.

Do not design an enterprise workflow engine.

## Component granularity

Review the component list proposed in sections 9 and 10 of `hmrc-transport.md`.

Consolidate components where separation provides no concrete benefit.

Retain separation where protocol, security, persistence or testability genuinely requires it.

The goal is the **smallest understandable Objective 4 implementation**, not maximum abstraction.

---

# Evidence rules

Use repository evidence first.

Where an answer depends upon current HMRC behaviour or protocol requirements, verify it against authoritative HMRC/GOV.UK documentation current at the date of this session.

Record source URLs and relevant publication/update/version information.

Clearly distinguish:

- verified repository fact;
- verified HMRC requirement;
- inference;
- recommendation;
- unresolved question.

If current code and HMRC requirements conflict, report the conflict.

Do not silently modify the interpretation of earlier objectives to make the conflict disappear.

---

# Deliverable

Update:

`docs\projects\Tax Hub\specs\reference\hmrc-transport.md`

Do not replace the useful reconnaissance already present.

Add a clearly identified follow-up section:

# Boundary Resolution Review

The section should contain:

1. **Verdict on each of the four boundary conflicts**
   - confirmed, partially confirmed or rejected;
   - repository evidence;
   - external protocol evidence where applicable;
   - exact recommended resolution.

2. **Approved candidate Objective 3 → Objective 4 boundary**
   - REST request;
   - REST dispatch context;
   - REST outcome;
   - Corporation Tax statutory/service package;
   - Transaction Engine outcome/conversation.

3. **Minimal type/file changes required before transport implementation**
   - existing type;
   - proposed change;
   - reason;
   - confirmation that prepared semantic bytes remain unchanged.

4. **Review of the previous architectural recommendations**
   - retain;
   - simplify;
   - defer;
   - reject.

5. **Revised implementation sequence**
   - particularly whether a small boundary-correction phase is required before REST transport begins.

6. **Decisions requiring human approval**
   - keep these few and concrete.

Do not implement any recommendation.

---

# Completion condition

The session is complete when a subsequent Codex implementation brief can state unambiguously:

**what crosses the Objective 3 → Objective 4 boundary, what Objective 4 may add, what Objective 4 must never alter, what result comes back across the boundary, and which minimal contract corrections must be completed before the first real HMRC transport adapter is written.**

The desired outcome is not a larger architecture. It is a precise, minimal and evidence-backed resolution of the remaining Objective 3 → Objective 4 boundary issues, sufficient to authorise the first controlled Objective 4 implementation phase.

## Note

Keep the Boundary Resolution Review focused. Do not repeat or restate material already adequately covered by the existing reconnaissance. Amend earlier conclusions only where this review changes them. Target approximately 2,000–3,000 words.

**End of session brief**
