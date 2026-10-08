# Phase 7.0 — Companies House Integration Boundary

8 October 2026

## Outcome

Phase 7.0 defines the TCWeb composition, persistence contract and protected-configuration boundary for unaudited filleted FRS 105 micro-entity accounts. It adds no filing UI and enables no external send.

The default and only dispatch mode is `SendDisabled`. An enabled development composition may select the official-test environment with a local evidence-store root. Production persistence is modelled using the previously accepted Key Vault, workflow database and private Blob facilities, but that composition fails validation until those facilities are implemented and reviewed.

## Product boundary

`ICompaniesHouseProductWorkflow` is the future Razor/API adapter boundary. Its callers may select an accounting period and pass opaque preparation, approval or conversation references. They cannot supply:

- tenant, company, principal or actor identity;
- company authentication, presenter credentials or package values;
- balance-sheet/statutory values;
- iXBRL, GovTalk XML or another request body; or
- an unrestricted Companies House response.

Implementations must derive tenant, company, ASP.NET principal and internal actor identity on the server. The accepted `CompaniesHouseAccountsPreparer` remains the only accounts/document preparation path.

## Durable records

The persistence port defines four related views of one logical filing:

1. **Preparation** — server-derived identities, period/profile, source snapshot, exact document/package digests and opaque protected-content handles. A preparation expires after 24 hours.
2. **Approval** — actor-attributable approval bound to the preparation, declaration version/digest, source snapshot, document/package digests and one logical filing identity.
3. **Conversation** — the one active XML-gateway conversation for that logical filing, including submission/transaction identifiers, state, exact request/response digests, opaque protected-content handles, conditional status-acknowledgement state and recovery-required state.
4. **History projection** — company-scoped safe metadata: period, supported profile, actor attribution, submission number, state, digests, timestamps and a bounded support reference. It never exposes protected-content handles.

The logical filing identity is tenant + company identity + period + filing profile. Actor or browser-session changes cannot create a second active conversation.

### Persistence rules

- Every lookup is tenant-scoped; company history is also scoped by the server-derived company-identity digest.
- Preparation, approval and conversation mutations must be atomic.
- At most one active conversation may exist for one logical filing identity.
- The exact document, package, sent request and received response remain in the protected-content facility. Metadata stores only opaque handles and SHA-256 values.
- Every protected-content retrieval must recompute and compare the retained digest before use.
- Missing, malformed or digest-mismatched evidence fails closed and becomes recovery-required; it never causes a reconstruction or repeat send.
- Restart recovery resumes the retained conversation. An ambiguous send is not converted into a new preparation or submission.
- Terminal conversation evidence is retained for seven years from its last event. This is a conservative Tax Hub operational-evidence policy, not a statement of the Companies Act accounting-record retention period.

The development-file composition remains useful for isolated automated tests, but it is not the product implementation path. The first real store will be Azure-managed and implemented with the Phase 7.1 preparation workflow. Phase 7.0 defines the records and invariants without adding a second ad-hoc JSON store that would immediately be replaced for deployment.

## Protected configuration

TCWeb host options contain no presenter ID, authentication value, package reference, company authentication code or configurable gateway URL.

- Presenter credentials and package data have named protected references only inside `TradeControl.Tax.UK.Adapters.Submission`.
- Company authentication is resolved at dispatch time by `ICompaniesHouseCompanyAuthenticationResolver` using a server-derived tenant GUID and company-identity SHA-256. The browser never supplies the company number or authentication value to that boundary.
- The resolver returns a short-lived `ProtectedSecret`; clear text must not be persisted, logged or placed in a workflow record.
- Official-test and future live transports use fixed allow-listed HTTPS endpoints rather than caller configuration.
- Receipt of Companies House's live-package approval is not treated as receipt or installation of the package itself.

The existing WebHarness test-account materializer remains diagnostic official-test infrastructure. Its one-shot controls, SQL connection strings and DTOs are not composed into TCWeb.

## Event Log and operational evidence

Ordinary Event Log entries may record operation name, safe outcome code, accounting period, workflow/conversation reference, state, timestamp and a bounded support reference. They must not include XML/iXBRL, company authentication, presenter/package values, full company identity, unrestricted examiner content, protected-store paths or authority response bodies.

Detailed evidence is recovered through authorised workflow services, tenant/company checks and digest-verified protected content. An authority outage is not an application-readiness failure; unavailable local database or selected evidence facilities are.

## Current gates

