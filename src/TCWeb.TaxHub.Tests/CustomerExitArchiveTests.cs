using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.AppServices;
using TradeControl.Web.Controllers;
using TradeControl.Web.Data;

internal static class CustomerExitArchiveTests
{
    public static async Task<int> RunAsync()
    {
        var assertions = 0;
        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        var root = Path.Combine(Path.GetTempPath(), $"tradecontrol-exit-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "content", "documents"));
            await File.WriteAllTextAsync(
                Path.Combine(root, "content", "documents", "terms.txt"),
                "tenant document");

            var data = new FakeDataSource();
            var audit = new RecordingAudit();
            using var files = new PhysicalFileProvider(root);
            var service = new CustomerExitArchiveService(
                data,
                files,
                audit,
                new FixedTimeProvider(new DateTimeOffset(2026, 10, 28, 12, 30, 0, TimeSpan.Zero)));

            var result = await service.CreateAsync("tenant-admin@example.test", CancellationToken.None);
            Assert(result.FileName == "tradecontrol-customer-exit-20261028-123000.zip",
                "The exit archive file name is not deterministic or interpretable.");
            Assert(result.TableCount == 1 && result.FileCount == 1,
                "The exit archive counts do not describe its contents.");
            Assert(audit.Message?.Contains("tenant-admin@example.test", StringComparison.Ordinal) == true,
                "The exit archive event is not attributable to its requesting administrator.");
            Assert(!string.IsNullOrWhiteSpace(result.Sha256) && result.Sha256.Length == 64,
                "The exit archive does not have a SHA-256 digest.");

            using (var zip = new ZipArchive(new MemoryStream(result.Bytes), ZipArchiveMode.Read))
            {
                Assert(zip.GetEntry("manifest.json") is not null,
                    "The exit archive has no interpretation manifest.");
                Assert(zip.GetEntry("data/Invoice.tbInvoice.json") is not null,
                    "The exit archive has no tenant business-data file.");
                Assert(zip.GetEntry("files/documents/terms.txt") is not null,
                    "The exit archive omitted a tenant-owned document.");

                var manifest = await ReadAsync(zip.GetEntry("manifest.json")!);
                Assert(manifest.Contains("OAuth grants", StringComparison.Ordinal)
                       && !manifest.Contains("super-secret", StringComparison.Ordinal),
                    "The exit manifest does not explain protected-data exclusions safely.");
                var invoices = await ReadAsync(zip.GetEntry("data/Invoice.tbInvoice.json")!);
                Assert(invoices.Contains("INV-001", StringComparison.Ordinal)
                       && !invoices.Contains("super-secret", StringComparison.Ordinal),
                    "The exit data is missing business records or contains a protected test value.");
            }

            var controller = new CustomerExitController(new StubArchiveService(result))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = CreateHttpContext("tenant-admin@example.test")
                }
            };
            var unconfirmed = await controller.Download(false, CancellationToken.None) as RedirectResult;
            Assert(unconfirmed?.Url == "/Admin/Manager/Index?node=CustomerExit",
                "The customer exit endpoint did not require the explanatory confirmation step.");

            var download = await controller.Download(true, CancellationToken.None) as FileContentResult;
            Assert(download is not null && download.ContentType == "application/zip",
                "The administrator exit endpoint did not return a ZIP download.");
            Assert(controller.Response.Headers.CacheControl == "no-store, max-age=0",
                "The customer exit response can be cached.");
            Assert(controller.Response.Headers["X-Content-SHA256"] == result.Sha256,
                "The customer exit response does not expose its evidence digest.");

            var authorisation = typeof(CustomerExitController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();
            Assert(authorisation?.Roles == "Administrators",
                "The customer exit controller is not restricted to tenant administrators.");

            var nodeOptions = new DbContextOptionsBuilder<NodeContext>()
                .UseSqlServer("Server=example.invalid;Database=customer-exit-model;Integrated Security=true;TrustServerCertificate=true")
                .Options;
            await using (var nodeContext = new NodeContext(nodeOptions))
            {
                var modelData = new EfCustomerExitDataSource(nodeContext);
                var period = modelData.Tables.Single(table =>
                    table.Schema == "Cash" && table.Name == "tbPeriod");
                Assert(!period.Columns.Contains("StartOnNavigationMonthNumber", StringComparer.OrdinalIgnoreCase)
                       && !period.Columns.Contains("StartOnNavigationYearNumber", StringComparer.OrdinalIgnoreCase),
                    "The exit archive treated convention-created EF shadow properties as physical SQL columns.");

                var ethereum = modelData.Tables.Single(table =>
                    table.Schema == "App" && table.Name == "tbEth");
                Assert(!ethereum.Columns.Contains("PrivateKey", StringComparer.OrdinalIgnoreCase),
                    "The exit archive exposed a private cryptographic key.");

                var options = modelData.Tables.Single(table =>
                    table.Schema == "App" && table.Name == "tbOptions");
                Assert(!options.Columns.Contains("HostId", StringComparer.OrdinalIgnoreCase),
                    "The exit archive exposed an infrastructure host binding.");
            }

            var anonymous = new CustomerExitController(new StubArchiveService(result))
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
            Assert(await anonymous.Download(true, CancellationToken.None) is ForbidResult,
                "The customer exit endpoint accepted an unauthenticated identity in its action.");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        return assertions;
    }

    private static DefaultHttpContext CreateHttpContext(string name)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, name), new Claim(ClaimTypes.Role, "Administrators")],
            "test"));
        return context;
    }

    private static async Task<string> ReadAsync(ZipArchiveEntry entry)
    {
        await using var stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private sealed class FakeDataSource : ICustomerExitDataSource
    {
        public IReadOnlyList<CustomerExitTable> Tables { get; } =
            [new("Invoice", "tbInvoice", ["InvoiceNumber", "Value"] )];

        public async Task WriteTableAsync(
            CustomerExitTable table,
            Stream destination,
            CancellationToken cancellationToken)
        {
            await JsonSerializer.SerializeAsync(destination, new[]
            {
                new { InvoiceNumber = "INV-001", Value = 125.50m }
            }, cancellationToken: cancellationToken);
        }
    }

    private sealed class RecordingAudit : ICustomerExitAudit
    {
        public string? Message { get; private set; }
        public Task RecordAsync(string message)
        {
            Message = message;
            return Task.CompletedTask;
        }
    }

    private sealed class StubArchiveService(CustomerExitArchive archive) : ICustomerExitArchiveService
    {
        public Task<CustomerExitArchive> CreateAsync(string requestedBy, CancellationToken cancellationToken) =>
            Task.FromResult(archive);
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
