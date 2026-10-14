# Limited Company Statutory Projection Field Sets — UK Micro-Entities

Status: research reference

Scope: UK private limited companies eligible for the micro-entities regime

Research cut-off: 1 September 2026

## Purpose and boundary

This reference defines the information model needed to support the current statutory filing obligations of a UK micro-entity limited company:

1. delivery of accounts to Companies House; and
2. submission of a Company Tax Return to HMRC.

Those are separate filings with different legal content, technical contracts, credentials, validations and acknowledgements. A product may prepare or transmit both, but it must not model them as one form or assume that acceptance by one authority implies acceptance by the other. The former joint HMRC and Companies House online filing service closed on 31 March 2026. Companies now use Companies House filing routes for accounts and commercial software for HMRC Company Tax Returns. [S1] [S2]

The model below deliberately separates:

- accounting and legal facts that can be shared;
- a Companies House accounts projection;
- an HMRC accounts and tax-computation projection;
- CT600 and supplementary-page return fields;
- presentation, taxonomy, packaging and transport metadata.

It is a research specification, not an implementation schema. Taxonomy names, XML schema identifiers and gateway artefacts are versioned external contracts. This document records verified FRC 2026 reference Names for the minimum accounts face, but a filing implementation must still bind and validate them against the exact entry point selected for that submission.

### Evidence and status convention

This document uses the following meanings consistently:

| Classification | Meaning in this reference |
|---|---|
| Statutory requirement | A requirement supported by legislation, FRS 105 or current official filing guidance; its source is cited |
| Technical contract | A versioned requirement of an official taxonomy, schema, RIM, gateway or validation specification |
| Interpretation | A conclusion drawn from the cited current contracts; expressly described as an interpretation rather than law |
| Implementation recommendation | A boundary or control proposed for later Trade Control design; it is not presented as an authority requirement |
| Unresolved | A published change or contract detail that is not final enough to specify safely at the cut-off date |

Tables describing statutory content use “required”, “conditional” and “derived” in the first sense. Sections headed as adapter ownership or Trade Control supply boundaries are implementation recommendations. The historical AC/CP conclusion is an interpretation from contract inspection. The change-watch list is unresolved work.

## Filing model at a glance

| Layer | Purpose | Typical owner | Reuse rule |
|---|---|---|---|
| Shared statutory and accounting facts | Company identity, reporting periods, current and comparative balances, accounting policies and disclosures | Trade Control accounting domain | Authoritative facts; no filing-service identifiers |
| Companies House accounts projection | The copy of accounts legally delivered to the registrar, including current filleting choices and statutory statements | Companies House adapter | Derived from shared facts plus registrar-specific declarations and submission metadata |
| HMRC accounts projection | Full statutory accounts attached to the Company Tax Return in iXBRL | HMRC adapter | Derived from the same accounting facts, but must include content omitted from public Companies House delivery, notably the profit and loss account under the current rules |
| Corporation Tax computation | Tax adjustments, allowances, claims, losses and calculation schedules for each Corporation Tax accounting period | Tax computation domain and HMRC adapter | Accounting facts are inputs, not a substitute for tax facts |
| CT600 return | Boxed return, conditional supplementary pages, declaration, repayment and bank details | HMRC adapter | Box semantics and arithmetic follow the current CT600/RIM, not ledger debit/credit signs |
| Filing envelope | Taxonomy/version, contexts, units, credentials, packaging, validation, submission and acknowledgement | Authority-specific adapter | Never shared across the two authorities merely because both use iXBRL |

### Current filing landscape

| Authority | What the company files | Current routes at cut-off | Contract character |
|---|---|---|---|
| Companies House | A legally deliverable copy of the annual accounts; for an eligible micro-entity the current copy can omit the profit and loss account and directors' report | Eligible accounts through WebFiling, commercial software through the XML Gateway, or paper where permitted; the latter two non-software routes are scheduled to close for accounts on 1 April 2028 | Statutory accounts plus registrar-specific filing contract |
| HMRC | A Company Tax Return package: CT600, applicable supplementary pages, full statutory accounts and tax computations, with permitted supporting documents | Commercial software using the Corporation Tax online XML/Transaction Engine contract; paper only under limited exceptions such as reasonable excuse or the Welsh-language route | Tax return and attachment package; not an MTD API |

The use of iXBRL by both authorities does not combine the submissions. The same FRC accounts taxonomy concepts can represent underlying accounts facts, but each authority has its own accepted versions, metadata, business rules, envelope, credentials and acknowledgement. [S1] [S2] [S11] [S13] [S16]

## Eligibility and reporting basis

### Micro-entity qualification

For a financial year beginning on or after 6 April 2025, a company normally qualifies as a micro-entity when it satisfies at least two of these three conditions: turnover not more than £1 million, balance-sheet total not more than £500,000, and no more than 10 employees on average. For periods beginning from 30 September 2013 through 5 April 2025 the monetary thresholds were £632,000 and £316,000, with the same employee threshold. [S3]

The qualification decision is period-specific. In a company's first financial year it applies the conditions for that year. In later years it normally considers both the current and preceding financial year, with the statutory one-year grace rules. The model must retain the measurements and the conclusion rather than a timeless `is_micro` flag.

The micro-entities regime is unavailable to specified entities, including public companies, charities, overseas or unregistered companies, companies within section 384/384B exclusions, and certain parents or subsidiaries in groups. The complete statutory exclusion test must be performed for the relevant period. [S3]

| Fact | Context/cardinality | Status | Calculation or validation | Primary source |
|---|---|---|---|---|
| `financial_year_start`, `financial_year_end` | One current period; prior comparative period where applicable | Required | End must follow start; drives the threshold version | S3, S4 |
| `qualifying_turnover` | One amount for each tested year | Required for eligibility | Compare with threshold effective at that year's start | S3 |
| `qualifying_balance_sheet_total` | One instant amount for each tested year | Required for eligibility | Compare with period-appropriate threshold | S3 |
| `average_number_of_employees` | One integer for current and comparative period | Required for eligibility and a note | Average calculated on the statutory/FRS 105 basis; do not substitute closing headcount | S3, S5 |
| `micro_thresholds_version` | One per tested year | Derived | `pre_2025_04_06` or `from_2025_04_06`, based on period start | S3 |
| `exclusion_tests[]` | One result per statutory exclusion | Required | All relevant exclusions false | S3 |
| `qualifies_as_micro_entity` | One result for current year | Derived, reviewable | Two-of-three tests, exclusions, prior-year rule and grace rule | S3 |
| `accounting_standard` | One per accounts set | Required | Normally FRS 105 for this field set | S5 |
| `frs105_edition` | One per accounts set | Required technical provenance | September 2024 edition plus applicable amendments; determine by period start | S5 |

FRS 105's periodic-review amendments are mandatory for periods beginning on or after 1 January 2026. The February 2026 adapted-format clarification is effective for periods beginning on or after 1 January 2027. Threshold-related amendments apply from 6 April 2025. Consequently, period start—not preparation or filing date—selects the accounting requirements. [S5]

### Accounting periods are not interchangeable

Maintain three explicit kinds of period:

- **Financial year / accounts period**: the period covered by one statutory set of accounts. It may exceed 12 months, particularly for first accounts, and may be extended up to 18 months in the ordinary case. [S4]
- **Corporation Tax accounting period**: never longer than 12 months. A long accounts period normally produces two Company Tax Returns, each with its own CT600 and computation.
- **iXBRL fact context**: duration or instant context attached to an individual fact, including current and comparative contexts and any dimensions.

For a long accounts period, HMRC normally receives the same accounts instance with each return, while each CT600 and computation covers only its own Corporation Tax accounting period. [S6] [S7]

| Period fact | Cardinality | Rule |
|---|---|---|
| Accounts period | Exactly one current period per accounts set | May be short or long; retains start, end and duration |
| Comparative accounts period | Normally one prior period | FRS 105 requires comparative amounts for all reported amounts; first accounts are the principal exception |
| Corporation Tax accounting period | One or more per accounts set | Maximum 12 months; periods must map explicitly to the accounts period |
| CT600 return period | Exactly one per CT600 | Same start/end as the corresponding Corporation Tax accounting period |
| Balance-sheet context | Current and comparative instants | At each statement-of-financial-position date |
| Profit-and-loss context | Current and comparative durations | Over the relevant reporting periods |
| Note context | Instant or duration as defined by the selected taxonomy concept | Never inferred solely from screen placement |

## Shared company and accounts facts

### Fact-class index

This index makes the brief's statutory fact classifications explicit. Detailed members and calculations follow in the subsequent tables.

| Classification | Coherent fact groups | Filing target | Data type / monetary nature | Authoritative identifier family | Context and status | Sign / calculation rule |
|---|---|---|---|---|---|---|
| Shared accounting facts | Company identity, financial year, current/comparative statement lines, notes, employee count, presentation currency, approval | Companies House accounts and HMRC accounts attachment | Strings, dates, booleans, integers, monetary amounts and narrative | Selected FRC taxonomy concept QName/Name plus legal company identifiers | Instant/duration as taxonomy defines; required or conditional | Accounting presentation and taxonomy balance/negation rules; statement totals derived |
| Companies House-specific facts | Delivered-copy omissions, registrar statements/elections, authentication, presenter and submission metadata | Companies House only | Booleans, enumerations, strings, dates; mostly non-monetary | Registrar schema/TIS elements and envelope identifiers | Filing/attempt context; required or conditional | No ledger sign; eligibility and wording validation |
| HMRC accounts/computation facts | Detailed P&L, tax adjustments, allowance pools, gains, losses, claims and computation narratives | HMRC package | Monetary schedules, dates, enumerations and narrative | Accepted CT computational taxonomy QNames/Names | One computation per CT accounting period; conditional sections | Computation direction controls add/deduct; nil differs from absent |
| Company Tax Return fields | CT600 boxes and supplementary pages | HMRC only | Strings, dates, booleans, integer counts and monetary amounts | CT600 box number and matching RIM element for the selected release | One return per CT accounting period; required/conditional by box | RIM/form arithmetic; generally positive magnitudes rather than ledger signs |
| Calculated / roll-up facts | Statement totals, net assets, accounting result, adjusted trade result, taxable totals, tax liability and payable/repayable amounts | Both, according to fact | Primarily monetary | Relevant taxonomy concept or CT600/RIM box | Same context as inputs unless the statutory formula specifies otherwise | Deterministic formula with reconciliation and unrounded inputs |
| Derived / external facts | Eligibility, associated-company facts, audit status, declarations, payments/credits, bank/nominee data and credentials | Authority-specific | Mixed; often non-ledger and sensitive | Legal/authority fields | Period, approval event or submission attempt | Statutory calculation, taxpayer assertion or external evidence |
| Conditional facts | Director loans/guarantees, commitments, special income, reliefs and every CT600 supplementary page | Accounts and/or HMRC | Repeating mixed records | Applicable taxonomy concepts or return/page elements | Present only when legal or transaction trigger applies | Preserve “not applicable”, “nil”, “unknown” and “omitted” separately |

No Trade Control Tax Tag is defined here. An authoritative concept identifier can only be recorded after choosing a filing authority, taxonomy release and entry point; a display label is not an identifier. [S12]

### Machine-key model for a Category Tree

The statutory heading is the human-readable label, not the machine key. In the FRC presentation sheets:

- **Label** is human-readable text;
- **Name** is the taxonomy element's unique machine-readable local name; and
- **Prefix** identifies the taxonomy namespace used by that workbook.

For example, the FRC 2026 presentation sheet publishes the turnover concept with prefix `core` and Name `TurnoverRevenue`, conventionally written `core:TurnoverRevenue`. The prefix is only a document alias: the durable external identifier is the expanded QName—namespace URI plus local Name—resolved through the selected taxonomy entry point. [S24]

An iXBRL fact is therefore not merely `key = value`. Its effective identity is:

```text
taxonomy release + entry point + expanded QName
+ reporting entity + period context + dimensions
+ unit + decimals/scaling + value
```

The same concept is legitimately repeated. Current-year and comparative turnover both use the turnover QName, but have different duration contexts. More importantly, some statutory headings share one concept and are distinguished by dimensions. In FRC 2026, both creditors due within one year and creditors due after one year use `core:Creditors`; the difference is:

```text
core:FinancialInstrumentCurrentNon-currentDimension = core:WithinOneYear
core:FinancialInstrumentCurrentNon-currentDimension = core:AfterOneYear
```

A flat Category Tree node containing only `core:Creditors` would therefore lose a statutory distinction and could not generate a conformant filing.

#### Required Category Tree binding record

This is an implementation recommendation for the later design phase. It does not introduce new statutory facts or Trade Control Tax Tags.

| Binding attribute | Purpose |
|---|---|
| Stable Trade Control semantic key | Identifies the accounting meaning independently of any authority release, for example a node representing turnover rather than `AC12` or a QName |
| Category path and roll-up parent | Places the fact in the accounting Category Tree and defines internal aggregation |
| Filing target and document role | Distinguishes Companies House accounts, HMRC accounts, HMRC computation and CT600 |
| Taxonomy family, release and entry point | Selects the applicable external contract and its validity window |
| Namespace URI and concept local Name | Stores the expanded QName; do not rely on a changeable XML prefix or human label |
| Data type, period type and balance | Carries the taxonomy's monetary/string/date type, instant/duration rule and debit/credit metadata |
| Required dimensions and members | Preserves distinctions such as within/after one year, current/comparative scope, company/group and other taxonomy axes |
| Unit and decimals/scaling policy | Separates the unrounded accounting value from its iXBRL representation |
| Source aggregation and sign normalisation | Defines how ledger categories roll up and how Trade Control signs become the taxonomy's presented value |
| Calculation/reconciliation rule | Records derived totals and cross-statement checks without treating them as transaction mappings |
| Applicability and omission rule | Distinguishes required, conditional, nil, omitted and not-applicable states |
| Source and verification evidence | Records the presentation-sheet row/concept, specification version and validator/conformance result |

The internal semantic key should remain stable when a taxonomy changes. A versioned binding maps that key to the external QName, dimensions and rendering rules. The external QName must not become the Category Tree's permanent primary key.

A practical binding can therefore reuse one semantic node in several projections without pretending the projections have the same contract:

