# Corporation Tax marginal relief and real-time liability work plan

## Status

This plan is a design and verification workstream within CT-A. It exists because the current-period
synthetic company falls inside the post-April-2023 marginal-relief band and has exposed the limits of
the legacy fixed-rate business-tax calculation.

Environment names, tenant identities and measured tenant values belong in protected operational
evidence rather than this public document.

## Objective

Evolve the existing business-tax statement so that it:

- calculates UK Corporation Tax correctly across the small-profits, marginal-relief and main-rate
  regions;
- responds immediately to accounting transactions, confirmed Projects and What-if Projects;
- preserves the executable MIS prototype embodied in the existing Project, Object and statement
  schema;
- keeps common statement mechanics separate from jurisdiction-specific tax law;
- leaves a practical extension point for another jurisdiction without imposing a generic framework or
  a performance penalty on the UK implementation; and
- continues to supply the approved Corporation Tax preparation and service-artifact pipeline without
  introducing a second statutory serializer.

This plan implements the UK Corporation Tax profile only. Jurisdiction awareness is an architectural
portability test: a sound boundary should accommodate a future regime with different classifications,
rates and instalment rules without changing the Company Statement.

## Architectural constraints

1. **Preserve MIS Mode.** `Cash.vwStatement`, `Cash.vwStatementBase`,
   `Cash.vwStatementWhatIf`, `Project.tbProject`, Project cost views and the Object/BOM foundation are
   executable product architecture, not disposable prototype debris.
2. **Account Mode is a surface, not a separate accounting engine.** The launch UI may hide Projects and Objects,
   but its Corporation Tax calculation must use a path that can consume them when MIS Mode is exposed.
3. **Transaction-grained means transaction-responsive.** Posting, changing or removing an actual,
   confirmed-order or scenario event must immediately change the projected liability and Company
   Statement. A month-end batch calculation is not acceptable.
4. **Tax is non-linear.** Marginal relief cannot be represented by multiplying every order by one
   `BusinessTaxRate`. Forecast tax must be the change in the period liability produced by the forecast
   facts.
5. **One statutory result.** Accounts, computation, CT600, service artifact, Company Statement and
   What-if projection must reconcile to the same reviewed calculation kernel and semantic inputs.
6. **Fail closed.** Missing jurisdiction facts, an unsupported company profile or an indeterminate tax
   treatment must produce a diagnostic/readiness failure, not silently select 19%, 25% or an inferred
   adjustment.
7. **No speculative schema abstraction.** Reuse the existing `App.tbJurisdiction`, authority,
   reporting-profile, Category Tree and tax-type foundations. Add schema only when a reviewed use case
   cannot be represented coherently by them.

## Existing statement path

The present executable path is:

```text
accounting periods and Category Tree totals
                  |
                  v
Cash.vwTaxBizComputationByYear
                  |
                  v
Cash.vwTaxBizStatement ---- business-tax payments
                  |
                  v
Cash.vwStatementBase ---- invoices ---- confirmed Projects ---- transfers
                  |
                  v
Cash.vwStatement
                  |
                  +---- Cash.vwStatementWhatIf ---- quotation/scenario Projects
```

`Cash.vwTaxBizStatement` is the correct integration boundary: it combines dated liabilities with
settlements and exposes a running tax balance. It must remain concerned with statement events. The UK
marginal-relief formula belongs immediately upstream in a jurisdiction-selected liability projection.

The initial change should therefore preserve the public shape of `Cash.vwTaxBizStatement` wherever
possible and replace its assumption that `Cash.vwTaxBizComputationByYear` is a universal fixed-rate
calculation.

## UK semantic model

For the supported ordinary-company profile, the calculation must distinguish at least:

- **N — taxable total profits:** the amount on which Corporation Tax is charged after reviewed tax
  adjustments and reliefs;
- **D — relevant distributions:** the separate distribution measure required when determining
  augmented profits;
- **A — augmented profits:** derived from the statutory N and D inputs, not stored as an independent
  accounting balance;
- accounting-period start, end and day count;
- the applicable financial-year rates, lower and upper limits, and standard fraction;
- the reviewed associated-company divisor;
- brought-forward loss and relief treatment;
- eligibility facts, including residence, close-investment-holding-company and ring-fence status; and
- an explicit reviewed monetary adjustment with provenance where the supported calculation cannot
  derive an amount automatically.

N and D require distinct Category Tree semantic roots in the UK profile because they classify different
facts. A is derived. These names must not become universal meanings for every jurisdiction.

For a full 12-month period, no associated companies and no ring-fence profits, the current ordinary
company calculation is expected to follow:

