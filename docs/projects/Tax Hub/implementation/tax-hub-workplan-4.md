# Tax Hub — Objective 3 Data Provision and HMRC Request Population Work Plan

## Objective

Complete Objective 3 in two parts:

1. establish a jurisdiction-neutral statutory data-provision foundation for subject identity, registrations, reporting profiles, authority identifiers and effective-dated settings which do not belong in the Category Tree; and
2. populate the existing Corporation Tax and Companies House contracts from authoritative company evidence; and
3. populate the existing VAT and MTD Income Tax contracts from authoritative accounting evidence, exposing exact offline artifact previews through `TradeControl.Tax.UK.WebHarness`.

At completion, every current VAT and MTD Income Tax endpoint descriptor and every Corporation Tax and Companies House service/package descriptor must be classified as supported, deferred or unsupported for Accounts Mode. Every supported operation must have a typed preparation path which returns:

- the selected contract operation and version;
- production or explicitly selected preview status;
- HTTP method;
- resolved relative path;
- ordered query parameters;
- required contract media-type headers;
- exact canonical JSON, XML, XHTML/iXBRL, document or package bytes;
- a SHA-256 digest of each exact byte-bearing artifact;
- safe source-period and projection provenance; and
- structured validation and reconciliation findings.

The minimum end-to-end accounting coverage is:

1. statutory accounts, accounts iXBRL, Corporation Tax computation, computation iXBRL, CT600/HMRC package and Companies House accounts package for the approved corporate profile;
2. one exact VAT return generated from `Cash.vwTaxVatSubmission`; and
3. one exact MTD Income Tax self-employment cumulative-period summary generated from `Cash.fnTaxBizCumulative(...)` for each supported MIN and STD profile.

Corporation support is the principal delivery priority. VAT and sole-trader MTD Income Tax remain complete supported capabilities with the same evidence, validation and exact-byte standard.

This work ends at a prepared, validated and immutable request. It does not authenticate with HMRC or send anything to HMRC.

Part I is deliberately not named or modelled as an HMRC subsystem. The initial consumer is UK Tax Hub, but the schema must be capable of representing another jurisdiction, authority, registration scheme and reporting profile without adding country-specific columns to `App.tbOptions` or redesigning the core relationships.

---

## Governing Inputs

Implementation must follow:

- `docs/projects/Tax Hub/implementation/tax-hub-implementation-3.md`;
- the endpoint descriptors and request types in `TradeControl.Tax.UK.Hmrc.Vat.Contracts`;
- the endpoint descriptors and request types in `TradeControl.Tax.UK.Hmrc.MtdIncomeTax.Contracts`;
- the existing VAT and MTD Income Tax contract tests and fixtures;
- the authoritative Trade Control projections and validation implemented in `sqlnode`; and
- the Objective 2 / Objective 3 reconciliation recorded in `docs/projects/Tax Hub/findings.md`.

Where these sources disagree, externally governed HMRC contract evidence governs the wire contract, while authoritative Trade Control SQL governs the accounting facts. The Application population layer owns the explicit translation between them.

---

## Architectural Boundary

The implementation path is:

```text
App.tbOptions.SubjectCode
        -> canonical subject identity and address
        -> jurisdiction/authority registrations and reporting profiles
        -> effective-dated statutory settings
Trade Control SQL projections
        -> Adapters.TradeControl
        -> authority-neutral source aggregates and readiness evidence
        -> UK versioned population use case
        -> exact Corporation Tax, Companies House, VAT or MTD Income Tax contract
        -> canonical contract serializer, invoked once
        -> PreparedApiRequest or PreparedSubmissionPackage
        -> WebHarness inspection and raw-body previews
        -> Objective 4 gateway port, without transport
```

Dependency rules:

- contract projects remain independent of Application, SQL, ASP.NET Core and adapters;
- Application owns source semantics, source ports, typed preparation use cases, population policy, validation orchestration and prepared-request artifacts;
- `Adapters.TradeControl` alone knows connection configuration, SQL object names, database conventions, `Tc*` row shapes and Tax Tag codes;
- WebHarness binds typed HTTP input and presents Application results; it does not query SQL, map Tax Tags or serialize HMRC bodies;
- `Adapters.Submission` is not invoked by any preview path;
- company XML/iXBRL and authority packages are first-class outputs of this work plan; and
- no request, result, preview, log or prepared artifact may contain a connection string, credential, OAuth token or fraud-prevention header value.

Data ownership rules:

- `App.tbOptions.SubjectCode` identifies the home/reporting subject;
- `Subject.tbSubject`, the selected `Subject.tbAddress` and `Subject.tbVirtual` remain authoritative for reusable name, address, contact, company-number, VAT-number and business-description data already held there;
- statutory data provision references those records and must not create competing copies merely to satisfy a request contract;
- jurisdictions, authorities, registration schemes and setting definitions are controlled data;
- subject registrations, reporting profiles and setting values are typed/effective-dated relational data;
- a data dictionary may define sparse extensible attributes, but a single unconstrained key/value table must not replace core identities, foreign keys, uniqueness, value typing or temporal rules; and
- UK/HMRC names belong in seeded jurisdiction/authority data and UK adapter policy, not in generic table or column names.

The existing Objective 2 harness payloads and builders may remain temporarily as clearly labelled legacy diagnostics. They must not be reused as HMRC wire models and must not sit on the new preparation path.

---

## Decisions Adopted by This Work Plan

### Accounts Mode support policy

The mandatory accounting-body operations for this plan are VAT return submission and self-employment cumulative-period submission. Other descriptor families are enabled only when the operation coverage matrix records a defensible Accounts Mode use case and an authoritative source for every request value.

Having a contract DTO does not make an operation supported. Any descriptor not implemented remains visible in the matrix and fails closed as deferred or unsupported.

### Identity and configuration

- Accounting values and their provenance come from Trade Control projections.
- Source selection uses a safe configured source key resolved in composition; a connection string is never use-case input.
- Reusable subject name, address, contact and existing organisation identifiers are read from the home subject selected by `App.tbOptions.SubjectCode`.
- NINO, UTR and HMRC business ID are resolved through the statutory data-provision foundation. VRN and company number remain the free user-maintained `Subject.tbVirtual.VatNumber` and `CompanyNumber` fields and are not duplicated.
- Accounting basis, tax year and obligation period must have a recorded source. They must not be inferred from a TaxSourceCode, database name or legal-form label.
- Fixture-safe identifiers live in test configuration or typed test input and are visibly synthetic.

### Cumulative expense shapes

Support both contract shapes where the accounting profile provides authoritative evidence:

- MIN uses the configured `consolidatedExpenses` projection;
- STD uses the detailed expense projection; and
- no request may populate detailed and consolidated expenses together.

The profile/mapping selection is explicit versioned policy. Unsupported, duplicate or contradictory source keys block preparation.

### Contract versions and preview contracts

The operation coverage matrix pins each supported tax-year range to an exact existing endpoint descriptor and request type. Production descriptors are the default. A preview descriptor is usable only through an explicit typed opt-in and must remain labelled as preview throughout the artifact and HTTP response.

