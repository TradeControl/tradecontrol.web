# SQL Node — Self Assessment Tax Integration Completion Brief

## Overview

The SQL Node repository contains the bootstrap procedures that construct the full accounting and tax environment for Sole Traders in TradeControl. This bootstrap must now support the submission models implemented in the HMRC MTD repository, specifically:

- MTD ITSA Self‑Employment (Quarterly Update + EOPS)
- SA100 / SA103F Self‑Employment

The HMRC MTD repository already contains the submission models, serializers, and API logic.  
The SQL Node must provide the corresponding **Tax Sources**, **Tax Tags**, and **Tag Mappings** so that the submission module can extract the correct totals from the Category Tree.

This brief describes the current state of the SQL Node bootstrap and the required changes to complete Self Assessment tax integration.

## Current State

### 1. Core Templates

The following procedures are complete and should not contain any tax logic:

- `App.proc_Template_ST_SOLE_CUR_MIN_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_2026`

These create the Sole Trader business environment (categories, cash codes, accounts, VAT handling, owner capital, etc.).

### 2. MTD Tax Procedure

`App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026` is implemented and contains:

- TagSource definitions for:
  - `UK-ITSA-SE-QU`
  - `UK-ITSA-SE-EOPS`
- Correct HMRC Quarterly Update (QU) tag list
- Correct HMRC EOPS tag list
- Validation

### 3. SA Tax Procedure

`App.proc_Template_ST_SOLE_CUR_TAX_SA_2026` is implemented and contains:

- TagSource definition for:
  - `UK-SA-SE-RETURN`
- Full SA103F canonical tag list
- Validation

### 4. Wrappers

Wrappers exist for MIN and STD variants:

- `App.proc_Template_ST_SOLE_CUR_MIN_MTD_2026`
- `App.proc_Template_ST_SOLE_CUR_MIN_SA_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_MTD_2026`
- `App.proc_Template_ST_SOLE_CUR_STD_SA_2026`

These wrappers currently call the MIN/STD templates but **do not yet call the tax procedures**.

### 5. Legacy Incorrect Code

Two sections contain outdated and incorrect MTD logic:

- **Section 10** in `proc_Template_ST_SOLE_CUR_MIN_2026`
- **Section 7** in `proc_Template_ST_SOLE_CUR_STD_2026`

These sections contain:

- Incorrect TagSource creation
- Incorrect TagSeed and EopsSeed lists (SA103F mash‑ups)
- Incorrect mappings
- Validation calls

These must be removed.

## Required Changes

### A. Remove Legacy Tax Code

#### 1. Remove Section 10 from MIN

Delete the entire block beginning with:

`-- 10. UK-ITSA-* tag mappings (Category tree mappings)`

This includes TagSource creation, TagSeed, EopsSeed, mappings, and validation.

#### 2. Remove Section 7 from STD

Delete the entire block beginning with:

`-- 7. UK-ITSA-* Slice 2 mappings (STD-owned additions only)`

These mappings will be relocated to the STD wrappers.

### B. Update Wrappers to Call Tax Procedures

#### MIN_MTD Wrapper

After calling `proc_Template_ST_SOLE_CUR_MIN_2026`, add:

```sql
EXEC App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026;
```

#### MIN_SA Wrapper

After calling `proc_Template_ST_SOLE_CUR_MIN_2026`, add:

```sql
EXEC App.proc_Template_ST_SOLE_CUR_TAX_SA_2026;
```

#### STD_MTD Wrapper

After calling `proc_Template_ST_SOLE_CUR_STD_2026`, add:

```sql
-- TODO: Insert STD-owned mappings (formerly section 7)
EXEC App.proc_Template_ST_SOLE_CUR_TAX_MTD_2026;
```

This ensures tax logic is executed exactly once and only via wrappers.

### C. Mapping Task (To Be Completed)

Mapping is intentionally deferred because it requires analysis of the Category Tree.

The mapping task consists of:

1. Inspecting:
    - Cash.tbCategory
    - Cash.tbCode
    - Category roll‑ups

2. For each tag in:
    - MTD QU
    - MTD EOPS
    - SA100 / SA103F

    determine whether it maps to:

    - a CategoryCode (e.g., CT-TURNOV, CT-OVERHD)
    - a CashCode (e.g., CC-LOINT, CC-PROF)
    - or remains unmapped

3. Insert mappings into wrappers:
    - MIN wrappers: MIN-owned categories only
    - STD wrappers: MIN + STD-owned categories (travel, motor, premises, admin, finance)

Mappings must not be added to MIN or STD templates.

After inserting mappings for each TaxSourceCode (UK-ITSA-SE-QU, UK-ITSA-SE-EOPS, UK-SA-SE-RETURN), call `Cash.proc_TaxTagMapValidate` for that source to verify the integrity of the Tag→Category/CashCode mappings.

## Summary

To complete Self Assessment tax integration:

1. Remove legacy tax code from MIN and STD.
2. Ensure wrappers call the correct tax procedures.
3. Relocate STD-owned mappings into STD wrappers.
4. Perform Category Tree analysis and add correct mappings into wrappers.

The TagSource and TagSeed definitions for both MTD and SA are already implemented.

This completes the SQL bootstrap required for HMRC MTD and SA100 submissions.

## Appendix - files

src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsAdjustments.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsAllowances.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsLosses.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsMetadata.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationDeductions.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationIncomeSummary.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationMetadata.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationTaxCalculation.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaBalanceDetails.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaChargeDetail.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaLiabilitiesEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaLiabilitiesRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaLiabilitiesResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Obligations/SaObligation.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Obligations/SaObligationsEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Obligations/SaObligationsRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Obligations/SaObligationsResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Payments/SaPayment.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Payments/SaPaymentsEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Payments/SaPaymentsRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Payments/SaPaymentsResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateAdjustments.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateExpenses.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateIncome.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdAdjustment.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdBusiness.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdError.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdExpenseCategory.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdIncomeCategory.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdMetadata.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdPeriod.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100BasisPeriodSummary.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100CapitalAllowanceSummary.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100LossSummary.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa102.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa102Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa103F.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa103FSerializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa105.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa105Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa106.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa106Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa108.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa108Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa110.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa110Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaCanonicaliser.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaEnvelope.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaEnvelopeBuilder.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaEnvelopeHeader.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaEnvelopeSerializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaIdAuthentication.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaIrmarkGenerator.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaMessageDetails.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaScheduleDocument.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaSenderDetails.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaSubmissionBuilder.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Shared/JsonExtract.cs  
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_NodeDataInit.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_BASE_MIN_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_SA_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_MTD_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_SA_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_TAX_MTD_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_TAX_SA_2026.sql
