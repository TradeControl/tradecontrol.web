# Tax Hub Data Readiness Matrix

**Status:** DP5 verified evidence  
**Scope:** Accounts Mode, UK launch, SQL build 4.1.5

This matrix assigns one authoritative owner to every non-ledger and accounting input required by the mandatory Corporation Tax, Companies House accounts, VAT return and sole-trader cumulative-period slices. `Ready` means the source exists and its DP5 SQL evidence passes. `Workflow` means the value belongs to the reviewed preparation/filing instance rather than permanent business master data. `Gap` blocks population until an authoritative source or an explicitly unsupported scenario is agreed.

Database names are never evidence of entity or reporting shape. Entity type comes from `Cash.fnGetBizTaxType()`. Sole-trader MIN/STD shape comes from the effective `Cash.tbTaxTagMap` support for consolidated or detailed cumulative expenses.

## Shared statutory identity

| Semantic | Authoritative owner | State | Evidence or constraint |
|---|---|---:|---|
| Reporting subject | `App.tbOptions.SubjectCode` → `Subject.tbSubject` | Ready | `Subject.fnStatutoryIdentity` resolves one home subject. |
| Legal/business name | `Subject.tbSubject.SubjectName` | Ready | Non-empty readiness finding and persisted DP5 test. |
| Node tax jurisdiction | `App.tbOptions.JurisdictionCode` | Ready | Required FK to `App.tbJurisdiction`. |
| Registry jurisdiction | `Subject.tbVirtual.RegistryJurisdictionCode`, falling back to node jurisdiction | Ready | Effective value projected by `Subject.fnStatutoryIdentity`. |
| Reporting currency | `App.tbOptions.UnitOfCharge` | Ready | Currency override remains distinct from jurisdiction. |
| Free-form contact address | selected `Subject.tbAddress` | Ready | Remains the ordinary Trade Control source record. |
| Statutory address | Company: registered `Subject.tbAddress`; sole trader: selected trading address | Ready as free-form source | `AddressTypeCode` prevents the default trading address being mistaken for a registered office. Contract-specific address lines require filing review. |
| Phone, email and website | `Subject.tbSubject` / `Subject.tbVirtual` | Ready | Reusable contact data; not mandatory for every filing. |
| Source freshness | row versions and update timestamps from identity/registration/profile/setting projections | Ready | The neutral adapter retains source versions. Accounting-artifact snapshot provenance is added with the accounting adapters. |

## Company accounts and Corporation Tax

| Semantic | Authoritative owner | State | Evidence or constraint |
|---|---|---:|---|
| Company number | `Subject.tbVirtual.CompanyNumber` | Ready | Not duplicated in `Subject.tbRegistration`. |
| Company name and registered office | shared statutory identity | Ready | Company sandbox passes DP5 identity checks. |
| UTR | reviewed `Subject.tbRegistration`, scheme `GB-UTR` | Ready | Raw value is restricted to preparation; list/diagnostic projection is masked. |
| Company versus sole trader | `Cash.fnGetBizTaxType()` | Ready | Company returns tax type 0; sole trader returns 4. |
| Corporation Tax profile | `Cash.tbReportingProfile`, type `COMPANY-TAX`, Tax Source `UK-CO-CT-2026` | Ready | Effective reviewed profile passes. |
| Statutory accounts profile | `Cash.tbReportingProfile`, type `STATUTORY-ACCOUNTS`, Tax Source `UK-CO-ACCTS-2026` | Ready | Independent Companies House authority profile passes. |
| FRS/account type/format | effective `Cash.tbReportingProfileSetting` | Ready | FRS 105, micro-entity and format are controlled reviewed values. |
| Principal activity | `Subject.tbVirtual.BusinessDescription` | Ready default | Suggested in the editable draft and may be overwritten by the operator. |
| Accounts period | `Cash.fnTaxTypeDueDates(Cash.fnGetBizTaxType(), 0)` | Ready default | Uses the tax horizon by policy; the submission draft permits an explicit start-date override. |
| Comparative period | preceding equal accounting horizon | Ready default | Pre-incorporation periods carry zero accounting values; the operator may override the dates. |
| Accounting policies narrative | effective `ACCOUNTING-POLICIES` profile setting | Ready default | Reusable suggestion in the existing data dictionary, editable per submission. |
| Average employees for period | `Subject.tbVirtual.NumberOfEmployees` | Ready default | Treated as a suggestion rather than a claim that a period average has been calculated. |
| Director advances | empty/zero submission schedule | Ready default | Absence defaults to none and remains operator-reviewable. |
| Commitments and contingencies | empty/zero submission schedule | Ready default | Absence defaults to none and remains operator-reviewable. |
| Accounts approval date and signing director | prepared filing workflow | Workflow | Must be captured per accounts artifact, not stored as permanent registration data. |
| Income-statement components | company Accounts Tax Tags and accounting projection | Partial | Tag definitions/mappings exist; CO1 must prove complete period and comparative semantics. |
| Balance-sheet components | existing balance-sheet/account evidence plus company semantic projection | Gap | `Cash.vwBalanceSheet` exists, but the authoritative statutory semantic adapter and comparative snapshot are not yet defined. |
| CT period boundaries | `Cash.fnTaxTypeDueDates(Cash.fnGetBizTaxType(), 0)` plus accounts/CT allocation rules | Partial | Tax window source exists; allocation against the approved accounts period remains to be implemented. |
| Profit before tax and accounting depreciation | approved company accounting evidence/Tax Tags | Partial | Mappings exist but need a coherent period snapshot. |
| Other add-backs and deductions | editable CT draft, default zero | Ready default | No Category Tree value is manufactured; the operator may supply adjustments. |
| Capital allowances | editable CT draft, default zero | Ready default | Accounting depreciation is not substituted for an allowance. |
| Losses and relief claims | editable CT draft, default zero | Ready default | Elections and claims remain explicit operator inputs when applicable. |
| Main rate | statutory rate effective for each CT period | Partial | Existing year-period rate data exists; CO1 must define authoritative selection for split periods. |
| CT600 supplementary pages | reviewed conditional filing context | Workflow | Unsupported unless applicability and every required input are explicitly established. |

