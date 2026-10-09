using System.Xml.Linq;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.AppServices.TaxHub.Vat;
using TradeControl.Web.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TradeControl.Web.Controllers;
using TradeControl.Tax.UK.Hmrc.Vat.v1_0.Returns;
using TradeControl.Tax.UK.Application.DataProvision;

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

var companiesHouseWorkflowContract = typeof(ICompaniesHouseProductWorkflow);
var companiesHouseContractTypes = companiesHouseWorkflowContract.GetMethods()
    .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
        .Append(method.ReturnType))
    .Append(companiesHouseWorkflowContract);
Assert(companiesHouseContractTypes.All(type => prohibitedContractNamespaces.All(prohibited =>
        !(type.Namespace ?? string.Empty).StartsWith(prohibited, StringComparison.Ordinal))),
    "The Companies House workflow contract contains a host, UI, MVC or EF type.");
var prohibitedCompaniesHouseInputs = new[]
{
    "tenant", "credential", "authentication", "presenter", "package", "xml", "ixbrl", "body",
    "companyNumber"
};
Assert(companiesHouseWorkflowContract.GetMethods().SelectMany(method => method.GetParameters()).All(parameter =>
        prohibitedCompaniesHouseInputs.All(prohibited =>
            !parameter.Name!.Contains(prohibited, StringComparison.OrdinalIgnoreCase))),
    "The Companies House workflow accepts caller-supplied identity, credentials or statutory content.");
Assert(typeof(CompaniesHouseProductHostOptions).GetProperties().All(property =>
        prohibitedCompaniesHouseInputs.Where(value => value is "credential" or "authentication" or "presenter"
            or "companyNumber").All(prohibited =>
                !property.Name.Contains(prohibited, StringComparison.OrdinalIgnoreCase))),
    "The TCWeb Companies House options expose protected transport inputs.");

var taxHubProjects = Directory.GetFiles(Path.Combine(root, "src", "tax-hub", "src"), "*.csproj",
    SearchOption.AllDirectories);
foreach (var project in taxHubProjects.Where(path => !path.Contains("WebHarness", StringComparison.OrdinalIgnoreCase)))
    Assert(ProjectReferences(project).All(reference => !reference.Contains("TCWeb", StringComparison.OrdinalIgnoreCase)),
        $"Tax Hub project '{Path.GetFileName(project)}' must not depend on TCWeb.");

var development = new TestHostEnvironment(Environments.Development);
var production = new TestHostEnvironment(Environments.Production);
var developmentValidator = new VatProductHostOptionsValidator(development);
var productionValidator = new VatProductHostOptionsValidator(production);
var companiesHouseDevelopmentValidator = new CompaniesHouseProductHostOptionsValidator(development);
var companiesHouseProductionValidator = new CompaniesHouseProductHostOptionsValidator(production);

Assert(companiesHouseDevelopmentValidator.Validate(null, new CompaniesHouseProductHostOptions()).Succeeded,
    "The disabled Companies House product boundary must permit an unconfigured host.");
var companiesHouseDevelopment = ValidCompaniesHouseDevelopmentOptions();
Assert(companiesHouseDevelopmentValidator.Validate(null, companiesHouseDevelopment).Succeeded,
    "A send-disabled Companies House host with an absolute development store should validate in Development.");
Assert(companiesHouseDevelopment.DispatchMode == CompaniesHouseDispatchMode.SendDisabled,
    "Phase 7.0 must not expose a send-enabled Companies House composition.");
Assert(companiesHouseProductionValidator.Validate(null, companiesHouseDevelopment).Failed,
    "A Companies House development file store was accepted outside Development.");
companiesHouseDevelopment.DevelopmentStoreRoot = ".local/tax-hub/companies-house";
Assert(companiesHouseDevelopmentValidator.Validate(null, companiesHouseDevelopment).Failed,
    "A relative Companies House development store path was accepted.");
var companiesHouseProduction = new CompaniesHouseProductHostOptions
{
    Enabled = true,
    AuthorityEnvironment = CompaniesHouseAuthorityEnvironment.Production,
    PersistenceMode = CompaniesHousePersistenceMode.AzureManaged,
    DispatchMode = CompaniesHouseDispatchMode.SendDisabled,
    TenantReference = Guid.NewGuid().ToString(),
    KeyVaultUri = new Uri("https://example.vault.azure.net/"),
    WorkflowConnectionName = "TaxHubWorkflow",
    EvidenceBlobServiceUri = "https://example.blob.core.windows.net/"
};
Assert(companiesHouseProductionValidator.Validate(null, companiesHouseProduction).Succeeded,
    "The reviewed Azure-managed Companies House persistence boundary was rejected.");
companiesHouseProduction.EvidenceContainerName = "Invalid_Container";
Assert(companiesHouseProductionValidator.Validate(null, companiesHouseProduction).Failed,
    "An invalid Companies House evidence-container name was accepted.");
var companiesHouseDefaults = new CompaniesHouseProductHostOptions
{
    Enabled = true,
    PersistenceMode = CompaniesHousePersistenceMode.DevelopmentFiles
};
new CompaniesHouseProductDevelopmentDefaults(new TestHostEnvironment(Environments.Development)
{
    ContentRootPath = Path.Combine(root, "src", "TCWeb")
}).PostConfigure(null, companiesHouseDefaults);
Assert(companiesHouseDefaults.DevelopmentStoreRoot is not null
    && Path.IsPathFullyQualified(companiesHouseDefaults.DevelopmentStoreRoot)
    && companiesHouseDefaults.DevelopmentStoreRoot.EndsWith(
        Path.Combine(".local", "tax-hub", "tcweb", "companies-house"), StringComparison.Ordinal),
    "Local development did not derive a git-ignored Companies House evidence-store path.");
