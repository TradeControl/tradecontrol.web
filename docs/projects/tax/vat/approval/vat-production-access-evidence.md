# HMRC VAT production-access evidence dossier

27 October 2026

**Status:** This is evidence from a pre-launch sandbox accreditation environment on the intended shared multi-tenant Azure topology; it is not a live customer service. B3/B4 proved the corrected OAuth-status and tenant-context boundaries. After B5/B6 were independently verified as valid commissioned VAT-registered tenants, both completed a clean HMRC VAT process without corrective code or tenant-data adjustment: fresh organisation OAuth, immutable preparation and approval, live fail-closed tenant submission gates, shared Linux App Service restart recovery, one explicitly authorised sandbox submission, HTTP `201`, fulfilled-obligation reconciliation, exact nine-box HMRC readback and durable filing history. Fraud-header validation used specification 3.3 with zero errors and the two previously disclosed truthful empty-value warnings. A separate pre-OAuth first-registration bootstrap defect remains a Business Node commissioning finding and production-launch gate. Production credentials have not been requested, HMRC terms have not been accepted, no production return has been filed and the final application has not been submitted.

## Purpose

This dossier defines the evidence required before Trade Control resumes the HMRC Developer Hub production-credentials application for VAT (MTD).

It is modelled on the completed Companies House approval package: planned results remain visibly pending, authority outcomes are distinguished from local assertions, and protected evidence stays outside Git. It separates HMRC VAT accreditation evidence from the steeper Business Node provisioning process. The sandbox environment proves the filing product and isolation controls on the intended topology; seamless empty-database Business Node delivery remains a distinct production-launch requirement.

## Reviewed baselines

| Repository | Reviewed revision | Working-tree state at review | Publication state |
|---|---|---|---|
| `tradecontrol.web` | `1f8deca` (B3/B4 failed-run provenance); `446e865` (B5/B6 deployed discovery baseline) | B5/B6 began clean; first-registration correction now uncommitted | Both revisions committed locally; neither pushed |
| `tradecontrol.hosting` | `a61040f` | Clean | Committed locally; not pushed |

These revisions define the attempted no-corrective-coding baselines. A later documentation-only evidence update does not change the deployed product baseline. Any product, database, infrastructure, configuration-contract or manual tenant-data correction needed to make a journey pass is recorded as a failed commissioning attempt.

The B3/B4 authorisations exposed the refresh-lease and circuit-scoped EF-context defects; those corrections were reviewed and committed as part of `446e865` before B5/B6 began. B5/B6 then exposed a different hosted first-registration defect: the configured bootstrap-administrator redirect reached Identity registration before `NodeContext.InitializeNode()`, so required jurisdictions, templates, units and months were absent when business commissioning began. A bounded transaction seeded the lookups, commissioned each tenant and restored its single registered Identity account byte-for-byte before OAuth. The product candidate now detects the missing lookup state and seeds it before the first Identity record is created; the regression suite and full Visual Studio build pass. This fails the seamless Business Node commissioning claim, but it does not invalidate the later clean HMRC process, which began only after each tenant's identity, statutory profile, year set and `18A2` eligibility had been independently verified.

The revisions are recorded as local identifiers rather than public links because neither commit has been pushed.

## Prerequisite evidence — B1 and B2

B1 and B2 independently passed the shared-host journey through:

- brokered OAuth;
- immutable VAT return preparation and attributable approval;
- explicit sandbox submission;
- HTTP `201` acceptance;
- obligation reconciliation;
- durable filing history; and
- exact HMRC readback of all nine VAT values.

This establishes that the intended multi-tenant hosting prerequisite is complete. B1 and B2 are reserved for the Corporation Tax journey and are not the dedicated VAT production-access matrix. Their HMRC identities, VAT registrations, filings and protected evidence must not be reused for B3 or B4.

## Public case names

`B1` to `B6` are local evidence-case aliases, not HMRC terminology and not product editions. Each alias denotes one independently resolved tenant on the same reviewed shared Azure application, with its own Azure SQL database, tenant-local Identity store, statutory profile and HMRC workflow state.

