# Phase 6.7 — HMRC VAT production approval and listing

## Status

The initial production-access request was sent to the Software Developers Support Team at 15:17 BST on 2 October 2026. They replied on 14 October 2026 with procedural guidance to use **Developer Hub → Applications → Get production credentials** once testing is complete. The green control is visible for the `Trade Control Tax Hub` sandbox application. HMRC stated that it is withheld when an application is subscribed to APIs that are unavailable in production.

This is an eligibility and process signal, not production approval. The response did not answer the two disclosed fraud-header questions. Phase 6.6 remains accepted and the VAT product remains technically approval-ready; production access has not been granted, no production credentials have been requested or created, no live VAT return has been filed and Trade Control is not yet recorded as compatible software.

**Decision (14 October 2026): production-credentials application deferred.** The owner has chosen not to apply against the temporary isolated single-tenant deployment. The application will remain unanswered until the intended shared multi-tenant Azure hosting environment has been implemented and independently reviewed. This is a deployment-assurance gate, not a technical VAT failure, and it does not reopen the accepted Objective 3 statutory contracts or Phase 6.6 evidence.

**Sequencing reaffirmed (22 October 2026):** the multi-tenant environment is now active programme
work. Resume this VAT phase only after that environment is ready and reviewed, alongside the resumed
CT Phase 5.10 evidence work. The streams retain separate HMRC credentials, tests and approval gates.

**Resumed at evidence-definition gate (26 October 2026).** The shared multi-tenant hosting prerequisite is complete. B1 and B2 independently passed brokered OAuth, immutable return preparation and approval, explicit sandbox submission, HTTP `201` acceptance, obligation reconciliation, durable filing history and exact nine-box HMRC readback. The reviewed baselines are `tradecontrol.web` `1f8deca` and `tradecontrol.hosting` `a61040f`; both were committed locally with clean working trees and had not been pushed at the review point.

B1 and B2 are reserved for the Corporation Tax journey and do not become the dedicated VAT production-access cases. Phase 6.7 now proceeds through the public B3/B4 evidence matrix under [`approval/`](approval/README.md). B3 is the VAT-registered Standard Micro Entity `Business 3 Ltd` and B4 the VAT-registered sole trader `Business 4 SA`. Both are provisioned with separate fresh HMRC organisation sandbox identities, but the earlier clean-commissioning claim was withdrawn after a protected record-selection error was found and corrected before OAuth.

**Corrective-code and discovery gate (27 October 2026).** B4 and B3 each completed brokered sandbox authorisation on the `1f8deca` deployment and retained a valid, correctly isolated VAT read/write grant. A transient status read then presented both connections as **Unavailable** even though neither grant was lost or invalid. The first correction removed an unnecessary refresh-lease dependency and added safe operational logging; simultaneous integration loading then reproduced the incident and exposed the actual root cause: concurrently rendering components shared one circuit-scoped EF context while resolving VAT workflow identity. The uncommitted candidate now creates a tenant-bound context per identity operation and is deployed to integration for discovery testing. Offline suites and the Release build pass, both tenant readiness endpoints return HTTP `200` after an explicit Linux App Service restart, and the restarted worker contains no further context-concurrency or VAT-status failure. B3/B4 remain diagnostic corrective runs rather than the final seamless accreditation evidence.

**Corrective discovery filing completed (27 October 2026).** Both cases independently retrieved `18A2`, prepared and explicitly approved the same deterministic nine-box return, and completed one contemporaneously authorised sandbox submission. Each returned HTTP `201`, durable accepted filing history and an exact nine-box view-return match. B3 recovered its durable approval across the pre-submission shared-host restart. B4 was accepted before the shared Linux App Service restart; after both readiness probes returned HTTP `200`, its accepted outcome and exact readback were recovered without a second submission path. Refreshed obligation views classify `18A2` as already filed and suppress repeat submission, while truthfully retaining the sandbox feed's blank received date.

Both topology-matched fraud-header validation calls used specification 3.3 and returned zero errors. The only warnings were the previously disclosed truthful empty value for the single-factor application sign-in and the absent originating-device vendor licence identifier; neither was fabricated. Protected identities, tax references, receipt/correlation metadata, raw headers and unredacted evidence remain outside Git.

