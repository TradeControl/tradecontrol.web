# Corporation Tax / Limited Company — Phase 1B Session Brief

**Project:** Trade Control Tax Hub  
**Phase:** 1B — Authoritative Company Filing Reconnaissance  
**Date:** 1 September 2026  
**Status:** Research only — no implementation

---

## 1. Objective

Establish the current authoritative statutory filing model for UK limited companies within the initial Trade Control scope.

The initial implementation target is:

> **UK micro-entity limited companies**

This phase must determine, from current authoritative sources, what such a company is required to submit to:

1. **HM Revenue & Customs (HMRC)** for Corporation Tax; and
2. **Companies House** for statutory company accounts.

The principal purpose is to establish the statutory target independently of Trade Control's existing implementation.

The resulting reference will subsequently be used to reconcile the existing SQL bootstrap implementation against current requirements.

This phase is therefore concerned with:

> **What must be filed?**

It is not yet concerned with:

> **How does the existing Trade Control implementation provide it?**

---

## 2. Project Context

Trade Control already contains a Limited Company accounting bootstrap rooted at:

`App.proc_Template_CO_MICRO_CUR_2026`

A separate Phase 1A reconnaissance has examined that implementation.

Phase 1A established that the existing bootstrap contains historical company Tax Sources, Tax Tags and mappings, including tags using `AC` and `CP` identifiers.

The `AC` and `CP` identifiers originate from an earlier Government Gateway company submission process.

Their historical origin is useful evidence, but they must **not** be assumed to represent current HMRC or Companies House contracts.

Unlike the completed Sole Trader / MTD Income Tax work, Trade Control currently has:

- no authoritative C# Corporation Tax payload models;
- no authoritative CT600 contract model;
- no authoritative Companies House accounts filing model;
- no authoritative company filing endpoint implementation.

The existing SQL therefore must not be treated as the specification for this research.

---

## 3. Previous Project Material

The completed Sole Trader / MTD Income Tax implementation material has been archived under:

`docs/projects/Tax Hub/implementation/history`

That material is historical reference only.

Do not use Sole Trader assumptions to infer Limited Company requirements.

In particular, company accounting periods and Corporation Tax filing rules must not be inferred from the Sole Trader cumulative reporting implementation.

---

## 4. Trade Control Accounting Period Context

During company initialisation, the user selects the company's financial year.

Trade Control therefore already establishes the accounting period applicable to the company rather than assuming a universal tax-year boundary.

This research must determine how that configured company accounting period relates to the statutory period/context requirements of:

- statutory company accounts;
- Corporation Tax computations;
- the Company Tax Return;
- any associated filing package.

Particular attention should be paid to whether HMRC and Companies House use the same accounting period boundaries in the normal case and what statutory exceptions exist.

Do not redesign Trade Control's accounting-period infrastructure during this phase.

---

## 5. Research Authority

The research must be based primarily on current authoritative sources.

Preferred sources are:

1. HMRC official guidance and technical/developer documentation;
2. Companies House official guidance and technical/developer documentation;
3. GOV.UK;
4. current official schemas, specifications and taxonomies published or explicitly adopted by HMRC or Companies House;
5. legislation or recognised statutory standards where necessary to resolve a requirement.

Secondary sources may be used only for orientation or clarification.

Where a secondary source conflicts with an authoritative source, the authoritative source governs.

Every substantive conclusion in the resulting reference document must be traceable to its authoritative source.

Clearly distinguish:

- verified statutory requirement;
- authoritative technical contract;
- interpretation;
- implementation recommendation;
- unresolved question.

Do not silently convert an interpretation into a requirement.

---

## 6. Initial Scope

The initial Trade Control target is a normal trading UK private limited company qualifying for the **micro-entities regime**.

Research should concentrate on that case.

Do not expand the implementation target merely because other company regimes exist.

However, identify important architectural boundaries where the micro-entity regime differs from broader company filing requirements if knowing that boundary would prevent Trade Control from being unnecessarily designed into a micro-only dead end.

This is reconnaissance only.

No implementation for small, medium or large companies is requested.

Likewise, dormant companies should be identified as a distinct case where relevant, but a dormant-company implementation is not part of this phase unless it is inseparable from understanding the filing contract.

---

## 7. Principal Research Question

Determine the actual relationship between:

1. **Companies House statutory accounts**;
2. **Corporation Tax computations**; and
3. **the HMRC Company Tax Return / CT600 submission**.

In particular, establish whether these are:

- independent submissions using independent information models;
- separate statutory presentations derived substantially from the same underlying accounting facts;
- components of a combined filing package;
- or some combination of the above.

Identify clearly:

> **what information is shared and what information is submission-specific.**

This distinction is fundamental to the subsequent Trade Control design.

---

## 8. Companies House Filing Model

