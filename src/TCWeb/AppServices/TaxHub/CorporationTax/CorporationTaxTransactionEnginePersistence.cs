using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.TransactionEngine;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

/// <summary>
/// Shared, tenant-owned persistence for the HMRC Transaction Engine state machine.
/// Every transition is a serializable compare-and-replace of one bounded record;
/// no authority credential or XML document is stored in this table.
/// </summary>
internal sealed class AzureCorporationTaxTransactionStore(
    IConfiguration configuration,
    IOptions<CompaniesHouseProductHostOptions> options,
    TimeProvider timeProvider) : ITransactionEngineConversationStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly CompaniesHouseProductHostOptions _options = options.Value;

    public async Task<TransactionEngineConversationRecord> CreateAsync(
        string tenantReference, string principalReference, string logicalSubmissionReference,
        TransactionEngineMaterializedSubmission submission, string payloadReference,
        CancellationToken cancellationToken = default)
    {
        ValidateReference(tenantReference, nameof(tenantReference));
        ValidateReference(principalReference, nameof(principalReference));
        ValidateReference(logicalSubmissionReference, nameof(logicalSubmissionReference));
        ValidateReference(payloadReference, nameof(payloadReference));
        ArgumentNullException.ThrowIfNull(submission);

        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await using var existing = new SqlCommand("""
            SELECT TOP (1) Reference
              FROM Tax.CorporationTaxConversation WITH (UPDLOCK, HOLDLOCK)
             WHERE TenantReference = @tenant
               AND LogicalSubmissionReference = @logical;
            """, connection, transaction);
        Add(existing, "tenant", Guid.Parse(tenantReference));
        Add(existing, "logical", logicalSubmissionReference);
        if (await existing.ExecuteScalarAsync(cancellationToken) is not null)
            throw new InvalidOperationException(
                "A Transaction Engine conversation already exists for this approved filing.");

        var now = timeProvider.GetUtcNow();
        var created = new TransactionEngineConversationRecord(
            Guid.NewGuid().ToString("N"), tenantReference, principalReference,
            logicalSubmissionReference, submission.TransactionId, submission.ServiceRootSha256,
            submission.WireSha256, payloadReference, TransactionEngineConversationStage.Prepared,
            now, now);
        await using var insert = new SqlCommand("""
            INSERT INTO Tax.CorporationTaxConversation
                (TenantReference, Reference, PrincipalReference, LogicalSubmissionReference,
                 StageCode, IsComplete, PollNotBeforeUtc, UpdatedAtUtc, PayloadJson)
            VALUES
                (@tenant, @reference, @principal, @logical,
                 @stage, 0, @pollNotBefore, @updatedAt, @payload);
            """, connection, transaction);
        Bind(insert, created);
        if (await insert.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new InvalidOperationException(
                "The Corporation Tax conversation was not retained.");
        await transaction.CommitAsync(cancellationToken);
        return created;
    }

    public async Task<TransactionEngineConversationRecord?> GetAsync(string tenantReference,
        string principalReference, string conversationReference,
        CancellationToken cancellationToken = default)
    {
        ValidateReference(tenantReference, nameof(tenantReference));
        ValidateReference(principalReference, nameof(principalReference));
        ValidateReference(conversationReference, nameof(conversationReference));
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson
              FROM Tax.CorporationTaxConversation
             WHERE TenantReference = @tenant
               AND PrincipalReference = @principal
               AND Reference = @reference;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "principal", principalReference);
        Add(command, "reference", conversationReference);
        return Deserialize(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<TransactionEngineConversationRecord?> GetForLogicalSubmissionAsync(
        string tenantReference, string logicalSubmissionReference,
        CancellationToken cancellationToken = default)
    {
        ValidateReference(tenantReference, nameof(tenantReference));
        ValidateReference(logicalSubmissionReference, nameof(logicalSubmissionReference));
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT PayloadJson
              FROM Tax.CorporationTaxConversation
             WHERE TenantReference = @tenant
               AND LogicalSubmissionReference = @logical;
            """, connection);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "logical", logicalSubmissionReference);
        return Deserialize(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<TransactionEngineConversationRecord>> ListForOwnerAsync(
        string tenantReference, string principalReference, int maximumRecords,
        CancellationToken cancellationToken = default)
    {
        ValidateReference(tenantReference, nameof(tenantReference));
        ValidateReference(principalReference, nameof(principalReference));
        if (maximumRecords is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(maximumRecords));
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT TOP (@maximum) PayloadJson
              FROM Tax.CorporationTaxConversation
             WHERE TenantReference = @tenant
               AND PrincipalReference = @principal
             ORDER BY UpdatedAtUtc DESC, Reference DESC;
            """, connection);
        Add(command, "maximum", maximumRecords);
        Add(command, "tenant", Guid.Parse(tenantReference));
        Add(command, "principal", principalReference);
        var records = new List<TransactionEngineConversationRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            records.Add(Deserialize(reader.GetString(0))!);
        return records;
    }

    public async Task<IReadOnlyList<TransactionEngineConversationRecord>> ListActionableAsync(
        DateTimeOffset now, int maximumRecords,
        CancellationToken cancellationToken = default)
    {
        if (maximumRecords is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(maximumRecords));
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT TOP (@maximum) PayloadJson
              FROM Tax.CorporationTaxConversation
             WHERE IsComplete = 0
               AND (StageCode IN (3, 4)
                    OR (StageCode = 2 AND PollNotBeforeUtc <= @now))
             ORDER BY COALESCE(PollNotBeforeUtc, UpdatedAtUtc), Reference;
            """, connection);
        Add(command, "maximum", maximumRecords);
        Add(command, "now", now.UtcDateTime);
        var records = new List<TransactionEngineConversationRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var record = Deserialize(reader.GetString(0))!;
            records.Add(record);
        }
        return records.OrderBy(record => record.PollNotBefore ?? record.UpdatedAt).ToArray();
    }

    public Task<TransactionEngineConversationRecord> BeginSubmissionAsync(string tenant,
        string principal, string reference, CancellationToken cancellationToken = default) =>
        TransitionAsync(tenant, principal, reference, TransactionEngineConversationStage.Prepared,
            current => current with { Stage = TransactionEngineConversationStage.SubmissionUncertain },
            cancellationToken);

    public Task<TransactionEngineConversationRecord> RecordAcknowledgementAsync(string tenant,
        string principal, string reference, TransactionEngineParsedResponse response,
        CancellationToken cancellationToken = default)
    {
        if (response.Kind != TransactionEngineResponseKind.Acknowledgement
            || response.CorrelationId is null || response.ResponseEndpoint is null
            || response.PollIntervalSeconds is null)
            throw new ArgumentException("A valid acknowledgement is required.", nameof(response));
        return TransitionAsync(tenant, principal, reference,
            TransactionEngineConversationStage.SubmissionUncertain,
            current => current with
            {
                Stage = TransactionEngineConversationStage.AwaitingPoll,
                CorrelationId = response.CorrelationId,
                ResponseEndpoint = response.ResponseEndpoint.AbsoluteUri,
                PollIntervalSeconds = response.PollIntervalSeconds,
                PollNotBefore = timeProvider.GetUtcNow().AddSeconds(response.PollIntervalSeconds.Value)
            }, cancellationToken);
    }

    public Task<TransactionEngineConversationRecord> RecordRecoveredCorrelationAsync(
        string tenant, string principal, string reference, string correlationId,
        Uri responseEndpoint, int pollIntervalSeconds,
        CancellationToken cancellationToken = default)
    {
        TransactionEngineWire.ValidateCorrelationId(correlationId);
        if (responseEndpoint.Scheme != Uri.UriSchemeHttps
            || pollIntervalSeconds is < 1 or > 86_400)
            throw new ArgumentException("Recovered polling information is invalid.");
        return TransitionAsync(tenant, principal, reference,
            TransactionEngineConversationStage.SubmissionUncertain,
            current => current with
            {
                Stage = TransactionEngineConversationStage.AwaitingPoll,
                CorrelationId = correlationId,
                ResponseEndpoint = responseEndpoint.AbsoluteUri,
                PollIntervalSeconds = pollIntervalSeconds,
                PollNotBefore = timeProvider.GetUtcNow().AddSeconds(pollIntervalSeconds)
            }, cancellationToken);
    }

    public Task<TransactionEngineConversationRecord> RecordPollAcknowledgementAsync(
        string tenant, string principal, string reference, TransactionEngineParsedResponse response,
        CancellationToken cancellationToken = default) => TransitionAsync(
            tenant, principal, reference, TransactionEngineConversationStage.AwaitingPoll,
            current =>
            {
                EnsureSameCorrelation(current, response);
                if (response.Kind != TransactionEngineResponseKind.Acknowledgement
                    || response.ResponseEndpoint is null || response.PollIntervalSeconds is null)
                    throw new ArgumentException("A valid poll acknowledgement is required.",
                        nameof(response));
                return current with
                {
                    ResponseEndpoint = response.ResponseEndpoint.AbsoluteUri,
                    PollIntervalSeconds = response.PollIntervalSeconds,
                    PollNotBefore = timeProvider.GetUtcNow().AddSeconds(
                        response.PollIntervalSeconds.Value)
                };
            }, cancellationToken);

    public Task<TransactionEngineConversationRecord> RecordTerminalAsync(
        string tenant, string principal, string reference, TransactionEngineParsedResponse response,
        string terminalResponseReference, string? receiptReference,
        CancellationToken cancellationToken = default) => TransitionAsync(
            tenant, principal, reference, TransactionEngineConversationStage.AwaitingPoll,
            current =>
            {
                EnsureSameCorrelation(current, response);
                if (response.Kind is not (TransactionEngineResponseKind.BusinessResponse
                    or TransactionEngineResponseKind.BusinessError)
                    || response.ResponseEndpoint is null || response.PollIntervalSeconds is null)
                    throw new ArgumentException(
                        "A terminal business response or error is required.", nameof(response));
                ValidateReference(terminalResponseReference, nameof(terminalResponseReference));
                if (response.ContainsIrmarkReceipt && string.IsNullOrWhiteSpace(receiptReference))
                    throw new InvalidOperationException(
                        "The signed IRmark receipt must be retained before deletion.");
                if (receiptReference is not null)
                    ValidateReference(receiptReference, nameof(receiptReference));
                return current with
                {
                    Stage = TransactionEngineConversationStage.TerminalPersisted,
                    TerminalResponseReference = terminalResponseReference,
                    TerminalResponseSha256 = response.ResponseSha256,
                    ResponseEndpoint = response.ResponseEndpoint.AbsoluteUri,
                    PollIntervalSeconds = response.PollIntervalSeconds,
                    ReceiptReference = receiptReference,
                    Outcome = response.Kind.ToString()
                };
            }, cancellationToken);

    public Task<TransactionEngineConversationRecord> RefreshTerminalRoutingAsync(
        string tenant, string principal, string reference, TransactionEngineParsedResponse response,
        string terminalResponseReference, CancellationToken cancellationToken = default) =>
        MutateAsync(tenant, principal, reference, current =>
        {
            if (current.Stage is not (TransactionEngineConversationStage.TerminalPersisted
                    or TransactionEngineConversationStage.DeletePending))
                throw new InvalidOperationException(
                    "Terminal routing cannot be refreshed at this stage.");
            EnsureSameCorrelation(current, response);
            if (response.Kind is not (TransactionEngineResponseKind.BusinessResponse
                    or TransactionEngineResponseKind.BusinessError)
                || response.ResponseEndpoint is null || response.PollIntervalSeconds is null)
                throw new ArgumentException(
                    "A terminal response with routing information is required.", nameof(response));
            if (current.TerminalResponseReference != terminalResponseReference
                || current.TerminalResponseSha256 != response.ResponseSha256)
                throw new InvalidDataException(
                    "Terminal routing does not originate from the retained response.");
            return current with
            {
                ResponseEndpoint = response.ResponseEndpoint.AbsoluteUri,
                PollIntervalSeconds = response.PollIntervalSeconds
            };
        }, cancellationToken);

    public Task<TransactionEngineConversationRecord> BeginDeleteAsync(string tenant,
        string principal, string reference, CancellationToken cancellationToken = default) =>
        TransitionAsync(tenant, principal, reference,
            TransactionEngineConversationStage.TerminalPersisted,
            current => current.MayDelete
                ? current with { Stage = TransactionEngineConversationStage.DeletePending }
                : throw new InvalidOperationException(
                    "Terminal evidence must be persisted before delete."), cancellationToken);

    public Task<TransactionEngineConversationRecord> RecordDeleteResponseAsync(string tenant,
        string principal, string reference, TransactionEngineParsedResponse response,
        CancellationToken cancellationToken = default) => TransitionAsync(
            tenant, principal, reference, TransactionEngineConversationStage.DeletePending,
            current =>
            {
                EnsureSameCorrelation(current, response);
                if (response.Kind != TransactionEngineResponseKind.DeleteResponse)
                    throw new ArgumentException("A delete response is required.", nameof(response));
                return current with { Stage = TransactionEngineConversationStage.Complete };
            }, cancellationToken);

    public Task<TransactionEngineConversationRecord> RequireRecoveryAsync(string tenant,
        string principal, string reference, string reason,
        CancellationToken cancellationToken = default) => MutateAsync(
            tenant, principal, reference, current =>
            {
                if (current.Stage == TransactionEngineConversationStage.Complete)
                    throw new InvalidOperationException(
                        "A completed conversation cannot enter recovery.");
                ValidateReference(reason, nameof(reason));
                return current with
                {
                    Stage = TransactionEngineConversationStage.RecoveryRequired,
                    RecoveryReason = reason
                };
            }, cancellationToken);

    public Task<TransactionEngineConversationRecord> RecordGatewayErrorAsync(string tenant,
        string principal, string reference, TransactionEngineParsedResponse response,
        string responseReference, CancellationToken cancellationToken = default) => MutateAsync(
            tenant, principal, reference, current =>
            {
                if (current.Stage is not (TransactionEngineConversationStage.SubmissionUncertain
                        or TransactionEngineConversationStage.AwaitingPoll))
                    throw new InvalidOperationException(
                        "A gateway error is not valid for the current stage.");
                if (response.Kind != TransactionEngineResponseKind.GatewayError)
                    throw new ArgumentException("A gateway error response is required.",
                        nameof(response));
                ValidateReference(responseReference, nameof(responseReference));
                return current with
                {
                    Stage = TransactionEngineConversationStage.RecoveryRequired,
                    TerminalResponseReference = responseReference,
                    TerminalResponseSha256 = response.ResponseSha256,
                    Outcome = response.Kind.ToString(),
                    RecoveryReason = "The Transaction Engine returned a gateway error."
                };
            }, cancellationToken);

    public Task<TransactionEngineConversationRecord> RecordResponseEvidenceAsync(
        string tenant, string principal, string reference, string responseReference,
        string responseSha256, CancellationToken cancellationToken = default) => MutateAsync(
            tenant, principal, reference, current =>
            {
                ValidateReference(responseReference, nameof(responseReference));
                ValidateSha256(responseSha256, nameof(responseSha256));
                var evidence = current.ResponseEvidence?.ToList() ?? [];
                if (evidence.Count >= 100)
                    throw new InvalidOperationException(
                        "The Transaction Engine response evidence limit was reached.");
                evidence.Add(new(responseReference, responseSha256, timeProvider.GetUtcNow()));
                return current with { ResponseEvidence = evidence };
            }, cancellationToken);

    private Task<TransactionEngineConversationRecord> TransitionAsync(string tenant,
        string principal, string reference, TransactionEngineConversationStage expected,
        Func<TransactionEngineConversationRecord, TransactionEngineConversationRecord> transition,
        CancellationToken cancellationToken) => MutateAsync(tenant, principal, reference,
        current => current.Stage == expected
            ? transition(current)
            : throw new InvalidOperationException(
                $"Conversation stage {current.Stage} does not permit this operation."),
        cancellationToken);

    private async Task<TransactionEngineConversationRecord> MutateAsync(string tenant,
        string principal, string reference,
        Func<TransactionEngineConversationRecord, TransactionEngineConversationRecord> mutation,
        CancellationToken cancellationToken)
    {
        ValidateReference(tenant, nameof(tenant));
        ValidateReference(principal, nameof(principal));
        ValidateReference(reference, nameof(reference));
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await using var read = new SqlCommand("""
            SELECT PayloadJson
              FROM Tax.CorporationTaxConversation WITH (UPDLOCK, HOLDLOCK)
             WHERE TenantReference = @tenant
               AND PrincipalReference = @principal
               AND Reference = @reference;
            """, connection, transaction);
        Add(read, "tenant", Guid.Parse(tenant));
        Add(read, "principal", principal);
        Add(read, "reference", reference);
        var current = Deserialize(await read.ExecuteScalarAsync(cancellationToken))
            ?? throw new KeyNotFoundException(
                "No Transaction Engine conversation exists for this owner.");
        var updated = mutation(current) with { UpdatedAt = timeProvider.GetUtcNow() };
        await using var write = new SqlCommand("""
            UPDATE Tax.CorporationTaxConversation
               SET StageCode = @stage,
                   IsComplete = @complete,
                   PollNotBeforeUtc = @pollNotBefore,
                   UpdatedAtUtc = @updatedAt,
                   PayloadJson = @payload
             WHERE TenantReference = @tenant
               AND PrincipalReference = @principal
               AND Reference = @reference;
            """, connection, transaction);
        Bind(write, updated);
        if (await write.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new InvalidOperationException(
                "The Corporation Tax conversation transition was not retained.");
        await transaction.CommitAsync(cancellationToken);
        return updated;
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

    private static void Bind(SqlCommand command, TransactionEngineConversationRecord record)
    {
        Add(command, "tenant", Guid.Parse(record.TenantReference));
        Add(command, "reference", record.ConversationReference);
        Add(command, "principal", record.PrincipalReference);
        Add(command, "logical", record.LogicalSubmissionReference);
        Add(command, "stage", (short)record.Stage);
        Add(command, "complete", record.Stage == TransactionEngineConversationStage.Complete);
        Add(command, "pollNotBefore",
            record.PollNotBefore?.UtcDateTime ?? (object)DBNull.Value);
        Add(command, "updatedAt", record.UpdatedAt.UtcDateTime);
        Add(command, "payload", JsonSerializer.Serialize(record, Json));
    }

    private static TransactionEngineConversationRecord? Deserialize(object? value) =>
        value is null or DBNull
            ? null
            : JsonSerializer.Deserialize<TransactionEngineConversationRecord>((string)value, Json)
              ?? throw new InvalidDataException(
                  "Corporation Tax conversation metadata is invalid.");

    private static void EnsureSameCorrelation(TransactionEngineConversationRecord current,
        TransactionEngineParsedResponse response)
    {
        if (current.CorrelationId is null || response.CorrelationId != current.CorrelationId)
            throw new InvalidDataException(
                "The response correlation ID does not match the conversation.");
    }

    private static void ValidateReference(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 256 || value.Any(char.IsControl))
            throw new ArgumentException(
                "A bounded conversation reference is required.", parameterName);
    }

    private static void ValidateSha256(string value, string parameterName)
    {
        if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
            throw new ArgumentException("A SHA-256 digest is required.", parameterName);
    }

    private static void Add(SqlCommand command, string name, object value) =>
        command.Parameters.AddWithValue($"@{name}", value);
}