B3/B4 remain diagnostic corrective runs rather than the final seamless accreditation evidence. Their populated reciprocal-isolation proof passed through reciprocal live enumeration plus direct negative boundary tests. Their corrected OAuth-status baseline was then reviewed and committed before the B5/B6 repeat described below.

**B5/B6 commissioning finding and clean HMRC repeat (27 October 2026).** B5 (`Business 5 Ltd`, Standard Micro Entity) and B6 (`Business 6 SA`, sole trader) were created from fresh databases on deployed discovery revisions `tradecontrol.web` `446e865` and `tradecontrol.hosting` `a61040f`, with separate tenant identities, custom hosts and fresh HMRC organisation sandbox identities. Their first business-initialisation attempt exposed a separate hosted bootstrap defect: Identity registration occurred before required SQL Node lookup data had been seeded. The failed B5 transaction rolled back and B6 was still untouched. A bounded repair then seeded and commissioned each tenant while preserving its single registered Identity account and roles byte-for-byte. The product correction performs that seed before the first hosted Identity record is created; its regression test, Tax Hub suites and full Visual Studio build pass. It is now committed in public baseline `f169e79`, but was not deployed for B5/B6. This is a Business Node commissioning failure and production-launch gate; it is not classified as an HMRC filing failure.

After the bounded repair and independent statutory-readiness verification, both tenants completed clean HMRC VAT journeys with no further code or tenant-data correction: brokered OAuth, fraud-header validation, open `18A2` obligation retrieval, immutable nine-box preparation and explicit approval. With their tenant gates disabled, each retained its approval and exposed no submit control. Both grants and approvals survived a shared Linux App Service restart. Following contemporaneous human approval, only the B5/B6 tenant gates were enabled; each exact approved return was submitted once and returned HTTP `201`, matching approved/dispatched digests, already-filed reconciliation, durable history and an exact nine-box HMRC readback. The B5/B6 gates were then restored to `false`. Protected identities, tax references, receipts, correlations, raw fraud facts and unredacted results remain under ignored `.local`.

**Application baseline and preflight (28 October 2026).** The reviewed changes were consolidated into public application baselines `tradecontrol.web` `f169e79` and Tax Hub `6bd0702`, with `tradecontrol.hosting` `001a5d5` retained as a private internal provenance reference. HMRC is not expected to access the private hosting repository; the public dossier states the material topology and evidence directly. Historical deployed discovery identifiers remain in the dossier so the recorded runs can be interpreted accurately. The authorised human began the production-credentials application, answered the initial software-distribution branch **Yes** and selected the VAT API. Work then paused on HMRC's checklist for a public-package consistency, URL and redaction preflight. No HMRC terms have been accepted, no production credentials have been requested or created and the application has not been finally submitted.

**Compliance Readiness Pass 1 (28 October 2026).** The non-penetration readiness work is now separately evidenced. A repeatable WCAG 2.2 Playwright/axe suite found and corrected shared blue-theme contrast defects; the final correction is deployed and the public sign-in/registration axe and 320px checks pass independently on B5 and B6, while authenticated/manual and screen-reader evidence remains open. The HMRC/ICO incident procedure, private record and synthetic cross-tenant tabletop are ready for owner adoption. A UK GDPR accountability inventory and Article 28 draft identify the remaining legal, retention, transfer and DPIA decisions rather than converting technical controls into a broad compliance claim. An administrator-only customer-exit ZIP is deployed with documented JSON/files, protected exclusions, no temporary server package, digest/no-cache delivery and attributable audit evidence. Its B5/B6 acceptance exposed and corrected a physical-column enumeration defect plus private-key/host-binding exclusions; the final 332-assertion boundary executable and fresh reciprocal archive inspections pass. Penetration testing and the ICO-aligned security-control audit are explicitly reserved for Security Assurance Pass 2 and have not begun.

The initial contact satisfied HMRC's two-week log-availability window following the completed sandbox tests on 2 October 2026. Any evidence later supplied with the deferred application must be current and topology-matched; the programme will repeat the required sandbox calls rather than rely on HMRC retaining the October logs indefinitely.

## Preserved technical baseline

The accepted Phase 6.6 evidence describes the product that was actually tested:

- Trade Control is free and open-source software for businesses managing their own accounts and filing their own VAT returns;
- the supported persona is a VAT-registered business filing for itself, not an agent acting for clients;
- the tested product is TCWeb in the present single-tenant Azure deployment;
- the required VAT operations are retrieval of obligations and submission of the return for an open period;
- Trade Control also uses HMRC view-return for exact post-filing reconciliation, so that optional endpoint is included in the tested and requested scope;
- liabilities, payments, penalties, customer information, VAT Assist and agent filing are not claimed; and
- technical approval is not represented as production access, a live filing, compatible-software listing or general commercial availability.

## Public supporting material

- Product source: <https://github.com/TradeControl/tradecontrol.web>
- Tax Hub source: <https://github.com/TradeControl/tax-hub>
- Published VAT user guide: <https://tradecontrol.github.io/tax/tax-hub-vat/>
- Phase 6.6 parent checkpoint: `d887d32` (`Vat Objective 5 Phase 6.6`)
- Phase 6.6 Tax Hub checkpoint: `0840a79` (`Vat Objective 5 Phase 6.6`)
- Detailed technical evidence: `phase-6.6-hmrc-approval-readiness.md`

The public repositories and guide are supporting evidence, not substitutes for HMRC's log inspection, questionnaires, terms or production-access decision.

## Evidence summary for first contact

- Sandbox testing completed on 2 October 2026 using a fresh HMRC organisation test user in deployed TCWeb.
- Retrieve-obligations returned the canned open `18A2` period.
- Submit-return returned HTTP 201 and a safe HMRC receipt.
- View-return readback matched the period key and all nine immutable approved values exactly.
- A refreshed obligation showed the return as already submitted, and Filing History retained approval, dispatch, receipt and readback evidence.
- A genuine App Service interruption after acceptance preserved the receipt, blocked repeat submission and was resolved through controlled HMRC readback with an exact nine-value match.
- The deployed fraud-header validator used specification 3.3 and returned no errors. It reported only the disclosed empty multi-factor value for the current single-factor application sign-in and the absent originating-device vendor licence value.
- Desktop, keyboard, 200% zoom and mobile presentation reviews passed.
- Both Release solutions built without warnings or errors and all applicable offline suites passed: 496 assertions plus 44 MTD Income Tax endpoint descriptors.

Sandbox application identifiers, test-user credentials, VAT registration numbers, unredacted request or response data, OAuth material and fraud facts must be supplied only through an HMRC-approved private channel when requested. They must not be added to tracked documentation or ordinary correspondence.

## Multi-tenant prerequisite

The production-credentials application describes the reviewed shared multi-tenant Azure delivery used by the pre-launch sandbox accreditation environment. The single-tenant Phase 6.6 deployment remains useful historical and technical evidence, but it is not presented as proof of production tenant isolation or topology-sensitive fraud-prevention behaviour.

The intended production deployment has passed its B1/B2 hosting prerequisite. B3/B4 discovery and the clean B5/B6 HMRC repeats now provide combined evidence for reciprocal isolation, tenant-scoped kill switches, shared-host restart recovery, both supported business forms, sandbox acceptance, exact readback and topology-matched fraud headers. The application preflight still requires:

1. redaction and consistency review of the completed public submission package;
2. verification that the public Privacy Statement and Terms and Conditions URLs resolve anonymously; and
3. human review and publication of the resulting documentation changes.

The application will ask HMRC to clarify any additional hosting-change review or selected retesting it requires.

The separate production-launch gate is deployment of the committed hosted first-registration correction and proof of seamless Business Node delivery from a fresh empty database. This launch gate is reported honestly but does not convert the completed HMRC filing journeys into failures or require another filing unless HMRC requests refreshed evidence.

The dedicated B3/B4 tenants were provisioned and locally aligned on 26 October 2026. Each has an independent database and custom TLS host, one tenant-local administrator, the intended VAT-registered Standard Micro Company or standard sole-trader profile, only accounting years 2016–2018, and an exact locally eligible `18A2` period. The authenticated home-page check exposed a protected commissioning-orchestration principal-binding defect. Later review also found that the ignored generator had selected historical test-user records positionally rather than the explicitly titled B3/B4 records. A bounded pre-OAuth correction aligned the associated-user display names and existing reviewed VAT profiles through the established sandbox-alignment/reporting-profile boundary without regenerating accounting data. Independent readback passed, but this correction means the clean-commissioning evidence row remains open. The reviewed application, schema and hosting baselines did not change; protected identities and operational evidence remain under ignored `.local`.

