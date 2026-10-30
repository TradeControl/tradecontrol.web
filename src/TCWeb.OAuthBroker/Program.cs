using System.Net;
using System.Threading.RateLimiting;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;
using TradeControl.Web.OAuthBroker;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Azure App Service is the broker's only ingress. Use its client/protocol
    // forwarding for rate-limit partitioning, but never accept a forwarded host.
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddSingleton<IValidateOptions<OAuthBrokerOptions>, OAuthBrokerOptionsValidator>();
builder.Services.AddOptions<OAuthBrokerOptions>()
    .Bind(builder.Configuration.GetSection(OAuthBrokerOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(provider =>
{
    var options = provider.GetRequiredService<IOptions<OAuthBrokerOptions>>().Value;
    var encodedKey = Environment.GetEnvironmentVariable(options.EnvelopeKeyEnvironmentVariable)
        ?? throw new InvalidOperationException("The OAuth broker envelope key is unavailable.");
    return OAuthBrokerTicketProtector.FromBase64(encodedKey);
});
builder.Services.AddSingleton(provider =>
{
    var options = provider.GetRequiredService<IOptions<OAuthBrokerOptions>>().Value;
    return new BlobServiceClient(options.BlobServiceUri!, new DefaultAzureCredential())
        .GetBlobContainerClient(options.StateContainerName);
});
builder.Services.AddSingleton<IOAuthBrokerStateStore, AzureOAuthBrokerStateStore>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(OAuthBrokerEndpoints.RateLimitPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddHealthChecks()
    .AddCheck<OAuthBrokerReadinessCheck>("oauth-broker-state", tags: ["ready"]);

var app = builder.Build();
app.UseForwardedHeaders();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/problem+json";
    await Results.Problem("The OAuth broker could not complete the request.", statusCode: 500)
        .ExecuteAsync(context);
}));
app.Use(async (context, next) =>
{
    var options = context.RequestServices.GetRequiredService<IOptions<OAuthBrokerOptions>>().Value;
    var requestHost = context.Request.Host.Host;
    var platformHost = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
    var developmentLoopback = app.Environment.IsDevelopment()
        && (requestHost.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || (IPAddress.TryParse(requestHost, out var address) && IPAddress.IsLoopback(address)));
    if (!requestHost.Equals(options.PublicOrigin!.IdnHost, StringComparison.OrdinalIgnoreCase)
        && !requestHost.Equals(platformHost, StringComparison.OrdinalIgnoreCase)
        && !developmentLoopback)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers.CacheControl = "no-store";
    await next();
});
app.UseRateLimiter();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapOAuthBroker();
app.Run();

public partial class Program;
