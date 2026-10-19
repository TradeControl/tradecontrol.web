# Cash Statement

This project establishes one authoritative Cash Statement definition and calculation pipeline, with Excel (`.xlsx`) and LibreOffice (`.ods`) as serialization formats over the same model.

The project is not an isolated LibreOffice addition. It affects the systems-critical Excel implementation and is governed by an explicit zero-unapproved-regression rule.

## Current status

Complete — Phases 0–8 were accepted on 17 October 2026. The shared C# statement builder is authoritative, the production XLSX and native ODS renderers consume the same immutable model, and the Python ODS implementation and production child-process dependency have been removed.

LibreOffice 26.2.6.3 remains the supported compatibility target. The accepted four-scenario comparison, tri-format semantic comparison, focused contracts, Azure qualification, and post-removal evidence are recorded in the [Cash Statement workplan](cash-statement-workplan.md).

The public Phase 2 records are the [shared-model contract](https://github.com/TradeControl/tradecontrol-exports/blob/main/docs/cash-statement-shared-model.md) and [XLSX traceability matrix](https://github.com/TradeControl/tradecontrol-exports/blob/main/docs/cash-statement-xlsx-traceability.md).

See [01-shared-statement-architecture-decision.md](01-shared-statement-architecture-decision.md) for the evidence, trade-offs, safety gates, and delivery sequence.

The executable project sequence, deliverables, acceptance gates, and rollback points are defined in [cash-statement-workplan.md](cash-statement-workplan.md).

## Accepted first gate

Before implementation began, all four reference sandbox scenarios were required to have a verified paired baseline:

1. a recoverable, point-in-time database snapshot or backup; and
2. current Excel Cash Statement outputs generated from that frozen database state.

The four scenarios are:

- company, minimal template;
- company, standard template;
- sole trader, minimal template; and
- sole trader, standard template.

The accepted baseline is independently restorable/readable, checksummed, and tied to the exact generator revision and request parameters.

Host-specific access-control procedures, exact database identities, and captured evidence are operational material and remain in the repository's ignored `.local` workspace rather than versioned documentation.

## Governing constraints

- XLSX and ODS consume the same authoritative data, period mapping, calculations, and semantic row model.
- Extracting the shared model must not cause an unapproved Excel value, formula, layout, style, protection, print, or interaction change.
- The Excel route remains the behavioural reference until the shared implementation passes the complete baseline comparison and is explicitly accepted.
- Renderers never receive database connection strings or perform accounting queries.
- Generated values are correct before office-suite recalculation; formulas provide traceability rather than acting as the accounting engine.
- ODS generation must run safely under concurrent, tenant-scoped ASP.NET requests in Azure.
- Production release of the ODS route remains subject to the normal controlled-release process; its financial parity and operational qualification gates are accepted.

## Source investigation

The starting feasibility record is [ODS Cash Statement Generation](https://github.com/TradeControl/tradecontrol-exports/blob/main/docs/ods-statement-generation.md). It established that native ODS output was feasible and identified correctness defects in the duplicate Python statement engine that has since been retired.

