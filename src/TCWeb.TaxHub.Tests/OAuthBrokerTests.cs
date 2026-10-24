using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;
using TradeControl.Web.AppServices.TaxHub.Vat;
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
            { Environment = "production" }, options, now), "invalid",
            "A handoff for the wrong authority environment was accepted.", ref assertions);
        AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
            { ExpiresAtUtc = now }, options, now), "invalid",
            "A handoff at its expiry boundary was accepted.", ref assertions);
        AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
            { IssuedAtUtc = now.AddMinutes(-11) }, options, now), "invalid",
            "A handoff exceeding the maximum lifetime was accepted.", ref assertions);
        AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
            { ReturnOrigin = "https://attacker.example/" }, options, now), "not approved",
            "An unapproved tenant return host was accepted.", ref assertions);
        foreach (var invalidReturnOrigin in new[]
        {
            "http://business1.tradecontrol.co.uk/",
            "https://user@business1.tradecontrol.co.uk/",
            "https://business1.tradecontrol.co.uk:444/",
            "https://127.0.0.1/",
            "https://business1.tradecontrol.co.uk/path",
            "https://business1.tradecontrol.co.uk/?return=https://attacker.example/",
            "https://business1.tradecontrol.co.uk/#attacker",
            "https://business1.tradecontrol.co.uk.attacker.example/",
            "https://businessı.tradecontrol.co.uk/"
        })
            AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
                { ReturnOrigin = invalidReturnOrigin }, options, now), "not approved",
                $"The unsafe return origin '{invalidReturnOrigin}' was accepted.", ref assertions);
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
        AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
            {
                AuthorisationUri = authority.Replace(Uri.EscapeDataString(options.CallbackUri.AbsoluteUri),
                    Uri.EscapeDataString("https://attacker.example/TaxHub/HmrcCallback"),
                    StringComparison.Ordinal)
            }, options, now), "does not match", "A substituted authority callback was accepted.", ref assertions);
        AssertRejected(() => OAuthBrokerEndpoints.ValidateHandoff(handoff with
            { AuthorisationUri = authority.Replace(state, state + "x", StringComparison.Ordinal) },
            options, now), "does not match", "An authorisation request with altered state was accepted.", ref assertions);

        var key = Enumerable.Range(80, 32).Select(value => (byte)value).ToArray();
        using var protector = new OAuthBrokerTicketProtector(key);
        CryptographicOperations.ZeroMemory(key);

        var tamperedContext = Context();
        var protectedHandoff = protector.Protect(handoff, OAuthBrokerContract.HandoffPurpose);
        var tampered = await OAuthBrokerEndpoints.BeginAsync(Tamper(protectedHandoff), tamperedContext,
            protector, new MemoryStateStore(), Options.Create(options), clock, CancellationToken.None);
        await tampered.ExecuteAsync(tamperedContext);
        Assert(tamperedContext.Response.StatusCode == StatusCodes.Status400BadRequest,
            "An altered handoff ticket was accepted.");

        var store = new MemoryStateStore();
        var beginContext = Context();
        var begin = await OAuthBrokerEndpoints.BeginAsync(
            protectedHandoff, beginContext,
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

        var wrongCookie = Context();
        wrongCookie.Request.Headers.Cookie = $"{OAuthBrokerContract.CorrelationCookie}=substituted-browser";
        var wrongCookieResult = await OAuthBrokerEndpoints.CompleteAsync(state, "code", null, wrongCookie,
            protector, store, clock, CancellationToken.None);
        await wrongCookieResult.ExecuteAsync(wrongCookie);
        Assert(wrongCookie.Response.StatusCode == StatusCodes.Status400BadRequest && store.Pending is not null,
            "A callback with a substituted browser correlation consumed broker state.");

        var alteredState = Context();
        alteredState.Request.Headers.Cookie = $"{OAuthBrokerContract.CorrelationCookie}={correlation}";
        var alteredStateResult = await OAuthBrokerEndpoints.CompleteAsync(state + "x", "code", null,
            alteredState, protector, store, clock, CancellationToken.None);
        await alteredStateResult.ExecuteAsync(alteredState);
        Assert(alteredState.Response.StatusCode == StatusCodes.Status400BadRequest && store.Pending is not null,
            "A callback with altered state consumed the valid pending journey.");

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

        var expiredStore = new MemoryStateStore();
        await expiredStore.CreateAsync(new(handoff with { ExpiresAtUtc = now },
            OAuthBrokerTicketProtector.Fingerprint(correlation)), CancellationToken.None);
        var expiredContext = Context();
        expiredContext.Request.Headers.Cookie = $"{OAuthBrokerContract.CorrelationCookie}={correlation}";
        var expired = await OAuthBrokerEndpoints.CompleteAsync(state, "code", null, expiredContext,
            protector, expiredStore, clock, CancellationToken.None);
        await expired.ExecuteAsync(expiredContext);
        Assert(expiredContext.Response.StatusCode == StatusCodes.Status400BadRequest
            && expiredStore.Pending is null, "Expired broker state was accepted or retained.");

        var concurrentStore = new MemoryStateStore();
        await concurrentStore.CreateAsync(new(handoff,
            OAuthBrokerTicketProtector.Fingerprint(correlation)), CancellationToken.None);
        var firstContext = Context();
        var secondContext = Context();
        firstContext.Request.Headers.Cookie = $"{OAuthBrokerContract.CorrelationCookie}={correlation}";
        secondContext.Request.Headers.Cookie = $"{OAuthBrokerContract.CorrelationCookie}={correlation}";
        var concurrentResults = await Task.WhenAll(
            CompleteAndExecuteAsync(firstContext, state, protector, concurrentStore, clock),
            CompleteAndExecuteAsync(secondContext, state, protector, concurrentStore, clock));
        Assert(concurrentResults.Count(status => status == StatusCodes.Status302Found) == 1
            && concurrentResults.Count(status => status == StatusCodes.Status400BadRequest) == 1,
            "Concurrent callbacks did not produce exactly one completion and one rejection.");

        var completion = new OAuthBrokerCompletion("sandbox", "tenant-a", "principal-a", "actor-a",
            state, "code", null, "https://business1.tradecontrol.co.uk/", now, now.AddMinutes(2));
        VatHmrcConnectionService.ValidateBrokerCompletion(completion, "sandbox", "tenant-a",
            "principal-a", "actor-a", "https://business1.tradecontrol.co.uk/", now);
        assertions++;
        AssertCompletionRejected(completion with { Environment = "production" }, "sandbox", "tenant-a",
            "principal-a", "actor-a", "https://business1.tradecontrol.co.uk/", now,
            "A completion for the wrong authority environment was accepted.", ref assertions);
        AssertCompletionRejected(completion with { TenantReference = "tenant-b" }, "sandbox", "tenant-a",
            "principal-a", "actor-a", "https://business1.tradecontrol.co.uk/", now,
            "A cross-tenant completion was accepted.", ref assertions);
        AssertCompletionRejected(completion with { PrincipalReference = "principal-b" }, "sandbox", "tenant-a",
            "principal-a", "actor-a", "https://business1.tradecontrol.co.uk/", now,
            "A completion for a different signed-in principal was accepted.", ref assertions);
        AssertCompletionRejected(completion with { ActorReference = "actor-b" }, "sandbox", "tenant-a",
            "principal-a", "actor-a", "https://business1.tradecontrol.co.uk/", now,
            "A completion for a different domain actor was accepted.", ref assertions);
        AssertCompletionRejected(completion with { ReturnOrigin = "https://business2.tradecontrol.co.uk/" },
            "sandbox", "tenant-a", "principal-a", "actor-a",
            "https://business1.tradecontrol.co.uk/", now,
            "A completion captured for another tenant origin was accepted.", ref assertions);
        AssertCompletionRejected(completion with { ExpiresAtUtc = now }, "sandbox", "tenant-a",
            "principal-a", "actor-a", "https://business1.tradecontrol.co.uk/", now,
            "A completion at its expiry boundary was accepted.", ref assertions);
        return assertions;
    }

    private static async Task<int> CompleteAndExecuteAsync(DefaultHttpContext context, string state,
        OAuthBrokerTicketProtector protector, IOAuthBrokerStateStore store, TimeProvider clock)
    {
        var result = await OAuthBrokerEndpoints.CompleteAsync(state, "code", null, context,
            protector, store, clock, CancellationToken.None);
        await result.ExecuteAsync(context);
        return context.Response.StatusCode;
    }

    private static string Tamper(string value)
    {
        var characters = value.ToCharArray();
        var index = characters.Length / 2;
        characters[index] = characters[index] == 'A' ? 'B' : 'A';
        return new string(characters);
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

    private static void AssertCompletionRejected(OAuthBrokerCompletion completion, string environment,
        string tenantReference, string principalReference, string actorReference, string expectedOrigin,
        DateTimeOffset now, string failure, ref int assertions)
    {
        assertions++;
        try
        {
            VatHmrcConnectionService.ValidateBrokerCompletion(completion, environment, tenantReference,
                principalReference, actorReference, expectedOrigin, now);
        }
        catch (OAuthCallbackValidationException)
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
        private readonly object _sync = new();
        public OAuthBrokerPendingState? Pending { get; private set; }

        public Task CreateAsync(OAuthBrokerPendingState pending, CancellationToken cancellationToken)
        {
            lock (_sync)
            {
                if (Pending is not null) throw new InvalidOperationException("duplicate");
                Pending = pending;
            }
            return Task.CompletedTask;
        }

        public Task<OAuthBrokerPendingState?> ConsumeAsync(string state, string correlation,
            DateTimeOffset now, CancellationToken cancellationToken)
        {
            lock (_sync)
            {
                if (Pending is null || Pending.Handoff.State != state
                    || Pending.CorrelationFingerprint != OAuthBrokerTicketProtector.Fingerprint(correlation))
                    return Task.FromResult<OAuthBrokerPendingState?>(null);
                var value = Pending;
                Pending = null;
                return Task.FromResult<OAuthBrokerPendingState?>(value.Handoff.ExpiresAtUtc > now
                    ? value : null);
            }
        }

        public Task ProbeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
