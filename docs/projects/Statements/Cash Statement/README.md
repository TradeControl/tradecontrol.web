# Cash Statement

This project will establish one authoritative Cash Statement definition and calculation pipeline, with Excel (`.xlsx`) and LibreOffice (`.ods`) as serialization formats over the same model.

The project is not an isolated LibreOffice addition. It affects the systems-critical Excel implementation and is governed by an explicit zero-unapproved-regression rule.

## Current decision

The shared statement builder and both production renderers will be implemented in C#. The existing Python ODS implementation will be retained temporarily as a presentation reference and regression oracle, but it will not form part of the target Azure runtime.

Phase 0A is accepted. ODS will use the narrow internal C# writer proven by the hostile fixture and representative baseline slice, with no external ODS package. LibreOffice 26.2.6.3 is the supported compatibility target. Phase 1 characterization and the XLSX comparison contract are accepted. The Phase 2 shared-model contract and traceability matrix were accepted on 15 October 2026. Phase 3 implementation is underway beside the untouched production Excel handler.

The public Phase 2 records are the [shared-model contract](https://github.com/TradeControl/tradecontrol-exports/blob/main/docs/cash-statement-shared-model.md) and [XLSX traceability matrix](https://github.com/TradeControl/tradecontrol-exports/blob/main/docs/cash-statement-xlsx-traceability.md).

See [01-shared-statement-architecture-decision.md](01-shared-statement-architecture-decision.md) for the evidence, trade-offs, safety gates, and delivery sequence.

The executable project sequence, deliverables, acceptance gates, and rollback points are defined in [cash-statement-workplan.md](cash-statement-workplan.md).

## Mandatory first gate

No Cash Statement implementation or refactoring work may begin until all four reference sandbox scenarios have a verified paired baseline:

1. a recoverable, point-in-time database snapshot or backup; and
2. current Excel Cash Statement outputs generated from that frozen database state.

The four scenarios are:

- company, minimal template;
- company, standard template;
- sole trader, minimal template; and
- sole trader, standard template.

The baseline must be independently restorable/readable, checksummed, tied to the exact generator revision and request parameters, and recorded before source changes begin.

Host-specific access-control procedures, exact database identities, and captured evidence are operational material and remain in the repository's ignored `.local` workspace rather than versioned documentation.

## Governing constraints

- XLSX and ODS consume the same authoritative data, period mapping, calculations, and semantic row model.
- Extracting the shared model must not cause an unapproved Excel value, formula, layout, style, protection, print, or interaction change.
- The Excel route remains the behavioural reference until the shared implementation passes the complete baseline comparison and is explicitly accepted.
- Renderers never receive database connection strings or perform accounting queries.
- Generated values are correct before office-suite recalculation; formulas provide traceability rather than acting as the accounting engine.
- ODS generation must run safely under concurrent, tenant-scoped ASP.NET requests in Azure.
- The ODS route remains disabled for production reporting until financial parity and operational qualification are accepted.

## Source investigation

The starting feasibility record is [ODS Cash Statement Generation](https://github.com/TradeControl/tradecontrol-exports/blob/main/docs/ods-statement-generation.md). It establishes that native ODS output is feasible and identifies correctness defects caused by the current duplicate Python statement engine.

