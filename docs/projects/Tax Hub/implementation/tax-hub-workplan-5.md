# Tax Hub Work Plan 5 — Objective 4: HMRC Transport Platform

24 September 2026
**Status:** approved for productiion.

Work Plan 5 follows the completed Objective 3 Work Plan 4. Work-plan numbers record project chronology and do not match programme objective numbers.

## Objective and authority

Objective 4 provides the external statutory submission pipeline for the initial limited-company product. Delivery order is:

1. Common transport foundations.
2. VAT transport and HMRC recognition readiness.
3. Corporation Tax transport and HMRC recognition readiness.
4. Company Accounts filing through Companies House and production approval.
5. MTD Income Tax/Self Assessment, if authorised at the programme exit gate.

Every outbound request must be attributable to a tenant, an authorised principal and a prepared contract. Writes also require a durable attempt. Authority outcomes return across the Application boundary as typed, auditable results.

Objective 4 transports the statutory representations established by Objective 3. It does not recalculate accounting, populate CT600 fields, rebuild JSON or reconstruct company accounts.

### Governing documents and scope correction

The [programme specification](../specs/tax-hub-spec-programme.md) governs objective ownership. Its Objective 4 heading and text describe **HMRC** transport but do not explicitly include Companies House. Its Objective 3 completion record does include Companies House packages. Before the Companies House phases are implemented, a separately authorised specification change must assign Companies House accounts transport to Objective 4 and align the completion criteria. This work plan does not change the specification.

[Implementation Guide 3](tax-hub-implementation-3.md) and the completed [Work Plan 4](tax-hub-workplan-4.md) define the preparation boundary. The [HMRC transport reconnaissance and Boundary Resolution Review](../specs/reference/hmrc-transport.md) provide protocol evidence; the later review takes precedence where recommendations differ. [Repository and assembly architecture](tax-hub-repo-structure.md) describes the dependency direction confirmed by source and architecture tests. Earlier work plans are historical records. They do not authorise SA100, the legacy harness runner or superseded company assumptions.

Each phase can be handed over separately and ends at a human review gate. An implementation instruction can therefore name one phase, require its tests and stop. Approval of this plan is not approval to implement any phase or enable production.

## Current baseline and boundary

### REST preparation

`src/tax-hub/src/TradeControl.Tax.UK.Application/Preparation` produces immutable `PreparedApiRequest` values. They contain the resolved path, ordered query, contract headers, optional exact `BodyBytes` and SHA-256. They do not yet carry the required OAuth scope, expected success status or response contract because `HmrcPreparedApiContracts.From(...)` drops those facts.

MTD Income Tax's `HmrcEndpoint` contains all three facts. VAT's `VatOperationDescriptor` contains scope and response type but not success status. `VatOperationCatalog` and `SaOperationCatalog` own supported-operation policy; the submission adapter does not. The gateway currently takes the prepared request and returns `Task`.

### Company preparation

The company gateway takes `PreparedSubmissionPackage` and also returns `Task`. Two preview boundaries need correction:

- `CorporationTaxPreparer` places diagnostic `CorporationTaxPackageSerializer` XML in `Transmission`. It marks the transmission and both documents `Preview`, and uses `SubmissionPollingMode.None`. `CorporationTaxEndpointSet.Submit.RequiresStatusPolling` is false. The serializer states that these bytes are not a gateway payload. `CompanyContractRegistry.HmrcComputationTaxonomy2025.SubmissionReady` is false. The validator covers a supported local catalog and selected reconciliations, not all official submission rules.
- `CompaniesHouseAccountsPreparer` produces a separate `COMPANIES-HOUSE-ACCOUNTS` preview package, including an exact full or filleted iXBRL document and a `PollUntilTerminal` marker. `CompaniesHouseEnvelopeSerializer` creates an offline logical envelope. `CompanyContractRegistry.CompaniesHouseAccountsTis59.SubmissionReady` is false because the separately referenced official Filing TIS schemas are not provisioned.

Current company handoff tests prove byte preservation, not submission readiness. Neither preview is sendable.

### Submission adapter and regression anchors

`TradeControl.Tax.UK.Adapters.Submission` references Application but contains only placeholders: `Configuration/HmrcSettings.cs`, `Configuration/EnvironmentSelector.cs` and `Audit/SubmissionLogger.cs`. `WebHarness/Program.cs` still references the assembly for the legacy `Diagnostics/Runner/HmrcSubmissionRunner.cs`, which simulates a result. Modern preparation controllers use the bounded `PreparedApiRequestStore` for preview; it is not a durable submission audit.

Preserve the dependency direction **contracts → Application ← adapters**, with WebHarness as host. Modern gateways must not be wired into legacy diagnostic controllers. A preview ID must never be sufficient authority to submit.

Regression anchors are:

- `Application.Tests/Program.cs`: VAT, MIN and STD canonical digests and fake handoff.
- `DataProvision.Tests/PreparedArtifactTests.cs` and `CorporateHandoffTests.cs`: immutability and package handoff.
- The three contract test projects, `WebHarness.Tests` and `Architecture.Tests/Program.cs`.

Adding contract metadata requires narrowing the architecture test's text ban on “OAuth” in preparation and `PreparedArtifactTests`' public-name ban on “OAuth” and “Response”. The safeguards against credentials, tokens, received responses and adapter dependencies must remain.

The historical `.local/vat_mtd_client_test-master` run demonstrates only the broad OAuth → obligations → fraud validation → VAT POST → `201` sequence. Its token, payload and host design are not the target architecture.

## Invariants and delivery sequence

- REST sends the stored `BodyBytes` and preserves `BodySha256`; bodyless operations stay bodyless. The adapter has no request-body JSON serialisation path.
- Contract/version/scope/status/response expectation comes from the Objective 3 descriptor through the prepared request. No adapter-side duplicate operation table is introduced.
- A request is eligible only when the operation is supported, not preview, and has no blocking findings; package transmission additionally requires an approved service-artifact identity, `SubmissionReady` on the service artifact and **every** document, and the correct authority-specific approval. CT and Companies House diagnostic envelopes cannot be promoted merely by relabelling status.
- Credentials, bearer tokens, presenter credentials, raw fraud facts and environment host selection never enter prepared statutory artifacts. Dispatch context supplies references, not tax-body instructions.
- HMRC REST, HMRC Transaction Engine and Companies House XML gateway remain separate handlers. They may share trusted configuration, secret access, durable attempt storage and redacted diagnostics where the same rule applies.

The intended execution order is:

1. **5.0–5.6:** foundations, VAT transport and HMRC recognition readiness.
2. **5.7–5.10:** Corporation Tax artifact and transport work, then recognition readiness.
3. **5.11–5.13:** Companies House artifact, transport and external approval work.
4. **5.14:** limited-company completion and the human programme exit decision.
5. **5.15–5.16:** full end-to-end MTD Income Tax/Self Assessment, only if authorised.

CT artifact assurance (5.8) and fixture-only protocol work (5.9) can proceed independently once their foundations exist. They do not displace the VAT delivery milestone. Companies House remains after CT in the delivery sequence, even if a prerequisite could be investigated sooner, and requires the programme-specification correction above.

At 5.14, the human reviewer may release Objective 5 while leaving 5.15–5.16 planned and deferred. Every numbered implementation gate requires evidence review before its next dependent phase. The cross-phase decisions appear at the end of this plan.

## Phase 5.0 — REST boundary correction

### Purpose

Carry the existing Objective 3 transport facts across the prepared-request boundary so dispatch and response interpretation are safe.

### Implementation

