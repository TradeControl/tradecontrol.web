using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace TradeControl.Web.Tenancy;

public sealed record CatalogueTenantRecord(
    Guid TenantReference,
    string HostName,
    string SqlServerFqdn,
    string DatabaseName,
    Guid DatabaseInstanceId,
    string BusinessTimeZoneId,
    bool VatSubmissionEnabled,
    string ActualSchemaVersion);

public interface ICatalogueTenantSource
{
    IReadOnlyList<CatalogueTenantRecord> LoadActive();
}

public sealed class SqlCatalogueTenantSource : ICatalogueTenantSource
{
    private readonly TenantHostingOptions _options;
    private readonly string _connectionString;

    public SqlCatalogueTenantSource(IOptions<TenantHostingOptions> options, IConfiguration configuration)
    {
        _options = options.Value;
        _connectionString = configuration.GetConnectionString(_options.CatalogueConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Catalogue connection '{_options.CatalogueConnectionStringName}' is not configured.");
    }

    public IReadOnlyList<CatalogueTenantRecord> LoadActive()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        ValidateCatalogueIdentity(connection);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TenantReference, HostName, SqlServerFqdn, DatabaseName,
                   DatabaseInstanceId, BusinessTimeZoneId, VatSubmissionEnabled,
                   ActualSchemaVersion
            FROM Catalogue.ActiveTenant
            ORDER BY HostName;
            """;

        var records = new List<CatalogueTenantRecord>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(new CatalogueTenantRecord(
                reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                reader.GetGuid(4), reader.GetString(5), reader.GetBoolean(6), reader.GetString(7)));
        }

        return records;
    }

    private void ValidateCatalogueIdentity(SqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DatabasePurpose, EnvironmentName, SchemaVersion
            FROM Catalogue.RuntimeIdentity;
            """;
        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        if (!reader.Read())
            throw new InvalidOperationException("The tenant catalogue has no database identity.");

        var purpose = reader.GetString(0);
        var environment = reader.GetString(1);
        var schemaVersion = reader.GetInt32(2);
        if (!string.Equals(purpose, "TenantCatalogue", StringComparison.Ordinal)
            || !string.Equals(environment, Required(_options.CatalogueEnvironmentName),
                StringComparison.Ordinal)
            || schemaVersion != _options.CatalogueSchemaVersion)
        {
            throw new InvalidOperationException(
                "The tenant catalogue identity or schema does not match the hosted environment.");
        }
    }

    private static string Required(string? value) => string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException("A catalogue environment name is required.")
        : value.Trim();
}

public sealed class CatalogueTenantDirectory : ITenantDirectory
{
    private readonly IReadOnlyDictionary<string, TenantDescriptor> _byHost;
    private readonly IReadOnlyList<TenantDescriptor> _tenants;

    public CatalogueTenantDirectory(ICatalogueTenantSource source,
        IOptions<TenantHostingOptions> options, IConfiguration configuration)
    {
        var settings = options.Value;
        if (!settings.SharedHostEnabled)
            throw new InvalidOperationException("Catalogue tenant resolution requires shared-host mode.");

        var template = configuration.GetConnectionString(settings.TenantConnectionStringTemplateName)
            ?? throw new InvalidOperationException(
                $"Tenant connection template '{settings.TenantConnectionStringTemplateName}' is not configured.");
        var supportedVersions = settings.SupportedTenantSchemaVersions
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToHashSet(StringComparer.Ordinal);
        if (supportedVersions.Count == 0)
            throw new InvalidOperationException("Hosted mode requires an explicit supported tenant-schema set.");

        var descriptors = source.LoadActive().Select(record =>
        {
            if (!supportedVersions.Contains(record.ActualSchemaVersion))
                throw new InvalidOperationException(
                    $"An active tenant has unsupported schema version '{record.ActualSchemaVersion}'.");

            var canonicalHost = CanonicalHost(record.HostName);
            var (timeZoneId, timeZone) = RequiredBusinessTimeZone(record.BusinessTimeZoneId);
            var connectionString = BuildTenantConnectionString(template,
                record.SqlServerFqdn, record.DatabaseName);
            return new TenantDescriptor(record.TenantReference, canonicalHost,
                $"catalogue:{record.DatabaseName}", connectionString,
                timeZoneId, timeZone, record.VatSubmissionEnabled);
        }).ToArray();

        if (descriptors.Length == 0)
            throw new InvalidOperationException("The hosted catalogue contains no active tenants.");

        _byHost = descriptors.ToDictionary(item => item.CanonicalHost,
            StringComparer.OrdinalIgnoreCase);
        _tenants = Array.AsReadOnly(descriptors);
    }

    public TenantDescriptor? ResolveHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return null;
        try { return _byHost.GetValueOrDefault(CanonicalHost(host)); }
        catch (InvalidOperationException) { return null; }
    }

    public IReadOnlyList<TenantDescriptor> ListEnabled() => _tenants;

    private static string BuildTenantConnectionString(string template, string server, string database)
    {
        var builder = new SqlConnectionStringBuilder(template)
        {
            DataSource = Required(server, "The catalogue supplied an empty SQL server."),
            InitialCatalog = Required(database, "The catalogue supplied an empty database name."),
            Encrypt = true,
            TrustServerCertificate = false
        };
        return builder.ConnectionString;
    }

    private static string CanonicalHost(string? host)
    {
        var value = Required(host, "The catalogue supplied an empty tenant host.").TrimEnd('.');
        if (value.Contains(':') || value.Contains('/') || value.Length > 253)
            throw new InvalidOperationException("A catalogue tenant host is not a canonical DNS name.");
        try { return new IdnMapping().GetAscii(value).ToLowerInvariant(); }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException("A catalogue tenant host is invalid.", exception);
        }
    }

    private static (string Id, TimeZoneInfo Zone) RequiredBusinessTimeZone(string? value)
    {
        var id = Required(value, "Each active catalogue tenant requires an IANA business time zone.");
        if (!TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out _))
            throw new InvalidOperationException(
                $"Business time zone '{id}' is not a supported IANA identifier.");
        try { return (id, TimeZoneInfo.FindSystemTimeZoneById(id)); }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new InvalidOperationException($"Business time zone '{id}' is unavailable.", exception);
        }
    }

    private static string Required(string? value, string message) => string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException(message)
        : value.Trim();
}
