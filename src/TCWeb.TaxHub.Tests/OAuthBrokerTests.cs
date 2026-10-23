using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;
using TradeControl.Web.OAuthBroker;

internal static class OAuthBrokerTests
{
    public static async Task<int> RunAsync()
    {
        var assertions = 0;
        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        var options = new OAuthBrokerOptions
        {
            AuthorityEnvironment = "sandbox",
            PublicOrigin = new Uri("https://auth.tradecontrol.co.uk/"),
            BlobServiceUri = new Uri("https://example.blob.core.windows.net/"),
            AllowedReturnHosts = ["business1.tradecontrol.co.uk", "business2.tradecontrol.co.uk"]
        };
        Assert(new OAuthBrokerOptionsValidator().Validate(null, options).Succeeded,
            "A valid, closed broker configuration was rejected.");
        options.AllowedReturnHosts = ["business1.tradecontrol.co.uk", "BUSINESS1.tradecontrol.co.uk"];
        Assert(new OAuthBrokerOptionsValidator().Validate(null, options).Failed,
            "Duplicate return hosts were accepted by the broker configuration.");
        options.AllowedReturnHosts = ["business1.tradecontrol.co.uk", "business2.tradecontrol.co.uk"];

        var now = new DateTimeOffset(2026, 10, 23, 14, 0, 0, TimeSpan.Zero);
        var clock = new FixedTimeProvider(now);
        const string state = "state-0123456789012345678901234567890123456789";
        var callback = Uri.EscapeDataString("https://auth.tradecontrol.co.uk/TaxHub/HmrcCallback");
        var authority = $"https://test-www.tax.service.gov.uk/oauth/authorize?response_type=code&client_id=test&scope=read%3Avat%20write%3Avat&redirect_uri={callback}&state={state}&code_challenge=challenge012345678901234567890123456789&code_challenge_method=S256";
        var handoff = new OAuthBrokerHandoff("sandbox", "tenant-a", "principal-a", "actor-a",
            authority, state, "https://business1.tradecontrol.co.uk/", now, now.AddMinutes(5));
        OAuthBrokerEndpoints.ValidateHandoff(handoff, options, now);
        assertions++;
        AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
            { ReturnOrigin = "https://attacker.example/" }, options, now), "not approved",
            "An unapproved tenant return host was accepted.", ref assertions);
        AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
            {
                AuthorisationUri = authority.Replace("test-www.tax.service.gov.uk", "attacker.example",
                    StringComparison.Ordinal)
            }, options, now), "authority endpoint", "An unapproved authority host was accepted.", ref assertions);
        AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
            {
                AuthorisationUri = authority.Replace("read%3Avat%20write%3Avat", "read%3Avat",
                    StringComparison.Ordinal)
            }, options, now), "does not match", "A reduced or substituted VAT consent scope was accepted.", ref assertions);

        var key = Enumerable.Range(80, 32).Select(value => (byte)value).ToArray();
        using var protector = new OAuthBrokerTicketProtector(key);
        CryptographicOperations.ZeroMemory(key);
        var store = new MemoryStateStore();
        var beginContext = Context();
        var begin = await OAuthBrokerEndpoints.BeginAsync(
            protector.Protect(handoff, OAuthBrokerContract.HandoffPurpose), beginContext,
            protector, store, Options.Create(options), clock, CancellationToken.None);
        await begin.ExecuteAsync(beginContext);
        var setCookie = beginContext.Response.Headers.SetCookie.ToString();
        var correlation = setCookie.Split(';', 2)[0].Split('=', 2)[1];
        Assert(beginContext.Response.StatusCode == StatusCodes.Status302Found
            && beginContext.Response.Headers.Location == authority
            && setCookie.Contains(OAuthBrokerContract.CorrelationCookie, StringComparison.Ordinal)
            && setCookie.Contains("httponly", StringComparison.OrdinalIgnoreCase)
            && setCookie.Contains("secure", StringComparison.OrdinalIgnoreCase)
            && setCookie.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase),
            "A valid handoff did not create the secure broker correlation and HMRC redirect.");

        var wrongBrowser = Context();
        var rejected = await OAuthBrokerEndpoints.CompleteAsync(state, "code", null, wrongBrowser,
            protector, store, clock, CancellationToken.None);
        await rejected.ExecuteAsync(wrongBrowser);
        Assert(wrongBrowser.Response.StatusCode == StatusCodes.Status400BadRequest && store.Pending is not null,
            "A callback without the initiating browser correlation consumed broker state.");

        var completeContext = Context();
        completeContext.Request.Headers.Cookie = $"{OAuthBrokerContract.CorrelationCookie}={correlation}";
        var completed = await OAuthBrokerEndpoints.CompleteAsync(state, "code", null, completeContext,
            protector, store, clock, CancellationToken.None);
        await completed.ExecuteAsync(completeContext);
        Assert(completeContext.Response.StatusCode == StatusCodes.Status302Found
            && completeContext.Response.Headers.Location.ToString().StartsWith(
                "https://business1.tradecontrol.co.uk/TaxHub/HmrcBrokerComplete?ticket=", StringComparison.Ordinal)
            && store.Pending is null,
            "A correlated callback did not atomically return to the captured tenant origin.");

        var replayContext = Context();
        replayContext.Request.Headers.Cookie = $"{OAuthBrokerContract.CorrelationCookie}={correlation}";
        var replay = await OAuthBrokerEndpoints.CompleteAsync(state, "code", null, replayContext,
            protector, store, clock, CancellationToken.None);
        await replay.ExecuteAsync(replayContext);
        Assert(replayContext.Response.StatusCode == StatusCodes.Status400BadRequest,
            "A consumed broker callback was replayed.");
        return assertions;
    }

    private static DefaultHttpContext Context()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();
        return context;
    }

    private static void AssertRejected(Action action, string safeMessage, string failure, ref int assertions)
    {
        assertions++;
        try { action(); }
        catch (InvalidOperationException exception) when (exception.Message.Contains(safeMessage, StringComparison.Ordinal))
        {
            return;
        }
        throw new InvalidOperationException(failure);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class MemoryStateStore : IOAuthBrokerStateStore
    {
        public OAuthBrokerPendingState? Pending { get; private set; }

        public Task CreateAsync(OAuthBrokerPendingState pending, CancellationToken cancellationToken)
        {
            if (Pending is not null) throw new InvalidOperationException("duplicate");
            Pending = pending;
            return Task.CompletedTask;
        }

        public Task<OAuthBrokerPendingState?> ConsumeAsync(string state, string correlation,
            DateTimeOffset now, CancellationToken cancellationToken)
        {
            if (Pending is null || Pending.Handoff.State != state
                || Pending.CorrelationFingerprint != OAuthBrokerTicketProtector.Fingerprint(correlation))
                return Task.FromResult<OAuthBrokerPendingState?>(null);
            var value = Pending;
            Pending = null;
            return Task.FromResult<OAuthBrokerPendingState?>(value);
        }

        public Task ProbeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