Assert(CompaniesHouseProductPolicy.RetainUntil(new DateTimeOffset(2026, 10, 8, 12, 0, 0, TimeSpan.Zero))
        == new DateOnly(2033, 10, 8),
    "The reviewed Companies House evidence-retention rule changed.");
Assert(CompaniesHouseReadinessService.MaskCompanyNumber("01234567") == "••••4567",
    "Companies House readiness did not bound the company number shown in the browser model.");
Assert(CompaniesHouseReadinessService.MaskCompanyNumber("  SC123456  ") == "••••3456",
    "Companies House readiness did not normalize and bound an alphanumeric company number.");
Assert(CompaniesHouseReadinessService.NormalizeCompanyNumber("123456") == "00123456"
       && CompaniesHouseReadinessService.NormalizeCompanyNumber(" sc123456 ") == "SC123456",
    "Companies House readiness did not canonicalise numeric registrations or preserve prefixed registrations.");
var octoberYearBounds = CompaniesHouseReadinessService.ResolvePeriodBounds([
    new DateTime(2027, 1, 1), new DateTime(2026, 10, 1), new DateTime(2027, 9, 1)]);
Assert(octoberYearBounds.PeriodStart == new DateTime(2026, 10, 1)
       && octoberYearBounds.LastPeriodStart == new DateTime(2027, 9, 1)
       && octoberYearBounds.PeriodEnd == new DateTime(2027, 9, 30),
    "Companies House readiness reverted to calendar-month ordering for an October financial year.");
var statutoryVersion = new SourceVersion("test", "01", null);
var nonVatContext = new StatutoryContextSnapshot(
    new("HOME", "Example Ltd", 0, "GB", "EW", "GBP", "01234567", null,
        "Example activity", 1, "Trading", "Registered", "Registered", [statutoryVersion]),
    [],
    [
        new("accounts", "STATUTORY-ACCOUNTS", "COMPANIES-HOUSE", "UK-CO-ACCTS-2026", null,
            true, "test", statutoryVersion),
        new("vat", "VAT", "HMRC", "HMRC-VAT", null, false, "test", statutoryVersion)
    ],
    [new("vat", "UNRELATED", "Text", "unreviewed", false, false, "test", statutoryVersion)],
    new(new DateOnly(2025, 10, 1), new DateOnly(2026, 9, 30)));
Assert(CompaniesHouseReadinessService.VerifyCompaniesHouseContext(nonVatContext, "accounts").Count == 0,
    "A non-VAT company or unrelated tax profile blocked Companies House readiness.");
var validFilingInputs = new TradeControl.Web.Pages.Tax.Hub.Models.TaxHubCompaniesHouseFilingInputDraft
{
    PeriodEnd = new DateOnly(2026, 9, 30),
    ApprovedOn = new DateTime(2026, 10, 8),
    SigningDirectorName = "Example Director",
    PrincipalActivity = "Software development",
    AccountingPolicies = "FRS 105 historical-cost basis.",
    AverageEmployees = 2,
    ConfirmsNoMaterialCommitmentsOrContingencies = true
};
Assert(new TradeControl.Web.Pages.Tax.Hub.Models.TaxHubCompaniesHouseFilingInputDraft()
        .ConfirmsNoMaterialCommitmentsOrContingencies == true,
    "The ordinary first-release filing eligibility answer does not default to confirmed none.");
var filingInputComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubCompaniesHouseFilingInputs.razor"));
Assert(new[] { "Opening balance", "Advances or credits", "Repayments", "Closing balance", "Add director" }
        .All(filingInputComponent.Contains),
    "The director movement schedule does not explain its four period-summary values.");
Assert(CompaniesHouseFilingInputReview.Validate(validFilingInputs, new DateOnly(2026, 10, 8)).Count == 0,
    "A valid reviewed Companies House filing-input draft was rejected.");
validFilingInputs.DirectorAdvances.Add(new()
{
    DirectorName = "Example Director", OpeningBalance = 10m, Advances = 5m,
    Repayments = 3m, ClosingBalance = 11m, Terms = "Repayable on demand"
});
Assert(CompaniesHouseFilingInputReview.Validate(validFilingInputs, new DateOnly(2026, 10, 8))
        .Any(error => error.Contains("does not reconcile", StringComparison.Ordinal)),
    "A non-reconciling director-advance disclosure was accepted.");
validFilingInputs.DirectorAdvances.Clear();
validFilingInputs.ConfirmsNoMaterialCommitmentsOrContingencies = false;
Assert(CompaniesHouseFilingInputReview.Validate(validFilingInputs, new DateOnly(2026, 10, 8))
        .Any(error => error.Contains("outside the first-release", StringComparison.Ordinal)),
    "A company with commitments or contingencies was accepted by the Accounts Mode filing gate.");