| Concern | Phase 7.0 position |
|---|---|
| Official-test preparation/transport contracts | Existing and retained |
| Block A simulator | Retained for deterministic lifecycle/failure regression |
| TCWeb product workflow contract | Defined, not yet implemented |
| Development evidence store | Isolated automated-test facility only; not the deployed product path |
| Azure managed evidence facilities | Selected as the first real implementation; begins with 7.1 |
| Presenter/package values in TCWeb | Prohibited |
| Company authentication | Protected dispatch-time resolver only |
| External dispatch | Send-disabled |
| Filing UI | Not added |
| Live package | Approved for issue but not yet received/installed |

## Human review decision — accepted 8 October 2026

The reviewer accepted:

1. seven years from the last conversation event as the Tax Hub operational-evidence retention policy; and
2. an Azure-first Phase 7.1 implementation rather than a disposable local product store.

The current single deployed Trade Control node will use one stable opaque tenant GUID, while schemas, keys, lookups, protected-content paths and tests remain tenant-partitioned from the start. Cross-tenant isolation will be exercised with artificial second-tenant test records even though only one tenant is initially deployed.

The intended accounting source is a synthetic Candidate 4 STD Year 3 company on Azure SQL server `tradecontrol-db96115e`. Read-only preflight established that this is the server name, not a database name: its current company database is the Basic-tier MIN instance `tcNodeDb4-COMIPFVT1-COMIN26`, while the deployed TCWeb application is separately bound to the VAT development database `tcNodeDb4-HMRC62-2017`. Candidate 4 therefore requires an explicit database/cutover decision rather than silently overwriting MIN or displacing VAT. Regeneration is a separately controlled operation: preserve the accepted Candidate 4 filing evidence, verify the exact database/resource and point-in-time recovery position, record the current service objective, temporarily promote capacity only for generation where required, validate the regenerated accounting/statutory state, and return the database to Basic. Phase 7.0 does not itself authorise an external submission.

## Azure Candidate 4 development database — completed 8 October 2026

The controlled regeneration created the separate database `tcNodeDb4-COSIPFVT1-COSTD26` on `tradecontrol-db96115e`. It was initialized from a server-side copy of the configured MIN sandbox so the supported generator could preserve an existing node/user identity, then upgraded to the current repository DACPAC. The MIN source, VAT database and both App Service connection bindings were left unchanged throughout generation and validation. The accepted Companies House test-submission evidence remains historical evidence and was neither overwritten nor represented as evidence from this new database.

Two portability defects were exposed and corrected in the repository before regeneration:

1. a copied node could carry a non-empty legacy VAT value that failed the current nine-digit reporting-profile constraint; the synthetic bootstrap and statutory-profile normalizer now preserve only an exact nine-digit value and otherwise use the existing synthetic fallback; and
2. the generator inferred the copied node's March year-end. It now accepts an optional, range-checked `@FinancialMonth`, retaining inference when omitted and allowing this controlled run to specify October explicitly.

The final base invocation selected the STD company template, three completed years, October financial month and the test-only `2026-10-07` temporal anchor. The read-only completed-year regression passed with periods ending `2024-09-30`, `2025-09-30` and `2026-09-30`, Equity Bridge variances `0.00`, `0.00` and `0.08`, ready statutory projections, 52 Object flows/27 multi-level Object links and 872 Project flows/754 multi-level Project links.

The tracked Year 3 additive script then passed its default rollback rehearsal before the identical change was applied. It proved that Year 1 and Year 2 statutory facts and Equity Bridges were unchanged. The final Year 3 contained 281 Projects, 274 invoices and 190 payments; its ready balance sheet reported fixed assets `6800.00000`, current assets `495943.40048`, creditors due within one year `40125.60613` and creditors due after one year `3000.00000`. These regenerated transaction-derived figures need not equal the earlier accepted local Candidate 4 snapshot; their continuity and validation evidence is the relevant development result.

S0 completed the base generation correctly but required about 30 minutes and is unsuitable for an interactive regeneration loop. The corrected S3 run completed in a few minutes. S3 was temporary only: the database is online at Basic/2 GiB after validation.

Only after those gates passed, `tcweb-payg-db96115e` was repointed through a new versioned Key Vault secret to the STD database and restarted. Its public root, `/health/live` and database-backed `/health/ready` all returned HTTP `200`. The separate `taxhub-payg-db96115e` Swagger/WebHarness App Service was stopped because this product-development increment does not require it. The old VAT and MIN databases remain intact and can be rebound explicitly if their reference journeys resume.

The first authenticated Tax Hub Dashboard request on Basic reached 100% DTU and CPU for two consecutive one-minute metric intervals and exceeded the application command timeout. Reads were modest and log-write pressure was negligible, identifying compute throttling rather than connection, identity or write-volume failure. The development database was therefore moved to Standard S2 (50 DTUs); it remained online and `/health/ready` continued to return HTTP `200`. S2 is a temporary interactive-development tier and should be reviewed/downscaled when this Azure development interval ends. No workflow-store implementation, gateway request or external submission occurred.
