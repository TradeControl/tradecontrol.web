using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

namespace TradeControl.Web.Controllers;

[Authorize]
[Route("TaxHub/CompaniesHouse")]
public sealed class TaxHubCompaniesHouseController(
    ICompaniesHousePreparationReviewService preparationService,
    ICompaniesHouseDraftPdfRenderer pdfRenderer) : Controller
{
    [HttpGet("Preparation/{reference}/Document")]
    public async Task<IActionResult> Document(string reference, CancellationToken cancellationToken)
    {
        try
        {
            var document = await preparationService.ReadDocumentAsync(reference, cancellationToken);
            Response.Headers.CacheControl = "no-store, max-age=0";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.XFrameOptions = "SAMEORIGIN";
            Response.Headers.ContentSecurityPolicy =
                "default-src 'none'; style-src 'unsafe-inline'; img-src data:; base-uri 'none'; form-action 'none'; frame-ancestors 'self'";
            return File(document.Bytes, document.MediaType);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException) { return BadRequest(); }
        catch (InvalidOperationException exception)
        {
            return StatusCode(409, new ProblemDetails { Title = exception.Message, Status = 409 });
        }
    }

    [HttpGet("Preparation/{reference}/Download")]
    public async Task<IActionResult> Download(string reference, CancellationToken cancellationToken)
    {
        try
        {
            var document = await preparationService.ReadDocumentAsync(reference, cancellationToken);
            Response.Headers.CacheControl = "no-store, max-age=0";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.XContentTypeOptions = "nosniff";
            return File(document.Bytes, "application/xhtml+xml", "companies-house-accounts-draft.xhtml");
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException) { return BadRequest(); }
        catch (InvalidOperationException exception)
        {
            return StatusCode(409, new ProblemDetails { Title = exception.Message, Status = 409 });
        }
    }

    [HttpGet("Preparation/{reference}/Draft.pdf")]
    public async Task<IActionResult> DraftPdf(string reference, CancellationToken cancellationToken)
    {
        try
        {
            var document = await preparationService.ReadDocumentAsync(reference, cancellationToken);
            var pdf = pdfRenderer.Render(document.Bytes, document.Sha256);
            Response.Headers.CacheControl = "no-store, max-age=0";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.XContentTypeOptions = "nosniff";
            Response.Headers["X-Source-Document-SHA256"] = document.Sha256;
            return File(pdf, "application/pdf", "companies-house-accounts-draft.pdf");
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
