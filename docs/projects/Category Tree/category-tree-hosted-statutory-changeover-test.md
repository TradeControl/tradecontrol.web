# Category Tree hosted statutory changeover test

## Status

**In progress on hosted Business 8 (`tc-business8`). Baseline commissioning passed on 6 November 2026; statutory-seal integration is the next gate.**

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
| HMRC authorisation | Sign in as the reviewed Carol Hughes sandbox user and grant the requested VAT authority. | Pending |
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

**Status: in progress. Canonical provenance integration passes locally; the durable SQL submission lock and authority-outcome lifecycle remain the gate to hosted execution.**

Before either authority journey, prove both filing paths consume one shared Category Tree provenance contract.

| Test | Expected evidence | Result |
|---|---|---|
| Seal input | Version ID, canonical profile, SHA-256 and byte length are explicit fields, not a broad row-version digest. | PASS — local contract |
| Preparation | Prepared VAT and CH artifacts retain the selected active version and digest. | PASS — local contract |
| Re-preparation | Approval and dispatch re-resolve the version and reject changed content or pointer drift. | PASS — local contract and pointer-drift test |
| Durable lock | A prepared/approved operation prevents Published mutations and successor publication until conclusive outcome. | Pending |
| Failure semantics | Conclusive rejection releases the lock; uncertain transport retains it; success records the seal relationship. | Pending |
| Success transition | Authority success changes Published to Submitted without changing canonical bytes. | Pending |
| Additional filing | An unchanged Submitted version can support another seal without becoming editable. | Pending |
| Tamper proof | One-bit canonical or seal mutation fails verification. | PASS — local canonical-vector tests |

## Test 2 — VAT submission seals version 1

**Status: pending Test 1.**

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
| Active version before preparation | `CV00000001`, Published | Pending |
| Prepared seal Category version | `CV00000001` and baseline digest | Pending |
| Mutation/clone rejection while locked | Rejected atomically | Pending |
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

## Completion gate

This hosted acceptance is complete only when both statutory channels have produced verified test receipts bound to their precise Category Tree versions, both earlier seals reconstruct after successor publication, the plant scenario explains the historical Equity Bridge failure without compromising the active bridge, all invariant/tamper/recovery checks pass, and the owner has reviewed the recorded evidence.
