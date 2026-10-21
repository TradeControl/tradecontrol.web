using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace TradeControl.Web.Controllers;

[Authorize]
[Route("tenant-assets")]
public sealed class TenantAssetsController(IFileProvider fileProvider) : Controller
{
    private static readonly IReadOnlyDictionary<string, string> ContentFolders =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Documents"] = "documents",
            ["Templates"] = "templates",
            ["Images"] = "images"
        };

    private static readonly IReadOnlyDictionary<string, string> MediaTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".gif"] = "image/gif",
            [".htm"] = "text/html",
            [".html"] = "text/html",
            [".jpeg"] = "image/jpeg",
            [".jpg"] = "image/jpeg",
            [".pdf"] = "application/pdf",
            [".png"] = "image/png",
            [".svg"] = "image/svg+xml",
            [".tpl"] = "text/plain"
        };

    [HttpGet("icon.svg")]
    public IActionResult Icon()
    {
        var icon = fileProvider.GetFileInfo("content/images/icon.svg");
        if (!icon.Exists)
            return Redirect("/content/images/icon.svg");

        Response.Headers.CacheControl = "no-store, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";

        return File(icon.CreateReadStream(), "image/svg+xml");
    }

    [HttpGet("download")]
    public IActionResult Download(string? contentType, string? fileName)
    {
        if (!ContentFolders.TryGetValue(contentType ?? string.Empty, out var folder)
            || !IsSafeFileName(fileName))
        {
            return NotFound();
        }

        var file = fileProvider.GetFileInfo($"content/{folder}/{fileName}");
        if (!file.Exists || file.IsDirectory)
            return NotFound();

        Response.Headers.CacheControl = "no-store, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentSecurityPolicy = "default-src 'none'; sandbox";

        var extension = Path.GetExtension(fileName!);
        var mediaType = MediaTypes.GetValueOrDefault(extension, "application/octet-stream");
        return File(file.CreateReadStream(), mediaType, fileName, enableRangeProcessing: true);
    }

    private static bool IsSafeFileName(string? fileName) =>
        !string.IsNullOrWhiteSpace(fileName)
        && fileName is not "." and not ".."
        && fileName.IndexOfAny(['/', '\\']) < 0
        && string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal);
}