validFilingInputs.ConfirmsNoMaterialCommitmentsOrContingencies = null;
Assert(CompaniesHouseFilingInputReview.Validate(validFilingInputs, new DateOnly(2026, 10, 8))
        .Any(error => error.StartsWith("Confirm whether", StringComparison.Ordinal)),
    "An unanswered commitments/contingencies eligibility decision was accepted.");
validFilingInputs.ConfirmsNoMaterialCommitmentsOrContingencies = true;
var prohibitedBalanceSheetOverrides = new[]
{
    "PrepaymentsAndAccruedIncome", "ComparativePrepaymentsAndAccruedIncome",
    "Provisions", "ComparativeProvisions",
    "AccrualsAndDeferredIncome", "ComparativeAccrualsAndDeferredIncome"
};
Assert(typeof(TradeControl.Web.Pages.Tax.Hub.Models.TaxHubCompaniesHouseFilingInputDraft).GetProperties()
        .All(property => !prohibitedBalanceSheetOverrides.Contains(property.Name, StringComparer.Ordinal)),
    "The browser filing-input model permits a balance-sheet value to override source accounting evidence.");
Assert(typeof(TradeControl.Web.Pages.Tax.Hub.Models.TaxHubCompaniesHouseFilingInputDraft).GetProperties()
        .All(property => prohibitedCompaniesHouseInputs.All(prohibited =>
            !property.Name.Contains(prohibited, StringComparison.OrdinalIgnoreCase))),
    "The reviewed filing-input model accepts identity, credentials or transport content.");
Assert(typeof(ICompaniesHouseReadinessService).GetMethods()
        .SelectMany(method => method.GetParameters())
        .All(parameter => prohibitedCompaniesHouseInputs.All(prohibited =>
            !parameter.Name!.Contains(prohibited, StringComparison.OrdinalIgnoreCase))),
    "The Companies House readiness boundary accepts caller-supplied identity, credentials or statutory content.");
var readinessProperties = typeof(TradeControl.Web.Pages.Tax.Hub.Models.TaxHubCompaniesHouseReadiness)
    .GetProperties();
Assert(readinessProperties.All(property => new[] { "xml", "ixbrl", "credential", "authentication", "presenter" }
        .All(prohibited => !property.Name.Contains(prohibited, StringComparison.OrdinalIgnoreCase))),
    "The Companies House readiness browser model exposes protected or unrestricted content.");
Assert(readinessProperties.Any(property => property.Name == "ExternalRequestMade")
       && readinessProperties.Any(property => property.Name == "IsExactDocumentPrepared"),
    "The readiness browser model does not distinguish assessment from preparation and external exchange.");
var preparationContract = typeof(ICompaniesHousePreparationReviewService);
Assert(preparationContract.GetMethods().SelectMany(method => method.GetParameters())
        .All(parameter => prohibitedCompaniesHouseInputs.All(prohibited =>
            !parameter.Name!.Contains(prohibited, StringComparison.OrdinalIgnoreCase))),
    "The exact-document review boundary accepts caller-supplied identity, credentials or statutory content.");
Assert(filingInputComponent.Contains("Prepare exact accounts", StringComparison.Ordinal)
       && filingInputComponent.Contains("Prepared — not approved or filed", StringComparison.Ordinal)
       && filingInputComponent.Contains("Download filing XHTML", StringComparison.Ordinal)
       && filingInputComponent.Contains("Download draft PDF", StringComparison.Ordinal)
       && filingInputComponent.Contains("Approve accounts — does not file", StringComparison.Ordinal)
       && filingInputComponent.Contains("Approved — not filed", StringComparison.Ordinal)
       && !filingInputComponent.Contains(">Submit<", StringComparison.Ordinal),
    "The Phase 7.4 UI does not clearly separate exact preparation from approval and submission.");
var approvalPolicy = new CompaniesHouseFilingAuthorisationPolicy();
var administrator = new ClaimsPrincipal(new ClaimsIdentity([
    new Claim(ClaimTypes.NameIdentifier, "admin"),
    new Claim(ClaimTypes.Role, Constants.AdministratorsRole)
], "test"));
var ordinaryUser = new ClaimsPrincipal(new ClaimsIdentity([
    new Claim(ClaimTypes.NameIdentifier, "user")
], "test"));
Assert(approvalPolicy.CanApproveAccounts(administrator)
       && !approvalPolicy.CanApproveAccounts(ordinaryUser)
       && CompaniesHouseApprovalDeclaration.Sha256.Length == 64,
    "Companies House approval lost its privileged-role or versioned-declaration boundary.");
var companiesHouseReviewController = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Controllers",
    "TaxHubCompaniesHouseController.cs"));
Assert(companiesHouseReviewController.Contains("frame-ancestors 'self'", StringComparison.Ordinal)
       && companiesHouseReviewController.Contains("no-store", StringComparison.Ordinal)
       && companiesHouseReviewController.Contains("Preparation/{reference}/Download", StringComparison.Ordinal)
       && companiesHouseReviewController.Contains("companies-house-accounts-draft.xhtml", StringComparison.Ordinal)
       && filingInputComponent.Contains("sandbox=\"\" referrerpolicy=\"no-referrer\"", StringComparison.Ordinal),
    "The retained Companies House document endpoint lost its safe inline-review controls.");
