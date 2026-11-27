# CT-E product test matrix

## Purpose and status

**Evidence status — 27 November 2026:** in progress. This matrix records Trade Control's own
pre-application product testing for the proposed Corporation Tax recognition scope. It does not
claim HMRC recognition, production access or completion of scenarios assigned by HMRC.

The matrix separates evidence already obtained from the remaining CT-E acceptance work. Public
references identify reproducible tests and safe summaries. Credentials, tax identifiers, exact
credentialed messages, tenant identities and unrestricted authority responses remain in protected
evidence storage.

Status terms are used consistently:

- **Complete** — the intended result has identifiable evidence and no further CT-E execution is
  currently required;
- **Partial** — useful evidence exists, but a stated execution or owner review remains; and
- **Outstanding** — the required CT-E evidence has not yet been obtained.

An implementation or automated-test result is not owner acceptance unless the **Owner review**
column says **Accepted**.

## Calculation and filing scope

| Scenario | Expected result | Evidence obtained | Status | Owner review / next action |
| --- | --- | --- | --- | --- |
| Supported MIN and STD company projections | Both supported accounting projections produce coherent accounts and Corporation Tax inputs without widening the filing scope. | Deterministic SQL, company-contract and data-population coverage exists; the supported scope is recorded in the [recognition dossier](corporation-tax-recognition-evidence.md). | Partial | Consolidate the latest MIN/STD results into one CT-E execution record. |
| Small-profits, marginal-relief and main-rate boundaries | The statutory kernel selects the correct basis and reconciles tax before relief, relief and tax due. | `CorporationTaxMarginalReliefAuthority.sql`, `CorporationTaxMarginalReliefBoundaries.sql` and the accepted [marginal-relief programme](../ct-marginal-relief.md). | Complete | Accepted during CT-A; retain in the final regression. |
| Relevant distributions | Distributions are excluded from taxable profits and included in augmented profits without becoming taxed trading profit. | `CorporationTaxRelevantDistributions.sql`, annual calculation review and genuine validated service artifacts. | Complete | Accepted during CT-A; retain in the final regression. |
| Loss followed by profitable recovery year | The first year creates a genuine accounting and tax loss with no tax due; the next year uses the opening loss and taxes only the residual profit. | `CorporationTaxLossCarryForwardRecovery.sql` passed on 27 November 2026. The accepted calibration produced a first-year accounting loss of £3,524.58, a tax loss of £8,516.58, full second-year use and residual Corporation Tax of £271.77. It also reconciled to the active annual calculation and Equity Bridge. | Complete | Observed in the local product on 27 November 2026; formal CT-E matrix acceptance pending. |
| Depreciation add-back and reviewed allowances | Book depreciation is added back once; reviewed allowances and adjustments remain attributable and reconcile into the computation. | Contract/population suites and the artifact assurance recorded in [Phase 5.8](../phase-5.8-corporation-tax-service-artifact-assurance.md). The earlier double-add defect was corrected before the accepted validation runs. | Complete | Include the accepted fixture identifiers in the protected application record. |
| Current and comparative accounts | The exact accounts contain distinguishable current and comparative facts and bind to the computation and CT600 period. | Company contract, data-provision and artifact-validation evidence; the completed product journey is documented by the [Company Corporation Tax User Guide](https://tradecontrol.github.io/tax/tax-hub-business-tax-company/). | Partial | Record one final frozen-artifact regression in the CT-E execution record. |
| Long accounts period | One accounts period is divided into no more than two valid Corporation Tax periods without duplicating the accounts or filing journey. | Filing-scope and contract coverage is implemented and recorded in the [recognition roadmap](../ct-recognition-roadmap.md). | Partial | Execute and retain a final two-period product case. |
| Conditional CT600A | CT600A is emitted only for the supported participator-loan case. | Conditional serialization and validation are implemented; safe baseline evidence is indexed by [Phase 5.8](../phase-5.8-corporation-tax-service-artifact-assurance.md). | Partial | Execute the positive case and a product-level negative case in the final CT-E run. |
| Unsupported supplementary pages and regimes | Unsupported scenarios fail closed with a useful scope finding and no substitute calculation. | The frozen exclusions are implemented and documented in the dossier and roadmap. | Partial | Retain representative product-level refusal evidence. |
| Jurisdiction isolation | The UK provider is selected only for the supported UK company context. | `CorporationTaxJurisdictionIsolation.sql` and provider-selection tests pass. | Complete | Retain in the final regression. |
| Provisional statutory parameters | Provisional rates support labelled planning accruals but cannot make a return filing-ready. | `CorporationTaxPeriodParametersUk.sql`, filing-readiness findings and annual Calculation presentation. | Complete | Retain in the final regression. |

## Artifact and HMRC test services

| Scenario | Expected result | Evidence obtained | Status | Owner review / next action |
| --- | --- | --- | --- | --- |
| Authoritative taxonomy and RIM validation | The frozen accounts, computation and CT600 package validates against the pinned authoritative assets. | Arelle and HMRC LTS passed without findings; versions and safe provenance are recorded in [Phase 5.8](../phase-5.8-corporation-tax-service-artifact-assurance.md). | Complete | Recheck versions immediately before applying. |
| Genuine TPVS validation | HMRC returns a successful validation response with no application findings. | Separately authorised TPVS run returned HTTP 200 and success messages, with exact evidence retained privately. | Complete | No replay unless the final frozen artifact changes materially. |
| Genuine External Test Service conversation | Submit, acknowledgement, polling, terminal response, receipt and delete follow the Transaction Engine protocol. | The separately authorised ETS run completed successfully; see [Phase 5.10](../phase-5.10-ct-external-test-local.md). | Complete | No replay unless required by HMRC or the frozen transport changes. |
| Exact artifact reproducibility | Approved accounts, computation and CT600 bytes are retained with SHA-256 identities and are not rebuilt at submission time. | Immutable preparation, approval and evidence-digest controls are implemented and exercised by the product journey. | Complete | Reproduce the final application artifact once during the frozen regression. |

## Product journey, negative outcomes and recovery

| Scenario | Expected result | Evidence obtained | Status | Owner review / next action |
| --- | --- | --- | --- | --- |
| Accepted Companies House accounts to CT review | The selected accepted accounts, current transactions and Corporation Tax period reconcile before preparation is allowed. | Two complete product walkthroughs and the published User Guide; stale or mismatched accounts block preparation. | Complete | Accepted during CT-D; retain the final frozen walkthrough. |
| Declaration, immutable approval and separate submission | Approval is attributable, binds exact document digests and does not itself submit; submission requires a later explicit action. | Deployed development journey completed and documented in the User Guide. | Complete | Accepted during CT-D. |
| Accepted fake-authority outcome | The development-only authority exercises the production-shaped workflow while remaining visibly distinguishable from HMRC. | Exactly-once send, durable outcome, receipt download and dashboard readback completed. | Complete | Accepted during CT-D; do not present it as HMRC evidence. |
| Validation failure before send | Invalid or stale inputs fail before authority dispatch and explain the corrective action. | Automated stale-source, immutable-approval and readiness coverage exists. | Partial | Capture representative product wording and confirm no dispatch occurred. |
| Credential failure | Missing or invalid credentials produce a safe retryable outcome without retaining a plaintext password or consuming another send. | Credential lifetime and redaction boundaries are implemented; production-shaped negative execution is not yet recorded in CT-E. | Partial | Execute and retain one controlled negative case. |
| Immediate gateway error and business rejection | Transport failure and HMRC business rejection are distinct, durable and do not masquerade as acceptance. | Direct presentation fixtures cover rejected and support-required wording. | Partial | Exercise product-level gateway-error and rejected conversations and review customer wording. |
| Pending conversation | A pending response displays safe status and continues outside the initiating browser circuit. | Background-worker and presentation coverage exists. | Partial | Retain an explicit pending-to-terminal product run. |
| Uncertain send and authenticated recovery | An ambiguous send blocks resubmission and offers recovery of the existing conversation. | Transaction Engine and product tests cover uncertain-send wording, recovery and the prohibition on blind resubmission. | Partial | Perform the explicit restart/worker-recovery exercise. |
| Delete failure | A terminal business outcome remains durable while delete cleanup is retried without changing the outcome. | Transaction Engine fixture covers delete failure and restart recovery. | Partial | Include the latest fixture result in the consolidated execution record. |
| Duplicate-send prevention | Only one durable conversation owns the permitted send for an accounting period. | Automated service and persistence coverage rejects duplicate submission once an attempt is claimed or uncertain. | Complete | Retain in the final regression. |
| Category Tree or source drift after approval | A changed source digest, Category Tree version or reviewed input prevents use of the stale approval. | Category Tree submission-lock and immutable-source tests are implemented. | Partial | Execute one product-level post-approval drift case. |

## Security, accessibility and operations

| Scenario | Expected result | Evidence obtained | Status | Owner review / next action |
| --- | --- | --- | --- | --- |
| Filing-owner and role isolation | A different owner or unauthorised role cannot read, approve, dispatch or download protected CT evidence. | Automated owner-scoped lookup, role-gated outcome access and encrypted-content tests pass. | Complete | Retain in the final security regression. |
| Cross-tenant and foreign-reference denial | Host, principal, workflow reference and protected content cannot cross tenant boundaries. | Shared-host resolution and general tenant isolation tests pass; CT protected content rejects a different filing owner. | Partial | Run a populated CT-specific cross-tenant/foreign-reference negative case. |
| Protected evidence retrieval | Downloads are decrypted only after authorisation, digest-checked, labelled safely and returned without cacheable disclosure. | Automated controls and the deployed accepted-path receipt download passed. | Complete | Accepted during CT-D. |
| Keyboard and accessible status semantics | The complete journey is operable by keyboard and exposes meaningful alert, status, disclosure and download labels. | Initial keyboard review and automated accepted/rejected/uncertain presentation semantics passed. | Partial | Complete the screen-reader and full-journey keyboard review. |
| Responsive presentation | Calculation, review, declaration, submission and outcome remain usable at the supported narrow viewport. | No complete CT-E responsive record yet. | Outstanding | Execute and record the responsive review. |
| Restart and worker recovery | Restart or worker turnover resumes the durable conversation without another send or lost terminal evidence. | Automated architecture and recovery fixtures exist. | Partial | Perform the explicit restart/scale-out exercise against shared persistence. |
| Customer and operator support | Rejected, uncertain and support-required states provide safe next actions and a support reference without exposing protected content. | Draft support/recovery guidance and presentation fixtures exist. | Partial | Review final customer wording and accept the operator/support record. |
| Production submission gate | Production CT dispatch remains closed until recognition and controlled activation. | The CT family gate remains fail-closed; development simulation and official test evidence are visibly classified. | Complete | Reconfirm immediately before applying. |

## Pre-application closure

The matrix is ready for owner acceptance only when:

- every **Partial** or **Outstanding** row required by the proposed scope is completed or explicitly
  removed from that scope with a fail-closed product boundary;
- the final regression identifies the exact source revision and frozen artifact digests;
- the current CT taxonomy, RIM, LTS/TPVS/ETS expectations and HMRC contact route have been rechecked;
- protected evidence has a controlled index suitable for an HMRC follow-up; and
- the owner explicitly authorises the recognition request.

HMRC-assigned recognition scenarios will be added only after HMRC supplies them. They will remain
separate from Trade Control's own CT-E evidence so the two assurance boundaries cannot be confused.
