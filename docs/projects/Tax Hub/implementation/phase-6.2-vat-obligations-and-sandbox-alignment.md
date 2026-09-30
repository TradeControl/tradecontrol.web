# Phase 6.2 VAT obligations and sandbox alignment

## Product boundary

TCWeb retrieves the active, reviewed `INDIRECT-TAX` reporting profile for the home reporting subject. The browser cannot provide a VRN, date range or HMRC period key. The product service uses a bounded 366-calendar-day window and obtains period keys only from HMRC.

HMRC periods are reconciled by exact inclusive start and end dates to the existing `Cash.vwTaxVatSubmission` path through `Cash.fnTaxTypeDueDates`. Local dashboard dates remain forecasts and are labelled separately. Only an open, exactly matched authority obligation exposes **Review return**. A fulfilled obligation is reserved for history/readback; unmatched authority and local periods are shown explicitly.

The synthetic placeholder VRN `999000001` is rejected at the product boundary even though it is nine digits.

## Disposable sandbox alignment

Alignment is destructive test provisioning. It is not registered as a TCWeb service or action and cannot be selected by the Production application composition.

### Why this database is deliberately historical

HMRC's VAT sandbox supplies deterministic canned obligations whose accounting periods are historical and do not move with the current calendar. Uri Nielsen's generated organisation currently receives the canned 2017 obligation. By contrast, the ordinary Trade Control synthetic-data generator anchors its accounting years and periods to the real current date. A normally generated 2026 node therefore cannot naturally reconcile one of its calculated VAT reporting periods to HMRC's 2017 obligation.

For integration evidence only, a complete disposable Trade Control node is generated with an historical as-of date. The temporal anchor is applied while the entire dataset is generated, so its accounting calendar, projects, invoices, payments, tax periods and calculated VAT evidence remain mutually coherent. It does not alter, fabricate or override any VAT-return value after calculation. The ordinary `Cash.vwTaxVatStatement` → `Cash.vwTaxVatSubmission` → Objective 3 preparation path remains authoritative.

Only the disposable synthetic accounting dataset is historically anchored. The TCWeb application and Azure host still run the current release; Azure, OAuth and fraud-prevention timestamps use the real current clock; and every HMRC request is made to the sandbox in real time. Thus a future maintainer may deliberately encounter an HMRC integration-test database whose accounting calendar appears nine years out of date while all security and communication evidence carries current timestamps.

The disposable Development host also supplies `SandboxObligationAsOfDate` so its bounded HMRC enquiry includes the canned historical period. This changes only the server-selected date window of the read-only sandbox obligation enquiry. It is rejected outside the Development + Sandbox + DevelopmentFiles composition, is not browser input, and does not replace the real clock used for OAuth, fraud evidence, communications or stored operational timestamps.

The generated organisation currently authenticates with HMRC's single-factor test-user journey and the development/reference product has no reviewed vendor licence identifier. HMRC's fraud-header validator reports these as the already accepted `Gov-Client-Multi-Factor` and `Gov-Vendor-License-IDs` warnings. The Phase 6.2 Azure evidence host may therefore select `AllowIncompleteSandboxFraudHeaders` only in the validated Development + Sandbox + DevelopmentFiles composition. This permits those empty warning headers while retaining the public-address, topology, ownership, integrity and freshness checks. The strict formatter remains the default and production validation rejects this setting; production enablement still requires the separate MFA/licensing decision.

This facility exists solely because HMRC's deterministic sandbox data cannot be moved onto Trade Control's present-day calendar. It is absent from production composition, cannot be selected through the ordinary UI, and is guarded so it operates only on a synthetic statutory profile. The existing Azure dataset must never be regenerated, re-dated or otherwise modified to obtain this evidence; a newly named disposable database is required for every destructive alignment exercise.

Use a fresh, disposable synthetic node. Generate any VAT-registered company or sole-trader template with an explicit temporal anchor that contains the selected canned HMRC period:

```sql
EXEC App.proc_DatasetSyntheticMIS
    @IsCompany = 1,                 -- 0 is equally supported
    @IsVatRegistered = 1,
    @UseStdCompanyTemplate = 0,     -- 1 is equally supported
    @AsOfDate = '2017-06-30';
```

The nullable `@AsOfDate` parameter exists only to make a disposable dataset reproducible. Omitting it preserves ordinary current-date generation. It changes the generated accounting calendar; it does not replace or edit VAT return values.

After HMRC has returned the selected obligation, align the generated organisation VRN and prove that an exact calculated local period exists:

```sql
EXEC App.proc_DatasetSyntheticMIS_VatSandboxAlign
    @SandboxVrn = '<generated organisation VRN>',
    @ObligationPeriodKey = '<opaque HMRC period key>',
    @ObligationStart = '<HMRC start date>',
    @ObligationEnd = '<HMRC end date>';
```

The alignment procedure fails unless the reporting profile was created by the synthetic generator and an exact period already exists through `Cash.vwTaxVatSubmission`. It rejects the placeholder VRN and calls the normal `Cash.proc_ReportingProfileSave` configuration boundary. The opaque authority period key is returned as evidence but is not written into accounting data or used to construct a filing request.

No nine-box value is accepted by either alignment operation. All values continue to flow from `Cash.vwTaxVatStatement` into `Cash.vwTaxVatSubmission` and subsequently through the Objective 3 preparation adapter.

## Interactive review evidence — 30 September 2026

The Phase 6.2 evidence run used the existing `tcweb-payg-db96115e` Development App Service pointed temporarily at the dedicated `tcNodeDb4-HMRC62-2017` database. No additional App Service was created and `tcNodeDb4-COMIPFVT1-COMIN26` was not changed.

The connected Uri Nielsen sandbox journey returned HMRC's deterministic obligations in real time:

- `18A2`, 1 April–30 June 2017, due 7 August 2017, was shown as **Open — local return available** and alone offered **Review return**;
- `18A1`, 1 January–31 March 2017, due 7 May and received 6 May 2017, was shown as **Fulfilled** and reserved for history/readback; and
- unmatched calculated Trade Control periods remained visibly labelled **Local forecast only**.

The first live response exposed a contract error rather than an HMRC error: the local `VatObligation` type required an `obligationId` property that is absent from HMRC's VAT obligations payload. The unsupported property was removed and the exact two-obligation sandbox response was added to the VAT contract regression suite. The full Tax Hub solution then built with zero warnings and errors, the VAT contract suite passed 19 assertions, the updated TCWeb package deployed successfully, and the live authenticated refresh produced the result above.

The reviewer accepted the Phase 6.2 gate on 30 September 2026 after inspecting this evidence. The existing four-template VAT regression matrix and corresponding `VT0` negative controls remain part of the retained evidence baseline.

The current Production App Service deliberately remains fail-closed until approved durable stores and trusted public ingress topology exist. Development file persistence must not be enabled there merely to obtain this evidence.

For the September 2026 evidence run, the isolated database is `tcNodeDb4-HMRC62-2017`. It was created alongside—not from and not over—`tcNodeDb4-COMIPFVT1-COMIN26`. The latter remains the current Azure demonstration dataset and is outside the alignment exercise.
