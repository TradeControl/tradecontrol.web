using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradeControl.Web.AppServices;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.Accounts;

public class CashStatementModel : PageModel
{
    private const short LibrePrintMode = 1;
    private const short ExcelPrintMode = 2;
    private readonly NodeContext _nodeContext;
    private readonly ICashStatementGenerationService _generationService;
    private readonly CashStatementGenerationOptions _generationOptions;

    public CashStatementModel(
        NodeContext nodeContext,
        ICashStatementGenerationService generationService,
        IOptions<CashStatementGenerationOptions> generationOptions)
    {
        _nodeContext = nodeContext;
        _generationService = generationService;
        _generationOptions = generationOptions.Value;
    }

    [BindProperty, Required]
    public string Format { get; set; } = "excel";

    [BindProperty, Range(1, 600), Display(Name = "Command timeout")]
    public int CommandTimeout { get; set; } = 30;

    [BindProperty] public bool IncludeActivePeriods { get; set; }
    [BindProperty] public bool IncludeBalanceSheet { get; set; } = true;
    [BindProperty] public bool IncludeBankBalances { get; set; } = true;
    [BindProperty] public bool IncludeOrderBook { get; set; }
    [BindProperty] public bool IncludeTaxAccruals { get; set; } = true;
    [BindProperty] public bool IncludeVatDetails { get; set; } = true;
    [BindProperty] public bool IncludeReconciliation { get; set; }
    [BindProperty] public string? CategoryVersionId { get; set; }

    public string? ErrorMessage { get; private set; }
    public bool OdsEnabled => _generationOptions.OdsEnabled;
    public string ActiveCategoryVersionId { get; private set; } = string.Empty;
    public IReadOnlyList<CashStatementCategoryVersionOption> CategoryVersions { get; private set; } =
        Array.Empty<CashStatementCategoryVersionOption>();
    public CashStatementCategoryVersionOption? ActiveCategoryVersion =>
        CategoryVersions.SingleOrDefault(version => version.IsActive);

    public async Task OnGetAsync(CancellationToken ct)
    {
        var printMode = await _nodeContext.App_tbOptions
            .AsNoTracking()
            .OrderBy(option => option.Identifier)
            .Select(option => (short?)option.DefaultPrintMode)
            .FirstOrDefaultAsync(ct);

        Format = printMode == LibrePrintMode && OdsEnabled ? "libre" : "excel";
        await LoadCategoryVersionsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!TryGetPrintMode(Format, out var printMode))
            ModelState.AddModelError(nameof(Format), "Choose Microsoft Excel or LibreOffice Calc.");
        else if (printMode == LibrePrintMode && !OdsEnabled)
            ModelState.AddModelError(nameof(Format),
                "LibreOffice generation is temporarily unavailable. Microsoft Excel remains available.");

        await LoadCategoryVersionsAsync(ct);
        if (!string.IsNullOrWhiteSpace(CategoryVersionId)
            && !CategoryVersions.Any(version => string.Equals(version.VersionId,
                CategoryVersionId.Trim(), StringComparison.Ordinal)))
        {
            ModelState.AddModelError(nameof(CategoryVersionId),
                "Choose Active or an available Published, Submitted or Archived Category Tree version.");
        }

        if (!ModelState.IsValid)
            return Page();

        var options = await _nodeContext.App_tbOptions
            .OrderBy(option => option.Identifier)
            .FirstOrDefaultAsync(ct);
        if (options is not null && options.DefaultPrintMode != printMode)
        {
            options.DefaultPrintMode = printMode;
            await _nodeContext.SaveChangesAsync(ct);
        }

        var result = await _generationService.GenerateAsync(
            new CashStatementGenerationRequest(
                Format,
                CommandTimeout,
                IncludeActivePeriods,
                IncludeBalanceSheet,
                IncludeBankBalances,
                IncludeOrderBook,
                IncludeTaxAccruals,
                IncludeVatDetails,
                IncludeReconciliation,
                string.IsNullOrWhiteSpace(CategoryVersionId) ? null : CategoryVersionId.Trim()),
            ct);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        return File(result.Content!, result.ContentType!, result.FileName!);
    }

    private async Task LoadCategoryVersionsAsync(CancellationToken ct)
    {
        ActiveCategoryVersionId = await _nodeContext.App_tbOptions
            .AsNoTracking()
            .Select(option => option.CategoryVersionId)
            .SingleAsync(ct);

        CategoryVersions = await
            (from version in _nodeContext.Cash_tbCategoryVersions.AsNoTracking()
             join status in _nodeContext.Cash_tbCategoryVersionStatuses.AsNoTracking()
                 on version.VersionStatusCode equals status.VersionStatusCode
             where version.VersionStatusCode == (short)NodeEnum.CategoryVersionStatus.Published
                || version.VersionStatusCode == (short)NodeEnum.CategoryVersionStatus.Submitted
                || version.VersionStatusCode == (short)NodeEnum.CategoryVersionStatus.Archived
             orderby version.EffectiveFromOn descending, version.VersionId descending
             select new CashStatementCategoryVersionOption(
                 version.VersionId,
                 version.Description,
                 status.VersionStatus,
                 version.EffectiveFromOn,
                 version.EffectiveToOn,
                 version.VersionId == ActiveCategoryVersionId))
            .ToArrayAsync(ct);
    }

    private static bool TryGetPrintMode(string? format, out short printMode)
    {
        if (string.Equals(format, "libre", StringComparison.OrdinalIgnoreCase))
        {
            printMode = LibrePrintMode;
            return true;
        }

        if (string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase))
        {
            printMode = ExcelPrintMode;
            return true;
        }

        printMode = default;
        return false;
    }

}

public sealed record CashStatementCategoryVersionOption(
    string VersionId,
    string Description,
    string Status,
    DateTime? EffectiveFromOn,
    DateTime? EffectiveToOn,
    bool IsActive);