An unrecognised tax year, version or preview selection fails closed. Dates alone must not silently select a future preview contract.

### Preview lifetime

WebHarness uses a bounded, short-lived in-memory preparation store so one immutable artifact backs both inspection and raw-body retrieval. Entries have opaque preparation IDs, a fixed size limit and expiry. The store is diagnostic only, is cleared on restart, and is not a submission audit.

The store retains the final bytes and safe metadata only. It must not retain mutable contract objects, connection strings or credentials.

### Validation and HTTP policy

Validation findings have stable codes, severity, stage, message, source path and contract path where applicable.

- invalid typed input returns `400 Bad Request`;
- a configured source or requested dataset that does not exist returns `404 Not Found`;
- source, accounting, mapping, population or contract failures return `422 Unprocessable Entity`;
- a known but unsupported/deferred operation or scenario returns `409 Conflict`;
- expired or unknown preparation IDs return `404 Not Found`; and
- unexpected failures use RFC problem details with a correlation ID and no internal SQL or secret detail.

Blocking findings prevent creation or storage of body bytes. Warnings remain visible in inspection output.

### Integration-test strategy

Application and ordinary adapter tests use checked-in, sanitised source snapshots and fakes so they run quickly and offline. Routine integration work uses two active development sandboxes:

- `tcNodeDb4-COSIPFVT1-COSTD26` — company, standard template;
- `tcNodeDb4-STSIPFVT1-STSTD26` — sole trader, standard template.

The MIN databases are retained as versioned reference snapshots rather than routinely provisioned:

- `tcNodeDb4-COMIPFVT1-COMIN26` — company, minimal template; and
- `tcNodeDb4-STMIPFVT1-STMIN26` — sole trader, minimal template.

`App.tbInstall` records the released major/minor line separately from the development `SQLBuild`. During development, `Settings:SqlNodeVersion` identifies the compatible release line as `4.1.*`, while the latest install row identifies the exact database build. A database is never classified by name or assumed current merely because it is reachable. The two parked databases may be deliberately reactivated or an active database regenerated with a MIN template when live MIN database evidence is required; ordinary MIN/STD contract-shape coverage remains offline.

Development credentials are resolved at runtime from the `TCWeb` client-secret setting `ConnectionStrings:TCNodeContext`. The secret value must never be copied into configuration committed to the repository, documentation, fixtures, test output or logs. Ordinary tests must not depend on mutable sandbox state for their pass/fail result; sandbox runs provide integration, reconciliation and approved golden-artifact evidence.

---

# Part I — Statutory Data Provision

## Purpose and Boundary

Part I establishes the non-ledger data required to construct statutory reports and authority requests. It answers four questions before population begins:

1. which legal or natural person is reporting;
2. in which jurisdiction and to which authority;
3. under which registration, business/reporting profile and effective rules; and
4. where each value came from and whether it is complete, current and safe to use.

This foundation is not an alternative Category Tree, an accounting projection, an HMRC contract mirror or a generic secrets store. It must not contain endpoint paths, JSON property names, OAuth credentials, access tokens or transport configuration.

The design should support the current UK requirements by seed/configuration data such as:

```text
Jurisdiction: GB
Authorities: HMRC, Companies House
Registration schemes: VAT registration, National Insurance number,
                      Unique Taxpayer Reference, company registration number
Reporting profiles: VAT, self-employment income tax, corporation tax,
                    statutory accounts
```

The codes are examples to be reconciled with existing repository conventions. The relational model must not assume that all authorities use UK tax years, UK identifier formats or one business per subject.

---

## Data Classification

Before adding tables, classify every required value into one of these ownership groups:

| Group | Examples | Owner |
|---|---|---|
| Reusable subject identity | name, address, telephone, email, business description | existing `Subject` schema |
| Existing organisation identity | company number, VAT number | existing `Subject.tbVirtual` pending reviewed migration |
| Authority registration | NINO, UTR, future non-UK registration identifiers | statutory registration model |
| Reporting/business profile | authority business ID, business type, TaxSourceCode association | statutory reporting-profile model |
| Effective-dated setting/election | accounting basis, quarterly period type, periods-of-account choice | statutory setting model |
| Filing-period/workflow input | VAT finalised declaration, annual adjustments, claims, signing approval | preparation/workflow, not master data |
| Authority response state | obligation period key, calculation ID, submission receipt | Objective 4 response/audit model, not Part I master data |
| Secret | OAuth token, client secret, presenter credential | external secret/configuration boundary, not the statutory dictionary |

The classification and source must be recorded in a data-requirement matrix covering every Accounts Mode operation in Part II. A request field with no authoritative owner blocks support for that operation.

---

## Phase DP1 — Statutory Data Requirement Inventory

Status: complete. Evidence and decisions are recorded in `tax-hub-data-provision-inventory.md`.

Inventory all non-Category data required by the Corporation Tax, Companies House, VAT and MTD Income Tax contract families. Reconcile each requirement against:

- `App.tbOptions.SubjectCode`;
- `Subject.tbSubject`;
- the selected `Subject.tbAddress`;
- `Subject.tbVirtual`;
- `App.tbJurisdiction`;
- `Cash.tbTaxTagSource` and TaxSourceCode;
- accounting/tax calendars and periods;
- `Cash.tbTaxType` and the reporting windows calculated by `Cash.fnTaxTypeDueDates(...)`;
- existing application configuration; and
- typed workflow input or future authority responses.

For every value record:

- semantic name, not an HMRC JSON property name;
- jurisdiction and authority where applicable;
- subject-level, registration-level, business-profile-level, tax-year-level or filing-level cardinality;
- authoritative source and steward;
- whether it is existing, missing or ambiguous;
- type, format and validation rules;
- sensitivity and masking requirements;
- effective-dating requirements;
- whether it may be locally configured, must be authority-confirmed or may be operator-supplied; and
- which preparation operations consume it.

The first inventory must explicitly settle:

- confirmation that `Subject.tbVirtual.VatNumber` remains the sole VRN field and is passed without claiming local legal validation;
- confirmation that `Subject.tbVirtual.CompanyNumber` remains the sole company-number field under the same rule;
- the storage and ownership of NINO and UTR;
- the cardinality of HMRC business IDs relative to subject and TaxSourceCode;
- the source of accounting basis/type and quarterly-period configuration;
- the source of periods-of-account dates versus accounting calendar dates;
- the distinction between configured, operator-supplied and authority-confirmed values; and
- whether the current free-form address is sufficient for each later statutory contract or needs a structured-address extension.
- the sources of company jurisdiction, principal activity, accounts framework/profile and reporting periods;
- the ownership of accounts approval date, signing director and statutory declarations;
- the distinction between accounts periods and Corporation Tax periods; and
- which company package inputs are master data, accounting evidence or per-filing workflow input.

### Acceptance criteria

