# Phase 6.7 — HMRC VAT production approval and listing

## Status

The initial production-access request was sent to the Software Developers Support Team at 15:17 BST on 2 October 2026. Phase 6.6 is accepted and the VAT product is technically approval-ready. The application is now awaiting HMRC's response and questionnaires; production access has not been granted, no live VAT return has been filed and Trade Control is not yet recorded as compatible software.

The first contact must be made within HMRC's two-week log-availability window following the completed sandbox tests on 2 October 2026. Contact should therefore be made promptly and no later than 16 October 2026.

## Approval basis

The application must describe the product that was actually tested:

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

## Multi-tenant disclosure

The first application seeks review of the tested single-tenant product. It must also disclose that Trade Control intends to use a shared multi-tenant Azure delivery before wider commercial rollout.

That future deployment is not being presented as tested or approved. Before activation it requires:

1. request-scoped, server-derived tenant identity and tenant-isolation evidence;
2. the approved Azure SQL, private Blob and Key Vault production composition;
3. a stable shared OAuth callback and isolated sandbox/production applications and stores;
4. an accurate final Azure ingress, TLS-hop and public-egress model for fraud-prevention headers;
5. tenant-attributable redacted monitoring, rate control, retention, recovery and a submission kill switch; and
6. disclosure to HMRC followed by any topology-sensitive retesting or change review HMRC requires.

Do not describe the later process as a guaranteed full reapplication. HMRC should determine whether it requires a refreshed questionnaire, selected endpoint evidence, fraud-header revalidation or a new production-access review.

## External action register

- [x] Send the initial Software Developers Support Team contact within the two-week window — sent 2 October 2026 at 15:17 BST.
- [x] Record the contact date and retain the sent correspondence outside the public repository.
- [ ] Receive and complete both HMRC questionnaires.
- [ ] Accept the current HMRC terms through the authorised human process.
- [ ] Resolve and record HMRC's VAT API and fraud-header findings.
- [ ] Record HMRC's production-access decision.
- [ ] Implement and verify the approved production secret, persistence, monitoring and kill-switch facilities.
- [ ] Obtain separate approval for one eligible business and one controlled live VAT submission.
- [ ] Reconcile the live return and retain its protected evidence.
- [ ] Request compatible-software listing and provide the live-submission VRN privately to HMRC.
- [ ] Record HMRC's confirmation of the compatible-software listing.
- [ ] Complete the later multi-tenant hosting-change review before wider rollout.

## Current gate

The next external action is receipt and human review of HMRC's response and questionnaires. No legal acceptance, credential creation, customer-identifier disclosure or live submission occurs without explicit contemporaneous authorisation.
