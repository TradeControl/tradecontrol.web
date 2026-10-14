# Self Assessment SQL Node — Phase 1 Session Brief

## Purpose

This is the operational brief for **Phase 1 — Structural Separation** of the Self Assessment SQL Node implementation.

Phase 0 reconnaissance is complete and reviewed. Its accepted evidence and decisions are recorded in:

`docs/projects/Tax Hub/findings.md`

The governing specification is:

`docs/projects/Tax Hub/specs/self-assessment-sql-node-spec.md`

The Tax Hub development documentation has been deliberately reorganised from its former `docs/tmp` locations into the permanent `docs/projects/Tax Hub` project structure.

These documentation moves are pre-existing project housekeeping and are not part of the Phase 1 implementation.

## Read before beginning

Read:

1. `docs/projects/Tax Hub/specs/self-assessment-sql-node-spec.md`
2. `docs/projects/Tax Hub/findings.md`
3. `docs/projects/Tax Hub/change-log.md`

The reviewed Phase 0 findings take precedence where they clarify live-state assumptions in the governing specification.

## Authority and scope

The complete `tradecontrol.web` checkout, including populated Git submodules, remains available for inspection.

Phase 1 implementation write scope is restricted to:

- `src/sqlnode`
- `docs/projects/Tax Hub/change-log.md`

Do not modify:

- `src/hmrc_mtd`;
- other submodules;
- unrelated superproject source;
- project documentation other than the change log;
- submodule pointers;
- repository history.

No commits or pushes are authorised.

## Assignment — Phase 1: Structural Separation

Implement Phase 1 only:

1. Remove obsolete Self Assessment/MTD tax-source, tax-tag, mapping and validation material from the MIN and STD accounting templates.
2. Correct comments in those templates that become stale or misleading as a consequence.
3. Remove the two stale `@IsMTD` forwarding arguments identified during Phase 0 from the MIN MTD and STD SA wrappers.
4. Preserve all unrelated accounting behaviour, procedure signatures, transaction/error conventions and repository boundaries.

Do not relocate or reinterpret the historical mappings.

## Explicit exclusions

Do not:

- add new Tax Tag mappings;
- select or redesign canonical QU, EOPS or SA vocabularies;
- modify the dedicated MTD or SA tax-seeding procedures except where an unforeseen compilation dependency makes this unavoidable — in that event, stop and report rather than expanding scope;
- make wrappers call the dedicated tax procedures; that is Phase 2;
- modify anything in `hmrc_mtd`, WebHarness, serializers, DTOs or payload handling;
- repair or promote SQL `Scripts` scratch material;
- begin the HMRC contract audit;
- begin Phase 2 or Phase 3 work.

The Category Tree remains an independent, customisable business classification structure. MIN and STD are bootstrap reference templates with different information granularity, not HMRC taxonomies.

No Phase 1 change should attempt to make MIN or STD resemble an HMRC taxonomy.

## Validation

After making the authorised changes:

1. Review the focused SQL diff for scope compliance.
2. Build the SQL database project and report the result.
3. Perform any safe, available static or isolated bootstrap verification that does not require inventing new test infrastructure.
4. Confirm that MIN and STD accounting templates no longer create Self Assessment/MTD tax sources, tags, mappings or validation calls.
5. Confirm that unrelated accounting behaviour has not intentionally changed.

If representative database execution is unavailable, state that explicitly rather than treating static inspection as runtime proof.

## Change record

Append a Phase 1 entry to:

`docs/projects/Tax Hub/change-log.md`

Record:

- the authorised objective;
- files changed;
- meaningful changes and why they were made;
- validation performed and its results;
- unexpected observations;
- anything deliberately left unchanged because it was outside Phase 1 scope.

Treat the change log as append-only project history. Do not rewrite earlier entries except to correct an objective factual error.

## Stop condition

Stop after Phase 1 implementation, validation and the change-log entry are complete.

Report the outcome and any issues requiring review.

Do not proceed to Phase 2.
