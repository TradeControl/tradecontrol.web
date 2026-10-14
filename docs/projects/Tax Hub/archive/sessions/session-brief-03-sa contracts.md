# Tax Hub — SA Objective 3 Contract Implementation

## Purpose

Implement the current HMRC Making Tax Digital for Income Tax / Self Assessment contract surface in the `hmrc_mtd` repository.

This is an Objective 3 contract-modelling implementation session.

The objective is to create authoritative C# contract classes and endpoint metadata which faithfully represent the supported current HMRC SA/Self Employment APIs already researched for the Tax Hub project.

This session may implement the complete supported SA contract surface in one pass.

Do not implement:

- Trade Control data population;
- Tax Tag sourcing;
- database changes;
- UI changes;
- payload inspection harnesses;
- HTTP transport;
- authentication;
- HMRC submission execution.

Those are later phases.

---

# Mandatory Repository Discipline

Do not stage any files.

Do not commit any files.

Do not push anything.

Do not stash changes.

Do not reset, revert, amend or otherwise alter Git history or index state.

Leave all implementation changes unstaged in the working tree for user review.

This applies to every repository touched during the session.

---

# Authoritative Source

Use:

`docs/projects/Tax Hub/specs/reference/sole-trader-contracts.md`

as the primary authoritative contract reference.

Also use directly supporting current Tax Hub findings where necessary.

The implementation must follow the verified HMRC contracts recorded there.

Do not reverse-engineer the external contract from existing Trade Control code.

Do not derive current HMRC payloads from the historical SQL templates.

Do not assume the existing C# MTDITSA classes are authoritative.

Where repository code conflicts with the verified contract reference, the verified contract reference wins.

If the reference itself explicitly records an unresolved point, preserve that uncertainty rather than inventing a solution.

---

# Architectural Boundary

The classes created in this session must be independent of the Trade Control database schema.

They are HMRC contract models.

Conceptually:

    HMRC endpoint
        ↓
    path/query contract
        ↓
    request DTO
        ↓
    JSON serialization
        ↓
    response DTO

No contract class should depend on:

- `Cash.*`;
- `App.*`;
- Tax Tags;
- Tax Sources;
- SQL schema objects;
- Entity Framework entities;
- Trade Control accounting Categories;
- Trade Control Cash Codes;
- company bootstrap templates.

Later population code will decide where each contract value comes from.

Possible future sources include:

- Objective 2 Tax Tag projections;
- existing database fields;
- workflow context;
- HMRC state;
- derived values;
- external values;
- submission-time user input.

None of that sourcing logic belongs in these contract classes.

---

# Namespace

Use the existing SA namespace family rooted at:

`TradeControl.Tax.UK.Hmrc.Sa.v1_0`

Preserve existing repository namespace conventions where they remain useful.

The internal `v1_0` namespace is the Trade Control adapter-generation/version boundary.

Do not confuse it with the external HMRC API version numbers.

External API versions must be represented explicitly in endpoint metadata and/or contract organisation where appropriate.

---

# Existing MTDITSA Classes

The current repository contains legacy/read-side classes beneath approximately:

    Hmrc
        Sa
            v1_0
                Submissions
                    MTDITSA
                        Liabilities
                        Obligations
                        Payments

Treat these as historical implementation evidence only.

Do not extend an obsolete model merely because it already exists.

Inspect them for useful repository conventions such as:

- class style;
- namespaces;
- JSON attributes;
- mutability conventions;
- shared helpers.

Do not inherit inaccurate external contract semantics from them.

If an existing class genuinely matches a current authoritative contract, it may be retained or reused.

If not, create the correct current model.

Do not perform unrelated cleanup of legacy classes unless directly necessary to prevent ambiguity or build failure.

---

# Required Contract Families

Implement the supported current SA contract surface described in the authoritative reference.

The expected family structure is approximately:

    TradeControl.Tax.UK.Hmrc.Sa.v1_0
        Shared

        BusinessDetails
            V2

        Obligations
            V3

        SelfEmployment
            V5
                Cumulative
                Annual

        BusinessAdjustments
            V7

        BusinessIncomeSummary
            V3

        Losses
            V6
            V7

        TaxLiabilityAdjustments
            V1

        Calculations
            V8

        Finalisation
            V8

        Accounts
            V4

