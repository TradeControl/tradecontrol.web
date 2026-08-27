# Tax Hub — Implementation 2  

## Objective 2: Submission Logic (Test Harness Payloads)

August 2026  
Version: Objective 2.2  
Status: Updated Implementation Instructions

## 1. Purpose

Objective 2 defines the **Submission Logic Layer** for the Tax Hub.

This layer generates **internal test harness payloads** from Trade Control accounting data.  
These payloads are **not HMRC payloads**.  
They are raw tag sets used for development, validation, and mapping verification.

HMRC payloads are defined separately under **Objective 3**.

## 2. Delivery Process

Read the following documents in order:

1. **tc-design-principles.md**  
2. **tc-development-contract.md**  
3. **tax-hub-spec-programme.md** (updated objectives)  
4. **tax-hub-implementation-2.md** (this document)  
5. **tax-hub-test-payloads.md** (harness payload spec)  
6. **tax-hub-hmrc-repo-structure.md** (updated repository structure)  
7. **tax-hub-workplan-2.md** (updated work plan)

Follow this sequence before implementation.

## 3. Instructions

Objective 2 must:

- Implement the **test harness payload models**  
- Implement the **test harness payload builders**  
- Implement the **test harness validators**  
- Implement the **test harness submission runner**  
- Implement the **WebHarness API**  

Objective 2 must **not**:

- Implement HMRC payloads  
- Implement HMRC transport  
- Implement CT600 XML  
- Implement iXBRL  
- Implement OAuth  
- Implement fraud headers  

These belong to Objectives 3 and 4.

## 4. Scope of Implementation

Objective 2 implements:

- Harness payload models  
- OperationType request/response contracts  
- Dataset readers for the authoritative SQL views  
- Payload builders for QU, EOPS, Micro, VAT (harness only)  
- Validators  
- Mapping utilities  
- Submission runner  
- WebHarness endpoints

Objective 2 does **not** implement HMRC submission.

## 5. Payload Model and OperationType Function Declarations

The HMRC_MTD module uses a TCExport-style function-call model. OperationType is
treated as a function declaration. Each function has a fixed parameter list,
fixed return type, and a dedicated validator.

TCWeb sends a JSON payload that encodes a function call:

```json
{
  "OperationType": "<function-name>",
  "Parameters": {
      "<parameter-name>": "<value>",
      ...
  }
}
```

The HMRC_MTD module must:

1. Identify the function from OperationType.
2. Validate the Parameters object using the correct validator.
3. Execute the function using the correct executor.
4. Return a canonical response payload.

OperationType determines the parameter list. Parameters must not be optional
unless explicitly stated. Enquiry functions do not use periodCode.

### 5.1 Submission Function Declarations

Submission functions generate canonical HMRC payloads from Trade Control
accounting data and submit them to HMRC. TCWeb always sends the HMRC period end
date as periodCode.

#### SUBMIT_VAT()

``` c
SUBMIT_VAT(
    taxSourceCode,     // e.g. "UK_MTD_VAT"
    periodEndOn,       // HMRC period end date (from Cash.vwTaxVatTotals.StartOn)
    tenantId,
    subjectId,
    connectionString,
    environment        // "sandbox" | "production"
)
```

#### SUBMIT_QU()

``` c
SUBMIT_QU(
    taxSourceCode,     // "QU"
    periodTo,          // HMRC period end date (from Cash.vwTaxBizSubmission.PeriodTo)
    tenantId,
    subjectId,
    connectionString,
    environment
)
```

#### SUBMIT_EOPS()

``` c
SUBMIT_EOPS(
    taxSourceCode,     // "EOPS"
    periodTo,
    tenantId,
    subjectId,
    connectionString,
    environment
)
```

#### SUBMIT_MICRO()

``` c
SUBMIT_MICRO(
    taxSourceCode,     // "MICRO"
    periodTo,
    tenantId,
    subjectId,
    connectionString,
    environment
)
```

### 5.2 Enquiry Function Declarations

Enquiry functions retrieve HMRC state directly from HMRC’s MTD APIs. They do not
use periodCode. Optional date ranges may be supplied if supported by HMRC.

#### GET_OBLIGATIONS()

``` c
GET_OBLIGATIONS(
    tenantId,
    subjectId,
    obligationStatus,  // "open" | "fulfilled"
    environment
)
```

#### GET_SUBMISSIONS()

``` c
GET_SUBMISSIONS(
    tenantId,
    subjectId,
    limit,             // e.g. 4
    dateFrom?,         // optional
    dateTo?,           // optional
    environment
)
```

#### GET_LIABILITIES()

``` c
GET_LIABILITIES(
    tenantId,
    subjectId,
    limit,
    dateFrom?,         // optional
    dateTo?,           // optional
    environment
)
```

