# Company Statutory Contract Design

## Status and Scope

This document defines a Stage 1 contract architecture for Companies House statutory accounts filing and HMRC Corporation Tax filing. It is a design specification only. It does not implement C#, SQL, Tax Tags, category mappings, templates, rendering, validation services, or submission transport.

The design is based on the complete contents of `specs/reference/company-field-sets.md` and on inspection of the existing Self Assessment Objective 3 contracts and offline contract tests. In particular, it carries forward their useful conventions: exact wire names, explicit endpoint metadata, omission of absent optional values without losing explicit zeroes, separation of production and preview contracts, small handwritten semantic types, generated types for large externally defined wire graphs, deterministic serialization checks, and fixture-based offline tests.

The company work is broader than the existing Self Assessment JSON API surface. It includes two authorities, XML envelopes, generated statutory return schemas, XBRL/iXBRL taxonomies, human-readable documents, attachments, asynchronous submission status, and accounting periods that do not always align with Corporation Tax accounting periods. Those differences require a richer architecture rather than an extension of the existing flat API DTO pattern.

The intended outcome is an accounting-system boundary that can ultimately support HMRC and Companies House assurance. The statutory contracts define the target required by the authorities. A later population layer will translate Trade Control data into that target and provide reconciliation evidence. The statutory contracts must not know how Trade Control categories, cash codes, Tax Tags, templates, or legacy account types are organised.

## Architectural Position

The design has five distinct layers:

1. **Authority-neutral statutory semantics** represent the company, reporting periods, accounts statements, disclosures, and tax computation concepts needed to prepare filings.
2. **XBRL and iXBRL infrastructure** represents qualified concept names, contexts, units, dimensions, facts, taxonomy releases, and rendered report artefacts.
3. **Companies House contracts** project statutory accounts into the filing options, document profile, XML envelope, authentication metadata, acknowledgements, and status lifecycle required by Companies House.
4. **HMRC Corporation Tax contracts** represent the CT600 return and supplementary pages, tax computations, HMRC accounts attachment, submission package, XML envelope, and acknowledgements.
5. **Validation and serialization services** transform models into deterministic wire artefacts and report errors without depending on application persistence or user-interface concerns.

The principal dependency direction is:

```text
Trade Control population and reconciliation (later)
                         |
                         v
       Authority-neutral statutory semantics
                  /                  \
                 v                    v
 Companies House projection    HMRC Corporation Tax projection
          |                       /              \
          v                      v                v
 CH iXBRL + XML envelope     CT600 XML     accounts/computations iXBRL
          |                       \              /
          v                        v            v
 CH transport/status             HMRC submission package/transport
```

No dependency is permitted in the reverse direction. In particular, the authority-neutral models must not contain submission credentials, endpoint paths, Companies House-only delivery choices, HMRC-only return box numbers, or database identifiers.

## Authority Boundary

Companies House and HMRC consume related information but do not receive one shared submission.

Companies House receives a statutory accounts filing. Its current contract includes the appropriate accounts document, Companies House filing statements and choices, presenter/company authentication at the submission boundary, and an asynchronous acknowledgement/status lifecycle.

HMRC receives a Corporation Tax return package. It contains a CT600 return, applicable supplementary pages, statutory accounts in iXBRL, tax computations in a separate iXBRL document, and any permitted supporting attachments. It uses HMRC's Corporation Tax submission envelope and acknowledgement/error contract.

The system may derive both projections from one approved statutory accounts set, but it must preserve these distinctions:

- A Companies House filing and an HMRC return package have different authority contracts, credentials, envelopes, business rules, status responses, and audit identities.
- Companies House accounts may be filleted when legally permitted. HMRC normally requires the full accounts information relevant to the return. Therefore the byte-level iXBRL document is not inherently shared.
- One accounts period can support two HMRC Corporation Tax returns when the accounting period exceeds twelve months. The Companies House filing remains one accounts filing for that accounts period.
- The two authorities can adopt taxonomy, schema, and transport revisions on different dates.
- Acceptance by one authority is evidence about that submission only; it is not proof that the other authority's contract is satisfied.

Authority concerns therefore live in separate namespaces and packages. Shared code is limited to genuinely shared semantics and infrastructure.

## Genuinely Shared Concepts

The following concepts are suitable for an authority-neutral statutory layer because they describe the accounts or their reporting meaning rather than an authority's wire format:

- Legal entity identity: company name, company number, incorporation jurisdiction where required, registered-office facts, and accounting-reference information.
- The statutory accounts period, comparative period, approval date, signing director, reporting framework, accounts type, and audit/exemption status.
- The statement of financial position, income statement where applicable, and structured notes and disclosures.
- Current-period and comparative monetary values, currencies, dates, booleans, enumerated statements, and narrative disclosures.
- The facts required to explain directors' advances, guarantees, commitments, contingencies, employees, average employee numbers, and other repeating disclosures.
- Tax computation semantics such as trading profit adjustments, capital allowance schedules, loss claims, chargeable gains, and calculation of Corporation Tax liability, where those semantics are independent of a CT600 box number.
- XBRL primitives such as expanded qualified names, entity identifiers, instant and duration periods, contexts, units, explicit dimensions, decimals/precision policy, facts, footnotes, and taxonomy-release metadata.

