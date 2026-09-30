# Tax Hub Work Plan 6 — Objective 5: VAT Workflow Integration and HMRC Approval Readiness

28 September 2026

## Objective and authority

This work plan defines the first delivery slice of **Tax Hub Objective 5 — Workflow Integration**. It continues the accepted VAT work from Objective 4 and deliberately pauses Corporation Tax, Companies House and MTD Income Tax transport.

The product outcome is an authenticated TCWeb journey through which a VAT-registered business can:

1. connect Trade Control to HMRC;
2. retrieve its HMRC VAT obligations;
3. review the exact nine-box return derived from Trade Control;
4. understand validation and reconciliation findings;
5. make the required legal declaration and explicitly approve one immutable return;
6. submit that exact return once;
7. see the authority outcome, receipt and subsequent reconciliation; and
8. review an attributable filing history.

The resulting product should be technically ready for the current HMRC VAT (MTD) production-approval process. HMRC approval, production credentials, a controlled live submission and compatible-software listing remain external milestones and must never be inferred from local or sandbox success.

### Governing sources

The following sources govern this plan, in descending order where their decisions overlap:

1. `docs/projects/Tax Hub/specs/tax-hub-spec-programme.md`.
2. Current authoritative HMRC material:
   - [VAT (MTD) end-to-end service guide](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/), updated 23 June 2026;
   - [VAT setup journeys](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/documentation/set-up.html);
   - [VAT obligations and returns journeys](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/documentation/obligations.html);
   - [VAT (MTD) API version 1.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0);
   - current HMRC fraud-prevention-header specification and terms of use.
3. The completed Objective 2 and Objective 3 records, especially `tax-hub-implementation-3.md` and `tax-hub-workplan-4.md`.
4. `tax-hub-workplan-5.md`, including the implemented and accepted VAT phases 5.0–5.6.
5. `docs/projects/Tax Hub/specs/reference/tax-hub-objective-5-hosting-decision.md`, including its Phase 6.0 review qualifications.
6. The actual source beneath `src/TCWeb`, `src/tax-hub/src` and their tests.
7. Earlier Tax Hub work plans and implementation history, where they do not conflict with later reviewed decisions or current source.

HMRC currently requires a VAT product seeking production credentials to send compliant fraud-prevention headers and let the customer retrieve VAT obligations and submit a VAT return. HMRC also requires testing of every optional endpoint the product uses, asks developers to contact the Software Developers Support Team within two weeks of completing API testing, requires its questionnaires and terms, and treats production access and compatible-software listing as distinct gates. A retail/commercial product must make an approved live submission before listing.

### Status of adjacent objectives

- Objective 1 supplied the existing TCWeb Tax Hub reporting workspace.
- Objective 2 owns the statutory projection and the authoritative relationship between `Cash.vwTaxVatStatement` and `Cash.vwTaxVatSubmission`.
- Objective 3 owns the VAT contracts, canonical serialization and immutable prepared request.
- Objective 4 phases 5.0–5.6 own OAuth, fraud headers, exact-byte REST transport, attempts, receipts, ambiguity protection and HMRC reconciliation.
- Objective 5 owns the hosted user journey, permissions, approval interaction, status/history presentation and orchestration of those capabilities.

This plan does not silently complete or cancel Objective 4 phases 5.7–5.16. They remain planned and deferred. The programme has chosen VAT workflow continuity before starting the other authority transports.

## Current implementation baseline

### TCWeb

The production web application is `src/TCWeb/TCWeb.csproj`, targeting .NET 9. It already provides ASP.NET Core Identity, role checks, a Razor Page host and a Blazor/MudBlazor Tax Hub shell.

Relevant existing assets include:

- `Pages/Tax/Hub/TaxHubShell.razor` — shell-owned workspace and period state;
- `Pages/Tax/Hub/Components/TaxHubVatWorkspace.razor` — VAT Statement, VAT Submission and VAT Summary reporting tabs;
- `Pages/Tax/Hub/Components/TaxHubDashboard.razor` — local obligation, readiness and audit presentation;
- `Pages/Tax/Hub/Models/TaxHubResult.cs` and `TaxHubWorkflowModels.cs` — UI read models;
- `AppServices/TaxHub/ITaxHubService.cs` and `TaxHubService.cs` — direct EF/SQL reporting orchestration;
- `Data/NodeContext.cs` — the current application data-access boundary, including `Cash.vwTaxVatSubmission` as `Cash_TaxVatTotals`;
- ASP.NET Identity roles, including Administrators and Managers;
- `Controllers/TaxHubDiagnosticsController.cs` — a temporary signed handoff to the separately deployed diagnostic WebHarness.

The current VAT workspace is a reporting surface. Its period selector is based on local accounting periods; its dashboard obligation cards are calculated locally. Neither is an HMRC filing workflow. Objective 5 must keep useful local forecasts visually and semantically distinct from authority obligations.

TCWeb does not yet reference or compose the Tax Hub Application, Trade Control adapter or Submission adapter projects. It has no durable filing approval model, product VAT workflow service, HMRC connection-status model or filing-history UI.

### Tax Hub libraries

The `src/tax-hub` submodule now contains the technical VAT path:

- `VatReturnPreparer` reads the reviewed statutory context and `Cash.vwTaxVatSubmission`, produces the canonical nine-box request and blocks errors or an unfinalised declaration.
- `PreparedApiRequest` carries immutable bytes, SHA-256, source evidence, exact method/path/media type, OAuth scope and expected response contract.
- `BodylessRequestDescriber` supports VAT obligations and view-return enquiries.
- `HmrcPreparedApiRequestGateway` sends exact prepared bytes, preserves HMRC outcomes and prevents unsafe duplicate VAT writes.
- `HmrcOAuthService`, fraud-capture/header services, protected content and attempt stores implement the Objective 4 boundaries.
- `VatReturnReconciliation` compares HMRC's retrieved return with the exact prepared artifact without recalculating or normalising it.

The WebHarness successfully exercised OAuth, fraud-header validation, obligations, a `201` VAT submission, view-return and exact reconciliation in Azure. It remains a diagnostic host. Its controllers, short-lived `PreparedApiRequestStore`, Swagger interactions and host-authentication handoff are not the product workflow and must not become TCWeb's production API.

The current file-backed secret, OAuth, attempt, response-content and fraud-context stores were accepted for development/reference use. Their use in production was explicitly left for separate review. Objective 5 must not conceal that decision by merely pointing TCWeb at the existing files.

### Accepted VAT truth chain

