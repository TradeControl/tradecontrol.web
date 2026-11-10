# Category Tree hosted statutory changeover test

## Status

**The hosted changeover is accepted on Business 8 (`tc-business8`). VAT is accepted and sealed against `CV00000001`. Companies House submission `S00001` was acknowledged and remains Pending at the authority; a clearly disclosed development simulation exercised the terminal-acceptance path and made `CV00000004` Submitted. Post-filing successor `CV00000005` is active from 1 April 2018, its later `CC-RND` transaction closes with a passing active Equity Bridge, and the CH predecessor exposes the expected £1,000 net classification difference. Recovery evidence now proves fail-closed restored-behind handling without rewriting the externally retained seal. Azure S0 Equity Bridge performance is an accepted separate follow-up; no terminal Companies House acceptance is claimed.**

This is the living acceptance record for the final Category Tree change-control boundary. Update each result in place as evidence is established. Do not infer a pass from implementation or from a successful authority response: every row records its own observed evidence.

## Purpose

The test proves one continuous business history across two statutory channels and a later accounting change:

1. a VAT submission seals the initial Category Tree version;
2. a deliberate successor introduces the plant-purchase scenario and supports Companies House accounts without changing the VAT evidence;
3. a further successor proves that the CH-bound tree remains immutable while the classification continues to evolve; and
4. active and historical Equity Bridges expose the expected classification difference through the one version-parameterised algorithm.

The two authority submissions must bind the exact Category Tree `VersionId`, canonical profile, SHA-256 digest and byte length used during preparation. Authority success makes that version Submitted. A later successor may evolve the tree, but it must not change either earlier seal or its reconstruction.

## Scope and safeguards

- Target host: `https://business8.tradecontrol.co.uk/`.
- Target database: Azure SQL database `tc-business8`, service tier S0 at baseline.
- This is a disposable pre-launch test business with synthetic data and HMRC sandbox identity **Carol Hughes**.
- Secrets and test-user credentials remain in ignored `.local` files and are never copied into this document.
- The B8 commissioning transaction is retained locally at `.local/sandbox/vat-b8-category-tree-commission.sql`.
- Local plant-scenario rationale and executable precedent: [Category Tree controlled changeover test](category-tree-controlled-changeover-test.md) and [`CategoryTreeControlledChangeover.sql`](../../../src/sqlnode/src/tcNodeDb4/Tests/CategoryTreeControlledChangeover.sql).
- No customer migration machinery, production authority submission, hosted-tenant rollout or User Guide publication is authorised by this test.
- Stop on any unexplained seal mismatch, lifecycle transition, active-pointer change, accounting variance or authority ambiguity. Preserve the failing state before correction.

## Human-action boundary

Codex steers the technical sequence, prepares each review surface, captures read-only evidence and stops when external consent or owner judgement is required. The owner is needed only for:

| Point | Required owner action | Status |
|---|---|---|
| HMRC authorisation | Sign in as the reviewed Carol Hughes sandbox user and grant the requested VAT authority. | PASS — connected 6 Nov 2026 |
| VAT filing | Review the nine boxes and explicitly approve the sandbox submission. | PASS — HMRC sandbox accepted `18A2` on 6 Nov 2026 |
| Companies House identity | Provide or confirm the test-company authentication/authority reference if it is not already configured. | PASS — protected synthetic official-test identity configured and accepted by the gateway |
| Companies House filing | Review the rendered accounts and explicitly approve the sandbox submission. | PASS — owner approved `S00001`; gateway acknowledged it and reports Pending manual attachment review |
| Material discrepancy | Decide whether an unexplained accounting or authority difference is an accepted fixture property or a defect. | As required |

## Baseline — 6 November 2026

### Commissioning outcome