The following are not shared statutory concepts and must stay in authority-specific contracts:

- CT600 box numbers, groupings, declarations, supplementary-page indicators, and RIM schema types.
- Companies House delivery statements, filleting selection, presenter identifiers, company authentication codes, package class, and status polling.
- HMRC transaction identifiers, submission envelope elements, gateway credentials, attachment declarations, acknowledgement codes, and error structures.
- Authority-specific endpoint addresses, media types, schema versions, namespaces, and validation profiles.
- The final rendered iXBRL document. The underlying facts may be shared, but document content and filing rules can differ by recipient.

Sharing must not be achieved by making an untyped property bag. Shared concepts should have explicit types and invariants. A generic XBRL fact collection is appropriate at the taxonomy/rendering boundary, but it is not a substitute for a typed statutory accounts or tax computation model.

## Established Solution and Project Boundary

The repository restructuring has established a neutral company contract assembly in the standalone `tax-hub` submodule. Companies House and Corporation Tax types do not share an assembly with the MTD Income Tax or VAT JSON contracts. The project and root namespace are:

```text
TradeControl.Tax.UK.Company.Contracts
TradeControl.Tax.UK.Company
```

This avoids making Companies House appear to be an HMRC or MTD concern and prevents the existing JSON API assumptions from becoming implicit dependencies. It also gives the relatively large schema, taxonomy metadata, fixtures, and generated sources a clear ownership boundary.

The proposed source organisation is:

```text
src/tax-hub/src/TradeControl.Tax.UK.Company.Contracts/
  Statutory/
    Identity/
    Accounts/
    Disclosures/
    TaxComputation/
  Xbrl/
    Model/
    Taxonomy/
    Validation/
    Serialization/
  CompaniesHouse/
    Accounts/Tis5_9/
      Contracts/
      Generated/
      Serialization/
      Validation/
    Submission/
  Hmrc/
    CorporationTax/
      Ct600/V2026/
        Contracts/
        Generated/
        Validation/
      Computations/Taxonomy2025/
      Submission/V2026/
  ContractInfrastructure/
    Endpoints/
    Artefacts/
    Validation/
```

The company contract assembly has no project references and currently contains no company implementation. That empty boundary was created deliberately during repository restructuring so implementation can begin with the intended dependency direction already enforced. Companies House types must not be placed below `TradeControl.Tax.UK.Hmrc`; only Corporation Tax authority contracts belong under the HMRC namespace.

A companion offline test project has also been established:

```text
src/tax-hub/tests/TradeControl.Tax.UK.Company.ContractTests/
  Fixtures/
    CompaniesHouse/
    HmrcCorporationTax/
    Ixbrl/
    Schemas/
  Program.cs
```

This follows the Objective 3 executable contract-test convention while keeping company fixtures separate from the MTD Income Tax and VAT fixtures. It references only `TradeControl.Tax.UK.Company.Contracts`. A conventional test framework could be adopted later, but it is not required to establish the contract boundary.

The wider project ownership is defined by `tax-hub-repo-structure.md`:

- `TradeControl.Tax.UK.Application` will own filing preparation, population orchestration, reconciliation policy, submission use cases, and ports.
- `TradeControl.Tax.UK.Adapters.TradeControl` will own SQL access and Trade Control-specific source projections and mappings.
- `TradeControl.Tax.UK.Adapters.Submission` will own live HMRC and Companies House clients, authentication, retries, environment configuration, polling, and operational audit implementations.
- `TradeControl.Tax.UK.WebHarness` remains a diagnostic composition root and is not part of the statutory contract model.

This document remains authoritative for the internal design of `Company.Contracts`; the repository architecture document is authoritative for dependencies between projects.

## Principal Authority-Neutral Types

Names below are design names rather than an instruction to create code in this stage.

### Identity and reporting periods

```text
CompanyIdentity
CompanyRegistrationNumber
AccountsPeriod
ComparativePeriod
CorporationTaxPeriod
AccountsApproval
ReportingFramework
AccountsProfile
AuditStatus
```

`AccountsPeriod` is the period covered by the statutory accounts. It must have an inclusive start and end date and may exceed twelve months where company law permits it.

`CorporationTaxPeriod` is a separate value type with HMRC-specific duration constraints. It must never be a type alias for `AccountsPeriod`. A period-allocation service will derive one or more Corporation Tax periods from the accounts period, but the result remains explicit and reviewable.

An `AccountsProfile` describes the legal/reporting choices that control required statements and disclosures, for example micro-entity status, small-company status, audit exemption, abridgement where applicable, and the reporting framework. It must not infer eligibility merely from which values happen to be populated.

### Statutory accounts aggregate

```text
StatutoryAccounts
StatementOfFinancialPosition
IncomeStatement
EquityStatement
CashFlowStatement
AccountsNotes
AccountingPolicies
AccountsApproval
```

`StatutoryAccounts` is the authority-neutral aggregate root. It identifies the entity and accounts period, carries the approved reporting profile, contains the applicable primary statements and notes, and records approval/signing facts. Optional statements remain explicit optional components; their absence is valid only when the reporting profile and validation rules permit it.

