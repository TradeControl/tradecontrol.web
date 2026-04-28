# Enquiry: UK MTD Sole Trader tag mapping (2026 templates)

## Summary

We’re implementing UK Making Tax Digital (MTD) export mapping for a Sole Trader template in our accounting system.  
The schema supports mapping HMRC tag codes to either:

- A category node in our cash category tree (`MapTypeCode = 0`, `CategoryCode`), or
- A specific cash code (`MapTypeCode = 1`, `CashCode`).

Our goal is a **practical, conservative mapping** that avoids double counting and works with a typical sole trader P&L structure. We can refine granularity later.

## What we need from you

Please propose an HMRC-compatible mapping for a **Sole Trader** (income tax) business using the information below.

### Outputs requested

1) A list of HMRC tag codes (and their names/meaning) that should be used for Sole Trader reporting.
2) A proposed mapping from our Categories / Cash Codes to those HMRC tags:
   - Prefer category-level totals where appropriate.
   - Use cash-code mappings only where category-level isn’t sufficient or creates overlap.
3) Notes on:
   - Which tags are “rollups” vs “components” (if applicable).
   - Any ambiguous tags or alternative interpretations.

## Constraints / rules

- The category tree is hierarchical. If you map a tag to a parent category total, **do not also map descendant cash codes to the same tag** (it will double count).
- Some tags may represent rollups (summary values) and may overlap component tags. If overlap is expected, identify those tags as rollups.
- Non-trade/disconnected operational categories (e.g., transfers, balance-sheet movement buckets) should not force mapping warnings.

## Data we can provide

We can provide an Excel workbook snapshot of:

- The Sole Trader category tree (parent/child totals)
- The nominal categories
- The cash codes and which nominal categories they belong to
- Existing UK-MTD tags already used for Micro templates (if relevant)

## Target Data XLS Pages

1. Category tree (totals rollup structure)
2. All categories (nominals/totals/expressions)
3. Cash codes with their nominal category
4. Identify disconnected nominal categories (not part of any total) - These are often operational/non-reporting categories (transfers etc.) and can be excluded from “must map” expectations.
5. Existing UK-MTD tags and mappings for Micro Company Accounts

## What to send back

Please respond with:

- The HMRC tag codes you recommend for Sole Trader reporting.
- A mapping table (tag → category/cash codes).
- Any assumptions or uncertainties.

We’re aiming for something conservative that can be upgraded later.
