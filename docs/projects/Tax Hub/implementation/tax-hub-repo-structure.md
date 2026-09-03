# Tax Hub Repository and Assembly Architecture

3 August 2026

## Status and Scope

This document proposes the repository and .NET assembly structure for the UK Tax Hub before Limited Company implementation begins. It is an architectural proposal only. It does not change existing Self Assessment or VAT behaviour, implement company contracts, or perform any project or source-file restructuring.

The proposal is based on inspection of:

- the approved `company-statutory-contract-design.md`;
- the current solution and all three current projects;
- project references and package dependencies;
- the existing Self Assessment, VAT, `Shared`, `Infrastructure`, `Models`, and `Services` source areas;
- namespace imports and actual cross-area dependencies; and
- the Self Assessment Objective 3 contract tests and fixtures.

The company contract design remains the direction for the company filing capability. This review places it within a coherent product-wide structure.

## Executive Recommendation

Recast the codebase as a **Tax Hub** product with three contract assemblies, an application assembly, and explicit external adapters:

1. `TradeControl.Tax.UK.Hmrc.MtdIncomeTax.Contracts`
2. `TradeControl.Tax.UK.Hmrc.Vat.Contracts`
3. `TradeControl.Tax.UK.Company.Contracts`
4. `TradeControl.Tax.UK.Application`
5. `TradeControl.Tax.UK.Adapters.TradeControl`
6. `TradeControl.Tax.UK.Adapters.Submission`
7. `TradeControl.Tax.UK.WebHarness`

This is enough separation to enforce the important dependencies without creating one project per endpoint, service, or XML concern.

The existing `HMRC_MTD` class library should disappear after its contents have been moved to their proper owners. It has no coherent residual responsibility: today it is simultaneously a wire-contract library, SQL adapter, mapping layer, test harness engine, validation layer, and submission runner.

The repository should be renamed from `hmrc_mtd` to `tax-hub`, subject to explicit approval. Companies House and Corporation Tax are first-class product capabilities, and Corporation Tax is not an MTD API. Keeping the old repository name would make the product boundary permanently misleading.

## Current Structure

The current repository contains this effective project structure:

```text
hmrc_mtd/
  src/
    hmrc_mtd.slnx
    HMRC_MTD/                    net8.0 class library
    HMRC_MTD.ContractTests/      net8.0 executable test project
    HMRC.WebHarness/             net9.0 web application
```

The solution file includes `HMRC_MTD` and `HMRC.WebHarness`, but does not currently include `HMRC_MTD.ContractTests`. The web harness references `HMRC_MTD`. The contract-test project also references `HMRC_MTD`. The class library has a direct dependency on `Microsoft.Data.SqlClient`.

### Contents of the current `HMRC_MTD` assembly

The class library contains approximately:

| Area | Current responsibility | Observed character |
|---|---|---|
| `Hmrc/Sa` | Self Assessment/MTD Income Tax contracts | 34 files; exact JSON contracts, endpoint metadata, generated response graphs, versioned production/preview models |
| `Hmrc/Vat` | VAT contracts | 29 smaller endpoint/request/response files |
| `Hmrc/Shared` | `JsonExtract` helper | Used only by older Self Assessment submission models |
| `Infrastructure/Config` | HMRC settings and environment selection | Runtime configuration concern |
| `Infrastructure/Db` | SQL connection and reader helpers | Trade Control persistence adapter concern |
| `Infrastructure/Logging` | submission logger | Runtime/adaptor concern |
| `Models/Tc` | SQL projection records | Trade Control adapter concern |
| `Models/Harness` | generic harness payloads | Web/application boundary concern |
| `Models/Alignment` | alignment status/report | Application use-case result concern |
| `Models/Hmrc` | runner error model | Submission/application concern, not an official contract |
| `Services/TcData` | SQL readers | Trade Control persistence adapter concern |
| `Services/Mapping` | tag/category-to-harness mapping | Trade Control population/application concern |
| `Services/Harness` | payload construction | Harness/application concern |
| `Services/Validation` | dictionary request validation, including database lookups | Harness/application concern mixed with persistence |
| `Services/Runner` | generic operation dispatch | Application orchestration concern |
| `Services/Alignment` | alignment orchestration | Application concern |