Statement lines should be represented as named semantic properties or small structured components, not by authority box numbers or taxonomy strings. Current and comparative values should be carried together where that improves invariants, for example:

```text
ComparativeAmount
  CurrentPeriodValue
  ComparativePeriodValue
  Currency
```

The exact representation must distinguish an absent fact from an explicitly reported zero. That distinction is required by both existing Objective 3 conventions and statutory validation.

Repeating or contextual disclosures require structures rather than flat fields. Examples include:

```text
DirectorAdvance
DirectorGuarantee
CommitmentOrContingency
RelatedPartyDisclosure
FixedAssetMovement
ShareClassDisclosure
EmployeeDisclosure
```

These types should retain identity, period, category, narrative, and monetary components as applicable. Flattening them into one value per statutory heading would lose cardinality and dimensional meaning.

### Tax computation aggregate

```text
CorporationTaxComputation
TradeComputation
ProfitAdjustmentSchedule
CapitalAllowanceSchedule
LossReliefSchedule
ChargeableGainsComputation
CorporationTaxCalculation
TaxPaymentSummary
```

The tax computation aggregate explains the bridge from accounts profit to taxable profit and tax liability. It is separate from the CT600 return because the computation is a calculation document while the CT600 is an authority-prescribed return form. The two must reconcile through explicit validation rules.

The initial supported computation profile should cover the ordinary trading-company path identified in the field-set research: accounts profit, add-backs and deductions, capital allowances, loss relief, taxable total profits, rate application, reliefs, and final liability/payment reconciliation. Unsupported schedules must be rejected explicitly rather than silently omitted.

## XBRL and iXBRL Contract Model

### Qualified concept identity

Statutory headings are not sufficient identifiers. Each reportable XBRL concept is identified by an expanded qualified name: a namespace URI plus a local name. The proposed primitive is:

```text
XbrlQName
  NamespaceUri
  LocalName
```

Prefixes such as `uk-core` are document aliases and must not form the durable key. The durable key is the expanded name, conventionally written `{namespace-uri}local-name`. Prefix allocation belongs to serialization.

Taxonomy bindings should be held in versioned catalog data:

```text
TaxonomyContract
  ReleaseId
  EntryPointUri
  PublicationDate
  EffectiveFrom
  EffectiveTo
  Status
  NamespaceCatalog
  ConceptCatalog

TaxonomyConcept
  Name: XbrlQName
  DataType
  PeriodType
  BalanceType
  Abstract
  Nillable
  AllowedDimensions
  Labels
  References
```

This catalog is the authoritative mapping from a stable application semantic concept to the external taxonomy concept for a given release. The stable semantic key is not a cash code, category code, Tax Tag, database column, display label, or QName. It is an internal statutory-contract identifier whose binding to a QName is versioned.

Thousands of taxonomy concepts should not be implemented as thousands of handwritten CLR properties. Taxonomy discovery metadata is better represented as generated catalog data. Typed statutory aggregates define the supported filing surface; adapters bind those typed values to catalog concepts and emit facts.

### Contexts, periods, units, and dimensions

The minimum infrastructure types are:

```text
XbrlEntityIdentifier
XbrlPeriod
  XbrlInstant
  XbrlDuration
XbrlDimensionMember
XbrlContext
XbrlUnit
XbrlDecimals
XbrlFact
XbrlFootnote
XbrlFactSet
```

`XbrlContext` contains an entity identifier, an instant or duration period, and zero or more explicit dimension/member pairs. It must be immutable after construction so a fact cannot change meaning through later context mutation.

Contexts should be value-deduplicated during document construction. Stable generated context IDs are serialization details; they are not domain identifiers. The same applies to unit IDs.

`XbrlUnit` must support at least ISO currency measures, pure numbers, and shares. It should be capable of representing numerator/denominator units even if the initial filing profile does not require them.

`XbrlFact` contains a QName, context reference, optional unit reference, value, nil state, decimals policy, language for text facts, and any permitted footnote reference. Fact values require a discriminated representation for monetary, decimal, integer, boolean, date, enumeration/QName, and text values. Storing all values as strings would defer type errors until external validation.

Dimensions are essential for repeated statutory disclosures and breakdowns. A fact's key is therefore not only its QName. Within a report it is effectively the combination of concept QName, entity, period, dimensions, and where relevant unit/language. Duplicate detection and reconciliation must use that full aspect set.

### iXBRL document representation

iXBRL is an XHTML document containing inline XBRL facts, resources, contexts, units, continuations, relationships, and human-readable presentation. It cannot be modelled adequately as a flat DTO graph alone.

The proposed types are:

```text
IxbrlReport
IxbrlPresentation
IxbrlResourceSet
IxbrlDocumentArtifact
IxbrlRenderProfile
IxbrlDocumentBuilder
```

`IxbrlReport` is a logical report containing a fact set, taxonomy reference, presentation specification, and report metadata. `IxbrlDocumentBuilder` produces a deterministic XHTML/XML document. `IxbrlDocumentArtifact` represents the resulting immutable bytes plus media type, taxonomy identity, validation result, and cryptographic digest.

