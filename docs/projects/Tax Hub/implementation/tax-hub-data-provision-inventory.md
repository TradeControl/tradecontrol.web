# Tax Hub — DP1 Statutory Data Requirement Inventory

8 September 2026  
Status: DP1 evidence and design input; no schema implementation in this phase

## 1. Scope

This inventory classifies non-Category data required to populate or describe the four Objective 3 service families:

1. HMRC Corporation Tax;
2. Companies House accounts filing;
3. HMRC VAT; and
4. HMRC MTD Income Tax for sole traders.

It distinguishes reusable subject identity, authority registration, reporting profile, effective setting, accounting projection, reviewed filing input, authority response state and secret/transport configuration.

This document does not design the final tables. That is DP2. It records the ownership, cardinality, temporal and sensitivity requirements which DP2 must satisfy.

## 2. Schema ownership rule

Trade Control business objects must use the established domain schemas. `dbo` is reserved for the standard ASP.NET Core authentication framework and must not own Tax Hub tables, views, functions or procedures.

The preliminary ownership boundary for DP2 is:

| Concern | Owning schema |
|---|---|
| Node-wide business jurisdiction, authorities, controlled definitions and value-source catalogues | `App` |
| Legal/natural persons, registrations, addresses and reusable identity | `Subject` |
| Tax/reporting profiles, TaxSourceCode relationships, effective tax settings and statutory accounting projections | `Cash` |
| Identity/authentication framework tables only | `dbo` |

Final object names and placement require DP2 review against existing `sqlnode` conventions. No generic Tax Hub object may be added to `dbo`.

## 3. Existing authoritative data

### 3.1 Home subject

`App.tbOptions.SubjectCode` is the authoritative pointer to the home/reporting subject.

The home subject currently provides:

| Semantic | Source | Assessment |
|---|---|---|
| Subject identifier | `App.tbOptions.SubjectCode` | Existing and authoritative |
| Legal/display name | `Subject.tbSubject.SubjectName` | Existing; legal-name readiness must be validated |
| Selected administrative address | `Subject.tbSubject.AddressCode` -> `Subject.tbAddress` | Existing free-form source |
| Telephone/email | `Subject.tbSubject` | Existing; not required by the first request bodies |
| Company registration number | `Subject.tbVirtual.CompanyNumber` | Existing sole user-maintained source; no migration or duplication proposed |
| VAT registration number | `Subject.tbVirtual.VatNumber` | Existing authoritative source; currently absent in all four sandboxes |
| Business description | `Subject.tbVirtual.BusinessDescription` | Existing candidate for principal activity; requires review |
| Employee count | `Subject.tbVirtual.NumberOfEmployees` | Existing point-in-time value; not sufficient evidence of period-average employees |
| Website | `Subject.tbVirtual.WebSite` | Existing; not required by the first filing artifacts |

These values must be exposed through one canonical home-subject projection. They must not be duplicated in a statutory dictionary.

### 3.2 Jurisdiction and accounting source

`App.tbJurisdiction` is the jurisdiction catalogue. A Trade Control node represents one business in one jurisdiction, so the selected `JurisdictionCode` belongs in `App.tbOptions` as a required foreign key to `App.tbJurisdiction`.

`Cash.tbTaxTagSource` identifies a statutory/accounting source and links to `Cash.tbTaxType`. Its current `JurisdictionCode` repeats a node-wide invariant and permits contradictory source rows. DP2 should migrate that ownership to `App.tbOptions`, remove the repeated column/foreign key from `Cash.tbTaxTagSource`, and update the Tax Configurator hierarchy and queries to obtain the jurisdiction from `App.tbOptions`.

TaxSourceCode identifies a projection profile. It is not proof of legal form, authority registration, accounting basis or authority business identity.

The node-wide business/tax jurisdiction is not necessarily the same semantic as a company's jurisdiction of incorporation or Companies House registry jurisdiction. Where a filing contract requires the latter (for example `EnglandAndWales`), retain it as a specific company legal-registration/profile value rather than overloading the broader `App.tbOptions.JurisdictionCode`.

