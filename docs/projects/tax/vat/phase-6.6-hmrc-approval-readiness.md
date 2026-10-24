# Phase 6.6 — HMRC VAT approval readiness

## Status

**Accepted on 2 October 2026.** Technical and interactive evidence is complete, and the owner has marked the VAT product technically approval-ready. This status does not claim HMRC approval, production credentials, a live filing or compatible-software listing.

## Current HMRC approval baseline

The implementation was reconciled on 2 October 2026 against HMRC's current [VAT (MTD) end-to-end service guide](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/) (updated 23 June 2026), [VAT API v1.0](https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0) (updated 5 August 2026) and [fraud-prevention specification 3.3](https://developer.service.hmrc.gov.uk/guides/fraud-prevention/).

Trade Control supports a business filing its own VAT return. It does not claim agent support. Its approval scope is:

| HMRC operation | Product use | Evidence required |
|---|---|---|
| Retrieve VAT obligations | Required journey and filing-period authority | Fresh organisation sandbox user, deployed TCWeb |
| Submit VAT return for period | Required journey | Open canned obligation, exact approved return, HTTP 201 receipt |
| View VAT return | Optional HMRC endpoint used by Trade Control | Exact nine-box readback after submission |
| Test Fraud Prevention Headers | Mandatory approval evidence | Deployed TCWeb product composition, specification 3.3, zero errors |

Liabilities, payments, penalties, customer-information and HMRC Assist endpoints are not part of this approval claim. The application links to authoritative GOV.UK guidance for Making Tax Digital for VAT, the correct Government Gateway account, correcting a filed return and paying VAT rather than pretending to implement those excluded journeys.

HMRC says the Software Developers Support Team must be contacted within two weeks after completing the API tests so that the test logs remain available. That external contact, the two HMRC questionnaires, acceptance of terms and any live filing belong to Phase 6.7 and require the owner's contemporaneous approval.

## Product hardening

- TCWeb now owns a sandbox-only, authenticated fraud-header validation route. It captures fresh browser facts and uses the same protected grant, tenant/principal context, deployment topology, formatter and public server path as ordinary TCWeb VAT requests. It has no dependency on the WebHarness.
- The connection panel explicitly tells a self-filer to use the business's VAT organisation Government Gateway account and links to current HMRC/GOV.UK journeys. Sandbox hosts expose **Validate fraud headers** only to the existing Administrator/Manager filing policy.
- Obligation and submission failures translate documented authorization, rate-limit, maintenance/service, invalid-profile and fraud-context outcomes into actionable language while retaining safe HMRC codes and support references. Tokens, secrets, fraud values, raw VAT bodies and unrestricted responses remain absent.
- Enquiry refresh is user-throttled and bounded. VAT writes are never automatically replayed. Timeout or transport ambiguity remains an unknown attempt requiring readback reconciliation.
- `/health/live` verifies the process, while `/health/ready` verifies the local Trade Control database and configured VAT evidence-store boundary. HMRC availability is deliberately excluded from App Service readiness so an authority outage cannot cause recycling of a healthy product.
- Identity and session cookies are HTTP-only, secure-only and `SameSite=Lax`. Unsafe MVC actions retain automatic antiforgery validation. Responses add `nosniff`, a strict-origin referrer policy, a restrictive permissions policy and a CSP baseline prohibiting plugins, foreign framing, foreign base URLs and foreign form actions.
- Interactive validation exposed and corrected a diagnostic-path defect: the validator initially passed the OAuth placeholder in place of the newly sealed browser-facts reference. The deployed route now resolves the current tenant/principal/actor-bound reference exactly as ordinary VAT calls do. The corrected Release passed the TCWeb boundary suite and the deployed validator returned no invalid-header errors.
- An App Service recycle immediately after HMRC accepted a sandbox filing exercised a genuine post-acceptance interruption. The HTTP 201 receipt remained durable, the browser returned to the dashboard, and a persistent warning directed the user not to resubmit but to use Filing History. Filing History exposed the missing-readback state and one controlled **Reconcile with HMRC** action. Readback then matched the period key and all nine approved values, removed the action and cleared the dashboard warning. Because acceptance was already known, the outcome truthfully remained `HMRC-SUCCESS (HTTP 201)`; only the missing reconciliation evidence was completed.