The human-readable presentation is not disposable decoration. HMRC requires accounts and computations to be submitted as readable iXBRL documents, and Companies House displays filed accounts. Document generation must therefore maintain an explicit link between displayed values and inline facts. It must not create a visible table and a separate hidden fact list whose values can diverge.

Companies House and HMRC projections can start from the same statutory fact source, but should build separate `IxbrlReport` instances. This permits lawful filleting and authority-specific statements without mutating or filtering a document already prepared for the other authority.

## Companies House Accounts Contract

### Filing aggregate

The principal types are:

```text
CompaniesHouseAccountsFiling
CompaniesHouseAccountsDocument
DeliveredAccountsProfile
RegistrarStatements
CompaniesHouseFilingPackage
```

`CompaniesHouseAccountsFiling` identifies the company, accounts period, applicable taxonomy/release, accounts type, delivery choice, and approved statutory accounts source. It produces a Companies House-specific iXBRL accounts document.

`DeliveredAccountsProfile` makes full versus filleted delivery explicit and records the legal basis and affected components. Filleting must be a projection rule applied to the authority-specific report, not destructive removal from the shared `StatutoryAccounts` aggregate.

`RegistrarStatements` contains the declarations and statements prescribed by the Companies House contract. These are authority facts even when they appear visibly within the accounts document.

### Envelope and transport contracts

The current filing family should be explicitly versioned for the present Companies House XML Gateway/TIS 5.9 contract:

```text
CompaniesHouse.Accounts.Tis5_9
  AccountsSubmissionEnvelope
  AccountsSubmissionRequest
  AccountsSubmissionResponse
  SubmissionStatusRequest
  SubmissionStatusResponse
  CompaniesHouseError
  CompaniesHouseEndpointSet
```

The exact GovTalk/XML schema names should be retained in generated wire types. Handwritten adapters should isolate generated schema naming from the rest of the application.

Presenter credentials and the company authentication code belong to a transport/security request context, not to `StatutoryAccounts`, the iXBRL document, persisted fixtures, logs, or validation reports:

```text
CompaniesHouseSubmissionCredentials
CompaniesHouseSubmissionContext
```

These types describe the information required at the final submission boundary, but their acquisition and live use belong in `TradeControl.Tax.UK.Adapters.Submission`. They should be designed to prevent accidental string formatting or diagnostic output of secrets. A submission audit record may retain a non-secret presenter identity and authority correlation identifiers, but not reusable credentials.

Status polling semantics are part of the contract. A successful initial gateway response does not necessarily mean that the filing has been accepted. Contract types must preserve the submission number/correlation identifier and expose terminal status and authority errors separately from HTTP/XML transport success. The polling loop, retry policy, timeouts, and network client belong in `TradeControl.Tax.UK.Adapters.Submission`.

### Future Companies House service

The expected 2028 Companies House software filing service must be a separate future contract family, not a conditional branch inside TIS 5.9 DTOs. No request or response type should be guessed before Companies House publishes a final specification.

A registry may advertise the future family as `Preview` or `Unavailable` metadata, with no production serializer or transport implementation. The current TIS 5.9 contract remains independently testable and supportable during transition.

## HMRC Corporation Tax Contract

### CT600 return and supplementary pages

The current return contract should be versioned against the applicable CT600 RIM release identified by the field-set research, initially the 2026 form family/RIM 1.994:

```text
Hmrc.CorporationTax.Ct600.V2026
  Ct600Return
  Ct600ReturnInformation
  Ct600CompanyInformation
  Ct600TaxCalculation
  Ct600Declaration
  Ct600SupplementaryPageSet
  Generated/
```

The CT600 is an externally prescribed XML form and is a strong candidate for generated wire types. The generated graph should preserve element names, ordering, multiplicity, simple-type restrictions, nil rules, and namespaces from the official RIM/XSDs.

Supplementary pages must be modelled as typed conditional components, not as an undifferentiated list of arbitrary fields. The family must be able to represent each page present in the official release, including CT600A and the other prescribed supplementary pages, while a support policy states which scenarios the first application increment can populate and validate.

The return aggregate must enforce agreement between:

- supplementary-page indicators in the CT600;
- the supplementary-page instances included in the return;
- the claims, reliefs, loans, groups, controlled-foreign-company matters, or other circumstances represented by the computation; and
- any required supporting attachment.

The recommended implementation strategy is to generate all page contracts supplied by the selected official schema release, but initially support population only for the agreed ordinary-company profile and any specifically approved pages. Attempting to file a scenario that requires an unsupported page must produce a blocking validation error.

### Tax computation document

The computation is separate from the CT600 return and from the statutory accounts document:

```text
Hmrc.CorporationTax.Computations.Taxonomy2025
  CorporationTaxComputationReport
  ComputationTaxonomyBinding
  CorporationTaxComputationRenderer
```

The authority-neutral `CorporationTaxComputation` supplies calculation semantics. A versioned taxonomy binding converts these to computation facts, contexts, units, and presentation sections. The renderer produces the HMRC computation iXBRL artifact.