```text
A <= L              tax = small-profits rate * N
L < A <= U          tax = main rate * N - F * (U - A) * N / A
A > U               tax = main rate * N

where L = 50,000, U = 250,000 and F = 3/200 for the current reviewed years.
```

The limits must be adjusted for short accounting periods and associated companies. Financial-year
rate changes, distributions, losses and eligibility rules must be tested from authoritative examples;
the simplified expression above must not be stretched beyond its stated assumptions.

## Proposed MR-1 calculation contract

The common statement boundary should expose liability events, not UK tax-law inputs. Its minimum
contract is:

| Field | Meaning |
| --- | --- |
| Jurisdiction and regime | Selects the provider once for the calculation set. |
| Tax period and effective date | Identifies the liability period and the dated statement event. |
| Basis | Actual, confirmed forecast or What-if scenario. |
| Liability amount | The cumulative liability produced by the selected provider. |
| Settlement amount | Payments, kept separate from tax expense. |
| Readiness and finding code | Makes missing or unsupported facts fail closed. |
| Calculation/source identity | Reproduces the result and supports immutable evidence. |

For the initial UK ordinary-company provider, the inputs and ownership are:

| Input | Ownership and derivation |
| --- | --- |
| N — taxable total profits | Derived from transaction-grained accounting profit, reviewed tax adjustments, capital allowances, gains and losses. It is not a user-entered rate-band selector. |
| D — relevant distributions | A distinct UK tax classification mapped through the Category Tree/tax-tag system. It must not be inferred from generic other income. |
| A — augmented profits | Derived as N + D and never stored as an independent accounting balance. |
| Associated-company count | Effective-dated reviewed reporting-profile setting; the value is the number of other associated companies. |
| Residence, close-investment-holding and ring-fence status | Effective-dated reviewed reporting-profile settings used as eligibility gates. |
| Rates, limits and fraction | Versioned UK statutory parameters selected once for the applicable financial-year part. They are not transaction attributes. |
| Manual monetary adjustment | Explicit reviewed filing input with provenance; never an automatic substitute for an unsupported calculation. |

The existing effective-dated reporting-profile settings and their review/provenance fields can hold
the non-transactional company facts without schema expansion. Proposed setting identities are
`CT-ASSOCIATED-COMPANIES`, `CT-UK-RESIDENT`, `CT-CLOSE-INVESTMENT-HOLDING` and
`CT-RING-FENCE-PROFITS`. They must be created unreviewed and block readiness until a responsible user
confirms them.

The UK kernel accepts explicit N, D, period, associated-company count and statutory parameters and
returns its branch, adjusted limits, tax before relief, marginal relief, final liability, effective
rate and a diagnostic code. It contains no statement, Category Tree, tenant or jurisdiction lookup.
The initial supported parameter window is one in which the rates, limits and fraction are uniform for
the calculation passed to it. A period requiring different financial-year parameters must be split
under the statutory rules before kernel invocation or fail closed; silently applying one rate to the
whole period is forbidden.

## Real-time and What-if calculation

The invariant is a liability function over the facts visible at a point in the statement:

```text
T(jurisdiction, company facts, tax period, cumulative tax measures)
```

For a deterministically ordered stream of tax-relevant events, the tax effect of event `i` is:

```text
tax delta(i) = T(cumulative facts through i) - T(cumulative facts through i - 1)
```

This supports movements into and out of a rate band and does not pretend that an order has a permanent
flat tax rate. The corresponding aggregate projections are:

```text
confirmed-order forecast = T(actual + confirmed) - T(actual)
What-if forecast          = T(actual + confirmed + scenario) - T(actual + confirmed)
```

Implementation should favour set-based windowed accumulation or an inline relational expression. It
must not invoke an expensive scalar calculation once per statement row. Same-day ordering and reversal
rules must be deterministic, while the final liability must be invariant under ordering of facts with
the same effective date.

## Jurisdiction boundary

The common statement layer should consume a small liability-event contract rather than tax-law fields:

- jurisdiction and regime identity;
- tax period;
- event/effective date;
- actual, confirmed or scenario basis;
- liability or instalment amount;
- settlement amount;
- status/readiness and calculation provenance; and
- stable source identity sufficient to reproduce the result.

The UK provider may calculate N, D, A and marginal relief. A future jurisdiction provider may require
entirely different tax-base measures, entity classifications, offsets and payment schedules. Neither
the UK concepts nor hypothetical future concepts belong in the common Company Statement.

Jurisdiction selection must occur once per calculation set from the node's reviewed statutory context.
It must not add a jurisdiction lookup or branch to every transaction row. A second jurisdiction should
be implemented only when its authoritative rules, supported company profile and test corpus are in
scope.

