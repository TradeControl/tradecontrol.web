# Corporation Tax Phase 5.10 — External Test Service local implementation

## Status

Local implementation is ready for human review as at 14 October 2026. It cannot send to HMRC in its current composition. No External Test Service request was made, the Phase 5.7 Corporation Tax family gate remains closed, and all CT artifacts remain `Preview` with `SubmissionReady=false`.

## Confirmed official route

HMRC's [basic guide for XML software developers](https://www.gov.uk/guidance/basic-guide-for-xml-software-developers) identifies the External Test Service Transaction Engine endpoints and explains that the service checks the GovTalk header and test credentials before routing the body to the Third Party Validation Service. For CT the local profile is pinned to:

- submission endpoint `https://test-transaction-engine.tax.service.gov.uk/submission`;
- message class `HMRC-CT-CT600`;
- `GatewayTest` value `1`;
- UTF-8 GovTalk XML sent by HTTP POST; and
- the polling endpoint returned by HMRC, restricted to the exact external-test host.

The retained SDST registration email confirms that the issued Vendor ID belongs in `ChannelRouting/Channel/URI`, the registered product name belongs in `Product`, and the supplied Sender ID, test UTR and one of the supplied test passwords are used for CT test submissions. The protected sources remain under `.local/hmrc/corporation-tax/credentials`; this record deliberately contains no values.

## Implemented local boundary

`HmrcCorporationTaxTestCredentialProvider` reads only the four approved values from the two SDST-supplied text files. It exposes them through the existing disposable `ProtectedSecret` boundary and rejects unapproved secret references. The credential source paths and resolved values are absent from diagnostic results.

`CorporationTaxOfficialTestMaterializer` reuses the frozen Objective 3 `PreparedSubmissionPackage`. It does not serialize accounts, computation or CT600 facts. At the final protocol boundary it:

1. resolves Vendor ID, Sender ID, password and test UTR;
2. inserts the Vendor ID in the official channel URI rather than the Phase 5.9 placeholder;
3. creates the `HMRC-CT-CT600` external-test GovTalk envelope with `GatewayTest=1`;
4. calculates and inserts the IRmark into the one reserved slot; and
5. retains service-root, attachment and final-wire SHA-256 identities.

The authenticated development-only WebHarness route is:

`POST /api/company/corporation-tax/official-test-preflight`

It uses the existing CT payload plus one durable upper-case hexadecimal `officialTest.transactionId`. Its response contains safe evidence only: class, intended endpoint, return period, service/attachment/final-wire digests and a digest of the IRmark. It never returns the credential-bearing XML or any credential value. The response always identifies the evidence as `HMRC ETS MATERIALISED — NOT SENT` and reports:

- `ExternalSendEnabled=false`;
- `CT-ETS-EXTERNAL-SEND-DISABLED`;
- `CT-RECOGNITION-EVIDENCE-PENDING`; and
- `CT-PRODUCTION-SUBMISSION-GATE-CLOSED`.

### Statutory-accounts identity boundary

Corporation Tax must not regenerate its full-accounts attachment from the mutable accounting database
after Companies House accounts have been approved. A new versioned
`CH-CT-ACCOUNTS-SNAPSHOT-1` manifest therefore binds the company identity, accounting period, filing
profile, source snapshot token, reviewed-input digest, Companies House filleted-document digest and
HMRC full-accounts-document digest. Both iXBRL projections are generated from the same populated
statutory-accounts contract during one Companies House preparation and retained as exact protected
bytes.

The approved-accounts snapshot reader verifies that manifest, the preparation and approval records,
the full-accounts content digest and any existing Companies House conversation before returning the
full iXBRL bytes to future CT orchestration. It never re-reads the ledger. Subsequent accounting-data
changes therefore cannot alter either filing lineage. Preparations created before this manifest was
introduced fail closed for CT use; the implementation does not reconstruct or infer a historical
cryptographic link.

The boundary deliberately distinguishes advance review from submission eligibility. The approved
reader may supply the frozen full accounts while Companies House processing continues, allowing the
user to review CT600 and the computation without waiting at the screen. The accepted-for-CT reader
returns a separately typed hand-off only when the matching Companies House conversation is exactly
terminal `Accepted`; prepared, acknowledged, pending, parked, rejected, missing and legacy
conversations cannot cross that gate.

