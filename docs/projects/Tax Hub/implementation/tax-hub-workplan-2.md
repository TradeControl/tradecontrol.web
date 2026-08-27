# Tax Hub — Work Plan 2  

## Objective 2: Submission Logic (Test Harness Payloads)

Version: Objective 2.2  
Status: Updated Engineering Work Plan

Objective 2 implements the **Submission Logic Layer** for the Tax Hub.  
This layer generates **internal test harness payloads**, not HMRC payloads.

HMRC payloads are defined under Objective 3.

HMRC transport is implemented under Objective 4.

## Objective 2 Scope Boundary (Updated)

Objective 2 includes:

- Test harness payload models  
- OperationType request/response contracts  
- Dataset readers  
- Harness payload builders  
- Harness validators  
- Mapping utilities  
- Submission runner  
- WebHarness endpoints  

Objective 2 does **not** include:

- HMRC payloads  
- HMRC transport  
- OAuth  
- Fraud headers  
- CT600 XML  
- iXBRL  
- HMRC endpoint definitions  

## Phase 1 — Operation Contract and Canonical Envelope

Define the execution contract used throughout the module.

Implement the request/response model exactly as defined in the implementation
instructions:

- `OperationType` as a function declaration
- `Parameters` as the operation argument list
- canonical submission response structure
- strict distinction between submission functions and enquiry functions

This phase establishes the fixed contract that all later validators, builders,
and runner logic depend upon.

**Deliverables:**

- `OperationType` enum
- `HmrcSubmissionRequest`
- canonical submission result model
- canonical envelope model structure
- parameter naming aligned with the implementation instructions

## Phase 2 — Harness Payload Models

Replace “canonical” with “harness”.

Deliverables:

- `QuHarnessPayload`  
- `EopsHarnessPayload`  
- `MicroHarnessPayload`  
- `VatHarnessPayload`  
- `HarnessEnvelope`  

## Phase 3 — Authoritative Dataset Surface

Implement the dataset readers for the two SQL views defined in the
implementation instructions.

### VAT

- Read from `Cash.vwTaxVatTotals`
- Use `StartOn` and `EndOn`
- Treat `EndOn` as the HMRC period end date supplied by the dataset

### QU / EOPS / Micro

- Read from `Cash.vwTaxBizSubmission`
- Use `PeriodFrom` and `PeriodTo`
- Treat `PeriodTo` as the HMRC period end date supplied by the dataset

Dataset access in this phase must be limited to the authoritative views defined
by the specification.

**Deliverables:**

- `TcVatReader`
- `TcBusinessTaxReader`
- dataset models containing the exact fields required by the payload builders
- no dependency on any other SQL tables or views

## Phase 4 — Payload Builders

Implement the four payload builders:

- `QuPayloadBuilder`
- `EopsPayloadBuilder`
- `MicroPayloadBuilder`
- `VatPayloadBuilder`

Each builder must:

- read from the correct dataset reader
- construct the correct canonical payload
- apply the parameter rules defined by the operation contract
- produce a `PayloadEnvelope`

Builders must remain explicit and operation-specific. They should not introduce
generic dispatch abstractions that obscure behaviour.

**Deliverables:**

- four builder classes
- correct tax-source-specific field population
- correct period semantics
- canonical envelope output for each submission function

## Phase 5 — Validators

Implement the per-operation validators required by the implementation
instructions.

Submission validators:

- `QuValidator`
- `EopsValidator`
- `MicroValidator`
- `VatValidator`

Enquiry validators:

- `ObligationValidator`
- `LiabilityValidator`
- `PaymentValidator`
- `SubmissionHistoryValidator`

Validators must enforce:

- required parameters
- parameter types
- parameter semantics
- rejection of missing parameters
- rejection of unused parameters
- rejection of invalid combinations
- dataset availability for submission functions
- structural and numeric payload rules where applicable

**Deliverables:**

- submission validators
- enquiry validators
- validation result model
- strict validator behaviour aligned to `OperationType`

## Phase 6 — Mapping Utilities

Implement the mapping utilities:

- `TagMapper`
- `CategoryMapper`

These utilities support payload construction by translating Trade Control data
into canonical HMRC payload fields. Their behaviour must remain explicit and
driven by the supplied specifications.

**Deliverables:**

- mapping utility classes
- deterministic tag/category mapping behaviour
- unit tests for mapping rules

## Phase 7 — Submission Runner

Implement the single execution entry point:

- `HmrcSubmissionRunner`

The runner must:

- accept an `OperationType`
- validate the supplied parameters using the correct validator
- dispatch to the correct payload builder for submission operations
- produce the final canonical response payload
- preserve strict separation between submission and enquiry semantics

This phase completes the submission logic core for Objective 2.

**Deliverables:**

- `HmrcSubmissionRunner`
- explicit switch-based dispatch logic
- integrated validation
- canonical submission response output

## Phase 8 — WebHarness (Internal Test Harness)

Implement the internal test harness controllers:

- `QuTestController`
- `EopsTestController`
- `MicroTestController`
- `VatTestController`

These controllers must:

- accept test requests
- call the submission runner
- return the canonical payload envelope and validation output
- contain no payload-building business logic of their own

This phase provides the end-to-end developer test path required by the
repository structure.

**Deliverables:**

- four test controllers
- request/response models for harness execution where needed
- end-to-end harness path for all four submission operations

## Phase 9 — Verification and Objective Closure

Verify the complete Objective 2 surface against the supplied specifications.

Verification must confirm:

- harness payload models match the payload specification
- dataset readers use only the authoritative views
- builders produce correct payloads for each tax source
- validators enforce strict parameter and structural rules
- mapping utilities behave deterministically
- runner dispatch is correct
- WebHarness endpoints return harness outputs through the runner

**Deliverables:**

- builder tests
- validator tests
- runner tests
- WebHarness tests
- confirmation that Objective 2 is complete and ready for transport-layer work in later objectives

## Completion Criteria

Objective 2 is complete when:

- Harness payload models are implemented  
- Dataset readers return correct values  
- Harness payload builders produce correct envelopes  
- Validators enforce strict rules  
- Mapping utilities behave deterministically  
- Runner dispatches correctly  
- WebHarness returns harness payloads  

**End of document.**