This is a conceptual organisation.

Use repository naming conventions where they improve consistency, but do not collapse materially different HMRC contract generations into one ambiguous model.

---

# Endpoint Metadata

Every supported operation must have explicit endpoint metadata sufficient for later transport code to know:

- HTTP method;
- path template;
- HMRC API version;
- relevant Accept media type;
- relevant Content-Type media type where applicable;
- OAuth scope where documented;
- path parameters;
- query parameters;
- whether a request body exists;
- expected success status code;
- response type where applicable.

Do not perform HTTP calls.

Do not create authentication code.

Do not create an HTTP client abstraction in this session.

The endpoint metadata is descriptive contract information only.

---

# Path, Query, Body and Response Separation

Model HTTP concerns correctly.

Do not represent GET query parameters as JSON request bodies.

Do not put path values inside DTO bodies merely for convenience.

Keep separate concepts for:

- path parameters;
- query parameters;
- request bodies;
- response bodies.

Examples of path/workflow values include:

- NINO;
- businessId;
- taxYear;
- calculationId.

These are not Tax Tags and should not be embedded into accounting DTOs unless the HMRC body contract itself explicitly contains them.

---

# 1. Business Details API v2

Implement the current Business Details contracts required by the reference.

Represent the current endpoint paths, query parameters and responses faithfully.

The primary purpose is to obtain authoritative business identity/details needed by later workflow/population logic.

Do not infer Trade Control business configuration from these classes.

---

# 2. Obligations API v3

Implement the current Obligations contracts.

Represent:

- endpoint metadata;
- path/query parameters;
- obligation response structures;
- obligation periods;
- statuses;
- relevant identifiers and dates.

The later application layer will use this API to determine statutory reporting periods.

Do not hard-code standard quarterly dates into the contract layer.

Do not derive obligation periods from Trade Control accounting periods.

---

# 3. Self Employment Business API v5 — Cumulative

Implement the current cumulative Self Employment GET and PUT contracts.

Current endpoint pattern:

    /individuals/business/self-employment/{nino}/{businessId}/cumulative/{taxYear}

Represent both GET and PUT operations correctly.

PUT success is HTTP 204.

GET returns the stored cumulative submission state.

The cumulative request body must support:

    periodDates
    periodIncome
    periodExpenses
    periodDisallowableExpenses

as defined by the authoritative reference.

---

## Cumulative Period Dates

Represent:

    periodStartDate
    periodEndDate

using appropriate ISO date types/serialization.

Do not embed HMRC calendar validation into generic DTO serialization.

Workflow validation belongs outside the DTO layer unless the external contract has a structural constraint that can be enforced without importing jurisdictional business logic.

---

## Cumulative Income

Support:

    turnover
    other
    taxTakenOffTradingIncome

Use exact HMRC JSON property names.

Do not rename the external property to match the Objective 2 Tax Tag `otherBusinessIncome`.

That translation belongs to the later population adapter.

Do not omit `taxTakenOffTradingIncome` merely because Objective 2 currently does not supply it.

The contract model must faithfully represent the HMRC field.

---

## Detailed Cumulative Expenses

Support all 15 current detailed expense properties:

    costOfGoods
    paymentsToSubcontractors
    wagesAndStaffCosts
    carVanTravelExpenses
    premisesRunningCosts
    maintenanceCosts
    adminCosts
    businessEntertainmentCosts
    advertisingCosts
    interestOnBankOtherLoans
    financeCharges
    irrecoverableDebts
    professionalFees
    depreciation
    otherExpenses

Use exact HMRC JSON names.

Do not remove fields simply because the default Trade Control template leaves them unmapped.

---

## Consolidated Expenses Alternative

Support the alternative:

    consolidatedExpenses

shape exactly as documented.

Detailed expenses and consolidated expenses are mutually exclusive HMRC request shapes.

Model this cleanly.

Prefer a design which makes an invalid payload difficult or impossible to construct.

