# Instruction: Replace Legacy VAT Logic with Cash.vwTaxVatSubmission

3 August 2026

## Purpose

The VAT submission tests failed because the legacy dataset view `Cash.vwTaxVatTotals` exposes pre‑Brexit VAT semantics that cannot be mapped to HMRC’s canonical VAT payload fields. You must replace all VAT dataset reads with the new post‑Brexit canonical VAT surface: `Cash.vwTaxVatSubmission`.

This view provides the correct VAT period boundaries, canonical VAT fields, and adjustment logic required by Objective 2 of the Tax Hub Programme.

## Background: Why the legacy VAT logic failed

The old view `vwTaxVatTotals` contains EU-era fields (dispatches, acquisitions, export VAT, polarity logic). These cannot be translated into the canonical HMRC VAT tags:

- vatDueSales  
- vatDueAcquisitions  
- totalVatDue  
- vatReclaimedCurrPeriod  
- netVatDue  
- totalValueSalesExVAT  
- totalValuePurchasesExVAT  
- totalValueGoodsSuppliedExVAT  
- totalValueGoodsReceivedExVAT  

The new view `Cash.vwTaxVatSubmission` provides:

- HMRC-aligned VAT period end date (`VatEndOn`)  
- Canonical VAT fields already aggregated  
- VAT adjustments  
- Post‑Brexit semantics  
- Zeroed goods-supplied/received fields (correct for GB-only traders)

This is the correct dataset surface for Objective 2.

## Required Implementation Changes

### 1. TcVatReader

Replace all references to:

`Cash.vwTaxVatTotals`

with:

`Cash.vwTaxVatSubmission`

Load the following canonical fields:

- VatEndOn  
- vatDueSales  
- vatDueAcquisitions  
- totalVatDue  
- vatReclaimedCurrPeriod  
- netVatDue  
- totalValueSalesExVAT  
- totalValuePurchasesExVAT  
- totalValueGoodsSuppliedExVAT  
- totalValueGoodsReceivedExVAT  

Remove all references to legacy EU VAT fields.

### 2. VatPayloadBuilder

- Remove all legacy transformation logic.
- Implement direct 1:1 mapping from TcVatReader model to VAT payload model.
- Ensure `PeriodEndOn` is taken from `VatEndOn`.

### 3. VatValidator

- Validate canonical VAT fields only.
- Remove any checks referencing legacy EU VAT fields.

### 4. Submission Runner

- No changes required.

## Aider Files Appendix

### File: src/HMRC_MTD/Services/TcData/TcVatReader.cs

Required Changes:

- Replace SQL source table/view with `Cash.vwTaxVatSubmission`
- Update field mappings to match canonical VAT fields:
  - VatEndOn → PeriodEndOn
  - vatDueSales
  - vatDueAcquisitions
  - totalVatDue
  - vatReclaimedCurrPeriod
  - netVatDue
  - totalValueSalesExVAT
  - totalValuePurchasesExVAT
  - totalValueGoodsSuppliedExVAT
  - totalValueGoodsReceivedExVAT

### Notes

Remove all references to:

- ExportSales
- ExportPurchases
- ExportSalesVat
- ExportPurchasesVat
- Polarity logic
- EUJurisdiction

### File: src/HMRC_MTD/Services/Payload/VatPayloadBuilder.cs

Required Changes

- Remove all legacy transformation logic.
- Implement direct 1:1 mapping from TcVatReader model to VAT payload model.
- Ensure `PeriodEndOn` is taken from `VatEndOn`.

### File: src/HMRC_MTD/Services/Validation/VatValidator.cs

Required Changes

- Validate canonical VAT fields only.
- Remove any checks referencing legacy EU VAT fields.

### File: docs/tmp/vwTaxVatSubmission.sql

Reference Only

This file defines the canonical VAT surface. Do not modify it.

### File: docs/tmp/vwTaxVatTotals.sql

Reference Only

Deprecated pre-Brexit VAT surface.

## Implementation Directive

Use `Cash.vwTaxVatSubmission` as the authoritative VAT dataset for Objective 2.  
Do not use `vwTaxVatTotals` or any pre‑Brexit VAT logic.
