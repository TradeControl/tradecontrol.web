using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.DataProtection;
using TradeControl.Tax.UK.Adapters.Submission.Audit;
using TradeControl.Tax.UK.Adapters.Submission.FraudPrevention;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

internal interface IVatPersistenceProbe
{
    Task ProbeAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Azure-backed VAT workflow persistence. Blob names contain only irreversible owner fingerprints;
/// all OAuth grants, browser facts, audit metadata and authority content are additionally protected
/// by the application's Data Protection key ring, whose keys are protected by Azure Key Vault.
/// </summary>
public sealed class AzureVatPersistence : IOAuthGrantStore, IFraudContextStore,
    ISubmissionAttemptStore, ISubmissionContentStore, IVatPersistenceProbe
{
    private const int MaximumFraudBytes = 64 * 1024;
    private const int MaximumPayloadBytes = 1_048_576;
    private const int MaximumResponseBytes = 262_144;
    private static readonly TimeSpan FraudRetention = TimeSpan.FromDays(30);
    private static readonly TimeSpan AttemptRetention = TimeSpan.FromDays(365.25 * 7);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly BlobContainerClient _container;
    private readonly IDataProtector _protector;
    private readonly TimeProvider _clock;

    public AzureVatPersistence(BlobContainerClient container, IDataProtectionProvider protection,
        TimeProvider clock)
    {
        _container = container;
        _protector = protection.CreateProtector("TradeControl.Web.TaxHub.Vat.AzurePersistence.v1");
        _clock = clock;
    }

    public async Task ProbeAsync(CancellationToken cancellationToken = default)
    {
        await _container.GetPropertiesAsync(cancellationToken: cancellationToken);
        var blob = _container.GetBlobClient("vat/v1/readiness");
        try
        {
            await blob.UploadAsync(BinaryData.FromBytes(_protector.Protect([1])), new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            }, cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status is 409 or 412) { }
        var marker = await ReadBytesAsync(blob, null, cancellationToken);
        if (marker is not [1])
            throw new InvalidDataException("The protected VAT readiness marker is invalid.");
    }

    public async Task SavePendingAsync(PendingOAuthAuthorisation pending,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pending);
        await UploadNewAsync(PendingPath(pending.State), JsonSerializer.SerializeToUtf8Bytes(pending, Json),
            cancellationToken);
    }