### 3.3 Periods

Tax reporting/accounting periods are authoritatively calculated from `Cash.tbTaxType` by `Cash.fnTaxTypeDueDates(@TaxTypeCode, @IsAccrual)`. The function applies the tax type's configured `MonthNumber`, `RecurrenceCode` and, for non-accrual output, `OffsetDays` over the calendar in `App.tbYearPeriod`.

The returned semantics are:

- `PayFrom` — inclusive start of the reporting interval;
- `PayTo` — exclusive end of the reporting interval; and
- `PayOn` — configured payment/due date, including `Cash.tbTaxType.OffsetDays` when `@IsAccrual = 0`.

Business-tax consumers use:

```sql
SELECT PayOn, PayFrom, PayTo
FROM Cash.fnTaxTypeDueDates(Cash.fnGetBizTaxType(), 0)
```

as demonstrated by `Cash.vwTaxBizStatement`. VAT projection uses `Cash.fnTaxTypeDueDates(1, 0)` in `Cash.vwTaxVatSubmission`.

`App.tbYear` and `App.tbYearPeriod` are therefore underlying calendar support, not the Tax Hub authority for calculating tax reporting windows. The calculated windows can supply accounts, VAT and cumulative-period candidates, but they do not by themselves establish:

- an HMRC tax year;
- an HMRC obligation period or VAT period key;
- a periods-of-account election;
- accounts approval;
- a Corporation Tax period where an accounts period must be split; or
- an authority-confirmed filing state.

## 4. Sandbox baseline

Read-only inspection used the `TCWeb` client secret `ConnectionStrings:TCNodeContext`; no secret value was printed or persisted.

| Sandbox | Intended profile | Home subject findings | Statutory sources |
|---|---|---|---|
| `tcNodeDb4-COMIPFVT1-COMIN26` | Company MIN, VAT registered | Name/address/company number present; VAT number and business description absent | `UK-CO-ACCTS-2026`, `UK-CO-CT-2026`, `UK-CO-CT600-2026` |
| `tcNodeDb4-COSIPFVT1-COSTD26` | Company STD, VAT registered | Name/address/company number present; VAT number and business description absent | same three company sources |
| `tcNodeDb4-STMIPFVT1-STMIN26` | Sole trader MIN, VAT registered | Name/address/company number present; VAT number and business description absent | `UK-ITSA-SE-CUM` |
| `tcNodeDb4-STSIPFVT1-STSTD26` | Sole trader STD, VAT registered | Name/address/company number present; VAT number and business description absent | `UK-ITSA-SE-CUM` |

All four currently use `SubjectTypeCode = 4` (`Company`), including the sole-trader databases. All contain the same underlying `App.tbYearPeriod` calendar of 48 monthly periods from April 2024 through March 2028; applicable reporting windows must be obtained through the relevant `Cash.fnTaxTypeDueDates(...)` calculation. Their domain objects are correctly distributed across `App`, `Cash`, `Invoice`, `Object`, `Project`, `Subject`, `Usr` and `Web`; existing `dbo` objects are authentication support.

Consequences:

- legal form must not be inferred from current `SubjectTypeCode` fixture data;
- company-number presence must not be used to infer corporation eligibility;
- VAT readiness currently fails for all four fixtures;
- the fixture provisioning phase must add synthetic registrations/profiles and correct legal-form evidence; and
- database names remain test catalogue identifiers, never statutory data sources.

## 5. Cross-family identity and context requirements

