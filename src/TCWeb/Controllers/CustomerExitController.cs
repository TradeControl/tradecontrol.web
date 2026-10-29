using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.AppServices;

namespace TradeControl.Web.Controllers;

[Authorize(Roles = "Administrators")]
[Route("Admin/CustomerExit")]
public sealed class CustomerExitController(ICustomerExitArchiveService archiveService) : Controller
{
    [HttpGet("Download")]
    public async Task<IActionResult> Download(
        [FromQuery] bool confirmed,
        CancellationToken cancellationToken)
    {
        var requestedBy = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(requestedBy))
            return Forbid();

        if (!confirmed)
            return Redirect("/Admin/Manager/Index?node=CustomerExit");

        var archive = await archiveService.CreateAsync(requestedBy, cancellationToken);
        Response.Headers.CacheControl = "no-store, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers["X-Customer-Exit-Reference"] = archive.ExportId;
        Response.Headers["X-Content-SHA256"] = archive.Sha256;

        return File(archive.Bytes, "application/zip", archive.FileName);
    }
}
