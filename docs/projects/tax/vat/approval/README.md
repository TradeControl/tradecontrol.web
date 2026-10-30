# VAT production-access approval evidence

This folder contains the public, non-secret evidence package for Trade Control's HMRC VAT (MTD) production-access preparation.

This package describes a pre-launch sandbox accreditation environment, not a live customer service. It uses the intended shared multi-tenant Azure topology so that OAuth, tenant isolation, fraud headers, restart recovery and filing behaviour are representative of the planned production deployment. The synthetic business labels used below are explained in the topology section. B3/B4 proved the corrected brokered OAuth and filing boundaries. Once B5/B6 were independently verified as valid VAT-registered Business Nodes, they each completed a clean HMRC VAT journey: fresh organisation OAuth, immutable preparation and approval, a live fail-closed submission gate, shared Linux App Service restart recovery, one explicitly authorised sandbox submission, HTTP `201`, obligation reconciliation, exact nine-box readback and durable filing history on 27 October 2026. Their earlier first-registration bootstrap-seeding defect belongs to Business Node commissioning assurance, not to the subsequent HMRC journey. Nothing in this folder claims production access, production credentials, acceptance of HMRC terms, a production filing, a live public service or compatible-software listing.

## Topology and synthetic business labels

The accreditation environment runs one shared Trade Control web application and one shared HMRC OAuth broker on Linux-based Azure App Service. Each synthetic business is reached through its own approved hostname and has a separate Azure SQL database, file namespace, users and roles. The hostname is resolved to the business boundary before its database is selected. This is the multi-tenant pattern intended for production; `tradecontrol.co.uk` remains a sandbox environment while accreditation is in progress.

`B3`, `B4`, `B5` and `B6` are concise evidence aliases for four separately instantiated synthetic businesses, not HMRC terminology, customer names or protected identifiers. B3 and B5 model VAT-registered Standard Micro Entities; B4 and B6 model VAT-registered sole traders. Each used a fresh HMRC organisation sandbox identity.

B3/B4 were the first paired VAT exercise. They supplied corrective discovery evidence while the brokered OAuth, tenant-context and filing boundaries were being completed. B5/B6 repeated the same two business forms after independent commissioning checks and then completed the HMRC journey without corrective code or tenant-data adjustment. The package therefore retains B3/B4 as provenance and uses B5/B6 as the clean, repeatable sandbox-filing evidence.

## Evidence package

- [Production-access evidence dossier](vat-production-access-evidence.md) — scope, evidence rules, completed HMRC evidence, and the separate Business Node production-launch gate.
- [Compliance accessibility review](accessibility-review.md) — WCAG 2.2 benchmark, repeatable checks, deployed corrections, manual journey evidence and explicit limitations.
- [Security incident response](security-incident-response.md) — public notification, containment and evidence-handling procedure supported by a private synthetic tabletop.
- [Customer data exit control](customer-data-exit.md) — deployed authenticated business-data export, exclusions, independent B5/B6 exercise and owner acceptance.
- [Software penetration-test summary](penetration-test-summary.md) — bounded OWASP/ZAP assessment, remediation, retest, exclusions and disclosed residual risk.
- [Data-protection security-controls audit](data-protection-security-controls-audit.md) — ICO-informed review of application and operational controls, with the production logging decision kept explicit.
- [Legal submission documents](legal/README.md) — pre-launch Privacy Statement and managed-hosting Terms and Conditions, with topology context, provenance and publication gates.
- [Phase 6.7 approval record](../phase-6.7-hmrc-production-approval.md) — governing status, external gates and owner controls.
- [Phase 6.6 readiness evidence](../phase-6.6-hmrc-approval-readiness.md) — preserved single-tenant technical baseline.

## Public/protected boundary

This folder may contain only safe aliases, dates, software/deployment revisions, evidence classifications, HTTP result classes, redacted findings and SHA-256 digests that are suitable for public review.

The following remain under the git-ignored `.local` area and must not be copied here:

- HMRC sandbox usernames, passwords and organisation-test-user details;
- VAT registration numbers or other tax identifiers;
- OAuth client identifiers, secrets, authorisation codes, access tokens and refresh tokens;
- raw fraud-prevention-header values or network identifiers;
- complete request or response bodies containing protected identifiers;
- unrestricted HMRC receipt, correlation, support or bundle references; and
- screenshots or operational exports that have not passed redaction review.

Public evidence records whether a required result was proved. Protected evidence retains the exact material needed for controlled audit or HMRC follow-up.

## Current gate

The public package now contains the completed application-scope accessibility, incident-response, customer-exit, penetration-test and security-controls evidence. The authenticated modern-client assessment disclosed the remaining inline-content-policy limitation and two browser-library dependency findings. Deployed jQuery 3.7.1 closed the core finding. The separate legacy validation dependency was then replaced with self-hosted version 1.22.1 and closed by browser-behaviour verification and bounded passive retest. No high or critical application finding remains open. The production diagnostic-log decision remains visible rather than hidden.

The package is ready for human review, publication and consistency checking before the time-limited Developer Hub application is resumed with the owner present. Accreditation remains a prerequisite to commercial launch. The separate live-customer gates—final contractual documents, retention/deletion implementation, production monitoring decisions and operational adoption—remain open and are not misrepresented as features of the sandbox environment.

B5/B6 already supply the clean HMRC VAT evidence from independently verified commissioned tenants. Their VAT filings need be repeated only if HMRC requests newer logs, selected API retesting or evidence from a materially changed filing topology. Nothing here requests production credentials, accepts HMRC terms or submits the application.
