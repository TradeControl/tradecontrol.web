# Companies House Development Simulator — Proposal and Work Plan

3 October 2026  
**Status:** Block A completed on 3 October 2026 and accepted by human review on 6 October 2026. Its Application lifecycle and development-only in-process WebHarness composition are preserved. Official test-account access arrived on 6 October 2026 before Block B authorisation. Block B remains deferred; real adapter work now proceeds in separately reviewed increments, using Block A behind the same XML exchange boundary.

## Purpose

Create a development-only Companies House XML Gateway simulator that allows the Tax Hub team to exercise filing transport, asynchronous status handling, recovery and product workflow. The simulator was completed while official XML software-filing test access was pending and is retained after access arrived as deterministic regression and failure-testing infrastructure.

The simulator is a tool used by developers and automated tests. It is not part of the statutory filing workflow, is not a substitute for Companies House developer testing, and cannot provide submission-readiness or production-approval evidence.

The intended outcome is that official access changes the work from a large first integration into a controlled reconciliation exercise: replace the simulated authority endpoint with the official test endpoint, compare actual behaviour with recorded assumptions, correct differences, and repeat the same lifecycle tests with official evidence.

## Programme position

The simulator is separate from Work Plan 5's authority-delivery sequence. It does not complete Phase 5.11, begin Phase 5.12, or alter the Phase 5.13 approval gate.

It depends on the published-contract work recorded in [Phase 5.11 prerequisite assurance](phase-5.11-companies-house-prerequisite-assurance.md):

- Companies House accounts TIS 6.0;
- GovTalk envelope contract;
- `FormSubmission-v2-11`;
- `GetSubmissionStatus-v2-9`;
- `GetStatusAck-v1-1`; and
- the fail-closed Companies House package boundary.

Companies House confirmed creation of the requested test account on 6 October 2026 and supplied protected test presenter credentials, a test flag and an allocated test package reference. The values are retained only under the git-ignored `.local/companies-house/test-account` area. Account creation is not developer-test acceptance or production approval.

## Approved review decisions

The human reviewer recorded these decisions on 3 October 2026:

1. The simulator is a standalone development tool outside the main Work Plan 5 authority-delivery sequence.
2. Boundary work and the first pure state-machine implementation may be delivered together; no artificial human gate is required between them.
3. Official XSD assets may be committed only after provenance, licensing and repository-role review. Work that does not require committed copies should continue independently.
4. The loopback host remains in scope, but only after acceptance of the pure state machine and with every proposed loopback and production-isolation safeguard.
5. Simulator state and evidence begin in memory only. Persistent simulator storage requires a later concrete need and separate justification.
6. Development-workflow integration remains in scope after the simulator and relevant transport boundary exist. It must remain development-only and visibly labelled `SIMULATED — NOT FILED`.

These decisions approve the direction, not implementation of every block.

## Governing distinction

Every result must carry one of these evidence classes:

| Evidence class | Meaning | May establish Companies House readiness? |
|---|---|---:|
| Published-contract validation | Behaviour derived directly from published TIS, schemas and examples | No |
| Simulator behaviour | Deliberately modelled local behaviour, including configurable assumptions | No |
| Official developer-test evidence | Observed response from the Companies House test gateway or review process | Potentially, after human review |
| Production evidence | Observed response from an authorised production operation | Only within the separately approved production process |

Simulator results must never be labelled simply `accepted`, `rejected`, `gateway receipt` or `Companies House response` without an explicit `SIMULATED` qualifier.

## Objectives

The tool should:

1. exercise the real application-facing Companies House transport and lifecycle boundaries without external network access;
2. consume the same materialised GovTalk XML that a future transport adapter would send;
3. validate the published structural contract where official schemas are available;
4. simulate synchronous acknowledgement and transport/parser/authentication failures;
5. simulate asynchronous filing states and status polling;
6. exercise conditional `StatusAck` behaviour, including redelivery when acknowledgement is omitted;
7. support deterministic recovery, duplicate and timeout scenarios;
8. preserve exact submitted bytes, digests and safe diagnostic evidence for comparison; and
9. make every inferred behaviour easy to locate and replace when official evidence arrives.

