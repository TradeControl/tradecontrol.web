using System;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal sealed record CorporationTaxApprovalRecord(
    string TenantReference,
    string Reference,
    string PreparationReference,
    string AccountsSnapshotSha256,
    string SourceSha256,
    string ReviewedInputSha256,
    string ReturnDetailsSha256,
    string DocumentManifestSha256,
    string DeclarationVersion,
    string DeclarationSha256,
    bool DeclarationConfirmed,
    string ApprovedBy,
    DateTimeOffset ApprovedAtUtc,
    string ApprovalSha256);

internal interface ICorporationTaxApprovalStore
{
    Task<CorporationTaxApprovalRecord?> GetAsync(string tenantReference,
        string approvalReference, CancellationToken cancellationToken = default);

    Task<CorporationTaxApprovalRecord?> GetForPreparationAsync(string tenantReference,
        string preparationReference, CancellationToken cancellationToken = default);

    Task<CorporationTaxApprovalRecord> AddOrGetAsync(CorporationTaxApprovalRecord approval,
        CancellationToken cancellationToken = default);
}

internal sealed class AzureCorporationTaxApprovalStore(
    IConfiguration configuration,
    IOptions<CompaniesHouseProductHostOptions> options) : ICorporationTaxApprovalStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly CompaniesHouseProductHostOptions _options = options.Value;

    public async Task<CorporationTaxApprovalRecord?> GetAsync(string tenantReference,
        string approvalReference, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson
              FROM Tax.CorporationTaxApproval
             WHERE TenantReference = @tenant
               AND Reference = @reference;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "reference", approvalReference);
        return Deserialize(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<CorporationTaxApprovalRecord?> GetForPreparationAsync(string tenantReference,
        string preparationReference, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson
              FROM Tax.CorporationTaxApproval
             WHERE TenantReference = @tenant
               AND PreparationReference = @preparation;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "preparation", preparationReference);
        return Deserialize(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<CorporationTaxApprovalRecord> AddOrGetAsync(
        CorporationTaxApprovalRecord approval,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approval);
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await using var read = new SqlCommand("""
            SELECT PayloadJson
              FROM Tax.CorporationTaxApproval WITH (UPDLOCK, HOLDLOCK)
             WHERE TenantReference = @tenant
               AND PreparationReference = @preparation;
            """, connection, transaction);
        Add(read, "tenant", Guid.Parse(approval.TenantReference));
        Add(read, "preparation", approval.PreparationReference);
        var existing = Deserialize(await read.ExecuteScalarAsync(cancellationToken));
        if (existing is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return existing;
        }

        await using var write = new SqlCommand("""
            INSERT INTO Tax.CorporationTaxApproval
                (TenantReference, Reference, PreparationReference, AccountsSnapshotSha256,
                 ApprovalSha256, ApprovedAtUtc, PayloadJson)
            VALUES
                (@tenant, @reference, @preparation, @accounts, @approval,
                 @approvedAt, @payload);
            """, connection, transaction);
        Add(write, "tenant", Guid.Parse(approval.TenantReference));
        Add(write, "reference", approval.Reference);
        Add(write, "preparation", approval.PreparationReference);
        Add(write, "accounts", approval.AccountsSnapshotSha256);
        Add(write, "approval", approval.ApprovalSha256);
        Add(write, "approvedAt", approval.ApprovedAtUtc.UtcDateTime);
        Add(write, "payload", JsonSerializer.Serialize(approval, Json));
        if (await write.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new InvalidOperationException("The Corporation Tax approval was not retained.");
        await transaction.CommitAsync(cancellationToken);
        return approval;
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

    private static CorporationTaxApprovalRecord? Deserialize(object? value) => value is null or DBNull
        ? null
        : JsonSerializer.Deserialize<CorporationTaxApprovalRecord>((string)value, Json)
          ?? throw new InvalidDataException("Corporation Tax approval metadata is invalid.");

    private static void Add(SqlCommand command, string name, object value) =>
        command.Parameters.AddWithValue($"@{name}", value);
}