- **B1 and B2** are the two earlier hosting-assurance tenants. Their completed VAT journeys proved the shared topology and they are now reserved for Corporation Tax evidence. They are prerequisite evidence only; their protected details are not part of this public package.
- **B3** is the dedicated VAT-registered Standard Micro Entity, locally named `Business 3 Ltd`.
- **B4** is the dedicated VAT-registered sole trader, locally named `Business 4 SA`.
- **B5** is the second VAT-registered Standard Micro Entity discovery tenant, locally named `Business 5 Ltd`.
- **B6** is the second VAT-registered sole-trader discovery tenant, locally named `Business 6 SA`.

This description is self-contained so a reviewer does not need access to the private hosting repository to understand the cases being submitted.

## Evidence rules

1. B3 and B4 each use a fresh HMRC organisation sandbox identity created only after this matrix is reviewed.
2. B3 is a VAT-registered Standard Micro Entity; B4 is a VAT-registered sole trader.
3. Each case uses the ordinary shared-host commissioning path. Tenant identity, reporting subject, HMRC principal, VAT registration and filing period are resolved server-side and are never supplied as browser authority.
4. The nine VAT values arise only from `Cash.vwTaxVatStatement` → `Cash.vwTaxVatSubmission` → the accepted immutable preparation path. No value may be edited or manufactured for the test.
5. Approval binds the exact prepared bytes, SHA-256, source snapshot, obligation, declaration version, tenant and actor before dispatch.
6. Submission is a separate, explicit, human-observed sandbox action. HTTP `201` proves acceptance of that request; exact view-return reconciliation supplies the post-filing value proof.
7. A planned result is never recorded as passed. Every pending cell is replaced only from retained evidence.
8. No automatic replay is permitted. Ambiguous or interrupted writes remain blocked pending controlled reconciliation.
9. Corrective coding is classified at the boundary where it occurred. B5/B6 failed seamless Business Node commissioning before OAuth, but their subsequent HMRC journeys passed without corrective code or tenant-data adjustment. A fresh commissioning pair is required after the bootstrap correction is reviewed and deployed; another VAT filing pair is required only if HMRC requests refreshed or topology-changed evidence.
10. Production credentials, legal terms, production filing and final application submission remain outside this dossier and require explicit contemporaneous human approval.

## Commissioning matrix

