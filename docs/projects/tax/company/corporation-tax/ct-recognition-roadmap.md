# Corporation Tax recognition roadmap

## Goal

Reach the point at which Trade Control can apply for HMRC Corporation Tax software recognition and
listing for its explicitly supported filing profile. Recognition is the public HMRC term; this
roadmap uses it for the programme goal sometimes described as accreditation.

Recognition is not the same as enabling production filing. Test-service execution, recognition,
listing and production activation retain separate evidence and human gates.

## Current position — 18 November 2026

| Layer | Position | Remaining gate |
|---|---|---|
| Accounting source and Category Tree | Company statutory sources and mappings are implemented; Category Tree Change Control is available, and CT-A supplies accepted N, D, A, rates, eligibility facts and marginal relief from the live statement path. | Preserve the accepted current-law baseline; do not alter the accounting dataset to avoid a tax scenario. |
| Objective 3 contracts and population | The approved ordinary micro-company surface carries the accepted marginal-relief calculation, associated-company facts and provisional-rate filing boundary into the same accounts, computation and CT600 artifact path. | Retain specialist regimes outside the supported scope. |
| Phase 5.8 service artifact | CT2025 computation, FRC 2026 accounts and RIM 1.994 assets remain pinned. The refreshed positive-D artifact passes the official CT2025 package in Arelle, HMRC LTS 8.3 and one separately authorised genuine TPVS validation, including its IRmark and tax calculator. MR-5 was accepted on 17 November 2026. | Local LTS and genuine remote TPVS evidence must retain their distinct classifications. |
| Phase 5.9 Transaction Engine | Complete and reviewed for offline protocol fixtures, IRmark and durable conversation semantics. | Confirm current ETS class, credentials, endpoints and receipt behaviour immediately before the external test. |
| Phase 5.10 local ETS boundary | CT-B and CT-C are accepted. The Development-only, authenticated diagnostic completed an identity-corrected ETS conversation with a genuine HMRC `BusinessResponse`, retained IRmark receipt, zero returned errors and acknowledged delete. Production remains disabled. | Implement CT-D as the coherent production-shaped filing journey. Do not open production submission before its gate is accepted. |
| Multi-tenant hosting | The shared production-shaped environment prerequisite is complete. | Exercise CT through the same tenant, identity, protected-content and operational boundaries rather than treating the WebHarness as the commercial product. |
| TCWeb CT product journey | Only the accepted Companies House accounts-snapshot hand-off exists. There is no complete CT preparation, review, declaration, approval, submit/poll/recovery UI. | Implement and review the production-shaped CT workflow before claiming product maturity. |
| HMRC recognition | Not applied for and not claimed. | Complete own/ETS testing and the mature product journey, then request and execute HMRC's current new-applicant scenarios. |

The earlier pause for the multi-tenant environment is cleared. Corporation Tax may resume locally.
No external ETS request or production submission is authorised by this document.

## How the existing plans relate

### Work Plan 3 — original contract-model plan

`tax-hub-workplan-3.md` is the historical Objective 3 design plan. It established the correct
contract boundaries but was not maintained as a progress tracker. Its broad wording was later
narrowed to the approved initial product profile.

| Work Plan 3 phase | Disposition |
|---|---|
| 1 — Repository reconnaissance | Complete; findings and the neutral company-contract assembly established the architecture. |
| 2 — CT600 core | Complete for the supported semantic surface, with the full official RIM generated at the wire boundary. It is not a claim that every CT600 business scenario is supported. |
| 3 — Supplementary pages | CT600A is implemented conditionally. Other specialist pages remain outside the initial supported profile rather than silently supported. |
| 4 — Computation model | Complete for the approved ordinary trading micro-company, profit/loss, depreciation, allowance and loss semantics used by the initial product. |
| 5 — Accounts attachment | Complete for the FRS 105/FRC 2026 supported profile. |
| 6 — Submission package | Complete for one CT600, computation, full accounts and conditional CT600A per CT period. |
| 7 — Serialization and fixtures | Complete for the supported profile, including current/comparative accounts, long-period allocation, profit/loss and CT600A coverage. |
| 8 — Contract review | Superseded and completed through Work Plan 4 plus Phase 5.8 authoritative-asset assurance. |
| 9 — Trade Control reconciliation | Superseded and completed through Work Plan 4 CO1–CO6 and the company statutory source/mapping work. The selected test dataset still needs a current change-controlled rebaseline. |

Work Plan 3 is therefore not the next execution queue.

### Work Plan 4 — Objective 3 population

Work Plan 4 is the completion record for database projection, reviewed inputs, deterministic iXBRL,
CT600 population, package preparation and the immutable Objective 4 hand-off. Its Objective 3 gate
passed on 21 September 2026. Phase 5.8 subsequently replaced its missing-taxonomy deferral with the
authoritative CT2025 package and genuine validation evidence.

### Work Plan 5, Phase 5.10 — current technical phase

Phase 5.10 is the immediate execution phase. It combines the approved Objective 3 artifact with the
Phase 5.9 Transaction Engine conversation and proves the credentialled HMRC External Test Service
journey. It is necessary but not sufficient for recognition because the current operation is a
development WebHarness preflight, not the mature multi-tenant CT product journey.

