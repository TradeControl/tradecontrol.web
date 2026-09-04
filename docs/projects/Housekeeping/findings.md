# Build-warning housekeeping findings

Date: 4 September 2026

Status: investigation and recommendations only. No production, project, or submodule source files have been changed.

## Executive recommendation

The three warning families do not have the same remedy:

| Warning | Cause | Recommended treatment |
| --- | --- | --- |
| `SQL71502` for `#DatasetCodes` | SSDT compiles each stored procedure independently and cannot infer the caller-created, session-scoped temporary table contract. | Targeted suppression on the synthetic-dataset child procedures. Do not suppress `SQL71502` project-wide. |
| `CS8632` | `TCWeb` uses nullable annotations but does not enable nullable annotations/analysis at project level. Debug currently hides this warning; Release does not. | Stage nullable adoption. First use `annotations` (or file/folder-scoped `#nullable enable annotations`) to make existing `?` syntax valid without unleashing the full analysis backlog; later enable full nullable analysis and fix it in bounded batches. |
| `MUD0002` for `ShowPager` | `TCWeb` targets MudBlazor 9.5.0, where `ShowPager` is not a `MudDataGrid` parameter. Paging is supplied by `PagerContent`. | Remove `ShowPager`; retain the existing `<PagerContent><MudDataGridPager /></PagerContent>`. Do not suppress the analyzer. |

The MudBlazor build also exposes related obsolete/invalid attributes (`Sortable`, `FilteredItemsChanged`, and `PanelClass`). They should be addressed in the same migration because fixing only `ShowPager` leaves most `MUD0002` warnings and some currently intended behaviour is not being wired to a real component parameter.

## Reproduction baseline

The following read-only/rebuild checks were run against the current checkout:

- `dotnet build src/TCWeb/TCWeb.csproj --configuration Release --no-restore --no-incremental`: succeeds with 265 warnings: 208 `CS8632` and 57 `MUD0002`.
- The same TCWeb build in Debug reports the 57 `MUD0002` warnings but hides `CS8632` through the Debug-only `NoWarn` entry in `TCWeb.csproj`.
- A Visual Studio MSBuild rebuild of `tcNodeDb4.sqlproj` succeeds and emits the `SQL71502` family for `#DatasetCodes` in the synthetic-dataset child procedures.
- `dotnet build src/tradecontrol.web.sln` is not a valid SQL-project verification route on this machine: the .NET CLI cannot load the legacy SSDT targets. Use Visual Studio `MSBuild.exe` for the solution/SQL-project acceptance build.

Build output may print warning lines once during compilation and again in its summary. Counts above are de-duplicated actual diagnostics, not raw console-line counts.

## 1. SQL71502: `#DatasetCodes`

### Finding

`App.proc_DatasetSyntheticMIS` creates `#DatasetCodes` and calls the synthetic-dataset procedures on the same SQL connection. Local temporary tables are visible to nested procedures, so the runtime design is coherent. The child procedures also check `OBJECT_ID('tempdb..#DatasetCodes')` and throw when called outside the orchestrator.

SSDT nevertheless validates each stored procedure as an independent model object. It sees references in the child procedure but not the caller's `CREATE TABLE`, hence the unresolved-object and unresolved-column warnings. The highlighted `proc_DatasetSyntheticMIS_Assets` procedure produces several diagnostics (reads plus its `MERGE`), not just a single warning.

This is a family-wide contract: 14 child procedure files reference the caller-owned table, while the fifteenth referencing file is the orchestrator that creates it. The affected children are Bootstrap, ProjectInit, ProjectTemplates, ProjectTran, ProjectInvoice, ProjectPay, PayInit, PayMisc, PayWages, Expenses, Assets, CompanyLoanPayback, SoleTraderCapitalIntroduced, and SoleTraderDrawings.

There is already a Debug-only project setting:

```xml
<SuppressTSqlWarnings>SQL71502</SuppressTSqlWarnings>
```

It is ineffective in the verified Debug rebuild. SSDT warning lists use the numeric code (`71502`), not the display prefix (`SQL71502`). It is also absent from Release.

### Recommended fix