| Evidence | Recorded value | Result |
|---|---|---|
| Web application | `2.0.2` | PASS |
| SQL Node build | `4.1.20` | PASS |
| Synthetic template | Standard Micro Company Accounts 2026; VAT-registered profit scenario | PASS |
| Temporal anchor | 30 June 2017; accounting years 2016–17 through 2018–19 | PASS |
| Preserved administrator | Carol Hughes; one enabled administrator and one ASP.NET Identity user | PASS |
| HMRC VAT identity | Reviewed synthetic `INDTAX` profile; VRN retained only in protected local evidence | PASS |
| HMRC obligation | `18A2`, 1 April–30 June 2017 | PASS |
| Local VAT match | Exact calculated period; submission anchor 1 July 2017; net VAT due £713.26 | PASS |
| Company tax profile | Reviewed synthetic `COMTAX` profile | PASS |
| Statutory accounts profile | Reviewed synthetic `STAACC` profile; authority reference not yet configured | PASS with follow-up |
| Generator/Event Log | Both rebuilds completed; no new error event from generation or VAT alignment | PASS |

### Initial Category Tree

| Evidence | Recorded value | Result |
|---|---|---|
| Active version | `CV00000001` | PASS |
| Lifecycle | Published | PASS |
| Effective interval | 30 June 2017–open | PASS |
| Description | Standard Micro Company Accounts 2026 | PASS |
| Canonical profile | `trade-control-category-tree-v1` | PASS |
| Canonical SHA-256 | `1B067CC83D99EB90BDB1BFC58B9866C4C3573CE0A7788F86C3F2B8603BE0DBF7` | PASS |
| Canonical byte length | 11,501 | PASS |
| Categories | 53 | PASS |
| Total links | 18 | PASS |
| Cash Code mappings | 38 | PASS |
| Roots | 2 | PASS |

The historical generator initially published the tree using the real clock even when `@AsOfDate` was supplied. `App.proc_BasicSetup` now uses its already-normalised `@AsOfDate`; ordinary commissioning remains current-date based while a deliberately historical synthetic node is coherent. B8's initial version was aligned to 30 June 2017 and the SQL project rebuild passed.

## Test 1 — statutory-seal and lock contract

**Status: hosted SQL boundary, deployed VAT preparation/lock workflow and accepted authority outcome passed. Additional-filing and recovery evidence remain.**

Before either authority journey, prove both filing paths consume one shared Category Tree provenance contract.

| Test | Expected evidence | Result |
|---|---|---|
| Seal input | Version ID, canonical profile, SHA-256 and byte length are explicit fields, not a broad row-version digest. | PASS — local contract |
| Preparation | Prepared VAT and CH artifacts retain the selected active version and digest. | PASS — local contract |
| Re-preparation | Approval and dispatch re-resolve the version and reject changed content or pointer drift. | PASS — local contract and pointer-drift test |
| Durable lock | A prepared/approved operation prevents Published mutations and successor publication until conclusive outcome. | PASS — hosted SQL boundary and live `18A2` preparation |
| Failure semantics | Conclusive rejection releases the lock; uncertain transport retains it; success records the seal relationship. | PASS — ownership/release SQL boundary; workflow recovery pending |
| Success transition | Authority success changes Published to Submitted without changing canonical bytes. | PASS — HMRC accepted `18A2`; `CV00000001` became Submitted with its digest unchanged |
| Additional filing | An unchanged Submitted version can support another seal without becoming editable. | Pending |
| Tamper proof | One-bit canonical or seal mutation fails verification. | PASS — local canonical-vector tests |

## Test 2 — VAT submission seals version 1

**Status: complete. HMRC accepted the sandbox return, the retained seal verified and `CV00000001` became Submitted.**

1. Confirm `CV00000001` remains active, Published and canonically identical to the baseline.
2. Connect the Tax Hub to Carol Hughes and retrieve the live sandbox obligations.
3. Match HMRC `18A2` to the calculated 1 April–30 June 2017 period.
4. Prepare the VAT return and record the nine-box digest, Category Tree provenance and durable lock reference.
5. Attempt a Published Category edit and successor publication while locked; both must fail without partial writes.
6. Owner reviews and submits the return.
7. Confirm the authority receipt, seal verification and `CV00000001` Published-to-Submitted transition.
8. Reconstruct the submitted VAT evidence after clearing transient application state.