| Evidence area | B3 — Standard Micro Entity | B4 — sole trader | Acceptance rule |
|---|---|---|---|
| Fresh identity | **Passed — 26 October 2026** | **Passed — 26 October 2026** | A distinct newly generated HMRC organisation sandbox identity is used; protected details remain under `.local`. |
| Tenant creation | **Passed — dedicated tenant provisioned 26 October 2026** | **Passed — dedicated tenant provisioned 26 October 2026** | Each tenant has its own database, Identity store and custom host. This row does not claim that the original protected identity selection was correct. |
| Statutory profile | **Passed after pre-OAuth bounded correction — VAT-registered Standard Micro Entity** | **Passed after pre-OAuth bounded correction — VAT-registered standard sole trader** | The titled business-associated user, reporting-subject VAT registration and reviewed profile were transactionally aligned and independently read back without changing accounting values or the year set. |
| Baseline identity | `tradecontrol.web` `1f8deca`; hosting `a61040f` | Same required baseline | Deployed application and hosting revisions are recorded before the journey; unreviewed drift fails the matrix. |
| Required legal documents | **Submission drafts present** | **Shared application material** | The public package contains a Privacy Statement and Terms and Conditions describing the intended accredited topology, explicitly marked pre-launch and not effective; remaining publication gates and legal review stay visible. |
| Brokered OAuth | **Discovery pass — fresh grant retained; Connected after restart** | **Discovery pass — fresh grant retained; Connected after restart** | Fixed shared callback, state/PKCE, tenant/principal/actor binding and exact VAT scopes succeed without token or credential exposure, and the protected status read remains reliably available after callback. |
| Tenant isolation | **Discovery pass — populated B3 host exposed only B3 workflow state** | **Discovery pass — populated B4 host exposed only B4 workflow state** | Live reciprocal enumeration showed one distinct tenant-owned grant and accepted filing per host with no foreign record. Direct negative tests reject foreign tenant claims, context rebinding, cross-origin OAuth completion, unknown hosts and tenant/principal-mismatched protected references; history and reconciliation resolve the tenant before an opaque attempt or approval. |
| Obligation authority | **Discovery pass — 27 October 2026; repeated by B5** | **Discovery pass — 27 October 2026; repeated by B6** | A live sandbox open obligation supplies the opaque period key and exactly matches an eligible local VAT period. |
| Immutable preparation | **Discovery pass — deterministic nine-box artifact; repeated by B5** | **Discovery pass — deterministic nine-box artifact; repeated by B6** | All nine displayed values, canonical bytes, digest and source snapshot agree; no browser VAT body or editable box exists. |
| Approval | **Discovery pass — explicit attributable approval retained** | **Discovery pass — explicit attributable approval retained** | The declaration is explicit and unchecked by default; the durable approval remains bound to the same tenant, actor, obligation, source and digest. |
| Kill switch | **Control deployed; live fail-closed evidence completed by B5** | **Control deployed; live fail-closed evidence completed by B6** | With the submission kill switch engaged, preparation and approval remain available but no submit control or outbound attempt exists. Releasing it requires an authorised operational action and does not mutate the approved bytes. |
| Explicit sandbox submission | **Discovery pass — one human-authorised action on 27 October 2026** | **Discovery pass — one human-authorised action on 27 October 2026** | One separately observed action sends the approved bytes once; durable attempt reservation prevents repeat activation. |
| HMRC acceptance | **Discovery pass — HTTP `201`** | **Discovery pass — HTTP `201`** | HMRC returns HTTP `201`; safe receipt classification is retained without publishing protected references. |
| Restart recovery | **Discovery pass — approved return recovered before submission** | **Discovery pass — accepted receipt and readback recovered after submission** | Restart at the assigned checkpoint recovers the same tenant-scoped approval/attempt/outcome and exposes continuation or reconciliation without duplicate submission. Both readiness endpoints returned HTTP `200` after the shared Linux App Service restart. |
| Obligation reconciliation | **Discovery pass — already-filed classification; no second submission offered** | **Discovery pass — already-filed classification; no second submission offered** | Refreshed obligations and retained filing state do not offer a second submission for the accepted period. The sandbox obligation feed retained a blank received date, so this row does not claim an invented date or a stronger authority status. |
| Exact readback | **Discovery pass — period key and all nine values matched** | **Discovery pass — period key and all nine values matched** | View-return period key and all nine values match the approved immutable request exactly, with an empty difference set. |
| Fraud headers | **Discovery pass — specification 3.3, zero errors, two disclosed empty-value warnings** | **Discovery pass — specification 3.3, zero errors, two disclosed empty-value warnings** | The deployed topology is validated against the current HMRC specification with zero errors; the warnings are the truthful empty single-factor `gov-client-multi-factor` value and absent `gov-vendor-license-ids`. No value was fabricated. |
| Durable evidence | **Discovery pass — accepted history and exact readback retained after restart** | **Discovery pass — accepted history and exact readback retained after restart** | Approval, matching approved/dispatched digests, attempt, HTTP outcome and exact readback survive restart; the completed reciprocal-isolation row remains separately classified. |
| No corrective coding | **Failed — retained as discovery evidence** | **Failed — retained as discovery evidence** | The first OAuth run required product corrections to the connection-status path and EF context lifetime. B5/B6 subsequently completed clean HMRC journeys on the corrected OAuth baseline. Their separate pre-OAuth bootstrap finding is governed by the Business Node production-launch gate. |

## Commissioning checkpoint — 26 October 2026

B3 and B4 were created as two additional tenants on the reviewed shared application. Each uses a dedicated Azure SQL database, tenant-local first administrator, custom hostname and Azure-managed TLS certificate. The same SQL Node DACPAC was published to both databases, database identity was stamped, and the existing App Service managed identity received the standard tenant runtime grant.

Each guarded first-administrator registration created exactly one tenant-local Identity user. The protected commissioning transaction then used the established SQL Node initialisation and synthetic-dataset procedures; it did not patch application, database-schema or infrastructure source. B3 selected the Standard Micro Company template and B4 the Standard Sole Trader template, both with VAT registration enabled. `App.proc_DatasetSyntheticMIS` rebuilt each tenant at the 30 June 2017 test anchor with one completed accounting year, leaving only accounting years 2016–2018. `App.proc_DatasetSyntheticMIS_VatSandboxAlign` then aligned each distinct protected organisation identity to the exact local `18A2` period, 1 April to 30 June 2017.

Both transactions asserted and passed:

- one preserved domain administrator and one matching tenant-local Identity user;
- the protected administrator display name supplied by the corresponding fresh sandbox organisation;
- exactly three accounting years, 2016 through 2018, with no irrelevant year remaining;
- a reviewed synthetic VAT reporting profile aligned to the tenant's protected sandbox registration;
- exact local eligibility for the `18A2` obligation period; and
- normal Accounts Mode activation after commissioning.

