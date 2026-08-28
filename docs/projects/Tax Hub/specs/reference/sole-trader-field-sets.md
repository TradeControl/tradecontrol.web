# Sole Trader Statutory Projection Field Sets

Trade Control Tax Hub Programme  
MTD Income Tax — 2026 Edition  
28 August 2026

## 1. Purpose

This document defines the statutory field concepts required by the Trade Control Tax Tag projection for a Sole Trader Business Node using Making Tax Digital for Income Tax.

It replaces the former:

`sole-trader-field-sets.md`

which modelled both:

- legacy SA100 / SA103F; and
- MTD Quarterly Update + EOPS.

That architecture is obsolete.

Trade Control supports Sole Trader Self Assessment submission through **Making Tax Digital for Income Tax only**.

This document is an **Objective 2 statutory projection reference**.

It does not define HMRC wire DTOs.

---

## 2. Architectural Boundary

The relevant programme boundary is:

**Trade Control accounting**  
→ **Tax Source / Tax Tag statutory projection — Objective 2**  
→ **HMRC contract adapter — Objective 3**  
→ **HMRC transport — Objective 4**

The field sets in this document therefore describe statutory concepts that Trade Control may need to expose from its accounting system.

They are not necessarily one-to-one copies of HMRC JSON properties.

An HMRC API may contain:

- required accounting values;
- optional accounting values;
- contextual data;
- identifiers;
- dates;
- derived values;
- values supplied elsewhere in the workflow;
- values that do not originate in Trade Control accounting.

Only information that legitimately belongs to the Trade Control statutory projection should become a Tax Tag.

---

## 3. Product Scope

### 3.1 Supported

Trade Control supports:

**Making Tax Digital for Income Tax — Self Employment**

including:

- periodic self-employment accounting information;
- annual self-employment adjustments and allowances where Trade Control can supply them;
- information required to support the current MTD finalisation process.

### 3.2 Not Supported

Trade Control does not support:

- SA100 XML submission;
- SA103F submission;
- a separate legacy Self Assessment statutory vocabulary;
- EOPS as a filing stage.

The following historical SQL concepts are therefore obsolete as supported statutory targets:

- `UK-SA-SE-RETURN`
- `UK-ITSA-SE-EOPS`

Their existing implementations remain historical evidence only until formally retired.

---

## 4. Tax Tag Principles

### 4.1 Tax Tags Are Not the Category Tree

The Trade Control Category Tree is an operational accounting classification.

It is configurable and may differ between businesses.

MIN and STD are bootstrap accounting templates, not statutory taxonomies.

Tax Tags form a separate statutory projection over available:

- CategoryCodes;
- CashCodes;
- derived accounting values;
- approved contextual sources.

### 4.2 Deterministic Mapping

A Tax Tag may be mapped only where its statutory meaning can be obtained deterministically from the accounting classification.

A similar label is not sufficient evidence.

### 4.3 MIN and STD May Differ

MIN intentionally provides coarse accounting classification.

STD provides greater detail.

Therefore:

- MIN may support fewer statutory distinctions;
- STD may support additional statutory distinctions;
- unsupported MIN detail must not be manufactured by allocating or estimating portions of broader totals.

### 4.4 Absence Is Valid

A field may legitimately be:

- unsupported;
- not applicable;
- contextual;
- externally supplied;
- derived elsewhere;
- optional and absent.

Absence must not automatically become zero.

---

# 5. Quarterly Self-Employment Accounting Field Set

## 5.1 Purpose

MTD quarterly reporting provides cumulative self-employment accounting information from the beginning of the tax year to the end of the applicable update period.

At the statutory accounting level, the core field set comprises:

- 2 income totals;
- 13 expense totals.

These are the primary accounting Tax Tag candidates for the Quarterly Update projection.

---

## 5.2 Income

| Proposed Tax Tag | Statutory Concept |
|---|---|
| `turnover` | Turnover |
| `otherBusinessIncome` | Other business income |

These values represent business accounting income.

They must be derived from approved Trade Control accounting sources.

---

## 5.3 Expenses

| Proposed Tax Tag | Statutory Concept |
|---|---|
| `costOfGoods` | Cost of goods bought for resale or goods used |
| `cisPaymentsToSubcontractors` | Construction industry payments to subcontractors |
| `wagesSalariesStaffCosts` | Wages, salaries and other staff costs |
| `carVanTravelExpenses` | Car, van and travel expenses |
| `rentRatesPowerInsurance` | Rent, rates, power and insurance costs |
| `repairsMaintenance` | Repairs and maintenance |
| `phoneFaxStationeryOfficeCosts` | Phone, fax, stationery and other office costs |
| `advertising` | Advertising |
| `businessEntertainment` | Business entertainment costs |
| `interestOnBankOtherLoans` | Interest on bank and other loans |
| `bankCreditCardFinancialCharges` | Bank, credit card and other financial charges |
| `accountancyLegalProfessionalFees` | Accountancy, legal and other professional fees |
| `otherBusinessExpenses` | Other business expenses |

