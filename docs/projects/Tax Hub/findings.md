# Corporation Tax / Limited Company — Phase 1A Current SQL State Findings

Date: 1 September 2026

Scope: static repository reconnaissance starting at `App.proc_Template_CO_MICRO_CUR_2026`

Authority: this is an inventory of existing code, not confirmation that any source, tag, mapping, filing or endpoint matches a current HMRC or Companies House contract.

## 1. Executive finding

The current Limited Company bootstrap is an old micro-company accounts design coupled to the generic business-tax extraction path. It creates one source, `UK-MTD`, with ten `AC`/`CP` tags, maps profit-and-loss and depreciation accounting data into those tags, and exposes them through a development-only `SubmitMicro` harness payload. It does not contain a Corporation Tax return model, computation model, CT600 model, Companies House accounts model, iXBRL taxonomy/instance builder, or authoritative submission endpoint.

The bootstrap is not compatible with the current Tax Tag schema. `Cash.tbTaxTag.CashPolarityCode` is mandatory, but `proc_Template_CO_MICRO_CUR_2026` does not provide it in any of its ten tag inserts. If that is corrected mechanically, a second current-architecture conflict remains: `AC435` is declared Rollup (`TagClassCode = 0`) but has an accounting mapping, while the generic validator permits mappings only for Component tags. The selectable MIN and STD wrappers both invoke that validator.

No source in the repository distinguishes Companies House statutory accounts from an HMRC Corporation Tax submission. The single source is named “MTD”, assigned `TaxTypeCode = 0` (Corporation Tax), and mixes apparent accounts concepts (`AC...`) with apparent computation concepts (`CP...`). Its statutory meaning cannot be established from repository evidence.

## 2. Bootstrap entry points and complete procedure call graph

`App.proc_NodeDataInit` registers two selectable company templates:

| Template code | Display name | Procedure |
|---|---|---|
| `COMIN26` | Minimal Micro Company Accounts 2026 | `App.proc_Template_CO_MICRO_CUR_MIN_2026` |
| `COSTD26` | Standard Micro Company Accounts 2026 | `App.proc_Template_CO_MICRO_CUR_STD_2026` |

`App.proc_Template_CO_MICRO_CUR_2026` itself is included in `tcNodeDb4.sqlproj` but is not registered as a selectable template. It is the common company core called by both wrappers.

### 2.1 MIN path

1. `proc_Template_CO_MICRO_CUR_MIN_2026`
2. `proc_Template_CO_MICRO_CUR_2026`
3. `proc_Template_BASE_MIN_2026`
4. Base helper calls:
   - `Subject.proc_DefaultSubjectCode` for government, bank/miner, current, reserve and dummy identities; this calls `App.proc_DefaultCodeGenerator`.
   - `Subject.proc_AddAddress` for a fiat bank; this calls `Subject.proc_NextAddressCode`.
   - `Subject.proc_DefaultAccountCode` for long-term liabilities, called-up share capital, equipment and equipment adjustments; this also calls `App.proc_DefaultCodeGenerator`.
5. MIN optionally calls `App.proc_Template_DisableVAT` when `@IsVATRegistered = 0`.
6. MIN adds `CP28 -> CC-DEPRC` and calls `Cash.proc_TaxTagMapValidate('UK-MTD')`; the procedure reads `Cash.fnTaxTagMapValidate`, logs warnings through `App.proc_EventLog`, and raises an error when validation errors exist.
7. Every TRY/CATCH procedure in this path calls `App.proc_ErrorLog` on failure. `proc_ErrorLog` rolls back an active transaction, logs through `App.proc_EventLog`, and reraises.

### 2.2 STD path

1. `proc_Template_CO_MICRO_CUR_STD_2026`
2. `proc_Template_CO_MICRO_CUR_2026`
3. `proc_Template_BASE_MIN_2026`, with the same helper calls described above.
4. STD calls `Subject.proc_DefaultAccountCode` three more times for plant/tools, motor vehicles and fixtures.
5. STD calls `App.proc_Template_CO_MICRO_CUR_STD_EXP_2026` to add reporting expressions.
6. STD optionally calls `App.proc_Template_DisableVAT` when `@IsVATRegistered = 0`.
7. STD calls `Cash.proc_TaxTagMapValidate('UK-MTD')` and uses the same error-log path.

The MIN and STD wrappers start named transactions before calling the core and commit after validation. The core and base do not start their own transaction. Direct execution of the unregistered core therefore has no encompassing bootstrap transaction and performs no map validation. The expression helper has no TRY/CATCH of its own and relies on the STD caller.

## 3. Corporation Tax / Companies House sources

The complete company-bootstrap source inventory is one row:

| Source | Jurisdiction | Name | Description | Tax type |
|---|---|---|---|---|
| `UK-MTD` | `UK` | `MTD` | `UK Making Tax Digital (template defaults)` | `0` — Corporation Tax |

No separate Companies House, statutory-accounts, Corporation Tax return, computation or CT600 source is created. Repository-wide searches found no other company procedure inserting these ten tags or another Corporation Tax/Companies House Tax Tag source.

`Cash.tbTaxType` is seeded by `proc_NodeDataInit` with Corporation Tax as code `0`, annual recurrence code `4`, month `1`, and offset `275`. The base assigns the government subject, `CC-BIZTX`, and the selected financial month to tax types `0`, `4` and `5`. The company core enables code `0` and disables Sole Trader Tax (`4`) and Quarterly Return (`5`). VAT remains controlled separately by the wrapper option.

## 4. Tax Tags, classes and polarity metadata

Tag classes are globally seeded as `0 Rollup`, `1 Component`, and `2 Derived`. The company core attempts to insert:

| Order | Tag | Name | Class | Seeded polarity |
|---:|---|---|---|---|
| 10 | `AC12` | Turnover | Component | omitted |
| 20 | `AC405` | Other Income | Component | omitted |
| 30 | `AC410` | Cost of Sales | Component | omitted |
| 40 | `AC415` | Staff Costs | Component | omitted |
| 50 | `AC420` | Depreciation Total | Component | omitted |
| 55 | `CP28` | Depreciation charge | Component | omitted |
| 56 | `CP46` | Depreciation adjustment | Component | omitted |
| 60 | `AC425` | Other Charges | Component | omitted |
| 70 | `AC34` | Tax On Profit | Component | omitted |
| 80 | `AC435` | Profit and Loss | Rollup | omitted |

`CashPolarityCode` is now `NOT NULL`, has a foreign key to `Cash.tbPolarity`, and is constrained to Expense (`0`) or Income (`1`). The insert column list predates that field and has no default, so all ten inserts are structurally invalid against the live schema. Consequently the current selectable company templates cannot complete on a database built from the present project.

The repository provides no tag descriptions, required/optional metadata, data types, units, period/context rules, taxonomy version, namespace, calculation relationships, presentation relationships or filing-target metadata for these tags.

## 5. Existing mappings and accounting roots

All rows below are enabled. Category mappings recursively expand through `Cash.tbCategoryTotal` to enabled nominal categories and their enabled cash codes; direct mappings name one cash code.

### 5.1 Core mappings inherited by MIN and STD

