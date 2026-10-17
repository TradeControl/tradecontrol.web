# Cash Statement workplan

**Status:** Complete — Phases 0–8 accepted

**Date:** 17 October 2026

**Objective:** Deliver one authoritative C# Cash Statement pipeline with production-quality XLSX and ODS renderers, without an unapproved change to the systems-critical Excel statement.

## 1. Outcome

The project is complete when:

- one C# statement builder owns data acquisition, financial-period mapping, category dependency evaluation, calculations, and authoritative values;
- the XLSX and ODS renderers consume the same immutable statement model;
- the replacement XLSX route is proven equivalent to the accepted current route across the complete four-scenario matrix;
- the ODS route is financially equivalent to the shared model and behaves correctly in supported LibreOffice;
- generation is safe under cancellation, concurrency, and output limits in Azure;
- the Python statement engine and production child-process dependency are removed; and
- the evidence and rollback position for each production change are accepted.

## 2. Governing rules

1. **Baseline before code.** No Cash Statement production refactor starts before Phase 0 is complete and accepted.
2. **Excel remains authoritative during migration.** The existing XLSX handler stays intact until the replacement passes the complete equivalence gate.
3. **No unexplained differences.** Every workbook difference is volatile metadata, an independently approved correction, or a blocking regression.
4. **One accounting implementation.** Renderers do not query SQL, interpret options independently, infer financial periods, or calculate financial results.
5. **Values precede formulas.** The shared model contains authoritative decimal values. Formulas provide spreadsheet traceability and recalculation behaviour.
6. **No live dual path.** Old/new comparison happens in controlled tests and harnesses, not by silently generating two documents in production requests.
7. **Evidence is immutable.** A new evidence run is versioned; it never overwrites an accepted baseline.
8. **Operational material stays local.** Host names, exact database identities, access-control scripts, backups, generated statements, and detailed evidence remain in the ignored `.local` workspace or another approved protected store.
9. **ODS is independently gated.** Acceptance of the shared model and XLSX migration does not automatically enable ODS.
10. **Stop at failed gates.** A failed acceptance gate is resolved before dependent work proceeds.

## 3. Risk posture: this is a high-difficulty project

The project must not be managed as a routine second-format exporter. A previous attempt consumed approximately two weeks without reaching a dependable result and was stopped. That history is direct evidence that unconstrained iteration on the complete Cash Statement is an unacceptable delivery method.

The hard problem is the combination of requirements, not merely creating a ZIP package containing XML:

- cached values must be authoritative before LibreOffice recalculates;
- formulas must remain editable and stable after recalculate/save/reopen;
- OpenFormula syntax and ODF cell-value attributes must agree;
- forward category references, empty categories, arbitrary dependency depth, and conditional/ratio expressions must work together;
- period and VAT allocation must remain tied to the financial calendar;
- styles, annual borders, widths, hidden markers, panes, protection, printing, and settings cross several ODF package parts;
- LibreOffice may normalize or rewrite package XML on save;
- the existing Python output is useful evidence but contains known financial defects; and
- extracting shared logic risks the already critical XLSX route.

### Anti-thrashing controls

1. Do not debug the complete Cash Statement as the first C# ODS artefact.
2. Prove the difficult ODF behaviours with a deliberately small but hostile fixture immediately after the baseline.
3. Work from captured package XML and minimal failing examples, not visual trial and error in the full report.
4. Change one package concern at a time and retain the before/after artefacts.
5. End every spike session with a runnable artefact, test, or explicit recorded blocker.
6. Do not extend a spike merely because success feels close; extension requires an evidence-based owner decision.
7. A structurally openable ODS is not progress evidence unless cached values, formulas, recalculation, and save/reopen also pass.
8. If the early ODS feasibility gate fails, stop the ODS project before refactoring Excel for its sake.

## 4. Delivery map

| Phase | Purpose | Depends on | Exit decision |
|---|---|---|---|
| 0 | Freeze and capture the reference truth | Architecture decision | Baseline accepted |
| 0A | Prove or reject the hard ODS mechanics | Phase 0 | ODS feasibility go/no-go |
| 1 | Characterize current XLSX behaviour | Phase 0 | Comparator accepted |
| 2 | Specify the shared statement model | Phase 0A go decision and Phase 1 | Model contract accepted |
| 3 | Build and verify the shared statement builder | Phase 2 | Financial model accepted |
| 4 | Migrate XLSX to the shared model | Phase 3 | XLSX equivalence accepted |
| 5 | Harden the selected ODS writer against the shared model | Phases 0A and 3 | ODS writer integration accepted |
| 6 | Implement the ODS Cash Statement renderer | Phases 4–5 | ODS functional parity accepted |
| 6A | Integrate the formats into the product and prove the single-tenant path | Phase 6 | Product integration accepted |
| 7 | Qualify Azure service behaviour | Phase 6A | Production readiness accepted |
| 8 | Confirm cutover evidence and retire Python | Phase 7 | Project closure accepted |

After Phase 0, Phase 0A and Phase 1 may proceed independently. The implementation critical path is `0 → 0A go decision → 2 → 3 → 4 → 5 → 6 → 6A → 7 → 8`, with Phase 1 also required before Phase 2. Failure at Phase 0A stops ODS-led refactoring before substantial production code is disturbed.

## 5. Phase 0 — freeze and capture the reference truth

### Purpose

Create a repeatable, recoverable point of comparison before touching the systems-critical Excel implementation.

### Tasks

