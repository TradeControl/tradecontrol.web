# Security incident and personal-data-breach response

**Owner:** Trade Control Ltd responsible individual

**Public reporting contact:** office@tradecontrol.co.uk

**Applies to:** the Trade Control managed service and statutory integrations
**Review date:** before commercial launch and after every material incident or exercise

This is the public, non-secret version of Trade Control Ltd's operational procedure. Private telephone numbers, protected identifiers, credentials, incident records and regulator correspondence are retained outside the public repository.

## Classification

- A **software defect** is a functional failure with no reasonable indication that confidentiality, integrity, availability or an authority operation has been compromised. It is handled through normal defect management unless later evidence changes the classification.
- A **security incident** is an event that may compromise a tenant, credential, service boundary, statutory operation, or the confidentiality, integrity or availability of information. It is recorded and triaged immediately.
- A **personal-data breach** is a security breach leading to accidental or unlawful destruction, loss, alteration, unauthorised disclosure of, or access to, personal information. A personal-data breach is assessed for notification even if it arose from human error rather than a hostile attack.

One event may fall into more than one category. Classification is reviewed as facts emerge; uncertainty is not used to delay containment or a time-critical notification.

## Receive, record and triage

1. Reports received through the public contact, service monitoring, a customer, a supplier or a contributor are time-stamped in the private incident record.
2. The responsible individual becomes Incident Lead and records what is known, the reporter, affected service and tenants, possible authority operations, information categories, time discovered, and all decisions. If the responsible individual is unavailable, the service remains fail-closed and no statutory submission is enabled merely to restore availability.
3. The Incident Lead determines immediate severity from the possible effect on people, customer businesses, statutory filings and service boundaries. A suspected cross-tenant disclosure, credential compromise or unauthorised filing is treated as critical until disproved.
4. The record contains references or digests rather than copying secrets and unredacted personal information into ordinary logs or public issue trackers.

## Containment and evidence

Containment is selected to avoid destroying evidence. Available actions include disabling the affected tenant, disabling the affected authority or all submission authority, disconnecting or revoking an authority grant, withdrawing a compromised service identity, restricting network access, rotating affected secrets or keys, and preserving a database or storage snapshot through approved protected facilities.

The tenant and authority kill switches remain independent. A service restart, incident investigation or availability pressure must not silently re-enable a disabled submission gate.

Original logs, snapshots and authority responses remain in restricted storage. The incident record identifies their location, collection time, custodian and digest where practical. Public reports and ordinary support tickets use redacted facts only.

## Notification decisions

### HMRC

Under HMRC's current production-access guidance, an issue concerning the security of personal or customer data is reported immediately by logging a Developer Hub support ticket. A breach contact name and telephone number and the available facts are supplied; full details are provided within 72 hours and supplemented as the investigation develops. Protected tax identifiers or credentials are supplied only through the channel HMRC designates for them.

### Customer controller

Where Trade Control Ltd processes personal information for a customer business, it informs that controller without undue delay after becoming aware of a personal-data breach. The notice gives the available facts needed for the controller's own assessment and is updated as facts develop. The customer controller normally makes the regulatory and data-subject notification decisions for that processing; Trade Control assists and does not assume that role without a documented legal reason.

### ICO and affected people

Where Trade Control Ltd is the controller, the Incident Lead assesses both likelihood and severity of risk to people's rights and freedoms. A notifiable breach is reported to the ICO without undue delay and, where feasible, within 72 hours of awareness. If the breach is unlikely to result in risk, the reason for not notifying is recorded. A high risk requires affected people to be informed without undue delay, using clear advice that helps them limit harm. Information may be supplied to the ICO in phases when the investigation cannot be completed within 72 hours.

Not every software defect, security incident or personal-data breach has the same notification outcome. Every personal-data breach is recorded, including the risk assessment and the reason for each notification decision.

## Recovery and closure

Recovery requires evidence that the affected boundary is contained, credentials or grants are safe or replaced, tenant isolation remains intact, statutory evidence is reconciled, submission authority is deliberately set to the approved state, and monitoring is operating. Restoring service does not by itself close the incident.

Closure records the final scope and timeline, cause, affected information and people, notifications, recovery evidence, corrective actions, owners and due dates. The Incident Lead performs a follow-up review, updates the procedure and tests where necessary, and confirms that protected evidence is retained or deleted under the approved schedule.

## Exercise evidence

The procedure was exercised on 28 October 2026 using a synthetic cross-tenant disclosure scenario. No real customer information was used and no regulator or customer notification was sent. The protected tabletop record demonstrates classification, independent tenant and authority containment, evidence handling, processor/controller notification paths and closure criteria.

## Sources

- [HMRC Developer Hub terms of use](https://developer.service.hmrc.gov.uk/api-documentation/docs/terms-of-use)
- [ICO personal data breaches guide](https://ico.org.uk/for-organisations/report-a-breach/personal-data-breach/personal-data-breaches-a-guide/)
- [ICO controller–processor contracts guidance](https://ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/accountability-and-governance/contracts-and-liabilities-between-controllers-and-processors-multi/what-needs-to-be-included-in-the-contract/)

---

## Document status

This procedure was adopted on 30 October 2026 by Trade Control Ltd's responsible individual for the pre-launch HMRC production-access process. The private incident telephone number is retained outside the public repository and will be supplied through the appropriate private authority channel when required. Adoption creates the operational process; it does not state that a real incident has occurred or that a notification has been made.