var retainedDownloadBytes = new byte[] { 0x3c, 0x68, 0x74, 0x6d, 0x6c, 0x3e };
var retainedPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
var downloadController = new TaxHubCompaniesHouseController(
    new StubCompaniesHousePreparationReviewService(retainedDownloadBytes),
    new StubCompaniesHouseDraftPdfRenderer(retainedPdfBytes))
{
    ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
};
var downloadResult = await downloadController.Download("chp1-test", CancellationToken.None)
    as FileContentResult;
Assert(downloadResult is not null
       && downloadResult.FileContents.SequenceEqual(retainedDownloadBytes)
       && downloadResult.ContentType == "application/xhtml+xml"
       && downloadResult.FileDownloadName == "companies-house-accounts-draft.xhtml"
       && downloadController.Response.Headers.CacheControl == "no-store, max-age=0",
    "The accounts-draft download does not return the exact retained bytes as a non-cacheable XHTML attachment.");
var pdfDownloadResult = await downloadController.DraftPdf("chp1-test", CancellationToken.None)
    as FileContentResult;
Assert(pdfDownloadResult is not null
       && pdfDownloadResult.FileContents.SequenceEqual(retainedPdfBytes)
       && pdfDownloadResult.ContentType == "application/pdf"
       && pdfDownloadResult.FileDownloadName == "companies-house-accounts-draft.pdf"
       && downloadController.Response.Headers["X-Source-Document-SHA256"] == "TEST-DIGEST",
    "The draft-PDF route does not bind its derivative to the retained source document.");
var pdfFixture = """
<?xml version="1.0" encoding="utf-8"?>
<html xmlns="http://www.w3.org/1999/xhtml"><body><div class="accounts-document">
<h1>Example Limited</h1><p class="identity">Registered number: 01234567</p>
<h2>Filleted micro-entity accounts</h2><h2>Balance sheet as at 2026-09-30</h2>
<table class="accounts"><thead><tr><th></th><th>2026 £'s</th><th>2025 £'s</th></tr></thead>
<tbody><tr><td>Fixed assets</td><td>6,800.00</td><td>3,000.00</td></tr>
<tr class="total"><td>Capital and reserves</td><td>455,817.79</td><td>318,406.91</td></tr></tbody></table>
<div class="statements"><h2>Statements</h2><p>These accounts have been prepared in accordance with the micro-entity provisions.</p></div>
<div class="filing-information"><h2>Accounts information</h2><ul><li>Accounts type: Filleted accounts</li></ul></div>
</div></body></html>
""";
var renderedPdf = new CompaniesHouseDraftPdfRenderer().Render(
    Encoding.UTF8.GetBytes(pdfFixture), new string('A', 64));
Assert(renderedPdf.Length > 1000 && Encoding.ASCII.GetString(renderedPdf, 0, 5) == "%PDF-",
    "The draft-PDF renderer did not produce a substantive PDF document.");
var pdfOutputArgument = args.FirstOrDefault(value => value.StartsWith("--pdf-output=", StringComparison.Ordinal));
if (pdfOutputArgument is not null)
{
    var pdfOutputPath = Path.GetFullPath(pdfOutputArgument["--pdf-output=".Length..]);
    Directory.CreateDirectory(Path.GetDirectoryName(pdfOutputPath)!);
    var pdfSourceArgument = args.FirstOrDefault(value => value.StartsWith("--pdf-source=", StringComparison.Ordinal));
    if (pdfSourceArgument is null)
    {
        File.WriteAllBytes(pdfOutputPath, renderedPdf);
    }
    else
    {
        var sourceBytes = File.ReadAllBytes(Path.GetFullPath(pdfSourceArgument["--pdf-source=".Length..]));
        var sourceDigest = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(sourceBytes));
        File.WriteAllBytes(pdfOutputPath, new CompaniesHouseDraftPdfRenderer().Render(sourceBytes, sourceDigest));
    }
}
var taxHubShell = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "TaxHubShell.razor"));
Assert(taxHubShell.Contains("_state?.SelectedWorkspace == TaxHubWorkspace.Accounts", StringComparison.Ordinal)
       && taxHubShell.Contains("LastOrDefault()?.StartOn", StringComparison.Ordinal),
    "The Accounts workspace no longer selects the chosen financial year's final period.");
var taxHubService = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub",
    "TaxHubService.cs"));
Assert(taxHubService.Contains("t.YearNumber == selectedYear && t.StartOn == periodStartOn.Value",
           StringComparison.Ordinal)
       && taxHubService.Contains("does not belong to the selected financial year", StringComparison.Ordinal),
    "The Accounts service accepts a period from a different financial year.");
var companiesHouseReviewService = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices",
    "TaxHub", "CompaniesHouse", "CompaniesHousePreparationReviewService.cs"));
Assert(companiesHouseReviewService.Contains("text/html; charset=utf-8", StringComparison.Ordinal),
    "The exact retained Companies House document is not exposed through a browser-renderable review media type.");
Assert(companiesHouseReviewService.Contains("CanApproveAccounts", StringComparison.Ordinal)
       && companiesHouseReviewService.Contains("reviewed-input", StringComparison.Ordinal)
       && companiesHouseReviewService.Contains("VerifyCurrentCandidateAsync", StringComparison.Ordinal)
       && companiesHouseReviewService.Contains("identity.AspNetSubjectReference", StringComparison.Ordinal)
       && companiesHouseReviewService.Contains("CompaniesHouseApprovalDeclaration.Sha256", StringComparison.Ordinal),
    "Companies House immutable approval lost role, principal, retained-input, stale-source or declaration binding.");
