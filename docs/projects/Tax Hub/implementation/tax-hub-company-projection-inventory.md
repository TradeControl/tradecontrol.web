# Tax Hub Company Projection Inventory

## Scope

This is the Phase CO1 inventory for the approved ordinary UK private micro-company MIN and STD profiles. It classifies the source of every semantic in the current `StatutoryAccounts`, `CorporationTaxComputation` and `Ct600Return` contracts and defines the work that must precede population.

The inventory concerns authority-neutral evidence. HMRC and Companies House envelope fields, credentials, package identifiers and transport state are not company-source facts.

## Source classifications

| Classification | Meaning |
|---|---|
| Accounting projection | Calculated from Trade Control accounting data through a reviewed SQL Node projection. |
| Statutory context | Effective identity, registration, reporting profile or setting supplied by Part I. |
| Reviewed filing input | Proposed by Trade Control where possible, but confirmed or replaced for one prepared filing. |
| Derived | Calculated deterministically from approved source values under a versioned rule. |
| Explicit zero | A reviewed default for an applicable ordinary-company schedule with no entries; it is not evidence that a source was queried and found empty. |
| Not applicable | The supported profile proves that the semantic does not apply. |
| Gap | No sufficiently authoritative projection exists yet. Population must stop until the gap is resolved or the scenario is declared unsupported. |

Every populated value must also retain its SQL row-version or immutable preparation-snapshot evidence. A zero retains its value state; it must not be indistinguishable from a ledger-derived zero.

## MIN and STD reconciliation

The company MIN and STD templates compose the same `UK-CO-ACCTS-2026`, `UK-CO-CT-2026` and `UK-CO-CT600-2026` semantic manifests. Their seven direct mappings use the same stable category roots:

- `CT-TURNOV`, `CT-OTHRIN`, `CT-CSTSAL`, `CT-STAFFC` and `CT-OVERHD` for accounts;
- `CA-DEPREC` for the Corporation Tax depreciation add-back evidence; and
- `CT-TURNOV` for CT600 turnover reconciliation.

STD adds detailed Cash Codes beneath those roots; it does not change their statutory meaning. MIN and STD therefore use one company source contract. Effective-map provenance records the actual contributing Cash Codes for each prepared snapshot.

The parked company MIN sandbox is not required to prove a second contract shape. Before CO3 completion it must be reactivated and exercised against the same source port; until then its template and mapping structure are the CO1 evidence.

## Statutory accounts source matrix

| Semantic | MIN and STD source | Classification | CO1 disposition |
|---|---|---|---|
| Company name, company number and registry jurisdiction | `Subject.fnStatutoryIdentity` | Statutory context | Ready; company number remains text. |
| Registered office | registered `Subject.tbAddress` selected by `AddressTypeCode` | Statutory context and reviewed filing input | Free-form source is explicit; contract-specific address lines require filing review. |
| Accounts period | Business-tax window from `Cash.fnTaxTypeDueDates(0, 0)` | Reviewed filing input | Default only; operator may alter the start date for first/long accounts. |
| Comparative period | Preceding equal horizon | Reviewed filing input | Default only; absent for first accounts. |
| FRS 105, micro-entity and audit profile | reviewed `STATUTORY-ACCOUNTS` profile/settings | Statutory context | Accounting standard is ready; audit assertions require filing review. |
| Currency | `App.tbOptions.UnitOfCharge` through statutory identity | Statutory context | Ready. |
| Turnover | `UK-CO-ACCTS-2026/IncomeStatement.Turnover` | Accounting projection | Mapping ready; bounded period projection required. |
| Other income | `UK-CO-ACCTS-2026/IncomeStatement.OtherIncome` | Accounting projection | Mapping ready; bounded period projection required. |
| Cost of sales | `UK-CO-ACCTS-2026/IncomeStatement.CostOfSales` | Accounting projection | Mapping ready; bounded period projection required. |
| Administrative expenses | two approved roots under `IncomeStatement.AdministrativeExpenses` | Accounting projection | Mapping ready; preserve both roots and contributor provenance. |
| Tax on profit | approved Corporation Tax computation | Derived | Must not use the tax-control-account balance. |
| Profit/loss for period | approved income-statement calculation | Derived | Must reconcile before and after tax explicitly. |
| Fixed assets | `Cash.vwBalanceSheet` account evidence | Gap | Requires a statutory, as-at-date classification projection. |
| Current assets | `Cash.vwBalanceSheet` account/subject evidence | Gap | Requires a statutory, as-at-date classification projection. |
| Prepayments and accrued income | period-end adjustment evidence | Gap | No authoritative statutory classification currently exists. |
| Creditors within one year | `Cash.vwBalanceSheet` evidence plus maturity | Gap | Maturity projection is not defined. |
| Net current assets/liabilities | approved balance-sheet components | Derived | Blocked by component gaps. |
| Total assets less current liabilities | approved balance-sheet components | Derived | Blocked by component gaps. |
| Creditors after one year | `Cash.vwBalanceSheet` evidence plus maturity | Gap | Maturity projection is not defined. |
| Provisions | reviewed period-end adjustment | Gap | No authoritative statutory classification currently exists. |
| Accruals and deferred income | reviewed period-end adjustment | Gap | No authoritative statutory classification currently exists. |
| Net assets/liabilities | approved balance-sheet components | Derived | Must reconcile to capital and reserves. |
| Capital and reserves | balance-sheet/equity evidence | Gap | Requires an as-at-date statutory equity projection and reconciliation. |
| Principal activity | `Subject.tbVirtual.BusinessDescription` | Reviewed filing input | Source suggestion; editable for the filing. |
| Accounting policies | reviewed `ACCOUNTING-POLICIES` setting | Reviewed filing input | Ready as a suggestion. |
| Average employees | `Subject.tbVirtual.NumberOfEmployees` | Reviewed filing input | Headcount is a suggestion, not yet the statutory period average. |
| Director advances | filing schedule | Explicit zero or reviewed filing input | Empty default is permitted; entered schedules require review. |
| Commitments and contingencies | filing schedule | Explicit zero or reviewed filing input | Empty default is permitted; entered schedules require review. |
| Approval date and signing director | preparation workflow | Gap | Requires a reviewed approval event and director reference; submission date is not a substitute. |

