# Phase 5.8 — Corporation Tax service-artifact assurance

## Historical outcome on 2 October 2026

Phase 5.8 is blocked by an unavailable authoritative validation asset. The fail-closed Corporation Tax family gate remains in place. No prepared artifact was promoted, no service-root golden was approved and no HMRC request was made.

HMRC lists **Corporation Tax computational 2025** as the applicable accepted computation taxonomy for accounting periods ending after 31 March 2026, but the accepted-taxonomies page supplies no download link for that release. The corresponding public entry point, `http://www.hmrc.gov.uk/schemas/ct/comp/2025-01-01/ct-comp-2025.xsd`, and the inferred official package locations checked during this review return `404`. The previous CT 2024 taxonomy is accepted only through 31 March 2026 and therefore cannot validate the target accounting period ending 30 June 2026.

The official Local Test Service does not close this gap. LTS 8.3 contains the update manager and generic validators. Its live service feed offers `ct_ct600_v1-994.zip`, which contains the RIM XSD, Schematron, envelope schema and `Calc.jar`, but no computation taxonomy. Substituting CT 2024, reconstructing CT 2025 from names or treating a derived QName catalog as validation evidence would violate the Objective 3 assurance boundary.

## Authoritative asset received on 8 October 2026

HMRC Software Developers Support Team supplied `CT2025-v1.0.0.zip` directly by email and confirmed that CT Computational 2025 is available in both test and live environments, can be used immediately, and applies to accounting periods starting on or after 1 April 2015. HMRC also explained that technical changes prevent it from hyperlinking taxonomies from the accepted-taxonomies page; future computation releases will be distributed through registered-developer communications.

The retained local evidence is:

- authoritative email: `.local/hmrc/corporation-tax/emails/Corporation Tax computational 2025 taxonomy package.txt`;
- original HMRC attachment: `.local/hmrc/corporation-tax/CT2025-v1.0.0.zip`;
- extracted review copy: `.local/hmrc/corporation-tax/CT2025-v1.0.0`;
- locally calculated archive SHA-256: `6A4E33434C3E546D664B7344B4490B87B95D854A41ED232F894D274E1FA8C604`; and
- taxonomy entry point: `http://www.hmrc.gov.uk/schemas/ct/comp/2025-01-01/ct-comp-2025.xsd`.

The package metadata identifies HM Revenue & Customs as publisher, version `1.0.0`, formal version date `2025-01-01` and release date `2024-12-19`. The package contains the taxonomy-package manifest, OASIS catalog, entry-point and supporting schemas/linkbases, schema-location and change reports, and XBRL/iXBRL validation samples.

Receipt resolved the external acquisition blocker but did not itself promote any artifact. Work resumed on 13 October 2026 after the Companies House Objective 5 slice paused. The CT family gate remains closed pending the remaining Phase 5.8 evidence and human review.

## Authoritative release selection

- CT600 V3 (2026) RIM 1.994 remains the selected RIM. HMRC identifies it as currently implemented in LTS, TPVS and live. RIM 1.995 is a 2027 release and is still awaiting implementation.
- Corporation Tax computational 2025 is the required computation taxonomy for the target period. HMRC lists it as accepted but does not expose a release link or public package.
- FRC 2026 taxonomy suite 1.0.0 is accepted for the target period and its official package is available.
- LTS 8.3 is the current downloadable Local Test Service. Its current CT update entry is RIM 1.994.

## Acquired-asset ledger

The authoritative archives remain in the ignored local evidence area. Required provenance, versions, entry points and checksums are pinned in source; the RIM XSD required for deterministic product validation is embedded as a build resource.