Do not rely solely on comments to communicate the mutual exclusion.

Do not introduce Trade Control MIN/STD concepts into the DTO layer.

The HMRC contract only knows detailed versus consolidated expense reporting.

---

## Disallowable Expenses

Represent all documented current disallowable expense properties in the contract.

They remain part of the HMRC surface even though Objective 2 does not currently supply them.

Do not fabricate Trade Control sourcing logic.

Do not omit them from the contract merely because they are currently unsupported by the accounting projection.

---

# 4. Self Employment Business API v5 — Annual

Implement current annual GET, PUT and DELETE contracts.

Endpoint family:

    /individuals/business/self-employment/{nino}/{businessId}/annual/{taxYear}

Represent the documented optional annual groups, including:

    adjustments
    allowances
    nonFinancials

and their supported child fields.

Use exact HMRC JSON property names and structure.

Do not infer annual values from quarterly accounting data.

Do not implement Trade Control sourcing.

Replacement semantics belong in endpoint/contract documentation where useful:

a replacement PUT must represent the complete desired retained state according to HMRC behaviour.

Do not manufacture hidden defaults for omitted values.

---

# 5. Business Source Adjustable Summary / Business Adjustments v7

Implement the current BSAS / Business Adjustments contracts described in the authoritative reference.

Preserve the distinction between this API and the cumulative Self Employment business submission.

Do not duplicate Objective 2 Tax Tags merely because an adjustment field resembles an accounting field.

This is a separate statutory contract.

Model:

- endpoint metadata;
- path/query parameters;
- request structures;
- response structures;
- documented adjustment categories;
- identifiers/status values required by the API.

Use the exact external contract terminology.

---

# 6. Business Income Source Summary v3

Implement the current BISS contracts.

Represent current endpoint metadata and response structures faithfully.

Do not treat BISS as another accounting-submission payload.

It is an HMRC-side summary/read contract.

Where response fields are calculated or supplied by HMRC, model them as response data rather than writable accounting Components.

---

# 7. Individual Losses

Implement both relevant contract generations.

## Losses v6

Support the version applicable through tax year 2025–26 as recorded in the authoritative reference.

## Losses v7

Support the version applicable from tax year 2026–27 as recorded in the authoritative reference.

Keep version selection explicit.

Do not silently deserialize both external generations into one ambiguous model if they materially differ.

Tax-year/API-version selection belongs to later orchestration, but the contract layer must make the available versions unambiguous.

---

# 8. Individuals Tax Liability Adjustments v1

Implement the current Tax Liability Adjustments contracts applicable from 2026–27.

Represent:

- endpoint metadata;
- request DTOs;
- response DTOs;
- documented adjustment structures.

These values may later be externally supplied or professionally reviewed.

Do not attempt to derive them from accounting data in this session.

---

# 9. Individual Calculations API v8

Implement the current calculation trigger contracts.

Current endpoint pattern includes:

    /individuals/calculations/{nino}/self-assessment/{taxYear}/trigger/{calculationType}

Support the documented calculation types:

    in-year
    intent-to-finalise
    intent-to-amend

Represent these with a constrained type such as an enum or equivalent rather than unconstrained arbitrary strings, while ensuring exact HMRC wire values serialize correctly.

Represent the successful response containing the calculation identifier.

Current success behaviour is HTTP 202.

Do not trigger actual calculations over HTTP.

---

# 10. Final Declaration / Finalisation v8

Implement the current final declaration contract.

Current endpoint pattern includes:

    /individuals/calculations/{nino}/self-assessment/{taxYear}/{calculationId}/final-declaration

Represent the operation accurately.

The current contract has no request body.

Success is HTTP 204.

Do not invent an EOPS request body.

Do not model legacy End of Period Statement filing as a current submission step.

Do not reintroduce legacy crystallisation terminology except where it remains part of an actual current HMRC read endpoint documented in the authoritative reference.

---

# 11. Self Assessment Accounts API v4

Implement the current Accounts contracts described in the reference.

Represent:

- endpoint metadata;
- path/query models;
- account/liability/payment response structures required by the current contract.

