# Category Tree hosted statutory changeover test

## Status

**In progress on hosted Business 8 (`tc-business8`). Baseline commissioning, build `4.1.21` statutory-lock deployment and the deployed VAT preparation/lock gate passed on 6 November 2026; owner approval and authority-outcome acceptance are next.**

This is the living acceptance record for the final Category Tree change-control boundary. Update each result in place as evidence is established. Do not infer a pass from implementation or from a successful authority response: every row records its own observed evidence.

## Purpose

The test proves one continuous business history across two statutory channels and a later accounting change:

1. a VAT submission seals the initial Category Tree version;
2. a deliberate successor supports Companies House accounts without changing the VAT evidence;
3. a further successor introduces the plant-purchase scenario; and
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
| VAT filing | Review the nine boxes and explicitly approve the sandbox submission. | Pending |
| Companies House identity | Provide or confirm the test-company authentication/authority reference if it is not already configured. | Pending |
| Companies House filing | Review the rendered accounts and explicitly approve the sandbox submission. | Pending |
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

**Status: hosted SQL boundary and deployed VAT preparation/lock workflow passed; authority outcome remains pending.**

Before either authority journey, prove both filing paths consume one shared Category Tree provenance contract.

| Test | Expected evidence | Result |
|---|---|---|
| Seal input | Version ID, canonical profile, SHA-256 and byte length are explicit fields, not a broad row-version digest. | PASS — local contract |
| Preparation | Prepared VAT and CH artifacts retain the selected active version and digest. | PASS — local contract |
| Re-preparation | Approval and dispatch re-resolve the version and reject changed content or pointer drift. | PASS — local contract and pointer-drift test |
| Durable lock | A prepared/approved operation prevents Published mutations and successor publication until conclusive outcome. | PASS — hosted SQL boundary and live `18A2` preparation |
| Failure semantics | Conclusive rejection releases the lock; uncertain transport retains it; success records the seal relationship. | PASS — ownership/release SQL boundary; workflow recovery pending |
| Success transition | Authority success changes Published to Submitted without changing canonical bytes. | PASS — rollback-only hosted SQL transition; authority outcome pending |
| Additional filing | An unchanged Submitted version can support another seal without becoming editable. | Pending |
| Tamper proof | One-bit canonical or seal mutation fails verification. | PASS — local canonical-vector tests |

## Test 2 — VAT submission seals version 1

**Status: prepared and locked; owner approval/submission pending.**

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
| HMRC outcome | Accepted sandbox receipt for `18A2` | Pending |
| Version after success | `CV00000001`, Submitted; digest unchanged | Pending |
| Historical VAT reconstruction | Exact seal and nine boxes verify | Pending |

## Test 3 — successor version and Companies House accounts

**Status: pending VAT acceptance and CH test-company configuration.**

1. Clone Submitted `CV00000001` into a Draft successor and prove byte-equivalent starting content.
2. Make one reviewed classification change that does not introduce the later plant Cash Code.
3. Validate and publish the successor with an effective date after its predecessor.
4. Confirm `CV00000001` becomes Archived, remains immutable and still verifies against the VAT seal.
5. Prepare the appropriate micro-entity accounts through the CH evidence/review path.
6. Record the successor version and digest in the CH statutory seal and acquire the same authoritative submission lock.
7. Owner reviews and submits the accounts.
8. Confirm the CH receipt, seal verification and successor Published-to-Submitted transition.

| Evidence | Expected result | Observed result |
|---|---|---|
| Draft clone | Canonically equal to `CV00000001` before edit | Pending |
| Successor publication | One active Published version; predecessor Archived | Pending |
| VAT predecessor after publication | Seal and reconstruction unchanged | Pending |
| Prepared CH seal | Successor version and current canonical digest | Pending |
| CH outcome | Accepted test submission and durable receipt | Pending |
| Successor after success | Submitted and immutable | Pending |

## Test 4 — post-CH plant purchase and Equity Bridge

**Status: pending Test 3.**

Create a further successor because the CH-supported tree is immutable. Reuse the local scenario's accounting method:

1. clone the CH-submitted version;
2. create `CA-INDCST — Indirect Costs` beneath Overheads;
3. validate and publish the successor;
4. add `CC-PLANT — Plant and Machinery` only to the new active version;
5. create supplier **Jones Machine Tools Ltd**;
6. transfer £50,000 from Reserve to Current;
7. buy and capitalise a £40,000 lathe;
8. write it off over four years using the existing plant asset/depreciation path;
9. close the affected period and run the mandatory rebuild; and
10. compare the active default, explicit active successor and explicit CH predecessor without repointing `App.tbOptions`.

The active Equity Bridge must pass within the established rounding tolerance. The historical CH version is expected to expose a failure because it cannot classify the later `CC-PLANT` transactions. Do not copy the local `-£26,999.95` result as an assumption: B8 uses a different temporal and transaction fixture. Record the observed difference and prove it equals the omitted after-tax plant classification effect plus the accepted active rounding variance.

| Evidence | Expected result | Observed result |
|---|---|---|
| Active pointer | Further successor throughout explicit historical reads | Pending |
| Active bridge | Passes within tolerance after rebuild | Pending |
| Explicit active function | Exactly equals operational `Cash.vwEquityReconciliationByYear` | Pending |
| CH predecessor bridge | Fails visibly by the mathematically explained post-supersession amount | Pending |
| VAT predecessor bridge | Remains independently selectable; no active fallback | Pending |
| Cash Statement XLSX/ODS | Active and eligible historical versions generate with correct Settings provenance and warnings | Pending |
| Earlier statutory seals | VAT and CH seals still verify against their original versions | Pending |

## Test 5 — final invariants and recovery evidence

**Status: pending Tests 1–4.**

| Test | Expected result | Observed result |
|---|---|---|
| Active-version invariant | Exactly one `App.tbOptions.CategoryVersionId`; target is Published or Submitted. | Pending |
| Immutable history | Direct SQL, EF, handler and JavaScript mutation attempts fail for Submitted/Archived versions. | Pending |
| Canonical isolation | Lifecycle changes do not alter semantic digests; content changes do. | Pending |
| Seal inventory | Each VAT/CH seal names exactly one retained version and digest. | Pending |
| Restart/retry | Prepared locks and authority attempts survive application restart without replay or unlocked mutation. | Pending |
| User access during lock | Login, accounting, reports, historical statements and the owning filing workflow remain available; only tree mutation/successor publication is gated. | Pending |
| Pre-dispatch crash | A retained preparation with no dispatch claim is resumed or conclusively invalidated/released without an age-based unlock. | Pending |
| Uncertain dispatch | A claimed send with no conclusive outcome remains locked and is reconciled without blind resubmission. | Pending |
| Recovery | Restored database state detects any disagreement with externally retained seal evidence; it does not rewrite that evidence. | Pending |
| Performance | Active accounting and Tax Hub reads remain acceptable; warm active Equity Bridge stays within the agreed sub-second target where the service tier permits. | Pending |

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
