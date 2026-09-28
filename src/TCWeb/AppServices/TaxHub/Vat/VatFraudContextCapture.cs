using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.FraudPrevention;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public sealed record VatBrowserScreen(int Width, int Height, decimal ScalingFactor, int ColourDepth);
public sealed record VatBrowserWindow(int Width, int Height);
public sealed record VatBrowserFacts(string JavascriptUserAgent, Guid DeviceId,
    IReadOnlyList<VatBrowserScreen> Screens, string Timezone, VatBrowserWindow WindowSize);

public interface IVatFraudContextCapture
{
    Task CaptureAsync(ClaimsPrincipal principal, VatBrowserFacts browser, IPAddress remoteAddress,
        int remotePort, string? forwardedFor, CancellationToken cancellationToken = default);
}

public sealed class VatFraudContextCapture : IVatFraudContextCapture, IDisposable
{
    private readonly IVatWorkflowIdentityAccessor _identities;
    private readonly VatProductHostOptions _host;
    private readonly FraudHeaderService? _fraud;
    private readonly HashSet<IPAddress> _trustedPeers;

    public VatFraudContextCapture(IVatWorkflowIdentityAccessor identities,
        IOptions<VatProductHostOptions> options)
    {
        _identities = identities;
        _host = options.Value;
        if (!_host.Enabled || !_host.FraudCaptureEnabled)
        {
            _trustedPeers = [];
            return;
        }
        _trustedPeers = _host.FraudTrustedProxyAddresses.Select(IPAddress.Parse).ToHashSet();

        var hops = _host.FraudPublicTlsAddresses.Select(IPAddress.Parse).ToArray();
        var topology = _trustedPeers.Count == 0
            ? FraudDeploymentTopology.Direct("tcweb-direct", hops.Single())
            : FraudDeploymentTopology.TrustedProxyChain("tcweb-trusted-proxy", _trustedPeers, hops);
        var root = Path.Combine(Path.GetFullPath(_host.DevelopmentStoreRoot!), "fraud-contexts");
        var key = LoadOrCreateKey(Path.Combine(Path.GetFullPath(_host.DevelopmentStoreRoot!), "fraud-store.key"));
        try
        {
            _fraud = FraudHeaderService.CreateFileBacked(root, key, topology,
                new FraudVendorConfiguration("Trade Control",
                    new Dictionary<string, string> { ["tcweb"] = "2.0.2" },
                    new Dictionary<string, string>()));
        }
        finally { CryptographicOperations.ZeroMemory(key); }
    }

    public async Task CaptureAsync(ClaimsPrincipal principal, VatBrowserFacts browser,
        IPAddress remoteAddress, int remotePort, string? forwardedFor,
        CancellationToken cancellationToken = default)
    {
        var fraud = _fraud ?? throw new InvalidOperationException("The VAT host is unavailable.");
        var identity = await _identities.GetRequiredAsync(principal, cancellationToken);
        ForwardedClientEndpoint? forwarded = null;
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            if (!_trustedPeers.Contains(remoteAddress))
                throw new UnauthorizedAccessException("Forwarded client data came from an untrusted peer.");
            forwarded = ParseForwardedEndpoint(forwardedFor.Split(',')[0].Trim());
        }
        var facts = new BrowserFraudFacts(browser.JavascriptUserAgent, browser.DeviceId,
            [], browser.Screens.Select(screen => new FraudScreen(screen.Width, screen.Height,
                screen.ScalingFactor, screen.ColourDepth)).ToArray(), browser.Timezone,
            new Dictionary<string, string> { ["TradeControl"] = identity.ActorReference },
            new FraudWindowSize(browser.WindowSize.Width, browser.WindowSize.Height));
        await fraud.CaptureAndSealAsync(new FraudActorIdentity(identity.TenantReference,
                identity.AspNetSubjectReference, identity.ActorReference), facts,
            new TrustedIngressConnectionObservation(remoteAddress, remotePort,
                DateTimeOffset.UtcNow, forwarded), cancellationToken);
    }

    private static ForwardedClientEndpoint ParseForwardedEndpoint(string value)
    {
        if (IPEndPoint.TryParse(value, out var endpoint) && endpoint is not null)
            return new(endpoint.Address, endpoint.Port);
        throw new ArgumentException("The trusted forwarded client endpoint is invalid.");
    }

    private static byte[] LoadOrCreateKey(string path)
    {
        if (File.Exists(path))
        {
            var existing = File.ReadAllBytes(path);
            if (existing.Length != 32) throw new InvalidOperationException("The fraud store key is invalid.");
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
            if (existing.Length != 32) throw new InvalidOperationException("The fraud store key is invalid.");
            return existing;
        }
    }

    public void Dispose() => _fraud?.Dispose();
}