The following chain is the central invariant of this work plan:

`Cash.vwTaxVatStatement`
→ `Cash.vwTaxVatSubmission`
→ `VatReturnPreparer`
→ immutable canonical nine-box bytes and SHA-256
→ explicit approval bound to those bytes and their source snapshot
→ exact bytes transmitted by `HmrcPreparedApiRequestGateway`
→ durable HMRC receipt/outcome
→ HMRC view-return response
→ `VatReturnReconciliation`

Objective 5 presents and orchestrates this chain. It must not introduce another VAT calculation, edit a prepared box, round a value again, rebuild JSON or treat a UI model as the authority payload.

## Product boundary and decisions

### Initial supported persona

The first supported approval profile is a VAT-registered business filing its own return through its own Government Gateway organisation account.

Trade Control does not currently provide the multi-client mandate, agent-services-account relationship and client-approval evidence required for a truthful agent filing product. Agent filing is therefore unsupported in this plan. The UI must not show the HMRC agent declaration or imply agent capability. Adding agents requires a separately reviewed product/authority model and HMRC journey.

### First approval scope

The first approval-ready scope includes:

- connect, connection status, reauthorise and disconnect HMRC;
- retrieve VAT obligations;
- select an open HMRC obligation;
- review the exact nine boxes and their Trade Control reconciliation;
- make the business declaration and approve the immutable return;
- submit once and present the complete outcome;
- retrieve the submitted return and reconcile it;
- show safe filing history and unresolved outcomes.

Customer information, liabilities, payments, penalties and penalty financial details remain optional API operations. Their Objective 3 contracts exist but are currently deferred and their Objective 4 transport is not implemented. They do not block HMRC's stated minimum VAT approval functionality and are excluded from the first approval application unless a later reviewed phase deliberately promotes and tests them.

HMRC Assist for VAT is scheduled by HMRC for April 2027. It is not a dependency of this work plan. Its draft-return feedback and acknowledgement journey requires a later contract, transport and UX review rather than speculative implementation now.

### Production host boundary

TCWeb shall host the product workflow in-process by referencing the required Tax Hub assemblies. It must not proxy product filing through Swagger or the WebHarness application.

The intended dependency direction is:

`TCWeb UI/controller endpoints`
→ `TCWeb VAT workflow orchestration`
→ `Tax.UK.Application` preparation/reconciliation ports
→ `Adapters.TradeControl` and `Adapters.Submission`
→ SQL Server and HMRC

TCWeb may supply host identity, tenant, current connection, browser/session facts and configuration. It must not gain access to OAuth tokens, client secrets or raw protected content. The Submission adapter must expose only the minimum public composition and status/history ports required by the product host. `TradeControl.Tax.UK.WebHarness` must not be referenced by TCWeb.

The current Azure deployment contains one Trade Control node, but the hosted-service destination is multi-tenant. Objective 5 is therefore **single-tenant in the current deployment and multi-tenant by design**. Each node receives a deliberately assigned opaque tenant GUID that survives deployment, restart, restore and migration. That identity scopes every grant, preparation, approval, attempt, protected-content reference and history record. It is never derived from transient deployment state or accepted from a browser. Multi-tenant provisioning and administration are not part of this objective.

Accounts Mode retains its deliberately simple security model. The initial use of Administrators and Managers for consequential VAT actions is product policy, not a permanent Tax Hub role model. The workflow depends on a narrow filing-authorisation decision which may later change without altering Objective 3, Objective 4 or the VAT workflow contract. Trade Control authentication, tenant membership, Accounts Mode policy, HMRC OAuth authority/scopes and attributable filing approval remain distinct. HMRC authorisation neither grants Trade Control membership nor elevates a local role.

## Cross-cutting invariants

Every phase must preserve these rules:

- **Exact financial meaning:** all nine boxes come only from the accepted Objective 2/3 path. No UI edit, correction or fallback calculation is permitted.
- **Immutable approval:** the user approves a digest, period key, VRN identity, source snapshot and declaration version. A changed source requires a new preparation and new approval.
- **Exact transmission:** the gateway receives the same immutable prepared artifact and exact bytes that were approved. In-memory object identity is not required across web requests, but byte/digest identity is. Objective 5 cannot deserialize and reserialize the body.
- **Obligation authority:** an HMRC open obligation supplies the filing period key. Local due-date calculations are forecasts, not filing authority.
- **One logical filing:** one tenant plus VRN plus period key identifies the VAT write. Principal changes do not permit a duplicate active submission.
- **Fail closed:** missing identity, stale facts, absent/insufficient OAuth scope, failed readiness, stale preparation, missing declaration, duplicate/unknown attempt or unsupported environment blocks submission.
- **Unknown is not failed:** a timeout or lost reply after sending may have begun is shown as reconciliation required and is never automatically replayed.
- **Identity separation:** ASP.NET Identity authenticates the Trade Control user. HMRC OAuth is a separately protected grant for that tenant and principal. Local sign-out does not revoke the grant; disconnect does.
- **Tenant and authorisation isolation:** identifiers, grants, preparations, approvals, attempts and content are scoped to the current node/tenant and authenticated actor. No caller supplies an arbitrary tenant reference. Consequential actions require the host's narrow filing-authorisation policy; OAuth authority is not a substitute for local tenant membership or policy.
- **Secret isolation:** client secrets, bearer/refresh tokens, encryption keys and raw fraud facts never enter UI models, prepared artifacts, URLs, logs or the Trade Control database unless an explicitly reviewed protected store requires it.
- **Protected-content integrity:** every prepared or authority-content object retains its recorded digest and SQL metadata relationship. Retrieval verifies the digest and fails closed on missing, orphaned, substituted or corrupt content.
- **Tenant-attributable operations:** production telemetry carries the opaque tenant identity so capacity and exceptional infrastructure consumption can be understood without recording payloads, tax identifiers, OAuth material, fraud facts or other protected content.
- **Truthful status:** local readiness, HMRC connection, preparation, approval, submission, reconciliation, production access and compatible-software listing are separate states.
- **Time handling:** persist instants in UTC and render them using the configured/user timezone. Authority dates retain their contract meaning.
- **Accessibility and security:** the legal declaration and final submission control are keyboard/screen-reader usable, antiforgery protected and resistant to double activation.

## Delivery sequence

