using System.Xml.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TradeControl.Web.AppServices.TaxHub.Vat;
using TradeControl.Web.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.Controllers;

var assertions = 0;
void Assert(bool condition, string message)
{
    assertions++;
    if (!condition) throw new InvalidOperationException(message);
}

var root = FindRoot(AppContext.BaseDirectory);
var webProject = Path.Combine(root, "src", "TCWeb", "TCWeb.csproj");
var webReferences = ProjectReferences(webProject);
Assert(webReferences.Any(reference => reference.Contains("Tax.UK.Application", StringComparison.Ordinal)),
    "TCWeb does not reference the Tax Hub Application boundary.");
Assert(webReferences.Any(reference => reference.Contains("Adapters.TradeControl", StringComparison.Ordinal)),
    "TCWeb does not reference the Trade Control adapter.");
Assert(webReferences.Any(reference => reference.Contains("Adapters.Submission", StringComparison.Ordinal)),
    "TCWeb does not reference the Submission adapter.");
Assert(webReferences.All(reference => !reference.Contains("WebHarness", StringComparison.OrdinalIgnoreCase)),
    "TCWeb must not reference the diagnostic WebHarness.");

var workflowContract = typeof(IVatProductWorkflow);
var prohibitedContractNamespaces = new[]
{
    "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore.Mvc",
    "MudBlazor", "TradeControl.Web.Model"
};
var contractTypes = workflowContract.GetMethods()
    .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
        .Append(method.ReturnType))
    .Append(workflowContract);
Assert(contractTypes.All(type => prohibitedContractNamespaces.All(prohibited =>
        !(type.Namespace ?? string.Empty).StartsWith(prohibited, StringComparison.Ordinal))),
    "The VAT workflow contract contains a host, UI, MVC or EF type.");
var prohibitedInputNames = new[] { "tenant", "body", "box1", "box2", "box3", "box4", "box5", "box6", "box7", "box8", "box9" };
Assert(workflowContract.GetMethods().SelectMany(method => method.GetParameters()).All(parameter =>
        prohibitedInputNames.All(prohibited => !parameter.Name!.Contains(prohibited, StringComparison.OrdinalIgnoreCase))),
    "The VAT workflow accepts caller-supplied tenant identity, request bodies or VAT boxes.");

var taxHubProjects = Directory.GetFiles(Path.Combine(root, "src", "tax-hub", "src"), "*.csproj",
    SearchOption.AllDirectories);
foreach (var project in taxHubProjects.Where(path => !path.Contains("WebHarness", StringComparison.OrdinalIgnoreCase)))
    Assert(ProjectReferences(project).All(reference => !reference.Contains("TCWeb", StringComparison.OrdinalIgnoreCase)),
        $"Tax Hub project '{Path.GetFileName(project)}' must not depend on TCWeb.");

var development = new TestHostEnvironment(Environments.Development);
var production = new TestHostEnvironment(Environments.Production);
var developmentValidator = new VatProductHostOptionsValidator(development);
var productionValidator = new VatProductHostOptionsValidator(production);

Assert(developmentValidator.Validate(null, new VatProductHostOptions()).Succeeded,
    "The disabled product boundary must permit an unconfigured host.");

var sandbox = ValidDevelopmentOptions();
Assert(developmentValidator.Validate(null, sandbox).Succeeded,
    "A sandbox host with an absolute development store should validate in Development.");

var environmentSecrets = ValidDevelopmentOptions();
environmentSecrets.SandboxSecretSource = VatSandboxSecretSource.EnvironmentVariables;
environmentSecrets.DevelopmentClientSettingsPath = null;
Assert(developmentValidator.Validate(null, environmentSecrets).Succeeded,
    "A hosted sandbox cannot select protected environment client credentials.");

var developmentHost = new TestHostEnvironment(Environments.Development)
{
    ContentRootPath = Path.Combine(root, "src", "TCWeb")
};
var localDefaults = new VatProductHostOptions
{
    Enabled = true,
    PersistenceMode = VatPersistenceMode.DevelopmentFiles
};
new VatProductDevelopmentDefaults(developmentHost).PostConfigure(null, localDefaults);
var localStoreRoot = localDefaults.DevelopmentStoreRoot;
var localClientSettings = localDefaults.DevelopmentClientSettingsPath;
Assert(localStoreRoot is not null && localClientSettings is not null
    && Path.IsPathFullyQualified(localStoreRoot)
    && Path.IsPathFullyQualified(localClientSettings)
    && localStoreRoot.Contains(Path.Combine(".local", "tax-hub", "tcweb"), StringComparison.Ordinal)
    && localClientSettings.EndsWith(
        Path.Combine(".local", "vat_mtd_client_test-master", "mtd-client-vat", "clientsettings.json"),
        StringComparison.Ordinal),
    "Local development did not derive its ignored store and credential paths from the repository root.");
Assert(productionValidator.Validate(null, sandbox).Failed,
    "A development file store must fail outside Development.");

sandbox.DevelopmentStoreRoot = ".local/tax-hub";
Assert(developmentValidator.Validate(null, sandbox).Failed,
    "A relative development store path must fail closed.");

sandbox = ValidDevelopmentOptions();
sandbox.FraudPublicTlsAddresses = ["127.0.0.1"];
Assert(developmentValidator.Validate(null, sandbox).Failed,
    "A non-public fraud-prevention TLS hop was accepted by product composition.");

sandbox = ValidDevelopmentOptions();
sandbox.AuthorityEnvironment = VatAuthorityEnvironment.Production;
Assert(developmentValidator.Validate(null, sandbox).Failed,
    "Production HMRC composition must remain unavailable in Phase 6.0.");

