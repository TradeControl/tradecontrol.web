using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;

namespace TradeControl.Web.OAuthBroker;

public static class OAuthBrokerEndpoints
{
    public const string RateLimitPolicy = "oauth-broker-entry";

    public static IEndpointRouteBuilder MapOAuthBroker(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/oauth/start", BeginAsync).AllowAnonymous().RequireRateLimiting(RateLimitPolicy);
        endpoints.MapGet("/TaxHub/HmrcCallback", CompleteAsync).AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicy);
        return endpoints;
    }

    internal static async Task<IResult> BeginAsync(string? handoff, HttpContext context,
        OAuthBrokerTicketProtector protector, IOAuthBrokerStateStore store,
        IOptions<OAuthBrokerOptions> configured, TimeProvider clock,
        CancellationToken cancellationToken)
    {
        OAuthBrokerHandoff value;
        try
        {
            value = protector.Unprotect<OAuthBrokerHandoff>(handoff!, OAuthBrokerContract.HandoffPurpose);
            ValidateHandoff(value, configured.Value, clock.GetUtcNow());
        }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException
            or InvalidOperationException or UriFormatException)
        {
            return Results.BadRequest("The OAuth handoff is invalid or expired.");
        }
        var correlation = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        try
        {
            await store.CreateAsync(new(value, OAuthBrokerTicketProtector.Fingerprint(correlation)),
                cancellationToken);
        }
        catch (InvalidOperationException) { return Results.BadRequest("The OAuth handoff cannot be reused."); }
        context.Response.Cookies.Append(OAuthBrokerContract.CorrelationCookie, correlation, new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, IsEssential = true, Path = "/",
            MaxAge = value.ExpiresAtUtc - clock.GetUtcNow()
        });
        return Results.Redirect(value.AuthorisationUri);
    }

    internal static async Task<IResult> CompleteAsync(string? state, string? code, string? error,
        HttpContext context, OAuthBrokerTicketProtector protector, IOAuthBrokerStateStore store,
        TimeProvider clock, CancellationToken cancellationToken)
    {
        if (!ValidCallbackValues(state, code, error)
            || !context.Request.Cookies.TryGetValue(OAuthBrokerContract.CorrelationCookie,
                out var correlation) || string.IsNullOrWhiteSpace(correlation))
            return Results.BadRequest("The OAuth callback is invalid or expired.");
        OAuthBrokerPendingState? pending;
        try { pending = await store.ConsumeAsync(state!, correlation, clock.GetUtcNow(), cancellationToken); }
        catch (CryptographicException) { return Results.BadRequest("The OAuth callback is invalid or expired."); }
        context.Response.Cookies.Delete(OAuthBrokerContract.CorrelationCookie,
            new CookieOptions { Secure = true, HttpOnly = true, SameSite = SameSiteMode.Lax, Path = "/" });
        if (pending is null) return Results.BadRequest("The OAuth callback is invalid or expired.");

        var now = clock.GetUtcNow();
        var completion = new OAuthBrokerCompletion(pending.Handoff.Environment,
            pending.Handoff.TenantReference, pending.Handoff.PrincipalReference,
            pending.Handoff.ActorReference, state!, code, error, pending.Handoff.ReturnOrigin,
            now, now + OAuthBrokerContract.MaximumCompletionLifetime);
        var ticket = protector.Protect(completion, OAuthBrokerContract.CompletionPurpose);
        var returnUri = new Uri(new Uri(pending.Handoff.ReturnOrigin), OAuthBrokerContract.CompletionPath);
        return Results.Redirect(QueryHelpers.AddQueryString(returnUri.AbsoluteUri, "ticket", ticket));
    }

    internal static void ValidateHandoff(OAuthBrokerHandoff handoff, OAuthBrokerOptions options,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(handoff);
        if (!handoff.Environment.Equals(options.AuthorityEnvironment, StringComparison.Ordinal)
            || handoff.IssuedAtUtc > now + TimeSpan.FromMinutes(1) || handoff.ExpiresAtUtc <= now
            || handoff.ExpiresAtUtc - handoff.IssuedAtUtc > OAuthBrokerContract.MaximumHandoffLifetime
            || handoff.TenantReference.Length is < 1 or > 128
            || handoff.PrincipalReference.Length is < 1 or > 256
            || handoff.ActorReference.Length is < 1 or > 128
            || handoff.State.Length is < 32 or > 512)
            throw new InvalidOperationException("The OAuth handoff is invalid.");
        var returnOrigin = new Uri(handoff.ReturnOrigin, UriKind.Absolute);
        if (!OAuthBrokerOptionsValidator.IsHttpsOrigin(returnOrigin)
            || !options.AllowedReturnHosts.Contains(returnOrigin.IdnHost, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("The tenant return origin is not approved.");
        var authorisation = new Uri(handoff.AuthorisationUri, UriKind.Absolute);
        var expectedAuthority = options.AuthorityEnvironment == "production"
            ? "www.tax.service.gov.uk" : "test-www.tax.service.gov.uk";
        if (authorisation.Scheme != Uri.UriSchemeHttps || !authorisation.IsDefaultPort
            || !authorisation.IdnHost.Equals(expectedAuthority, StringComparison.OrdinalIgnoreCase)
            || authorisation.AbsolutePath != "/oauth/authorize"
            || !string.IsNullOrEmpty(authorisation.UserInfo)
            || !string.IsNullOrEmpty(authorisation.Fragment))
            throw new InvalidOperationException("The authority endpoint is not approved.");
        var query = QueryHelpers.ParseQuery(authorisation.Query);
        if (!query.TryGetValue("state", out var requestState) || requestState.Count != 1
            || requestState[0] != handoff.State
            || !query.TryGetValue("response_type", out var responseType) || responseType.Count != 1
            || responseType[0] != "code"
            || !query.TryGetValue("client_id", out var clientId) || clientId.Count != 1
            || string.IsNullOrWhiteSpace(clientId[0])
            || !query.TryGetValue("scope", out var scope) || scope.Count != 1
            || scope[0] != HmrcOAuthScopes.VatReadWrite
            || !query.TryGetValue("redirect_uri", out var callback) || callback.Count != 1
            || callback[0] != options.CallbackUri.AbsoluteUri
            || !query.TryGetValue("code_challenge", out var challenge) || challenge.Count != 1
            || string.IsNullOrWhiteSpace(challenge[0])
            || !query.TryGetValue("code_challenge_method", out var method) || method.Count != 1
            || method[0] != "S256")
            throw new InvalidOperationException("The authority request does not match the broker contract.");
    }

    private static bool ValidCallbackValues(string? state, string? code, string? error) =>
        state is { Length: >= 32 and <= 512 }
        && ((code is { Length: >= 1 and <= 4096 } && string.IsNullOrEmpty(error))
            || (error is { Length: >= 1 and <= 256 } && string.IsNullOrEmpty(code)));
}