## Supported recognition scope to freeze

Before asking HMRC for scenarios, explicitly freeze the smallest product claim:

- UK private company filing for itself, not agent filing;
- ordinary unaudited FRS 105 micro-entity accounts;
- CT600 Version 3 using the release actually active in HMRC test/live services;
- full accounts and computation iXBRL generated within Trade Control;
- Corporation Tax computational 2025 and FRC 2026 taxonomies where applicable;
- one or two Corporation Tax periods derived from one accounts period;
- CT600A only when its supported conditions apply; and
- Companies House followed by Corporation Tax as one guided product journey, with distinct
  approvals, credentials, transports and authority outcomes.

Do not claim specialist supplementary pages, charities, insurance, tonnage tax, ring-fence trades,
R&D schedules or other regimes merely because the generated RIM contains their wire types.

HMRC currently identifies CT600 V3 (2026) RIM 1.994 as implemented in LTS, TPVS and live. RIM 1.995
is the published 2027 successor but is awaiting implementation. Re-check this immediately before
each formal ETS or recognition run; do not migrate while the target service does not implement it.

## Test-environment evidence boundary

The first end-to-end case may use one disposable integration-test tenant. Its concrete tenant,
server, database and availability details belong in protected operational evidence rather than this
public roadmap. Add another test tenant only when an assigned scenario requires a materially
different company/profile, an isolation test requires simultaneous populated tenants, or consumed
authority state prevents truthful reuse. Every tenant must use the reviewed commissioning path and
retain its own identity and evidence boundary.

## Execution plan

### CT-A — test-tenant baseline and contract reconciliation

**Progress — 17 November 2026:** the current-period tenant is prepared through Objective 3 using the
reviewed current-law marginal-relief source. N, D, A, associated-company count, rates, limits,
eligibility facts, calculation branch and provenance now cross the preparation boundary explicitly.
The CT600 carries relevant distributions in box 620 as well as the CT2025 computation, allowing HMRC's
calculator to reproduce augmented profits and marginal relief. The refreshed immutable artifact
passes the official CT2025 package in Arelle and HMRC LTS 8.3 returns a success receipt after schema,
business-rule, IRmark and calculator validation. Protected tenant values, artifact identities and
response evidence remain outside the public repository. One explicitly authorised TPVS validation
returned HTTP 200 with messages `0000` and `077001` and no application findings. No ETS or production
request was made, and no retry was attempted.

The calculation redesign, MIS/What-if preservation rules and verification programme are governed by
the [marginal relief and real-time liability work plan](ct-marginal-relief.md).

1. Inventory the selected tenant's company profile, accounts period, CT periods, UTR/test identity, Category Tree
   change-control state and statutory-source readiness without changing accounting data.
2. Prepare accounts, computation, CT600 and service root through the existing Objective 3 path.
3. Compare values, provenance and SHA-256 identities with the approved contract expectations.
4. Re-run local schema/rules, Arelle/CT2025 and LTS/TPVS diagnostics as appropriate.
5. Classify every difference as expected test evidence, an approved version change or a defect. No
   serializer/value change may be made merely to recover an old digest.
6. Prove the rate-status boundary: provisional rates remain available for tax accruals but produce
   `CT-RATE-PROVISIONAL`, prevent a submission-ready artifact and cannot pass the CT-B eligibility
   boundary. Repeat with an accepted non-provisional rate to prove the constraint clears mechanically.

**Gate accepted on 17 November 2026:** the test-tenant statutory values, supported scope,
change-control provenance and refreshed Arelle/LTS/TPVS evidence are accepted; no provisional
financial-year rate is present in the accepted artifact.

**MR-6 accepted on 17 November 2026:** the complete SQL suite passes on independently regenerated
marginal-relief and small-profits company datasets and the retained test company; common statement
contracts are mechanically jurisdiction-neutral; and retained performance evidence records the
accepted tier-aware trade-off without weakening the statutory calculation. This completes CT-A but
does not open CT-B, ETS or production submission.

### CT-B — Phase 5.10 ETS-ready boundary

**Implementation update — 18 November 2026:** the obsolete recognition blocker has been removed.
The preflight now evaluates four independent conditions: a bounded CT-A baseline-approval reference,
an exact SHA-256 match to the freshly materialised service artifact, availability of the protected
ETS configuration, and a bounded attempt-authorisation reference tied to the exact transaction ID.
Missing or mismatched conditions fail closed with distinct diagnostic codes. Even when all four are
satisfied, the diagnostic still reports `CT-ETS-EXTERNAL-SEND-DISABLED` and
`CT-PRODUCTION-SUBMISSION-GATE-CLOSED`.

The Phase 5.9 durable coordinator is now composed with the one-attempt HTTP gateway for
submit, `DATA_REQUEST`, poll and delete transitions. Each operation performs at most one HTTP
exchange, applies the response through the durable state machine and retains exact response bytes
and SHA-256 evidence. A Development-only authenticated WebHarness diagnostic exposes the bounded
operations; external exchange remains independently configuration-disabled and the production
family gate remains closed.

1. Confirm the current RIM/test-service release, `HMRC-CT-CT600` class, ETS endpoints, supplied test
   credentials, channel values, poll contract and receipt verification.
