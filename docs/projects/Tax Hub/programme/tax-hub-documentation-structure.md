# Tax Hub Documentation Housekeeping Proposal

14 October 2026

**Implementation status:** applied as a documentation-only migration on 14 October 2026. Counts and path observations below describe the pre-migration tree and are retained as the review record.

## Purpose and scope

This proposal reorganises the Tax Hub documentation so that a developer can find the current programme position and the governing material within a minute or two.

It is deliberately a small documentation-housekeeping exercise. It does not introduce a document-management system, mandatory metadata, new status registers or a hierarchy of document types. It does not change any statutory contract, implementation, review gate, external dependency or production restriction.

No files have been moved or edited as part of this proposal. The later migration should preserve document contents, repair paths and update the documentation entries in `src/tradecontrol.web.sln` as one documentation-only change.

## Recommended structure

Use a shallow set of directories based on the programme's stable architectural boundaries. Documents sit directly in their owning directory unless a real collection, such as Companies House approval evidence or archived sessions, justifies another level.

```text
docs/projects/Tax Hub/
  README.md
  change-log.md
  findings.md

  programme/
    tax-hub-spec-programme.md
    tax-hub-documentation-structure.md

  platform/
    README.md
    tax-hub-repo-structure.md
    tax-hub-test-payloads.md
    tax-hub-implementation-3.md
    tax-hub-workplan-4.md
    tax-hub-workplan-5.md
    tax-hub-data-provision-inventory.md
    tax-hub-data-readiness-matrix.md
    hmrc-transport.md

  mtd-income-tax/
    README.md
    self-assessment-sql-node-spec.md
    sole-trader-contracts.md
    sole-trader-field-sets.md
    sole-trader-capital.md
    sa-change-log.md
    sa-findings.md

  vat/
    README.md
    tax-hub-workplan-6.md
    phase-6.1-tcweb-hmrc-setup.md
    phase-6.2-vat-obligations-and-sandbox-alignment.md
    phase-6.3-vat-return-review.md
    phase-6.5-vat-filing-history.md
    phase-6.6-hmrc-approval-readiness.md
    phase-6.7-hmrc-production-approval.md
    brexit-vat-surface.md
    tax-hub-vat-surface-sql.md

  company/
    README.md
    shared/
      company-field-sets.md
      company-statutory-contract-design.md
      tax-hub-company-projection-inventory.md
    corporation-tax/
      README.md
      tax-hub-workplan-3.md
      phase-5.8-corporation-tax-service-artifact-assurance.md
      phase-5.9-transaction-engine-fixture.md
      phase-5.10-ct-external-test-local.md
    companies-house/
      README.md
      tax-hub-workplan-7.md
      companies-house-development-simulator-workplan.md
      phase-5.11-companies-house-prerequisite-assurance.md
      phase-7.0-companies-house-integration-boundary.md
      approval/
        README.md
        companies-house-test-submission-evidence.md
        STD-cash-statement-year-3-additive-transition.xlsx
        STD-cash-statement-years-1-and-2-baseline.xlsx

  ui/
    tax-hub-objective-5-hosting-decision.md

  archive/
    legacy/
      tax-hub-implementation-1.md
      tax-hub-implementation-2.md
      tax-hub-workplan-1.md
      tax-hub-workplan-2.md
      tax-hub-hmrc-repo-structure-2.md
    sessions/
      <existing session briefs, with their current filenames>
```

This is a target layout, not an instruction to create empty directories. The `ui` directory needs no README while it contains only the shared hosting decision; the root README can link directly to that document. The same rule applies elsewhere: add a short README only when a directory has enough material or status to navigate.

The older Objective 1 and 2 instructions and plans are proposed for `archive/legacy` because later reviewed programme, SQL Node and Objective 3–5 documents now govern the active work. Their contents remain intact and reachable. If review establishes that any still governs unfinished work, leave it with its active boundary and identify that fact in the relevant README instead.

## What moves and what stays

The following is sufficient for the migration; a separate per-file metadata manifest is unnecessary.

| Current material | Treatment |
|---|---|
| `change-log.md` and `findings.md` | Keep at the Tax Hub root, with all historical content intact. Continue using them for genuinely cross-programme matters. Add a stream-specific record later only when active work makes it useful. |
| `specs/tax-hub-spec-programme.md` | Move to `programme/`; it remains the programme-level governing specification. |
| This proposal | Move to `programme/` when the housekeeping change is implemented. |
| Repository architecture, HMRC transport, data-provision/readiness material, Implementation Guide 3 and Work Plans 4–5 | Move intact to `platform/`. Work Plans 4 and 5 are not split or copied. Stream READMEs link to their relevant headings. |
| MTD Income Tax SQL specification, sole-trader references and retained SA history files | Move to `mtd-income-tax/`, retaining filenames except for the space in `sa- findings.md`, which may be corrected mechanically to `sa-findings.md`. |
| Work Plan 6 and Phase 6 evidence | Move intact to `vat/`. Keep the two older VAT reference/history documents there and label them historical in the VAT README rather than adding another directory. |
| Shared company field-set, statutory-contract and projection documents | Move to `company/shared/`; they must not imply ownership by either CT or Companies House. |
| Work Plan 3 and Phase 5.8–5.10 CT records | Move intact to `company/corporation-tax/`. |
| Work Plan 7, the simulator plan and CH phase records | Move intact to `company/companies-house/`. Move the existing `companies-house-approval` directory beneath it as `approval`, preserving its contents. |
| Shared Objective 5 hosting decision | Move to `ui/`. VAT- and Companies House-specific UI plans remain with their authority stream. |
| Older Objective 1/2 instructions and plans, plus the superseded repository proposal | Move intact to `archive/legacy/`, subject to the governing-status check noted above. |
| `sessions/` | Move as a directory to `archive/sessions/`. Preserve the briefs and their original filenames; do not classify or rename all 31 files. |
| Empty root `session-brief.md` | Do not repurpose it as an index. Delete it only if the reviewer confirms that the zero-byte file has no external purpose. |

