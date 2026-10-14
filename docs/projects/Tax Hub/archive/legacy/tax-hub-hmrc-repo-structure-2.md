# hmrc_mtd — Objective 2 Repository Structure  

August 2026  
Version: Objective 2.3  
Status: Architectural Specification  
Scope: HMRC Integration Module (GitHub Submodule for TCWeb)

## 1. Overview (Updated)

The `hmrc_mtd` repository provides:

- Test harness payload generation (Objective 2)  
- HMRC API payload generation (Objective 3)  
- HMRC transport (Objective 4)  
- Submission audit and history  
- WebHarness API for development/testing  
- Future HMRC Alignment Agent  

The module runs inside TCWeb’s process.

## 2. High-Level Architecture

``` text
TCWeb (multi-tenant host)
|
|-- passes TenantId, SubjectId, Period, TaxType
|-- passes SQL connection string
|-- passes optional XML snapshot (audit)
|
hmrc_mtd (submodule)
|
|-- loads TC accounting views (VAT, business tax, reconciliation)
|-- loads HMRC obligations + submissions
|-- compares TC vs HMRC state
|-- generates canonical payload
|-- validates + reconciles
|-- performs OAuth + fraud headers
|-- submits to HMRC
|-- logs submission + alignment status
|
```

TCWeb has **no HMRC knowledge**.  
All HMRC semantics live inside `hmrc_mtd`.

## 3. Repository Layout (Updated)

### New structure

``` text
hmrc_mtd/
│
├── src/
│   ├── Models/
│   │   ├── Harness/
│   │   │   ├── QuHarnessPayload.cs
│   │   │   ├── EopsHarnessPayload.cs
│   │   │   ├── MicroHarnessPayload.cs
│   │   │   ├── VatHarnessPayload.cs
│   │   │   └── HarnessEnvelope.cs
│   │   │
│   │   ├── Hmrc/
│   │   │   ├── VatHmrcPayload.cs
│   │   │   ├── QuHmrcPayload.cs
│   │   │   ├── EopsHmrcPayload.cs
│   │   │   ├── Ct600XmlPayload.cs
│   │   │   └── IxbrlPayload.cs
│   │   │
│   │   ├── Tc/
│   │   │   ├── TcVatStatement.cs
│   │   │   ├── TcBusinessTaxView.cs
│   │   │   ├── TcReconciliation.cs
│   │   │   └── TcSubmissionHistory.cs
│   │   │
│   │   └── Alignment/
│   │       ├── AlignmentStatus.cs
│   │       └── AlignmentReport.cs
│   │
│   ├── Services/
│   │   ├── Runner/
│   │   │   ├── HmrcSubmissionRunner.cs
│   │   │   └── HmrcSubmissionRequest.cs
│   │   │
│   │   ├── Harness/
│   │   │   ├── PayloadBuilders/
│   │   │   │   ├── QuHarnessPayloadBuilder.cs
│   │   │   │   ├── EopsHarnessPayloadBuilder.cs
│   │   │   │   ├── MicroHarnessPayloadBuilder.cs
│   │   │   │   └── VatHarnessPayloadBuilder.cs
│   │   │   │
│   │   │   ├── Validators/
│   │   │   │   ├── QuValidator.cs
│   │   │   │   ├── EopsValidator.cs
│   │   │   │   ├── MicroValidator.cs
│   │   │   │   └── VatValidator.cs
│   │   │   │
│   │   │   └── Controllers/
│   │   │       ├── QuTestController.cs
│   │   │       ├── EopsTestController.cs
│   │   │       ├── MicroTestController.cs
│   │   │       └── VatTestController.cs
│   │   │
│   │   ├── Hmrc/
│   │   │   ├── PayloadBuilders/
│   │   │   ├── Validators/
│   │   │   ├── XmlBuilders/
│   │   │   ├── IxbrlBuilders/
│   │   │   └── ApiModels/
│   │   │
│   │   ├── Mapping/
│   │   │   ├── TagMapper.cs
│   │   │   └── CategoryMapper.cs
│   │   │
│   │   ├── Transport/
│   │   │   ├── HmrcClient.cs
│   │   │   ├── OAuthService.cs
│   │   │   └── FraudHeaderService.cs
│   │   │
│   │   ├── TcData/
│   │   │   ├── TcVatReader.cs
│   │   │   ├── TcBusinessTaxReader.cs
│   │   │   ├── TcReconciliationReader.cs
│   │   │   └── TcSubmissionHistoryReader.cs
│   │   │
│   │   └── Alignment/
│   │       ├── AlignmentEngine.cs
│   │       └── AlignmentScheduler.cs
│   │
│   ├── Infrastructure/
│   │   ├── Db/
│   │   │   ├── ConnectionFactory.cs
│   │   │   └── SqlHelpers.cs
│   │   ├── Logging/
│   │   │   └── SubmissionLogger.cs
│   │   └── Config/
│   │       ├── HmrcSettings.cs
│   │       └── EnvironmentSelector.cs
│   │
│   └── hmrc_mtd.csproj
│
├── tests/
│   ├── HarnessTests/
│   ├── HmrcPayloadTests/
│   ├── ValidationTests/
│   ├── TransportTests/
│   ├── AlignmentTests/
│   └── WebHarnessTests/
│
└── docs/

```

## 4. WebHarness API

Purpose: **developer testing**, mirroring TCExport’s WebHarness.

Endpoints:

``` text
POST /harness/itsa/qu
POST /harness/itsa/eops
POST /harness/mtd/micro
POST /harness/vat
GET  /harness/status?taxType=VAT&period=2024Q2
```

Each endpoint accepts:

- connection string  
- tenant ID  
- subject ID  
- period  
- tax type  
- optional XML snapshot  

Returns:

- validation results  
- canonical payload  
- HMRC submission simulation (sandbox)  
- alignment status (TC vs HMRC)

## 5. HMRC Alignment Agent (future)

Runs as:

- Azure Function  
- WebJob  
- or background worker inside TCWeb

Responsibilities:

- read TC VAT/business views  
- read HMRC obligations + submissions  
- compare TC vs HMRC  
- write alignment status  
- notify TCWeb (dashboard, warnings)

Uses:

- `AlignmentEngine`  
- `AlignmentScheduler`  
- `TcData` readers  
- `HmrcClient`

## 6. Integration Contract (TCWeb → hmrc_mtd)

TCWeb passes:

``` json
{
"tenantId": "...",
"subjectId": "...",
"period": "2024Q2",
"taxType": "VAT",
"connectionString": "...",
"xmlSnapshot": "<TCExport>...</TCExport>"
}
```

hmrc_mtd returns:

``` json
{
"status": "ready | conflict | mismatch | already_submitted | error",
"hmrc": { ... },
"tc": { ... },
"comparison": { ... },
"submissionReference": "...",
"submittedAt": "..."
}
```

TCWeb never sees HMRC payloads.

## 7. Summary

Updated to reflect:

- Harness payloads (Objective 2)  
- HMRC payloads (Objective 3)  
- Transport (Objective 4)  
- Workflow integration (Objective 5)

**End of document.**