2. Remove `CT-RECOGNITION-EVIDENCE-PENDING` as an ETS blocker. Replace it with explicit baseline
   approval, exact-artifact approval, protected-configuration availability and named-attempt
   authorisation.
3. Introduce a dedicated official-test eligibility boundary. It may send only to the allow-listed ETS
   endpoints, must reject `CT-RATE-PROVISIONAL`, and must not promote or expose the production CT gateway.
4. Connect the Phase 5.9 durable submit → acknowledgement → poll → terminal → receipt → delete
   coordinator to the one-attempt HTTP gateway.
5. Retain exact request/response/receipt bytes and digests in protected tenant-scoped storage.

**Gate:** send-disabled preflight reviewed with no credentials or protected XML exposed. A first ETS
exchange requires separate contemporaneous human authorisation.

**Gate accepted on 18 November 2026:** the explicit baseline, exact-artifact,
protected-configuration and named-attempt checks are accepted together with the internal durable
coordinator-to-HTTP composition. The WebHarness remains unable to send, the production family gate
remains closed and no HMRC request has been made. CT-C may now stage one fresh B8 artifact and
transaction ID for contemporaneous review and authorisation immediately before its first ETS
exchange.

### CT-C — controlled External Test Service evidence

**Execution update — 18 November 2026:** the first separately authorised ETS conversation reached
HMRC and was deleted successfully after retaining its terminal result. It produced a genuine ChRIS
5005 business error because the protected test UTR in `GovTalkDetails` did not match the reviewed
tenant UTR in `IRheader`. The same run also exposed two transport defects: poll must use
`Qualifier=poll` with `Function=submit`, and delete must use the terminal response's returned
`ResponseEndPoint`. Both protocol corrections are covered by the adapter tests. Terminal routing is
now recovered from the exact retained, digest-verified response, so restart recovery does not rely
on mutable metadata.

The official-test preparation boundary now replaces only the UTR with the protected ETS identity
before Objective 3 preparation. Materialisation fails closed unless the resulting `IRheader` UTR
equals the outer Transaction Engine identity. Accounting values and statutory meaning are not
changed.

Following exact-artifact review and separate authorisation, transaction `20261118B8C00002`
completed the full acknowledgement → poll → terminal → delete sequence. HMRC returned a terminal
`BusinessResponse` with no error elements and an `IRmarkReceipt` containing `Signature` and
`Message`; the exact receipt was retained before delete. HMRC then acknowledged deletion. The
conversation is durably `Complete`, and production remains disabled. CT-C is ready for human gate
review; successful execution does not itself constitute recognition or production approval.

1. Execute one authorised test-tenant ETS submission.
2. Follow the returned polling interval; persist correlation and terminal evidence before deletion.
3. Exercise restart recovery and `DATA_REQUEST` only where the actual conversation requires it.
4. Record HMRC validation findings without changing approved statutory values to satisfy a test.
5. Correct evidenced defects through the appropriate Objective 3, transport or product boundary and
   repeat only with a fresh explicitly authorised transaction.

**Gate:** complete, durable, reproducible successful ETS conversation with accepted artifact
identities and no production capability enabled.

**Gate accepted on 18 November 2026:** the corrected conversation satisfied the mechanical gate
conditions and its retained evidence was accepted by human review. This acceptance opens CT-D only;
it does not enable production submission.

### CT-D — production-shaped TCWeb journey

CT-D turns the accepted calculation, artifact and Transaction Engine plumbing into a filing product.
It is not a cosmetic wrapper around the current **HMRC View**. The existing Business Tax workspace
mixes live tax forecasting, transaction-mapping diagnostics and filing artifacts in nested tabs; users cannot
readily tell what is a projection, what requires action, or what has actually been filed.

#### Product and navigation contract

For a company with a Corporation Tax reporting profile, the sidebar may retain the jurisdiction-
neutral **Business Tax** workspace label, but the workspace heading and filing language must say
**Corporation Tax**. Sole traders and partnerships must not see CT filing controls or CT-only cash
codes.

The company workspace will have four primary destinations:

1. **Overview** — current tax position, payment balance, filing status and next action;
2. **Calculation** — an understandable bridge from accounting profit to taxable profit and tax due;
3. **Losses and adjustments** — reviewable inputs and their effect on the computation; and
4. **File return** — one guided statutory journey, not another collection of technical tabs.

The current **HMRC View**, tag codes, cash-code mappings, raw payload rows and artifact digests are
valuable diagnostic evidence. They move behind a right-aligned, closed-by-default **Diagnostics**
link available only to Managers and Administrators. They are not the ordinary filing journey.

The global financial-year and month filters continue to control planning views. They must not
silently change a filing already in progress. **File return** selects a durable Companies House
accounts snapshot and its associated CT accounting period or periods; that filing scope remains
fixed until the draft is deliberately abandoned.

#### Continuous Companies House → Corporation Tax journey

The normal route is:

1. File accounts and obtain a terminal Companies House `Accepted` outcome.
2. On the accepted filing card, offer **Continue to Corporation Tax**. The action carries only the
   safe Companies House approval/conversation reference; it does not pass mutable transaction-derived values.
