using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.Controllers;

internal static class TenantAssetDownloadTests
{
    public static int Run()
    {
        var assertions = 0;
        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        var root = Path.Combine(Path.GetTempPath(), $"tradecontrol-download-{Guid.NewGuid():N}");
        var images = Path.Combine(root, "content", "images");

        try
        {
            Directory.CreateDirectory(images);
            File.WriteAllBytes(Path.Combine(images, "company_logo.png"), [0x89, 0x50, 0x4e, 0x47]);

            using var provider = new PhysicalFileProvider(root);
            var controller = CreateController(provider);
            var result = controller.Download("Images", "company_logo.png") as FileStreamResult;

            Assert(result is not null, "A tenant image did not produce a file download.");
            Assert(result!.ContentType == "image/png", "The tenant image download has the wrong media type.");
            Assert(result.FileDownloadName == "company_logo.png", "The tenant image download lost its file name.");
            Assert(result.EnableRangeProcessing, "The tenant file download does not support range requests.");
            result.FileStream.Dispose();

            Assert(CreateController(provider).Download("Unknown", "company_logo.png") is NotFoundResult,
                "An unknown tenant content type was accepted.");
            Assert(CreateController(provider).Download("Images", "../company_logo.png") is NotFoundResult,
                "A relative traversal file name was accepted.");
            Assert(CreateController(provider).Download("Images", "..\\company_logo.png") is NotFoundResult,
                "A Windows traversal file name was accepted.");
            Assert(CreateController(provider).Download("Images", "missing.png") is NotFoundResult,
                "A missing tenant file did not return not found.");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        return assertions;
    }

    private static TenantAssetsController CreateController(IFileProvider provider) =>
        new(provider)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
}
