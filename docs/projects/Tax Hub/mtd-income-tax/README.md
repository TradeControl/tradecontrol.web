# MTD Income Tax

## Scope

Trade Control supports sole-trader filing through Making Tax Digital for Income Tax only. Legacy SA100/SA103F XML and EOPS are not supported filing paths. Historical documents retain those terms where they explain earlier decisions.

## What governs

1. Current HMRC MTD Income Tax specifications govern external statutory meaning and API behaviour.
2. The [Tax Hub programme specification](../programme/tax-hub-spec-programme.md) governs programme boundaries.
3. The [SQL Node implementation specification](self-assessment-sql-node-spec.md) governs the accounting-to-statutory projection.
4. [Work Plan 4](../platform/tax-hub-workplan-4.md), especially the MTD Income Tax cumulative vertical slice and Objective 3 completion gate, records accepted preparation work.
5. [Work Plan 5](../platform/tax-hub-workplan-5.md), especially phases 5.15–5.16, governs planned transport and handoff work.

Supporting references are [HMRC contracts](sole-trader-contracts.md), [field sets](sole-trader-field-sets.md) and [capital treatment](sole-trader-capital.md). The retained [SA findings](sa-findings.md) and [SA change log](sa-change-log.md) are historical evidence.

## Current position

Implemented and accepted:

- the MTD-only product boundary and retirement of the live legacy SA/EOPS direction;
- the Objective 2 MIN/STD cumulative accounting projection and mapping foundation;
- the Objective 3 self-employment cumulative PUT preparation for both profiles;
- the income-and-expenditure obligations request description; and
- immutable request bytes, validation, safe preview and handoff evidence.

Still deferred or incomplete:

- twelve body-bearing operations remain deferred and one preview operation remains unsupported in the accepted Objective 3 coverage record;
- annual adjustments, losses, calculation and final-declaration journeys lack a complete reviewed product workflow;
- MTD Income Tax OAuth scopes and live transport remain disabled pending Work Plan 5 Phase 5.15; and
- Objective 5 hosted review, approval, submission, status and history are not authorised for this stream.

## Restrictions and next gate

- Do not revive SA100/SA103F XML or EOPS as a filing stage.
- Do not infer unsupported statutory values or treat accounting data alone as authority for filing choices.
- Calendar-quarter preparation remains blocked until its dates come from an HMRC obligation-backed workflow.
- An endpoint's presence in the contract catalogue does not authorise population or sending.
- No production access, live request or product enablement follows from this documentation migration.

There is no authorised MTD Income Tax implementation phase beyond the accepted foundation. The next programme decision is whether to resume Work Plan 5 Phase 5.15. If authorised separately, its first bounded activity is the contract-and-customer-journey reconciliation described there.