- Every known non-Category request value is classified.
- Existing subject data is reused rather than copied.
- Missing and ambiguous values are explicit.
- Cardinality and effective-dating requirements are known before schema design.
- No filing/workflow value or secret is misclassified as reusable master data.
- The inventory is jurisdiction-neutral while recording UK seed requirements.

---

## Phase DP2 — Jurisdiction-Neutral Schema Design

Status: complete. Implemented in `sqlnode` and TCWeb and provisioned to all four development sandboxes.

Design the smallest relational model required by the DP1 inventory. Reuse `App.tbJurisdiction`, make `App.tbOptions.JurisdictionCode` the required owner of the node's single business/tax jurisdiction, and introduce authority-neutral concepts rather than UK-specific tables.

The target logical model should contain the following concepts; final names must follow `sqlnode` conventions:

```text
Jurisdiction
  JurisdictionCode

Authority
  AuthorityCode
  JurisdictionCode
  AuthorityName

RegistrationScheme
  RegistrationSchemeCode
  AuthorityCode
  Value type, format and sensitivity metadata

SubjectRegistration
  SubjectCode
  RegistrationCode
  RegistrationSchemeCode
  RegistrationValue
  ValidFrom / ValidTo
  Status and provenance

ReportingProfile
  SubjectCode
  ReportingProfileCode
  TaxSourceCode where accounting facts are required
  AuthorityCode
  ProfileTypeCode
  AuthorityBusinessReference where applicable
  ValidFrom / ValidTo or effective tax-year range
  Status and provenance

SettingDefinition
  SettingCode
  JurisdictionCode and/or AuthorityCode
  ValueTypeCode
  permitted-value/validation metadata
  sensitivity and applicability metadata

ReportingProfileSetting
  SubjectCode
  ReportingProfileCode
  SettingCode
  effective period or tax-year range
  one typed value
  value source and review evidence
```

Prefer typed relational columns for stable, frequently queried identities and relationships. Use the setting dictionary only for genuinely sparse/evolving values. If typed value columns are used, enforce that exactly one value is populated and that it agrees with the definition's value type.

Required integrity rules include:

- foreign keys to subject, jurisdiction, authority and TaxSourceCode where applicable;
- no overlapping active registrations for a scheme whose cardinality is one;
- no overlapping active settings for the same profile/setting/effective range;
- no duplicate authority business reference within the applicable authority scope;
- explicit status rather than deletion for historical registrations/profiles;
- row-version and audit columns following repository conventions; and
- validation constraints which are generic structurally, with jurisdiction-specific format validation supplied by policy or reviewed seed metadata.

Treat jurisdiction ownership as a coordinated compatibility migration, not an isolated DDL edit:

1. add `App.tbOptions.JurisdictionCode` in a migration-safe form;
2. backfill it only where existing Tax Tag sources agree;
3. replace bootstrap's primary currency selection with jurisdiction selection, defaulting `App.tbOptions.UnitOfCharge` from `App.tbJurisdiction.UocCode` and allowing an explicit currency override where supported;
4. update company/sole-trader tax templates and test provisioning;
5. update EF models, `NodeContext`, Tax Configurator queries/models and its jurisdiction/source hierarchy;
6. verify all four sandboxes and reject or require review of any database whose sources disagree;
7. make the options relationship required once all creation/upgrade paths supply it; and
8. remove `Cash.tbTaxTagSource.JurisdictionCode` and its foreign key only after no consumer depends on them.

### DP2 implementation checkpoint

Implemented objects:

- controlled catalogues: `App.tbAuthority`, `App.tbRegistrationScheme`, `App.tbReportingType`, `App.tbSettingDefinition`, `App.tbValueType`, `App.tbValueSource` and `App.tbStatutoryStatus`;
- subject values: `Subject.tbRegistration`;
- business/reporting context: `Cash.tbReportingProfile` and `Cash.tbReportingProfileSetting`; and
- node ownership: required `App.tbOptions.JurisdictionCode` referencing `App.tbJurisdiction`.

All catalogue initialization and reset ordering is owned by `App.proc_NodeDataInit`; no DP2 data is initialized by pre- or post-deployment scripts. Synthetic regeneration preserves the existing node jurisdiction and Unit of Charge before calling `App.proc_NodeDataInit`, then supplies both values to `App.proc_NodeBusinessInit` before applying the selected template. Bootstrap, synthetic regeneration, company and sole-trader templates, EF models, `NodeContext`, Tax Configurator and the diagnostic enquiry consume the options-owned jurisdiction. `Cash.tbTaxTagSource.JurisdictionCode` and its foreign key have been removed.

SQL integrity enforces validity ranges, one typed setting value, definition/value-type and scope agreement, allowed-value metadata, unique authority references, reporting-type authority and Tax Source requirements, and non-overlapping active registrations, profiles and settings. Status, provenance, review state, audit columns and row versions are first-class.

The initial UK catalogue definitions contain two authorities, two registration schemes, four reporting types and five setting definitions. They contain no subject registration or reporting-profile values. VAT and company numbers remain solely in `Subject.tbVirtual`; they are not duplicated as subject registrations.

Verification evidence:

- TCWeb and the SQL project build successfully with zero warnings and zero errors;
- all four sandboxes report required `App.tbOptions.JurisdictionCode = UK`, `UnitOfCharge = GBP`, the expected catalogue counts and no legacy Tax Tag jurisdiction column;
- rolled-back SQL tests reject overlapping registrations (`51020`), overlapping profiles (`51022`), typed/allowed-value mismatches (`51023`), and leave no test records behind.

Company incorporation/registry jurisdiction remains a separate legal-profile value and must not be conflated with the node-wide business/tax jurisdiction.

`App.tbJurisdiction.UocCode` is the default Unit of Charge, not the jurisdiction identity. A bootstrap override updates `App.tbOptions.UnitOfCharge` without changing jurisdiction. Once accounting data exists, a Unit of Charge change requires a separately controlled accounting migration and must not remain a casual settings edit. Do not introduce a many-to-many jurisdiction/currency table in DP2 without a demonstrated requirement; it can be added later if jurisdiction-specific alternative-currency eligibility becomes necessary.

Do not force existing `VatNumber` and `CompanyNumber` into a new table. They remain free user-maintained fields in `Subject.tbVirtual`, reflecting the business's legal responsibility to enter the identifiers accepted by the relevant authority. The statutory-context projection reads them directly. It may require nonblank/path-safe values and emit diagnostic warnings, but local validation must not be represented as proof of a valid registration.

### Acceptance criteria

- No table or column is named for the UK, HMRC, VAT, NINO or UTR.
- A second jurisdiction can add authorities, schemes, profiles and settings through reviewed definitions rather than schema changes.
- Core registrations and reporting-profile relationships retain foreign keys and uniqueness constraints.
- The dictionary cannot accept arbitrary untyped values.
- Effective dating and provenance are first-class.
- Existing VAT and company identifiers have one authoritative source.
- Secrets and authority response/audit state are outside the model.

---

## Phase DP3 — Canonical Subject and Statutory Context Projections

