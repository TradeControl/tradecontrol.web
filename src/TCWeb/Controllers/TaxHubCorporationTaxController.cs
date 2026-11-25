using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TradeControl.Web.AppServices.TaxHub.CorporationTax;
using TradeControl.Web.Authorization;

namespace TradeControl.Web.Controllers;

[Authorize(Roles = "Managers,Administrators")]
[Route("TaxHub/CorporationTax")]
public sealed class TaxHubCorporationTaxController(
    IServiceScopeFactory serviceScopeFactory) : Controller
{
    [HttpGet("Conversation/{reference}/outcome")]
    public async Task<IActionResult> Outcome(string reference,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider
                .GetRequiredService<ICorporationTaxTransactionEngineService>();
            var outcome = await service.ReadOutcomeAsync(User, reference, cancellationToken);
            Response.Headers.CacheControl = "no-store, max-age=0";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.XContentTypeOptions = "nosniff";
            Response.Headers["X-Corporation-Tax-Outcome-SHA256"] = outcome.Sha256;
            return File(outcome.Content, "application/xml", outcome.FileName);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException) { return BadRequest(); }
        catch (InvalidOperationException exception)
        {
            return StatusCode(409, new ProblemDetails { Title = exception.Message, Status = 409 });
        }
        catch (IOException exception)
        {
            return StatusCode(409, new ProblemDetails { Title = exception.Message, Status = 409 });
        }
    }

    [HttpGet("Preparation/{reference}/{logicalFilingIdentity}/{kind}")]
    public async Task<IActionResult> Artifact(string reference, string logicalFilingIdentity,
        string kind, bool download, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var preparationService = scope.ServiceProvider
                .GetRequiredService<ICorporationTaxPreparationService>();
            var (content, artifact) = await preparationService.ReadArtifactAsync(User,
                reference, logicalFilingIdentity, kind, cancellationToken);
            Response.Headers.CacheControl = "no-store, max-age=0";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.XContentTypeOptions = "nosniff";
            Response.Headers["X-Corporation-Tax-Artifact-SHA256"] = artifact.Sha256;
            if (download)
                return File(content.Bytes, artifact.MediaType, artifact.FileName);

            Response.Headers.XFrameOptions = "SAMEORIGIN";
            Response.Headers.ContentSecurityPolicy =
                "default-src 'none'; style-src 'unsafe-inline'; img-src data:; base-uri 'none'; form-action 'none'; frame-ancestors 'self'";
            return File(content.Bytes, artifact.MediaType);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException) { return BadRequest(); }
        catch (InvalidOperationException exception)
        {
            return StatusCode(409, new ProblemDetails { Title = exception.Message, Status = 409 });
        }
    }
}