The first authenticated home-page check failed in both tenants because bootstrap had bound the domain administrator to the commissioning operator's SQL principal rather than the shared application's proven runtime principal. A guarded repair restored service while the protected process was made self-contained: it derives the qualified runtime identity from the deployed App Service managed identity, uses the authenticated operator binding only inside the commissioning transaction where the existing dataset guard requires it, and restores the runtime binding before commit. A first sequencing attempt failed the current-user guard and rolled back.

Subsequent review found that the protected generator had selected the last two records in a longer local test-user file rather than the records explicitly titled `BUSINESS 3` and `BUSINESS 4`. Before OAuth, a bounded transaction set the associated-user display names from those titled records and called `App.proc_DatasetSyntheticMIS_VatSandboxAlign` for each corresponding VAT registration. It did not rebuild the dataset, change accounting values or alter the three-year 2016–2018 set. Independent readback found exactly one intended user, one safe tenant business name and one reviewed synthetic VAT profile in each database. The live B3 Tax Hub then presented **Not connected**, and B4 exposed its intended associated user in the ordinary Admin Manager surface. Protected generated SQL, sandbox identities, tax identifiers and operational outputs remain under ignored `.local`.

Several attempted full reruns were interrupted or timed out while the Azure SQL synthetic rebuild was still active; their transactions were allowed to roll back or reach quiescence before the bounded correction. They are not accepted commissioning evidence. This checkpoint does not yet prove seamless commissioning, HMRC obligation retrieval, OAuth, approval, submission or any remaining filing-assurance row.

The first protected B3 runner invocation was rejected by the SQL parser before its transaction began because the local evidence script supplied a conditional expression directly as a stored-procedure argument. The later principal-binding repair changed tenant data but remained confined to the two new tenants and the ignored commissioning helper. Neither finding changed the reviewed application, schema, infrastructure or deployment-contract source. Both findings remain visible for human classification; tenant creation and the whole-journey no-corrective-coding row remain pending until a clean reviewed repeat rather than being silently marked passed.

## Assigned recovery coverage

Both cases must prove restart-safe persistence. To avoid two identical exercises, they use complementary checkpoints:

- **B3:** restart after immutable approval and before submission. Recovery must present the same approved digest and permit only the original explicit sandbox submission after all gates, including the kill switch, are satisfied.
- **B4:** restart after HTTP `201` evidence is durably retained and before readback reconciliation completes. Recovery must preserve the accepted outcome, prohibit resubmission and expose only controlled obligation/view-return reconciliation.

If operational timing prevents the assigned checkpoint, stop and review the alternative before the sandbox write. Do not improvise a restart around an uncertain outbound request.

## B3/B4 discovery filing result — 27 October 2026

The owner gave contemporaneous approval for the two sandbox writes after reviewing the exact immutable return. Each case then sent the approved `18A2` return once and received HTTP `201`. Filing History retained the accepted outcome, matching approved/dispatched payload digests and an exact view-return match for all nine values. Protected receipt, correlation, bundle, charge, tax and user references remain outside this public package.

B3 recovered the same durable approval after the pre-submission shared-host restart and then completed its one authorised submission. B4 was accepted first; the shared Linux App Service was then restarted before reconciliation was revisited. Both tenant readiness endpoints returned HTTP `200`, and B4's post-restart Filing History recovered the accepted outcome and exact readback without exposing a submit control. B3's accepted history was also retrieved after that restart.

The refreshed sandbox obligation feed continued to omit a received date for `18A2`. The product did not manufacture one: it combined the authority response with the durable accepted filing and presented **Already submitted to HMRC / Already filed — readback available**, thereby preventing a repeat write. This satisfies the dossier's safe reconciliation rule but is not described as a separately returned HMRC received date.

Both fraud-header validation calls used the reviewed shared-host browser, broker, ingress, TLS and public-egress path. HMRC specification 3.3 returned zero errors and only the known `EMPTY_HEADER` warnings for the truthful single-factor application sign-in and the absence of an originating-device vendor licence identifier. Those limitations must be disclosed in the production-access application; placeholder values must not be supplied.

## B5/B6 commissioning finding and clean HMRC repeat — 27 October 2026

