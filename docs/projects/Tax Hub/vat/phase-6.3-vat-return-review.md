# Phase 6.3 — VAT return review and approval evidence

## Scope

Phase 6.3 presents the exact Objective 3 prepared VAT request for human review and records a durable, attributable approval. It does not send a VAT return to HMRC. Dispatch remains disabled until Phase 6.4 is implemented and approved.

The review is reached only from an open HMRC obligation that exactly matches a calculated `Cash.vwTaxVatSubmission` accounting period. The browser supplies neither the VRN, period dates, nine boxes nor tenant/actor identity.

## Implementation boundary

- TCWeb invokes `VatReturnPreparer` against the authoritative Trade Control adapter with `finalised: true`.
- The exact canonical body bytes are retained in the protected development content store for 24 hours. The UI deserializes those retained bytes into a read-only nine-box view; it is not a second payload builder.
- Period, masked VRN, source dataset, scoped source-snapshot digest and prepared-body SHA-256 are shown with the return.
- Errors block approval. Warnings require a separate unchecked acknowledgement.
- The HMRC business declaration is an embedded, versioned resource. Its confirmation is unchecked by default.
- Approval is restricted to Administrators and Managers and binds tenant, ASP.NET subject, internal actor, logical filing identity, obligation, exact declaration digest, exact body digest and source snapshot.
- Approval re-runs preparation and rejects an expired candidate or any change to VRN, body digest, source dataset/snapshot or obligation dates.
- Unexpected preparation/approval failures are written through the application Error Log and return a safe support reference.

The declaration wording was verified against HMRC's [VAT MTD end-to-end service guide](https://developer.service.hmrc.gov.uk/guides/vat-mtd-end-to-end-service-guide/documentation/obligations.html) on 30 September 2026.

## Integration correction found during review

The first Azure review correctly exposed an Objective 3 adapter mismatch. Obligation reconciliation used the Trade Control accounting bucket from `Cash.fnTaxTypeDueDates`, while the VAT source reader selected the unrelated monthly `VatEndOn` settlement column. For the aligned obligation, the statutory accounting period ended on 30 June 2017 while `VatEndOn` was 31 July 2017.

The reader now selects by the same accounting-period end used by obligation reconciliation. Its snapshot identity is a SHA-256 over the exact raw VAT projection and period rather than the database-wide `@@DBTS`; unrelated application writes can therefore no longer invalidate a VAT read. This does not alter any calculated VAT value. The live adapter suite against `tcNodeDb4-HMRC62-2017` passed 13 assertions after the correction.

## Azure review evidence

The review-only build was deployed to the existing `tcweb-payg-db96115e` App Service, still connected to the dedicated aligned database. No Azure resource or database data was created or changed for Phase 6.3.

HMRC obligation `18A2` (1 April–30 June 2017) prepared successfully with no findings. The UI showed:

| Box | Value |
|---:|---:|
| 1 | 6534.28 |
| 2 | 277.84 |
| 3 | 6812.12 |
| 4 | 1413.55 |
| 5 | 5398.57 |
| 6 | 32671 |
| 7 | 7404 |
| 8 | 0 |
| 9 | 0 |

Canonical Objective 3 body:

```json
{"periodKey":"18A2","vatDueSales":6534.28,"vatDueAcquisitions":277.84,"totalVatDue":6812.12,"vatReclaimedCurrPeriod":1413.55,"netVatDue":5398.57,"totalValueSalesExVAT":32671,"totalValuePurchasesExVAT":7404,"totalValueGoodsSuppliedExVAT":0,"totalAcquisitionsExVAT":0,"finalised":true}
```

- Prepared SHA-256: `373DB6751055E2FB07B7C4C4D3550A97A15E918543FE74BACC01EAA912A2C328`
- Source: `TradeControl / Cash.vwTaxVatSubmission`
- Source snapshot: `85B857E12846716BD2BE1F4B029075A2330FAC3682C44A25922482C6E6F4F9CF`
- Preparation result: PASS, no findings

The deployed UI digest exactly matched the independently prepared canonical body. The declaration remained unchecked and **Record approval** remained disabled. No approval was recorded during the automated smoke check. This wording deliberately separates the Phase 6.3 approval record from the Phase 6.4 HMRC submission action.

The period display uses HMRC's inclusive final date. A concise note explains that Trade Control internally represents `PayTo` as the following, excluded day; the underlying exclusive boundary remains unchanged because it avoids end-of-day ambiguity and adjoining-period overlap.

## Automated evidence

- TCWeb build: passed with 0 warnings and 0 errors.
- TCWeb Tax Hub boundary suite: passed, 59 assertions.
- Trade Control adapter offline suite: passed, 10 assertions.
- Trade Control adapter live aligned-database suite: passed, 13 assertions.
- Visual smoke check: VAT Statement remained the default tab; `18A2` alone exposed review; the read-only nine boxes, PASS result, advanced evidence and declaration rendered successfully.

## Review gate

Accepted on 30 September 2026 after the reviewer inspected the deployed `18A2` review, nine values, exact digest and declaration behaviour. The reviewer requested explicit clarification of HMRC's inclusive final date versus Trade Control's following-day exclusive boundary; that clarification was added without changing either representation or any VAT value. Recording approval does not submit to HMRC; submission remains disabled until Phase 6.4.
