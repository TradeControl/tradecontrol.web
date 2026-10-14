# Tax Hub — Objective 4 HMRC Transport Reconnaissance

22 September 2026

## Session purpose

Prepare the technical foundation for **Objective 4 — HMRC Transport Platform**.

Objective 3 is complete. Its responsibility ends with preparation of the exact HMRC request/package artefacts and their handoff through the defined gateway ports. Objective 4 must consume those outputs and provide the machinery required to transmit them to HMRC and process HMRC's responses.

This session is **reconnaissance and design research only**.

Do not implement Objective 4.

Do not modify application source code.

Do not refactor existing code.

The sole permitted project write is the report specified under **Deliverable** below.

---

## Governing specification

The top-level programme specification is:

`docs\projects\Tax Hub\specs\tax-hub-spec-programme.md`

Treat this as the governing project specification.

Read the relevant supporting material beneath:

`docs\projects\Tax Hub`

in particular the completed Objective 3 specifications, findings, implementation records and any material defining the HMRC contracts and gateway boundary.

Do not rely on superseded material where later project documentation has replaced it.

---

## Current implementation

The current Tax Hub implementation is beneath:

`src\tax-hub\src`

Inspect this code thoroughly.

Objective 3 has established the HMRC payload/contracts and the boundary through which prepared artefacts are handed to Objective 4.

Identify the **actual types, interfaces, gateway ports, prepared artefacts and call paths present in the repository**. Do not infer an alternative interface merely from the programme specification.

A fundamental architectural constraint is:

**Objective 4 must not reinterpret accounting or tax meaning already established by earlier objectives.**

In particular, where Objective 3 has produced an exact prepared request or package for transmission, Objective 4 must treat that artefact as immutable transport input. Do not propose reconstructing, normalising, deserialising/re-serialising or otherwise altering it unless an existing contract explicitly requires such behaviour.

---

## Historical working reference

A historical VAT MTD test application has been placed at:

`.local\vat_mtd_client_test-master`

This directory is intentionally excluded from Git.

Treat it as **read-only historical/reference evidence**, not as application source and not as a design template.

The application dates from approximately 2020 and targets .NET 5. It extended HMRC's original .NET API example to exercise VAT MTD, including OAuth and fraud-prevention headers, and was intended for deployment to Azure App Service.

### Verification performed on 22 September 2026

The historical application was run locally against the current HMRC sandbox using a newly-created HMRC developer application and newly-generated organisation test user.

After correcting the historical ASP.NET middleware ordering so that authentication precedes authorization, the following were demonstrated successfully:

1. OAuth authorisation through the current HMRC sandbox.
2. Receipt and recovery of an OAuth access token.
3. Authenticated retrieval of VAT obligations.
4. Submission of a VAT return.
5. HMRC accepted the submitted VAT return and returned **HTTP 201 CREATED**.
6. The application successfully invoked HMRC's fraud-prevention-header validation service.

The fraud-prevention-header validator identified itself as **specVersion 3.3**.

The validator reported two invalid headers:

- `Gov-Client-Public-IP` — the locally-running application supplied `::1`, which is not a public IP address.
- `Gov-Vendor-Forwarded` — consequently contained the same non-public client address.

It also reported warnings concerning:

- empty `Gov-Client-Multi-Factor`;
- empty `Gov-Vendor-License-IDs`;
- potentially incorrect percent encoding in `Gov-Vendor-Forwarded`.

The historical application contains a fixed public IP address which appears to relate to its former Azure deployment. Its README confirms Azure App Service was the intended deployment environment.

These observations establish that the historical application is still useful evidence for the fundamental HMRC REST/OAuth transport flow. They **do not establish that its implementation is suitable for reuse unchanged**.

Assess it accordingly.

Do not expose, reproduce or copy credentials, secrets or test-user passwords found beneath `.local`.

---

## Authoritative external evidence

Research the **current HMRC Developer Hub documentation** relevant to Objective 4.

Prefer authoritative HMRC sources over third-party descriptions.

Establish the current requirements rather than assuming that either:

- the historical VAT implementation; or
- the present Tax Hub code

is correct.

Record source URLs and, where useful, document/version dates so that conclusions can subsequently be verified.

At minimum investigate the current requirements applicable to:

- HMRC environments and base endpoints;
- OAuth and user-restricted endpoints;
- authorisation scopes;
- access-token handling and lifecycle;
- fraud-prevention headers;
- REST request requirements;
- HTTP/content negotiation and API versioning;
- HMRC error and response handling;
- retry behaviour and transient failures;
- correlation/request identifiers where applicable;
- audit information that should be retained around submissions;
- VAT MTD transport;
- MTD Income Tax transport required by the Tax Hub programme;
- Corporation Tax/iXBRL submission transport;
- XML canonicalisation, IRmark and Transaction Engine submission where required by the programme;
- acknowledgement/receipt handling.