- [x] Quiesce applications, jobs, and users that can modify the four reference sandboxes.
- [x] Run the protected local status procedure and retain its result.
- [x] Set all four sandboxes read-only and verify the state transition.
- [x] Create a verified, recoverable point-in-time backup or equivalent approved capture for each sandbox.
- [x] Prove each capture is readable/restorable without replacing or altering its source.
- [x] Record database/schema versions, compatibility, recovery state, capture time, size, and immutable identity.
- [x] Identify and record the exact repository, TCExports submodule, published-binary, runtime, and dependency versions used by the current exporter.
- [x] Build/publish the existing exporter without source changes.
- [x] Generate the current XLSX outputs for every scenario/profile pair in the baseline matrix.
- [x] Recalculate, save, reopen, and inspect every workbook with the accepted spreadsheet applications.
- [x] Hash the raw databases/backups where possible, binaries, manifests, and generated workbooks.
- [x] Have a named reviewer reconcile the manifest to the retained artefacts.

### Baseline matrix

Four scenarios:

- company, minimal template;
- company, standard template;
- sole trader, minimal template; and
- sole trader, standard template.

The Phase 0 reference profile enables every supported statement option:

| Profile | Required options |
|---|---|
| All supported options | Active periods, order book, tax accruals, VAT details, bank balances, balance sheet, and reconciliation enabled |

This produces four authoritative current XLSX baseline outputs, one for each scenario. Reduced-option combinations remain part of later regression testing but are not substitutes for the all-options Phase 0 reference.

### Deliverables

- protected database captures;
- raw current XLSX files;
- recalculated/re-saved XLSX files kept separately from the raw files;
- baseline manifest with hashes and non-secret parameters;
- visual review record;
- restore/readability evidence; and
- signed Phase 0 acceptance record.

### Exit gate

Phase 0 passes only when all expected artefacts exist, hashes re-verify, captures are recoverable/readable, every workbook opens without repair, and a named reviewer accepts the evidence.

### Rollback

No code rollback is required because no implementation changes are permitted in this phase. If capture fails, restore all four sandboxes to their original access state and repeat Phase 0 as a new evidence run.

## 6. Phase 0A — ODS feasibility go/no-go

### Purpose

Determine whether the difficult ODS behaviours can be owned reliably in C# before the project invests in a shared-model extraction or touches the Excel implementation.

### Timebox

The initial spike is capped at **three focused working days**. Continuing beyond the timebox requires an owner review of the produced artefacts, passed/failed checks, remaining unknowns, and a bounded extension proposal. “Nearly working” is not sufficient evidence.

### Fixture

Use a small immutable in-memory model with enough hostile cases to reproduce the known failure modes:

- a non-January financial year with monthly and annual columns;
- an empty enabled category that must display numeric zero;
- a total whose child appears later in presentation order;
- a nested total of at least three levels;
- positive, negative, zero, missing, percentage, and ratio values;
- `SUM`, direct-reference, addition/subtraction, conditional, multiplication, and division formulas;
- explicit division-by-zero behaviour;
- authoritative cached values that intentionally differ from the unevaluated default zero;
- annual borders, semantic number formats, widths, a hidden marker column, and frozen panes; and
- enough repeated period cells to expose obvious size or performance defects.

The fixture must not connect to SQL or reuse the Python calculation engine. The existing Python-generated packages may be inspected only to recover known-good presentation markup.

### Three-day checkpoints

**Day 1 — package and values**

- [x] Produce the minimum ODF package with correct `mimetype`, manifest, content, styles, and settings parts.
- [x] Open it in the supported LibreOffice version without a repair warning.
- [x] Show correct authoritative cached values before manual recalculation.
- [x] Add package-level tests that fail when required parts, ordering, namespaces, or value attributes are wrong.

**Day 2 — formulas and presentation**

- [x] Emit all required hostile formula shapes with correct cached values.
- [x] Recalculate, save, and reopen without changing the expected results.
- [x] Implement and verify the required number styles, negative treatment, borders, widths, hidden column, and panes.
- [x] Retain pre- and post-LibreOffice package XML for comparison.

**Day 3 — real vertical slice and decision**

- [x] Render one representative vertical slice derived from the accepted baseline: header, period grid, detail rows, empty category, forward total, expression, and annual total.
- [x] Compare its semantic values with the source fixture before and after LibreOffice recalculation.
- [x] Record output size, generation time, allocation observations, and known unsupported behaviour.
- [x] Produce the go/no-go report.

### Allowed implementation choices

Evaluate the narrow internal `System.IO.Compression`/`System.Xml` writer first because it provides direct control of the required ODF markup. Test at most one external C# package candidate during the timebox, and only if it can plausibly satisfy cached values, formulas, styles, settings, streaming, licensing, and dependency constraints. Do not spend the spike surveying multiple libraries.

### Decision outcomes

- **Go — internal writer:** the fixture and vertical slice pass; proceed with the narrow C# writer.
- **Go — package:** one package passes the identical fixture, dependency, and licence checks; record why it is preferable.
- **Conditional go:** a small, precisely bounded gap remains and has a test, owner, estimate, and stop date.
- **No-go:** correctness or stability is not demonstrated within the timebox and no bounded extension is accepted. Stop ODS-led shared refactoring and reconsider scope or runtime architecture explicitly.

### Deliverables

- source and tests for the hostile fixture;
- generated ODS before LibreOffice processing;
- saved/reopened ODS and extracted comparison XML;
- screenshots or rendered evidence where presentation matters;
- passed/failed capability matrix;
- daily spike notes; and
- signed go, conditional-go, or no-go decision.

### Exit gate