| Evidence | Expected result | Observed result |
|---|---|---|
| Active version before preparation | `CV00000001`, Published | PASS |
| Prepared seal Category version | `CV00000001` and baseline digest | PASS — profile `trade-control-category-tree-v1`, SHA-256 `1B067CC83D99EB90BDB1BFC58B9866C4C3573CE0A7788F86C3F2B8603BE0DBF7`, 11,501 bytes |
| Mutation/clone rejection while locked | Rejected atomically | PASS — live prepared lock rejected direct category mutation with SQL error 51097; successor-publication rejection already passed the hosted SQL boundary |
| HMRC outcome | Accepted sandbox receipt for `18A2` | PASS — `HMRC-SUCCESS`, HTTP 201 |
| Version after success | `CV00000001`, Submitted; digest unchanged | PASS |
| Historical VAT reconstruction | Exact seal and nine boxes verify | PASS — retained filing history and HMRC read-back match all nine approved values |

## Test 3 — successor version, plant scenario and Companies House accounts

**Status: complete with an explicit authority limitation. The successor and plant accounting are established, the active Equity Bridge passes, and official-test submission `S00001` was acknowledged. Companies House has not supplied a terminal acceptance; the development-only simulation exercised the accepted-outcome transition and is retained as simulated evidence, not an authority receipt.**

The execution order was deliberately adjusted during acceptance: the plant purchase is included before the Companies House filing so the filed accounts exercise the new classification and asset accounting. A further successor after filing will then prove that the CH-bound version becomes immutable without preventing later evolution.

1. Clone Submitted `CV00000001` into a Draft successor and prove byte-equivalent starting content.
2. Add the reviewed Indirect Costs classification beneath Overheads.
3. Validate and publish the successor with an effective date after its predecessor.
4. Confirm `CV00000001` becomes Archived, remains immutable and still verifies against the VAT seal.
5. Add `CC-PLANT — Plant and Machinery` to the active successor and transact the reviewed lathe purchase/depreciation scenario through the interface.
6. Close the affected financial year through Admin Manager and confirm the active Equity Bridge.
7. Prepare the appropriate micro-entity accounts through the CH evidence/review path.
8. Record the successor version and digest in the CH statutory seal and acquire the same authoritative submission lock.
9. Owner reviews and submits the accounts.
10. Confirm the CH receipt, seal verification and successor Published-to-Submitted transition.

| Evidence | Expected result | Observed result |
|---|---|---|
| Draft clone | Canonically equal to `CV00000001` before edit | PASS — clone/review workflow completed; discarded intermediate identifiers account for the final `CV00000004` code |
| Successor publication | One active Published version; predecessor Archived | PASS — `CV00000004` active from 1 Jul 2017; `CV00000001` Archived for 30 Jun 2017 |
| VAT predecessor after publication | Seal and reconstruction unchanged | PASS — archived Settings evidence retains the baseline SHA-256 and 11,501 bytes |
| Plant accounting | Net £33,333.33 capitalisation and four-year depreciation schedule | PASS after correcting the synthetic asset fixture; active Equity Bridge passes |
| Prepared CH seal | Successor version and current canonical digest | PASS — preparation `chp1-ea679c4f36ba44728cb695dbbf9d4175` retained `CV00000004`, profile `trade-control-category-tree-v1`, SHA-256 `9CE6EC429DB66348D513151D33D436AC69A49CA0E6E16DB79B5F4FD344B02FE9` and 11,770 bytes |
| CH outcome | Accepted test submission and durable receipt | PARTIAL — `S00001` was acknowledged and remains Pending manual attachment review; no Companies House terminal acceptance has been claimed |
| Successor after success | Submitted and immutable | PASS — development-only Simulate Acceptance used the same terminal completion boundary, recorded actor/time and explicitly disclosed that Companies House did not return the acceptance |

## Test 4 — historical reporting and post-CH evolution