Do not expand Objective 4 into accounting, tax classification, statutory projection or payload-generation responsibilities owned by earlier objectives.

---

## Reconnaissance questions

The report should answer the following from repository evidence and current HMRC documentation.

### 1. What exactly does Objective 3 hand to Objective 4?

Trace the concrete implementation.

Identify:

- relevant projects/namespaces;
- interfaces and gateway ports;
- request/package types;
- immutable/prepared representations;
- metadata supplied alongside them;
- existing environment or endpoint information;
- expected result/response contracts;
- current call sites.

Where useful, cite file paths, type names and method names.

### 2. What HMRC transport families are actually required?

Determine which Objective 3 outputs use:

- REST/JSON APIs;
- XML/Transaction Engine submission;
- iXBRL or other packaged artefacts.

Do not assume all HMRC submissions should pass through an artificially uniform protocol.

Identify genuine common infrastructure separately from protocol-specific machinery.

### 3. What can be learned from the historical VAT application?

Compare `.local\vat_mtd_client_test-master` with current HMRC requirements.

Classify its relevant concepts as:

- still valid;
- valid in principle but requiring modern implementation;
- obsolete;
- incomplete;
- deployment-specific;
- unsafe or unsuitable for production.

Pay particular attention to OAuth, authentication-ticket/token handling, endpoint construction, API version headers, fraud-prevention headers, HTTP client usage, environment configuration and response handling.

Do not recommend copying historical code merely because today's sandbox test succeeded.

### 4. What transport infrastructure already exists?

Search the current codebase before proposing new abstractions.

Identify anything already implementing or anticipating:

- HMRC gateways;
- HTTP clients;
- authentication;
- configuration/environment selection;
- request dispatch;
- response mapping;
- retry;
- logging;
- audit;
- durable submission state;
- receipts or acknowledgements.

Distinguish implemented behaviour from placeholders and design intent.

### 5. What is missing for Objective 4?

Produce a gap analysis between:

**completed Objective 3 outputs → current repository → current HMRC requirements → Objective 4 programme requirements.**

For each significant gap, identify why it belongs to Objective 4 rather than another programme objective.

### 6. What should the implementation shape be?

Without writing code, propose a concrete implementation guide.

Prefer the smallest architecture that satisfies the established contracts.

Identify:

- components/responsibilities;
- likely projects/namespaces and file locations;
- interfaces that already exist and should be implemented;
- any new interfaces/types genuinely required;
- shared transport infrastructure;
- REST-specific infrastructure;
- XML/Transaction Engine-specific infrastructure;
- configuration/secrets requirements;
- persistence/audit requirements;
- testing strategy;
- sensible implementation phases and acceptance criteria.

Where possible, express the guide in terms of existing repository types rather than hypothetical replacements.

---

## Architectural constraints

Preserve the Tax Hub layering established by the programme specification.

Objective 4 owns **transport**, not tax meaning.

Do not:

- change Objective 3 payload semantics;
- regenerate already-prepared payloads;
- introduce accounting or tax calculations;
- redesign completed earlier objectives without evidence of a genuine incompatibility;
- introduce generic infrastructure merely because it might be useful later;
- assume the old VAT prototype defines the target architecture;
- expose secrets or credentials;
- modify production/application code during this reconnaissance.

If current HMRC requirements conflict with an existing Objective 3 contract, **report the conflict explicitly rather than silently correcting either side**.

Separate verified facts from recommendations.

Where evidence is uncertain or HMRC documentation is ambiguous, say so.

---

## Deliverable

Create:

`docs\projects\Tax Hub\specs\reference\hmrc-transport.md`

This is a **reference/reconnaissance document**, not an implementation commit.

It should contain, at minimum:

1. Executive summary.
2. Evidence and sources examined.
3. Current Objective 3 → Objective 4 boundary.
4. Required HMRC transport families.
5. Current HMRC transport requirements.
6. Historical VAT implementation assessment.
7. Current repository transport capabilities.
8. Gap analysis.
9. Proposed Objective 4 implementation architecture.
10. File/type-level implementation map.
11. Proposed implementation phases.
12. Acceptance/test strategy.
13. Risks, uncertainties and decisions requiring confirmation.
14. Authoritative HMRC references.

Be specific enough that the report can subsequently be reviewed and converted into a controlled Objective 4 implementation work plan for Codex.

Do not implement that plan during this session.

---

## Completion condition

The reconnaissance is complete when we can answer, with repository and authoritative external evidence:

**Precisely what must be built between the completed Objective 3 gateway outputs and HMRC, where should it live, what existing code/contracts should it use, and how can each transport path be proven correct without changing the meaning or bytes prepared by Objective 3?**

**End of Session Brief**