B5 (`Business 5 Ltd`, Standard Micro Entity) and B6 (`Business 6 SA`, sole trader) were created on the committed `tradecontrol.web` `446e865` and `tradecontrol.hosting` `a61040f` discovery baseline. Each received its own database, tenant reference, custom TLS hostname, tenant-local first administrator and fresh HMRC organisation sandbox identity. Independent readback confirmed distinct database and tenant identities, the intended business form, one domain user, one Identity user, accounting years 2016–2018 only, and an exact locally eligible `18A2` period.

Business Node commissioning was not seamless. Hosted first-administrator registration created the Identity account before SQL Node lookup data had been seeded. The first B5 business-initialisation call therefore failed on a missing jurisdiction and rolled back; B6 had not yet been changed. A bounded repair for each tenant preserved the single Identity account and its role state, ran the ordinary seed and business-initialisation procedures, restored the Identity records byte-for-byte, completed Basic Setup and independently verified the resulting profile. The product correction now performs the missing lookup seed before the first hosted Identity account is created. Its regression assertion, Tax Hub boundary suite and full Visual Studio solution build pass, but the change remains uncommitted and was not deployed for this run. This finding remains open in the Business Node commissioning track and must be closed before production launch.

The HMRC VAT evidence boundary begins with those independently verified commissioned tenants. From that boundary, both cases completed the ordinary product journey without any corrective code or tenant-data adjustment:

- separate brokered VAT read/write authorisations remained **Connected**;
- the live fraud-header validator used specification 3.3, returned zero errors and reported only the two disclosed truthful empty-value warnings;
- each retrieved open obligation `18A2`, prepared the same deterministic nine-box return with no findings and retained an explicit approval while its tenant submission gate was disabled;
- a shared Linux App Service restart preserved both brokered grants and both immutable approvals, while each tenant remained fail-closed for submission;
- after contemporaneous human approval, only the B5/B6 tenant gates were enabled and each exact approved return was submitted once;
- each submission returned HTTP `201` and a safe `HMRC-SUCCESS` classification;
- refreshed obligations classified `18A2` as already filed and did not offer another submission;
- Filing History retained matching approved/dispatched payload digests and an exact HMRC view-return match for all nine values; and
- the B5/B6 tenant gates were returned to `false` after both reconciliations completed.

The redacted isolation preflight also passed: the live tenant directory contained unique hosts and tenant references, B5/B6 resolved distinct Azure SQL databases and database identities, each anonymous canonical host identified only its own business, and both connection strings used managed identity to reach the expected database. After filing, each authenticated host exposed exactly one tenant-owned accepted history with distinct actor, attempt and authority references and did not expose the other tenant's filing identity. The same direct negative-boundary suite used for B3/B4 continued to reject foreign tenant claims, origins, context rebinding and mismatched protected references. Protected tax, user, OAuth, receipt, correlation and raw fraud-header material remains under ignored `.local`.

## Populated reciprocal-isolation result — 27 October 2026

The authenticated B3 and B4 hosts were revisited with both workflows populated. Each canonical host resolved its intended business, retained **Connected**, and exposed exactly one accepted tenant-owned filing with its own distinct masked registration, actor, attempt and authority references. Neither history exposed the other tenant's filing identity. The complementary restart evidence also remained tenant-specific: B3 recovered its approval before its write, while B4 recovered its accepted result and readback after the shared-host restart.

The product deliberately exposes no browser field or route through which an operator can inject another tenant's protected attempt, preparation, approval or content reference. The reciprocal live enumeration was therefore combined with the direct negative boundary suite: a foreign tenant claim and context rebind are rejected; an unknown host fails closed; cross-tenant and foreign-origin OAuth completion are rejected; protected content references require the resolved tenant and principal fingerprints; filing history enumerates the resolved tenant before resolving attempts or approvals; and reconciliation refuses an attempt absent from that tenant. The refreshed suites passed 314 TCWeb boundary assertions and 176 submission-adapter assertions.

This is a hybrid live-and-negative-boundary proof, not a claim that a non-existent public cross-reference input was exercised. Protected references and screenshots remain under ignored `.local`; the public result records only the classification.

## Tenant-isolation evidence

The isolation result is one two-tenant test, not two self-contained happy paths. With B3 and B4 both provisioned, retain redacted evidence that:

- a B3 session cannot read or act on B4's HMRC connection, obligations, preparation, approval, attempt, content, history or recovery reference;
- the reciprocal B4-to-B3 checks also fail;
- guessed or deliberately supplied references do not substitute for server-derived tenant authority;
- background or broker callback processing restores the correct tenant context before protected access;
- safe operational telemetry identifies the responsible opaque tenant without recording tax identifiers, OAuth material, fraud facts or VAT bodies; and
- support and restart paths preserve the same isolation boundary.

The public result records only the checked surfaces and pass/fail classification. Exact references, logs and protected object identifiers remain under `.local`.

## Fraud-prevention evidence

Each case must capture browser facts freshly and use the same broker, ingress, TLS-hop, public-egress and application composition intended for the production application. Record publicly:

- specification version and validation date;
- connection method;
- error count;
- warning categories and their truthful rationale; and
- confirmation that the request used the reviewed shared-host topology.

Do not publish header values, IP addresses, device identifiers, user identifiers or encoded topology strings. An empty multi-factor field remains truthful when the tested Trade Control sign-in is single-factor. A vendor licence identifier remains absent when no licensed software is installed on the originating device. Neither value may be invented to silence a warning.

## Kill-switch evidence

For each case, engage the environment- and tenant-scoped submission kill switch before the authorised sandbox write and prove:

1. preparation, exact review and retained history remain available;
2. submission fails closed before network I/O;
3. no outbound attempt, ambiguous write or consumed filing identity is manufactured by the blocked action;
4. the approved candidate bytes and digest remain unchanged;
5. release is attributable, explicitly authorised and limited to the intended sandbox tenant/environment; and
6. one subsequent explicit submission is still protected by the normal durable one-write rule.

The public dossier records classifications and safe digests only. Operational switch identifiers and audit details remain protected.

## Execution sequence

1. Retain B3/B4 as corrective HMRC discovery evidence and B5/B6 as clean HMRC VAT journeys from independently verified commissioned tenants.
2. Review the uncommitted hosted first-registration seed correction and its regression coverage.
3. Commit and deploy the reviewed application revision only after human review; record the exact application and hosting revisions without pushing as part of the evidence action.
4. Prove seamless Business Node delivery from empty databases through registration, lookup seeding, business commissioning and independently verified statutory readiness before production launch.
5. Repeat HMRC filings only if HMRC requests current logs or selected retesting, or if the OAuth, fraud-header, tenant-isolation or filing topology changes materially.
6. Complete redaction and consistency review of the submission package.
7. Ask HMRC whether the hosting change requires further review or retesting before the production-credentials application is completed.

Every future external write in this sequence remains separately human-gated. This document does not authorise another sandbox submission, production credentials, acceptance of HMRC terms, a production filing or final application submission.

## B3 result sheet — Standard Micro Entity

| Item | Result |
|---|---|
| Commissioning date | 26 October 2026 |
| Deployed revisions | `tradecontrol.web` `1f8deca`; hosting `a61040f` |
| Fresh organisation identity | **Passed — explicitly titled protected B3 record; safe fingerprint retained locally** |
| Tenant/statutory profile | **Passed — dedicated tenant, Standard Micro Company, VAT registered** |
| Brokered OAuth | **Discovery pass — 27 October 2026; healthy isolated grant retained and Connected after restart** |
| Reciprocal isolation checks | **Discovery pass — live B3 enumeration plus reciprocal negative-boundary suite disclosed no B4 workflow state** |
| Obligation and local-period match | **Discovery pass — live open `18A2` matched the eligible local period** |
| Prepared-body SHA-256 | `1E1F9E862011250F9A7BD88FD40D87E4A29A36BBB06A4B27D71F4D9EB36CFE0F` |
| Source snapshot classification | **Discovery pass — `TradeControl / Cash.vwTaxVatSubmission`; protected snapshot token retained locally** |
| Approval/declaration evidence | **Discovery pass — explicit attributable approval bound to the immutable request** |
| Kill-switch result | **Tenant-scoped control deployed; live fail-closed evidence was subsequently captured with B5** |
| Restart checkpoint | **Discovery pass — approved return recovered before the authorised submission** |
| Submission outcome | **Discovery pass — one explicit sandbox submission; HTTP `201`** |
| Safe request/response digests | **Discovery pass — approved and dispatched payload digests match; protected response evidence retained privately** |
| Obligation reconciliation | **Discovery pass — already-filed/readback classification suppresses resubmission; sandbox received date remained blank** |
| Exact nine-box readback | **Discovery pass — period key and all nine values matched exactly** |
| Fraud-header validation | **Discovery pass — specification 3.3; zero errors; two disclosed truthful empty-value warnings** |
| Durable history/evidence | **Discovery pass — accepted receipt class, digest match and exact readback retained after restart** |
| Corrective coding required | Yes — the journey exposed connection-status representation, OAuth lease-surface and EF context-lifetime defects; uncommitted candidate deployed for discovery |
| Case conclusion | **Corrective HMRC discovery complete; B5 later supplied the clean VAT repeat and live kill-switch evidence** |

