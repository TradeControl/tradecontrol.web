# SQL Migration Instruction for GPT‑5.4 (tcNodeDb4)

## Goal

- Update the pre‑Brexit VAT prototype to use post‑Brexit HMRC VAT logic and submission protocols.  
- Rewrite VAT‑related SQL objects to use a new jurisdiction model based on an enumerated type stored in `Subject.tbSubject`.  
- Remove all uses of `EUJurisdiction` and replace them with `ExportTypeCode`.  
- Implement correct NI acquisition logic, export exclusion logic, polarity rules, and canonical HMRC VAT submission fields.

## Delivery Process

Trade Control development follows a staged AI-assisted engineering process.

The coding assistant shall work through the following documents in sequence:

1. tc-development-contract.md
2. tax-hub-vat-surface-sql.md

The repository investigation shall be performed only after these documents have been analysed and a new optimal Work Plan section has been added to tax-hub-vat-surface-sql.md.

## Work Plan

1. Introduce the new jurisdiction schema surface:
   - create `Subject.tbExportType`
   - add `Subject.tbSubject.ExportTypeCode`
   - add foreign key and index
   - remove `EUJurisdiction` from `Subject.tbVirtual`

2. Re-seed node initialisation for the new jurisdiction model:
   - seed `Home`, `Export`, and `Northern Ireland`
   - ensure initialisation clears `Subject.tbVirtual` before `Subject.tbSubject`
   - ensure `Subject.tbExportType` is reset and reseeded

3. Rewrite subject presentation surfaces:
   - update `Subject.vwDatasheet`
   - update `Subject.vwStatusReport`
   - update `Subject.vwVirtual`

4. Rewrite synthetic dataset generation:
   - replace `EUJurisdiction` assignment logic with `ExportTypeCode`
   - set export customers to `1`
   - set `PlasticSupplier` to `2`
   - default all other dataset subjects to `0`

5. Rewrite VAT transaction and accrual views:
   - exclude `ExportTypeCode = 1`
   - treat `ExportTypeCode = 2` sales as domestic
   - treat `ExportTypeCode = 2` purchases as NI acquisitions
   - preserve polarity with purchase VAT values negative

6. Replace the legacy VAT totals surface:
   - remove dependency on `Cash.vwTaxVatTotals`
   - introduce `Cash.vwTaxVatSubmission`
   - emit canonical HMRC submission fields
   - preserve supporting reporting columns where needed by existing consumers

7. Update dependent reporting and balance surfaces:
   - `Cash.vwTaxVatStatement`
   - `Cash.vwBalanceSheetVat`
   - `Cash.vwFlowVatPeriodAccruals`
   - `Cash.vwFlowVatPeriodTotals`
   - `Cash.vwFlowVatRecurrence`
   - `Cash.vwFlowVatRecurrenceAccruals`
   - `Cash.vwStatementBase`
   - `Cash.vwSummary`

8. Update dependent procedures where impacted:
   - `Cash.proc_FlowCashCodeValues`
   - `Cash.proc_PaymentPostMisc`
   - `Cash.proc_TaxObligations`
   - `Cash.proc_VatBalance`

9. Canonical VAT rebuild hardening:
   - compute canonical HMRC VAT fields directly from raw `InvoiceValue`, `TaxValue`, `InvoiceTypeCode`, and `ExportTypeCode`
   - remove all legacy VAT fields from all VAT-facing views, including intermediate CTEs
   - ensure `Cash.vwTaxVatSubmission` is the minimal canonical submission surface
   - update dependent VAT consumers to use canonical fields only
   - allow compile-time breakage for any remaining legacy consumers outside the supplied scope

## New Jurisdiction Model

### New table: `Subject.tbExportType`

Columns:

- `ExportTypeCode` tinyint PRIMARY KEY  
- `ExportType` nvarchar(50)

Seed data in `App.proc_NodeDataInit`:

- `0, 'Home'`  
- `1, 'Export'`  
- `2, 'Northern Ireland'`

### Modify `Subject.tbSubject`

