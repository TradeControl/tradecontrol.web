using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal sealed record CorporationTaxPreparedArtifactRecord(
    string Kind,
    string FileName,
    string MediaType,
    PreparedArtifactStatus Status,
    string Sha256,
    string ProtectedContentReference,
    long Length,
    IReadOnlyList<PreparedArtifactFinding> Findings);

internal sealed record CorporationTaxPreparedPeriodRecord(
    string LogicalFilingIdentity,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    CorporationTaxPreparedArtifactRecord ServiceRoot,
    CorporationTaxPreparedArtifactRecord Computation);

internal sealed record CorporationTaxPreparationRecord(
    string TenantReference,
    string Reference,
    string AccountsSnapshotSha256,
    string SourceSha256,
    string ReviewedInputSha256,
    string ReturnDetailsSha256,
    string DeclarantName,
    DateOnly DeclarationDate,
    string PreparedBy,
    DateTimeOffset PreparedAtUtc,
    CorporationTaxPreparedArtifactRecord Accounts,
    IReadOnlyList<CorporationTaxPreparedPeriodRecord> Periods);

internal interface ICorporationTaxPreparationStore
{
    Task<CorporationTaxPreparationRecord?> GetAsync(string tenantReference, string reference,
        CancellationToken cancellationToken = default);
    Task<CorporationTaxPreparationRecord?> GetLatestAsync(string tenantReference,
        string accountsSnapshotSha256, CancellationToken cancellationToken = default);
    Task InsertAsync(CorporationTaxPreparationRecord preparation,
        CancellationToken cancellationToken = default);
}

internal sealed class AzureCorporationTaxPreparationStore(
    IConfiguration configuration,
    IOptions<CompaniesHouseProductHostOptions> options) : ICorporationTaxPreparationStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly CompaniesHouseProductHostOptions _options = options.Value;

    public async Task<CorporationTaxPreparationRecord?> GetAsync(string tenantReference,
        string reference, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson
              FROM Tax.CorporationTaxPreparation
             WHERE TenantReference = @tenant
               AND Reference = @reference;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "reference", reference);
        return Deserialize(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<CorporationTaxPreparationRecord?> GetLatestAsync(string tenantReference,
        string accountsSnapshotSha256, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT TOP (1) PayloadJson
              FROM Tax.CorporationTaxPreparation
             WHERE TenantReference = @tenant
               AND AccountsSnapshotSha256 = @accounts
             ORDER BY PreparedAtUtc DESC, Reference DESC;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "accounts", accountsSnapshotSha256);
        return Deserialize(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task InsertAsync(CorporationTaxPreparationRecord preparation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await using var command = new SqlCommand("""
            INSERT INTO Tax.CorporationTaxPreparation
                (TenantReference, Reference, AccountsSnapshotSha256, SourceSha256,
                 ReviewedInputSha256, ReturnDetailsSha256, PreparedBy, PreparedAtUtc, PayloadJson)
            VALUES
                (@tenant, @reference, @accounts, @source, @input, @returnDetails,
                 @preparedBy, @preparedAt, @payload);
            """, connection, transaction);
        Add(command, "tenant", Guid.Parse(preparation.TenantReference));
        Add(command, "reference", preparation.Reference);
        Add(command, "accounts", preparation.AccountsSnapshotSha256);
        Add(command, "source", preparation.SourceSha256);
        Add(command, "input", preparation.ReviewedInputSha256);
        Add(command, "returnDetails", preparation.ReturnDetailsSha256);
        Add(command, "preparedBy", preparation.PreparedBy);
        Add(command, "preparedAt", preparation.PreparedAtUtc.UtcDateTime);
        Add(command, "payload", JsonSerializer.Serialize(preparation, Json));
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new InvalidOperationException("The Corporation Tax preparation was not retained.");
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<SqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var name = _options.WorkflowConnectionName;
        var connectionString = string.IsNullOrWhiteSpace(name)
            ? null
            : configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "The named Corporation Tax workflow connection is unavailable.");
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static CorporationTaxPreparationRecord? Deserialize(object? value) => value is null or DBNull
        ? null
        : JsonSerializer.Deserialize<CorporationTaxPreparationRecord>((string)value, Json)
          ?? throw new InvalidDataException("Corporation Tax preparation metadata is invalid.");

    private static void Add(SqlCommand command, string name, object value) =>
        command.Parameters.AddWithValue($"@{name}", value);
}
