using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;
using TradeControl.Web.AppServices.TaxHub.Vat;

namespace TradeControl.Web.Controllers;

[Authorize]
[Route("TaxHub/Hmrc")]
public sealed class TaxHubHmrcController(
    IVatHmrcConnectionService connection,
    IVatFilingAuthorisationPolicy policy,
    IVatFraudContextCapture fraudCapture) : Controller
{
    [HttpGet("Status")]
    public async Task<ActionResult<VatConnectionStatus>> Status(CancellationToken cancellationToken) =>
        Ok(await connection.GetStatusAsync(User, cancellationToken));

    [HttpGet("Connect")]
    public Task<IActionResult> Connect(CancellationToken cancellationToken) =>
        BeginAsync(cancellationToken);

    [HttpGet("Reauthorise")]
    public Task<IActionResult> Reauthorise(CancellationToken cancellationToken) =>
        BeginAsync(cancellationToken);

    [HttpGet("/TaxHub/HmrcCallback")]
    public async Task<IActionResult> Callback(string? state, string? code, string? error,
        CancellationToken cancellationToken)
    {
        if (!policy.CanManageHmrcConnection(User)) return Forbid();
        if (string.IsNullOrWhiteSpace(state))
            return BadRequest(new ProblemDetails { Title = "The HMRC callback is invalid.", Status = 400 });
        try
        {
            var status = await connection.CompleteCallbackAsync(User, state, code, error, cancellationToken);
            return status.State == VatConnectionState.Connected
                ? Redirect("/Tax/Hub")
                : Redirect("/Tax/Hub?hmrc=reauthorisation-required");
        }
        catch (OAuthCallbackValidationException)
        {
            return BadRequest(new ProblemDetails { Title = "The HMRC callback is invalid or expired.", Status = 400 });
        }
    }

    [HttpPost("Disconnect")]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken)
    {
        if (!policy.CanManageHmrcConnection(User)) return Forbid();
        await connection.DisconnectAsync(User, cancellationToken);
        return Ok(new VatConnectionStatus(VatConnectionState.Disconnected));
    }

    [HttpPost("ClientFacts")]
    public async Task<IActionResult> ClientFacts([FromBody] VatBrowserFacts browser,
        CancellationToken cancellationToken)
    {
        if (!policy.CanManageHmrcConnection(User)) return Forbid();
        var remote = HttpContext.Connection.RemoteIpAddress;
        var port = HttpContext.Connection.RemotePort;
        if (remote is null || port is < 1 or > 65535) return BadRequest();
        try
        {
            await fraudCapture.CaptureAsync(User, browser, remote, port,
                Request.Headers["X-Forwarded-For"].FirstOrDefault(), cancellationToken);
            return NoContent();
        }
        catch (ArgumentException) { return BadRequest(); }
        catch (UnauthorizedAccessException) { return BadRequest(); }
        catch (InvalidOperationException)
        {
            return StatusCode(503, new ProblemDetails
            {
                Title = "HMRC client-fact capture is unavailable for this deployment.", Status = 503
            });
        }
    }

    private async Task<IActionResult> BeginAsync(CancellationToken cancellationToken)
    {
        if (!policy.CanManageHmrcConnection(User)) return Forbid();
        try { return Redirect((await connection.BeginConnectAsync(User, cancellationToken)).AbsoluteUri); }
        catch (InvalidOperationException)
        {
            return StatusCode(503, new ProblemDetails
            {
                Title = "The HMRC connection service is unavailable.", Status = 503
            });
        }
    }
}