## Delivery phases

### MR-0 — exercise the kernel on the existing company with D = 0

**Status:** Complete. The committed kernel is deployed to the retained CT test database without any
statement or Category Tree change. Both regression scripts passed, the existing D = 0 annual result
was reconciled to the marginal-relief branch, and protected performance evidence includes the observed
outlier rather than smoothing it away.

1. Deploy only `Cash.fnTaxCorpMarginalReliefUK` to the retained CT test database without changing the
   active statement views or Category Tree.
2. Execute `CorporationTaxMarginalReliefBoundaries.sql` and
   `CorporationTaxMarginalReliefAuthority.sql` against that database.
3. Read the existing annual N from `Cash.fnTaxBizComputation`, pass D = 0 and zero other associated
   companies to the kernel, and record every returned component.
4. Reconcile the kernel result independently to the expected marginal-relief liability while retaining
   the existing flat-rate statement result as the control.
5. Capture elapsed time, CPU, logical reads and plans for the current CT and Company Statement queries.
   Do not infer missing measurements.

**Gate:** passed. The real database executed the kernel and both regression scripts successfully; the
D = 0 result and the unchanged legacy flat-rate control are retained in protected evidence.

### MR-1 — add the D classification to the default Category Tree

**Status:** Complete. A fresh company bootstrap on the retained CT test node produced the published
N, VAT and D roots, the `CT-DIST -> CA-DIVRC -> CC-DIVRC` classification and a supported zero-D
projection. The web interface marks the D hierarchy recursively and withholds D-root controls from
sole traders. Both rollback-only Category Tree/projection fixtures pass on the populated multi-year
dataset. `App.tbOptions.CategoryVersionId` selects the active tree version; roots remain in
`Cash.tbCategoryRoot`, so no new `App.tbOptions` column was required.

1. Extend `Cash.tbCategoryRootType` with a third root type for relevant distributions and update its
   current `RootTypeCode <= 1` constraint.
2. Add a relevant-distributions total root to the default company Category Tree and register it in
   `Cash.tbCategoryRoot` for each new tree version.
3. Add a received-dividend nominal category and cash code. Make that category a child of both the
   accounting Other Income branch and the relevant-distributions root, following the existing
   multi-root pattern used by the Profit and VAT trees.
4. Add a `RelevantDistributions` tax tag and map it to the D branch. Do not infer D from all Other
   Income or from dividends paid to the company's shareholders.
5. Add effective-dated reporting-profile definitions for associated-company count, UK residence,
   close-investment-holding status and ring-fence status. Seed them unreviewed so CT readiness fails
   closed until a responsible user confirms them.
6. Extend Category Tree canonicalisation, validation, change-control and template tests for the third
   root without weakening submitted-version immutability.

**Gate:** passed. The newly initialised company contains the N, VAT and D roots; D is zero with no
received-dividend transaction; the active mapping resolves to `CC-DIVRC`; and all Category Tree and
company-projection integrity tests pass. The repeatability fixture also corrected two assumptions
exposed by Azure managed identity and the populated multi-year dataset.

### MR-2 — add received dividends to the synthetic dataset

**Status:** Complete. The independently switchable generator now produces one deterministic,
non-VAT `CC-DIVRC` receipt for each closed company year, while `@EnableDividends = 0` retains the
permanent zero-D control. A fresh retained-company bootstrap produced £4,992 D for 2024-25 and
£5,350 D for 2025-26. The acceptance fixture reconciles every posted receipt to D, deducts the exempt
distribution from accounting profit to obtain N, proves `A = N + D`, and verifies that D changes the
marginal-relief result without entering VAT.

1. Add an external investee/payer and deterministic incoming dividend payments to the synthetic MIS
   dataset using the received-dividend cash code and a non-VAT treatment.
2. Prove the receipts appear in accounting Other Income while the same transactions are selected
   separately as D.
3. Derive the corresponding exempt-distribution deduction when calculating N, so accounting profit is
   not taxed merely because the dividend appears in the accounts.
4. Prove A = N + D and demonstrate that adding, amending or reversing a dividend changes A and the
   marginal-relief result without corrupting accounting profit or VAT.
5. Retain a zero-D synthetic case as a permanent regression alongside the positive-D case.

**Gate:** passed. Both zero-D and positive-D modes are deterministic and independently reconcilable.
For 2025-26 the retained test company produced accounting result £216,844.79428, N £211,494.79428,
D £5,350 and A £216,844.79428; the kernel returned £485.05800 marginal relief and £52,388.64057
Corporation Tax chargeable. All Category Tree, company-projection and relevant-distributions fixtures
passed after the positive-D bootstrap.