| Tag | Mapping root | Type | Effective base accounting root/contributors |
|---|---|---|---|
| `AC12` | `CT-TURNOV` | Category | `CA-SALES`; `CC-SALES` (plus STD sales split codes) |
| `AC405` | `CT-OTHRIN` | Category | `CA-INCOME`; `CC-INCME` |
| `AC410` | `CT-CSTSAL` | Category | `CA-DIRECT`; `CC-DIRCT` and conditional `CC-MINER` (plus STD direct-cost splits) |
| `AC415` | `CT-STAFFC` | Category | `CA-WAGES`; `CC-WAGES`, `CC-PENSN`, `CC-EMPNI` (plus STD `CC-SALRY`) |
| `AC425` | `CT-OVERHD` | Category | `CA-ADMIN` and, in STD, `CA-BUILD`; their enabled cash codes |
| `AC34` | `CA-TAXCO` | Category | `CC-BIZTX` |
| `AC435` | `CT-PANDL` | Category | the whole configured P&L tree: gross profit, staff, overhead and `CA-ASSET` branches |
| `AC420` | `CC-DEPRC` | Cash code | base depreciation |
| `AC420` | `CC-DEPRJ` | Cash code | depreciation adjustment |
| `CP46` | `CC-DEPRJ` | Cash code | depreciation adjustment |

There is a commented-out alternative `AC420 -> CA-ASSET` category mapping. It is not live.

### 5.2 MIN delta

MIN adds `CP28 -> CC-DEPRC`. It also opens all accounts that the base created as closed, including `EQUIPMENT` and `EQUIPMENT ADJUSTMENTS`.

### 5.3 STD delta

STD deletes the base `EQUIPMENT` account using `CC-DEPRC`, creates three replacement depreciation cash codes/accounts (`CC-DEPPL`, `CC-DEPMV`, `CC-DEPFX`), maps all three to `CP28`, then deletes every mapping whose cash code is `CC-DEPRC` and deletes that cash code. The inherited `AC420 -> CC-DEPRJ` and `CP46 -> CC-DEPRJ` rows remain.

Thus MIN reports `CC-DEPRC` under both `AC420` and `CP28`, and `CC-DEPRJ` under both `AC420` and `CP46`. STD reports its three replacement depreciation codes under `CP28`, while `AC420` contains only `CC-DEPRJ`; `CC-DEPRJ` is also reported under `CP46`. The generic validator deliberately detects repeated routes within one tag but does not prohibit the same cash code appearing in different tags.

## 6. SQL extraction and readers

The legacy/discrete extraction chain is:

`Cash.tbTaxTagMap` -> `Cash.fnCategoryCashCodes` -> `Cash.vwTagCashPeriodMap` -> `Cash.vwCashCodePeriodValues` -> `Cash.vwTaxBizPayload` -> `Cash.vwTaxBizSubmission` -> C# `TcBusinessTaxReader.ReadAsync`.

- `vwTagCashPeriodMap` expands mappings and attaches periods from `Cash.fnTaxTypeDueDates(source.TaxTypeCode, 0)`.
- `vwTaxBizPayload` joins mapped cash codes to accounting-period invoice values inside those windows.
- `vwTaxBizSubmission` groups values by source, tag and window.
- `TcBusinessTaxReader.ReadAsync` selects a requested `PeriodTo` and converts every result to `Math.Abs`, discarding accounting sign.

Supporting/audit consumers are `Cash.vwTaxBizPayloadAudit` and the EF models `Cash_vwTaxBizPayload`, `Cash_vwTaxBizPayloadAudit` and `Cash_vwTaxBizSubmission` in `TCWeb`. `vwTaxBizPayloadAudit` builds its “raw” side by joining each tag window to every cash code in the period, rather than only the tag's mapped cash codes; its diagnostic meaning is therefore structurally suspect.

The newer generic cumulative path is:

`Cash.tbTaxTagMap` -> `Cash.vwTaxTagCashCode` -> `Cash.fnTaxBizCumulative` -> `TcBusinessTaxReader.ReadCumulativeAsync`.

It validates tag class and polarity, expands effective leaves, and returns orientation-aware values. No company-specific caller of `ReadCumulativeAsync` was found. Its date rules are hard-coded to a tax year beginning 6 April and require the requested end to precede a stored accounting period boundary; those rules came from the Sole Trader cumulative work and are not evidence of a Corporation Tax or Companies House period contract.

The generic Tax Configurator in `TCWeb` reads and edits sources, tags, classes and mappings without company-specific semantics. The Tax Hub business-tax workspace reads the generic business-tax views; no company accounts or Corporation Tax contract layer was found there.

## 7. C# references and apparent submission surface

The only exact C# consumer of the ten-tag vocabulary is `MicroHarnessPayloadBuilder`. It contains the same ten hard-coded tag codes, calls `TcBusinessTaxReader.ReadAsync`, fills missing expected tags through the generic `TagMapper`, and builds `MicroHarnessPayload` with:

- `PayloadVersion = "2026.1"`;
- source, period and subject identifiers;
- a generic list of tag/value items; and
- metadata operation `SUBMIT_MICRO`.

`HMRC.WebHarness` exposes `POST /harness/mtd/micro`. `MicroTestController` sends operation `SubmitMicro` to `HmrcSubmissionRunner`; the runner validates database availability, builds the harness payload, returns/logs an internal success result, and does not transmit a statutory company filing. This is a development harness contract, not an HMRC or Companies House endpoint contract.

No exact references to the ten company tag codes were found elsewhere in current SQL/C# beyond the company seed and `MicroHarnessPayloadBuilder`. The sample `.http` request names `UK-MTD`. No C# Corporation Tax payload model, CT600 model, company-accounts payload model, iXBRL instance builder, Companies House/HMRC company endpoint class, URI, OAuth scope or transport registration was found.

The apparent historical intention is therefore limited to: generate micro-company P&L/depreciation figures; group them into accounts/computation-looking codes; window them using the Corporation Tax schedule; and observe a generic “SubmitMicro” harness payload. The repository alone does not establish whether this was intended for Companies House accounts, HMRC computations, an HMRC Company Tax Return, a joint filing flow, or an internal prototype combining them.

## 8. Obsolete, duplicate, inconsistent or structurally suspicious material