**Status: complete. Historical Cash Statement selection and the VAT-predecessor Equity Bridge difference are proved. `CV00000005` is the active Published successor from 1 April 2018, while `CV00000004` is Archived and visibly read-only. The posted `CC-RND` transaction closes through the ordinary period-end workflow: the active bridge passes and the CH predecessor differs by exactly the £1,000 net R&D amount that it cannot classify.**

After the CH filing, create a further successor because the CH-supported tree is immutable. The plant scenario now belongs to Test 3; this test retains the historical comparison and proves subsequent evolution:

1. clone the CH-submitted version;
2. make one bounded reviewed classification change;
3. validate and publish the successor; and
4. compare the active default, explicit active successor, explicit CH predecessor and VAT predecessor without repointing `App.tbOptions`.

The active Equity Bridge must pass within the established rounding tolerance. B8 has already proved that selecting VAT predecessor `CV00000001` against later plant activity exposes a visible `-£33,333.33` failure, exactly equal to the net plant capitalisation that the predecessor cannot classify. The future CH predecessor must likewise be tested only after a genuinely later classification or Cash Code exists; no failure is to be manufactured or assumed.

| Evidence | Expected result | Observed result |
|---|---|---|
| Active pointer | Further successor throughout explicit historical reads | PASS — `App.tbOptions` now selects Published `CV00000005`; `CV00000004` is Archived for 1 Jul 2017–31 Mar 2018 |
| Active bridge | Passes within tolerance after ordinary period close | PASS — owner closed April 2018 through Admin Manager and confirmed the current version passes; a direct read returned maximum absolute variance £0.01 across three years |
| Explicit active function | Exactly equals operational `Cash.vwEquityReconciliationByYear` | PASS — bidirectional `EXCEPT` returned zero rows after the R&D transaction and period close |
| CH predecessor bridge | Fails visibly by the mathematically explained post-supersession amount | PASS — `CV00000004` differs by exactly £1,000, the VAT-exclusive R&D purchase classified only by successor `CC-RND` |
| VAT predecessor bridge | Remains independently selectable; no active fallback | PASS — selected `CV00000001` reports `-£33,333.33` for 2017–18 |
| Cash Statement XLSX/ODS | Active and eligible historical versions generate with correct Settings provenance and warnings | PASS — selector, warning, Settings provenance and historical failure screenshots retained |
| Earlier statutory seals | VAT and CH seals still verify against their original versions | PASS — retained version records remain `CV00000001` / 11,501 bytes / `1B067C…0DBF7` and `CV00000004` / 11,770 bytes / `9CE6EC…02FE9`; CH authority outcome remains acknowledged/Pending |

## Test 5 — final invariants and recovery evidence

**Status: complete for Category Tree release acceptance. Functional invariants, statutory provenance and recovery pass. The owner has explicitly deferred the unrelated hosted S0 Equity Bridge performance investigation.**

