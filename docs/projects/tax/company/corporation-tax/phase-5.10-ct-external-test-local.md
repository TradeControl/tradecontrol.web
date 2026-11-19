# Corporation Tax Phase 5.10 — External Test Service local implementation

## Status

CT-B was accepted by human review on 18 November 2026. CT-C now provides a Development-only,
authenticated diagnostic over the durable one-exchange Transaction Engine conversation. The first
separately authorised ETS conversation completed with retained HMRC evidence and a ChRIS 5005 UTR
identity mismatch; it did not produce an accepted return. The poll/delete protocol defects exposed
by that conversation are corrected and covered by tests. After exact-artifact review and separate
authorisation, the identity-corrected conversation completed with a genuine HMRC
`BusinessResponse`, retained IRmark receipt, zero response errors and acknowledged delete. CT-C was
accepted by human review on 18 November 2026.

The Phase 5.7 Corporation Tax family gate remains closed, production submission remains disabled,
and all CT artifacts remain `Preview` with `SubmissionReady=false`.

**Programme status (22 October 2026): paused for the multi-tenant environment.** HMRC has confirmed
that recognition is optional and that ETS testing precedes any recognition application. The owner
has nevertheless chosen to defer the first controlled ETS attempt until the production-ready
multi-tenant environment is implemented and reviewed, so that CT and VAT resume against the
intended topology. This pause is not an HMRC prerequisite and does not reopen the accepted CT2025,
RIM 1.994, service-artifact or Phase 5.9 protocol decisions.

**Resumption status (11 November 2026):** the reviewed multi-tenant hosting prerequisite is complete
and the programme has resumed Corporation Tax. The next action is CT-A in the
[recognition roadmap](ct-recognition-roadmap.md): recommission the selected disposable test dataset
for the approved current accounting period, rebaseline it under Category Tree Change Control,
refresh the approved validation evidence and return with a send-disabled Phase 5.10 preflight. No
external HMRC request or production activation is authorised. Concrete tenant, server and database
identities remain in protected operational records.

**CT-A inventory update (11 November 2026):** the selected integration-test tenant is technically
ready but contains a historical company period that is not CT2025-eligible. The protected inventory
and recommissioning instructions are retained outside the public repository. Phase 5.10 remains
send-disabled while the current-period scenario is commissioned and reviewed.

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

It uses the existing CT payload plus one durable upper-case hexadecimal `officialTest.transactionId`.
The request also carries explicit references for the accepted CT-A baseline, the approved exact
service-artifact SHA-256 and the named-attempt authorisation bound to that transaction ID. Its
response contains safe evidence only: class, intended endpoint, return period,
service/attachment/final-wire digests, a digest of the IRmark and Boolean gate results. It never
returns the credential-bearing XML, a credential value or protected source location. The response
always identifies the evidence as `HMRC ETS MATERIALISED — NOT SENT` and reports:

- `ExternalSendEnabled=false`;
- `CT-ETS-EXTERNAL-SEND-DISABLED`; and
- `CT-PRODUCTION-SUBMISSION-GATE-CLOSED`.

Until their corresponding evidence is present, the preflight additionally reports a distinct
blocker for missing baseline approval, missing or mismatched exact-artifact approval, unavailable
protected configuration, or missing/mismatched named-attempt authorisation. Recognition is not an
ETS prerequisite and `CT-RECOGNITION-EVIDENCE-PENDING` is no longer emitted.

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
full iXBRL bytes to future CT orchestration. That reader deliberately returns the frozen filing artifact;
it is not the current transaction-data validator. CT orchestration must separately rebuild a non-filing
comparison projection from Trade Control transactions and fail closed unless it reconciles to the
accepted accounts. Subsequent accounting-data changes therefore cannot alter either filing lineage or
pass silently into CT. Preparations created before this manifest was introduced fail closed for CT use;
the implementation does not reconstruct or infer a historical cryptographic link.

The boundary deliberately distinguishes advance review from submission eligibility. The approved
reader may supply the frozen full accounts while Companies House processing continues, allowing the
user to review CT600 and the computation without waiting at the screen. The accepted-for-CT reader
returns a separately typed hand-off only when the matching Companies House conversation is exactly
terminal `Accepted`; prepared, acknowledged, pending, parked, rejected, missing and legacy
conversations cannot cross that gate.

