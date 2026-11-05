# Category Tree controlled changeover test

## Status

**Executed locally on 5 November 2026; active and historical reconciliation acceptance passed.**

The COPY_ONLY backup passed independent verification before execution. The guarded scenario committed successfully, and acceptance proved that both the active successor and Archived predecessor generate without changing the active pointer. The first apparent historical Equity Bridge failure was stale pre-rebuild state. After the mandatory period-close rebuild, a version-explicit function hierarchy was proved exactly equivalent for the active version before the operational view became its active-version wrapper and TCExports used the same head function for historical selection.

## Purpose

This is the executable acceptance scenario for UI plan Step 5.5. It proves that a business can deliberately evolve its Category Tree and continue operating with the successor. It also proves that an immutable predecessor can be rendered honestly when later transactions use a Cash Code that predecessor never contained: the Cash Flow and the separate historical Equity Bridge both use the selected version, while the operational bridge remains active-rooted and unchanged.

The scenario is deliberately realistic. It combines a structural tree change with a new supplier, a bank transfer, acquisition and capitalisation of plant, four-year depreciation, period closure and active-versus-historical Cash Statement generation. The Archived selection now produces its own Equity Bridge, exposing the expected post-supersession classification discontinuity without altering the operational bridge.

## Scope and safeguards