1. In `Hmrc.Vat.Contracts/VatContractInfrastructure.cs`, pin the documented success status and existing response type for each of the three supported VAT operations. Verify each endpoint against the current HMRC reference; do not infer its status from generic `2xx` guidance or the historical sandbox.
2. In `Application/Preparation/PreparedApiRequestPipeline.cs`, `HmrcPreparedApiContracts.cs` and `PreparedArtifacts.cs`, carry the selected descriptor's scope, accepted success status, response DTO/body expectation and supported eligibility into immutable `PreparedApiContract` and `PreparedApiRequest` values. Existing catalogues may supply the supported flag; do not create another operation registry.
3. Define a small `AuthorityDispatchContext` with tenant, authorisation-principal, actor/approval references and a sealed client-facts reference for interactive calls. Define `PreparedApiOutcome` and change `IPreparedApiRequestGateway` to return it.
4. Enforce unsupported, preview and error-bearing rejection at the port. Keep environment selection outside the dispatch context.
5. Correct the supported VAT view-return `#001` period key in `BodylessRequestDescriber.cs` before enabling the enquiry. The existing path-escaping pipeline should encode it as `%23001`.

### Dependencies and exclusions

The completed Objective 3 baseline is the only dependency. This phase does not add an HTTP client, OAuth, fraud-header generation, an audit database or a CT serializer. Prepared artifacts must not contain credentials or received authority responses.

### Tests and acceptance

- Extend VAT and Income Tax contract tests and `DataProvision.Tests/PreparedArtifactTests.cs`. Update the `Application.Tests` fake gateway and `Architecture.Tests` guards.
- Assert exact scope, status and response metadata for every supported operation; a null body expectation for MTD `204`; and correct supported, deferred and preview classification.
- Test special VAT period-key escaping and rejection of unknown or incomplete metadata.
- Keep the three existing REST body digests and exact path, query and header checks unchanged. The fake gateway must return an outcome without changing input bytes.
- Keep reflection and source guards against secrets and received response state in prepared requests. Build `src/tax-hub/src/TaxHub.slnx` and run affected offline suites.

### Review gate

Record implementation evidence and any newly pinned HMRC endpoint URL/version in Work Plan 5 and forward-going `findings.md`/`change-log.md`. A human reviews this narrow, breaking port change before Phase 5.1.

### Phase 5.0 implementation evidence — 24 September 2026

**Implementation status:** accepted by the human reviewer on 24 September 2026.

- Rechecked the current HMRC [VAT (MTD) API version 1.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0), last updated 5 August 2026, and its [obligations and returns guidance](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/documentation/obligations.html). The supported operations remain `GET /organisations/vat/{vrn}/obligations` (`200`, `read:vat`, `VatObligationsResponse`), `POST /organisations/vat/{vrn}/returns` (`201`, `write:vat`, `VatReturnResponse`) and `GET /organisations/vat/{vrn}/returns/{periodKey}` (`200`, `read:vat`, `VatViewReturnResponse`). All use `application/vnd.hmrc.1.0+json`.
- `VatOperationDescriptor`, `PreparedApiContract` and `PreparedApiRequest` now preserve exact scope, success status, response-body expectation, response DTO type and catalogue eligibility. Missing or internally inconsistent transport metadata fails during preparation.
- `AuthorityDispatchContext` carries tenant, authorisation-principal, actor, approval and sealed client-facts references; it deliberately has no environment, credential, bearer-token or raw fraud-fact field. `PreparedApiOutcome` is the typed return boundary.
- `PreparedApiRequestGateway` enforces supported, non-preview and error-free eligibility before its handler is invoked. The fake handoff returns an outcome while preserving the same request instance and the existing VAT/MIN/STD bytes and SHA-256 values.
- VAT special period key `#001` is accepted and resolves to `/organisations/vat/{vrn}/returns/%23001`, matching HMRC's current guidance.
- The architecture/reflection guards were narrowed only for required scope and response-contract metadata. They continue to reject transport clients, submission-adapter dependencies, access/bearer tokens, client secrets, connection strings, fraud headers and received response state in prepared requests.
- Verification: `TaxHub.slnx` builds with 0 warnings and 0 errors. VAT contract tests passed (18 assertions), MTD Income Tax contract tests passed (91 assertions, 44 descriptors), Application tests passed (62 assertions), Data Provision prepared-artifact/corporate handoff tests passed in `--offline` mode, WebHarness tests passed (11 assertions), and Architecture tests passed (11 assertions).

## Phase 5.1 — Trusted configuration, secret and attempt foundations

### Purpose

Provide trusted configuration, secret access and durable attempt records for later authority handlers.

### Implementation

- Replace the `Adapters.Submission/Configuration` and `Audit` placeholders with validated, closed sandbox/production profiles, an HMRC host allow-list, a secret-provider boundary and a small durable attempt store.
- Keep only minimal outcome ports and record identifiers in Application. `Adapters.Submission` owns persistence and redacted operational logging. Select the concrete secret and storage facilities from the approved deployment, not in Application.
- For REST reads, persist bounded request/outcome metadata and correlation. A resumable workflow machine is unnecessary.
- Before each write, persist its logical operation, tenant, principal, subject/period reference, prepared digest, approval reference, environment and attempt state. Permit only one active send per logical submission.
- Protect raw payload and response references separately from routine logs. Reject request-supplied hosts, production selection and arbitrary authority-returned URLs.

### Dependencies and exclusions

Phase 5.0 must be accepted. CT and Companies House may later reuse only the attempt and secret primitives that fit their protocols. This phase does not implement OAuth, fraud formatting, REST sends or a generic workflow engine. Choose storage schema and file names when the actual host persistence setup is known. Expected code remains under `Adapters.Submission/Configuration` and `Audit`, with narrow Application ports only if needed.

### Tests and acceptance

Add `TradeControl.Tax.UK.Adapters.Submission.Tests` when executable behaviour first exists. Prove:

- Environment allow-listing and protected secret resolution.
- Tenant/principal isolation, atomic attempt reservation and retrieval after restart.
- Bounded response storage and redaction of tokens, identifiers, credentials and fraud values.
- Prepared content and user input cannot activate a production profile.

### Review gate

Record the selected storage/secret facility and retention decision in implementation history. Human review of the tenant/principal model and storage, access and retention policy is required before token or filing data is persisted there.

### Phase 5.1 implementation evidence — 24 September 2026

**Implementation status:** accepted by human review on 24 September 2026. The local JSON secret provider, file-backed attempt/content stores and retention periods are accepted as development/reference facilities only. Production secrets, persistence, access control and retention remain subject to separate review before production enablement.

- The sandbox secret facility is the existing git-ignored `.local/vat_mtd_client_test-master/mtd-client-vat/clientsettings.json`, accessed only through `JsonFileSecretProvider` and an explicit `clientId`/`clientSecret` property allow-list. The path is supplied by trusted host composition; no secret value or file-derived `uri` is copied into source, configuration profiles, prepared artifacts or logs. A value scan confirmed that neither sandbox credential occurs in tracked text files, and `git check-ignore` confirms the settings file remains ignored.
- `ProtectedSecret` prevents incidental string rendering and clears its in-memory character buffer on disposal. The production profile has no configured secret source and remains disabled pending a separately reviewed production deployment decision.
- HMRC sandbox and production API/authorisation hosts are closed HTTPS profiles. Only the sandbox selector is public. Prepared requests and request input have no environment selector, absolute/scheme-relative paths are rejected, and the production profile cannot be activated through the public adapter surface.
- Attempt metadata uses a host-supplied absolute-path JSON store with same-process and cross-process exclusion, write-through temporary files and atomic replacement. Each write records logical operation, tenant, principal, subject/period, prepared SHA-256, approval, environment and state before later network work. One active write is permitted per tenant/logical submission even if the principal changes; `Unknown` remains active until reconciliation. Retrieval requires the original tenant and principal.
- Attempt metadata is capped at 100,000 records by default. Terminal metadata retention is seven years; active/unknown attempts are not age-pruned. Raw payload/response bytes are held, when later required, in a separate host-access-controlled content root, capped by default at 1 MiB/256 KiB and retained for 30 days. Only opaque store-relative references enter attempt metadata; authority URLs are rejected. The deployment directory must be restricted to the service identity before this facility is hosted.
- Routine diagnostics contain operation/state classification plus hashed references. Tenant, principal, attempt and correlation identifiers are not written verbatim; token, secret, password, credential and fraud-header-shaped classifications are redacted. The retained legacy runner uses this redacted no-output compatibility path and is not connected to the modern gateway.
- Verification: `TaxHub.slnx` builds with 0 warnings and 0 errors. The new Submission adapter foundation suite passes 27 assertions covering profile closure, protected secret resolution, atomic reservation, restart recovery, tenant/principal isolation, unknown-outcome duplicate protection, bounded protected response storage, hostile URL rejection and redaction. Architecture tests pass 13 assertions and confirm the adapter still depends only on Application and contains no REST/OAuth/fraud-header implementation.