#### GET_PAYMENTS()

``` c
GET_PAYMENTS(
    tenantId,
    subjectId,
    limit,
    dateFrom?,         // optional
    dateTo?,           // optional
    environment
)
```

### 5.3 Submission Return Payload

All submission functions return the same canonical structure:

``` json
{
    "status": "success | validation_error | hmrc_error",
    "canonicalPayload": { ... },
    "hmrcResponse": { ... },
    "submissionReference": "...",
    "submittedAt": "YYYY-MM-DDTHH:MM:SS",
    "warnings": [ ... ],
    "errors": [ ... ]
}
```

### 5.4 Enquiry Return Payloads

Enquiry functions return HMRC’s canonical MTD VAT enquiry payloads:

- obligations[]
- submissions[]
- liabilities[]
- payments[]

The HMRC_MTD module must not alter HMRC’s canonical structures.

### 5.5 Validator Requirements

Each OperationType must have a dedicated validator. Validators must:

- enforce required parameters
- enforce parameter types
- enforce parameter semantics
- reject unused parameters
- reject missing parameters
- reject invalid combinations (e.g., dateFrom without dateTo)
- ensure dataset availability for submission functions

Enquiry validators must be added to the Services.Validation namespace.

### 5.6 Period Semantics

Submission functions use HMRC period end dates:

- VAT → StartOn (from Cash.vwTaxVatTotals)
- QU/EOPS/Micro → PeriodTo (from Cash.vwTaxBizSubmission)

Enquiry functions do not use periodCode.

The HMRC_MTD module must not compute StartOn. It must use the dataset value
provided by TCWeb.

### 5.7 JSON Payload Implementation Rules

The coding model must implement:

- OperationType as a function declaration
- Parameters as the function argument list
- Validators bound to OperationType
- Canonical return payloads bound to OperationType
- Strict parameter enforcement
- Strict separation of submission vs enquiry semantics

This model follows the OperationModel conventions defined in the HMRC_MTD repository structure (see: tax-hub-hmrc-repo-structure.md, Section 3 — OperationModel).

## 6. Authoritative Dataset Surface

The HMRC_MTD module requires exactly two SQL views from the Trade Control
accounting database. These views provide the complete dataset surface for all
HMRC submission operations. No additional tables or views are required.

Submission functions (SUBMIT_VAT, SUBMIT_QU, SUBMIT_EOPS, SUBMIT_MICRO) must
read exclusively from these views.

Enquiry functions (GET_OBLIGATIONS, GET_SUBMISSIONS, GET_LIABILITIES,
GET_PAYMENTS) do not use SQL datasets; they query HMRC directly.

---

### 6.1 VAT Submission Dataset

VAT submissions use the `Cash.vwTaxVatTotals` view. Th HMRC period end date is the 'StartOn' column, which is the name of the composite Primary Key of Cash.tbYearPeriod ('YearNumber;StartOn')

Required columns:

```sql
SELECT YearNumber,
       Description,
       Period,
       StartOn,
       HomeSales,
       HomePurchases,
       ExportSales,
       ExportPurchases,
       HomeSalesVat,
       HomePurchasesVat,
       ExportSalesVat,
       ExportPurchasesVat,
       VatAdjustment,
       VatDue
FROM Cash.vwTaxVatTotals;
```

The `StartOn` column is the HMRC period end date and is passed to SUBMIT_VAT as `periodEndOn`.

### 6.2 Business Tax Submission Dataset (QU, EOPS, Micro)

Quarterly Update, End‑of‑Period Statement, and Micro submissions use the
`Cash.vwTaxBizSubmission` view. This view provides the HMRC‑aligned period end date (`PeriodTo`) directly.

Required columns:

``` sql
SELECT TaxSourceCode,
       TagCode,
       PeriodFrom,
       PeriodTo,          -- HMRC period end date
       TaxableAmount
FROM Cash.vwTaxBizSubmission;
```

### 6.3 Dataset Rules

- Submission functions must read only from these two views.
- HMRC_MTD must not compute HMRC period dates; it must use the values provided by TCWeb from these views.
- VAT submissions use `EndOn` from `Cash.vwTaxVatTotals`.
- QU/EOPS/Micro submissions use `PeriodTo` from `Cash.vwTaxBizSubmission`.
- Enquiry functions do not use SQL datasets.

These two views constitute the complete dataset surface required for HMRC_MTD submission operations.

## 7. Completion Criteria

Objective 2 is complete when:

- Harness payload models are implemented  
- Dataset readers return correct values  
- Payload builders produce correct harness payloads  
- Validators enforce strict rules  
- Mapping utilities behave deterministically  
- Submission runner dispatches correctly  
- WebHarness controllers return harness payloads  

**End of document.**
