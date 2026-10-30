# Trade Control — Managed Hosting Terms and Conditions

**Trade Control Ltd**

Company number: 07569044

[Document status](#document-status)

## 1. About the service

Trade Control is free and open-source accounting and business-management software.

Trade Control Ltd intends to offer a managed hosting service for businesses that prefer not to operate the software themselves. The service is not yet commercially live.

These terms concern the managed hosting service. They do not replace or restrict the open-source licences governing the software.

## 2. Free and open-source software

Trade Control software, including its multi-tenant hosting components, is distributed under applicable GNU open-source licences.

Users may obtain, use, study, modify and redistribute the software in accordance with those licences.

The source code is publicly available through:

<https://github.com/TradeControl>

Trade Control Ltd charges for operating and maintaining a hosted service, not for permission to use the software.

## 3. Managed hosting

The managed service is intended to provide:

- Secure hosting of business accounting records.
- User authentication and access controls.
- Data storage, backups and operational maintenance.
- Supported statutory reporting and filing integrations.
- Service monitoring and technical support.
- Additional MIS and ERP functionality as it becomes available.

Only features identified as available in the current service are included. Planned functionality is not guaranteed to be available by a particular date.

Trade Control Ltd will operate the routine backup and recovery arrangements for the managed service. Customers will not be expected to administer infrastructure backups, although they remain responsible for reviewing their records and meeting their own statutory record-keeping obligations.

## 4. Charges

The proposed standard hosting charge is **£5 per business per month**, excluding any applicable VAT.

This is a proposed tariff and will be confirmed before commercial subscriptions begin.

The base charge is intended to cover ordinary hosting consumption.

Additional charges may apply where a business consumes resources beyond the included allowance, such as exceptional database, storage, processing or AI-service usage.

Before introducing consumption-based charges, Trade Control Ltd will publish:

- The resources measured.
- The included allowances.
- The applicable rates.
- How consumption can be reviewed.
- How customers will be notified of additional charges.
- Any available spending limits or controls.

No additional charge will be imposed without an applicable published charging arrangement.

**Publication gate:** Confirm the tariff, included storage and processing allowances, payment provider, billing cycle, tax treatment, cancellation and refund arrangements before subscriptions are offered. The commissioning service does not yet perform tenant-level commercial metering or billing.

## 5. Users and functionality

Trade Control's commercial model does not charge licence fees per user or per software feature.

Businesses may use the functionality made available through the hosted service, subject to technical capacity, security controls, acceptable-use requirements and any separately identified third-party charges.

Future MIS and ERP functionality is intended to be available without additional software licence fees.

## 6. Customer responsibilities

Customers are responsible for:

- Maintaining accurate business and accounting records.
- Managing their authorised users.
- Protecting their account credentials.
- Reviewing statutory documents before submission.
- Ensuring filings are authorised and submitted by the applicable deadlines.
- Complying with relevant tax, accounting and company-law obligations.

Trade Control provides tools to support these activities but does not replace professional accounting, tax or legal advice.

## 7. Statutory filing services

The managed service may provide access to supported HMRC and Companies House filing facilities.

Such facilities are available only where Trade Control Ltd has the necessary authority registrations, credentials and technical arrangements.

Each filing requires the appropriate customer authorisation and must satisfy the applicable statutory requirements.

A submission acknowledgement does not necessarily mean that an authority has accepted the filing.

Trade Control Ltd will retain the exact approved filing payload, the authority request and response, the acknowledgement or receipt, and the related approval and audit evidence for the published retention period. Filing evidence will be sealed against undetected alteration and exportable independently of the running service.

The commercial service will not enable an authority or filing type until Trade Control Ltd has completed the applicable production approval, installed the production identity and verified the complete preparation, approval, submission, read-back, correction and recovery process.

## 8. Independent installations

Businesses and developers may operate independent installations of Trade Control under the applicable open-source licences.

Such installations do not automatically acquire the right to use Trade Control Ltd's statutory filing credentials, vendor registrations, hosting infrastructure or managed services.

Independent operators must establish their own arrangements with the relevant authorities where required.

## 9. Customer data and confidentiality

Customer business records remain under the customer's control.

Trade Control Ltd will process hosted information in accordance with its Privacy Policy and applicable data-protection arrangements.

Customer information will not be disclosed to other hosted businesses except where an authorised service or explicitly agreed business-to-business interaction requires it.

The customer data-processing agreement and subprocessor schedule will form part of the contractual documents for the launch service. Trade Control Ltd and authorised personnel will be subject to documented confidentiality and access-control obligations.

**Publication gate:** Complete legal review of the controller/processor allocation, data-processing agreement, confidentiality terms, lawful bases, subprocessors and international-transfer provisions.

## 10. Security and service operation

The accreditation topology uses a multi-tenant application with a separate database and file namespace for each business. Requests are bound to an approved business hostname before the corresponding database is selected, and identities and roles are held within that business boundary. The intended commercial service will use this reviewed architectural pattern, subject to completion of the remaining accreditation, security and publication gates.

The underlying security mechanisms are implemented in publicly available open-source software.

Operational security also depends on the configuration and maintenance of the hosted infrastructure.

Trade Control Ltd will maintain reasonable technical and organisational security measures appropriate to the service and applicable legal obligations. These will include encrypted network connections and storage, protected secrets and cryptographic keys, least-privilege service identities, restricted database access, operational and security monitoring, backup and recovery procedures, change control, and incident management.

Backups, operational logs and statutory evidence serve different purposes and will have separately published retention and recovery arrangements. Statutory evidence will use an approved immutable-retention policy; ordinary backup or soft-deletion controls will not be represented as immutable filing evidence.

The application-scope tenant-isolation and penetration testing is complete, and the responsible individual has adopted the incident-response procedure for the accreditation process. Before these terms are offered to customers, complete the restore and disaster-recovery exercises, activate and test the final live contact route, complete production authority checks, approve retention and perform the operational access review. Insert final backup, recovery-point and recovery-time commitments only after those exercises establish what can be promised.

## 11. Data export and termination

Customers will be able to obtain their supported business data through documented export arrangements in durable, machine-readable formats. Available financial reports and the Identity account's personal-data download do not, by themselves, constitute a business account export.

An authenticated tenant administrator can generate a customer-exit archive on demand. The archive is generated in application memory and returned directly over the authenticated HTTPS response; the service does not create a reusable public link or server-side temporary package. The response prohibits caching, provides an export reference and content digest, and the generation event is recorded against the requesting administrator.

The first-release archive contains:

- supported physical tenant business tables as UTF-8 JSON arrays;
- the business's uploaded documents, images and templates;
- a manifest identifying the table paths, columns, scope, formats, creation time and exclusions; and
- a SHA-256 digest for package verification.

Infrastructure configuration, operational logs, host passwords, encryption material, OAuth grants and tokens, vendor credentials, central identity credentials and any other tenant's information are excluded. This control provides interpretable customer data; it does not promise automated import into another product or export of protected authority credentials.

Before an account is closed, the customer will be given a reasonable opportunity to obtain the complete export. The published closure process will explain the termination notice, access period, secure deletion timetable, treatment of backups, and any records or immutable filing evidence that Trade Control Ltd must retain. It will also explain the customer's responsibility to retain accounting and statutory records after leaving the service.

**Publication gate:** Distinct B5/B6 archives, formats, manifests and synthetic records have been inspected. Before these terms are offered to customers, complete the account-closure, deletion and recovery processes and insert fulfilment, reasonable-use, notice and approved retention terms.

Any outstanding charges and applicable statutory record-retention obligations will be handled under the published service terms.

## 12. Availability and liability

The service depends on hosting infrastructure, network connectivity and external statutory authority systems.

Trade Control Ltd will take reasonable steps to maintain service availability and recover from failures. Planned maintenance, authority outages and circumstances outside Trade Control Ltd's reasonable control will be addressed in the final service terms.

**Publication gate:** Insert only availability, support response, maintenance, recovery, service-credit and liability commitments that have been operationally tested, costed and legally reviewed. No service-level commitment is created by this draft.

Nothing in these terms is intended to exclude liability that cannot lawfully be excluded.

## 13. Changes to the service and terms

Trade Control Ltd may develop the service, introduce additional capabilities and revise hosting arrangements.

Material changes to charges or contractual terms will be communicated with appropriate notice.

## 14. Contact

Questions about the managed service may be sent to:

**Trade Control Ltd**

Email: office@tradecontrol.co.uk
Company number: 07569044

---

## Document status

These are pre-launch Terms and Conditions supplied in support of Trade Control's HMRC production-credentials application. They describe the intended managed hosting service; they are not yet offered customer terms, and the commercial service is not live.

The `tradecontrol.co.uk` deployment is the sandbox and accreditation environment used to demonstrate the intended multi-tenant topology. Production filing authority is not enabled. Technical evidence and outstanding approval controls are recorded separately in the [VAT production-access evidence dossier](../vat-production-access-evidence.md).

This submission copy was derived from the corresponding draft in Trade Control's private internal hosting repository and reconciled against the completed public accreditation evidence on 30 October 2026. The public `tradecontrol.web` revision containing this document is the controlling submission source. Private source paths and revisions remain in the internal evidence record and are not presented as material HMRC must access.

Before these terms are offered to customers, the final production composition and commercial terms must be checked against this document, every publication gate must be resolved and the text must receive legal approval.

**Submission draft date:** 30 October 2026

**Effective date:** Not yet effective.