| Stable semantic meaning | Accounts binding | Additional context | Separate tax use |
|---|---|---|---|
| Turnover | `core:TurnoverRevenue` | Current and comparative duration contexts; GBP unit and declared decimals | Input to trade turnover and taxable-profit computation; not a direct copy to every CT600 income box |
| Creditors due within one year | `core:Creditors` | Instant context plus `core:FinancialInstrumentCurrentNon-currentDimension = core:WithinOneYear` | Usually no direct CT600 box; may supply computation or disclosure evidence |
| Creditors due after one year | `core:Creditors` | Instant context plus `core:FinancialInstrumentCurrentNon-currentDimension = core:AfterOneYear` | Usually no direct CT600 box |
| Book depreciation | Component of `core:DepreciationAmortisationImpairmentExpense` | Duration context, with detailed accounting children retained | Separate Corporation Tax computation adjustment and capital-allowance inputs; never reuse the accounts QName as a computation key |

This is why the Category Tree is the semantic accounting layer beneath the filing adapters, not a serialised iXBRL tree.

### Identity and legal status

These facts are reusable, but each submission must render them according to its own contract.

| Fact | Context/cardinality | Status | Validation / derivation | Consumers | Source |
|---|---|---|---|---|---|
| Legal company name | One current legal name; retain name changes relevant to accounts | Required | Must reconcile to registrar identity; disclose a change during the period where required | CH accounts, HMRC accounts, CT600 | S5, S8 |
| Companies House registered number | One | Required | Jurisdiction-sensitive format; do not parse as a number | CH accounts, HMRC accounts | S5 |
| Corporation Tax UTR | One | Required for HMRC return | 10-digit identifier validation without treating it as an arithmetic number | HMRC envelope/CT600 | S8 |
| Country of registration | One | Required in accounts | England and Wales, Wales, Scotland, or Northern Ireland as applicable | Accounts | S5 |
| Legal form | One | Required in accounts | Private; limited by shares or guarantee, as applicable | Accounts | S5 |
| Registered office address | One at approval date, with effective dating where needed | Required in accounts | Structured address plus presentation text | Accounts | S5 |
| Incorporation date | One | Required for period control | Used to derive first accounting reference date and first-filing deadlines | CH control | S4 |
| Accounting reference date | One effective value | Required for CH control | May be changed under statutory restrictions | CH control | S4 |
| Company winding-up status | One at accounts date | Conditional disclosure | Disclose when the company is being wound up | Accounts | S5 |
| Principal activity / trade description | One or more | Required for tax computation; useful accounts metadata | Must support separate trades where tax computations require them | HMRC computation | S7 |

The following are verified FRC 2026 v1.0.0 reference Names for core identity and filing-context facts. They are a minimum crosswalk, not an assertion that every identity or address disclosure is scalar. [S24]

| Semantic fact | Published FRC 2026 binding | Type / period semantics | Category Tree treatment |
|---|---|---|---|
| Legal company name | `bus:EntityCurrentLegalOrRegisteredName` | String / duration | Shared semantic fact; retain effective-dated former names separately |
| Companies House registered number | `bus:UKCompaniesHouseRegisteredNumber` | String / duration | Preserve leading jurisdiction letters and zeros |
| Accounts period start | `bus:StartDateForPeriodCoveredByReport` | Date / instant in taxonomy metadata | Bind to the accounts period, not a CT period |
| Accounts period end | `bus:EndDateForPeriodCoveredByReport` | Date / instant in taxonomy metadata | Bind to the accounts period |
| Balance-sheet date | `bus:BalanceSheetDate` | Date / instant | Must equal the current statement-of-financial-position context date |
| Principal report currency | `bus:PrincipalCurrencyUsedInBusinessReport` | Fixed-item / duration | Presentation metadata; monetary facts still require an XBRL unit |
| Accounting standard | `bus:AccountingStandardsDimension = bus:Micro-entities` | Dimension/member | A context classification, not a boolean value or ledger category |
| Accounts copy type | `bus:AccountsTypeDimension = bus:FullAccounts` or `bus:FilletedAccounts` | Dimension/member | Filing-projection choice; do not remove facts from the shared accounting model |
| Accounts status | `bus:AccountsStatusDimension` with applicable member, including `bus:AuditExempt-NoAccountantsReport` where correct | Dimension/member | Derived from the statutory audit assessment |
| Accounts authorisation date | `core:DateAuthorisationFinancialStatementsForIssue` | Date / instant | Board approval event, separate from submission date |
| Signing director | `core:DirectorSigningFinancialStatements` | Fixed-item / duration | Binding requires the corresponding director identity/name structure |

### Presentation, approval and sign-off

| Fact | Context/cardinality | Status | Rule | Consumers | Source |
|---|---|---|---|---|---|
| Presentation currency | One per accounts set | Required | Currency named in accounts; normally GBP | Both accounts projections | S5 |
| Rounding convention | One per accounts set and, if necessary, per statement | Required | State whether units, thousands, etc.; retain unrounded source values | Both accounts projections | S5, S19 |
| Accounts approval date | One | Required | Board approval date, not submission date | CH and HMRC accounts | S9 |
| Signing director name | One | Required | Printed name associated with the balance-sheet approval/signature | CH and HMRC accounts | S9 |
| Signing director identity/reference | One | Required internally | Must resolve to a director in office when approved; not a taxonomy identifier | Accounts generation | S9 |
| Auditor status | One per accounts set | Required | Audited, audit-exempt, or other supported state | Accounts and CH statements | S9 |
| Audit-exemption basis | One or more statutory bases | Conditional | Required when claiming exemption; section and statement wording are versioned law/registrar requirements | CH accounts and full accounts | S9 |
| Members' audit demand | Boolean/result for relevant deadline | Conditional | Audit exemption cannot be claimed if members validly require an audit under section 476 | CH accounts | S9 |
| CT600 declaration name, date and status | Exactly one per return | Required | Boxes 975, 980 and 985; declaration is separate from accounts approval | CT600 | S8 |

## Statutory accounts field set

FRS 105 requires a complete set of accounts comprising a statement of financial position with notes at its foot, an income statement and the required notes. Current and comparative statements are presented with equal prominence. Companies House may currently receive a reduced public copy, but that does not alter the full accounts prepared for members or attached to the HMRC return. [S5] [S9]

All statement-line values below are monetary decimals retained at full calculation precision. Their authoritative machine identifiers are the applicable QNames in the selected FRC taxonomy entry point. The reference bindings below were verified against the FRC 2026 v1.0.0 presentation sheet; they are an auditable starting crosswalk, not a substitute for entry-point and validator checks at filing time. [S12] [S24]

### Statement of financial position

FRS 105 permits either of the statutory formats. Store the underlying facts independently of the chosen presentation and retain `balance_sheet_format` as an explicit election.