**Status: complete (SQL build 4.1.2).**

Implement reviewed read projections or functions which give adapters a coherent statutory context without exposing raw table joins.

At minimum provide:

```text
Home subject identity
  selected through App.tbOptions.SubjectCode
  name, subject kind, selected address, contact details
  existing company/VAT identifiers and business description

Subject registrations
  active registrations by jurisdiction/authority/scheme and effective date

Reporting profile
  subject, TaxSourceCode, authority, profile type and authority business reference
  effective settings with value-source status

Statutory context readiness
  missing, duplicate, expired, unreviewed or contradictory values
```

Projections must expose structured values rather than an opaque JSON document. They should include sufficient row-version/update provenance to show which source state produced a preview.

Address handling requires an explicit decision. The existing `Subject.tbAddress.Address` remains the source of record. If statutory contracts require address lines, town, postcode and country separately, add a reusable structured-address satellite linked to `AddressCode`; do not parse the free-form address during request population and do not add UK-only address columns to the statutory setting dictionary.

### Acceptance criteria

- The home subject is resolved consistently from `App.tbOptions.SubjectCode`.
- Consumers do not duplicate the subject/address/registration joins.
- Effective registrations and settings are resolved for a supplied date or reporting period.
- Missing, expired and contradictory data returns typed readiness findings.
- Projection output is structurally jurisdiction-neutral.
- Address structure is authoritative or explicitly unavailable; it is never guessed during population.

### Implemented evidence

- `Subject.tbAddress.AddressTypeCode` distinguishes trading, registered and finance addresses without changing the free-form address model. Company statutory context selects the registered address explicitly; filing-specific structured lines remain reviewed preparation values.
- Entity type is derived from the existing business-tax configuration (`Cash.fnGetBizTaxType()`); no duplicate legal-form catalogue or subject legal-profile table is maintained.
- `Subject.tbVirtual.RegistryJurisdictionCode` records the optional place of legal registration. `Subject.fnStatutoryIdentity` falls back to the node jurisdiction for Accounts Mode while preserving a distinct value for foreign-incorporated and future MIS subjects.
- `Subject.fnStatutoryIdentity` and `Subject.vwStatutoryIdentity` resolve the home subject exclusively through `App.tbOptions.SubjectCode`, derive subject class from `Subject.tbType`/`Subject.tbClass`, and expose source row versions and update timestamps.
- `Subject.fnRegistration`, `Cash.fnReportingProfile` and `Cash.fnReportingProfileSetting` resolve active records for an explicit date and retain source/status/provenance metadata.
- `App.fnStatutoryContextReadiness` returns typed, non-sensitive findings for missing, expired, duplicate or unreviewed identity/context evidence. Operation policy supplies the registration and setting codes; the generic SQL layer does not embed UK filing requirements.
- Statutory master data has no identity keys: registrations use `(SubjectCode, RegistrationCode)`, profiles use `(SubjectCode, ReportingProfileCode)`, and settings use `(SubjectCode, ReportingProfileCode, SettingCode, EffectiveFrom)`. Default registration/profile codes are generated through `App.proc_DefaultCodeGenerator` wrappers.
- The SQL project builds successfully. Rollback-only tests on the active company STD and sole-trader STD sandboxes proved identity resolution, structural readiness, registration/profile/setting resolution, overlap rejection and complete test-data cleanup.

---

## Phase DP4 — Initial UK Seed Data and Configuration Workflow

**Status: complete (SQL build 4.1.3).**

Seed the generic catalogues with the minimum reviewed UK definitions required by the operation matrix. Initial definitions include HMRC and Companies House authority records; NINO and UTR registration schemes; VAT, self-employment income-tax and company reporting profiles; and controlled settings for accounting type, quarterly period type, periods of account, late-accounting-date-rule election and Class 4 exemption reason where those operations are supported. Existing VAT and company numbers remain on `Subject.tbVirtual` and are not duplicated in the registration catalogue.

Seed values are definitions and allowed literals, not fabricated subject registrations. Fixture templates may supply conspicuously synthetic registrations and business references for test databases.

Provide a controlled maintenance workflow which:

- selects the home subject or another permitted reporting subject;
- creates and effective-dates registrations and reporting profiles;
- associates a business reporting profile with an existing TaxSourceCode;
- requires identifiers needed by the selected operation to be present and safely representable, while treating any local syntax checks as diagnostics rather than proof of authority validity;
- records whether a value is locally configured, operator supplied, imported or authority confirmed;
- masks sensitive identifiers in lists and diagnostics;
- prevents an invalid or incomplete profile being marked ready; and
- never stores credentials or access tokens.

UI design may be delivered separately, but the SQL procedures/service boundary and validation behaviour must exist before Part II relies on the data.

### Acceptance criteria

- UK requirements are represented entirely through generic structures plus reviewed UK definitions/policy.
- Fixture databases can be provisioned with valid synthetic VAT and self-employment profiles.
- NINO and UTR are masked outside the restricted preparation boundary; raw-value authorization is enforced when that boundary is introduced.
- One subject can support multiple authority business profiles without ambiguity.
- A reporting profile maps deliberately to TaxSourceCode.
- Missing required values, overlapping effective records and incomplete profiles are rejected; authority validity is not claimed locally. Path safety is enforced by prepared-request route construction, where the operation descriptor defines whether and how a value enters a path.
- No real credential or production identifier is introduced by seed data.

### Implemented evidence

- `App.proc_NodeDataInit` now owns the reviewed company-account setting definitions for FRS 105, micro-entity accounts and balance-sheet format; entity type continues to come from the existing tax configuration.
- No subject registration, authority business reference, credential or filing secret is created by catalogue initialization.
- `Subject.proc_RegistrationSave`, `Cash.proc_ReportingProfileSave` and `Cash.proc_ReportingProfileSettingSave` provide durable-code, effective-dated maintenance boundaries over the DP2/DP3 tables.
- `Cash.proc_ReportingProfileSave` derives authority from the controlled reporting type and rejects an unreviewed profile being made active.
- `App.fnIdentifierMask` and `Subject.fnRegistrationMasked` provide a non-sensitive registration-list projection while retaining the raw resolver solely for the later restricted preparation boundary. TCWeb does not expose a raw-registration read surface.
- `Subject.tbVirtual.RegistryJurisdictionCode` is maintained through the existing organisation editor. A null value deliberately inherits `App.tbOptions.JurisdictionCode`; no separate Accounts Mode configuration page or legal-form catalogue is required.
- Both active STD sandboxes contain the same nine setting definitions, including the reusable accounting-policies suggestion. The rollback-only fixture proves company-tax and self-employment profile creation, independent HMRC and Companies House profiles, typed setting creation, incomplete and complete readiness states, NINO/UTR masking, generated durable codes and complete cleanup. Earlier negative tests proved that an unreviewed active profile is rejected without persistence.
- Operation-specific registration/setting requirements remain application policy because they vary by contract operation. Raw identifier authorization and path-segment safety remain responsibilities of the prepared-request boundary because no DP4 database object can know the consuming operation or route template.