var companiesHousePreparer = File.ReadAllText(Path.Combine(root, "src", "tax-hub", "src",
    "TradeControl.Tax.UK.Application", "Preparation", "CompaniesHouseAccountsPreparer.cs"));
Assert(companiesHousePreparer.Contains("CH-TRANSPORT-MATERIALISATION-REQUIRED", StringComparison.Ordinal)
       && !companiesHousePreparer.Contains("CH-PENDING-DEVELOPER-TEST", StringComparison.Ordinal),
    "The exact preparation path still claims developer-test acceptance is pending or lost its transport boundary.");
Assert(typeof(AzureCompaniesHousePersistence).GetInterfaces().Contains(typeof(ICompaniesHouseWorkflowStore))
       && typeof(AzureCompaniesHousePersistence).GetInterfaces()
           .Contains(typeof(ICompaniesHouseProtectedContentStore)),
    "The Azure composition does not provide both workflow metadata and protected-content boundaries.");
var companiesHouseDdl = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub",
    "CompaniesHouse", "Sql", "CompaniesHouseWorkflow.sql"));
Assert(companiesHouseDdl.Contains("PRIMARY KEY (TenantReference, Reference)", StringComparison.Ordinal)
       && companiesHouseDdl.Contains("WHERE IsActive = 1", StringComparison.Ordinal),
    "Companies House workflow SQL lost tenant partitioning or one-active-filing enforcement.");
Assert(companiesHouseDdl.Contains("FOREIGN KEY (TenantReference, PreparationReference)",
           StringComparison.Ordinal)
       && companiesHouseDdl.Contains("FOREIGN KEY (TenantReference, ApprovalReference)",
           StringComparison.Ordinal),
    "Companies House workflow SQL no longer preserves its tenant-scoped evidence chain.");
Assert(companiesHouseDdl.Contains("UX_CompaniesHouseApproval_Preparation", StringComparison.Ordinal),
    "Companies House workflow SQL permits competing approvals for one exact preparation.");
Assert(!companiesHouseDdl.Contains("DROP TABLE", StringComparison.OrdinalIgnoreCase)
       && !companiesHouseDdl.Contains("DELETE FROM", StringComparison.OrdinalIgnoreCase),
    "The additive Companies House workflow deployment script contains destructive DDL or DML.");

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
        "Review return", "History/readback", "Already submitted to HMRC",
        "separate from local forecast dates" }
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
Assert(vatWorkspaceComponent.Contains("IsMobile", StringComparison.Ordinal)
    && vatWorkspaceComponent.Contains("\"HMRC\" : \"HMRC Obligations\"", StringComparison.Ordinal),
    "The VAT tab labels no longer adapt to the mobile workspace.");
var taxHubShellComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "TaxHubShell.razor"));
var mobileGridStart = taxHubShellComponent.IndexOf("<TaxHubGrid", StringComparison.Ordinal);
var mobileGridEnd = taxHubShellComponent.IndexOf("/>", mobileGridStart, StringComparison.Ordinal);
Assert(mobileGridStart >= 0 && mobileGridEnd > mobileGridStart
    && taxHubShellComponent[mobileGridStart..mobileGridEnd].Contains("VatFilingEnabled=\"@ShowHmrcConnection\"", StringComparison.Ordinal),
    "The mobile Tax Hub no longer exposes the HMRC VAT filing workspace when configured.");
foreach (var vatGridFile in new[]
{
    "TaxHubVatStatementGrid.razor",
    "TaxHubVatTotalsGrid.razor",
    "TaxHubVatPeriodsGrid.razor"
})
{
    var vatGrid = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub", "Components", vatGridFile));
    Assert(vatGrid.Contains("Breakpoint=\"Breakpoint.Md\"", StringComparison.Ordinal)
        && !vatGrid.Contains("Breakpoint=\"Breakpoint.None\"", StringComparison.Ordinal),
        $"{vatGridFile} no longer enables MudBlazor's responsive card layout.");
}
foreach (var responsiveGridFile in new[]
{
    "TaxHubObligationsGrid.razor",
    "TaxHubBusinessTaxTotalsGrid.razor",
    "TaxHubBusinessTaxStatementGrid.razor",
    "TaxHubBusinessTaxLossesGrid.razor",
    "TaxHubBusinessTaxSubmissionGrid.razor",
    "TaxHubBusinessTaxPayloadGrid.razor",
    "TaxHubProfitAndLossGrid.razor",
    "TaxHubProfitAndLossDetailGrid.razor",
    "TaxHubBalanceSheetGrid.razor"
})
{
    var responsiveGrid = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub", "Components", responsiveGridFile));
    Assert(responsiveGrid.Contains("Breakpoint=\"Breakpoint.Md\"", StringComparison.Ordinal)
        && !responsiveGrid.Contains("Breakpoint=\"Breakpoint.None\"", StringComparison.Ordinal),
        $"{responsiveGridFile} no longer enables MudBlazor's responsive card layout.");
}
var businessTaxWorkspaceComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubBusinessTaxWorkspace.razor"));
var accountsWorkspaceComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubAccountsWorkspace.razor"));
Assert(businessTaxWorkspaceComponent.Contains("IsMobile ? \"Totals\"", StringComparison.Ordinal)
    && businessTaxWorkspaceComponent.Contains("IsMobile ? \"HMRC\"", StringComparison.Ordinal)
    && accountsWorkspaceComponent.Contains("IsMobile ? \"Annual\"", StringComparison.Ordinal)
    && accountsWorkspaceComponent.Contains("IsMobile ? \"Balance\"", StringComparison.Ordinal),
    "Business Tax or Accounts no longer provides compact mobile tab labels.");