The first supported profile should be explicit. It should not imply general support for every Corporation Tax scenario merely because the generic XBRL infrastructure can emit arbitrary facts. Every supported computation path needs calculation, applicability, tagging, presentation, and cross-document reconciliation rules.

### HMRC statutory accounts document

The HMRC accounts attachment is a separate iXBRL report:

```text
Hmrc.CorporationTax.Accounts
  HmrcStatutoryAccountsReport
  HmrcAccountsRenderProfile
  HmrcAccountsTaxonomyBinding
```

It consumes the same approved statutory accounts semantics as the Companies House projection, but uses the HMRC-required content profile. It must not reuse a Companies House filleted artifact merely because the dates and company number match.

### Return package and envelope

The package aggregate is:

```text
CorporationTaxReturnPackage
  CorporationTaxPeriod
  Ct600Return
  Ct600SupplementaryPageSet
  AccountsArtifact
  ComputationArtifact
  SupportingAttachments
  Declaration
  ContractManifest
```

The HMRC wire family is:

```text
Hmrc.CorporationTax.Submission.V2026
  CorporationTaxSubmissionEnvelope
  CorporationTaxSubmissionRequest
  CorporationTaxSubmissionAcknowledgement
  CorporationTaxSubmissionError
  CorporationTaxEndpointSet
```

`CorporationTaxReturnPackage` is a logical aggregate, not an assumption that the wire transport is a ZIP file. Its serializer must follow the official HMRC submission schema and attachment mechanism exactly.

The package validates that return amounts, period dates, page indicators, computation results, accounts facts, declarations, and attachment metadata agree before serialization. It also produces an immutable manifest of the component artifacts, their media types, byte lengths, SHA-256 digests, and contract/taxonomy versions. Transient transport identifiers and credentials do not contribute to document digests.

## Accounts Period and Corporation Tax Period Composition

Accounts periods and Corporation Tax periods are independent first-class values.

The proposed orchestration types are:

```text
CorporationTaxPeriodAllocation
CorporationTaxPeriodAllocationRule
CompanyFilingComposition
```

For an accounts period of twelve months or less, the usual composition is one Companies House accounts filing and one HMRC Corporation Tax return package. For a longer accounts period, the composition may be one Companies House accounts filing and two HMRC return packages, each with its own CT600, computation allocation, declaration, submission identity, and status.

The two HMRC packages may reference the same immutable full accounts artifact when HMRC rules permit the same accounts document to accompany each return. They must not share a mutable package or computation object. Each computation must show the correct allocation of profits, losses, allowances, rates, payments, and other period-dependent values.

The allocation operation must be deterministic and produce an auditable explanation. It must not simply divide annual values pro rata where tax law requires a different treatment. The contract layer expresses the allocation result and its validation; later tax calculation policy determines the values.

Date validation must include:

- accounts and comparative period ordering;
- approval/signing dates;
- maximum Corporation Tax period duration;
- continuous coverage where an accounts period is split;
- no overlap between resulting Corporation Tax periods;
- correct rate/effective-date contract selection for each period; and
- consistency of every XBRL context with the document and return period it supports.

## Versioning and Contract Selection

Version identity is multi-dimensional. It includes at least:

- authority;
- submission service/envelope release;
- return form/RIM release;
- accounts taxonomy release and entry point;
- computation taxonomy release and entry point;
- business-rule/validation profile; and
- production, preview, retired, or unsupported status.

The proposed descriptors are:

```text
ExternalContractId
ExternalContractStatus
SubmissionContractDescriptor
TaxonomyContract
ValidationProfileId
CompanyContractRegistry
```

Versioned external DTOs should live in separate namespaces and directories. A single DTO decorated with conditions such as “serialize this property only for 2027” is not acceptable for materially different official schemas.

`CompanyContractRegistry` selects a coherent contract set from explicit facts such as authority, filing type, accounts period end, Corporation Tax period end, and requested environment. Selection must return the exact descriptor or a clear unsupported result. It must not fall back silently to the newest known version.

Preview types and rules are physically and logically isolated, following the existing Objective 3 convention. Production orchestration must reject preview descriptors unless preview use has been explicitly enabled. A published future date alone is not authority to treat a draft contract as production-ready.

Official source provenance should be recorded in a checked-in manifest containing source title, source URL, publication/retrieval date, official version, local asset path where applicable, and checksum. Generated output should include the manifest version in a header and be reproducible from the pinned source inputs.

## Generated and Handwritten Code Boundary

### Generated code

Generation is appropriate where an official machine-readable schema defines a large wire graph:

- CT600 and supplementary-page RIM/XSD types;
- HMRC submission envelope types where an official XSD is available;
- Companies House/GovTalk envelope and response types where official schemas are available; and
- taxonomy concept catalog data derived from official taxonomy files.

Generated files should live under `Generated/`, carry an auto-generated marker, and never be edited manually. Generation must be deterministic and invoked by a documented repository tool or script. A manifest/checksum test should detect drift between official inputs and checked-in generated output.

Generated XSD types should not leak throughout the application. Handwritten adapters convert between the stable aggregates and generated wire graphs. This contains generator quirks, optional-value companion properties, schema naming, and release churn.

