using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.Tenancy;

/// <summary>
/// Creates short-lived node contexts for concurrent server-side UI operations. A scoped
/// <see cref="NodeContext"/> must not be shared by concurrently rendering Blazor components.
/// </summary>
public sealed class TenantNodeContextFactory(ITenantContext tenantContext) : IDbContextFactory<NodeContext>
{
    public NodeContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NodeContext>()
            .UseSqlServer(tenantContext.RequiredTenant.ConnectionString)
            .Options;
        return new NodeContext(options);
    }

    public Task<NodeContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateDbContext());
    }
}