## Phase 5.2 — OAuth and token lifecycle

### Purpose

Authorise HMRC API Platform requests for the user-restricted VAT and MTD Income Tax scopes carried by prepared requests.

### Implementation

Build the authorisation component under `Adapters.Submission/OAuth`. It must cover:

- Authorisation state and PKCE; validated callbacks and token exchange.
- Encrypted, principal-scoped token storage and expiry skew.
- Atomic single-use refresh rotation, including locking against concurrent refresh.
- Grant and scope checks, revocation and a typed re-authorisation-required outcome.

Bind the initiating authenticated actor to the selected tenant/principal. Only the required scope belongs in the prepared request. Compose callbacks at an approved host entry point, not in a preparation controller. Follow HMRC's [user-restricted endpoint guidance](https://developer.service.hmrc.gov.uk/api-documentation/docs/authorisation/user-restricted-endpoints) at implementation time.

### Dependencies and exclusions

Requires 5.0 and 5.1. Production credentials come from the approved secret boundary. MTD Income Tax scopes remain represented by Objective 3 but are not enabled until 5.15. This phase does not construct a VAT body, call a REST resource, build an authentication-status UI page or reuse the legacy authentication-ticket token store.

### Tests and acceptance

With a fake clock and token endpoint, test state mismatch, expired grants, wrong scope, concurrent refresh, single-use rotation, revocation, re-authorisation and tenant separation. Logs and audit must contain no code, token or secret. Offline tests require no HMRC account.

### Review gate

Record token lifecycle and sandbox-authorisation evidence. Human review of principal and grant ownership precedes authenticated requests in Phase 5.4.

### Phase 5.2 implementation evidence — 24 September 2026

**Implementation status:** accepted by human review on 24 September 2026. Phase 5.3 may proceed; production OAuth activation and production persistence remain closed.

- OAuth is isolated under `Adapters.Submission/OAuth`. The public composition path remains sandbox-only and uses the closed HMRC hosts plus the registered local callback `https://localhost:44362/VatMTD`; callback location is not inferred from request headers or prepared content. Production OAuth activation remains unavailable.
- Authorization requests use 256-bit random state, a 512-bit PKCE verifier and `S256`. Pending authorization is single-use, expires within ten minutes, and is bound to the initiating tenant, authorization principal and authenticated actor. Callback mismatch, expiry and replay fail closed before code exchange.
- The token endpoint is fixed to sandbox `/oauth/token`, disables automatic redirects before sending credentials, sends the authorization code or refresh token only in a form body, bounds responses to 64 KiB, accepts bearer tokens only and emits only bounded safe error classifications. Client credentials continue to resolve through the Phase 5.1 secret boundary and are not placed in authorization URLs or persistence.
- Access and refresh tokens, state and PKCE verifiers are held in one AES-256-GCM encrypted file envelope with atomic replacement. The 256-bit encryption key and absolute store path are host-supplied; the reference implementation does not generate, persist or choose a production key. Grants are keyed by tenant, principal and required scope.
- Access uses a five-minute expiry skew. Authorization lifetime is capped at 18 calendar months. Refresh is protected by a tenant/principal/scope cross-thread and cross-process lease; refreshed single-use tokens must rotate and the encrypted document is replaced before waiters proceed. Invalid, non-rotating, expired, revoked or wrong-scope grants return a typed reauthorization-required result.
- Revocation retires and overwrites local token values. No undocumented HMRC network revocation endpoint was invented; users may separately revoke authority consent through HMRC's authorised-app management. MTD Income Tax scope names remain represented but are rejected by this component until Phase 5.15.
- Tax Hub application sign-in is distinct from HMRC authority consent. Signing out of the host application must end only its authenticated user session; it must not revoke the tenant/principal/scope-bound HMRC grant. After the same principal signs back into Tax Hub, the stored grant is reused. The HMRC authorization journey is repeated only when the grant is missing, expired, explicitly disconnected/revoked, has insufficient scope or cannot be refreshed. The development WebHarness shares the existing Trade Control ASP.NET Identity cookie through a Windows-DPAPI-protected development key ring; it does not treat HMRC OAuth as a login provider. `POST /diagnostics/hmrc/sign-out` ends only the shared Identity session, while the separately named `POST /diagnostics/hmrc/disconnect` retires that authenticated principal's HMRC grant without signing out of Trade Control.
- Offline fake-clock/fake-endpoint coverage exercises fixed redirect and PKCE, state mismatch/replay, actor binding, tenant isolation, exact scope, concurrent refresh, persisted single-use rotation, expiry, revocation, denied authorization, rejected refresh and encrypted-at-rest/redacted diagnostic representations. No HMRC account or live API request is used.
- Verification: Release `TaxHub.slnx` build succeeds with 0 warnings and 0 errors; the Submission adapter suite passes 45 assertions, architecture tests pass 14 assertions, all other offline/contract/application/adapter/web-harness suites pass, and the local sandbox credential-value scan reports zero matches outside excluded secret/build paths. The secret-backed sandbox database integration branch also passes (`DP5 context and CO1-CO4 source, artifact and reconciliation verification passed`); its connection string was resolved in-process from the existing TCWeb user secret and was not printed or persisted.

## Phase 5.3 — Fraud facts and HMRC headers

### Purpose

Capture trustworthy client and deployment facts, then produce HMRC fraud-prevention headers without changing prepared statutory artifacts.

### Implementation

Build a typed capture, seal and format boundary under `Adapters.Submission/FraudPrevention`:

- The initiating web request supplies browser and device values.
- Trusted ingress records the client's public network facts and timestamp.
- Configured proxy/WAF topology supplies vendor hops and server facts.

Objective 4 validates and seals these values, formats headers for the approved connection method, protects compliance evidence and rejects missing or untrusted fields under the current specification. `WebHarness/Program.cs` may expose a sandbox-only capture diagnostic. Modern preview controllers and the legacy runner remain non-submitting. Objective 5 later supplies the filing UI and makes fact references available; it does not construct HMRC headers.

Confirm deployment topology before applying HMRC's [connection-method guide](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/connection-method/) and [web application via server requirements](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/connection-method/web-app-via-server/).

### Dependencies and exclusions

Requires 5.1 and the dispatch context from 5.0; it can proceed independently of 5.2. Never invent browser/device facts for a background job, reuse historical hard-coded public IPs, or put raw fraud-header values in prepared artifacts or normal logs.

### Tests and acceptance

- Test client IP, port and time capture across every trusted proxy hop; reject spoofed forwarding data.
- Cover required and optional fields, encoding, freshness and redaction.
- Exercise HMRC's sandbox [fraud-header validator](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/txm-fph-validator-api/1.0) for the approved topology. Require zero errors and review warnings.

### Review gate

Record connection method, header-specification version, topology and validator result. A human confirms the actual proxy/network facts before REST calls are enabled.

### Phase 5.3 implementation evidence — 24 September 2026

**Implementation status:** local implementation complete and the Tax Hub flow has reached HMRC's live sandbox validator; deployment-topology confirmation, application-user identity and a zero-error validator result remain open. Phase 5.4 has not started.

