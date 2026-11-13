# Corporation Tax recognition roadmap

## Goal

Reach the point at which Trade Control can apply for HMRC Corporation Tax software recognition and
listing for its explicitly supported filing profile. Recognition is the public HMRC term; this
roadmap uses it for the programme goal sometimes described as accreditation.

Recognition is not the same as enabling production filing. Test-service execution, recognition,
listing and production activation retain separate evidence and human gates.

## Current position — 12 November 2026

| Layer | Position | Remaining gate |
|---|---|---|
| Accounting source and Category Tree | Company statutory sources and mappings are implemented; Category Tree Change Control is available, and CT-A has now prepared a genuine current-period tenant snapshot. | Replace the legacy single-rate Corporation Tax source result with reviewed current-law inputs; do not alter the accounting dataset to avoid a tax scenario. |
| Objective 3 contracts and population | The approved ordinary micro-company surface remains implemented. Current-period CT-A evidence corrected depreciation polarity and CT600 whole-pound financial-year projection, then exposed that the model does not yet represent post-April-2023 marginal relief. | Add reviewed associated-company and augmented-profit inputs, financial-year rate selection and marginal-relief computation before accepting the current-period artifact. |
| Phase 5.8 service artifact | CT2025 computation, FRC 2026 accounts and RIM 1.994 assets remain pinned. The refreshed computation passes the official CT2025 taxonomy in Arelle; local LTS reaches its calculator and reports the unsupported marginal-relief result. | Re-freeze and rerun local validation after the statutory tax calculation is implemented. Do not reuse the earlier synthetic 19% liability as filing evidence. |
| Phase 5.9 Transaction Engine | Complete and reviewed for offline protocol fixtures, IRmark and durable conversation semantics. | Confirm current ETS class, credentials, endpoints and receipt behaviour immediately before the external test. |
| Phase 5.10 local ETS boundary | Implemented as a Development-only, send-disabled WebHarness preflight. | Replace the obsolete recognition blocker with explicit test-readiness and per-attempt authorisation; add the durable ETS executor without opening production. |
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

**Progress — 12 November 2026:** a current-period tenant has now been recommissioned and prepared
through Objective 3. Its accounting sources, statutory mappings and immutable artifact chain are
healthy. The run corrected a depreciation bridge defect and a CT600 whole-pound financial-year
projection defect. Official CT2025 taxonomy validation is clear, and local LTS proceeds through
schema, business-rule and IRmark validation to the tax calculator. The calculator then correctly
rejects the legacy flat 19% result because the current-period profit lies in the post-April-2023
marginal-relief band. CT-A therefore remains open while current-law rate and marginal-relief inputs
are added to the supported semantic model. Protected tenant values and evidence remain outside the
public repository. No TPVS, ETS or production request was made.

The calculation redesign, MIS/What-if preservation rules and verification programme are governed by
the [marginal relief and real-time liability work plan](ct-marginal-relief.md).

1. Inventory the selected tenant's company profile, accounts period, CT periods, UTR/test identity, Category Tree
   change-control state and statutory-source readiness without changing accounting data.
2. Prepare accounts, computation, CT600 and service root through the existing Objective 3 path.
3. Compare values, provenance and SHA-256 identities with the approved contract expectations.
4. Re-run local schema/rules, Arelle/CT2025 and LTS/TPVS diagnostics as appropriate.
5. Classify every difference as expected test evidence, an approved version change or a defect. No
   serializer/value change may be made merely to recover an old digest.

**Gate:** human review accepts the test-tenant statutory values, supported scope, change-control provenance
and refreshed validation evidence.

### CT-B — Phase 5.10 ETS-ready boundary

1. Confirm the current RIM/test-service release, `HMRC-CT-CT600` class, ETS endpoints, supplied test
   credentials, channel values, poll contract and receipt verification.
2. Remove `CT-RECOGNITION-EVIDENCE-PENDING` as an ETS blocker. Replace it with explicit baseline
   baseline approval, exact-artifact approval, protected-configuration availability and named-attempt
   authorisation.
3. Introduce a dedicated official-test eligibility boundary. It may send only to the allow-listed ETS
   endpoints and must not promote or expose the production CT gateway.
4. Connect the Phase 5.9 durable submit → acknowledgement → poll → terminal → receipt → delete
   coordinator to the one-attempt HTTP gateway.
5. Retain exact request/response/receipt bytes and digests in protected tenant-scoped storage.

**Gate:** send-disabled preflight reviewed with no credentials or protected XML exposed. A first ETS
exchange requires separate contemporaneous human authorisation.

### CT-C — controlled External Test Service evidence

1. Execute one authorised test-tenant ETS submission.
2. Follow the returned polling interval; persist correlation and terminal evidence before deletion.
3. Exercise restart recovery and `DATA_REQUEST` only where the actual conversation requires it.
4. Record HMRC validation findings without changing approved statutory values to satisfy a test.
5. Correct evidenced defects through the appropriate Objective 3, transport or product boundary and
   repeat only with a fresh explicitly authorised transaction.

**Gate:** complete, durable, reproducible ETS conversation with accepted artifact identities and no
production capability enabled.

### CT-D — production-shaped TCWeb journey

1. Add the CT continuation after a cryptographically linked Companies House terminal `Accepted`
   outcome. Legacy `S00018` cannot provide that link.
2. Read the retained full-accounts iXBRL through the approved accounts-snapshot boundary; never
   regenerate it from the mutable ledger.
3. Present computation, CT600, attachments, validation and declaration for attributable human review.
4. Persist immutable approval, exact artifact identities, one-send claim, conversation state,
   polling/recovery, terminal outcome, receipt and safe downloadable evidence.
5. Enforce tenant/principal isolation, role policy, protected credentials/content, duplicate refusal,
   kill switch, rate/size limits, monitoring and operational recovery in the shared host.
6. Provide a truthful pending experience rather than a browser-bound polling spinner.

**Gate:** the supported CT product journey is mature, accessible and independently tested on the
production-shaped multi-tenant topology.

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

Verified on 11 November 2026 without an HMRC request:

- `TaxHub.slnx` Debug build: succeeded, zero warnings/errors;
- Company Contract Tests: 109 assertions;
- Application Tests: 90 assertions;
- Submission Adapter Tests: 176 assertions;
- Architecture Tests: 32 assertions;
- WebHarness Tests: 87 assertions; and
- Data Provision CO1/CO2/CO6 corporate handoff: passed in supported offline mode.

The reviewed CT service-root SHA-256 remains
`A3F3729F7236B342FC8C6D10C423104EA9B6F7C86CE86C396D78B2D586A77F2F`.

## Official references

- [Corporation Tax software developer support collection](https://www.gov.uk/government/collections/corporation-tax-online-support-for-software-developers)
- [CT600 RIM artifacts and implementation status](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-ct600-rim-artefacts)
- [Taxonomies accepted by HMRC](https://www.gov.uk/government/publications/taxonomies-accepted-by-hm-revenue-and-customs/taxonomies-accepted-by-hmrc)
- [Basic guide for XML software developers](https://www.gov.uk/guidance/basic-guide-for-xml-software-developers)
- [Recognised Corporation Tax commercial software suppliers](https://www.gov.uk/government/publications/corporation-tax-commercial-software-suppliers)