1. **Non-compiling tag seed:** all tag inserts omit mandatory `CashPolarityCode`.
2. **Mapped Rollup:** `AC435` is Rollup but mapped to `CT-PANDL`; the current generic validator rejects any non-Component mapping.
3. **Mixed statutory domains in one source:** `AC` and `CP` codes share `UK-MTD`, one period schedule and one payload despite names suggesting accounts and computation concepts.
4. **Ambiguous source identity:** `UK-MTD`/“MTD” is generic and does not identify Corporation Tax, Companies House, taxonomy, filing type or version.
5. **No source separation:** no independent accounts/computation/return sources, contexts or validation rules exist.
6. **Overlapping depreciation evidence:** the same cash codes intentionally contribute to multiple tags; whether these are complementary schedules or double counting is not encoded.
7. **Profit-and-loss overlap:** `AC435 -> CT-PANDL` includes cash codes already reported under `AC12`, `AC405`, `AC410`, `AC415`, `AC425` and depreciation mappings. The architecture has no calculation-link semantics to distinguish a statutory total from a mappable component.
8. **Tax-on-profit circularity risk:** `AC34` reads `CC-BIZTX`, the same cash code configured for Corporation Tax liabilities/payments, rather than a separately evidenced current-tax charge or computation result.
9. **Legacy sign handling:** `ReadAsync` applies absolute value to all tags, so losses, credits, reversals and polarity distinctions cannot survive this path.
10. **Period-model conflation:** the source's Corporation Tax due-date recurrence supplies extraction windows for every tag, including apparent accounts tags. Companies House accounts periods and HMRC computation/return periods are not represented separately.
11. **Hard-coded Sole Trader cumulative dates:** the newer generic cumulative function assumes 6 April, so it is not presently a generic company-period replacement despite its generic name.
12. **Validator coverage:** it verifies generic mapping integrity and polarity, not tag manifest completeness, calculation relationships, statutory contexts, taxonomy validity or filing readiness.
13. **Direct core execution:** the core can be executed without the wrapper transaction or validator, although it is not a registered UI template.
14. **Non-idempotent seeding:** source, tag and map inserts are unconditional. Re-execution against populated data fails on keys.
15. **Stale VAT disable identifiers:** `proc_Template_DisableVAT` attempts to disable categories `TC-VAT`/`TC-TAXGD` and cash codes `TC600`/`TC501`/`TC602`, while the current base creates `CT-VAT`/`CA-TAXGD` and `CC-VAT`; those targeted updates are no-ops. It still zeroes VAT-bearing tax codes and disables Tax Type 1.
16. **Expression/name drift:** MIN's cash-operating-surplus expression refers to `Direct Purchases`, absent from the base names. Several STD expressions refer to names such as `Wages & Salaries`, `Employer Pension` and `Depreciation` that do not match the descriptions seeded by the current base/STD procedures.
17. **Questionable template description:** `COMIN26` is described as suitable for dormant companies although the bootstrap creates and enables trading/VAT structures; no dormant-company filing state is modeled.
18. **Audit-view mismatch:** `vwTaxBizPayloadAudit` compares mapped payload values with raw totals assembled from all period cash codes for every tag window.
19. **Historical duplication:** the older `tcNode` generated creation script and archived V3 upgrades contain the pre-Tax-Tag Corporation Tax forecasting/statement engine. They are historical copies, not additional company Tax Tag sources, but searches can conflate them with current `tcNodeDb4`.

## 9. Generic Tax Tag architecture conflicts

The current architecture gives a Tax Tag an independently required Income/Expense polarity, permits mappings only for Component tags, expands accounting mappings to effective nominal leaves, and validates each leaf against the tag orientation. The company seed predates all three assumptions: it supplies no polarity; maps a Rollup; and uses total-like tags (`AC435`, apparently also `AC420`) as if they were ordinary additive components.

The architecture stores only a flat tag list plus accounting mappings. It has no generic representation for taxonomy relationships, contexts, dimensions, units, non-monetary fact types, nil/omission rules, computations, attachments, declarations, identifiers or filing packages. Those omissions may be acceptable for an internal numeric projection, but repository evidence does not show that the present structure can represent statutory accounts or a Corporation Tax return.

## Questions requiring authoritative contract verification

1. Which current Companies House accounts filing regime(s) and HMRC Corporation Tax filing regime(s) are in scope, and for which accounting periods and entity eligibility rules?
2. Are micro-entity accounts, filleted accounts, full accounts, dormant accounts, Corporation Tax computations and the Company Tax Return separate submissions or parts of one filing package in the intended product flow?
3. What are the authoritative current schemas/taxonomies, versions, namespaces and validation rules for each filing target?
4. Do the ten seeded `AC`/`CP` codes belong to a current contract? If so, which taxonomy/version, and what are their exact labels, types, periods, signs and required/optional rules?
5. What do the `AC` and `CP` prefixes mean, and may those facts legitimately share a source and payload?
6. Should accounts facts, computation facts and Corporation Tax return fields use distinct `TaxSourceCode` values, manifests, period contexts and mappings?
7. Which facts are components, calculated totals/rollups or derived computation results? In particular, may `AC435` and `AC420` be directly mapped under the current architecture?
8. What statutory sign convention applies to income, expenses, depreciation, tax charges, profits and losses, including reversals and negative values?
9. Are the overlapping `AC420`/`CP28`/`CP46` depreciation mappings intentional, and what reconciliation/calculation relationships must hold among them?
10. Is `AC34` intended to represent the accounts tax charge, the computed Corporation Tax liability, tax paid, or another concept? What accounting evidence should supply it?
11. What is the authoritative accounting-period and submission-window model for Companies House and HMRC, including short/long periods and multiple returns for a long period?
12. Does any company filing contract use an MTD API, or are iXBRL/XML and document-package transports required instead? What are the authoritative endpoints, authentication/scopes, headers and test environments?
13. Which company identity, UTR, Companies House number, declarations, signatory data, computations, attachments and accounts metadata are required, and where should they come from?
14. What rounding, precision, units, nil/zero/omission and comparative-prior-period rules apply?
15. What micro-entity eligibility and dormant-company rules must templates enforce rather than merely describe?
16. Can the generic flat Tax Tag architecture remain the accounting projection layer while a separate contract-specific model supplies contexts, calculations and filing packaging, or must the schema itself represent those concepts?
17. Is the existing `SubmitMicro` harness to be retained only as a diagnostic, replaced by a contract-shaped builder, or removed?
18. Which legacy Corporation Tax forecast, accrual, statement and loss-carry-forward calculations remain valid internal planning features, and which—if any—may feed statutory computations?

## 10. Workplan 3 Phase 1 — current `HMRC_MTD` repository reconnaissance

Date: 1 September 2026

Scope: current working tree only. The removed `Sa/v1_0/Submissions/SA100` model is not part of this analysis, is not an architectural precedent, and is not recommended for reconstruction or retention.

### 10.1 Current contract-model surface

The supported contract-shaped area now consists of:

```text
Hmrc/
├── Shared/
│   └── JsonExtract.cs
├── Vat/v1_0/
│   ├── CustomerInformation/
│   ├── FinancialDetails/
│   ├── Liabilities/
│   ├── Obligations/
│   ├── Payments/
│   ├── Penalties/
│   ├── Returns/
│   └── ViewReturn/
└── Sa/v1_0/Submissions/MTDITSA/
    ├── Liabilities/
    ├── Obligations/
    └── Payments/
```

The stable namespace/folder convention is `Hmrc/<Regime>/v1_0/<operation>`, rooted in the project namespace `TradeControl.Tax.UK`. The full requested Corporation Tax namespace is therefore `TradeControl.Tax.UK.Hmrc.CorporationTax.v1_0`. Folders track namespaces, public type names carry a regime prefix, and one principal public class normally occupies each file.

VAT groups classes by API resource and normally supplies `<Resource>Request`, `<Resource>Response`, `<Resource>Endpoint` and child DTO files. MTD ITSA follows the same operation grouping beneath an additional `Submissions/MTDITSA` boundary. There is no current repository precedent for a multi-document XML statutory return package. Its Corporation Tax shape must come from the authoritative CT contract and the workplan, not from a removed model.

### 10.2 DTO/model conventions

Current DTOs are mutable classes with public `get; set;` properties. There is no shared request, response, endpoint or statutory-document interface.

