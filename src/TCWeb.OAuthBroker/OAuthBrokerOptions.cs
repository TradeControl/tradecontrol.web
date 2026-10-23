using Microsoft.Extensions.Options;

namespace TradeControl.Web.OAuthBroker;

public sealed class OAuthBrokerOptions
{
    public const string SectionName = "OAuthBroker";
    public string AuthorityEnvironment { get; set; } = "sandbox";
    public Uri? PublicOrigin { get; set; }
    public string CallbackPath { get; set; } = "/TaxHub/HmrcCallback";
    public Uri? BlobServiceUri { get; set; }
    public string StateContainerName { get; set; } = "oauth-broker-state";
    public string EnvelopeKeyEnvironmentVariable { get; set; } = "OAUTH_BROKER_ENVELOPE_KEY";
    public string[] AllowedReturnHosts { get; set; } = [];
    public Uri CallbackUri => new(PublicOrigin!, CallbackPath);
}

public sealed class OAuthBrokerOptionsValidator : IValidateOptions<OAuthBrokerOptions>
{
    public ValidateOptionsResult Validate(string? name, OAuthBrokerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var failures = new List<string>();
        if (options.AuthorityEnvironment is not ("sandbox" or "production"))
            failures.Add("AuthorityEnvironment must be sandbox or production.");
        if (!IsHttpsOrigin(options.PublicOrigin)) failures.Add("PublicOrigin must be an exact HTTPS origin.");
        if (!IsPath(options.CallbackPath)) failures.Add("CallbackPath must be one application-relative path.");
        if (options.BlobServiceUri is null || options.BlobServiceUri.Scheme != Uri.UriSchemeHttps
            || !options.BlobServiceUri.IsDefaultPort
            || !options.BlobServiceUri.IdnHost.EndsWith(".blob.core.windows.net", StringComparison.OrdinalIgnoreCase))
            failures.Add("BlobServiceUri must identify an Azure Blob service over HTTPS.");
        if (!IsContainerName(options.StateContainerName))
            failures.Add("StateContainerName is not a valid Azure Blob container name.");
        if (string.IsNullOrWhiteSpace(options.EnvelopeKeyEnvironmentVariable))
            failures.Add("EnvelopeKeyEnvironmentVariable is required.");
        if (options.AllowedReturnHosts is null || options.AllowedReturnHosts.Length == 0
            || options.AllowedReturnHosts.Any(host => !IsCanonicalHost(host))
            || options.AllowedReturnHosts.Distinct(StringComparer.OrdinalIgnoreCase).Count()
                != options.AllowedReturnHosts.Length)
            failures.Add("AllowedReturnHosts must contain distinct canonical DNS host names.");
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    internal static bool IsHttpsOrigin(Uri? value) => value is { IsAbsoluteUri: true }
        && value.Scheme == Uri.UriSchemeHttps && value.IsDefaultPort && value.AbsolutePath == "/"
        && string.IsNullOrEmpty(value.Query) && string.IsNullOrEmpty(value.Fragment)
        && string.IsNullOrEmpty(value.UserInfo)
        && Uri.CheckHostName(value.IdnHost) == UriHostNameType.Dns;

    internal static bool IsCanonicalHost(string? value) => !string.IsNullOrWhiteSpace(value)
        && !value.EndsWith('.') && Uri.CheckHostName(value) == UriHostNameType.Dns;

    private static bool IsPath(string? value) => !string.IsNullOrWhiteSpace(value)
        && value.StartsWith('/') && !value.StartsWith("//", StringComparison.Ordinal)
        && !value.Contains('\\') && !value.Contains('?') && !value.Contains('#')
        && !value.Any(char.IsControl);

    private static bool IsContainerName(string value) => value.Length is >= 3 and <= 63
        && value[0] != '-' && value[^1] != '-'
        && value.All(character => char.IsAsciiLetterOrDigit(character) || character == '-')
        && !value.Contains("--", StringComparison.Ordinal);
}
