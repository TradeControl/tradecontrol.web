# Phase 5.8 — Corporation Tax service-artifact assurance

## Outcome on 2 October 2026

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

Receipt resolves the external acquisition blocker but does not promote any artifact. Work remains deliberately sequenced behind the active Companies House Objective 5 slice. The CT family gate stays closed until Phase 5.8 provisions and validates the complete asset set, constructs the genuine service root, collects LTS/TPVS evidence and passes human review.

## Authoritative release selection

- CT600 V3 (2026) RIM 1.994 remains the selected RIM. HMRC identifies it as currently implemented in LTS, TPVS and live. RIM 1.995 is a 2027 release and is still awaiting implementation.
- Corporation Tax computational 2025 is the required computation taxonomy for the target period. HMRC lists it as accepted but does not expose a release link or public package.
- FRC 2026 taxonomy suite 1.0.0 is accepted for the target period and its official package is available.
- LTS 8.3 is the current downloadable Local Test Service. Its current CT update entry is RIM 1.994.

## Acquired-asset ledger

These files were acquired into a temporary review directory only. They were not copied into the source tree because the validation set is incomplete.

| Asset | Bytes | SHA-256 | Finding |
|---|---:|---|---|
| `HMRC-CT-2014-v1-994.zip` | 7,026,277 | `504C9DC643195BB5B9AB25A86B9CFF59C6312DE36ADE035BE01CA848D0B81BED` | Official RIM XSD, Schematron, compiled rules, specification and envelope schema available. |
| `ct_ct600_v1-994.zip` | 25,880,047 | `6BA951E830E597C69811F189089CD0B9F890180B73AD845DD7AFCEF03E543F35` | Official LTS update payload; contains RIM XSD/Schematron, envelope schema and calculator, not taxonomies. |
| `LTS8.3.zip` | 31,547,095 | `42352E965F8C4EE1D115653860618AB45171DF7DF3624F34FF85C47299905674` | Official LTS distribution and update-manager metadata. |
| `FRC-2026-Taxonomy-v1.0.0.zip` | 7,467,044 | `AE80AE12D9D747AC531150B0051BCD67E7C9ACF44313DA19105B4CC013566462` | Official accounts taxonomy package available. |
| `CT600-Sample-No-attachments.xml` | 4,355 | `83F115AB6184E21876DFC261E51FFD2FBD7A5ED07D120FF4F4FF5CD86C2FD1F4` | Official structural sample. |
| `CT600-Sample-accounts.xml` | 4,618 | `61ECCEB25401339F7522AD012BDAC5A3E519A0646C86C209ACD76E64DBA7DC70` | Official accounts-attachment sample. |
| `CT600-Sample-computations.xml` | 4,621 | `2B6460121614B22EA818C49CAD1EC69D813C6CE9877C27820A454E3F5BAFE3B8` | Official computation-attachment sample. |
| `CT2024-v1.0.0.zip` | 593,165 | `2870E82E2B5813389B886514434E6655F1F56C2247E38F4D25707BE51BEEFD9F` | Comparison only; not applicable to the target period and not a substitute. |

The absent ledger item is the official CT computational 2025 taxonomy package, including its authoritative checksum and entry points.

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

## Validation disposition

The available RIM and FRC assets are individually identifiable, but the required whole-package validation cannot be completed without the official CT 2025 computation taxonomy. Consequently:

- official computation-taxonomy validation is not claimed;
- LTS/TPVS validation of a genuine target-period artifact is not claimed;
- no new `PreparedSubmissionPackage.Transmission` bytes were created;
- the existing accounts, computation and diagnostic transmission bytes and digests remain unchanged;
- `HmrcComputationTaxonomy2025.SubmissionReady` remains `false`;
- the Phase 5.7 family gate remains closed before outbound I/O.

## Resumption requirements

The authoritative taxonomy-bundle requirement is now satisfied. When the sequential plan returns to Corporation Tax, provision the complete pinned validation set, construct the service root, validate XSD/Schematron/taxonomies, obtain the relevant LTS/TPVS evidence, freeze the new golden and submit the readiness change for human review. Until then, `HmrcComputationTaxonomy2025.SubmissionReady` remains `false` and no CT bytes may reach test or live HMRC.
