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
    public string DefaultBusinessTimeZoneId { get; set; } = "Europe/London";
    public List<TenantRegistration> Tenants { get; set; } = [];
}

public sealed class TenantRegistration
{
    public string? TenantReference { get; set; }
    public string? Host { get; set; }
    public string? ConnectionStringName { get; set; }
    public string BusinessTimeZoneId { get; set; } = "Europe/London";
    public bool Enabled { get; set; } = true;
    public bool VatSubmissionEnabled { get; set; }
}

public sealed class TenantDescriptor(
    Guid tenantId,
    string canonicalHost,
    string connectionStringName,
    string connectionString,
    string businessTimeZoneId,
    TimeZoneInfo businessTimeZone,
    bool vatSubmissionEnabled = true)
{
    public TenantDescriptor(Guid tenantId, string canonicalHost, string connectionStringName,
        string connectionString)
        : this(tenantId, canonicalHost, connectionStringName, connectionString,
            "Europe/London", TimeZoneInfo.FindSystemTimeZoneById("Europe/London")) { }

    public Guid TenantId { get; } = tenantId;
    public string CanonicalHost { get; } = canonicalHost;
    public string ConnectionStringName { get; } = connectionStringName;
    public string ConnectionString { get; } = connectionString;
    public string BusinessTimeZoneId { get; } = businessTimeZoneId;
    public TimeZoneInfo BusinessTimeZone { get; } = businessTimeZone;
    public bool VatSubmissionEnabled { get; } = vatSubmissionEnabled;

    public override string ToString() => TenantId.ToString("D");
}
