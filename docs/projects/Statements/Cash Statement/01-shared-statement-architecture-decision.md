# Shared Cash Statement architecture decision

**Status:** Accepted in principle; implementation blocked on the baseline gate  
**Date:** 14 October 2026  
**Decision owner:** Trade Control project  
**Scope:** Shared Cash Statement model and the production Excel and LibreOffice renderers

## Decision

Treat Excel (`.xlsx`) and LibreOffice (`.ods`) as two renderers of one systems-critical Cash Statement, not as independent report implementations.

Implement the shared statement builder and the production ODS renderer in **C#**, hosted in the existing `TCExports.Generator` process. Refactor the current Excel handler to consume the shared model only after the existing behaviour has been captured and the replacement can be proved equivalent against frozen, controlled baselines.

Do not carry Python, `pyodbc`, `odfdo`, `lxml`, or a child-process protocol into the target Azure deployment. Preserve the current Python exporter during delivery as:

- a catalogue of proven ODS layout and styling techniques;
- a source of representative ODS packages for XML comparison;
- a temporary visual regression oracle; and
- a fallback implementation for local investigation only, never for production financial output.

Retire the Python runtime path after both C# renderers pass the agreed financial, workbook, LibreOffice, concurrency, cancellation, and resource-bound tests.

## Non-negotiable Excel safety rule

The current Excel implementation is the production behavioural reference. The shared-model extraction is not permission to redesign or tidy its visible behaviour.

No Excel difference is acceptable merely because the new implementation appears more elegant or the resulting figure appears more plausible. Every difference must be classified as one of:

1. volatile metadata that was identified before comparison;
2. an approved defect correction with its own evidence and acceptance; or
3. a regression that blocks the change.

There must be no third, unexplained class. The existing Excel path remains available until the replacement passes the full comparison and is explicitly accepted.

"100% certainty" cannot sensibly mean binary identity because the current workbook contains timestamps and ZIP/OOXML metadata. For this project it means complete, explained coverage of every meaningful workbook property, backed by frozen input data, automated comparison, independent recalculation, and human review. A normalized package comparison must account explicitly for every excluded volatile field.

## Baseline gate

No production Cash Statement code changes begin until the following gate is complete.

### Databases in scope

| Scenario |
|---|
| Company, minimal template |
| Company, standard template |
| Sole trader, minimal template |
| Sole trader, standard template |

### Required database evidence

For each database:

1. record the exact source identity, database/schema version, compatibility level, recovery state, and capture time in the protected operational evidence;
2. create a recoverable point-in-time SQL Server snapshot or backup using the mechanism approved for the source environment;
3. verify the captured artefact rather than relying on command success alone;
4. record its immutable identity, location, size, and SHA-256 where the storage form permits hashing;
5. prove restore/readability without overwriting or altering the source sandbox; and
6. leave the four source databases unchanged during baseline export generation.

The physical capture mechanism and retention location must be agreed before execution. A SQL Server database snapshot is convenient for short-lived local comparison but is not a substitute for a portable backup; if the baseline must survive storage loss or movement, a verified backup is required.

### Required Excel baseline evidence

Generate the current Excel statement from the frozen state of each database using the existing accepted generator revision and an all-supported-options request: active periods, order book, tax accruals, VAT details, bank balances, balance sheet, and reconciliation enabled. The unused legacy prototype `includeBankTypes` flag is excluded and has been removed from request normalization; it is not part of the shared contract. Reduced-option combinations remain part of later regression testing but are not substitutes for this Phase 0 reference.

For every output, retain a manifest containing:

- database snapshot/backup identity;
- database and application version evidence;
- source repository and TCExports submodule commit identities;
- published generator binary hashes;
- the complete non-secret request parameters;
- generation time in UTC;
- output filename, byte length, and SHA-256;
- generation result and any bounded diagnostics; and
- the baseline tooling version.

Connection strings, credentials, user secrets, and other authentication material must not enter the manifest or retained diagnostic output.

### Baseline acceptance

The gate is complete only when:

- all four database captures are verified and recoverable/readable;
- every required current XLSX output opens without repair;
- formulas have been recalculated and the saved result re-opened;
- the baseline manifest accounts for every expected artefact;
- output and manifest hashes have been independently rechecked; and
- a named reviewer accepts the baseline record.

The accepted baseline becomes read-only project evidence. Regeneration creates a new versioned baseline; it does not silently replace the original.

## Why the earlier provisional runtime recommendation changes

The September feasibility record recommended Python as the quickest route to a first reliable ODS release unless deployment constraints made it unacceptable. The intended deployment constraints are now concrete: the exporter is a project reference inside a .NET 9 ASP.NET application deployed to Windows Azure App Service, and the product is moving from a single-node deployment to a shared multi-tenant service.