| Asset | Bytes | SHA-256 | Finding |
|---|---:|---|---|
| `CT2025-v1.0.0.zip` | 583,118 | `6A4E33434C3E546D664B7344B4490B87B95D854A41ED232F894D274E1FA8C604` | Official HMRC computation-taxonomy package supplied directly by Software Developers Support; version 1.0.0 and entry point pinned. |
| `HMRC-CT-2014-v1-994.zip` | 7,026,277 | `504C9DC643195BB5B9AB25A86B9CFF59C6312DE36ADE035BE01CA848D0B81BED` | Official RIM XSD, Schematron, compiled rules, specification and envelope schema available. |
| `ct_ct600_v1-994.zip` | 25,880,047 | `6BA951E830E597C69811F189089CD0B9F890180B73AD845DD7AFCEF03E543F35` | Official LTS update payload; contains RIM XSD/Schematron, envelope schema and calculator, not taxonomies. |
| `LTS8.3.zip` | 31,547,095 | `42352E965F8C4EE1D115653860618AB45171DF7DF3624F34FF85C47299905674` | Official LTS distribution and update-manager metadata. |
| `FRC-2026-Taxonomy-v1.0.0.zip` | 7,467,044 | `AE80AE12D9D747AC531150B0051BCD67E7C9ACF44313DA19105B4CC013566462` | Official accounts taxonomy package available. |
| `CT600-Sample-No-attachments.xml` | 4,355 | `83F115AB6184E21876DFC261E51FFD2FBD7A5ED07D120FF4F4FF5CD86C2FD1F4` | Official structural sample. |
| `CT600-Sample-accounts.xml` | 4,618 | `61ECCEB25401339F7522AD012BDAC5A3E519A0646C86C209ACD76E64DBA7DC70` | Official accounts-attachment sample. |
| `CT600-Sample-computations.xml` | 4,621 | `2B6460121614B22EA818C49CAD1EC69D813C6CE9877C27820A454E3F5BAFE3B8` | Official computation-attachment sample. |
| `CT2024-v1.0.0.zip` | 593,165 | `2870E82E2B5813389B886514434E6655F1F56C2247E38F4D25707BE51BEEFD9F` | Comparison only; not applicable to the target period and not a substitute. |

No authoritative release asset required for the supported Phase 5.8 fixture is now absent.

## Confirmed service-root contract

Review of RIM 1.994 and HMRC's valid samples establishes the following map for the future serializer:

- the immutable service root is `IRenvelope` in `http://www.govtalk.gov.uk/taxation/CT/5`;
- `IRheader` contains the UTR key, period end, default currency, manifest, exactly one reserved `IRmark` element with `Type="generic"`, and sender;
- `CompanyTaxReturn` contains the CT600 return and its supplementary content;
- `AttachedFiles/XBRLsubmission` embeds actual XHTML document elements, not base64 text;
- when both documents are present, `Computation` precedes optional `Accounts` in the schema sequence;
- each `InlineXBRLDocument` contains the source XHTML tree, so the accounts and computation documents must retain separately traceable source bytes and digests even though their XML nodes are embedded in the service root;
- the RIM Schematron evaluates the complete GovTalk path, so Phase 5.8 service-root checks and the later Phase 5.9 wrapper test must use the same immutable root without allowing transport code to rebuild tax content.

The existing `CorporationTaxPackageSerializer` does not satisfy this contract: it emits a diagnostic `IRenvelope`, simplified return summary and base64 attachment text. Relabelling those bytes cannot make them submission-ready.

## Implementation and validation evidence on 13 October 2026

The supported ordinary UK private micro-company fixture now produces a distinct CT600 V3 service artifact identified as `CT600-V3-2026-RIM-1.994+CT-COMP-2025-V1.0.0`. The implementation:

- retains `CorporationTaxPackageSerializer` unchanged as diagnostic-only XML;
- projects the computation using the official CT2025 namespace, entry point, published concepts, mandatory facts and dimensional contexts, including the typed business-name dimension;
- serializes the generated RIM 1.994 `IRenvelope`, the CT600 return and supported CT600A aggregate;
- reserves exactly one empty generic `IRmark` slot;
- embeds the actual computation XHTML followed by accounts XHTML; and
- places these explicit preview service-root bytes in `PreparedSubmissionPackage.Transmission` without changing the Phase 5.7 outbound family gate.

Validation of the frozen supported fixture produced the following results:

| Check | Result |
|---|---|
| Official RIM 1.994 XSD | Pass; zero schema findings. |
| Official RIM 1.994 executable Schematron/XSLT | Pass; zero business-rule errors when the immutable root is mounted at the ruleset's required GovTalk path for validation only. This wrapper is not transport implementation. |
| Official CT2025 taxonomy package | Pass; Arelle 2.46.0 loaded `CT2025-v1.0.0.zip` offline and reported no XBRL or dimensional validation findings for the computation iXBRL. |
| Product contract tests | Pass; 109 assertions, including deterministic serialization, exact IRmark and attachment order, embedded XHTML, source-byte preservation, CT identity-language constraints, a negative diagnostic-relabel case and direct RIM-XSD validation. |
| Application / architecture / submission-adapter / WebHarness suites | Pass; 75, 22, 137 and 86 assertions respectively. |
| Release solution build | Pass; zero errors. Ten `NU1900` warnings were limited to unavailable NuGet vulnerability-service metadata. |

