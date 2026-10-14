# Corporation Tax Project — Phase 1A: Current SQL State Reconnaissance

1 September 2026

The previous Sole Trader / MTD Income Tax implementation project has been completed and archived under:
`docs\projects\Tax Hub\implementation\history`

The active project is now Corporation Tax / Limited Company support.

Important context: unlike the Sole Trader work, there are currently **no C# Corporation Tax HMRC payload models or authoritative endpoint classes in this codebase**. The existing Limited Company SQL bootstrap predates the contract work we are about to undertake. Therefore do not assume that its Tax Sources, Tax Tags, mappings, names or submission structure represent current HMRC or Companies House contracts.

This phase is reconnaissance only. Do not edit anything.

Starting from `proc_Template_CO_MICRO_CUR_2026`, inspect its full dependency graph and report the current Limited Company tax implementation as it exists today.

Determine:

- the complete bootstrap call graph;
- all Corporation Tax / Companies House Tax Sources created;
- all Tax Tags, classes and polarity metadata;
- all existing Tax Tag mappings and their accounting roots;
- all extraction functions/views/readers that consume them;
- any references elsewhere in SQL or C# to those sources/tags;
- what statutory submissions or endpoints the existing design appears to have been intended to support;
- obsolete, duplicate, inconsistent or structurally suspicious material;
- any assumptions that conflict with the current generic Tax Tag architecture.

Do **not** yet decide that an apparent source/tag/endpoint is statutorily correct merely because it exists.

Return a factual inventory of the current implementation, followed by a short section headed:

**Questions requiring authoritative contract verification**

This should list every point that cannot be resolved from the repository alone.  
No implementation. No schema changes. No commits. Just record the results of your investigation in `docs\projects\Tax Hub\findings.md`.