### MR-3 — replace the flat-rate actual CT statement calculation

**Status:** Complete. MR-2 established the reconciled positive-D input and kernel control. The first
MR-3 increment introduced
optional UK statutory parameters per financial period. The second adds
`Cash.vwTaxCorpLiabilityUK` alongside the live statement: it derives N and D, applies the existing
loss policy, invokes the marginal-relief kernel and separates tax-accrual readiness from filing
readiness. The positive-D retained-company fixture reconciles through this view and the rollback-only
readiness fixture passes. Removing the transitional dependency on the legacy recursive annual view
reduced the projection from 12,934 to 8,916 logical reads while preserving the legacy two-scan
accounting pattern. This remains 16.1% above the 7,681-read legacy control because D classification,
UK period parameters and reviewed profile facts are additional statutory work; the measured 9 ms
warm server-time difference was accepted for the current scale.

The third increment has cut the actual annual computation and due-date statement over on the retained
company. Provider selection is performed once, so SQL Server does not execute both the UK and legacy
calculations. The complete flat-rate path and `App.tbYearPeriod.BusinessTaxRate` remain as an explicit
higher-volume control. The selected liabilities, historic payment, running balance, period function
and unsupported-profile fail-closed behaviour reconcile under the rollback fixture. Fixed-rate
Project/What-if accruals remain unchanged and belong to MR-4.

1. Add the optional one-to-one `App.tbYearPeriodTaxUk` extension for UK company periods. Store the
   main rate, small-profits rate, marginal-relief fraction and annual base limits there, expose them
   through the existing period administration interface, and update a month or range atomically.
   Other jurisdictions and sole traders must not acquire placeholder UK rows. Keep
   `App.tbYearPeriod.BusinessTaxRate` as the legacy path until statement cutover. Provision rows only
   from the explicit, effective-dated `App.vwYearPeriodTaxUkSeed` statutory-rate schedule pinned to the SQL release;
   routine period generation must contain no anonymous statutory defaults. Published financial years
   use their explicit schedule entry. Later tax-accrual periods carry the latest published entry forward
   visibly as a provisional accrual basis. Forward financial years may remain explicitly provisional even when an
   official planning rate is published; statutory filing readiness must require an accepted,
   non-provisional financial-year entry.
2. Carry an explicit statutory-parameter status into the liability result. A provisional rate is a
   valid basis for a forward tax accrual, but must emit `CT-RATE-PROVISIONAL` and make statutory
   artifact/submission readiness false. Add regression coverage proving that the accrual remains
   available while filing cannot proceed; zero, a generic legacy rate or a user adjustment must not
   bypass the constraint.
3. Add a set-based UK period-liability projection that resolves the reviewed jurisdiction/profile
   facts once, obtains N and D from the active Category Tree, and invokes the kernel.
4. Make `Cash.vwTaxBizComputationByYear`, `Cash.fnTaxBizComputation` and
   `Cash.vwTaxBizStatement` consume the reviewed liability result rather than N ×
   `App.tbYearPeriod.BusinessTaxRate`. **Implemented for the UK company provider. The legacy field
   remains available to the embedded non-UK fallback, while the superseded calculation body is
   materialised only by regression/performance scripts as a scale control; it is not retained as a
   production view.**
5. Preserve existing due dates, business-tax adjustments, payments and running balances while exposing
   the kernel branch, limits, relief and diagnostic provenance for audit. **Implemented: the common
   liability contract exposes jurisdiction, source/profile identity, calculation identity, primary
   finding, due date and readiness; UK-only branch components remain on the UK projection. Due dates,
   adjustments, payments and balances reconcile.**
6. Prove that the actual annual liability event equals the final period liability. Replace the
   fixed-rate confirmed-Project and What-if accruals with cumulative liability deltas under MR-4,
   where those forecast facts are introduced.
7. Reconcile the synthetic company's tax expense, tax-control balance and Company Statement before and
   after the change. **The annual computation, tax statement and historic payment reconcile; the
   existing statutory balance-sheet regression passes and the complete Company Statement executes on
   the retained company. Higher-volume comparison remains an MR-6 performance control.**

**Gate:** passed. Actual-accounting statement and existing statutory balance-sheet tests pass. A
rollback-only settlement/reversal fixture proves payments affect the balance but not tax expense. The
legacy flat-rate result is no longer used for supported UK company periods, while remaining available
as the explicit higher-volume control. A provisional rate produces a labelled tax accrual and
deterministically blocks statutory filing readiness.