This arrangement makes the compiled dependency boundary much broader than the namespaces suggest. Any consumer that needs one VAT request type also receives SQL client dependencies, Trade Control data models, harness services, logging, and all Self Assessment contracts.

### Actual dependency observations

The source imports reveal useful natural seams:

- The new Objective 3 Self Assessment contracts depend on `TradeControl.Tax.UK.Hmrc.Sa.v1_0.Shared`, but VAT does not.
- The `Hmrc/Shared/JsonExtract.cs` helper is not genuinely shared across regimes. Its only consumers are older Self Assessment obligation, liability, and payment types.
- No VAT source imports a Self Assessment namespace, and no Self Assessment source imports a VAT namespace.
- The non-contract `Infrastructure`, `Models`, and `Services` sources do not currently import the versioned HMRC contract namespaces. They communicate through generic harness payloads and runner models.
- SQL readers depend on SQL infrastructure and `Models/Tc`; validators for VAT and micro data directly depend on those readers.
- Harness builders depend on mapping and Trade Control readers.
- The submission runner depends on logging, harness builders, validation, and its own generic HMRC error model.

These edges demonstrate that SA and VAT can be extracted independently and that the runtime code can be separated from both. They also show that a generic `Shared` assembly is not justified by the current code.

### Current testing pattern

`HMRC_MTD.ContractTests` is a lightweight executable containing Objective 3 Self Assessment contract tests and JSON fixtures. It proves useful conventions:

- exact property and literal serialization;
- explicit zero versus absent optional values;
- mutually exclusive shapes;
- version separation and preview gating;
- endpoint inventory invariants;
- generated wire-model coverage; and
- offline fixture operation.

Despite its generic name, it is presently a Self Assessment contract-test suite. VAT has no equivalent isolated fixture suite in that project. The test project should therefore be renamed and owned by the extracted Income Tax contract assembly rather than becoming a single ever-growing test executable for every regime and document format.

## Design Principles

The target architecture follows these rules:

1. **Contracts are owned by the filing regime or coherent filing capability.** A change to a VAT wire contract should not force a Companies House or Self Assessment consumer to accept that dependency.
2. **The application depends inward on contracts and its own ports.** Contracts never depend on orchestration, databases, web applications, authentication clients, or Trade Control.
3. **External systems are adapters.** Trade Control SQL access and authority submission transport implement application ports; they do not define the application's semantic centre.
4. **Namespaces describe owned concepts.** Generic buckets such as `Models`, `Services`, `Infrastructure`, and `Shared` are removed from the public architecture.
5. **Sharing must be demonstrated.** Similar-looking types are not moved into a common assembly until at least two consumers need the same semantics and lifecycle.
6. **External contract versions remain visible.** Moving files must not erase current API/RIM/taxonomy version boundaries or change serialization.
7. **Tests follow the boundary they protect.** Contract fixtures live with a test project for that contract assembly; application and adapter tests are separate.
8. **Project count reflects dependency boundaries, not folders.** Endpoint families and internal technical components remain namespaces within their owning assembly.

## Proposed Solution Tree

