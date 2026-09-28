# Work Plan 6 — Phase 6.0 review qualifications

The Phase 6.0 hosting decision and Work Plan 6 have been reviewed.

The overall design is accepted and is considered a strong basis for Objective 5 VAT integration.

The selected Azure production direction is approved in principle:

- Azure Key Vault;
- Azure SQL Tax Hub persistence;
- private Azure Blob storage;
- App Service managed identity;
- fail-closed production composition; and
- direct TCWeb integration with the Tax Hub Application/adapters rather than WebHarness.

The following points are **qualifications and contextual improvements**, not a request to redesign the plan.

Update:

`docs/projects/Tax Hub/implementation/tax-hub-workplan-6.md`

and, where appropriate:

`docs/projects/Tax Hub/specs/reference/tax-hub-objective-5-hosting-decision.md`

to make these decisions explicit.

Do not implement Phase 6.1 yet.

## 1. Make the multi-tenant destination explicit

The current Azure deployment contains one Trade Control node, but this is the first deployment shape of a future **multi-tenant hosted Trade Control service**.

The architecture must therefore be described as:

**single-tenant in the current deployment, multi-tenant by design.**

The opaque tenant GUID introduced by Phase 6.0 is a durable architectural identity, not merely a convenient identifier for the current App Service.

It must:

- be generated/assigned deliberately rather than from transient deployment state;
- remain stable across deployment, restart, restore and migration;
- scope HMRC grants, preparations, approvals, attempts, protected content and filing history;
- later support many tenants within the hosted service without changing the Tax Hub contracts; and
- never be accepted as an arbitrary browser-supplied value.

Do not introduce multi-tenant provisioning or administration in Objective 5. The purpose of this clarification is to prevent current single-node assumptions becoming embedded in the Tax Hub architecture.

## 2. Preserve the deliberately simple Accounts Mode security model

Accounts Mode currently has a deliberately simple user/role model.

Objective 5 VAT integration must not introduce a general ERP permission framework.

The existing proposal that Administrators and Managers perform HMRC connection, disconnection, approval and submission should therefore be described as an **initial product-policy proposal**, not an architectural requirement of Tax Hub.

Do not assume that an ordinary authenticated Trade Control user is incapable of possessing legitimate HMRC authority.

Likewise, successful HMRC authorisation does not itself grant Trade Control membership, elevated Trade Control privileges or cross-tenant access.

Keep these concepts distinct:

- Trade Control authentication;
- tenant membership;
- current Accounts Mode role/policy;
- HMRC OAuth authority and scopes; and
- attributable filing approval.

The VAT workflow should depend only on a narrow filing-authorisation decision that can initially use the existing simple Accounts Mode policy and evolve later without changing Objective 3, Objective 4 or the VAT workflow contracts.

Do not design a generic RBAC/permissions subsystem as part of this work.

## 3. HMRC connection visibility should use actual connection state

The product now stores/shares the HMRC OAuth connection state through the ASP.NET Core Identity/product identity integration.

Do not use Administrator/Manager role membership as a proxy for whether HMRC functionality should be visible or meaningful to a user.

Where appropriate, UI presentation should use the actual protected HMRC connection/authorisation state available for the authenticated tenant/user context.

For example, distinguish truthfully between:

- not connected;
- connected;
- reauthorisation required;
- disconnected; and
- unavailable.

Role/policy checks still govern consequential actions where required, but UI visibility and connection status should not imply that only Administrators or Managers can ever possess HMRC authority.

Do not expose tokens, secrets or raw OAuth state to the UI merely to make this decision.

## 4. Clarify Key Vault versus tenant OAuth storage

Azure Key Vault is approved for **application-level secrets and cryptographic keys**, including:

- HMRC application client secret;
- encryption/envelope keys; and
- other deployment-level secrets where appropriate.

Per-tenant/per-principal mutable OAuth grant material belongs in the protected Tax Hub grant store in Azure SQL, encrypted using the approved key hierarchy.

Do not model every tenant access token or refresh token as an individual Key Vault secret.

Persist enough key/version metadata with encrypted material to permit controlled key rotation and continued decryption of retained records.

The existing ignored development `clientsettings.json` remains a development-only static sandbox credential source and must not define production secret architecture.

## 5. Strengthen protected-content integrity and recovery requirements

Prepared candidates, exact submitted VAT bytes and protected authority evidence stored in Blob must retain their recorded digest/integrity relationship with SQL metadata.

On retrieval, protected content used for filing or evidence should be verified against its recorded digest and fail closed on mismatch.

Backup/restore planning must consider SQL metadata and Blob content together.

A restore exercise should demonstrate that:

- SQL opaque references resolve to the intended Blob content/version;
- stored digests still verify;
- missing or orphaned content is detected rather than silently ignored; and
- restored approval/attempt history remains internally consistent.

Do not design a large disaster-recovery framework in Phase 6.0. Make the invariant and later production acceptance requirement explicit.

## 6. Add tenant-level resource telemetry as a hosting design requirement

The future Trade Control commercial model is intended to charge primarily for managed hosting rather than functionality or user count.

The base service may include normal microbusiness usage, while materially exceptional infrastructure or AI consumption can later be attributed to the tenant.

Tenant-level resource telemetry should therefore be designed in **from the beginning**.

This phase does not implement billing.

It should establish that production observability can attribute appropriate operational consumption to the opaque tenant identity, for example:

- workflow/request activity;
- Tax Hub SQL/storage consumption where measurable;
- protected Blob storage/read activity;
- HMRC/API activity;
- submission attempts and operational load; and
- later AI model/token consumption.

Telemetry must not expose VAT payloads, tax identifiers, OAuth secrets, fraud facts or other protected content.

Do not create per-user or per-feature licensing machinery.

The purpose is future cost attribution and capacity planning for the multi-tenant hosted service.

## 7. Preserve the existing Objective 5 boundaries

These qualifications must not weaken the existing Work Plan 6 invariants.

In particular preserve:

- the accepted VAT truth chain;
- exact Objective 3 prepared bytes;
- immutable approval;
- HMRC obligation authority over filing periods;
- fail-closed submission;
- durable duplicate protection;
- unknown-outcome handling;
- separation of ASP.NET Identity from HMRC OAuth;
- secret isolation;
- truthful external-status reporting;
- TCWeb's direct use of Application/adapters;
- WebHarness as diagnostics only; and
- the initial business-self-filer product scope.

Do not expand this revision into agent filing, generic ERP security, multi-tenant provisioning, billing, AI integration or other tax families.

Those are future concerns.

## 8. Nature of this revision

This is a clarification pass over an already accepted design.

Prefer small additions and qualifications to wholesale rewriting.

The intention is to ensure that Work Plan 6:

1. reflects the future multi-tenant hosted-service destination;
2. does not accidentally fossilise today's simple Accounts Mode roles into a general ERP security architecture;
3. uses actual HMRC connection state rather than role membership as a proxy for connection visibility;
4. makes the Azure secret/key/token boundaries explicit;
5. preserves content integrity through storage and recovery; and
6. establishes tenant-level operational attribution before the hosted service grows.

Do not modify source code.

After updating the documentation, summarise the changes made and stop at the Phase 6.0 human review gate.