---

## Phase DP5 — Data Provision Verification Gate

**Status: complete (current SQL build 4.1.5; original gate passed at 4.1.4).**

Add database, adapter and application tests proving that the statutory data foundation supplies every input required by the corporate, VAT and sole-trader mandatory vertical slices.

Verify at least:

- home-subject identity resolution;
- existing VAT-number resolution and normalisation;
- NINO and HMRC business-reference resolution with masking;
- reporting-profile-to-TaxSourceCode association;
- accounting-basis/type resolution for the selected effective tax year;
- MIN and STD profile distinction without relying on database names;
- company number, UTR, registered-office and jurisdiction resolution;
- company accounts profile, principal activity and reporting-period context;
- correct classification of approval/signing/declaration data as filing context rather than permanent registration data;
- missing, duplicate, expired, overlapping and unreviewed value findings;
- provenance/row-version capture; and
- absence of credentials and secrets from projections and diagnostics.

Produce a signed-off data-readiness matrix showing one authoritative source for every corporate, VAT and cumulative preparation input. Any unresolved input remains a blocker or forces the consuming operation to be marked deferred.

The working matrix is maintained in `tax-hub-data-readiness-matrix.md`. It is evidence for this gate, not a substitute for executable verification.

Provision the two active company/sole-trader STD sandboxes with conspicuously synthetic statutory registrations and reporting profiles. Keep the two MIN databases parked until live MIN evidence is required; MIN/STD contract shape remains covered offline and is derived from configuration rather than database names. Resolve the base development connection exclusively through `ConnectionStrings:TCNodeContext`; select only a validated sandbox database name from the fixed catalogue.

### Acceptance criteria

- The schema can provide all non-ledger inputs for company accounts/Corporation Tax packages, VAT and MIN/STD cumulative previews.
- The same schema can represent a hypothetical second jurisdiction without a structural migration.
- All integrity and effective-date tests pass.
- Synthetic fixtures contain no production personal identifiers.
- The data-readiness matrix has no guessed or multiply owned value.
- Part II may begin only after this gate passes.

### Implemented evidence

- The verified matrix in `tax-hub-data-readiness-matrix.md` assigns every permanent non-ledger input to an authoritative source, editable default or filing-workflow boundary.
- `PhaseDP5_Provision.sql` is idempotent, restricted to the four named sandboxes and provisions only conspicuously synthetic reviewed context. It is not part of node initialization.
- `PhaseDP5_Verification.sql` proves persisted company and sole-trader identity, registrations, authority profiles, effective settings, Tax Source association and MIN/STD classification without using database names.
- `PhaseDP5_Portability.sql` proves through complete rollback that a second jurisdiction, authority, registration scheme, reporting type, setting, subject registration and profile can be represented without schema migration.
- `App.proc_StatutoryContext` owns the provider-specific relational composition; `IStatutoryContextSource`, `TcStatutoryContextReader` and `StatutoryContextVerifier` establish the neutral Application/adapter boundary with masked identifiers and source row-version provenance.
- Company and Corporation Tax draft-default factories preserve the distinction between sourced values, system suggestions and explicit operator overrides. Optional schedules default to zero/empty without being persisted as facts.
- The full Tax Hub solution builds without warnings. SQL and Application/adapter gates pass on both active STD sandboxes; the parked MIN databases remain unchanged.

---

# Part II — Corporation Tax and Companies House Population

Part II is the principal population programme. It uses one reconciled company evidence set to produce distinct HMRC Corporation Tax and Companies House artifacts without conflating their contracts or filing lifecycles.

## Phase CO1 — Company Projection Inventory and Source Boundary

**Status: in progress.** Initial inventory and typed source boundary are recorded in `tax-hub-company-projection-inventory.md`; the identified SQL projection work packages require review before the CO1 gate can close.

Inventory the authoritative `sqlnode` accounts and Corporation Tax projections for the approved ordinary UK private micro-company MIN and STD profiles. Map every required `StatutoryAccounts` and `CorporationTaxComputation` semantic to an existing projection, statutory-context value or reviewed filing input.

Define neutral `CompanyStatutorySource` and `CorporationTaxSource` aggregates with current/comparative periods, accounts profile, statements, notes, approval context, CT periods, adjustments, allowances, gains, losses/reliefs, calculation, reconciliation, provenance and value state. Do not reuse historical `AC`/`CP` fields as the model and do not flatten company evidence into `BusinessIncomeSource`.

Any missing authoritative fact becomes a reviewed `sqlnode` work package or an explicitly unsupported scenario; it must not be manufactured in Application.

### Acceptance criteria

- Every required company semantic has one classified source or an explicit gap.
- MIN and STD corporate profiles are reconciled against the implemented contracts.
- Current/comparative and accounts/CT period semantics are explicit.
- Company source ports are narrow, typed and authority-neutral.
- Unsupported group, specialist and supplementary scenarios fail closed.

---

## Phase CO2 — Common Prepared Artifact Core

Implement immutable `PreparedStatutoryArtifact`, `PreparedApiRequest` and `PreparedSubmissionPackage` concepts. The common artifact carries jurisdiction, authority, operation, version/preview status, media type, exact bytes, digest, source evidence and findings.

An API request adds method, relative path, ordered query and headers. A submission package adds service metadata, exact envelope/package bytes, named constituent documents and polling semantics. It must distinguish the bytes actually transmitted from separately inspectable document bytes.

### Acceptance criteria

- JSON requests and company packages share provenance/validation without sharing an unsuitable transport shape.
- No artifact contains mutable contracts, credentials, base addresses or fabricated authority responses.
- Every digest is calculated over final stored bytes.
- Objective 4 gateway ports accept API requests or packages without regeneration.

---

## Phase CO3 — Statutory Accounts and iXBRL Vertical Slice

Populate the existing `StatutoryAccounts` contract from `CompanyStatutorySource`. Enforce company identity, reporting/comparative periods, approved accounts profile, statement reconciliation, notes, audit/exemption statements and reviewed approval/signing context.

Use the contract-owned taxonomy/catalogue and `IxbrlDocumentBuilder` to produce deterministic full and, where authorised, filleted accounts iXBRL. Validate exact taxonomy/version selection, contexts, units, facts, declarations and document digest.

Expose inspection and exact document routes through WebHarness. XML/XHTML bytes must never be reparsed or reformatted after digest calculation.

### Acceptance criteria

- Representative corporate MIN and STD fixtures populate supported statutory accounts without invented facts.
- Current and comparative statements reconcile.
- Full/filleted applicability is explicit.
- Golden iXBRL bytes are deterministic and schema/taxonomy validation passes.
- Raw preview bytes match the prepared document and digest.

---

## Phase CO4 — Corporation Tax Computation and CT600 Vertical Slice

Populate `CorporationTaxComputation` from the approved source and reconcile it to statutory accounts. Allocate long accounts periods into valid Corporation Tax periods, apply explicit versioned adjustment/allowance/loss policy and generate deterministic computation iXBRL.

