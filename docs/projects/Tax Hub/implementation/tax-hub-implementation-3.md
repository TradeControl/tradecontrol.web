# Tax Hub — Implementation Guide 3

## Statutory data provision, request population and exact artifact previews

8 September 2026  
Status: implementation-design handoff; no implementation authorised by this document

## 1. Purpose and terminology

Objective 3 covers four Accounts Mode service families:

1. HMRC VAT;
2. HMRC MTD Income Tax for sole traders;
3. HMRC Corporation Tax; and
4. Companies House accounts filing.

“MTD” may be used conversationally for HMRC digital tax services. Architecture, code and acceptance evidence must name the exact family. `MTD Income Tax` does not implicitly include Corporation Tax, and Companies House remains a separate authority even where it consumes related company accounts.

The implementation has four layers:

1. jurisdiction-neutral provision of identity, registrations, reporting profiles and effective settings which are not Category Tree facts;
2. authority-neutral accounting evidence from Trade Control;
3. UK population of the existing versioned authority contracts; and
4. WebHarness previews of safe metadata and the exact JSON, XML, XHTML/iXBRL or package bytes Objective 4 would transmit.

This phase is offline. It must not call an authority, authenticate, construct fraud-prevention headers, poll a filing service or record a preview as a submission.

## 2. Required outcome

A developer must be able to select a configured fixture source and supported operation and inspect:

- jurisdiction, authority, operation, contract family/version and preview status;
- HTTP method/path/query/headers or submission-service/package metadata;
- exact canonical bytes and media type for each body, document or package;
- SHA-256 digests over those exact bytes;
- validation and reconciliation findings; and
- subject, period, projection and configuration provenance.

The first corporate slice must produce statutory accounts, accounts iXBRL, Corporation Tax computation, computation iXBRL, CT600 return package and Companies House accounts package for the approved ordinary UK private micro-company profile.

The first API slices must produce one exact VAT return and one exact MTD Income Tax cumulative-period summary for each supported MIN and STD sole-trader profile.

Corporation support is the principal delivery priority, VAT directly supports corporate users, and sole-trader MTD is a fully supported capability intended to broaden the user base. Delivery order does not lower the acceptance standard for any family.

Previewed bytes must be the bytes later passed to Objective 4. Objective 4 must not reconstruct, remap, round, repackage or reserialize them. Bodyless operations must not acquire an invented payload.

## 3. Existing boundaries

Objective 2 projections and legacy harness payloads are evidence, not wire contracts. `PayloadHarnessEnvelope`, `VatHarnessPayload`, `MicroHarnessPayload`, historical `AC`/`CP` tags, string operation names and loose dictionaries must not sit on the new preparation path. Company accounts and computations must not be flattened into the sole-trader keyed-fact shape.

The authoritative contract surface is:

- `TradeControl.Tax.UK.Hmrc.Vat.Contracts`;
- `TradeControl.Tax.UK.Hmrc.MtdIncomeTax.Contracts`; and
- `TradeControl.Tax.UK.Company.Contracts`, including `TradeControl.Tax.UK.Hmrc.CorporationTax` and `TradeControl.Tax.UK.CompaniesHouse`.

Population targets these types directly. WebHarness and SQL must not reproduce wire names, versions, paths, namespaces, taxonomy identities or package rules.

Objective 4 adds authentication, routing, transport, polling, responses and submission audit. It consumes the immutable artifacts produced here unchanged.

## 4. Current implementation assessment

Reusable assets include typed VAT/Income Tax descriptors and DTOs; statutory accounts and Corporation Tax models; CT600 RIM 1.994 types; deterministic iXBRL, HMRC package and Companies House package serializers; contract tests; `App.tbOptions.SubjectCode`; reusable subject/address/company data; Trade Control readers; `Cash.vwTaxVatSubmission`; `Cash.fnTaxBizCumulative(...)`; company/accounting SQL; and four corporate/sole-trader MIN/STD sandbox databases:

- `tcNodeDb4-COMIPFVT1-COMIN26` — company, minimal template;
- `tcNodeDb4-COSIPFVT1-COSTD26` — company, standard template;
- `tcNodeDb4-STMIPFVT1-STMIN26` — sole trader, minimal template; and
- `tcNodeDb4-STSIPFVT1-STSTD26` — sole trader, standard template.

Development access is resolved at runtime from the `TCWeb` client-secret setting `ConnectionStrings:TCNodeContext`. Its value must never be copied into source, documentation, fixtures, test output, prepared artifacts or logs. Database selection changes only the validated catalogue/database target; it must not accept an arbitrary connection string from a WebHarness request.

Gaps include statutory registration/profile storage, complete source ports, company population, adapter-boundary translation, a common artifact abstraction, WebHarness mapping leakage and missing Application/adapter/host tests.

## 5. Dependency direction

