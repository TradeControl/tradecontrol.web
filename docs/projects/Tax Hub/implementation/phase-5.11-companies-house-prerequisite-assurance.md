# Phase 5.11 — Companies House prerequisite assurance

## Outcome updated 6 October 2026

The approved internal portion of Phase 5.11 is implemented and remains fail-closed. Published TIS 6.0 and live schemas support the Objective 3 accounts correction and contract modelling. Companies House created the requested developer test account on 6 October 2026 and supplied the presenter-side values needed for controlled integration with its test gateway. Submission `S00013` has now received an error-free synchronous gateway acknowledgement. That acknowledgement establishes receipt, not the terminal filing decision, developer-test acceptance, manual-review acceptance or production approval.

Controlled test filings have been sent under separate one-shot human authorisations; no live filing has been sent. The application was submitted at 12:22 on 3 October 2026. Companies House's 6 October account response, its protected values and exact protected exchange evidence are retained under the git-ignored `.local/companies-house/test-account` area. No credential value is recorded in tracked documentation, fixtures, source code or ordinary diagnostics.

The account-issuance and test-presenter-credential dependencies are resolved. The remaining inputs and evidence are:

1. poll `S00013` through the separately modelled specific `GetSubmissionStatus` operation without resubmitting the accounts;
2. preserve any pending, parked, rejected, failed or accepted authority result as protected evidence;
3. notify the XML team of the submitted test when directed by the reviewed workflow; and
4. obtain the terminal status, current testing-criteria and manual-review evidence before submission readiness can change.

The absence of a separately attached testing-criteria document does not block construction of the adapter or the first controlled test under the published instructions. It does block any claim that Companies House testing is complete. A company authentication code is not part of the issued presenter account; it is a normal per-company filing input and need not be supplied by the XML team.

## External test-account evidence — 6 October 2026

The retained Companies House correspondence confirms that the XML software-filing test account has been created. It supplies four protected/configuration values: a test presenter ID, presenter authentication value, test flag and allocated test package reference. It further instructs that submission numbers must be unique and incremental, otherwise they will be rejected immediately, and that the XML team must be told when tests have been submitted so manual review can occur.

The response does not include the requested current testing-criteria document, a company authentication value, a test result, manual-review acceptance or production presenter approval. This is not evidence that the issued test account is unusable: presenter credentials and the test package configuration are sufficient to target the test gateway once the chosen test company's separate authentication code is supplied. The published XML Gateway address remains authoritative published material rather than a new value supplied by the correspondence. Account creation therefore resolves access and presenter-credential availability, but not final envelope completeness or developer-test acceptance.

## Authoritative selection

- The official GOV.UK publication was updated on 25 September 2026. Its current accounts supplement is **version 6.0**, not TIS 5.9. The document-control entry states that version 6.0 was issued in September 2026 and introduced overseas functionality plus the missing mandatory average-employees note information.
- The current general filing TIS download is named `Companies_House_TIS_v5.3.odt`. Its title page still says version 5.2 dated 7 April 2022, while its document-control history records version 5.03 in June 2024. The official publication continues to present that file as the current general TIS.
- The current schema-status register marks `FormSubmission-v2-11.xsd`, `GetSubmissionStatus-v2-9.xsd` and `GetStatusAck-v1-1.xsd` live. The GovTalk examples reference `Egov_ch-v2-0.xsd`.
- Accounts are sent as the exact self-contained iXBRL file, base64 encoded in `FormSubmission/Document/Data`, with `ContentType` `application/xml` and `Category` `ACCOUNTS`. They are not sent in a `CompanyAccounts` body.
- The accounts supplement recognises audit-exempt micro-entity accounts and requires the FRS 102 entry point for FRS 105 micro entities. The supported account document must identify the micro-entity accounting standard and unaudited status, and contain all current mandatory facts and balance-sheet statements.

## Acquired-asset ledger

The files below were downloaded from the official GOV.UK or Companies House XML Gateway locations into a temporary review directory only. They were not added to the repository.

