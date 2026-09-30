using System.Xml.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TradeControl.Web.AppServices.TaxHub.Vat;
using TradeControl.Web.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.Controllers;
using TradeControl.Tax.UK.Hmrc.Vat.v1_0.Returns;

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
var historicalSandbox = ValidDevelopmentOptions();
historicalSandbox.SandboxObligationAsOfDate = new DateOnly(2017, 6, 30);
Assert(developmentValidator.Validate(null, historicalSandbox).Succeeded,
    "A DevelopmentFiles sandbox host rejected its controlled historical obligation anchor.");
Assert(productionValidator.Validate(null, historicalSandbox).Failed,
    "A historical sandbox obligation anchor was accepted outside Development.");
historicalSandbox.AllowIncompleteSandboxFraudHeaders = true;
Assert(developmentValidator.Validate(null, historicalSandbox).Succeeded,
    "The reviewed DevelopmentFiles sandbox warning mode was rejected in Development.");
Assert(productionValidator.Validate(null, historicalSandbox).Failed,
    "Incomplete sandbox fraud evidence was accepted outside Development.");
Assert(sandbox.OAuthCallbackPath == "/TaxHub/HmrcCallback",
    "The fixed rooted callback path changed; it must validate consistently on Windows and Linux.");
var unsafeCallback = ValidDevelopmentOptions();
unsafeCallback.OAuthCallbackPath = "//attacker.example/callback";
Assert(developmentValidator.Validate(null, unsafeCallback).Failed,
    "A network-path callback was accepted by the product host.");

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

var window = VatObligationWorkspaceService.SearchWindow(new DateOnly(2026, 9, 28));
Assert(window.From == new DateOnly(2025, 9, 28) && window.To == new DateOnly(2026, 9, 28)
    && window.To.DayNumber - window.From.DayNumber == 365,
    "The inclusive HMRC obligation search window is not bounded to 366 calendar days.");
var historicalWindow = VatObligationWorkspaceService.SearchWindow(new DateOnly(2017, 6, 30));
Assert(historicalWindow.From == new DateOnly(2016, 6, 30)
    && historicalWindow.To == new DateOnly(2017, 6, 30),
    "The controlled sandbox anchor did not produce the same bounded obligation window.");
var obligationReconciler = new VatObligationReconciler();
var obligationRows = obligationReconciler.Reconcile(
    [
        new("#123", new(2026, 7, 1), new(2026, 9, 30), new(2026, 11, 7), "O"),
        new("18A1", new(2026, 4, 1), new(2026, 6, 30), new(2026, 8, 7), "F", new(2026, 8, 6)),
        new("18A0", new(2026, 1, 1), new(2026, 3, 31), new(2026, 5, 7), "O")
    ],
    [
        new(new(2026, 7, 1), new(2026, 9, 30)),
        new(new(2026, 4, 1), new(2026, 6, 30)),
        new(new(2025, 10, 1), new(2025, 12, 31))
    ]);
Assert(obligationRows.Count == 4
    && obligationRows[0].PeriodKey == "#123" && obligationRows[0].CanReview
    && obligationRows.Any(item => item.PeriodKey == "18A0"
        && item.MatchState == VatObligationMatchState.OpenLocalPeriodMissing)
    && obligationRows.Any(item => item.PeriodKey == "18A1"
        && item.MatchState == VatObligationMatchState.Fulfilled && item.Received == new DateOnly(2026, 8, 6))
    && obligationRows.Any(item => item.PeriodKey is null
        && item.MatchState == VatObligationMatchState.LocalWithoutAuthorityObligation),
    "Authority/local VAT obligation reconciliation did not preserve all required match states.");
Assert(obligationReconciler.Reconcile([], []).Count == 0,
    "An empty authority/local obligation result did not remain empty.");
Assert(typeof(IVatObligationWorkspaceService).GetMethod(nameof(IVatObligationWorkspaceService.GetAsync))!
        .GetParameters().All(parameter => parameter.ParameterType == typeof(CancellationToken)),
    "The product obligation boundary accepts a browser-supplied VRN, period key or date range.");
Assert(VatObligationWorkspaceService.SyntheticPlaceholderVrn == "999000001",
    "The product obligation boundary no longer blocks the synthetic placeholder VRN.");
var obligationComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubVatObligations.razor"));
Assert(new[] { "Retrieving authority obligations", "returned no VAT obligations", "Refresh HMRC",
        "Review return", "History/readback", "separate from local forecast dates" }
    .All(obligationComponent.Contains),
    "The VAT obligations UI lost a required loading, empty, retry, review, readback or forecast distinction.");
Assert(!obligationComponent.Contains("@bind", StringComparison.OrdinalIgnoreCase),
    "The authority-obligation surface introduced editable browser fields.");
var vatWorkspaceComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubVatWorkspace.razor"));
Assert(vatWorkspaceComponent.Contains("@if (VatFilingEnabled)", StringComparison.Ordinal),
    "A non-VAT scenario can render the HMRC VAT filing journey.");
Assert(vatWorkspaceComponent.IndexOf("VAT Statement", StringComparison.Ordinal)
        < vatWorkspaceComponent.IndexOf("HMRC Obligations", StringComparison.Ordinal),
    "The HMRC obligations surface displaced the original VAT workspace as the default tab.");