**Equity Bridge cutover correction:** the first filing-journey readiness review exposed one remaining
consumer of the flat-rate comparison path: the version-explicit Equity Bridge. The UK liability and
jurisdiction-selection boundaries are now Category-Tree-version-explicit, while their ordinary views
remain active-version wrappers. The Equity Bridge consumes that same selected provider, including for
historical reconstruction, rather than mixing current capital with legacy tax expense. Active-wrapper
parity, selected-provider parity, rounded tax-expense identity and the reporting tolerance are enforced
by the versioned reconciliation fixture. A rollback-only lifecycle test also published a real successor,
reconstructed its Archived predecessor through the same provider and restored the database unchanged.
The correction does not remove the legacy comparison provider for non-UK/other-regime operation.

### MR-4 — apply the same calculation to order-book tax accruals and What-if

**Status:** Implementation and retained-company acceptance evidence complete; awaiting human review.
The confirmed-Project and What-if paths are live on the retained company. The independent four-state
Cash Statement option matrix passes in both XLSX and ODS. Threshold mutation, reversal,
deterministic-ordering and Project/Object uniqueness acceptance pass through rollback-only fixtures.
In Accounts Mode, the principal user surface is the generated Cash Statement: its
**Tax accruals** option requests
the tax expected to fall due from the selected order book. The underlying CT source is
`Cash.vwTaxBizAccruals`, consumed by `Cash.proc_FlowCashCodeValues`, the Company Statement and summary
views. MR-4 must change that shared source rather than introduce an MIS-only forecast result.

The first increment adds `Cash.vwTaxBizOrderBook`, which selects eligible Project cash codes from the
active Profit root through `App.vwTaxBizCashCodes`, consolidates invoice rows before subtracting them
from confirmed Projects and returns one row per genuinely uninvoiced Project. A parallel
`Cash.vwTaxCorpOrderBookLiabilityUK` recomputes the annual loss roll-forward and establishes
`T(actual + confirmed) - T(actual)`. Deterministic adjacent Project-prefix kernel deltas, plus an
explicit later-year loss-carry bridge when required, now feed the unchanged five-column
`Cash.vwTaxBizAccruals` contract through one selected provider. The audit, Company Statement and Cash
Statement procedure reconcile exactly on the retained populated company. XLSX/ODS option acceptance
and the populated What-if scenario now pass.

The retained-company regression originally took roughly two minutes because it expanded and
compiled the annual projection separately for each assertion. The fixture now materialises the
canonical order book, actual liability and projected liability once. The projected view also resolves
its small reporting-profile/tax-source context directly instead of expanding the actual annual view
only to discover the tax source. The same regression now completes in about 25 seconds; warm projected
execution is about two seconds. A full Project-prefix × annual-liability-vector prototype exceeded
the five-minute diagnostic timeout and was removed from both source and the test database. The
accepted provider computes the annual result once, retains `Cash.vwTaxBizOrderBook` as the direct
canonical Project source, and invokes only the pure kernel over cumulative Project prefixes. This reduced the
live provider to 65 rows and an exact £29,006.44365 total on B8 without a second tax engine.

1. Feed confirmed Project transactions into the same N/D accumulator used by actual accounting and
   calculate `T(actual + confirmed) - T(actual)`. **Complete for the confirmed UK company path.**
2. Replace the flat-rate result in `Cash.vwTaxBizAccruals` while preserving the order-level audit
   contract consumed by `Cash.vwTaxBizAuditAccruals`, `Cash.vwStatementBase`, `Cash.vwSummary` and
   `Cash.proc_FlowCashCodeValues`. **Complete on B8; the common five-column contract is unchanged and
   the non-UK/other-regime branch retains the configured-rate fallback over the canonical order book.**
3. Exercise the Accounts Mode Cash Statement with **Order book** and **Tax accruals** independently
   enabled and disabled. Prove the XLSX and ODS outputs place the incremental CT accrual at its
   statutory due period and that disabling tax accruals removes it without removing the order book.
   **Complete on the retained company. The SQL procedure off/on toggle and statutory due placement
   pass. A protected four-state run generated XLSX and ODS from the same immutable model for neither,
   Order-book-only, Tax-accruals-only and both. The algebraic comparison found 76 tax-option cells and
   110 order-book-option cells, proved the two switches independent and measured the exact
   £29,006.44365 CT delta whether or not Order book was enabled.**
4. Feed quotation/scenario Projects into the same kernel and calculate
   `T(actual + confirmed + scenario) - T(actual + confirmed)`. **The scenario Project source,
   annual loss roll-forward, provider selection and `Cash.vwStatementWhatIf` cutover are deployed to
   B8. The empty-cost-set identity passes. For populated acceptance, a finished-good network has
   been cloned through `Project.proc_Copy`, converted to quotation status and placed in Carol
   Hughes's owner-scoped `Project.vwCostSet`. **Complete on B8:** moving only the finished-good root
   from June to July caused `Project.proc_Schedule` to split its automatically moved production and
   purchase facts between two financial years. The preserved £4,305.56 scenario produced exact CT
   deltas of −£167.06660 and £937.84000 on their statutory payment dates, while the confirmed order
   book remained unchanged.**