| Asset | Official URL | Bytes | SHA-256 | Finding |
|---|---|---:|---|---|
| `Companies_House_TIS_v5.3.odt` | `https://assets.publishing.service.gov.uk/media/66618d1d2aec9626ea8805c1/Companies_House_TIS_v5.3.odt` | 157,357 | `6155AACFDB2D80369ABC8D12F82EB6FF5AD52C8667CD5079B279B6A4B5649E80` | Current general filing TIS download. |
| `TIS_September_2026.odt` | `https://assets.publishing.service.gov.uk/media/6ab53893fceb6fb3a65011b6/TIS_September_2026.odt` | 316,948 | `6C1E4D516B8DAE50E1C34E9B453C5209F6958034CDDB88F1CEB7E5FF331CD05D` | Current accounts supplement, version 6.0. |
| `SchemaStatus.html` | `https://xmlgw.companieshouse.gov.uk/SchemaStatus` | 128,331 | `23D9095D2797EA2B330714FC7BE8B55660BDB969B63ECA5F700CDF74EFCFB232` | Snapshot of live/deprecated schema status on 3 October 2026. |
| `Egov_ch-v2-0.xsd` | `https://xmlgw.companieshouse.gov.uk/v2-1/schema/Egov_ch-v2-0.xsd` | 11,303 | `27ED00933423964F3E00C165FCDFE975632AAF58CACDE31C2C9241638AFD51A1` | GovTalk envelope schema used by current official status examples. |
| `FormSubmission-v2-11.xsd` | `https://xmlgw.companieshouse.gov.uk/v1-0/schema/forms/FormSubmission-v2-11.xsd` | 8,551 | `57AAC40066B002F772ADBF9A880DFE337AEA3253C13451509BC34F89F6FA991C` | Live filing wrapper; includes the company-authentication slot and accounts document attachment. |
| `GetSubmissionStatus-v2-9.xsd` | `https://xmlgw.companieshouse.gov.uk/v1-0/schema/forms/GetSubmissionStatus-v2-9.xsd` | 4,044 | `BFB920A8DF64604A47AACB864D090DA98E597F971CE526865707557995418054` | Live status request/response body. |
| `baseTypes-v3-4.xsd` | `https://xmlgw.companieshouse.gov.uk/v1-0/schema/baseTypes-v3-4.xsd` | 78,253 | `C6C987305E9B3D70522B7AD127B18B778F6D8CC90C03AC7C8B766740FF95C79A` | Dependency named by the live status schema. |
| `GetStatusAck-v1-1.xsd` | `https://xmlgw.companieshouse.gov.uk/v1-0/schema/forms/GetStatusAck-v1-1.xsd` | 726 | `B1BFFEBB5C5AFF2FBC5218AE9F4EB0B8914A8687704EA1ED17B0ABF103BF5B6A` | Live acknowledgement body. |
| `baseTypes-v1-1.xsd` | `https://xmlgw.companieshouse.gov.uk/v1-0/schema/baseTypes-v1-1.xsd` | 47,246 | `93D54A3850907207CBA39EDE010677B2D11D4FFAEB11B10D2C6BA09E213B7759` | Dependency named by the acknowledgement schema. |
| `GetSubmissionStatus_request.xml` | `https://xmlgw.companieshouse.gov.uk/examples/GetSubmissionStatus_request.xml` | 1,427 | `AF2C0B5341899F4AEE4419B22B5937BB697969A4593D38CB0EE348EDE954FC59` | Official example, but still references deprecated status schema 2.5. |
| `GetSubmissionStatus_response.xml` | `https://xmlgw.companieshouse.gov.uk/examples/GetSubmissionStatus_response.xml` | 9,623 | `86BD12E70F4E74BC23D01458C9F3F43987B9BE4E6950901CC19AFEAAD0C4EA60` | Official ACCEPT/REJECT example, also tied to the older example family. |
| `GetStatusAck.xml` | `https://xmlgw.companieshouse.gov.uk/examples/GetStatusAck.xml` | 1,233 | `2F7A53D9094C9139C7523A887C0AA42E36F13EF744599EB9B8A84C43393865E1` | Official acknowledgement example. |
| `AccountsImage_Request.xml` | `https://xmlgw.companieshouse.gov.uk/examples/AccountsImage_Request.xml` | 28,666 | `AD40F19DEB837BDE0D66F2CDC0CAA4E84DB78DAAFE1E92AFB0E7A831D864FBDB` | Image-service example only; references FormSubmission 2.5 and a 2009 taxonomy, so it is not a current filing golden. |
| `AccountsImage_Result.xml` | `https://xmlgw.companieshouse.gov.uk/examples/AccountsImage_Result.xml` | 45,378 | `5FBAEA27BEA0486E713D5C1509A1C33401A6369F89C4D28B1743755133D81A0C` | Image-service response only, not filing acceptance evidence. |

`FormSubmission-v2-10.xsd` was also checked because the schema-status page attaches the image examples to it. It is live and differs from 2.11 only by two additional document categories in 2.11; 2.11 remains the latest live wrapper. Its size is 8,469 bytes and SHA-256 is `9E94245F4BF050D7722365ED5D4E1720197831BC7F57047C70B75C1946DBAC86`.

## Filing and lifecycle reconciliation

| Concern | Official contract | Current preview | Required correction |
|---|---|---|---|
| Envelope/body | GovTalk envelope with a `FormSubmission` body in `http://xmlgw.companieshouse.gov.uk/Header`. | `CompanyAccounts` in a fabricated `.../schema/forms/CompanyAccounts-v1-0` namespace. | Replace the logical serializer with a separately named official filing-artifact builder; never promote the existing bytes. |
| Class and routing | Filing Class identifies the submitted document type; the general TIS lists `AA` for accounts. `Qualifier=request`; `Function=submit` is permitted. | `Class=CompanyAccounts`, `Qualifier=request`, `Function=submit`. | Pin the accounts Class and other message fields from developer-test evidence; `CompanyAccounts` is not an official filing schema/class. |
| Session/correlation | `TransactionID` identifies one gateway session, must advance, and is distinct from the six-character `SubmissionNumber` used with presenter ID after parse acceptance. | One unconstrained `EnvelopeNumber` is placed in `CorrelationID` and reused as a REST-like status key. | Model session transaction ID and submission number separately; enforce their different format and uniqueness policies. Do not treat `CorrelationID` as the filing identity. |
| Presenter authentication | GovTalk `Header/SenderDetails/IDAuthentication` contains hashed presenter `SenderID`, `Authentication/Method=clear`, hashed authentication `Value`, and normally email. | No `SenderDetails` or presenter-authentication slot. | Add a narrow transport-time credential slot outside `PreparedStatutoryArtifact`; Phase 5.12 supplies protected values. |
| Company authentication | `FormSubmission/FormHeader/CompanyAuthenticationCode`, six to eight characters; required for accounts authority. | Absent. | Add a narrow transport-time company-authentication slot outside the prepared statutory artifact and include it only when materialising the sendable envelope. |
| Filing metadata | `FormHeader` carries numeric company number plus company type where applicable, company name, package reference, `FormIdentifier`, exactly six-character submission number and optional contact/customer reference; `DateSigned` is required. | Uses company number, period, logical delivery and declarations in the invented body; no package reference, form identifier, submission number or date-signed contract. | Establish a reviewed filing-header record. Package reference is Companies-House allocated and must not be invented. Date signed must derive from reviewed approval. |
| Accounts bytes | Exact self-contained iXBRL bytes are base64 encoded once in `Document/Data`; filename is at most 32 characters, content type is `application/xml`, category is `ACCOUNTS`. | Exact prepared bytes are base64 encoded, but under invented `AccountsData`; current sample filename exceeds 32 characters. | Preserve the iXBRL source bytes/digest and encode them in the official `Document`; enforce official filename/media/category constraints. |
| Full/omitted content | The accounts supplement recognises audit-exempt micro-entity accounts. It does not define a `Delivery=Filleted` envelope value. Required iXBRL facts and statements determine the submitted content; if a trading micro omits profit and loss, the micro statement must also reflect the small-companies regime. | A separate `Delivery` enum controls omission of profit-and-loss facts and is emitted in the invented envelope. | Retain the reviewed full source, but replace `Delivery` wire semantics with an explicitly validated accounts-document profile and exact required statement/fact set. Do not infer gateway support from the word `Filleted`. |
| Immediate acknowledgement | Successful synchronous parse returns an acknowledgement with gateway timestamp and may supply poll guidance. Parser/authentication errors appear in `GovTalkErrors`; parse failure means the accounts were not delivered. | One `CompaniesHouseSubmissionAcknowledgement` collapses receipt and later status into `Received/Pending/Accepted/Rejected`. | Separate gateway acknowledgement/error from asynchronous submission status. |
| Status polling | Submit a GovTalk `GetSubmissionStatus` request with presenter ID and optionally submission number/company number. Live status codes are `ACCEPT`, `REJECT`, `PENDING`, `PARKED`, `INTERNAL_FAILURE`. General polling may return up to 20 completed statuses. | `PollUntilTerminal` with fabricated path `submission-status/{envelopeNumber}`; state lacks parked/internal-failure. | Replace the path with an XML status operation descriptor and typed request/response contract keyed by presenter/submission semantics. |
| Status acknowledgement | After a general status poll, send GovTalk Class `StatusAck` with the empty `StatusAck` body; otherwise completed responses can repeat. It is not required for a specific-submission poll. | No acknowledgement operation. | Model conditional status acknowledgement separately; transport remains Phase 5.12. |