```text
tax-hub/                                      recommended repository name
  docs/
  src/
    TaxHub.slnx

    TradeControl.Tax.UK.Hmrc.MtdIncomeTax.Contracts/
      Shared/
      Accounts/
      BusinessDetails/
      BusinessIncomeSummary/
      SelfEmployment/
      BusinessAdjustments/
      Losses/
      Calculations/
      Finalisation/
      Obligations/
      TaxLiabilityAdjustments/
      Submissions/
      Generated/

    TradeControl.Tax.UK.Hmrc.Vat.Contracts/
      Obligations/
      Returns/
      ViewReturn/
      Liabilities/
      Payments/
      Penalties/
      FinancialDetails/
      CustomerInformation/

    TradeControl.Tax.UK.Company.Contracts/
      Statutory/
      Xbrl/
      CompaniesHouse/
      Hmrc/CorporationTax/
      Validation/
      Serialization/
      Generated/

    TradeControl.Tax.UK.Application/
      Alignment/
      Population/
      Reconciliation/
      Submission/
      Validation/
      Ports/

    TradeControl.Tax.UK.Adapters.TradeControl/
      Data/
      Mapping/
      Configuration/

    TradeControl.Tax.UK.Adapters.Submission/
      Hmrc/
        Authentication/
        Http/
      CompaniesHouse/
        Authentication/
        Gateway/
      Audit/
      Configuration/

    TradeControl.Tax.UK.WebHarness/
      Controllers/
      Requests/
      Composition/

  tests/
    TradeControl.Tax.UK.Hmrc.MtdIncomeTax.ContractTests/
      Fixtures/
    TradeControl.Tax.UK.Hmrc.Vat.ContractTests/
      Fixtures/
    TradeControl.Tax.UK.Company.ContractTests/
      Fixtures/
        CompaniesHouse/
        HmrcCorporationTax/
        Ixbrl/
        Schemas/
    TradeControl.Tax.UK.Application.Tests/
    TradeControl.Tax.UK.Adapters.Tests/
```

Only the three contract test projects are necessary immediately. Application and adapter test projects should be introduced when their corresponding behaviour is migrated or implemented; empty speculative projects add no value.

## Assembly Responsibilities and Namespace Ownership

### `TradeControl.Tax.UK.Hmrc.MtdIncomeTax.Contracts`

This assembly owns the exact HMRC MTD Income Tax/Self Assessment external contract surface:

- request and response types;
- endpoint descriptors;
- API version and media-type metadata;
- exact JSON serialization options and converters;
- HMRC API error response types used by these endpoints;
- generated wire response graphs; and
- production/preview isolation.

Root namespace:

```text
TradeControl.Tax.UK.Hmrc.MtdIncomeTax
```

The current `TradeControl.Tax.UK.Hmrc.Sa.v1_0` namespace can initially be retained during a file-only extraction to minimise risk, then renamed mechanically to `Hmrc.MtdIncomeTax` as an explicitly reviewed migration step. Because the product is unreleased, the cleaner name is recommended. “SA” is less precise than the actual MTD Income Tax API capability.

The existing `Sa.v1_0.Shared.ContractInfrastructure` remains internal to this assembly. Although it contains names such as `HmrcEndpoint` and `HmrcResponse`, the evidence shows its lifecycle and consumers are currently Income Tax-specific. It must not become a product-wide dependency merely because its type names are generic.

The current `Hmrc.Shared.JsonExtract` also moves here. It is an implementation helper for older Income Tax JSON models, not a shared HMRC contract. It should become internal if public access is unnecessary.

This assembly has no reference to VAT, Company contracts, Application, SQL, ASP.NET Core, or submission adapters.

### `TradeControl.Tax.UK.Hmrc.Vat.Contracts`

This assembly owns the exact MTD VAT external contract surface:

- obligations;
- VAT returns and view-return operations;
- liabilities and payments;
- penalties;
- financial details and customer information;
- endpoint metadata; and
- exact request/response serialization.

Root namespace:

```text
TradeControl.Tax.UK.Hmrc.Vat
```

Existing API-version namespaces remain beneath this root. Current cross-folder dependencies among VAT return, financial-detail, penalty, and customer-information types remain inside the assembly; they do not justify more projects.

The VAT assembly does not reference Income Tax or Company contracts. If VAT later needs an endpoint descriptor structurally similar to the Income Tax descriptor, the first choice is a VAT-owned descriptor with the correct VAT semantics. A common contract primitive should be extracted only if it is genuinely identical and stable for at least two regimes.

### `TradeControl.Tax.UK.Company.Contracts`

This is the neutral company filing capability proposed by the approved company design. It owns:

- authority-neutral company identity and reporting periods;
- statutory accounts and disclosure aggregates;
- authority-neutral tax computation semantics where appropriate;
- XBRL QNames, contexts, units, dimensions, facts, taxonomy metadata, and iXBRL artefact abstractions;
- Companies House accounts contracts, filing profiles, envelopes, acknowledgements, status contracts, and endpoint metadata;
- HMRC Corporation Tax CT600 and supplementary-page contracts;
- HMRC accounts and computation iXBRL projections;
- Corporation Tax submission packages and envelopes;
- contract-version selection;
- contract, schema, XBRL, and cross-document validation; and
- deterministic contract serialization/document generation.

