using System;
using System.Collections.Generic;

namespace TradeControl.Web.Tenancy;

public sealed class TenantHostingOptions
{
    public const string SectionName = "Tenancy";
    public bool SharedHostEnabled { get; set; }
    public bool SeedOnStartup { get; set; } = true;
    public string? DefaultTenantReference { get; set; }
    public string DefaultConnectionStringName { get; set; } = "TCNodeContext";
    public List<TenantRegistration> Tenants { get; set; } = [];
}

public sealed class TenantRegistration
{
    public string? TenantReference { get; set; }
    public string? Host { get; set; }
    public string? ConnectionStringName { get; set; }
    public bool Enabled { get; set; } = true;
}

public sealed class TenantDescriptor(
    Guid tenantId,
    string canonicalHost,
    string connectionStringName,
    string connectionString)
{
    public Guid TenantId { get; } = tenantId;
    public string CanonicalHost { get; } = canonicalHost;
    public string ConnectionStringName { get; } = connectionStringName;
    public string ConnectionString { get; } = connectionString;

    public override string ToString() => TenantId.ToString("D");
}