5. Exercise insertion, amendment, cancellation and reversal across the £50,000 and £250,000 limits,
   including movements caused by D and associated-company limits. **Complete on the retained
   company through a rollback-only confirmed-Project fixture. The live annual projection produced
   the exact independently specified small-profits, marginal-relief and main-rate amounts immediately
   after each mutation; cancellation returned to zero, restoration recovered the prior amount,
   £49,000 N plus £2,000 D produced £9,382.05882, and one associated company changed £30,000 N to
   £6,075.00000. A fresh-connection post-check found no fixture Project or configuration change.**
6. Prove deterministic same-day ordering and verify that Project/Object expansion does not double count
   tax-relevant facts. **Complete on the retained company. Two confirmed Projects on the same day,
   sharing one Object template, were ordered by stable Project code and received exact £5,700 and
   £9,100 deltas that telescoped to the £14,800 annual liability. The populated owner-scoped finished-
   goods cost set contained 9 rows and 9 distinct Project identities through both
   `Project.vwCostSetProjects` and `Cash.vwTaxBizWhatIfOrderBook`. A fresh-connection post-check found
   no fixture Project after rollback.**
7. Compare execution plans and timings with MR-0 before accepting the statement integration.
   **The first retained-company comparison is complete. The statement result is small but expands a
   large tax, VAT, invoice, Project and Category Tree calculation graph. Replacing the independent
   composition unions with `UNION ALL` preserved the economic rows exactly but produced no reliable
   elapsed-time improvement, so it is not being promoted as a performance change. A temporary
   higher-service-tier comparison reduced cold statement latency by approximately six to eight times
   and warm latency further, establishing compute and compilation capacity as the immediate
   constraint. Correctness remains the acceptance priority. The planned MIS seal boundary may avoid
   recomputing unchanged Project/Object subtrees; deployment tiers and an honest calculation-progress
   experience remain operational choices. Representative MIS-volume closure remains MR-6 work.**

**Gate accepted:** the Accounts Mode Cash Statement, Company Statement and
What-if Statement all change immediately and reversibly for confirmed and scenario Projects. Cash
Statement XLSX/ODS option tests pass, and no month-end calculation or second tax engine is introduced.

### MR-5 — carry the reviewed calculation into the CT artifact

**Status:** Complete and accepted on 17 November 2026. The refreshed immutable artifact passes the
pinned CT2025 package in Arelle, HMRC LTS 8.3 and one separately authorised genuine TPVS validation,
including IRmark and calculator validation. CT submission remains disabled.

1. Extend the existing Trade Control CT source contract with explicit N, D, A, associated-company
   count, rates/limits, marginal relief, calculation branch and provenance from MR-3.
2. Replace validation that assumes `TaxableTotalProfits * MainRate` with validation of the reviewed
   kernel components.
3. Make Objective 3 preparation reject `CT-RATE-PROVISIONAL` as a filing-readiness finding. It must
   not freeze, approve or expose a submission-ready CT service artifact for a period containing a
   provisional financial-year slice, even when its accrual calculation is complete.
4. Remove the CT600 serializer's current inference of associated-company information from a 19% rate
   and populate the RIM fields from explicit reviewed facts.
5. Rebuild the same immutable accounts, computation, CT600 and service artifact; do not introduce
   another serializer.
6. Re-run local contracts, CT2025/Arelle, LTS and genuine TPVS diagnostics as authorised, preserving
   exact bytes, responses and SHA-256 evidence.

Implementation checkpoint for review:

- the Trade Control source now reads the exact annual result from `Cash.vwTaxCorpLiabilityUK` and
  carries N, D, A, associated companies, the complete rate/limit set, adjusted limits, calculation
  branch, marginal relief, final tax, findings, readiness and a deterministic source digest;
- relevant distributions are an explicit source-owned deduction in the computation bridge, rather
  than an operator adjustment or an inferred filing value;
- the contract validator independently reconciles N, D, A, adjusted limits, branch, tax before
  relief, marginal relief, kernel tax, final tax and payable tax. A manual accrual adjustment is
  deliberately rejected because it cannot be represented by the CT600 statutory-rate detail lines;
- Objective 3 rejects `CT-RATE-PROVISIONAL` before population of a filing artifact. The offline
  D > 0 source-population test and explicit provisional-rate negative test pass; the refreshed
  retained-B8 artifact rerun remains part of the official evidence step;
