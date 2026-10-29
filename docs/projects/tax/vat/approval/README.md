# VAT production-access approval evidence

This folder contains the public, non-secret evidence package for Trade Control's HMRC VAT (MTD) production-access preparation.

This package describes a pre-launch sandbox accreditation environment, not a live customer service. It uses the intended shared multi-tenant Azure topology so that OAuth, tenant isolation, fraud headers, restart recovery and filing behaviour are representative of the planned production deployment. B3/B4 proved the corrected brokered OAuth and filing boundaries. Once B5/B6 were independently verified as valid VAT-registered Business Nodes, they each completed a clean HMRC VAT journey: fresh organisation OAuth, immutable preparation and approval, a live fail-closed submission gate, shared Linux App Service restart recovery, one explicitly authorised sandbox submission, HTTP `201`, obligation reconciliation, exact nine-box readback and durable filing history on 27 October 2026. Their earlier first-registration bootstrap-seeding defect belongs to Business Node commissioning assurance, not to the subsequent HMRC journey. Nothing in this folder claims production access, production credentials, acceptance of HMRC terms, a production filing, a live public service or compatible-software listing.

## Evidence package

- [Production-access evidence dossier](vat-production-access-evidence.md) — scope, evidence rules, completed HMRC evidence, and the separate Business Node production-launch gate.
- [Compliance accessibility review](accessibility-review.md) — WCAG 2.2 benchmark, repeatable checks, detected contrast correction and explicit remaining evidence.
- [Security incident response](security-incident-response.md) — public notification, containment and evidence-handling procedure supported by a private synthetic tabletop.
- [Customer data exit control](customer-data-exit.md) — deployed authenticated business-data export, exclusions, independent B5/B6 exercise and owner acceptance.
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

The first-registration bootstrap correction is reviewed, committed and published in `tradecontrol.web` `f169e79`. Deploy that correction and prove seamless Business Node delivery from an empty database before production launch. B5/B6 already supply the clean HMRC VAT evidence from independently verified commissioned tenants. Their VAT filings need be repeated only if HMRC requests newer logs, selected API retesting or evidence from a materially changed filing topology. Production deployment remains gated on both evidence streams passing independently.