var obligationService = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "VatObligationWorkspaceService.cs"));
Assert(obligationService.Contains("nodeContext.EventLog", StringComparison.Ordinal)
    && obligationService.Contains("nodeContext.ErrorLog", StringComparison.Ordinal)
    && obligationService.Contains("Support reference:", StringComparison.Ordinal),
    "VAT authority failures are no longer linked to the application Event Log and support reference.");
var alignmentSql = File.ReadAllText(Path.Combine(root, "src", "sqlnode", "src", "tcNodeDb4", "App",
    "Stored Procedures", "proc_DatasetSyntheticMIS_VatSandboxAlign.sql"));
Assert(alignmentSql.Contains("ValueSourceCode = N'SYNTHETIC'", StringComparison.Ordinal)
    && alignmentSql.Contains("Cash.vwTaxVatSubmission", StringComparison.Ordinal)
    && alignmentSql.Contains("Cash.proc_ReportingProfileSave", StringComparison.Ordinal)
    && alignmentSql.Contains("@SandboxVrn = N'999000001'", StringComparison.Ordinal),
    "The disposable VAT alignment guard or authoritative calculation/configuration boundary changed.");

var returnReview = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubVatReturnReview.razor"));
Assert(returnReview.Contains("The values below are read from the immutable prepared request", StringComparison.Ordinal)
    && returnReview.Contains("following day as the exclusive period boundary", StringComparison.Ordinal)
    && returnReview.Contains("Record approval", StringComparison.Ordinal)
    && returnReview.Contains("Submission remains disabled until Phase 6.4", StringComparison.Ordinal),
    "The Phase 6.3 exact-review boundary or no-submit gate is no longer explicit in the UI.");
Assert(returnReview.Contains("@bind=\"_confirmed\"", StringComparison.Ordinal)
    && !returnReview.Contains("checked=", StringComparison.OrdinalIgnoreCase)
    && returnReview.Contains("_warningsAcknowledged", StringComparison.Ordinal),
    "The legal declaration or warning acknowledgement is not explicit and unchecked by default.");
Assert(!returnReview.Contains("<input", StringComparison.OrdinalIgnoreCase)
        || !returnReview.Contains("VatDueSales", StringComparison.Ordinal),
    "The exact VAT boxes appear to be editable or reconstructed in the browser.");
var reviewService = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "VatReturnReviewService.cs"));
Assert(reviewService.Contains("new VatReturnPreparer", StringComparison.Ordinal)
    && reviewService.Contains("Cash.vwTaxVatSubmission", StringComparison.Ordinal) == false
    && reviewService.Contains("current.Request.BodySha256 != candidate.PreparedSha256", StringComparison.Ordinal)
    && reviewService.Contains("source.SnapshotToken != candidate.SnapshotToken", StringComparison.Ordinal),
    "VAT approval no longer reuses the Objective 3 preparer or rejects a stale exact candidate.");
Assert(reviewService.Contains("_filingPolicy.CanManageHmrcConnection", StringComparison.Ordinal)
    && reviewService.Contains("TenantReference == identity.TenantReference", StringComparison.Ordinal)
    && reviewService.Contains("PrincipalReference == identity.AspNetSubjectReference", StringComparison.Ordinal)
    && reviewService.Contains("ActorReference == identity.ActorReference", StringComparison.Ordinal),
    "VAT approval lost its filing-role or server-derived tenant/principal/actor boundary.");
Assert(reviewService.Contains("FileSubmissionContentStore", StringComparison.Ordinal)
    && reviewService.Contains("PreparedSha256", StringComparison.Ordinal)
    && reviewService.Contains("DeclarationSha256", StringComparison.Ordinal)
    && reviewService.Contains("nodeContext.ErrorLog", StringComparison.Ordinal),
    "The exact body, declaration evidence, digest, or Event Log support path is missing.");
var declaration = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "Declarations", "hmrc-vat-business-2026-09-30.txt")).Trim();
Assert(declaration == "When you submit this VAT information you are making a legal declaration that the information is true and complete. A false declaration can result in prosecution.",
    "The versioned HMRC VAT business declaration changed without an explicit version change.");
Assert(typeof(IVatReturnReviewService).GetMethod(nameof(IVatReturnReviewService.ApproveAsync))!
        .GetParameters().All(parameter => !parameter.Name!.Contains("body", StringComparison.OrdinalIgnoreCase)
            && !parameter.Name.Contains("tenant", StringComparison.OrdinalIgnoreCase)
            && !parameter.Name.Contains("actor", StringComparison.OrdinalIgnoreCase)),
    "The approval boundary accepts a caller-supplied body or protected identity.");
var exactReturn = new VatReturnRequest
{
    PeriodKey = "18A2", VatDueSales = 101.23m, VatDueAcquisitions = 2.34m,
    TotalVatDue = 103.57m, VatReclaimedCurrPeriod = 40.12m, NetVatDue = 63.45m,
    TotalValueSalesExVat = 1001m, TotalValuePurchasesExVat = 402m,
    TotalValueGoodsSuppliedExVat = 3m, TotalAcquisitionsExVat = 4m, Finalised = true
};
var exactBoxes = VatReturnReviewService.ProjectBoxes(exactReturn);
Assert(exactBoxes.Select(box => box.Number).SequenceEqual(Enumerable.Range(1, 9))
    && exactBoxes.Select(box => box.Value).SequenceEqual(
        ["101.23", "2.34", "103.57", "40.12", "63.45", "1001", "402", "3", "4"]),
    "The review projection does not reproduce all nine prepared VAT values exactly and invariantly.");

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