These are statutory accounting classifications.

They are not permission to split a Trade Control accounting value artificially merely because the statutory model contains a more detailed field.

---

## 5.4 Quarterly Field Count

The core statutory quarterly accounting field set therefore contains:

**15 monetary totals**

comprising:

**2 income + 13 expense**

The existing SQL MTD Quarterly Update seed also contains 15 monetary fields.

That numerical agreement is useful evidence of earlier design intent, but it does not by itself validate existing SQL tag names or mappings.

Every retained tag must be reconciled semantically.

---

# 6. Additional HMRC Self-Employment API Properties

The current HMRC Self Employment Business API exposes a richer request structure than the core quarterly statutory accounting set.

That API structure includes additional concepts such as:

- tax deducted from trading income;
- consolidated expenses;
- irrecoverable debts;
- depreciation;
- disallowable expense values;
- other optional or context-dependent properties.

The existence of an API property does **not** automatically require a corresponding mandatory Tax Tag.

For Objective 2 each such property must first be classified as one of:

1. operational accounting value;
2. statutory adjustment;
3. contextual/workflow information;
4. derived value;
5. optional accounting detail;
6. externally supplied value;
7. unsupported value.

Only then may a Tax Tag be approved.

This distinction prevents the SQL taxonomy from becoming a mechanical copy of an HMRC DTO.

---

# 7. Annual Self-Employment Statutory Information

## 7.1 No EOPS Field Set

There is no current Trade Control EOPS Tax Tag set.

The historical model:

`UK-ITSA-SE-EOPS`

must not be preserved merely by renaming it.

The former EOPS grouping mixed several distinct statutory concepts:

- adjustments;
- allowances;
- losses;
- basis information;
- derived totals.

Those concepts must now be considered individually against the current MTD architecture.

---

## 7.2 Adjustments

Annual self-employment processing may require adjustments to the accounting result.

Potential adjustment concepts must be admitted into the Tax Tag vocabulary only where:

- the current statutory process requires them; and
- Trade Control has a legitimate deterministic or contextual source.

Adjustment values are not automatically accounting Category Tree totals.

Examples may include:

- private-use adjustments;
- accounting adjustments;
- balancing adjustments;
- other statutory adjustments required by the current MTD Self Employment contract.

The exact supported adjustment vocabulary is to be established by the contract-aligned mapping reconnaissance required by the Self Assessment SQL Node Specification Revision 3.

No historical EOPS adjustment tag is canonical merely because it already exists.

---

## 7.3 Capital Allowances

Capital allowances are statutory tax concepts and must not be confused with accounting depreciation.

Where current MTD reporting requires capital-allowance information, Trade Control must classify each value according to its legitimate source.

A capital allowance Tax Tag may be:

- derived by an approved Trade Control tax calculation;
- entered through statutory workflow context;
- supplied by another authoritative subsystem;
- unsupported.

Accounting depreciation must not be mapped directly to capital allowances merely to populate a field.

---

## 7.4 Losses

Losses do not belong to an EOPS field set.

Current MTD architecture treats loss information through the relevant dedicated statutory process.

Trade Control may expose accounting information required to support loss handling where that information can be determined legitimately.

Loss creation, use, carry-forward and similar concepts must not be recreated from historical SA103F or EOPS tags without current statutory verification.

The Objective 2 Tax Tag model must distinguish between:

- accounting loss derived from business results;
- statutory loss;
- loss claims or elections;
- losses brought forward;
- losses used;
- losses remaining.

These are not assumed to be interchangeable.

---

# 8. Period and Context Information

Not every statutory property belongs in `Cash.tbTaxTag`.

Examples include:

- tax year;
- period start;
- period end;
- business identifier;
- NINO;
- calculation identifier;
- submission identifier;
- declaration type;
- workflow status.

Such values generally belong to:

- Business Node configuration;
- Tax Hub workflow context;
- Objective 3 contract construction;
- submission history.

They should become Tax Tags only where there is a specific approved architectural reason.

---

# 9. Personal Income Tax Liability

The Sole Trader Tax Tag projection does not calculate definitive personal Income Tax.

Trade Control deterministically establishes:

- business transactions;
- accounting classifications;
- business profit;
- supported statutory self-employment values.

Personal Income Tax may depend upon information outside the Business Node.

Trade Control may therefore maintain an estimated Income Tax provision for forecasting and production scheduling.

Once the submission process produces an authoritative tax liability, the difference between estimated and actual liability may be processed through the existing period-adjustment mechanism.

That liability is workflow feedback.

It is not part of the self-employment accounting Tax Tag field set.

---

# 10. Mapping Classification

Each approved Tax Tag must be assigned one of the following source classifications:

| Classification | Meaning |
|---|---|
| `CategoryCode` | Complete Category total has the exact statutory meaning |
| `CashCode` | Individual Cash Code provides the required statutory distinction |
| `Derived` | Deterministically calculated from approved accounting values |
| `Contextual` | Supplied by Tax Hub, user workflow or Business Node configuration |
| `External` | Supplied by an authoritative external process |
| `OptionalAbsent` | Statutory field may legitimately be omitted |
| `Unsupported` | Trade Control cannot currently supply the statutory concept deterministically |

The exact stored representation of these classifications is an implementation concern.

This table defines their semantic meaning.

---

# 11. Mapping Rules

For every Tax Tag:

1. establish the current statutory meaning;
2. identify whether Trade Control should supply the value;
3. identify its correct source classification;
4. inspect MIN and STD independently;
5. identify the proposed CategoryCode or CashCode where applicable;
6. trace category ancestry;
7. check for overlapping parent/child mappings;
8. verify that no additive amount can be counted twice;
9. identify unsupported distinctions explicitly;
10. obtain review approval before SQL insertion.

Historical mappings may be cited as evidence.

They are never authority.

---

# 12. Quarterly Projection — Canonical Objective 2 Starting Set

Subject to Phase 3 semantic reconciliation, the canonical starting set for the Trade Control MTD Quarterly Self-Employment projection is:

```text
turnover
otherBusinessIncome

costOfGoods
cisPaymentsToSubcontractors
wagesSalariesStaffCosts
carVanTravelExpenses
rentRatesPowerInsurance
repairsMaintenance
phoneFaxStationeryOfficeCosts
advertising
businessEntertainment
interestOnBankOtherLoans
bankCreditCardFinancialCharges
accountancyLegalProfessionalFees
otherBusinessExpenses
```

This is a statutory **projection vocabulary**.

It is not an HMRC wire DTO.

Names may be adjusted during Phase 3 if the approved Trade Control Tax Tag naming convention requires it, provided their statutory semantics remain explicit and traceable.

---

# 13. Annual Projection — Deliberately Not Frozen

No canonical annual Tax Tag list is declared by this reference yet.

That is intentional.

The previous document failed by treating an historical filing structure as a future-proof statutory taxonomy.

Revision 3 of the Self Assessment SQL Node Specification therefore requires Phase 3 reconnaissance to establish:

- current annual adjustment concepts;
- current capital-allowance concepts;
- current loss responsibilities;
- which values belong in Objective 2;
- which belong in Objective 3 or workflow context;
- which MIN can support;
- which STD can support;
- which Trade Control does not support.

Only approved findings from that work should become a canonical annual Tax Tag set.

This reference must then be amended from verified evidence rather than anticipated API structure.

---

# 14. Explicitly Retired Concepts

The following must not be treated as current canonical Sole Trader field sets:

```text
SA100
SA103F
UK-SA-SE-RETURN

EOPS
UK-ITSA-SE-EOPS
```

Individual statutory concepts that once appeared beneath those structures may survive only after independent verification against the current MTD architecture.

Their historical container provides no authority.

---

# 15. Relationship to MIN and STD

This reference defines statutory meaning.

It does not assert that every statutory concept is supported by every Trade Control accounting template.

The Phase 3 mapping matrix must determine:

| Statutory Concept | MIN | STD |
|---|---|---|
| Deterministically supported | Map | Map |
| Supported only by detailed classification | Unsupported | Map |
| Requires contextual value | Contextual | Contextual |
| Derived elsewhere | Derived | Derived |
| Not available | Unsupported | Unsupported |

This is expected behaviour.

A valid statutory projection is preferable to a falsely complete one.

---

# 16. Authority and Maintenance

This reference is governed by the Tax Hub Programme Specification and the Self Assessment SQL Node Implementation Specification.

For externally governed statutory meaning, current authoritative HMRC specifications take precedence over:

- this document;
- SQL Tax Tag seeds;
- C# classes;
- historical payloads;
- test harnesses;
- previous implementation notes.

When HMRC changes a statutory contract, the correct process is:

**verify external change**  
→ **assess Objective 2 projection impact**  
→ **amend this reference where necessary**  
→ **amend mapping specification**  
→ **implement through an approved phase**

The external contract must not be reverse-engineered from existing Trade Control code.

---

# 17. Definition of Correctness

The Sole Trader statutory field model is correct when:

- it represents only the supported MTD Income Tax architecture;
- the quarterly accounting projection reflects the approved current statutory accounting concepts;
- obsolete SA100/SA103F and EOPS structures are absent;
- annual concepts are admitted only after current statutory verification;
- Tax Tags contain only information appropriately owned by Objective 2;
- every mapped accounting value is deterministic and traceable;
- unsupported distinctions are explicit;
- MIN is not forced to simulate STD detail;
- Objective 3 can consume the projection without inventing accounting information;
- historical implementation is treated as evidence rather than statutory authority.