3. Resolve the accepted `ICompaniesHouseAccountsSnapshotService` record and verify the company,
   period, full-accounts SHA-256, accounts-snapshot SHA-256 and Category Tree seal.
4. Create or resume exactly one CT filing plan for that company and accounts snapshot. A long
   accounts period may require two CT returns; show both explicitly, share the same accepted accounts
   artifact, and give each return its own preparation, approval and HMRC conversation.
5. If Companies House is still pending, show **Check Companies House status** and explain that CT
   preparation may be previewed but submission remains locked. Never ask the user to guess whether
   acceptance has happened.
6. If the user enters through Business Tax instead, find and resume the same filing plan. Do not
   create a parallel journey or permit duplicate preparation.

The accepted accounts bytes are read through the cryptographic snapshot boundary and attached to
the CT service artifact unchanged. They are not substituted with a newly generated accounts document.
Before CT preparation, the system independently recalculates the accounting and tax projections from
Trade Control transactions using the bound statutory context and requires the accounting projection
to reconcile to the accepted accounts. A transaction, reviewed-input or Category Tree mismatch stops
the journey for review; it never silently changes the attachment. Later changes may affect planning,
but cannot mutate an approved or submitted return.

#### Overview and deadline experience

The overview answers five ordinary questions without requiring knowledge of CT600 tags:

- Which accounting period is this?
- How much Corporation Tax is currently expected, paid and outstanding?
- When is payment due and when is the return due?
- Are the accounts accepted and is the return ready to file?
- What should I do next?

Payment and filing are distinct. For the currently supported ordinary-company scope, display the
usual payment date of nine months and one day after the CT accounting period and the filing date of
twelve months after it, while preserving any more specific statutory schedule supplied by the
source. The product does not imply that filing pays the tax. These dates and the distinction between
them follow GOV.UK's [Company Tax Return overview](https://www.gov.uk/company-tax-returns/overview).

Show separate, plainly named cards for **Estimated tax**, **Paid**, **Outstanding**, **Pay by** and
**File by**. Label figures from the open order book **Forecast tax accruals**. Label the closed,
reviewed return figure **Return tax payable**. Never call tax accruals “forecast adjustments”.

#### Guided File return journey

Use a single vertical stepper with durable completion state. Reloading, signing out or changing
device must resume at the same safe step.

1. **Scope**
   - company name, company number and UTR;
   - accepted Companies House accounts period and snapshot identity;
   - one or two CT accounting periods and their payment/filing deadlines;
   - supported filing profile and explicit unsupported-regime findings.
2. **Accounts**
   - identify the accepted full-accounts iXBRL attachment;
   - show key accounts totals and reconciliation status;
   - allow safe preview/download of the exact retained accounts artifact;
   - provide a link back to the accepted Companies House evidence.
3. **Tax calculation**
   - accounting profit;
   - add-backs, including depreciation exactly once;
   - deductions, capital allowances, chargeable gains, losses and other reliefs;
   - taxable total profits `N`, relevant distributions `D`, augmented profits `A` and associated-
     company threshold facts;
   - small-profits, marginal-relief or main-rate branch and final tax charge;
   - reconciliation to `Cash.vwTaxBizStatement` and the accounts tax charge.
4. **Return details**
   - present CT600 answers in business language, grouped by topic;
   - collect only reviewed inputs that cannot be derived safely, including declaration details and
     supported conditional schedules;
   - show blocking explanations for unsupported supplementary pages or specialist regimes rather
     than silently omitting them.
5. **Documents and checks**
   - accounts iXBRL, computation iXBRL and CT600/service artifact listed by friendly name;
   - local contract validation, taxonomy validation and preparation findings;
   - exact SHA-256 identities under a technical disclosure;
   - no “ready” state while rates are provisional or any artifact is `Preview`, stale or invalid.
6. **Declaration and approval**
   - reproduce HMRC's correctness-and-completeness declaration in approved wording;
   - require an attributable authorised user and an explicit confirmation action;
   - bind the declaration, reviewer, timestamp, filing scope, accounts snapshot, Category Tree seal,
     source/review inputs and every artifact digest into one immutable approval digest.
7. **Submit and track**
   - show the exact company, period and tax payable immediately before the one-send action;
   - claim the durable transaction before network I/O and refuse duplicate submission;
   - after acknowledgement, return immediately to a calm status page rather than holding a browser
     spinner open;
   - server-side work observes HMRC's poll interval and durable recovery rules;
   - **Check status** remains available as a safe fallback and never creates a new submission.
8. **Outcome and evidence**
   - accepted: receipt, submitted document identities, submission time and downloadable safe evidence;
   - rejected: bounded HMRC errors translated into actionable language while retaining exact evidence;
   - recovery required: support reference and an explicit statement that resubmission is blocked;
   - deletion failure: preserve the terminal outcome and retry only the protocol cleanup;
   - source changes after acceptance do not rewrite history or imply that the filed return changed.

#### Durable workflow and authority boundaries

The TCWeb product workflow needs a persistent record distinct from the Development WebHarness. Its
minimum identity is tenant + company identity + accounts-snapshot SHA-256 + CT period. It records:

- `Draft`, `ReviewRequired`, `ReadyForApproval`, `Approved`, `Dispatching`, `SubmissionUncertain`,
  `AwaitingPoll`, `Accepted`, `Rejected` and `RecoveryRequired` product states;
- preparation, approval and conversation references;
- the accepted Companies House approval/conversation references and accounts proof;
- reviewed-input/source digest, Category Tree seal and exact artifact digests;
- transaction ID, correlation ID, poll eligibility and one-send claim;
- response evidence, receipt, deletion state and bounded support-safe errors; and
- actor and timestamp for every state-changing action.

Only `Approved` may cross the production CT submission gate. `Dispatching` is persisted before the
first HTTP byte is sent. `SubmissionUncertain` requires authenticated `DATA_REQUEST` recovery by
transaction ID; it never permits “try submit again”. `Accepted` and pending states prevent a second
filing for the same immutable identity. A rejected filing may be corrected only by creating a new
preparation and approval while retaining the rejected attempt.

Tenant/principal isolation, filing-role policy, protected credentials/content, anti-forgery,
duplicate refusal, a CT dispatch kill switch, request/response size limits, rate limits, audit,
monitoring and operational recovery are mandatory in the shared host. No raw authority URL,
credential, password, UTR or protected XML is written to ordinary logs or browser state.

#### CT-D delivery slices

Each slice must leave a reviewable UI and automated evidence; avoid building the entire journey
behind one final integration test.

1. **CT-D1 — information architecture and filing plan**
   **Status: accepted in the deployed B8 review UI; implementation remains uncommitted.** The new
   workspace and immutable identity contract are present, Diagnostics is closed by default, and no
   filing-plan persistence or submission action has been introduced.
   - replace the nested Business Tax/HMRC tab hierarchy with Overview, Calculation, Losses and
     adjustments, and File return;
   - add dynamic company language and hide CT controls for non-company profiles;
   - introduce the immutable CT filing-plan identity and continuation lookup;
   - keep privileged Diagnostics available without making it the default experience.
2. **CT-D2 — Companies House continuation**
   **Status: accepted in the development product journey.** The original 19 November 2026 review exercised
   an official-test submission and exposed an accounts tax defect. The corrected successor was subsequently
   acknowledged by the official-test gateway, accepted only through the explicitly labelled development
   simulation, and opened the single
   1 July 2025 to 30 June 2026 CT development scope. The File return workspace rediscovers a
   genuine accepted filing after reload, re-reads and verifies the protected full-accounts artifact,
   and derives one or two immutable CT-period identities. Multiple accepted filings require an
   explicit choice. Missing, pending, legacy and mismatched evidence fails closed. A simulated
   acceptance may open a visibly marked preparation/validation scope only when the host is pinned to
   the Companies House official-test environment and simulation facility; the same evidence is rejected
   by the production host and cannot qualify a genuine HMRC submission. The surface contains no CT
   approval or submission action. The readiness
   review also exposed and closed the remaining MR-3 Equity Bridge cutover defect: reconciliation now
   consumes the same Category-Tree-version-explicit jurisdiction-selected CT provider as the statement,
   with active parity, rollback-only Archived-predecessor reconstruction and tolerance tests preventing
   a return to the legacy flat-rate expense. The fresh-synthetic-business readiness path also now
   re-anchors its generated CT and Companies House profiles/settings to the complete accounting
   history without overlapping effective rows; B8 resolves the reviewed Companies House profile at
   the target period end.
   - add **Continue to Corporation Tax** to terminal accepted accounts;
   - resolve and display the cryptographically linked accounts snapshot;
   - block missing, pending, simulated, legacy or mismatched acceptance according to the approved
     product policy;
   - cover the one-accounts-period/two-CT-return case.
2A. **Companies House amended-accounts dependency**
   **Status: satisfied for the development journey.** The detailed correction lifecycle belongs to
   Companies House [Phase 7.7A](../companies-house/phase-7.7a-amended-accounts.md). CT retains only the
   fail-closed consumption rule: while a correction exists, the known-wrong predecessor snapshot is
   unavailable; CT may resume only from the latest accepted revised snapshot after accounting profit
   and the accounts Corporation Tax charge reconcile to current transactions. The corrected successor
   now reconciles both accounting profit and Corporation Tax charge to current transactions with zero
   difference. Its simulated acceptance remains development evidence and cannot qualify a genuine HMRC
   submission.
