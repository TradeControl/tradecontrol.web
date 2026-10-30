# Software penetration-test summary

[Document status](#document-status)

Trade Control has completed a bounded security assessment of the pre-launch Azure Integration environment used for HMRC VAT accreditation. The environment uses the intended shared multi-tenant application topology, but contains synthetic businesses and HMRC sandbox identities rather than live customers or production filing authority.

The initial assessment found no critical or high-severity vulnerability, tenant crossover, unauthorised statutory-state change, server HTTP 5xx response or persistent service-health failure. The first bounded remediation cycle corrected and retested the actionable configuration findings. A subsequent authenticated modern-client assessment identified vulnerable jQuery core 3.4.1; its deployed 3.7.1 remediation removed that alert. The remediation retest then identified a separate high-severity legacy validation-library dependency on the login path. That dependency was replaced with self-hosted version 1.22.1, deployed and closed by browser-behaviour verification and a clean affected-path passive retest.

## Scope

The assessment covered:

- the shared Trade Control web application used by the B5 and B6 synthetic VAT tenants;
- server-side tenant resolution and separation of tenant databases, users and retained VAT workflow state;
- authentication, session, request-integrity and host-boundary controls;
- the dedicated HMRC OAuth broker, including invalid-input failure and abuse controls;
- the VAT preparation, approval, submission-gate, recovery and filing-history boundaries;
- public security headers, cookies and externally loaded browser resources; and
- relevant application dependencies and Azure security configuration evidence.

B5 and B6 are evidence aliases, not HMRC terminology or customer identities. They represent a VAT-registered Standard Micro Entity and a VAT-registered sole trader on separate tenant boundaries within the same application.

## Method

Trade Control conducted the assessment on 30 October 2026 using recognised OWASP methods, OWASP ZAP 2.17.0, source and configuration review, dependency analysis, repeatable boundary tests and manually reasoned tenant-isolation checks.

The work included explicit passive mapping, separate low-strength unauthenticated active-rule runs, invalid-input OAuth checks, read-only authenticated inspection and post-test health and state reconciliation. It also included one bounded authenticated ZAP Client Spider phase against the B5 VAT workspace so that Blazor-rendered component state and browser-loaded dependencies were observed through a real browser. Original results were retained before remediation, and the relevant checks were repeated after correction.

This was a genuine internally managed penetration-testing exercise, not an external consultancy engagement or independent certification. Automated scanning was treated as one source of evidence rather than proof by itself.

## Safety and exclusions

Testing was confined to the named synthetic accreditation tenants and broker. Request rates, concurrency, duration and stop conditions were fixed in advance. No denial-of-service testing, uncontrolled fuzzing, AJAX spidering, HMRC production traffic, production credential use, live customer data, Azure control-plane attack or statutory write was authorised. The Client Spider was browser-based passive analysis, not a state-changing active scan.

The assessment was deliberately bounded. It does not claim that every authenticated endpoint received unrestricted session replay or that Microsoft-managed infrastructure was penetration tested.

## Findings and retest

| Finding | Original classification | Disposition |
|---|---|---|
| Browser content policy did not define a complete restrictive source allow-list | Medium defence-in-depth | Partially remediated. Explicit source, framing, form, object, connection, manifest and worker restrictions are deployed. Legacy inline script and style support remains disclosed below. |
| An externally hosted icon stylesheet lacked Subresource Integrity | Medium supply-chain hardening | Corrected with exact version pinning and verified SRI. The alert was absent on retest. |
| The anonymous antiforgery cookie lacked an explicit `Secure` flag | Low configuration weakness | Corrected to `Secure`, `HttpOnly` and strict same-site handling. The alert was absent on retest. |
| Bundled jQuery 3.4.1 is affected by CVE-2020-11022 and CVE-2020-11023 | Medium dependency risk | Closed. Version 3.7.1 is deployed and the former alert is absent on authenticated retest. |
| The login path loaded legacy `jquery-validation` 1.17.0 with known security advisories | High dependency risk | Closed. Self-hosted 1.22.1 is deployed; ordinary required-field validation passed; the bounded passive retest reported no vulnerable-validation-library, former external-origin or high alert. |

Supporting hardening also removed public server banners, strengthened new-password policy, refreshed development-tool dependencies, moved tenant static content behind tenant resolution, rejected unknown application hosts and added bounded OAuth broker throttling.

The first post-remediation passive retest recorded no high or low alert. Its eight medium instances were the known use of `unsafe-inline` for legacy script and style compatibility; the remaining observations were informational.

The first authenticated Client Spider completed after 87 proxied messages. It produced no high or low finding, service-health failure or VAT-state change. Its medium observations comprised the already disclosed inline content-policy instances and the former jQuery core dependency.

The deployed jQuery-core retest removed that alert but exposed the separate validation-library finding above. ZAP also generated two local HTTP 504 responses when its exact 20-second read limit expired on the server-prerendered VAT workspace. The protected session showed that every other captured request completed, with the next slowest taking 2.3 seconds. Azure recorded zero corresponding server HTTP 5xx responses, all final health probes passed and the VAT submission gates remained disabled. The safety rules were applied literally: the authenticated crawl was stopped without extending the timeout or repeating the VAT request. The event is retained as an operational tool-timeout observation, not represented as an application 504 or a security defect.

The validation correction was then tested independently without reopening the stopped VAT crawl. The public login page and its two self-hosted validation assets returned HTTP 200 in three sequential requests. Empty-form browser validation exposed both required-field messages, and passive ZAP analysis reported zero high alert, zero vulnerable-validation-library alert and zero alert for the former external validation origin. The six remaining passive records were the disclosed inline script/style policy allowances and four informational observations. B5, B6 and broker health was 6/6 before and after.

## Residual position

The current content policy still permits legacy inline scripts and styles. Removing that permission would require a broader nonce/hash and user-interface refactor. No injection exploit or policy bypass was demonstrated, and the application retains its normal output encoding, request-integrity and source restrictions. Trade Control accepts this as a visible defence-in-depth limitation for the accreditation submission rather than presenting it as fully eliminated.

The assessment supports the bounded security conclusion needed for pre-launch HMRC application review: no high or critical application finding remains open, and no tenant-isolation or statutory-workflow defect was demonstrated. The accepted inline-content-policy limitation remains visible, and comprehensive Azure diagnostic export remains a live-production monitoring and retention decision rather than a concealed accreditation-environment claim. The assessment is not a guarantee that the software is free from vulnerabilities, and relevant testing must be repeated when material identity, tenant-routing, OAuth or statutory-workflow boundaries change.

Accreditation remains a prerequisite to commercial launch. This assessment does not request production credentials, accept HMRC terms, authorise a production filing or represent the service as available to customers.

## Related public evidence

- [VAT production-access evidence dossier](vat-production-access-evidence.md)
- [Data-protection security-controls audit](data-protection-security-controls-audit.md)
- [OWASP Web Security Testing Guide 4.2](https://wstg.owasp.org/v4.2/)
- [OWASP Application Security Verification Standard 5.0.0](https://asvs.owasp.org/)
- [OWASP ZAP](https://www.zaproxy.org/)

---

## Document status

Public, non-secret summary prepared for HMRC production-credentials review. Assessment evidence is retained privately with protected identifiers and operational details excluded. Assessed application security baseline: the public source revision containing this document, deployed for review on 30 October 2026; Tax Hub baseline: `6bd0702`.