| Statutory heading | FRC 2026 reference binding | Context | Status | Sign / calculation | Source |
|---|---|---|---|---|---|
| Called up share capital not paid | `core:CalledUpShareCapitalNotPaidNotExpressedAsCurrentAsset` | Current and comparative instant | Conditional; omit only when no current and no comparative amount | Asset amount; taxonomy type `monetaryItemType`, debit balance | S5, S10, S24 |
| Fixed assets | `core:FixedAssets` | Current and comparative instant | Conditional | Roll-up of applicable fixed-asset classes, net carrying amount; monetary/debit | S5, S24 |
| Current assets | `core:CurrentAssets` | Current and comparative instant | Conditional | Roll-up of inventory, debtors, cash and other applicable current assets; monetary/debit | S5, S24 |
| Prepayments and accrued income | `core:PrepaymentsAccruedIncomeNotExpressedWithinCurrentAssetSubtotal` when presented as the separate statutory line; `core:PrepaymentsAccruedIncome` when included within the current-assets analysis | Current and comparative instant | Conditional | Asset amount; presentation choice must agree with the statement roll-up | S5, S24 |
| Creditors: amounts falling due within one year | `core:Creditors` with `core:FinancialInstrumentCurrentNon-currentDimension = core:WithinOneYear` | Current and comparative instant | Conditional | Monetary/credit taxonomy balance; render liability magnitude rather than ledger sign | S5, S24 |
| Net current assets (liabilities) | `core:NetCurrentAssetsLiabilities` | Current and comparative instant | Derived | Current assets + separately presented prepayments/accrued income − creditors due within one year; monetary/debit taxonomy balance | S5, S24 |
| Total assets less current liabilities | `core:TotalAssetsLessCurrentLiabilities` | Current and comparative instant | Derived | Fixed assets + net current assets/liabilities + called-up capital unpaid, subject to selected format; monetary/debit | S5, S24 |
| Creditors: amounts falling due after more than one year | `core:Creditors` with `core:FinancialInstrumentCurrentNon-currentDimension = core:AfterOneYear` | Current and comparative instant | Conditional | Monetary/credit taxonomy balance; same base concept as the within-one-year line | S5, S24 |
| Provisions for liabilities | `core:ProvisionsForLiabilitiesBalanceSheetSubtotal` | Current and comparative instant | Conditional | Liability magnitude; monetary/credit | S5, S24 |
| Accruals and deferred income | `core:AccruedLiabilitiesDeferredIncome`; the taxonomy also provides `core:AccruedLiabilitiesNotExpressedWithinCreditorsSubtotal` for the narrower outside-creditors presentation | Current and comparative instant | Conditional | Select the concept that matches the rendered line and calculation tree; monetary/credit | S5, S24 |
| Capital and reserves | `core:Equity` | Current and comparative instant | Required as the residual/equity total | Reconciles to net assets; monetary/credit | S5, S24 |
| Total assets | `core:TotalAssets` | Current and comparative instant, format 2 | Derived | Sum of asset headings; monetary/debit | S5, S24 |
| Total capital, reserves and liabilities | Derived display total; the FRC 2026 balance-sheet presentation uses `core:TotalLiabilities` and `core:Equity` rather than exposing a distinct matching total concept | Current and comparative instant, format 2 | Derived | Sum of equity and liability headings; equals `core:TotalAssets`; do not invent a QName | S5, S24 |

A statutory heading can be omitted only if there is no amount for both the current and preceding financial year. A zero current amount does not by itself permit omission when a comparative exists. [S10]

### Income statement

| Statutory heading | FRC 2026 reference binding | Context | Status | Sign / calculation | Tax relationship | Source |
|---|---|---|---|---|---|---|
| Turnover | `core:TurnoverRevenue` | Current and comparative duration | Required when applicable | Monetary/credit; underlying credits must be normalised to the rendered value | Starting evidence for trade turnover, not necessarily CT600 taxable total | S5, S24 |
| Other income | `core:OtherOperatingIncomeFormat2` is the FRC 2026 format-2 reference concept | Current and comparative duration | Conditional | Monetary/credit; final concept selection must match the chosen rendered FRS 105 presentation | Must be classified by tax source before computation | S5, S7, S24 |
| Cost of raw materials and consumables | `core:RawMaterialsConsumablesUsed` | Current and comparative duration | Conditional | Monetary/debit | Accounting expense may require tax adjustments | S5, S7, S24 |
| Staff costs | `core:StaffCostsEmployeeBenefitsExpense` | Current and comparative duration | Conditional | Monetary/debit | Detailed computation may separate allowable and disallowable items | S5, S7, S24 |
| Depreciation and other amounts written off assets | `core:DepreciationAmortisationImpairmentExpense` is the FRC 2026 aggregate expense reference concept | Current and comparative duration | Conditional | Monetary/debit; retain lower-level book depreciation, amortisation and impairment categories beneath the statutory roll-up | Normally added back or otherwise adjusted in computing taxable trade profit; capital allowances are separate | S5, S7, S24 |
| Other charges | `core:OtherOperatingExpensesFormat2` is the FRC 2026 format-2 reference concept | Current and comparative duration | Conditional | Monetary/debit; requires detailed accounting children rather than one undifferentiated posting category | Requires tax classification; not a single tax deduction | S5, S7, S24 |
| Tax | `core:TaxTaxCreditOnProfitOrLossOnOrdinaryActivities` | Current and comparative duration | Conditional | Monetary/debit taxonomy balance; supports a tax credit through sign/presentation rules | Must not be equated with CT600 Corporation Tax liability or amount payable | S5, S8, S24 |
| Profit or loss | `core:ProfitLoss` | Current and comparative duration | Derived | Monetary/credit taxonomy balance; income less expenses and tax under the adopted format | Reconciles accounts, not directly the taxable total | S5, S7, S24 |

The minimum FRS 105 face does not remove the need for a detailed profit-and-loss analysis in the tax computation. HMRC expects the return entries to be linked to the accounts and, ordinarily, a detailed profit-and-loss account to support the computation. [S20]

### Notes and conditional disclosures

| Disclosure | Structure | Trigger / status | Required detail | Source |
|---|---|---|---|---|
| Average number of employees | One current and comparative integer | Required | Average employed during each period | S5 |
| Off-balance-sheet arrangements | Repeating narrative/amount records | Material arrangements | Nature and business purpose; information needed to assess financial impact | S5 |
| Total financial commitments, guarantees and contingencies not on balance sheet | Amounts plus narrative | Conditional | Total; pension commitments separately identified; group/participating-interest commitments separately identified | S5 |
| Valuable security | Narrative/structured security record | Conditional | Nature and form of security given in connection with commitments, guarantees or contingencies | S5 |
| Director advances and credits | Repeating per arrangement plus totals | Conditional | Amount, interest rate, main conditions, amounts repaid/written off/waived, and applicable totals | S5 |
| Guarantees entered into on behalf of directors | Repeating per guarantee plus totals | Conditional | Main terms, maximum liability, amounts paid and liability incurred, and applicable totals | S5 |
| Accounting policy and other taxonomy-supported facts | Narrative or structured | As required by FRS 105/law and selected presentation | Use the applicable taxonomy concept where one exists; otherwise permitted narrative treatment | S5, S12 |

The required employee note has the verified FRC 2026 reference binding `core:AverageNumberEmployeesDuringPeriod` (`nonNegativeDecimalItemType`, duration). The remaining notes should be mapped from their structured source records to the applicable concepts, dimensions and repeating structures in the selected entry point; they should not be reduced to free-form Category Tree leaves merely to obtain a scalar key. [S24]

FRS 105 permits an immaterial disclosure to be omitted unless company law requires it regardless of materiality. The system therefore needs both `materiality_assessment` and `statutory_disclosure_required` decisions; one must not be collapsed into the other. [S5]

Notes are not adequately represented by a flat tag/value list. Director arrangements, securities and commitments can repeat, have parties and dates, and combine amounts with narrative terms. They require structured source records before taxonomy mapping.

## Companies House-specific field set