- VAT uses `required` selectively for identifiers such as VRN and period key.
- MTD ITSA requires UTR through constructors.
- VAT mostly uses `DateTime`; MTD ITSA uses `DateOnly` for date-only API fields.
- Monetary values use `decimal`; optional amounts use `decimal?`.
- Repeating response groups use initialised `List<T>` properties.
- Constrained statuses and methods are generally strings.
- No immutable aggregate pattern, money type, identifier value object, accounting-period type, document type or provenance type exists.

Mutable classes and one-type-per-file are reasonable style defaults for Corporation Tax. Requiredness, nullability, date types, enumerations, sign/precision and cardinality must be selected from the CT600/RIM/schema contract in Phase 2. The inconsistent VAT/MTD choices are observations, not rules to import.

### 10.3 Serialization conventions

The remaining supported contract surface is JSON-oriented:

- request classes commonly expose `ToJson(bool indented = false)` using `System.Text.Json`;
- path identifiers are held on requests and excluded from bodies with `[JsonIgnore]`;
- static endpoint descriptors contain `Path`, `Method`, `Version` and `Scope` constants;
- VAT responses commonly use whole-object `JsonSerializer.Deserialize<T>` plus `FromJson`;
- MTD ITSA response types commonly accept `JsonElement` and use exact camel-case property names through `Hmrc.Shared.JsonExtract`.

There is no shared `JsonSerializerOptions`, naming policy, source-generated context, date converter, required-member policy, deterministic fixture framework or schema-validation hook. Default `System.Text.Json` behaviour is therefore not a safe external-contract template without fixtures.

No XML serializer remains in the supported contract tree. Phase 1 consequently provides no local basis for choosing Corporation Tax serialization. VAT JSON methods and REST endpoint descriptors do not imply that CT600, computations or accounts should use JSON or the VAT operation shape. Serialization must wait for the authoritative Corporation Tax technical contract and Phase 7 fixtures.

### 10.4 Validation conventions

VAT and MTD ITSA contract DTOs do not implement model-level validation. `Services/Validation` validates `Dictionary<string, object?>` inputs to the development runner, checks keys/environment/date strings, and sometimes queries Trade Control through SQL readers. Its `ValidationResult` is a simple string warning/error collection.

That is harness/application validation, not statutory contract validation. It cannot validate CT600 requiredness, conditional pages, box arithmetic, accounting-period limits, computation consistency, schema rules or package completeness. It should not be referenced by CT DTOs merely because it is named `Validation`.

The reusable principle is separation of validation orchestration from DTO storage. Actual Corporation Tax validators, result types and rule placement must be designed from authoritative rules in later authorised phases.

### 10.5 Versioning conventions

Live external-model namespaces use `v1_0`; endpoint descriptors expose `Version = "1.0"`. Separately, `ModuleInfo.Version` and internal harness payloads use `2026.1`.

These are distinct version axes. `Hmrc.CorporationTax.v1_0` permits side-by-side internal contract versions, but does not identify the CT600 form, RIM, schema, generic validation rules, computational taxonomy or accounts taxonomy release. The eventual model needs explicit external provenance once those authorities are fixed.

### 10.6 Contract versus transport

VAT and MTD ITSA folders describe bodies and endpoint metadata but send no requests. `Services/Transport` is empty. Configuration, SQL data access, mapping, runner validation, logging and the WebHarness sit outside `Hmrc`:

```text
Hmrc/...             external contract shapes and operation metadata
Infrastructure/...  configuration, database and logging
Services/...        Trade Control readers, mapping, validation and runner orchestration
HMRC.WebHarness      development observation host
```

This separation is directly relevant. CT600, supplementary pages, computations, accounts and contract-shared values belong below `Hmrc.CorporationTax.v1_0.Submissions`. Credentials, environment selection, SQL projection, Tax Tags, logging, HTTP/gateway clients and harness payloads do not.

The existing `SubmitMicro` runner/harness is historical integration material, not a Corporation Tax contract or transport precedent.

### 10.7 Reusable infrastructure

`Hmrc.Shared` contains exactly one type: `JsonExtract`. It extracts strings, decimals and `DateOnly` values from `JsonElement`. It is reusable only if a later Corporation Tax JSON response has identical parsing semantics. Its non-nullable getters silently return null, zero or a default date when a value is absent/invalid, so it cannot distinguish contract failure from a legitimate zero and is unsuitable for required CT fields or contract validation.

There are no reusable shared primitives for UTR, company number, accounting period, money, bank details, declaration, attachment, document identity, schema version or endpoint metadata.

Infrastructure outside `Hmrc.Shared` is generic only at the application layer:

- `EnvironmentSelector` and `HmrcSettings` configure environments;
- `ConnectionFactory` and `SqlHelpers` support Trade Control data access;
- `SubmissionLogger` logs runner activity;
- `ValidationResult` reports runner validation messages.

None belongs in Phase 2 CT600 DTOs. A new type should enter `Hmrc.Shared` only when identical cross-regime external semantics are proven; sharing the CLR type `string` or `decimal` is insufficient. SQL readers, raw tag mappers, reconciliation, harness and runner types are explicitly non-reusable for the authoritative CT model.

### 10.8 Proposed Corporation Tax namespace/file structure

The proposal is deliberately limited to ownership boundaries already required by the workplan. It does not invent fields, serializers, validators, endpoints, envelopes or base classes.

```text
src/HMRC_MTD/Hmrc/CorporationTax/v1_0/
└── Submissions/
    ├── CT600/
    │   ├── Return/
    │   │   └── Ct600Return.cs
    │   ├── SupplementaryPages/
    │   │   └── <one folder/namespace per implemented page>
    │   └── Shared/
    │       └── <types proven common to CT600 return and pages>
    ├── Computations/
    │   └── <computation aggregate and contract-proven sections>
    ├── Accounts/
    │   └── <HMRC accounts-attachment model, deferred to Phase 5>
    └── Shared/
        └── <types proven common across CT600, computations and accounts>
```

Phase ownership controls when these folders gain files:

- Phase 2 creates only the CT600 core and supporting statutory groups proven by the current form/RIM/schema.
- Phase 3 adds each supplementary page separately. It should not add a common interface/base class unless real shared contract behaviour is demonstrated.
- Phase 4 adds computations without merging accounting facts into CT600 boxes.
- Phase 5 adds the accounts attachment/iXBRL boundary.
- Phase 6 may add a separate package namespace once authoritative package cardinality and envelope rules are known.
- Phase 7 decides serializer placement from actual contract fixtures.

`Validation`, `Serialization`, `Transport`, `Endpoints` and `Package` are intentionally absent from the initial physical tree. They should be introduced only by the phase with authoritative evidence for their contents.

### 10.9 Safe conventions and exclusions

Safe to carry forward:

1. Full namespace `TradeControl.Tax.UK.Hmrc.CorporationTax.v1_0`.
2. Folder/namespace alignment and one principal public type per file.
3. Versioned regime isolation.
4. Regime/document-prefixed public names.
5. Contract types separated from infrastructure, Trade Control projection, harness and transport.
6. Separate CT600, supplementary-page, computation and accounts families.
7. Shared types placed at the narrowest scope with proven common semantics.

Not safe without authoritative Corporation Tax evidence:

- VAT REST/request-response patterns or JSON serialization;
- MTD ITSA UTR constructors, query dates, response parsing or scopes;
- `DateTime` versus `DateOnly` choices;
- silent defaults from `JsonExtract`;
- runner dictionary validation;
- SQL readers, Tax Tags, generic harness items or `SubmitMicro`;
- any removed SA100 class, schedule, serializer, envelope, canonicalisation or IRmark design;
- any XML/package design inferred from repository history.

### 10.10 Phase 1 conclusion and Phase 2 gate

Current conventions are understood: versioned regime namespaces, small operation-focused DTO files, endpoint metadata separate from transport, and physical separation between `Hmrc` contracts and Trade Control/harness services. Reusable contract infrastructure is minimal and there is no generic statutory-document framework to adopt.

The proposed Corporation Tax tree establishes boundaries without inventing CT content. Phase 2 must not begin until separately authorised and supplied with the exact current CT600 form, guide, RIM, generic validation rules and schema releases specified by the workplan. The removed SA100 implementation has no role in that decision.

## Sole Trader Objective 2 / Objective 3 Contract Reconciliation

Date: 2 September 2026

Scope: static reconciliation of the current `tcNodeDb4` Sole Trader Objective 2 bootstrap and cumulative projection against `specs/reference/sole-trader-contracts.md`. This is a proposal only; no SQL, C#, harness or project change has been made.

### 1. Current Objective 2 state

The selectable MTD bootstrap paths are:

- MIN: `App.proc_Template_ST_SOLE_CUR_MIN_MTD_2026` -> `App.proc_Template_ST_SOLE_CUR_MIN_2026` -> `App.proc_Template_BASE_MIN_2026`, followed by `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026` and MIN mappings;
- STD: `App.proc_Template_ST_SOLE_CUR_STD_MTD_2026` -> `App.proc_Template_ST_SOLE_CUR_STD_2026` -> the same Sole Trader MIN/base accounting bootstrap, followed by the same Tax Source/Tag seed and STD mappings.

Both use Tax Source `UK-ITSA-SE-CUM`, jurisdiction `UK`, Tax Type `5` (`Quarterly Return`), and source description `MTD ITSA Sole Trader cumulative accounting projection`. `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026` seeds 16 tags: two income, one consolidated expense and 13 detailed expense tags. It gives every tag `TagClassCode = 1` (Component), with income polarity `1` and expense polarity `0`.

The effective extraction path is:

`Cash.tbTaxTagMap` -> `Cash.vwTaxTagCashCode` -> `Cash.fnTaxBizCumulative` -> `TcBusinessTaxReader.ReadCumulativeAsync`.

`Cash.fnTaxTagMapValidate` correctly restricts direct Category/CashCode mappings to Components, recursively expands Category mappings to enabled nominal CashCodes, rejects within-tag parent/child or multiple-root duplicates, and checks contributor polarity. It warns about uncovered enabled P&L CashCodes. It does not encode HMRC manifest requiredness, detailed/consolidated workflow choice, cross-tag exclusivity, tax allowability, or annual ownership.

### 2. Authoritative contract mismatches

1. The current Self Employment v5 detailed cumulative contract has 15 expense properties. Objective 2 has only 13 detailed concepts; `irrecoverableDebts` and `depreciation` are absent.
2. `consolidatedExpenses` is currently a mapped Component (`CT-CUMEXP`). Authoritatively it is a Rollup plus a workflow election and eligibility decision, and must be mutually exclusive with detailed expenses.
3. The SQL model contains no general allowable/disallowable split. It therefore cannot currently populate the 15 disallowable properties deterministically.
4. No authoritative accounting source was found for `taxTakenOffTradingIncome`.
5. `Cash.fnTaxBizCumulative` hard-codes 6 April and configured accounting-period boundaries. It does not support a later business-commencement start and cannot accept all obligation-supplied standard/calendar cumulative end dates.
6. A mapped tag with no period rows is emitted as supported zero because `Amounts` uses `SUM(COALESCE(p.InvoiceValue, 0))`. Objective 2 therefore cannot distinguish “no contributing accounting fact” from a genuine recorded zero.
7. The current cumulative path preserves reversals and negative expenses and does not apply `Math.Abs`; the historical discrete reader's `Math.Abs` defect is not present in this path.

### 3. Quarterly field reconciliation

| HMRC field/concept | Current Objective 2 code | Current status | Ownership/conclusion | Proposal class |
|---|---|---|---|---|
| `turnover` | `turnover` -> `CT-TURNOV` | MIN/STD supported | Component, polarity 1 | **No change required** |
| `other` | `otherBusinessIncome` -> `CT-OTHRIN` | MIN/STD supported | Component; adapter renames to wire `other` | **No change required** |
| `taxTakenOffTradingIncome` | none | unsupported | External/reviewed input; not turnover or subcontractor expense | **No change required** to Objective 2 |
| `costOfGoods` | STD `costOfGoods` -> `CA-COGS` | STD supported | Component, polarity 0 | **No change required** |
| `paymentsToSubcontractors` | STD tag -> `CA-SUBCON` | STD supported | Component, polarity 0; means expense, not CIS tax deducted | **No change required** |
| `wagesAndStaffCosts` | STD tag -> `CT-STAFFC` | STD supported | Component; includes `CC-WAGES`, `CC-PENSN`, `CC-EMPNI` | **No change required** |
| `carVanTravelExpenses` | STD tag -> `CA-MOTOR` and `CA-TRAVEL` | STD supported | One Component with two disjoint Category roots | **No change required** |
| `premisesRunningCosts` | STD tag -> `CA-PREMS` | STD supported | Component | **No change required** |
| `maintenanceCosts` | STD tag -> `CA-REPAIR` | STD supported | Component | **No change required** |
| `adminCosts` | STD tag -> `CA-OFFICE` | STD supported | Component | **No change required** |
| `businessEntertainmentCosts` | STD tag -> `CA-ENTERT` | STD total supported | Component total; no disallowable split | **No change required** for total |
| `advertisingCosts` | STD tag -> `CA-ADVERT` | STD supported | Component | **No change required** |
| `interestOnBankOtherLoans` | STD tag -> `CA-LOANINT` | STD supported | Component | **No change required** |
| `financeCharges` | STD tag -> `CA-FINANCE` | STD supported | Component | **No change required** |
| `irrecoverableDebts` | none | unsupported | Missing Component candidate; see section 6 | **Required before Objective 3** for a complete STD detailed projection |
| `professionalFees` | STD tag -> `CA-PROF` | STD supported | Component | **No change required** |
| `depreciation` | none | unsupported | Missing Component candidate; see section 7 | **Required before Objective 3** for a complete STD detailed projection |
| `otherExpenses` | STD tag -> `CA-OTHER` | STD supported | Component | **No change required** |
| `consolidatedExpenses` | MIN tag -> `CT-CUMEXP` | value extractable but misclassified | Rollup plus workflow choice, not a directly mapped Component | **Required before Objective 3** |
| 15 disallowable fields | none | unsupported | OptionalAbsent unless an exact allowable/disallowable source is added | **No change required** to current mappings; see section 8 |

The Objective 2 names need not match JSON property names. The present names are mostly wire-shaped and are not contract-blocking. If projection-oriented names such as `cisPaymentsToSubcontractors`, `wagesSalariesStaffCosts`, `rentRatesPowerInsurance`, `repairsMaintenance`, `phoneFaxStationeryOfficeCosts`, `businessEntertainment`, `advertising`, `bankCreditCardFinancialCharges`, `accountancyLegalProfessionalFees` and `otherBusinessExpenses` are preferred, rename them only as a separately migrated cleanup with adapter fixtures. This is **Recommended cleanup**, not a prerequisite for correct DTO population.