| Semantic | Cardinality/effective scope | Proposed owner | Current state | Sensitivity | Decision |
|---|---|---|---|---|---|
| Reporting subject | One home subject per node | `App.tbOptions` / `Subject` | Existing | Internal | Reuse |
| Legal/display name | Subject, effective through subject audit history | `Subject` | Existing | Low | Reuse; validate nonblank |
| Legal form/person kind | Subject, effective-dated if change is permitted | `Subject` | Ambiguous/inaccurate in fixtures | Low | New explicit legal-form concept; do not infer from TaxSourceCode |
| Business/tax jurisdiction | One per Trade Control node | `App.tbOptions` -> `App.tbJurisdiction` | Selection missing from options | Low | Move ownership from each Tax Tag source to node options |
| Jurisdiction of incorporation/registry | Company registration/profile | `Subject` plus `App` definition | Missing | Low | Keep distinct from the node-wide business/tax jurisdiction |
| Administrative/registered address | Subject/address, potentially effective-dated | `Subject` | Free-form only | Personal for sole traders | Reuse source; add structured satellite if contract requires components |
| Authority | Jurisdiction catalogue | `App` | Missing as distinct concept | Low | New authority catalogue; seed HMRC and Companies House |
| Registration scheme | Authority-controlled definition | `App` | Missing | Varies | New controlled definition |
| Registration value | Subject + scheme + validity | `Subject` | NINO/UTR missing; VAT/company number existing elsewhere | High for NINO/UTR | New registration store/resolver without duplicating existing values |
| Reporting profile | Subject + authority + profile + validity | `Cash` | Missing | Internal | New; may link TaxSourceCode |
| Authority business reference | Reporting profile + validity | `Cash` | Missing | Personal/tax | New; HMRC business ID belongs here, not at subject level |
| Value provenance | Every configured value | `App` definition plus owning record | Missing | Internal | Record local/operator/imported/authority-confirmed/fixture |
| Review/readiness state | Registration/profile/setting | Owning domain | Missing | Internal | New typed state; absence/expired/unreviewed block where required |

## 6. Corporation Tax and statutory accounts

### 6.1 Identity and reporting context

| Semantic | Scope | Source/owner | State and rule |
|---|---|---|---|
| Company legal name | Subject | `Subject.tbSubject.SubjectName` | Existing; validate as legal filing name |
| Company registration number | Existing user-maintained subject field | `Subject.tbVirtual.CompanyNumber` | Existing in company fixtures; preserve the entered value and leading zeroes; Companies House is the final authority on acceptability |
| UTR | Subject registration | New `Subject` registration | Missing; sensitive; masked in diagnostics |
| Company jurisdiction | Subject/company profile | New explicit value | Missing; current contract default must not silently supply production data |
| Registered office | Subject address | Selected/typed `Subject` address | Free-form; structure decision required in DP2 |
| Principal activity | Reporting period/profile | `BusinessDescription` candidate or reviewed filing narrative | Empty in fixtures; requires stewardship decision |
| Reporting currency | Reporting profile/period | Effective setting | Missing; GBP may be seeded for UK profile but must be explicit |
| Accounts period | Reporting period | `Cash.fnTaxTypeDueDates(Cash.fnGetBizTaxType(), 0)` window plus reviewed filing selection | Calculated candidates exist; selection/approval missing |
| Comparative period | Reporting period | Prior approved period | Derivable candidate; applicability must be explicit |
| Reporting framework | Reporting profile + period | Effective controlled setting | Missing; initial supported value FRS 105 |
| Accounts type | Reporting profile + period | Effective controlled setting | Missing; initial supported value micro-entity |
| Audit status/exemption | Filing period | Reviewed filing setting/declaration | Missing; not permanent subject identity |
| Members did not require audit | Filing period declaration | Reviewed workflow input | Missing |
| Directors acknowledge responsibilities | Filing period declaration | Reviewed workflow input | Missing |
| Accounts approval date | Filing period event | Workflow/approval record | Missing; must not be generic setting |
| Signing director | Filing period event + person identity | Workflow/approval record | Missing; free text may satisfy contract initially but needs accountable source |

### 6.2 Notes and non-ledger disclosures