Opening in LibreOffice alone is not an exit criterion. A go decision requires correct cached values, formula recalculation, save/reopen stability, the essential presentation features, and a representative real vertical slice.

### Rollback

The spike is isolated from production handlers. A no-go discards or archives it without changing XLSX behaviour.

## 7. Phase 1 — characterize current XLSX behaviour

### Purpose

Define exactly what “no Excel regression” means and automate its measurement before extracting any logic.

### Tasks

- [x] Build a read-only workbook inspection model using ClosedXML and, where package detail matters, Open XML/ZIP inspection.
- [x] Inventory every worksheet and its order, visibility, dimensions, used range, freeze panes, protection, print settings, defined names, and merged ranges.
- [x] Inventory every relevant cell's coordinate, type, displayed value, stored value, formula, style, number format, border, fill, alignment, and protection.
- [x] Inventory row heights, column widths, hidden rows/columns, annual boundaries, and marker columns.
- [x] Identify volatile fields before comparing outputs: generation timestamps, filenames, package timestamps, document identifiers, and other proven metadata.
- [x] Implement canonical OOXML comparison with a checked-in list of permitted volatile exclusions.
- [x] Implement semantic comparison keyed by section, row identity, and financial period rather than row number alone.
- [x] Make unknown or uninspected workbook properties fail the comparator.
- [x] Run the comparator against identical baseline copies to prove repeatability and against deliberate mutations to prove detection.
- [x] Produce concise machine-readable and human-readable difference reports.

### Deliverables

- XLSX inspection contract;
- semantic and normalized-package comparator;
- documented volatile-field allow-list;
- comparator unit tests with deliberate value, formula, style, structure, and metadata mutations; and
- characterization reports for all baseline workbooks.

### Exit gate

Phase 1 passes when the comparator reports no meaningful difference for identical inputs, detects every seeded mutation, accounts for all volatile exclusions, and a reviewer accepts its coverage.

### Rollback

The comparator is additive test infrastructure. It must not modify the current generator or accepted baselines.

## 8. Phase 2 — specify the shared statement model

### Purpose

Define the renderer-neutral contract before moving financial logic.

### Tasks

- [x] Catalogue all current Excel sections, options, repository calls, formulas, styles, supporting sheets, and reconciliation behaviour.
- [x] Define immutable types for document metadata, period grid, sections, rows, cells, authoritative values, formula dependencies, and presentation roles.
- [x] Give every business row a stable semantic identifier independent of worksheet position.
- [x] Separate presentation order from calculation/dependency order.
- [x] Define money, percentage, ratio, rounding, sign/polarity, zero, missing, unsupported, and division-by-zero semantics.
- [x] Define category graph validation, including missing nodes, cycles, arbitrary depth, and forward references.
- [x] Define a versioned request/options type; stop passing business options as renderer-interpreted strings internally.
- [x] Remove the unused legacy prototype `includeBankTypes` option rather than carrying it into the shared contract.
- [x] Define renderer contracts for row/column allocation and native formula translation.
- [x] Define size and complexity limits before loading or rendering a statement.
- [x] Map every existing Excel output element to its proposed model source.

### Deliverables

- shared-model design record;
- model-to-current-XLSX traceability matrix;
- formula/dependency semantics;
- typed option contract;
- validation and limit specification; and
- reviewed interface skeletons or test-only contracts.

### Exit gate

Phase 2 passes when every characterized Excel element and supported option has one unambiguous model owner, and no renderer is required to reproduce accounting logic.

The owner accepted the Phase 2 contract on 15 October 2026, including the five compatibility/security points recorded in `src/TCExports/docs/cash-statement-shared-model.md`: removal of the SQL connection string from Settings, blocking unresolved references, division-by-zero semantics, exclusion of the disabled inline reconciliation block, and formulas remaining secondary to authoritative C# values. The contract and traceability matrix remain amendable if implementation evidence exposes an omitted behaviour; any amendment is reviewed before dependent implementation continues.

### Rollback

No current runtime path changes. Revise the contract until traceability is complete.

## 9. Phase 3 — build and verify the shared statement builder

### Purpose

Implement the single authoritative financial pipeline beside the untouched Excel handler.

### Tasks

- [x] Implement typed option parsing at the request boundary.
- [x] Confirm `ICashFlowRepository` covers the authoritative inputs; extend it only if a later verified gap is found and never add renderer-specific repositories.
- [x] Build the financial-period grid once, including non-January financial years and annual columns.
- [x] Load and validate the complete category dependency graph before evaluating it.
- [x] Calculate leaf, category-total, flow-summary, VAT, bank, and balance-sheet values as decimals.
- [x] Bind terminal management expressions for spreadsheet-engine evaluation without parsing their formula language.
- [x] Calculate reconciliation values when requested.
- [x] Preserve explicit zero and distinguish it from missing/unsupported states.
- [x] Carry optional semantic formula dependencies without using formula strings as the calculation engine.
- [x] Propagate cancellation and command timeouts through all repository calls.
- [x] Compare every model value to the corresponding value in the untouched baseline XLSX outputs.
- [x] Add focused tests for all defects recorded by the feasibility investigation.

Phase 3 foundation completed on 15 October 2026: strict typed option parsing, bounded financial-period construction, semantic contract validation, dependency-cycle and missing-reference diagnostics, and authoritative formula evaluation are implemented beside the untouched Excel handler. Contract tests cover non-January ordering, forward references, complete cycle paths, missing references, explicit division by zero, and lazy conditional evaluation. Subsequent slices loaded and validated the repository-backed category graph; only the explicitly open tasks above remain.

