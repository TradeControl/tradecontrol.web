# Corporation Tax

## Current position

Corporation Tax resumes at Phase 5.10. The earlier multi-tenant hosting pause is cleared, but the CT
family gate remains closed, all artifacts remain preview-only and production submission is disabled.
The authoritative contract, Objective 3 population, validated service artifact and offline
Transaction Engine conversation already exist. CT-A has confirmed that the selected integration-test
tenant is technically healthy but its retained accounting period is not a truthful CT2025 scenario.
The immediate work is to recommission that disposable test dataset for the approved current period,
complete the artifact rebaseline, then return to the send-disabled ETS preflight and one separately
authorised test conversation.

HMRC recognition is the programme goal, not the current implementation state. Phase 5.10 ETS is
necessary but insufficient: the CT preparation, review, declaration, approval and durable status
journey must also be integrated into the multi-tenant TCWeb product before applying.

## Governing and working documents

- [Work Plan 3](tax-hub-workplan-3.md) records the original CT contract-model programme.
- [Recognition roadmap](ct-accreditation-roadmap.md) is the current status and execution plan.
- [Work Plan 5](../../platform/tax-hub-workplan-5.md), phases 5.7–5.10, governs current service-artifact and transport work.
- [Phase 5.8 assurance](phase-5.8-corporation-tax-service-artifact-assurance.md)
- [Phase 5.9 Transaction Engine fixture](phase-5.9-transaction-engine-fixture.md)
- [Phase 5.10 External Test Service implementation](phase-5.10-ct-external-test-local.md)
- [Shared company statutory material](../shared/)

Work Plan 3 is not an unstarted implementation queue; later plans completed and narrowed it to the
supported profile. Environment-specific tenant availability, database identities and commissioning
instructions belong in protected operational records, not this public product documentation. Do not
infer production readiness or HMRC recognition from local validation.