The reviewed fixture anchors are:

- accounts iXBRL: `FF0DDE716CACA048D25BDCB3BF4AA36911F80EB78982AF2AE358AA8A2524986B`;
- computation iXBRL: `2744751A585814882A7CC35DCB6BE5C875D7D02AA69AE9D223B08DC01FA5829B`;
- Companies House artifact: `3AFEEB70542913A12BF360C5F03E9F4BE5BF8178908CDF9F337C5177B5B8C210`; and
- CT service root: `A3F3729F7236B342FC8C6D10C423104EA9B6F7C86CE86C396D78B2D586A77F2F`.

The computation golden changed deliberately because the former derived QNames and dimensionless contexts were replaced by the authoritative CT2025 contract. The later TPVS correction changed the accounts, computation, Companies House and CT service-root bytes because their shared iXBRL namespace declaration now uses HMRC's supported Transformation Registry 2 QName. Displayed figures, extracted numeric values, contexts, units, accounting values and statutory text are unchanged.

## LTS and TPVS diagnostic evidence on 13 October 2026

HMRC's supported invocation mechanisms were established before adding the WebHarness operations:

- LTS 8.3 is a separately running local Java service. Its Update Manager obtains service rules from HMRC's `https://www.tpvs.hmrc.gov.uk/tools/v2/services.xml` feed. The current CT update is `ct_ct600_v1-994.zip`. Programmatic callers post the CT GovTalk validation document to the loopback `/LTS/LTSPostServlet` endpoint; the multipart `LTSServlet?client=thin` route is the browser upload form, not the programmatic interface.
- Direct TPVS posts the same CT GovTalk validation document to `https://www.tpvs.hmrc.gov.uk/HMRC/CT600`. It is a body-validation facility and does not use the Vendor ID or External Test Service credentials. The credentialled Transaction Engine submit/poll conversation remains Phase 5.9 work and was not started.
- Both diagnostic requests wrap the already prepared, immutable CT600 service root. The source artifact is retained separately with its existing digest. The development-only wrapper supplies only the validation protocol class and inert validation identifiers; it does not rebuild CT600 fields, reserialize a computation from business objects, calculate an IRmark or alter the approved computation.

The authenticated development WebHarness now exposes separate `POST` operations at:

- `/api/company/corporation-tax/validation/lts`; and
- `/api/company/corporation-tax/validation/tpvs?confirmExternalSend=true`.

Both operations rerun the existing Objective 3 `CorporationTaxRunner`, select its immutable `PreparedSubmissionPackage.Transmission`, and return the exact request/response text, parsed findings, endpoint, evidence classification and SHA-256 digests. Exact service-root, request and response bytes plus metadata are retained under `.local/hmrc/corporation-tax/evidence/webharness-validation`. The implementation is disabled outside Development; the LTS endpoint is constrained to loopback, the TPVS endpoint is constrained to HMRC's published HTTPS host/path, and TPVS requires explicit confirmation on every call.

The frozen supported fixture was also exercised once against each real validator to establish the observed protocol and preserve review evidence:

| Evidence | Result |
|---|---|
| Local LTS 8.3 + official CT v1.994 update | HTTP 200; error `2022`, `IRmark not found`. LTS stops in its IRmark handler before later validation. No IRmark was calculated because that remains Phase 5.9. Evidence: `.local/hmrc/corporation-tax/evidence/20261013T194701-lts-437E3E04C14D`. |
| Genuine HMRC TPVS | HTTP 200; aggregate error `3001` and 36 application findings: 34 accounts `xbrl.ixbrl.FormatUndefined` findings for `ixt:num-dot-decimal` in the 2020 transformation namespace, plus two computation schema findings because `xml:lang` is not permitted on `ctcomp:CompanyName` and `ctcomp:TaxReference`. Evidence: `.local/hmrc/corporation-tax/evidence/20261013T194729-tpvs-437E3E04C14D`. |

Evidence anchors are:

- source service root: `3B3470341AFB68C2958FB8E0F8861BBC173781FF51C9B1C0A8918A5DDE01A92F`;
- validation wrapper used for both observed calls: `437E3E04C14DCA51EDB1E3FC42B8EAFA7EA23ABBB5E65305179A91878E170CAD`;
- LTS response: `959F2BB62351BB912EEEE8867F7D0A894E44C5016EE9647C7F3C7C074C6FC6E6`; and
- genuine TPVS response: `2FFF4FF3F613CE486EABD7F5DC9BF1000B864B0BEFCBC990E06B1CD1AC9EEDAC`.

The TPVS findings superseded the earlier assumption that local validation was sufficient to approve this artifact. They were retained and then corrected under explicit human approval.

## Approved TPVS correction and revalidation on 13 October 2026

The correction was deliberately limited to the validation findings. The shared iXBRL builder now emits HMRC-supported Transformation Registry 2 namespace `http://www.xbrl.org/inlineXBRL/transformation/2011-07-31` with its equivalent `ixt:numdotdecimal` transform. `ctcomp:CompanyName` and `ctcomp:TaxReference` retain their exact values but no longer carry schema-invalid `xml:lang` attributes. All other text facts retain their existing language semantics.

An intermediate diagnostic using the formally correct Registry 4 namespace `http://www.xbrl.org/inlineXBRL/transformation/2020` confirmed that TPVS does not configure that registry: the two computation findings cleared, while the 34 accounts format findings remained. The final Registry 2 artifact was therefore validated again locally and remotely:

| Evidence | Result |
|---|---|
| Product RIM XSD and contract suite | Pass; 109 assertions. |
| Official RIM 1.994 executable rules | Pass; zero rule errors. Result SHA-256 `B2958CBF824CD84ACBF558F3C515C43D565650E09E31DF6592C55862AB51EC63`. |
| Official CT2025 package in Arelle 2.46.0 | Pass offline; no XBRL or dimensional findings. Log SHA-256 `10B92720EC91C7567D4C930ED1F8868FD8668576B0C9E3627AEA67257586CE1E`. |
| Application / architecture / submission-adapter / WebHarness suites | Pass; 75, 22, 137 and 86 assertions. |
| Release solution build | Pass; zero errors. Five `NU1900` warnings were limited to unavailable NuGet vulnerability-service metadata. |
| Local LTS 8.3 + CT v1.994 | HTTP 200; only `2022`, `IRmark not found`. LTS still stops before its later handlers. Evidence: `.local/hmrc/corporation-tax/evidence/20261013T202341-lts-E8752A45F60F`. |
| Genuine HMRC TPVS | HTTP 200; only `2022`, `IRmark not found`. All 34 transformation findings and both computation schema findings are absent. Evidence: `.local/hmrc/corporation-tax/evidence/20261013T202522-tpvs-E8752A45F60F`. |

Final evidence anchors are:

- source service root: `A3F3729F7236B342FC8C6D10C423104EA9B6F7C86CE86C396D78B2D586A77F2F`;
- validation wrapper used for both final calls: `E8752A45F60F5B8C903416479DA8D311B2D5FE27BAB8CF24770C8F6524EEBA31`;
- LTS response: `0D759B53C48B5094EA5B40D4515CA7B365A09A4ECD28F5B8B84FC0CB8C466B05`; and
- genuine TPVS response: `BE4FA1D09EDBBB8F1B8CD45467A587CAE84F1946E82330D92239F0EB8FF68624`.

## Validation disposition

The authoritative local validation set is complete and genuine TPVS no longer reports an iXBRL-content finding. Its sole remaining finding is the deliberately empty IRmark slot, which belongs to Phase 5.9 transport. This is not approval to dispatch:

- LTS and TPVS cannot proceed beyond their IRmark handler without the later protocol work;
- the Objective 3 contract change has not yet received human approval;
- `HmrcComputationTaxonomy2025.SubmissionReady` remains `false`;
- every prepared artifact remains `Preview`; and
- the Phase 5.7 family gate still rejects before outbound I/O.

No External Test Service or production submission request was made. Phase 5.9 IRmark calculation, Transaction Engine submit/poll/delete and receipt handling remain excluded.

## Remaining review requirements

Review the corrected service artifact, its supported-profile assumptions and the Objective 3 contract change. Only that review may decide whether the taxonomy eligibility marker can change. The Phase 5.7 family gate remains closed until Phase 5.9 separately establishes the IRmark and Transaction Engine transport boundary; this implementation changes neither gate.