Root namespaces:

```text
TradeControl.Tax.UK.Company.Statutory
TradeControl.Tax.UK.Company.Xbrl
TradeControl.Tax.UK.CompaniesHouse
TradeControl.Tax.UK.Hmrc.CorporationTax
```

These namespaces intentionally express separate authority ownership inside one assembly. One assembly is appropriate because the company capability must compose a single approved statutory accounts set into coordinated Companies House and HMRC filings, and because the XBRL/taxonomy machinery is shared heavily within that capability.

This preserves the approved company design while avoiding premature fragmentation into separate XBRL, Companies House, CT600, and statutory-model projects. If a future non-company consumer genuinely needs the XBRL engine independently, it can then be extracted behind its existing namespace without changing company semantics.

The assembly must remain independent of Trade Control storage, Tax Tags, category trees, cash codes, templates, web types, credentials, and live transport clients.

### `TradeControl.Tax.UK.Application`

This assembly owns product use cases and ports, not external wire contracts or technical integrations:

- alignment workflow;
- population of filing-contract aggregates from accounting source models;
- reconciliation and evidence results;
- filing preparation and submission orchestration;
- use-case request and result types;
- application-level validation policy;
- environment-independent submission lifecycle; and
- interfaces for source data, clocks, artefact stores, audit stores, credential providers, and authority gateways.

Root namespace:

```text
TradeControl.Tax.UK.Application
```

The current `Models/Alignment` types move beside the `Alignment` use case. Generic `Models` is removed. The current `Services/Alignment` and the coherent parts of `Services/Runner` move here after being reshaped around typed use cases and ports.

Application validation must be distinguished from contract validation. Contract validation belongs to the contract assembly whose rules it enforces. Checking whether a web/harness request supplied `tenantId` or `connectionString` is input-boundary validation and does not belong beside HMRC statutory validation.

This assembly references all three contract assemblies because it coordinates their use. None of those assemblies references it.

### `TradeControl.Tax.UK.Adapters.TradeControl`

This assembly owns integration with the Trade Control accounting system:

- SQL connection factory and SQL reader helpers;
- query implementations for VAT, business tax, reconciliation, and submission history;
- source projection records shaped by Trade Control views;
- category and Tax Tag mapping implementations;
- population of application input models from Trade Control data; and
- Trade Control-specific configuration.

Root namespace:

```text
TradeControl.Tax.UK.Adapters.TradeControl
```

The current `Infrastructure/Db`, `Models/Tc`, `Services/TcData`, and `Services/Mapping` areas move here. This is the only principal assembly that should require `Microsoft.Data.SqlClient`.

It references `Application` to implement application ports and references the relevant contract assemblies only where a mapping adapter must construct a contract aggregate. Prefer returning application source models to direct construction of HMRC wire DTOs; the application/population layer should remain responsible for the semantic translation and reconciliation boundary.

Trade Control database names and legacy codes must not escape this adapter in public contract types.

### `TradeControl.Tax.UK.Adapters.Submission`

This assembly owns live communication with authorities and operational infrastructure:

- HMRC OAuth and fraud-prevention headers;
- HMRC HTTP/Transaction Engine clients;
- Companies House presenter/company authentication handling;
- Companies House gateway communication and status polling;
- environment/base-address configuration;
- retries, timeouts, idempotency/correlation propagation, and transport diagnostics;
- submission audit persistence adapters; and
- secure credential-provider implementations.

Root namespace:

```text
TradeControl.Tax.UK.Adapters.Submission
TradeControl.Tax.UK.Adapters.Submission.Hmrc
TradeControl.Tax.UK.Adapters.Submission.CompaniesHouse
```

This single adapter assembly is initially preferable to separate HMRC and Companies House client projects. They share operational hosting, resilience, secure configuration, telemetry, and audit concerns, while namespaces maintain authority boundaries. A later split is warranted only if deployment, security ownership, dependencies, or release cadence diverge materially.