| Semantic | Classification | State and rule |
|---|---|---|
| Accounting policies | Versioned profile narrative plus reviewed override | Missing; not a Category fact |
| Average employees | Period-derived/reviewed disclosure | `Subject.tbVirtual.NumberOfEmployees` is point-in-time only; insufficient without calculation policy |
| Director advances | Repeating structured filing evidence | Missing; cannot be a scalar dictionary setting |
| Commitments and contingencies | Repeating structured filing evidence | Missing; absence must be explicit/reviewed |
| Prepayments/accrued income | Accounting/period-end adjustment evidence | Manifest marks derived/external; source must be confirmed in CO1 |
| Provisions | Accounting/period-end adjustment evidence | Source gap to confirm in CO1 |
| Accruals/deferred income | Accounting/period-end adjustment evidence | Source gap to confirm in CO1 |

### 6.3 Corporation Tax computation and CT600 context

| Semantic | Classification | State and rule |
|---|---|---|
| Corporation Tax period(s) | Derived from approved accounts period | Split policy exists in contract code; must retain derivation evidence |
| Other add-backs/deductions | Repeating reviewed computation input | Missing outside mapped depreciation |
| Capital allowances | Structured computation input/asset workflow | Missing; never substitute accounting depreciation |
| Loss relief schedule | Structured authority/reviewed state | Existing accounting loss views may contribute; exact source incomplete |
| Statutory tax rate | Effective jurisdiction rule | Missing from DP1 provision; must be version/effective dated |
| Reliefs | Structured reviewed claim | Missing |
| CT600A loans to participators | Conditional structured filing input | Missing; explicitly not applicable or supplied |
| Accounts/computations attached | Package-derived declarations | Derived from prepared package, not stored master data |
| Declarant name/date | Filing declaration | Workflow input, not registration/profile setting |
| Supporting attachments | Filing workflow artifacts | Optional structured package input |
| HMRC correlation/submission state | Authority response | Objective 4 only |

### 6.4 Companies House context

| Semantic | Classification | State and rule |
|---|---|---|
| Delivery profile (full/filleted) | Filing choice constrained by accounts profile | Missing workflow input |
| Registrar statements | Filing declarations | Derived/confirmed from approved accounts declarations |
| Envelope number | Submission correlation | Diagnostic value in preview; durable authority lifecycle belongs to Objective 4 |
| Presenter/company authentication | Secret | Objective 4 secret boundary; never DP1 data |
| Submission number/status/errors | Authority response | Objective 4 only |

## 7. VAT

| Semantic | Scope | Source/owner | State and rule |
|---|---|---|---|
| VRN | Existing user-maintained subject field | `Subject.tbVirtual.VatNumber` | Absent in all sandboxes; preserve the entered value; HMRC is the final authority on acceptability |
| VAT registration validity/status | Authority state | Objective 4 response/authority context | Do not infer legal validity from local syntax checks |
| VAT accounting period | Accounting projection | `Cash.fnTaxTypeDueDates(1, 0)` as consumed by `Cash.vwTaxVatSubmission` | Existing; preserve inclusive `PayFrom`/exclusive `PayTo` semantics |
| VAT period key | Obligation/authority state | Workflow input for preview; Objective 4 response state later | Missing; never inferred from local period description |
| Finalised declaration | Individual return preparation | Explicit reviewed workflow input | Missing; never default silently |
| Obligation dates/status filters | Request query | Typed operator/workflow input | Not master data |
| Liability/payment date filters | Request query | Typed operator input | Not master data |
| Penalty charge reference | Authority response/user-selected authority record | Objective 4 response state | Not local master data |
| Nine VAT amounts | Accounting projection | `Cash.vwTaxVatSubmission` | Existing Category/derived facts; outside DP1 storage |

The existing VAT number column remains authoritative. It is intentionally a free user-maintained field because the legal obligation rests with the business and HMRC ultimately determines whether a submission identifier is acceptable. DP2 must not duplicate it in the registration store. Preparation may require a nonblank, safely representable path value and may report obvious diagnostic warnings, but it must not claim that local format validation proves a valid VAT registration.

## 8. Sole-trader MTD Income Tax

### 8.1 Reusable identity/profile