Under those conditions, Python is no longer merely an implementation detail. It introduces a second runtime, a second dependency and patch chain, native packages, a process boundary, temporary inter-process payloads, and separate concurrency and cancellation behaviour inside every scaled-out web instance. Those costs would remain after the duplicate financial logic was removed.

The target architecture therefore favours the larger one-off renderer port over a permanent operational split.

## Evidence from the current implementation

The present `CashStatementLibreHandler`:

1. writes the complete request, including its SQL connection string, to a temporary JSON file;
2. resolves an ambient executable named `python`;
3. starts a child process;
4. captures an unbounded base64 document through standard output; and
5. treats standard error as response diagnostic content.

The repository has no locked Python environment or package manifest. A successful deployment currently depends on compatible versions of Python, `odfdo`, `lxml`, `pyodbc`, and an ODBC driver being installed and discoverable by the App Service worker.

The Python exporter also owns a second database repository and repeats the statement's period mapping, category aggregation, VAT allocation, expression evaluation, and balance-sheet logic. The feasibility investigation demonstrated that those copies have already drifted from C# and can produce financially incorrect results even when LibreOffice accepts the file.

These are not intrinsic limitations of Python. They are a poor fit between a mixed-runtime child-process design and this application's deployment and assurance goals.

## Decision matrix

Scores are relative for this project: 1 is poor, 3 is workable, and 5 is strong.

| Criterion | Weight | Thin Python ODS serializer | Shared C# implementation | Reason |
|---|---:|---:|---:|---|
| One authoritative accounting pipeline | 5 | 4 | 5 | Both can consume a shared model, but C# does so without a serialized cross-runtime contract. |
| Excel non-regression assurance | 5 | 3 | 5 | Shared typed contracts and in-process tests can compare the existing and replacement Excel implementations directly. |
| Azure deployment and patching | 5 | 2 | 5 | Python needs an interpreter, locked packages, native dependencies, and health checks on each instance. C# uses the deployed application runtime. |
| Tenant and secret boundary | 5 | 3 | 5 | A thin Python process can avoid credentials, but still receives tenant-derived report data through an IPC boundary. C# keeps it in-process. |
| Cancellation, timeouts, and failure handling | 4 | 2 | 5 | Child-process cancellation, pipe draining, orphan prevention, and exit diagnostics require a separate supervisor. |
| Concurrency and resource governance | 4 | 2 | 4 | Per-request processes have material startup and memory costs. An in-process renderer can use normal ASP.NET admission control and bounded streams. |
| Observability and support | 3 | 2 | 5 | One structured logging and tracing pipeline is materially simpler than correlating process output. |
| Time to first equivalent-looking ODS | 3 | 5 | 2 | Python already contains the layout and styling implementation. |
| Preservation of existing ODS work | 2 | 5 | 3 | Python reuses it directly; C# ports the relevant package, XML, style, and settings behaviour. |
| Long-term maintainability | 5 | 2 | 5 | The product team already owns a .NET codebase and C# test/deployment toolchain. |
| Dependency maturity/control | 3 | 4 | 4 | `odfdo` is useful, but a deliberately narrow in-house writer avoids dependence on an immature general-purpose C# ODF abstraction. |
| **Weighted total** |  | **129** | **200** | C# is the stronger production choice; Python remains valuable during migration. |

## Shared implementation strategy

### 1. Characterize the current Excel implementation before extracting it

Build a comparison harness around the untouched Excel handler and the accepted controlled baselines. Record workbook structure, typed cell values, formulas, styles, number formats, borders, merged ranges, hidden rows/columns, widths/heights, freeze panes, protection, print settings, defined names, worksheet order, and supporting-sheet content.

The comparator must report differences by semantic section and cell coordinate, retain concise evidence, and fail closed on properties it does not understand. Raw binary and normalized OOXML package comparisons supplement this workbook-level inspection.

### 2. Share the statement, not renderer internals

Create a format-neutral C# statement builder that owns:

- database acquisition through `ICashFlowRepository`;
- the financial period grid;
- option interpretation;
- semantic row identities and presentation order;
- category graph validation and dependency evaluation;
- authoritative decimal values;
- expression results and zero/division policy; and
- reconciliation inputs and results.

Both XLSX and ODS renderers receive the resulting immutable document model. Neither renderer queries SQL or makes accounting decisions. Presentation abstractions are shared only where their meaning is genuinely format-neutral; renderer-specific formula syntax and package details remain separate.

### 3. Migrate Excel behind an equivalence gate

