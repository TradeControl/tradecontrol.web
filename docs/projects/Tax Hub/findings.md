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