The dedicated `TransactionEngineHttpGateway` provides the later one-attempt transport primitive. It validates the outbound digest and endpoint, sends one `application/xml; charset=utf-8` POST, applies a bounded timeout and response size, disables redirects, cookies and automatic decompression, and preserves exact response bytes plus SHA-256. It does not retry or decide whether another request is safe. It is internal and unregistered; the durable Phase 5.9 coordinator remains responsible for uncertain submission, `DATA_REQUEST`, polling, terminal persistence, receipt retention and deletion.

## Offline evidence

The expanded adapter tests prove:

- protected SDST-format credential parsing, including the malformed closing Sender ID tag present in the supplied plain-text email export;
- protected Vendor ID insertion into `ChannelRouting/Channel/URI`;
- the pinned external-test endpoint, message class and `GatewayTest=1`;
- preservation of the immutable service-artifact and attachment identities;
- one exact POST with XML media type and unchanged wire bytes;
- exact response retention and SHA-256; and
- redirect rejection without a second request.

Current verification:

- Submission Adapter Tests: 169 assertions;
- WebHarness hardening: 87 assertions;
- Architecture Tests: 26 assertions;
- Company Contract Tests: 109 assertions;
- Trade Control Adapter: 10 assertions;
- Application: 75 assertions;
- Companies House simulator: 31 assertions;
- VAT contracts: 19 assertions;
- MTD Income Tax contracts: 91 assertions and 44 endpoint descriptors;
- Data Provision source, artifact and corporate hand-off coverage passed in supported `--offline`
  mode because `TC_NODE_CONTEXT` was intentionally not supplied;
- the protected local SDST sources resolved all four approved values (4/4) without displaying them;
- solution build: zero warnings and errors at the implementation checkpoint; and
- TCWeb Tax Hub boundary tests: 164 assertions, including exact retrieval, legacy/tamper rejection
  and refusal to release the CT hand-off before Companies House acceptance; and
- no network request to HMRC.

## Remaining review and execution gate

Human review should confirm this protected configuration and send-disabled preflight before any credentialed external-test attempt. A later explicitly authorised test execution must register the one-attempt transport, retain the exact request/response evidence, observe submit → acknowledgement → poll → terminal → receipt → delete, and record the HMRC recognition result. That authorisation would apply to the named external-test attempt only; it would not enable live filing or relax the production family gate.

The new-applicant-pack request was sent to HMRC Software Developers Support Team on 14 October 2026
with the issued Vendor ID. Until HMRC supplies its current checklist and recognition scenarios, local
implementation and offline verification may continue, but no application completion or recognition
claim can be made.

## Draft April 2027 material received

On 16 October 2026 HMRC SDST circulated draft `CT600 04-27 v0.1` and `CT600L 04-27 v0.1` forms,
together with CT600 and CT600E guidance-change schedules. Developer feedback is requested by
30 November 2026. This circular is separate from, and does not satisfy, the outstanding request for
the new-applicant pack and assigned recognition scenarios.

The retained source identities are:

| Evidence | SHA-256 |
| --- | --- |
| `CT600 (2027) - Draft forms and guidance.txt` | `9124259FAE35F655B4DB805CBB56C73D711A14501878C3729A33D2C1DF602B0F` |
| `CT600 04-27 v0.1.pdf` | `F2A6A05F7BC5D4472CFB9A2B3C77C79C34C35C01E7981DAA6C69C12073EE69D1` |
| `CT600L 04-27 v0.1.pdf` | `A1AC45AA6CC6E42D376A5D6F5F60FC76DDC023F6AE2F01F05603225791B5435A` |
| `CT600 Guidance Changes 2027.docx` | `CC93648B85DE6BD727443538948EEB75725287DB6C4CC45D59BFDE661E2C3A2C` |
| `CT600E Guidance Changes 2027.docx` | `7F3696464CB7996392EF1588A670AD0DFCD69AEE8F87942CB3200A2840D6A9EB` |

The draft evidence identifies future contract-review items: 40% first-year-allowance boxes 696 and
739 for qualifying expenditure from 1 January 2026; changed end-date and validation treatment for
electric charge-point and zero-emission-car allowances; optionalised CT600E legacy-detail wording;
and an expanded CT600L R&D-credit surface. The current supported fixture does not populate CT600E or
CT600L, but the capital-allowance change can be relevant to an accounting period containing
qualifying expenditure and must be assessed when final executable artifacts are available.

HMRC supplied no April 2027 RIM, XSD, Schematron, calculator or effective contract-selection
instruction with this draft. Accordingly it is consultation/change-watch evidence only. RIM 1.994,
the immutable Objective 3 artifact and all Phase 5.10 submission/recognition gates remain unchanged;
no schema fields or validation rules may be inferred from these documents.