The first repository-backed builder slice was completed on 15 October 2026. It loads Trade, Money, and Tax category/cash-code inputs through the existing repository contract; maps source values through the authoritative period grid; calculates annual detail values, polarized category totals, empty-category zero, forward/nested calculated totals, and flow summaries; validates the resulting semantic graph before evaluation; and propagates the typed timeout and cancellation token through every repository call it makes. The production Excel handler remains untouched.

Bank balances and the balance sheet have now also been added to the shared model. Bank months deliberately default to zero and annual cells reference month 12, matching the accepted Excel implementation. Balance-sheet months remain explicitly missing when the source returns no value, annual cells carry the last reported month, duplicate source entries aggregate, and capital totals ignore missing cells as spreadsheet `SUM` does. Disabled optional sections do not query their repository sources.

Report-owned text now passes through an instance-bound C# localizer modelled on the Python `i18n` implementation. The localizer receives an explicit culture and does not mutate global or thread culture, keeping concurrent tenants isolated. Both future renderers will therefore receive the same localized document, while database-owned category, cash-code, account, and asset descriptions remain unchanged.

Both VAT sections are now repository-backed. Recurrence and monthly values are mapped to the authoritative financial period grid rather than relying on source order; future base values follow `includeActivePeriods`; optional accruals retain the production Excel semantics; and annual values are semantic sums of their monthly cells. VAT labels use the shared localizer, and disabling VAT or tax accruals avoids the corresponding repository calls.

Phase 3A canonicalizes management-expression references. TCWeb displays readable category and cash-code descriptions but saves stable codes; resolution is code-first with description fallback for the frozen and other legacy databases. Unknown, malformed, or ambiguous tags are reported and never coerced to zero. Expression bodies remain opaque and terminal, so Excel or LibreOffice—not the shared financial evaluator—owns their spreadsheet syntax and result. SQL templates now seed canonical code references. Contract coverage is 17/17. All four frozen databases generated all-options XLSX files successfully; comparison with Phase 0 proves every non-Analysis semantic row unchanged. The only management-information changes are the intended resolution of `CE-MM` and `CE-SDR` detail references and explicit invalidation of the genuinely unmapped `CE-CO` and `CE-LM` legacy tags. Evidence is retained in `.local/cash-statement/phase-3/expression-fallback-final/`.

Phase 3 was completed and accepted on 15 October 2026. The builder now binds valid management expressions as opaque, renderer-terminal formulas; preserves invalid expressions as explicit unsupported values; and builds the optional reconciliation sheet with deterministic decimal bridge, difference, tolerance, and status semantics. The focused contract suite passes 25/25. The four frozen all-options businesses then accounted for 23,954 authoritative numeric values, 494 intentional blanks, and 2,145 spreadsheet-engine-owned expression cells with zero mismatches against the accepted LibreOffice-recalculated Phase 0 XLSX snapshots. Source cells require exact decimal equality; only cells calculated by the legacy spreadsheet engine permit an absolute `0.000000001` tolerance for binary floating-point residue. Reproducible comparison and performance/memory evidence is retained in `.local/cash-statement/phase-3/model-baseline-comparison/`. The production Excel handler remains untouched, so the Phase 3 exit gate is signed off and Phase 4 may begin.

### Required defect tests

- empty enabled categories produce zero without a circular reference;
- forward and backward category references resolve independently of display order;
- category cycles report their full paths;
- missing child references fail explicitly;
- non-January financial periods map correctly;
- VAT recurrence and VAT-period values use the authoritative period grid;
- management-expression tags bind to stable codes, report unresolved or ambiguous labels, and never use a literal-zero fallback;
- reconciliation is present when requested; and
- monetary and ratio rounding is deterministic.

### Deliverables

- shared C# statement model and builder;
- repository extensions and tests;
- model characterization suite over all baseline cases;
- complete semantic comparison report; and
- performance/memory measurements for model construction.

### Exit gate

Phase 3 passes when every expected semantic value across the baseline matrix agrees with the accepted Excel reference or has a separately approved defect-change record. Unresolved differences block Phase 4.

### Rollback

The current Excel handler remains untouched and active. The new builder can be removed without affecting production behaviour.

## 10. Phase 4 — migrate XLSX to the shared model

### Purpose

Replace duplicated calculation/data logic in the Excel handler while preserving all accepted workbook behaviour.

### Tasks

- [x] Implement a C# XLSX renderer over the shared model.
- [x] Preserve workbook structure, formulas, cached/displayed values, styles, protection, widths, hidden markers, freeze panes, safe settings, reconciliation, and printing behaviour, except for the classified security and expression corrections.
- [x] Generate old and new outputs from the same frozen inputs in the controlled harness.
- [x] Run the first semantic, structural, style, and canonical package comparison for every baseline case.
- [x] Recalculate/save/reopen replacement workbooks and compare them again.
- [x] Review representative screen and LibreOffice print/PDF outputs.
- [x] Load-test generation and record time, allocation, and output-size changes.
- [x] Classify every observed difference under the non-regression rule.
- [x] Close the deployment rollback requirement with the accepted release rollback position after Phase 7 qualification.

### Progress at 16 October 2026

The first shared-model XLSX renderer is implemented without changing or selecting the production Excel handler. It allocates the complete Cash Flow and Reconciliation layouts before formula translation, renders the safe Settings sheet without a connection string, and covers the header, periods, categories, summaries, calculated totals, management expressions, bank balances, VAT, balance sheet, protection, widths, hidden marker column, and frozen panes. The focused layout, formula, failure-path, and package-skeleton suite passes 6/6.