| Phase | Deliverable | Principal gate |
|---|---|---|
| 6.0 | Product integration boundary and production persistence decision | Architecture and storage approval |
| 6.1 | TCWeb identity, HMRC connection and trusted browser facts | Authentication/permission review |
| 6.2 | HMRC-obligation-led VAT workspace | Authority/local-period reconciliation review |
| 6.3 | Exact return review, validation and legal approval | Declaration and immutable-approval review |
| 6.4 | Controlled submission and complete outcome handling | Sandbox write and ambiguity review |
| 6.5 | Filing history, readback and reconciliation | Audit/retention/access review |
| 6.6 | Approval-ready product hardening and evidence pack | Technical approval-readiness sign-off |
| 6.7 | HMRC production approval, controlled live filing and listing | External and human live-enable gates |

Phases are in execution order. A phase may be implemented only after its preceding review gate is accepted. Phase 6.7 includes external activities that the repository cannot complete by itself.

## Phase 6.0 — Product integration boundary and durable facilities

### Purpose

Establish a production-shaped boundary between TCWeb and the implemented VAT libraries before adding filing UI. Resolve persistence and secret ownership explicitly rather than carrying diagnostic-host assumptions into the product.

### Implementation

1. Add direct project references from `TCWeb.csproj` to the minimum Tax Hub assemblies required for Application preparation/reconciliation and the two adapters. Do not reference `TradeControl.Tax.UK.WebHarness`.
2. Add a VAT product workflow boundary under `TCWeb/AppServices/TaxHub/Vat` or an equally narrow reviewed namespace. Suggested public responsibilities are:
   - read connection state;
   - retrieve authority obligations;
   - prepare an immutable candidate return;
   - record/retrieve approval;
   - submit the approved preparation;
   - read filing status/history; and
   - reconcile a completed filing.
   Treat this as an API-shaped, transport-neutral use-case contract. TCWeb may call it in-process, while a future authenticated HTTP host may adapt the same operations and safe DTOs. Do not expose Razor/MudBlazor, `HttpContext`, MVC result, EF entity, secret/token, raw fraud-fact or caller-supplied VAT-box types through this boundary. WebHarness remains a replaceable test client/composition root over the same Tax Hub Application and adapter capabilities, never a product dependency.
3. Expose narrow public composition/port types from the Tax Hub submodule where the current `internal` diagnostic composition prevents product hosting. Do not make stores, tokens, ciphers or raw fraud-header construction generally public.
4. Resolve the current node/tenant reference, ASP.NET subject, internal user/actor and reporting subject server-side. Establish stable opaque references for `AuthorityDispatchContext`; do not use email addresses as durable keys. Assign the tenant GUID deliberately and persist it independently of deployment state so it remains stable through restart, restore and migration and can later coexist with other tenants without changing Tax Hub contracts.
5. Define environment options with a startup validation rule. Development/sandbox and production hosts, redirect URIs and store locations must be explicit. Production must fail closed if sandbox settings or diagnostic stores are selected.
6. Select and document production implementations for:
   - application-level client secrets and encryption/envelope keys in Azure Key Vault through managed identity;
   - per-tenant/per-principal OAuth grants and pending states in the protected Azure SQL grant store, encrypted with versioned key metadata rather than represented as individual Key Vault secrets;
   - fraud-context storage;
   - short-lived immutable prepared-request storage, including exact bytes and contract metadata;
   - filing approval records;
   - submission-attempt metadata;
   - bounded raw authority content; and
   - retention, purge, coordinated SQL/Blob backup and restore, integrity verification and access control.
7. Keep existing file implementations available for local development and reference tests only unless the human review explicitly approves a constrained hosting use.
8. Define tenant-level operational attribution as an observability requirement. Production request/workflow activity, measurable Tax Hub SQL and Blob consumption, HMRC/API activity, attempts and operational load must be attributable to the opaque tenant identity without capturing protected content. This is capacity/cost telemetry, not billing, licensing or per-user monitoring.

Production persistence may be implemented in a later phase after the interfaces are accepted, but Phase 6.0 must choose its intended facility and leave production composition disabled until it exists. Likely Azure choices include managed identity plus Key Vault for secrets and a durable database/blob design for records/content; the plan does not pre-approve a provider.

### Dependencies and exclusions

Requires accepted Objective 4 VAT phases 5.0–5.6. It does not build UI, change VAT calculations, migrate all Tax Hub reporting to the submodule, promote optional VAT endpoints or remove the WebHarness.

Do not merge TCWeb's `TaxHubService` reporting queries with HMRC transport internals into a single large service. The UI-facing workflow may coordinate both, but preparation and transport retain their current owners.

### Tests and acceptance

- Add architecture tests proving TCWeb does not reference WebHarness and no Tax Hub contract/adapter references TCWeb.
- Add a structural test proving the public VAT workflow contract remains host/transport neutral and accepts neither caller-supplied tenant identity nor VAT return body/box values.
- Add composition tests for sandbox versus production configuration, including rejection of sandbox endpoints, missing keys, relative store paths and development stores in production.
- Prove tenant, principal and actor references are server-derived and stable, and that cross-tenant/principal access fails.
- Prove the filing-authorisation decision is a replaceable host policy rather than a role assumption embedded in the workflow contract.
- Record as production acceptance requirements that stored content is digest-verified, missing/orphaned/mismatched SQL/Blob evidence fails closed, and a later restore exercise verifies references, versions, digests and approval/attempt consistency together.
- Define a tenant-attribution telemetry and redaction contract which carries the opaque tenant identity but excludes VAT bodies, tax identifiers, OAuth material and fraud facts. Concrete telemetry is implemented and verified with production facilities in later phases.
- Preserve every VAT canonical byte and digest test and all Objective 4 adapter tests.
- Build `tradecontrol.web.sln` and `TaxHub.slnx` without warnings.

### Review gate

The human reviewer approves the dependency direction, supported business-filer persona, initial filing-authorisation policy, durable tenant identity and selected production secret/persistence/retention/telemetry architecture. No filing UI or production host composition proceeds with these decisions unresolved.

### Implementation evidence and accepted gate — 28 September 2026

**Status: Phase 6.0 accepted.** TCWeb now references Application, Trade Control adapter and Submission adapter directly, with no WebHarness reference. The API-shaped `IVatProductWorkflow` boundary accepts only safe operation references and result models; it accepts no tenant identifier, authority body or VAT boxes from a caller. The authenticated ASP.NET subject, internal actor and reporting subject are resolved server-side, and authority dispatch construction rejects a principal from another tenant.