### Current delivered copy

As at the research cut-off, a micro-entity prepares full accounts for members, including the income statement, but can normally deliver a copy to Companies House that omits the profit and loss account and directors' report. The delivered balance sheet and notes still require the applicable micro-entity and audit-exemption statements, board approval and director identification. HMRC's accounts attachment remains the full statutory accounts. [S9] [S11]

| Field / decision | Context | Status | Validation / wording ownership | Source |
|---|---|---|---|---|
| `accounts_type = micro_entity` | Filing | Required | Must agree with qualification evidence | S3, S9 |
| Delivered accounts period | Filing | Required | Must match company, accounting reference period and submission | S4 |
| Balance-sheet format | Accounts set | Required | Format 1 or 2, consistently rendered | S5 |
| Profit-and-loss omission election | Filing copy | Current optional route where eligible | Controls Companies House delivered copy only; does not remove HMRC/full-accounts facts | S9, S11 |
| Directors' report omission election | Filing copy | Current optional route where eligible | Applies only where law permits | S9 |
| Micro-entity delivery statement | Balance sheet, above signature | Required | Exact current wording: “The accounts have been prepared in accordance with the micro-entity provisions and have been delivered in accordance with the provisions applicable to companies subject to the small companies regime.” | S9 |
| Audit-exemption statements | Balance sheet | Conditional when exemption claimed | Current statements must cover section 477 entitlement, no section 476 member demand, directors' responsibilities and preparation under the small companies regime | S9 |
| Approval date | Balance sheet | Required | Board approval date | S9 |
| Director signature/name | Balance sheet | Required | A director signs; printed name displayed | S9 |
| Company authentication code | Submission envelope | Required for current software filing | Secret credential; never an accounts fact or persisted in a report | S13 |
| Presenter ID and authentication | Submission envelope | Required for XML Gateway route | Environment-specific credentials | S13 |
| Submission number / envelope identifiers | Per attempt | Required technical metadata | Unique per attempt; supports status polling and audit | S13 |
| Gateway test indicator | Per attempt | Required in test environment | Must not leak into production requests | S14 |

### Companies House filing surface

Current software accounts filing uses the Companies House XML Gateway, a GovTalk envelope and an iXBRL accounts document conforming to the applicable registrar schema and taxonomy rules. WebFiling remains available for eligible micro-entity accounts before the April 2028 transition. The registrar does not currently offer an accounts-filing REST API equivalent to the public company-data APIs. [S13] [S14]

The current XML Gateway service address published in the technical interface material is `https://xmlgw.companieshouse.gov.uk/v1-0/xmlgw/Gateway`; the envelope's gateway-test flag and assigned test credentials distinguish test traffic. The browser WebFiling route is a filing service, not an API endpoint. The applicable Technical Interface Specification version at the cut-off is 5.9, published for 1 April 2026. [S13] [S14]

The adapter must own:

- selection of the current Companies House schema, taxonomy and entry point;
- iXBRL rendering, contexts, dimensions, units and precision;
- the registrar XML base schema and GovTalk envelope;
- company authentication code and presenter credentials;
- test/live environment selection;
- pre-submission validation, submission status polling and immutable evidence of the response.

Companies House provides a test-account process and an iXBRL validator. Test submission requires test presenter credentials and the gateway test flag; a syntactically accepted test may then receive manual review. [S14]

### April 2028 transition

Companies House has announced that, from 1 April 2028, all companies must file accounts through commercial software in iXBRL. Web and paper accounts filing will close, abridged accounts will be removed, and the component parts of accounts must be filed together. Micro-entities and small companies will have to deliver a profit and loss account, with an option for it not to be made public; the detailed mechanism is still to be confirmed. Enhanced audit-exemption statements will also apply. [S15]

Model the 2028 change as a future contract version, not as a present rule. In particular, do not implement “profit and loss not public” as omission from the submitted document: the announced policy requires delivery and describes non-publication as a separate choice.

## HMRC Corporation Tax field set

### Submission package

A complete electronic Company Tax Return package consists of:

- one CT600 for one Corporation Tax accounting period;
- all applicable CT600 supplementary pages;
- the full company accounts in iXBRL;
- tax computations in iXBRL; and
- permitted supporting documents, normally PDF, where applicable.

The accounts and computations are separate iXBRL instance documents within the overall return. Receipt is not HMRC agreement with the return. Schema validation, CT business rules, XBRL checks and other validation rules can reject a package before acceptance. [S2] [S16]

At the cut-off, the current CT600 Version 3 2026 RIM artefacts are release 1.994 (version date 10 October 2025), the published generic validation rules are version 1.17a, and the current Local Test Service release is 8.3. These are technical-contract versions, not permanent business identifiers. [S17] [S22] [S23]

### Computation field groups

HMRC's November 2025 CT computations format prescribes accounts-adjustment and capital-allowance sections. It expressly anticipates further sections, so it is not yet a complete universal computation data model. A computation is prepared separately for each Corporation Tax accounting period. For long accounts, only the computation relevant to that return period accompanies each CT600 even though the accounts cover the longer financial year. [S7]

| Computation group | Key inputs / outputs | Cardinality and trigger | Sign / zero convention | Source |
|---|---|---|---|---|
| Trade identity and period | Trade description, commencement/cessation where relevant, CT accounting-period start/end | One or more trades per return | Dates DD/MM/YYYY in prescribed presentation | S7 |
| Accounting profit/loss by trade | Detailed income and expenses linked to accounts | Per trade | Accounting presentation normalised into computation direction | S7, S20 |
| Disallowable expenditure | Capital items, non-wholly-and-exclusively costs, depreciation, specified remuneration/professional/entertainment/fines/pension and other adjustments | Repeatable by category where present | Addbacks shown in the section's expected direction; brackets mean against it | S7 |
| Taxable income not credited in accounts | Taxable receipts omitted from accounting result | Conditional, by category | Adds to taxable result | S7 |
| Allowable deductions from accounting income | Receipts in accounts not taxable in the trade computation | Conditional, by category | Deducts from taxable result | S7 |
| Deductions not in accounts | Capital allowances, qualifying R&D and other statutory deductions | Conditional, by category | Deducts from taxable result | S7 |
| Adjusted trade profit/loss | Accounting result plus/minus adjustments | One per trade | Derived and reconciled | S7 |
| Capital-allowance summary | Allowances and balancing charges by regime | Conditional | Reconciles detailed pools/schedules to computation deductions/addbacks | S7 |
| Plant and machinery pools | Brought forward tax written-down value, additions, disposals, AIA/FYA/WDA, balancing adjustment, carried forward | Per applicable pool | Nil reported when an applicable pool/section produces zero; absent if the entire section is inapplicable | S7 |
| Structures and buildings allowance | Qualifying expenditure, allowance and carried-forward information | Conditional, repeatable by asset/allowance statement | Prescribed computation direction | S7 |
| Other allowance regimes | BPRA, mineral extraction, R&D allowances, know-how, patents and dredging | Conditional | Separate schedules; do not merge with book depreciation | S7 |
| Chargeable gains | Proceeds, allowable cost, indexation where applicable, gains/losses and claims | Conditional | Feeds CT600 chargeable gains group | S8 |
| Non-trading loan relationships and other sources | Income, deficits, management expenses, property income and other taxable sources | Conditional | Source-specific computation and relief rules | S8 |
| Losses and claims | Current/brought-forward losses, use, carry-forward, carry-back, group relief and surrender | Conditional, period- and source-specific | Never infer solely from negative accounting profit | S8 |