var dashboardObligationsGrid = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubObligationsGrid.razor"));
var dashboardComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubDashboard.razor"));
Assert(dashboardObligationsGrid.Contains("@if (Items.Count > 10)", StringComparison.Ordinal)
    && dashboardObligationsGrid.Contains("RowsPerPage=\"10\"", StringComparison.Ordinal),
    "The Dashboard obligations summary always renders a pager or lost its bounded page size.");
Assert(dashboardComponent.Contains("A VAT filing needs confirmation", StringComparison.Ordinal)
    && dashboardComponent.Contains("Do not submit the return again", StringComparison.Ordinal)
    && dashboardComponent.Contains("Reconcile with HMRC", StringComparison.Ordinal),
    "The Dashboard no longer restores actionable filing-reconciliation guidance after reconnect or restart.");
var taxHubGridComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubGrid.razor"));
var taxHubCss = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "wwwroot", "css", "modules", "taxHub.css"));
Assert(taxHubGridComponent.Contains("tc-tax-hub-grid-dashboard", StringComparison.Ordinal)
    && taxHubCss.Contains(".tc-tax-hub-grid-dashboard", StringComparison.Ordinal)
    && taxHubCss.Contains("overflow-x: hidden", StringComparison.Ordinal),
    "The Dashboard can inherit the register workspace's horizontal scroller.");
Assert(obligationComponent.Contains("tc-vat-obligations-list", StringComparison.Ordinal)
    && obligationComponent.Contains("data-label=\"Status\"", StringComparison.Ordinal),
    "The HMRC obligation rows no longer provide their mobile card labels.");
var obligationService = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "VatObligationWorkspaceService.cs"));
Assert(obligationService.Contains("nodeContext.EventLog", StringComparison.Ordinal)
    && obligationService.Contains("nodeContext.ErrorLog", StringComparison.Ordinal)
    && obligationService.Contains("readback.ExistsAsync", StringComparison.Ordinal)
    && obligationService.Contains("AuthorityReturnAlreadyFiled", StringComparison.Ordinal)
    && obligationService.Contains("Cash.vwTaxVatIdentity", StringComparison.Ordinal)
    && obligationService.Contains("Support reference:", StringComparison.Ordinal),
    "The obligations workspace lost VAT identity validation, authority preflight or Event Log diagnostics.");
var reportingProfilePanel = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Admin",
    "Manager", "Components", "ReportingProfilePanel.razor"));
Assert(reportingProfilePanel.Contains("VAT registration number", StringComparison.Ordinal)
    && reportingProfilePanel.Contains("_subjectVatNumber", StringComparison.Ordinal)
    && reportingProfilePanel.Contains("Maintained in the reporting subject's organisation details", StringComparison.Ordinal),
    "Indirect-tax reporting still exposes an ambiguous independently editable authority reference.");
var profileSaveSql = File.ReadAllText(Path.Combine(root, "src", "sqlnode", "src", "tcNodeDb4", "Cash",
    "Stored Procedures", "proc_ReportingProfileSave.sql"));
var readinessSql = File.ReadAllText(Path.Combine(root, "src", "sqlnode", "src", "tcNodeDb4", "App",
    "Functions", "fnStatutoryContextReadiness.sql"));
Assert(profileSaveSql.Contains("SET @AuthorityReference = @SubjectVatNumber", StringComparison.Ordinal)
    && readinessSql.Contains("VAT-IDENTITY-MISMATCH", StringComparison.Ordinal)
    && readinessSql.Contains("Cash.vwTaxVatIdentity", StringComparison.Ordinal),
    "The database no longer derives or validates the indirect-tax VAT identity from the reporting subject.");
var reportingTypeSql = File.ReadAllText(Path.Combine(root, "src", "sqlnode", "src", "tcNodeDb4", "App",
    "Tables", "tbReportingType.sql"));
Assert(reportingTypeSql.Contains("[ReportingTypeCode] SMALLINT", StringComparison.Ordinal)
    && reportingTypeSql.Contains("[ReportingTypeName] NVARCHAR", StringComparison.Ordinal),
    "The reporting-type catalogue no longer follows the numeric enum schema convention.");
var alignmentSql = File.ReadAllText(Path.Combine(root, "src", "sqlnode", "src", "tcNodeDb4", "App",
    "Stored Procedures", "proc_DatasetSyntheticMIS_VatSandboxAlign.sql"));
Assert(alignmentSql.Contains("ValueSourceCode = N'SYNTHETIC'", StringComparison.Ordinal)
    && alignmentSql.Contains("Cash.vwTaxVatSubmission", StringComparison.Ordinal)
    && alignmentSql.Contains("Cash.proc_ReportingProfileSave", StringComparison.Ordinal)
    && alignmentSql.Contains("UPDATE Subject.tbVirtual", StringComparison.Ordinal)
    && alignmentSql.Contains("@SandboxVrn = N'999000001'", StringComparison.Ordinal),
    "The disposable VAT alignment guard or authoritative calculation/configuration boundary changed.");