| Test | Expected result | Observed result |
|---|---|---|
| Active-version invariant | Exactly one `App.tbOptions.CategoryVersionId`; target is Published or Submitted. | PASS through successor publication — sole active pointer moved atomically from Submitted `CV00000004` to Published `CV00000005` |
| Immutable history | Direct SQL, EF, handler and JavaScript mutation attempts fail for Submitted/Archived versions. | PASS — hosted SQL guards reject protected writes; archived `CV00000004` renders without mutation controls after successor publication |
| Canonical isolation | Lifecycle changes do not alter semantic digests; content changes do. | PASS — CV1 and CV4 retain their recorded digests after later lifecycle transitions; adding `CA-RND`/`CC-RND` invalidated CV5 until explicit revalidation produced 12,038 bytes and SHA-256 `47EE11D776CAC487F1F5C30A4893BC0477D0BCA1F2344333FFEADBAA889E239E` |
| Seal inventory | Each VAT/CH seal names exactly one retained version and digest. | PASS — VAT evidence binds CV1 and CH preparation binds CV4; neither follows the active pointer to CV5 |
| Restart/retry | Prepared locks and authority attempts survive application restart without replay or unlocked mutation. | PASS — acknowledged `S00001` and its lock survived application deployment/restart; the resumed workspace displayed `EXISTING FILING — DO NOT RESUBMIT` |
| User access during lock | Login, accounting, reports, historical statements and the owning filing workflow remain available; only tree mutation/successor publication is gated. | PASS — Tax Hub, Cash Statement, Cash Manager and Category Tree review remained available while mutations were suppressed |
| Pre-dispatch crash | A retained preparation with no dispatch claim is resumed or conclusively invalidated/released without an age-based unlock. | PASS — staging is idempotent for the retained preparation, conclusively stale preparation can be durably invalidated, and the 452-assertion boundary suite rejects replay or silent unlock |
| Uncertain dispatch | A claimed send with no conclusive outcome remains locked and is reconciled without blind resubmission. | PASS — the acknowledged/Pending CH attempt retained its lock and disabled re-preparation/resubmission until the explicitly simulated terminal outcome |
| Recovery | Restored database state detects any disagreement with externally retained seal evidence; it does not rewrite that evidence. | PASS — a deterministic restored-behind node fixture presents an older version/digest to newer retained CH evidence; staging fails closed and a subsequent evidence read returns the original version, snapshot hash and canonical evidence unchanged |
| Performance | Active accounting and Tax Hub reads remain acceptable; warm active Equity Bridge stays within the agreed sub-second target where the service tier permits. | DEFERRED BY OWNER — B8 S0 returned the correct active bridge in 59.107 seconds and exact parity in 245.157 seconds. This isolated Equity Bridge performance issue will be revisited separately and does not reopen Category Tree correctness or change-control acceptance |

## Evidence ledger

Add one row for every execution or owner review. Keep credentials, access tokens, full statutory payloads and private authority references in ignored evidence only.