Populate the exact CT600 contract, including company identity, company number, UTR, return periods, tax calculation, declarations and supported supplementary pages. Assemble `CorporationTaxReturnPackage` using the contract-owned serializer with accounts iXBRL, computation iXBRL and only authorised attachments.

### Acceptance criteria

- Accounting profit/loss reconciles to the computation and CT600 outcome.
- CT period allocation is contiguous and contract-valid.
- Unsupported supplementary pages or facts block the package.
- CT600 RIM/version and package composition are pinned.
- Golden envelope/package and constituent-document bytes are deterministic.
- WebHarness inspection/raw routes return exact bytes and matching digests.

---

## Phase CO5 — Companies House Accounts Filing Slice

Build the separate `CompaniesHouseFilingPackage` from the same approved statutory accounts evidence. Apply Companies House contract/version, delivery profile, registrar statements and package rules independently of HMRC Corporation Tax.

Expose package and constituent-document previews, including asynchronous/polling metadata, without implementing submission or status polling.

### Acceptance criteria

- Companies House and HMRC packages share evidence but not declarations, versions or envelopes.
- The approved filing profile produces deterministic validated package bytes.
- Unsupported replacement/future contracts require explicit preview opt-in.
- Raw package/document previews match stored bytes and digests.

---

## Phase CO6 — Corporate Handoff and Coverage Gate

Complete the Corporation Tax and Companies House service/package matrix. Use fake gateways to prove both prepared packages cross the Objective 4 boundary unchanged, including exact envelope bytes, named documents, media types and polling semantics.

### Acceptance criteria

- Every company contract/service is supported, deferred or unsupported.
- Both mandatory corporate packages work from representative fixtures.
- No live authority transport is reachable.
- Objective 4 can transmit packages without population, serialization or package reconstruction.

---

# Part III — VAT and Sole-Trader MTD Income Tax Population and Preview

Part III retains the original API request implementation sequence. VAT and sole-trader MTD Income Tax are complete supported service families. Their preparation inputs come from Part I statutory context and authoritative accounting projections rather than ad hoc controller input or connection-specific configuration.

---

## Phase 1 — Contract Inventory and Canonical Wire Baseline

Build a generated-from-code operation coverage matrix for every current VAT and MTD Income Tax endpoint descriptor.

For each descriptor record:

- stable operation ID;
- contract family and API version;
- descriptor and request type;
- production or preview status;
- method, path template, path parameters and ordered query parameters;
- Accept and Content-Type values;
- accounting body, configuration body, bodyless command or bodyless enquiry shape;
- required accounting/configuration/identifier source;
- Accounts Mode decision: supported, deferred or unsupported;
- planned typed use case and WebHarness route; and
- contract fixture, population fixture and harness coverage.

Make canonical request serialization explicit in each contract assembly. The serializer must return deterministic UTF-8 bytes with no accidental BOM and must preserve contract property names, enum literals, invariant dates/numbers, optional-member omission, explicit zero and explicit `false`.

Extend contract tests with approved positive and negative golden requests. At minimum cover VAT return, cumulative detailed expenses, cumulative consolidated expenses and every other body-bearing operation marked supported in the matrix.

Do not add population, SQL or WebHarness behaviour in this phase.

### Acceptance criteria

- Every current descriptor is represented exactly once in the matrix.
- Production and preview descriptors are distinguishable.
- Accounts Mode status is explicit and fail-closed.
- Contract-owned serializers produce stable byte-for-byte golden output.
- Absence versus zero and production versus preview behaviour are tested.
- Endpoint inventory tests detect an added, removed or unclassified descriptor.

---

## Phase 2 — Application Source Vocabulary and Validation Model

Add the small authority-neutral source boundary inside `TradeControl.Tax.UK.Application`, using the `TradeControl.Tax.Data` namespace and a clear folder boundary rather than a new project.

Implement typed semantic models for:

- tax subject and legal form;
- reporting period and period kind;
- source dataset identity and capture/projection metadata;
- VAT return source with nine named VAT-box semantics;
- business-income source with typed keyed income/expense facts;
- absent, zero, unsupported, invalid and not-applicable value states;
- fact and dataset provenance; and
- structured source/readiness findings.

Define narrow asynchronous, cancellation-aware ports for:

- reading a VAT return source;
- reading a cumulative business-income source; and
- evaluating source/accounting/mapping readiness.

Define typed selectors and identifiers. Do not put connection strings, SQL row models, HMRC JSON property names or ASP.NET Core types in this boundary.

Create `TradeControl.Tax.UK.Application.Tests` and prove the semantics using in-memory fakes and checked-in source snapshots.

### Acceptance criteria

- Neutral models contain no SQL object names or HMRC wire property names.
- Stable keys, display labels and provenance are distinct.
- Absence, explicit zero, unsupported, invalid and not-applicable states cannot be confused.
- All money remains `decimal` before explicit population rounding.
- Ports describe accounting meaning rather than tables or generic repositories.
- Application tests run without SQL, ASP.NET Core or HMRC connectivity.

---

## Phase 3 — Trade Control Adapter and Safe Source Selection

Refactor `TradeControl.Tax.UK.Adapters.TradeControl` to implement the Application ports and translate SQL rows into neutral aggregates at the adapter boundary. Add a statutory-context port which consumes the Part I projections and returns only the effective subject, registration and reporting-profile data required by a typed preparation use case.

VAT reads use `Cash.vwTaxVatSubmission`. Confirm and correct the existing period selector semantics before relying on the current `StartOn = @StartOn` query.

Cumulative business reads use:

```sql
Cash.fnTaxBizCumulative(@TaxSourceCode, @PeriodStart, @PeriodEnd)
```

The adapter must:

- use parameterised, cancellation-aware SQL;
- resolve a safe configured source key to a connection at composition time;
- resolve identity, authority registrations and effective settings through the Part I statutory context rather than controller-supplied loose values;
- translate dates, nullability, enums, support state and polarity explicitly;
- preserve contributor/source presence so a missing fact is not converted into zero;
- preserve source, Tag-key, mapping and accounting-period provenance;
- reject duplicate, contradictory or incoherent rows;
- consume authoritative accounting and Tax Tag validation results;
- distinguish a missing dataset from a valid nil/zero dataset; and
- read one coherent source snapshot or detect source-version change.

Do not reproduce accounting or reconciliation calculations in C# and do not add ad hoc SQL joins to manufacture missing facts. Any missing authoritative projection becomes a separately reviewed `sqlnode` work package.

Create `TradeControl.Tax.UK.Adapters.TradeControl.Tests` with checked-in adapter row snapshots. Add separately categorised fixture-database integration tests for the four synthetic databases where available.

### Acceptance criteria

- Application and WebHarness no longer receive `Tc*` projection models.
- Connection strings do not cross the composition boundary.
- VAT and cumulative source snapshots can be reproduced from representative fixtures.
- Duplicate, invalid, unsupported and absent-row cases have tests.
- No absolute-value or sign transformation occurs unless the neutral accounting semantic explicitly requires it.
- Integration tests are optional/categorised; the ordinary suite remains deterministic and offline.

