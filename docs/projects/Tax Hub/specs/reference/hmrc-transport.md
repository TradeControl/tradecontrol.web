# HMRC Transport Platform Reconnaissance

**Research date:** 22 September 2026  
**Programme objective:** Objective 4 — HMRC Transport Platform  
**Status:** Reference and implementation-design evidence only; no transport implementation is authorised by this document

## 1. Executive summary

Objective 4 should be implemented as two protocol-specific adapters behind the existing Application gateway boundary, supported by a small set of genuinely shared operational services:

1. an HMRC API Platform adapter for VAT and MTD Income Tax REST/JSON requests; and
2. an HMRC Transaction Engine adapter for Corporation Tax GovTalk/XML submission, IRmark generation, acknowledgement polling, terminal response handling and receipt retention.

The two families share environment selection, secret acquisition, tenant/subject scoping, durable attempt state, redacted diagnostics, clocks, correlation and failure classification. They do **not** share authentication, wire envelope construction, retry rules or acknowledgement state machines. An artificially uniform `HttpClient.Send` abstraction would conceal material differences.

The existing repository already establishes most of the semantic boundary:

- `PreparedApiRequest` contains a resolved relative path, ordered query, exact canonical JSON body bytes where applicable, contract media-type headers, digest and source evidence.
- `PreparedSubmissionPackage` contains a service code, a transmission artifact, named document artifacts and polling metadata.
- `IPreparedApiRequestGateway` and `IPreparedSubmissionPackageGateway` prove unchanged handoff in tests.
- VAT and MTD Income Tax descriptors already record OAuth scopes, media versions and, for MTD Income Tax, success status codes.
- response DTOs exist for VAT and MTD Income Tax, and acknowledgement types exist for Corporation Tax.

Nothing in `TradeControl.Tax.UK.Adapters.Submission` performs transport today. `HmrcSettings`, `EnvironmentSelector` and `SubmissionLogger` are placeholders. The old WebHarness `HmrcSubmissionRunner` simulates success and generates a local reference; it is not an authority client.

Four boundary conflicts must be resolved before production transport can be enabled:

1. **REST dispatch metadata is incomplete.** `PreparedApiRequest` does not carry the descriptor's OAuth scope, expected success status or response contract. Objective 4 cannot reliably authorise or interpret a request without duplicating Objective 3's operation catalogue.
2. **The Corporation Tax bytes are explicitly diagnostic.** `CorporationTaxPackageSerializer` says its output is “not a gateway payload,” but `CorporationTaxPreparer` places those bytes in `PreparedSubmissionPackage.Transmission`.
3. **Corporation Tax polling metadata conflicts with HMRC's protocol.** The repository declares `SubmissionPollingMode.None` and `RequiresStatusPolling = false`. Transaction Engine Document Submission Protocol v2.0 requires acknowledgement, correlation-ID polling until a terminal response, and deletion of the stored response.
4. **The company package remains preview-only.** The computation taxonomy is not submission-ready because official validation assets are absent. Every prepared company artifact is marked `Preview`. Objective 4 must reject it rather than transmit it.

These are not accounting or tax-meaning defects. They are contract/transport-boundary readiness defects. They should be corrected through a narrow metadata and packaging clarification, preserving all Objective 3 semantic values and already-prepared JSON/document bytes.

The smallest safe delivery order is:

1. harden the gateway contracts and introduce durable submission state;
2. implement OAuth/token storage and fraud-prevention-header capture;
3. implement read-only REST enquiries, then VAT and MTD Income Tax writes using exact prepared bytes;
4. obtain and pin the missing official Corporation Tax validation assets and define the exact service-body/transport-envelope boundary;
5. implement and recognise the Transaction Engine/IRmark path; and
6. integrate the resulting application outcomes into Objective 5 later.

No live request should be possible when an artifact has errors, is a preview, has an unrecognised operation/version, lacks required authorisation metadata, or lacks a durable attempt record.

## 2. Evidence and sources examined

### 2.1 Governing and completed-project evidence

The following repository documents were treated as the current project record:

- `docs/projects/Tax Hub/specs/tax-hub-spec-programme.md` — governing programme specification;
- `docs/projects/Tax Hub/implementation/tax-hub-implementation-3.md` — Objective 3 design and boundary;
- `docs/projects/Tax Hub/implementation/tax-hub-workplan-4.md` — completed Objective 3 work record despite its filename;
- `docs/projects/Tax Hub/implementation/tax-hub-repo-structure.md` — target dependency and assembly ownership;
- `docs/projects/Tax Hub/findings.md` and `change-log.md` — reviewed decisions and implementation history; and
- the current session brief and its recorded 22 September 2026 sandbox verification.

Older findings were used only where they were not superseded by a later reviewed decision or the current code.

### 2.2 Current code examined

The code inspection covered all projects beneath `src/tax-hub/src`, with particular attention to:

- `TradeControl.Tax.UK.Application/Preparation`;
- `TradeControl.Tax.UK.Adapters.Submission`;
- the VAT and MTD Income Tax contract catalogues and DTOs;
- the Corporation Tax and Companies House package contracts and serializers;
- WebHarness composition, controllers, preparation stores and legacy diagnostic runner; and
- Application, data-provision, contract and architecture tests beneath `src/tax-hub/tests`.

The historical `.local/vat_mtd_client_test-master` application was read as evidence only. Configuration files containing possible credentials were not reproduced or quoted.

### 2.3 Authoritative external evidence

The external research used HMRC Developer Hub and GOV.UK sources current on the research date. Important source dates are recorded because the REST APIs and MTD Income Tax roadmap are actively changing.

Key sources included:

- HMRC REST [reference guide](https://developer.service.hmrc.gov.uk/api-documentation/docs/reference-guide), including TLS, versioning, standard errors and the normal three-requests-per-second application limit;
- [user-restricted endpoint authorisation](https://developer.service.hmrc.gov.uk/api-documentation/docs/authorisation/user-restricted-endpoints), including Authorization Code Grant, optional PKCE, four-hour access tokens, single-use refresh tokens and the 18-month re-authorisation boundary;
- [VAT (MTD) API v1.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0), last updated 5 August 2026;
- [VAT MTD end-to-end guide](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/), updated 23 June 2026;
- [Self Employment Business (MTD) API v5.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-employment-business-api/5.0), last updated 18 September 2026;
- [MTD Income Tax end-to-end guide](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/), updated 15 September 2026;
- fraud-prevention [connection-method selector](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/connection-method/), [web application via server requirements](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/connection-method/web-app-via-server/), [format/missing-data guidance](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/getting-it-right/), [v3.3 change log](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/change-log/) and [Test Fraud Prevention Headers API](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/txm-fph-validator-api/1.0);
- [Corporation Tax online developer collection](https://www.gov.uk/government/collections/corporation-tax-online-support-for-software-developers), including the 7 April 2026 RIM artifacts;
- [Corporation Tax generic technical specifications](https://www.gov.uk/government/publications/corporation-tax-generic-technical-specifications), updated 13 June 2025;
- [Transaction Engine Document Submission Protocol v2.0](https://www.gov.uk/government/publications/transaction-engine-document-submission-protocol);
- [basic guide for XML software developers](https://www.gov.uk/guidance/basic-guide-for-xml-software-developers), including current test/live Transaction Engine endpoints;
- [IRmark Gateway Protocol guide and examples](https://www.gov.uk/government/publications/hmrc-irmark-for-gateway-protocol-services); and
- [generic IRmark specification](https://www.gov.uk/government/publications/hmrc-irmark-generic-irmark-specification).

HMRC publishes old-looking dates for some still-current XML protocol documents. Their continued inclusion in the current Corporation Tax developer collection is evidence that the protocol remains applicable, but the exact Corporation Tax class, schema, RIM release, validation bundle and recognition scenarios must be pinned again at implementation start.

## 3. Current Objective 3 to Objective 4 boundary

### 3.1 Projects and dependency direction

The implemented dependency direction is appropriate:

```text
VAT / MTD Income Tax / Company contract assemblies
                         ↓
           TradeControl.Tax.UK.Application
                         ↑
 TradeControl adapter     Submission adapter
                         ↑
                    WebHarness
```

`TradeControl.Tax.UK.Adapters.Submission` references Application and is the intended home of external transport. Contract assemblies do not reference it. Objective 4 should preserve that direction.

### 3.2 REST handoff

`PreparedApiRequest` in `TradeControl.Tax.UK.Application.Preparation` is the concrete REST handoff. It provides:

- `OperationId`, `ContractFamily` and `ContractVersion`;
- `IsPreview`;
- upper-case HTTP `Method`;
- an already-resolved relative path;
- ordered query name/value pairs;
- contract headers (`Accept` and, for body operations, `Content-Type`);
- optional immutable body bytes and their SHA-256 digest;
- source evidence and validation findings; and
- an invariant that credential headers cannot be present.

`PreparedApiRequestPipeline` resolves and escapes path parameters, orders query parameters by the descriptor contract, invokes the canonical serializer once only when validation has no errors, and rejects bodies for bodyless requests. `VatReturnPreparer` and `CumulativePeriodSummaryPreparer` are the implemented body-bearing producers. `BodylessRequestDescriber` prepares approved VAT and MTD Income Tax enquiry descriptions without inventing bodies.

The exact JSON body is therefore immutable transport input. Objective 4 may add the absolute authority base address, bearer token, fraud-prevention headers and transport-only headers. It must not deserialize, rename properties, re-round, pretty-print, reserialize or otherwise replace `BodyBytes`.

Current REST call paths are diagnostic rather than live:

```text
WebHarness controller
  -> Trade Control readers/statutory context
  -> VatReturnPreparer or CumulativePeriodSummaryPreparer
  -> PreparedApiRequestPipeline
  -> PreparedApiRequestStore
  -> inspection/raw-body preview
```

`IPreparedApiRequestGateway.SendAsync(PreparedApiRequest, ...)` is exercised only by capturing fakes in tests. No production implementation or application orchestration invokes it.

### 3.3 Package handoff

`PreparedSubmissionPackage` is the company/package handoff. It contains:

- `ServiceCode`;
- a `PreparedStatutoryArtifact` named `Transmission`;
- named constituent `PreparedPackageDocument` artifacts; and
- `PreparedPollingSemantics`.

Each artifact carries jurisdiction, authority, operation, contract version, preview/submission-ready status, media type, immutable bytes, SHA-256, source versions and findings.

For Corporation Tax, the current path is:

```text
CorporationTaxRunner
  -> Trade Control company and corporation-tax readers
  -> CompanyAccountsPopulator / CorporationTaxPopulator
  -> CompanyAccountsPreparer / CorporationTaxPreparer
  -> accounts iXBRL + computation iXBRL + diagnostic CT XML
  -> PreparedSubmissionPackage("HMRC-CORPORATION-TAX")
```

The current Corporation Tax package carries accounts and computation documents separately and puts the diagnostic CT XML in `Transmission`. `CorporateHandoffTests` prove that a fake `IPreparedSubmissionPackageGateway` receives the same object and unchanged bytes.

For Companies House, the equivalent package has service code `COMPANIES-HOUSE-ACCOUNTS` and declares terminal polling. It is a separate authority transport and not an HMRC path. The current serializer explicitly says its logical envelope is not submission-ready because the separate Filing TIS schemas are absent. Objective 4 HMRC work should not accidentally enable it. A later Companies House transport phase may reuse shared durable-attempt concepts, but requires its own protocol evidence and approval.

### 3.4 Metadata not currently crossing the boundary

The REST descriptors know more than `PreparedApiRequest` carries:

- VAT and MTD Income Tax descriptors have `OAuthScope`;
- MTD Income Tax descriptors have `SuccessStatusCode` and request/response types;
- VAT descriptors do not record a success status at all; and
- none of these response expectations reaches the gateway.

The gateway method also receives no tenant, HMRC authorisation principal, initiating-user/fraud context or approval/audit context. Those values must not be inserted into the immutable tax body, but Objective 4 needs a separate typed `AuthorityDispatchContext` (or equivalent application command) to select the correct credentials and build truthful per-request headers. Inferring them from a VRN, NINO, source key or relative URL would be unsafe.

There is one smaller current-contract conflict: `BodylessRequestDescriber` accepts only four alphanumeric characters for a VAT return `periodKey`, while HMRC's current VAT guide documents special keys such as `#001`, to be percent-encoded as `%23001`. `PreparedApiRequestPipeline` can perform the path escaping, but the earlier validation rejects the value. The descriptor/preparation validation must be aligned with the official endpoint before that enquiry is enabled; Objective 4 must not bypass the validator or rewrite the path.

The package boundary likewise lacks a trustworthy declaration of whether `Transmission` is a final wire document or an immutable service payload that Objective 4 must place inside a transport envelope.

This is a real incompatibility. Building an Objective 4 lookup table keyed only by string operation IDs would duplicate versioned Objective 3 facts in the transport assembly and create silent drift. Section 9 proposes the narrow correction.

## 4. Required transport families

| Objective 3 output | Protocol | Authentication | Body ownership | Response model |
|---|---|---|---|---|
| VAT API requests | HTTPS REST/JSON | OAuth 2.0 user-restricted bearer token; `read:vat` or `write:vat` | Entire optional JSON body is immutable Objective 3 output | synchronous HTTP status plus JSON success/error body |
| MTD Income Tax requests | HTTPS REST/JSON | OAuth 2.0 user-restricted bearer token; `read:self-assessment` or `write:self-assessment` | Entire optional JSON body is immutable Objective 3 output | synchronous HTTP status; some writes return no body, enquiries return JSON |
| Corporation Tax return | Transaction Engine GovTalk XML over HTTP POST | Government Gateway sender/presenter credentials inside protocol envelope; not API Platform OAuth | Exact CT600 and iXBRL semantic artifacts come from Objective 3; Objective 4 owns the required GovTalk envelope, permitted IRmark insertion and protocol conversation | acknowledgement with correlation ID, repeated polls, terminal business response/error, digital receipt, then delete |
| Companies House accounts | Companies House XML gateway | presenter/company authentication | separate authority package | acknowledgement and status polling; outside this HMRC implementation until separately authorised |

Common infrastructure should stop at configuration, secret handles, durable attempts, clocks, redaction, metrics, correlation and outcome classification. REST OAuth/fraud headers and Transaction Engine GovTalk/IRmark are protocol-specific.

## 5. Current HMRC transport requirements

### 5.1 Environments and endpoints

For HMRC REST APIs, the current documented base addresses are:

- sandbox API and token endpoint base: `https://test-api.service.hmrc.gov.uk`;
- production API and token endpoint base: `https://api.service.hmrc.gov.uk`;
- sandbox user-authorisation journey: HMRC's test web authorisation host; and
- production user-authorisation journey: `https://www.tax.service.gov.uk`.

Authorisation, token exchange and API resource bases must be separately configured. The historical application's single `Uri` concatenation is not a safe target design.

For Transaction Engine, current HMRC guidance lists:

- test submission: `https://test-transaction-engine.tax.service.gov.uk/submission`;
- test poll: `https://test-transaction-engine.tax.service.gov.uk/poll`; and
- live submission: `https://transaction-engine.tax.service.gov.uk/submission`.

The acknowledgement supplies the response endpoint and poll interval. The client must validate the supplied endpoint against an allow-listed HMRC host before following it; it must not treat an arbitrary response URL as trusted configuration.

Environment selection must be a closed enum/profile chosen by deployment configuration. A prepared request must never supply a base address, and production must not be selectable from user input.

### 5.2 OAuth, scopes and token lifecycle

VAT and MTD Income Tax endpoints are user-restricted. Current HMRC guidance requires OAuth 2.0 Authorization Code Grant and supports PKCE. Access tokens last four hours. Refresh tokens are single-use: a successful refresh immediately rotates the refresh token and invalidates the original access token if it was still live. After 18 months, refresh stops working and the user must repeat authorisation.

The implementation consequently needs:

- a tenant- and HMRC-principal-scoped authorisation record;
- encrypted access and refresh token storage outside ordinary application tables/logs;
- exact granted-scope storage;
- atomic refresh-token rotation;
- a per-authorisation refresh lock to prevent concurrent calls spending the same single-use token;
- expiry skew so calls do not start with a nearly expired token;
- explicit revoked/reauthorisation-required states; and
- no access token, refresh token, client secret or authorisation code in a prepared artifact, URL, audit text or diagnostic log.

Required scopes already appear in the repository descriptors: `read:vat`, `write:vat`, `read:self-assessment` and `write:self-assessment`. Objective 4 should request the smallest set required by the approved product workflow and reject dispatch if the stored grant lacks the prepared operation's declared scope.

### 5.3 Fraud-prevention headers

HMRC currently requires fraud-prevention header data for all VAT and MTD Income Tax API endpoints. The exact header set depends on the connection method. Tax Hub appears to be a web application via server, but that must be confirmed against the final deployment topology; reverse proxies, WAFs and other vendor-controlled hops affect the required values.

For a web application via server, the current v3.3 material includes client connection/device/browser, public IP/port/timestamp, screens/window/timezone/user identifiers and vendor forwarding/product/public-IP/version/license information. Data must describe the originating user device and every vendor-controlled intermediary hop. US-ASCII and percent-encoding rules are significant. Placeholder values such as `null` or `undefined` are prohibited; exceptional missing data follows HMRC's documented process.

This data cannot be reconstructed reliably in a background submission adapter. Objective 5's initiating web request must capture validated client facts and pass a sealed, short-lived transport context to Objective 4. Trusted proxy configuration must determine the real public client and vendor hop chain; raw inbound forwarding headers must never be trusted without that boundary.

The sandbox Test Fraud Prevention Headers API validates current format and gives errors/warnings. Passing it is necessary evidence, not a guarantee of production compliance. Acceptance should require zero errors and reviewed advisories for every supported deployment topology.

### 5.4 REST request construction and versioning

HMRC requires TLS 1.2 or higher. The contract version is selected through `Accept: application/vnd.hmrc.<version>+json`; incompatible versions use a new media version. Body operations use `application/json`. These values are already prepared by Objective 3 and should be copied without interpretation.

The REST adapter must:

- combine an allow-listed environment base URI with the prepared relative path;
- append the prepared ordered query with RFC-compliant escaping;
- copy prepared `Accept` and `Content-Type` values exactly;
- attach a bearer token with the required grant/scope;
- attach freshly collected fraud-prevention headers;
- send the exact `BodyBytes` as byte content, without a JSON serializer; and
- enforce bounded connection and overall request timeouts through managed `HttpClient` instances.

`Gov-Test-Scenario` is sandbox-only test input. It must be accepted only by an explicit typed test configuration and must never flow from production workflow input.

### 5.5 REST responses and errors

Success is operation-specific. The MTD Income Tax descriptors already pin status codes such as `204` for create/amend cumulative summary. The VAT submission was observed returning `201 Created` in the 22 September sandbox run, but the authoritative endpoint definition must be pinned into the descriptor and its fixture rather than inferred from the observation.

HMRC uses normal HTTP categories and endpoint-specific error codes. The shared reference guide includes, among others:

- `401 INVALID_CREDENTIALS`;
- `403 RESOURCE_FORBIDDEN`, `INVALID_SCOPE` and `FORBIDDEN`;
- `406 ACCEPT_HEADER_INVALID`;
- `429 MESSAGE_THROTTLED_OUT`;
- `500 INTERNAL_SERVER_ERROR`;
- `503 SERVER_ERROR` or `SCHEDULED_MAINTENANCE`; and
- `504 GATEWAY_TIMEOUT`.

The adapter must retain the raw bounded response body and parse it into a distinct outcome without losing unknown fields. Authority business errors, local validation findings, authentication failures, throttling/transient faults, ambiguous network outcomes and unexpected local faults must remain different types.

For successful VAT submission, retain the response identifiers represented by `VatReturnResponse`, including processing date, form bundle number, charge reference and payment indicator when present. For bodyless `204` success, record the status and headers without inventing a response body.

No universal client-supplied correlation header is documented for these VAT/MTD endpoints. Record the application's own attempt ID and any authority correlation/request identifier actually returned, but do not invent or send an unsupported header.

### 5.6 Retry and ambiguity policy

HMRC's normal application limit is three requests per second, and the reference guide recommends pausing before retry after `429`. Retry policy must be operation-aware:

- GET enquiries may retry bounded transient network, `429`, `500`, `503` and `504` failures with jittered exponential backoff, respecting `Retry-After` if returned.
- Token refresh may occur once after an explicit authentication failure when safe, using atomic rotation.
- Identical MTD Income Tax `PUT` requests are naturally replace/create operations, but retries still require the same prepared bytes and the same durable attempt record.
- VAT submission is a `POST`. Never blindly replay it after a timeout, connection reset or other ambiguous failure after bytes may have left the process. Mark the attempt `OutcomeUnknown` and reconcile through obligations/view-return workflows or explicit operator action.
- Deterministic `4xx` contract/business errors are not transient and must not be retried unchanged.

Retries are additional attempts of one logical submission, not new submissions. Attempt number, timestamps and every response/fault must be durable.

### 5.7 Corporation Tax Transaction Engine and IRmark

Transaction Engine Document Submission Protocol v2.0 uses UTF-8 GovTalk 2.0 XML over HTTP POST. The normal sequence is:

```text
SUBMISSION_REQUEST
  -> SUBMISSION_ACKNOWLEDGEMENT(CorrelationID, ResponseEndPoint, PollInterval)
  -> wait PollInterval
  -> SUBMISSION_POLL(CorrelationID)
  -> acknowledgement again while pending, or terminal response/error
  -> persist terminal response and digital receipt
  -> DELETE_REQUEST(CorrelationID)
  -> DELETE_RESPONSE
```

The protocol permits retrieving the terminal response more than once before deletion, which is useful after a failed response download. Its `DATA_REQUEST` can list outstanding submissions and supports recovery/reconciliation.

The IRmark is calculated over the XML body with the IRmark element excluded, using W3C Canonical XML 1.0 without comments, SHA-1, then Base64 for the submitted 28-character value. The same digest may be Base32 encoded for human display. Whitespace and comment handling matter. HMRC's guidance specifically advises Base64-encoding Corporation Tax iXBRL attachments before IRmark calculation/transmission where comments cannot be avoided.

Although SHA-1 is obsolete for general security design, this is a protocol-mandated compatibility calculation, not a password or signature choice. It must be isolated, documented and tested only for IRmark. The application should continue using SHA-256 for its own artifact integrity.

The current Objective 3 diagnostic serializer does not build this full protocol envelope, does not calculate or insert an IRmark, and does not implement the acknowledgement conversation. Those are Objective 4 duties, but the exact immutable input and only permitted insertion point must first be made explicit.

### 5.8 Audit and evidence retention

The programme requires durable submission audit. For every logical submission retain at least:

- internal attempt ID, tenant, authority principal/agent context and environment;
- operation/service, contract version, prepared body/package SHA-256 and byte length;
- an immutable link to the exact prepared bytes/documents used, under an approved retention and encryption policy;
- source snapshot evidence and Objective 3 findings present at approval time;
- user/declaration approval evidence and initiating request time;
- required OAuth scope and a token-record identifier, never the token itself;
- fraud-header specification version, connection method and protected/redacted evidence sufficient for compliance investigation;
- dispatch/retry timestamps and fault classification;
- HTTP status, selected response headers and raw bounded response bytes for REST;
- Transaction Engine transaction/correlation IDs, poll interval/history, responses, delete result and signed digital receipt for Corporation Tax; and
- parsed authority references and final state.

Fraud headers contain personal/device/network data. They must not be copied into general logs. Retention, encryption, access, masking and deletion periods need an explicit security/privacy decision before production.

## 6. Historical VAT implementation assessment

The historical application remains useful as an end-to-end protocol probe. Its successful 22 September 2026 sandbox run proves that the broad OAuth-to-bearer-to-VAT flow and the fraud-header validator remain operational. It does not prove production suitability.

| Concept | Assessment | Reason |
|---|---|---|
| OAuth Authorization Code flow and user consent | Still valid in principle | This remains HMRC's required user-restricted model. Modern implementation should add PKCE, robust state/correlation protection and durable token storage. |
| `read:vat` and `write:vat` scopes | Still valid | They match the current VAT contract descriptors. Request least privilege for the approved journey. |
| Bearer token on VAT API calls | Still valid | Required by current user-restricted endpoint guidance. |
| Saving tokens in the ASP.NET authentication ticket | Incomplete/unsafe for target | It does not provide tenant/principal modelling, durable encrypted refresh rotation, concurrency control or operational recovery. |
| One URI used to build authorisation, token and API endpoints | Obsolete design | Current authorisation web host and API/token hosts are separate concerns and must be environment-profiled. |
| Creating `HttpClient` per controller action | Unsuitable | Use managed/factory clients with controlled handlers, timeouts and resilience. |
| Controller-built VAT JSON | Obsolete for Tax Hub | Objective 3 already owns canonical exact bytes. Transport must not rebuild them. |
| Versioned `Accept` header and JSON content type | Still valid | Current HMRC media-version policy uses the same pattern. Values should come from the prepared request. |
| Fraud-prevention-header concept | Still valid and mandatory | The current validator is specVersion 3.3. Header content/format must follow the current connection-method guide. |
| Deriving client public IP directly from `RemoteIpAddress` | Deployment-specific and incomplete | Behind a proxy it is often the proxy address; localhost produced `::1`. Trusted proxy/WAF topology is required. |
| Fixed configured vendor public IP | Deployment-specific and unsafe as a general rule | It may have represented the old Azure deployment. The actual egress/vendor hop must be environment-owned and verified. |
| Empty multi-factor and vendor-license headers | Incomplete | The validator warned. Current missing-data rules and actual product/licensing behaviour must drive omission/value decisions. |
| `Gov-Vendor-Forwarded` formatting | Incorrect/incomplete | The sandbox warning and current v3.3 first-hop/percent-encoding rules require a topology-aware builder. |
| New device GUID on every call | Likely semantically wrong | Device identifiers should be stable according to HMRC's header definition, not regenerated for each request. |
| Hard-coded test scenarios | Sandbox-only | Must be explicit test configuration and impossible in production. |
| Treating non-`200 OK` as failure for VAT submit | Incorrect | The verified submit returned `201 Created`; success is endpoint-specific. |
| Returning raw authority content directly to the browser | Unsafe/incomplete | Parse, bound, classify and audit responses; expose safe application outcomes. |
| No retry/ambiguity/durable attempt model | Incomplete | Production transport must survive timeouts, token rotation, throttling and process restarts without duplicate filing. |
| .NET 5 target | Obsolete | The current solution targets .NET 8. |

The historical code should not be copied. Its most useful reusable artifact is the proven sequence of external interactions and the observed fraud-header failure modes.

## 7. Current repository transport capabilities

### 7.1 Implemented and reusable

- Exact immutable REST body storage (`ImmutableArray<byte>`) with SHA-256.
- Resolved relative paths and ordered queries.
- Exact contract media-type headers.
- Credential-header exclusion from prepared requests.
- VAT and MTD Income Tax endpoint catalogues with OAuth scopes and API versions.
- VAT and MTD Income Tax success/error DTOs, including extensible HMRC error data for Income Tax.
- Corporation Tax acknowledgement/error domain types.
- Package/document separation and artifact digests.
- Preview/status flags and validation findings.
- Fake gateway tests proving unchanged handoff.
- A bounded WebHarness preview store and exact raw-body retrieval.
- Correct assembly direction for a submission adapter.

### 7.2 Placeholders or design intent only

- `HmrcSettings` contains only a free-form environment string.
- `EnvironmentSelector.Select` returns its input unchanged.
- `SubmissionLogger.LogAsync` does nothing.
- `IPreparedApiRequestGateway` and `IPreparedSubmissionPackageGateway` have no implementation.
- `PreparedPollingSemantics` expresses intent but no state machine.
- `PreparedArtifactStatus.SubmissionReady` exists, but current company preparers create `Preview` artifacts.
- Corporation Tax and Companies House acknowledgement records exist but are never parsed or returned.

### 7.3 Legacy diagnostics, not transport

`HmrcSubmissionRunner` uses dictionary requests, legacy payload builders and validators, then simulates a success response and creates a local GUID. Its enquiries report “not implemented.” It never calls HMRC. `VatTestController` and `MicroTestController` invoke this simulation path.

The modern preparation controllers also remain harness-only. They construct adapters manually and currently accept a connection string in the HTTP body, contrary to the final design guidance; this should not be copied into Objective 4 or Objective 5.

### 7.4 Missing entirely

There is no implemented OAuth client, token store, fraud-header collector/builder, REST dispatcher, Transaction Engine client, IRmark calculator, GovTalk envelope builder/parser, retry policy, durable submission store, receipt verifier, response mapper, safe authority logging or production environment configuration.

## 8. Gap analysis

| Gap | Evidence | Why Objective 4 owns it |
|---|---|---|
| Closed environment profiles and endpoint allow-list | Placeholder `EnvironmentSelector`; external sandbox/live endpoints | Routing is transport, not payload meaning. |
| OAuth authorisation, encrypted token store and refresh rotation | No implementation; HMRC four-hour/single-use rules | Authentication state is explicitly Objective 4. |
| Scope metadata at gateway | Descriptors have scope; prepared request drops it | Transport must select/validate credentials without duplicating contract catalogues. Narrow boundary amendment required. |
| Expected success/response metadata | MTD descriptors have it; VAT/prepared request do not | Response interpretation is transport, while versioned expectation originates with the contract. |
| Tenant/principal/request context | Current gateway accepts only the prepared artifact | Credential selection and truthful fraud headers require separate typed transport context; they cannot be inferred from tax identifiers. |
| Special VAT period keys | Current retrieve validator rejects HMRC-documented `#001`-style keys | Versioned contract validation must be corrected before transport; Objective 4 only escapes and transmits the approved value. |
| Fraud-header client capture and server builder | Only obsolete historical sample exists | These are per-request transport metadata, not tax data. |
| Exact-byte REST dispatch | Prepared bytes exist; no gateway | Core Objective 4 responsibility. |
| REST response/error mapping | DTOs exist; gateway returns only `Task` | Objective 4 receives authority outcomes. Gateway contract must return them. |
| Durable attempt/outcome state | Preview store is explicitly not audit; logger is no-op | Audit, retry safety and crash recovery are Objective 4. |
| Operation-aware retry and ambiguous-outcome handling | None | Network reliability must not reinterpret or duplicate a filing. |
| Production Corporation Tax body/package | Serializer says diagnostic; taxonomy assets missing | Objective 4 cannot transmit preview bytes; missing official assets are a precondition shared with Objective 3 contract assurance. |
| GovTalk envelope, credentials and IRmark | Not present | Programme explicitly assigns XML envelope/canonicalisation/IRmark to Objective 4. |
| Transaction Engine acknowledgement/poll/delete | Repository incorrectly says no polling | Protocol transport state belongs to Objective 4; metadata must be corrected first. |
| Receipt parsing/verification and recovery | Acknowledgement type only | Authority receipt handling and durable state are Objective 4. |
| Companies House live filing | Explicitly deferred; missing Filing TIS schemas | Separate authority transport; not enabled by this HMRC work. |

No gap in this table requires recalculating VAT boxes, MTD business income/expenses, Corporation Tax, CT600 fields or iXBRL facts. Those remain earlier-objective responsibilities.

## 9. Proposed Objective 4 implementation architecture

### 9.1 Application contract corrections required first

Make the smallest metadata-only changes necessary to express the real boundary:

1. Extend `PreparedApiContract` and `PreparedApiRequest` with:
   - required OAuth scope;
   - accepted success status code(s);
   - a stable response contract identifier or response-body expectation (`None`, `Json`, possibly exact type metadata kept in the contract layer); and
   - an explicit dispatch eligibility flag derived from supported/preview status.
2. Add the authoritative success status to `VatOperationDescriptor` and its fixtures.
3. Change the API gateway to return a typed outcome, for example `Task<PreparedApiOutcome> SendAsync(...)`, rather than discarding HMRC's response.
4. Add a separate typed dispatch context to gateway calls containing tenant, authority-principal reference, initiating-user approval and the sealed fraud-prevention context where applicable. It contains no tax-body reconstruction instructions.
5. Change the package gateway to return a typed package outcome/receipt and receive only the credential-selection context it needs.
6. Correct Corporation Tax protocol metadata to require the Transaction Engine conversation. Do not model it as a simple REST polling path.
7. Clarify `PreparedSubmissionPackage` by separating:
   - immutable semantic/service artifacts supplied by Objective 3; and
   - an Objective 4 transport envelope generated only from those artifacts plus credentials, IRmark and protocol metadata.
8. Align VAT period-key validation with the current official endpoint contract, retaining ordinary and special keys and relying on the existing path escaping.
9. Reject any artifact whose status is not `SubmissionReady`.

These changes add routing/response metadata and outcome flow. They must not change the Objective 3 JSON or iXBRL semantic bytes.

### 9.2 Shared transport services

Place these in `TradeControl.Tax.UK.Adapters.Submission` unless an interface is needed by Application:

- `AuthorityEnvironmentOptions` — closed sandbox/production records with allow-listed hosts;
- `SubmissionAttemptStore` — durable logical submission and per-attempt state;
- `AuthoritySecretProvider` — returns secret handles/values without exposing them to prepared artifacts;
- `TransportClock`/`TimeProvider` use;
- `TransportRedactor` — allow-list logging with token, credential, identifier and fraud-header protection;
- `TransportOutcomeMapper` — shared coarse categories only; protocol parsers remain separate; and
- telemetry for latency, retry, throttle and terminal status, keyed by safe operation/version/environment labels.

Avoid a generic authority payload builder. Objective 3 has already built the payload.

### 9.3 REST-specific components

Suggested components:

- `OAuth/HmrcAuthorizationService` — builds state/PKCE authorisation requests and handles callbacks;
- `OAuth/HmrcTokenStore` — encrypted, tenant/principal scoped, atomic rotation;
- `OAuth/HmrcAccessTokenProvider` — scope validation and refresh locking;
- `FraudPrevention/FraudPreventionContext` — sealed client facts captured by the initiating web workflow;
- `FraudPrevention/WebAppViaServerHeaderBuilder` — exact v3.3 formatting and proxy-hop rules;
- `Rest/HmrcApiPlatformClient` — managed HTTP client and endpoint allow-list;
- `Rest/HmrcPreparedApiRequestGateway` — implements the Application port, sends exact bytes and returns typed outcomes;
- `Rest/HmrcApiErrorParser` — preserves endpoint-specific error codes/details and unknown fields; and
- `Rest/HmrcRestRetryPolicy` — uses operation method/idempotence and durable attempt state.

The gateway algorithm should be mechanically simple:

```text
validate supported/non-preview/no-error request
  -> reserve durable logical attempt by operation + subject + prepared digest
  -> obtain correctly scoped access token
  -> build current fraud headers from sealed request context
  -> compose allow-listed absolute URI
  -> copy prepared headers/query/body bytes unchanged
  -> send once
  -> persist raw response/fault before parsing
  -> parse to typed outcome
  -> retry only when policy proves safe
  -> return outcome to Application
```

### 9.4 Transaction Engine-specific components

Suggested components:

- `TransactionEngine/TransactionEngineOptions` — test/live submission and allowed response hosts;
- `TransactionEngine/GovTalkEnvelopeBuilder` — creates only transport-owned envelope/header fields around the immutable service payload;
- `TransactionEngine/IrmarkCalculator` — exclusive canonical XML 1.0 without comments, SHA-1 and Base64/Base32 implementation;
- `TransactionEngine/GovTalkParser` — hardened XML settings, no DTD/external entity resolution, bounded documents;
- `TransactionEngine/TransactionEngineClient` — submission, poll, data recovery and delete messages;
- `TransactionEngine/CorporationTaxSubmissionGateway` — package port implementation and state machine;
- `TransactionEngine/CorporationTaxReceiptParser` — terminal response/business errors/digital receipt;
- `TransactionEngine/ReceiptVerifier` — verifies the signature/certificate according to the pinned HMRC receipt schema and trust policy; and
- `TransactionEngine/OutstandingSubmissionReconciler` — recovers acknowledged submissions after process interruption before considering resubmission.

Credentials belong only in the transport envelope at send time. The prepared package and audit-facing metadata must never contain their clear values.

The state machine should persist before and after every network transition:

```text
Prepared -> Dispatching -> Acknowledged(correlation, endpoint, interval)
         -> Polling -> Accepted | Rejected | ProtocolFailed | OutcomeUnknown
         -> ReceiptStored -> DeletePending -> Deleted
```

An accepted/rejected business result is not the same as successful deletion. A delete failure should retain `DeletePending` for recovery without changing the filing result.

### 9.5 Security and configuration

- Bind environment options at startup and fail startup on invalid/mixed hosts.
- Keep OAuth client secrets, tokens and Government Gateway credentials in an approved external secret store; do not use committed `appsettings` values.
- Encrypt sensitive database columns and restrict operational access.
- Use hardened XML readers and size limits before parsing authority XML.
- Never follow authority-supplied endpoints outside the environment allow-list.
- Redact authorisation headers, tokens, credentials, NINO/UTR/VRN, full response bodies and fraud-header values from ordinary logs.
- Preserve raw audit artifacts in protected storage separate from searchable diagnostic logs.
- Require an explicit production enablement switch and recognised contract/product evidence.

## 10. File/type-level implementation map

| Location | Existing type to use/change | Proposed work |
|---|---|---|
| `Application/Preparation/PreparedApiRequestPipeline.cs` | `PreparedApiContract` | Carry required scope, accepted status and response-body expectation. |
| `Application/Preparation/PreparedArtifacts.cs` | `PreparedApiRequest` | Add immutable dispatch metadata; retain exact bytes unchanged. |
| same | `IPreparedApiRequestGateway` | Accept a separate typed dispatch context and return a typed REST authority outcome. |
| same | `PreparedSubmissionPackage`, `PreparedPollingSemantics` | Express protocol family/service payload versus transport envelope; remove REST-shaped assumptions for Transaction Engine. |
| same | `IPreparedSubmissionPackageGateway` | Accept credential-selection context and return typed acknowledgement/receipt outcome. |
| `Hmrc.Vat.Contracts/VatContractInfrastructure.cs` | `VatOperationDescriptor` | Pin operation success status/response expectation from current official endpoint reference. |
| `Hmrc.MtdIncomeTax.Contracts/.../ContractInfrastructure.cs` | `HmrcEndpoint` | Reuse existing scope, success status and response type; no payload changes. |
| `Company.Contracts/.../CorporationTaxPackageSerializer.cs` | diagnostic serializer | Keep explicitly preview-only or replace through a separately reviewed submission-ready service-body serializer once official assets are present. Do not relabel current bytes. |
| `Company.Contracts/.../CorporationTaxPackage.cs` | `CorporationTaxEndpointSet` | Correct the polling/conversation declaration and pin class/version/receipt contracts. |
| `Adapters.Submission/Configuration` | placeholder settings/selector | Replace with validated REST and Transaction Engine environment records. |
| `Adapters.Submission/OAuth` | new | Authorisation, callback, encrypted token store and atomic refresh. |
| `Adapters.Submission/FraudPrevention` | new | Topology-specific collection/formatting and validator test support. |
| `Adapters.Submission/Rest` | new | Exact-byte HMRC API client, response/error parser and retry policy. |
| `Adapters.Submission/TransactionEngine` | new | GovTalk, IRmark, submit/poll/delete/recovery and receipts. |
| `Adapters.Submission/Audit` | `SubmissionLogger` placeholder | Replace with durable attempt/audit store; keep ordinary logs redacted. |
| `WebHarness/Program.cs` | composition root | Register typed clients and fake/sandbox-only diagnostic endpoints; never enable production by default. |
| new Objective 4 test projects under `src/tax-hub/tests` | existing test style | Add adapter, protocol fixture, sandbox-contract and architecture tests. |

The WebHarness legacy runner should remain visibly separate until removed by a later controlled cleanup. It must not become a wrapper over production credentials or be presented as the new gateway.

## 11. Proposed implementation phases

### Phase 4.0 — Boundary correction and safety gate

- Add missing REST dispatch/response metadata.
- Return typed outcomes from gateway ports.
- Correct Corporation Tax polling metadata.
- Define semantic artifact versus transport envelope ownership.
- Add a hard non-preview/no-errors/submission-ready gate.

**Acceptance:** existing golden JSON/iXBRL bytes and SHA-256 values are unchanged; architecture tests still pass; a transport fake can select scope/status/parser without a duplicate operation table; current company packages are rejected as previews.

### Phase 4.1 — Durable attempts, configuration and redaction

- Implement closed environment profiles, secret-provider boundary and durable attempt store.
- Define state transitions, uniqueness/idempotency keys and ambiguous outcome handling.
- Add redacted structured logging/telemetry.

**Acceptance:** crash/restart tests resume a pending attempt; tokens/credentials/fraud values never appear in logs; production cannot be selected from request input.

### Phase 4.2 — OAuth and fraud-prevention foundation

- Implement Authorization Code Grant with PKCE/state.
- Implement encrypted token storage and atomic single-use refresh rotation.
- Implement the confirmed deployment connection method and trusted proxy rules.
- Integrate the sandbox fraud-header validator.

**Acceptance:** parallel refresh test spends one refresh token once; scope mismatch blocks dispatch; validator shows zero errors and all warnings are reviewed; localhost/private addresses cannot masquerade as public values.

### Phase 4.3 — REST enquiries

- Implement the exact-byte REST gateway first for approved bodyless obligations/view operations.
- Add response DTO mapping and error classification.
- Exercise throttling, timeouts and token expiry safely.

**Acceptance:** sandbox VAT and Income Tax obligations calls use correct version/scope/fraud headers; raw and parsed outcomes are durably linked; bounded GET retries work.

### Phase 4.4 — VAT and MTD Income Tax writes

- Send prepared VAT `POST` and cumulative-summary `PUT` bodies as byte content.
- Persist VAT receipt identifiers and empty-body MTD success correctly.
- Add ambiguous VAT outcome reconciliation and explicit no-blind-replay policy.

**Acceptance:** a recording handler proves transmitted body bytes equal Objective 3 bytes and digest; sandbox VAT returns `201` and cumulative write returns its pinned success status; known error scenarios map without losing HMRC codes; no serializer is reachable in the adapter.

### Phase 4.5 — Corporation Tax submission-readiness prerequisite

- Obtain/pin official computation taxonomy and current CT validation assets.
- Produce a submission-ready service payload and golden official-validation fixtures.
- Confirm current Transaction Engine class, credentials, endpoints, receipt schema and recognition scenarios with HMRC/SDST.

**Acceptance:** all artifacts are explicitly `SubmissionReady`; diagnostic serializer output cannot enter the live gateway; official local/test validation succeeds. This phase may require a separately controlled Objective 3 contract-assurance change, but no tax values may be altered by transport.

### Phase 4.6 — Transaction Engine and IRmark

- Implement canonicalisation/IRmark and GovTalk envelope.
- Implement submit/acknowledge/poll/terminal/delete/recovery.
- Persist and verify receipts.

**Acceptance:** HMRC published IRmark examples match exactly; XML schema/protocol fixtures pass; polling honours returned interval; restart recovery uses correlation ID; terminal responses are retrieved before deletion; exact Objective 3 attachment bytes survive unchanged; ETS/TIL scenarios and recognition evidence pass.

### Phase 4.7 — Production hardening and handoff to Objective 5

- Capacity/rate-limit tests, observability, security/privacy review, key rotation and runbooks.
- Expose safe application outcomes for later UI workflow integration.

**Acceptance:** production enablement requires approved credentials/recognition and explicit configuration; operational drills cover token revocation, `429`, HMRC maintenance, ambiguous POST and pending Transaction Engine correlation IDs.

## 12. Acceptance and test strategy

### 12.1 Offline unit and golden tests

- Assert exact URI, query, method, media headers, scope and expected status for every supported operation.
- Feed a recording HTTP handler and compare transmitted body byte-for-byte with `PreparedApiRequest.BodyBytes` and SHA-256.
- Assert no JSON serializer is invoked in submission adapter code.
- Test all documented error shapes, unknown JSON properties, empty success bodies and oversized/malformed responses.
- Use deterministic clocks for token expiry, refresh skew, retry and audit state.
- Test concurrent token refresh and tenant/principal isolation.
- Test every v3.3 fraud-header value/encoding rule for the selected topology.
- Match HMRC's published canonical payload, IRmark Base64/Base32 and response examples.
- Test XML entity/DTD rejection, namespace correctness, size limits and receipt signature failures.

### 12.2 State-machine and fault-injection tests

Inject faults before connect, during upload, after upload/before response, after acknowledgement, during polling, after terminal response storage and during delete. Prove that:

- safe operations retry within bounds;
- ambiguous VAT POSTs become `OutcomeUnknown` and are not replayed;
- Transaction Engine recovery continues from a persisted correlation ID;
- a receipt is durable before delete; and
- one logical submission cannot be concurrently dispatched twice.

### 12.3 Sandbox and external-test evidence

- VAT: authorisation, obligations, exact prepared return, `201` response, receipt identifiers and post-submit obligation reconciliation.
- MTD Income Tax: business/obligation discovery, exact cumulative `PUT`, retrieve/verify where supported and approved test scenarios.
- Fraud headers: validator `validate` during development and `validation-feedback` after real sandbox calls.
- Corporation Tax: local/official validation assets, ETS submit/poll/delete, TIL where applicable, HMRC recognition scenarios and retained receipts.

Sandbox tests must use newly created synthetic users and secret configuration. Test identifiers, credentials and full fraud-header output must not be committed.

### 12.4 Architecture tests

- Contract projects remain independent of Application and adapters.
- Application never references `HttpClient`, OAuth libraries, secret stores or GovTalk implementation types.
- Submission adapter may reference Application contracts but not Trade Control SQL adapters.
- WebHarness normal actions call application use cases; production transport is not assembled inside controllers.
- No transport project references legacy WebHarness payload builders or dictionary runner types.

## 13. Risks, uncertainties and decisions requiring confirmation

1. **Corporation Tax boundary:** decide and document the exact immutable service-body artifact that Objective 4 may wrap and the reserved IRmark insertion mechanism. Current `Transmission` bytes cannot be used.
2. **Official computation taxonomy assets:** live Corporation Tax remains blocked until the repository can validate against the official bundle; derived QNames are explicitly not submission-ready.
3. **Corporation Tax metadata:** current `Polling.None`/`RequiresStatusPolling=false` conflicts with the Transaction Engine protocol and must be corrected.
4. **REST success metadata:** pin VAT submit's official success status and all supported response contracts, rather than relying on the successful sandbox observation.
5. **Gateway context and outcomes:** approve a narrow breaking change that supplies typed tenant/principal/request context and returns typed outcomes. Without it, Objective 4 cannot safely select credentials or return HMRC results to Application/Objective 5.
6. **VAT special period keys:** correct the current alphanumeric-only retrieve validator for HMRC-documented `#` period keys and add fixtures before enabling that operation.
7. **Deployment topology:** confirm web application via server, all proxy/WAF hops, public egress addresses and how browser/device facts are captured.
8. **OAuth principal model:** decide how direct businesses and agents with multiple HMRC accounts are keyed to tenants and clients.
9. **Audit privacy/retention:** approve storage, encryption, access and retention for exact payloads, identifiers, responses and fraud-header evidence.
10. **Ambiguous VAT submission UX:** Objective 5 must provide a reconciliation/operator path; Objective 4 must not silently retry.
11. **MTD Income Tax production access:** HMRC's 15 September 2026 guide says new 2026–27 quarterly-update product credential requests are no longer being accepted because that market window has closed. Product/credential status must be confirmed before scheduling production launch; sandbox work can continue.
12. **Rate limits:** three requests per second is the normal HMRC application limit, not a guaranteed per-operation allowance. Production capacity and any approved exception must be configuration/evidence, not code constants.
13. **Companies House:** it is a separate authority and remains explicitly deferred. Shared audit infrastructure must not be mistaken for authorisation to implement or transmit its logical preview envelope.

## 14. Authoritative HMRC references

### REST platform and authorisation

- [HMRC API reference guide](https://developer.service.hmrc.gov.uk/api-documentation/docs/reference-guide)
- [Authorisation overview](https://developer.service.hmrc.gov.uk/api-documentation/docs/authorisation)
- [User-restricted endpoints](https://developer.service.hmrc.gov.uk/api-documentation/docs/authorisation/user-restricted-endpoints)
- [VAT (MTD) API v1.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0)
- [VAT MTD end-to-end service guide](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/)
- [VAT obligations and returns](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/documentation/obligations.html)
- [Self Employment Business (MTD) API v5.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/self-employment-business-api/5.0)
- [MTD Income Tax end-to-end service guide](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/)

### Fraud-prevention headers

- [Choose the connection method](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/connection-method/)
- [Web application via server](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/connection-method/web-app-via-server/)
- [Fraud-header format and missing-data guidance](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/getting-it-right/)
- [Fraud-prevention change log, including v3.3](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/change-log/)
- [Test Fraud Prevention Headers API](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/txm-fph-validator-api/1.0)

### Corporation Tax, Transaction Engine and IRmark

- [Corporation Tax online support for software developers](https://www.gov.uk/government/collections/corporation-tax-online-support-for-software-developers)
- [Corporation Tax generic technical specifications](https://www.gov.uk/government/publications/corporation-tax-generic-technical-specifications)
- [Corporation Tax Online XML API landing page](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/xml/Corporation%20Tax%20Online)
- [Basic guide for XML software developers](https://www.gov.uk/guidance/basic-guide-for-xml-software-developers)
- [Transaction Engine support collection](https://www.gov.uk/government/collections/government-gateway-support-for-software-developers)
- [Transaction Engine Document Submission Protocol v2.0](https://www.gov.uk/government/publications/transaction-engine-document-submission-protocol)
- [IRmark support collection](https://www.gov.uk/government/collections/hmrcirmark-support-for-software-developers)
- [IRmark for Gateway Protocol services](https://www.gov.uk/government/publications/hmrc-irmark-for-gateway-protocol-services)
- [Generic IRmark specification](https://www.gov.uk/government/publications/hmrc-irmark-generic-irmark-specification)

## 15. Completion-gate answer

Between Objective 3 and HMRC, Tax Hub must build a durable, tenant-aware submission platform with two concrete paths:

- an OAuth- and fraud-header-aware REST adapter that transmits `PreparedApiRequest` paths, queries, headers and JSON bytes exactly, then maps and audits operation-specific HTTP outcomes; and
- a Corporation Tax Transaction Engine adapter that wraps approved immutable CT600/iXBRL artifacts in the pinned GovTalk protocol, generates the protocol-mandated IRmark without changing their statutory meaning, persists the acknowledgement correlation ID, polls to a terminal response, retains/verifies the digital receipt and deletes the stored response.

It belongs primarily in `TradeControl.Tax.UK.Adapters.Submission`, implementing strengthened ports defined by Application. The existing prepared artifacts, operation catalogues, response DTOs, package documents and unchanged-handoff tests should be reused. The historical VAT application should be retained only as evidence.

Correctness is proven by byte-identity tests at the REST send boundary; official JSON/error fixtures and sandbox journeys; fraud-header v3.3 validation; HMRC-published canonicalisation/IRmark examples; official Corporation Tax schema/taxonomy validation; and a fault-injected, restart-safe Transaction Engine acknowledgement/poll/delete state machine.

Production enablement is blocked until the four boundary conflicts in the executive summary are resolved. Resolving them must add transport metadata and a valid packaging boundary, not reinterpret or regenerate the tax meaning already prepared by Objective 3.

# Boundary Resolution Review

**22 September 2026. Status:** candidate boundary for a controlled implementation brief; this review does not approve transmission or change code. “Verified repository” and “verified HMRC” below identify evidence; “recommendation” identifies a proposed correction. Earlier sections remain reconnaissance, with the qualifications here taking precedence where they differ.

## Four conflict verdicts

### 1. REST dispatch metadata — confirmed, with a narrower remedy

**Verified repository.** `HmrcEndpoint` in `Hmrc.MtdIncomeTax.Contracts/MtdIncomeTax/v1_0/Shared/ContractInfrastructure.cs` already has `OAuthScope`, one `SuccessStatusCode`, `ResponseType`, API version, method, path, media types and request-body presence. For example, cumulative `PUT` declares `write:self-assessment`, `204` and no response DTO; cumulative `GET` declares `read:self-assessment`, `200` and a response DTO. The MTD contract tests assert these statuses. `VatOperationDescriptor` in `Hmrc.Vat.Contracts/VatContractInfrastructure.cs` has scope and response `Type`, but no success status. Its supported operations are submit return, obligations and view return; other entries are deferred. `HmrcPreparedApiContracts.From(...)` copies version, method, path, parameters and media types, discarding scope, status, response type and coverage decision. `PreparedApiRequestPipeline` then freezes only the copied values in `PreparedApiRequest`. Both gateway ports currently return `Task`, so even an HMRC response cannot cross back. The prepared-artifact test explicitly forbids public member names containing `OAuth` or `Response`; that assertion must be refined to exclude credentials and received responses, while allowing contract metadata.

**Verified HMRC.** The [VAT API version 1.0 landing page](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0) (updated 5 August 2026) identifies REST endpoints and requires fraud-prevention headers; endpoint-specific success responses are authoritative, not its generic `2xx` explanation. **Recommendation.** Pin VAT's success status for each *supported* operation against its endpoint reference (submit `201`, supported enquiries `200`, subject to endpoint verification at implementation). Copy scope, exact accepted success status and response-body expectation through `PreparedApiContract` into `PreparedApiRequest`. Use the existing descriptor `ResponseType` as the narrow response contract marker: null means no success body; a non-null `Type` identifies the already-defined DTO. Validate the paired status/body expectation (`204` must be empty). Include a supported/dispatchable flag derived from VAT `AccountsMode` or MTD `SaAccountsModeDecision`; `IsPreview` alone does not exclude deferred VAT operations. No second adapter-side operation catalogue or generic metadata framework is needed. These are contract facts, not new tax semantics; `BodyBytes`, path and query stay identical. At dispatch, Objective 4 needs scope and eligibility. At response interpretation, it needs status and body expectation; error shapes remain parsed from HMRC's actual response. An unknown status is an unexpected authority outcome, not success merely because it is `2xx`.

### 2. Corporation Tax `Transmission` — confirmed; current bytes are unusable for submission

**Verified repository.** `CorporationTaxPreparer` takes populated `Ct600Return` and computation plus full company accounts, projects and builds computation iXBRL, copies full accounts iXBRL, creates a `CorporationTaxReturnPackage`, validates selected relationships, then calls `CorporationTaxPackageSerializer.Serialize`. That serializer says its XML is a deterministic diagnostic for offline reconciliation, explicitly “not a gateway payload”; it hand-builds a small `IRenvelope` rather than serialising the generated RIM 1.994 graph. Nevertheless the preparer puts it into `PreparedSubmissionPackage.Transmission`. `Documents` separately hold accounts and computation iXBRL. All three prepared artifacts are `Preview`. `CorporateHandoffTests` prove the same object and bytes reach a fake gateway; they do not prove HMRC validity. The `correlationId` accepted by the preparer is embedded in the diagnostic XML; it is not an HMRC-issued Transaction Engine correlation ID.

**Verified HMRC.** The [Transaction Engine protocol v2.0](https://assets.publishing.service.gov.uk/media/5b90f59de5274a0bd7d11954/Transaction.pdf) (published via GOV.UK 6 September 2018) puts one opaque, single-root service document in the GovTalk `Body`. The [CT600 RIM artefacts](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-ct600-rim-artefacts) identify V3 (2026) v1.994 as live from 7 April 2026. HMRC's [Company Tax Return obligations](https://www.gov.uk/guidance/company-tax-return-obligations) (updated 1 June 2026) require CT600, applicable supplementary pages, accounts and computations; the latter two are iXBRL. The [electronic communications direction](https://www.gov.uk/government/publications/directions-under-regulations-3-and-10-of-the-income-and-corporation-taxes-electronic-communications-regulations-2003-si-2003282/income-and-corporation-taxes-electronic-communications-direction) (updated 8 July 2026) requires the CT600 XML to conform to the relevant XML Schema and Schematron, and separate iXBRL instances within the overall return.

**Recommendation.** Objective 3 should eventually hand over one immutable, validated **CT600 service-root artifact** (`IRenvelope` conforming to the pinned RIM), containing the complete CT600/supplementary statutory content and the separately identifiable, approved accounts and computation iXBRL attachments in their schema-required representation. `Documents` retain original attachment bytes and digests for traceability; a test must prove the attachments represented in the service document are derived exactly from those originals. The service artifact should have an explicit, uniquely located IRmark insertion slot and an unmarked digest. Objective 4 may wrap that root in a GovTalk `Body`/`GovTalkMessage`, add protocol class, sender credentials, transaction ID and other transport fields, canonicalise the specified `Body`, calculate the mandated IRmark, and insert **only** its value into the reserved element. The final wire digest must be recorded separately. It may not rebuild CT600 fields, re-encode attachments from accounting objects, alter iXBRL content or insert other values inside the service root. HMRC's [IRmark Gateway Protocol guide v2.0](https://assets.publishing.service.gov.uk/media/5a74efece5274a3cb2868633/irmark_step_by_step_govtalk.pdf) (19 July 2011) explicitly hashes the canonicalised `Body` with the IRmark node excluded, so this one protocol-defined insertion is the necessary exception to byte identity for the service XML. Accounts and computation source bytes remain immutable. The precise slot/QName and attachment representation must be pinned from the current RIM/sample XML before a submission-ready serializer is implemented; the diagnostic XML must never be relabelled.

### 3. Corporation Tax polling — confirmed; the generic mode is only a coarse hint

**Verified repository.** `CorporationTaxEndpointSet.Submit` has `RequiresStatusPolling=false`; `CorporationTaxPreparer` supplies `PreparedPollingSemantics(SubmissionPollingMode.None)`; `CorporateHandoffTests` assert `None`. `PreparedPollingSemantics` offers `None` or `PollUntilTerminal` plus an optional relative status path, originally suitable for Companies House. There is no Transaction Engine outcome or persistent conversation. **Verified HMRC.** Protocol v2.0 sections 3.2–3.9 require submission, an acknowledgement carrying `CorrelationID`, `ResponseEndPoint` and `PollInterval`, repeated `SUBMISSION_POLL`, a terminal business response/error, and `DELETE_REQUEST` after processing. `DATA_REQUEST` supports recovery when response state is uncertain. Deleting early does not recall the filing.

**Recommendation.** Keep the existing package port for now, set `RequiresStatusPolling=true`, and add **one** `SubmissionPollingMode.TransactionEngine` value for the CT package. Require its `RelativeStatusPath` to be null; the response supplies the poll endpoint and interval, which Objective 4 must validate against configured HMRC hosts. Do not set the generic `PollUntilTerminal` value or manufacture a REST path. This enum value names the required conversation, while the CT-specific Objective 4 handler owns acknowledgement, polling, recovery and deletion. `PreparedPollingSemantics` is therefore an imperfect name, but renaming it throughout the company path adds no immediate safety. If another Transaction Engine service later differs materially, revisit the shape then. The old `HmrcCorporationTaxAcknowledgement` (`Received/Accepted/Rejected`) is not sufficient: the package gateway must return a CT-specific outcome that distinguishes pending correlation, terminal accepted/rejected with receipt/error, and unknown/recovery-required state; deletion is recorded as transport completion, not a different tax verdict. Correct the tests that currently ratify `None`.

### 4. Corporation Tax readiness — confirmed, with two distinct gates

**Verified repository.** `CompanyAccountsPreparer` always emits `Preview`; `CorporationTaxPreparer` also marks computation, diagnostic transmission and copied accounts `Preview`. `CompanyContractRegistry.HmrcCt600V1994.SubmissionReady` is true for the *versioned CT600 contract*, while `HmrcComputationTaxonomy2025.SubmissionReady` is false because the official validation bundle is not provisioned and derived QNames are barred from submission. `CompanyServiceCoverageCatalog` defers CT submission. `CompanyContractValidator` checks selected reconciliations and that facts occur in its supported catalog; it does not establish full official RIM/Schematron or iXBRL taxonomy validity. The handoff tests only check immutability. Thus “CT600 contract ready” does not mean “prepared package ready.”

**Verified HMRC.** HMRC lists [2025 computation taxonomy and accepted FRC taxonomies](https://www.gov.uk/government/publications/taxonomies-accepted-by-hm-revenue-and-customs/taxonomies-accepted-by-hmrc) (updated 17 April 2026), publishes [CT600 RIM v1.994](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-ct600-rim-artefacts) and [CT online validation rules v1.17a](https://www.gov.uk/government/publications/corporation-tax-generic-technical-specifications), and updates the [iXBRL technical guidance and validations](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-xbrl-and-ixbrl) (15 May 2026). **Recommendation.** Objective 3 contract assurance must provision/version the official computation taxonomy and applicable accounts taxonomy, RIM XSD/Schematron, validation rules and representative official samples; produce and validate the real service root and both separate iXBRL documents; establish periods, declarations, mappings, attachment encoding and digest linkage; then promote each artifact and the whole package to `SubmissionReady` only with no blocking findings. Objective 4 must reject any preview/error/unsupported or mixed-readiness package at entry. Separately, Objective 4 must validate GovTalk, IRmark, credentials, Transaction Engine class/endpoints, signed receipt handling and recognition/test-service journeys. Actual sandbox or live transmission requires **both** gates plus explicit authorised filing context. CT transport can be built and exercised against synthetic protocol fixtures while the Objective 3 gate is closed; no current preview package may be sent to HMRC. The exact RIM slot/attachment mapping and current CT class/recognition evidence remain open until the official artefacts and test service confirm them.

## Candidate crossing and outcome

| Crossing | Minimal content and owner | Permitted Objective 4 action / returned result |
| --- | --- | --- |
| REST prepared request | Objective 3: existing immutable operation/version, method, escaped relative path, ordered query, media headers, optional canonical body bytes/digest, findings, plus scope, exact success status, response DTO/body expectation and supported status. | Choose configured HMRC base host, acquire scoped token, add truthful transport/fraud headers, send the exact body bytes. Return a typed REST outcome with attempt ID, actual status, selected headers, bounded raw response reference, parsed success DTO or HMRC error, and unknown/transient classification. Do not put a received response onto the prepared request. |
| REST dispatch context | Application/workflow: tenant ID, **authorisation-principal reference**, authenticated initiating actor/approval reference, and a trusted captured client-facts reference for an interactive request. Environment comes from trusted deployment configuration, not browser input. | Resolve credentials/grant and audit/approval, seal/validate facts, derive HMRC headers. Tax identifiers and `SourceEvidence` must not be used to guess principal. For a later noninteractive request, use a separately defined lawful provenance mode; do not invent browser facts. |
| CT service package | Objective 3: pinned protocol/service identity, validated service-root bytes with reserved IRmark slot, separately named full accounts and computation iXBRL artifacts, digests, source findings and whole-package readiness. | Build GovTalk around it and fill only IRmark. Return CT-specific pending/terminal/unknown outcome, correlation ID and receipt/error reference; track delete acknowledgement separately. No tax field or attachment regeneration. |

The existing `IPreparedApiRequestGateway` and `IPreparedSubmissionPackageGateway` can carry the respective context and return typed outcomes. No common REST/XML outcome hierarchy is needed. Approval evidence belongs to the dispatch command or its durable attempt, not to the canonical tax bytes. A response DTO `Type` is contract metadata only; Objective 4 still stores the bounded raw response and must not infer success from DTO deserialisation alone.

## Minimal changes before transport

| Type/file | Candidate correction | Effect on prepared semantic bytes |
| --- | --- | --- |
| `VatOperationDescriptor` in `VatContractInfrastructure.cs` | Add authoritative success status per supported operation; use existing response type and scope. | None. |
| `PreparedApiContract`, `HmrcPreparedApiContracts.From`, `PreparedApiRequestPipeline`, `PreparedApiRequest` in `Application/Preparation` | Carry scope, success status, response-body/DTO marker and supported flag unchanged from one descriptor; validate completeness. Update tests that forbid any `OAuth`/`Response` member. | None; assert existing golden bodies/digests and paths. |
| `IPreparedApiRequestGateway` in `PreparedArtifacts.cs` | Add narrow dispatch context and `Task<PreparedApiOutcome>`; define response/outcome records beside the port. | None. |
| `CorporationTaxEndpointSet`, `CorporationTaxPreparer`, `SubmissionPollingMode`, `IPreparedSubmissionPackageGateway` | Correct CT conversation marker/port outcome and test assertions; block diagnostic `Transmission` from dispatch. Keep company preview artifacts until the separate Objective 3 service-root change. | Current preview bytes unchanged; future service bytes require their own reviewed contract-assurance phase. |
| `BodylessRequestDescriber` | Correct supported VAT view-return period-key validation for HMRC's [documented special keys](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/documentation/obligations.html), including `#001`, in a small Objective 3 contract fix before enabling that enquiry. | No existing VAT JSON body affected. |

These are the minimum **boundary** corrections. OAuth, fraud headers, durable storage and actual adapters belong to the following Objective 4 implementation, not this review.

## Secondary recommendations, narrowed

**Dispatch context: retain, simplify.** A reference to tenant and HMRC principal is essential for token selection; the actor/approval reference is essential for writes and traceability. The captured client facts may be a sealed reference rather than a large object passed through every layer. Do not pass a user-selected environment, arbitrary host, token, raw credential or duplicated tax subject. The transport resolves those from trusted configuration and scoped stores.

**Fraud evidence: retain, clarify ownership.** The initiating web flow can collect browser/device values, and its trusted ingress must capture public network facts and time. Objective 4's fraud component validates/seals those facts, combines them with trusted proxy/WAF topology and server/vendor facts, formats HMRC headers and persists protected compliance evidence. Operations/deployment owns the actual proxy trust list and public egress configuration. Objective 5 orchestrates user consent/filing and ensures the facts reference is available; it does not build HMRC headers. HMRC's [web application via server guide](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/connection-method/web-app-via-server/) specifies browser/device, client IP/port/time and vendor forwarding hops; the final connection method remains a deployment decision.

**Durability: retain at the point of risk, simplify for reads.** For a REST enquiry, bounded request/response metadata, safe correlation and error evidence suffice; a persistent resumable state machine is unnecessary. Before a VAT `POST` or MTD Income Tax write, persist one logical operation/subject/prepared digest, principal, approval and an attempt marked before send; then persist actual status, response/receipt or `OutcomeUnknown`. Only safe, explicitly classified retries may re-use that logical attempt. VAT `POST` ambiguity must stop for reconciliation; MTD `PUT` can retry the same bytes under reviewed policy. For Transaction Engine persist class, transaction ID, service-root and final-wire digests, submit state, returned correlation ID/endpoint/interval, each poll result, terminal business response/receipt **before** delete, and delete result. `DATA_REQUEST` can reconcile a lost acknowledgement. One small append-only attempt/conversation store with unique logical ID and protected payload/response references is sufficient; no workflow engine or universal retry machine.

**Components: simplify.** Retain two concrete protocol handlers (REST and Transaction Engine), one token/authorisation store, one fraud fact/header component, one durable attempt store and a small configuration/secret boundary. Combine the earlier `HmrcApiPlatformClient` and REST gateway unless multiple independent callers emerge; keep error parsing beside that handler. Keep OAuth callback/refresh logic together initially, with atomic token storage behind an interface. Keep GovTalk build/parse, IRmark and CT receipt interpretation within one Transaction Engine module, with isolated IRmark calculation tests. Use `TimeProvider` and normal structured logging rather than separate `TransportClock`, `TransportRedactor`, `TransportOutcomeMapper`, retry-policy framework or telemetry service classes without a demonstrated need. Retain redaction, bounded responses and safe retry rules as behaviours. Defer Companies House transport, cross-authority abstractions and production CT enablement.

## Revised sequence and decisions

**4.0 boundary correction:** pin supported VAT statuses; propagate REST metadata and eligibility; add dispatch context and typed REST outcome; correct CT conversation marker and CT-specific outcome shape; hard-reject preview/error packages. Verify existing JSON/iXBRL goldens unchanged. The CT service-root serializer is explicitly *not* part of this small phase.

**4.1 REST foundation:** configure allowed hosts, principal-scoped OAuth tokens, fraud capture/header validation and the minimal audit/attempt store. **4.2 enquiries:** send only approved bodyless VAT/MTD requests and interpret pinned outcomes. **4.3 writes:** add exact-byte VAT `POST` and MTD `PUT`, receipts and ambiguity handling. **Parallel CT contract assurance:** obtain official taxonomy/RIM/validation evidence, produce the real service root and establish the IRmark slot; only then promote readiness. **CT transport:** implement GovTalk/IRmark and Transaction Engine conversation with synthetic fixtures first, then official test/recognition and enabled submissions after both readiness gates pass. Production hardening and Objective 5 workflow follow. Thus REST implementation has a small, necessary boundary-correction predecessor; CT artifact assurance is a separate controlled Objective 3 change.

Human approval is needed for three concrete choices before the corresponding implementation/enabling step: (1) the tenant/principal and approval-reference model, including agent authority; (2) the deployment connection method, proxy topology and protected fraud/audit retention policy; (3) promotion of a specifically validated CT service-root/IRmark-slot design and later CT test/live enablement after official artefacts and recognition evidence. The first REST boundary correction itself is described here for review, not silently authorised by this reference document.