Host configuration is disabled by default. Startup validation confines file stores to Development with absolute paths and rejects production HMRC selection and Azure-managed composition until the approved facilities exist. The accepted production direction is Key Vault for application secrets/keys, versioned encrypted per-tenant grants and workflow metadata in Azure SQL, and digest-verified protected content in private Blob storage. The current deployment is single-tenant while the durable tenant identity, storage scoping and safe telemetry requirements are multi-tenant by design. Administrators and Managers remain only the initial host filing-policy proposal; connection presentation uses actual protected HMRC state.

The mixed .NET/SSDT `tradecontrol.web.sln` build passed through Visual Studio MSBuild, and `TaxHub.slnx` built with zero warnings. The new TCWeb boundary suite passed 20 assertions covering dependency direction, host-neutral workflow shape, fail-closed configuration, stable context construction and cross-tenant rejection. All nine established Tax Hub suites passed; the secret-backed data-provision executable used its supported offline source/preparation/handoff path because no database secret was supplied to that test process. No credential or protected value was read or committed.

The approved detail is recorded in `docs/projects/Tax Hub/specs/reference/tax-hub-objective-5-hosting-decision.md`. Phase 6.1 requires separate implementation authority and does not inherit production enablement from this approval.

## Phase 6.1 — Product authentication, HMRC connection and client facts

### Purpose

Integrate HMRC OAuth into the authenticated TCWeb experience so an authorised user connects once, returns safely to Tax Hub and can use the stored grant until expiry, revocation or deliberate disconnect.

### Implementation

1. Add product endpoints for connect, OAuth callback, reauthorise and disconnect. Use TCWeb's public HTTPS origin and registered redirect URI. Protect state, PKCE/verifier data, return targets and correlation against replay/open redirect.
2. Keep local sign-in and HMRC connection distinct:
   - an unauthenticated Trade Control request challenges ASP.NET Identity;
   - an authenticated user with a valid HMRC grant continues without reauthorising;
   - an expired access token refreshes through Objective 4;
   - revoked/expired consent produces a clear reconnect action;
   - local sign-out ends only the TCWeb session;
   - **Disconnect HMRC** revokes/removes the stored grant and requires the grant-authority journey next time.
3. Add a connection-status model to the VAT workspace: Not connected, Connected, Reauthorisation required, Disconnected or Unavailable. Derive presentation from the actual protected HMRC connection state for the authenticated tenant/user context, not from role membership. Do not expose tokens, expiry internals or raw HMRC errors.
4. Capture browser facts through a small TCWeb JavaScript module at the moment required by an interactive HMRC call. Combine them with trusted server/proxy facts only inside Objective 4, seal them to tenant/principal/actor and enforce freshness.
5. Apply a narrow filing-authorisation policy to consequential actions. The initial product policy recommends Administrators and Managers for connect/disconnect and submit, with read-only VAT reporting retained for other authenticated users. Do not embed those role names in the Tax Hub workflow contract or infer that another authenticated user cannot possess legitimate HMRC authority. Confirm the initial policy at the gate without creating a generic RBAC subsystem.
6. Present HMRC setup guidance and state that the user must sign in with the correct VAT (MTD) Government Gateway organisation account. Do not collect HMRC credentials in Trade Control.

The existing `TaxHubDiagnosticsController` handoff may remain while diagnostics are useful, but product navigation must no longer depend on it. Remove or clearly label it as development diagnostics only once equivalent product OAuth and fraud validation are proven.

### Dependencies and exclusions

Requires accepted 6.0. This phase does not retrieve obligations, prepare or submit a VAT return, infer that an OAuth grant proves the VRN, or add agent support.

### Tests and acceptance

- Cover first connect, callback replay, state mismatch, refresh, revoked grant, scope escalation, reconnect and explicit disconnect.
- Cover local sign-out/sign-in with the HMRC grant retained, and disconnect followed by a new grant-authority journey.
- Cover forged return targets, missing authenticated actor, policy denial and tenant/principal isolation.
- Verify fraud facts and tokens are absent from HTML, URLs, logs, exceptions and browser storage. A device identifier may remain in browser storage only under the reviewed fraud specification.
- Run the Objective 4 OAuth/fraud suites and new TCWeb host integration tests.

### Review gate

The reviewer exercises the complete TCWeb → HMRC → TCWeb sandbox journey, signs out/in locally, disconnects HMRC and confirms the expected distinctions. Approve the initial filing policy and browser/proxy topology before 6.2.

### Implementation evidence — 28 September 2026

**Status: implemented and accepted at the Phase 6.1 human review gate.** TCWeb now owns fixed connect, callback, reauthorise, status, disconnect and client-fact endpoints under `/TaxHub/Hmrc`; no product path depends on WebHarness. One state-bound PKCE journey requests the exact `read:vat write:vat` consent set and stores one encrypted refresh-token lineage. The existing per-scope Objective 4 behavior remains compatible. The callback has no return-target parameter, pending state remains tenant/principal/actor bound and single-use, and only the fixed configured callback is accepted.

The VAT workspace presents the protected connection state independently of ASP.NET Identity roles. Local sign-out has no grant-store side effect; explicit disconnect retires both VAT scopes. Connect, reauthorise, disconnect and browser-fact capture use the replaceable TCWeb filing policy, initially Administrators and Managers. Other authenticated users may see the actual safe state but cannot perform those consequential actions.

The browser contract contains only user-agent, persistent device identifier, screens, timezone and window size. TCWeb supplies tenant, ASP.NET subject, internal actor and socket facts. Forwarded client data is accepted only from an exact configured immediate-peer allow-list; strict Objective 4 topology validation rejects non-public client/server facts and encrypted evidence retains its 15-minute freshness boundary. Only the reviewed device identifier is stored in the browser. Setup and fixed redirect guidance is recorded in `phase-6.1-tcweb-hmrc-setup.md`; no credentials or local secret paths are committed.

`TCWeb.csproj` and `TaxHub.slnx` build with zero warnings. Submission adapter tests pass 95 assertions, WebHarness hardening passes 34, TCWeb host/policy tests pass 32, and the remaining Objective 4/application/contract/architecture suites pass. Data Provision passed its supported offline source/preparation/handoff path because `TC_NODE_CONTEXT` was intentionally absent. The reviewer completed the interactive TCWeb sandbox consent journey with the generated organisation user, confirmed the retained connection, and accepted the collapsed connected-state controls. An isolated no-data-mutation UI acceptance run confirmed that a missing indirect-tax reporting profile presents configuration guidance and no HMRC actions.