Authority endpoint/service metadata that defines the external contract remains in the relevant contract assembly. Code that opens connections, obtains credentials, performs retries, or polls lives here. Credentials never move into contract aggregates.

This adapter references `Application` and the relevant contract assemblies. Nothing below it references this adapter.

### `TradeControl.Tax.UK.WebHarness`

This assembly is a composition root and diagnostic host. It owns:

- HTTP controllers/endpoints used to exercise Tax Hub use cases;
- transport request binding and web response presentation;
- dependency injection and configuration composition; and
- harness-only request/result presentation types.

Root namespace:

```text
TradeControl.Tax.UK.WebHarness
```

The current `Models/Harness`, `Services/Harness`, and dictionary-based request validators should either move here when they are truly harness-specific or be replaced by typed application requests. The current controller namespace `TradeControl.Tax.UK.Controllers` is too broad and should become `TradeControl.Tax.UK.WebHarness.Controllers`.

The harness references `Application` and the two adapter assemblies. It should not directly compose SQL readers, mapping services, and loggers inside controller actions. It may reference contract assemblies for diagnostic display or strongly typed request examples, but normal actions should call application use cases.

The target frameworks across the solution should be deliberately aligned. The current net9.0 web host referencing net8.0 libraries is legal, but a new product should select one supported baseline rather than inherit an accidental mismatch. The exact target framework is an implementation-time decision because support dates may affect it.

## Dependency Direction

The permitted compile-time dependencies are:

```text
MtdIncomeTax.Contracts       Vat.Contracts       Company.Contracts
           ^                     ^                     ^
            \                    |                    /
             \                   |                   /
                    Application
                    ^         ^
                   /           \
       Adapters.TradeControl   Adapters.Submission
                   \           /
                    WebHarness
```

Expressed as rules:

- Contract assemblies reference no product assembly and no other regime contract assembly.
- `Company.Contracts` contains its internally shared company/XBRL concepts; it does not reference Income Tax or VAT.
- `Application` may reference all contract assemblies.
- Adapters reference `Application` and only the contracts necessary to implement their ports.
- `WebHarness` is the composition root and may reference Application and adapters.
- Tests reference the production assembly they test and, where needed, test-only fixture libraries—not the reverse.

No `Application -> Adapter` reference is permitted. No `Contract -> Application` reference is permitted. These two constraints prevent the most likely circular dependencies.

## Treatment of Shared Types and Infrastructure

### No initial `Shared` project

A new `TradeControl.Tax.UK.Shared`, `Common`, `Core`, or `Models` assembly is not recommended. Such projects tend to accumulate unrelated low-level types and become dependencies of everything.

The current evidence does not justify one:

- `Hmrc/Shared/JsonExtract` is Self Assessment-only.
- `Sa/v1_0/Shared` is Self Assessment contract infrastructure.
- VAT and SA do not currently share source types.
- company XML/iXBRL submission metadata differs materially from the JSON/HTTP MTD endpoint descriptor.

Small value objects may be duplicated when their semantics differ. Extraction is appropriate only when the same type, invariants, ownership, and release lifecycle are genuinely shared. If that evidence later appears, introduce the smallest named assembly that describes the concept—for example an authority-transport abstraction—not a general utility library.

### Contract metadata versus transport infrastructure

Endpoint paths, API versions, media types, expected success status, request/response types, schema versions, and preview status describe an external contract and remain in that contract assembly.

HTTP clients, XML gateway clients, OAuth flows, secret acquisition, retry policy, socket/time-out configuration, logging implementations, and environment configuration are operational mechanisms and live in `Adapters.Submission`.

Application ports should use semantic operations such as submitting a VAT return or a Corporation Tax package, not a generic `SendAsync(string operation, Dictionary<string, object?> parameters)` API. Authority adapters translate these operations to the precise endpoint descriptors.

### Validation ownership

Validation is owned according to the rule being enforced:

- wire shape, schema, statutory, XBRL, and authority business rules: relevant contract assembly;
- cross-source reconciliation and filing-readiness policy: `Application`;
- incoming web request syntax: `WebHarness`;
- database availability and projection integrity: `Adapters.TradeControl`;
- transport response/protocol validity: `Adapters.Submission`.