    public async Task<PendingOAuthAuthorisation?> ConsumePendingAsync(string state, string tenantReference,
        string principalReference, string actorReference, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(PendingPath(state));
        await using var lease = await AcquireLeaseAsync(blob, createIfMissing: false, cancellationToken);
        if (lease is null) return null;
        var pending = await ReadAsync<PendingOAuthAuthorisation>(blob, lease.LeaseId, cancellationToken);
        if (pending is null) return null;
        var matches = pending.ExpiresAtUtc > nowUtc && FixedEquals(pending.State, state)
            && pending.TenantReference == tenantReference
            && pending.PrincipalReference == principalReference
            && pending.ActorReference == actorReference;
        if (matches || pending.ExpiresAtUtc <= nowUtc)
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots,
                new BlobRequestConditions { LeaseId = lease.LeaseId }, cancellationToken);
        return matches ? pending : null;
    }

    public async Task<StoredOAuthGrant?> GetGrantAsync(string tenantReference, string principalReference,
        string requiredScope, CancellationToken cancellationToken = default)
    {
        var grant = await ReadAsync<StoredOAuthGrant>(_container.GetBlobClient(GrantPath(tenantReference,
            principalReference)), null, cancellationToken);
        return grant?.Scopes.Contains(requiredScope) == true ? grant : null;
    }

    public Task SaveGrantAsync(StoredOAuthGrant grant, CancellationToken cancellationToken = default) =>
        WriteAsync(_container.GetBlobClient(GrantPath(grant.TenantReference, grant.PrincipalReference)),
            JsonSerializer.SerializeToUtf8Bytes(grant, Json), overwrite: true, cancellationToken);

    public async Task RevokeAsync(string tenantReference, string principalReference, string requiredScope,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireRefreshLeaseAsync(tenantReference, principalReference,
            requiredScope, cancellationToken);
        var grant = await GetGrantAsync(tenantReference, principalReference, requiredScope, cancellationToken);
        if (grant is null) return;
        await SaveGrantAsync(grant with { Revoked = true, AccessToken = "-", RefreshToken = "-" },
            cancellationToken);
    }

    public async Task<IAsyncDisposable> AcquireRefreshLeaseAsync(string tenantReference,
        string principalReference, string requiredScope, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient($"vat/v1/oauth/leases/{Fingerprint(tenantReference, principalReference)}");
        return await AcquireLeaseAsync(blob, createIfMissing: true, cancellationToken)
            ?? throw new InvalidOperationException("The OAuth refresh lease is unavailable.");
    }

    public async Task<SealedFraudContextReference> StoreAsync(FraudContextSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(snapshot, Json);
        if (bytes.Length > MaximumFraudBytes)
            throw new FraudContextRejectedException("The sealed fraud context exceeds its storage bound.");
        var reference = new SealedFraudContextReference(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
        await UploadNewAsync(FraudPath(reference, new(snapshot.TenantReference, snapshot.PrincipalReference,
            snapshot.ActorReference)), bytes, cancellationToken);
        return reference;
    }

    public async Task<FraudContextSnapshot?> ReadAsync(SealedFraudContextReference reference,
        FraudActorIdentity identity, CancellationToken cancellationToken = default)
    {
        identity = identity.Validate();
        var blob = _container.GetBlobClient(FraudPath(reference, identity));
        try
        {
            var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);
            if (properties.Value.LastModified < _clock.GetUtcNow() - FraudRetention)
            {
                await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                return null;
            }
        }
        catch (RequestFailedException exception) when (exception.Status == 404) { return null; }
        var snapshot = await ReadAsync<FraudContextSnapshot>(blob, null, cancellationToken);
        if (snapshot is null) return null;
        if (snapshot.TenantReference != identity.TenantReference
            || snapshot.PrincipalReference != identity.PrincipalReference
            || snapshot.ActorReference != identity.ActorReference)
            throw new FraudContextRejectedException("The sealed fraud context ownership is invalid.");
        return snapshot;
    }

    public async Task<SubmissionAttemptRecord> ReserveAsync(SubmissionAttemptReservation reservation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        var method = reservation.Method.Trim().ToUpperInvariant();
        var isWrite = method is not "GET" and not "HEAD";
        return await MutateAttemptsAsync(reservation.TenantReference, records =>
        {
            records.RemoveAll(item => !item.IsActive && item.UpdatedAt < _clock.GetUtcNow() - AttemptRetention);
            var blocking = isWrite ? records.FirstOrDefault(item => item.IsWrite && item.BlocksReplay
                && item.PrincipalReference == reservation.PrincipalReference
                && item.LogicalSubmissionReference == reservation.LogicalSubmissionReference) : null;
            if (blocking is not null) throw new ActiveSubmissionAttemptException(blocking.AttemptReference);
            var now = _clock.GetUtcNow();
            var record = new SubmissionAttemptRecord(Guid.NewGuid().ToString("N"),
                reservation.LogicalSubmissionReference, reservation.OperationId, method,
                reservation.TenantReference, reservation.PrincipalReference,
                reservation.SubjectPeriodReference, reservation.PreparedDigest,
                reservation.ApprovalReference, reservation.Environment, SubmissionAttemptState.Reserved,
                now, now, SafePayloadReference: reservation.SafePayloadReference);
            records.Add(record);
            return record;
        }, cancellationToken);
    }

    public async Task<SubmissionAttemptRecord?> GetAsync(string tenantReference, string principalReference,
        string attemptReference, CancellationToken cancellationToken = default) =>
        (await ReadAttemptsAsync(tenantReference, cancellationToken)).SingleOrDefault(item =>
            item.PrincipalReference == principalReference && item.AttemptReference == attemptReference);

    public async Task<IReadOnlyList<SubmissionAttemptRecord>> ListAsync(string tenantReference,
        string principalReference, int maximumRecords = 100, CancellationToken cancellationToken = default) =>
        (await ReadAttemptsAsync(tenantReference, cancellationToken))
            .Where(item => item.PrincipalReference == principalReference)
            .OrderByDescending(item => item.UpdatedAt).Take(maximumRecords).ToArray();

    public async Task<IReadOnlyList<SubmissionAttemptRecord>> ListTenantAsync(string tenantReference,
        int maximumRecords = 100, CancellationToken cancellationToken = default) =>
        (await ReadAttemptsAsync(tenantReference, cancellationToken))
            .OrderByDescending(item => item.UpdatedAt).Take(maximumRecords).ToArray();

    public Task<SubmissionAttemptRecord> RecordPayloadAsync(string tenantReference, string principalReference,
        string attemptReference, string safePayloadReference, CancellationToken cancellationToken = default) =>
        UpdateAttemptAsync(tenantReference, principalReference, attemptReference, current =>
        {
            if (!current.IsWrite || current.State != SubmissionAttemptState.Reserved
                || current.SafePayloadReference is not null)
                throw new InvalidOperationException("A protected payload can only be attached once to a reserved write attempt.");
            return current with { SafePayloadReference = safePayloadReference, UpdatedAt = _clock.GetUtcNow() };
        }, cancellationToken);

    public Task<SubmissionAttemptRecord> RecordOutcomeAsync(string tenantReference, string principalReference,
        string attemptReference, SubmissionAttemptOutcome outcome,
        CancellationToken cancellationToken = default) => UpdateAttemptAsync(tenantReference,
        principalReference, attemptReference, current =>
        {
            if (!CanTransition(current.State, outcome.State))
                throw new InvalidOperationException("The requested attempt-state transition is not permitted.");
            return current with
            {
                State = outcome.State, UpdatedAt = _clock.GetUtcNow(), OutcomeCode = outcome.OutcomeCode,
                ActualStatusCode = outcome.ActualStatusCode, CorrelationReference = outcome.CorrelationReference,
                SafeResponseReference = outcome.SafeResponseReference, Receipt = outcome.Receipt
            };
        }, cancellationToken);

    public Task<SubmissionAttemptRecord> RecordReconciliationAsync(string tenantReference,
        string principalReference, string attemptReference, SubmissionReconciliationEvidence reconciliation,
        CancellationToken cancellationToken = default) =>
        UpdateAttemptAsync(tenantReference, principalReference, attemptReference, current =>
        {
            if (current.Reconciliation is not null && current.Reconciliation != reconciliation)
                throw new InvalidOperationException("Reconciliation evidence is immutable once recorded.");
            return current with { Reconciliation = reconciliation, UpdatedAt = _clock.GetUtcNow() };
        }, cancellationToken);

    public async Task<string> StoreAsync(string tenantReference, string principalReference,
        SubmissionContentKind kind, ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default)
    {
        var maximum = kind == SubmissionContentKind.Payload ? MaximumPayloadBytes : MaximumResponseBytes;
        if (content.IsEmpty || content.Length > maximum)
            throw new ArgumentException("Protected content is empty or exceeds its configured bound.", nameof(content));
        var kindName = kind == SubmissionContentKind.Payload ? "payload" : "response";
        var reference = $"{kindName}/{Fingerprint(tenantReference)}/{Fingerprint(principalReference)}/{Guid.NewGuid():N}.bin";
        await UploadNewAsync($"vat/v1/content/{reference}", content.ToArray(), cancellationToken);
        return reference;
    }

    public async Task<byte[]?> ReadAsync(string tenantReference, string principalReference,
        string contentReference, CancellationToken cancellationToken = default)
    {
        var segments = contentReference.Split('/');
        if (segments.Length != 4 || segments[0] is not ("payload" or "response")
            || segments[1] != Fingerprint(tenantReference) || segments[2] != Fingerprint(principalReference)
            || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(segments[3]), "N", out _)) return null;
        return await ReadBytesAsync(_container.GetBlobClient($"vat/v1/content/{contentReference}"), null,
            cancellationToken);
    }

    internal Task AddReviewCandidateAsync(VatReviewCandidateRecord candidate,
        CancellationToken cancellationToken = default) =>
        MutateReviewCandidatesAsync(candidate.TenantReference, records =>
        {
            records.RemoveAll(item => item.ExpiresAtUtc <= _clock.GetUtcNow());
            if (records.Any(item => item.Reference == candidate.Reference))
                throw new InvalidOperationException("The VAT preparation reference already exists.");
            records.Add(candidate);
            return true;
        }, cancellationToken);

    internal async Task<VatReviewCandidateRecord?> GetReviewCandidateAsync(string reference,
        VatWorkflowIdentity identity, CancellationToken cancellationToken = default) =>
        (await ReadReviewCandidatesAsync(identity.TenantReference, cancellationToken)).SingleOrDefault(item =>
            item.Reference == reference && item.PrincipalReference == identity.AspNetSubjectReference
            && item.ActorReference == identity.ActorReference);

    internal async Task<VatReviewCandidateRecord?> GetReviewCandidateForTenantAsync(string tenantReference,
        string reference, CancellationToken cancellationToken = default) =>
        (await ReadReviewCandidatesAsync(tenantReference, cancellationToken)).SingleOrDefault(item =>
            item.Reference == reference && item.TenantReference == tenantReference);

    internal async Task<IReadOnlyList<VatReviewCandidateRecord>> GetReviewCandidatesAsync(
        string tenantReference, IReadOnlySet<string> references,
        CancellationToken cancellationToken = default) =>
        (await ReadReviewCandidatesAsync(tenantReference, cancellationToken))
            .Where(item => references.Contains(item.Reference)).ToArray();

    internal async Task<IReadOnlyList<VatReviewCandidateRecord>> GetReviewCandidatesAsync(
        VatWorkflowIdentity identity, CancellationToken cancellationToken = default) =>
        (await ReadReviewCandidatesAsync(identity.TenantReference, cancellationToken))
            .Where(item => item.PrincipalReference == identity.AspNetSubjectReference
                && item.ActorReference == identity.ActorReference).ToArray();

    internal Task<VatReviewApprovalRecord> AddReviewApprovalAsync(VatReviewApprovalRecord proposed,
        CancellationToken cancellationToken = default) =>
        MutateReviewApprovalsAsync(proposed.TenantReference, records =>
        {
            var existing = records.SingleOrDefault(item =>
                item.PreparationReference == proposed.PreparationReference
                && item.PrincipalReference == proposed.PrincipalReference);
            if (existing is not null) return existing;
            records.Add(proposed);
            return proposed;
        }, cancellationToken);

    internal async Task<VatReviewApprovalRecord?> GetReviewApprovalAsync(string reference,
        VatWorkflowIdentity identity, CancellationToken cancellationToken = default) =>
        (await ReadReviewApprovalsAsync(identity.TenantReference, cancellationToken)).SingleOrDefault(item =>
            item.Reference == reference && item.PrincipalReference == identity.AspNetSubjectReference
            && item.ActorReference == identity.ActorReference);

    internal async Task<VatReviewApprovalRecord?> GetReviewApprovalForTenantAsync(string tenantReference,
        string reference, CancellationToken cancellationToken = default) =>
        (await ReadReviewApprovalsAsync(tenantReference, cancellationToken)).SingleOrDefault(item =>
            item.Reference == reference && item.TenantReference == tenantReference);

    internal async Task<IReadOnlyList<VatReviewApprovalRecord>> GetReviewApprovalsAsync(
        string tenantReference, IReadOnlySet<string> references,
        CancellationToken cancellationToken = default) =>
        (await ReadReviewApprovalsAsync(tenantReference, cancellationToken))
            .Where(item => references.Contains(item.Reference)).ToArray();

    internal async Task<IReadOnlyList<VatReviewApprovalRecord>> GetReviewApprovalsAsync(
        VatWorkflowIdentity identity, CancellationToken cancellationToken = default) =>
        (await ReadReviewApprovalsAsync(identity.TenantReference, cancellationToken))
            .Where(item => item.PrincipalReference == identity.AspNetSubjectReference
                && item.ActorReference == identity.ActorReference).ToArray();

    private async Task<SubmissionAttemptRecord> UpdateAttemptAsync(string tenantReference,
        string principalReference, string attemptReference, Func<SubmissionAttemptRecord, SubmissionAttemptRecord> update,
        CancellationToken cancellationToken) => await MutateAttemptsAsync(tenantReference, records =>
    {
        var index = records.FindIndex(item => item.PrincipalReference == principalReference
            && item.AttemptReference == attemptReference);
        if (index < 0) throw new KeyNotFoundException("No attempt exists for the supplied tenant and principal.");
        return records[index] = update(records[index]);
    }, cancellationToken);

    private async Task<T> MutateAttemptsAsync<T>(string tenantReference, Func<List<SubmissionAttemptRecord>, T> action,
        CancellationToken cancellationToken)
    {
        var owner = Fingerprint(tenantReference);
        await using var lease = await AcquireLeaseAsync(_container.GetBlobClient($"vat/v1/attempts/{owner}.lock"),
            createIfMissing: true, cancellationToken)
            ?? throw new InvalidOperationException("The VAT attempt store lease is unavailable.");
        var blob = _container.GetBlobClient($"vat/v1/attempts/{owner}.json");
        var records = await ReadAsync<List<SubmissionAttemptRecord>>(blob, null, cancellationToken) ?? [];
        var result = action(records);
        await WriteAsync(blob, JsonSerializer.SerializeToUtf8Bytes(records, Json), overwrite: true, cancellationToken);
        return result;
    }

    private async Task<List<SubmissionAttemptRecord>> ReadAttemptsAsync(string tenantReference,
        CancellationToken cancellationToken) =>
        await ReadAsync<List<SubmissionAttemptRecord>>(_container.GetBlobClient(
            $"vat/v1/attempts/{Fingerprint(tenantReference)}.json"), null, cancellationToken) ?? [];

    private async Task<T> MutateReviewCandidatesAsync<T>(string tenantReference,
        Func<List<VatReviewCandidateRecord>, T> action, CancellationToken cancellationToken)
    {
        var owner = Fingerprint(tenantReference);
        await using var lease = await AcquireLeaseAsync(
            _container.GetBlobClient($"vat/v1/reviews/{owner}.lock"), createIfMissing: true,
            cancellationToken) ?? throw new InvalidOperationException("The VAT review-store lease is unavailable.");
        var blob = _container.GetBlobClient($"vat/v1/reviews/{owner}-candidates.json");
        var records = await ReadAsync<List<VatReviewCandidateRecord>>(blob, null, cancellationToken) ?? [];
        var result = action(records);
        await WriteAsync(blob, JsonSerializer.SerializeToUtf8Bytes(records, Json), overwrite: true,
            cancellationToken);
        return result;
    }

    private async Task<T> MutateReviewApprovalsAsync<T>(string tenantReference,
        Func<List<VatReviewApprovalRecord>, T> action, CancellationToken cancellationToken)
    {
        var owner = Fingerprint(tenantReference);
        await using var lease = await AcquireLeaseAsync(
            _container.GetBlobClient($"vat/v1/reviews/{owner}.lock"), createIfMissing: true,
            cancellationToken) ?? throw new InvalidOperationException("The VAT review-store lease is unavailable.");
        var blob = _container.GetBlobClient($"vat/v1/reviews/{owner}-approvals.json");
        var records = await ReadAsync<List<VatReviewApprovalRecord>>(blob, null, cancellationToken) ?? [];
        var result = action(records);
        await WriteAsync(blob, JsonSerializer.SerializeToUtf8Bytes(records, Json), overwrite: true,
            cancellationToken);
        return result;
    }

    private async Task<List<VatReviewCandidateRecord>> ReadReviewCandidatesAsync(string tenantReference,
        CancellationToken cancellationToken) =>
        await ReadAsync<List<VatReviewCandidateRecord>>(_container.GetBlobClient(
            $"vat/v1/reviews/{Fingerprint(tenantReference)}-candidates.json"), null, cancellationToken) ?? [];

    private async Task<List<VatReviewApprovalRecord>> ReadReviewApprovalsAsync(string tenantReference,
        CancellationToken cancellationToken) =>
        await ReadAsync<List<VatReviewApprovalRecord>>(_container.GetBlobClient(
            $"vat/v1/reviews/{Fingerprint(tenantReference)}-approvals.json"), null, cancellationToken) ?? [];

    private async Task UploadNewAsync(string path, byte[] plaintext, CancellationToken cancellationToken)
    {
        var blob = _container.GetBlobClient(path);
        try
        {
            await blob.UploadAsync(BinaryData.FromBytes(_protector.Protect(plaintext)), new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            }, cancellationToken);
        }
        finally { CryptographicOperations.ZeroMemory(plaintext); }
    }

    private async Task WriteAsync(BlobClient blob, byte[] plaintext, bool overwrite,
        CancellationToken cancellationToken)
    {
        try
        {
            await blob.UploadAsync(BinaryData.FromBytes(_protector.Protect(plaintext)), overwrite,
                cancellationToken);
        }
        finally { CryptographicOperations.ZeroMemory(plaintext); }
    }

    private async Task<T?> ReadAsync<T>(BlobClient blob, string? leaseId,
        CancellationToken cancellationToken)
    {
        var bytes = await ReadBytesAsync(blob, leaseId, cancellationToken);
        if (bytes is null) return default;
        try { return JsonSerializer.Deserialize<T>(bytes, Json); }
        finally { CryptographicOperations.ZeroMemory(bytes); }
    }

    private async Task<byte[]?> ReadBytesAsync(BlobClient blob, string? leaseId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await blob.DownloadContentAsync(new BlobDownloadOptions
            {
                Conditions = leaseId is null ? null : new BlobRequestConditions { LeaseId = leaseId }
            }, cancellationToken);
            return _protector.Unprotect(response.Value.Content.ToArray());
        }
        catch (RequestFailedException exception) when (exception.Status == 404) { return null; }
        catch (CryptographicException) { throw new InvalidDataException("Protected VAT workflow data failed integrity validation."); }
    }

    private async Task<BlobLeaseHandle?> AcquireLeaseAsync(BlobClient blob, bool createIfMissing,
        CancellationToken cancellationToken)
    {
        if (createIfMissing)
        {
            try
            {
                await blob.UploadAsync(BinaryData.FromBytes([]), new BlobUploadOptions
                {
                    Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
                }, cancellationToken);
            }
            catch (RequestFailedException exception) when (exception.Status is 409 or 412) { }
        }
        var lease = blob.GetBlobLeaseClient();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var acquired = await lease.AcquireAsync(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);
                return new BlobLeaseHandle(lease, acquired.Value.LeaseId);
            }
            catch (RequestFailedException exception) when (!createIfMissing && exception.Status == 404) { return null; }
            catch (RequestFailedException exception) when (exception.Status is 409 or 412)
            {
                await Task.Delay(50, cancellationToken);
            }
        }
    }

    private static string PendingPath(string state) => $"vat/v1/oauth/pending/{Fingerprint(state)}";
    private static string GrantPath(string tenant, string principal) =>
        $"vat/v1/oauth/grants/{Fingerprint(tenant, principal)}";
    private static string FraudPath(SealedFraudContextReference reference, FraudActorIdentity identity) =>
        $"vat/v1/fraud/{Fingerprint(identity.TenantReference, identity.PrincipalReference)}/{reference.Value}";
    private static string Fingerprint(params string[] values) => Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes(string.Join('\n', values)))).ToLowerInvariant()[..24];
    private static bool FixedEquals(string left, string right)
    {
        var a = Encoding.UTF8.GetBytes(left ?? string.Empty);
        var b = Encoding.UTF8.GetBytes(right ?? string.Empty);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }

    private static bool CanTransition(SubmissionAttemptState current, SubmissionAttemptState next) => current switch
    {
        SubmissionAttemptState.Reserved => next is SubmissionAttemptState.Sending
            or SubmissionAttemptState.Succeeded or SubmissionAttemptState.Rejected
            or SubmissionAttemptState.Failed or SubmissionAttemptState.Unknown,
        SubmissionAttemptState.Sending or SubmissionAttemptState.Unknown => next is SubmissionAttemptState.Succeeded
            or SubmissionAttemptState.Rejected or SubmissionAttemptState.Failed or SubmissionAttemptState.Unknown,
        _ => false
    };

    public void Dispose() { }

    private sealed class BlobLeaseHandle(BlobLeaseClient lease, string leaseId) : IAsyncDisposable
    {
        public string LeaseId { get; } = leaseId;
        public async ValueTask DisposeAsync()
        {
            try { await lease.ReleaseAsync(); }
            catch (RequestFailedException exception) when (exception.Status is 404 or 409 or 412) { }
        }
    }
}