- Target: local IAM-01 database `tcNodeDb4-COSIPFVT1-COSTD26` only.
- Azure, hosted tenants, accreditation and customer migration are out of scope.
- Execution script: [`CategoryTreeControlledChangeover.sql`](../../../src/sqlnode/src/tcNodeDb4/Tests/CategoryTreeControlledChangeover.sql).
- Governing step: [UI change plan, Step 5.5](category-tree-ui-change-plan.md#step-55--execute-the-controlled-local-changeover).
- The source script is inert with `@Execute = 0` and refuses the wrong machine, database, predecessor, period or backup identity.
- The complete scenario runs inside one outer transaction and rolls back if an executable postcondition fails.
- The test did not begin until the named backup had passed `RESTORE VERIFYONLY WITH CHECKSUM` under an appropriately privileged login.

Run the remaining verification on IAM-01 under a login permitted to verify restores:

```sql
RESTORE VERIFYONLY
FROM DISK = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup\CategoryTreeChangeover-20261105-120213-tcNodeDb4-COSIPFVT1-COSTD26.bak'
WITH CHECKSUM;
```

## Recovery point

| Field | Recorded value |
|---|---|
| Created | 5 November 2026, 12:02 GMT |
| Database | `tcNodeDb4-COSIPFVT1-COSTD26` |
| Backup set UUID | `C9CBCC6A-2142-48FE-BAEA-038815C645C2` |
| Database GUID | `0057597A-053D-4014-907A-AB5D3879128F` |
| Backup file | `CategoryTreeChangeover-20261105-120213-tcNodeDb4-COSIPFVT1-COSTD26.bak` in the IAM-01 SQL Server default backup directory |
| COPY_ONLY | Yes |
| Backup checksums | Yes |
| Backup size | 43,074,560 bytes |
| Compressed size | 3,898,321 bytes |
| `RESTORE VERIFYONLY` | **Passed — independently confirmed before execution** |

Before execution, the guarded script completed its preflight with `ExecuteGuard = 0`. A separate read-only query confirmed that the database contained two versions and none of `CA-INDCST`, `CC-PLANT` or Jones Machine Tools Ltd. No scenario business data was written by that validation.

## Starting state

| Item | Recorded value |
|---|---|
| Active version | `CV00000002` — Published |
| Active effective date | 4 November 2025 |
| Archived predecessor | `CV00000001`, 3 April 2024–3 November 2025 |
| Active accounting period | November 2026 |
| Current account | `CurrentAcc` |
| Reserve account | `Reserveacc` |
| Plant asset account | `PLATOO` — PLANT & TOOLS (`CC-DEPPL`) |
| Supplier namespace root | `Seller` |

The predecessor dates reflect the owner's deliberate manual effectivity test and are accepted as this scenario's baseline.

## Implementation narrative

1. Clone active `CV00000002` with today's date as the successor proposal.
2. Add nominal expense category `CA-INDCST — Indirect Costs` and link it beneath `CT-OVERHD — Overheads`.
3. Validate and publish the successor, atomically archiving `CV00000002` on the preceding date.
4. Add persistent Cash Code `CC-PLANT — Plant and Machinery` to the new active Published version and revalidate its canonical evidence.
5. Create supplier **Jones Machine Tools Ltd** beneath the Seller namespace.
6. Transfer £50,000 from Reserve to Current using the established synthetic transfer procedure.
7. Pay £40,000 for a lathe from Current using `CC-PLANT` and tax code `T1`.
8. Capitalise £40,000 into the existing PLANT & TOOLS asset account.
9. Add four annual £10,000 depreciation entries using the existing `CC-DEPPL` account classification.
10. Close November 2026 and assert the successor classifies `CC-PLANT` while the predecessor does not.

The paired purchase and asset-account entries follow `App.proc_DatasetSyntheticMIS_Assets`: the supplier payment records the cash cost, the asset-account receipt capitalises it, and the annual asset-account payments write it off over four years.

## Expected Cash Statement evidence

| Generation | Expected result |
|---|---|
| Active default | Success; identical to explicit successor selection. |
| Explicit successor | Success in XLSX and ODS; November includes the plant event and Settings records current canonical evidence. |
| Predecessor, periods before successor | Success with historical classification and orange headers only where applicable versions differ. |
| Predecessor, 2026–27 horizon containing November | Success without active-tree fallback; the version-explicit Equity Bridge exposes the discontinuity caused by `CC-PLANT`. |
| Any historical selection | `App.tbOptions.CategoryVersionId` remains the successor. |
| Draft selection | Rejected. |

## Recorded outcome

The executable scenario completed and committed all postconditions:

- `CV00000002` became Archived through 4 November 2026; `CV00000008` became the Published active successor from 5 November 2026.
- The successor canonical evidence is SHA-256 `65D459B35BA412CC48E9B96B9348A26A92995B50D98E5DCF3234E1051BD65BD7`, 11,757 bytes.
- `CA-INDCST — Indirect Costs` exists beneath `CT-OVERHD`, and `CC-PLANT — Plant and Machinery` is mapped only in the successor.
- Jones Machine Tools Ltd, the £50,000 Reserve-to-Current transfer, £40,000 supplier payment, £40,000 plant capitalisation and four annual £10,000 depreciation entries were created.
- November 2026 closed successfully.
- Active-default XLSX and ODS generation succeeded and recorded `CV00000008` plus its current canonical evidence in Settings.
- Explicit predecessor XLSX and ODS generation succeeded. Settings recorded selected `CV00000002`, active `CV00000008`, both evidence sets and only the periods whose historically applicable version differs from the selection.
- Before the mandatory close-period rebuild, a predecessor workbook appeared to fail the 2026–27 Equity Bridge by £80,000.04. That result was stale and is not accepted as evidence.
- After rebuild, `Cash.vwEquityReconciliationByYear` reports 2026–27 profit of £91,006.39, profit after tax of £81,315.18 and variance of £0.04, so the active bridge passes. `App.vwTaxBizCashCodes`, `App.vwTaxVatCashCodes`, `Cash.vwProfitAndLossByYear` and the dependent tax computation remain rooted through `App.tbOptions.CategoryVersionId`.
- `Cash.fnEquityReconciliationByYearByVersion` matched every active-version row and value in both directions, using a source-controlled acceptance script. With `App.tbOptions.CategoryVersionId` supplied at the top, warm executions took 608–704 ms against 588–703 ms for the original view. `Cash.vwEquityReconciliationByYear` is now the active-version compatibility wrapper over that single algorithm.
- With `CV00000002` selected, the version-explicit 2026–27 bridge reports profit of £124,339.72, profit after tax of £108,315.17, capital delta of £81,315.22 and variance of -£26,999.95. The Archived tree omits the £33,333.33 net plant cost; the resulting £6,333.34 additional corporation tax reduces the after-tax discontinuity to £26,999.99, which combines with the accepted active bridge variance of £0.04 to produce exactly -£26,999.95. That is the intended visible discontinuity after the version was superseded; it is no longer masked by an active-rooted bridge.
- TCExports retains the view for active/default generation and queries the same head function through `Cash.tbCategoryVersion` for an explicit non-active selection. The obsolete Cash Statement projection `Cash.vwFlowReconciliationByYear` and the experimental duplicate procedure were removed. Active and Archived generation both returned successful XLSX and ODS packages against the IAM-01 database.
- A final database query confirmed `App.tbOptions.CategoryVersionId = CV00000008`; historical generation did not repoint the business.

### Build 4.1.20 Equity Bridge validation

The source-controlled [`CategoryTreeEquityReconciliationBuild20.sql`](../../../src/sqlnode/src/tcNodeDb4/Tests/CategoryTreeEquityReconciliationBuild20.sql) preserves the committed build `4.1.19` view body as test evidence and compares it, column-for-column and in both directions, with the build `4.1.20` active wrapper and active-version function. A bounded reporting-only uplift was applied transactionally to each backed-up local build `4.1.19` database and committed only after this gate passed:

| Database | Shape | Years | Maximum absolute variance | Result |
| --- | --- | ---: | ---: | --- |
| `tcNodeDb4-COMIPFVT1-COMIN26` | Company MIN | 3 | £0.08 | PASS |
| `tcNodeDb4-STMIPFVT1-STMIN26` | Sole trader MIN | 3 | £0.04 | PASS |
| `tcNodeDb4-STSIPFVT1-STSTD26` | Sole trader STD | 3 | £0.04 | PASS |

There were no missing, additional or changed rows on any shape. All three databases now report build `4.1.20`.

The guarded, dry-run-by-default [`CategoryTreeEquityReconciliationLossDataset.sql`](../../../src/sqlnode/src/tcNodeDb4/Tests/CategoryTreeEquityReconciliationLossDataset.sql) records the destructive company loss fixture. A fresh COPY_ONLY backup with SQL Server checksums completed before regeneration; the restricted test login could not execute `RESTORE VERIFYONLY`, while the earlier independently verified recovery point remains available. The accepted fixture used the Standard company template, a `0.5000000` selling-price ratio, two completed years and the 5 November 2026 test anchor. It produced three loss years, zero corporation-tax expense, increasing carried-forward losses of £38,972.19, £79,242.47 and £110,536.48, and Equity Bridge variances of £0.00, £0.02 and £0.04. The complete build `4.1.19` body, build `4.1.20` wrapper and version-explicit function were identical.

Two design corrections arose during acceptance. Pre-generation compatibility gates were removed because they prevented deliberate historical rendering; eligibility checks for existence, lifecycle status and effective interval remain. Reconciliation is now explicit only at the historical statement boundary. The operational Equity Bridge remains the accounting-system contract and is guarded by active-version equivalence and timing tests before any future change is considered.