Within a present section, required numeric results use zero rather than a null value. If the entire section or pool is inapplicable it is omitted. Amounts are in sterling unless an applicable exemption permits otherwise. The signed-off computation is the authoritative tax calculation; calculations must not depend on display-rounded accounts amounts. [S7]

### CT600 box groups

The current form at the research cut-off is CT600 (2026) Version 3. The box number is the durable human cross-reference for that release; the executable contract is the matching CT600 Return Information Model (RIM), currently published as the 2026 artefacts. [S8] [S17]

| CT600 group | Boxes | Typical content | Status / source of value |
|---|---:|---|---|
| Company and return identity | 1–8 | Company name, tax reference and Northern Ireland indicators | Required/conditional identity facts |
| Return context | 30–75 | Accounting period, return type and situation indicators | Required with conditional flags |
| Attached documents and supplementary pages | 80–144 | Accounts/computation attachments and CT600A–N/P presence indicators | Derived from package and conditional-page triggers |
| Income and gains | 145–235 | Trading profits, non-trading income, property, gains and other sources | Derived from tax computations, not directly from accounts headings |
| Deductions and reliefs | 240–325 | Losses, management expenses, donations and reliefs | Derived from claims and computations |
| Associated companies, rates and tax calculation | 326–440 | Associated-company counts, augmented profits, rates and calculated tax | Required/conditional; formula-controlled |
| Reliefs and tax liability | 445–528; 986–987 | Reliefs against tax, liability and energy-profits-levy entries | Derived/conditional |
| Credits and reconciliation | 530–615 | Tax deducted, payable, paid and repayment calculation | Derived plus external payment/credit facts |
| Indicators and export matters | 616–647 | Tax/accounting and special-situation indicators | Conditional |
| Enhanced expenditure and R&D | 650–685 | Enhanced expenditure and land-remediation/R&D amounts | Conditional; may require supplementary pages |
| Capital allowances and qualifying expenditure | 688–775 | Allowances, balancing charges and expenditure | Conditional; reconciles to allowance schedules |
| Losses and excess amounts | 780–858 | Losses carried forward/back and excess amounts | Conditional, period/source specific |
| Repayments and surrender | 860–915 | Repayment claims, recipient and surrender information | Conditional election/claim fields |
| Bank and nominee details | 920–970 | Repayment bank/nominee details | Conditional; sensitive payment data |
| Declaration | 975–985 | Signer name, date and status | Required; explicit approval event |

CT600 supplementary pages are conditional, not part of every micro-company return:

| Page | Trigger category |
|---|---|
| CT600A | Close-company loans and arrangements conferring benefits on participators |
| CT600B | Controlled foreign companies |
| CT600C | Group and consortium relief |
| CT600D | Insurance |
| CT600E | Charities and community amateur sports clubs |
| CT600F | Tonnage tax |
| CT600G | Corporate interest restriction |
| CT600H | Cross-border royalty payments |
| CT600I | Supplementary charge in respect of ring-fence trades |
| CT600J | Disclosure of tax avoidance schemes |
| CT600K | Restitution tax |
| CT600L | Research and development |
| CT600M | Freeports and investment zones |
| CT600N | Residential property developer tax |
| CT600P | Energy profits levy |

The adapter must use the current form, guide and RIM to determine exact triggers and validation. A company being “micro” does not itself suppress any page; the underlying transaction or status determines applicability.

### CT600 signs, units and roll-ups

- CT600 monetary boxes generally hold positive magnitudes and the form's printed/RIM formula determines addition or subtraction. Do not send ledger debit/credit signs or accounting presentation signs unchanged.
- Use whole pounds unless the current box or supplementary-page instruction expressly permits pence or another precision. Do not round the underlying CT600 arithmetic merely because the attached accounts are displayed in thousands. iXBRL scaling/decimals must describe the tagged value faithfully. [S18] [S19] [S20]
- The accounting tax charge is an accrual under accounting rules. CT600 liability, tax payable after credits and repayment due are different facts with explicit reconciliations.
- Each source of income, deduction, loss and relief must retain its tax character and Corporation Tax accounting-period context. A single net profit/loss is insufficient.
- Associated-company counts, rates and marginal-relief inputs can require externally supplied group/legal facts; they cannot safely be derived from the ledger.

## iXBRL, taxonomy and contexts

### Accepted taxonomies

HMRC's accepted-taxonomies list is versioned and date-bounded. As at 17 April 2026 it accepts the FRC 2025 and 2026 accounts taxonomies for relevant periods, while older releases have stated end dates; the CT computational taxonomy 2025 is the current listed computation taxonomy. Always select a taxonomy that HMRC accepts for the accounts/return period on the actual submission date. [S12]

| Document | Current listed taxonomy at cut-off | Published period boundary |
|---|---|---|
| Accounts | FRC 2026 UK and Irish taxonomy | Accepted for periods starting on or after 1 April 2015; end date to be announced |
| Accounts | FRC 2025 UK and Irish taxonomy | Accepted for periods starting on or after 1 April 2015; end date to be announced |
| Accounts, older supported release | FRC 2024 UK taxonomy | Accepted only where the accounts period ends by 31 March 2027 |
| Corporation Tax computations | CT computational taxonomy 2025 | Accepted for periods starting on or after 1 April 2015; end date to be announced |

Those broad HMRC acceptance dates do not override the accounting standard effective for the company's period. They only describe the electronic service's taxonomy acceptance window. Companies House software filing also uses registrar-accepted FRC accounts taxonomy releases, but adds registrar schema/envelope rules and may not apply an identical version window. [S12] [S13]

FRC taxonomy “Names” are the unique machine identifiers; labels are human-readable and may change or repeat. A production mapping therefore records at least:

- authority and document type;
- taxonomy family, release and exact entry point;
- concept QName/Name, not label alone;
- context type (instant or duration);
- dimensions and members, if any;
- unit and scaling/decimals;
- calculation/presentation role where relevant;
- applicability dates and validation provenance.

This reference intentionally does not guess concept QNames. They must be resolved and tested against the selected FRC/HMRC/Companies House taxonomy package at implementation time.

### Context rules

| Fact class | Required context behaviour |
|---|---|
| Balance-sheet facts | Instant at current and comparative statement dates |
| Income-statement facts | Duration for current and comparative reporting periods |
| Corporation Tax computations | Duration for the specific CT accounting period, not automatically the accounts period |
| Company identity | Entity identifier and the context type required by the selected concept |
| Repeating/dimensional disclosures | Explicit dimensions/members or tuple/repeating structure supported by the taxonomy release |
| Monetary facts | ISO currency unit plus taxonomy-appropriate decimals/precision; display rounding remains separate |
| Counts | Pure/integer unit and appropriate duration/instant context |

HMRC expects all data on the balance sheet, profit and loss account and notes to be tagged where a suitable taxonomy tag exists, including comparative figures. Reports and narrative are tagged where supported; content without a suitable tag remains human-readable text as permitted by the technical rules. [S11]

## Submission contracts and testing

### Companies House