3. **CT-D3 — calculation and reviewed inputs**
   **Status: accepted in the deployed B8 product journey on 21 November 2026.** The product now re-reads
   the immutable accepted full-accounts artifact, rebuilds the Objective 3 calculation from current
   Trade Control transactions and presents the accounting-profit and profit-to-tax bridge without
   creating a filing artifact or submission action. The 19 November 2026 S00002 test reconciled its
   accounting profit but correctly failed closed because the accepted accounts contained a zero tax
   charge while the current marginal-relief calculation produced a non-zero charge. Investigation
   found that Companies House preparation had hard-coded both current and comparative tax-on-profit
   facts to zero. Future accounts preparations now derive both facts from the same filing-ready
   `Cash.vwTaxCorpLiabilityUK` boundary used by Objective 3, including the shared one/two CT-period
   allocation and the pre-approval stale check. S00002 remains immutable and unsuitable for CT; the
   UI requires a new valid accounts snapshot rather than altering CT merely to fit the old artifact.
   The corrected successor now proves the intended positive path: accepted accounting profit and tax
   charge both equal the current transaction-derived calculation, with zero difference. The local
   successor slice adds one durable review per CT accounting period for other add-backs, deductions,
   capital allowances, losses used, chargeable gains, other reliefs and conditional loans-to-
   participators inputs. Each review records the authenticated reviewer, timestamp, revision, input
   digest and source digest. A later source-version change retains the entered values but marks the
   review stale and closes readiness until an authorised Administrator or Manager reviews it again.
   Associated-company facts remain effective-dated reporting-profile inputs rather than being copied
   into the filing draft. The deployed B8 commissioning pass on 21 November 2026 saved neutral
   reviewed inputs as revision 1, retained the authenticated reviewer and timestamp after a full page
   reload, failed closed when a controlled source snapshot changed, and restored readiness only after
   an explicit re-review created revision 2. The source probe rolled back completely and changed no
   accounting values. The production Corporation Tax submission gate remained closed throughout.
   - rebuild a non-filing comparison projection from current Trade Control transactions and require
     it to reconcile to the accepted accounts snapshot; never substitute those comparison bytes for
     the accepted full-accounts attachment;
   - build the readable profit-to-tax bridge from accepted CT-A sources;
   - add attributable review/edit surfaces for supported adjustments, allowances, losses, reliefs,
     associated companies and conditional CT600 inputs;
   - invalidate only the draft when its reviewed source changes.
4. **CT-D4 — immutable preparation and document review**
   **Status: complete.** CT600 contains the declarant name and declaration date, so D4 collects
   those document facts before composing the immutable preview. CT-D5 remains a separate legal
   declaration and approval over the exact reviewed bytes; it must not regenerate the return.
   - compose the accepted accounts snapshot with computation and CT600 through Objective 3;
   - promote artifacts from `Preview` only through a new reviewed production-preparation boundary;
   - expose previews, findings, provenance and stale-artifact refusal.

   Practical delivery slices:

   1. **CT-D4.1 — preparation prerequisites:** resolve the reviewed Corporation Tax UTR from the
      statutory registration source without displaying or persisting it in clear text; collect the
      declarant name and declaration date needed in the CT600 document.
   2. **CT-D4.2 — immutable preview:** require reconciled accepted accounts and current CT-D3 reviews,
      compose each CT-period service root through `CorporationTaxPreparer`, retain the exact accounts,
      computation and CT600 service bytes, and record their SHA-256 digests and provenance centrally.
   3. **CT-D4.3 — document review:** show period, status, findings and digests; provide safe no-store
      previews/downloads of the retained accounts, computation and service document.
   4. **CT-D4.4 — staleness acceptance:** prove a later transaction, statutory identity, UTR or reviewed
      input change refuses the frozen preview and requires a new preparation. Never mutate an old one.

   **Acceptance result:** D4.1-D4.4 are implemented and accepted. The application reuses the
   Objective 3 source reader, populator and preparer; resolves one active reviewed UTR without exposing
   or retaining it in workflow metadata; stores append-only preparation metadata and exact protected
   preview bytes; and presents no-store document views, findings and SHA-256 digests to authorised
   Managers and Administrators. A later attributable review revision was proven to mark the existing
   preparation stale without altering its retained bytes or digests. A successor preparation received
   a new identity while reproducing the same document digests from the restored unchanged accounting
   inputs. Proposed reviewed inputs are now evaluated before persistence, so a value that breaks the
   accepted-accounts reconciliation is rejected without stranding the durable review. Every retained
   artifact remains explicitly `Preview`; there is no approval or submission action.
5. **CT-D5 — declaration and approval**
   **Status: complete.** The implementation adds an
   authorised Manager/Administrator declaration over the exact D4 preparation, an immutable approval
   manifest and digest binding the retained accounts, computation and service-document identities,
   and append-only approval evidence. Approval re-reads the current preparation, statutory identity,
   transaction source and protected document bytes immediately before recording the decision; any
   mismatch fails closed and requires a new preparation. The workflow schema migration is applied.
   Build, Tax Hub boundary and architecture tests pass. Deployed human acceptance on 22 November 2026
   proved the declaration, exact retained accounts, computation and CT600 service-document identities,
   immutable approval evidence and the separation of approval from submission. No submission was made.
   A transient workflow-database login failure had previously left the File return lookup waiting
   indefinitely; the UI now bounds that lookup, reports a safe retryable failure and cannot approve or
   submit while the accepted-accounts dependency is unavailable. The post-approval experience now
   identifies submission as the next step while explaining that it is not yet enabled. The production
   submission gate remains closed.
   - implement role-controlled declaration, immutable approval digest and approval evidence;
   - re-read and compare all bound identities immediately before approval and dispatch;
   - make approved records immutable and require a new preparation after any correction.