- Add column `ExportTypeCode` tinyint NOT NULL DEFAULT 0
- Add index on ExportTypeCode

### Remove

- `EUJurisdiction` from `Subject.tbVirtual`  
- All references to `EUJurisdiction` in SQL objects listed below

## Jurisdiction Semantics

- `ExportTypeCode = 0` (**Home**)  
  - Domestic GB VAT rules.

- `ExportTypeCode = 1` (**Export**)  
  - Ignore completely for VAT submission.

- `ExportTypeCode = 2` (**NorthernIreland**)  
  - Domestic VAT rules plus NI acquisition VAT.

## Canonical HMRC VAT Submission Fields

- `vatDueSales = SalesVat`  
- `vatDueAcquisitions = NIPurchasesVat`  
- `totalVatDue = SalesVat + VatDueAcquisitions + VatAdjustment`  
- `vatReclaimedCurrPeriod = PurchasesVat + NIPurchasesVat`  
- `netVatDue = totalVatDue + PurchasesVat + NIPurchasesVat`  
- `totalValueSalesExVAT = Sales`  
- `totalValuePurchasesExVAT = Purchases + NIPurchases`  
- `totalValueGoodsSuppliedExVAT = 0`  
- `totalValueGoodsReceivedExVAT = 0`

## Polarity Rules

- `SalesVat` is positive.  
- `PurchasesVat` and `NIPurchasesVat` are negative.  
- All VAT arithmetic must preserve polarity.  
- Genesis VAT logic must remain O(1) and unchanged.

## Rewrite Requirements

For all affected objects:

- Remove `EUJurisdiction` logic.  
- Join to `Subject.tbSubject.ExportTypeCode`.  
- Apply new jurisdiction semantics.  
- Exclude `ExportTypeCode = 1` rows entirely from VAT submission.  
- Treat NI sales as domestic.  
- Include NI acquisitions in `vatDueAcquisitions` and `vatReclaimedCurrPeriod`.  
- Remove legacy EU VAT semantics.  
- Remove export VAT semantics.  
- Preserve polarity.  
- Produce canonical HMRC VAT submission fields as defined above.

## Synthetic Dataset

Procedures `App.proc_Dataset*`

- Where `EUJuridiction == TRUE` pass in `@ExportTypeCode == 1`
- In `App.proc_DatasetSyntheticMIS_ProjectInit` set **PlasticSupplier** to `@ExportTypeCode == 2`
- For all other instances, set `@ExportTypeCode == 0`

## Objects Containing `EUJurisdiction` (must be rewritten)

- `App.proc_DatasetSyntheticMIS_ProjectInit`  
- `App.proc_DatasetSyntheticMIS_Assets`  
- `App.proc_DatasetSyntheticMIS_PayMisc`  
- `Cash.vwTaxVatAccruals`  
- `Cash.vwTaxVatAuditInvoices`  
- `Cash.vwTaxVatSummary`  
- `Subject.tbVirtual`  
- `Subject.vwDatasheet`  
- `Subject.vwStatusReport`  
- `Subject.vwVirtual`

## VAT Objects (review and rewrite where necessary)

- `App.proc_DatasetSyntheticMIS_TaxVat`  
- `Cash.proc_VatBalance`  
- `Cash.vwBalanceSheetVat`  
- `Cash.vwFlowVatPeriodAccruals`  
- `Cash.vwFlowVatPeriodTotals`  
- `Cash.vwFlowVatRecurrence`  
- `Cash.vwFlowVatRecurrenceAccruals`  
- `Cash.vwTaxVatAccruals`  
- `Cash.vwTaxVatAuditAccruals`  
- `Cash.vwTaxVatAuditInvoices`  
- `Cash.vwTaxVatDetails`  
- `Cash.vwTaxVatStatement`  
- `Cash.vwTaxVatSummary`  
- `Cash.vwTaxVatTotals` rename to `Cash.vwTaxVatSubmission`

## Referencing Objects (update where impacted)