Use `<SuppressTSqlWarnings>71502</SuppressTSqlWarnings>` as item metadata on the 14 named synthetic-dataset child `<Build Include="...">` entries. This records the known cross-procedure temp-table limitation at the narrowest practical scope while preserving `SQL71502` for unrelated, genuinely missing objects elsewhere in the database project. Remove the ineffective Debug-only project-level entry after the targeted metadata is verified.

Before committing, rebuild Debug and Release with Visual Studio MSBuild and confirm that:

1. all `#DatasetCodes` diagnostics disappear;
2. a deliberately unresolved reference in an unrelated scratch procedure would still be reported (or, less intrusively, inspect the evaluated project to confirm suppression metadata is limited to the intended items);
3. the generated DACPAC remains valid; and
4. the synthetic dataset executes through `App.proc_DatasetSyntheticMIS` for company and sole-trader paths.

### Alternatives considered

- **Project-wide `71502` suppression:** small change, but rejected because it can conceal future schema mistakes unrelated to this intentional temp-table pattern.
- **Add a conditional `CREATE TABLE #DatasetCodes` declaration to every child:** may satisfy static analysis, but duplicates the schema in 14 places and risks drift. It adds dead-path code solely for the build system.
- **Table-valued parameter:** not a direct replacement because SQL Server TVPs are read-only, whereas several children `MERGE` new codes back into the shared table.
- **Permanent keyed staging table or a monolithic procedure:** eliminates the modelling limitation but materially changes lifetime, cleanup, concurrency, and transaction semantics. This is disproportionate housekeeping work.

Conclusion: this is one of the uncommon cases where narrow suppression is more honest than a structural code rewrite.

## 2. CS8632: nullable annotations outside a nullable context

### Finding

`TCWeb.csproj` has no `<Nullable>` setting, while newer application/service code uses nullable reference annotations extensively. Debug suppresses `CS8632`; Release does not. The other current .NET submodule projects generally use `<Nullable>enable</Nullable>`, so TCWeb is the outlier.

The warning is not evidence that the annotated values are wrong. It means the compiler is being asked to parse nullable intent while nullable annotations are disabled. Suppressing it discards useful type-contract information.

A trial build with `-p:Nullable=enable` confirms that an immediate full switch is too broad for a warning-cleanup change: it removes almost all `CS8632` diagnostics but exposes about 219 substantive nullable-flow/initialisation warnings (`CS8618`, `CS8625`, `CS8604`, `CS8601`, `CS8600`, `CS8603`, `CS8602`, and related codes). Two `CS8632` diagnostics remain in `Models/Usr_tbUser.cs` because that generated/legacy model explicitly says `#nullable disable` but contains `?` annotations.

The large `Models` area and `Data/NodeContext.cs` already carry explicit `#nullable disable` directives, apparently reflecting generated or legacy Entity Framework code. A project-wide full enable would therefore be a migration, not a harmless warning toggle.

### Recommended staged fix

1. **Warning-clean baseline:** set `<Nullable>annotations</Nullable>` in the unconditional TCWeb property group, remove `CS8632` from Debug `NoWarn`, and fix the internally inconsistent `Usr_tbUser.cs` directive/annotations. `annotations` makes `T?` contracts legal but does not yet enable flow warnings.
2. **Preserve legacy boundaries:** retain explicit `#nullable disable` in generated/legacy EF model files for now. If these files are regenerated, prefer configuring the generator rather than hand-editing hundreds of outputs. For `Usr_tbUser.cs`, either remove the two reference-type `?` annotations to match the disabled file or enable annotations for that file; choose based on whether it is generated and reproducible.
3. **Adopt analysis by area:** migrate maintained code folders (for example `AppServices`, then page models/components, then data access) using `#nullable enable` or scoped MSBuild item metadata. Fix real `CS86xx` findings in each batch and add tests around boundary/null cases.
4. **End state:** change TCWeb to `<Nullable>enable</Nullable>` only when the full analysis build is clean, leaving explicit opt-outs solely on generated legacy code with a comment or generator rationale.

If the team wants the smallest possible change, `<Nullable>annotations</Nullable>` is preferable to suppressing `CS8632`: both quiet this diagnostic, but only the former makes the compiler understand and emit the intended nullable metadata. Continuing the current Debug-only suppression is not recommended, and adding Release suppression would deepen the configuration mismatch.