## VAT return

| Semantic | Authoritative owner | State | Evidence or constraint |
|---|---|---:|---|
| VAT number (VRN) | `Subject.tbVirtual.VatNumber` | Ready where VAT enabled | Not duplicated in the registration catalogue. Synthetic VAT-enabled sandboxes receive a conspicuous test VRN. |
| VAT reporting profile | `Cash.tbReportingProfile`, type `INDIRECT-TAX` | Ready where VAT enabled | Correctly has no Tax Source because VAT accounting comes from the established VAT projection. |
| VAT period window | `Cash.fnTaxTypeDueDates(1, ...)` | Ready | Tax type 1 owns VAT periods. |
| Period key | authority obligation/selected filing context | Workflow | It is an HMRC route value, not accounting master data. |
| Nine VAT boxes | `Cash.vwTaxVatSubmission` | Ready pending selector check | Existing source; Phase 3 must confirm the exact period selector semantics. |
| Finalised declaration | prepared filing workflow | Workflow | Captured per request; never permanent master data. |
| Exact snapshot provenance | prepared VAT source snapshot | Preparation | Captured when the VAT accounting adapter creates its immutable source snapshot. |

## Sole-trader cumulative submission

| Semantic | Authoritative owner | State | Evidence or constraint |
|---|---|---:|---|
| NINO | reviewed `Subject.tbRegistration`, scheme `GB-NI` | Ready | Masked outside restricted preparation. |
| UTR | reviewed `Subject.tbRegistration`, scheme `GB-UTR` | Ready | Available for the wider income-tax workflow without duplicating NINO. |
| HMRC business ID | `Cash.tbReportingProfile.AuthorityReference` | Ready | Belongs to the self-employment authority profile, not general subject identity. |
| Self-employment Tax Source | reporting profile → `UK-ITSA-SE-CUM` | Ready | Deliberate FK-backed association. |
| Accounting basis | effective `ACCOUNTING-BASIS` profile setting | Ready | Controlled `CASH`/`ACCRUAL` value. |
| Quarterly period type | effective `QUARTERLY-PERIOD-TYPE` profile setting | Ready | Controlled setting; operation applicability remains Application policy. |
| Tax/reporting window | `Cash.fnTaxTypeDueDates(Cash.fnGetBizTaxType(), ...)` | Ready | Uses sole-trader business tax type 4. |
| Cumulative income and expenses | `Cash.fnTaxBizCumulative(...)` | Ready | Preserves supported, unsupported and invalid states. |
| MIN consolidated shape | supported `consolidatedExpenses` Tax Tag mapping | Ready offline | Classified from mapping, never database name. |
| STD detailed shape | supported detailed expense Tax Tag mappings | Ready active | Active sole-trader STD sandbox passes unambiguous shape test. |
| Disallowable expenses | editable submission draft, absent/zero by default | Ready default | Omitted when the contract permits absence; otherwise the operator supplies the required values. |
| Exact snapshot provenance | prepared cumulative source snapshot | Preparation | Captured when the cumulative accounting adapter creates its immutable source snapshot. |

## Filing-context boundary

Approval dates, signatories, declarations, elections, claim choices, authority period keys, correlation IDs and response state are not reusable business registration data. They belong to a specific prepared artifact or submission workflow. Credentials, access tokens and client secrets remain outside every projection and prepared artifact.

## DP5 gate position

The two active STD sandboxes prove the shared statutory context and profile model. The parked MIN databases remain unchanged; MIN contract shape retains offline mapping/contract coverage until those databases are deliberately reactivated. All permanent non-ledger values now have one source, default or filing-workflow classification. Company and accounting semantic projection work begins in CO1; immutable accounting snapshot provenance belongs to the corresponding preparation adapters.