## Non-objectives

The simulator must not:

- connect to any Companies House host;
- accept or store real presenter or company credentials;
- claim to reproduce unpublished Companies House accounting validation rules;
- infer test acceptance, manual-review outcomes or production approval;
- mark a package or contract `SubmissionReady`;
- change VAT or Corporation Tax contracts, gates or artifacts;
- become a fallback authority endpoint in production;
- generate plausible-looking official receipt references without a simulator marker;
- conceal ambiguity behind a generic success response; or
- implement the future replacement filing API.

## Proposed location and dependency boundary

The preferred implementation is a dedicated development-tool project, tentatively:

`src/TradeControl.Tax.UK.Tools.CompaniesHouseSimulator`

with a focused test project:

`tests/TradeControl.Tax.UK.CompaniesHouseSimulator.Tests`

The simulator may reference the published Companies House contract assembly. It must not be referenced by contract projects, Application, production adapters or TCWeb. A development host or test harness may opt into it explicitly.

The dependency direction should be:

```text
Companies House contracts
          ↓
development simulator ← simulator tests / explicit development host

Application and production adapters do not depend on the simulator.
```

An architecture test should enforce the absence of simulator references from production assemblies.

## Proposed architecture

### 1. Protocol state machine

A pure, deterministic state machine owns simulated submissions and status-delivery rules. It has no HTTP dependency and no system-clock dependency.

Its inputs are:

- exact request bytes;
- a simulator-only authentication context;
- an explicit scenario definition;
- a deterministic clock; and
- a deterministic reference generator.

Its outputs are exact response bytes plus simulator diagnostics kept outside the wire document.

### 2. Authority-facing contract processor

The processor parses GovTalk safely, classifies the request and routes only the supported classes:

- `AA` / `FormSubmission`;
- `GetSubmissionStatus`; and
- `StatusAck`.

Unsupported classes fail explicitly. The processor performs only validation justified by published material or clearly labelled scenario rules.

### 3. Scenario engine

Scenarios are immutable, named and checked into source control. They define a sequence such as:

```text
submission → synchronous acknowledgement → pending → accepted
```

or:

```text
submission → acknowledgement → pending → rejected with two rejection details
```

No test should depend on random timing or hidden mutable configuration.

### 4. Optional loopback host

After the state machine is accepted, an optional host may expose it over HTTP on loopback only. This layer exists to test request transmission, response byte handling, interruption and retry behaviour. It must add no statutory logic.

Binding to a non-loopback interface must fail during startup. The host must not support TLS certificates, DNS names or configuration that resembles a Companies House environment.

### 5. Simulator evidence store

A small local store may retain:

- simulator attempt reference;
- request and response SHA-256 values;
- exact request and response bytes where test policy permits;
- scenario name and version;
- deterministic simulated time;
- simulated submission number and state transitions; and
- whether a returned general-poll result has been acknowledged.

The store is test evidence, not a statutory submission audit. Its schema and UI must use `simulator` or `simulated` terminology throughout.

## Credential and environment guardrails

The simulator should accept only a dedicated value type such as `SimulatorAuthentication`, not the future production secret-provider interface.

Synthetic values must satisfy all of the following:

- carry an unmistakable prefix such as `SIM-`;
- be rejected if they match configured real-secret shapes or known environment values;
- never be read from the production secret store;
- never be copied into `PreparedStatutoryArtifact`; and
- be redacted from ordinary diagnostics even though they are synthetic.

The simulator package reference, transaction identifiers, gateway timestamps and receipt references must also carry a simulator marker where the official field permits it. Where a published field has a restrictive format, the simulator evidence surrounding the field must identify the response as simulated.

Configuration must require an explicit development switch. Absence of that switch disables the tool rather than selecting it by default.

## Published behaviour to model

### Filing request

For `AA` submission, the simulator should check:

- safe XML parsing and expected GovTalk namespace;
- request qualifier and submit function;
- `FormSubmission` namespace and required header ordering;
- six-character submission number;
- supported company type and company-number representation;
- package reference presence;
- synthetic presenter and company-authentication presence;
- `DateSigned` presence;
- a single supported accounts document;
- filename length;
- `ContentType=application/xml`;
- `Category=ACCOUNTS`;
- valid base64; and
- preservation of decoded iXBRL bytes and their SHA-256.

The first version should not invent accounting validation rules beyond the published TIS 6.0 requirements already represented by the Objective 3 contract tests.

### Synchronous response

Configurable synchronous outcomes should include:

- parsed and acknowledged;
- malformed XML;
- unsupported class;
- schema failure;
- presenter-authentication failure;
- company-authentication failure;
- duplicate active submission number; and
- simulated internal gateway failure.

A synchronous parse or authentication failure means the filing was not accepted into asynchronous processing.

### Asynchronous lifecycle

The simulator should support every published status code:

- `PENDING`;
- `ACCEPT`;
- `REJECT`;
- `PARKED`; and
- `INTERNAL_FAILURE`.

Rejections should support multiple code, description and instance-number values. Scenario definitions should state whether a status is terminal for the product workflow without embedding that policy in the XML parser.

### Status queries

The simulator should distinguish:

1. a specific submission-number poll;
2. a company-number poll; and
3. a presenter-wide poll.

General polls should support a configurable published maximum result count and deterministic ordering. Specific polls do not require `StatusAck`. Company and presenter-wide polls do.

### Status acknowledgement

For general polls:

- unacknowledged completed results are returned again;
- a valid empty `StatusAck` marks the delivered batch acknowledged;
- acknowledged results are absent from subsequent general polls; and
- duplicate acknowledgements are safe and observable.

The precise official batching/session correlation behaviour remains an assumption until developer-test evidence is obtained and must be isolated behind one policy component.

## Candidate scenario catalogue

The complete candidate catalogue is below. Block A implements only its six named core scenarios; later scenarios are added when a delivery block identifies a concrete integration need.

| Scenario | Purpose |
|---|---|
| `happy-accept` | Acknowledgement, pending poll and terminal acceptance |
| `business-reject` | Acknowledgement followed by a rejection with bounded details |
| `parked-then-accept` | Non-terminal manual-review-like delay followed by acceptance |
| `internal-failure` | Authority-side asynchronous failure |
| `malformed-envelope` | Synchronous parser rejection and no stored submission |
| `invalid-form-submission` | Published-schema failure and no asynchronous processing |
| `invalid-presenter-auth` | Synchronous synthetic authentication failure |
| `invalid-company-auth` | Synchronous synthetic company-authentication failure |
| `duplicate-submission-number` | Repeat identity handling without accidental duplicate filing |
| `timeout-before-response` | Request delivery unknown to the caller |
| `timeout-after-acceptance` | Stored submission with lost synchronous response |
| `general-poll-redelivery` | Missing `StatusAck` causes deterministic repeat delivery |
| `general-poll-acknowledged` | Successful `StatusAck` removes delivered completed results |
| `max-batch` | More completed results than one general-poll response can return |
| `exact-byte-integrity` | Decoded attachment matches the prepared iXBRL digest |

## Incremental delivery strategy

The simulator should earn its next block by helping the real integration. It is not a miniature platform to complete before transport development can continue.

The blocks below are cumulative but independently useful. Completing one block does not imply that the next is necessary. Every block ends with a short value review:

1. What real Companies House integration question can now be answered?
2. What implementation or recovery behavior can now be tested that could not be tested before?
3. Is the next simulator block still the cheapest way to remove the current integration risk?
4. Has new official evidence made any planned simulator behavior unnecessary?

If a block is no longer the best route to real integration progress, it should be deferred or cancelled without treating the simulator as incomplete.

## Delivery blocks

### Block A — Useful core simulator

**Maps to:** S0 plus the valuable core of S1.  
**Estimated effort:** 4–6 focused hours.  
**Authorisation:** authorised by the user on 3 October 2026; implemented and awaiting review.

**Development goal:** provide the earliest useful, network-free Companies House lifecycle double for contract and Application tests.

Work:

1. Create the standalone simulator and test projects and confirm their one-way dependency boundary.
2. Record a compact rule/assumption ledger distinguishing published rules from simulator choices.
3. Define unmistakably synthetic authentication and reference values.
4. Implement safe request classification for `AA`, `GetSubmissionStatus` and `StatusAck`.
5. Implement a deterministic, memory-only submission state machine with injected time and reference generation.
6. Preserve exact submitted XML and decoded iXBRL bytes/digests without reconstruction.
7. Support synchronous acknowledgement, parser failure and synthetic authentication failure.
8. Support every published asynchronous status value through data-driven transitions.
9. Implement specific-submission and presenter-wide polling.
10. Implement conditional `StatusAck` and redelivery when acknowledgement is omitted.
11. Add the six initial scenarios: `happy-accept`, `business-reject`, `parked-then-accept`, `internal-failure`, `malformed-envelope` and `general-poll-redelivery`.
12. Add an architecture test proving no production assembly references the simulator and prove the block has no network API.

Deliberately excluded:

- committed official XSDs;
- comprehensive golden fixtures and mutation tests;
- company-wide and maximum-batch behavior unless needed by current transport design;
- timeout/reset simulation;
- HTTP hosting;
- persistent evidence;
- WebHarness or UI integration; and
- comparison-report machinery.

Exit evidence:

- the six core scenarios pass offline and deterministically;
- all response evidence is explicitly simulated;
- exact attachment-byte integrity is proved;
- `StatusAck` redelivery behavior is proved;
- unsupported and ambiguous input fails closed;
- the solution Release build passes; and
- the value review identifies the real integration work enabled by the block.

**Immediate value:** Application and future adapter code can develop against a realistic submission/status lifecycle without waiting for HTTP hosting or official credentials.

#### Block A implementation outcome — 3 October 2026

- Added a standalone `TradeControl.Tax.UK.Tools.CompaniesHouseSimulator` project that references only the company contract assembly.
- Added a dedicated offline console test project and solution entries under Development Tools and Tests.
- Implemented safe GovTalk classification for `AA`, `GetSubmissionStatus` and empty `StatusAck` requests.
- Implemented deterministic, memory-only state with injected time and transaction-reference generation, immutable scenario/response values and exact submitted request plus decoded attachment bytes and SHA-256 evidence.
- Implemented synthetic-only out-of-band authentication so Block A does not invent the Phase 5.12 credential-materialisation boundary.
- Implemented `PENDING`, `ACCEPT`, `REJECT`, `PARKED` and `INTERNAL_FAILURE` transitions, bounded simulated rejection details, specific and presenter-wide polling, conditional acknowledgement and unacknowledged general-poll redelivery.
- Added fail-closed handling for malformed XML, unsupported classes, missing scenarios, invalid synthetic authentication, duplicate submission numbers and cross-presenter specific polling.
- Added an assumption ledger and architecture checks proving the simulator has no network/host dependency and is not referenced by production projects.

Verification at implementation handoff:

- Block A simulator suite: 20 assertions passed;
- architecture suite: 19 assertions passed in the final Release verification;
- exact attachment digest and deterministic response bytes proved; and
- no HTTP host, persistent store, WebHarness integration, committed XSD or external Companies House call was introduced.

The Block A value review is positive: a future Companies House adapter can now exercise receipt, polling, rejection, parked, failure and acknowledgement behavior without waiting for official access. The next simulator block is not implied; choose Block B or C only when the real adapter work identifies the corresponding need.

#### Block A closure review — 3 October 2026

A literal review of the Block A plan found and closed three gaps before final acceptance:

- the in-memory evidence now retains immutable copies of the exact request XML and decoded iXBRL bytes as well as both digests;
- transaction-reference generation is explicitly injected and is proved controllable and deterministic; and
- outstanding general-poll batches are scoped by synthetic presenter, so a different presenter cannot inspect or acknowledge them.

The evidence accessor enforces the same presenter boundary. The focused suite now passes 31 assertions, including exact-byte retrieval, controlled reference generation, cross-presenter evidence refusal and cross-presenter `StatusAck` refusal. These changes complete Block A without introducing a network endpoint or persistent storage.