## Objective 3 correction required before Phase 5.11 implementation

The user approved this smallest coherent correction on 3 October 2026, and it was implemented as follows:

1. Change the pinned Companies House accounts contract from TIS 5.9 to the current TIS 6.0 and update provenance/effective-date evidence without marking it submission-ready.
2. Correct the Objective 3 accounts iXBRL profile before preserving any new golden. The current projection omits TIS 6.0 mandatory facts including reporting-period start/end, audited/unaudited status, accounts-type dimension, accounting-standard/micro-entity dimension, trading status where required, signing-director identity and the required unaudited micro balance-sheet statements. It currently carries only three logical registrar booleans and omits the section 477 statement. These must become correctly tagged document facts/statements rather than invented envelope elements.
3. Replace the `Full`/`Filleted` wire assumption with reviewed full and filing-content profiles whose omitted sections and statement wording satisfy TIS 6.0. Preserve the reviewed full accounts source and exact resultant iXBRL bytes/digests; do not mutate them in transport.
4. Introduce separate immutable contract values for the official GovTalk/FormSubmission filing artifact, gateway acknowledgement/error, `GetSubmissionStatus` request/response and conditional `StatusAck`. Keep presenter and company credentials as narrow unresolved transport slots, not fields on `PreparedStatutoryArtifact`.
5. Correct operation identity and polling metadata: `TransactionID` is not `SubmissionNumber`, `CorrelationID` is not the status key, and no REST-like status path exists.

This is an Objective 3 filing-contract correction because it changes the statutory accounts document and authority package shape, not merely Objective 4 transport. New goldens must not be treated as Companies House-accepted until developer-test evidence exists.

## External prerequisite and unblock evidence

The public assets are sufficient to disprove the current preview, but not to claim a complete accepted filing implementation:

- the official status examples reference deprecated schema 2.5 rather than the live 2.9 schema;
- the only linked accounts example is an old Accounts Image request using FormSubmission 2.5 and a 2009 taxonomy, not a current TIS 6.0 full or filing-content micro-entity golden;
- the accounts TIS states that Companies House gives software testers access to its internal accounting validation rules; and
- official developer guidance requires a test account and test presenter credentials, gateway parsing, manual Companies House review of pending submissions, and confirmation that the product meets its testing criteria.

The official route is through `xml@companieshouse.gov.uk`. The application was submitted at 12:22 on 3 October 2026 and the account plus protected presenter test values were supplied on 6 October 2026. A protected synthetic company-authentication value has enabled controlled test requests. The error-free synchronous acknowledgement for `S00013` establishes that the gateway accepted the credentialed exchange for downstream processing; it does not establish a terminal filing decision or manual test acceptance. The repository now has observed routing, embedded-document validation and acknowledgement behaviour, but still needs current criteria/version evidence, a terminal micro-entity test result and the Companies House review outcome. Local checks, account creation and a gateway acknowledgement do not establish production approval.

## Human review gate

Internal published-specification implementation and Block A are accepted. The adapter hop through protected materialisation, offline transport and evidence-led one-shot exchanges has reached an official synchronous acknowledgement for `S00013`. The next external step is a specific `GetSubmissionStatus` poll for that existing submission, not another accounts submission. Current criteria, terminal-status and developer-test/manual-review evidence remain required before submission readiness can change.

Phase 5.12 external transport activation remains out of scope until its explicit human send gate. Submission readiness, developer-test acceptance and production approval remain false.

### Adapter-hop increments — approved direction 6 October 2026

1. **Gateway boundary:** route the development conversation through a transport-neutral raw-XML exchange boundary. Keep the in-process Block A implementation and add an official-test implementation that is visibly and mechanically send-disabled. No credentials or network I/O.
2. **Protected materialisation:** resolve presenter ID, authentication value, test flag, package reference and company authentication from protected configuration; insert them only into the final wire envelope; prove redaction and exact accounts-byte continuity. Still no send.
3. **Offline official transport:** add the pinned test host, bounded HTTP behaviour and parsers using recording handlers and authoritative/captured fixtures. Exercise the unchanged Application conversation through both implementations. Still no external send.
4. **Controlled official test:** present the fully materialised request digest, host, filing identity and gates for human approval; make exactly the authorised test submission; poll without inventing a second filing; retain redacted official evidence and notify the XML team as instructed.