- `proc_DatasetSyntheticMIS` → `proc_DatasetSyntheticMIS_TaxVat`  
- `proc_FlowCashCodeValues` → `vwTaxVatAccruals`  
- `proc_FlowCashCodeValues` → `vwTaxVatStatement`  
- `proc_PaymentPostMisc` → `vwTaxVatCashCodes`  
- `proc_TaxObligations` → `vwTaxVatStatement`  
- `vwBalanceSheet` → `vwBalanceSheetVat`  
- `vwStatementBase` → `vwTaxVatAccruals`  
- `vwStatementBase` → `vwTaxVatStatement`  
- `vwSummary` → `vwTaxVatAccruals`  
- `vwSummary` → `vwTaxVatStatement`

## Constraints

- Do not modify genesis VAT logic.  
- Do not modify polarity arithmetic.  
- Do not modify accounting surfaces.  
- Do not modify non‑VAT objects unless listed above.  
- Do not introduce EU semantics.  
- Do not include export VAT in any submission field.  
- NI acquisition VAT must be included.  
- NI dispatch VAT must be excluded.  
- NI sales VAT must be treated as domestic.

## Task

Rewrite all objects listed above in the SQL Server project `tcNodeDb4` to implement:

- The new jurisdiction model.  
- The new `Subject.tbExportType` table.  
- The new `ExportTypeCode` column on `Subject.tbSubject`.  
- Removal of `EUJurisdiction`.  
- Correct NI VAT semantics.  
- Correct export exclusion semantics.  
- Correct polarity rules.  
- Canonical HMRC VAT submission fields.

## Appendix 1 - dependancies

``` sql
SELECT 
    o.type_desc,
    CONCAT(SCHEMA_NAME(o.schema_id), '.', o.name) AS [object_name]
FROM 
    sys.objects o
WHERE 
    o.name LIKE '%vat%' COLLATE Latin1_General_CI_AI and NOT type_desc IN ('DEFAULT_CONSTRAINT')
ORDER BY 
    o.type_desc, o.name;

SELECT 
    OBJECT_NAME(d.referencing_id)   AS referencing_object,
    OBJECT_NAME(d.referenced_id)    AS referenced_object
FROM 
    sys.sql_expression_dependencies d
WHERE 
    OBJECT_NAME(d.referenced_id) LIKE '%vat%' COLLATE Latin1_General_CI_AI AND NOT OBJECT_NAME(d.referencing_id) LIKE '%vat%'
ORDER BY 
    referencing_object,
    referenced_object;
```

## Appendix 2 - Aider Files

/add docs/specs/tax-hub-vat-surface-sql.md  
/add docs/specs/tax-hub-spec-programme.md  
/add docs/specs/tc-development-contract.md  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_DatasetSyntheticMIS_Assets.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_DatasetSyntheticMIS_PayMisc.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_DatasetSyntheticMIS_ProjectInit.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_DatasetSyntheticMIS_TaxVat.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_DatasetSyntheticMIS.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_CO_MICRO_CUR_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql"  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_2026.sql"
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_NodeDataInit.sql"
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatAccruals.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatAuditInvoices.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatSummary.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwBalanceSheetVat.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwFlowVatPeriodAccruals.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwFlowVatPeriodTotals.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwFlowVatRecurrence.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwFlowVatRecurrenceAccruals.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatAccruals.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatAuditAccruals.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatAuditInvoices.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatDetails.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatStatement.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatSummary.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatTotals.sql  
/add "src/sqlnode/src/tcNodeDb4/Cash/Stored Procedures/proc_VatBalance.sql"  
/add "src/sqlnode/src/tcNodeDb4/Cash/Stored Procedures/proc_FlowCashCodeValues.sql"  
/add "src/sqlnode/src/tcNodeDb4/Cash/Stored Procedures/proc_PaymentPostMisc.sql"  
/add "src/sqlnode/src/tcNodeDb4/Cash/Stored Procedures/proc_TaxObligations.sql"  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwBalanceSheet.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwStatementBase.sql  
/add src/sqlnode/src/tcNodeDb4/Cash/Views/vwSummary.sql  

**End of Document**
