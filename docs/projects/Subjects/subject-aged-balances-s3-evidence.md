# Subject Aged Balances — Phase S3 Evidence

10 October 2026
**Status:** complete; automatically verified and accepted on IAM-01.

## Outcome

Phase S3 provides a read-only, authority-neutral year-end debtors and creditors evidence contract for later guided accounts preparation. Companies House and HMRC remain consumers of the evidence rather than owners of its accounting calculation.

The `subject-balances/v1` snapshot contains:

- the accounting effective date;
- an immutable, canonically ordered list of constituent Subject balances;
- gross debtor and creditor totals and counts without cross-Subject netting;
- the net business-polarity balance;
- reconciliation between the source aggregate and constituent population; and
- a deterministic SHA-256 snapshot token.

Each constituent retains its stable Subject code and name, native statement balance, business-polarity balance and debtor or creditor position. Zero balances are excluded. The contract rejects duplicate Subject codes, invalid polarity and mismatched native/business balances.

## Boundaries

The contract is implemented in the Tax UK Application layer and contains no Companies House, HMRC, credential, declaration or submission concept. TCWeb obtains the evidence through `Data.Subjects`, which composes Entity Framework LINQ over the centrally mapped `Subject.fnDatedBalances` function; no provider-specific SQL is embedded in the intermediary.

`Data.Subjects` deliberately performs separate aggregate and constituent reads. If relevant accounting data changes between those reads, the two populations do not reconcile and the snapshot is marked for review. A stable source produces the same canonical digest regardless of database row order; any effective-date, identity or balance change produces a different digest.

## Tax Hub consumption

The year-end Balance Sheet workspace presents a compact evidence summary containing:

- the effective date;
- gross debtor and creditor totals and Subject counts;
- reconciliation and closed-period state; and
- the immutable snapshot token.

It does not duplicate the Subject Browser's operational population. Its review link opens the Browser in Historical Balances mode with the same accounting period end preselected.

## Automated evidence

The Tax UK Application contract tests prove canonical ordering, gross totals, net balance, deterministic identity, digest invalidation, reconciliation failure on changed source data and rejection of incorrect polarity.

The TCWeb boundary tests prove that:

- `Data.Subjects` is the application intermediary;
- the SQL Server provider translates the dated-balance aggregate through `Subject.fnDatedBalances` with server-side `COUNT` and `SUM`;
- the Tax Hub renders the evidence summary and Browser deep-link without cloning the constituent list; and
- the Subject Browser accepts the Historical view and effective date from the deep link.

The Tax UK Application suite passes 75 assertions, the TCWeb Tax Hub suite passes 144 assertions, and TCWeb builds with zero warnings and zero errors.

## IAM-01 runtime acceptance

After restoring the Tailscale connection, TCWeb was run locally against the configured IAM-01 sandbox and the completed 2025–26 financial year was reviewed in the live interface. The year-end Balance Sheet displayed a reconciled snapshot at 30 September 2026 with five debtor Subjects totalling £43,339.76 and three creditor Subjects totalling £1,531.98. Its review link opened the Subject Browser in Historical mode with the same effective date and totals.

The complete path was repeated at a 390 by 844 phone viewport. The evidence card, historical date control, debtor/creditor position controls, totals, Subject population and responsive site header remained readable and operable.

The first direct-link attempt exposed a script-start ordering race: the interactive Blazor circuit could call the Subject Browser editor hook before the page had defined it. The page now defines its local JavaScript before starting Blazor, and balance-only mode does not initialise editor or split-pane hooks it does not use. The repaired direct link was retested successfully and the regression boundary is covered by the TCWeb test suite.

No Azure service or Azure SQL resource was started or changed. Phase S3 is accepted; Phase S4 prototype retirement can remain deferred while work returns to the Companies House wizard.
