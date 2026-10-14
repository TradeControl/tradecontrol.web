# Tax Hub Objective 5 — VAT hosting and persistence decision for review

28 September 2026

## Status

The overall Phase 6.0 design and Azure direction are accepted in principle, subject to the qualifications recorded below. Production composition remains disabled pending final gate approval and implementation of the selected facilities.

## Product and dependency boundary

The first product persona is a VAT-registered business filing its own return. Agent filing is not represented. Administrators and Managers are the initial product-policy proposal for HMRC connection, disconnection, approval and submission; this is not an architectural Tax Hub role model. The workflow depends on a narrow replaceable filing-authorisation decision and does not introduce a general ERP permission system.

TCWeb references the Tax Hub Application, Trade Control adapter and Submission adapter directly. It does not reference or call the diagnostic WebHarness. The UI-facing VAT workflow accepts operation references such as period, preparation, approval and attempt references; it does not accept a tenant identifier, nine VAT boxes or an authority request body from the browser.

`IVatProductWorkflow` is an API-shaped, transport-neutral use-case boundary even when TCWeb invokes it in-process. Razor Pages and Blazor components are clients of that boundary, not the implementation boundary itself. A future authenticated HTTP API may adapt the same operations and safe result models without moving tax logic into controllers or changing the immutable preparation and dispatch path. The contract therefore contains no Razor/MudBlazor types, `HttpContext`, `ClaimsPrincipal`, MVC results, EF entities, connection strings, secrets, tokens, raw fraud facts or caller-supplied VAT boxes.

The WebHarness remains another test client/composition root over the Tax Hub Application and adapters. It may exercise the same public use cases and HMRC contract descriptors, which keeps Swagger and sandbox testing useful, but TCWeb and the Tax Hub libraries never call into WebHarness. WebHarness-specific stores, authentication bridges and diagnostic routes are not part of the product API. This is analogous to the Cash Statement page invoking an export request in-process today: the host interaction is not currently an HTTP API, but its request/result capability can be exposed through one without making the page the service boundary.

The current deployment is single-tenant, but the hosted-service architecture is multi-tenant by design. Each Trade Control node is deliberately assigned one opaque GUID tenant reference in protected host configuration. It is not derived from an App Service instance, process, database connection or other transient deployment state, and it remains stable across deployment, restart, restore and migration. It scopes grants, preparations, approvals, attempts, protected content and filing history, and later permits multiple tenants without changing Tax Hub contracts. Objective 5 does not implement tenant provisioning or administration.

The authenticated ASP.NET Identity subject, mapped internal `Usr.tbUser` identifier and configured home reporting subject are resolved on the server. The HMRC authorisation-principal reference comes from the protected grant facility. A dispatch context may be constructed only when the protected principal and current Trade Control identity belong to the same tenant. Trade Control authentication, tenant membership, Accounts Mode policy, HMRC OAuth authority/scopes and filing approval remain separate decisions.

## Selected production facilities

| Concern | Selected facility | Boundary |
|---|---|---|
| HMRC application client secret and data-protection/envelope keys | Azure Key Vault, accessed through the App Service managed identity | Key Vault holds deployment/application secrets and cryptographic keys. Values are resolved only inside Submission composition and are never returned through workflow/UI models. |
| OAuth pending states and grants | Dedicated Azure SQL Tax Hub workflow schema/database | Per-tenant/per-principal mutable grants and tokens are encrypted using the approved key hierarchy rather than stored as individual Key Vault secrets. Records retain key/version metadata for controlled rotation and historical decryption, unique tenant/principal/scope ownership, optimistic concurrency and auditable revocation. |
| Fraud context | Azure SQL metadata plus encrypted private content where required | Tenant/principal/actor binding, topology fingerprint and expiry are enforced by the Submission boundary. Header values are not written to ordinary logs. |
| Prepared candidates and exact bytes | Azure SQL metadata plus a private Azure Blob container | Short-lived opaque reference; digest, Blob version and contract metadata in SQL; exact bytes encrypted at rest, digest-verified on retrieval and never deserialised/reserialised by TCWeb. |
| Approval and attempt metadata | Azure SQL | Append-oriented approval/attempt records with unique tenant plus logical-submission identity and pre-I/O reservation. |
| Bounded authority payload/response content | Private Azure Blob container with SQL opaque references | No public access, managed-identity access only, strict payload/response size bounds, digest/version verification and separate privileged evidence access. |
| Backup and recovery | Azure SQL point-in-time/long-term retention and Blob versioning/soft delete according to the approved production policy | SQL and Blob are one evidence set. Restore exercises verify references, versions and digests, detect missing/orphaned content and retain internally consistent approval/attempt history. |