---

## Phase 4 — Prepared API Request Mechanics and Common Preparation Pipeline

Extend the Part II prepared-artifact core with `PreparedApiRequest` mechanics shared by typed VAT and MTD Income Tax use cases.

The API request contains:

- stable operation ID;
- contract family and version;
- explicit preview flag;
- method and resolved relative path;
- ordered query pairs;
- safe contract headers;
- Content-Type where applicable;
- exact immutable body bytes or no body;
- SHA-256 digest only when a body exists;
- source evidence; and
- non-blocking findings.

Implement common services for:

- strict path-template resolution and escaping;
- ordered query construction with omission rules;
- contract-header extraction;
- canonical serializer invocation;
- SHA-256 calculation over final bytes;
- validation-stage orchestration; and
- a future Objective 4 gateway port which accepts the prepared artifact unchanged.

The common pipeline must not become a universal mapper or arbitrary operation dispatcher. Each operation retains typed input and explicit construction of its exact contract type.

### Acceptance criteria

- The artifact contains bytes, not a mutable request DTO.
- Bodyless artifacts have neither invented `{}` bytes nor a body digest.
- Digest tests operate over the exact stored bytes.
- Path and query validation rejects missing, duplicate or unknown values.
- No base address, OAuth data, fraud header, connection string or response data is representable in the artifact.
- A fake gateway can receive all prepared fields unchanged without transport.

---

## Phase 5 — VAT Return Vertical Slice

Implement a typed `PrepareVatReturn` use case against the production VAT return descriptor.

The use case must:

1. validate the safe source key/reporting profile, period selector and finalised declaration input;
2. resolve the effective VAT registration and subject provenance through the Part I statutory context;
3. read `VatReturnSource` and readiness evidence through Application ports;
4. enforce dataset existence, period coherence and authoritative reconciliation readiness;
5. map the nine named source values to `VatReturnRequest` explicitly;
6. apply named VAT field rounding/whole-pound rules from the contract authority;
7. validate VAT arithmetic and contract invariants;
8. serialize exactly once with the VAT contract serializer; and
9. return `PreparedApiRequest` with source evidence and digest.

Add typed WebHarness routes:

```text
POST /harness/hmrc/vat/returns/prepare
GET  /harness/hmrc/vat/returns/{preparationId}
GET  /harness/hmrc/vat/returns/{preparationId}/body
```

The prepare response returns safe inspection metadata and the preparation ID. The raw route writes the stored bytes directly with the exact contract Content-Type and `X-TaxHub-Preview: true`.

### Acceptance criteria

- A representative fixture database produces an approved exact VAT return body.
- Explicit zero values survive and optional absence follows contract policy.
- Raw HTTP response bytes equal Application `BodyBytes` byte-for-byte.
- SHA-256 in inspection metadata matches the raw response.
- Invalid arithmetic, missing periods and failed readiness return findings without a body.
- No submission adapter or outbound HTTP handler is reachable from the route.

---

## Phase 6 — MTD Income Tax Cumulative Vertical Slice

Implement a typed `PrepareCumulativePeriodSummary` use case against the Self Employment Business API v5 cumulative PUT descriptor.

The input must type and validate:

- safe source key;
- reporting-profile identity or an unambiguous TaxSourceCode selector;
- requested tax year;
- cumulative period start/end;
- and any explicitly reviewed filing-level choice which is not persistent master data.

The use case resolves and validates through the Part I statutory context:

- reporting subject;
- effective NINO;
- HMRC business ID;
- business type;
- accounting basis/type;
- TaxSourceCode association; and
- MIN consolidated or STD detailed population profile.

Create versioned, explicit Tag-key mapping profiles from `BusinessIncomeSource` to `CumulativeSubmission`. Each mapping records target member, income/expense orientation, rounding rule, aggregation rule, optionality/applicability and supported accounting basis/tax-year range.

Population must enforce:

- correct cumulative period and tax-year rules;
- accounting-basis compatibility;
- detailed/consolidated expense exclusivity;
- stable mapping of every supported key;
- duplicate and unsupported key rejection;
- correct sign handling without generic `Abs` conversion;
- required-zero completion only for applicable, supported concepts; and
- omission of genuinely absent optional concepts.

Add typed WebHarness routes:

```text
POST /harness/hmrc/mtd-income-tax/cumulative/prepare
GET  /harness/hmrc/mtd-income-tax/cumulative/{preparationId}
GET  /harness/hmrc/mtd-income-tax/cumulative/{preparationId}/body
```

### Acceptance criteria

- Representative MIN fixtures produce the approved consolidated-expense body.
- Representative STD fixtures produce the approved detailed-expense body.
- Both shapes cannot appear in one request.
- Unsupported or invalid mappings block bytes and expose stable findings.
- Each semantic key has a focused mapping test to its exact contract member.
- Tax year, dates, accounting basis, rounding, zero and omission rules have positive and negative tests.
- Raw response bytes and digest match the immutable prepared artifact.

---

## Phase 7 — Remaining Approved Body-Bearing Operations

Implement additional body-bearing operations in the order recorded in the coverage matrix. Likely families include annual submissions, business adjustments, losses/claims, tax-liability adjustments and business configuration, but none is automatically authorised by its presence in the contract assembly.

Each operation family is a separate vertical slice and requires:

- an approved Accounts Mode use case;
- authoritative ownership of every accounting or operator-supplied value;
- typed input and typed preparation use case;
- an explicit versioned population/configuration policy;
- contract, Application and WebHarness tests;
- positive and negative golden bodies; and
- an inspection/raw-body route where supported.

External or reviewed workflow values must remain typed operator/configuration input. Do not create Tax Tags or infer accounting facts merely to populate optional HMRC properties.

### Acceptance criteria

- Every Accounts Mode body operation is supported with evidence or explicitly deferred/unsupported.
- Every supported request has one canonical serialization path and golden body.
- Configuration-body operations do not pretend operator input came from accounting SQL.
- Unsupported tax years, profiles and optional concepts fail or omit according to documented policy.
- No generic reflection mapper or `Dictionary<string, object?>` dispatcher is introduced.

---

## Phase 8 — Bodyless Request Descriptions

Implement typed describe use cases for approved VAT and MTD Income Tax enquiry/command descriptors.

Prioritise operations needed to inspect the workflows surrounding the two accounting vertical slices, including VAT obligations and view-return requests, and MTD Income Tax income-and-expenditure obligations. Add other Accounts Mode enquiries only after their descriptor and identifier semantics are classified.

Each describe use case validates its typed identifiers and permitted query combinations, resolves the descriptor path, preserves query order and returns the required contract headers. It does not query HMRC, fabricate a response or create a JSON body.

Expose coherent typed routes such as:

```text
POST /harness/hmrc/vat/obligations/describe
POST /harness/hmrc/vat/returns/view/describe
POST /harness/hmrc/mtd-income-tax/obligations/describe
```

