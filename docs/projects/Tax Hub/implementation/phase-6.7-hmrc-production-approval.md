# Phase 6.7 — HMRC VAT production approval and listing

## Status

The initial production-access request was sent to the Software Developers Support Team at 15:17 BST on 2 October 2026. They replied on 14 October 2026 with procedural guidance to use **Developer Hub → Applications → Get production credentials** once testing is complete. The green control is visible for the `Trade Control Tax Hub` sandbox application. HMRC stated that it is withheld when an application is subscribed to APIs that are unavailable in production.

This is an eligibility and process signal, not production approval. The response did not answer the two disclosed fraud-header questions. Phase 6.6 remains accepted and the VAT product remains technically approval-ready; production access has not been granted, no production credentials have been requested or created, no live VAT return has been filed and Trade Control is not yet recorded as compatible software.

**Decision (14 October 2026): production-credentials application deferred.** The owner has chosen not to apply against the temporary isolated single-tenant deployment. The application will remain unanswered until the intended shared multi-tenant Azure hosting environment has been implemented and independently reviewed. This is a deployment-assurance gate, not a technical VAT failure, and it does not reopen the accepted Objective 3 statutory contracts or Phase 6.6 evidence.

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

The production-credentials application will describe the intended shared multi-tenant Azure delivery after that environment exists and has passed independent review. The single-tenant Phase 6.6 deployment remains useful historical and technical evidence, but it must not be presented as proof of production tenant isolation or topology-sensitive fraud-prevention behaviour.

Before the application is completed, the intended production deployment requires:

1. request-scoped, server-derived tenant identity and tenant-isolation evidence;
2. the approved Azure SQL, private Blob and Key Vault production composition;
3. a stable shared OAuth callback and isolated sandbox/production applications and stores;
4. an accurate final Azure ingress, TLS-hop and public-egress model for fraud-prevention headers;
5. tenant-attributable redacted monitoring, rate control, retention, recovery and a submission kill switch; and
6. an independent security and operational review, including adversarial cross-tenant tests and recovery/incident procedures;
7. refreshed HMRC sandbox obligations, submission and view-return evidence where required for current proof; and
8. fraud-header validation from the actual final ingress/TLS/public-egress topology, followed by HMRC clarification of any additional review or retesting it requires.

Do not describe the later process as a guaranteed full reapplication. HMRC should determine whether it requires a refreshed questionnaire, selected endpoint evidence, fraud-header revalidation or a new production-access review.

## External action register

- [x] Send the initial Software Developers Support Team contact within the two-week window — sent 2 October 2026 at 15:17 BST.
- [x] Record the contact date and retain the sent correspondence outside the public repository.
- [x] Receive HMRC's procedural response — received 14 October 2026; it directs the owner to the Developer Hub production-credentials application and does not grant approval.
- [x] Inspect the production-credentials application and capture its questions without selecting or saving an answer or submitting the request. The protected local inventory records the 14 October review; tracked documentation contains no application identifier or protected answer.
- [x] Defer the production-credentials application until the intended multi-tenant Azure environment and its assurance evidence are complete — owner decision recorded 14 October 2026.
- [ ] Implement the intended multi-tenant Azure production topology and managed secret/persistence facilities.
- [ ] Independently review tenant isolation, security, operational controls, recovery, monitoring and the submission kill switch against the actual topology.
- [ ] Confirm with HMRC whether the hosting/topology change requires additional review, refreshed application material or selected API retesting.
- [ ] Repeat the required HMRC sandbox obligations, submission, view-return and fraud-header evidence against the intended production topology.
- [ ] Complete the Developer Hub production-credentials application and any follow-up questionnaires or evidence requests HMRC issues.
- [ ] Accept the current HMRC terms through the authorised human process.
- [ ] Resolve and record HMRC's VAT API and fraud-header findings.
- [ ] Record HMRC's production-access decision.
- [ ] Obtain separate approval for one eligible business and one controlled live VAT submission.
- [ ] Reconcile the live return and retain its protected evidence.
- [ ] Request compatible-software listing and provide the live-submission VRN privately to HMRC.
- [ ] Record HMRC's confirmation of the compatible-software listing.

## Current gate

The production-credentials flow has been inspected through its first unanswered question. The public preview identifies 25 further questions covering organisation identity, marketing, development and error handling, accessibility, incident management, GDPR and data isolation, SaaS penetration testing and security audit, fraud-prevention data, server location, privacy policy and terms. The live flow begins with the additional branching question, **“Do you sell, resell or distribute your software?”** No answer will be selected or saved while the application is deferred.

The next programme work is implementation and independent review of the intended multi-tenant Azure environment. That gate includes server-derived tenant authority, cross-tenant negative tests, Azure SQL/Blob/Key Vault integrity and recovery, least-privilege operations, monitoring/rate control/retention, kill-switch behaviour and the final ingress/TLS/public-egress facts. After the topology is stable, obtain HMRC clarification on additional review, refresh the required sandbox and fraud-header evidence, and only then return to the application answers. No answer should be saved, no legal terms accepted, no production credentials created, no protected identifier disclosed and no final application submitted without explicit contemporaneous authorisation.
