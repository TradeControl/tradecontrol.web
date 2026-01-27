using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using TCExports.Generator;
using TCExports.Generator.Contracts;

namespace TradeControl.Web.Pages.Cash.Accounts;

public class CashStatementModel : PageModel
{
    private readonly IConfiguration _config;

    public CashStatementModel(IConfiguration config) => _config = config;

    [BindProperty] public int CommandTimeout { get; set; } = 30;
    [BindProperty] public bool IncludeActivePeriods { get; set; } = false;
    [BindProperty] public bool IncludeBalanceSheet { get; set; } = true;
    [BindProperty] public bool IncludeBankBalances { get; set; } = true;
    [BindProperty] public bool IncludeOrderBook { get; set; } = false;
    [BindProperty] public bool IncludeTaxAccruals { get; set; } = true;
    [BindProperty] public bool IncludeVatDetails { get; set; } = true;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var adoConn = _config.GetConnectionString("TCNodeContext") ?? string.Empty;

        var payload = new ExportPayload {
            SqlConnection = adoConn,
            UserName = User.Identity?.Name ?? "anonymous",
            DocumentType = "cashflow",
            Format = "excel",
            Params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                ["commandTimeout"] = CommandTimeout.ToString(CultureInfo.InvariantCulture),
                ["includeActivePeriods"] = IncludeActivePeriods ? "false" : "false", // sample shows false by default
                ["includeBalanceSheet"] = IncludeBalanceSheet ? "true" : "false",
                ["includeBankBalances"] = IncludeBankBalances ? "true" : "false",
                ["includeOrderBook"] = IncludeOrderBook ? "false" : "false",
                ["includeTaxAccruals"] = IncludeTaxAccruals ? "true" : "false",
                ["includeVatDetails"] = IncludeVatDetails ? "true" : "false",
            }
        };

        var result = await ExportRunner.ExportDataAsync(payload);

        if (!string.Equals(result.Status, "success", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { result.Status, result.Code, result.Message, result.Details });

        var bytes = Convert.FromBase64String(result.FileContent!);
        var name = string.IsNullOrWhiteSpace(result.FileName) ? "cash-statement.xlsx" : result.FileName;
        const string xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return File(bytes, xlsx, name);
    }
}