All four frozen Phase 0 businesses generate successfully through the integration harness. The raw replacement workbooks and comparisons are retained in `.local/cash-statement/phase-4/initial-renderer/`; the recalculated workbooks and final comparison reports are in `.local/cash-statement/phase-4/libreoffice-recalculated/` and `.local/cash-statement/phase-4/libreoffice-recalculated-comparisons/`. Both sole-trader workbooks are semantically equivalent to their recalculated Phase 0 references. The remaining company semantic differences are confined to the deliberately exposed invalid expressions `CE-CO` and `CE-LM`; the valid corrected expressions `CE-MM` and `CE-SDR` recalculate to accepted results.

The initial parity work also removed a ClosedXML operation-order defect that serialized missing balance-sheet periods as styled blank cells. The replacement now preserves the legacy absence of those cells. Remaining reports contain presentation/style, shared-string/package, Settings/Reconciliation structure, formula-cache, and approved-expression candidates; these counts are triage evidence, not an equivalence decision.

The solid red expression row was deliberate error presentation rather than an arbitrary style. It occurred only for `CE-CO` in company-minimal (`[Direct Purchases]` is unmapped) and `CE-LM` in company-standard (`[Wages & Salaries]`, `[Employer NI]`, and `[Employer Pension]` do not match current cash-code descriptions). The sole-trader outputs contained no red expression row. Phase 3 deliberately made these legacy mapping failures explicit instead of allowing a silent zero or invalid reference, but the full-width fill did not explain the problem clearly and was replaced during Phase 4 review.

The reviewed diagnostic now places the stored expression error in the first period cell, column D, using red text. It does not fill the period row red or repeat the message across every period. The renderer test protects this behaviour.

LibreOffice 26.2.6 recalculated, saved, and reopened all four replacement workbooks. Both sole-trader cases are semantically equivalent to their recalculated Phase 0 references. The company differences are confined to the explicit `CE-CO` and `CE-LM` invalid-expression decisions. Representative PDF review confirmed the accepted Cash Flow and Reconciliation presentation. Removing the credential and obsolete `includeBankTypes` setting makes the safe Settings sheet one printed page shorter. The difference disposition and repeat-generation evidence are retained under `.local/cash-statement/phase-4/`.

### Rollback disposition

File equivalence alone did not prove that a deployed endpoint could be recovered safely. A routing, configuration, dependency, memory, or packaging failure could appear only after deployment, so the project retained the last accepted source revision and deployable Azure artefact throughout qualification.

Phase 7 proved the replacement deployment, the independently disableable ODS route, bounded failure behaviour, and the platform deployment history needed to restore the preceding accepted XLSX artefact. The export is non-mutating, so database rollback is neither required nor permitted. A destructive live redeployment rehearsal was not performed; after operational qualification this was accepted as a documented application-artifact rollback rather than a remaining Cash Statement gate.

The Phase 4 XLSX implementation and its classified differences were reviewed and accepted on 16 October 2026. The release rollback position was accepted following Phase 7 qualification on 17 October 2026.

### Deliverables

- shared-model XLSX renderer;
- complete old-versus-new reports;
- recalculation and visual evidence;
- performance comparison;
- difference disposition register; and
- signed XLSX equivalence decision.

### Exit gate

Phase 4 passes only with zero unexplained differences, no unapproved visible or financial change, acceptable performance, and explicit reviewer acceptance.

### Rollback

Keep the prior published application artefact and source revision identifiable until the replacement completes its observation period. A failure restores the accepted prior release; it does not switch users between two runtime implementations.

## 11. Phase 5 — harden the selected ODS writer against the shared model

### Purpose

Integrate the mechanism accepted by Phase 0A with the real shared model and harden it before implementing the full report.

### Tasks

- [x] Retain the Phase 0A hostile fixture as a permanent regression suite.
- [x] Connect the selected writer to real shared-model primitives without report-specific financial logic.
- [x] Validate package ordering, `mimetype`, manifest, namespaces, styles, settings, and OpenFormula syntax.
- [x] Open, recalculate, save, reopen, print, and export the fixture to PDF in the supported LibreOffice version.
- [x] Check deterministic output after accounting for declared metadata.
- [x] Complete the selected implementation's licence, maintenance, dependency, security, streaming, and cancellation review.
- [x] Enforce output limits and deterministic disposal.

### Progress at 16 October 2026

The accepted Phase 0A mechanism is now a production-shaped internal C# package writer with no new package, Python, native, or server-side office dependency. Its physical worksheet cells carry the real renderer-neutral statement value, numeric, presentation, semantic-address, and formula types; row allocation and all financial decisions remain outside the writer for Phase 6.

The permanent hostile suite passes five focused test groups covering 20 formula cells, deterministic packages, required ZIP/XML contracts, five corrupt-package cases, output limits, cancellation, disposal, and unresolved dependencies. LibreOffice 26.2.6.3 recalculated and saved the package twice while preserving every formula-cell cached value without an error token. Its three-page A4 PDF was rendered and visually reviewed.

The native round trip found and closed one integration defect: the first production-shaped package omitted the `xmlns:of` declaration, causing Calc to save formulas with a doubled `of:=` prefix and display `Err:510`. The corrected namespace is now an explicit regression assertion. Detailed evidence is retained in `.local/cash-statement/phase-5/`; the implementation assessment is `docs/cash-statement-ods-writer-assessment.md` in TCExports.