var returnReview = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubVatReturnReview.razor"));
Assert(returnReview.Contains("The values below are read from the immutable prepared request", StringComparison.Ordinal)
    && returnReview.Contains("following day as the exclusive period boundary", StringComparison.Ordinal)
    && returnReview.Contains("Approve return — does not submit", StringComparison.Ordinal)
    && returnReview.Contains("Not yet submitted to HMRC", StringComparison.Ordinal)
    && returnReview.Contains("Final step — submit to HMRC", StringComparison.Ordinal),
    "The Phase 6.3 exact-review boundary or separate submit gate is no longer explicit in the UI.");
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
Assert(returnReview.Contains("Submit VAT return to HMRC", StringComparison.Ordinal)
    && returnReview.Contains("@bind=\"_submitConfirmed\"", StringComparison.Ordinal)
    && returnReview.Contains("taxHubHmrc.captureClientFacts", StringComparison.Ordinal)
    && returnReview.Contains("AuthorityErrors", StringComparison.Ordinal)
    && returnReview.Contains("VatAuthorityUserMessages.Submission", StringComparison.Ordinal),
    "The controlled submit confirmation, rejection detail, fresh fraud capture or unknown-outcome warning is missing.");
var submissionService = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "VatReturnSubmissionService.cs"));
Assert(submissionService.Contains("IVatApprovedReturnResolver", StringComparison.Ordinal)
    && submissionService.Contains("VatSubmissionResultState.OutcomeUnknown", StringComparison.Ordinal)
    && submissionService.Contains("Succeeded when dispatched.Receipt is not null", StringComparison.Ordinal)
    && submissionService.Contains("Automatic replay remains blocked pending reconciliation", StringComparison.Ordinal)
    && submissionService.Contains("nodeContext.EventLog", StringComparison.Ordinal)
    && submissionService.Contains("dispatched.Errors", StringComparison.Ordinal)
    && !submissionService.Contains("HttpClient", StringComparison.Ordinal)
    && !submissionService.Contains("JsonSerializer", StringComparison.Ordinal),
    "TCWeb submission bypasses the durable approval/gateway boundary or lacks safe outcome diagnostics.");
var connectionService = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "VatHmrcConnectionService.cs"));
Assert(connectionService.Contains("_gateway.SendAsync(approved.Request", StringComparison.Ordinal)
    && connectionService.Contains("vat-return-preflight", StringComparison.Ordinal)
    && connectionService.Contains("ActiveSubmissionAttemptException", StringComparison.Ordinal)
    && connectionService.Contains("VatReturnReconciliation.Compare", StringComparison.Ordinal)
    && connectionService.Contains("ParseAuthorityErrors", StringComparison.Ordinal)
    && connectionService.Contains("FRAUD-CONTEXT-REQUIRED", StringComparison.Ordinal),
    "Approved dispatch lost duplicate prevention, exact readback reconciliation or fresh fraud evidence.");
Assert(reviewService.Contains("WithVerifiedBody(bytes)", StringComparison.Ordinal)
    && reviewService.Contains("GetApprovalAsync", StringComparison.Ordinal)
    && reviewService.Contains("approval.PreparedSha256 != candidate.PreparedSha256", StringComparison.Ordinal),
    "Submission no longer resolves and verifies the durable approval and retained exact bytes.");
var filingHistory = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "VatFilingHistoryService.cs"));
var filingHistoryView = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubVatFilingHistory.razor"));
Assert(filingHistory.Contains("attempts.ListTenantAsync(identity.TenantReference", StringComparison.Ordinal)
    && filingHistory.Contains("filingPolicy.CanManageHmrcConnection", StringComparison.Ordinal)
    && filingHistory.Contains("attempt.PrincipalReference", StringComparison.Ordinal)
    && filingHistory.Contains("SHA256.HashData(bytes)", StringComparison.Ordinal)
    && filingHistory.Contains("IsVisible(approved, currentVrnDigest", StringComparison.Ordinal)
    && filingHistory.Contains("vat.returns.submit", StringComparison.Ordinal),
    "VAT filing history is not scoped to the server-derived tenant/principal or does not verify dispatched bytes.");
Assert(filingHistoryView.Contains("VAT filing history", StringComparison.Ordinal)
    && filingHistoryView.Contains("Approved return and audit references", StringComparison.Ordinal)
    && filingHistoryView.Contains("Prepared SHA-256", StringComparison.Ordinal)
    && filingHistoryView.Contains("MaskedVrn", StringComparison.Ordinal)
    && filingHistoryView.Contains("All nine retrieved VAT values exactly match", StringComparison.Ordinal)
    && filingHistoryView.Contains("Do not repeat an uncertain submission", StringComparison.Ordinal)
    && filingHistoryView.Contains("Reconcile with HMRC", StringComparison.Ordinal)
    && filingHistoryView.Contains("taxHubHmrc.captureClientFacts", StringComparison.Ordinal)
    && filingHistoryView.Contains("selected Tax Hub period", StringComparison.Ordinal)
    && !filingHistoryView.Contains("SafePayloadReference", StringComparison.Ordinal)
    && !filingHistoryView.Contains("SafeResponseReference", StringComparison.Ordinal),
    "The ordinary filing-history view lost its evidence summary, masked identity or protected-content boundary.");
