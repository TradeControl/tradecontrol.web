using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TradeControl.Web.Tenancy;

public sealed class TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext, ITenantDirectory directory, ITenantContext tenantContext)
    {
        var tenant = directory.ResolveHost(httpContext.Request.Host.Host);
        if (tenant is null)
        {
            logger.LogWarning("Request rejected because its host did not resolve to an enabled tenant.");
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        tenantContext.Bind(tenant);
        await next(httpContext);
    }
}
