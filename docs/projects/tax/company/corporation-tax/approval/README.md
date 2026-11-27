# Corporation Tax recognition evidence

This folder contains the public, non-secret evidence package being assembled for Trade Control's
HMRC Corporation Tax software-recognition application.

> **Application status — 27 November 2026:** preparation is in progress. Trade Control has not yet
> submitted a recognition application, HMRC has not assigned recognition scenarios, and Corporation
> Tax production filing remains disabled.

The proposed initial recognition scope is deliberately narrow:

- a UK private company filing for itself rather than acting as an agent;
- ordinary unaudited FRS 105 micro-entity accounts;
- integrated full accounts, Corporation Tax computation and CT600 service document;
- one or two Corporation Tax periods derived from one accounts period; and
- CT600A only where the supported participator-loan conditions apply.

Specialist regimes and unsupported supplementary pages fail closed and are not recognition claims.

## Evidence package

- [Recognition evidence dossier](corporation-tax-recognition-evidence.md) — proposed scope, completed
  validation, product-journey evidence, open gates and the evidence expected for assigned HMRC
  scenarios.
- [CT-E product test matrix](ct-e-product-test-matrix.md) — the living scenario-by-scenario record of
  completed evidence, partial coverage, remaining execution and owner acceptance.
- [Recognition roadmap](../ct-recognition-roadmap.md) — governing implementation and application
  sequence.
- [Company Corporation Tax User Guide](https://tradecontrol.github.io/tax/tax-hub-business-tax-company/)
  — the public, application-pending customer journey.
- [Phase 5.8 service-artifact assurance](../phase-5.8-corporation-tax-service-artifact-assurance.md)
  — taxonomy, RIM, Arelle, LTS and TPVS assurance.
- [Phase 5.10 External Test Service evidence](../phase-5.10-ct-external-test-local.md) — the separately
  authorised genuine HMRC Transaction Engine exercise.
- [Marginal-relief work programme](../ct-marginal-relief.md) — the transaction-derived annual
  calculation, statutory kernel and validation boundary.

## Public/protected boundary

This folder may contain safe product descriptions, evidence classifications, dates, versioned
artifact identifiers, redacted findings and SHA-256 digests suitable for public review.

The following remain under the git-ignored `.local` evidence area and must not be copied here:

- Vendor ID, Government Gateway and test-user credentials;
- UTRs, company authentication values and other protected identifiers;
- complete credentialed GovTalk requests or unrestricted authority responses;
- tenant, database, host, network and protected-storage identifiers;
- unredacted HMRC correspondence; and
- operational exports or screenshots that have not passed a public-disclosure review.

The public package records what was proved. Protected evidence retains the exact material required
for a controlled HMRC follow-up, demonstration or assigned-scenario submission.