This removes the present ambiguity of one generic `Services.Validation` namespace containing both dictionary syntax checks and synchronous SQL lookups.

## Proposed Moves, Renames, and Deletions

The following is a responsibility map, not an instruction to move files during this design stage.

| Current area | Target | Action |
|---|---|---|
| `HMRC_MTD/Hmrc/Sa` | `Hmrc.MtdIncomeTax.Contracts` | Extract; preserve wire behaviour; optionally rename `Sa` namespace in a separate mechanical step |
| `HMRC_MTD/Hmrc/Vat` | `Hmrc.Vat.Contracts` | Extract unchanged |
| `HMRC_MTD/Hmrc/Shared/JsonExtract.cs` | `Hmrc.MtdIncomeTax.Contracts` | Move with its actual consumers; make internal if possible |
| `HMRC_MTD/Infrastructure/Db` | `Adapters.TradeControl` | Move |
| `HMRC_MTD/Models/Tc` | `Adapters.TradeControl` | Move and rename by projection purpose |
| `HMRC_MTD/Services/TcData` | `Adapters.TradeControl` | Move; implement application ports |
| `HMRC_MTD/Services/Mapping` | `Adapters.TradeControl` and/or `Application/Population` | Split policy from Trade Control-specific lookup |
| `HMRC_MTD/Models/Alignment` | `Application/Alignment` | Move beside owning use case |
| `HMRC_MTD/Services/Alignment` | `Application/Alignment` | Move |
| `HMRC_MTD/Models/Harness` | `WebHarness/Requests` | Move if still needed; prefer typed application requests |
| `HMRC_MTD/Services/Harness` | `WebHarness` or `Application` | Split presentation-only construction from use-case orchestration |
| `HMRC_MTD/Services/Validation` | Contract, Application, WebHarness, or adapter owner | Split by rule ownership; delete generic namespace |
| `HMRC_MTD/Services/Runner` | `Application/Submission` plus `Adapters.Submission` | Split orchestration from external I/O |
| `HMRC_MTD/Models/Hmrc/HmrcError.cs` | Application result or authority adapter | Rename according to whether it is semantic or transport error; do not confuse it with official error contracts |
| `HMRC_MTD/Infrastructure/Config` | composition root and appropriate adapter | Split typed options by owner |
| `HMRC_MTD/Infrastructure/Logging` | `Adapters.Submission/Audit` | Move implementation behind an application audit port |
| `HMRC_MTD/ModuleInfo.cs` | none | Delete; assembly metadata and product versioning replace the misleading module constant |
| `HMRC_MTD.ContractTests` | `Hmrc.MtdIncomeTax.ContractTests` | Rename and reference only the extracted Income Tax contract assembly |
| `HMRC.WebHarness` | `TradeControl.Tax.UK.WebHarness` | Rename and make composition root |
| `HMRC_MTD` project | none | Remove after all owned content is extracted |

No compatibility facade is recommended because the product has not been released. A temporary empty or forwarding `HMRC_MTD` assembly would weaken the new boundaries and create migration work with no consumer benefit.

## Offline Contract Tests and Fixtures

Each external contract assembly should have its own offline contract-test project:

### MTD Income Tax

Move the existing Objective 3 test program and fixtures intact into `TradeControl.Tax.UK.Hmrc.MtdIncomeTax.ContractTests`. Preserve its serialization, endpoint-inventory, version, preview, and generated-wire checks. Make namespace/project changes mechanically and verify fixture outputs have not changed.

### VAT

Create `TradeControl.Tax.UK.Hmrc.Vat.ContractTests` when VAT extraction occurs. Cover every endpoint descriptor, request/response serialization, exact enum/date/number behaviour, zero versus omission, unknown-field handling where intended, and representative positive/error fixtures. Do not make VAT depend on the Income Tax test project for generic helpers; tiny test helpers can be linked or duplicated until a stable test-only abstraction emerges.

### Company

Create `TradeControl.Tax.UK.Company.ContractTests` according to the approved company design. It owns Companies House, CT600, accounts/computation iXBRL, schema/taxonomy, package composition, period split, and deterministic artefact fixtures. Keeping these in one test project matches the coherent company filing assembly and permits cross-authority composition tests.