### 4. MIN mapping assessment

MIN installs exactly:

| Tag | Root | Effective bootstrap contributors |
|---|---|---|
| `turnover` | `CT-TURNOV` | `CA-SALES` -> `CC-SALES` |
| `otherBusinessIncome` | `CT-OTHRIN` | `CA-INCOME` -> `CC-INCME` |
| `consolidatedExpenses` | `CT-CUMEXP` | `CT-CSTSAL`, `CT-STAFFC`, `CT-OVERHD`; ultimately enabled `CA-DIRECT`, `CA-WAGES`, `CA-ADMIN` CashCodes |

The roots are disjoint, contributor polarities agree with their tags, and the base Sole Trader path disables `CC-DEPRC` and `CC-DEPRJ`, so `CA-ASSET` contributes no enabled code. No bootstrap-enabled MIN P&L CashCode is left outside the two income roots and `CT-CUMEXP`.

The mapping is nevertheless not statutorily sufficient as implemented. HMRC defines consolidated expenses as allowable expenses and restricts their use by turnover/workflow choice. `CT-CUMEXP` is a raw accounting total; neither its tree nor `Cash.fnTaxBizCumulative` proves allowability or the under-£90,000 eligibility/election. MIN must remain an accounting template, not simulate 15 detailed categories. The safe boundary is:

- retain `CT-CUMEXP` as an internal accounting rollup;
- change Tax Tag `consolidatedExpenses` to `TagClassCode = 0` (Rollup);
- remove its row from `Cash.tbTaxTagMap` because only Components may be mapped;
- have the population/contract adapter select consolidated mode only after workflow eligibility and derive the approved allowable rollup without installing detailed MIN mappings;
- do not make generic mapping validation source-specific. Detailed/consolidated exclusivity belongs in population/contract validation.

This correction is **Required before Objective 3**. The existing MIN accounting categories and CashCodes otherwise require **No change**.

### 5. STD mapping assessment

STD maps two income tags and 13 detailed expense concepts. Its effective Category roots are disjoint between tags. The two roots for `carVanTravelExpenses` (`CA-MOTOR`, `CA-TRAVEL`) are siblings under `CT-OVERHD`, not a parent/child overlap. Every mapped income leaf has polarity 1 and every mapped expense leaf has polarity 0. No mapped parent/child conflict or within-tag double count was found.

STD disables the coarse `CC-DIRCT` and `CC-ADMIN` codes, preserving the underlying MIN accounting structure while requiring detailed posting. It does not install a consolidated mapping, so the current STD bootstrap does not mix detailed and consolidated modes.

One conditional enabled P&L CashCode can remain uncovered: `CC-MINER`, created by `proc_Template_BASE_MIN_2026` under `CA-DIRECT` when `CoinTypeCode < 2`. STD maps `CA-COGS` and `CA-SUBCON`, not their parent `CT-CSTSAL` or sibling `CA-DIRECT`, and disables `CC-DIRCT` but not `CC-MINER`. The generic validator will warn, but not fail. Classify and either remap/disable `CC-MINER` for STD Sole Trader nodes according to its real accounting meaning. This is **Required before Objective 3** if that conditional code can exist in a supported STD node; it must not silently escape the statutory projection.

STD otherwise remains a valid accounting template rather than an HMRC taxonomy. Adding two dedicated accounting classifications for bad debts and depreciation is preferable to broadening parent mappings or forcing their values into `otherExpenses`.

### 6. `irrecoverableDebts` assessment

Repository searches found no Sole Trader Category or CashCode whose accounting meaning is irrecoverable/bad debt. `CA-OTHER`/`CC-OTHER` is too broad and mapping it simultaneously to `irrecoverableDebts` would double count `otherExpenses`. No existing child beneath `CA-OTHER` isolates the amount.

| Branch | Deterministic source now | Candidate | Ancestry/overlap | Classification |
|---|---|---|---|---|
| MIN | No | none | Coarse consolidated accounting cannot isolate bad debt | OptionalAbsent in detailed mode; included only through an approved consolidated allowable rollup |
| STD | No | add a dedicated expense nominal Category/CashCode beneath `CT-OVERHD` | Must be a sibling of `CA-OTHER`, not its child while `CA-OTHER` remains mapped; polarity 0 | Missing supported Component candidate |

For STD, add a dedicated nominal expense Category (exact new code to be chosen under repository conventions), a dedicated CashCode, and a Component Tax Tag `irrecoverableDebts` with `CashPolarityCode = 0`. Map only that dedicated root. Do not map `CA-OTHER` to both tags and do not manufacture a zero merely to complete the manifest. This is **Required before Objective 3** if STD detailed submission is in scope. Existing/historical `CC-OTHER` transactions cannot be reclassified automatically; any migration is an accounting review, not a mechanical split.

### 7. `depreciation` assessment

The base template has exact accounting candidates `CC-DEPRC` (Depreciation) and `CC-DEPRJ` (Depreciation Adjustment), both under neutral money Category `CA-ASSET` (`CashPolarityCode = 2`), whose parent is `CT-PANDL`. The Sole Trader MIN bootstrap explicitly disables both as company-only codes, and STD inherits that state. Therefore neither MIN nor STD has an enabled, polarity-compatible depreciation contributor today.

`CC-DEPRC` is the semantic accounting-depreciation candidate; `CC-DEPRJ` is not interchangeable and must not be swept into the quarterly field without a separately proven meaning. Mapping `CA-ASSET` is invalid because it is neutral and would also risk combining charge and adjustment. Mapping depreciation to annual capital allowances is prohibited: accounting depreciation and statutory capital allowances are distinct.

| Branch | Deterministic source now | Correct direction | Classification |
|---|---|---|---|
| MIN | No; both codes disabled | Do not add a detailed mapping. If consolidated mode is chosen, only an approved allowable rollup may be sent; accounting depreciation is not silently treated as capital allowance or allowable consolidated expense. | OptionalAbsent for detailed property |
| STD | No enabled source, but `CC-DEPRC` is an exact dormant accounting candidate | Place/enable the accounting charge in a dedicated enabled nominal expense Category with polarity 0 and map a new Component tag `depreciation`; keep `CC-DEPRJ` separate unless its semantics are proven. | Missing supported Component candidate |

The STD accounting classification and tag are **Required before Objective 3** for complete detailed support. Capital-allowance treatment and any matching `depreciationDisallowable` rule remain a separate annual/disallowable decision.

### 8. Disallowable-expense assessment

Neither `Cash.tbCode`, `Cash.tbCategory`, `Cash.tbTaxTagMap` nor the Sole Trader templates store an allowable amount, a disallowable amount, an allowability percentage, private-use fraction, or reviewed statutory allocation. VAT `TaxCode` is not income-tax allowability. MIN's coarse Categories cannot distinguish the split. STD distinguishes accounting totals by expense kind, but not allowable and disallowable portions within a kind.

