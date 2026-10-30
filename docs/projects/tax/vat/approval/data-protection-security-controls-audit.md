# Data-protection security-controls audit

[Document status](#document-status)

Trade Control has completed an internally managed audit of the technical and organisational security controls supporting the pre-launch HMRC VAT accreditation environment. The audit was informed by ICO security outcomes and was kept distinct from the application penetration test.

The reviewed environment uses synthetic tenants and HMRC sandbox identities on the intended shared Azure topology. It is not processing live customer VAT data, and this audit is not represented as legal certification or approval to begin live customer processing.

## Control areas reviewed

| Area | Evidence-based position |
|---|---|
| Tenant routing and data separation | Canonical host resolution selects one tenant boundary before tenant content or data access. B5/B6 use separate databases, Identity stores, file namespaces and VAT histories. Live reciprocal inspection and negative boundary tests found no crossover. |
| Authentication and authorisation | Confirmed accounts, role controls, secure cookies, lockout and stronger new-password requirements are configured. Administrator-only functions, including Customer Exit, were exercised at the intended role boundary. |
| Request and browser security | Global antiforgery validation, secure cookie attributes, HTTPS/HSTS, restrictive response headers, pinned external resources and an explicit content-source policy are deployed. The disclosed legacy inline-content limitation remains under review as defence-in-depth. |
| OAuth and authority credentials | A dedicated broker applies state, correlation, PKCE, return-host, expiry and single-use controls. Protected credentials and tokens remain outside tenant databases and public evidence. Invalid requests failed closed. |
| Secrets and service identity | Azure managed identities and protected configuration separate runtime access from source code and tenant data. Public documentation contains no credentials, protected identifiers or raw fraud-header values. |
| Statutory workflow integrity | Preparation, declaration, approval, dispatch and readback remain distinct. Approval binds the immutable nine-box return, while tenant-scoped kill switches and durable reconciliation prevent an ambiguous or repeated filing path. |
| Availability and recovery | Liveness/readiness checks, monitored platform metrics, restart recovery and retained VAT workflow evidence were exercised. The shared application recovered tenant grants, approvals and filing histories without cross-tenant repair. |
| Vulnerability and dependency management | Runtime dependency checks reported no known vulnerable .NET package in the reviewed baseline. Deployed jQuery 3.7.1 closed the former core-library finding. The separately detected legacy validation library was replaced with self-hosted `jquery-validation` 1.22.1; identity behaviour and the affected public assets were retested without the former vulnerable-library or external-origin alert. |
| Incident response and evidence | A public incident-response procedure, private incident record and synthetic cross-tenant tabletop define containment, evidence preservation, notification assessment and fail-closed operation for the single-operator company. |
| Data portability and exit | An authenticated tenant administrator can obtain a documented machine-readable archive. B5/B6 exercises verified tenant separation, protected-field exclusions, attributable audit and direct no-cache delivery. |

## Open operational decision

Application telemetry, health checks, Azure platform metrics and alerts are present. Comprehensive export of every Azure resource diagnostic category has not been enabled for the accreditation environment.

That is a live-production design decision rather than evidence of an application exploit. Before live customer processing, Trade Control will select proportionate log categories, destinations, access controls and retention periods, taking account of incident usefulness, personal data, operational responsibility and Azure cost. Until then, the company does not claim complete resource-level log export or indefinite security-log retention.

The application penetration test separately records a legacy content-policy limitation permitting inline scripts and styles. No exploit was demonstrated. It remains an acknowledged hardening limitation rather than an unreported compliance claim. Both browser-library dependency findings were remediated and closed by deployed retest.

## Conclusion

The reviewed controls provide a reasonable security basis for pre-launch HMRC application review. No high or critical application finding or tenant-isolation failure remains open. The accepted content-policy limitation and the production diagnostic-log decision remain explicit; neither is presented as full elimination or complete live-production monitoring.

This conclusion is limited to the reviewed application and accreditation topology. Commercial launch remains prohibited until HMRC grants the necessary accreditation and the separate customer-processing gates are completed, including final contractual documents, verified processor/subprocessor arrangements, implemented retention/deletion procedures, production monitoring decisions and final operational adoption.

The audit does not request credentials, accept terms, authorise production filing or certify general UK GDPR compliance.

## Related public evidence

- [Software penetration-test summary](penetration-test-summary.md)
- [Security incident-response procedure](security-incident-response.md)
- [Customer data exit control](customer-data-exit.md)
- [ICO security outcomes](https://cy.ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/security/a-guide-to-data-security/security-outcomes/)

---

## Document status

Public, non-secret audit summary prepared for HMRC production-credentials review. Detailed configuration, raw scanner output, protected identifiers and operational evidence remain in controlled local storage. Reviewed application security baseline: the public source revision containing this document, deployed for review on 30 October 2026; Tax Hub baseline: `6bd0702`.