- The selected application connection method is `WEB_APP_VIA_SERVER`, matching the browser-based Tax Hub and its server-side HMRC client. Formatting targets HMRC fraud-prevention specification 3.3 (issued 27 January 2025) and produces the 16 headers currently required for that connection method. The strict submission path fails closed on missing MFA or vendor-license evidence unless HMRC separately agrees an exceptional omission; placeholders are never generated. The sandbox-validator diagnostic preserves a genuinely empty collected value so HMRC can report it.
- `Adapters.Submission/FraudPrevention` separates browser facts, trusted ingress observations, configured deployment topology and vendor facts. Browser inputs include the JavaScript user agent, persistent device UUID, MFA evidence, screens, timezone, user identifiers and window size. Trusted ingress supplies the public client address, originating port and millisecond UTC capture time. Product, version, hashed licence identifiers and the ordered public TLS hop chain are host configuration.
- A direct topology rejects all forwarded client data. A proxy topology accepts a forwarded client endpoint only when the socket peer is in the exact trusted-immediate-peer allow-list; absence of forwarded evidence or an untrusted peer fails closed. The strict path rejects private, loopback, link-local, multicast, documentation-only and other non-public client addresses, plus invalid/client-server ports. Every configured public TLS terminator is emitted in client-to-server order in `Gov-Vendor-Forwarded`. A separately named sandbox-validator topology preserves actual non-public localhost socket addresses so HMRC, rather than local test data, reports the environment defect; that mode is not available through a submission gateway.
- Captured facts are bound to tenant, authorization principal and actor, plus a fingerprint of the reviewed topology. The returned 256-bit opaque reference is suitable for `AuthorityDispatchContext.SealedClientFactsReference`. Evidence is AES-256-GCM encrypted with associated ownership data, written atomically below a host-supplied absolute root and never exposed through routine `ToString()` output. It is usable for at most 15 minutes, permits one minute of clock skew and has a development/reference maximum retention of 30 days; production key custody, ACL and retention remain separately reviewable.
- Formatting percent-encodes structured keys/values and non-ASCII text as required, preserves separators, emits exact millisecond UTC timestamps, and rejects control-bearing or non-US-ASCII output. Strict formatting rejects empty required values. Header values never enter prepared statutory artifacts or routine logs.
- The current WebHarness is direct Kestrel development hosting with no forwarded-header middleware or declared proxy/WAF chain. The former Azure hosting arrangement has not been assumed. Swagger loads a same-origin script which obtains the browser-only user agent, persistent local-storage device ID, screens (including decimal device-pixel ratio), timezone and window size, validates them through `POST /diagnostics/hmrc/fraud-prevention/browser-session`, and retains them in a 15-minute HTTP-only development session. Bodyless, parameterless `GET /diagnostics/hmrc/fraud-prevention/validate` combines that capture with the authenticated Trade Control principal/MFA claims, timestamp, and the actual remote/local socket addresses and originating port. An unauthenticated request is sent to the existing Trade Control Identity login and returns to Swagger; it never starts HMRC OAuth. An authenticated principal without a usable HMRC grant receives a separately marked `401`, and only that response starts top-level HMRC authorization. `/VatMTD` returns to Swagger, browser capture refreshes automatically, and the principal-scoped encrypted access/refresh grant plus its development-only encryption key persist below the git-ignored `.local/tax-hub/web-harness` runtime root so authorization survives application sign-out and process restarts. The grant is reused until expiry, revocation or refresh rejection. The explicit authorization operation remains usable for diagnostics but is not a routine prerequisite. The routine formats through Phase 5.3, calls HMRC and returns the bounded validator response verbatim. All diagnostic routes remain visible in Swagger; callers cannot supply network, vendor, licence, OAuth or credential data.
- WebHarness now uses the authenticated Trade Control Identity subject as both its principal-bound grant key and fraud-header user identity. It does not infer Tax Hub MFA from HMRC sandbox login; MFA remains empty unless the host Identity ticket supplies the reviewed Tax Hub MFA claims. No product-licence hash is configured, so that value remains honestly empty and HMRC may warn. Local Kestrel also observes loopback rather than a public client/vendor address, which the diagnostic deliberately sends for HMRC to identify.
- Offline tests cover all 16 headers, fractional screen scaling, IPv6 forwarded-hop percent encoding, direct and two-hop proxy chains, exact trusted-peer enforcement, spoofed forwarding, private/documentation address rejection, server-port rejection, tenant/actor isolation, restart recovery, freshness, encrypted-at-rest evidence, redacted diagnostics, tamper detection and fail-closed missing required facts. Architecture tests keep ASP.NET/header parsing, VAT/SA serialization and submission paths outside this boundary.
- Verification: Release `TaxHub.slnx` builds with 0 warnings and 0 errors. Company, VAT and MTD Income Tax contract suites pass 61, 18 and 91 assertions; Application, Trade Control adapter, Submission adapter, WebHarness and Architecture suites pass 62, 10, 67, 31 and 16 assertions respectively; the Data Provision offline source/artifact/handoff branch passes.
- A live `GET /test/fraud-prevention-headers/validate` call was made through the ignored, historical `.local` test client after sandbox sign-in and app authorization. The validator confirmed specification 3.3 but returned `INVALID_HEADERS`: non-public `Gov-Client-Public-IP`, invalid `Gov-Client-Screens` scaling factor and non-public `Gov-Vendor-Forwarded`; it also warned about empty `Gov-Client-Multi-Factor`, incomplete percent encoding in `Gov-Vendor-Forwarded` and empty `Gov-Vendor-License-IDs`. This is diagnostic evidence about that several-years-old incomplete client only; its fraud-header code is not a Tax Hub implementation template and the result is not Phase 5.3 acceptance evidence.
- The Tax Hub WebHarness flow now completes top-level HMRC sandbox authorization from Swagger and calls the live validator. Specification 3.3 accepted the browser/device collection, fractional screen scaling and structured encoding. It reported four errors: non-public `Gov-Client-Public-IP`, empty `Gov-Client-User-IDs`, non-public `Gov-Vendor-Forwarded` and non-public `Gov-Vendor-Public-IP`; it reported warnings for empty `Gov-Client-Multi-Factor` and `Gov-Vendor-License-IDs`. The three network errors accurately reflect localhost. The HMRC sandbox login is authority consent, not the Tax Hub application user, so it is not reused as client-user or MFA evidence. Before Phase 5.4, exercise an approved public test topology using a genuinely authenticated Trade Control principal, require zero errors and review both missing-data warnings with HMRC; production TLS/proxy facts remain a separate enablement review.

## Phase 5.4 — VAT REST enquiries

### Purpose

Establish HMRC REST sending and response handling with bodyless VAT enquiries before adding a write operation.

### Implementation

Implement `HmrcPreparedApiRequestGateway` under `Adapters.Submission/Rest`. Start with the approved VAT obligations and view-return enquiries from `BodylessRequestDescriber.cs`. The described MTD Income Tax enquiry stays disabled until 5.15.

For each enquiry, use a configured base host, scoped bearer token and fresh fraud headers. Preserve the prepared method, escaped relative path, ordered query and Objective 3 `Accept` header. Send no body. Parse the pinned success DTO and a bounded raw response. Preserve HMRC error codes, details and unknown fields. Return `PreparedApiOutcome` with attempt ID, actual status, safe response reference and classified failure or unknown outcome. Only bounded, demonstrably safe enquiry retries are permitted. Fix the documented VAT special period key before view-return is enabled.

### Dependencies and exclusions

Requires 5.0–5.3. No VAT `POST`, MTD `PUT`, CT XML or deferred catalogue operation is sent here. A different `2xx` status does not count as success unless the prepared contract expects it. Do not add an adapter-side operation lookup table.

### Tests and acceptance

- A recording handler proves exact URI, query and headers; no body; one token scope; and one sealed fraud context.
- Classify `200`, errors, timeouts and `429`. Bound and classify malformed, oversized and empty responses.
- Architecture tests prove the REST adapter has no path to `VatJson.Serialize`, `SaJson.SerializeCanonical` or legacy builders. Existing body goldens stay unchanged.
- Run affected contract, Application, Submission adapter and architecture suites. Use new synthetic HMRC sandbox accounts for controlled enquiry evidence.

