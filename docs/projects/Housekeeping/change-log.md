# Build-warning housekeeping change log

## 4 September 2026

Implemented the recommendations approved in `findings.md` for the `SQL71502`, `CS8632`, and MudBlazor `MUD0002` warning families.

### Nullable annotations (`CS8632`)

- Added `<Nullable>annotations</Nullable>` to the unconditional `TCWeb` project properties. This makes nullable reference annotations part of the compiled API contract in Debug and Release without enabling the larger nullable-flow-analysis migration.
- Removed `CS8632` from the Debug-only `NoWarn` list. Debug and Release now use the same nullable-annotation context instead of hiding the warning in one configuration.
- Removed two ineffective nullable annotations from `Models/Usr_tbUser.cs`, which explicitly remains in a legacy `#nullable disable` context:
  - `ThemeCode`
  - `ThemeCodeNavigation`
- Changed the initial `Cash_tbCategoryExp.ErrorMessage` value in `EditExpression.cshtml.cs` from `null` to `string.Empty`. The model documents both null and empty as the no-error state, and this removes the page's existing `CS8625` warning under its explicit nullable-enabled context.

Full nullable flow analysis remains deferred. The project is deliberately set to `annotations`, not `enable`, as the investigation found a substantial pre-existing flow-analysis backlog.

### MudBlazor 9.5 migration (`MUD0002`)

- Removed invalid `ShowPager="true"` from 18 `MudDataGrid` instances. Every affected grid retains its existing `PagerContent` and `MudDataGridPager`.
- Removed invalid grid-level `Sortable="true"` from the same 18 grids. MudBlazor's default `SortMode.Multiple` preserves the intended enabled-sorting behaviour.
- Replaced 15 invalid `FilteredItemsChanged` handlers with the supported `FilterChanged` callback.
- The replacement handlers read the grid's supported `FilteredItems` property and continue to update visible-row counts, highlighted rows, and footer totals.
- Added `@ref` fields to `InvoiceRegisterDetailsGrid` and `InvoiceRegisterCashCodeGrid`, where a grid reference was not previously present.
- Removed six invalid `MudTabs.PanelClass` parameters. The `pt-3` and `pa-4` classes now live on the corresponding `MudTabPanel` components, preserving panel-content spacing without applying it to the tab headers.
- Did not suppress `MUD0002`; all reported invalid component attributes were corrected.

### SSDT temporary-table references (`SQL71502`)

- Removed the ineffective Debug-only `<SuppressTSqlWarnings>SQL71502</SuppressTSqlWarnings>` project property.
- Added numeric `<SuppressTSqlWarnings>71502</SuppressTSqlWarnings>` metadata only to the 14 synthetic-dataset child procedure build items that intentionally consume the caller-created `#DatasetCodes` table.
- No stored-procedure body or database runtime behaviour was changed.
- Unrelated unresolved-reference warnings remain enabled for the rest of the SQL project.

The narrowly suppressed procedures are:

- `proc_DatasetSyntheticMIS_Bootstrap`
- `proc_DatasetSyntheticMIS_ProjectInit`
- `proc_DatasetSyntheticMIS_ProjectTemplates`
- `proc_DatasetSyntheticMIS_ProjectTran`
- `proc_DatasetSyntheticMIS_ProjectInvoice`
- `proc_DatasetSyntheticMIS_ProjectPay`
- `proc_DatasetSyntheticMIS_PayInit`
- `proc_DatasetSyntheticMIS_PayMisc`
- `proc_DatasetSyntheticMIS_PayWages`
- `proc_DatasetSyntheticMIS_Expenses`
- `proc_DatasetSyntheticMIS_Assets`
- `proc_DatasetSyntheticMIS_CompanyLoanPayback`
- `proc_DatasetSyntheticMIS_SoleTraderCapitalIntroduced`
- `proc_DatasetSyntheticMIS_SoleTraderDrawings`

### Verification

All verification used clean/non-incremental or rebuild targets.

| Verification | Result |
| --- | --- |
| `dotnet build src/TCWeb/TCWeb.csproj --configuration Debug --no-restore --no-incremental` | Succeeded: 0 warnings, 0 errors |
| `dotnet build src/TCWeb/TCWeb.csproj --configuration Release --no-restore --no-incremental` | Succeeded: 0 warnings, 0 errors |
| Visual Studio MSBuild rebuild of `tcNodeDb4.sqlproj`, Debug/AnyCPU | Succeeded; no warning diagnostics |
| Visual Studio MSBuild rebuild of `tcNodeDb4.sqlproj`, Release/AnyCPU | Succeeded; no warning diagnostics |
| Visual Studio MSBuild rebuild of `tradecontrol.web.sln`, Debug/Any CPU | Succeeded; no warning or error diagnostics across included projects |
| Visual Studio MSBuild rebuild of `tradecontrol.web.sln`, Release/Any CPU | Succeeded; no warning or error diagnostics across included projects |
| Search for removed MudBlazor attributes and callback name | No remaining `ShowPager`, `FilteredItemsChanged`, `OnFilteredItemsChanged`, or `PanelClass` occurrences in TCWeb Razor files |
| `git diff --check` | Passed; only Git line-ending notices were printed |

No database deployment or browser-based UI interaction was performed. Compile-time component binding is verified; filtering, clearing filters, pagination, footer totals, highlighted VAT rows, and tab spacing should receive a focused UI smoke test when the application is next run against a development database.
