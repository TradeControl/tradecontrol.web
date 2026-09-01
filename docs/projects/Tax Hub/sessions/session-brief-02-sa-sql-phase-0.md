Review the current `specs/self-assessment-sql-node-spec.md` Revision 3 against the current `src/sqlnode` implementation and the completed findings/change-log history. Do not edit anything. Determine whether the Self Assessment SQL Node implementation is complete under the specification’s Definition of Completion and Acceptance Criteria. For every requirement that is not fully satisfied, identify the exact clause, the live evidence, and whether it is:

- an implementation omission;
- an empirical validation item not yet executed;
- an existing-instance deployment/migration issue;
- deliberately outside scope or superseded.

Return a concise verdict: **Complete**, **Implementation complete but acceptance pending**, or **Incomplete**. Do not propose Objective 3 work.

If there are no further changes to the schema required, I will:

1. **Deploy the schema changes to an existing instance**. This is where we prove the upgrade path rather than just fresh bootstrap behaviour, especially around StatutoryPolarityCode, new/retired Tax Sources and Tags, mappings, views/functions and any already-seeded historical objects.
2. **Bootstrap a minimal SA business and inspect it**. Run the real MIN MTD template, execute the new validator and cumulative projection fixtures, then inspect the accounting tree, Tax Source/Tags/mappings and representative figures—not merely whether the procedure returned successfully.
3. **Repeat with STD**. This is the more interesting proof because it exercises the thirteen detailed expense mappings rather than MIN's consolidated-expense path. We should reconcile representative values and verify that the same Objective 2 interface works purely from configuration.