Do not describe the later process as a guaranteed full reapplication. HMRC should determine whether it requires a refreshed questionnaire, selected endpoint evidence, fraud-header revalidation or a new production-access review.

## External action register

- [x] Send the initial Software Developers Support Team contact within the two-week window — sent 2 October 2026 at 15:17 BST.
- [x] Record the contact date and retain the sent correspondence outside the public repository.
- [x] Receive HMRC's procedural response — received 14 October 2026; it directs the owner to the Developer Hub production-credentials application and does not grant approval.
- [x] Inspect the production-credentials application and capture its questions without selecting or saving an answer or submitting the request. The protected local inventory records the 14 October review; tracked documentation contains no application identifier or protected answer.
- [x] Defer the production-credentials application until the intended multi-tenant Azure environment and its assurance evidence are complete — owner decision recorded 14 October 2026.
- [x] Implement the intended multi-tenant Azure production topology and managed secret/persistence facilities — reviewed baselines `tradecontrol.web` `1f8deca` and `tradecontrol.hosting` `a61040f`.
- [x] Complete the independent B1/B2 hosting review of brokered OAuth, immutable preparation/approval, explicit sandbox submission, HTTP `201`, obligation reconciliation, durable history and exact readback.
- [x] Define the public, non-secret B3/B4 evidence matrix before commissioning either dedicated VAT tenant.
- [x] Commission B3 as a VAT-registered Standard Micro Entity with a fresh HMRC organisation sandbox identity — completed 26 October 2026.
- [x] Commission B4 as a VAT-registered sole trader with a separate fresh HMRC organisation sandbox identity — completed 26 October 2026.
- [x] Exercise brokered OAuth independently for B3 and B4 on the initial baseline — both grants were created correctly on 27 October 2026, but the run exposed a connection-status availability defect and is not final acceptance evidence.
- [x] Deploy the uncommitted connection-status diagnostic hardening, reproduce the incident and identify the EF context-lifetime root cause; deploy the second candidate and explicitly restart the integration App Service.
- [x] Repeat the authenticated B3/B4 status check on the restarted candidate — both tenants presented **Connected** without reauthorisation or grant repair.
- [x] Retrieve the live `18A2` obligation and prepare the immutable nine-box return independently for B3 and B4.
- [x] Record explicit attributable approval for both immutable returns and recover B3's approval across its assigned pre-submission restart checkpoint.
- [x] Complete one contemporaneously authorised B3 sandbox submission — HTTP `201`, exact nine-box readback and durable accepted history retained.
- [x] Complete one contemporaneously authorised B4 sandbox submission — HTTP `201`; restart the shared Linux App Service after acceptance, restore both tenant readiness probes at HTTP `200`, and recover the accepted history and exact readback without a resubmission path.
- [x] Refresh the B3/B4 obligation views after acceptance — both classify `18A2` as already filed and suppress a second submission; the sandbox feed's blank received date is retained rather than invented.
- [x] Validate fraud-prevention headers for B3 and B4 on the reviewed shared-host topology — specification 3.3, zero errors and only the two disclosed truthful empty-value warnings.
- [x] Complete B3/B4 as corrective discovery cases and retain the failed no-corrective claim rather than reopening or relabelling their history.
- [x] Add the required Privacy Statement and Terms and Conditions to the public submission package as provenance-pinned pre-launch drafts that describe the intended topology without presenting the sandbox environment as a live service.
- [x] Complete populated reciprocal tenant-isolation checks for B3/B4 — each live host exposed only its own distinct grant and accepted filing identity; direct negative tests rejected foreign tenant claims, origins, context rebinding and tenant/principal-mismatched protected references. The product exposes no public cross-reference input, so the evidence is explicitly classified as hybrid live enumeration plus negative-boundary proof. The completed B3/B4 writes cannot now provide a truthful pre-I/O kill-switch refusal followed by acceptance of the same period, so retain that final live capture for B5/B6.
- [x] Review and commit the B3/B4 OAuth-status corrections as `tradecontrol.web` `446e865`, retain hosting `a61040f`, and commission fresh B5/B6 databases and organisation identities.
- [x] Record the B5/B6 first-registration bootstrap failure and bounded tenant repair; implement and test the seed-before-first-Identity correction. It was not deployed for the evidence run and was later consolidated into public baseline `f169e79`.
- [x] Complete B5/B6 brokered OAuth, topology-matched fraud-header validation, immutable preparation and explicit approval.
- [x] Prove the B5/B6 live tenant kill switches and recover both grants and approvals after a shared Linux App Service restart.
- [x] Complete one contemporaneously authorised B5 submission and one B6 submission — each HTTP `201`, already-filed reconciliation, durable history, matching approved/dispatched digests and exact nine-box readback.
- [x] Restore both B5/B6 tenant submission gates to disabled after reconciliation.
- [x] Remove the temporary B5/B6 operator SQL firewall exception after retained evidence was complete.
- [x] Consolidate and publish the reviewed application baselines as `tradecontrol.web` `f169e79` and Tax Hub `6bd0702`; retain `tradecontrol.hosting` `001a5d5` as private internal provenance without implying HMRC can inspect it.
- [x] Begin the authorised Developer Hub application by answering the initial software-distribution branch **Yes** and selecting the VAT API; pause on HMRC's checklist for preflight review.
- [ ] Complete the public-package consistency, anonymous-URL and redaction preflight before supplying further application answers.
- [x] Complete Compliance Readiness Pass 1 implementation and return truthful READY/HOLD answers, incident/tabletop evidence, GDPR inventory, customer-exit implementation and the separate Pass 2 handoff for owner review.
- [ ] Complete the remaining Pass 1 acceptance evidence: authenticated/manual/screen-reader accessibility assessment; adopt the incident procedure; resolve the listed GDPR owner/legal decisions. B5/B6 public accessibility automation and distinct customer-exit archive inspection are complete.
- [ ] Conduct the separately authorised Security Assurance Pass 2 penetration test and ICO-aligned security-control audit against the synthetic Integration topology; do not substitute functional test evidence.
- [ ] Deploy the committed hosted bootstrap correction, then prove seamless Business Node commissioning from fresh empty databases before production launch; repeat VAT filings only if HMRC requests refreshed evidence or the filing topology changes materially.
- [ ] Confirm with HMRC whether the hosting/topology change requires additional review, refreshed application material or selected API retesting.
- [x] Complete the refreshed HMRC sandbox obligations, submission, view-return and fraud-header evidence for B3 and B4 against the intended production topology — retained as corrective discovery evidence, not the final seamless run.
- [ ] Complete the Developer Hub production-credentials application and any follow-up questionnaires or evidence requests HMRC issues.
- [ ] Accept the current HMRC terms through the authorised human process.
- [ ] Resolve and record HMRC's VAT API and fraud-header findings.
- [ ] Record HMRC's production-access decision.
- [ ] Obtain separate approval for one eligible business and one controlled live VAT submission.
- [ ] Reconcile the live return and retain its protected evidence.
- [ ] Request compatible-software listing and provide the live-submission VRN privately to HMRC.
- [ ] Record HMRC's confirmation of the compatible-software listing.