Each increment ends in human review. Official behaviour replaces simulator assumptions wherever they differ. Simulator extensions are added only for a demonstrated regression or recovery need.

Increment 1 uses the explicit development setting `TaxHub:CompaniesHouse:DiagnosticGateway`. `Simulator` selects Block A. `OfficialTestSendDisabled` selects the official-test placeholder and proves that it rejects the exchange before any network facility exists. Absence of the setting fails closed.

### Increment 2 outcome — protected materialisation, 6 October 2026

Increment 2 is implemented without transport activation:

- `CompaniesHouseEnvelopeMaterializer` accepts only the reviewed TIS 6.0 preview package and its two known fail-closed findings; any other preparation error is rejected.
- For `FormSubmission`, presenter ID and presenter authentication are resolved as disposable protected values and MD5 encoded only at final wire materialisation, as required by current Companies House developer guidance. Their clear values are absent from that submission XML. The separately published status-query body necessarily contains the clear presenter ID as well as the hashed GovTalk sender ID; that credentialed status XML is therefore equally protected and must not be logged or returned by diagnostics.
- The issued test flag and package reference are resolved from protected configuration. The materialiser requires test flag `1` and a four-digit package reference.
- The selected company's six-to-eight-character authentication code is resolved separately and inserted only into `FormSubmission/FormHeader/CompanyAuthenticationCode`.
- `TransactionID`, `GatewayTest` and `SenderDetails/IDAuthentication` are inserted only into the final GovTalk envelope. The credential-free prepared artifact remains immutable.
- The decoded accounts attachment is compared byte-for-byte with the prepared iXBRL and its SHA-256 before the final wire digest is returned.
- The safe result exposes only preview, wire and accounts digests with evidence class `MATERIALISED — NOT SENT`; it exposes no credential fields and has no network client.

The existing git-ignored `.local/companies-house/test-account/credentials.txt` labels were checked without displaying their values. All four issued entries are present; test flag and package-reference shapes satisfy the materialiser. A strict labelled-text provider now maps those four entries without copying them into source or tracked configuration. Company authentication is deliberately a separate routed protected value because it is not part of the issued presenter account.

The implementation is proved with synthetic protected values only. No real credentialed envelope has been generated because no company authentication value has yet been supplied to this boundary. The official-test gateway remains send-disabled. At the Increment 2 checkpoint, HTTP work and every external request remained subject to further human review; Increment 3 was subsequently authorised without authorising an external request.

### Increment 3 outcome — offline official transport, 6 October 2026

Increment 3 is implemented as offline adapter evidence only:

- the endpoint is pinned to the current GOV.UK `Test link`, `https://xmlgw.companieshouse.gov.uk/v1-0/xmlgw/Gateway`; host, HTTPS default port, exact path, empty query and empty fragment are all enforced;
- an internal, non-public `CompaniesHouseOfficialTestHttpGateway` sends exact request bytes using `POST` and `text/xml; charset=utf-8`, with a bounded timeout, no retry and a bounded response body;
- the adapter validates the final response URI, HTTP status, secure XML parse and operation/qualifier pairing before returning a response;
- acknowledgement metadata and GovTalk errors are parsed into the existing typed contract, including the alternate published legacy GovTalk response namespace currently observable from the gateway;
- `GetSubmissionStatus` responses are validated through the existing typed status parser, and `StatusAck` acknowledgements are classified separately;
- offline evidence is explicitly `OFFICIAL CONTRACT — OFFLINE HTTP — NOT FILED` and preserves exact request/response hashes; and
- recording-handler tests prove the pinned URI, method, media type, exact request bytes, acknowledgement/status/error parsing, host rejection and response-size gate.

The HTTP implementation is internal to the Submission adapter, has no public constructor and is not registered in WebHarness or any production host. The only selectable official implementation remains `SendDisabledCompaniesHouseOfficialTestGateway`. Therefore Increment 3 introduces production-shaped code but no application route capable of reaching Companies House. No external request was made.

### Increment 3 closure — protected polling and acknowledgement requests, 6 October 2026

The no-send transport boundary now covers the complete published asynchronous request family rather than only `FormSubmission`:

- `GetSubmissionStatus` resolves presenter ID, presenter authentication and test flag at final materialisation, inserts the same published GovTalk test-authentication header and preserves the selected submission/company/presenter query scope;
- the required clear `PresenterID` status-body value is created inside the protected materialiser and is not accepted as an ordinary WebHarness input;
- status polling deliberately does not resolve company authentication or package reference because neither belongs to the published status contract;
- conditional `StatusAck` receives its own protected GovTalk header while preserving the published empty `StatusAck` body; and
- safe results expose the gateway operation, final bytes, wire digest and `MATERIALISED — NOT SENT` evidence class, but no credential properties.

This shape was checked against the official Companies House `GetSubmissionStatus_request.xml` and `GetStatusAck.xml` examples. The former still cites the deprecated 2.5 body schema, so the repository retains its separately verified live 2.9 body contract while using the examples as evidence for the common GovTalk header only. The offline Submission adapter suite passes 131 assertions. The HTTP implementation remains internal and unregistered, and no external request was made.

### Visible official-test preflight — 6 October 2026

The Development WebHarness now exposes `POST /api/company/companies-house/accounts/official-test-preflight` so the no-send progress can be inspected without reading console-test assertions. It:

- executes the real `ICompaniesHouseAccountsRunner` database/preparation path from the supplied WebHarness payload;
- reads the issued presenter configuration from the git-ignored labelled file and checks the separate company-authentication source without returning either value;
- materialises the final envelope in memory only when both protected sources are available;
- returns the preparation route, pinned intended test endpoint, preview/accounts/wire SHA-256 values, boolean protected-configuration status and named blocking gates; and
- returns neither the credentialed XML nor any presenter/company authentication value.