Run the original and shared-model Excel paths side by side against the same frozen inputs. Compare every accepted option profile and every meaningful workbook property. Keep the original path selectable for test comparison until acceptance; do not create a runtime dual-write or user-visible fallback that could conceal failures.

The shared-model Excel path becomes authoritative only after:

- all automated comparisons pass or contain only pre-approved differences;
- independent spreadsheet recalculation preserves the expected values;
- representative printed/PDF output is reviewed;
- a reviewer signs the evidence record; and
- rollback to the prior published version has been exercised or otherwise proven.

### 4. Build a narrow ODS package writer

Do not begin by adopting a broad spreadsheet abstraction or attempting to make ClosedXML produce ODS. The Cash Statement needs a small, known subset of ODF:

- the approved worksheets;
- strings, decimals, dates, and formula cells with authoritative cached values;
- row, column, and cell styles;
- borders and number formats;
- column widths and hidden marker columns;
- frozen panes and active-sheet settings; and
- the standard ODS ZIP parts and manifest.

Implement that subset using the .NET base class library (`System.IO.Compression` and `System.Xml`) behind an internal interface such as `IOdsDocumentWriter`. Prefer forward-only XML writing for the large worksheet content and deterministic in-memory/package streams with configured size limits.

This is not a proposal to implement the whole ODF standard. It is a controlled serializer for this report family, backed by package-level tests and native LibreOffice acceptance tests. ODF 1.3 defines an ODS document as a ZIP package containing XML parts and prescribes details such as an uncompressed first `mimetype` entry and `META-INF/manifest.xml`; these requirements are directly representable with the .NET ZIP and XML APIs.

### 5. Keep formulas secondary to values

The shared model supplies the authoritative decimal value for every calculated cell. Each renderer may also emit native formulas for traceability and interactive use, but it must never parse formula text to determine cached values.

Semantic references are resolved only after physical rows and columns have been allocated. This avoids the current display-order/forward-reference defect while allowing XLSX and ODS to use their respective formula syntax.

### 6. Treat generated documents as bounded request results

Both production renderers should:

- accept a cancellation token throughout;
- enforce configured maximum period, row, cell, and output-byte counts before or during serialization;
- avoid shared mutable renderer state and tenant-specific static caches;
- use per-request streams and deterministic disposal;
- emit structured, redacted diagnostics without report data or connection strings;
- expose generation duration and output-size metrics; and
- support host-level concurrency limits if load testing shows that generation can starve interactive requests.

Moving ODS to C# removes a process boundary; it does not by itself provide multi-tenant safety. The model builder must obtain tenant-authorised data server-side, and tests must prove that neither cached state nor output paths can cross tenant boundaries.

## Why not select a C# ODF package now?

The available NuGet landscape contains ODS-capable packages, but the evidence reviewed does not justify making an unfamiliar general-purpose package part of the accounting export trust boundary yet. Some packages are new or have small adoption footprints; older options may not provide the exact cached-value, OpenFormula, style, settings, and package control already shown to be necessary.

The first ODS spike should compare a minimal internal writer with the best credible package candidate against a fixed capability test. A package should be adopted only if it:

- supports the target .NET runtime and has an acceptable licence;
- produces deterministic ODF 1.3-compatible packages;
- preserves explicit cached numeric values alongside formulas;
- exposes the required styles, frozen panes, widths, hidden columns, and settings without post-save corruption;
- works without LibreOffice or another executable on the server;
- has no native runtime dependency;
- supports bounded stream output and cancellation, or can be safely wrapped; and
- passes malformed-input and dependency/security review.

If no package passes, the narrow internal writer is the default. The standard, rather than a third-party object's API, remains the contract.

## Delivery sequence

### Phase 0 — freeze, capture, and characterize

- Complete and accept the paired database/XLSX baseline gate.
- Freeze the comparison harness rules and volatile-field exclusions before refactoring.
- Keep the production ODS route disabled.
- Retain representative Python-generated packages and screenshots as ODS presentation evidence, not golden accounting values.
- Record the spreadsheet applications, LibreOffice version, ODF validation tooling, and comparison-tool versions used by the acceptance environment.

### Phase 1 — shared statement model beside the current Excel path

- Define stable semantic identifiers, period coordinates, row kinds, style roles, values, and formula dependencies.
- Move SQL acquisition and calculations into a C# statement builder under characterization tests.
- Add graph validation, cycle diagnostics, forward-reference support, and authoritative value tests.
- Keep the existing Excel handler unchanged and compare model values with its outputs.

### Phase 2 — Excel renderer migration

- Implement the XLSX renderer over the shared model.
- Run original and replacement paths against every frozen database/profile pair.
- Resolve every workbook difference under the non-regression rule.
- Do not remove or bypass the original implementation until the equivalence evidence is accepted.

