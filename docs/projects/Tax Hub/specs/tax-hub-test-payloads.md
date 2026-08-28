# Tax Hub — Test Harness Specification

**Trade Control Tax Hub Programme**  
**Development and Verification Infrastructure**  
**28 August 2026**

## 1. Purpose

The Tax Hub Test Harness is a **development, diagnostic and verification tool**.

Its purpose is to allow developers and development agents to exercise the real Tax Hub implementation, inspect data travelling through it, and verify behaviour at useful points in the submission pipeline.

The Test Harness is **not part of the HMRC integration architecture**.

It does not define:

- a statutory reporting model;
- a Tax Tag vocabulary;
- an alternative submission model;
- an HMRC payload schema;
- an HMRC simulator;
- a production API contract;
- or a transport protocol.

The Test Harness exists to make the real implementation observable and testable during development.

---

## 2. Governing Principle

> **The Test Harness exists to expose and exercise the real Tax Hub implementation during development. It does not define an alternative submission model, statutory vocabulary, canonical payload representation, or production integration API.**

Where practical, harness endpoints shall invoke the same production components that the Tax Hub itself uses.

The harness shall expose what those components actually produce rather than constructing a parallel representation of what they are expected to produce.

---

## 3. Development-Agent Usage

The Test Harness is explicitly intended for use by development agents such as Codex.

Development agents are encouraged to use the Test Harness for:

- reconnaissance;
- information gathering;
- exercising application components;
- inspecting intermediate data;
- verifying mappings;
- inspecting serialization;
- regression testing;
- HMRC Sandbox testing;
- and end-to-end verification.

A development agent may call existing harness endpoints and inspect their responses as part of its authorised work.

A development agent may also add, modify or remove harness endpoints where doing so materially improves observability, diagnosis or verification.

This standing permission applies to **Test Harness infrastructure only**. It does not authorise changes to production behaviour outside the write scope of the current development phase.

If obtaining an observation requires a change to production logic, normal programme approval and write-scope rules continue to apply.

---

## 4. Architectural Position

The production flow remains:

    Operational Transactions
            ↓
    Accounting Engine
            ↓
    Tax Classification Layer
            ↓
    Statutory Projection / Tax Tags
            ↓
    Submission Logic
            ↓
    HMRC Contract Adapter
            ↓
    HMRC Transport
            ↓
    HMRC
            ↓
    Response / Submission History

The Test Harness sits **outside this flow**.

It may observe or exercise useful points within the flow, for example:

                        Test Harness
                      ↙      ↓       ↘
              SQL Projection   HMRC Payload   Transport
                    ↓               ↓             ↓
                  JSON          Serialized      Sandbox
                 output           payload       response

No production component shall depend upon the Test Harness.

Removing the Test Harness must not break the production HMRC integration.

---

## 5. Observation Points

Harness endpoints may be provided wherever they are useful during development.

Likely observation points include, but are not limited to:

### 5.1 Accounting and SQL Output

The harness may expose data produced by authoritative SQL views or other accounting projections.

This allows developers to inspect the source information before it is transformed into statutory or HMRC representations.

### 5.2 Statutory Projection

The harness may expose the Tax Tags or other Objective 2 statutory projection produced from accounting data.

This allows mappings, omissions, classifications and values to be inspected directly.

### 5.3 HMRC Contract Generation

The harness may invoke the real Objective 3 payload generator and return the resulting HMRC representation.

This may include the exact serialized JSON or XML that would be presented to the transport layer.

The harness shall not create a separate "harness version" of an HMRC payload.

### 5.4 HMRC Transport

Where useful, the harness may expose requests entering the Objective 4 transport layer and responses returned from it.

### 5.5 HMRC Sandbox

The harness may be used to exercise HMRC Sandbox endpoints and expose the actual responses returned by HMRC.

Sandbox responses should be preserved as faithfully as practical so that unexpected behaviour remains visible.

---

## 6. Faithful Observation

The harness shall prefer **actual component output** over normalized or reconstructed representations.

In particular, the harness must not manufacture apparently complete data merely to satisfy a harness-specific schema.

Missing information shall remain missing unless the production component being exercised legitimately supplies a value.

The harness must not:

- substitute zero for an absent monetary value merely for structural completeness;
- invent Tax Tags;
- manufacture HMRC fields;
- silently repair mappings;
- normalize an erroneous production payload into a correct-looking harness payload;
- or conceal serialization or contract defects.

If the real component produces incorrect output, the purpose of the harness is to make that defect visible.

---

## 7. Harness Responses

Harness responses should be as simple as practical.

Where useful, a response may contain minimal diagnostic metadata such as:

- observation stage;
- timestamp;
- requested operation;
- source or Tax Source;
- correlation identifier;
- HTTP status;
- content type;
- and diagnostic information.

The substantive data should remain the real output of the component being inspected wherever practical.

Diagnostic wrapping must not alter the semantics of that output.

---

## 8. Serialization

Serialization is itself an important observable result.

When testing an HMRC contract, the harness should expose the **actual serialized representation produced by the production serializer**.

This allows development agents to detect issues including:

- incorrect property names;
- incorrect casing;
- unwanted zero values;
- missing properties;
- unexpected properties;
- incorrect nesting;
- incorrect data types;
- XML structure errors;
- and other contract discrepancies.

The harness must not independently reserialize an alternative model merely to produce cleaner diagnostic output.

---

## 9. Extensibility

The Test Harness is intentionally extensible.

Development agents may introduce diagnostic or exercise endpoints when this materially assists implementation or verification.

New endpoints should be narrowly targeted at the question being investigated.

A harness endpoint does not require a permanent architectural justification merely because it is useful during development.

Temporary endpoints may be removed when their purpose has been served.

The preferred principle is:

> **Add the visibility needed to prove the implementation; do not build infrastructure merely to preserve the visibility mechanism.**

---

## 10. Disposable Infrastructure

Harness endpoints are development infrastructure.

Their existence does not create a compatibility obligation.

An endpoint may be:

- introduced for a particular development phase;
- changed as the implementation evolves;
- replaced by a more useful observation point;
- or removed when no longer required.

A harness endpoint becomes a supported product contract only through an explicit architectural decision.

Production code must not acquire dependencies on temporary harness DTOs, routes, conventions or response formats.

---

## 11. Repository and Contract Boundaries

The `hmrc_mtd` repository contains HMRC integration code, but the existence of historical classes within that repository does not establish that those classes represent valid HMRC contracts.

During contract-alignment reconnaissance, relevant classes shall be classified according to their actual responsibility, including:

1. **HMRC Contract** — represents a verified external HMRC request, response or related contract.
2. **Trade Control Statutory Projection** — represents internal Objective 2 statutory information.
3. **Harness / Development Infrastructure** — exists solely to exercise or observe the implementation.
4. **Obsolete / Legacy** — represents superseded workflows, contracts or abandoned implementation.

Internal harness representations must not masquerade as HMRC contracts merely because they currently reside in the `hmrc_mtd` repository.

The appropriate namespace and repository disposition of existing classes shall be proposed from reconnaissance evidence rather than inferred from their current location.

---

## 12. Existing Harness Endpoints

Existing Test Harness endpoints are **not protected behaviour**.

They were developed while the purpose of the Test Harness was being interpreted differently and therefore must not be treated as architectural authority.

In particular, existing Self Assessment QU and EOPS harness endpoints may be removed rather than preserved or repaired if they implement obsolete harness-specific payload models.

Legacy EOPS behaviour must not be retained merely to maintain Test Harness compatibility.

The existing VAT-MTD harness endpoint may be retained provisionally where reconnaissance confirms that it provides a useful example of exercising the real implementation.

Its existence does not make its present design automatically canonical.

---

## 13. Relationship to Objective 2

Objective 2 defines Trade Control's internal statutory projection and submission logic.

The Test Harness may expose Objective 2 data, but **does not define Objective 2 data**.

Tax Tags and statutory field sets are governed by the relevant Objective 2 specifications.

No Tax Tag becomes canonical because it appears in a harness builder or response.

The Test Harness therefore requires no independent canonical QU, annual, VAT or Corporation Tax tag inventory.

---

## 14. Relationship to Objective 3

Objective 3 defines verified HMRC endpoints, versions and exact external payload contracts.

The Test Harness may exercise Objective 3 and expose its generated payloads.

It must not independently define those payloads.

Where a harness observation conflicts with the verified Objective 3 contract, the conflict is evidence of an implementation defect; the harness representation is not authority.

---

## 15. Relationship to Objective 4

Objective 4 implements HMRC transport.

The Test Harness may exercise that transport and expose useful request and response information, including HMRC Sandbox interactions.

The Test Harness does not implement an alternative transport stack.

OAuth, fraud-prevention headers, REST/JSON handling, XML transport, canonicalisation, IRmark handling and related production responsibilities remain Objective 4 concerns.

---

## 16. Automated Verification

The Test Harness should be usable from automated development tooling wherever practical.

A development agent should be able to:

    submit known request
            ↓
    exercise real component
            ↓
    receive observable output
            ↓
    compare against governing specification
            ↓
    identify discrepancy
            ↓
    correct authorised implementation
            ↓
    repeat request

This capability is particularly valuable for iterative implementation because it allows correctness to be demonstrated from actual runtime behaviour rather than inferred solely from source inspection or successful compilation.

Where stable test cases prove useful, they may subsequently be promoted into formal automated tests.

The Test Harness itself does not replace unit, integration or contract testing.

---

## 17. Safety and Isolation

Harness functionality must remain development-oriented and appropriately isolated.

It must not create an unintended production submission route.

Where endpoints can initiate external HMRC Sandbox communication or other consequential operations, their purpose and target must be explicit.

Test Harness functionality must respect normal tenant isolation and must not expose credentials, authentication secrets or other sensitive transport information merely for diagnostic convenience.

---

## 18. Implementation Guidance

When adding a harness endpoint, the preferred order is:

1. identify the production component or boundary that needs to be observed;
2. call that real component where practical;
3. expose its input or output with minimal transformation;
4. add only the diagnostic metadata needed to interpret the result;
5. use the endpoint to verify the implementation;
6. retain, simplify or remove the endpoint according to continuing development value.

Do not create a new domain model simply because a diagnostic endpoint needs to return JSON.

Do not move production business logic into the Test Harness.

---

## 19. Authority

The Test Harness is an **observer and exerciser**, not an authority.

Authority remains with:

- Trade Control accounting specifications for accounting behaviour;
- approved Tax Hub statutory projection specifications for Objective 2;
- current authoritative HMRC specifications for Objective 3;
- approved transport specifications for Objective 4;
- and the governing Tax Hub Programme Specification for architectural boundaries.

Historical harness code and historical harness documentation are evidence of previous implementation only.

---

## 20. Definition of Correctness

The Test Harness satisfies this specification when:

- it can expose useful points in the real Tax Hub data flow;
- development agents can use it for reconnaissance and verification;
- diagnostic endpoints can be added without creating production architectural dependencies;
- actual component output is preserved faithfully;
- missing information is not silently manufactured;
- HMRC payload generation is exercised rather than duplicated;
- HMRC Sandbox responses can be inspected when transport is available;
- obsolete QU/EOPS harness models are not preserved merely for compatibility;
- non-HMRC harness models are not mistaken for HMRC contracts;
- and the complete production HMRC integration remains independent of the Test Harness.

---

**End of document.**