### Acceptance criteria

- Every in-scope bodyless descriptor has typed path/query validation.
- Descriptions agree with the authoritative descriptor metadata.
- Optional query parameters are omitted rather than emitted with empty values.
- Bodyless requests expose no body, `{}` placeholder or digest.
- Response contracts remain fixture/test data until Objective 4 performs real requests.

---

## Phase 9 — WebHarness Hardening and Legacy Separation

Create `TradeControl.Tax.UK.WebHarness.Tests` and harden the diagnostic host.

Implement:

- a bounded, expiring, thread-safe preparation store;
- opaque preparation IDs;
- safe inspection DTOs distinct from Application artifacts;
- direct byte response writing;
- exact Content-Type and preview headers;
- stable finding-to-HTTP mapping;
- correlation-safe problem details;
- Swagger documentation for typed request models; and
- redaction tests for connections, credentials and internal exceptions.

Move all new mapping and validation registrations out of WebHarness and into Application/adapter composition. Clearly segregate existing `PayloadHarnessEnvelope`, `VatHarnessPayload`, `MicroHarnessPayload`, builders, mappers, validators and `HmrcSubmissionRunner` as legacy Objective 2 diagnostics. Remove them only after an explicit parity/deprecation review; do not silently route them into the Objective 3 implementation.

Remove the WebHarness dependency on `Adapters.Submission` if it is used only by the legacy runner and can be retired safely. Otherwise prove through architecture and route tests that no Objective 3 preview resolves or invokes submission services.

### Acceptance criteria

- Inspection and raw endpoints retrieve the same stored immutable artifact.
- Store capacity, expiry and unknown-ID behaviour are tested.
- Raw bodies are not wrapped, reformatted or reserialized by MVC.
- Errors never return partial sendable bytes.
- Swagger exposes real typed parameters rather than loose dictionaries.
- Logs and responses contain no connection strings, credentials or unredacted SQL detail.
- Preview routes cannot invoke `Adapters.Submission` or an outbound network handler.

---

## Phase 10 — Architecture Tests and Objective 4 Handoff Proof

Add architecture tests which protect project references and prohibited namespace dependencies:

- contracts cannot reference Application, adapters, SQL or ASP.NET Core;
- Application cannot reference SQL or ASP.NET Core;
- WebHarness controllers cannot use SQL readers, `Tc*` rows or contract serializers directly;
- Objective 3 preview code cannot reference transport/authentication implementation; and
- adapter SQL knowledge does not leak into neutral models.

Exercise the complete offline pipeline against checked-in snapshots and the representative synthetic fixtures:

1. read one coherent accounting source;
2. populate the exact contract;
3. serialize once;
4. create the prepared artifact;
5. retrieve inspection metadata;
6. retrieve the raw body; and
7. pass the same artifact through a fake Objective 4 gateway.

The fake gateway asserts that method, relative path, ordered query, contract headers and exact body bytes arrive unchanged. It does not implement HTTP, authentication, environment routing, fraud-prevention headers, response handling or audit persistence.

### Acceptance criteria

- Architecture tests fail on prohibited reference or namespace drift.
- VAT and both cumulative profiles pass deterministic offline end-to-end tests.
- The fake gateway receives the exact previewed bytes without remapping or reserialization.
- The full `TaxHub.slnx` builds and every contract, Application, adapter and WebHarness test passes.
- Operation coverage documentation matches implemented route and test coverage.

---

## Objective 3 Completion Gate

Objective 3 is complete only when all of the following are true:

- the Part I statutory data requirement and readiness matrices are complete;
- reusable subject data has one authoritative source and is not duplicated in tax-specific storage;
- registrations, reporting profiles and settings are jurisdiction-neutral, constrained, effective-dated and provenance-bearing;
- the schema supplies every non-ledger input required by the supported operations without inference from database names or labels;
- sensitive tax identifiers are access-controlled and masked in diagnostics;
- every current VAT and MTD Income Tax endpoint descriptor and Corporation Tax/Companies House service package is classified;
- every Accounts Mode operation is explicitly supported, deferred or unsupported;
- the authority-neutral vocabulary contains no SQL or HMRC wire naming leakage;
- SQL-specific knowledge is confined to `Adapters.TradeControl`;
- identity and configuration values have explicit, non-guessed sources;
- UK population uses typed use cases, versioned profiles and exact contract types;
- blocking input, source, readiness, mapping, population, contract and wire findings prevent bytes;
- corporate fixtures produce reconciled statutory accounts, accounts/computation iXBRL, CT600 and deterministic HMRC/Companies House packages;
- VAT and MIN/STD cumulative fixtures produce approved deterministic JSON;
- every other supported body operation has a golden request fixture;
- raw preview output is byte-identical to its `PreparedApiRequest` body or `PreparedSubmissionPackage`/document bytes and digest;
- bodyless operations expose only method/path/query/header descriptions;
- source selection and previews do not disclose secrets or connection strings;
- preview routes have no path to live HMRC transport;
- all ordinary tests run offline without HMRC credentials or mutable developer databases; and
- the Objective 4 gateway port accepts the prepared request unchanged.

---

## Explicit Non-Goals

This work plan does not authorise:

- live HMRC requests or submissions;
- OAuth, token refresh, scope or fraud-prevention header implementation;
- HMRC sandbox enrolment or production environment configuration;
- persistence of submission history, prepared requests or authority responses;
- claims that a preview was submitted, accepted or approved;
- reconstruction or reserialization of request bodies in Objective 4;
- alteration of Tax Tag, category-tree, Cash Statement or accounting calculations without a separately reviewed `sqlnode` work package;
- replacement of authoritative SQL validation/reconciliation logic;
- a universal tax dictionary, reflection mapper or loose parameter dispatcher; or
- removal of legacy Objective 2 diagnostics before a separate retirement review.

---

## Recommended Delivery Sequence

Deliver in small reviewable increments with a working demonstration at each gate:

1. DP1–DP5 statutory data provision and verification for corporate, VAT and sole-trader profiles;
2. CO1 company projection inventory and neutral source boundary;
3. CO2 common prepared-artifact core;
4. CO3 statutory accounts and iXBRL;
5. CO4 Corporation Tax computation, CT600 and HMRC package;
6. CO5 Companies House accounts package;
7. CO6 corporate coverage and Objective 4 handoff gate;
8. API descriptor inventory and canonical JSON serializers;
9. neutral VAT/sole-trader accounting-source vocabulary and adapters;
10. prepared API request mechanics;
11. VAT exact-body vertical slice;
12. MIN and STD cumulative exact-body vertical slice;
13. remaining approved API operations;
14. approved bodyless descriptions;
15. WebHarness hardening and legacy separation; and
16. architecture enforcement and complete Objective 4 handoff proof.

Do not begin Part II population until the Part I data-provision gate proves that the required context is authoritative and complete. Do not begin broad operation coverage until the VAT and cumulative slices prove the complete source-to-byte design. Do not begin Objective 4 transport until the final handoff gate passes.