6. **CT-D6 — production Transaction Engine orchestration**
   **Status: in progress; local orchestration foundation implemented, production family gate closed.**
   HMRC's current [XML developer guide](https://www.gov.uk/guidance/basic-guide-for-xml-software-developers)
   and [Document Submission Protocol](https://www.gov.uk/government/publications/transaction-engine-document-submission-protocol)
   continue to identify `https://transaction-engine.tax.service.gov.uk/submission` as the live
   endpoint and distinguish end-user live credentials from SDST test credentials. The implementation
   pins that host in a production-only protocol profile; ETS is not trusted by that profile.

   The proven Phase 5.9 coordinator now depends on a persistence contract rather than its development
   file store. TCWeb supplies a serializable, tenant-partitioned shared workflow store with an immutable
   one-conversation-per-approved-period identity and keeps exact wire/response bytes behind the existing protected-
   content boundary. Only the exact current D5-approved preparation can be reconstructed as
   `SubmissionReady`; reconstruction re-reads and digest-verifies the retained accounts, computation
   and service-root bytes and does not invoke another serializer. The one-send transition persists
   `SubmissionUncertain` before network I/O. A server worker can perform due poll and delete exchanges
   independently of a browser circuit, while uncertain-send `DATA_REQUEST` recovery remains an
   explicit credentialled operation rather than an automatic resubmit.

   Both `Mode=Production` and `FamilyGateEnabled=true` are required to register the live orchestrator
   and worker. Defaults register a disabled service and no UI action calls the orchestrator. Live
   Government Gateway credentials cross the orchestration boundary only for the initial send or an
   explicit uncertain-send recovery; they are not retained in workflow metadata or logs. Because the
   password is necessarily part of the exact GovTalk request, retained wire evidence is application-
   encrypted with tenant-and-owner-bound persistent keys. Polling and deletion do not require fresh
   credentials, and automatic recovery is deliberately forbidden.
   Completed conversations remain bound to their approved-period identity, so cleanup cannot reopen a
   second-send path. The local fake-authority suite now proves one-send, lost-reply recovery, restart,
   poll, terminal evidence, delete retry and completion without contacting HMRC. No HMRC request has
   been made. A server-selected `DevelopmentFake` mode now surfaces the request-scoped credential and
   explicit one-send journey while replacing the network handler with an in-process authority double;
   the UI and retained response identify that evidence as simulated. The shared-store migration has
   been exercised without changing existing workflow evidence. Human exercise on 25 November 2026
   exposed and corrected a lower-case transaction-ID defect before any conversation was created. The
   safely retried journey then retained exactly one conversation, progressed through browser-independent
   poll and delete processing, and reached the terminal accepted fake-authority outcome. The boundary
   and Cash Statement suites pass after the correction. No HMRC request was made; the production family
   gate remains closed pending D6 acceptance and any separately authorised official interaction.
   - reuse the accepted Phase 5.9/CT-C serializer and state machine;
   - add the live credential provider and production endpoint only behind the closed family gate;
   - persist the one-send claim, background poll scheduling, recovery and delete cleanup;
   - keep official-test and production evidence unmistakably distinct.
7. **CT-D7 — status, outcome and support experience**
   **Status: implementation ready for deployed review.** File return now presents the filing lifecycle
   in business language rather than exposing protocol stages: approved, received, completing,
   accepted, rejected, uncertain-send and support-required each have distinct guidance. An uncertain
   send offers recovery of the existing transaction and explicitly forbids another submission. The
   latest durable result also appears on the Tax Hub dashboard; the background worker remains
   responsible for polling, so leaving the browser or dashboard open is not required.

   Managers and Administrators can download the retained terminal authority response from the owning
   business only. The download re-verifies its SHA-256 digest after application-level decryption,
   returns with no-store headers and is labelled as a receipt when the authority supplied an IRmark
   receipt. A collapsed Submission details section supplies the transaction ID, update time, safe
   support reference and request/response digests. Completed outcomes remain readable when the
   production submission gate is closed. The deployed accepted-path review on 25 November 2026 proved
   the dashboard notification, dedicated File return result card and authenticated receipt download;
   the downloaded bytes matched the retained authority-response digest. Negative and recovery-state
   presentation remains part of D8 rather than being inferred from the successful case.
   - implement pending, accepted, rejected and recovery-required pages;
   - add safe status refresh, receipt/evidence download and support references;
   - integrate Tax Hub dashboard next-action cards and notifications without browser-bound polling.
8. **CT-D8 — accessibility, security and operational review**
   **Status: in progress.** Automated coverage now includes owner-scoped conversation lookup,
   application-encrypted exact evidence, rejection of a different filing owner, duplicate-send
   prevention, immutable approval/source checks, role-gated outcome access and integrity-checked
   response download. Release build, Transaction Engine, architecture, Cash Statement and Tax Hub
   boundary suites pass. Direct presentation fixtures now exercise accepted, rejected, uncertain-send
   and recovery-required wording, alert roles and download labels without manufacturing authority
   evidence. The first deployed keyboard check passed: the receipt link and native Submission details
   disclosure were reachable in sequence, `Enter` expanded the disclosure, and its transaction,
   support-reference and digest terms were present in the accessibility tree. The remaining acceptance
   work is the complete screen-reader and responsive review; explicit restart/scale-out and worker-recovery exercises; review of safe
   customer wording for rejected and uncertain outcomes; and the final operator/customer support
   record. No production dispatch is enabled by D8 testing.
   - keyboard/screen-reader and responsive review of the complete journey;
   - cross-tenant, privilege, replay, stale-source, duplicate-send and protected-content tests;
   - restart/scale-out exercises against the shared persistence and background worker;
   - operator runbook and customer-facing supported-scope/help content.

#### Deliberate launch boundaries

The first recognised product journey supports the frozen ordinary UK company profile already
represented by Objective 3. Specialist regimes, unsupported supplementary pages, group relief and
quarterly instalment payment companies must fail with a useful scope explanation; they are not
approximated. Companies House Phase 7.7A supports replacement-method revised accounts for the frozen
ordinary-company profile only. HMRC CT600 amendments remain outside the first filing journey until
their own immutable correction workflow is implemented. An accepted return remains visible and
immutable; the UI directs the user to support rather than presenting an ordinary second Submit
button.

**Gate:** an authorised user can move from an accepted, cryptographically linked Companies House
filing through understandable calculation review, immutable approval, exactly-once submission and
durable HMRC outcome evidence on the production-shaped multi-tenant topology. The journey is
accessible, independently tested and usable without exposing raw payload tables. Production remains
closed until this gate is explicitly accepted.

### CT-E — comprehensive product testing

Build an evidence matrix covering at least:

- supported MIN and STD accounting projections where both remain in product scope;
- profit and loss cases, depreciation/add-back and reviewed allowance/loss inputs;
- current/comparative accounts and long-period allocation;
- conditional CT600A and rejection of unsupported supplementary pages;
- validation failures, credential failure, immediate gateway error and business rejection;
- pending/restart/recovery/delete failure and duplicate-send prevention;
- Category Tree version/change-control drift after approval;
- cross-tenant and foreign-reference denial; and
- accessible review, declaration, status and evidence retrieval.

Use the selected disposable tenant for the first end-to-end case. Add tenants only when the matrix demonstrates a real need.

### CT-F — HMRC recognition application

When CT-A through CT-E are complete:

1. confirm to SDST that the product has completed its own testing and ETS testing and is ready for
   release within the frozen supported scope;
2. request the current new-applicant recognition instructions and assigned scenarios;
3. execute each assigned scenario through the same approved product/transport path;
4. retain and provide the requested XML outputs and supporting evidence;
5. correct findings without broadening the supported scope silently; and
6. obtain written recognition and verify the intended entries on HMRC's commercial software list.

The intended listing claims are likely **Produce and submit CT600** and **iXBRL integrated Software
Application provider**, subject to HMRC's classification. Do not claim either until HMRC confirms it.

## Recognition application gate

Do not apply until all are true:

- the supported scope and exclusions are published and reflected in product behaviour;
- the selected tenant and any additional required scenarios pass current local and HMRC test services;
- the multi-tenant CT journey is complete rather than WebHarness-only;
- exact XML outputs can be reproduced from retained approved evidence;
- security, privacy, accessibility, operational recovery and customer-support evidence are current;
- no test relies on an unavailable or reserved environment;
- RIM/taxonomy/test-service selection has been rechecked; and
- the owner explicitly authorises the application and each externally consequential test action.

## Current verification baseline

Verified on 17 November 2026 without a remote HMRC request:

- `TaxHub.slnx` Debug build: succeeded, zero warnings/errors;
- Company Contract Tests: 115 assertions;
- Application Tests: 90 assertions;
- Submission Adapter Tests: 176 assertions;
- Architecture Tests: 32 assertions;
- WebHarness Tests: 87 assertions; and
- Data Provision CO1/CO2/CO6 corporate handoff: passed in supported offline mode;
- Trade Control Adapter Tests: 11 assertions;
- Companies House Simulator Tests: 31 assertions;
- MTD Income Tax Contract Tests: 91 assertions across 44 endpoint descriptors;
- VAT Contract Tests: 19 assertions; and
- HMRC LTS 8.3: accepted the protected materialised CT600 locally with no validation findings; and
- genuine HMRC TPVS: HTTP 200 with response messages `0000` and `077001` and no application findings.

The reviewed deterministic contract-test CT service-root SHA-256 is
`6EAF0C1A0A1A85452FBCA2449498723BB614C508FF99E0E13FEA720043FDFC87`.
Protected retained-company artifact and response identities remain with the local evidence.

## Official references

- [Corporation Tax software developer support collection](https://www.gov.uk/government/collections/corporation-tax-online-support-for-software-developers)
- [CT600 RIM artifacts and implementation status](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-ct600-rim-artefacts)
- [Taxonomies accepted by HMRC](https://www.gov.uk/government/publications/taxonomies-accepted-by-hm-revenue-and-customs/taxonomies-accepted-by-hmrc)
- [Basic guide for XML software developers](https://www.gov.uk/guidance/basic-guide-for-xml-software-developers)
- [Recognised Corporation Tax commercial software suppliers](https://www.gov.uk/government/publications/corporation-tax-commercial-software-suppliers)
- [Correcting and amending company accounts](https://www.gov.uk/annual-accounts/corrections-and-amendments)
- [Companies House technical interface specifications](https://www.gov.uk/government/publications/technical-interface-specifications-for-companies-house-software)