### Handwritten code

Handwritten types and services are appropriate for:

- authority-neutral statutory accounts and tax computation aggregates;
- value objects and invariants;
- contract descriptors and version selection;
- package composition;
- applicability and cross-document validation;
- XBRL context, unit, dimension, and fact models;
- taxonomy bindings from supported semantic fields to QNames;
- iXBRL presentation and deterministic document construction;
- generated-wire adapters; and
- immutable submission artefact manifests.

Taxonomy catalogs may be generated, but the supported semantic binding should be reviewed code or reviewed declarative data. Automatically selecting a concept merely because its English label resembles a statutory heading is not acceptable.

## Validation Architecture

Validation is a staged pipeline. Each stage returns findings and does not mutate the submitted model.

```text
ContractValidationResult
ContractValidationFinding
  Severity
  Code
  Message
  ObjectPath
  RuleSource
  ExternalReference
```

The stages are:

1. **Structural validation** checks required values, types, cardinality, period ordering, and aggregate invariants.
2. **Accounting validation** checks statement arithmetic, current/comparative consistency, balance relationships, and required note-to-face relationships.
3. **Tax computation validation** checks adjustment schedules, allowances, losses, rates, reliefs, liability arithmetic, and payment reconciliation.
4. **Applicability validation** checks accounts profile choices, exemptions, required statements, CT600 supplementary pages, declarations, and attachment requirements.
5. **Cross-document reconciliation** checks CT600 boxes against computations, computation starting points against accounts, dates and company identity across all artifacts, and declared page/attachment indicators against actual contents.
6. **XBRL aspect validation** checks QNames, data types, contexts, period types, units, dimensions, decimals, duplicates, and taxonomy entry points.
7. **Schema and syntax validation** checks XML/XHTML well-formedness, official XSD/RIM rules, namespaces, ordering, and permitted content.
8. **Authority business-rule validation** applies the pinned Companies House or HMRC rule set for the selected contract version.
9. **Rendered-document validation** checks that human-readable presentation contains and agrees with the tagged values, has required statements, and is suitable for human review.
10. **External conformance validation** records results from official validators/test services when later implementation introduces those integrations.

Findings must identify their source and contract version. A local validation success means only that the local pinned rule set passed. It must not be labelled “HMRC approved” or “Companies House accepted.” Final authority responses are separate audit events.

Warnings may be permitted by a filing policy; errors are blocking. The policy decision must be explicit and recorded with the submission attempt.

## Serialization and Document Generation

Serialization must be deterministic and culture-independent. The same approved logical input and contract versions should produce the same document bytes except for fields that the official contract requires to be generated per submission.

Common rules include:

- UTF-8 using the exact declaration/byte-order policy required by the authority;
- invariant formatting of dates, decimals, booleans, and enumeration literals;
- explicit namespace URIs and controlled prefix allocation;
- schema-defined element ordering and omission/nil behaviour;
- preservation of explicit monetary and numeric zeroes;
- XML readers configured to prohibit DTD processing and uncontrolled external resource resolution;
- normalized and tested line-ending/whitespace policy;
- deterministic context IDs, unit IDs, document anchors, continuation IDs, and fact ordering;
- no credentials or volatile transport metadata embedded in reusable document artifacts; and
- a digest calculated over the final bytes actually submitted.

The serializer families should remain separate:

```text
CompaniesHouseAccountsSerializer
CompaniesHouseEnvelopeSerializer
Ct600Serializer
HmrcCorporationTaxEnvelopeSerializer
IxbrlDocumentBuilder
```

XML serializers are appropriate for official XML graphs. iXBRL requires an XML-aware XHTML document builder because presentation structure, inline facts, contexts, continuations, and relationships must be composed together. String concatenation is not an acceptable document-generation strategy.

The renderer should expose a logical presentation model rather than accept arbitrary application HTML. Styling and layout assets must be versioned with the render profile. Visible values should be generated from the same fact objects that produce inline tags so tagged and displayed values cannot diverge.

## Endpoint and Submission Metadata

The existing `HmrcEndpoint` record is useful precedent but is specific to JSON/HTTP API operations. It should not be stretched to describe XML gateway services and asynchronous filing lifecycles.

The common metadata abstraction should be small:

```text
SubmissionServiceDescriptor
  Authority
  Operation
  Environment
  TransportProtocol
  Address
  HttpMethodOrOperation
  RequestMediaType
  ResponseMediaType
  AuthenticationScheme
  EnvelopeContractId
  SuccessSemantics
  StatusPollingSemantics
  Status
```

Authority-specific contract descriptors then add typed details:

```text
CompaniesHouseEndpointSet
HmrcCorporationTaxEndpointSet
```

Descriptors contain no credentials. They define the authority operation and its required protocol semantics. Concrete base addresses, secrets, HTTP/XML clients, retry policies, and environment configuration belong in `TradeControl.Tax.UK.Adapters.Submission`. Production selection must not be the implicit fallback from an unknown environment name.