`CA-ENTERT`/`CC-ENTERT` may look like a potential fully disallowable category, and a dedicated depreciation category could support a depreciation addition, but the current repository does not encode or validate those statutory rules. They cannot be declared deterministic solely from their labels. All 15 disallowable fields are therefore currently **OptionalAbsent/Unsupported** in Objective 2. The family is not deterministically supportable today.

Do not create parallel Tax Tags or artificial allocations. A later, separately authorised phase may make selected fields supportable only by introducing explicit accounting/statutory classification and tests (for example, an approved fully-disallowable business-entertainment code or reviewed private-use split). This is **Deferred**. Omitting unsupported optional fields is **No change required** before Objective 3 DTO implementation, provided the adapter preserves absence and the product does not claim to submit those adjustments.

### 9. `taxTakenOffTradingIncome` ownership assessment

No authoritative CategoryCode, CashCode, Tax Tag, payment field or directly relevant procedure identifies tax deducted from this sole trader's trading income. `CC-SUBCON` is expenditure paid to subcontractors; VAT/general/business-tax codes represent other liabilities; turnover and supplier-payment deductions are not substitutes.

Classify `taxTakenOffTradingIncome` as **External/reviewed workflow input**, not an Objective 2 accounting Component. Do not add a Tax Tag. This is **No change required** to Objective 2; Objective 3 must keep the property nullable/absent unless a legitimate external source is supplied.

### 10. Annual Objective 2 candidate classification

| Annual concept | Current Trade Control evidence | Ownership | Proposal |
|---|---|---|---|
| `includedNonTaxableProfits` | no distinct Category/CashCode | OptionalAbsent | **Deferred** until deterministic classification exists |
| `basisAdjustment` | no basis-period calculation in reviewed path | External (potential reviewed Derived value) | **No Objective 2 mapping** |
| `accountingAdjustment` | no change-of-practice statutory source | External | **No Objective 2 mapping** |
| `outstandingBusinessIncome` | no distinct source; `otherBusinessIncome` is not equivalent | OptionalAbsent | **Deferred** |
| `balancingChargeBpra` | no BPRA asset/allowance source | External | **No Objective 2 mapping** |
| `balancingChargeOther` | no statutory disposal/allowance calculation | External | **No Objective 2 mapping** |
| `goodsAndServicesOwnUse` | `CC-OWNCAP` records owner capital movements, not goods/services at normal sale value | OptionalAbsent | **Deferred**; add only with a dedicated Component source |
| `transitionProfitAmount` | no transition-profit calculation | External | **No Objective 2 mapping** |
| `transitionProfitAccelerationAmount` | user election with no accounting source | Contextual/External | **No Objective 2 mapping** |
| current capital-allowance scalar fields | accounting depreciation exists only as a separate dormant accounting mechanism; no statutory allowance engine | External, potentially Derived in a future asset workflow | **Deferred**; never map from depreciation |
| `tradingIncomeAllowance` | user election; requires removal of final cumulative expenses | Contextual | **No Objective 2 mapping** |
| structured/enhanced structured-building allowances | no qualifying-expenditure/building statutory dataset | External; building identity/address is Contextual | **Deferred** |
| Class 4 exemption reason | taxpayer status, not accounting | Contextual | **No Objective 2 mapping** |
| current-year accounting profit/loss | P&L can supply an accounting starting point | Derived after approved statutory adjustments | **Deferred** to annual/loss workflow |
| brought-forward losses, claims, preference order and carry-back liability credits | `Cash.vwTaxLossesCarriedForward` is internal planning based on configured tax rates, not authoritative HMRC per-business loss/claim state | External/Contextual; liability credit is professional calculation | **No Objective 2 mapping** |

No annual value above is currently a proven Component suitable for `Cash.tbTaxTagMap`. `goodsAndServicesOwnUse`, `includedNonTaxableProfits` and `outstandingBusinessIncome` may become Components only after dedicated deterministic accounting classifications exist. The annual DTO must not recreate an EOPS aggregate.

### 11. Existing quarterly Tax Tag vocabulary

| Current tag | Decision |
|---|---|
| `turnover` | retain unchanged |
| `otherBusinessIncome` | retain unchanged; adapter emits `other` |
| `consolidatedExpenses` | retain name, change class from Component (1) to Rollup (0), remove direct map |
| `costOfGoods` | retain unchanged |
| `paymentsToSubcontractors` | retain; optional later rename to `cisPaymentsToSubcontractors` for projection clarity |
| `wagesAndStaffCosts` | retain; optional later projection-oriented rename |
| `carVanTravelExpenses` | retain unchanged |
| `premisesRunningCosts` | retain; optional later projection-oriented rename |
| `maintenanceCosts` | retain; optional later projection-oriented rename |
| `adminCosts` | retain; optional later projection-oriented rename |
| `advertisingCosts` | retain unchanged |
| `businessEntertainmentCosts` | retain unchanged |
| `interestOnBankOtherLoans` | retain unchanged |
| `financeCharges` | retain; optional later projection-oriented rename |
| `professionalFees` | retain; optional later projection-oriented rename |
| `otherExpenses` | retain; optional later projection-oriented rename |
| `irrecoverableDebts` | missing required candidate for STD detailed support |
| `depreciation` | missing required candidate for STD detailed support |

No existing quarterly tag is obsolete. The required vocabulary change is two missing Component candidates and the corrected class/derivation semantics for `consolidatedExpenses`. Wire-name versus projection-name cleanup is not blocking.

### 12. Cumulative extraction assessment

`Cash.fnTaxBizCumulative` is cumulative in aggregation: it accepts explicit start/end dates and sums all configured period values inclusively across that interval. It has no period key, obligation ID, NINO, business ID, calculation ID or declaration state. Those identifiers correctly remain outside Objective 2.

The date validation is not contract-compatible in all supported cases:

- start must be exactly 6 April and the first configured `App.tbYearPeriod.StartOn` for the year;
- start cannot be a later business-commencement boundary;
- end must be the day before a configured accounting-period start;
- standard HMRC obligation ends such as 5 July are rejected unless the accounting calendar happens to contain 6 July;
- calendar-quarter and first-year choice boundaries are not reliably represented;
- annual/latent sources for which HMRC permits omitted body dates are not represented by this function.

The period end should come from Objective 3/workflow obligation context and be checked there against HMRC rules. Objective 2 should aggregate to an supplied valid accounting cutoff without pretending its monthly accounting calendar is the HMRC obligation model. Correcting this boundary is **Required before Objective 3**.

Sign handling is compatible: `Cash.vwCashCodePeriodValues` orients expense accounting values, and `fnTaxBizCumulative` converts them to statutory orientation without `ABS`. Credits and reversals can remain negative; `Tests/Phase4D_CumulativeProjection.sql` explicitly tests this. This requires **No change**.

Absence handling is not fully compatible. An unmapped tag returns `Unsupported` and `NULL`, which is correct. A mapped tag with no period facts returns `Supported` zero, making absence indistinguishable from a genuine zero row. Preserve contributor presence (or equivalent provenance) so the adapter can apply the authoritative zero/omission rule and OQ-1 decision. This is **Required before Objective 3**; do not solve it by serializing every missing value as zero.

### 13. Exact proposed changes before Objective 3

#### Required before Objective 3

