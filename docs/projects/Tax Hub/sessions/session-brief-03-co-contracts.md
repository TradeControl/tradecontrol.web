# Tax Hub — Limited Company Statutory Contract Design

## Purpose

We are implementing the UK statutory filing layer for Trade Control's Limited Company support.

The Tax Hub architecture separates:

1. Trade Control accounting and business data.
2. Statutory projection — Tax Sources, Tax Tags and accounting/category mappings.
3. Independent statutory contracts representing the external authority interfaces.
4. Population/adaptation from Trade Control data into those contracts.
5. Transport, authentication and submission.

For Sole Trader MTD Income Tax, the statutory projection was implemented before the external HMRC contracts were fully modelled. That worked, but subsequently required reconciliation of the SQL projection against the authoritative contract surface.

For Limited Companies we want to work in the opposite direction.

Before redesigning the existing company MIN and STD accounting templates, Tax Tags or statutory mappings, we want a clear Objective 3 design derived from the current statutory requirements. The eventual company accounting projection can then be designed deliberately to supply those contracts.

The existing company SQL/templates contain historical assumptions from the obsolete Government Gateway-era implementation. They are evidence only and must not determine the new external contract design.

## Primary Authority

Read in full:

`docs/projects/Tax Hub/specs/reference/company-field-sets.md`

This is the project's current research reference:

**Limited Company Statutory Projection Field Sets — UK Micro-Entities**

Treat it as the primary authority for this design exercise.

Where the document distinguishes between Companies House and HMRC requirements, preserve that distinction.

Where it records uncertainty, future changes, version boundaries or unresolved questions, preserve them rather than inventing a solution.

Do not reverse-engineer the current statutory contract from historical Trade Control SQL, Tax Tags, Gateway codes, payloads or other legacy implementation.

## Existing Sole Trader Implementation

Inspect the current Sole Trader Objective 3 contract implementation under the HMRC/SA area of the repository.

Use it only to understand useful project conventions such as:

- namespace and directory organisation;
- separation of contracts from Trade Control schema concerns;
- endpoint/contract metadata where applicable;
- serialization conventions;
- version handling;
- fixtures and offline contract testing;
- separation between contract modelling and transport.

Do not assume that the Limited Company implementation should structurally imitate SA.

Company filing has materially different statutory mechanisms and document/package requirements. The design must follow the company research and the actual authority boundaries described there.

## Stage 1 — Design Only

This task is reconnaissance and architectural design.

Do **not** implement the company contracts.

Produce one new document:

`docs/projects/Tax Hub/implementation/company-statutory-contract-design.md`

Do not modify any other file.

The document should propose the architecture for the Limited Company Objective 3 statutory contract layer.

It should be detailed enough that, after review and approval, a subsequent Codex session can implement the contracts without having to rediscover the architecture.

## Required Design Coverage

The design should address, where supported by the research paper:

- Companies House statutory accounts filing contract;
- HMRC Corporation Tax filing contract;
- the boundary between those two authorities;
- any genuinely shared statutory/accounting fact concepts without incorrectly merging the authority-specific contracts;
- CT600 and applicable supplementary return structures;
- statutory accounts;
- tax computations;
- iXBRL document requirements;
- XML/envelope/package structures where applicable;
- taxonomy and schema versioning;
- accounting-period and Corporation Tax period semantics;
- document/package composition;
- validation responsibilities;
- serialization/document-generation strategy;
- namespace and directory structure;
- proposed C# contract families and principal types;
- generated versus handwritten contract/model code, if generation is appropriate;
- treatment of taxonomy concepts, QNames, contexts, units, dimensions and other iXBRL concerns identified by the research;
- endpoint/submission metadata where applicable;
- fixtures and offline contract-testing strategy;
- handling of future or preview statutory versions without contaminating the current implementation.

Pay particular attention to whether a simple flat field/value DTO model is sufficient for each part of the company filing surface. Do not assume that the Tax Tag approach used for SA maps directly onto iXBRL or Corporation Tax filing.

## Trade Control Boundary

Objective 3 contracts must remain independent of the Trade Control database schema.

The design must not depend upon:

- `Cash.tbTaxTag`;
- `Cash.tbTaxTagMap`;
- existing company Tax Tags;
- Category/Cash codes;
- MIN or STD template structure;
- existing Government Gateway mappings;
- Trade Control SQL functions or procedures.

Those belong to the later statutory projection and population work.

The eventual purpose of this contract layer is to give that later work a stable external target.

## Existing Company Implementation

You may inspect existing company-related code and SQL where useful to understand repository history or identify legacy assumptions.

Treat it as historical evidence, not statutory authority.

In particular, do not preserve an existing abstraction merely because the old implementation used it.

If current code conflicts with the research reference, the research reference governs this design unless the conflict represents an explicit unresolved question that should be brought back for review.

## Decisions and Uncertainties

Do not silently resolve material ambiguities.

The design document must clearly distinguish:

- conclusions supported by the research;
- architectural recommendations derived from those conclusions;
- implementation choices that require our approval;
- statutory or technical questions for which the available research is insufficient.

End the document with a section:

`## Open Questions / Decisions Required`

Use this to surface anything that should be decided during the human review before implementation begins.

A small number of well-explained alternatives is preferable to choosing an unsupported architecture.

## Non-Goals

This stage does **not** include:

- C# implementation;
- SQL changes;
- Tax Source or Tax Tag changes;
- MIN/STD template redesign;
- Companies House or HMRC population logic;
- UI work;
- payload/package generation implementation;
- HTTP/network transport;
- authentication;
- live submission;
- sandbox submission;
- changes to the existing Sole Trader contracts.

Those belong to later stages.

## Completion

When complete:

1. Confirm that `company-field-sets.md` was read in full.
2. Confirm that the existing SA Objective 3 implementation was inspected for project conventions.
3. Confirm that no implementation work was performed.
4. Summarise the principal architectural recommendations and the important decisions requiring review.
5. Stop and wait for review.

Do not proceed to implementation.

## Git Safety

Do not stage, commit, push, amend, reset, revert, stash, or otherwise alter Git history or index state.

The only permitted working-tree change is:

`docs/projects/Tax Hub/implementation/company-statutory-contract-design.md`

Leave that file unstaged for user review.

The user will perform all Git operations separately.