| Semantic | Scope | Source/owner | State and rule |
|---|---|---|---|
| NINO | Natural-person registration | New `Subject` registration | Missing; high sensitivity and masked diagnostics |
| HMRC business ID | Subject + business/reporting profile | New `Cash` reporting profile | Missing; multiple businesses per person supported |
| Business type | Reporting profile | Controlled effective profile value | Missing; must not derive from TaxSourceCode label |
| TaxSourceCode association | Reporting profile | FK to `Cash.tbTaxTagSource` | Missing relationship; sole fixtures contain `UK-ITSA-SE-CUM` |
| Accounting type/basis | Business profile + tax year | Effective setting, with provenance | Missing |
| Quarterly period type | Business profile + tax year | Effective setting/authority-confirmed state | Missing |
| Periods-of-account choice/dates | Business profile + tax year | Effective setting plus reviewed dates | Missing; local accounting calendar is only a candidate source |
| Late-accounting-date-rule election | Business profile + tax year | Authority-confirmed/workflow state | Missing |
| Tax year | Operation/reporting period | Typed selection derived/validated against dates | Not permanent master data |
| MIN consolidated/STD detailed profile | Reporting profile/population policy | Explicit link to supported TaxSource/profile | Must not derive from database name |

### 8.2 Filing and authority-state values

| Semantic family | Classification | Storage decision |
|---|---|---|
| Cumulative period start/end | Reporting operation context | Supplied/validated against obligation and accounting period |
| Annual adjustments and allowances | Reviewed filing-period input/accounting evidence | Structured workflow data, not generic settings |
| Structured-building allowance identity | Repeating structured filing evidence | Dedicated workflow/evidence model if supported |
| Class 4 exemption reason | Filing/tax-year reviewed input | Effective filing value, not permanent identity |
| Business adjustment/BSAS choices | Authority workflow plus reviewed adjustment | Not reusable master data |
| Loss and claim values | Accounting/reviewed filing input | Structured filing model if supported |
| Loss ID, claim ID, calculation ID | Authority response identifiers | Objective 4 response/audit state |
| Calculation type | Command input | Typed operation input |
| Final declaration/amendment confirmation | Irreversible command intent | Explicit workflow confirmation; never a stored default |
| Obligation/account filters and statuses | Query input | Typed request input, not master data |
| Account document/payment references | Authority response/query context | Objective 4 state or typed operator input |
| Cumulative income/expenses | Accounting projection | `Cash.fnTaxBizCumulative(...)`; outside DP1 storage |

## 9. Provenance, sensitivity and temporal rules

Every new registration/profile/setting value must record:

- source classification: local configuration, operator supplied, imported, authority confirmed or synthetic fixture;
- effective start and end where the semantic can change;
- inserted/updated audit fields and row version;
- review/readiness state where human confirmation is required; and
- sensitivity/masking policy.

NINO and UTR must be access-controlled and masked outside the narrow preparation boundary. VRN and company number may be less sensitive but must still not be emitted incidentally. Credentials, OAuth tokens, fraud-prevention values and Companies House presenter secrets are never stored in the statutory data dictionary.

Authority-confirmed state must not be implied by a locally configured value. A preview may use a synthetic or operator-supplied value only when its provenance is visible and the operation policy permits it.

## 10. DP1 decisions

