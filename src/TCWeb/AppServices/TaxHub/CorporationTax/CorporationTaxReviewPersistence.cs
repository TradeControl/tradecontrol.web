using System;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Application.DataProvision;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal sealed record CorporationTaxReviewedPeriodRecord(
    string TenantReference,
    string LogicalFilingIdentity,
    string AccountsSnapshotSha256,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    CorporationTaxPeriodReviewedInput Input,
    string SourceSha256,
    string InputSha256,
    int Revision,
    string ReviewedBy,
    DateTimeOffset ReviewedAtUtc);

internal interface ICorporationTaxReviewStore
{
    Task<CorporationTaxReviewedPeriodRecord?> GetAsync(string tenantReference,
        string logicalFilingIdentity, CancellationToken cancellationToken = default);

    Task<bool> TrySaveAsync(CorporationTaxReviewedPeriodRecord review, int expectedRevision,
        CancellationToken cancellationToken = default);
}

internal sealed class AzureCorporationTaxReviewStore(
    IConfiguration configuration,
    IOptions<CompaniesHouseProductHostOptions> options) : ICorporationTaxReviewStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly CompaniesHouseProductHostOptions _options = options.Value;

    public async Task<CorporationTaxReviewedPeriodRecord?> GetAsync(string tenantReference,
        string logicalFilingIdentity, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson
              FROM Tax.CorporationTaxReview
             WHERE TenantReference = @tenant
               AND LogicalFilingIdentity = @logical;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "logical", logicalFilingIdentity);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull
            ? null
            : JsonSerializer.Deserialize<CorporationTaxReviewedPeriodRecord>((string)value, Json)
              ?? throw new InvalidDataException("Corporation Tax review metadata is invalid.");
    }

    public async Task<bool> TrySaveAsync(CorporationTaxReviewedPeriodRecord review,
        int expectedRevision, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (review.Revision != expectedRevision + 1)
            throw new ArgumentException("The Corporation Tax review revision is invalid.",
                nameof(review));

        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await using var read = new SqlCommand("""
            SELECT Revision
              FROM Tax.CorporationTaxReview WITH (UPDLOCK, HOLDLOCK)
             WHERE TenantReference = @tenant
               AND LogicalFilingIdentity = @logical;
            """, connection, transaction);
        Add(read, "tenant", Guid.Parse(review.TenantReference));
        Add(read, "logical", review.LogicalFilingIdentity);
        var current = await read.ExecuteScalarAsync(cancellationToken);
        var currentRevision = current is null or DBNull ? 0 : Convert.ToInt32(current);
        if (currentRevision != expectedRevision)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await using var write = new SqlCommand(currentRevision == 0 ? """
            INSERT INTO Tax.CorporationTaxReview
                (TenantReference, LogicalFilingIdentity, AccountsSnapshotSha256,
                 PeriodStart, PeriodEnd, SourceSha256, InputSha256, Revision,
                 ReviewedBy, ReviewedAtUtc, PayloadJson)
            VALUES
                (@tenant, @logical, @accounts, @periodStart, @periodEnd, @source,
                 @input, @revision, @reviewedBy, @reviewedAt, @payload);
            """ : """
            UPDATE Tax.CorporationTaxReview
               SET AccountsSnapshotSha256 = @accounts,
                   PeriodStart = @periodStart,
                   PeriodEnd = @periodEnd,
                   SourceSha256 = @source,
                   InputSha256 = @input,
                   Revision = @revision,
                   ReviewedBy = @reviewedBy,
                   ReviewedAtUtc = @reviewedAt,
                   PayloadJson = @payload
             WHERE TenantReference = @tenant
               AND LogicalFilingIdentity = @logical
               AND Revision = @expectedRevision;
            """, connection, transaction);
        Add(write, "tenant", Guid.Parse(review.TenantReference));
        Add(write, "logical", review.LogicalFilingIdentity);
        Add(write, "accounts", review.AccountsSnapshotSha256);
        Add(write, "periodStart", review.PeriodStart.ToDateTime(TimeOnly.MinValue));
        Add(write, "periodEnd", review.PeriodEnd.ToDateTime(TimeOnly.MinValue));
        Add(write, "source", review.SourceSha256);
        Add(write, "input", review.InputSha256);
        Add(write, "revision", review.Revision);
        Add(write, "reviewedBy", review.ReviewedBy);
        Add(write, "reviewedAt", review.ReviewedAtUtc.UtcDateTime);
        Add(write, "payload", JsonSerializer.Serialize(review, Json));
        if (currentRevision != 0)
            Add(write, "expectedRevision", expectedRevision);
        var changed = await write.ExecuteNonQueryAsync(cancellationToken) == 1;
        if (changed)
            await transaction.CommitAsync(cancellationToken);
        else
            await transaction.RollbackAsync(cancellationToken);
        return changed;
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

    private static void Add(SqlCommand command, string name, object value) =>
        command.Parameters.AddWithValue($"@{name}", value);
}