`SuccessSemantics` must distinguish transport receipt, schema acceptance, processing acceptance, and final filing acceptance. Correlation identifiers and status transitions should be represented as typed submission results so the audit layer can retain the authority's response without parsing log text.

## Why Flat DTOs Are Not Sufficient

Flat DTOs remain useful at narrow wire boundaries, but they are insufficient for the complete company filing domain.

- A generated CT600 XML graph is appropriately DTO-like because the external schema defines its shape.
- Companies House and HMRC XML envelopes are appropriately DTO-like for the same reason.
- Statutory statements need typed aggregates because values have accounting relationships and current/comparative meaning.
- Notes, fixed-asset movements, director transactions, share classes, and supplementary pages have conditional or repeating structures.
- XBRL facts require QNames, contexts, units, dimensions, data types, and duplicate-aspect rules.
- iXBRL is both a machine-readable fact document and a human-readable XHTML presentation.
- A Corporation Tax package is an aggregate of several independently versioned artifacts with cross-document invariants.

The appropriate combination is therefore typed domain aggregates, generated wire DTO graphs, a generic but strongly typed XBRL fact model, and explicit adapters between them.

## Isolation from Trade Control Implementation

The statutory contract layer must compile and pass its offline tests without a Trade Control database, schema, category tree, cash code, Tax Tag, template, web application, or authentication session.

The following dependencies are prohibited:

- `tb_*` or `vw_*` database objects;
- database primary keys or SQL types;
- cash codes, category codes, category expressions, or tree-node identifiers;
- Tax Tag identifiers;
- Razor, HTML, or other existing report templates;
- legacy `AC`/`CP` type constants or assumptions;
- controller/view models; and
- application service locators or database-backed configuration.

A later population workflow will be coordinated by `TradeControl.Tax.UK.Application`, using source-data and mapping implementations from `TradeControl.Tax.UK.Adapters.TradeControl`. Those higher layers may reference this contract assembly to create accounts and computation aggregates and produce reconciliation evidence. The contract assembly must never reference either of them.

To enforce the boundary, contract tests should inspect project references and namespaces as well as behaviour. The contract project currently has no project or package references; future dependencies must be limited to platform libraries and explicitly approved XML/document dependencies.

## Offline Fixtures and Contract Tests

Tests must run without network access and without credentials. They should use pinned official schemas/taxonomy assets where redistribution and repository-size policy permit, plus small curated fixtures that exercise the supported surface.

The minimum test inventory is:

### Infrastructure tests

- QName equality uses namespace URI plus local name and ignores the chosen prefix.
- Instant and duration contexts serialize correctly and cannot be confused.
- Context and unit value-deduplication is deterministic.
- Explicit dimensions participate in fact identity and duplicate detection.
- Monetary, integer, decimal, boolean, date, enumeration, and narrative facts retain their wire type.
- Explicit zero is serialized while an absent optional fact is omitted.
- XML parsers reject DTD/external-entity input.
- Repeated serialization produces identical bytes and digests.

### Statutory accounts tests

- A representative micro-entity accounts fixture contains current and comparative statement-of-financial-position facts.
- Required approval, signing, exemption, and average-employee facts are enforced for the selected profile.
- A repeating directors' advance/guarantee disclosure retains each occurrence and its dimensions/context.
- Arithmetic and note-to-face reconciliation failures produce stable finding codes and object paths.
- Full and filleted projections leave the shared statutory accounts object unchanged.

### Companies House tests

- A current TIS 5.9 filing serializes with exact namespaces, element ordering, envelope metadata, and document attachment relationship.
- Filleted and full fixtures contain the correct authority-specific content.
- Presenter/company credentials are required only at submission and never appear in artifact manifests or diagnostic serialization.
- Initial receipt, pending status, accepted status, rejected status, and authority-error fixtures deserialize exactly.
- Endpoint inventory identifies production/test/validator operations and the status-polling lifecycle.
- Future/preview contracts cannot be selected by production orchestration.

### HMRC Corporation Tax tests

- A core CT600 fixture round-trips through generated wire types with exact literals and namespaces.
- A CT600A fixture demonstrates a conditional supplementary page and matching page indicator.
- A declared but absent supplementary page, or an undeclared supplied page, is rejected.
- Accounts profit reconciles to the computation start, adjustments and capital allowances reconcile to taxable profit, and the computation reconciles to CT600 liability boxes.
- HMRC accounts and computations are two separate iXBRL documents with the correct entry points and document roles.
- The logical package serializes according to the official envelope/attachment schema and does not assume an invented archive format.
- A long accounts period composes into two non-overlapping Corporation Tax packages with distinct computations and a single Companies House filing.
- Version selection at effective-date boundaries is deterministic.
- A known acknowledgement and representative schema/business-rule errors deserialize without loss of unknown forward-compatible fields where the schema allows extensions.

### iXBRL tests

- Golden iXBRL fixtures are XML/XHTML well formed and validate against the pinned local taxonomy/schema set.
- Every displayed test value is linked to the expected inline fact.
- Current and comparative facts use the correct duration/instant contexts.
- Creditors and other dimensional breakdown fixtures use the expected dimension/member QNames.
- Units, scale, sign, decimals, nil state, continuations, and escaped narrative content serialize correctly.
- No orphan facts, contexts, units, or presentation anchors are introduced.
- Rendered structural snapshots remain stable; later visual tests may render fixtures to images for human-layout regression review.

