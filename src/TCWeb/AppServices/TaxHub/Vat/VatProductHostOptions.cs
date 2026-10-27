using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.FraudPrevention;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public enum VatAuthorityEnvironment
{
    Sandbox,
    Production
}

public enum VatPersistenceMode
{
    Disabled,
    DevelopmentFiles,
    AzureManaged
}

public enum VatSandboxSecretSource
{
    DevelopmentJsonFile,
    EnvironmentVariables
}

public enum VatOAuthCallbackMode
{
    Direct,
    Brokered
}

public sealed class VatProductHostOptions
{
    public const string SectionName = "TaxHub:VatProduct";

    public bool Enabled { get; set; }
    public bool ReviewEnabled { get; set; }
    public bool SubmissionEnabled { get; set; }
    public VatAuthorityEnvironment AuthorityEnvironment { get; set; } = VatAuthorityEnvironment.Sandbox;
    public VatPersistenceMode PersistenceMode { get; set; } = VatPersistenceMode.Disabled;
    public string? TenantReference { get; set; }
    public Uri? PublicOrigin { get; set; }
    public string OAuthCallbackPath { get; set; } = "/TaxHub/HmrcCallback";
    public VatOAuthCallbackMode OAuthCallbackMode { get; set; } = VatOAuthCallbackMode.Direct;
    public Uri? OAuthBrokerStartUri { get; set; }
    public Uri? OAuthBrokerCallbackUri { get; set; }
    public string OAuthBrokerEnvelopeKeyEnvironmentVariable { get; set; } = "OAUTH_BROKER_ENVELOPE_KEY";
    public string? DevelopmentStoreRoot { get; set; }
    public string? DevelopmentClientSettingsPath { get; set; }
    public VatSandboxSecretSource SandboxSecretSource { get; set; } = VatSandboxSecretSource.DevelopmentJsonFile;
    public bool FraudCaptureEnabled { get; set; } = true;
    public bool AllowIncompleteSandboxFraudHeaders { get; set; }
    public string[] FraudPublicTlsAddresses { get; set; } = [];
    public string[] FraudTrustedProxyAddresses { get; set; } = [];
    public DateOnly? SandboxObligationAsOfDate { get; set; }
    public Uri? KeyVaultUri { get; set; }
    public string? WorkflowConnectionName { get; set; }
    public string? EvidenceBlobServiceUri { get; set; }
    public string EvidenceContainerName { get; set; } = "statutory-evidence";
}