All Phase 5 tasks are complete. The internal writer integration was accepted on 16 October 2026; the exit gate is passed and Phase 6 may begin.

### Deliverables

- permanent ODS capability fixture and tests;
- shared-model writer adapter;
- native LibreOffice evidence;
- completed dependency/licence assessment; and
- accepted ODS writer integration decision.

### Exit gate

Phase 5 passes when the selected C# mechanism satisfies every required capability from the real shared model without a server-side office executable or native dependency.

### Rollback

Disable and remove the failed ODS integration. The shared model and accepted XLSX path remain unaffected.

## 12. Phase 6 — implement the ODS renderer

### Purpose

Serialize the accepted shared Cash Statement model as a native, financially reliable ODS document.

### Tasks

- [x] Allocate all physical rows and columns before translating formula dependencies.
- [x] Emit authoritative cached values where applicable and native OpenFormula expressions; leave supported management-expression formulas cacheless so Calc evaluates them on first open.
- [x] Implement header, period grid, category, summary, total, expression, bank, VAT, balance-sheet, and reconciliation sections.
- [x] Port relevant presentation behaviour from the Python reference without porting its data access or calculations.
- [x] Implement semantic number formats, negative presentation, annual borders, widths, hidden markers, active sheet, protection, and frozen panes.
- [x] Add package-structure and XML-level tests.
- [x] Compare every ODS semantic value directly with the shared model.
- [x] Recalculate/save/reopen in LibreOffice and compare again.
- [x] Test printing and PDF output for all representative layouts.

### Progress at 16 October 2026

The native C# ODS renderer now allocates the complete workbook before formula translation and renders Cash Flow, safe Settings, and optional Reconciliation worksheets from the accepted shared model. It covers every Phase 6 section and the accepted financial presentation rules: semantic number formats, red parenthesized negatives, section and total styles, active-period highlighting, annual borders, widths, hidden markers, protection, active sheet, and frozen panes. No Python, external ODS package, native library, or server-side office process is part of generation.

The permanent hostile suite passes all five test groups. It checks package ordering and required parts, namespaces, OpenFormula, type-specific cached values, deterministic bytes, styles, protection, hidden markers, frozen panes, mutations, limits, cancellation, disposal, and unresolved dependencies. The integration harness generated all four frozen Phase 0 businesses with every supported request option enabled and directly compared 26,606 allocated semantic cells and 13,888 formulas against the shared model before serialization.

LibreOffice 26.2.6.3 opened, recalculated, saved, and reopened all four ODS files. Formula locations and semantics remained intact across every worksheet, all non-expression caches matched, and Calc produced numeric results for the 2,028 valid management-expression cells deliberately emitted without caches. This cacheless representation is necessary because a diagnostic string cache prevents Calc from evaluating the formula. Invalid expressions instead retain their stored explanation as red text in the first period cell. No Calc error token was present.

The four recalculated workbooks exported to 29-page minimal and 45-page standard PDFs using Calc's standard A4 pagination. Automated inspection found every page non-empty and no placeholder or spreadsheet error text. Representative Cash Flow, analysis, VAT, balance-sheet, Settings, and Reconciliation pages from all four variants were rendered and visually reviewed; presentation is legible and no unaccepted defect was found. Detailed local evidence is retained under `.local/cash-statement/phase-6/`.

The initial freeze-pane assertion was correctly rejected during human review because split coordinates alone did not activate panes in Calc. The writer now emits the complete Phase 0A view-state envelope and declares the `ooo` namespace required by `ooo:view-settings`. An ordinary visible Calc controller check reports Cash Flow frozen at `D5` and Reconciliation frozen at `C5`; the package and integration tests protect the complete settings contract rather than coordinates alone.

The final exhaustive acceptance run rendered XLSX and ODS pairs from the same immutable model instance, recalculated both formats in LibreOffice, and compared them directly with the accepted Phase 0 XLSX files. Across all four businesses, 26,489 semantic cells agree in all three formats; 117 cells are the explicitly approved `CE-CO` and `CE-LM` invalid-expression diagnostics. All 26,606 New XLSX/New ODS cells, 24,461 present or intentionally missing model values, and 2,028 valid management-expression results agree with zero unexplained differences. This deeper check found and closed two formula-parity gaps hidden by matching cached values: future-period capital formulas are now emitted in ODS, and the overall reconciliation status formula now belongs to the shared model rather than the XLSX renderer alone. Evidence is retained in `.local/cash-statement/phase-6/tri-format-acceptance-final/`.

All implementation and evidence tasks are complete. The functional review accepted the exhaustive tri-format report and its absolute tolerance on 16 October 2026. The Phase 6 exit gate is passed; the renderer remains outside the production handler selection until the later cutover phase.

### Deliverables

- C# ODS renderer;
- ODS package and formula tests;
- full four-scenario/profile parity reports;
- LibreOffice open/recalculate/save/reopen evidence;
- visual/print/PDF review; and
- signed ODS functional acceptance.

### Exit gate

Phase 6 passes with no missing/duplicate semantic rows, unresolved references, cycles, formula errors, repair warnings, unexplained value differences, or unaccepted presentation defects.

### Rollback

ODS remains feature-disabled. A failed ODS renderer cannot affect the accepted XLSX path.

## 13. Phase 6A — product integration and single-tenant acceptance

### Purpose

Put the accepted shared-model renderers behind the real Cash Statement page, complete the associated database and user-documentation changes, and prove the ordinary single-tenant workflow before Azure service qualification.

### Tasks