var azure = new VatProductHostOptions
{
    Enabled = true,
    AuthorityEnvironment = VatAuthorityEnvironment.Sandbox,
    PersistenceMode = VatPersistenceMode.AzureManaged,
    TenantReference = Guid.NewGuid().ToString(),
    PublicOrigin = new Uri("https://tcweb.example.test/"),
    KeyVaultUri = new Uri("https://example.vault.azure.net/"),
    WorkflowConnectionName = "TaxHubWorkflow",
    EvidenceBlobServiceUri = "https://example.blob.core.windows.net/"
};
Assert(productionValidator.Validate(null, azure).Failed,
    "Selected Azure facilities must remain disabled until their implementations exist.");

var tenant = Guid.NewGuid().ToString();
var identity = new VatWorkflowIdentity(tenant, "aspnet-subject", "internal-actor", "HOME");
var authority = new VatAuthorityPrincipal(tenant, "hmrc-principal");
var context = new VatAuthorityDispatchContextFactory().Create(identity, authority,
    "approval", "facts", "logical", "18A2");
Assert(context.TenantReference == tenant && context.ActorReference == "internal-actor"
    && context.AuthorisationPrincipalReference == "hmrc-principal",
    "The authority context did not preserve the server-derived identities.");

var crossTenantRejected = false;
try
{
    _ = new VatAuthorityDispatchContextFactory().Create(identity,
        new VatAuthorityPrincipal(Guid.NewGuid().ToString(), "other-principal"), "approval", "facts");
}
catch (UnauthorizedAccessException)
{
    crossTenantRejected = true;
}
Assert(crossTenantRejected, "A cross-tenant HMRC principal was accepted.");

var filingPolicy = new VatFilingAuthorisationPolicy();
Assert(filingPolicy.CanManageHmrcConnection(Principal(Constants.AdministratorsRole)),
    "An administrator was denied the initial HMRC connection policy.");
Assert(filingPolicy.CanManageHmrcConnection(Principal(Constants.ManagersRole)),
    "A manager was denied the initial HMRC connection policy.");
Assert(!filingPolicy.CanManageHmrcConnection(Principal("Users")),
    "A general user was permitted to change the HMRC connection.");
Assert(!filingPolicy.CanManageHmrcConnection(new ClaimsPrincipal(new ClaimsIdentity())),
    "An unauthenticated principal was permitted to change the HMRC connection.");

var hmrcController = typeof(TaxHubHmrcController);
Assert(hmrcController.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Length == 1,
    "The TCWeb HMRC product endpoints are not protected by ASP.NET Identity.");
var callback = hmrcController.GetMethod(nameof(TaxHubHmrcController.Callback))!;
Assert(callback.GetParameters().All(parameter => !parameter.Name!.Contains("return", StringComparison.OrdinalIgnoreCase)),
    "The fixed HMRC callback accepts a caller-controlled return target.");
var disconnect = hmrcController.GetMethod(nameof(TaxHubHmrcController.Disconnect))!;
Assert(disconnect.GetCustomAttributes(typeof(HttpPostAttribute), inherit: true).Length == 1,
    "Disconnect HMRC is not a protected state-changing POST.");
var browserProperties = typeof(VatBrowserFacts).GetProperties().Select(property => property.Name).ToArray();
Assert(new[] { "Tenant", "Principal", "Actor", "UserIds", "Remote", "Forwarded" }
        .All(name => browserProperties.All(property => !property.Contains(name, StringComparison.OrdinalIgnoreCase))),
    "The browser facts contract accepts a protected server-derived identity or ingress fact.");
Assert(typeof(IVatHmrcConnectionService).GetMethods().All(method =>
        !method.ReturnType.Name.Contains("Token", StringComparison.OrdinalIgnoreCase)),
    "The TCWeb HMRC connection boundary exposes bearer-token material.");

Console.WriteLine($"TCWeb Tax Hub boundary tests passed ({assertions} assertions).");

static VatProductHostOptions ValidDevelopmentOptions() => new()
{
    Enabled = true,
    AuthorityEnvironment = VatAuthorityEnvironment.Sandbox,
    PersistenceMode = VatPersistenceMode.DevelopmentFiles,
    TenantReference = Guid.NewGuid().ToString(),
    PublicOrigin = new Uri("https://localhost:44381/"),
    DevelopmentStoreRoot = Path.Combine(Path.GetTempPath(), "tax-hub-tests"),
    DevelopmentClientSettingsPath = Path.Combine(Path.GetTempPath(), "tax-hub-clientsettings.json"),
    FraudPublicTlsAddresses = ["8.8.8.8"]
};

static ClaimsPrincipal Principal(string role) => new(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.NameIdentifier, "subject"),
    new Claim(ClaimTypes.Role, role)
}, "test"));

static string FindRoot(string start)
{
    for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
        if (File.Exists(Path.Combine(directory.FullName, "src", "tradecontrol.web.sln")))
            return directory.FullName;
    throw new DirectoryNotFoundException("Could not locate the tradecontrol.web repository root.");
}

static IReadOnlyList<string> ProjectReferences(string project)
{
    var document = XDocument.Load(project);
    return document.Descendants().Where(element => element.Name.LocalName == "ProjectReference")
        .Select(element => (string?)element.Attribute("Include") ?? string.Empty).ToArray();
}

file sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;
    public string ApplicationName { get; set; } = "TradeControl.Web.TaxHub.Tests";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