### Review gate

Record redacted sandbox status and response classifications. A human reviews the enquiry path before any write path is added.

## Phase 5.5 — VAT `POST` write and ambiguity

### Purpose

Add VAT return submission without risking a duplicate return after an uncertain network outcome.

### Implementation

Extend the REST gateway for only `vat.returns.submit` from `VatReturnPreparer.cs`:

1. Persist the logical submission and send-attempt marker before network I/O.
2. Send `PreparedApiRequest.BodyBytes` directly as byte content and retain its prepared digest.
3. Record the documented `201` response, the processing date and form-bundle/charge references available in `VatReturnResponse`, a bounded raw-response reference and approval evidence.
4. If a timeout or reset occurs after sending may have begun, mark `OutcomeUnknown`, block automatic replay and return a reconciliation-required outcome. Enquiries may help reconciliation, but acceptance requires HMRC evidence.

The [VAT API v1.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0) and [obligations/returns guidance](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/documentation/obligations.html) govern this slice.

### Dependencies and exclusions

Requires 5.4 and an approved declaration/actor reference. This phase does not build the Objective 5 filing UI, automatically resubmit VAT or recalculate any nine-box value. WebHarness `PreparedApiRequestStore` is not the durable attempt store.

### Tests and acceptance

- Compare transmitted bytes and SHA-256 with the same Objective 3 prepared request.
- Inject failures before connection, during upload and after upload but before a response.
- Prove ambiguous `POST`s are not silently resent, concurrent duplicate dispatch is refused, and a restart recovers unresolved attempts.
- Preserve `201` receipt fields and HMRC error codes. Keep the `Application.Tests` VAT golden digest.
- After explicit review, make one controlled happy-path submission with a synthetic sandbox user.

### Review gate

Record sandbox receipt and ambiguity evidence without sensitive values. Human review of the VAT declaration, approval and unknown-outcome handling precedes Phase 5.6.

## Phase 5.6 — VAT HMRC recognition and production-readiness milestone

### Purpose

Finish VAT as the first usable product transport and complete its HMRC recognition/production-readiness milestone.

### Implementation

Consolidate evidence from 5.0–5.5: OAuth, compliant fraud headers, enquiries and submission, durable attempts, `201` receipts, HMRC errors, ambiguous-write recovery, tenant/principal controls, secret rotation and redacted audit. Run current sandbox scenarios.

Follow the [VAT MTD production approvals and software choices process](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/). It covers the production application, terms, HMRC questionnaires, fraud-header and API testing, and production access. If Trade Control is a retail/commercial product, request compatible-software listing separately. HMRC requires an approved live submission before listing; obtain human authority for that controlled live test and retain its evidence. The [VAT API](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0) remains the endpoint contract.

### Dependencies and exclusions

Requires accepted 5.0–5.5, including a human-approved fraud topology and declaration/approval model. It does not depend on CT, Companies House or SA. A responding sandbox does not enable production. An application, granted production access and public listing are distinct states.

### Tests and acceptance

- Run all affected offline suites, controlled synthetic sandbox obligation/view/submit journeys and fault/restart exercises.
- Preserve canonical VAT bytes and digest. Verify scope, `200` and `201` contracts, HMRC error fidelity, protection of evidence and no duplicate ambiguous `POST`.
- Record HMRC's approval, controlled live-submission outcome and listing status separately.
- Production readiness requires approved product access, credentials, fraud compliance, operational support and a human live-enable decision.

### Review gate

Update Work Plan 5 phase status, forward-going `findings.md` for external evidence and decisions, and `change-log.md` for implemented changes. Human review signs off the VAT milestone before CT begins. It must distinguish technical readiness, production approval and public listing; pending external action is never reported as complete.

## Phase 5.7 — CT conversation marker and fail-closed package gate

### Purpose

Represent the CT Transaction Engine conversation accurately while keeping all diagnostic packages blocked from submission.

### Implementation

- In `Company.Contracts/Hmrc/CorporationTax/Submission/V2026/CorporationTaxPackage.cs`, correct `CorporationTaxEndpointSet.Submit.RequiresStatusPolling` and pin the protocol family.
- Add `SubmissionPollingMode.TransactionEngine` to `Application/Preparation/PreparedArtifacts.cs`. Use it in `CorporationTaxPreparer.cs` without inventing a relative status path. Do not change Companies House `PollUntilTerminal`.
- Define a CT-specific package outcome and port return. Distinguish pending correlation, terminal acceptance/rejection with receipt or error, and unknown/recovery-required state. Deletion state is separate from tax acceptance.
- Reject preview, error-bearing, unsupported and mixed-status packages at the package port or initial adapter entry, before outbound I/O.

The CT family stays disabled regardless of individual artifact labels until 5.8 establishes a validated service-artifact identity and explicit eligibility marker. Relabelling a diagnostic serializer output cannot enable it.

### Dependencies and exclusions

This follows the VAT milestone 5.6 and uses the dispatch-context/port convention from 5.0 and store from 5.1. Do not change `CorporationTaxPackageSerializer` bytes, generate IRmark, validate official taxonomies or send GovTalk. `PreparedPollingSemantics` remains a coarse marker; the later Transaction Engine handler owns returned endpoint, interval, polling and deletion.

### Tests and acceptance

- Change `CorporateHandoffTests.cs` and `Company.ContractTests/Program.cs` assertions that currently approve `None` and false.
- Test all present preview artifacts, a forged mixed-ready package and a diagnostic operation against a recording handler. Every case must produce zero sends.
- Preserve the original company document and diagnostic bytes and SHA-256. Run company, Data Provision and architecture suites.

### Review gate

Record corrected metadata and blocked diagnostic status in phase history. A human confirms that no company preview can reach an authority before CT work proceeds.

## Phase 5.8 — Corporation Tax Objective 3 service-artifact assurance

### Purpose

Establish the genuine CT600 service artifact required at the Objective 3 → Objective 4 boundary. This is a separately controlled Objective 3 prerequisite, not a transport implementation.

### Implementation

1. In `Company.Contracts` and `Application/Preparation/CorporationTaxPreparer.cs`, provision and pin the applicable official computation and accounts taxonomies, CT600 V3 (2026) RIM 1.994 XSD/Schematron, validation rules and samples.
2. Extend `CompanyContractValidator`, `CorporationTaxComputationProjection`, `IxbrlDocumentBuilder` or a new CT600 service-root serializer only where the evidence requires it.
3. Produce a schema-valid, complete `IRenvelope` service artifact with one identified IRmark slot. Keep full accounts and computation iXBRL documents separately traceable.
4. Establish exact attachment representation and digests, CT600/supplementary fields, periods and cross-document reconciliation. Put new service-root bytes into `PreparedSubmissionPackage.Transmission` only when their role and status are explicit.

The old `CorporationTaxPackageSerializer` stays diagnostic and preview-only. Official validation and reviewed evidence must justify promotion of each artifact and the entire package. HMRC's [RIM release](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-ct600-rim-artefacts), [accepted taxonomies](https://www.gov.uk/government/publications/taxonomies-accepted-by-hm-revenue-and-customs/taxonomies-accepted-by-hmrc) and [iXBRL specifications](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-xbrl-and-ixbrl) are authoritative.

### Dependencies and exclusions

Requires 5.7 and follows the VAT milestone. It is independent of the fixture-only protocol tests in 5.9. Only **new CT service bytes** may change, with a reviewed golden. Existing accounts and computation semantics change only for an evidenced Objective 3 correction. Excluded: GovTalk credentials, Transaction Engine HTTP, IRmark calculation, Companies House filing and general accounting refactoring.

### Tests and acceptance

- Validate with official XSD, Schematron and taxonomies; collect relevant LTS/TPVS evidence.
- Add a service-root golden and a negative fixture showing that status relabelling cannot promote old diagnostic XML.
- Prove both iXBRL source documents retain their bytes and digests and map into the service document as the pinned schema requires.
- Keep company preview golden and immutability tests fixed, or explicitly version them alongside the new artifact.

