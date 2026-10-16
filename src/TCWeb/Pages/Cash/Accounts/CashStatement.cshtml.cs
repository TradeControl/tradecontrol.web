using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TCExports.Generator;
using TCExports.Generator.Contracts;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.Accounts;

public class CashStatementModel : PageModel
{
    private const short LibrePrintMode = 1;
    private const short ExcelPrintMode = 2;
    private readonly IConfiguration _config;
    private readonly NodeContext _nodeContext;

    public CashStatementModel(IConfiguration config, NodeContext nodeContext)
    {
        _config = config;
        _nodeContext = nodeContext;
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

    public async Task OnGetAsync(CancellationToken ct)
    {
        var printMode = await _nodeContext.App_tbOptions
            .AsNoTracking()
            .OrderBy(option => option.Identifier)
            .Select(option => (short?)option.DefaultPrintMode)
            .FirstOrDefaultAsync(ct);

        Format = printMode == LibrePrintMode ? "libre" : "excel";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!TryGetPrintMode(Format, out var printMode))
            ModelState.AddModelError(nameof(Format), "Choose Microsoft Excel or LibreOffice Calc.");

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

        var payload = new ExportPayload
        {
            SqlConnection = _config.GetConnectionString("TCNodeContext") ?? string.Empty,
            UserName = User.Identity?.Name ?? "anonymous",
            DocumentType = "cashflow",
            Format = Format,
            Params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["commandTimeout"] = CommandTimeout.ToString(CultureInfo.InvariantCulture),
                ["includeActivePeriods"] = BooleanText(IncludeActivePeriods),
                ["includeBalanceSheet"] = BooleanText(IncludeBalanceSheet),
                ["includeBankBalances"] = BooleanText(IncludeBankBalances),
                ["includeOrderBook"] = BooleanText(IncludeOrderBook),
                ["includeTaxAccruals"] = BooleanText(IncludeTaxAccruals),
                ["includeVatDetails"] = BooleanText(IncludeVatDetails),
                ["includeReconciliation"] = BooleanText(IncludeReconciliation)
            }
        };

        var result = await ExportRunner.ExportDataAsync(payload);
        if (!string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = BuildErrorMessage(result);
            return Page();
        }

        var bytes = Convert.FromBase64String(result.FileContent!);
        var isLibre = string.Equals(Format, "libre", StringComparison.OrdinalIgnoreCase);
        var fallbackName = isLibre ? "cash-statement.ods" : "cash-statement.xlsx";
        var contentType = isLibre
            ? "application/vnd.oasis.opendocument.spreadsheet"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = string.IsNullOrWhiteSpace(result.FileName) ? fallbackName : result.FileName;
        return File(bytes, contentType, fileName);
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

    private static string BooleanText(bool value) => value ? "true" : "false";

    private static string BuildErrorMessage(ExportResult result)
    {
        var details = result.Details?
            .SelectMany(item => item.Value)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .ToArray() ?? Array.Empty<string>();
        var message = string.IsNullOrWhiteSpace(result.Message)
            ? "The Cash Statement could not be generated."
            : result.Message;
        return details.Length == 0 ? message : $"{message} {string.Join(" ", details)}";
    }
}