### Phase 3 — ODS writer spike

- Produce a small ODS containing every required cell type, formula/cache pair, style, border, hidden column, width, and frozen-pane setting.
- Implement it once with the proposed internal ZIP/XML writer.
- Optionally implement the same capability fixture with the strongest package candidate.
- Inspect package XML, open/recalculate/save/reopen in LibreOffice, and compare the results before confirming the writer mechanism.

The spike is deliberately narrow and time-boxed. It validates serialization mechanics; it does not defer the production runtime decision.

### Phase 4 — Cash Statement ODS renderer

- Allocate rows and columns from the shared model.
- Translate semantic dependencies into OpenFormula.
- Emit authoritative cached values.
- Port the proven Python presentation details section by section.
- Add reconciliation and every supported request option.

### Phase 5 — parity and operational qualification

- Run the full four-database option matrix.
- Compare both formats with the shared model by semantic identity, not worksheet position.
- Re-run the new XLSX output against the original accepted baseline.
- Validate ODS open, full recalculation, save/reopen, print, and PDF export in supported LibreOffice.
- Exercise parallel tenants, cancellation, request aborts, output bounds, scale-out-safe behaviour, and redacted errors.
- Perform dependency/licence and malformed-package security review.

### Phase 6 — cutover and removal

- Accept the new Excel path independently before removing the original implementation.
- Enable ODS behind a feature flag only after its separate qualification is accepted.
- Remove Python process invocation and Python content from the published generator.
- Archive the Python source or retain it outside the production publish graph with a clear historical/reference label.
- Confirm the deployed App Service contains no unowned Python/ODBC requirement for statement generation.

## Reversal criteria

Reconsider a thin Python ODS serializer only if the ODS spike proves all of the following:

1. the required ODF behaviour cannot reasonably be produced or maintained in C#;
2. a pinned Python runtime can be deployed as an explicit, supported component rather than discovered from the host;
3. the serializer accepts only a bounded, credential-free statement model;
4. process supervision, cancellation, concurrency limits, telemetry, patching, and scale-out have written owners and tests; and
5. the operational cost is accepted as a product constraint.

Difficulty reproducing one style or settings fragment is not sufficient reason to reverse the decision; the existing generated package and Python XML transformations should first be used to identify the exact ODF markup.

## Consequences

### Positive

- one statement definition and calculation pipeline for both formats;
- one runtime, deployment unit, patch policy, and diagnostic pipeline;
- no connection strings or financial rules crossing a process boundary;
- simpler tenant isolation, cancellation, bounded execution, and load control;
- compile-time contracts between the model and both renderers;
- a clean path to remove `pyodbc`, the ODBC driver requirement, ambient Python discovery, and temporary request files; and
- lower long-term risk of format drift becoming accounting drift.

### Negative

- systems-critical Excel code must be characterized and refactored under a demanding equivalence gate;
- the first native C# ODS document will take longer than adapting the existing Python renderer;
- ODF package and XML knowledge becomes an explicitly owned part of the codebase;
- existing Python styling behaviour must be ported and verified rather than reused directly; and
- robust Excel and LibreOffice integration testing remains necessary even though office applications are not installed in production.

These costs are concentrated in delivery. The independent Python alternative concentrates less work initially but carries correctness, operational, and assurance costs for the lifetime of the service.

## References

### Repository evidence

- [ODS Cash Statement Generation](https://github.com/TradeControl/tradecontrol-exports/blob/main/docs/ods-statement-generation.md)
- `src/TCExports/src/TCExports.Generator/Handlers/CashStatementLibreHandler.cs`
- `src/TCExports/src/TCExports.Generator/Handlers/CashStatementExcelHandler.cs`
- `src/TCExports/src/TCExports.Generator/python/exporters/cash_statement_ods.py`
- `src/TCWeb/TCWeb.csproj`
- `src/TCWeb/Properties/ServiceDependencies/TCWeb - Web Deploy/profile.arm.json`
- `docs/projects/Tax Hub/ui/tax-hub-objective-5-hosting-decision.md`

### External specifications

- [OASIS OpenDocument 1.3, Part 2: Packages](https://docs.oasis-open.org/office/OpenDocument/v1.3/OpenDocument-v1.3-part2-packages.html)
- [OASIS OpenDocument 1.3, Part 3: Schema](https://docs.oasis-open.org/office/OpenDocument/v1.3/OpenDocument-v1.3-part3-schema.html)
- [OASIS OpenDocument 1.3, Part 4: OpenFormula](https://docs.oasis-open.org/office/OpenDocument/v1.3/OpenDocument-v1.3-part4-formula.html)