```text
VAT.Contracts       MtdIncomeTax.Contracts       Company.Contracts
      \                       |                         /
       +----------------------v------------------------+
                       UK.Application
                              ^
                              |
            Adapters.TradeControl / statutory context
                              ^
                              |
                         WebHarness

UK.Application <------ authority gateways (Objective 4)
```

Contracts remain independent of Application, SQL, ASP.NET Core and adapters. Application owns neutral models, typed use cases, UK population, validation and prepared artifacts. The Trade Control adapter alone knows SQL names, connections, `Tc*` rows and Tax Tag codes. WebHarness binds and presents; it does not populate or serialize. Preview routes cannot invoke submission adapters. Secrets and connection strings never enter application requests or results.

## 6. Statutory data provision

`App.tbOptions.SubjectCode` identifies the home/reporting subject. Because one Trade Control node represents one business in one jurisdiction, `App.tbOptions.JurisdictionCode` owns the required business/tax jurisdiction and references `App.tbJurisdiction`; Tax Tag sources inherit it rather than duplicating it. Bootstrap selects jurisdiction first and defaults `App.tbOptions.UnitOfCharge` from `App.tbJurisdiction.UocCode`; an explicit currency override does not alter jurisdiction. A company's more specific incorporation/registry jurisdiction remains a distinct legal-profile value where required. Existing `Subject.tbSubject`, selected `Subject.tbAddress` and `Subject.tbVirtual` remain authoritative for reusable name, address, contact, company number, VAT number and business description.

The jurisdiction-neutral logical model comprises:

```text
Jurisdiction
Authority
RegistrationScheme
SubjectRegistration
ReportingProfile
SettingDefinition
ReportingProfileSetting
StatutoryContextReadiness
```

Initial UK seed data may define HMRC, Companies House, VAT registration, NINO, UTR, company registration and the four reporting-profile families. UK names belong in seed data and UK policy, not generic table/column names.

Use relational structures for identities and relationships. A controlled data dictionary may define sparse settings and permitted values, but must not replace foreign keys, cardinality, uniqueness, typing, effective dates, provenance or sensitivity/masking. Existing VAT and company numbers remain free user-maintained fields in `Subject.tbVirtual`; they are not duplicated in the dictionary, and local checks must not claim legal validity that only the authority can determine. Secrets, filing declarations and authority response IDs remain outside master data.

Before population, a requirement matrix assigns one authoritative owner to every input: reusable identity, registration, reporting profile, effective setting, accounting projection, reviewed workflow input, authority response state or secret. Ambiguous/unowned inputs block the operation.

## 7. Authority-neutral accounting evidence

Initial aggregates are:

- `CompanyStatutorySource`: identity, current/comparative periods, profile, statements, notes, approval context and validation;
- `CorporationTaxSource`: CT periods, profit/loss, adjustments, allowances, gains, losses/reliefs, calculation and reconciliation;
- `VatReturnSource`: period and nine named VAT-box values; and
- `BusinessIncomeSource`: cumulative sole-trader keyed income/expense facts.

Every aggregate carries source/version, period, provenance and readiness evidence. Absence, explicit zero, unsupported, invalid and not-applicable remain distinct. Money remains `decimal`; contract profiles own rounding and presentation.

Application exposes narrow asynchronous ports, not a generic repository.

## 8. Trade Control adapter

`Adapters.TradeControl` implements statutory-context and accounting ports. It resolves safe source keys, performs parameterised cancellation-aware reads, translates rows into neutral aggregates, preserves signs/nullability/provenance, rejects contradictions, consumes authoritative validation and obtains a coherent snapshot.

VAT and sole-trader sources remain `Cash.vwTaxVatSubmission` and `Cash.fnTaxBizCumulative(...)`. Tax reporting/accounting windows are calculated from `Cash.tbTaxType` by `Cash.fnTaxTypeDueDates(...)`; `App.tbYearPeriod` supplies the underlying calendar but is not itself the reporting-period authority. Business tax uses `Cash.fnTaxTypeDueDates(Cash.fnGetBizTaxType(), 0)`, while VAT uses tax type `1`. The work plan must inventory the smallest authoritative company accounts/Corporation Tax projections. Missing projections require separately reviewed `sqlnode` work; no ad hoc joins may manufacture facts.

## 9. Population and validation

Use typed use cases such as `PrepareStatutoryAccounts`, `PrepareCorporationTaxReturnPackage`, `PrepareCompaniesHouseAccountsPackage`, `PrepareVatReturn` and `PrepareCumulativePeriodSummary`. Reflection mappers and arbitrary dictionary dispatchers are prohibited.

Every byte-bearing use case selects an exact descriptor, validates typed context, reads neutral evidence, requires readiness, applies a versioned UK profile, populates exact contracts, validates cross-field/cross-document reconciliation, serializes/packages once, calculates digests and returns an immutable artifact.

Validation stages cover input, statutory context, source, accounting/reconciliation, mapping, population, contract/cross-document and wire/package validity. Errors block bytes; warnings remain visible.

