using System;
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

    public string? ErrorMessage { get; private set; }
    public bool OdsEnabled => _generationOptions.OdsEnabled;

    public async Task OnGetAsync(CancellationToken ct)
    {
        var printMode = await _nodeContext.App_tbOptions
            .AsNoTracking()
            .OrderBy(option => option.Identifier)
            .Select(option => (short?)option.DefaultPrintMode)
            .FirstOrDefaultAsync(ct);

        Format = printMode == LibrePrintMode && OdsEnabled ? "libre" : "excel";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!TryGetPrintMode(Format, out var printMode))
            ModelState.AddModelError(nameof(Format), "Choose Microsoft Excel or LibreOffice Calc.");
        else if (printMode == LibrePrintMode && !OdsEnabled)
            ModelState.AddModelError(nameof(Format),
                "LibreOffice generation is temporarily unavailable. Microsoft Excel remains available.");

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
                IncludeReconciliation),
            ct);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        return File(result.Content!, result.ContentType!, result.FileName!);
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