The same release was deployed successfully to `tcweb-payg-db96115e` and smoke-tested after App Service restart. Its Production composition remains deliberately fail-closed and reports that HMRC VAT is not configured, with no actionable controls, until the separately approved Azure SQL/Blob/Key Vault implementations and public-TLS/trusted-proxy topology are available. No development file store or localhost fraud topology was enabled in Azure.

## Phase 6.2 — HMRC-obligation-led VAT workspace

### Purpose

Replace arbitrary filing-period selection with an authority-led journey while preserving the existing accounting views for analysis.

### Implementation

1. Add an HMRC Obligations section to the VAT workspace. Query the supported obligations endpoint through the product workflow and show open and fulfilled periods, start/end, due date, received date and period key.
2. Default the filing journey to open obligations. The period key must come from the selected HMRC response and remain opaque, including special encoded keys.
3. Reconcile each selected obligation to an available `Cash.vwTaxVatSubmission` period and effective indirect-tax reporting profile/VRN. Show distinct states for:
   - authority obligation and exact local period available;
   - authority obligation but local period/readiness missing;
   - local VAT period with no matching authority obligation;
   - fulfilled obligation; and
   - connection/authority error.
4. Keep `Cash.fnTaxTypeDueDates` and dashboard dates labelled as local forecasts. Do not overwrite them with or present them as HMRC obligations.
5. Provide a clear route from an open matched obligation to **Review return**. Fulfilled obligations route to history/readback, not submission.
6. Preserve full HMRC error codes and correlation references behind safe user messages. Rate-limit refreshes and avoid repeated calls during component rendering.
7. Add a test-only synthetic sandbox-alignment facility for controlled end-to-end evidence. It must:
   - operate only on a disposable synthetic node/database;
   - start from any VAT-registered synthetic template rather than encode a company or sole-trader dependency;
   - replace the synthetic placeholder VAT identity with the VRN issued for the generated HMRC sandbox organisation through the normal statutory-identity configuration boundary;
   - align one synthetic VAT reporting period with the start, end and period key of the selected canned HMRC sandbox obligation;
   - preserve the ordinary `Cash.vwTaxVatStatement` → `Cash.vwTaxVatSubmission` calculation and source-evidence path; and
   - remain impossible to select in production composition.

The existing reporting tabs remain useful. Their accounting-year/month filters must not silently drive an HMRC submission period.

The alignment facility is test provisioning, not a filing override. Production and ordinary sandbox UI requests must still obtain the VRN from reviewed statutory identity and the period key from HMRC. They must not accept a browser-supplied VRN, arbitrary period key or alternate nine-box values merely to make a canned obligation match.

### Dependencies and exclusions

Requires 6.1 and a connected read-scoped grant. The alignment facility additionally requires a generated organisation test user and a disposable synthetic database; it must never alter a real tenant. No prepared write, declaration or submission occurs here. Optional liabilities/payments/penalties endpoints remain deferred.

### Tests and acceptance

- Test obligation ordering, `O`/`F` states, received/due dates, a 366-day search boundary, special period keys, empty results and HMRC errors.
- Test exact matching and each mismatch state against sanitised SQL and authority fixtures.
- Prove no arbitrary client-supplied VRN or period key can cross the application boundary.
- Recording-handler tests prove a bodyless `GET`, correct scope/Accept/path/query and no unsafe retry behaviour.
- UI tests cover loading, no-data, reconnect, retry and permission states.
- Exercise the alignment facility against at least one VAT-registered synthetic scenario and the generated HMRC organisation. Either company or sole trader is sufficient for the approval evidence because both use the unified VAT path.
- Keep a business-model-neutral regression matrix covering representative VAT-registered minimal and standard company/sole-trader scenarios, including profit/loss, adjustments and zero-value boxes. Representative scenario codes include `COMIPFVT1`, `COSIPFVT1`, `STMIPFVT1` and `STSIPFVT1`; equivalent reviewed variants may be substituted without changing the supported VAT product.
- Use the corresponding `VT0` non-VAT scenarios as negative controls: no HMRC VAT filing journey may be offered.
- Prove production startup and production workflow cannot invoke the synthetic alignment facility or use the placeholder VRN.

### Review gate

The reviewer confirms that authority obligations and local forecasts cannot be confused and that only a matching open HMRC obligation can enter return review. The reviewer also inspects one aligned synthetic sandbox case and confirms that its VRN/period provisioning did not alter or bypass the authoritative VAT calculation and preparation path.

### Implementation checkpoint (28 September 2026)

The product implementation and offline evidence are complete pending the interactive review gate. TCWeb now retrieves the reviewed statutory VRN server-side, obtains the opaque period key and bounded obligation dates from HMRC, and exactly reconciles those dates to `Cash.vwTaxVatSubmission`. The VAT workspace distinguishes matched open, unmatched open, fulfilled and local-only periods; only a matched open period exposes **Review return**. Authority dates are explicitly separated from local forecasts, refresh is bounded, and protected gateway evidence retains HMRC outcome/support references behind safe UI messages.

The product boundary accepts no browser VRN, period key or search dates and explicitly rejects the synthetic placeholder VRN. The existing recording gateway evidence covers bodyless `GET`, pinned scope/Accept/path/query, special period keys and bounded enquiry retry behaviour. TCWeb host tests cover the 366-day inclusive search, ordering and all reconciliation states.

The SQL synthetic generator now supports a nullable test temporal anchor, and the guarded `App.proc_DatasetSyntheticMIS_VatSandboxAlign` procedure accepts only a non-placeholder generated organisation VRN on an existing synthetic indirect-tax profile with an exact calculated submission period. It calls `Cash.proc_ReportingProfileSave`; it does not write a period key or any nine-box value. The procedure is not exposed by TCWeb and therefore is not selectable in Production composition. The controlled procedure is documented in `phase-6.2-vat-obligations-and-sandbox-alignment.md`.

The review gate was accepted on 30 September 2026 after the reviewer inspected the aligned Azure sandbox journey. HMRC obligation `18A2` matched the calculated local 1 April–30 June 2017 period and alone exposed **Review return**; fulfilled `18A1` remained readback-only and unmatched periods remained explicitly local forecasts. The exercise also corrected an unsupported local `obligationId` requirement, added Event Log/support-reference diagnostics and retained the original VAT workspace as the default view with HMRC obligations in a separate tab. The existing Production composition remains deliberately fail-closed.

## Phase 6.3 — Exact return review, validation and legal approval

### Purpose

Give the user a comprehensible review of the exact statutory return and capture an attributable legal approval without allowing the UI to become a second payload builder.

### Implementation