1. `Subject.tbVirtual.VatNumber` remains the sole user-maintained VRN source and is not copied into a registration table. Local checks do not establish legal validity.
2. `Subject.tbVirtual.CompanyNumber` remains the sole user-maintained company-number source under the same rule.
3. NINO and UTR require a new jurisdiction/authority-neutral subject-registration model in `Subject`.
4. HMRC business ID belongs to an effective reporting profile associated with subject and TaxSourceCode in `Cash`, not to the subject globally.
5. Accounting type, quarterly type and similar values belong to effective reporting-profile settings, with configured/operator/authority-confirmed provenance.
6. Reporting-period candidates come from `Cash.tbTaxType` through `Cash.fnTaxTypeDueDates(...)`; periods-of-account dates still require distinct reviewed/authority context and must not be derived directly from `App.tbYearPeriod`.
7. The current free-form address is authoritative human-readable content but is insufficient wherever a contract needs address components; DP2 should design a reusable `Subject` structured-address satellite rather than parse during population.
8. Principal activity may be sourced from `BusinessDescription` only after review and fixture provisioning; it is not currently ready.
9. Company framework/type are effective reporting-profile values; audit statements, approval, signing and declarations are filing-period workflow data.
10. Corporation Tax periods are derived from the approved accounts period and retained with derivation provenance.
11. Repeating disclosures, adjustments, allowances, reliefs and claims require typed workflow/evidence structures, not scalar dictionary entries.
12. `App.tbOptions` owns the node's single business/tax `JurisdictionCode`; `Cash.tbTaxTagSource` must inherit it and must not retain a duplicate jurisdiction column.
13. A company's incorporation/registry jurisdiction remains a distinct legal semantic where the filing contract requires it.
14. New Tax Hub SQL objects must target `App`, `Subject` or `Cash` according to domain ownership. `dbo` is prohibited except for ASP.NET Core authentication support.
15. Bootstrap selects the node jurisdiction before currency. `App.tbJurisdiction.UocCode` supplies the jurisdiction's default Unit of Charge and populates `App.tbOptions.UnitOfCharge`.
16. A permitted currency override during bootstrap changes only `App.tbOptions.UnitOfCharge`; it does not change or redefine `App.tbOptions.JurisdictionCode`. After accounting data exists, changing the node Unit of Charge is a separately controlled accounting migration, not an ordinary settings edit.
17. DP2 need not introduce a jurisdiction/currency bridge merely to anticipate multiple reporting currencies. If alternative currencies need controlled jurisdiction-specific eligibility later, add that relationship from evidence rather than treating currency as the jurisdiction key.

## 11. Gaps blocking DP2/first population

### Required foundation gaps

- authority catalogue;
- a coordinated `App.tbOptions.JurisdictionCode` migration and later removal of the repeated `Cash.tbTaxTagSource.JurisdictionCode`, including bootstrap, templates, EF models and Tax Configurator;
- registration-scheme definitions and subject registrations;
- explicit legal form and jurisdiction evidence;
- reporting profiles linked deliberately to TaxSourceCode;
- effective typed settings with provenance/readiness;
- canonical home-subject/statutory-context projections; and
- structured-address decision/design.

### Sandbox provisioning gaps

- correct corporation versus sole-trader legal-form evidence;
- synthetic VRNs in all VAT-enabled fixtures;
- synthetic NINOs and HMRC business IDs in sole-trader fixtures;
- synthetic UTRs in company fixtures;
- corporate principal activity, profile and filing-context test values;
- sole-trader accounting type and profile selection; and
- explicit synthetic provenance on all fixture-only identifiers.

### Company source gaps deferred to CO1 but identified by DP1

- authoritative period-average employee calculation;
- statutory narrative/disclosure workflow;
- director advances and commitments/contingencies;
- period-end balance-sheet adjustments;
- non-depreciation tax adjustments;
- capital allowances, reliefs and complete loss state; and
- CT600A applicability/evidence.

These gaps do not justify storing accounting facts in the statutory data dictionary.

## 12. DP1 acceptance assessment

| Criterion | Result |
|---|---|
| Every known non-Category request value classified | Met for the implemented four-family contract surface; later unsupported descriptors remain subject to their operation coverage matrices |
| Existing subject data reused | Met; canonical projection/resolver required in DP2 |
| Missing and ambiguous values explicit | Met |
| Cardinality and effective dating known | Met at logical level; physical constraints belong to DP2 |
| Workflow values and secrets separated from master data | Met |
| Jurisdiction-neutral inventory with UK seed requirements | Met |
| Schema ownership rule recorded | Met; no Tax Hub objects in `dbo` |

DP1 is complete as an inventory. DP2 may begin with the gaps and decisions above as its design inputs. No table, view or stored procedure was created or changed during DP1.