## B4 result sheet — sole trader

| Item | Result |
|---|---|
| Commissioning date | 26 October 2026 |
| Deployed revisions | `tradecontrol.web` `1f8deca`; hosting `a61040f` |
| Fresh organisation identity | **Passed — explicitly titled protected B4 record; safe fingerprint retained locally** |
| Tenant/statutory profile | **Passed — dedicated tenant, standard sole trader, VAT registered** |
| Brokered OAuth | **Discovery pass — 27 October 2026; healthy isolated grant retained and Connected after restart** |
| Reciprocal isolation checks | **Discovery pass — live B4 enumeration plus reciprocal negative-boundary suite disclosed no B3 workflow state** |
| Obligation and local-period match | **Discovery pass — live open `18A2` matched the eligible local period** |
| Prepared-body SHA-256 | `1E1F9E862011250F9A7BD88FD40D87E4A29A36BBB06A4B27D71F4D9EB36CFE0F` |
| Source snapshot classification | **Discovery pass — `TradeControl / Cash.vwTaxVatSubmission`; protected snapshot token retained locally** |
| Approval/declaration evidence | **Discovery pass — explicit attributable approval bound to the immutable request** |
| Kill-switch result | **Tenant-scoped control deployed; live fail-closed evidence was subsequently captured with B6** |
| Restart checkpoint | **Discovery pass — HTTP `201` retained before restart; accepted outcome and exact readback recovered afterwards** |
| Submission outcome | **Discovery pass — one explicit sandbox submission; HTTP `201`** |
| Safe request/response digests | **Discovery pass — approved and dispatched payload digests match; protected response evidence retained privately** |
| Obligation reconciliation | **Discovery pass — already-filed/readback classification suppresses resubmission; sandbox received date remained blank** |
| Exact nine-box readback | **Discovery pass — period key and all nine values matched exactly** |
| Fraud-header validation | **Discovery pass — specification 3.3; zero errors; two disclosed truthful empty-value warnings** |
| Durable history/evidence | **Discovery pass — accepted receipt class, digest match and exact readback retained across the post-acceptance restart** |
| Corrective coding required | Yes — the journey exposed connection-status representation, OAuth lease-surface and EF context-lifetime defects; uncommitted candidate deployed for discovery |
| Case conclusion | **Corrective HMRC discovery complete; B6 later supplied the clean VAT repeat and live kill-switch evidence** |

## B5 result sheet — Standard Micro Entity

| Item | Result |
|---|---|
| Commissioning date | 27 October 2026 |
| Deployed revisions | `tradecontrol.web` `446e865`; hosting `a61040f` |
| Fresh organisation identity | **Passed — separate protected organisation record; details retained locally** |
| Tenant/statutory profile | **Passed after bounded bootstrap repair — dedicated tenant, Standard Micro Entity, VAT registered** |
| Seamless commissioning | **Failed — hosted first registration omitted required SQL Node lookup seeding** |
| Brokered OAuth | **Passed — isolated grant retained and Connected after restart** |
| Tenant/database isolation | **Passed — unique host/reference and database identity; live filing history exposed only B5 and no B6 filing identity** |
| Obligation and local-period match | **Passed — live open `18A2` matched the eligible local period** |
| Immutable preparation | **Passed — deterministic nine-box artifact; no findings or editable VAT body** |
| Approval/declaration evidence | **Passed — explicit attributable approval retained before submission enablement** |
| Kill-switch result | **Passed — approval remained available while tenant submission was disabled; no outbound attempt existed** |
| Restart checkpoint | **Passed — grant and approval survived a shared Linux App Service restart while the gate remained disabled** |
| Submission outcome | **Passed — one contemporaneously authorised sandbox action; HTTP `201`** |
| Safe request/response evidence | **Passed — approved and dispatched payload digests match; protected receipt evidence retained privately** |
| Obligation reconciliation | **Passed — refreshed view classified `18A2` as already filed and suppressed resubmission** |
| Exact nine-box readback | **Passed — all nine HMRC values matched the approved return exactly** |
| Fraud-header validation | **Passed — specification 3.3; zero errors; two disclosed truthful empty-value warnings** |
| Durable history/evidence | **Passed — accepted outcome, digest match and exact readback retained** |
| Post-write gate | **Passed — B5 tenant submission returned to disabled** |
| Business Node corrective action | **Required before OAuth — first-registration lookup seeding correction is implemented and tested but remains uncommitted and undeployed** |
| HMRC journey corrective action | **None — no code or tenant-data correction from verified VAT readiness through exact readback** |
| Case conclusion | **Clean HMRC VAT journey passed; seamless Business Node delivery remains a separate production-launch gate** |

