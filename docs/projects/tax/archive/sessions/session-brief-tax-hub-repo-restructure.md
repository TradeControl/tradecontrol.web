# Tax Hub — Repository and Assembly Architecture Review

## Context

We have reviewed and broadly approved:

`docs/projects/Tax Hub/implementation/company-statutory-contract-design.md`

The proposed company statutory contract architecture is sound, and the decisions identified there are accepted in principle for the current Objective 3 scope.

Before implementing those company contracts, we want to take advantage of the fact that the product has not yet been released and review the larger solution structure.

The repository is named:

`hmrc_mtd`

and currently contains a project also named:

`hmrc_mtd`

That project presently contains the existing Self Assessment, VAT and shared contract namespaces together with infrastructure, models and services.

The proposed company design introduces a separate neutral company-contract assembly. That raises a broader architectural question: whether the existing SA, VAT, shared HMRC contracts and remaining infrastructure/services should also be reorganised into clearer assemblies before the company contracts are added.

There is no compatibility requirement with a released product.

Renaming, splitting, merging, deleting or creating projects is acceptable if it produces a materially cleaner architecture.

## Objective

Design the overall repository/solution assembly structure for the UK Tax Hub before Limited Company contract implementation begins.

Consider the whole picture:

- Self Assessment / MTD Income Tax;
- VAT;
- Limited Company statutory contracts;
- Companies House contracts;
- HMRC Corporation Tax contracts;
- genuinely shared HMRC contract types;
- authority-neutral UK tax/company statutory concepts where appropriate;
- transport/infrastructure concerns;
- services;
- models;
- offline contract tests and fixtures.

The purpose is to establish clean dependency and ownership boundaries now, before more code is added.

## Sources to Inspect

Read:

`docs/projects/Tax Hub/implementation/company-statutory-contract-design.md`

Also inspect the current solution/project structure and the existing SA, VAT, Shared, Infrastructure, Models and Services namespaces/files.

Inspect project references and actual namespace dependencies rather than reasoning from directory names alone.

Use the existing SA Objective 3 implementation and tests as evidence of the current contract pattern.

The company design remains the approved direction unless this wider review identifies a genuine structural conflict.

## Design Questions

Determine:

- what assemblies/projects should exist;
- what each assembly owns;
- which namespaces belong in each;
- dependency direction between them;
- whether SA and VAT contracts should be extracted from the current `hmrc_mtd` project;
- whether current `Shared` content is genuinely shared and, if so, at what level;
- what should remain of the existing `hmrc_mtd` project;
- whether that remaining project should be renamed, split further or removed;
- where transport, authentication, endpoint/service metadata and common infrastructure should live;
- where company, Companies House and Corporation Tax contracts should live;
- where offline contract tests and fixtures should live;
- whether any current `Models` or `Services` namespaces are misplaced;
- whether the repository name `hmrc_mtd` is still appropriate once Companies House and Corporation Tax are first-class parts of the product.

Prefer explicit domain/authority boundaries over generic assemblies such as `Models` or `Services` unless those names genuinely describe a coherent responsibility.

Avoid excessive fragmentation. A separate project should represent a meaningful compile-time/dependency boundary, not merely a directory preference.

## Important Constraint

This review is architectural, not behavioural.

Do not redesign the statutory contracts already implemented for SA or VAT.

Do not change their external semantics.

Do not implement the company contracts yet.

The question is where those responsibilities should live and how the projects should depend on one another.

## Output

Create:

`docs/projects/Tax Hub/implementation/hmrc_mtd-repo-structure.md`

This is a separate architectural document. Do not modify:

`docs/projects/Tax Hub/implementation/company-statutory-contract-design.md`

Treat the approved company contract design as an input to this wider repository/assembly review.

The new document should clearly describe:

- the current repository/project structure;
- the proposed target structure;
- the responsibility of each proposed assembly/project;
- namespace ownership;
- dependency direction;
- proposed moves, renames, splits, mergers or deletions;
- the treatment of SA, VAT, Companies House and Corporation Tax;
- the appropriate home for genuinely shared contracts and infrastructure;
- what becomes of the existing `hmrc_mtd` project;
- any recommendation concerning the `hmrc_mtd` repository name;
- migration/refactoring risks, particularly circular dependencies;
- a concise proposed solution/project tree;
- decisions requiring user review before implementation.

Do not perform the restructuring.

Stop after creating the design document.

Include a concise proposed solution/project tree.

If you believe the repository itself should be renamed, treat that as a recommendation requiring explicit approval rather than assuming it.

## No Implementation

Do not create, rename, move or delete any source projects or files in this stage.

Do not modify `.sln`, `.csproj` or source files.

Only update:

`docs/projects/Tax Hub/implementation/hmrc_mtd-repo-structure.md`

Stop after producing the architectural proposal.

## Git Safety

Do not stage, commit, push, amend, reset, revert, stash, or otherwise alter Git history or index state.

Leave the Markdown change unstaged for user review.

The user will perform all Git operations separately.