public sealed class VatProductHostOptionsValidator(IHostEnvironment hostEnvironment)
    : IValidateOptions<VatProductHostOptions>
{
    public ValidateOptionsResult Validate(string? name, VatProductHostOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Enabled)
            return ValidateOptionsResult.Success;

        var failures = new List<string>();
        if (string.IsNullOrWhiteSpace(options.OAuthCallbackPath)
            || !options.OAuthCallbackPath.StartsWith('/')
            || options.OAuthCallbackPath.StartsWith("//", StringComparison.Ordinal)
            || options.OAuthCallbackPath.Contains('\\')
            || options.OAuthCallbackPath.Contains('?')
            || options.OAuthCallbackPath.Contains('#')
            || options.OAuthCallbackPath.Any(char.IsControl))
            failures.Add("OAuthCallbackPath must be one application-relative path.");

        if (options.AuthorityEnvironment == VatAuthorityEnvironment.Production)
            failures.Add("Production HMRC composition is unavailable until the Phase 6.0 durable facilities are implemented and approved.");
        if (options.SandboxObligationAsOfDate.HasValue
            && options.AuthorityEnvironment != VatAuthorityEnvironment.Sandbox)
            failures.Add("SandboxObligationAsOfDate is permitted only for an HMRC sandbox host.");
        if (options.SubmissionEnabled && !options.ReviewEnabled)
            failures.Add("VAT submission cannot be enabled while preparation and review are disabled.");
        if (options.SubmissionEnabled && options.AuthorityEnvironment != VatAuthorityEnvironment.Sandbox)
            failures.Add("The accreditation dispatch gate is restricted to the HMRC sandbox authority environment.");
        if (options.SubmissionEnabled && !options.FraudCaptureEnabled)
            failures.Add("VAT submission requires fresh fraud-prevention evidence capture.");
        if (options.AllowIncompleteSandboxFraudHeaders
            && options.AuthorityEnvironment != VatAuthorityEnvironment.Sandbox)
            failures.Add("Incomplete fraud headers are permitted only for an HMRC sandbox host.");

        if (options.OAuthCallbackMode == VatOAuthCallbackMode.Brokered)
        {
            if (options.PersistenceMode != VatPersistenceMode.AzureManaged)
                failures.Add("Brokered OAuth is permitted only for an AzureManaged product host.");
            if (!IsBrokerUri(options.OAuthBrokerStartUri, "/oauth/start"))
                failures.Add("OAuthBrokerStartUri must be the broker's exact HTTPS /oauth/start endpoint.");
            if (!IsBrokerUri(options.OAuthBrokerCallbackUri, "/TaxHub/HmrcCallback"))
                failures.Add("OAuthBrokerCallbackUri must be the broker's exact HTTPS callback endpoint.");
            if (!string.Equals(options.OAuthBrokerStartUri?.IdnHost,
                    options.OAuthBrokerCallbackUri?.IdnHost, StringComparison.OrdinalIgnoreCase))
                failures.Add("The OAuth broker start and callback endpoints must use the same host.");
            if (string.IsNullOrWhiteSpace(options.OAuthBrokerEnvelopeKeyEnvironmentVariable))
                failures.Add("OAuthBrokerEnvelopeKeyEnvironmentVariable is required in brokered mode.");
        }

        switch (options.PersistenceMode)
        {
            case VatPersistenceMode.Disabled:
                failures.Add("An enabled VAT product host requires an approved persistence mode.");
                break;
            case VatPersistenceMode.DevelopmentFiles:
                if (!Guid.TryParse(options.TenantReference, out _))
                    failures.Add("TenantReference must be an opaque GUID in single-tenant DevelopmentFiles mode.");
                if (options.PublicOrigin is null || !options.PublicOrigin.IsAbsoluteUri
                    || options.PublicOrigin.Scheme != Uri.UriSchemeHttps
                    || options.PublicOrigin.AbsolutePath != "/"
                    || !string.IsNullOrEmpty(options.PublicOrigin.Query)
                    || !string.IsNullOrEmpty(options.PublicOrigin.Fragment))
                    failures.Add("PublicOrigin must be an HTTPS origin without a path, query or fragment in DevelopmentFiles mode.");
                if (!hostEnvironment.IsDevelopment())
                    failures.Add("DevelopmentFiles is permitted only in the Development host environment.");
                if (string.IsNullOrWhiteSpace(options.DevelopmentStoreRoot)
                    || !Path.IsPathFullyQualified(options.DevelopmentStoreRoot))
                    failures.Add("DevelopmentStoreRoot must be an absolute path.");
                if (options.SandboxSecretSource == VatSandboxSecretSource.DevelopmentJsonFile
                    && (string.IsNullOrWhiteSpace(options.DevelopmentClientSettingsPath)
                        || !Path.IsPathFullyQualified(options.DevelopmentClientSettingsPath)))
                    failures.Add("DevelopmentClientSettingsPath must be an absolute, git-ignored client settings path when DevelopmentJsonFile is selected.");
                if (options.FraudCaptureEnabled)
                {
                    if (options.FraudPublicTlsAddresses is null || options.FraudPublicTlsAddresses.Length == 0
                        || options.FraudPublicTlsAddresses.Any(value => !IsPublicAddress(value)))
                        failures.Add("FraudPublicTlsAddresses must contain explicit public server-side IP addresses.");
                    if (options.FraudTrustedProxyAddresses is null
                        || options.FraudTrustedProxyAddresses.Any(value => !System.Net.IPAddress.TryParse(value, out _)))
                        failures.Add("FraudTrustedProxyAddresses contains an invalid IP address.");
                    else if (options.FraudTrustedProxyAddresses.Length == 0
                        && options.FraudPublicTlsAddresses?.Length != 1)
                        failures.Add("A direct fraud topology requires exactly one public TLS address.");
                }
                break;
            case VatPersistenceMode.AzureManaged:
                if (options.KeyVaultUri is null || !IsAzureHttpsHost(options.KeyVaultUri, ".vault.azure.net"))
                    failures.Add("AzureManaged requires an HTTPS Azure Key Vault URI.");
                if (string.IsNullOrWhiteSpace(options.WorkflowConnectionName))
                    failures.Add("AzureManaged requires a named workflow database connection.");
                if (!Uri.TryCreate(options.EvidenceBlobServiceUri, UriKind.Absolute, out var blobUri)
                    || !IsAzureHttpsHost(blobUri, ".blob.core.windows.net"))
                    failures.Add("AzureManaged requires an HTTPS Azure Blob service URI.");
                if (string.IsNullOrWhiteSpace(options.EvidenceContainerName))
                    failures.Add("AzureManaged requires an evidence container name.");
                if (options.SandboxSecretSource != VatSandboxSecretSource.EnvironmentVariables)
                    failures.Add("AzureManaged sandbox credentials must be supplied through environment variables or Key Vault references.");
                break;
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsAzureHttpsHost(Uri uri, string suffix) => uri.IsAbsoluteUri
        && uri.Scheme == Uri.UriSchemeHttps
        && uri.IsDefaultPort
        && uri.IdnHost.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
        && uri.IdnHost.Length > suffix.Length;

    private static bool IsBrokerUri(Uri? uri, string path) => uri is { IsAbsoluteUri: true }
        && uri.Scheme == Uri.UriSchemeHttps && uri.IsDefaultPort
        && uri.AbsolutePath.Equals(path, StringComparison.Ordinal)
        && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment)
        && Uri.CheckHostName(uri.IdnHost) == UriHostNameType.Dns;

    private static bool IsPublicAddress(string value)
    {
        if (!System.Net.IPAddress.TryParse(value, out var address)) return false;
        try { _ = FraudDeploymentTopology.Direct("options-validation", address); return true; }
        catch (ArgumentException) { return false; }
    }
}

public sealed class VatProductDevelopmentDefaults(IHostEnvironment hostEnvironment)
    : IPostConfigureOptions<VatProductHostOptions>
{
    public void PostConfigure(string? name, VatProductHostOptions options)
    {
        if (!hostEnvironment.IsDevelopment() || !options.Enabled
            || options.PersistenceMode != VatPersistenceMode.DevelopmentFiles)
            return;

        var repositoryRoot = Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, "..", ".."));
        options.DevelopmentStoreRoot ??= Path.Combine(repositoryRoot, ".local", "tax-hub", "tcweb");
        if (options.SandboxSecretSource == VatSandboxSecretSource.DevelopmentJsonFile)
            options.DevelopmentClientSettingsPath ??= Path.Combine(repositoryRoot, ".local",
                "vat_mtd_client_test-master", "mtd-client-vat", "clientsettings.json");
    }
}