The result is labelled `OFFICIAL TEST PREFLIGHT — NOT SENT`, and `ExternalSendEnabled` is always `false`. The HTTP gateway remains internal and unregistered. At the original checkpoint the separate company authentication input was absent, so no wire digest could be produced. That protected input is now available: current preflights can materialise the complete credentialed envelope and report its digest while retaining the external-send and developer-test-evidence blockers. This is observable preparation evidence, not a Companies House response or test acceptance.

The next review decision is whether to prepare Increment 4's controlled-test activation. That work first needs the chosen company's authentication code, a reviewed unique submission/transaction identity and explicit human approval of the exact host and safe request digests. It must not infer send permission from completion of this offline increment. Any first official exchange should use a specific-submission poll, which does not require `StatusAck`; the general-poll acknowledgement path remains deterministic offline regression infrastructure unless observed official behaviour creates a concrete need for it.

### First controlled official-test exchange — 6 October 2026

Human review authorised one test-gateway submission using synthetic six-character company authentication `TST001`. The code was kept in the git-ignored protected configuration and was not added to tracked source or documentation. Preflight prepared the company MIN database package, resolved the issued presenter configuration and synthetic company authentication, and materialised wire digest `5C5DDE6932C46D25FCE3EE6C406E23A2D4D28CC109BA0F1248E8D1FBCA0C0C87` while send remained disabled.

A separately launched Development process then enabled a two-key, in-memory one-shot gate: launch-time `OfficialTestSendEnabled` plus request-time explicit authorisation. It retained the protected request before dispatch, sent submission `S00001` with transaction `2026100600000001` exactly once to the pinned official test endpoint, disabled automatic redirects and retries, and consumed the process gate. The temporary send-enabled process was stopped immediately afterwards.

Companies House returned a synchronous GovTalk error rather than a filing acknowledgement:

- response kind: `GatewayError`;
- authority transaction ID: `2026100600000001`;
- error `501`: `Invalid Gateway Target (Class) supplied [AA]`;
- response SHA-256: `B26024B9B01829931338E76CFB202EA730B7BEF6595D5C4C18142B3E08C5CF77`; and
- no terminal filing decision and no status-poll obligation.

The retained request proves that the rejected shape used `Class=AA`, `Qualifier=request`, `Function=submit`, `GatewayTest=1`, a `FormSubmission` body, `FormIdentifier=AA` and `Category=ACCOUNTS`. The general TIS 5.3 explicitly lists `AA` as an acceptable document-submission Class, but that statement conflicts with more specific and more recent Companies House evidence. The live schema-status accounts wrapper example uses `Class=Accounts` and `FormIdentifier=Accounts`; a 2025 accounts test exchange using those values passed routing and reached presenter authentication; and on 30 March 2026 a Companies House XML Gateway forum representative explicitly instructed an accounts filer to use `Accounts` for the Class. The subsequent request passed routing and reached package-reference validation.

The 501 is therefore best explained by a stale or misleading general-TIS route value rather than by the new presenter's enablement. The filing contract and deterministic simulator have been corrected to `Class=Accounts` and `FormIdentifier=Accounts`. The endpoint, `GatewayTest=1`, `Function=submit`, `Method=clear`, lowercase MD5 presenter material and four-digit package-reference shape remain consistent with current published examples. Presenter enablement and the synthetic company authentication are still unproved downstream inputs: they can only be evaluated after a separately reviewed request passes the corrected route. Do not send the earlier support email or automatically retry `S00001`.