1. Prepare a candidate `vat.returns.submit` request from the selected HMRC obligation, effective statutory context and `Cash.vwTaxVatSubmission` source snapshot.
2. Present the nine boxes by reading the immutable prepared request through a purpose-built safe view model. Show:
   - box number and statutory description;
   - value exactly as it will be submitted;
   - period and masked VRN;
   - source dataset and snapshot identity in an audit/details view;
   - prepared SHA-256 in an advanced audit view; and
   - PASS/WARN/FAIL preparation and reconciliation findings.
3. Show the relationship to the existing VAT Statement and Submission views without recalculating either. Drill-through may explain source values, but no field is editable in the filing screen.
4. Display HMRC's current business declaration verbatim from a versioned application resource immediately before approval. Require an unchecked-by-default confirmation and a deliberate **Approve and submit** continuation.
5. Persist an immutable approval record containing tenant, logical filing identity, actor, UTC time, declaration type/version and exact-resource digest, prepared body digest, source evidence/snapshot and obligation identity. Preserve the versioned declaration resource so the precise approved wording remains reproducible. Do not store only a boolean or an arbitrary string.
6. Immediately before dispatch, validate that the candidate is unexpired, still belongs to the same tenant/actor journey and still represents the current authoritative source snapshot. Any drift invalidates the approval and returns the user to review.
7. Set `finalised: true` only in the exact candidate intended for this declaration/approval. The approval reference passed to Objective 4 must resolve to the durable record above.

Warnings require reviewed policy. They may permit submission only when their meaning is explicitly non-blocking and the UI records acknowledgement. Errors always block. Do not introduce a generic “submit anyway” path.

### Dependencies and exclusions

Requires an accepted 6.2 matched open obligation and the 6.0 approval store. This phase does not send to HMRC, amend accounting data, support agent declarations or let a user enter a period key/VRN/nine-box value.

### Tests and acceptance

- Golden UI/application tests prove the displayed nine values and digest correspond to the same prepared bytes used by Objective 3 tests.
- Test each blocking readiness condition, warning acknowledgement policy, stale source snapshot, changed obligation, expired candidate and cross-user/tenant access.
- Test declaration text/version, unchecked default, double activation and approval-record completeness.
- Prove no controller/component has a VAT serializer or accepts nine boxes from the browser.
- Preserve the accepted `Cash.vwTaxVatStatement` → `Cash.vwTaxVatSubmission` reconciliation tests and canonical VAT SHA-256.
- Perform keyboard, focus order, screen-reader naming, responsive layout and high-contrast checks on the review/declaration surface.

### Review gate

The human reviewer compares the UI with the prepared JSON, source snapshot and HMRC business declaration. Approval of that evidence is required before any Objective 5 write is enabled.

## Phase 6.4 — Controlled submission and outcome handling

### Purpose

Submit the one approved immutable return and present success, rejection, failure or uncertainty without creating duplicate filings.

### Implementation

1. Dispatch only from the server-side approved candidate retained by the protected prepared-request store. Bind the `AuthorityDispatchContext` to its tenant, HMRC principal, actor, durable approval, fresh sealed client facts and logical `VRN:periodKey` identity.
2. Disable repeat UI activation while the request is in progress, but rely on the durable attempt store—not the button—for duplicate prevention.
3. Present typed outcomes:
   - **Accepted:** HTTP `201`, processing date and safe receipt references;
   - **Rejected:** HMRC status/code/details with actionable guidance;
   - **Not sent/failed:** a pre-send/configuration/authentication failure that can be safely restarted after correction; and
   - **Outcome unknown:** sending may have begun, automatic replay is blocked and reconciliation is required.
4. On accepted submission, retrieve obligations again and retrieve the return by period key. Reconcile it to the approved prepared bytes and source evidence. Do not rewrite the success receipt if readback is temporarily unavailable.
5. On revoked/insufficient authority, retain the preparation and approval only according to their validity rules, guide the user through reauthorisation and never bypass the duplicate/unknown guard.
6. Provide correlation/support references safe for display. Raw responses remain protected and access-controlled.

### Dependencies and exclusions

Requires approved 6.3, a write-scoped grant, fresh fraud context and production-shaped stores. This phase does not auto-submit, schedule VAT returns, retry ambiguous writes, amend a return or mark HMRC acceptance merely because a request left TCWeb.

### Tests and acceptance

- End-to-end recording tests assert exact stored bytes and digest from approval to handler and absence of a request-body serializer in TCWeb and Submission adapter. They must not rely on an in-memory object surviving the review/submit request boundary.
- Cover `201`, documented `4xx` errors, unexpected success status, malformed/oversized response, token refresh, pre-send cancellation, timeout/reset before and after send, restart and concurrent double submission.
- Prove the same tenant/logical filing remains blocked when a different principal tries to replay it.
- Prove accepted receipt fields and raw-content references persist before the UI reports completion.
- Run a single human-approved Azure sandbox journey from TCWeb: obligations → review/declaration → submit → obligation refresh → view-return reconciliation.
- Run all Tax Hub suites and the full TCWeb build.

### Review gate

The reviewer inspects the sandbox receipt, exact digest/readback match and injected ambiguity evidence. No production application or live filing proceeds until unknown outcomes are visibly safe and duplicate writes are impossible under the tested failure model.

## Phase 6.5 — Filing history, readback and reconciliation

### Purpose

Make VAT filing evidence usable after the interactive submission session and expose unresolved work without exposing secrets or raw protected data.

### Implementation

1. Add a VAT Filing History surface sourced from durable approval/attempt/outcome records, not from ephemeral UI state or the current WebHarness preparation store.
2. Show period, status, submitted/updated time, actor display reference, prepared digest, authority correlation/receipt references and reconciliation state. Mask the VRN and protect tenant boundaries.
3. For accepted filings, show the approved nine boxes beside HMRC's retrieved values and the exact `VatReturnReconciliation` result. An empty difference set is explicit evidence, not an assumption.
4. For rejected filings, preserve the attempted digest and HMRC error. Permit a new preparation only after the user returns to the obligation/review journey; never mutate the rejected record.
5. For unknown outcomes, provide a controlled reconciliation action and operational guidance. If the VAT API cannot prove the write outcome, require human resolution; do not present “try again” as ordinary recovery.
6. Show fulfilled obligations and available returns up to the current HMRC API limits. Do not claim access to returns filed before the business joined VAT MTD.
7. Enforce tenant membership and the applicable host access policy for history, with a narrower privileged path for protected raw evidence. Ordinary users never download bearer tokens, fraud facts, secrets or unrestricted raw bodies.
8. Verify every protected Blob object against the digest and version recorded in SQL before it is used for filing or evidence. Treat missing, orphaned, substituted or corrupt content as an explicit integrity failure. Backup and restore procedures must handle SQL metadata and Blob content as one referential evidence set.

