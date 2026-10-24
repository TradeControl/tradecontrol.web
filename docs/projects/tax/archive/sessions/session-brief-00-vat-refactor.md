# VAT Canonical Migration — EF Model Alignment (ASP.NET Core)

## Objective

Align the ASP.NET Core EF models with the new canonical VAT SQL surface.  
Remove all legacy VAT fields from the EF layer and rewrite models to consume ONLY canonical VAT fields.

This process has four stages:

1) EF model selection  

2) Submission of SQL objects and EF models  
3) Model rewrite  
4) Compile error processing (to be handled after generation)

---

## 1) EF Model Selection

Completed.

## 2) Submission of SQL Objects and EF Models

### EF Models

The following models have been selected as the basis for the rewrite in Step 3.

Do NOT infer schema from previous legacy VAT fields.  
Use ONLY the canonical VAT SQL surface as the source of truth.

src/sqlnode/src/tcNodeDb4/Cash/Views/vwBalanceSheet.sql  
src/sqlnode/src/tcNodeDb4/Cash/Views/vwSummary.sql  
src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatSummary.sql  
src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatAuditAccruals.sql  
src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatAuditInvoices.sql  
src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatDetails.sql  
src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatStatement.sql  
src/sqlnode/src/tcNodeDb4/Cash/Views/vwTaxVatSumission.sql  
src/sqlnode/src/tcNodeDb4/Subject/Tables/tbExportType.sql  
src/sqlnode/src/tcNodeDb4/Subject/Tables/tbSubject.sql  
src/sqlnode/src/tcNodeDb4/Subject/Tables/tbVirtual.sql  
src/sqlnode/src/tcNodeDb4/Subject/Views/vwStatusReport.sql  
src/sqlnode/src/tcNodeDb4/Subject/Views/vwVirtual.sql  
src/sqlnode/src/tcNodeDb4/Subject/Views/vwDatasheet.sql  
src/TCWeb/Models/Cash_vwBalanceSheet.cs  
src/TCWeb/Models/Cash_vwSummary.cs  
src/TCWeb/Models/Cash_vwTaxVatAuditAccrual.cs  
src/TCWeb/Models/Cash_vwTaxVatAuditInvoice.cs  
src/TCWeb/Models/Cash_vwTaxVatDetail.cs  
src/TCWeb/Models/Cash_vwTaxVatStatement.cs  
src/TCWeb/Models/Cash_vwTaxVatSubmission.cs  
src/TCWeb/Models/Cash_vwTaxVatSummary.cs  
src/TCWeb/Models/Subject_tbSubject.cs  
src/TCWeb/Models/Subject_tbVirtual.cs  
src/TCWeb/Models/Subject_vwDatasheet.cs  
src/TCWeb/Models/Subject_vwStatusReport.cs  
src/TCWeb/Models/Subject_vwVirtual.cs  

### Data Context

src/TCWeb/Data/NodeContext.cs  

## 3) Model Rewrite

Rewrite the EF models and related classes so that they:

- remove all legacy VAT fields (e.g., HomeSales, ExportSales, HomePurchasesVat, NIPurchasesVat, etc.),
- add and map ONLY the canonical VAT fields exposed by the new SQL surface:

  - vatDueSales  
  - vatDueAcquisitions  
  - totalVatDue  
  - vatReclaimedCurrPeriod  
  - netVatDue  
  - totalValueSalesExVAT  
  - totalValuePurchasesExVAT  
  - totalValueGoodsSuppliedExVAT  
  - totalValueGoodsReceivedExVAT  
  - VatAdjustment (where present)

Update:

- entity properties,
- mapping configurations,
- LINQ projections,
- DTOs,
- view models.

Add:

- new model for table Cash.tbExportType

Do NOT:

- reintroduce legacy VAT fields,
- alias canonical fields under legacy names,
- preserve compatibility layers for legacy VAT logic.

The goal is to make the EF layer fail to compile wherever legacy VAT fields were used, so that all consumers must be updated.

---

## 4) Process Compile Errors (Post-Generation)

After the EF models and related classes are rewritten:

- the ASP.NET Core project will be recompiled,
- compile errors will identify all remaining references to legacy VAT fields,
- these errors will be used to drive further code updates in controllers, services, and UI components.

You do NOT need to define the exact steps for handling compile errors.  
They will be processed manually after your rewrite.

---

## STOP

After completing the EF model and related class rewrites, STOP.  
Do not produce a Work Plan, summary, explanation, or any additional output.  
Only output the modified EF and related C# files.