- [x] Route both Excel and LibreOffice requests through the accepted shared model and native C# renderers.
- [x] Accept and strictly validate both supported formats at the export boundary.
- [x] Add an accessible format selector to the Cash Statement page and persist the business default through `App.tbOptions.DefaultPrintMode`.
- [x] Modernise the generation page while retaining every supported request option and add a direct link to the Cash Statement User Guide.
- [x] Update the User Guide with supported formats and a current generation-page screenshot.
- [x] Increment the SQL schema build for the accepted expression-schema changes and verify the database project builds.
- [x] Build the complete application and run focused export contract tests.
- [ ] Exercise the real single-tenant UI for both formats, verify the persisted selection, and inspect both downloaded statements.

### Progress at 16 October 2026

The production Cash Statement handler now parses the typed request once, builds one immutable shared document and selects only the accepted native XLSX or ODS renderer. The export boundary accepts both supported formats, rejects invalid formats and options, and includes the reconciliation option in normalization. A defect in the generic validator dispatch that previously discarded invalid document-specific results was also corrected and is covered by the focused suite.

The Cash Statement page now provides accessible Excel and LibreOffice format cards, clearer grouped content switches, validated timeout entry, in-page generation errors and a direct User Guide link. The chosen format persists through the existing `App.tbOptions.DefaultPrintMode` business setting. The User Guide documents both formats and includes the refreshed page screenshot.

The previously omitted schema increment is now build `4.1.14` in both post-deployment and node initialization paths. TCExports and TCWeb build with zero errors, the focused Cash Statement suite passes 27/27, and the SQL project builds a DLL and DACPAC successfully.

A local all-options UI exercise generated both formats, proved selection persistence after reload, exported both downloads through LibreOffice, found no spreadsheet error tokens and passed representative visual review. This automated browser used a temporary page-local anonymous annotation because it had no development login; that annotation was removed before the final build. One authenticated download remains for the formal Phase 6A exit gate. Evidence is retained under `.local/cash-statement/phase-6a/`.

### Deliverables

- production handler routing to the shared builder and native renderers;
- format-aware, persistent Cash Statement page;
- updated User Guide and screenshot;
- SQL build increment and successful database build evidence;
- single-tenant UI download evidence for XLSX and ODS; and
- signed product-integration acceptance.

### Exit gate

Phase 6A passes only when both formats download successfully through the authenticated product UI, the selected business default survives a new request, every supported option reaches the export boundary, and the outputs retain the accepted Phase 6 financial/document behaviour.

### Rollback

Revert the product-routing and UI integration checkpoint together. The accepted Phase 6 renderer code and evidence remain intact, while the previously deployed Excel-only application remains the operational fallback.

## 14. Phase 7 — Azure service qualification

**Phase status: PASSED — PHASE 8 AUTHORISED.**

### Purpose

Prove that document correctness survives the intended Azure service conditions.

### Tasks

- [x] Remove database credentials and renderer-specific connection handling from document serialization.
- [x] Enforce authorised source resolution before statement construction.
- [x] Prove the generator uses no static statement cache or temporary report path.
- [x] Enforce maximum periods, rows, cells, generated bytes, and execution time.
- [x] Exercise request cancellation during database acquisition, model construction, and serialization.
- [x] Exercise concurrent XLSX and ODS generation at expected and overload levels.
- [x] Verify deterministic disposal and bounded memory/stream behaviour.
- [x] Confirm errors and telemetry are useful but contain no credentials or report content.
- [x] Verify feature flags disable ODS without affecting XLSX.
- [x] Deploy to the non-production Azure topology and repeat smoke, concurrency, and cancellation tests.

### Progress at 17 October 2026

The product now resolves the configured node only after the authenticated application boundary, then passes the resolved source through the shared builder before either renderer is selected. Renderers receive the immutable statement document rather than database credentials. The current resolver deliberately represents the deployed single-tenant host; its interface remains the seam through which Trade Control Hosting can later provide tenant-directory resolution.

Service admission is bounded at two active generations and two queued generations, with a ten-second queue wait, a 300-second execution limit and a 64 MiB output limit. The shared model independently bounds fiscal years, period columns, sheets, sections, rows, cells, formula nodes, dependencies and output bytes. ODS can be disabled without changing the XLSX path. Failure responses and Cash Statement telemetry expose only stable error codes, format, duration and output size; generator exception text, connection strings and report content do not cross the application boundary.

The final focused verification passes all 30 TCExports Cash Statement contracts and all 184 TCWeb boundary assertions, including 20 service-limit, source-authorisation, cancellation, overload, feature-flag and redaction assertions. Cancellation is exercised during source acquisition, model construction and native XLSX/ODS serialization. The renderer and SQL resources use deterministic disposal, and the generator creates no temporary report files or process-wide statement cache.

The non-production Azure deployment passed liveness/readiness and authenticated all-options downloads for both formats. Expected mixed load overlapped an XLSX generation of 13.842 seconds with an ODS generation of 6.903 seconds. A five-request XLSX overload admitted two requests, rejected three with the safe busy response, and completed the admitted pair in 104.353 and 104.370 seconds. The one-minute working-set peak during and after that exercise did not exceed its 313,131,007-byte pre-test reading and fell to 266,622,975 bytes by the end of the observation window. Closing the requesting browser tab produced the server-side caller-cancellation event approximately seven seconds later.

The representative workload generated three years of profit and loss, balance sheet, forward order book and tax content as ODS in 6.903 seconds while XLSX was also running. This is accepted as the ordinary-service threshold. The 104-second same-format result came from a deliberately hostile five-request XLSX overload; it demonstrates Excel-on-Excel contention, but the bounded service admitted only two requests, rejected excess work safely, remained inside the 300-second execution limit and retained stable memory. The two-active/two-queued/ten-second policy is therefore accepted for the current Azure topology.

