using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace TradeControl.Web.Controllers
{
    [AllowAnonymous]
    [Route("TaxHub")]
    public sealed class TaxHubDiagnosticsController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public TaxHubDiagnosticsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet("HmrcDiagnosticsReturn")]
        public IActionResult HmrcDiagnosticsReturn([FromQuery] string returnTo = null)
        {
            if (!_environment.IsDevelopment()) return NotFound();
            return Redirect(string.Equals(returnTo, "validate", StringComparison.Ordinal)
                ? "https://localhost:44362/diagnostics/hmrc/fraud-prevention/validate"
                : "https://localhost:44362/swagger/index.html");
        }
    }
}