### Review gate

Attach asset versions/checksums, validation findings and the IRmark slot/attachment map to history. Human approval of this Objective 3 contract change is required before any real CT package becomes eligible for 5.10.

## Phase 5.9 — Transaction Engine protocol, fixture only

### Purpose

Prove the CT Transaction Engine protocol against approved fixtures while the real package gate remains closed.

### Implementation

Build the CT-specific handler under `Adapters.Submission/TransactionEngine`. It must:

1. Wrap an immutable service root in GovTalk and insert only the protocol-defined IRmark. Retain service and attachment digests; record the final wire digest.
2. Parse acknowledgement correlation ID, response endpoint and poll interval. Validate the returned endpoint against configured hosts.
3. Persist and resume the sequence: submit → acknowledgement → poll → terminal business response/error → receipt → delete.
4. Use `DATA_REQUEST` to reconcile uncertain correlation state. Persist terminal response and receipt before delete; deletion is not a recall.

HMRC's [Transaction Engine Document Submission Protocol v2.0](https://www.gov.uk/government/publications/transaction-engine-document-submission-protocol) and [Gateway IRmark guide](https://www.gov.uk/government/publications/hmrc-irmark-for-gateway-protocol-services) govern the mechanics.

### Dependencies and exclusions

Requires 5.7 and the configuration/attempt foundation in 5.1. Phase 5.8 is not required for fixture-only work. No real preview CT bytes may reach test or live HMRC. Protocol code must not populate CT600 fields, serialize accounting objects or reconstruct iXBRL. Confirm class, authentication level and receipt-validation rules against current CT service/recognition guidance before external testing.

### Tests and acceptance

- Match published IRmark examples exactly. Reject XML DTDs, external entities and excessive sizes. Only the reserved IRmark element may change within the service root.
- Inject failures at submission, lost acknowledgement, polling, terminal persistence and delete. Verify interval handling, restart recovery and no duplicate new submission.
- Distinguish gateway errors from departmental business rejections. Preserve receipts and errors.
- Run new adapter tests and the architecture suite offline.

### Review gate

Record fixture provenance and state-machine evidence. Human review confirms class, credential and receipt assumptions before HMRC test-service connection.

## Phase 5.10 — CT test-service recognition and production-readiness milestone

### Purpose

Combine the approved CT service artifact with the tested protocol handler, then complete the applicable HMRC test and recognition route.

### Implementation

Use synthetic CT credentials and the configured HMRC external test path. Exercise submission, correlation and polling, terminal response, signed receipt handling and deletion. Retain an auditable end-to-end attempt.

With current HMRC guidance or its Software Developers Support Team, confirm the service contract/version, class, endpoint, credentials, recognition scenarios, software recognition/listing route and attachment encoding. Complete applicable recognition/application and production-access evidence before Company Accounts delivery starts.

A real package is eligible only when Objective 3 submission readiness and Objective 4 protocol readiness are recorded, every constituent artifact is `SubmissionReady`, findings are clear and an authorised filing/approval reference exists.

### Dependencies and exclusions

Requires 5.8 and 5.9. A passing local fixture does not permit live filing. This phase does not implement Companies House transport or the Objective 5 UI. The preview gate must not be relaxed to pass a test journey.

### Tests and acceptance

- Pass official local/test validation and approved recognition scenarios.
- Recording tests show that only IRmark insertion changes the CT service root; underlying iXBRL bytes remain identical.
- Reject an unapproved response endpoint. Exercise lost-reply recovery, receipt verification, terminal-before-delete ordering and `DATA_REQUEST` reconciliation.
- Confirm the current diagnostic preview still causes zero outbound calls.

### Review gate

Keep redacted ETS/recognition evidence, HMRC application/recognition status and signed-receipt references in history. Human review distinguishes technical readiness from pending external approval. Live CT enablement needs separate human approval before the milestone is accepted.

## Phase 5.11 — Companies House official artifact and eligibility assurance

### Purpose

Replace the Companies House logical preview envelope with a validated filing artifact before transport becomes eligible.

### Implementation