The dedicated `TransactionEngineHttpGateway` provides the one-attempt transport primitive. It
validates the outbound digest and endpoint, sends one `application/xml; charset=utf-8` POST, applies
a bounded timeout and response size, disables redirects, cookies and automatic decompression, and
preserves exact response bytes plus SHA-256. It does not retry or decide whether another request is
safe. An internal `TransactionEngineExchangeCoordinator` now composes that primitive with the
durable Phase 5.9 state machine for one submit, `DATA_REQUEST`, poll or delete exchange at a time.
It applies each exact response through the durable conversation before returning. Neither class is
registered in WebHarness or another host, and the architecture suite forbids this coordinator from
being introduced into WebHarness.

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

- Submission Adapter Tests: 178 assertions;
- WebHarness hardening: 91 assertions;
- Architecture Tests: 32 assertions;
- Company Contract Tests: 115 assertions;
- Trade Control Adapter: 11 assertions;
- Application: 90 assertions;
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

The completed run retained exact request/response evidence and observed submit → acknowledgement →
poll → terminal → receipt → delete. Its authorisation applied only to that named ETS attempt; its
acceptance opens CT-D product-journey work but does not enable live filing, relax the production
family gate or constitute a recognition application.

The new-applicant-pack request was sent to HMRC Software Developers Support Team on 14 October 2026
with the issued Vendor ID. HMRC replied on 21 October that recognition is optional and must follow
research, comprehensive testing including extensive ETS use, and a commercially mature product
ready for release. CT Recognition 2025 is closed; HMRC is preparing the 2026 instructions. The
retained reply has SHA-256
`AC045975953F60F473B7CEDECEC1BFE184A6410714CB477F77ED58CAF34ACE37`.
Accordingly, an applicant pack is no longer a Phase 5.10 dependency. Recognition may be considered
after ETS and product-readiness evidence exists; no recognition or production status is claimed.

## Draft April 2027 material received

On 16 October 2026 HMRC SDST circulated draft `CT600 04-27 v0.1` and `CT600L 04-27 v0.1` forms,
together with CT600 and CT600E guidance-change schedules. Developer feedback is requested by
30 November 2026. This circular is separate from HMRC's later recognition-sequencing reply and has
no bearing on whether Phase 5.10 may perform ETS testing.

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

### Draft v0.2 update received 18 November 2026

HMRC subsequently supplied v0.2 forms and expanded change schedules. The material is still draft
consultation evidence: it contains no RIM, XSD, Schematron, calculator, taxonomy package or
instruction changing the executable contract selected by ETS. The retained source identities are:

| Evidence | SHA-256 |
| --- | --- |
| accompanying email export | `93F8E46FC224FE9B47F11B452D6CE40E2F23C8EAF7E4E5B110E344F2C865F0C0` |
| `CT600 04-27 v0.2.pdf` | `783D046BB88A91D5FB10982BCAAEDC9C52D7CF00E3EE1D274495E4DA527091D5` |
| CT600A guidance changes | `46250126BE347E3A15C53432BE4AA90A428CB822E7757820011647BB43591229` |
| CT600E guidance changes | `C32B7A214857F5202C7FBBB7904A2024A42003E6C254D4B3FAEC55E651E37B67` |
| `CT600L 04-27 v0.2.pdf` | `9F411450A29334ECF2DD4E73C2F36BE8FDC02CBFE01C9AD877DDB0C939C6DEAD` |
| CT600L guidance changes | `B5132E885093DCECDF22588F58C14D0AB964F83429CC1C71650142409586AEF0` |
| CT600 guidance changes | `9C98B8E30A74E01B1D523CEC4C9D7EF51E6FED7C48F9A053938D80C6DB2FBC86` |

The v0.2 review records the following impact:

- CT600 boxes 696 and 739 are renamed for the Main Rate first-year allowance: 40% for qualifying
  expenditure in 2026 and 50% from 1 January 2027. This does not alter the approved target-period
  artifact, but final executable assets must be reviewed before supporting the later allowance.
- CT600A guidance changes the section 455 rate to 35.75% from 6 April 2026. A return period ending
  30 June 2026 can overlap that commencement date. The current supported CT600A boundary accepts a
  separately reviewed tax-charge input; a future calculation source must split/apply the lawful rate
  and fail closed where the applicable result has not been reviewed. No rate is inferred into the
  current serializer from draft guidance.
- CT600E and CT600L clarify specialist/legacy aggregation, RDEC carry-forward and surrender wording.
  They remain outside the initial supported filing profile.
- Comparison of the v0.1 and v0.2 main/CT600L form surfaces found no new supported box requirement;
  the visible changes are guidance or layout rather than an executable contract migration.

Therefore CT-B continues against CT2025 and RIM 1.994. The draft does not change the immutable
service artifact, validation baseline, ETS endpoint selection or send-disabled decision.
