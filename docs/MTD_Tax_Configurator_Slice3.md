
# Tax Configurator — (Slice 3) Mappings Specification

Blazor implementation using **Admin Manager** pattern.

## 1. Purpose

The Tax Configurator is a **Blazor-based tool** for managing mappings between:

- **Tax Tags** (`Cash.tbTaxTag`)
- **Categories** (semantic categories)
- **Cash Codes** (transaction-level codes)

This slice covers **Component Tag → Category/Cash Code mappings** only.  
**Derived** and **Rollup** tags are **read-only** in this slice.

The UI is a **tree on the left** and a **details pane on the right**, following the **Admin Manager** pattern.

## 2. Scope of this slice

This slice implements:

- Loading **jurisdictions**, **tax sources**, and **tags**
- Grouping tags by **TagClass**
- Selecting a **Component** tag
- Viewing existing mappings
- Adding/removing/toggling mappings
- Server-side validation via stored procedure
- Blazor component structure (tree + details)

This slice **does not** implement:

- Derived tag logic
- Rollup tag logic
- Adjustments
- Capital allowances
- Final declaration
- Submission

Those belong to later slices.

## 3. Tag classes and mappability

`TagClassCode` defines the semantic role of a tag:

| TagClass  | Meaning              | Mappable? |
|-----------|----------------------|-----------|
| Component | Direct P&L line item | **Yes**   |
| Derived   | Computed field       | **No**    |
| Rollup    | Total/summary        | **No**    |

Only **Component** tags expose mapping controls in the details pane.  
**Derived** and **Rollup** tags appear in the tree but are **read-only**.

---

## 4. Data model

### 4.1 `App.tbJurisdiction`

- Top-level jurisdiction (e.g., `UK`).
- Root of the tree for future multi-jurisdiction support.

### 4.2 `Cash.tbTaxSource`

- Tax authority schemas (e.g., `UK-ITSA-SE-QU`, `UK-ITSA-SE-EOPS`).
- Linked to a jurisdiction.

### 4.3 `Cash.tbTaxTag`

- Individual submission fields within a tax source.

### 4.4 `Cash.tbTaxTagMap`

- Mappings between tags and categories/cash codes.

**Key fields:**

| Column        | Purpose                     |
|---------------|-----------------------------|
| `TaxSourceCode` | FK to tax schema          |
| `TagCode`       | FK to tag                 |
| `MapTypeCode`   | `0` = Category, `1` = Cash Code |
| `CategoryCode`  | When `MapTypeCode = 0`    |
| `CashCode`      | When `MapTypeCode = 1`    |
| `IsEnabled`     | `0/1`                     |

The **composite key**:

- `(TaxSourceCode, TagCode, MapTypeCode, CategoryCode/CashCode)`

is **intentional** and enforces semantic uniqueness. No surrogate ID is required.

---

## 5. Mapping invariants (Slice 2 rules)

These are **semantic invariants**, not UI hints:

1. **Prefer category mappings** (`MapTypeCode = 0`); they survive cash-code customisation.
2. **Use cash code mappings only for splits** within a category.
3. **Never map structural rollups** (categories with children in `tbCategoryTotal`).
4. **A category or cash code must not map to multiple tags** within the same source.
5. **Validation is mandatory** via:

   ```sql
   EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = ...
   ```

The UI must call validation after **every mutation**.

## 6. Blazor component structure

### 6.1 `TaxConfigurator.razor`

- Page shell.
- Loads jurisdictions, tax sources, and tags.
- Hosts tree + details components.
- Manages selected node state.

### 6.2 `TaxConfiguratorTree.razor`

- Renders the **LHS tree**.
- Emits `OnTagSelected` events.
- Uses Admin Manager patterns for:
    - expand/collapse
    - keyboard navigation
    - mobile behaviour

### 6.3 `TaxTagDetails.razor`

- **RHS details pane.**

For **Component** tags:

- Show mappings.
- Provide add/remove/toggle actions.

For **Derived/Rollup** tags:

- Show read-only explanation.
- No mapping controls.

### 6.4 `TaxConfiguratorService.cs`