## 3. MUD0002: invalid MudBlazor attributes

### Finding

The project references MudBlazor 9.5.0. Its `MudDataGrid<T>` API has `PagerContent`, but no `ShowPager` parameter. Every affected grid already supplies a `MudDataGridPager` inside `PagerContent`, so `ShowPager="true"` is redundant invalid markup rather than a functional paging switch.

There are 18 source files containing `ShowPager`; the clean Release build reports 18 actual `ShowPager` diagnostics. The apparent count of 36 in raw grouped output is the compiler plus repeated build-summary output.

The same 57-diagnostic `MUD0002` set comprises:

| Invalid parameter | Actual diagnostics | Required migration |
| --- | ---: | --- |
| `MudDataGrid.ShowPager` | 18 | Remove it; `PagerContent` already enables the pager. |
| `MudDataGrid.Sortable` | 18 | Remove it when sorting should remain enabled, because `SortMode` defaults to `Multiple`. If sorting should be disabled, express that on individual columns using their valid `Sortable` parameter (or use the supported grid `SortMode` where appropriate). Current `Sortable="true"` has no effect. |
| `MudDataGrid.FilteredItemsChanged` | 15 | Replace with the valid `FilterChanged` callback and read `_grid.FilteredItems` after the filter change to refresh visible-row counts and footer totals. Confirm callback timing with a component test/manual filter test. Current callback is not a real MudDataGrid event. |
| `MudTabs.PanelClass` | 6 | Move the CSS class to each `MudTabPanel` via its valid class parameter, or wrap the panel content in an element with that class, depending on whether the styling is intended per panel or for the shared content host. Inspect rendered layout before choosing. |

Counts above are based on unique diagnostics from the Release no-incremental build.

### Recommended fix for the highlighted warning

Apply this mechanical change to the 18 grids:

```razor
<MudDataGrid ...
             Sortable="true"
             ShowPager="true">
```

becomes, for the paging/sorting defaults currently intended:

```razor
<MudDataGrid ...>
```

and retain:

```razor
<PagerContent>
    <MudDataGridPager />
</PagerContent>
```

Do not add `MUD0002` to `NoWarn`. The analyzer is correctly detecting attributes that Razor otherwise places into unmatched attributes and that MudBlazor does not consume as component parameters.

For `FilteredItemsChanged`, a representative replacement should follow this shape (exact async timing to be verified during implementation):

```razor
FilterChanged="OnFilterChanged"
```

```csharp
private void OnFilterChanged(IReadOnlyCollection<IFilterDefinition<RowType>> _)
{
    VisibleItems = _grid?.FilteredItems?.ToList() ?? Items.ToList();
    RecalculateTotals();
}
```

Because these callbacks drive displayed row counts and totals, this is a functional correction rather than cosmetic suppression. Test filtering, clearing filters, pagination, row counts, and footer totals on at least one invoice grid and each distinct Tax Hub grid pattern.

## Proposed implementation order

1. Fix all four MudBlazor invalid-attribute groups together and validate UI behaviour; this is the clearest code/API correction.
2. Establish the TCWeb nullable `annotations` baseline and remove the Debug suppression; treat full nullable analysis as subsequent bounded work.
3. Apply targeted numeric `71502` suppression to the 14 SQL child procedure build items and verify Debug/Release SSDT builds plus synthetic-dataset execution.
4. Run clean, non-incremental Debug and Release builds with Visual Studio MSBuild and record remaining warning families before expanding housekeeping scope.

## Files/configuration expected to change after review

- `src/TCWeb/TCWeb.csproj` (nullable context and removal of the `CS8632` suppression)
- the 18 Razor files currently containing `ShowPager`, plus the files using `FilteredItemsChanged` and the affected `MudTabs`
- possibly `src/TCWeb/Models/Usr_tbUser.cs`, subject to confirming its generation source
- `src/sqlnode/src/tcNodeDb4/tcNodeDb4.sqlproj` only (targeted warning metadata; the SQL procedure bodies need not change)

No implementation changes should begin until the suppression-versus-migration choices above are approved.