1. Obtain and pin the current [Companies House general and accounts TIS](https://www.gov.uk/government/publications/technical-interface-specifications-for-companies-house-software), separately referenced Filing TIS/base/envelope schemas, [filing schemas and examples](https://xmlgw.companieshouse.gov.uk/SchemaStatus), validation rules and approved micro-entity full/filleted profile.
2. After schema and example validation, produce a separately identified, officially valid immutable filing artifact. Relevant code is `Company.Contracts/CompaniesHouse/Accounts/Tis5_9/CompaniesHouseEnvelopeSerializer.cs`, `CompaniesHouseContracts.cs`, `CompanyContractRegistry.cs`, `CompanyServiceCoverage.cs` and `Application/Preparation/CompaniesHouseAccountsPreparer.cs`.
3. Preserve full or filleted iXBRL bytes and digest, approved accounts and registrar declarations. Expose only a reviewed, narrow slot for protocol-required presenter/company authentication fields that cannot enter a prepared statutory artifact.
4. Correct `PollUntilTerminal` metadata if `submission-status/{envelopeNumber}` is not the XML gateway's actual status contract. Do not present an XML status request as a fabricated REST endpoint.

The package and every document remain fail-closed until official assets and approval evidence justify `SubmissionReady`.

### Dependencies and exclusions

This follows accepted CT milestone 5.10 and the separately authorised programme-specification correction assigning Companies House transport to Objective 4. Reuse `CompaniesHouseAccountsPreparer`, `PreparedSubmissionPackage`, `CompanyContractValidator` and the reviewed accounts source. Do not reuse CT service-root or Transaction Engine serializers.

Official Filing TIS schemas are an unresolved external prerequisite. If unavailable, or if they conflict with the current package structure, stop for a narrow Objective 3 contract correction and human approval. Do not relabel `CompaniesHouseEnvelopeSerializer` output or infer acceptance from the diagnostic fixture. Excluded: presenter secrets, network calls, accounting recalculation, CT changes and the future replacement API.

### Tests and acceptance

- Extend `Company.ContractTests/Program.cs`, `DataProvision.Tests/CorporateHandoffTests.cs` and architecture tests with official XSD/example validation and full/filleted service-artifact goldens.
- Prove exact iXBRL attachment bytes and digests, company number/period/declaration consistency and unique-envelope-number policy.
- Negative fixtures show that old preview XML and mixed-status packages produce zero sends through a recording gateway.
- Pin asset versions/checksums, the document/authentication-slot map and official operation identifiers.

### Review gate

Record schema acquisition, validation findings and any required Objective 3 correction in Work Plan 5 and forward-going `findings.md`/`change-log.md`. Human approval of the precise service artifact and readiness change precedes 5.12.

## Phase 5.12 — Companies House XML gateway submission and status transport

### Purpose

Transport the approved accounts artifact through the Companies House XML gateway and track it to a terminal authority outcome.

### Implementation

Implement a Companies House-specific handler under `Adapters.Submission/CompaniesHouse` and a typed package outcome at the Application port. Use configured test/live XML gateway hosts, protected presenter ID/code and company authentication code, and an authenticated actor/approval context. Persist a logical filing keyed by company, accounts period, document digest and unique envelope number.

The handler must:

1. Transmit the approved 5.11 artifact and constituent iXBRL without regeneration. If the pinned protocol requires authentication or test-flag insertion, change only the reviewed reserved fields. Retain original artifact/document digests and record the final wire digest.
2. Parse acknowledgement, submission number/correlation, pending and terminal acceptance/rejection, authority error codes and any receipt/document references.
3. Implement official XML `GetSubmissionStatus` and related status-ack/document exchanges where required. Poll within bounds and resume after restart. Initial acknowledgement is not acceptance.

`CompaniesHouseSubmissionAcknowledgement` and `CompaniesHouseEndpointSet` are starting types, not a complete response parser. [Companies House developer guidance](https://www.gov.uk/government/publications/technical-interface-specifications-for-companies-house-software/important-information-for-software-developers-read-first) requires unique envelope numbers and polling. Use the pinned TIS for exact messages and timing.

### Dependencies and exclusions

Requires 5.11 and the trusted configuration, secret and attempt foundations in 5.1. This is not HMRC OAuth, HMRC REST or the CT Transaction Engine state machine. Share storage, host validation and redaction only when their semantics match.

Never send preview or incomplete packages, invent a status URL, log authentication values or automatically create a new envelope after an ambiguous send. Verify presenter-authentication encoding and status-ack protocol against the official TIS before finalising code.

### Tests and acceptance

- Recording-handler and fault/restart tests prove exact approved document bytes, reserved-field-only wire differences, redacted credentials, allowed hosts and unique envelope numbers.
- Cover acknowledgement versus terminal result, pending polling, rejection/error fidelity, status acknowledgements, hostile or malformed XML, oversized replies and restart after lost acknowledgement.
- An uncertain send keeps its original filing identity and requires status/reconciliation rather than duplicate filing. A current `CompaniesHouseAccountsPreparer` preview produces zero outbound calls.

### Review gate

Record offline protocol fixture provenance and transition evidence. Human review confirms presenter/company-secret handling and recovery policy before external test submission.

## Phase 5.13 — Companies House external testing and production-readiness milestone

### Purpose

Complete Companies House testing and establish production readiness for the approved micro-entity accounts scope.

### Implementation

Follow [Companies House's developer guidance](https://www.gov.uk/government/publications/technical-interface-specifications-for-companies-house-software/important-information-for-software-developers-read-first): obtain a test account and presenter credentials, set the test flag, use unique envelope numbers, notify its XML team of test submissions, obtain manual review and poll for final status.

Exercise full and filleted synthetic accounts, valid and rejected cases, acknowledgement/pending/terminal transitions and any applicable document/reference retrieval. Establish presenter-account and production credentials, plus any approval or listing requirements, with Companies House. Its [software-filing guidance](https://www.gov.uk/guidance/using-software-to-file-your-companys-information) distinguishes presenter ID/code from the company's authentication code. Record actual external requirements; sandbox success does not imply approval or listing.

### Dependencies and exclusions

Requires 5.11–5.12, test credentials and official Filing TIS assets. Real company accounts require separate approval. Specialist ZIP/package accounts and a future REST replacement require a new reviewed contract. A pending test case is not an accepted filing.

### Tests and acceptance

- Run company contract, Application, Data Provision, Submission adapter and architecture suites.
- Conduct controlled test submissions. Retain redacted request hashes, envelope/submission numbers, parsed status and Companies House review outcome.
- Verify preview remains blocked; accepted/rejected terminal state and references survive restart; production requires approved presenter/company credentials and host configuration.

### Review gate

Retain redacted testing, presenter/approval and production-access evidence in Work Plan 5 and forward-going project records. Human sign-off distinguishes technical readiness from pending Companies House action before the limited-company milestone is accepted.

## Phase 5.14 — Limited-company completion, hardening and programme exit gate

### Purpose

Accept the transport layer for the initial limited-company product, then decide whether to continue Objective 4 with SA or start Objective 5.

### Implementation

Verify the three initial-product families together:

- VAT is operational and HMRC recognition/production-ready under 5.6.
- CT is operational and HMRC recognition/production-ready under 5.10.
- Company Accounts submission is operational and Companies House production-ready/approved as applicable under 5.13.

Harden rate limits and throttling, token/presenter credential rotation, authority maintenance, process restart, network uncertainty, protected audit retention/access, redacted telemetry, configuration validation and runbooks. Expose safe typed outcomes and status references through Application. Filing UI, history pages and reconciliation screens belong to Objective 5.

Review `WebHarness/Program.cs`: preview diagnostics and legacy `HmrcSubmissionRunner` must not select live credentials or appear to be modern gateways. Preserve assembly dependency direction and three distinct authority handlers.

### Dependencies and exclusions

Requires accepted VAT, CT and Companies House milestones, resolved official assets and external readiness evidence. Pending recognition/approval, credentials or live-enable decision blocks this milestone; record the specific outstanding item. MTD Income Tax is not a dependency. No unrelated refactoring, broad schema migration or legacy SA100 work.

### Tests and acceptance

- Build full `TaxHub.slnx` and run all affected offline suites.
- Fault/restart coverage includes ambiguous VAT `POST`, CT submit/poll/delete, Companies House acknowledgement/status/reconciliation, OAuth and presenter-secret rotation, fraud topology and tenant isolation.
- Every outbound host is configured, every write has approval and a durable attempt, and every outcome has a safe history reference. Preview, unsupported and error-bearing packages produce zero sends.
- Protect existing VAT, CT and Companies House source-document goldens.

### Review and programme exit gate

Record milestone status and operational/test evidence in Work Plan 5, durable discoveries and external decisions in `findings.md`, and significant implementation changes in `change-log.md`.

A human makes one of two explicit decisions:

1. **Continue Objective 4:** authorise 5.15–5.16.
2. **Proceed to Objective 5:** defer SA and start ASP.NET Core Tax Hub integration. Record the limited-company milestone as achieved, but full Objective 4 as partially complete/deferred.

## Phase 5.15 — Full end-to-end MTD Income Tax/Self Assessment contracts and transport, if continued

### Purpose and product boundary

After the 5.14 **Continue Objective 4** decision, deliver a full end-to-end Trade Control MTD Income Tax/Self Assessment product for an agreed supported profile. The current quarterly work is its first prepared slice, not its completion boundary.

Before promoting another operation, reconcile the complete customer and agent journey against HMRC's [minimum-functionality and production-access guidance](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/documentation/how-to-integrate.html), [end-to-end service guide](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/) and current endpoint documentation. Define the supported customers, agents and income sources. Map each of these needs to an implemented operation or an explicit unsupported customer scenario:

- Business-ID discovery, digital-record ownership and export.
- Obligations and quarterly updates for every supported mandated business income source.
- Estimated liability display or signposting.
- Annual information, required adjustments and finalisation of business income.
- Permitted losses and claims, non-mandated income handling and HMRC calculation.
- Tax-return/final-declaration, amendment and confirmation journeys.

Trade Control itself must deliver the tax-return/final-declaration path for its supported profile. A permitted handoff for an unsupported non-mandated income source does not replace that core capability. Do not claim support for property, multiple businesses or other income types that Trade Control cannot prepare and file. Human review settles the precise product boundary and any HMRC-permitted diversion.

### Contract and journey reconciliation

Generate an endpoint, version and coverage matrix from `SaOperationCatalog`, `SaOperationCoverage` and the 44 production plus one preview descriptors in `TradeControl.Tax.UK.Hmrc.MtdIncomeTax.Contracts/MtdIncomeTax/v1_0`. For each required journey, inventory the Objective 3 DTO/schema/serializer, authoritative source, typed preparer, fixture and gateway eligibility. Pin the then-current HMRC production version for every selected endpoint.

Two building blocks are already prepared:

- The income-and-expenditure obligations enquiry in `BodylessRequestDescriber.cs`.
- The Self Employment Business v5 cumulative MIN/STD `PUT` in `CumulativePeriodSummaryPreparer.cs`.

Preserve their exact bytes and digests, `204` empty success and obligation-backed calendar-quarter gate. They do **not** define the complete SA product.

`AnnualContracts.cs`, `BusinessDetailsContracts.cs`, `BusinessAdjustmentContracts.cs`, `LossContracts.cs`, `CalculationContracts.cs` and `FinalisationContracts.cs` are candidate descriptors, not ready population paths. Keep `AnnualEndpoints.Put2026Preview` fail-closed until an official production contract supersedes it.

### Implementation slices

Implement small slices in this reviewed sequence:

1. Business/customer identity, business details, obligations and supported quarterly update/readback.
2. Annual self-employment information and reviewed accounting/tax adjustments, BSAS, losses and claims where applicable.
3. Calculation, intent to finalise and tax-return/final-declaration, followed by applicable amendment, confirmation and receipt journeys.
4. Safeguards for unsupported income or source combinations, agent roles and permitted diversion.

For each selected deferred operation, Objective 3 first supplies missing exact contracts, source mappings, reviewed filing input, typed preparation, validation and golden bytes. Objective 4 then supplies the operation's REST transport, response/error/attempt handling and recovery rules. Record deliberate API exclusions from the reviewed journeys. An API's presence in the catalogue is not authority to send it.

### Dependencies and exclusions

Requires the 5.14 continuation decision and accepted VAT REST, OAuth, fraud, secret and attempt infrastructure. If 5.14 selects **Proceed to Objective 5**, the entire SA programme remains planned/deferred. It cannot delay the limited-company milestone or Objective 5 integration.

Objective 3 owns statutory meanings, payloads and canonical bytes. Objective 4 sends them with scoped OAuth, truthful fraud headers, durable attempts, bounded authority outcomes and operation-specific retry/ambiguity handling. Do not add an adapter-side endpoint catalogue, reserialize a request body, invent accounting facts, claim unsupported income sources or revive SA100/XML. Objective 5 supplies the hosted filing and approval journey but cannot fill Objective 3 or 4 gaps.

### Tests and acceptance

Each slice requires a reviewed coverage-matrix change, contract/preparation/source fixtures, positive and negative goldens with exact SHA-256, fake gateway identity checks, recording-handler wire/status/error tests, concurrency and restart/fault tests for writes, and affected offline/architecture suites before promotion.

Reuse the existing `Application.Tests` MIN and STD goldens. The obligations enquiry remains bodyless; `204` success remains empty; scope mismatches fail closed; safe retries of the same cumulative `PUT` attempt retain identical bytes. Synthetic end-to-end sandbox journeys must cover the agreed quarterly, annual/year-end, calculation and tax-return/final-declaration path. Test reviewed alternative-product handoffs or explicit unsupported scenarios where applicable.

### External production-access restriction

HMRC currently states:

> “HMRC is no longer accepting production credential access requests for new 2026–27 quarterly update products, as the market window for these products has now closed.”

This restricts external production access. It does not prohibit Objective 3 contract work, Objective 4 transport, sandbox testing, supported end-to-end sandbox journeys or preparation for a later applicable process. It does **not** establish that Trade Control can obtain recognition now with credentials merely delayed. It provides no reopening date. Recheck [HMRC's current service guide](https://developer.service.hmrc.gov.uk/guides/income-tax-mtd-end-to-end-service-guide/) at the production gate.

### Review gates

Human review first approves the product, journey and endpoint matrix. It then approves each small Objective 3 correction and Objective 4 slice. Record evidence and exclusions in Work Plan 5, durable discoveries in `findings.md` and significant changes in `change-log.md`. Technical completion of these slices does not imply HMRC recognition or production access.

## Phase 5.16 — Full Objective 4 completion and Objective 5 handoff, if continued

### Purpose

Integrate the agreed full end-to-end SA scope from 5.15 with the hardened limited-company transport, then establish a truthful full Objective 4 completion status.

### Implementation

Finalise SA token revocation, fraud facts, rate limits, restart and unknown-outcome handling, and typed status references across quarterly, annual/year-end, calculation and tax-return/final-declaration journeys. Compare Objective 3 contracts, Objective 4 operations, supported customer/income boundaries, sandbox evidence and the Objective 5 handoff with HMRC's then-current minimum-functionality and recognition requirements.

Treat production recognition/application and credential access as external milestones. Neither follows automatically from local or sandbox tests.

### Dependencies and exclusions

Requires accepted 5.15 technical scope and the external-access decision. If SA is deferred, do not execute or complete this phase; Objective 5 may already proceed under 5.14. Recheck the current HMRC recognition and credential process before live enablement and confirm unclear points with the Software Developers Support Team.

Do not assume a reopening date, build Objective 5 UI here, expand operations beyond the reviewed end-to-end matrix, or reopen VAT, CT or Companies House statutory semantics.

### Tests and acceptance

- Run the full solution and contract, Application, Data Provision, WebHarness, Architecture and Submission adapter suites.
- Cover quarterly-to-return sandbox journeys, errors, fault/restart behaviour and tenant isolation.
- Verify all approved Objective 3 canonical bytes and SHA-256 values, absence of an adapter body serializer, full authority status/error preservation, unsupported-scenario diversion and family-specific production gates.
- Record **technical/sandbox readiness**, **HMRC recognition/application status** and **production credential/access status** separately.

### Review gate

Update Work Plan 5 and forward-going findings/change-log with evidence. Human review may mark **full Objective 4 complete** only when the agreed end-to-end product and applicable external recognition/production-access gate are met. If HMRC access remains closed or undecided, record technical readiness and leave full completion open. Live activation needs separate authorisation.

## Cross-phase verification and decisions

### Verification across phases

Run the smallest affected offline suites after each phase and each reviewed 5.15 slice. Build the full solution and run contract, Application, Data Provision, WebHarness, Architecture and Submission adapter suites at integration gates 5.5, 5.6, 5.10, 5.13, 5.14 and, if authorised, 5.16.

Extend rather than duplicate existing VAT/MIN/STD REST digests in `Application.Tests/Program.cs` and company handoff tests. New annual, adjustment, calculation and finalisation operations need their own Objective 3 goldens before transport. REST tests compare sent bytes with the *same* prepared instance and prove the adapter has no request-body serializer path. Company tests keep diagnostic previews distinct from approved CT and Companies House service-artifact goldens while preserving original iXBRL bytes.

External test journeys use synthetic identities and protected configuration. They provide integration evidence, not ordinary offline-test dependencies.

### Human decisions

The programme requires decisions at these points:

1. Approve tenant/principal/agent and actor/filing-approval references before storing credentials or sending writes.
2. Confirm the connection method, trusted proxy/WAF path and audit privacy/retention before fraud-protected VAT calls.
3. Approve VAT recognition and production readiness before CT delivery.
4. Approve the CT service-root/IRmark slot and recognition evidence before Companies House delivery.
5. Authorise the programme-specification correction for Companies House scope and approve the Filing TIS artifact/authentication boundary before Companies House transport.
6. Confirm Companies House test, presenter and production evidence before the limited-company milestone.
7. At 5.14, choose **Continue Objective 4** or **Proceed to Objective 5**.
8. If SA continues, approve its full end-to-end customer/income journey and each Objective 3/4 slice. Confirm HMRC's then-current recognition and credential route separately before live use.

Each review can accept, revise or stop the next phase. Completed accounting and Objective 3 semantic decisions stay closed, except for the explicit narrow SA contract prerequisites.

### Limited-company milestone

VAT, CT and Company Accounts have accepted submission paths, exact approved artifacts, durable attributable authority outcomes, recovery and family-specific recognition/production-readiness evidence. Unsupported, preview and legacy paths remain closed.

After the human 5.14 decision, this milestone permits Objective 5 integration; it does not itself implement the UI.

### Full Objective 4 completion

Full completion adds the agreed end-to-end Trade Control MTD Income Tax/Self Assessment product to the limited-company milestone. It requires:

- Objective 3 contracts and preparation for the approved scope.
- Objective 4 exact-byte transport and durable outcomes for quarterly, annual/year-end, calculation and tax-return/final-declaration journeys.
- Supported end-to-end sandbox evidence, resilience and Objective 5 handoff evidence from 5.15–5.16.
- Applicable external HMRC recognition and production access.

Quarterly obligations and cumulative `PUT` alone do not pass this gate. If 5.14 defers SA, record Objective 4 as partially complete/deferred and preserve its plan. If HMRC production access remains closed, record technical/sandbox readiness separately; do not claim full completion.