- Loads jurisdictions, sources, tags, mappings.
- Performs CRUD operations.
- Calls validation stored procedure.
- Returns **domain DTOs**, not EF entities.

## 7. Tree structure (LHS)

Logical structure:

```text
ROOT (App.tbJurisdiction)
└─ Jurisdiction (e.g., UK)
   └─ TaxSource (e.g., UK-ITSA-SE-QU)
      ├─ Component
      │   ├─ turnover
      │   ├─ costOfGoods
      │   └─ ...
      ├─ Derived
      │   └─ adjustedProfitOrLoss
      └─ Rollup
          └─ ...
```

**Node types:**

- `jurisdiction`
- `source`
- `tagClass`
- `tag`

Only `tag` nodes with **TagClass = Component** are **mappable**.

## 8. Details pane (RHS)

### 8.1 Component tags

**Header:**

- Tag code
- Description
- TagClass = Component

**Mappings table:**

| Type | Code | Name | Enabled | Actions |
| --- | --- | --- | --- | --- |
| Category | ``CA-MOTOR`` | Motor running costs | ✓ | Remove |
| CashCode | ``CC-LOINT`` | Loan interest | ✓ | Remove |

**Actions:**

- Add Category Mapping
- Add Cash Code Mapping
- Toggle Enabled
- Remove Mapping

### 8.2 Derived/Rollup tags

Show:

- Tag code
- Description
- TagClass
- Message: **“This tag is computed and cannot be mapped.”**

No mapping grid or actions.

## 9. CRUD operations

### 9.1 Add category mapping

``` csharp
Task AddCategoryMappingAsync(string sourceCode, string tagCode, string categoryCode);
```

### 9.2 Add cash code mapping

``` csharp
Task AddCashCodeMappingAsync(string sourceCode, string tagCode, string cashCode);
```

### 9.3 Remove mapping

Use the composite key:

``` csharp
Task RemoveMappingAsync(
    string sourceCode,
    string tagCode,
    short mapTypeCode,
    string categoryCode,
    string cashCode
);

```

### 9.4 Toggle enabled

``` csharp
Task ToggleMappingEnabledAsync(
    string sourceCode,
    string tagCode,
    short mapTypeCode,
    string categoryCode,
    string cashCode
);
```

**After each mutation:**

1. Update UI optimistically.
2. Call server.
3. Run validation (Cash.proc_TaxTagMapValidate).
4. Refresh mapping list from server.

## 10. Validation logic

- **Duplicate mappings** (same source + tag + category/cash code): **block**.
- **Category is a rollup** (structural total): **block**.
    Detect via:
    - `CategoryTypeCode = 1`, or
    - presence as a parent in `tbCategoryTotal`.

- **Category/cash code mapped to multiple tags** within one source: **block**.
- All checks enforced via `Cash.proc_TaxTagMapValidate`.

## 11. Mobile behaviour

Follow **Admin Manager** pattern:

- Tree collapses into a **drawer/accordion**.
- On mobile, selecting a tag shows the **details pane full-screen**.
- Back navigation returns to the tree.

## 12. Security

Admin-only actions require:

``` csharp
User.IsInRole(Constants.AdministratorsRole)
```

Non-admin users:

- May view mappings (read-only).
- Cannot modify them.

## 13. Starting point

**Recommended: DB-driven tree from the start:**

- Load jurisdictions from `App.tbJurisdiction`.
- Load tax sources from `Cash.tbTaxSource`.
- Load tags from `Cash.tbTaxTag`.

The schema is **stable and small**; hard-coded trees are unnecessary.

## 14. Summary for GPT‑5.4‑mini

This document is the **authoritative spec for Tax Configurator Slice 3**:

- Blazor tree + details UI.
- `Jurisdiction → TaxSource → TagClass → Tag` structure.
- **Component** tags are mappable.
- **Derived/Rollup** tags are read-only.
- Category/cash code mappings with strict invariants.
- Validation via `Cash.proc_TaxTagMapValidate`.
- Admin Manager patterns for structure and UX.
- No adjustments/capital allowances/submission in this slice.