The facilities should normally use the existing Azure region and resource boundary, but VAT workflow records must not be added to the Trade Control accounting schema merely for convenience. Azure SQL and Blob firewalls/private networking, managed identities and least-privilege roles are required before production enablement.

The git-ignored development `clientsettings.json` remains a static sandbox credential source for local/reference testing only. It does not define production secret or grant storage.

## Proposed retention for approval

| Record | Proposed policy |
|---|---|
| OAuth pending state and verifier | Maximum 10 minutes, consumed once, then purged. |
| Active OAuth grant | Until disconnect, revocation, expiry or replacement; superseded token material is removed promptly. |
| Sealed browser/fraud context | Usable for no more than 15 minutes; purge protected facts within 24 hours unless a documented security incident hold applies. Do not retain rendered fraud headers. |
| Unapproved prepared candidate | Maximum 24 hours; earlier on replacement or source change. |
| Durable approval, submission attempt, exact submitted VAT bytes and safe receipt evidence | Seven years from the filing event, subject to final legal/privacy review. Unknown attempts are retained until resolved and then enter the normal period. |
| Bounded raw authority response/error content | 30 days by default; retain longer only through a privileged, audited support/legal hold. Safe parsed receipt metadata remains with the attempt. |
| General application logs | No tokens, client secrets, raw fraud facts, prepared bodies or unrestricted authority responses. Normal operational retention applies only to redacted correlation data. |

Purges must be tenant-scoped, auditable and ordered so metadata never points silently to missing content. Legal/support holds require a recorded privileged action. Ordinary Tax Hub users cannot access raw protected evidence.

## Connection presentation and filing policy

VAT UI visibility uses the actual protected HMRC connection state for the authenticated tenant/user context: not connected, connected, reauthorisation required, disconnected or unavailable. Role membership is not a proxy for connection state and the UI receives no token, secret or raw OAuth record.

Consequential actions use a narrow host filing-authorisation policy. Administrators and Managers are the initial policy, but ordinary authenticated users are not architecturally presumed incapable of legitimate HMRC authority. Successful HMRC authorisation does not confer Trade Control membership, elevated Accounts Mode privileges or cross-tenant access.

## Tenant-attributable observability

Production observability must attribute appropriate workflow/request activity, measurable Tax Hub SQL and Blob usage, HMRC/API activity, attempts and operational load to the opaque tenant identity. The same correlation model can later include exceptional AI consumption. Telemetry must not contain VAT payloads, tax identifiers, OAuth material, fraud facts or protected evidence.

This requirement supports capacity planning and future cost attribution. Phase 6.0 does not implement billing, per-user/per-feature licensing or AI integration.

## Fail-closed host configuration

`TaxHub:VatProduct` is disabled by default. When enabled, startup validation requires an opaque tenant GUID, an HTTPS public origin, a relative OAuth callback and an approved persistence mode. Development file stores require an absolute path and a Development host. Production HMRC selection is rejected, and the selected Azure mode is also rejected, until its concrete implementations and composition are reviewed. Consequently Phase 6.0 cannot accidentally turn the current App Service into a filing host.

## Review decisions required

The Phase 6.0 gate asks the owner to approve or amend:

1. direct TCWeb → Application/adapters dependency direction and exclusion of WebHarness;
2. the API-shaped workflow contract and WebHarness's continuing role as a replaceable test client;
3. business self-filer persona and the replaceable filing-policy model, initially backed by Administrators/Managers;
4. deliberately assigned, restore-stable and future-multi-tenant identity plus the server-derived ASP.NET subject, internal actor, reporting subject and protected HMRC principal;
5. Key Vault for application secrets/keys, versioned encrypted OAuth grants in Azure SQL, and digest-verified private Blob content;
6. proposed retention, coordinated recovery and privileged-access policy; and
7. protected tenant-level operational attribution without billing implementation.

Approval authorises later implementation of these facilities. It does not enable production HMRC access, filing UI or a live submission.