Assert(connectionService.Contains("RecordReconciliationAsync", StringComparison.Ordinal)
    && connectionService.Contains("ReconcileApprovedAsync", StringComparison.Ordinal)
    && connectionService.Contains("VatReturnReconciliation.Compare", StringComparison.Ordinal)
    && connectionService.Contains("HMRC-SUCCESS-RECONCILED", StringComparison.Ordinal),
    "Exact authority readback is not durably attached to the immutable filing attempt.");
var visibleApproval = new VatApprovalEvidence("approval", "18A2", new DateOnly(2017, 4, 1),
    new DateOnly(2017, 6, 30), "*****8554", "actor", "VRN-DIGEST", "BODY-DIGEST",
    DateTimeOffset.UtcNow, [], true);
Assert(VatFilingHistoryService.IsVisible(visibleApproval, "VRN-DIGEST", new DateOnly(2017, 6, 30),
        new DateOnly(2017, 4, 1), new DateOnly(2018, 3, 31))
    && !VatFilingHistoryService.IsVisible(visibleApproval, "OLD-VRN", new DateOnly(2017, 6, 30), null, null)
    && !VatFilingHistoryService.IsVisible(visibleApproval, "VRN-DIGEST", new DateOnly(2017, 7, 1), null, null)
    && !VatFilingHistoryService.IsVisible(visibleApproval, "VRN-DIGEST", new DateOnly(2017, 6, 30),
        new DateOnly(2015, 4, 1), new DateOnly(2016, 3, 31)),
    "Filing history did not enforce current VAT identity, adoption date and selected period boundaries.");

Assert(VatAuthorityUserMessages.Obligations("HMRC-CLIENT_OR_AGENT_NOT_AUTHORISED")
        .Contains("correct organisation account", StringComparison.OrdinalIgnoreCase)
    && VatAuthorityUserMessages.Obligations("HMRC-HTTP-429")
        .Contains("Wait", StringComparison.OrdinalIgnoreCase)
    && VatAuthorityUserMessages.Obligations("HMRC-SCHEDULED_MAINTENANCE")
        .Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase),
    "Documented HMRC obligation failures no longer have actionable, non-technical user guidance.");
var duplicateMessage = VatAuthorityUserMessages.Submission(new(VatSubmissionResultState.Rejected,
    "HMRC-BUSINESS_ERROR", "attempt", 403,
    AuthorityErrors: [new("DUPLICATE_SUBMISSION", "The period was already filed.")]));
Assert(duplicateMessage.Contains("already been filed", StringComparison.OrdinalIgnoreCase)
    && duplicateMessage.Contains("Do not submit", StringComparison.OrdinalIgnoreCase),
    "A duplicate HMRC submission no longer fails safely with reconciliation guidance.");
var connectionView = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubHmrcConnection.razor"));
Assert(connectionView.Contains("correct-errors-in-your-vat-return", StringComparison.Ordinal)
    && connectionView.Contains("www.gov.uk/pay-vat", StringComparison.Ordinal)
    && connectionView.Contains("Validate fraud headers", StringComparison.Ordinal),
    "The VAT connection panel lost required HMRC journey guidance or deployed fraud validation.");
var hmrcControllerSource = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Controllers",
    "TaxHubHmrcController.cs"));
Assert(hmrcControllerSource.Contains("FraudPrevention/Validate", StringComparison.Ordinal)
    && hmrcControllerSource.Contains("IVatFraudHeaderValidationService", StringComparison.Ordinal),
    "TCWeb no longer validates fraud headers through its own deployed product composition.");
var startupSource = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Startup.cs"));
Assert(startupSource.Contains("/health/live", StringComparison.Ordinal)
    && startupSource.Contains("/health/ready", StringComparison.Ordinal)
    && startupSource.Split(".AllowAnonymous();", StringSplitOptions.None).Length >= 3,
    "The App Service liveness/readiness probes are missing or protected by interactive authentication.");

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

static CompaniesHouseProductHostOptions ValidCompaniesHouseDevelopmentOptions() => new()
{
    Enabled = true,
    AuthorityEnvironment = CompaniesHouseAuthorityEnvironment.OfficialTest,
    PersistenceMode = CompaniesHousePersistenceMode.DevelopmentFiles,
    DispatchMode = CompaniesHouseDispatchMode.SendDisabled,
    TenantReference = Guid.NewGuid().ToString(),
    DevelopmentStoreRoot = Path.Combine(Path.GetTempPath(), "tax-hub-companies-house-tests")
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

file sealed class StubCompaniesHousePreparationReviewService(byte[] bytes)
    : ICompaniesHousePreparationReviewService
{
    public Task<CompaniesHousePreparedReview> PrepareAsync(short yearNumber, DateTime selectedPeriodStart,
        CompaniesHousePreparationInput input, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<CompaniesHouseReviewDocument> ReadDocumentAsync(string preparationReference,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new CompaniesHouseReviewDocument("text/html; charset=utf-8", bytes, "TEST-DIGEST"));

    public Task<CompaniesHouseApprovalReview> ApproveAsync(string preparationReference,
        string declarationVersion, bool declarationsConfirmed,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

file sealed class StubCompaniesHouseDraftPdfRenderer(byte[] bytes) : ICompaniesHouseDraftPdfRenderer
{
    public byte[] Render(byte[] retainedIxbrl, string sourceSha256) => bytes;
}