## Current gate

The production-credentials application is now in progress under direct human control. The initial branching question, **“Do you sell, resell or distribute your software?”**, has been answered **Yes** and the VAT API has been selected. The live flow is paused on HMRC's checklist. Pass 1 has made the incident-notification answer ready on owner adoption, completed the deployed customer-exit acceptance, and bounded the remaining accessibility and GDPR work. Penetration testing and the security-control audit remain Pass 2 HOLDs. No answer should be converted to **Yes** merely because supporting text exists.

The [production-access evidence dossier](approval/vat-production-access-evidence.md) separates the two assurance streams. B3/B4 retain corrective OAuth-status discovery provenance. B5/B6 provide clean, independently repeated HMRC VAT journeys and add live tenant kill-switch and shared-restart evidence from the pre-launch sandbox accreditation environment. Their earlier bounded bootstrap repair remains a Business Node commissioning finding, not an HMRC filing failure. B1 and B2 remain reserved for Corporation Tax. The first-registration correction is committed and public in `f169e79`; its deployment and seamless fresh-database Business Node proof remain a separate production-launch gate.

The production-credentials application remains unsubmitted. Complete the preflight, then ask HMRC whether the topology change requires additional review, refreshed answers or selected API retesting. Further non-binding answers may be prepared and supplied under direct human review, but no legal terms may be accepted, no production credentials requested or created, no protected identifier disclosed and no final application submitted without explicit contemporaneous human authorisation.