| Date/time (Europe/London) | Test | Actor | Evidence/result | Status |
|---|---|---|---|---|
| 6 Nov 2026 | Baseline commissioning | Codex + owner | B8 registered, initialised, upgraded to S0, standard historical synthetic dataset generated and Carol Hughes VAT profile aligned to exact `18A2` period. | PASS |
| 6 Nov 2026 | Baseline Category Tree | Codex | `CV00000001`; 53 categories, 18 links, 38 mappings, 2 roots; canonical digest and length recorded above. | PASS |
| 6 Nov 2026 | Historical anchor correction | Codex | `App.proc_BasicSetup` changed to publish the first version on `@AsOfDate`; B8 corrected to 30 Jun 2017; SQL build passed. | PASS |
| 6 Nov 2026 | Shared statutory provenance contract | Codex | VAT seal profile v2 and the CH/CT accounts snapshot bind `VersionId`, canonical profile, SHA-256 and byte length; preparation and pre-dispatch checks reject active-pointer/content drift. Tax Hub application tests passed 90 assertions and TCWeb Tax Hub boundary tests passed 438 assertions. | PASS — local |
| 6 Nov 2026 | Durable lock deployment | Codex | SQL build `4.1.21` published serially to hosted B3-B6 and B8 with constraint/data validation; the shared integration app deployed runtime-successfully and `business8.tradecontrol.co.uk` returned HTTP 200. No statutory submission was initiated. | PASS — deployment |
| 6 Nov 2026 | Hosted durable-lock boundary | Codex | B8 rejected stale canonical evidence, successor publication, direct category mutation, competing ownership, wrong-owner release and the retired manual-submit path. Idempotent acquisition preserved the original timestamp; conclusive release cleared only the matching lock; rollback-only completion produced the expected Submitted event. Final evidence restored Published `CV00000001`, the baseline SHA/11,501 bytes, no lock and no acceptance Draft. | PASS — hosted SQL |
| 6 Nov 2026 | Deployed Tax Hub smoke | Codex + owner | Authenticated B8 Tax Hub loaded after an approximately 50-second cold initialization. Dashboard Equity Bridge passed 2/2; VAT Statement and VAT Submission rendered the aligned 2017-18 figures, including £713.26 net VAT for JUL. HMRC remained locally signed out, so no preparation or authority request was attempted. | PASS with HMRC connection pending |
| 6 Nov 2026 | Equity Bridge hosted optimisation | Codex | B8 isolated the regression to recursive category membership being expanded inside the version-aware P&L function. Separating category recursion from Cash Code mapping retained exact membership, P&L and Equity Bridge outputs; warm P&L improved from 3.4–4.0 seconds to 30 ms and warm Equity Bridge from 10.9–29.5 seconds to approximately 1.2 seconds on S0. The SQL project built and the five-iteration equivalence gate passed. | PASS |
| 6 Nov 2026 | Progressive Tax Hub deployment | Codex | The shared integration app deployed runtime-successfully from Release package SHA-256 `3E6412A4DA95928A11C4C99AB821444602F76C7997798390FB7FAA18B7490D29`. The shell, ordinary dashboard, Accounts Validation, Payload Audit and filing-reconciliation warning now load in stages behind the existing scoped-context gate. TCWeb built with zero warnings and 441 Tax Hub boundary assertions passed. | PASS |
| 6 Nov 2026 | VAT `18A2` preparation and live lock | Codex + owner | Carol Hughes connected to HMRC sandbox. Obligation `18A2` (1 Apr–30 Jun 2017) prepared with PASS/no findings. The immutable review records Category Tree `CV00000001`, baseline profile/SHA and 11,501 bytes; B8 retained a durable workflow lock and rejected a direct category mutation with error 51097. Prepared Box 5 is £1,350.29: the local JUL row is £713.26 and the statutory adapter adds the absolute £637.03 acquisition-VAT Box 2 when recomputing Boxes 3 and 5. Legal declaration, approval and submission remain untouched pending owner review. | PASS — preparation/lock; authority pending |
| 9 Nov 2026 | Synthetic White Van asset correction | Codex + owner | Invoice `030722.SA` already retained the correct £4,166.67 net and £833.33 VAT split. B8's asset entries were corrected to one £4,166.67000 capitalisation and five £833.33400 write-offs while the supplier bank payment remains £5,000 gross. The closed-period editor had rebuilt the account but left `Cash.tbPeriod` stale; after the existing system rebuild, the 2016–17 Equity Bridge variance returned to £0.01. The 2017–18 £833.34 variance is the expected scheduled 31 Mar 2018 write-off, still unposted until the period-close workflow. | PASS — accounting correction; period close pending |
| 9 Nov 2026 | Closed-period correction rebuild | Codex | TCWeb now invokes the existing financial system rebuild after a closed/archived-period edit and reports rebuild failure instead of silently leaving projections stale. Synthetic Opening Balance rows no longer expose payment-maintenance actions. Release deployment started `18:14:28Z`; both B8 and the default App Service health endpoints returned HTTP 200. | PASS — deployed |
| 9 Nov 2026 | Synthetic asset generator regression | Codex | `App.proc_DatasetSyntheticMIS_Assets` now derives recoverable-VAT capital value from `App.tbTaxCode`, retains the gross supplier payment, and uses five-decimal write-offs consistent with `Cash.proc_AssetReversal`. The completed-year regression asserts the gross payment, net capitalisation and five equal write-offs. SQL project build passed; changes remain local for owner review. | PASS — local |
| 7–9 Nov 2026 | Successor and historical statement evidence | Owner + Codex | Submitted `CV00000001` was cloned, reviewed and replaced by active Published `CV00000004` from 1 Jul 2017. The Indirect Costs/plant classification, historical selector, affected-period warning and Settings provenance were exercised through the UI. Archived `CV00000001` retained its baseline digest and produced the expected 2017–18 Equity Bridge difference of `-£33,333.33`, equal to the later net plant capitalisation. Twelve synthetic screenshots are retained for the User Guide. | PASS — CH filing pending |
| 9 Nov 2026 | Active Equity Bridge restoration | Owner + Codex | The White Van synthetic asset was corrected to its £4,166.67 VAT-exclusive capital value and equal £833.334 write-offs; the closed-period rebuild boundary was corrected. The owner confirmed that the operational Equity Bridge now passes. | PASS |
| 9 Nov 2026 | Public demonstration access | Codex + owner | The accidental main-site commissioning-IP restriction was removed from the B8 App Service while the SCM/deployment endpoint remained restricted to `78.148.7.78/32`. Public health and root requests returned HTTP 200 and the owner confirmed that B8 loads on iPhone. Mobile interaction differences are deferred to a dedicated project and are not a Category Tree acceptance result. | PASS — reachability only |
| 10 Nov 2026 | Companies House preparation and dispatch | Codex + owner | Exact approved document SHA-256 `EC41F30186AA2AE6641D064F508792282E3467B4D08591F957F3CDBDA60EF1E1` and package SHA-256 `79CD3E609DEBB6FA114C233CC895CA33E18F582EBDC6920DAC043E4E9E71DA21` were dispatched once as `S00001`. Companies House acknowledged the package and reports `Pending review of attachment`; the durable conversation prevents resubmission. | PASS — acknowledged/Pending, not accepted |
| 10 Nov 2026 | Development acceptance simulation | Codex | The official-test-only red `Simulate Acceptance` control used the normal terminal-completion boundary, explicitly recorded that Companies House did not return the acceptance, and changed `CV00000004` to Submitted/read-only. | PASS — lifecycle simulation only |
| 10 Nov 2026 | Post-CH successor publication | Codex | Submitted `CV00000004` was cloned to `CV00000005`; review recorded added category `CA-RND` and edge `CT-OVERHD → CA-RND`. Validation produced 11,992 canonical bytes. Publication from 1 Apr 2018 atomically archived `CV00000004` at 31 Mar 2018 and activated Published `CV00000005`; `CC-RND` was then created in the active tree. | PASS — transaction comparison pending |
| 10 Nov 2026 | Post-CH R&D transaction and close | Owner + Codex | Purchase invoice `030729.SA` for Jones Machine Tools Ltd posted at £1,200 gross (£1,000 net plus £200 VAT) to `CC-RND`; payment `SA_261110_173948_796` settled it exactly. The owner closed April 2018 through Admin Manager. CV5 passed its active Equity Bridge, while archived CV4 failed by exactly £1,000 because it cannot classify the later R&D Cash Code. | PASS |
| 10 Nov 2026 | Post-transaction canonical and algorithm evidence | Codex | CV5 was explicitly revalidated through the Category Tree UI at 12,038 canonical bytes, SHA-256 `47EE11D776CAC487F1F5C30A4893BC0477D0BCA1F2344333FFEADBAA889E239E`. A bidirectional `EXCEPT` between the operational Equity Bridge view and explicit CV5 function returned zero rows. CV1 and CV4 retained their original canonical lengths and digests. | PASS |
| 10 Nov 2026 | Hosted S0 reporting timing | Codex | Direct read-only SQL isolated the delay below workbook rendering: active Equity Bridge returned maximum absolute variance £0.01 in 59.107 seconds; the exact active-view/CV5 parity comparison returned zero differences in 245.157 seconds. | FAIL — performance only; functional evidence unaffected |
| 10 Nov 2026 | Restored-behind seal recovery | Codex | The TCWeb boundary suite now presents an older restored Category Tree version, digest and byte length to a newer retained CH preparation/approval. Staging rejects the mismatch; reopening the retained snapshot returns its original Category Tree and accounts-snapshot evidence unchanged. The complete suite passes 452 assertions. | PASS |
| 10 Nov 2026 | Web acceptance gate | Owner + Codex | Owner-led desktop and mobile acceptance exercised Published editing, Draft clone/review/publication, Submitted and Archived read-only behaviour, Cash Statement version selection/warnings and the retained JavaScript tree. Because this is a two-person pre-launch project, the owner accepted this executed browser evidence rather than duplicating it in a new Playwright journey. | PASS |
| 10 Nov 2026 | Final technical gate | Codex | TCWeb built with zero warnings/errors; its boundary suite passed 452 assertions. The Tax Hub solution built under installed SDK 10.0.403 without changing its 10.0.204 repository pin; offline company, application, submission, Trade Control adapter, architecture, WebHarness, Companies House simulator, SA and VAT suites passed 645 assertions. The tcNodeDb4 DACPAC built successfully and the User Guide production build generated 72 pages. | PASS |