The current contract has eleven balance-sheet lines. `company-field-sets.md` additionally identifies called-up share capital not paid and alternative format totals. Those are outside the current supported typed surface and must not be silently folded into another line.

## Corporation Tax computation source matrix

| Semantic | Source | Classification | CO1 disposition |
|---|---|---|---|
| CT periods | allocation from reviewed accounts period, maximum twelve months each | Derived | Rule is explicit; long accounts may produce multiple CT periods. |
| Accounts profit/loss before tax | approved accounts income statement | Derived | Must reconcile to the accounts artifact. |
| Accounting depreciation add-back | `UK-CO-CT-2026/AddBacks.AccountingDepreciation` | Accounting projection | Direct evidence ready; it is never a capital allowance. |
| Other add-backs | filing adjustment schedule | Explicit zero or reviewed filing input | No entries default to explicit zero. |
| Deductions | filing adjustment schedule | Explicit zero or reviewed filing input | No entries default to explicit zero. |
| Capital allowances | filing allowance schedule | Explicit zero or reviewed filing input | No entries default to explicit zero; detailed pools are not yet supported. |
| Loss relief | filing loss schedule | Explicit zero or reviewed filing input | Do not infer a claim from negative accounting profit. |
| Chargeable gains | filing computation schedule | Explicit zero or reviewed filing input | Typed source retains the value; detailed gains computation is not yet supported. |
| Taxable total profits | approved computation inputs | Derived | Versioned calculation required. |
| Main rate | period-effective statutory rate | Gap | `App.tbYearPeriod.BusinessTaxRate` is an estimate, not yet an approved statutory-rate projection. |
| Corporation Tax chargeable | taxable profits and approved rate calculation | Derived | Blocked by rate policy. |
| Other reliefs | filing relief schedule | Explicit zero or reviewed filing input | Unsupported relief types must not be collapsed into this value. |
| Tax payable | approved liability calculation | Derived | Must retain calculation and rounding evidence. |
| Tax paid/payment position | tax statement/payment evidence | Gap | Requires period-specific payment allocation and reconciliation. |
| CT600A | reviewed loans-to-participators schedule | Conditional reviewed filing input | Supported only when the complete page input is supplied and validated. |

## CT600 population relationship

The current `Ct600Return` surface does not create another accounting source:

| CT600 semantic | Owner |
|---|---|
| Company name and registration number | `CompanyStatutorySource.Identity` |
| UTR | reviewed masked/unmasked registration flow; previews use only the permitted representation |
| Return period | one allocated `CorporationTaxPeriodSource.Period` |
| Turnover and profit before tax | reconciled statutory accounts source |
| Taxable profits, tax chargeable and tax payable | approved Corporation Tax computation |
| Accounts/computations attached | prepared-package composition |
| CT600A indicator/page | reviewed conditional CT600A source |
| Declaration name and date | separate reviewed return-declaration event |

Group, charity, insurance, tonnage-tax, ring-fence, energy-profits, R&D, restitution, residential-property-developer and other specialist or supplementary scenarios are unsupported by the ordinary-company profile. Detection must fail before contract population.

## Source boundary

`TradeControl.Tax.UK.Application.DataProvision` owns two narrow ports:

- `ICompanyStatutorySource` returns company identity, periods, profile, current/comparative statements, notes, approval and provenance.
- `ICorporationTaxSource` returns one or more CT periods, computation inputs/results, payment reconciliation and provenance.

The ports contain no HMRC box numbers, Companies House delivery choices, taxonomy QNames, database identifiers, credentials or serialized documents. SQL Server query composition remains in SQL Node routines invoked by `Adapters.TradeControl`, following the relational provider boundary established in the SQL Node specification.

## SQL Node work packages for CO2/CO3 entry

The following reviewed SQL work is required before a representative accounts artifact can be populated:

1. a period-bounded company income-statement projection returning current/comparative semantic values and effective contributor provenance;
2. an as-at-date company balance-sheet projection that classifies statutory asset, liability and equity headings without relying on display names;
3. explicit maturity evidence for creditors within and after one year;
4. a reconciliation projection proving income-statement totals, net assets and capital/reserves;
5. a period-effective statutory Corporation Tax rate policy distinct from an accounting estimate;
6. a CT tax-payment allocation/reconciliation projection; and
7. persistent or immutable workflow evidence for accounts approval, signing director and CT600 declaration.

These are work packages, not permission to add tables. Each must first prefer existing durable classifications and carefully scoped projections. A new table is justified only for genuinely persistent facts that have no existing owner.

## CO1 gate conclusion

The neutral source shape is defined and MIN/STD contract equivalence is established. Population must not begin yet: the balance-sheet, approval, statutory-rate and tax-payment gaps above require reviewed designs. Group and specialist scenarios are explicitly unsupported and fail closed through `CompanySourceSupport`.