If current contracts supersede the existing historical `Liabilities` / `Payments` classes, implement the authoritative versions rather than preserving obsolete shape for compatibility.

Do not perform transport work.

---

# Shared Types

Create shared types where they genuinely represent repeated external HMRC contract concepts.

Likely candidates include:

- ISO date wrappers only if repository conventions justify them;
- monetary values;
- tax-year representation;
- identifiers;
- status enums;
- common error structures;
- shared response metadata.

Do not create a speculative generic statutory framework.

Prefer small explicit contract types over an abstraction designed for hypothetical future regimes.

Corporation Tax will have materially different XML/iXBRL requirements and must not distort these JSON contracts.

---

# JSON Serialization

The resulting classes must serialize and deserialize using the repository's supported JSON stack.

Use explicit external JSON naming where necessary.

Do not rely on accidental CLR naming convention equivalence when the contract would be clearer and safer with explicit names.

Verify:

- exact property names;
- nesting;
- arrays;
- nullable/optional values;
- enums/string literals;
- date formatting;
- decimal formatting;
- omission behaviour.

Do not use `Math.Abs`.

Do not apply a blanket negative-expense rule.

The authoritative contract permits signed values in relevant expense/disallowable fields.

Preserve the external contract semantics.

---

# Optionality and Omission

Model optional fields as genuinely optional.

Do not silently serialize unsupported values as zero merely because a CLR numeric property has a default value.

Use nullable value types or an equivalent explicit mechanism where omission is meaningful.

Do not add default constructors or property initializers which accidentally turn:

    absent

into:

    0

or:

    empty object

unless the external HMRC contract explicitly requires that representation.

---

# OQ-1 — Zero Versus Omission

The research records an unresolved question concerning HMRC narrative guidance versus schema/OpenAPI behaviour around zero and omission.

Do not invent a final answer.

Implement DTO optionality so that the later population layer can deliberately choose between:

- omitted;
- explicit zero;
- non-zero value.

Serialization tests should prove that both omission and explicit zero can be represented where the contract model permits them.

Record the unresolved HMRC Sandbox verification point rather than hard-coding an assumption.

---

# Unknown Response Fields

For read-side HMRC response models, preserve forward compatibility where appropriate.

If the repository's JSON stack supports extension data cleanly, use it for response contracts where unknown fields may reasonably appear and where dropping them would obstruct diagnostics.

Do not use extension-data machinery indiscriminately on every request DTO.

Requests should remain strict and intentional.

---

# Validation

The contract layer may enforce structural invariants intrinsic to the external contract.

Examples:

- detailed and consolidated expenses cannot coexist;
- required nested objects are required;
- constrained endpoint literals use valid values;
- structurally invalid combinations cannot serialize as apparently valid requests.

Do not implement:

- Trade Control accounting validation;
- database validation;
- business eligibility rules requiring external state;
- turnover-threshold workflow;
- obligation-period selection;
- user permissions;
- transport/authentication validation.

Those belong elsewhere.

---

# Endpoint Versioning

External HMRC API versions must remain visible and explicit.

Known versions in scope are:

    Business Details                 v2
    Obligations                      v3
    Self Employment Business         v5
    Business Adjustments / BSAS      v7
    Business Income Summary / BISS   v3
    Individual Losses                v6 / v7
    Tax Liability Adjustments        v1
    Individual Calculations          v8
    Self Assessment Accounts         v4

Do not infer external version numbers from the internal namespace `v1_0`.

---

# Testing

Create comprehensive contract/serialization tests.

Tests should not require:

- Trade Control database access;
- network access;
- HMRC credentials;
- Tax Tags;
- bootstrap templates.

Use representative fixtures based on the authoritative contract reference.

At minimum prove:

1. Every supported endpoint has the correct HTTP method.
2. Every endpoint has the correct path template.
3. Required HMRC media/API version metadata is represented.
4. Path parameters are not serialized into request bodies.
5. GET query models are not treated as bodies.
6. Cumulative detailed expense payload serializes with all supported fields correctly named.
7. Cumulative consolidated expense payload serializes correctly.
8. Detailed and consolidated expense shapes cannot be combined into a valid request.
9. `other` serializes as HMRC `other`, not `otherBusinessIncome`.
10. `taxTakenOffTradingIncome` is represented.
11. `irrecoverableDebts` is represented.
12. `depreciation` is represented.
13. All documented disallowable expense fields are represented.
14. Annual optional groups serialize correctly.
15. Omitted nullable fields are actually omitted where intended.
16. Explicit zero can remain explicit where intended.
17. Signed expense values are preserved.
18. Calculation types serialize to exact HMRC wire values.
19. Calculation trigger success response models `calculationId`.
20. Final declaration has no request body.
21. Losses v6 and v7 remain distinguishable.
22. Tax Liability Adjustments v1 remains distinguishable and date/version gated conceptually.
23. Response contracts deserialize representative HMRC JSON.
24. Existing unrelated repository tests continue to pass.

Do not call HMRC.

Do not create Sandbox requests.

---

# Fixtures

Create readable JSON fixtures where they improve confidence.

At minimum consider fixtures for:

- detailed cumulative Self Employment PUT;
- consolidated cumulative Self Employment PUT;
- cumulative GET response;
- annual PUT;
- representative annual GET response;
- BSAS request/response;
- BISS response;
- Losses v6;
- Losses v7;
- Tax Liability Adjustments v1;
- calculation trigger response;
- Accounts response.

Fixtures should demonstrate the authoritative wire contract, not Trade Control internal naming.

---

# Existing Code Compatibility

Build the repository after implementation.

Fix build/test failures caused directly by this implementation.

Do not undertake unrelated refactoring.

If an existing obsolete class conflicts with a new authoritative class, prefer a clean namespace/type separation rather than silently changing unrelated consumers.

If removing or altering a legacy class would cause broader application changes, stop and report the dependency rather than expanding scope.

---

# Documentation

Append a concise Objective 3 implementation entry to:

`docs/projects/Tax Hub/change-log.md`

Record:

- contract families implemented;
- external HMRC API versions represented;
- serialization/contract tests added;
- unresolved contract questions;
- explicit confirmation that no population, harness or transport work was performed.

Update `docs/projects/Tax Hub/findings.md` only where implementation discovers material evidence not already recorded.

Preserve historical findings.

Do not rewrite earlier reconnaissance.

---

# Explicit Non-Goals

Do not implement Objective 2 changes.

Do not modify the now-complete Sole Trader MIN/STD templates.

Do not change Tax Tags.

Do not change Tax Tag mappings.

Do not modify SQL schema.

Do not modify application UI.

Do not populate contracts from Trade Control.

Do not add missing Trade Control database fields.

Do not implement the population bridge.

Do not implement the JSON payload inspection web interface.

Do not implement test-harness HTTP endpoints.

Do not implement HMRC transport.

Do not implement OAuth.

Do not call HMRC Sandbox.

Do not implement Corporation Tax.

Do not implement Companies House.

Do not design a shared CT/SA submission framework.

---

# Completion Report

Stop once the complete supported SA Objective 3 contract surface has been implemented, built and tested.

Report:

- files created;
- files modified;
- namespaces/families created;
- endpoints represented;
- HTTP methods/path templates/version metadata represented;
- request DTOs created;
- response DTOs created;
- shared types created;
- serialization fixtures/tests created;
- test results;
- build result;
- any legacy classes retained or superseded;
- any unresolved authoritative-contract questions;
- any contract fields whose later sourcing will require workflow/external/UI/database work;
- confirmation that no Trade Control schema dependency was introduced;
- confirmation that no population code was introduced;
- confirmation that no harness code was introduced;
- confirmation that no transport code was introduced;
- confirmation that all changes remain unstaged;
- confirmation that no commit was created.

Completion gate:

> The `Hmrc.Sa` contract layer independently models the verified supported current HMRC SA/Self Employment endpoints and JSON request/response contracts sufficiently for the next phase to populate those contracts from Trade Control and workflow data.

Do not proceed into population, harness or transport work after reaching that gate.