#### First downstream integration increment — 3 October 2026

Following acceptance of the network-free approach, Block A now drives an immutable Application-level Companies House filing conversation directly in process. The conversation:

- preserves the six-character submission identity plus the prepared transmission and accounts-document SHA-256 values;
- distinguishes initial gateway acknowledgement from authority acceptance;
- models pending, parked, accepted, rejected and internal-failure outcomes;
- retains bounded rejection details and refuses a changed result after a terminal outcome;
- records only safe local evidence explicitly classified `SIMULATED — NOT FILED`; and
- retains and completes the conditional `StatusAck` obligation for presenter-wide results.

This is downstream use of Block A, not Block B or Phase 5.12 transport activation. It adds no adapter, endpoint, credential materialisation, persistence or external send. Its purpose is to establish the lifecycle model that a later Companies House adapter can implement without coupling Application to the simulator.

#### Development-only WebHarness composition — 3 October 2026

The development sequence was clarified after Block A: the WebHarness should follow the successful VAT integration structure now, without waiting for the simulator itself to become a web service. A new `POST /api/company/companies-house/accounts/simulate` endpoint therefore:

1. accepts the established Companies House accounts preparation payload;
2. invokes the existing Trade Control database reader and statutory-accounts/iXBRL preparation path;
3. retains the normal `CH-PENDING-CREDENTIALS` and `CH-PENDING-DEVELOPER-TEST` authority-dispatch blockers while refusing every other preparation error;
4. sends the exact prepared transmission bytes to a fresh in-process simulator instance with server-owned synthetic authentication;
5. drives bounded specific-submission polling through the Application conversation; and
6. returns a two-part development response: a safe Tax Hub lifecycle summary and the exact simulated GovTalk/XML request-response transcript, both under the explicit evidence class `SIMULATED — NOT FILED`; no database or authority credential is returned.

The composition is registered only when the WebHarness environment is Development. A disabled implementation returns no simulation facility elsewhere. It introduces a WebHarness endpoint for developers, not a Companies House-shaped simulator endpoint; Block C remains the later replacement boundary for loopback HTTP and transport-failure testing.

#### Database-to-simulator endpoint sign-off — 3 October 2026

The development-only vertical slice was exercised against the active synthetic company STD sandbox through `POST /api/company/companies-house/accounts/simulate`. The secret-backed database connection was resolved in process and was not printed or persisted. The request used the filleted TIS 6.0 profile and synthetic submission number `S00001`.

The endpoint returned HTTP `200` in 1,427 ms with evidence class `SIMULATED — NOT FILED`, lifecycle `Acknowledged → Pending → Accepted`, terminal state `Accepted`, zero errors and no `StatusAck` requirement. The prepared accounts digest was `3FE6960FC4C83A277B765C0415F72B1F514C51ED4A7A7F0887963D98B83209AC`; the distinct transmission digest was `A7265F94542A007283F289018C9189AF18BEA626F0AD49B26C2FD1B0ABD73709`.

The endpoint response was subsequently corrected to distinguish the Tax Hub interpretation from the simulated authority wire contract. Its `taxHub` section contains the developer-oriented summary; its `companiesHouse` section contains the exact `FormSubmission`/acknowledgement and `GetSubmissionStatus` request-response XML exchanges. The immediate acknowledgement now uses the published GovTalk acknowledgement shape—gateway metadata with an empty body—rather than a simulator-only body element. Simulator identity remains outside the authority XML as `SIMULATED — NOT FILED`.

Before this sign-off, the balance-sheet source boundary was corrected and separated into generic `Cash.fnTaxBizBalanceSheet` classifications and the explicitly jurisdictional `Cash.fnTaxBizBalanceSheetUK` extrapolation consumed by Tax Hub. Both active company MIN and STD sandboxes pass the DP5/CO1–CO4 source, artifact and reconciliation suite. The endpoint result is local development evidence only: it does not change the pending presenter-credential, company-authentication or Companies House developer-test gates and does not authorise Block B, Block C or Phase 5.12 transport activation.

#### Block A human sign-off — 6 October 2026