Company profiles additionally own comparative periods, taxonomy selection, CT-period allocation, accounts/computation reconciliation, supplementary-page applicability and package composition. Sole-trader profiles explicitly select MIN consolidated or STD detailed expenses and prohibit both shapes together.

## 10. Prepared artifacts

```text
PreparedStatutoryArtifact
  jurisdiction + authority + operation + contract/version
  preview status + media type + exact bytes + SHA-256
  source evidence + findings

PreparedApiRequest
  method + relative path + ordered query + contract headers
  optional exact body artifact

PreparedSubmissionPackage
  submission service + exact package/envelope artifact
  named document artifacts + asynchronous/polling semantics
```

Package bytes transmitted are distinguished from constituent document bytes retained for inspection. Artifacts exclude base addresses, credentials, fraud headers, mutable DTOs, responses and fabricated submission references.

## 11. Service-family coverage

### 11.1 Corporation Tax and accounts filing

```text
CompanyStatutorySource -> StatutoryAccounts -> accounts iXBRL
CorporationTaxSource + accounts -> computation -> computation iXBRL
                               -> CT600 -> HMRC Corporation Tax package
approved accounts              -> Companies House accounts package
```

Implement the approved ordinary UK private micro-company first and fail closed for unsupported group/specialist scenarios. HMRC and Companies House packages remain distinct despite shared evidence.

### 11.2 VAT

VAT return submission is the first accounting-body slice. Obligations, view return, liabilities, payments, penalties, financial details and customer information receive typed request descriptions until Objective 4 obtains responses.

### 11.3 Sole-trader MTD Income Tax

The cumulative self-employment summary is the first slice, with MIN consolidated and STD detailed profiles. Remaining annual, adjustment, loss, configuration, calculation, finalisation and enquiry families require explicit source and support decisions. Sole-trader support is a complete product capability, not a demonstration path.

Maintain an API descriptor matrix for VAT/Income Tax and a service/package matrix for Corporation Tax/Companies House. Every contract is supported, deferred or unsupported with identifiers, sources, route, fixtures and gateway shape recorded.

## 12. WebHarness previews

Every artifact has safe inspection metadata and every byte-bearing artifact has a raw route which writes stored bytes directly with the exact media type. JSON is not wrapped/pretty-printed; XML/XHTML is not reparsed/reformatted; packages are not rebuilt on retrieval.

Use coherent routes for company accounts/documents, HMRC Corporation Tax packages, Companies House packages, VAT bodies and sole-trader bodies. A bounded expiring in-memory store may retain immutable bytes and safe metadata. It is not an audit. Errors expose stable findings and no partial bytes; previews never claim submission or approval.

## 13. Serialization, tests and delivery

Contract assemblies own canonical JSON, XML, XHTML/iXBRL and package policy. Application digests only final bytes; WebHarness returns them; Objective 4 transmits them.

Add Application, Trade Control adapter and WebHarness tests. Contract tests protect golden bytes, versions, previews, schemas/taxonomies and packages. Application tests prove mappings, CT allocation/reconciliation, VAT boxes, sole-trader mappings and state distinctions. Adapter tests use sanitised snapshots for ordinary repeatable runs and the four named sandbox databases for categorised integration, reconciliation and artifact-generation runs. Host tests prove raw identity, media types, expiry, redaction and absence of outbound calls. Architecture tests protect dependency direction.

Delivery sequence:

1. statutory data provision and complete corporate/VAT/sole-trader fixture profiles;
2. coverage matrices, canonical serializers and prepared-artifact core;
3. company statutory accounts and iXBRL;
4. Corporation Tax computation, CT600 and HMRC package;
5. Companies House accounts package;
6. VAT exact-body slice;
7. sole-trader MIN/STD cumulative slice;
8. remaining approved operations; and
9. unchanged Objective 4 gateway proof.

## 14. Completion criteria

Objective 3 completes when all statutory inputs have authoritative sources; the data schema is jurisdiction-neutral; all four family inventories are classified; corporate fixtures produce reconciled accounts, computation, CT600 and both packages; VAT and MIN/STD fixtures produce deterministic JSON; errors block bytes; all supported artifacts have golden bytes/digests; bodyless operations have no body; previews disclose no secrets and cannot call authorities; tests run offline; and fake Objective 4 gateways receive exact prepared API requests/packages unchanged.

## 15. Explicit non-goals

This guide does not authorise live submissions, authentication, environment routing, fraud headers, presenter credentials, response polling/audit, unsupported corporate profiles/supplementary pages, invention of missing facts, unreviewed accounting changes, replacement of SQL reconciliation, universal reflection/EAV mapping or transport-time regeneration.

## 16. Objective 4 boundary

```text
statutory context + accounting projections
  -> neutral evidence -> versioned UK population -> exact contracts
  -> canonical serialization/document/package assembly once
  -> PreparedApiRequest or PreparedSubmissionPackage
  -> exact WebHarness previews -> Objective 4 gateway (later)
```

Objective 4 begins at the last arrow and must not reinterpret evidence or regenerate prepared bytes.
