using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.Configuration;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;
using TradeControl.Tax.UK.Application.Preparation;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public interface IVatHmrcConnectionService
{
    Task<VatConnectionStatus> GetStatusAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task<Uri> BeginConnectAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task<VatConnectionStatus> CompleteCallbackAsync(ClaimsPrincipal principal, string state,
        string? code, string? error, CancellationToken cancellationToken = default);
    Task DisconnectAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public sealed class VatHmrcConnectionService : IVatHmrcConnectionService, IDisposable
{
    private readonly IVatWorkflowIdentityAccessor _identities;
    private readonly VatProductHostOptions _host;
    private readonly HmrcOAuthService? _oauth;
    private readonly HmrcOAuthTokenEndpoint? _tokens;

    public VatHmrcConnectionService(IVatWorkflowIdentityAccessor identities,
        IOptions<VatProductHostOptions> hostOptions)
    {
        _identities = identities;
        _host = hostOptions.Value;
        if (!_host.Enabled) return;
        if (_host.PersistenceMode != VatPersistenceMode.DevelopmentFiles)
            throw new InvalidOperationException("The configured VAT persistence facility is not implemented.");

        var root = Path.GetFullPath(_host.DevelopmentStoreRoot!);
        Directory.CreateDirectory(root);
        var key = LoadOrCreateKey(Path.Combine(root, "oauth-store.key"));
        try
        {
            ISecretProvider secrets = _host.SandboxSecretSource switch
            {
                VatSandboxSecretSource.DevelopmentJsonFile => new JsonFileSecretProvider(
                    _host.DevelopmentClientSettingsPath!, HmrcSecretReferences.LegacyVatSandboxJsonProperties),
                VatSandboxSecretSource.EnvironmentVariables => new EnvironmentSecretProvider(
                    HmrcSecretReferences.AppServiceEnvironmentVariables),
                _ => throw new InvalidOperationException("The sandbox secret source is not supported.")
            };
            var environment = EnvironmentSelector.Sandbox();
            _tokens = new HmrcOAuthTokenEndpoint(environment);
            var callback = new Uri(_host.PublicOrigin!, _host.OAuthCallbackPath);
            _oauth = HmrcOAuthService.CreateFileBackedSandbox(Path.Combine(root, "oauth-grants.json.enc"),
                key, secrets, _tokens, options: new HmrcOAuthOptions(callback, TimeSpan.FromMinutes(10),
                    TimeSpan.FromMinutes(5), 18));
        }
        finally { CryptographicOperations.ZeroMemory(key); }
    }

    public async Task<VatConnectionStatus> GetStatusAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (_oauth is null) return new(VatConnectionState.Unavailable, "VAT-HOST-DISABLED");
        var context = await ContextAsync(principal, cancellationToken);
        using var read = await _oauth.GetAccessAsync(context, HmrcOAuthScopes.ReadVat, cancellationToken);
        if (read.Kind != OAuthAccessOutcomeKind.Available)
            return Map(read.ReauthorisationReason);
        using var write = await _oauth.GetAccessAsync(context, HmrcOAuthScopes.WriteVat, cancellationToken);
        return write.Kind == OAuthAccessOutcomeKind.Available
            ? new(VatConnectionState.Connected)
            : Map(write.ReauthorisationReason);
    }

    public async Task<Uri> BeginConnectAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var oauth = _oauth ?? throw new InvalidOperationException("The VAT host is unavailable.");
        var start = await oauth.BeginAuthorisationAsync(await ContextAsync(principal, cancellationToken),
            HmrcOAuthScopes.VatReadWrite, cancellationToken);
        return start.AuthorisationUri;
    }

    public async Task<VatConnectionStatus> CompleteCallbackAsync(ClaimsPrincipal principal, string state,
        string? code, string? error, CancellationToken cancellationToken = default)
    {
        var oauth = _oauth ?? throw new InvalidOperationException("The VAT host is unavailable.");
        using var outcome = await oauth.CompleteCallbackAsync(await ContextAsync(principal, cancellationToken),
            new OAuthCallback(state, code, error), cancellationToken);
        return outcome.Kind == OAuthAccessOutcomeKind.Available
            ? new(VatConnectionState.Connected)
            : Map(outcome.ReauthorisationReason);
    }

    public async Task DisconnectAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var oauth = _oauth ?? throw new InvalidOperationException("The VAT host is unavailable.");
        var context = await ContextAsync(principal, cancellationToken);
        await oauth.RevokeAsync(context, HmrcOAuthScopes.ReadVat, cancellationToken);
        await oauth.RevokeAsync(context, HmrcOAuthScopes.WriteVat, cancellationToken);
    }

    private async Task<AuthorityDispatchContext> ContextAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var identity = await _identities.GetRequiredAsync(principal, cancellationToken);
        return new AuthorityDispatchContext(identity.TenantReference, identity.AspNetSubjectReference,
            identity.ActorReference, "hmrc-connection", "client-facts-not-required");
    }

    private static VatConnectionStatus Map(OAuthReauthorisationReason? reason) => reason switch
    {
        OAuthReauthorisationReason.MissingGrant => new(VatConnectionState.NotConnected),
        OAuthReauthorisationReason.RevokedGrant => new(VatConnectionState.Disconnected),
        _ => new(VatConnectionState.ReauthorisationRequired, reason?.ToString())
    };

    private static byte[] LoadOrCreateKey(string path)
    {
        if (File.Exists(path))
        {
            var existing = File.ReadAllBytes(path);
            if (existing.Length != 32) throw new InvalidOperationException("The OAuth store key is invalid.");
            return existing;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var key = RandomNumberGenerator.GetBytes(32);
        try
        {
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            stream.Write(key);
            stream.Flush(true);
            return key;
        }
        catch (IOException) when (File.Exists(path))
        {
            CryptographicOperations.ZeroMemory(key);
            var existing = File.ReadAllBytes(path);
            if (existing.Length != 32) throw new InvalidOperationException("The OAuth store key is invalid.");
            return existing;
        }
    }

    public void Dispose()
    {
        _oauth?.Dispose();
        _tokens?.Dispose();
    }
}