Human review accepted Block A, including the pure state machine, Application conversation and development-only database-to-simulator WebHarness composition, as the correct foundation for the eventual Companies House adapter. This acceptance is limited to development scaffolding and simulated evidence. It does not treat the pending test-account application as approved, does not satisfy Companies House developer testing, and does not authorise Phase 5.12 or any external submission.

### Block B — Targeted conformance support

**Maps to:** the remaining useful parts of S1 and selected parts of S2.  
**Estimated effort:** 3–5 focused hours, selected by need rather than implemented wholesale.  
**Trigger:** Block A is accepted and contract/adapter development has identified concrete validation or repeatability gaps.

**Development goal:** add only the fixtures and adverse cases needed to stabilise the next real transport change.

Candidate work:

1. Add `invalid-form-submission`, invalid presenter/company authentication and duplicate-submission scenarios.
2. Add company-wide polling and maximum-batch behavior if the adapter or product intends to use those modes.
3. Add checked-in fixtures only for requests or responses that serve a current regression test.
4. Add targeted mutations for namespace, ordering, identifier, attachment metadata and base64 defects that have practical failure value.
5. Add a compact in-memory transcript containing scenario version, simulated time, state transitions and request/response digests.
6. Validate against official XSDs from an external verified location, or commit them only after the separate provenance/licensing/repository-role review.

Block B is not a requirement to build every mutation or every possible fixture. A future official comparison report remains deferred until official evidence exists.

Exit evidence:

- each added scenario protects a named integration behavior;
- fixture and mutation coverage is traceable to a practical failure risk;
- schema results are labelled local validation, never authority acceptance; and
- no persistent store or network host has been introduced.

**Immediate value:** the current adapter increment gains stable regressions without front-loading an exhaustive simulator conformance suite.

#### Changed circumstances before Block B authorisation — 6 October 2026

Companies House created the requested XML software-filing test account and supplied protected test presenter credentials, a required test flag and an allocated test package reference. It also confirmed that submission numbers must be unique and incremental and that the XML team must be notified after test submissions so they can be manually reviewed. The response did not provide the requested current testing-criteria document or a company authentication value, and no test submission or acceptance evidence exists yet.

Because access arrived after Block A but before Block B was authorised, the prematurely started targeted Block B regression increment was withdrawn. Block B is not started. The cheapest next step is now the real adapter hop, not broader simulator conformance. New fixtures, mutations or simulator behaviours must be justified by an observed authority response, a concrete adapter/recovery requirement, or a failure that cannot safely be exercised against Companies House. Checked-in official XSD assets remain subject to the separate provenance, licensing and repository-role review.

Block A remains valuable for deterministic accepted, rejected, parked and internal-failure lifecycles; duplicate and malformed input; exact-byte/digest continuity; presenter isolation; general-poll redelivery; and conditional `StatusAck`. These capabilities are preserved because they provide repeatable regression and failure evidence that official manual testing cannot safely or cheaply supply.

#### Real-adapter hand-off — 6 October 2026

The supplied test presenter ID, presenter authentication value, test flag and package reference resolve the presenter-account dependency. The chosen company's authentication code remains a distinct protected filing input. A separately supplied testing-criteria document is not required to build the adapter or make the first separately authorised controlled test; it remains part of the evidence needed to claim testing completion.

The first adapter increment introduces a transport-neutral raw-XML gateway session used by the existing WebHarness conversation. Its two implementations are:

- the accepted in-process Block A simulator, which remains `SIMULATED — NOT FILED`; and
- an official-test placeholder marked `OFFICIAL COMPANIES HOUSE TEST — SEND DISABLED`, which has no HTTP client, reads no credentials and always rejects an exchange.

This boundary is the hand-off from deterministic scaffolding to real integration. Later increments may materialise protected credentials and add offline HTTP behaviour, but no external send becomes possible without a separate human gate.

#### Official-example alignment — 6 October 2026

The later adapter review found one concrete Block A correction in the published Companies House examples: `Function=submit` is present on the `AA` filing envelope but absent from `GetSubmissionStatus` and `StatusAck` control requests. The shared contract serializer and simulator classifier now enforce that distinction. This is a narrow evidence-driven correction, not Block B or a redesign of the accepted state machine. Block A still passes its 31 deterministic assertions.

