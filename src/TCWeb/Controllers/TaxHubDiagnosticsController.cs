using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace TradeControl.Web.Controllers
{
    [Route("TaxHub")]
    public sealed class TaxHubDiagnosticsController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public TaxHubDiagnosticsController(IWebHostEnvironment environment,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        [HttpGet("HmrcDiagnosticsReturn")]
        public IActionResult HmrcDiagnosticsReturn([FromQuery] string returnTo = null)
        {
            var originText = _configuration["TaxHub:Diagnostics:Origin"];
            var secretText = _configuration["TaxHub:Diagnostics:HandoffSecret"];
            if (!string.IsNullOrWhiteSpace(originText) && !string.IsNullOrWhiteSpace(secretText))
            {
                if (!Uri.TryCreate(originText, UriKind.Absolute, out var origin) || origin.Scheme != Uri.UriSchemeHttps)
                    return NotFound();
                var subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (User.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(subject))
                    return Challenge();
                var payload = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    Subject = subject,
                    Name = User.Identity.Name ?? subject,
                    ReturnTo = string.Equals(returnTo, "validate", StringComparison.Ordinal) ? "validate" : "swagger",
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds(),
                    Nonce = Guid.NewGuid().ToString("N")
                });
                var encoded = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(payload);
                byte[] secret;
                try { secret = Convert.FromBase64String(secretText); }
                catch (FormatException) { return NotFound(); }
                try
                {
                    if (secret.Length < 32) return NotFound();
                    var signature = HMACSHA256.HashData(secret, Encoding.ASCII.GetBytes(encoded));
                    var token = $"{encoded}.{Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(signature)}";
                    return Redirect(new Uri(origin,
                        $"/diagnostics/hmrc/host-sign-in?ticket={Uri.EscapeDataString(token)}").AbsoluteUri);
                }
                finally { CryptographicOperations.ZeroMemory(secret); }
            }

            if (!_environment.IsDevelopment()) return NotFound();
            return Redirect(string.Equals(returnTo, "validate", StringComparison.Ordinal)
                ? "https://localhost:44362/diagnostics/hmrc/fraud-prevention/validate"
                : "https://localhost:44362/swagger/index.html");
        }
    }
}
