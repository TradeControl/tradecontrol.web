using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public enum CompaniesHouseAuthorityEnvironment
{
    OfficialTest,
    Production
}

public enum CompaniesHousePersistenceMode
{
    Disabled,
    DevelopmentFiles,
    AzureManaged
}

public enum CompaniesHouseDispatchMode
{
    SendDisabled
}

/// <summary>
/// Selects only product-host facilities. Presenter credentials, package references and company
/// authentication are deliberately absent and belong to protected Submission-adapter composition.
/// </summary>
public sealed class CompaniesHouseProductHostOptions
{
    public const string SectionName = "TaxHub:CompaniesHouseProduct";

    public bool Enabled { get; set; }
    public CompaniesHouseAuthorityEnvironment AuthorityEnvironment { get; set; } =
        CompaniesHouseAuthorityEnvironment.OfficialTest;
    public CompaniesHousePersistenceMode PersistenceMode { get; set; } =
        CompaniesHousePersistenceMode.Disabled;
    public CompaniesHouseDispatchMode DispatchMode { get; set; } =
        CompaniesHouseDispatchMode.SendDisabled;
    public string? TenantReference { get; set; }
    public string? DevelopmentStoreRoot { get; set; }
    public Uri? KeyVaultUri { get; set; }
    public string? WorkflowConnectionName { get; set; }
    public string? EvidenceBlobServiceUri { get; set; }
}

public sealed class CompaniesHouseProductHostOptionsValidator(IHostEnvironment hostEnvironment)
    : IValidateOptions<CompaniesHouseProductHostOptions>
{
    public ValidateOptionsResult Validate(string? name, CompaniesHouseProductHostOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Enabled)
            return ValidateOptionsResult.Success;

        var failures = new List<string>();
        if (!Guid.TryParse(options.TenantReference, out _))
            failures.Add("TenantReference must be an opaque GUID assigned to this Trade Control node.");
        if (options.DispatchMode != CompaniesHouseDispatchMode.SendDisabled)
            failures.Add("Companies House dispatch remains send-disabled until a later reviewed phase composes it.");

        switch (options.PersistenceMode)
        {
            case CompaniesHousePersistenceMode.Disabled:
                failures.Add("An enabled Companies House product host requires an approved persistence mode.");
                break;
            case CompaniesHousePersistenceMode.DevelopmentFiles:
                if (!hostEnvironment.IsDevelopment())
                    failures.Add("DevelopmentFiles is permitted only in the Development host environment.");
                if (string.IsNullOrWhiteSpace(options.DevelopmentStoreRoot)
                    || !Path.IsPathFullyQualified(options.DevelopmentStoreRoot))
                    failures.Add("DevelopmentStoreRoot must be an absolute path.");
                break;
            case CompaniesHousePersistenceMode.AzureManaged:
                if (options.KeyVaultUri is null || !IsAzureHttpsHost(options.KeyVaultUri, ".vault.azure.net"))
                    failures.Add("AzureManaged requires an HTTPS Azure Key Vault URI.");
                if (string.IsNullOrWhiteSpace(options.WorkflowConnectionName))
                    failures.Add("AzureManaged requires a named workflow database connection.");
                if (!Uri.TryCreate(options.EvidenceBlobServiceUri, UriKind.Absolute, out var blobUri)
                    || !IsAzureHttpsHost(blobUri, ".blob.core.windows.net"))
                    failures.Add("AzureManaged requires an HTTPS Azure Blob service URI.");
                failures.Add("AzureManaged Companies House persistence is selected but is not implemented; hosted composition remains disabled.");
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

public sealed class CompaniesHouseProductDevelopmentDefaults(IHostEnvironment hostEnvironment)
    : IPostConfigureOptions<CompaniesHouseProductHostOptions>
{
    public void PostConfigure(string? name, CompaniesHouseProductHostOptions options)
    {
        if (!hostEnvironment.IsDevelopment() || !options.Enabled
            || options.PersistenceMode != CompaniesHousePersistenceMode.DevelopmentFiles)
            return;

        var repositoryRoot = Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, "..", ".."));
        options.DevelopmentStoreRoot ??= Path.Combine(repositoryRoot, ".local", "tax-hub", "tcweb",
            "companies-house");
    }
}