## Next-session restart point

The Category Tree project is closed. Revisit the separately deferred Azure S0 Equity Bridge performance issue as its own follow-up without changing the accepted algorithm or weakening the exact parity gate.

The mobile presentation issues observed on iPhone are deliberately outside this restart sequence. The public B8 endpoint is reachable; responsive/mobile acceptance will be planned separately rather than mixed into the statutory changeover evidence.

## Completion gate

This hosted acceptance is complete only when both statutory channels have produced verified test receipts bound to their precise Category Tree versions, both earlier seals reconstruct after successor publication, the plant scenario explains the historical Equity Bridge failure without compromising the active bridge, all invariant/tamper/recovery checks pass, and the owner has reviewed the recorded evidence.

## Hosted submission-lock UI acceptance — 6 November 2026

- Preparing VAT period `18A2` acquired a durable lock on Published version `CV00000001`.
- A direct Category mutation was rejected by `Cash_tbCategory_TriggerVersionGuard` with error `51097`, confirming the database invariant.
- The first hosted UI check exposed a capability-projection defect: the workspace passed `hasSubmissionLock: false`, so the page still described the version as editable and rendered mutation controls.
- The workspace now derives the flag from `Cash.tbCategoryVersion.SubmissionLockReference` and projects it through the common capability contract.
- The deployed page retains the lifecycle status `Live · Published`, displays `Filing in progress`, explains the temporary read-only state, suppresses clone/validate and tree mutation controls, and disables JavaScript mutation paths.
- A locked category detail was opened after deployment; it displayed read-only data with no edit, delete, disable, add-category, new-total or new-category actions.
- TCWeb build: passed with zero warnings and zero errors.
- Automated boundaries: 26 Cash Statement assertions and 443 Tax Hub assertions passed, including locked and unlocked Published capability regression checks.
- Azure deployment `1084ad8d-dc8b-4d18-bf7a-a16e3c42fe20`: `RuntimeSuccessful`, one successful instance, zero failures.
- Deployment package SHA-256: `005FBD42FED0DDB8CBAA2A2BC5A977733F379D7A4BE4C83A91B02FAE8F5C96FB`.

