using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

public sealed class VatProductHostOptions
{
    public const string SectionName = "TaxHub:VatProduct";

    public bool Enabled { get; set; }
    public VatAuthorityEnvironment AuthorityEnvironment { get; set; } = VatAuthorityEnvironment.Sandbox;
    public VatPersistenceMode PersistenceMode { get; set; } = VatPersistenceMode.Disabled;
    public string? TenantReference { get; set; }
    public Uri? PublicOrigin { get; set; }
    public string OAuthCallbackPath { get; set; } = "/TaxHub/HmrcCallback";
    public string? DevelopmentStoreRoot { get; set; }
    public Uri? KeyVaultUri { get; set; }
    public string? WorkflowConnectionName { get; set; }
    public string? EvidenceBlobServiceUri { get; set; }
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
        if (!Guid.TryParse(options.TenantReference, out _))
            failures.Add("TenantReference must be an opaque GUID assigned to this Trade Control node.");
        if (options.PublicOrigin is null || !options.PublicOrigin.IsAbsoluteUri
            || options.PublicOrigin.Scheme != Uri.UriSchemeHttps
            || options.PublicOrigin.AbsolutePath != "/"
            || !string.IsNullOrEmpty(options.PublicOrigin.Query)
            || !string.IsNullOrEmpty(options.PublicOrigin.Fragment))
            failures.Add("PublicOrigin must be an HTTPS origin without a path, query or fragment.");
        if (string.IsNullOrWhiteSpace(options.OAuthCallbackPath)
            || !options.OAuthCallbackPath.StartsWith('/')
            || options.OAuthCallbackPath.StartsWith("//", StringComparison.Ordinal)
            || Uri.TryCreate(options.OAuthCallbackPath, UriKind.Absolute, out _))
            failures.Add("OAuthCallbackPath must be one application-relative path.");

        if (options.AuthorityEnvironment == VatAuthorityEnvironment.Production)
            failures.Add("Production HMRC composition is unavailable until the Phase 6.0 durable facilities are implemented and approved.");

        switch (options.PersistenceMode)
        {
            case VatPersistenceMode.Disabled:
                failures.Add("An enabled VAT product host requires an approved persistence mode.");
                break;
            case VatPersistenceMode.DevelopmentFiles:
                if (!hostEnvironment.IsDevelopment())
                    failures.Add("DevelopmentFiles is permitted only in the Development host environment.");
                if (string.IsNullOrWhiteSpace(options.DevelopmentStoreRoot)
                    || !Path.IsPathFullyQualified(options.DevelopmentStoreRoot))
                    failures.Add("DevelopmentStoreRoot must be an absolute path.");
                break;
            case VatPersistenceMode.AzureManaged:
                if (options.KeyVaultUri is null || !IsAzureHttpsHost(options.KeyVaultUri, ".vault.azure.net"))
                    failures.Add("AzureManaged requires an HTTPS Azure Key Vault URI.");
                if (string.IsNullOrWhiteSpace(options.WorkflowConnectionName))
                    failures.Add("AzureManaged requires a named workflow database connection.");
                if (!Uri.TryCreate(options.EvidenceBlobServiceUri, UriKind.Absolute, out var blobUri)
                    || !IsAzureHttpsHost(blobUri, ".blob.core.windows.net"))
                    failures.Add("AzureManaged requires an HTTPS Azure Blob service URI.");
                // The facility contracts are selected, but deliberately not composed in Phase 6.0.
                failures.Add("AzureManaged VAT persistence has been selected but is not implemented; hosted composition remains disabled.");
                break;
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsAzureHttpsHost(Uri uri, string suffix) => uri.IsAbsoluteUri
        && uri.Scheme == Uri.UriSchemeHttps
        && uri.IsDefaultPort
        && uri.IdnHost.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
        && uri.IdnHost.Length > suffix.Length;
}