| Concern | Current contract |
|---|---|
| Production transport | Companies House XML Gateway at `https://xmlgw.companieshouse.gov.uk/v1-0/xmlgw/Gateway`, using the Technical Interface Specification and GovTalk envelope |
| Payload | Registrar-compliant iXBRL accounts plus required XML metadata |
| Authentication | Presenter credentials and company authentication code |
| Pre-flight | Official iXBRL validator plus local schema/taxonomy/business-rule validation |
| Test path | Companies House software-developer test account, test presenter credentials and gateway test flag |
| Result handling | Poll submission status; retain every acknowledgement/rejection and the exact submitted artefact |

### HMRC

| Concern | Current contract |
|---|---|
| Production transport | HMRC Transaction Engine over HTTP at `https://transaction-engine.tax.service.gov.uk/submission`, using the Corporation Tax online technical specification; this is not an MTD REST API [S21] |
| Payload | CT600/RIM XML, conditional supplementary pages, separate accounts and computations iXBRL instances, and permitted supporting PDFs |
| Authentication | HMRC online-service credentials and submission-envelope identifiers |
| Pre-flight | Current schemas, generic validations, CT business rules, accepted taxonomies and iXBRL validation |
| Test paths | HMRC Local Test Service for local conformance; External Test Service at `https://test-transaction-engine.tax.service.gov.uk/submission`; and TPVS body validation at `https://www.tpvs.hmrc.gov.uk/HMRC/CT600`, where specified by the current pack [S21] [S23] |
| Result handling | Poll where required; retain correlation identifiers, acknowledgement, errors and the immutable package |

For both authorities, an integration test fixture needs enough data to exercise:

1. a straightforward first-year audit-exempt micro-company;
2. current and comparative accounts;
3. a long first accounts period split into two CT accounting periods;
4. a zero-current/non-zero-comparative line that must remain present;
5. accounting depreciation added back with capital allowances claimed separately;
6. a director loan requiring both an accounts note and CT600A consideration;
7. a loss and relief claim;
8. rejection caused by schema/business-rule failure and successful resubmission;
9. test/live credential isolation; and
10. a taxonomy release rollover without changing the underlying accounting facts.

Acceptance should assert not only gateway success but also rendered-document review, cross-statement arithmetic, current/comparative context accuracy, CT600-to-computation reconciliation, and a stored authority acknowledgement.

## Historical AC and CP identifiers

Phase 1A material referred to identifiers including `AC12`, `AC405`, `AC410`, `AC415`, `AC420`, `AC425`, `AC34`, `AC435`, `CP28` and `CP46`. These are historical online-service/screen identifiers, not current statutory or technical contract identifiers.

The economic concepts may survive. For example, the `AC` examples broadly correspond to income-statement headings, while the `CP` examples represent computation items such as depreciation and its tax adjustment. The identifiers themselves must not survive as canonical keys because:

- the joint online accounts and Company Tax Return service to which such screen fields belonged closed on 31 March 2026; [S1]
- current accounts facts are identified by versioned taxonomy QNames/Names and contexts; [S12]
- current CT600 data is identified by form boxes and RIM elements; [S8] [S17]
- current tax computations use the accepted computational taxonomy and prescribed computation structure. [S7] [S12]

This is a contract-inspection conclusion: no reviewed current official filing specification uses `AC…` or `CP…` as the public machine contract. Preserve them only in migration documentation if old records need interpretation. Map old data to semantic facts first, then map those facts to the current authority contract.

## Distinct cases outside the initial target

A dormant company is a separate statutory/tax state, not a normal trading micro-company with zero transactions. Companies House has a dormant-accounts route, while HMRC can treat a company as dormant for Corporation Tax so that a return is not normally required unless HMRC issues a notice or the company becomes active. Group, charity, insurance, ring-fence, R&D and other specialist cases likewise activate different disclosures or supplementary pages. [S2] [S3]

These boundaries should be represented as applicability decisions, but this phase does not specify their implementations or broaden the initial target beyond a normal trading private micro-entity.

## Minimum Trade Control supply boundary

Before a filing adapter can generate either submission reliably, Trade Control needs to supply or obtain the following semantic facts:

| Domain | Minimum supply |
|---|---|
| Company | Legal name/history, registered number, UTR, jurisdiction, legal form, registered office, incorporation date and accounting reference date |
| Periods | Financial year, comparative year, every mapped CT accounting period, trade commencement/cessation dates and context mapping |
| Eligibility | Current/prior threshold measurements, employee average, exclusion tests, group facts, micro conclusion and applicable FRS 105 edition |
| Ledger and accounts | Current/prior trial-balance facts, chosen statutory formats, statement roll-ups, detailed profit-and-loss analysis, currency and unrounded amounts |
| Assets and allowances | Fixed-asset register, book depreciation, additions/disposals, tax-pool classifications, elections and allowance schedules |
| Tax adjustments | Allowability classifications, non-accounting income/deductions, property/loan-relationship/gains facts, donations, R&D and other claims |
| Losses and group facts | Losses by source and period, brought-forward balances, intended claims/surrenders, associated-company and group-relief information |
| Notes | Employee average, director advances/credits, director guarantees, commitments, contingencies, securities and other required narrative facts |
| Governance | Accounts approval, signing director, audit status/basis, member audit demand, CT declaration actor/date/status and filing elections |
| External amounts | Tax already paid/deducted, credits, repayment instructions, nominee/bank details and any values not present in the accounting ledger |
| Conditional features | A structured applicability assessment for every CT600 supplementary page and special filing regime |

Trade Control can deterministically calculate accounting roll-ups and provide well-defined computation inputs. It should not own authority-specific QNames, schema versions, iXBRL layout, gateway envelopes, credentials or transient validation rules. Those belong in versioned filing adapters.

A flat `tag -> value` boundary is insufficient. The supply contract must support current/comparative and instant/duration contexts, multiple trades and CT periods, repeating schedules, dimensions, narrative-plus-amount disclosures, provenance, review decisions and unrounded values.

## Required controls and invariants

- One accounts set may map to one or more CT600 returns; every CT600 maps to exactly one CT accounting period.
- Current and comparative facts remain separate even when their displayed label is identical.
- All calculated statement totals reconcile before either authority projection is produced.
- Capital and reserves reconcile to net assets/total assets less liabilities under the selected balance-sheet format.
- The income-statement result reconciles to retained earnings movements after dividends and other equity movements, where applicable.
- The tax computation starts from traceable accounting results and records every adjustment; it never overwrites the accounts result.
- Book depreciation and tax capital allowances remain distinct.
- Accounting tax charge, CT600 tax liability, net tax payable and repayment due remain distinct.
- Omitted, not applicable, nil and unknown are different states.
- Display rounding never changes the unrounded calculation or return amounts.
- Authority credentials and bank data are secrets/sensitive fields, excluded from ordinary reports and diagnostic logs.
- Every generated artefact records the contract/taxonomy versions and a content hash, and every submission attempt records its environment and acknowledgement.
- A successful transport acknowledgement is not a substantive agreement by HMRC or Companies House.

## Open items and change watch

These matters were not sufficiently final at the research cut-off and must be rechecked before implementation or release:

1. the detailed April 2028 Companies House profit-and-loss non-publication election, final regulations, schemas and TIS;
2. the exact enhanced audit-exemption machine-readable statements for April 2028;
3. the remaining standardised HMRC tax-computation sections beyond accounts adjustments and capital allowances, which HMRC says are to follow;
4. the taxonomy release and entry point accepted on the actual filing date, including mandatory-tag and calculation rules;
5. the effect of FRS 105's adapted-format amendments for periods beginning on or after 1 January 2027 on the chosen accounts presentation;
6. CT600/RIM changes announced for April 2027, including the 40% first-year allowance; and
7. Companies House presenter identity-verification requirements and implementation timetable, currently planned no earlier than November 2027.

No dependable timetable was found for replacing the Companies House XML Gateway accounts service with a REST/JSON accounts-filing API. Treat such a replacement as an unconfirmed future possibility, not a design assumption.

## Official sources

Sources were checked against the law, standards and filing material available by 1 September 2026. The applicable version must be checked again at implementation and submission time.

- **S1 — Service closure:** [The online accounts and Company Tax Return service is closing](https://www.gov.uk/government/news/the-online-accounts-and-company-tax-return-service-is-closing), HMRC and Companies House, 6 January 2026.
- **S2 — HMRC return obligations:** [Company Tax Return obligations](https://www.gov.uk/guidance/company-tax-return-obligations), HMRC, updated 1 June 2026.
- **S3 — Micro-entity eligibility:** [Life of a company: Part 1, accounts](https://www.gov.uk/government/publications/life-of-a-company-annual-requirements/life-of-a-company-part-1-accounts), Companies House, updated 30 June 2026.
- **S4 — Accounts periods and deadlines:** [Accounts and tax returns for private limited companies](https://www.gov.uk/prepare-file-annual-accounts-for-limited-company) and [First company accounts and return](https://www.gov.uk/first-company-accounts-and-return), GOV.UK.
- **S5 — Accounting standard:** [FRS 105: The Financial Reporting Standard applicable to the Micro-entities Regime](https://www.frc.org.uk/library/standards-codes-policy/accounting-and-reporting/uk-accounting-standards/frs-105/), Financial Reporting Council, September 2024 edition and published amendments.
- **S6 — Corporation Tax accounting periods:** [Corporation Tax accounting periods](https://www.gov.uk/corporation-tax-accounting-period), HMRC.
- **S7 — Computation specification:** [CT computations format, version 1.1](https://assets.publishing.service.gov.uk/media/691c7cd40dcbf6343e9a2999/ct-computations-format-version-1.1.pdf), HMRC, November 2025.
- **S8 — Current return:** [Company Tax Return CT600 (2026) Version 3](https://assets.publishing.service.gov.uk/media/69c543424a06660f085442bd/ct600.pdf) and [Company Tax Return guide](https://www.gov.uk/guidance/the-company-tax-return-guide), HMRC.
- **S9 — Micro accounts and audit statements:** [Companies House accounts guidance](https://www.gov.uk/government/publications/life-of-a-company-annual-requirements/life-of-a-company-part-1-accounts), sections on micro-entities, approval and audit exemption.
- **S10 — Statutory formats:** [The Small Companies and Groups (Accounts and Directors' Report) Regulations 2008](https://www.legislation.gov.uk/uksi/2008/409/pdfs/uksi_20080409_en.pdf), as amended, and [The Small Companies (Micro-Entities' Accounts) Regulations 2013](https://www.legislation.gov.uk/uksi/2013/3008/pdfs/uksi_20133008_en.pdf).
- **S11 — iXBRL accounts content:** [XBRL guide for UK businesses](https://www.gov.uk/government/publications/xbrl-guide-for-uk-businesses/xbrl-guide-for-uk-businesses), HMRC.
- **S12 — Taxonomies:** [Taxonomies accepted by HMRC](https://www.gov.uk/government/publications/taxonomies-accepted-by-hm-revenue-and-customs/taxonomies-accepted-by-hmrc) and [2026 UK and Irish digital reporting taxonomies](https://www.frc.org.uk/library/standards-codes-policy/accounting-and-reporting/frc-taxonomies/current-uk-and-irish-digital-reporting-taxonomies/2026-uk-and-irish-digital-reporting-taxonomies/).
- **S13 — Companies House software filing:** [Using software to file your company's information](https://www.gov.uk/guidance/using-software-to-file-your-companys-information) and [Technical Interface Specifications](https://www.gov.uk/government/publications/technical-interface-specifications-for-companies-house-software), Companies House.
- **S14 — Companies House testing:** [Important information for software developers — read first](https://www.gov.uk/government/publications/technical-interface-specifications-for-companies-house-software/important-information-for-software-developers-read-first) and [Companies House iXBRL validator](https://test-validator.companieshouse.gov.uk/xbrl_validate).
- **S15 — 2028 filing changes:** [Companies House to bring in changes to accounts filing from April 2028](https://www.gov.uk/government/news/companies-house-to-bring-in-changes-to-accounts-filing-from-april-2028) and [ECCTA outline transition plan](https://www.gov.uk/government/publications/economic-crime-and-corporate-transparency-act-outline-transition-plan-for-companies-house/economic-crime-and-corporate-transparency-act-outline-transition-plan-for-companies-house), Companies House.
- **S16 — Electronic return direction and specifications:** [Income and Corporation Taxes electronic communications direction](https://www.gov.uk/government/publications/directions-under-regulations-3-and-10-of-the-income-and-corporation-taxes-electronic-communications-regulations-2003-si-2003282/income-and-corporation-taxes-electronic-communications-direction) and [Corporation Tax technical specifications](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-xbrl-and-ixbrl), HMRC.
- **S17 — CT600 executable artefacts:** [CT600 RIM artefacts](https://www.gov.uk/government/publications/corporation-tax-technical-specifications-ct600-rim-artefacts), HMRC, 2026 Version 3 release.
- **S18 — Supplementary-page amount conventions:** [CT600A supplementary pages guidance](https://www.gov.uk/guidance/supplementary-pages-ct600a-2015-version-3-close-company-loans-and-arrangements-to-confer-benefits-on-participators), HMRC.
- **S19 — XBRL precision and presentation:** [XBRL tagging style guide](https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/434588/xbrl-style-guide.pdf), HMRC.
- **S20 — Computation detail and rounding:** [COTAX manual COM130040: computations and accounts](https://www.gov.uk/hmrc-internal-manuals/cotax-manual/com130040), HMRC.
- **S21 — HMRC XML service mechanics:** [Basic guide for XML software developers](https://www.gov.uk/guidance/basic-guide-for-xml-software-developers) and the [Corporation Tax online support collection](https://www.gov.uk/government/collections/corporation-tax-online-support-for-software-developers), HMRC.
- **S22 — HMRC generic validations:** [Corporation Tax generic technical specifications](https://www.gov.uk/government/publications/corporation-tax-generic-technical-specifications), HMRC.
- **S23 — HMRC test tooling:** [Local Test Service and update manager](https://www.gov.uk/government/publications/local-test-service-and-lts-update-manager), HMRC, release 8.3.
- **S24 — FRC machine Names and taxonomy hierarchy:** [2026 FRC Taxonomy Suite presentation sheets](https://www.frc.org.uk/documents/8900/Presentation_Sheets_for_2026_Taxonomies.zip), Financial Reporting Council, v1.0.0, published 18 November 2025. The published `Prefix`, `Name`, `Type`, `Period Type`, `Balance`, hypercube and dimension-member columns were used for the reference bindings in this document.