The subsequent protected-materialisation increment was completed on 6 October 2026 without extending Block A or beginning Block B. It resolves the issued presenter configuration and separate company authentication through protected providers, materialises the published reserved fields in memory, and returns digest-only `MATERIALISED — NOT SENT` evidence. The simulator remains free of real credentials; its only role in the adapter path is the deterministic implementation of the shared raw-XML exchange boundary.

The following offline HTTP increment likewise did not extend the simulator. The real adapter now has a pinned-host recording-handler path and typed acknowledgement/status/error parsing, while its host-facing implementation remains internal and unregistered. Block C loopback hosting is still unnecessary: the recording handler already proves exact HTTP request construction, and no timeout-after-receipt recovery requirement has yet justified a separate simulator web service.

A Development-only official-test preflight now makes that hand-off visible in the WebHarness. It runs the real database preparation path, checks the two protected configuration sources separately, optionally materialises the wire envelope in memory, and returns only route/readiness flags and SHA-256 values. It is explicitly `OFFICIAL TEST PREFLIGHT — NOT SENT`; it neither calls the simulator nor exposes or invokes the official HTTP implementation. This is adapter tooling, not Block B, C or D simulator scope.

### Block C — Loopback transport and recovery

**Maps to:** S3.  
**Estimated effort:** 3–5 focused hours.  
**Trigger:** the pure state machine is accepted and a real Companies House transport adapter boundary exists that needs byte-level and recovery testing.

**Development goal:** test actual HTTP/XML transmission and ambiguous-delivery recovery without an external authority call.

Work:

1. Expose the accepted state machine through a loopback-only HTTP host.
2. Accept and return raw XML bytes without statutory-model reconstruction at the host boundary.
3. Add only the network failure modes required by the adapter: delayed response, timeout, connection reset after receipt and truncated response.
4. Prove exact request/response byte handling.
5. Prove the host refuses non-loopback binding and cannot select official Companies House hostnames.
6. Keep simulator registration out of production dependency injection and configuration.

Exit evidence:

- loopback and production-isolation tests pass;
- at least one before-delivery and one after-delivery failure are distinguishable;
- adapter recovery behavior is deterministic; and
- no route from simulator configuration to an external address exists.

**Immediate value:** the production-shaped adapter can be hardened for delivery ambiguity before official credentials arrive.

### Block D — Development journey

**Maps to:** S4.  
**Estimated effort:** 3–5 focused hours.  
**Trigger:** Block A supports an in-process WebHarness preparation/simulation slice. The remaining transport-backed journey requires Block C, an accepted adapter boundary, and evidence that the fuller journey will materially help product work or official-test preparation.

**Development goal:** let developers rehearse the user-facing filing lifecycle without creating records that resemble real statutory submissions.

Work:

1. Add an explicitly selected simulator mode to the development harness only.
2. Display a persistent `SIMULATED — NOT FILED` banner and evidence class.
3. Exercise preparation, approval reference, simulated dispatch, polling, acknowledgement and terminal presentation.
4. Keep simulator history separate from genuine filing history; memory-only state remains the default.
5. Document start, reset and scenario-selection procedures.
6. Add accessibility and failure-message review for the simulated journey.

Persistent simulator storage is excluded unless this block reveals a concrete need and a separate change is approved.

Exit evidence:

- the development journey is visibly and structurally simulated;
- simulator results cannot close a statutory obligation or appear as an official attempt;
- zero route to an external host is proved; and
- the workflow demonstrates a named improvement to official-test preparation.

**Immediate value:** product and integration behavior can be rehearsed end to end shortly before or during official developer testing.

### Block E — Official-access reconciliation

**Maps to:** S5.  
**Estimated effort:** determined after credentials and criteria arrive.  
**Trigger:** separate authorisation to use issued test credentials and make external Companies House test submissions.

**Development goal:** replace simulator assumptions with observed authority evidence.

Work:

1. Freeze the simulator version used before the first official test.
2. Run only the authorised official test cases with safe evidence capture.
3. Compare acknowledgement, errors, polling, status and `StatusAck` behavior field by field.
4. Classify differences as simulator defect, implementation defect, documentation ambiguity or environment-specific behavior.
5. Update or remove simulator behavior only after preserving the original comparison evidence.
6. Keep official acceptance evidence separate from simulator results.

Exit evidence:

- official test transcript references;
- a focused simulator-versus-official difference report;
- corrected contract and transport tests; and
- a separate human decision on Phase 5.12/5.13 readiness.

**Immediate value:** the simulator becomes a disposable comparison aid rather than an alternative source of authority.

## Block ordering and stopping rule

The expected order is A → B or C → D → E, but only Block A is foundational:

- Block B may be skipped if the core tests are sufficient and official access arrives quickly.
- Block C may precede Block B when transport recovery is the next real problem.
- The narrow in-process WebHarness composition may follow Block A so database-derived preparation can exercise the simulator. The remaining Block D transport-backed journey waits for Block C and should be skipped if it would duplicate official testing.
- Block E supersedes simulator assumptions wherever official behavior differs.

At no point is completion of all blocks a project goal in itself. The stopping rule is: stop extending the simulator when the next increment does not remove a current risk or accelerate a named Companies House integration task.

## Verification strategy

At minimum, an implementation should run:

- simulator unit and scenario tests;
- Companies House contract tests;
- Application tests if the port or dispatch boundary changes;
- offline corporate handoff tests;
- architecture tests proving production isolation;
- focused transport tests proving zero external I/O;
- loopback safety tests if Block C is authorised; and
- a Release build of `src/TaxHub.slnx`.

Tests must assert both positive behaviour and refusal behaviour. A passing happy-path test alone is insufficient.

## Readiness and fail-closed rules

Simulator success must not influence these production facts:

- `CompanyContractRegistry.CompaniesHouseAccountsTis60.SubmissionReady` remains false until separately reviewed official evidence supports changing it.
- Prepared packages with pending credentials or developer-test findings remain non-dispatchable.
- Simulator receipts cannot satisfy a durable official authority-attempt outcome.
- Simulator status cannot close an actual statutory obligation.
- Simulator credentials cannot be promoted or copied into a future Companies House secret store.
- Official endpoint selection must remain impossible until Phase 5.12 is separately authorised.

## Risks and controls

| Risk | Control |
|---|---|
| Simulator behaviour is mistaken for authority behaviour | Evidence classification, persistent simulated labels and separate records |
| Tests overfit invented rules | Central assumption ledger and scenario policies; reconcile in Block E |
| Simulator leaks into production | One-way project references, architecture tests and loopback-only host |
| Synthetic credentials are replaced with real values | Dedicated value types, prefixes and refusal rules |
| Happy path hides recovery defects | Mandatory timeout, duplicate, redelivery and ambiguity scenarios |
| Local validation is reported as acceptance | Documentation and result types distinguish schema, simulator and official evidence |
| Simulator becomes costly to maintain | Limit scope to the three published operations and approved accounts profile |
| Official behaviour differs substantially | Preserve comparison evidence and replace assumptions rather than defending compatibility |

## Cumulative deliverables

The deliverables grow only as blocks are justified:

| Block | Deliverable added |
|---|---|
| A | Standalone projects, assumption ledger, memory-only state machine and six core lifecycle scenarios |
| B | Targeted fixtures, mutations, optional local schema checks and in-memory transcripts |
| C | Loopback-only host and adapter recovery/fault tests |
| D | Visibly simulated development journey and operating guidance |
| E | Official comparison evidence and corrections that supersede simulator assumptions |

## Next human review gate

The direction and six boundary decisions are approved, Block A is complete, and its Application plus development-only WebHarness integrations are implemented for review.

The next decision is whether the database-to-simulator vertical slice and Application conversation are the right foundation for the eventual adapter. That review does not authorise Block B, Block C, the remaining Block D journey, Block E or Phase 5.12 transport activation. Each later simulator block still requires evidence that it is the cheapest useful step toward the real Companies House integration.
