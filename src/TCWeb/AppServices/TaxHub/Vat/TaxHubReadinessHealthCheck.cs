using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TradeControl.Web.Data;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

/// <summary>
/// Verifies only local dependencies required to serve Tax Hub. HMRC availability is deliberately
/// excluded so an authority outage does not cause the App Service to recycle a healthy application.
/// </summary>
public sealed class TaxHubReadinessHealthCheck(
    IServiceScopeFactory scopes,
    IOptions<VatProductHostOptions> hostOptions,
    IOptions<CompaniesHouseProductHostOptions> companiesHouseHostOptions) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var node = scope.ServiceProvider.GetRequiredService<NodeContext>();
            if (!await node.Database.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Unhealthy("The Trade Control node database is unavailable.");

            var options = hostOptions.Value;
            if (options.Enabled && options.PersistenceMode == VatPersistenceMode.DevelopmentFiles
                && (string.IsNullOrWhiteSpace(options.DevelopmentStoreRoot)
                    || !Path.IsPathFullyQualified(options.DevelopmentStoreRoot)))
                return HealthCheckResult.Unhealthy("The VAT evidence store is not configured.");

            var companiesHouse = companiesHouseHostOptions.Value;
            if (companiesHouse.Enabled
                && companiesHouse.PersistenceMode == CompaniesHousePersistenceMode.DevelopmentFiles
                && (string.IsNullOrWhiteSpace(companiesHouse.DevelopmentStoreRoot)
                    || !Path.IsPathFullyQualified(companiesHouse.DevelopmentStoreRoot)))
                return HealthCheckResult.Unhealthy("The Companies House evidence store is not configured.");

            return HealthCheckResult.Healthy();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("A required local dependency is unavailable.", exception);
        }
    }
}
