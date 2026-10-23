using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;
using TradeControl.Web.OAuthBroker;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddHealthChecks()
    .AddCheck<OAuthBrokerReadinessCheck>("oauth-broker-state", tags: ["ready"]);

var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/problem+json";
    await Results.Problem("The OAuth broker could not complete the request.", statusCode: 500)
        .ExecuteAsync(context);
}));
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
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapOAuthBroker();
app.Run();

public partial class Program;
