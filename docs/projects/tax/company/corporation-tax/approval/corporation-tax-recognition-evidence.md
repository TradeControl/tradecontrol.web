# Corporation Tax recognition evidence dossier

## Status and purpose

**Preparation status — 27 November 2026:** pre-application evidence assembly. This dossier does not
claim HMRC recognition, compatible-software listing, production credentials or a production filing.
The Corporation Tax production family gate remains closed.

This document provides a public index to the evidence supporting Trade Control's intended HMRC
Corporation Tax software-recognition application. Exact credentialed requests, responses, tax
identifiers, tenant identities and correspondence remain protected.

## Proposed recognition claim

Trade Control intends to ask HMRC to assess the smallest commercially useful company-filing scope:

| Area | Proposed scope |
| --- | --- |
| Filer | UK private company filing for itself; no agent filing in the initial claim. |
| Accounts | Ordinary unaudited FRS 105 micro-entity full accounts generated within Trade Control. |
| Computation | Integrated iXBRL Corporation Tax computation using the applicable accepted taxonomy. |
| Return | CT600 Version 3 using the RIM release implemented by the target HMRC service. |
| Periods | One or two Corporation Tax periods derived from one accounts period. |
| Supplementary pages | CT600A only when the supported participator-loan conditions apply. |
| Journey | Accepted Companies House accounts followed by a separately reviewed and approved Company Tax Return. |

The claim excludes agent filing, charities, insurance, tonnage tax, ring-fence trades, unsupported
R&D schedules, group-relief scenarios and specialist supplementary pages. An available RIM wire type
is not treated as a supported business scenario.

## Product journey

The deployed development journey carries an authorised Manager or Administrator through:

1. selection of the accepted Companies House accounts for the accounting period;
2. recalculation from the current Trade Control transactions and reconciliation to those accounts;
3. review of the annual Corporation Tax calculation, losses and filing-specific inputs;
4. preparation of exact full accounts, computation and CT600 service documents;
5. review of the documents and their SHA-256 identities;
6. an attributable legal declaration and immutable approval that does not submit;
7. a separate, explicitly confirmed, exactly-once submission action;
8. durable background acknowledgement, polling, terminal outcome and receipt retention; and
9. dashboard readback, safe diagnostics and support references.

