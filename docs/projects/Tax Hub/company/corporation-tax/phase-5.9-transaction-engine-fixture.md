# Corporation Tax Phase 5.9 — Transaction Engine protocol fixture

## Status

Accepted by human review on 14 October 2026. This phase is fixture-only. It does not register an HTTP client, expose a WebHarness operation, contact HMRC's External Test Service, enable Corporation Tax submission or alter the Phase 5.7 fail-closed family gate. Phase 5.10 may proceed locally; any external HMRC send still requires separate explicit approval.

## Official protocol basis

The implementation is pinned to HMRC's [Transaction Engine Document Submission Protocol v2.0](https://www.gov.uk/government/publications/transaction-engine-document-submission-protocol), updated 15 August 2018, and the [Gateway IRmark guide and examples](https://www.gov.uk/government/publications/hmrc-irmark-for-gateway-protocol-services).

The protocol establishes:

- UTF-8 GovTalk XML over HTTP POST;
- a submission request followed by an acknowledgement containing a 32-character upper-case hexadecimal `CorrelationID`, a `ResponseEndPoint` and integer `PollInterval`;
- polling no earlier than that interval until a departmental business response or business error is returned;
- retention and processing of the terminal response before `DELETE_REQUEST`, followed by `DELETE_RESPONSE`;
- authenticated `DATA_REQUEST`/`DATA_RESPONSE` recovery for outstanding submissions; and
- the published production submission endpoint `https://transaction-engine.tax.service.gov.uk/submission`.

HMRC's published full IRmark example is retained exactly as base64 test data so source newline and indentation bytes cannot be rewritten by Git. Its SHA-256 is `E6A38D07BDD1F400A34E8BFD95A69B1718F4F10A3B01BA04602788287CA353E9`. The implementation reproduces HMRC's expected base64 SHA-1 IRmark `RPfWtxHeCZRcwfitnIJmK9xc4OQ=`.

## Implemented boundary

`Adapters.Submission/TransactionEngine` now contains an internal, unregistered fixture boundary:

- `TransactionEngineWire` accepts the existing `PreparedSubmissionPackage` and immutable CT service-root bytes. It does not reference the Corporation Tax contract/serializer project and cannot reconstruct a CT600, accounts document or computation.
- It securely parses the root with DTD processing prohibited, no XML resolver and bounded document size; requires the approved CT namespace and exactly one empty generic `IRmark`; removes only the XML declaration needed to embed the root; calculates C14N 1.0 without comments over the GovTalk `Body` after removing the IRmark node; and replaces only that reserved element in the embedded root.
- It retains the original service-root digest, all prepared attachment digests, a client-generated upper-case hexadecimal `TransactionID`, the calculated IRmark and final wire SHA-256. The source package remains `Preview` and unchanged.
- Poll, delete and authenticated data-request messages contain protocol fields only. They cannot serialize statutory content.
- `TransactionEngineResponseParser` strictly validates the GovTalk class and message shape, constrains acknowledgement polling endpoints to configured exact HTTPS hosts, bounds errors/status records, preserves the exact response SHA-256 and distinguishes immediate gateway errors from correlated departmental business errors.
- A data response must contain exactly one status record matching the persisted client `TransactionID` before a lost acknowledgement can be reconciled to an HMRC `CorrelationID`.
- `FileTransactionEngineConversationStore` atomically persists `Prepared → SubmissionUncertain → AwaitingPoll → TerminalPersisted → DeletePending → Complete`. Submission becomes uncertain before the first exchange, so a restart cannot silently issue a second new submission. Terminal response and returned receipt references must be durable before delete. A delete failure retains `DeletePending` without changing the business outcome.
- Exact request/response bytes use the existing protected content store; routine conversation metadata keeps only bounded safe references and SHA-256 evidence.

`System.Security.Cryptography.Xml` 8.0.5 supplies the standard C14N transform. The earlier locally cached 8.0.1 servicing version was rejected after NuGet audit warnings; the selected 8.0.5 build reports no vulnerability warning.

## Offline evidence

The Phase 5.9 tests cover:

- exact reproduction of the official IRmark example and its pinned source digest;
- immutable service-root and attachment digests plus a separately calculated final wire digest;
- DTD/external-entity and oversize rejection;
- rejection of downgraded, suffix-confused and otherwise untrusted polling endpoints;
- acknowledgement correlation, interval enforcement and restart recovery;
- durable transaction-ID matching through `DATA_RESPONSE` after a lost acknowledgement;
- duplicate-submit prevention once an attempt is uncertain;
- correlation mismatch rejection;
- separate gateway and departmental business-error classifications;
- exact terminal-response and signed-receipt retention before delete; and
- delete failure/restart recovery without loss or mutation of the terminal outcome.

Verification results:

- Release `TaxHub.slnx`: 0 warnings, 0 errors;
- Company Contract Tests: 109 assertions, CT service-root digest remains `A3F3729F7236B342FC8C6D10C423104EA9B6F7C86CE86C396D78B2D586A77F2F`;
- Application Tests: 75 assertions;
- Submission Adapter Tests: 166 assertions;
- Architecture Tests: 25 assertions;
- VAT Contract Tests: 19 assertions;
- MTD Income Tax Contract Tests: 91 assertions and 44 endpoint descriptors;
- Trade Control Adapter Tests: 10 assertions;
- WebHarness hardening: 86 assertions;
- Companies House simulator: 31 assertions; and
- Data Provision source/preparation/handoff: supported `--offline` path passed because no database secret was supplied.

## Assumptions reserved for Phase 5.10

The fixture uses `HMRC-CT-CT600`, the class already observed in the official CT validation material and genuine TPVS response. Before any External Test Service call, Phase 5.10 must confirm that class, the current CT authentication/enrolment level, synthetic credential route, test response-host set, product/channel values and receipt-signature validation rules against current HMRC CT service guidance or Software Developers Support. HMRC confirmed on 21 October 2026 that recognition is optional and follows comprehensive testing, including ETS, so recognition scenarios are not an ETS prerequisite.

An IRmark match and a green offline state machine prove protocol mechanics only. They do not make the Objective 3 artifact submission-ready. `HmrcComputationTaxonomy2025.SubmissionReady` remains false, every CT artifact remains `Preview`, and `CorporationTaxSubmissionGateway` still rejects before outbound I/O.

## Review gate

Human review accepted the fixture provenance, IRmark boundary, uncertain-send recovery policy and terminal-before-delete ordering on 14 October 2026. The class, credential, channel and receipt assumptions remain Phase 5.10 confirmation work. Optional recognition is a later product-readiness decision. This acceptance permits local Phase 5.10 implementation but does not authorise an external HMRC send.