- the CT600 RIM now emits the reviewed associated-company count and uses the explicit calculation
  branch. Small-profits and marginal-relief entitlement is no longer inferred from a 19% rate;
- relevant distributions are emitted in CT600 box 620 as well as the CT2025 computation. This is the
  input the HMRC calculator uses to derive augmented profits; omitting it caused LTS error `9043`
  even though the computation attachment carried D correctly;
- one shared financial-year allocation projects the statutory-rate bands into both CT600 and CT2025
  computation facts, preventing those artifacts from acquiring separate rounding algorithms;
- local company contracts pass for D = 0 and a D > 0 marginal-relief example, the full Tax Hub
  solution builds without warnings, and all local Tax Hub test executables pass in their supported
  offline modes;
- the protected retained-company evidence preserves exact accounts, computation, service-root,
  materialised-wire and LTS response bytes with SHA-256 identities. Arelle reports only successful
  package activation/load/validation information, and LTS returns a success receipt with no
  validation findings. This is explicitly classified as local HMRC LTS evidence;
- one separately authorised TPVS request transmitted the exact materialised wire already accepted by
  LTS. HMRC returned HTTP 200, response messages `0000` and `077001`, and no application findings.
  Exact request and response bytes are retained separately and classified as a genuine remote HMRC
  TPVS response, not a production submission. No retry was made.

**Gate accepted:** Company Statement, computation, CT600, Arelle, LTS and genuine TPVS evidence agree
for both D = 0 and D > 0; confirmed rates permit artifact review; provisional rates produce
`CT-RATE-PROVISIONAL` and no submission-ready artifact; CT submission remains disabled.

### MR-6 — recognition and performance closure

**Status:** Complete and accepted on 17 November 2026.

1. Run the full SQL suite on a fresh database and the retained CT test company. **Complete. A current
   DACPAC was applied to an independently regenerated standard-company dataset, and every arithmetic,
   rate-authority, distribution, liability, statement, order-book, What-if, jurisdiction-isolation and
   performance fixture passed there and on the retained company. A second naturally generated
   company was calibrated below the £50,000 augmented-profits limit; it selected the small-profits
   branch with positive relevant distributions, zero marginal relief and exactly 19% of taxable total
   profits. Its immutable Objective 3 computation also passed the pinned CT2025 taxonomy and HMRC LTS
   locally without findings. Environment identities and values remain in protected evidence.**
2. Compare statement execution plans, reads and median timings with MR-0 at representative Account and
   MIS volumes; investigate regressions above the stated envelope. **Complete for the current Account
   and prototype-MIS corpus. Earlier plan/read evidence established that compilation and CPU, rather
   than data IO, dominate the complete statement graph; the rejected `UNION ALL` and extra
   materialisation experiments produced no reliable improvement. A final three-run diagnostic proved
   stable rows, totals and balance checksums on both environments. Warm retained-company S0 timings
   remain materially slower than the temporary S2 comparison, so tier-aware execution and the planned
   MIS seal remain explicit operational decisions rather than concealed correctness compromises.**
3. Demonstrate that common statement output contains no UK-only N/D/A or marginal-relief field and
   that jurisdiction selection occurs once per calculation set. **Complete. The deterministic
   `CorporationTaxJurisdictionIsolation.sql` regression fixes the common statement/accrual shapes,
   rejects UK-only measures on those contracts, checks actual dependency metadata and proves that the
   annual, confirmed-order and What-if selected providers each resolve the node jurisdiction exactly
   once. The existing rollback fixture proves an unsupported UK profile fails closed.**
4. Assemble the calculation narrative, boundary results and official validation evidence required by
   the CT recognition workstream. **Complete. The narrative now links the statutory kernel, positive-D
   statement path, transaction-grained accruals, immutable Objective 3 artifact and distinct
   Arelle/LTS/TPVS evidence. Exact tenant, artifact and response identities remain protected.**
5. Remove unnecessary abstraction or transitional fixed-rate paths that are no longer used. **Complete.
   The rejected order-book wrapper and experimental statement composition were already removed. The
   period `BusinessTaxRate` path remains intentionally reachable only through the selected non-UK or
   other-regime fallback and as the regression control; it is not used for supported UK companies and
   is therefore not dead transitional code.**

**Gate accepted on 17 November 2026:** UK correctness, transaction-grained MIS behaviour, tier-aware
performance and recognition evidence are accepted; portability remains an extension point rather than
an implemented second regime. CT submission remains disabled.

## SQL verification suite