## B6 result sheet — sole trader

| Item | Result |
|---|---|
| Commissioning date | 27 October 2026 |
| Deployed revisions | `tradecontrol.web` `446e865`; hosting `a61040f` |
| Fresh organisation identity | **Passed — separate protected organisation record; details retained locally** |
| Tenant/statutory profile | **Passed after bounded bootstrap repair — dedicated tenant, standard sole trader, VAT registered** |
| Seamless commissioning | **Failed — hosted first registration omitted required SQL Node lookup seeding** |
| Brokered OAuth | **Passed — isolated grant retained and Connected after restart** |
| Tenant/database isolation | **Passed — unique host/reference and database identity; live filing history exposed only B6 and no B5 filing identity** |
| Obligation and local-period match | **Passed — live open `18A2` matched the eligible local period** |
| Immutable preparation | **Passed — deterministic nine-box artifact; no findings or editable VAT body** |
| Approval/declaration evidence | **Passed — explicit attributable approval retained before submission enablement** |
| Kill-switch result | **Passed — approval remained available while tenant submission was disabled; no outbound attempt existed** |
| Restart checkpoint | **Passed — grant and approval survived a shared Linux App Service restart while the gate remained disabled** |
| Submission outcome | **Passed — one contemporaneously authorised sandbox action; HTTP `201`** |
| Safe request/response evidence | **Passed — approved and dispatched payload digests match; protected receipt evidence retained privately** |
| Obligation reconciliation | **Passed — refreshed view classified `18A2` as already filed and suppressed resubmission** |
| Exact nine-box readback | **Passed — all nine HMRC values matched the approved return exactly** |
| Fraud-header validation | **Passed — specification 3.3; zero errors; two disclosed truthful empty-value warnings** |
| Durable history/evidence | **Passed — accepted outcome, digest match and exact readback retained** |
| Post-write gate | **Passed — B6 tenant submission returned to disabled** |
| Business Node corrective action | **Required before OAuth — first-registration lookup seeding correction is implemented and tested but remains uncommitted and undeployed** |
| HMRC journey corrective action | **None — no code or tenant-data correction from verified VAT readiness through exact readback** |
| Case conclusion | **Clean HMRC VAT journey passed; seamless Business Node delivery remains a separate production-launch gate** |

## Completion gate

The HMRC VAT sandbox matrix is complete. B3/B4 provide corrective discovery provenance; B5/B6 provide clean, independently repeated HMRC journeys from verified commissioned tenants. Together they prove populated reciprocal isolation, brokered OAuth, immutable approval, live fail-closed tenant gates, shared-host restart recovery, separately authorised sandbox acceptance, already-filed reconciliation, exact readback, fraud-header validation and retained history on the intended multi-tenant topology.

The separate Business Node production-launch gate remains open until the first-registration seed fix is reviewed, committed and deployed and fresh empty databases commission without corrective action. Closing that gate does not require another VAT filing unless HMRC asks for refreshed evidence or the relevant filing topology changes.

Completion of this matrix permits human review of the production-credentials application evidence. It does not request credentials, accept terms, authorise a production filing or submit the final application.

## Related records

- [Phase 6.7 production approval](../phase-6.7-hmrc-production-approval.md)
- [Phase 6.6 readiness evidence](../phase-6.6-hmrc-approval-readiness.md)
- [VAT Work Plan 6](../tax-hub-workplan-6.md)
- [Tax findings](../../findings.md)
- [Tax change log](../../change-log.md)