Official schemas and taxonomy assets belong under the company fixture/contract asset policy, not in a product-wide `Shared` fixture directory. Source provenance, licensing, checksums, and generation manifests remain mandatory.

Contract tests must not reference `Application`, Trade Control adapters, live submission adapters, SQL Server, or credentials. Conversely, application tests may use contract fixtures/builders but should not redefine wire expectations.

## Repository Name

`hmrc_mtd` no longer describes the intended product:

- Companies House is not HMRC.
- Corporation Tax online filing is not one of the MTD JSON API families represented by the current name.
- the product coordinates statutory accounts, XBRL/iXBRL, tax computations, several filing regimes, reconciliation, and authority submission.

The recommended repository name is:

```text
tax-hub
```

The solution should correspondingly become `TaxHub.slnx`. Assembly names remain explicit and should not collapse into a single `TaxHub.dll`.

Because this repository is currently a submodule of `tradecontrol.web`, renaming has operational consequences: remote repository name, submodule path and URL, `.gitmodules`, local developer checkouts, CI paths, documentation links, and any deployment scripts must be updated together. The rename should therefore be a separately approved migration step even though there is no released-product compatibility constraint.

If the repository rename is deferred, the target project and namespace structure should still be adopted. Repository naming should not block correct compile-time boundaries.

## Migration Strategy

The restructuring should be performed before company contract implementation, in small behaviour-preserving stages:

1. Record a clean baseline build and run the existing Objective 3 contract tests directly, since they are not presently included in the solution.
2. Align or explicitly document target frameworks and introduce common build settings only where they are genuinely common.
3. Extract Self Assessment into `Hmrc.MtdIncomeTax.Contracts`; move its two apparent shared areas with it; rename its contract-test project and prove byte-equivalent fixtures.
4. Extract VAT into `Hmrc.Vat.Contracts`; add its offline endpoint and fixture tests before changing any VAT names or behaviour.
5. Create `Application` with ports and move alignment/orchestration types without introducing adapter references.
6. Extract Trade Control SQL/data/mapping code into `Adapters.TradeControl`, leaving `Microsoft.Data.SqlClient` there.
7. Extract submission/authentication/logging implementations into `Adapters.Submission` behind application ports.
8. Rename and reduce `WebHarness` to a composition root with typed input boundaries.
9. Delete the empty `HMRC_MTD` project and add every active production/test project to `TaxHub.slnx`.
10. Implement `Company.Contracts` only after the dependency rules compile and architectural tests can enforce them.
11. Perform the repository/submodule rename as a coordinated, explicitly approved operation.

Namespace renaming should not be mixed with behavioural edits. Where practical, first move a compiling source set into its new project while retaining namespaces, then perform a dedicated namespace rename with serialization regression tests. JSON and XML names must never be derived accidentally from CLR namespace/type renames.

## Migration Risks and Controls

### Circular dependencies

The largest risk is allowing Application to know concrete SQL or transport types while adapters also need Application contracts. Prevent this by defining ports in `Application` and implementing them in adapters. Dependency checks should fail the build if contracts reference Application or Application references an adapter.

Company population creates another potential cycle: `Company.Contracts -> TradeControl mapping -> Company.Contracts`. The mapping belongs in `Adapters.TradeControl` or an Application population service. `Company.Contracts` knows only statutory inputs.

### False sharing

Moving generic-looking types such as `HmrcEndpoint`, `HmrcResponse`, `ValidationResult`, or `HmrcError` into a common assembly could couple JSON MTD APIs to Corporation Tax XML and Companies House workflows. Keep them with their present semantic owners until genuine commonality is proved.

### Contract drift during file moves

Changing namespaces, serializer options, converters, generated source settings, nullable settings, or target frameworks can alter wire behaviour. Preserve exact fixtures, compare serialized bytes, and move generated code and its converters as one unit.

### Generated source and default namespace changes

Generated Objective 3 types contain their own namespaces and serializer helpers. Project `RootNamespace` does not rewrite them, but regeneration settings may. Pin generation commands and compare output before accepting a move.

