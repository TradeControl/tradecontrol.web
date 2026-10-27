# Trade Control — Privacy Statement

**Trade Control Ltd**

Company number: 07569044

[Document status](#document-status)

## 1. About Trade Control

Trade Control Ltd develops free and open-source business management and accounting software.

Trade Control also intends to provide a managed hosting service through which businesses can maintain their accounts and use supported statutory filing services, including HMRC and Companies House integrations.

This statement explains how personal information is handled when using Trade Control Ltd's managed service. It does not govern independent installations operated by third parties.

**Contact:** office@tradecontrol.co.uk

## 2. Information we process

Depending on the services used, information may include:

- Business and user registration details.
- Customer, supplier and other business contact information.
- Accounting transactions, invoices, payments and supporting records.
- Statutory reporting and tax information.
- Authentication and authorisation records.
- Technical logs, security events and resource-consumption information.
- Billing and subscription information, where applicable.

Trade Control processes information needed to provide the services selected by each business.

## 3. How information is used

Information is processed to:

- Operate and maintain the hosted accounting service.
- Authenticate users and protect business records.
- Prepare and submit authorised statutory filings.
- Maintain filing histories and supporting evidence.
- Provide technical support and investigate problems.
- Monitor service reliability, security and resource consumption.
- Administer hosting subscriptions and payments when commercial subscriptions are introduced.

Personal information is not sold to advertisers.

## 4. Business ownership and data protection responsibilities

Businesses using the managed service retain control over their own business records.

Trade Control Ltd will generally act as a data processor when hosting personal information on behalf of a customer business, subject to the actual processing arrangements.

Trade Control Ltd acts as a data controller for information it uses for its own purposes, such as service administration, security, billing and customer communications.

Before commercial operation, Trade Control Ltd will document the applicable legal bases and enter into an appropriate data-processing agreement with each customer business.

## 5. Tenant isolation and security

The submission topology uses a multi-tenant application in which each business has a separate database and a separate tenant file namespace. A request is associated with a business from its approved hostname before the application selects that business's database. User identities and roles are held inside the applicable business database. This is the architecture intended for the accredited commercial service; the current `tradecontrol.co.uk` deployment remains a sandbox and accreditation environment rather than a live customer service.

**Tenant security is embodied in the open-source code.**

The software mechanisms used to enforce tenant isolation and access controls are publicly available for independent examination and improvement.

Trade Control Ltd will also use deployment-level controls including encrypted HTTPS connections, encrypted storage, managed service identities, restricted database networking, role-based access to secrets and cryptographic keys, and monitoring of security and operational events.

Open-source transparency supports independent scrutiny but does not constitute a guarantee that software or infrastructure is free from vulnerabilities.

The source code and the hosted configuration are separate security boundaries. Publication of the source supports scrutiny of application controls; access to the production subscription, databases, storage, keys and credentials will remain restricted to authorised operators and service identities.

The shared topology has passed independent B1/B2 brokered-authorisation, immutable-approval, explicit sandbox-submission, reconciliation, durable-history and exact-readback journeys. The corrective B3/B4 VAT discovery cases also passed brokered authorisation, populated reciprocal isolation, approval, one HTTP `201` sandbox submission each, exact readback, fraud-header validation and complementary restart recovery. The final live kill-switch refusal belongs to the fresh B5/B6 seamless run governed by the approval matrix.

**Publication gate:** Complete the remaining tenant-isolation review, penetration test, restoration exercise, incident-response exercise and authority security checks before relying on these controls in an effective customer statement.

## 6. Hosting and third-party services

The accreditation topology uses Microsoft Azure infrastructure in the United Kingdom. The customer-facing web application and the dedicated HMRC credential broker run on Linux-based Azure App Service. The application, protected storage, cryptographic keys and operational monitoring are hosted in the UK West Azure region, with Azure SQL databases in the UK South region. Data may move between those UK regions to provide the service, monitoring and recovery arrangements. The production-credentials submission is based on this reviewed topology; the final commercial deployment must be reconfirmed before this statement becomes effective.

Microsoft Azure will provide the principal cloud infrastructure. Any transactional email, payment, support, monitoring or other provider that processes customer information will be identified in the final subprocessor schedule before the relevant service is enabled.

Where customers authorise statutory filings, information is transmitted to HMRC or Companies House as required for the selected filing service.

Payment providers and any future AI services will be identified together with the information shared with them before they are enabled for customers.

**Publication gate:** Confirm the final production regions, data-residency commitments, backup locations, international-transfer position and complete subprocessor schedule before publication.

## 7. HMRC authorisation

Trade Control uses the applicable HMRC authorisation mechanisms for supported services.

Customers authenticate with HMRC through HMRC's own authorisation process. Trade Control does not require customers to disclose their HMRC sign-in passwords.

HMRC authorisation is implemented in the accreditation topology through a dedicated credential-broker service using a fixed registered callback address. The broker uses short-lived, single-use state to return each completed authorisation to the originating business and accepts returns only for approved tenant hostnames. Production authority remains disabled until the applicable accreditation and explicit human approval are complete.

HMRC tokens will be encrypted at rest and separated from the customer-facing application. Access will be limited to the service identities and operations that require it. Disconnecting an HMRC connection will remove Trade Control's stored grant; it does not change the customer's HMRC sign-in credentials.

Companies House credentials will be held outside customer databases and application source code, using protected production configuration available only to the filing service. The production identity will not be enabled until Companies House commissioning and the associated evidence controls are complete.

**Publication gate:** Replace all sandbox and test identities, verify revocation and recovery behaviour, and complete VAT, Corporation Tax and Companies House production approval before describing any filing service as available.

## 8. AI-assisted services

No AI provider is integrated into the launch baseline described by this draft, and the service will not send customer business information to an AI provider.

Trade Control may offer AI-assisted accounting and business-management features in future. Before such services are enabled, this statement will identify the relevant providers, purposes, data-processing arrangements, retention policies and customer controls.

Customer business information will not be made available to an AI service without an identified service purpose and appropriate data-protection arrangements.

## 9. Retention and deletion

Information will be retained only for as long as necessary to provide the service, meet applicable legal obligations, protect the service and maintain required statutory filing evidence.

The launch retention schedule will separately define periods for:

- active and closed customer records;
- database backups and disaster-recovery copies;
- operational, security and database-audit logs;
- short-lived authorisation state and stored authority grants;
- statutory requests, responses, receipts and sealed filing evidence; and
- billing, support and account-administration records.

Short-lived HMRC authorisation state will be deleted when consumed and abandoned state will expire automatically. Statutory filing evidence will be protected against alteration for its approved retention period. Backup retention will not be presented as the retention period for a customer's accounting records or filing evidence.

Customers will be provided with documented arrangements for obtaining their data before account closure and for requesting deletion. Deletion will remain subject to applicable legal obligations, the customer's own record-keeping responsibilities, fraud and security requirements, and any immutable statutory evidence that must be retained.

**Publication gate:** Approve and insert the actual retention periods, implement the full tenant export and closure workflow, lock the statutory-evidence retention policy, and verify deletion and recovery before publication.

## 10. Individual rights

Individuals may have rights under UK data-protection law, including rights of access, correction, erasure, restriction and objection, depending on the circumstances.

Requests may be sent to office@tradecontrol.co.uk.

Where Trade Control Ltd processes information on behalf of a customer business, requests may need to be handled by that business as the data controller.

Individuals may also raise concerns with the UK Information Commissioner's Office.

## 11. Security concerns

Security concerns may be reported to office@tradecontrol.co.uk until a dedicated security address is published.

Trade Control welcomes responsible security research and contributions to its open-source projects.

Before launch, Trade Control Ltd will publish a vulnerability-disclosure procedure and establish documented incident assessment, containment, recovery and legally required notification processes.

**Publication gate:** Insert the final security contact and link to the reviewed vulnerability-disclosure procedure.

## 12. Changes to this statement

This statement will be reviewed as the managed service develops and updated when material processing arrangements change.

---

## Document status

This is a pre-launch Privacy Statement supplied in support of Trade Control's HMRC production-credentials application. It describes the intended managed service; it is not yet an effective customer notice, and the commercial service is not live.

The `tradecontrol.co.uk` deployment is the sandbox and accreditation environment used to demonstrate the intended multi-tenant topology. Production filing authority is not enabled. Technical evidence and outstanding approval controls are recorded separately in the [VAT production-access evidence dossier](../vat-production-access-evidence.md).

This submission copy was derived from `tradecontrol.hosting/docs/legal/trade-control-privacy-policy.md` on the clean hosting revision `a61040fdccea7b66937b95717061d4269012674f`. The source legal documents were last changed in hosting commit `d16a2ae` on 24 October 2026.

Before publication as an effective customer notice, the final production composition must be checked against this statement, every publication gate must be resolved and the text must receive legal approval.

**Submission draft date:** 26 October 2026

**Effective date:** Not yet effective.
