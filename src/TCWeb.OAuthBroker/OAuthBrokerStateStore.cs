using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;

namespace TradeControl.Web.OAuthBroker;

public sealed record OAuthBrokerPendingState(OAuthBrokerHandoff Handoff, string CorrelationFingerprint);

public interface IOAuthBrokerStateStore
{
    Task CreateAsync(OAuthBrokerPendingState pending, CancellationToken cancellationToken);
    Task<OAuthBrokerPendingState?> ConsumeAsync(string state, string correlation,
        DateTimeOffset now, CancellationToken cancellationToken);
    Task ProbeAsync(CancellationToken cancellationToken);
}

public sealed class AzureOAuthBrokerStateStore(
    BlobContainerClient container,
    OAuthBrokerTicketProtector protector) : IOAuthBrokerStateStore
{
    public async Task CreateAsync(OAuthBrokerPendingState pending, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pending);
        var blob = container.GetBlobClient(BlobName(pending.Handoff.State));
        var protectedState = protector.Protect(pending, OAuthBrokerContract.HandoffPurpose + ".state");
        try
        {
            await blob.UploadAsync(BinaryData.FromString(protectedState), new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
                HttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" }
            }, cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status is 409 or 412)
        {
            throw new InvalidOperationException("The OAuth state has already been registered.", exception);
        }
    }

    public async Task<OAuthBrokerPendingState?> ConsumeAsync(string state, string correlation,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlation);
        var blob = container.GetBlobClient(BlobName(state));
        Response<BlobDownloadResult> response;
        try { response = await blob.DownloadContentAsync(cancellationToken); }
        catch (RequestFailedException exception) when (exception.Status == 404) { return null; }

        OAuthBrokerPendingState pending;
        try
        {
            pending = protector.Unprotect<OAuthBrokerPendingState>(response.Value.Content.ToString(),
                OAuthBrokerContract.HandoffPurpose + ".state");
        }
        catch
        {
            await TryDeleteAsync(blob, response.Value.Details.ETag, cancellationToken);
            throw;
        }
        var valid = pending.Handoff.State.Equals(state, StringComparison.Ordinal)
            && pending.CorrelationFingerprint.Equals(OAuthBrokerTicketProtector.Fingerprint(correlation),
                StringComparison.Ordinal)
            && pending.Handoff.ExpiresAtUtc > now;
        if (!await TryDeleteAsync(blob, response.Value.Details.ETag, cancellationToken)) return null;
        return valid ? pending : null;
    }

    public async Task ProbeAsync(CancellationToken cancellationToken)
    {
        _ = await container.GetPropertiesAsync(cancellationToken: cancellationToken);
    }

    private static async Task<bool> TryDeleteAsync(BlobClient blob, ETag etag,
        CancellationToken cancellationToken)
    {
        try
        {
            await blob.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots,
                new BlobRequestConditions { IfMatch = etag }, cancellationToken);
            return true;
        }
        catch (RequestFailedException exception) when (exception.Status is 404 or 412) { return false; }
    }

    private static string BlobName(string state) =>
        $"pending/{OAuthBrokerTicketProtector.Fingerprint(state)}";
}
