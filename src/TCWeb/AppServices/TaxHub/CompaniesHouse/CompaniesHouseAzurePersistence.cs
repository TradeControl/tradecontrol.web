using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

internal interface ICompaniesHousePersistenceProbe
{
    Task ProbeAsync(CancellationToken cancellationToken = default);
}

internal sealed class AzureCompaniesHousePersistence(
    IConfiguration configuration,
    IOptions<CompaniesHouseProductHostOptions> options,
    BlobContainerClient evidenceContainer) : ICompaniesHouseWorkflowStore,
    ICompaniesHouseProtectedContentStore, ICompaniesHousePersistenceProbe
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly CompaniesHouseProductHostOptions _options = options.Value;

    public async Task ProbeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            "SELECT CASE WHEN OBJECT_ID(N'TaxHub.CompaniesHousePreparation', N'U') IS NOT NULL " +
            "AND OBJECT_ID(N'TaxHub.CompaniesHouseApproval', N'U') IS NOT NULL " +
            "AND OBJECT_ID(N'TaxHub.CompaniesHouseConversation', N'U') IS NOT NULL THEN 1 ELSE 0 END;",
            connection);
        if (Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) != 1)
            throw new InvalidOperationException("The Companies House workflow schema is unavailable.");
        await evidenceContainer.GetPropertiesAsync(cancellationToken: cancellationToken);
    }

    public async Task AddPreparationAsync(CompaniesHousePreparationRecord preparation,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            INSERT INTO TaxHub.CompaniesHousePreparation
                (TenantReference, Reference, CompanyIdentitySha256, PeriodEnd, CreatedAtUtc, PayloadJson)
            VALUES (@tenant, @reference, @company, @periodEnd, @created, @payload);
            """, connection);
        Add(command, "tenant", Guid.Parse(preparation.TenantReference));
        Add(command, "reference", preparation.Reference);
        Add(command, "company", preparation.CompanyIdentitySha256);
        Add(command, "periodEnd", preparation.PeriodEnd.ToDateTime(TimeOnly.MinValue));
        Add(command, "created", preparation.CreatedAtUtc.UtcDateTime);
        Add(command, "payload", JsonSerializer.Serialize(preparation, Json));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task<CompaniesHousePreparationRecord?> GetPreparationAsync(string tenantReference,
        string reference, CancellationToken cancellationToken = default) => ReadPayloadAsync<CompaniesHousePreparationRecord>(
        "TaxHub.CompaniesHousePreparation", tenantReference, reference, cancellationToken);

    public async Task AddApprovalAsync(CompaniesHouseApprovalRecord approval,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            INSERT INTO TaxHub.CompaniesHouseApproval
                (TenantReference, Reference, PreparationReference, LogicalFilingIdentity,
                 CompanyIdentitySha256, PeriodEnd, ApprovedAtUtc, PayloadJson)
            VALUES (@tenant, @reference, @preparation, @logical, @company, @periodEnd, @approved, @payload);
            """, connection);
        Add(command, "tenant", Guid.Parse(approval.TenantReference));
        Add(command, "reference", approval.Reference);
        Add(command, "preparation", approval.PreparationReference);
        Add(command, "logical", approval.LogicalFilingIdentity);
        Add(command, "company", approval.CompanyIdentitySha256);
        Add(command, "periodEnd", approval.PeriodEnd.ToDateTime(TimeOnly.MinValue));
        Add(command, "approved", approval.ApprovedAtUtc.UtcDateTime);
        Add(command, "payload", JsonSerializer.Serialize(approval, Json));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task<CompaniesHouseApprovalRecord?> GetApprovalAsync(string tenantReference, string reference,
        CancellationToken cancellationToken = default) => ReadPayloadAsync<CompaniesHouseApprovalRecord>(
        "TaxHub.CompaniesHouseApproval", tenantReference, reference, cancellationToken);

    public async Task<CompaniesHouseApprovalRecord?> GetApprovalForPreparationAsync(string tenantReference,
        string preparationReference, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson FROM TaxHub.CompaniesHouseApproval
            WHERE TenantReference = @tenant AND PreparationReference = @preparation;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "preparation", preparationReference);
        return Deserialize<CompaniesHouseApprovalRecord>(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<CompaniesHouseConversationRecord?> GetActiveConversationAsync(string tenantReference,
        string logicalFilingIdentity, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson FROM TaxHub.CompaniesHouseConversation
            WHERE TenantReference = @tenant AND LogicalFilingIdentity = @logical AND IsActive = 1;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "logical", logicalFilingIdentity);
        return Deserialize<CompaniesHouseConversationRecord>(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task UpsertConversationAsync(CompaniesHouseConversationRecord conversation,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var isActive = IsActive(conversation.State);
        await using var command = new SqlCommand("""
            IF EXISTS (SELECT 1 FROM TaxHub.CompaniesHouseConversation WITH (UPDLOCK, HOLDLOCK)
                       WHERE TenantReference = @tenant AND Reference = @reference)
                UPDATE TaxHub.CompaniesHouseConversation
                   SET ApprovalReference = @approval, LogicalFilingIdentity = @logical,
                       CompanyIdentitySha256 = @company, State = @state, IsActive = @active,
                       UpdatedAtUtc = @updated, PayloadJson = @payload
                 WHERE TenantReference = @tenant AND Reference = @reference;
            ELSE
                INSERT INTO TaxHub.CompaniesHouseConversation
                    (TenantReference, Reference, ApprovalReference, LogicalFilingIdentity,
                     CompanyIdentitySha256, State, IsActive, UpdatedAtUtc, PayloadJson)
                VALUES (@tenant, @reference, @approval, @logical, @company, @state, @active,
                        @updated, @payload);
            """, connection, transaction);
        Add(command, "tenant", Guid.Parse(conversation.TenantReference));
        Add(command, "reference", conversation.Reference);
        Add(command, "approval", conversation.ApprovalReference);
        Add(command, "logical", conversation.LogicalFilingIdentity);
        Add(command, "company", conversation.CompanyIdentitySha256);
        Add(command, "state", (int)conversation.State);
        Add(command, "active", isActive);
        Add(command, "updated", conversation.UpdatedAtUtc.UtcDateTime);
        Add(command, "payload", JsonSerializer.Serialize(conversation, Json));
        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompaniesHouseConversationRecord>> ListConversationsAsync(
        string tenantReference, string companyIdentitySha256, CancellationToken cancellationToken = default)
    {
        var result = new List<CompaniesHouseConversationRecord>();
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson FROM TaxHub.CompaniesHouseConversation
            WHERE TenantReference = @tenant AND CompanyIdentitySha256 = @company
            ORDER BY UpdatedAtUtc DESC;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "company", companyIdentitySha256);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(JsonSerializer.Deserialize<CompaniesHouseConversationRecord>(reader.GetString(0), Json)
                ?? throw new InvalidDataException("Companies House conversation metadata is invalid."));
        return result;
    }

    public async Task<CompaniesHouseProtectedContent> WriteAsync(string tenantReference, string contentKind,
        ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        var tenant = Guid.Parse(tenantReference).ToString("N");
        if (string.IsNullOrWhiteSpace(contentKind) || contentKind.Length > 64
            || contentKind.Any(character => !(char.IsLower(character) || char.IsDigit(character) || character == '-')))
            throw new ArgumentException("A bounded lower-case content kind is required.", nameof(contentKind));
        if (bytes.IsEmpty) throw new ArgumentException("Protected content cannot be empty.", nameof(bytes));

        var reference = $"chb1-{Guid.NewGuid():N}";
        var sha256 = Convert.ToHexString(SHA256.HashData(bytes.Span));
        var blob = evidenceContainer.GetBlobClient($"{tenant}/{reference}");
        await blob.UploadAsync(BinaryData.FromBytes(bytes), new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" },
            Metadata = new Dictionary<string, string>
            {
                ["sha256"] = sha256,
                ["kind"] = contentKind,
                ["tenant"] = tenant
            },
            Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
        }, cancellationToken);
        return new(reference, sha256, bytes.Length, bytes.ToArray());
    }

    public async Task<CompaniesHouseProtectedContent> ReadAsync(string tenantReference, string reference,
        string expectedSha256, CancellationToken cancellationToken = default)
    {
        var tenant = Guid.Parse(tenantReference).ToString("N");
        if (!IsReference(reference)) throw new ArgumentException("An opaque Blob reference is required.", nameof(reference));
        if (!IsSha256(expectedSha256)) throw new ArgumentException("A SHA-256 value is required.", nameof(expectedSha256));

        var blob = evidenceContainer.GetBlobClient($"{tenant}/{reference}");
        var download = await blob.DownloadContentAsync(cancellationToken);
        var bytes = download.Value.Content.ToArray();
        var actual = Convert.ToHexString(SHA256.HashData(bytes));
        if (!CryptographicOperations.FixedTimeEquals(Convert.FromHexString(actual),
                Convert.FromHexString(expectedSha256)))
            throw new InvalidDataException("Protected Companies House content failed digest verification.");
        if (!download.Value.Details.Metadata.TryGetValue("tenant", out var storedTenant)
            || !string.Equals(storedTenant, tenant, StringComparison.Ordinal)
            || !download.Value.Details.Metadata.TryGetValue("sha256", out var storedSha)
            || !string.Equals(storedSha, actual, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Protected Companies House content metadata is invalid.");
        return new(reference, actual, bytes.LongLength, bytes);
    }

    private async Task<T?> ReadPayloadAsync<T>(string table, string tenantReference, string reference,
        CancellationToken cancellationToken)
    {
        if (table is not ("TaxHub.CompaniesHousePreparation" or "TaxHub.CompaniesHouseApproval"))
            throw new ArgumentOutOfRangeException(nameof(table));
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            $"SELECT PayloadJson FROM {table} WHERE TenantReference = @tenant AND Reference = @reference;",
            connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "reference", reference);
        return Deserialize<T>(await command.ExecuteScalarAsync(cancellationToken));
    }

    private async Task<SqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var name = _options.WorkflowConnectionName;
        var connectionString = string.IsNullOrWhiteSpace(name) ? null : configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The named Companies House workflow connection is unavailable.");
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static T? Deserialize<T>(object? value) => value is null or DBNull
        ? default
        : JsonSerializer.Deserialize<T>((string)value, Json)
          ?? throw new InvalidDataException("Companies House workflow metadata is invalid.");

    private static void Add(SqlCommand command, string name, object value) =>
        command.Parameters.AddWithValue($"@{name}", value);

    private static bool IsActive(TradeControl.Tax.UK.Application.Preparation.CompaniesHouseConversationState state) =>
        state is not (TradeControl.Tax.UK.Application.Preparation.CompaniesHouseConversationState.Accepted
            or TradeControl.Tax.UK.Application.Preparation.CompaniesHouseConversationState.Rejected
            or TradeControl.Tax.UK.Application.Preparation.CompaniesHouseConversationState.GatewayRejected);

    private static bool IsReference(string value) => value.Length == 37
        && value.StartsWith("chb1-", StringComparison.Ordinal)
        && Guid.TryParseExact(value[5..], "N", out _);

    private static bool IsSha256(string value) => value.Length == 64 && value.All(Uri.IsHexDigit);
}