No historical entry is redistributed between journals. No document is decomposed. Where a large work plan covers several streams, the index links to the document and describes the relevant phase in words.

## Draft root README

The following is illustrative content for the new `README.md`. Status wording must be rechecked against the working tree immediately before migration because the active CT and Companies House documents are still changing.

```markdown
# Tax Hub documentation

Tax Hub projects Trade Control accounting data into supported statutory products. Start with the [programme specification](programme/tax-hub-spec-programme.md). Current HMRC or Companies House specifications remain authoritative for their external contracts.

## Current position

| Area | Current position | Governing and working documents | Next action or gate |
|---|---|---|---|
| MTD Income Tax | Objective 2 projection and the supported Objective 3 cumulative/obligations slice are implemented. Objective 4 phases 5.15–5.16 remain deferred. | [MTD Income Tax index](mtd-income-tax/README.md) | Human decision to resume Phase 5.15; begin with its contract-and-journey reconciliation. No implementation is authorised by this index. |
| VAT | Product workflow is technically approval-ready; production access, live filing and compatible-software status have not been granted. | [VAT index](vat/README.md) and [Work Plan 6](vat/tax-hub-workplan-6.md) | Await and respond to the HMRC approval process recorded in Phase 6.7. |
| Corporation Tax | Local service-artifact and External Test Service work exists, but the family gate remains closed and external recognition evidence is pending. | [Corporation Tax index](company/corporation-tax/README.md) | Complete the recorded human-review/external-test gates; do not enable production. |
| Companies House | Phase 7.6 is implemented and awaiting human review; Phase 7.7 is only partly complete and live activation is deferred. | [Companies House index](company/companies-house/README.md) and [Work Plan 7](company/companies-house/tax-hub-workplan-7.md) | Complete the existing review/status continuation before further history/recovery work. No second test filing or live test is authorised. |

## Shared material

- [Platform architecture and cross-cutting work](platform/README.md)
- [Shared company statutory material](company/README.md)
- [Shared UI hosting decision](ui/tax-hub-objective-5-hosting-decision.md)
- [Programme findings](findings.md) and [change log](change-log.md)
- [Historical and superseded material](archive/)

Historical session briefs record the instruction at the time. They are evidence, not current authority, unless an active document explicitly says otherwise.
```

This index deliberately summarises rather than duplicates the work plans. Each status statement links to the document that owns the detail.

## Draft MTD Income Tax README

This is the principal navigation improvement needed before the MTD Income Tax programme resumes.

```markdown
# MTD Income Tax

## Scope

Trade Control supports sole-trader filing through Making Tax Digital for Income Tax only. Legacy SA100/SA103F XML and EOPS are not supported filing paths. Historical documents may retain those terms to explain earlier decisions.

## What governs

1. Current HMRC MTD Income Tax specifications for external statutory meaning and API behaviour.
2. The [Tax Hub programme specification](../programme/tax-hub-spec-programme.md) for programme boundaries.
3. The [SQL Node implementation specification](self-assessment-sql-node-spec.md) for the accounting-to-statutory projection.
4. [Work Plan 4](../platform/tax-hub-workplan-4.md), especially the MTD Income Tax cumulative vertical slice and Objective 3 completion gate, for accepted preparation evidence.
5. [Work Plan 5](../platform/tax-hub-workplan-5.md), especially phases 5.15–5.16, for planned transport and handoff work.

Supporting references are [HMRC contracts](sole-trader-contracts.md), [field sets](sole-trader-field-sets.md) and [capital treatment](sole-trader-capital.md). The retained [SA findings](sa-findings.md) and [SA change log](sa-change-log.md) are historical evidence, not a later source of authority.

## Current position

Implemented and accepted:

- the MTD-only product boundary and retirement of the live legacy SA/EOPS direction;
- the Objective 2 MIN/STD cumulative accounting projection and mapping foundation;
- the Objective 3 self-employment cumulative PUT preparation for both profiles;
- the income-and-expenditure obligations request description; and
- immutable request bytes, validation, safe preview and handoff evidence.

Still deferred or incomplete:

- twelve body-bearing operations remain deferred and one preview operation remains unsupported in the accepted Objective 3 coverage record;
- annual adjustments, losses, calculation and final-declaration journeys do not yet have a complete reviewed product workflow;
- MTD Income Tax OAuth scopes and live transport remain disabled pending Work Plan 5 Phase 5.15; and
- Objective 5 hosted review, approval, submission, status and history have not been authorised for this stream.

## Restrictions that still apply

- Do not revive SA100/SA103F XML or EOPS as a filing stage.
- Do not infer unsupported statutory values or treat accounting data alone as authority for filing choices.
- Calendar-quarter preparation remains blocked until its dates come from an HMRC obligation-backed workflow.
- An endpoint's presence in the contract catalogue does not authorise population or sending.
- No production access, live request or product enablement follows from documentation housekeeping.

## Next authorised step

There is currently no authorised MTD Income Tax implementation phase beyond the accepted foundation. The next programme decision is to resume Work Plan 5 Phase 5.15. If authorised, its first bounded activity is the contract-and-customer-journey reconciliation described in that phase; operation promotion and implementation follow only from the resulting reviewed scope.
```