### Framework and package drift

The current web project targets net9.0 while the library/tests target net8.0. Project extraction is an opportunity to align frameworks, but framework upgrades should be a distinct change with full tests. Avoid mixing architecture moves with library upgrades.

### Harness behaviour becoming application architecture

The current dictionary-driven runner and validators are useful diagnostic scaffolding but should not define permanent application use cases. Preserve the harness externally where useful, while translating it to typed Application requests at the web boundary.

### Submission error ambiguity

There are official HMRC error DTOs and a generic runner `HmrcError`. Renaming/moving must preserve the distinction among authority-declared business errors, protocol/transport failures, application validation findings, and unexpected faults.

### Repository/submodule rename

A remote/path rename can break the parent repository and CI even when code compiles. Treat it as an atomic repository-maintenance operation with a verified parent submodule update and documentation-link scan.

### Oversized company assembly

The company assembly will be larger than the VAT assembly because it includes XBRL, Companies House, CT600, and composition. That size is acceptable initially because these parts collaborate around one company filing aggregate. Monitor dependencies and build/release cadence. Split only if a stable reusable XBRL component or independently deployed authority client emerges.

## Architectural Enforcement

The future solution should make the intended structure executable rather than relying only on this document:

- contract project files contain no references to Application, adapters, ASP.NET Core, or SQL client packages;
- `Application` contains no adapter project references;
- only `Adapters.TradeControl` references `Microsoft.Data.SqlClient`;
- namespaces beginning `TradeControl.Tax.UK.Models`, `.Services`, `.Infrastructure`, or `.Shared` are rejected once migration completes;
- contract tests run offline and are included in the solution/build;
- generated source directories are explicitly marked and reproducible;
- production and preview external contracts remain separately selectable; and
- a small architecture test or build script validates the permitted project-reference graph.

These checks should be added during restructuring, not deferred until company implementation has created more dependency edges.

## Recommended Decisions

The recommended choices are:

- Approve `Tax Hub` as the product boundary and `tax-hub` as the repository name, with the physical rename performed as a separate coordinated operation.
- Delete the existing `HMRC_MTD` project after extraction; do not retain a compatibility facade.
- Extract SA/MTD Income Tax and VAT into separate contract assemblies without changing external semantics.
- Keep the approved company statutory, XBRL, Companies House, and Corporation Tax contracts in one company capability assembly initially.
- Do not create a general shared/common assembly.
- Create one Application assembly and two adapter assemblies: Trade Control integration and authority submission infrastructure.
- Rename the web host as a Tax Hub harness and keep it as the composition root.
- Place endpoint/schema metadata with its owning contract and live authentication/transport code in the submission adapter.
- Split the current generic `Models`, `Services`, `Infrastructure`, and `Shared` content by responsibility and remove those generic namespaces.
- Give each regime/capability an independent offline contract-test project and include all test projects in the solution.

## Decisions Required Before Restructuring

1. Approve the proposed seven production projects, or identify a boundary that should be combined before migration starts.
2. Approve removal of the existing `HMRC_MTD` project after its contents are extracted, with no compatibility facade.
3. Approve `TradeControl.Tax.UK.Hmrc.MtdIncomeTax` as the replacement for the current `TradeControl.Tax.UK.Hmrc.Sa.v1_0` root, with external JSON semantics held unchanged.
4. Approve retaining the complete company filing capability in one `TradeControl.Tax.UK.Company.Contracts` assembly initially, using separate Company, Companies House, and HMRC Corporation Tax namespaces internally.
5. Approve the rule that no general `Shared`, `Common`, `Models`, `Services`, or `Infrastructure` project will be introduced without a demonstrated coherent owner and at least two genuine consumers.
6. Approve one combined `Adapters.Submission` assembly initially, with HMRC and Companies House namespaces and a future split only if their operational dependencies or deployment boundaries diverge.
7. Decide whether to align the solution on the current .NET LTS baseline during restructuring or treat the framework change as a later independent task.
8. Approve renaming the repository to `tax-hub` and the solution to `TaxHub.slnx`, subject to a separately planned parent-submodule, CI, and documentation update.