## Security review

| Area | Release-candidate finding |
|---|---|
| OAuth | CSPRNG state, S256 PKCE, fixed registered callback, tenant/principal/actor binding, ten-minute pending lifetime, single consumption, exact scopes and encrypted grants are enforced. Callback values are never logged. |
| Browser requests | Connect is a top-level navigation. Disconnect, local sign-out and client-fact capture are authenticated and antiforgery protected. No return URL or tenant/VRN/body is accepted for filing. |
| TLS and ingress | Public origin and callback are fixed protected configuration. Fraud capture accepts forwarding only from configured trusted peers and binds evidence to the reviewed topology. Production ingress and stable public addresses must be revalidated before activation. |
| Protected stores | The deployed sandbox uses the explicitly accepted encrypted file/reference implementation with tenant/principal access checks, bounded content, digest verification and atomic replacement. This is not a production hosting decision. |
| Redaction | Routine representations redact secrets, tokens, identifiers and fraud facts. User-visible diagnostics expose bounded safe codes, attempt/support/correlation references and masked VRNs only. |
| Authorization | The current replaceable filing policy permits Administrators and Managers. Trade Control membership, reporting subject, tenant, OAuth authority and exact approval remain independent checks. |
| Integrity and replay | Approval binds the exact prepared digest and source snapshot. The attempt is reserved before network I/O; tenant plus logical submission blocks duplicates across actors. Missing/corrupt content fails closed. Unknown writes cannot be retried ordinarily. |
| Production keys and persistence | The accepted target remains managed identity + Key Vault, versioned encrypted workflow records in Azure SQL and digest/version-verified private Blob evidence. Rotation, least-privilege roles and coordinated SQL/Blob restore must be implemented and exercised before production HMRC is enabled. |

## Operational runbook

| Symptom | Safe action |
|---|---|
| `/health/live` fails | Treat the application process as unavailable; inspect App Service deployment/startup logs. Do not infer anything about a VAT filing. |
| `/health/ready` fails | Check the Trade Control database and local evidence-store configuration. Keep submission disabled until readiness returns. |
| HMRC rate limit | Stop manual refreshes, retain the support/attempt reference and wait before another enquiry. Never replay a write because of a 429 response. |
| HMRC maintenance or 5xx | Leave local records and approvals unchanged; use HMRC service status and retry only a read later. Treat an interrupted write according to its durable attempt state. |
| Reauthorisation required | Reauthorise the same business account. If the VAT identity/account is wrong, disconnect and connect the correct organisation account. |
| Client not authorised | Verify the reporting-subject VAT registration and the Government Gateway organisation account. Do not change calculated return values. |
| Evidence-store/integrity failure | Fail closed, preserve metadata and support references, restrict raw-evidence access and investigate missing, substituted, corrupt or orphaned content. |
| Unknown write | Do not submit again. Use Filing History **Reconcile with HMRC**; retain the attempt and correlation references if readback cannot prove the outcome. |
| Retention/purge | Never remove active/unknown attempts. Apply tenant-scoped, auditable purge order so metadata cannot silently point to missing content; honour recorded legal/support holds. |
| Suspected credential compromise | Disable new submissions, rotate the affected application secret/key through the approved versioned process, retire grants where required and preserve redacted incident evidence. |

## Interactive evidence sequence

The fresh sandbox organisation user is kept only in the ignored `.local/sandbox` location. No credential, user ID, VAT registration number, token or unrestricted response is copied into this document.

1. Disconnect the previous sandbox grant and align the reporting subject's VAT registration with the fresh organisation's VRN.
2. Connect from deployed TCWeb, using the fresh organisation Government Gateway account.
3. Capture the VAT Submission, open HMRC obligation, exact return review, approved-not-submitted state, accepted receipt, fulfilled obligation and filtered Filing History screenshots for the user guide.
4. Record redacted endpoint/status evidence for obligations, submission and view-return, plus timestamps and safe attempt/correlation references suitable for HMRC log inspection.
5. Run **Validate fraud headers** from the deployed TCWeb topology. Acceptance requires zero errors. The truthful empty `Gov-Client-Multi-Factor` and `Gov-Vendor-License-IDs` advisories remain owned external follow-ups: the current application sign-in is single-factor and no reviewed originating-device software licence exists, so values must not be fabricated.
6. Repeat the principal desktop journey with keyboard-only operation and browser zoom, and inspect the responsive/mobile presentation for connection, obligations, review, outcome, history and errors.
7. Exercise an interrupted filing outcome without replaying the VAT write. Capture the persistent dashboard warning, the controlled Filing History recovery action and the completed exact readback.