The implementation must add deterministic SQL verification scripts under
`src/sqlnode/src/tcNodeDb4/Tests`. Final names may follow repository conventions, but the suite must
cover the following responsibilities:

| Proposed script | Required coverage |
| --- | --- |
| `CorporationTaxMarginalReliefBoundaries.sql` | Loss, zero, each side of £50,000 and £250,000, an interior marginal-relief case, D greater than zero, short periods and associated-company divisors. |
| `CorporationTaxMarginalReliefAuthority.sql` | Published HMRC examples and independently calculated expected amounts, including statutory rounding. |
| `CorporationTaxStatementReconciliation.sql` | Annual liability equals dated liability events; payments affect balance but not tax expense; reversals restore the prior result. |
| `CorporationTaxOrderBook.sql`, `CorporationTaxOrderBookLiabilityUK.sql`, `CorporationTaxOrderBookAccrualUK.sql` and rollback mutation fixtures | Confirmed Projects cross both thresholds in both directions and produce `T(actual + confirmed) - T(actual)`. |
| `CorporationTaxWhatIfUK.sql` and the retained rescheduling fixture | Scenario insertion, amendment, cross-year rescheduling and removal produce the correct reversible delta without changing the baseline. |
| `CorporationTaxJurisdictionIsolation.sql` and `CorporationTaxStatementReconciliation.sql` | UK selection is explicit; an unsupported UK profile fails closed; common statement output carries no UK-only assumptions while other regimes retain the selected fallback extension point. |
| `CorporationTaxPerformance.sql` | Account and MIS-scale statement queries remain set based and within the accepted baseline envelope. |

Every verification script must:

- be self-contained or declare its prerequisites clearly;
- avoid tenant, server and credential literals;
- use exact decimal expected values rather than merely comparing two implementations of the same
  formula;
- throw on failure and report the failed invariant;
- run inside a transaction and roll back fixture changes unless it operates on a disposable database;
- test negative and reversal paths as well as success paths; and
- distinguish a law/configuration failure from a SQL arithmetic or statement-integration failure.

At minimum the no-distribution, full-year, no-associate corpus must establish independently that:

- £50,000 gives £9,500 at the small-profits rate;
- £100,000 gives £22,750 after marginal relief; and
- £250,000 gives £62,500 at the main rate.

Boundary-adjacent values must be included to expose incorrect comparison and rounding behaviour.
Protected tests should then exercise the current synthetic company without publishing its environment
or accounting values.

## Performance acceptance

Before implementation, MR-0 must capture elapsed time, CPU time, logical reads, row counts and query
plans for at least:

- `Cash.vwTaxBizStatement`;
- `Cash.vwStatement` over the retained horizon;
- `Cash.vwStatementWhatIf` with no scenario and with a representative scenario; and
- the statutory annual computation consumed by Objective 3.

The new path must remain set based, avoid per-row scalar UDF execution, and show bounded growth when
the transaction/Project corpus is scaled. A regression greater than 10% in median elapsed time or
logical reads against a stable baseline requires investigation and explicit review; it is not an
automatic licence to weaken correctness. Query plans must also be inspected for spills, repeated scans
of the Category Tree and accidental Cartesian expansion. Performance acceptance is tier-aware: a
larger service tier may be an explicit commercial option, but it must not conceal wrong arithmetic or
unbounded growth. Long, valid calculations should expose a truthful calculating state and a
calculation-specific timeout rather than a fictional percentage. When MIS sealing is implemented,
reuse must be keyed to the sealed scenario identity so that any tax-relevant transaction change
invalidates the result mechanically.

Performance evidence belongs with the protected run evidence when it identifies a tenant or host.

## Non-goals

- No implementation of another jurisdiction under this plan.
- No expansion into ring-fence, close-investment-holding or specialist company regimes without a
  separately approved scope.
- No enabling of the CT submission gate or Phase 5.10 transport.
- No adjustment inserted merely to make a validation service accept an otherwise incorrect result.
- No replacement of the Project/Object prototype with an Accounts-Mode-only monthly tax snapshot.

## Authoritative references

- [HMRC: Corporation Tax marginal relief](https://www.gov.uk/guidance/corporation-tax-marginal-relief)
- [HMRC: Completing your Company Tax Return](https://www.gov.uk/guidance/the-company-tax-return-guide)
- [HMRC: Corporation Tax rates and allowances](https://www.gov.uk/government/publications/rates-and-allowances-corporation-tax/rates-and-allowances-corporation-tax)
- [Corporation Tax Act 2010, Part 3A](https://www.legislation.gov.uk/ukpga/2010/4/part/3A)

The implementation must pin and retain the authoritative rules and worked examples actually used for
each supported accounting period; this overview is not itself a tax-law source.