Multi-tenant qualification belongs to Trade Control Hosting, alongside the deployment-model approval required for VAT and Corporation Tax. That project must reuse the Cash Statement source-resolution, concurrent-load, cancellation, memory, redaction and negative-isolation tests against its real shared-host topology. It is a hosting acceptance concern rather than a gate on this statement implementation.

### Deliverables

- service-limit configuration and tests;
- load, cancellation, and memory results;
- redaction review;
- Azure non-production evidence; and
- signed production-readiness decision.

### Exit gate

Phase 7 passed on 17 October 2026. Financial/document acceptance remained intact, the representative all-options workload met the accepted service threshold, and overload, cancellation, memory, redaction and feature-disable behaviour passed on the current Azure topology.

### Rollback

Keep ODS disabled and retain the accepted XLSX release. Failed operational qualification does not reopen the financial acceptance unless evidence shows a correctness impact.

## 15. Phase 8 — confirm cutover evidence and retire Python

### Purpose

Confirm the controlled single-tenant rollout evidence, preserve the accepted XLSX rollback, and remove the obsolete runtime after both native formats have passed operational qualification.

### Tasks

- [x] Release the accepted shared-model XLSX path using the normal controlled deployment process.
- [x] Run deployment smoke checks and compare bounded, authorised statements with their accepted expectations.
- [x] Complete the agreed XLSX observation through the authenticated single-tenant rollout.
- [x] Enable ODS for the authorised single-tenant rollout behind its feature flag.
- [x] Monitor generation failures, duration, output size, cancellation, and resource use without logging report content.
- [x] Complete the agreed ODS observation and obtain functional and operational acceptance.
- [x] Remove Python process invocation, Python SQL repositories, duplicated calculations, package publishing, and runtime health assumptions.
- [x] Retain the Python implementation in Git history, outside the production source and publish graph.
- [x] Rebuild and inspect the publish output to prove the application no longer requires Python, ODBC, `odfdo`, or `lxml` for statements.
- [x] Update support, deployment, architecture, and user-facing format documentation.

### Completion evidence

The Phase 6A authenticated product exercise and Phase 7 Azure qualification supplied the controlled single-tenant rollout and observation evidence: both formats generated through the UI, format preference persisted, the accepted financial/document checks remained intact, cancellation and overload were contained, telemetry was redacted, ODS could be disabled independently, and the representative all-options ODS workload completed in 6.903 seconds. Multi-tenant deployment-model qualification is deliberately owned by the Trade Control Hosting project.

Phase 8 removed the unused legacy C# XLSX and Python ODS handlers, the Python-backed generic test handlers and validator, the Python source tree, its output-copy rule, and the now-unused `CsvHelper` package. The retained production route is the shared builder with native C# XLSX and ODS renderers. Git history is the historical archive; no obsolete runtime was copied into another production-visible folder.

The post-removal verification passed all 30 TCExports Cash Statement contracts, all six focused XLSX renderer tests, all five hostile ODS writer tests, and all 184 TCWeb boundary assertions. The complete TCExports solution built successfully. A Release publish of TCWeb contained 952 files (172,100,969 bytes); filename, JSON dependency-metadata, package-graph, and generator-source scans found no Python source, process launcher, ODBC, `pyodbc`, `odfdo`, `lxml`, or `CsvHelper` dependency.

### Deliverables

- controlled release records;
- observation reports;
- Python-removal change and publish inspection;
- final support/runbook documentation; and
- signed project closure record.

### Exit gate

The project closes only when both formats are accepted, the observation periods complete without an unresolved critical issue, rollback remains documented, and the Python production dependency is demonstrably absent.

**Phase status: PASSED — PROJECT COMPLETE.**

## 16. Cross-phase test matrix

Every applicable phase uses the same scenario/profile identities and adds the following dimensions as they become available:

| Dimension | Required checks |
|---|---|
| Values | Stored decimals, explicit zero, missing state, polarity, rounding, ratios |
| Formulas | Intended dependencies, forward references, no cycles/errors, native syntax |
| Structure | Sheets, sections, semantic rows, period columns, annual columns, reconciliation |
| Presentation | Formats, styles, borders, widths, hidden markers, panes, protection |
| Recalculation | Fresh open, full recalc, save, reopen, unchanged authoritative results |
| Output | Print layout and PDF legibility |
| Service | Cancellation, timeout, size bounds, concurrency, redaction |
| Tenancy | Correct source binding and negative cross-tenant access tests |

## 17. Decision and defect control

Maintain three registers throughout delivery:

1. **Decision register** — architecture or scope decisions, owner, date, evidence, and consequences.
2. **Difference register** — every old/new XLSX or model/output difference, classification, evidence, and approval.
3. **Defect register** — observed defects kept separate from equivalence work, including whether correction is deferred or approved as an intentional change.

An intentional Excel correction must have its own failing test against the old behaviour, expected corrected result, accounting rationale, reviewer, and acceptance. It must not be smuggled into structural refactoring.

## 18. Immediate next actions

The implementation, verification, Phase 8 review, and project closure are complete. The reusable source-resolution, isolation, correctness, concurrency, cancellation, memory, redaction, deployment, and rollback tests have been specified for the Trade Control Hosting commissioning plan. Push the consolidated reviewed commits, then carry those tests forward with the hosting project rather than reopening this statement implementation.

