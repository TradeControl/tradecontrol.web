using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TradeControl.Web.OAuthBroker;

public sealed class OAuthBrokerReadinessCheck(IOAuthBrokerStateStore store) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await store.ProbeAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("The broker state store is unavailable.", exception);
        }
    }
}