For an in-scope micro-entity company, determine the current authoritative Companies House filing requirements.

Establish:

- what accounts must be filed;
- which statements are required;
- which disclosures are required;
- whether filleted accounts remain applicable and under what conditions;
- current micro-entity presentation requirements;
- comparative-period requirements;
- accounting-period/context requirements;
- company identity requirements;
- declarations or approval information;
- director/signatory requirements;
- rounding and units;
- zero, nil and omission rules;
- relevant current schemas or taxonomies;
- required electronic filing format;
- submission mechanism;
- validation rules;
- available test or sandbox facilities.

Where requirements depend upon accounting periods beginning or ending on particular dates because of legislative transition, record those boundaries explicitly.

---

## 9. HMRC Corporation Tax Filing Model

For the same in-scope company, determine the current authoritative HMRC Corporation Tax filing requirements.

Establish the relationship between:

- statutory accounts;
- Corporation Tax computations;
- CT600 / Company Tax Return data;
- supplementary pages where applicable;
- supporting documents or attachments;
- iXBRL or other structured-document requirements.

Determine:

- what constitutes the complete filing package;
- which elements are structured data;
- which elements are documents;
- applicable schemas and taxonomies;
- period/context requirements;
- company and tax identity requirements;
- declarations;
- monetary units and rounding;
- nil/zero/omission rules;
- validation requirements;
- submission mechanism;
- authentication requirements;
- test or sandbox facilities.

Do not assume that the current mechanism is an MTD API.

Establish the actual current submission technology from authoritative evidence.

---

## 10. Accounting Periods

Establish how accounting periods are represented by Companies House and HMRC.

The normal Trade Control case begins with a company-selected financial year.

Determine:

- normal statutory accounts period rules;
- Corporation Tax accounting-period rules;
- whether one company financial year normally corresponds to one Corporation Tax return;
- treatment of accounting periods longer than 12 months;
- treatment of short accounting periods;
- circumstances requiring more than one Corporation Tax accounting period or return for one set of company accounts;
- whether Companies House and HMRC period boundaries can diverge;
- which dates must be represented in each filing contract.

The purpose is to identify statutory period semantics, not to redesign Trade Control's date functions.

---

## 11. Statutory Field / Fact Analysis

Produce an authoritative field-set analysis for the initial micro-entity target.

Do not begin with the existing Trade Control `AC` or `CP` tags.

Begin with the current statutory contracts.

Classify required information into at least:

### 11.1 Shared accounting facts

Facts originating in company accounting which are required, directly or indirectly, by both Companies House and HMRC.

### 11.2 Companies House-specific facts

Facts, metadata, declarations, contexts or disclosures required for the statutory accounts filing but not part of the Corporation Tax submission.

### 11.3 HMRC accounts/computation facts

Accounting or computation facts required by HMRC beyond the Companies House filing requirement.

### 11.4 Company Tax Return fields

Fields belonging specifically to the CT600 / Company Tax Return rather than the underlying accounts.

### 11.5 Calculated / roll-up facts

Values calculated from other accounting or statutory facts rather than directly mapped from accounting transactions.

### 11.6 Derived / external facts

Values requiring taxpayer-level information, statutory calculations, declarations, identity information or external input rather than direct accounting mappings.

### 11.7 Conditional facts

Facts required only when a particular circumstance applies.

For every identified fact or coherent fact group, record where possible:

- authoritative name;
- authoritative identifier;
- filing target;
- data type;
- monetary/non-monetary nature;
- period or instant context;
- required/optional/conditional status;
- sign convention;
- calculation relationship;
- source authority;
- relevant notes.

Do not invent Trade Control Tax Tags during this phase.

The statutory field set must be established before deciding how it maps onto the Tax Tag architecture.

---

## 12. Taxonomy and Structured Accounts

Determine the role of iXBRL and any applicable UK taxonomies in current company filing.

Establish:

- whether HMRC requires accounts in iXBRL;
- whether computations require iXBRL;
- whether Companies House accepts or requires iXBRL for the in-scope filing route;
- which current taxonomy or taxonomies apply;
- whether HMRC and Companies House consume the same taxonomy facts;
- whether filing-specific extensions, contexts or metadata differ;
- how much of the statutory field set is defined by taxonomy rather than an API payload schema.

This section is particularly important because Trade Control currently has a flat Tax Tag projection architecture.

Do not attempt to force taxonomy concepts into that architecture.

Instead, determine what the external contract actually requires.

---

## 13. Submission Surfaces and Endpoints

Identify the current authoritative electronic submission surfaces.

For each filing target, record:

- filing authority;
- service/API/interface name;
- submission format;
- endpoint or service mechanism;
- authentication mechanism;
- production availability;
- test/sandbox availability;
- relevant official technical documentation;
- current version where applicable.

Distinguish clearly between:

- an API endpoint;
- an XML submission service;
- an iXBRL filing package;
- a browser/manual filing service;
- any legacy Government Gateway mechanism.

Do not infer an endpoint merely from terminology in the existing Trade Control repository.

---

## 14. Historical AC / CP Codes

Phase 1A found historical Trade Control tags using `AC` and `CP` identifiers.

These are known to originate from an earlier Government Gateway company submission process.

Research whether these identifiers:

- remain part of a current supported contract;
- correspond to concepts retained under another current schema or taxonomy;
- have been superseded;
- or are now purely historical.

Do not assume that an old identifier should survive merely because the underlying accounting concept still exists.

Record the result for later Phase 1C reconciliation.

---

## 15. Architecture Boundary Question

The Trade Control Tax Hub currently has a generic accounting projection architecture based on:

> Tax Source → Tax Tag → accounting mapping → extracted value

This research must **not** optimise the statutory model to fit that architecture.

Instead, determine whether the authoritative company filing model naturally separates into:

1. an accounting/statutory fact projection that could reasonably be supplied by Tax Tags; and
2. a contract-specific layer responsible for taxonomy contexts, calculations, declarations, identities, filing packaging and transport.

If the statutory requirements demonstrate that this separation is inappropriate or incomplete, state that explicitly.

Do not redesign the architecture during Phase 1B.

The architectural decision belongs to a later phase.

---

## 16. Questions This Phase Must Answer

At minimum, the completed research must allow us to answer:

1. What exactly does a normal UK micro-entity company currently file with Companies House?
2. What exactly does the same company currently file with HMRC for Corporation Tax?
3. What is the relationship between the statutory accounts, tax computations and CT600?
4. Which underlying accounting facts are shared?
5. Which facts are unique to Companies House?
6. Which facts are unique to HMRC?
7. What current schemas/taxonomies/contracts define those facts?
8. What are the authoritative filing formats?
9. What are the current electronic submission mechanisms?
10. How are company accounting periods represented?
11. What happens when an accounts period exceeds 12 months?
12. What identities, declarations and signatory information are required?
13. What current test facilities exist for each filing authority?
14. Are the historical `AC` / `CP` Government Gateway codes still relevant?
15. Can the existing Trade Control Tax Tag layer plausibly remain an accounting projection layer beneath separate filing-contract adapters?
16. What information must Trade Control be capable of supplying before implementation of those adapters can begin?

---

## 17. Deliverable

Create:

`docs/projects/Tax Hub/specs/reference/company-field-sets.md`

Suggested title:

> **Limited Company Statutory Projection Field Sets — UK Micro-Entities**

The document should be a durable reference specification rather than a research diary.

It should contain:

- scope and authority;
- filing landscape;
- Companies House requirements;
- HMRC Corporation Tax requirements;
- accounts/computations/CT600 relationship;
- accounting-period semantics;
- authoritative field/fact sets;
- shared versus filing-specific facts;
- taxonomy/schema information;
- submission mechanisms;
- test facilities;
- historical AC/CP status;
- unresolved questions;
- source references;
- conclusions relevant to later Objective 2 design.

Use tables where they improve precision.

Every important statutory or technical assertion must be traceable to an authoritative source.

---

## 18. Explicit Non-Goals

Do not:

- modify SQL;
- modify C#;
- modify bootstrap templates;
- modify Tax Sources;
- modify Tax Tags;
- modify mappings;
- create filing DTOs;
- create serializers;
- create endpoint implementations;
- create iXBRL documents;
- modify the test harness;
- reconcile the existing SQL against the new field set;
- redesign the Tax Tag architecture;
- implement support for non-micro companies.

Those activities belong to later phases.

---

## 19. Relationship to Phase 1A and Phase 1C

Phase 1A answered:

> **What does Trade Control currently contain?**

Phase 1B must answer:

> **What do the current statutory authorities actually require?**

A later Phase 1C will answer:

> **How does the existing Trade Control implementation compare with the authoritative requirement, and what should survive, change or be removed?**

Keep those concerns separate.

---

## 20. Completion Criteria

Phase 1B is complete when:

1. the current micro-entity filing landscape is unambiguous;
2. Companies House and HMRC filing responsibilities are clearly separated;
3. the relationship between accounts, computations and CT600 is documented;
4. the applicable schemas/taxonomies and filing mechanisms are identified;
5. accounting-period semantics are documented;
6. the authoritative statutory field/fact sets are sufficiently defined for Objective 2 design;
7. shared versus filing-specific information is identified;
8. historical AC/CP identifiers have been investigated;
9. test/sandbox facilities are identified;
10. unresolved statutory questions are explicitly listed rather than guessed;
11. `company-field-sets.md` contains enough authoritative information for Phase 1C repository reconciliation.

No implementation should be performed during this phase.