## VAT submission acceptance — 6 November 2026

- VAT period `18A2` was accepted by the HMRC sandbox with `HMRC-SUCCESS` (`HTTP 201`).
- Processing time: `2026-11-06 19:09:34Z`.
- Attempt reference: `bbd45964ea9745cb967adea60d2c4762`.
- HMRC correlation: `9cc2a65d-794a-40f4-9904-04e3a6337afe`.
- Form bundle: `612767305340`; payment indicator: `DD`; charge reference: `4rvT8G04V64El1Ah`.
- The filing-history evidence confirms that the approved and dispatched payload digests match.
- HMRC read-back confirms that all nine retrieved VAT values exactly match the approved return.
- An authority-obligation refresh reports period `18A2` as already submitted, with read-back available.
- Active Category Tree `CV00000001` transitioned from Published (`1`) to Submitted (`2`).
- Submission event `16` records event type `4`, Published-to-Submitted, at `2026-11-06 19:09:35`, by Carol Hughes, with note `HMRC VAT submission accepted (HMRC-SUCCESS).`
- The durable submission lock was cleared only after the conclusive accepted outcome.
- The event evidence digest equals the retained canonical digest: `1B067CC83D99EB90BDB1BFC58B9866C4C3573CE0A7788F86C3F2B8603BE0DBF7`.
- Canonical evidence remains `11,501` bytes and `App.tbOptions.CategoryVersionId` remains `CV00000001`.