The final README should link directly to the relevant Work Plan 4 and 5 headings if stable explicit anchors are added. Until then, linking to the intact plan and naming the phase is less fragile than copying its content.

## Link, authority and migration risks

### Existing links

A path-level scan found 24 local Markdown link occurrences and no currently broken Markdown file targets. Most documents are nevertheless hard to discover because they are not linked from an index and many cross-references are plain code-formatted paths rather than links.

Every move will invalidate some of the 24 relative links unless they are changed in the same commit. The most connected documents are Work Plans 5 and 7 and the Companies House approval evidence; they deserve focused checking after the move.

### Existing solution entry

`src/tradecontrol.web.sln` currently contains one already-stale Tax Hub documentation entry:

```text
..\docs\projects\Tax Hub\implementation\companies-house-test-submission-evidence.md
```

The file actually lives under `companies-house-approval`. The solution currently lists 41 Tax Hub Markdown paths while 38 Markdown files, including this proposal, are not represented. The migration should replace the documentation solution-folder entries with the intended new navigation set; it need not add every historical session brief to the solution.

### Ambiguous authority

- `tax-hub-workplan-5.md` begins with the text `approved for productiion`, but the plan itself contains narrower gates and several explicit production restrictions. The typo and broad phrase are potentially misleading. The root and stream indexes must use the phase-specific status, not that header line. Correcting the header would be a separate reviewed textual correction, not part of the mechanical move.
- The older Objective 1/2 implementation instructions and work plans may contain historically valid unfinished items, but later specifications and Work Plans 4–7 now control the active architecture. Their archive placement should be confirmed once rather than inferred from dates alone.
- `brexit-vat-surface.md` reads like historical migration guidance. Keeping it in `vat/` with an index label of “historical reference” avoids a needless nested directory while preventing it from appearing current.
- The zero-byte `session-brief.md` has no content from which to infer a role.

### Migration risks

- A summary can accidentally promote “implemented” to “accepted”, or “technically ready” to “production approved”. Copy the exact gate language from the governing work plan.
- Moving Work Plans 4 and 5 without fixing their relative links would damage the strongest existing navigation chain.
- The solution file already has unrelated active changes. Update its documentation entries only after coordinating with that work, and do not alter projects or build configuration.
- Historical filenames contain legacy terminology and one filename contains a space. Preserve them unless a minimal rename fixes a practical path problem.

## Practical migration plan

1. **Confirm the small tree and governing labels.** Recheck the current phase tables in Work Plans 5–7, settle the three decisions below and prepare the root and stream README text against the last reviewed statuses.
2. **Move documents mechanically.** Use history-preserving moves, keep Work Plans 4 and 5 intact, move the sessions directory without renaming its briefs, and preserve the contents of findings, change log and approval evidence.
3. **Add navigation and repair paths.** Add the root, platform, MTD Income Tax, VAT, company, CT and Companies House READMEs; update relative Markdown links and the Tax Hub solution-folder entries. Make no source, statutory-contract or gate change.
4. **Validate and stop.** Check every local Markdown target, spot-check links into Work Plans 4–7, confirm all intended solution items exist, inspect the Git diff for content preservation, and open the solution. Stop for human review before any MTD Income Tax implementation begins.

## Decisions requiring human input

1. Approve or amend the shallow directory tree, particularly the proposed `archive/legacy` placement of the older Objective 1/2 instructions and plans.
2. Confirm whether the empty root `session-brief.md` may be deleted. Retaining it serves no navigation purpose, but deletion should be explicit.
3. Separately from this housekeeping proposal, decide when to authorise Work Plan 5 Phase 5.15. Moving its documentation must not be treated as that authorisation.

All other choices in this proposal are mechanical applications of decisions already accepted: `mtd-income-tax` naming, the shared-company boundary, intact Work Plans 4 and 5, preserved journals and sessions, authority-specific streams and unchanged review/external gates.

Implementation should stop for review after this proposal. No documentation move is authorised by the proposal alone.
