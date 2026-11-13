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
2. **Account Mode is a surface, not a separate ledger.** The launch UI may hide Projects and Objects,
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

**Status:** In progress. Source now provides an independently switchable deterministic positive-D
generator for companies, while `@EnableDividends = 0` retains the permanent zero-D control. The new
acceptance fixture reconciles posted non-VAT `CC-DIVRC` receipts to D and proves `A = N + D`. SQL and
web builds pass; deployment and positive-D execution remain subject to human review.

1. Add an external investee/payer and deterministic incoming dividend payments to the synthetic MIS
   dataset using the received-dividend cash code and a non-VAT treatment.
2. Prove the receipts appear in accounting Other Income while the same transactions are selected
   separately as D.
3. Derive the corresponding exempt-distribution deduction when calculating N, so accounting profit is
   not taxed merely because the dividend appears in the accounts.
4. Prove A = N + D and demonstrate that adding, amending or reversing a dividend changes A and the
   marginal-relief result without corrupting accounting profit or VAT.
5. Retain a zero-D synthetic case as a permanent regression alongside the positive-D case.

**Gate:** a fresh synthetic bootstrap produces deterministic zero-D and positive-D evidence, with
accounting profit, N, D and A independently reconcilable.

### MR-3 — replace the flat-rate actual CT statement calculation

**Status:** Not started. `Cash.vwTaxBizStatement` and its upstream computation still use the legacy
fixed-rate result.

1. Add a set-based UK period-liability projection that resolves the reviewed jurisdiction/profile
facts once, obtains N and D from the active Category Tree, and invokes the kernel.
2. Make `Cash.vwTaxBizComputationByYear`, `Cash.fnTaxBizComputation` and
   `Cash.vwTaxBizStatement` consume the reviewed liability result rather than N ×
   `App.tbYearPeriod.BusinessTaxRate`.
3. Preserve existing due dates, business-tax adjustments, payments and running balances while exposing
   the kernel branch, limits, relief and diagnostic provenance for audit.
4. Replace fixed-rate accruals with cumulative liability deltas and prove that the sum of deltas equals
   the final period liability.
5. Reconcile the synthetic company's tax expense, tax-control balance and Company Statement before and
   after the change.

**Gate:** actual-accounting statement tests pass; payments affect the balance but not tax expense; the
legacy flat-rate result is no longer used for supported UK company periods.

### MR-4 — apply the same calculation to Projects and What-if

**Status:** Not started. Existing Project and What-if behaviour remains unchanged.

1. Feed confirmed Project transactions into the same N/D accumulator used by actual accounting and
   calculate `T(actual + confirmed) - T(actual)`.
2. Feed quotation/scenario Projects into the same kernel and calculate
   `T(actual + confirmed + scenario) - T(actual + confirmed)`.
3. Exercise insertion, amendment, cancellation and reversal across the £50,000 and £250,000 limits,
   including movements caused by D and associated-company limits.
4. Prove deterministic same-day ordering and verify that Project/Object expansion does not double count
   tax-relevant facts.
5. Compare execution plans and timings with MR-0 before accepting the statement integration.

**Gate:** the Company Statement changes immediately and reversibly for confirmed and What-if Projects,
with no month-end calculation or second tax engine.

### MR-5 — carry the reviewed calculation into the CT artifact

**Status:** Not started. The Objective 3 preparation and immutable service artifact remain unchanged,
and CT submission remains disabled.

1. Extend the existing Trade Control CT source contract with explicit N, D, A, associated-company
   count, rates/limits, marginal relief, calculation branch and provenance from MR-3.
2. Replace validation that assumes `TaxableTotalProfits * MainRate` with validation of the reviewed
   kernel components.
3. Remove the CT600 serializer's current inference of associated-company information from a 19% rate
   and populate the RIM fields from explicit reviewed facts.
4. Rebuild the same immutable accounts, computation, CT600 and service artifact; do not introduce
   another serializer.
5. Re-run local contracts, CT2025/Arelle, LTS and genuine TPVS diagnostics as authorised, preserving
   exact bytes, responses and SHA-256 evidence.

**Gate:** Company Statement, computation, CT600 and validation evidence agree for both D = 0 and D > 0;
CT submission remains disabled pending human review.

### MR-6 — recognition and performance closure

**Status:** Not started.

1. Run the full SQL suite on a fresh database and the retained CT test company.
2. Compare statement execution plans, reads and median timings with MR-0 at representative Account and
   MIS volumes; investigate regressions above the stated envelope.
3. Demonstrate that common statement output contains no UK-only N/D/A or marginal-relief field and
   that jurisdiction selection occurs once per calculation set.
4. Assemble the calculation narrative, boundary results and official validation evidence required by
   the CT recognition workstream.
5. Remove unnecessary abstraction or transitional fixed-rate paths that are no longer used.

**Gate:** UK correctness, MIS responsiveness, performance and recognition evidence are accepted;
portability remains an extension point rather than an implemented second regime.

## SQL verification suite

The implementation must add deterministic SQL verification scripts under
`src/sqlnode/src/tcNodeDb4/Tests`. Final names may follow repository conventions, but the suite must
cover the following responsibilities:

| Proposed script | Required coverage |
| --- | --- |
| `CorporationTaxMarginalReliefBoundaries.sql` | Loss, zero, each side of £50,000 and £250,000, an interior marginal-relief case, D greater than zero, short periods and associated-company divisors. |
| `CorporationTaxMarginalReliefAuthority.sql` | Published HMRC examples and independently calculated expected amounts, including statutory rounding. |
| `CorporationTaxStatementReconciliation.sql` | Annual liability equals dated liability events; payments affect balance but not tax expense; reversals restore the prior result. |
| `CorporationTaxProjectForecast.sql` | Confirmed Projects cross both thresholds in both directions and produce `T(actual + confirmed) - T(actual)`. |
| `CorporationTaxWhatIf.sql` | Scenario insertion, amendment and removal produce the correct reversible delta without changing the baseline. |
| `CorporationTaxJurisdictionIsolation.sql` | UK selection is explicit; missing or unsupported jurisdiction/profile fails closed; common statement output carries no UK-only assumptions. |
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
of the Category Tree and accidental Cartesian expansion.

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
- [HMRC: Corporation Tax rates and allowances](https://www.gov.uk/government/publications/rates-and-allowances-corporation-tax/rates-and-allowances-corporation-tax)
- [Corporation Tax Act 2010, Part 3A](https://www.legislation.gov.uk/ukpga/2010/4/part/3A)

The implementation must pin and retain the authoritative rules and worked examples actually used for
each supported accounting period; this overview is not itself a tax-law source.
