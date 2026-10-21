using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TradeControl.Web.Tenancy;

internal static class TenantIsolationTests
{
    public static async Task<int> RunAsync()
    {
        var assertions = 0;
        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:TenantA"] = "Server=a;Database=a",
            ["ConnectionStrings:TenantB"] = "Server=b;Database=b"
        }).Build();
        var options = Options.Create(new TenantHostingOptions
        {
            SharedHostEnabled = true,
            Tenants =
            [
                new() { TenantReference = tenantA.ToString(), Host = "Alpha.Example.Test.", ConnectionStringName = "TenantA" },
                new() { TenantReference = tenantB.ToString(), Host = "beta.example.test", ConnectionStringName = "TenantB" }
            ]
        });
        var directory = new ConfiguredTenantDirectory(options, configuration);

        Assert(directory.ResolveHost("ALPHA.example.test")?.TenantId == tenantA,
            "Canonical host resolution did not select tenant A.");
        Assert(directory.ResolveHost("beta.example.test")?.ConnectionStringName == "TenantB",
            "Canonical host resolution did not select tenant B.");
        Assert(!directory.ResolveHost("beta.example.test")!.ToString().Contains("Server=", StringComparison.Ordinal),
            "A tenant descriptor rendered its protected connection locator.");
        Assert(directory.ResolveHost("unknown.example.test") is null,
            "An unknown host resolved to a tenant.");
        Assert(directory.ResolveHost("beta.example.test:443") is null,
            "A host containing a caller-supplied port was accepted by the directory.");

        var scope = new TenantContext();
        scope.Bind(directory.ResolveHost("alpha.example.test")!);
        Assert(scope.RequiredTenant.TenantId == tenantA, "The scoped tenant was not bound.");
        var rebindRejected = false;
        try
        {
            scope.Bind(directory.ResolveHost("beta.example.test")!);
        }
        catch (InvalidOperationException)
        {
            rebindRejected = true;
        }
        Assert(rebindRejected && scope.RequiredTenant.TenantId == tenantA,
            "The tenant changed within an existing scope.");

        var ambient = new TenantAmbientContext();
        using var parentScope = new TenantContext(ambient);
        parentScope.Bind(directory.ResolveHost("alpha.example.test")!);
        using (var childScope = new TenantContext(ambient))
        {
            Assert(childScope.RequiredTenant.TenantId == tenantA,
                "A nested service scope did not inherit the resolved tenant.");
        }
        Assert(parentScope.RequiredTenant.TenantId == tenantA,
            "Disposing a nested service scope cleared its parent's tenant binding.");

        var matchingPrincipal = Principal(tenantA);
        Assert(TenantIdentity.Matches(matchingPrincipal, scope.RequiredTenant),
            "A correctly tenant-bound principal was rejected.");
        Assert(!TenantIdentity.Matches(Principal(tenantB), scope.RequiredTenant),
            "A copied foreign-tenant principal was accepted.");
        Assert(!TenantIdentity.Matches(new ClaimsPrincipal(new ClaimsIdentity("test")), scope.RequiredTenant),
            "A legacy principal without a tenant binding was accepted.");

        var reachedApplication = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            reachedApplication = true;
            return Task.CompletedTask;
        }, NullLogger<TenantResolutionMiddleware>.Instance);
        var acceptedHttp = new DefaultHttpContext();
        acceptedHttp.Request.Host = new HostString("beta.example.test");
        var acceptedScope = new TenantContext();
        await middleware.InvokeAsync(acceptedHttp, directory, acceptedScope);
        Assert(reachedApplication && acceptedScope.RequiredTenant.TenantId == tenantB,
            "Resolved tenant middleware did not bind before entering the application.");

        reachedApplication = false;
        var rejectedHttp = new DefaultHttpContext();
        rejectedHttp.Request.Host = new HostString("unknown.example.test");
        await middleware.InvokeAsync(rejectedHttp, directory, new TenantContext());
        Assert(!reachedApplication && rejectedHttp.Response.StatusCode == StatusCodes.Status404NotFound,
            "Unknown-host middleware did not fail closed.");

        reachedApplication = false;
        var livenessHttp = new DefaultHttpContext();
        livenessHttp.Request.Host = new HostString("platform-probe.example.test");
        livenessHttp.Request.Path = "/health/live";
        await middleware.InvokeAsync(livenessHttp, directory, new TenantContext());
        Assert(!reachedApplication && livenessHttp.Response.StatusCode == StatusCodes.Status404NotFound,
            "Tenant middleware allowed a platform liveness request to bypass host isolation.");

        reachedApplication = false;
        var readinessHttp = new DefaultHttpContext();
        readinessHttp.Request.Host = new HostString("platform-probe.example.test");
        readinessHttp.Request.Path = "/health/ready";
        await middleware.InvokeAsync(readinessHttp, directory, new TenantContext());
        Assert(!reachedApplication && readinessHttp.Response.StatusCode == StatusCodes.Status404NotFound,
            "The dependency-readiness probe bypassed tenant resolution.");

        return assertions;
    }

    private static ClaimsPrincipal Principal(Guid tenantId) => new(new ClaimsIdentity(
        [new Claim(TenantIdentity.TenantClaimType, tenantId.ToString("D"))], "test"));
}