### Dependencies and exclusions

Requires 6.4 and the approved persistence/retention model. This is VAT filing history, not a generic cross-authority event store UI. Liabilities, payments and penalties remain excluded.

### Tests and acceptance

- Cover accepted, rejected, failed and unknown attempts across restart and retention boundaries.
- Cover exact reconciliation, every field mismatch, unavailable readback, special period key and stale/rotated OAuth grant.
- Test tenant, policy and raw-content access controls, redaction and purge/audit behaviour.
- Test digest mismatch, missing/orphaned content and coordinated SQL/Blob restore consistency.
- Test history ordering, pagination, time-zone rendering and accessibility.
- Verify the history record links the authority outcome to the immutable approval, body digest and source snapshot.

### Review gate

The reviewer signs off ordinary and privileged evidence views, retention/purge behaviour and the operational procedure for unknown outcomes.

## Phase 6.6 — HMRC approval-ready product hardening

### Purpose

Complete the technical, UX, security and evidence work required before asking HMRC to review the VAT product.

### Implementation

1. Reconcile the implemented business journey against the current HMRC setup and obligations/returns journeys. Provide contextual links for VAT MTD signup, correct Government Gateway account use, amending a filed return and paying VAT.
2. Implement clear user responses for documented OAuth, VAT API, validation, rate-limit and service-unavailable errors. Preserve authority codes for support while avoiding technical or secret leakage.
3. Validate fraud-prevention headers from the deployed production topology with HMRC's validator. Resolve every error. Document any accepted warning and the HMRC discussion required for genuinely unavailable fields such as multi-factor information or vendor licence identifiers.
4. Exercise the required HMRC sandbox sequence with a newly generated organisation test user:
   - retrieve obligations;
   - submit for the open period;
   - exercise view-return because the product uses it; and
   - validate all fraud headers from the deployed topology.
5. Complete security review of OAuth state, antiforgery, CSP, TLS/proxy forwarding, protected stores, log redaction, least-privilege access, versioned key/secret rotation, coordinated SQL/Blob backup and restore, integrity failure handling and incident response.
6. Complete accessibility and responsive-browser review of connect, obligation, review/declaration, outcome, history and error journeys.
7. Add operational health checks and runbooks for HMRC availability, rate limiting, expiring/revoked grants, store failure, unknown attempts, retention/purge and support correlation. Operational telemetry must attribute appropriate activity and measurable resource consumption to the opaque tenant reference without collecting protected tax or identity content.
8. Assemble a redacted approval evidence pack containing application/environment identity, tested endpoint/version matrix, timestamps/correlation references for HMRC log inspection, fraud-validator results, screenshots/journey notes, error handling, supported persona, privacy/support information and test results.

Optional VAT API functions remain excluded from the approval claim unless they have been separately promoted through Objective 3/4, integrated in Objective 5 and included in HMRC testing.

### Dependencies and exclusions

Requires accepted 6.0–6.5 in an internet-reachable HTTPS deployment whose topology matches the proposed production design. This phase does not itself grant production credentials, accept HMRC terms on behalf of the owner, make a live VAT filing or request public listing.

### Tests and acceptance

- Full `tradecontrol.web.sln` and `TaxHub.slnx` builds succeed without warnings.
- All existing contract, Application, adapter, architecture, data-provision and host suites remain green; new TCWeb workflow/UI tests are green.
- Controlled sandbox and fraud-validator evidence has no unresolved error. Any warning has an owner, rationale and explicit external follow-up.
- Fault/restart exercises prove approval immutability, exact bytes, no ambiguous replay, durable outcome/history and tenant isolation.
- A reviewer can trace one filing end to end from VAT Statement through source snapshot, prepared digest, approval, wire attempt, receipt and HMRC readback.

### Review gate

Human review may mark **VAT product technically approval-ready**. This status does not mean HMRC approved, production access granted, a live return filed or the product listed.

## Phase 6.7 — HMRC approval, production activation and compatible-software listing

### Purpose

Complete the external VAT production process while retaining explicit human control over credentials, live filing and public product claims.

### External sequence

1. Within HMRC's current two-week window after completing API testing, the product owner contacts the Software Developers Support Team and supplies the redacted test references needed for log inspection.
2. The owner completes HMRC's requested questionnaires and accepts the current terms of use. Repository work may prepare answers/evidence but cannot accept legal terms on the owner's behalf.
3. Resolve HMRC findings from fraud-header and VAT API testing. Code or journey changes return to the relevant earlier phase and repeat its regression/sandbox evidence.
4. After HMRC grants production access, provision production application credentials and keys through the approved secret facility. Keep sandbox and production applications, redirect URIs, stores and hosts isolated.
5. Perform a production smoke test that does not file a return, where the authority permits it, then obtain separate human approval for one controlled live VAT submission by an eligible Trade Control business.
6. Retain the live attempt, approval, receipt and HMRC reconciliation evidence under the approved production policy. Never copy customer identifiers into tracked project documentation.
7. If Trade Control is offered for retail/commercial use, request compatible-software listing and provide the live-submission VRN directly to HMRC through the approved channel. Listing is recorded only after HMRC confirms it.
8. Make the production feature available to further tenants only after the owner approves rollout, support and monitoring. Provide an immediate kill switch that blocks new submissions without corrupting history or revoking grants unnecessarily.

### Dependencies and exclusions

Requires 6.6 technical approval-readiness, HMRC cooperation, production credentials, an eligible real business and explicit owner approval. Sandbox users and synthetic returns cannot satisfy the live-submission/listing requirement.

This phase does not authorise Codex or an unattended process to accept terms, disclose a customer's VRN, create production credentials, submit a live VAT return, contact HMRC or publish a compatibility claim without the required contemporaneous human instruction.

### Tests and acceptance

- Record separately: HMRC test review, fraud-header review, questionnaire/terms status, production access, production credentials, controlled live outcome and compatible-software listing.
- Verify production host allow-list, redirect URI, secret provider, encryption keys, audit persistence, monitoring, rate limit and kill switch before live enablement.
- Reconcile the controlled live return to its approved Trade Control source and HMRC view-return response where available.
- Repeat relevant regression and security checks after every HMRC-requested change.