1. In `App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026`, seed `consolidatedExpenses` as Rollup (`TagClassCode = 0`) rather than Component.
2. In `App.proc_Template_ST_SOLE_CUR_MIN_MTD_2026`, remove the direct `consolidatedExpenses -> CT-CUMEXP` mapping. Retain `CT-CUMEXP` only as an internal accounting rollup and define the population adapter's approved allowable-rollup/election path.
3. Add STD-only dedicated accounting classifications and polarity-0 Component tags/mappings for `irrecoverableDebts` and `depreciation`, or explicitly declare STD detailed mode unsupported until those classifications exist. Do not reuse `CA-OTHER`, neutral `CA-ASSET`, or `CC-DEPRJ` mechanically.
4. Resolve conditional uncovered `CC-MINER` in STD: classify it into one supported detailed root or disable it for that template based on its actual accounting meaning.
5. Change cumulative extraction so the applicable tax-year/business-commencement start and obligation period end can be supplied by workflow without requiring HMRC dates to coincide with `App.tbYearPeriod` month boundaries.
6. Preserve the distinction between no accounting contribution and a genuine zero in `Cash.fnTaxBizCumulative`/its reader contract.
7. Add/adjust acceptance fixtures for: the 18-tag manifest (2 income, consolidated Rollup, 15 detailed Components); MIN Rollup/no direct map; STD 15-field coverage; no overlap; bad-debt/depreciation polarity and ancestry; `CC-MINER`; commencement and standard/calendar ends; negative reversals; unsupported/null versus recorded zero; and detailed/consolidated mutual exclusion at the adapter boundary.

#### Recommended cleanup

1. Document projection-name to HMRC-wire-name mappings explicitly in the future adapter rather than relying on identical strings.
2. Consider projection-oriented tag renames only with a deliberate data/adapter migration; they are not contract blockers.
3. Rename or narrow generic cumulative objects only if later work needs to make their Sole Trader date assumptions explicit; do not mix that cleanup into the blocking corrections.

#### Deferred

1. Explicit allowable/disallowable accounting splits and selected disallowable Tax Tags.
2. Annual Component candidates for own use, non-taxable profits or outstanding income, if dedicated sources are later introduced.
3. Capital-allowance, basis-period, transition-profit, structured-building and loss-claim workflows.
4. HMRC Sandbox resolution of OQ-1 and the 2026-27 preview annual schema.

### 14. Items requiring no change

1. `UK-ITSA-SE-CUM` remains the appropriate Objective 2 source identity for the supported cumulative self-employment projection.
2. The two income mappings are semantically correct.
3. The existing 13 STD detailed expense mappings are semantically correct and polarity-compatible.
4. STD's separate `CA-MOTOR` and `CA-TRAVEL` roots under one tag are disjoint and intentional.
5. MIN should remain coarse and should not simulate STD's detailed taxonomy.
6. NINO, HMRC business ID, tax year, obligation dates, accounting type, period-of-account details, calculation IDs, declaration state, Class 4 exemption and structured-building identity remain workflow/Objective 3 data, not Tax Tags.
7. Negative expense values and reversals remain signed; no blanket `Math.Abs` belongs in the cumulative adapter.
8. Unsupported optional disallowable, tax-deducted and annual values should remain absent rather than fabricated.
9. Retired SA100, SA103F, EOPS, period-key and crystallisation models remain retired.

### 15. Unresolved questions

1. HMRC OQ-1 remains: confirm in the stateful Sandbox which zero leaves are mandatory versus omissible before serialization fixtures are frozen.
2. Product decision: will MIN consolidated mode be supported, and where will the under-£90,000 eligibility, trading-allowance interaction and approved allowable-expense review be owned?
3. Accounting decision: what permanent CategoryCode/CashCode identifiers should be assigned to STD irrecoverable debts and depreciation, and how should existing transactions currently posted to `CC-OTHER` or dormant depreciation codes be reviewed?
4. Accounting decision: what is `CC-MINER` intended to represent in fiat/crypto configurations, and which STD statutory category, if any, owns it?
5. Tax-policy decision: can any dedicated category (notably business entertainment or future depreciation) be asserted fully disallowable, and what evidence/rules/tests establish that assertion?

### 16. Completion-gate answer

The current Objective 2 projection is sufficient for the two ordinary income fields, the existing 13 STD detailed expense concepts, signed cumulative accounting values, and a coarse MIN accounting expense rollup. It is not yet sufficient to populate the current contract safely because consolidated expenses are misclassified/directly mapped, STD lacks deterministic irrecoverable-debt and depreciation classifications, one conditional STD P&L code may be uncovered, and cumulative date/absence semantics conflate accounting calendar boundaries and missing facts with the HMRC workflow contract.

The smallest safe implementation phase is therefore the seven **Required before Objective 3** corrections in section 13. It must not add disallowable, tax-deducted or annual Tax Tags without deterministic sources, and it must not reintroduce EOPS or retired Self Assessment structures.

### 17. Reviewed implementation decision addendum

The implementation review refined the TagClass interpretation used in sections 4, 6, 7, 11, 13 and 16 above. This addendum records the final decision without rewriting the historical reconnaissance:

1. TagClass describes the statutory field's behaviour, not whether its Trade Control accounting source is itself aggregated. A writable HMRC input is a Component and may be mapped; a Rollup is read-only/calculated on the HMRC side; a Derived value is obtained outside normal Business Node accounting mapping.
2. `consolidatedExpenses` is a writable HMRC input and remains `TagClassCode = 1`, `CashPolarityCode = 0`. MIN intentionally retains the Category mapping `CT-CUMEXP -> consolidatedExpenses`. Eligibility, detailed/consolidated exclusivity and trading-allowance workflow remain Objective 3/population concerns; they do not change this Objective 2 mapping.
3. `irrecoverableDebts` and `depreciation` are valid writable expense Components in the `UK-ITSA-SE-CUM` manifest. Both are seeded with polarity 0 and left unmapped by default. No accounting Category/CashCode is created, enabled or inferred for either field. A business may later configure an appropriate accounting treatment and map it through the Tax Configurator.
4. Accounting depreciation remains distinct from capital allowances. The dormant base depreciation codes are not enabled or restructured by this synchronisation.
5. `CC-MINER` is confined to cryptocurrency nodes by repository evidence: `Cash.tbCoinType` defines codes 0/1 as Main/TestNet and code 2 as Fiat; the base creates `CC-MINER` only when `CoinTypeCode < 2`; and `App.proc_BasicSetup` forces Fiat unless the unit of charge is BTC. It is not an Objective 3 blocker and receives no statutory mapping change.
6. Missing-row versus explicit-zero behaviour remains unchanged pending the authoritative OQ-1 Sandbox decision. No provenance enhancement is included.
7. `Cash.fnTaxBizCumulative` now validates only the generic chronological condition `@PeriodStart <= @PeriodEnd`. It no longer requires 6 April, the first configured financial period, or an end immediately before another configured accounting period. Obligation/calendar validation belongs to Objective 3 workflow.
8. Disallowable expenses, `taxTakenOffTradingIncome` and annual fields remain outside this implementation exactly as classified above.

The final quarterly manifest is 18 writable Components: two income, one consolidated expense and 15 detailed expenses. MIN maps two income fields plus consolidated expenses. STD maps two income fields plus its existing 13 detailed accounting fields; `irrecoverableDebts` and `depreciation` remain available but unmapped by default.