The published [Company Corporation Tax User Guide](https://tradecontrol.github.io/tax/tax-hub-business-tax-company/)
documents this journey with development-mode screenshots and states **Application Pending**. The
fake-authority outcome proves the product workflow without representing an HMRC result. Genuine HMRC
test-service evidence is classified separately below.

## Statutory calculation and artifact baseline

The supported calculation is transaction-derived rather than a filing-time override. It preserves
the accounting-profit bridge, depreciation add-back, relevant distributions, loss use, taxable total
profits, augmented profits, marginal-relief basis, adjustments and Corporation Tax due. Provisional
rates may support labelled planning accruals but fail the filing gate.

The current validated artifact baseline is:

| Asset | Reviewed baseline |
| --- | --- |
| Corporation Tax computation | Corporation Tax computational 2025, official HMRC package version 1.0.0. |
| Accounts | FRC 2026 taxonomy suite 1.0.0 for the supported period/profile. |
| CT600 | RIM 1.994, subject to a fresh target-service check before every formal run. |
| Package | Full accounts, computation and CT600 service document retained as one immutable approved preparation. |

The authoritative packages and exact retained-company documents remain protected. Public provenance,
versions and safe digests are recorded in the linked Phase 5.8 assurance.

## Completed validation and HMRC test evidence

| Evidence boundary | Result | Classification |
| --- | --- | --- |
| Contract and population suites | The supported accounts, computation and CT600 contracts build and pass their deterministic test suites. | Local automated evidence. |
| Official CT2025 package in Arelle | The materialised computation passed with no validation findings. | Local validation using the authoritative package supplied by HMRC. |
| HMRC Local Test Service 8.3 | The protected service artifact passed schema, business-rule, IRmark and calculator validation with no findings. | Local HMRC LTS evidence. |
| Genuine HMRC TPVS | HTTP 200; messages `0000` and `077001`; no application findings. | Remote validation evidence, separately authorised. |
| Genuine HMRC External Test Service | Submit, acknowledgement, polling, terminal `BusinessResponse`, IRmark receipt and acknowledged deletion completed with zero returned errors. | HMRC official-test Transaction Engine evidence, separately authorised. |
| Deployed product journey | Preparation, declaration, approval, exactly-once dispatch orchestration, durable outcome, receipt download and dashboard readback completed against the visibly labelled fake authority. | Product/workflow evidence; not an HMRC submission. |

The TPVS, ETS and product-simulation results are intentionally not conflated. None is a production
filing or recognition decision.

## Safety and operational boundaries

The product applies the following recognition-relevant controls:

- tenant and filing-owner scope is resolved server-side;
- accepted accounts and current transactions must reconcile before preparation;
- stale source, Category Tree, identity or reviewed-input evidence invalidates preparation;
- approval binds the exact documents and their digests and remains separate from submission;
- one durable conversation owns the accounting period's permitted send;
- browser closure does not create a second send or stop eligible background processing;
- uncertain-send and support-required states prohibit blind resubmission;
- exact protected evidence is application-encrypted and digest-checked when retrieved;
- ordinary users do not receive the privileged diagnostic mapping surface; and
- production Corporation Tax submission remains disabled pending recognition and controlled activation.

Shared-host catalogue routing, separate tenant databases and negative isolation coverage support the
pre-launch multi-tenant assessment. Concrete infrastructure identities and operational access details
remain protected.

## Application gate and remaining work

Detailed scenario coverage and owner acceptance are maintained in the
[CT-E product test matrix](ct-e-product-test-matrix.md). This dossier records the application-level
gate without duplicating the matrix.

Before the owner sends a recognition request, record explicit acceptance of each item:

- [x] The proposed initial filing scope and exclusions are documented.
- [x] The authoritative CT2025, FRC 2026 and RIM 1.994 artifact baseline is pinned.
- [x] Arelle, LTS and genuine TPVS validation pass for the supported artifact.
- [x] One genuine ETS conversation proves the Transaction Engine protocol and receipt lifecycle.
- [x] The multi-tenant product journey provides review, immutable approval, exactly-once orchestration and durable outcomes.
- [x] The public Company Corporation Tax User Guide is published as **Application Pending**.
- [ ] Complete and accept the CT-E evidence matrix, including negative, recovery, accessibility and isolation cases required for the application scope.
- [ ] Recheck the RIM, taxonomy and HMRC test-service versions immediately before the formal request.
- [ ] Confirm the current HMRC recognition contact route and the evidence HMRC wants with the initial request.
- [ ] Obtain explicit owner authority to send the application request.

Assigned HMRC recognition scenarios cannot be completed before HMRC supplies them. When received,
record each scenario, exact approved inputs, artifact identities, test-service result, correction cycle
and HMRC disposition without widening the supported scope silently.

## Proposed initial contact

The initial contact should state, in the first person, that Trade Control has completed its own testing
and HMRC ETS testing for the narrow scope above and is ready to begin the current Corporation Tax
recognition process. It should ask HMRC to confirm the applicable applicant instructions, scenario
pack, submission route and required supporting material. It must not claim recognition, production
access or compatible-software listing.

The actual email, Vendor ID and correspondence belong in the protected `.local` application record.

## Governing references

- [CT-E product test matrix](ct-e-product-test-matrix.md)
- [Corporation Tax recognition roadmap](../ct-recognition-roadmap.md)
- [Phase 5.8 service-artifact assurance](../phase-5.8-corporation-tax-service-artifact-assurance.md)
- [Phase 5.10 External Test Service evidence](../phase-5.10-ct-external-test-local.md)
- [Marginal-relief implementation and evidence](../ct-marginal-relief.md)
- [Tax Hub platform transport](../../../platform/hmrc-transport.md)
- [HMRC Corporation Tax software developer support](https://www.gov.uk/government/collections/corporation-tax-online-support-for-software-developers)
- [HMRC CT600 RIM artifacts](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-ct600-rim-artefacts)