### Review and completion gate

The human owner records one of these truthful statuses:

- **Technically approval-ready** — Phase 6.6 accepted; external review not complete.
- **HMRC production approved** — HMRC has granted the applicable production access.
- **Live VAT proven** — the separately approved live submission succeeded and reconciled.
- **Compatible software listed** — HMRC has confirmed the public listing.

Objective 5's VAT-continuity slice is complete when the agreed external target is reached and the implemented workflow remains operational. If HMRC action is pending, retain the technical milestone and leave external completion open.

## Test strategy and regression protection

### Test layers

- **VAT contract tests:** endpoint metadata, request/response parsing, number/date/omission behaviour and error fixtures.
- **Application tests:** exact source mapping, preparation findings, canonical bytes/digest and reconciliation.
- **Trade Control adapter tests:** `Cash.vwTaxVatSubmission`, statutory context/readiness and source snapshot identity.
- **Submission adapter tests:** OAuth, fraud headers, exact wire request, durable attempt/content handling, ambiguity and restart.
- **Architecture tests:** dependency direction, no WebHarness production dependency, no UI/adapter VAT serializer and secret isolation.
- **TCWeb service tests:** tenant/actor resolution, connection/obligation workflow, approval binding, replaceable filing-policy checks and safe view models driven by actual connection state.
- **TCWeb component/end-to-end tests:** connect, select obligation, review, declare, submit, outcome, history and accessibility states.
- **Controlled external tests:** deployed fraud validator and synthetic HMRC sandbox journeys. These are evidence runs, not ordinary build dependencies.

### Non-negotiable regression assertions

- Existing canonical VAT JSON and SHA-256 remain identical unless an externally required contract correction is separately approved and versioned.
- The prepared request approved by the user is the exact request passed to the gateway.
- TCWeb contains no VAT request serializer and accepts no nine-box submission body from the browser.
- An authority obligation, not a local month picker, supplies the period key.
- A changed `Cash.vwTaxVatSubmission` snapshot invalidates approval.
- The attempt exists durably before write I/O and only one active write exists per tenant/VRN/period.
- Timeout/reset ambiguity never becomes an automatic retry.
- HMRC response codes, receipt fields and bounded raw evidence remain available without leaking secrets.
- Reconciliation compares all nine boxes and the period key without rounding, calculation or correction.
- Cross-tenant, cross-principal and insufficient-policy access is denied and audited.
- Protected content is verified against its recorded digest and version; missing, orphaned or corrupt evidence fails closed.
- Operational telemetry permits tenant-level attribution without containing VAT payloads, tax identifiers, OAuth secrets/tokens or fraud facts.
- Preview, unsupported, error-bearing, unapproved and production-misconfigured requests produce zero outbound calls.

## Documentation and history

After each accepted phase:

- record implementation evidence and current status beneath that phase in this work plan;
- add durable discoveries, external decisions and unresolved risks to `docs/projects/Tax Hub/findings.md`;
- add significant implemented changes to `docs/projects/Tax Hub/change-log.md`;
- update user/admin guidance for any visible workflow or operational change; and
- keep secrets, tokens, real VRNs, user credentials and unrestricted authority responses out of the repository.

The HMRC evidence pack should live in an approved protected location. Tracked documentation records redacted references and status only.

## Explicit exclusions

This work plan does not implement or redesign:

- VAT accounting, VAT adjustments or any of the nine-box calculations;
- arbitrary editing of a VAT return in Tax Hub;
- VAT return amendment through an unsupported API;
- an agent/client filing product;
- optional liabilities, payments, penalties, financial-details or customer-information operations;
- HMRC Assist for VAT before its contract and product journey are separately reviewed;
- Corporation Tax, Companies House or MTD Income Tax workflow/transport;
- a generic cross-authority workflow engine;
- a generic ERP RBAC/permissions system;
- multi-tenant provisioning or administration;
- billing, per-user/per-feature licensing or AI integration;
- legacy SA100/XML;
- public listing before HMRC confirms it; or
- unattended production activation or live filing.

## Principal risks and controls

- **Diagnostic architecture leakage:** prevent TCWeb from calling or referencing WebHarness; compose product libraries directly.
- **Second VAT calculation:** render a safe projection of immutable prepared bytes and retain source reconciliation; never populate boxes in UI code.
- **Stale approval:** bind approval to digest/source snapshot/obligation and revalidate immediately before dispatch.
- **Duplicate filing:** durable logical identity, pre-I/O attempt persistence, concurrency control and no ambiguous replay.
- **Identity confusion:** keep ASP.NET session, HMRC grant, reporting subject, tenant and filing actor separate and server-derived.
- **Single-node assumptions:** use a deliberately assigned durable tenant GUID and tenant-scope all workflow/evidence records without implementing provisioning in Objective 5.
- **Role fossilisation:** enforce consequential actions through a narrow replaceable host policy and show HMRC connection state from the protected grant state, not role membership.
- **Agent overclaim:** limit the initial product to business self-filing and use only the business declaration.
- **Authority/local period confusion:** label local dates as forecasts and use HMRC obligations for filing.
- **Production store shortcut:** fail production startup until the separately approved secret, persistence, access and retention facilities exist.
- **Split-store integrity:** verify Blob digest/version against SQL metadata and treat SQL/Blob backup, restore and orphan detection as one production acceptance concern.
- **Unattributable hosting cost:** tag safe operational telemetry with opaque tenant identity from the outset while excluding protected content and deferring billing.
- **App Service topology drift:** validate forwarded headers/trusted peers and fraud facts in the actual deployed topology.
- **Approval overclaim:** track technical readiness, HMRC review, production access, live proof and listing separately.
- **External contract drift:** recheck HMRC guidance, API version, declaration and fraud-header specification at 6.6 and immediately before 6.7.

## Completion condition

Work Plan 6 is delivered incrementally. A later instruction may name one phase, require its tests and stop at its review gate.

The VAT workflow milestone is technically complete when an authorised business user can perform the entire TCWeb journey from an HMRC open obligation through exact review, declaration, one safe submission, durable outcome/history and source-to-HMRC reconciliation, with all regression, security, accessibility and sandbox evidence accepted.

The external milestone is complete only at the explicitly chosen status in Phase 6.7. Other taxes remain paused, and their deferred Objective 4/5 work is neither invalidated nor implied complete by the VAT milestone.

Approval of this work plan authorises planning only. It does not authorise implementation, HMRC contact, acceptance of terms, production credential creation, disclosure of a real VRN, a live submission or public listing.