## Interactive evidence result

The deployed TCWeb journey completed on 2 October 2026 against the dedicated historically aligned synthetic dataset and a fresh HMRC sandbox organisation account. Tracked documentation deliberately omits the account credentials and VAT registration number.

- The open `18A2` obligation was retrieved and used to review all nine immutable VAT values for 1 April to 30 June 2017.
- Internal approval remained visibly distinct from submission. One separately acknowledged submission returned HTTP 201 and an HMRC receipt.
- View-return readback matched the approved period key and all nine values exactly. A subsequent obligation refresh presented `18A2` as already submitted, and Filing History retained matching approval, dispatch, receipt and readback evidence.
- The deployed specification 3.3 fraud-header validator returned `POTENTIALLY_INVALID_HEADERS` with no errors. Its only warnings were the truthful empty `Gov-Client-Multi-Factor` value for the current single-factor journey and the absent `Gov-Vendor-License-IDs` value because no reviewed originating-device software licence exists. These warnings require disclosure to HMRC during external review; fabricated values remain prohibited.
- Human inspection passed at normal desktop width, 200% browser zoom, keyboard-only navigation and a 390 by 844 responsive viewport. Content remained readable, focus and controls remained reachable, information panels reflowed, mobile content was visible and no page-wide horizontal overflow was reported.
- A controlled App Service recycle after HMRC returned HTTP 201 interrupted the browser before the automatic readback completed. The accepted receipt and immutable filing evidence survived the restart. The dashboard warned against a repeat submission, Filing History offered controlled reconciliation, and the subsequent HMRC readback matched all nine approved values exactly. The recovery action then disappeared and the warning cleared. Redacted before-and-after screenshots are retained as `vat-filing-history-reconciliation.png` and `vat-filing-history-reconciled.png` in the user-guide repository.
- The final recovery-feedback Release was deployed to the existing `tcweb-payg-db96115e` App Service as deployment `6e82b841-80f5-49b7-83cd-b73389070cb8`; one instance succeeded and `/health/ready` returned HTTP 200 `Healthy`.
- `tradecontrol.web.sln` and `TaxHub.slnx` built in Release with zero warnings and zero errors. VAT, MTD Income Tax and Company contract suites, Application, Trade Control adapter, Submission adapter, Architecture, WebHarness and TCWeb boundary suites all passed (496 assertions plus 44 MTD Income Tax endpoint descriptors). The database-backed Data Provision executable was not rerun because its required secret-backed `TC_NODE_CONTEXT` was intentionally absent; the solution build compiled it successfully, and no data-provision code changed in this phase.

## Evidence checklist

- [x] Fresh organisation OAuth and correct-account journey completed.
- [x] Obligations retrieval recorded.
- [x] Open-period review and exact approval recorded.
- [x] Submission accepted with HTTP 201 receipt evidence.
- [x] View-return readback exactly matches all nine approved values and period key.
- [x] Refreshed obligation is fulfilled/already filed.
- [x] Filing History contains immutable approval, dispatch, receipt and readback evidence.
- [x] Deployed TCWeb fraud validator has zero errors; any warnings have rationale and owner.
- [x] Desktop, zoom, keyboard and mobile accessibility review completed.
- [x] Full Release builds and applicable offline suites pass; the unchanged secret-backed Data Provision executable is explicitly qualified above.
- [x] Interrupted filing is blocked from replay, reconciled by HMRC readback and captured for the user guide.
- [x] Redacted screenshots and HMRC-log references assembled, including interruption recovery.
- [x] Human reviewer marks the product technically approval-ready.

The technical and interactive checklist is complete and the Phase 6.6 human review gate is accepted. Phase 6.7 remains closed pending explicit owner approval for the external HMRC process.
