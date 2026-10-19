using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace TradeControl.Web.Tenancy;

public interface ITenantDirectory
{
    TenantDescriptor? ResolveHost(string host);
    IReadOnlyList<TenantDescriptor> ListEnabled();
}

public sealed class ConfiguredTenantDirectory : ITenantDirectory
{
    private readonly IReadOnlyDictionary<string, TenantDescriptor> _byHost;
    private readonly IReadOnlyList<TenantDescriptor> _tenants;
    private readonly TenantDescriptor? _singleTenant;

    public ConfiguredTenantDirectory(IOptions<TenantHostingOptions> options, IConfiguration configuration)
    {
        var settings = options.Value;
        var registrations = settings.Tenants.Where(item => item.Enabled).ToArray();
        var tenants = new List<TenantDescriptor>(registrations.Length);
        foreach (var registration in registrations)
        {
            if (!Guid.TryParse(registration.TenantReference, out var tenantId))
                throw new InvalidOperationException("Each enabled tenant requires an opaque GUID reference.");
            var host = CanonicalHost(registration.Host);
            var connectionName = Required(registration.ConnectionStringName,
                "Each enabled tenant requires a connection-string name.");
            var connectionString = configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Tenant connection '{connectionName}' is not configured.");
            tenants.Add(new TenantDescriptor(tenantId, host, connectionName, connectionString));
        }

        _byHost = tenants.ToDictionary(item => item.CanonicalHost, StringComparer.OrdinalIgnoreCase);
        _tenants = tenants.AsReadOnly();
        if (settings.SharedHostEnabled)
        {
            if (tenants.Count == 0)
                throw new InvalidOperationException("Shared-host mode requires at least one enabled tenant.");
            return;
        }

        var defaultTenant = settings.DefaultTenantReference;
        if (string.IsNullOrWhiteSpace(defaultTenant))
            defaultTenant = configuration["TaxHub:VatProduct:TenantReference"];
        if (!Guid.TryParse(defaultTenant, out var defaultTenantId))
            throw new InvalidOperationException("Single-tenant mode requires an opaque default tenant GUID.");
        var defaultConnection = configuration.GetConnectionString(settings.DefaultConnectionStringName);
        if (string.IsNullOrWhiteSpace(defaultConnection))
            throw new InvalidOperationException("The default tenant connection is not configured.");
        _singleTenant = new TenantDescriptor(defaultTenantId, "single-tenant",
            settings.DefaultConnectionStringName, defaultConnection);
        _tenants = [_singleTenant];
    }

    public TenantDescriptor? ResolveHost(string host)
    {
        if (_singleTenant is not null)
            return _singleTenant;
        if (string.IsNullOrWhiteSpace(host))
            return null;
        try { return _byHost.GetValueOrDefault(CanonicalHost(host)); }
        catch (InvalidOperationException) { return null; }
    }

    public IReadOnlyList<TenantDescriptor> ListEnabled() => _tenants;

    private static string CanonicalHost(string? host)
    {
        var value = Required(host, "Each enabled tenant requires a canonical host.").TrimEnd('.');
        if (value.Contains(':') || value.Contains('/') || value.Length > 253)
            throw new InvalidOperationException("A tenant host must be a DNS name without a port or path.");
        try { return new IdnMapping().GetAscii(value).ToLowerInvariant(); }
        catch (ArgumentException exception) { throw new InvalidOperationException("A tenant host is invalid.", exception); }
    }

    private static string Required(string? value, string message) => string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException(message)
        : value.Trim();
}