Routing reconciliation sources: [live schema status and examples](https://xmlgw.companieshouse.gov.uk/SchemaStatus), [July 2025 accounts test exchange](https://xmlforum.companieshouse.gov.uk/t/assistance-with-ixbrl-micro-accounts-formsubmission-formidentifier-accounts-via-xml-gateway/1761) and [March 2026 Companies House Class correction](https://xmlforum.companieshouse.gov.uk/t/govtalk-form-submission-to-ammend-accounts-new-accounts/1924).

Protected exact request/response evidence is retained under `.local/companies-house/test-account/evidence/20261006T161029Z-S00001`. Treat both the submission number and transaction ID as consumed; any authorised follow-up must use the next unique/incremental identities and must not be an automatic retry.

The next no-send review candidate is `S00002` with transaction `2026100600000002`. The rebuilt Development harness generated it through `official-test-preflight` with both protected inputs available, database preparation complete, credentialed-envelope materialisation complete and `ExternalSendEnabled=false`. Safe review digests are:

- preview SHA-256: `D8788554439F0317CC214CD8DDC4AC782B2CAF05B8F570421170E7E9CFDBE7CC`;
- accounts SHA-256: `1F321A10E343D9467D2100827D2E33D9D6B4E482C0B7268DF704243DEDF1B08C`; and
- corrected wire SHA-256: `8EE86CF7D12DB6162C19661BEF44457ADBEC8E6A2153830C1648F84FD8BAC44C`.

The no-send outcome retains `CH-EXTERNAL-SEND-DISABLED` and `CH-DEVELOPER-TEST-EVIDENCE-PENDING`. A fresh one-shot exchange still requires explicit human authorisation; preflight is not that authorisation.

### Second controlled official-test exchange — 6 October 2026

Human review explicitly authorised the reviewed `S00002` / `2026100600000002` request with wire SHA-256 `8EE86CF7D12DB6162C19661BEF44457ADBEC8E6A2153830C1648F84FD8BAC44C`. The one-shot Development process sent it exactly once to the pinned official test endpoint and was stopped immediately after the response. No retry or status poll occurred.

The corrected `Accounts` / `Accounts` route passed gateway target validation. Companies House then returned synchronous GovTalk error `9999` from embedded iXBRL schema validation: `ix:resources` appeared where XHTML flow content or `ix:header` was expected. This is authoritative evidence that the request reached accounts-document validation; it is not a presenter-authentication, company-authentication, filing-acceptance or terminal-status result. Response SHA-256 is `73A270E7C8E6D74B43060085BB5850FD44B8345C90886D44715CDC2186762234`.

The local cause was narrow and deterministic: `IxbrlDocumentBuilder` placed `ix:resources` directly inside an XHTML `div`. The first correction introduced the required `ix:header`; the next controlled exchange then exposed the distinct schema-reference placement defect recorded below. These tests and exchanges establish progressively corrected document structure only; they do not predict Companies House acceptance.

Protected request/response evidence is retained under `.local/companies-house/test-account/evidence/20261006T163803Z-S00002`. Treat both second-exchange identifiers as consumed. Any further official exchange requires a fresh unique submission/transaction identity, a newly materialised digest and separate human review and authorisation. No third exchange is authorised by this correction.

### Third controlled official-test exchange — 6 October 2026

The user subsequently authorised another corrected attempt. A fresh preflight prepared `S00003` / `2026100600000003`, accounts SHA-256 `C60E1379F12011F6A3CA647F0AAA090756EFD15FE9688C19A6673FEEFFA5627E` and wire SHA-256 `52BDE62A80792428F887C5486AD4A5EC8B6B39CCFAA3093D8AE2E0645C9491A8`. The one-shot Development process sent that exact request once and was stopped immediately after the response. No retry or status poll occurred.

Companies House again progressed into embedded iXBRL validation and returned synchronous error `9999`: `link:schemaRef` was found in `ix:resources`, whose allowed content is relationships, role/arcrole references, contexts and units. This is not an authentication verdict or filing acceptance. Response SHA-256 is `81CC7B552A420BE05568A5AD3A0E50E1AA6DA3BE9F4FE87474D82899989028C1`.

The gateway result agrees with the normative [Inline XBRL 1.1 specification](https://www.xbrl.org/Specification/inlineXBRL-part1/REC-2013-11-18%2Berrata-2026-07-14/inlineXBRL-part1-REC-2013-11-18%2Bcorrected-errata-2026-07-14.html): `ix:header` content is ordered as optional `ix:hidden`, zero or more `ix:references`, then optional `ix:resources`; `ix:references` contains `link:schemaRef`/`link:linkbaseRef`, while `ix:resources` contains contexts, units and relationship resources. `IxbrlDocumentBuilder` now emits a hidden XHTML `div` containing `ix:header`, then `ix:references/link:schemaRef`, followed by `ix:resources` containing contexts and units. Regression assertions fix the hierarchy, order and exclusive schema-reference placement.

Protected exact evidence is retained under `.local/companies-house/test-account/evidence/20261006T164903Z-S00003`. Treat both third-exchange identifiers as consumed. The correction is locally verified only; a fourth exchange requires a new preflight, digest review and explicit human authorisation.

### Fourth controlled official-test exchange — 6 October 2026

Human review authorised the no-send `S00004` / `2026100600000004` preflight with accounts SHA-256 `C5D3B496DEC63DA09FC7E93B33FDE9AF4491DD46158FBE9141EFD761F7008837` and wire SHA-256 `1A4A6F4204F4AF585A26AE3977508F908C172C3762B57E55750AE78801ACA69A`. The one-shot process sent that exact digest once and was stopped immediately afterwards. No retry or poll occurred.

Companies House accepted the corrected `ix:references` / `ix:resources` hierarchy and returned seven deeper validation diagnostics. The primary error was that the `http://xbrl.frc.org.uk/FRS-102/2026-01-01/FRS-102-2026-01-01.xsd` schema reference could not be obtained. Five errors also rejected `xml:lang` on FRC `fixedItemType` facts. The remaining `AccrualsDeferredIncome` content error is treated as potentially consequent on the unavailable taxonomy and has not been changed speculatively. Response SHA-256 is `E81DE7FDD1486794B4A94F7D0C3F0FAD03CEB0D4CE4BE8B44463FAF06D251D0F`. This remains schema evidence, not an authentication or filing decision.

The official [FRC 2026 taxonomy publication](https://www.frc.org.uk/library/standards-codes-policy/accounting-and-reporting/frc-taxonomies/current-uk-and-irish-digital-reporting-taxonomies/2026-uk-and-irish-digital-reporting-taxonomies/) identifies suite v1.0.0. A temporary copy of that official ZIP was downloaded only to the git-ignored `.local` inspection area (SHA-256 `AE80AE12D9D747AC531150B0051BCD67E7C9ACF44313DA19105B4CC013566462`); no official taxonomy asset was committed. Inspection and direct HTTP checks establish that the published entry point is retrievable at the equivalent `https://` URI, while the old `http://` URI redirects. The builder now references HTTPS directly. The official type schema also proves that the five affected concepts use a zero-length `fixedItemType`, so the model now represents these as explicit empty fixed values without `xml:lang`.

Protected request/response evidence is retained under `.local/companies-house/test-account/evidence/20261006T170032Z-S00004`. Treat both fourth-exchange identifiers as consumed. A fifth exchange requires a fresh preflight, digest review and explicit human authorisation.

### Fifth controlled official-test exchange — 6 October 2026

Human review authorised `S00005` / `2026100600000005` with wire SHA-256 `2A100BE0B236045DBD037A2E2172AE319A14F71E102D30F7F1921E6F9B382DC6`. It was sent exactly once and the one-shot process was stopped. Companies House returned only one remaining synchronous validation diagnostic: `core:AccrualsDeferredIncome` was not a declared XBRL item. Response SHA-256 is `0C6C1538A8DDB0568AADA400EFC9CB6A0CCC44E3E3FFFCD06685F8399DC57304`. The absence of the earlier errors confirms that the HTTPS taxonomy entry point, header hierarchy and fixed-fact corrections passed this validation stage.

The official FRC 2026 core schema declares the monetary instant item `core:AccruedLiabilitiesDeferredIncome`, and the FRS-102 presentation hierarchy labels it “Accrued liabilities and deferred income.” The internal semantic/source field remains accruals and deferred income; only its erroneous authority QName has been corrected. A regression assertion pins the declared 2026 local name.

Protected exact evidence is retained under `.local/companies-house/test-account/evidence/20261006T170746Z-S00005`. Treat both fifth-exchange identifiers as consumed. A sixth exchange requires a new preflight, digest review and explicit human authorisation.

### Sixth controlled official-test exchange — 6 October 2026

Human review authorised `S00006` / `2026100600000006` with wire SHA-256 `79507C168A97DF73AA404D403B275F05B6467C816D091D5A5991EAC80880BE5E`. It was sent exactly once and the one-shot process was stopped. Companies House returned synchronous validation error `9999` at the first alphabetical report fact, `core:CapitalAndReserves`. Response SHA-256 is `9CB4C74EF7DD4E98EB5B480494D12B0D7F9AF221DCACB65D494B2C7999A669D7`.

The diagnostic's generic statement that an `xbrli:item`, tuple, context, unit or footnote was expected initially resembled a document-ordering error. Inspection of the official FRC 2026 v1.0.0 schemas established the more precise cause: `CapitalAndReserves` is not declared in the 2026 core schema, so it cannot participate in the `xbrli:item` substitution group. The published replacement is `core:Equity`, whose verbose label is “Equity / share capital and reserves.” A complete audit of the narrow 35-concept catalog found seven further obsolete or incorrectly namespaced mappings before another send was attempted:

- company name, Companies House number and principal activity belong to the published business namespace;
- tax on profit maps to `core:TaxTaxCreditOnProfitOrLossOnOrdinaryActivities`;
- period profit or loss maps to `core:ProfitLoss`;
- creditors within and after one year map to `core:CurrentLiabilities` and `core:Non-currentLiabilities`; and
- capital and reserves maps to `core:Equity`.

All 35 configured concepts now exist in their declared official 2026 schema and are non-abstract. Regression assertions pin the corrected namespaces and local names. This is authoritative published-schema validation plus official gateway evidence; it remains short of filing acceptance.

Protected exact evidence is retained under `.local/companies-house/test-account/evidence/20261006T171430Z-S00006`. Treat both sixth-exchange identifiers as consumed. Do not retry or poll `S00006`.

The next no-send review candidate is `S00007` with transaction `2026100600000007`. The rebuilt send-disabled Development harness prepared the real database package and materialised the protected wire envelope. `ExternalSendEnabled=false`; safe review digests are:

- preview SHA-256: `35EEDB6FDED47A87F6ED975A383C86C043F05990423B52EA1A2100BA36590904`;
- accounts SHA-256: `943DBE381FB91576A98CE3474B7342EA4AC684C95711EEFE4724F863A91DD988`; and
- wire SHA-256: `2A5B19BAE388481DAEFC9E84B557AA346DF936553E6FF32572354A7172F62C92`.

The no-send outcome retains `CH-EXTERNAL-SEND-DISABLED` and `CH-DEVELOPER-TEST-EVIDENCE-PENDING`. No seventh exchange is authorised by this preflight; it requires separate human review and explicit send approval.

### S00007 presentation review correction — 6 October 2026

Human review of the prepared payload identified two boundary defects before any seventh exchange. The authority-neutral statutory source, balance-sheet aggregate, reconciliation and FRC facts completed under Objective 3 were already present and remain unchanged. However, the generic iXBRL renderer displayed an unlabeled list of tagged values rather than the human-readable statutory balance sheet required by the approved contract design. The Companies House WebHarness request also inherited the Corporation Tax-only `taxOnProfit` review fields from the general accounts diagnostic contract.

The Companies House filleted micro-entity slice now renders a recognisable balance sheet, comparative columns, statutory statements, notes and approval from the same inline fact objects submitted to the authority; every projected fact is rendered exactly once. The Companies House-specific request no longer exposes Corporation Tax review fields and rejects them as unknown. The general accounts/Corporation Tax request remains separate and unchanged. The Companies House diagnostic is explicitly limited to the currently reviewed `filleted` profile rather than pretending that its zero-tax internal bridge supports a reviewed full-accounts filing.

The three `S00007` digests above describe the superseded no-send candidate and must not be authorised or sent. A new send-disabled preflight and digest review are required before any seventh exchange. This correction is locally validated implementation evidence only; no external request was made and Companies House developer-test acceptance remains pending.

### Seventh controlled official-test exchange — S00008, 6 October 2026

Human authorisation was given to submit the corrected human-readable accounts document using `S00008` / `2026100600000008`. An initial Swagger invocation accidentally selected the sole-trader STD sandbox and failed locally with HTTP 422 before materialisation or transmission; it did not consume the identifiers. The request was corrected to the company STD sandbox and sent exactly once. Request SHA-256 is `A16F2D6310E1EE8C83130B7DA835A2FC855ADEC4CF2CB7F12B832482F26DE0B0`; response SHA-256 is `98669F556B01295198C9D318C2DBF143E7F671D28E91E43C85EE43A3EA494510`.

Companies House returned two synchronous XHTML validation errors: the accepted content model does not include the HTML5 `main` element, and its XHTML `style` element requires `type="text/css"`. The presentation wrapper now uses an XHTML `div` and the stylesheet declares its type. Regression assertions pin both compatibility rules. This exchange establishes only the gateway's XHTML schema requirements; it is not authentication, filing acceptance or a terminal decision. Protected evidence is retained under `.local/companies-house/test-account/evidence/20261006T174649Z-S00008`. Treat both identifiers as consumed and do not retry or poll them.

### Eighth controlled official-test exchange — S00009, 6 October 2026

Human review authorised the fixed document as `S00009` / `2026100600000009`. It was sent exactly once with request SHA-256 `0C26E4602656B15D9C44C9BD959AB3241576F02FD84512CBA43EAC7BC8B6FF6E`. The preceding `main` and untyped-`style` diagnostics were absent. Companies House returned one further XHTML content-model error for the HTML5 `section` element; response SHA-256 is `D0BF6F56726840B0E4018C76CF8C093BD492AEEDD72A256A24CADF700903251A`.

All presentation `section` wrappers now use XHTML-compatible `div` elements, and the regression rule excludes both unsupported structural elements. This remains progressive gateway schema evidence, not authentication, filing acceptance or a terminal decision. Protected evidence is retained under `.local/companies-house/test-account/evidence/20261006T175114Z-S00009`. Treat both identifiers as consumed and do not retry or poll them.

### Ninth and tenth controlled official-test exchanges — S00010 and S00011, 6 October 2026

`S00010` / `2026100600000010` was sent exactly once after the XHTML compatibility correction. Companies House returned a well-formed GovTalk message, but the transport classifier rejected its operation shape before retaining the raw response. The one-shot process remained fail-closed and prohibited automatic retry. Protected pre-send and ambiguity evidence is retained under `.local/companies-house/test-account/evidence/20261006T180222Z-S00010`; request SHA-256 is `E208FAA40C21BB0B7BC10863CC399D90B73F4F808BDE89557644745F3DD83086`. Both identifiers are consumed.

The adapter was then corrected to retain an unclassified authority response exactly before surfacing the fail-closed error. `S00011` / `2026100600000011` was sent once to obtain the missing evidence. Its request SHA-256 is `F88CE0341DB5F8EF8EA03C9A81B5AD1E30C57D777322BEAF684C2E1841C5AD58`; the 1,085-byte response SHA-256 is `A79DE2915C51E358F6757CA971335B0131B65D43B6D1EDF766F790E35DC63689`.

The exact response is an empty-body GovTalk receipt with `Class=Accounts`, `Qualifier=response`, matching transaction ID and gateway timestamp `2026-10-06T19:09:58Z`. It contains no `GovTalkErrors`. This is authoritative test-gateway evidence that a successful Accounts submission receipt uses `Qualifier=response`, despite the earlier simulator/offline assumption that the literal would be `acknowledgement`. The narrow classifier correction accepts both the published acknowledgement form and this observed Accounts/response form only for `FormSubmission`; other operations remain unchanged and fail closed. The receipt is non-terminal: it proves successful synchronous gateway receipt and creates a specific-submission polling obligation, not Companies House filing acceptance.

The S00010/S00011 payload reused the earlier development sandbox's 31 March 2027 reporting date. That date was not required by Companies House and is inappropriate for the controlled scenario. The company STD database exposes a ready `UK-CO-ACCTS-2026` balance sheet at 31 March 2026. The WebHarness also incorrectly passed the accounts year-end into contract-version selection, which conflated the reporting period with the date on which the filing contract is used. Contract selection now uses the actual preparation/submission date, while subsequent accounts preparation uses the period 1 April 2025 to 31 March 2026. This correction is locally verified; it did not cause another external submission.

Protected exact evidence is retained under `.local/companies-house/test-account/evidence/20261006T180955Z-S00011`. Treat `S00011` and its transaction ID as consumed. The smallest next external step is a specific `GetSubmissionStatus` poll for `S00011`, which does not require `StatusAck`; do not send a duplicate accounts submission merely to exercise the corrected reporting date.

### Eleventh and twelfth controlled official-test exchanges — S00012 and S00013, 6 October 2026

The MIN and STD synthetic company instances were regenerated for a completed accounting period ending 30 September 2026, with a comparative period ending 30 September 2025. Both passed the no-send database-to-envelope preflight. Human review then authorised `S00012` / `2026100600000012`; it was sent exactly once with request SHA-256 `F2C08868401A710C2E726F81E110C5BB115331EBA2AF56E3514C93353DAFFA37`. Companies House returned synchronous validation error `9999`: `DateAuthorisationFinancialStatementsForIssue must be provided for the current accounting period.` Response SHA-256 is `480812C2D82E059FB02D7820F2386B614802334C1891B483836ED7DA5A6FC835`.

The fact and its value were present, but its XBRL context instant incorrectly used the 6 October approval date. Date-valued facts describing the current accounting period must carry the current period-end context while retaining their actual date as the fact value. The projection now gives both `DateAuthorisationFinancialStatementsForIssue` and `StartDateForPeriodCoveredByReport` the 30 September 2026 current-period context. Regression assertions pin that distinction.

After the full solution built without warnings or errors and the company contract suite passed 87 assertions, the corrected MIN accounts were prepared as `S00013` / `2026100600000013`. Its complete credentialed envelope passed the no-send preflight with wire SHA-256 `CF5CCF3284742868D858130E1E0A3BAECE2254C0EA1C6A38903B0BD2A969F0FA`. Human review sent it once. Companies House returned an error-free synchronous `Acknowledgement` at `2026-10-06T19:55:08Z`; response SHA-256 is `7EE6CD0A7CDF03495D65BB1D1714888D4FDCD5B13D78BFF81ED7A89B5DE65C5D`.

This is authoritative developer-test evidence that the corrected envelope and embedded accounts crossed synchronous gateway validation. It is non-terminal: `terminalFilingDecisionReceived=false`, so it is not yet an `ACCEPT`, developer-test approval or production-readiness evidence. Protected exact evidence is retained under `.local/companies-house/test-account/evidence/20261006T185505Z-S00013`. Treat `S00012`, `S00013` and both transaction IDs as consumed. The smallest next external step is a specific `GetSubmissionStatus` poll for `S00013`; it does not require `StatusAck`.

## Internal implementation verification

- The generated credential-free `FormSubmission` body validates locally against the downloaded live `FormSubmission-v2-11.xsd`; this checks the wrapper structure only, not the embedded iXBRL, authentication, gateway business rules or acceptance. The second official exchange exposed and drove correction of an embedded iXBRL `ix:header` defect that wrapper-only validation did not detect.
- Contract tests prove exact iXBRL byte preservation, TIS 6.0 mandatory profile/statutory facts, visible statutory balance-sheet presentation tied to every projected inline fact, all published status codes, rejection detail parsing and the empty conditional `StatusAck` body.
- Application, offline corporate handoff and architecture suites pass; the Companies House family gate records zero outbound sends while registry readiness is false or blocking findings remain.
- The corrected solution build and relevant offline suites complete with no warnings or errors. Those verification runs made no external request; every external request is an explicitly authorised, one-shot exchange recorded above.