Official positive examples should be retained unmodified where licensing permits. Negative fixtures should each identify the single rule they are intended to violate. Fixture manifests should record source, contract version, expected outcome, and checksum.

Network validation and test-submission suites belong in a later integration-test project. They must not replace offline contract tests, because official services may be unavailable or change independently of the pinned implementation.

## Implementation Sequence After Approval

The recommended implementation order is:

1. Use the already-established `TradeControl.Tax.UK.Company.Contracts` and `TradeControl.Tax.UK.Company.ContractTests` project boundary; add version/status descriptors, validation result types, and the first substantive offline tests.
2. Add XBRL primitives, taxonomy manifest/catalog loading, deterministic context/unit handling, and focused contract-infrastructure fixtures.
3. Add the minimum authority-neutral statutory accounts and tax computation aggregates for the approved filing profile.
4. Pin current official schemas/taxonomies and establish reproducible generation for generated wire types and catalogs.
5. Implement Companies House TIS 5.9 contracts, serializer boundaries, projection rules, and offline fixtures inside `Company.Contracts`.
6. Implement current CT600/RIM contracts and supplementary-page applicability validation inside `Company.Contracts`.
7. Implement HMRC accounts and computation iXBRL projections and cross-document reconciliation.
8. Implement logical package/envelope serialization and acknowledgement/status contracts.
9. Add typed application ports and orchestration in `TradeControl.Tax.UK.Application`, followed by Trade Control population/mapping implementations in `Adapters.TradeControl`.
10. Add live Companies House and HMRC clients in `Adapters.Submission` only after documents and packages pass offline validation.
11. Add official validator and test-service integrations after offline conformance is stable.

Each increment should end with an exact fixture that proves the new contract behaviour. External transport should not begin until documents and packages can be built and validated entirely offline.

## Decisions and Recommendations Summary

- Use one authority-neutral statutory semantic layer, but separate Companies House and HMRC projections and submission packages.
- Treat QNames as namespace URI/local-name keys; treat prefixes as serialization aliases.
- Keep versioned taxonomy bindings outside the semantic model.
- Model contexts, units, dimensions, facts, presentation, and artifacts explicitly; do not flatten iXBRL into heading/value DTOs.
- Generate large official XSD/RIM wire graphs and taxonomy catalogs; handwrite semantic aggregates, adapters, validators, renderers, and package composition.
- Keep accounts periods and Corporation Tax periods as different types and explicitly compose one or two Corporation Tax packages as required.
- Keep present and preview contracts in separate namespaces and prevent implicit production selection of previews.
- Reuse the discipline of the Objective 3 endpoint inventory and offline fixture tests, but introduce submission-service metadata suitable for XML gateways and asynchronous acceptance.
- Make the contract assembly independent of Trade Control storage and classification. Category-tree and Tax Tag work belongs in a later population layer.
- Use the established neutral `TradeControl.Tax.UK.Company.Contracts` assembly; do not place company contracts in the MTD Income Tax or VAT assemblies.

## Open Questions / Decisions Required

The assembly, repository, project-reference, test-project, and live-transport boundaries are settled by the completed repository restructuring. The remaining decisions concern filing scope and implementation policy:

1. **Initial company profile:** is the first supported production profile a UK private micro-entity with ordinary trading activity, unaudited accounts where eligible, and no group/overseas/insurance/tonnage-tax complexity?
2. **Supplementary pages:** approve generating all pages present in the current CT600 schema while enabling population only for the explicitly supported subset, initially core CT600 plus CT600A where required?
3. **Computation breadth:** approve an initial computation surface covering trading profit adjustments, plant and machinery capital allowances, losses, taxable total profits, rate calculation, reliefs, liability, and payment reconciliation, with other scenarios rejected as unsupported?
4. **Companies House baseline:** approve TIS 5.9 as the current implementable contract and keep the expected 2028 service as an isolated unavailable/preview family until final official specifications exist?
5. **Taxonomy assets:** may the implementation check pinned official schemas and taxonomy files into the `tax-hub` repository when licensing permits, or should it check in only derived catalogs plus a separately provisioned offline validation bundle?
6. **Generation tooling:** should the implementation select and pin a .NET schema-generation tool after a short proof against the current RIM/GovTalk schemas, with generated source committed for deterministic builds?
7. **iXBRL renderer:** should the initial renderer be a small XML-aware, contract-specific .NET document builder, with external XBRL tooling used for validation rather than as an application runtime dependency?
8. **Filleting workflow:** should full statutory accounts be the immutable approved source, with Companies House filleting always represented as an explicit, separately approved filing projection?
9. **Evidence and storage:** should artifact manifests and SHA-256 digests form part of the contract now, while persistence locations and retention policy remain a later application/audit design decision?
10. **Acceptance threshold:** which official Companies House validator/test-service and HMRC test-submission outcomes will constitute the implementation milestone for “conformant,” distinct from any broader claim of HMRC approval?
