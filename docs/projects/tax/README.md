# Tax documentation

This project owns Trade Control's tax projections, statutory contracts, authority workflows, evidence seals and tax-calculation continuity. The Tax Hub is the user interface over those capabilities; it is not the parent architecture.

Start with the [programme specification](programme/tax-hub-spec-programme.md). Current HMRC or Companies House specifications remain authoritative for their external contracts.

## Current position

| Area | Current position | Governing and working documents | Next action or gate |
|---|---|---|---|
| Seal and tax continuity | Preparation-only VAT evidence exists. Statement opening/closing balance continuity and the durable tax-balance checkpoint remain to be implemented. | [Seal and calculation-continuity index](seal/README.md) | Complete Stage 1 before finalising VAT or Corporation Tax accreditation evidence. Stage 2 real-time optimisation remains deferred to MIS Mode. |
| MTD Income Tax | Objective 2 projection and the supported Objective 3 cumulative/obligations slice are implemented. Objective 4 phases 5.15–5.16 remain deferred. | [MTD Income Tax index](mtd-income-tax/README.md) | Human decision to resume Phase 5.15, beginning with its contract-and-journey reconciliation. No implementation is authorised by this index. |
| VAT | Sandbox OAuth and obligation retrieval operate through the hosted broker; production access, live filing and compatible-software status have not been granted. | [VAT index](vat/README.md) | Complete Stage 1 seal/statement continuity and the remaining production-assurance evidence, then resume Phase 6.7. |
| Corporation Tax | Local service-artifact and External Test Service work exists, but the family gate remains closed. HMRC recognition is optional and follows ETS/product maturity. | [Corporation Tax index](company/corporation-tax/README.md) | Complete the shared Stage 1 seal/statement continuity work, then resume Phase 5.10 with an explicitly authorised ETS attempt; do not enable production. |
| Companies House | The unaudited filleted FRS 105 micro-entity submission scope is approved and its live package reference has been created. Production/live dispatch remains disabled. | [Companies House index](company/companies-house/README.md) | Replace the test presenter identity with the protected production identity in the production-shaped environment, revalidate configuration, and authorise the first controlled filing. The live service must not be used for testing. |

## Shared material

- [Programme specification and documentation proposal](programme/)
- [Platform architecture and cross-cutting work](platform/README.md)
- [Statutory seal and tax calculation continuity](seal/README.md)
- [Shared company statutory material](company/README.md)
- [Tax Hub UI hosting decision](hub/tax-hub-objective-5-hosting-decision.md)
- [Programme findings](findings.md) and [change log](change-log.md)
- [Legacy programme documents](archive/legacy/) and [historical session briefs](archive/sessions/)

Historical documents and session briefs record the instruction or evidence available at the time. They are not current authority where a later reviewed specification or work plan supersedes them.
