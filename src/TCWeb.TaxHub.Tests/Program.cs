using System.Xml.Linq;
using System.Text;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.AppServices.TaxHub.Vat;
using TradeControl.Web.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Controllers;
using TradeControl.Web.Data;
using TradeControl.Web.Tenancy;
using TradeControl.Tax.UK.Hmrc.Vat.v1_0.Returns;
using TradeControl.Tax.UK.Application.DataProvision;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.CompaniesHouse.Accounts.Tis6_0;

var assertions = 0;
void Assert(bool condition, string message)
{
    assertions++;
    if (!condition) throw new InvalidOperationException(message);
}

async Task AssertThrowsAsync<TException>(Func<Task> action, string message)
    where TException : Exception
{
    assertions++;
    try
    {
        await action();
    }
    catch (TException)
    {
        return;
    }
    throw new InvalidOperationException(message);
}

var root = FindRoot(AppContext.BaseDirectory);
var tenantAmbient = new TenantAmbientContext();
using (var tenantContext = new TenantContext(tenantAmbient))
{
    const string tenantConnection = "Server=tenant-factory;Database=tenant-factory;Integrated Security=True;TrustServerCertificate=True";
    tenantContext.Bind(new TenantDescriptor(Guid.NewGuid(), "tenant-factory.example", "TenantFactory",
        tenantConnection));
    var nodeFactory = new TenantNodeContextFactory(tenantContext);
    await using var firstNodeContext = await nodeFactory.CreateDbContextAsync();
    await using var secondNodeContext = await nodeFactory.CreateDbContextAsync();
    Assert(!ReferenceEquals(firstNodeContext, secondNodeContext)
        && firstNodeContext.Database.GetConnectionString()?.Contains("tenant-factory",
            StringComparison.Ordinal) == true
        && secondNodeContext.Database.GetConnectionString()?.Contains("tenant-factory",
            StringComparison.Ordinal) == true,
        "Concurrent tenant operations do not receive independent tenant-bound node contexts.");
}
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
assertions += await CashStatementServiceTests.RunAsync();
assertions += await TenantIsolationTests.RunAsync();
assertions += BusinessClockTests.Run();
assertions += TenantFileProviderTests.Run();
assertions += await FileUploadValidationTests.RunAsync();
assertions += TenantAssetDownloadTests.Run();
assertions += await CustomerExitArchiveTests.RunAsync();
assertions += TemplateBootstrapTests.Run(root);
assertions += PlatformEmailTests.Run(root);
assertions += await OAuthBrokerTests.RunAsync();

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
    "Companies House development defaults must remain send-disabled.");
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
var companiesHouseOfficialTest = new CompaniesHouseProductHostOptions
{
    Enabled = true,
    AuthorityEnvironment = CompaniesHouseAuthorityEnvironment.OfficialTest,
    PersistenceMode = CompaniesHousePersistenceMode.AzureManaged,
    DispatchMode = CompaniesHouseDispatchMode.OfficialTestSingleSend,
    TenantReference = Guid.NewGuid().ToString(),
    KeyVaultUri = new Uri("https://example.vault.azure.net/"),
    WorkflowConnectionName = "TaxHubWorkflow",
    EvidenceBlobServiceUri = "https://example.blob.core.windows.net/",
    OfficialTestSubmissionSequenceFloor = 17
};
Assert(companiesHouseProductionValidator.Validate(null, companiesHouseOfficialTest).Succeeded,
    "The reviewed official-test-only single-send composition was rejected.");
companiesHouseOfficialTest.AuthorityEnvironment = CompaniesHouseAuthorityEnvironment.Production;
Assert(companiesHouseProductionValidator.Validate(null, companiesHouseOfficialTest).Failed,
    "Official-test dispatch was accepted against the production authority environment.");
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
var companyIdentity = CompaniesHouseFilingIdentity.Company("tenant-a", " 01234567 ");
var logicalFilingIdentity = CompaniesHouseFilingIdentity.Logical("tenant-a", companyIdentity,
    new DateOnly(2025, 10, 1), new DateOnly(2026, 9, 30),
    CompaniesHouseProductPolicy.SupportedFilingProfile);
Assert(companyIdentity == CompaniesHouseFilingIdentity.Company("tenant-a", "01234567")
       && logicalFilingIdentity == CompaniesHouseFilingIdentity.Logical("tenant-a", companyIdentity,
           new DateOnly(2025, 10, 1), new DateOnly(2026, 9, 30),
           CompaniesHouseProductPolicy.SupportedFilingProfile)
       && logicalFilingIdentity != CompaniesHouseFilingIdentity.Logical("tenant-a", companyIdentity,
           new DateOnly(2026, 10, 1), new DateOnly(2027, 9, 30),
           CompaniesHouseProductPolicy.SupportedFilingProfile),
    "Companies House company/period filing identities are not deterministic and period-specific.");
var stagingTenant = Guid.NewGuid().ToString();
var stagingDocumentSha = new string('A', 64);
var stagingPackageSha = new string('B', 64);
var stagingFullAccounts = Encoding.UTF8.GetBytes("full-accounts");
var stagingFullAccountsSha = Convert.ToHexString(SHA256.HashData(stagingFullAccounts));
var stagingReviewedInputSha = new string('E', 64);
var stagingAccountsSnapshotSha = CompaniesHouseAccountsSnapshot.Create(
    new string('C', 64), new DateOnly(2025, 10, 1), new DateOnly(2026, 9, 30),
    CompaniesHouseProductPolicy.SupportedFilingProfile, "snapshot", stagingReviewedInputSha,
    stagingDocumentSha, stagingFullAccountsSha);
var stagingPreparation = new CompaniesHousePreparationRecord(
    "chp1-stage", stagingTenant, "HOME", "SA", new string('C', 64), "••••4567",
    new DateOnly(2025, 10, 1), new DateOnly(2026, 9, 30),
    CompaniesHouseProductPolicy.SupportedFilingProfile, "TradeControl", "UK-CO-ACCTS-2026",
    "snapshot", stagingDocumentSha, stagingPackageSha, "document", "package",
    DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1),
    ReviewedInputSha256: stagingReviewedInputSha,
    FullAccountsSha256: stagingFullAccountsSha,
    ProtectedFullAccountsReference: "full-accounts",
    AccountsSnapshotSha256: stagingAccountsSnapshotSha);
var stagingApproval = new CompaniesHouseApprovalRecord(
    "cha1-stage", stagingPreparation.Reference, stagingTenant, "subject", "SA",
    new string('D', 64), stagingPreparation.CompanyIdentitySha256,
    stagingPreparation.PeriodStart, stagingPreparation.PeriodEnd, stagingPreparation.FilingProfile,
    CompaniesHouseApprovalDeclaration.Version, CompaniesHouseApprovalDeclaration.Sha256,
    stagingPreparation.SourceSystem, stagingPreparation.DatasetKey, stagingPreparation.SnapshotToken,
    stagingDocumentSha, stagingPackageSha, true, DateTimeOffset.UtcNow,
    stagingFullAccountsSha, stagingAccountsSnapshotSha);
var stagingStore = new StubCompaniesHouseWorkflowStore(stagingPreparation, stagingApproval);
var stagingContent = new StubCompaniesHouseProtectedContentStore(new Dictionary<string, byte[]>
{
    ["document"] = Encoding.UTF8.GetBytes("document"),
    ["package"] = Encoding.UTF8.GetBytes("package"),
    ["full-accounts"] = stagingFullAccounts
});
var stagingHttp = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
stagingHttp.HttpContext.User = Principal(Constants.AdministratorsRole);
var stagingService = new CompaniesHouseSubmissionStagingService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    stagingStore, stagingContent, new CompaniesHouseFilingAuthorisationPolicy(), stagingHttp,
    Options.Create(new CompaniesHouseProductHostOptions
    {
        Enabled = true,
        DispatchMode = CompaniesHouseDispatchMode.SendDisabled
    }), TimeProvider.System);
var accountsSnapshotService = new CompaniesHouseAccountsSnapshotService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    stagingStore, stagingContent);
var approvedAccountsSnapshot = await accountsSnapshotService.ReadApprovedAsync(stagingApproval.Reference);
Assert(approvedAccountsSnapshot.AccountsSnapshotSha256 == stagingAccountsSnapshotSha
       && approvedAccountsSnapshot.FullAccountsSha256 == stagingFullAccountsSha
       && approvedAccountsSnapshot.FullAccountsBytes.SequenceEqual(stagingFullAccounts),
    "The approved Companies House snapshot did not return the exact cryptographically paired full accounts.");
var legacySnapshotService = new CompaniesHouseAccountsSnapshotService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    new StubCompaniesHouseWorkflowStore(
        stagingPreparation with
        {
            FullAccountsSha256 = null,
            ProtectedFullAccountsReference = null,
            AccountsSnapshotSha256 = null
        },
        stagingApproval with { FullAccountsSha256 = null, AccountsSnapshotSha256 = null }),
    stagingContent);
await AssertThrowsAsync<InvalidOperationException>(
    () => legacySnapshotService.ReadApprovedAsync(stagingApproval.Reference),
    "A legacy Companies House approval without paired CH/CT evidence was accepted.");
var tamperedSnapshotService = new CompaniesHouseAccountsSnapshotService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    new StubCompaniesHouseWorkflowStore(
        stagingPreparation with { AccountsSnapshotSha256 = new string('F', 64) },
        stagingApproval with { AccountsSnapshotSha256 = new string('F', 64) }),
    stagingContent);
await AssertThrowsAsync<InvalidOperationException>(
    () => tamperedSnapshotService.ReadApprovedAsync(stagingApproval.Reference),
    "A tampered CH/CT accounts snapshot proof was accepted.");
var stagedFirst = await stagingService.StageApprovedAsync(stagingApproval.Reference);
var stagedAgain = await stagingService.StageApprovedAsync(stagingApproval.Reference);
await AssertThrowsAsync<InvalidOperationException>(
    () => accountsSnapshotService.ReadAcceptedForCorporationTaxAsync(stagingApproval.Reference),
    "A prepared but unaccepted Companies House conversation released the CT accounts hand-off.");
var recordingExchange = new StubCompaniesHouseSubmissionExchange("RECORDED0001");
var dispatchService = new CompaniesHouseSubmissionDispatchService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    stagingStore, stagingContent, new CompaniesHouseFilingAuthorisationPolicy(), stagingHttp,
    new StubCompaniesHouseAuthorityIdentifierSource(new("R00001", "RECORDED0001")),
    new StubCompaniesHouseSubmissionMaterializer(), recordingExchange, TimeProvider.System);
var dispatched = await dispatchService.DispatchAsync(stagedFirst.ConversationReference);
var dispatchedAgain = await dispatchService.DispatchAsync(stagedFirst.ConversationReference);
stagingStore.SetState(CompaniesHouseConversationState.Accepted);
var acceptedAccountsSnapshot = await accountsSnapshotService
    .ReadAcceptedForCorporationTaxAsync(stagingApproval.Reference);
var stagedAfterTerminal = await stagingService.StageApprovedAsync(stagingApproval.Reference);
Assert(stagedFirst.State == CompaniesHouseConversationState.Prepared
       && stagedFirst.ConversationReference == stagedAgain.ConversationReference
       && dispatched.State == CompaniesHouseConversationState.Acknowledged
       && dispatchedAgain.State == CompaniesHouseConversationState.Acknowledged
       && dispatched.SubmissionNumber == "R00001"
       && acceptedAccountsSnapshot.ConversationReference == stagedFirst.ConversationReference
       && acceptedAccountsSnapshot.Accounts.AccountsSnapshotSha256 == stagingAccountsSnapshotSha
       && recordingExchange.ExchangeCount == 1
       && stagedAfterTerminal.State == CompaniesHouseConversationState.Accepted
       && stagedAfterTerminal.ConversationReference == stagedFirst.ConversationReference
       && stagingStore.UpsertCount == 2
       && stagingStore.TryBeginDispatchCount == 1
       && stagingContent.ReadCount == 6
       && stagingContent.WriteCount == 2
       && stagingStore.ActiveConversation?.AuthorityTransactionId == "RECORDED0001",
    "The recording lifecycle did not retain exact evidence, claim once, separate acknowledgement, or prevent replay.");

var recoveryPreparation = stagingPreparation with
{
    Reference = "chp1-recovery",
    CompanyIdentitySha256 = new string('E', 64),
    ProtectedDocumentReference = "recovery-document",
    ProtectedPackageReference = "recovery-package"
};
var recoveryApproval = stagingApproval with
{
    Reference = "cha1-recovery",
    PreparationReference = recoveryPreparation.Reference,
    CompanyIdentitySha256 = recoveryPreparation.CompanyIdentitySha256,
    LogicalFilingIdentity = new string('F', 64)
};
var recoveryStore = new StubCompaniesHouseWorkflowStore(recoveryPreparation, recoveryApproval);
var recoveryContent = new StubCompaniesHouseProtectedContentStore(new Dictionary<string, byte[]>
{
    ["recovery-document"] = Encoding.UTF8.GetBytes("recovery-document"),
    ["recovery-package"] = Encoding.UTF8.GetBytes("recovery-package")
});
var recoveryStaging = new CompaniesHouseSubmissionStagingService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    recoveryStore, recoveryContent, new CompaniesHouseFilingAuthorisationPolicy(), stagingHttp,
    Options.Create(new CompaniesHouseProductHostOptions
    {
        Enabled = true,
        DispatchMode = CompaniesHouseDispatchMode.SendDisabled
    }), TimeProvider.System);
var recoveryConversation = await recoveryStaging.StageApprovedAsync(recoveryApproval.Reference);
var ambiguousExchange = new StubCompaniesHouseSubmissionExchange("RECORDED0002", fail: true);
var recoveryDispatch = new CompaniesHouseSubmissionDispatchService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    recoveryStore, recoveryContent, new CompaniesHouseFilingAuthorisationPolicy(), stagingHttp,
    new StubCompaniesHouseAuthorityIdentifierSource(new("R00002", "RECORDED0002")),
    new StubCompaniesHouseSubmissionMaterializer(), ambiguousExchange, TimeProvider.System);
var uncertain = await recoveryDispatch.DispatchAsync(recoveryConversation.ConversationReference);
var uncertainAgain = await recoveryDispatch.DispatchAsync(recoveryConversation.ConversationReference);
Assert(uncertain.State == CompaniesHouseConversationState.Dispatching
       && uncertain.RecoveryRequired && uncertain.SupportReference is not null
       && uncertainAgain.ConversationReference == uncertain.ConversationReference
       && ambiguousExchange.ExchangeCount == 1
       && recoveryStore.TryBeginDispatchCount == 1,
    "An ambiguous Companies House exchange was not retained as recovery-required or was replayed.");

var statusPreparation = stagingPreparation with
{
    Reference = "chp1-status",
    CompanyIdentitySha256 = new string('1', 64),
    ProtectedDocumentReference = "status-document",
    ProtectedPackageReference = "status-package"
};
var statusApproval = stagingApproval with
{
    Reference = "cha1-status",
    PreparationReference = statusPreparation.Reference,
    CompanyIdentitySha256 = statusPreparation.CompanyIdentitySha256,
    LogicalFilingIdentity = new string('2', 64)
};
var statusStore = new StubCompaniesHouseWorkflowStore(statusPreparation, statusApproval);
var statusContent = new StubCompaniesHouseProtectedContentStore(new Dictionary<string, byte[]>
{
    ["status-document"] = Encoding.UTF8.GetBytes("status-document"),
    ["status-package"] = Encoding.UTF8.GetBytes("status-package")
});
var statusStaging = new CompaniesHouseSubmissionStagingService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    statusStore, statusContent, new CompaniesHouseFilingAuthorisationPolicy(), stagingHttp,
    Options.Create(new CompaniesHouseProductHostOptions
    {
        Enabled = true,
        DispatchMode = CompaniesHouseDispatchMode.SendDisabled
    }), TimeProvider.System);
var statusConversation = await statusStaging.StageApprovedAsync(statusApproval.Reference);
var statusDispatch = new CompaniesHouseSubmissionDispatchService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    statusStore, statusContent, new CompaniesHouseFilingAuthorisationPolicy(), stagingHttp,
    new StubCompaniesHouseAuthorityIdentifierSource(new("R00003", "RECORDED0003")),
    new StubCompaniesHouseSubmissionMaterializer(),
    new StubCompaniesHouseSubmissionExchange("RECORDED0003"), TimeProvider.System);
var statusAcknowledgement = await statusDispatch.DispatchAsync(statusConversation.ConversationReference);
var originalRequestSha = statusStore.ActiveConversation!.LastRequestSha256;
var statusExchange = new StubCompaniesHouseStatusExchange("R00003",
    CompaniesHouseSubmissionStatusCode.Pending, CompaniesHouseSubmissionStatusCode.Accept);
var statusService = new CompaniesHouseSubmissionStatusService(
    new StubCompaniesHouseWorkflowIdentityAccessor(new(stagingTenant, "subject", "SA", "HOME")),
    statusStore, statusContent, new CompaniesHouseFilingAuthorisationPolicy(), stagingHttp,
    new StubCompaniesHouseStatusTransactionIdSource("STATUS0001", "STATUS0002"),
    new StubCompaniesHouseStatusRequestMaterializer(), statusExchange, TimeProvider.System);
var pendingStatus = await statusService.CheckAsync(statusAcknowledgement.ConversationReference);
var acceptedStatus = await statusService.CheckAsync(statusAcknowledgement.ConversationReference);
var acceptedAgain = await statusService.CheckAsync(statusAcknowledgement.ConversationReference);
Assert(pendingStatus.State == CompaniesHouseConversationState.Pending
       && acceptedStatus.State == CompaniesHouseConversationState.Accepted
       && acceptedAgain.State == CompaniesHouseConversationState.Accepted
       && statusExchange.ExchangeCount == 2
       && statusStore.TryBeginStatusCheckCount == 2
       && statusStore.ActiveConversation?.StatusChecks?.Count == 2
       && statusStore.ActiveConversation.StatusChecks.All(check =>
           check.CheckState == CompaniesHouseStatusCheckState.Completed
           && check.ResponseSha256 is not null && check.ProtectedResponseReference is not null)
       && statusStore.ActiveConversation.LastRequestSha256 == originalRequestSha
       && !statusStore.ActiveConversation.StatusAcknowledgementRequired
       && !statusStore.ActiveConversation.StatusAcknowledged,
    "Specific Companies House status checks did not preserve the filing exchange, progress Pending to Accepted, or stop after the terminal result.");
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
       && filingInputComponent.Contains("Download official-test submitted PDF", StringComparison.Ordinal)
       && filingInputComponent.Contains("OFFICIAL TEST — NOT LIVE FILED", StringComparison.Ordinal)
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
var submittedPdfDownloadResult = await downloadController.OfficialTestSubmissionPdf(
        "chp1-test", CancellationToken.None) as FileContentResult;
Assert(submittedPdfDownloadResult is not null
       && submittedPdfDownloadResult.FileContents.SequenceEqual(retainedPdfBytes)
       && submittedPdfDownloadResult.ContentType == "application/pdf"
       && submittedPdfDownloadResult.FileDownloadName ==
          "companies-house-accounts-official-test-S00017.pdf"
       && downloadController.Response.Headers["X-Companies-House-Evidence-Class"] ==
          "OFFICIAL TEST - NOT LIVE FILED",
    "The post-dispatch PDF route is not constrained to clearly labelled official-test evidence.");
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
var renderedSubmittedPdf = new CompaniesHouseDraftPdfRenderer().RenderOfficialTestSubmission(
    Encoding.UTF8.GetBytes(pdfFixture), new string('A', 64), "S00017",
    new DateTimeOffset(2026, 10, 12, 12, 0, 0, TimeSpan.Zero));
Assert(renderedSubmittedPdf.Length > 1000
       && Encoding.ASCII.GetString(renderedSubmittedPdf, 0, 5) == "%PDF-",
    "The official-test submitted-copy renderer did not produce a substantive PDF document.");
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
        var submittedNumberArgument = args.FirstOrDefault(value =>
            value.StartsWith("--official-test-submission=", StringComparison.Ordinal));
        var pdfBytes = submittedNumberArgument is null
            ? new CompaniesHouseDraftPdfRenderer().Render(sourceBytes, sourceDigest)
            : new CompaniesHouseDraftPdfRenderer().RenderOfficialTestSubmission(sourceBytes, sourceDigest,
                submittedNumberArgument["--official-test-submission=".Length..], DateTimeOffset.UtcNow);
        File.WriteAllBytes(pdfOutputPath, pdfBytes);
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
var companiesHouseContinuationService = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices",
    "TaxHub", "CompaniesHouse", "CompaniesHouseFilingContinuationService.cs"));
var appServiceRegistration = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices",
    "ServiceCollectionExtensions.cs"));
Assert(companiesHouseReviewService.Contains("text/html; charset=utf-8", StringComparison.Ordinal),
    "The exact retained Companies House document is not exposed through a browser-renderable review media type.");
Assert(companiesHouseReviewService.Contains("CanApproveAccounts", StringComparison.Ordinal)
       && companiesHouseReviewService.Contains("reviewed-input", StringComparison.Ordinal)
       && companiesHouseReviewService.Contains("VerifyCurrentCandidateAsync", StringComparison.Ordinal)
       && companiesHouseReviewService.Contains("identity.AspNetSubjectReference", StringComparison.Ordinal)
       && companiesHouseReviewService.Contains("CompaniesHouseApprovalDeclaration.Sha256", StringComparison.Ordinal),
    "Companies House immutable approval lost role, principal, retained-input, stale-source or declaration binding.");
Assert(companiesHouseReviewService.Contains("Continue that filing instead of preparing another submission",
           StringComparison.Ordinal)
       && companiesHouseContinuationService.Contains("ListConversationsAsync", StringComparison.Ordinal)
       && companiesHouseContinuationService.Contains("conversation.DocumentSha256", StringComparison.Ordinal)
       && companiesHouseContinuationService.Contains("conversation.PackageSha256", StringComparison.Ordinal),
    "Companies House preparation no longer blocks duplicate period filings or rehydrates digest-bound continuation state.");
Assert(appServiceRegistration.Contains("ICompaniesHouseSubmissionStagingService", StringComparison.Ordinal)
       && appServiceRegistration.Contains("CompaniesHouseDispatchMode.OfficialTestSingleSend", StringComparison.Ordinal)
       && appServiceRegistration.Contains("ICompaniesHouseSubmissionDispatchService", StringComparison.Ordinal)
       && appServiceRegistration.Contains("ICompaniesHouseFilingContinuationService", StringComparison.Ordinal)
       && appServiceRegistration.Contains("ICompaniesHouseSubmissionStatusService", StringComparison.Ordinal)
       && appServiceRegistration.Contains("CompaniesHouseOfficialTestStatusRequestMaterializer", StringComparison.Ordinal),
    "The reviewed staging, official-test dispatcher or specific-submission status composition is unavailable.");
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
       && companiesHouseDdl.Contains("WHERE IsActive = 1", StringComparison.Ordinal)
       && companiesHouseDdl.Contains("CompaniesHouseAuthoritySequence", StringComparison.Ordinal),
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
var disabledVatHistory = new DisabledVatFilingHistoryService();
Assert((await disabledVatHistory.GetAsync()).Count == 0,
    "A disabled VAT host must expose an empty filing history without constructing a persistence store.");
await AssertThrowsAsync<VatReturnReviewException>(
    () => disabledVatHistory.ReconcileAsync("attempt"),
    "A disabled VAT host must fail reconciliation closed with a safe product error.");

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
    ReviewEnabled = true,
    AuthorityEnvironment = VatAuthorityEnvironment.Sandbox,
    PersistenceMode = VatPersistenceMode.AzureManaged,
    TenantReference = Guid.NewGuid().ToString(),
    PublicOrigin = new Uri("https://tcweb.example.test/"),
    SandboxSecretSource = VatSandboxSecretSource.EnvironmentVariables,
    FraudPublicTlsAddresses = ["198.51.100.10"],
    KeyVaultUri = new Uri("https://example.vault.azure.net/"),
    WorkflowConnectionName = "TaxHubWorkflow",
    EvidenceBlobServiceUri = "https://example.blob.core.windows.net/"
};
Assert(productionValidator.Validate(null, azure).Succeeded,
    "The Azure-managed sandbox connection and read-only journey was rejected.");
azure.AllowIncompleteSandboxFraudHeaders = true;
Assert(productionValidator.Validate(null, azure).Succeeded,
    "The reviewed Azure-managed sandbox warning mode was rejected.");
azure.OAuthCallbackMode = VatOAuthCallbackMode.Brokered;
azure.OAuthBrokerStartUri = new Uri("https://auth.tradecontrol.co.uk/oauth/start");
azure.OAuthBrokerCallbackUri = new Uri("https://auth.tradecontrol.co.uk/TaxHub/HmrcCallback");
Assert(productionValidator.Validate(null, azure).Succeeded,
    "A fixed-host brokered OAuth configuration was rejected.");
azure.OAuthBrokerCallbackUri = new Uri("https://attacker.example/TaxHub/HmrcCallback");
Assert(productionValidator.Validate(null, azure).Failed,
    "Broker start and callback endpoints on different hosts were accepted.");
azure.OAuthBrokerCallbackUri = new Uri("https://auth.tradecontrol.co.uk/TaxHub/HmrcCallback");
azure.SubmissionEnabled = true;
Assert(productionValidator.Validate(null, azure).Succeeded,
    "The explicit Azure-managed HMRC sandbox accreditation dispatch composition was rejected.");
azure.FraudCaptureEnabled = false;
Assert(productionValidator.Validate(null, azure).Failed,
    "Sandbox dispatch was accepted without fresh fraud-prevention evidence capture.");
azure.FraudCaptureEnabled = true;
azure.AuthorityEnvironment = VatAuthorityEnvironment.Production;
Assert(productionValidator.Validate(null, azure).Failed,
    "The accreditation dispatch gate was accepted against the production HMRC authority environment.");

var lockedTenantContext = new TenantContext();
lockedTenantContext.Bind(new TenantDescriptor(Guid.NewGuid(), "locked.example.test", "LockedTenant",
    "Server=(local);Database=locked;Trusted_Connection=True;", "Europe/London",
    TimeZoneInfo.FindSystemTimeZoneById("Europe/London"), vatSubmissionEnabled: false));
var openEnvironment = Microsoft.Extensions.Options.Options.Create(new VatProductHostOptions
    { SubmissionEnabled = true });
Assert(!new VatSubmissionGate(openEnvironment, lockedTenantContext).IsEnabled,
    "The shared environment gate bypassed a locked tenant VAT submission switch.");

var openTenantContext = new TenantContext();
openTenantContext.Bind(new TenantDescriptor(Guid.NewGuid(), "open.example.test", "OpenTenant",
    "Server=(local);Database=open;Trusted_Connection=True;", "Europe/London",
    TimeZoneInfo.FindSystemTimeZoneById("Europe/London"), vatSubmissionEnabled: true));
var lockedEnvironment = Microsoft.Extensions.Options.Options.Create(new VatProductHostOptions
    { SubmissionEnabled = false });
Assert(!new VatSubmissionGate(lockedEnvironment, openTenantContext).IsEnabled,
    "A tenant VAT submission switch bypassed the locked environment gate.");
Assert(new VatSubmissionGate(openEnvironment, openTenantContext).IsEnabled,
    "The explicitly enabled sandbox environment and tenant did not open the VAT submission gate.");

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
var profitAndLossGridComponent = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubProfitAndLossGrid.razor"));
Assert(businessTaxWorkspaceComponent.Contains("IsMobile ? \"Totals\"", StringComparison.Ordinal)
    && businessTaxWorkspaceComponent.Contains("IsMobile ? \"HMRC\"", StringComparison.Ordinal)
    && accountsWorkspaceComponent.Contains("IsMobile ? \"Annual\"", StringComparison.Ordinal)
    && accountsWorkspaceComponent.Contains("IsMobile ? \"Balance\"", StringComparison.Ordinal),
    "Business Tax or Accounts no longer provides compact mobile tab labels.");
Assert(accountsWorkspaceComponent.Contains("ActivePanelIndexChanged=\"LoadSectionAsync\"", StringComparison.Ordinal)
    && accountsWorkspaceComponent.Contains("_loadedSections.Contains(section)", StringComparison.Ordinal)
    && accountsWorkspaceComponent.Contains("TaxHubAccountsWorkspaceSection.Filing", StringComparison.Ordinal),
    "Accounts no longer loads and caches its expensive report sections on demand.");
Assert(profitAndLossGridComponent.Contains("<HierarchyColumn", StringComparison.Ordinal)
       && profitAndLossGridComponent.Contains("<ChildRowContent>", StringComparison.Ordinal)
       && profitAndLossGridComponent.Contains("Collapse cash-code detail", StringComparison.Ordinal)
       && profitAndLossGridComponent.Contains("ToggleDetailAsync(context.Item, context.Open)", StringComparison.Ordinal)
       && profitAndLossGridComponent.Contains("CollapseAllHierarchy", StringComparison.Ordinal)
       && profitAndLossGridComponent.Contains("ToggleHierarchyVisibilityAsync(item)", StringComparison.Ordinal)
       && profitAndLossGridComponent.Contains("@onclick:stopPropagation=\"true\"", StringComparison.Ordinal)
       && profitAndLossGridComponent.Contains("ButtonDisabledFunc=\"@HasNoDetail\"", StringComparison.Ordinal)
       && accountsWorkspaceComponent.Contains("Sections=\"@Workspace.AnnualDetails\"", StringComparison.Ordinal)
       && accountsWorkspaceComponent.Contains("Sections=\"@Workspace.MonthlyDetails\"", StringComparison.Ordinal)
       && !accountsWorkspaceComponent.Contains("<h2 class=\"h5 mb-1\">Cash Code Details</h2>", StringComparison.Ordinal),
    "Accounts no longer presents annual and monthly cash-code details as one-row-at-a-time hierarchy children.");
Assert(accountsWorkspaceComponent.Contains("OFFICIAL TEST — NOT A LIVE FILING", StringComparison.Ordinal)
       && accountsWorkspaceComponent.Contains("live package reference is not configured", StringComparison.Ordinal)
       && filingInputComponent.Contains("OFFICIAL TEST — NOT LIVE", StringComparison.Ordinal),
    "The filing journey no longer makes its official-test-only environment unmistakable.");
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
    && alignmentSql.Contains("@ValidFrom > @ObligationStart", StringComparison.Ordinal)
    && alignmentSql.Contains("@SandboxVrn = N'999000001'", StringComparison.Ordinal),
    "The disposable VAT alignment guard, historical profile coverage or authoritative calculation/configuration boundary changed.");
var syntheticBootstrapSql = File.ReadAllText(Path.Combine(root, "src", "sqlnode", "src", "tcNodeDb4", "App",
    "Stored Procedures", "proc_DatasetSyntheticMIS_Bootstrap.sql"));
var restoreDomainUsersAt = syntheticBootstrapSql.IndexOf("INSERT INTO Usr.tbUser", StringComparison.Ordinal);
var restoreIdentityUsersAt = syntheticBootstrapSql.IndexOf("INSERT INTO dbo.AspNetUsers", StringComparison.Ordinal);
Assert(syntheticBootstrapSql.Contains("SELECT * INTO #Keep_AspNetUsers FROM dbo.AspNetUsers;", StringComparison.Ordinal)
    && !syntheticBootstrapSql.Contains("#Keep_AspNetUsers FROM dbo.AspNetUsers WHERE", StringComparison.Ordinal)
    && syntheticBootstrapSql.Contains("#Keep_UsrUsers", StringComparison.Ordinal)
    && syntheticBootstrapSql.Contains("#Keep_AspNetUserRoles", StringComparison.Ordinal)
    && syntheticBootstrapSql.Contains("#Keep_AspNetUserClaims", StringComparison.Ordinal)
    && syntheticBootstrapSql.Contains("#Keep_AspNetUserLogins", StringComparison.Ordinal)
    && syntheticBootstrapSql.Contains("#Keep_AspNetUserTokens", StringComparison.Ordinal)
    && restoreDomainUsersAt >= 0
    && restoreIdentityUsersAt > restoreDomainUsersAt
    && syntheticBootstrapSql.Contains("PhoneNumberConfirmed = source.PhoneNumberConfirmed", StringComparison.Ordinal)
    && syntheticBootstrapSql.Contains("tenant user directory was not restored exactly", StringComparison.Ordinal),
    "Synthetic recommissioning no longer preserves and verifies the complete tenant user directory before restoring Identity.");
var syntheticMonthConsumers = new[]
{
    "proc_DatasetSyntheticMIS_ProjectPay.sql",
    "proc_DatasetSyntheticMIS_PayMisc.sql",
    "proc_DatasetSyntheticMIS_PayWages.sql",
    "proc_DatasetSyntheticMIS_Expenses.sql"
};
Assert(syntheticMonthConsumers.All(fileName => File.ReadAllText(Path.Combine(root, "src", "sqlnode", "src",
        "tcNodeDb4", "App", "Stored Procedures", fileName))
    .Contains("OPTION (MAXRECURSION 0)", StringComparison.Ordinal)),
    "A synthetic month-expansion consumer can still fail beyond SQL Server's default 100 recursion levels.");

var returnReview = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubVatReturnReview.razor"));
var obligationsPanel = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubVatObligations.razor"));
Assert(obligationsPanel.Contains("item.CanReview && VatHost.Value.ReviewEnabled", StringComparison.Ordinal)
    && obligationsPanel.Contains("_selected is not null && VatHost.Value.ReviewEnabled", StringComparison.Ordinal)
    && obligationsPanel.Contains("Return review is currently locked", StringComparison.Ordinal),
    "The VAT obligations panel no longer has an independent preparation-and-review gate.");
Assert(appServiceRegistration.Contains("VatPersistenceMode.DevelopmentFiles", StringComparison.Ordinal)
    && appServiceRegistration.Contains("or VatPersistenceMode.AzureManaged", StringComparison.Ordinal)
    && appServiceRegistration.Contains("IVatFilingHistoryService, VatFilingHistoryService", StringComparison.Ordinal),
    "Azure-managed composition did not activate filing history over durable dispatch evidence.");
var sharedLayout = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Shared", "_Layout.cshtml"));
Assert(sharedLayout.Contains("@inject UserManager<TradeControlWebUser> UserManager", StringComparison.Ordinal)
    && sharedLayout.Contains("@inject NodeContext NodeContext", StringComparison.Ordinal)
    && !sharedLayout.Contains("CreateScope()", StringComparison.Ordinal),
    "The shared layout creates a detached dependency scope that can lose the resolved tenant during Tax Hub prerendering.");
Assert(returnReview.Contains("The values below are read from the immutable prepared request", StringComparison.Ordinal)
    && returnReview.Contains("following day as the exclusive period boundary", StringComparison.Ordinal)
    && returnReview.Contains("Approve return — does not submit", StringComparison.Ordinal)
    && returnReview.Contains("Not yet submitted to HMRC", StringComparison.Ordinal)
    && returnReview.Contains("Final step — submit to HMRC", StringComparison.Ordinal)
    && returnReview.Contains("aria-label=\"Prepared VAT return values\"", StringComparison.Ordinal)
    && returnReview.Contains("alert alert-success py-2\" role=\"status\"", StringComparison.Ordinal)
    && returnReview.Contains("FindingClass(finding.Severity) py-2\" role=\"alert\"", StringComparison.Ordinal),
    "The Phase 6.3 exact-review boundary or separate submit gate is no longer explicit in the UI.");
Assert(returnReview.Contains("!SubmissionGate.IsEnabled", StringComparison.Ordinal)
    && returnReview.Contains("Approved and retained — not submitted to HMRC", StringComparison.Ordinal),
    "An approved accreditation candidate can expose the HMRC submission action while its tenant dispatch gate is disabled.");
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
    && reviewService.Contains("FindReusableReviewAsync", StringComparison.Ordinal)
    && reviewService.Contains("OrderByDescending(item => item.Approval is not null)", StringComparison.Ordinal)
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
var evidence = new VatSourceEvidenceSnapshot("database-a", "HOME", 42, "4.1.16", new string('A', 64),
    [new(new DateOnly(2026, 6, 1), 0.05m, "Supplier rounding evidence")]);
Assert(evidence.SameBoundaryAndContent(evidence with { }),
    "An unchanged VAT evidence boundary was treated as concurrent mutation.");
Assert(evidence.SameBoundaryAndContent(evidence with { AccountingEventId = 43 })
    && !evidence.SameBoundaryAndContent(evidence with { SourceInventorySha256 = new string('B', 64) })
    && !evidence.SameBoundaryAndContent(evidence with
        { Adjustments = [new(new DateOnly(2026, 6, 1), 0.05m, "Changed explanation")] }),
    "VAT preparation either treated an unrelated database row-version as a tax mutation or failed to detect scoped source/adjustment mutation.");
var evidenceReader = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "VatSourceEvidenceReader.cs"));
Assert(evidenceReader.Contains("App.proc_VatSourceEvidence", StringComparison.Ordinal)
    && reviewService.Contains("evidenceBefore.SameBoundaryAndContent(evidenceAfter)", StringComparison.Ordinal)
    && reviewService.Contains("JsonSerializer.Deserialize<VatReturnRequest>(bodyBytes.Span, VatBodyJson)", StringComparison.Ordinal)
    && reviewService.Contains("PropertyNameCaseInsensitive = true", StringComparison.Ordinal)
    && reviewService.Contains("VatStatutorySeal.Verify", StringComparison.Ordinal)
    && reviewService.Contains("DatabaseInstanceReference != candidate.DatabaseInstanceReference", StringComparison.Ordinal)
    && !reviewService.Contains("AccountingEventId != candidate.AccountingEventId", StringComparison.Ordinal)
    && reviewService.Contains("SourceInventorySha256 != candidate.SourceInventorySha256", StringComparison.Ordinal)
    && reviewService.Contains("changed since this review was prepared", StringComparison.Ordinal),
    "VAT preparation lost its scoped source guard, durable database identity, or retained seal verification, or again treated an unrelated row-version as a tax mutation.");
Assert(returnReview.Contains("Submit VAT return to HMRC", StringComparison.Ordinal)
    && returnReview.Contains("@bind=\"_submitConfirmed\"", StringComparison.Ordinal)
    && returnReview.Split("taxHubHmrc.captureClientFacts", StringSplitOptions.None).Length - 1 >= 2
    && returnReview.Contains("if (SubmissionGate.IsEnabled)", StringComparison.Ordinal)
    && returnReview.Contains("AuthorityErrors", StringComparison.Ordinal)
    && returnReview.Contains("VatAuthorityUserMessages.Submission", StringComparison.Ordinal),
    "The approval/submit controls, rejection detail, fresh fraud capture or unknown-outcome warning are missing.");
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
    && connectionService.Contains("if (!_submissionGate.IsEnabled)", StringComparison.Ordinal)
    && connectionService.Contains("vat-return-preflight", StringComparison.Ordinal)
    && connectionService.Contains("ActiveSubmissionAttemptException", StringComparison.Ordinal)
    && connectionService.Contains("VatReturnReconciliation.Compare", StringComparison.Ordinal)
    && connectionService.Contains("ParseAuthorityErrors", StringComparison.Ordinal)
    && connectionService.Contains("FRAUD-CONTEXT-REQUIRED", StringComparison.Ordinal),
    "Approved dispatch lost duplicate prevention, exact readback reconciliation or fresh fraud evidence.");
var azureVatPersistence = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "AzureVatPersistence.cs"));
Assert(azureVatPersistence.Contains("var isWrite = method is not \"GET\" and not \"HEAD\"", StringComparison.Ordinal)
    && azureVatPersistence.Contains("var blocking = isWrite ?", StringComparison.Ordinal)
    && azureVatPersistence.Contains("item.IsWrite && item.BlocksReplay", StringComparison.Ordinal),
    "Azure VAT persistence incorrectly applies submission replay protection to read-only HMRC enquiries.");
Assert(reviewService.Contains("AzureVatPersistence? azurePersistence", StringComparison.Ordinal)
    && reviewService.Contains("_azurePersistence.AddReviewCandidateAsync", StringComparison.Ordinal)
    && reviewService.Contains("_azurePersistence.AddReviewApprovalAsync", StringComparison.Ordinal)
    && azureVatPersistence.Contains("vat/v1/reviews/", StringComparison.Ordinal)
    && azureVatPersistence.Contains("GetReviewCandidateAsync", StringComparison.Ordinal)
    && azureVatPersistence.Contains("GetReviewApprovalAsync", StringComparison.Ordinal),
    "Azure-managed VAT preparation and approval evidence is not retained in the protected tenant store.");
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
Assert(filingHistory.Contains("AzureVatPersistence? azurePersistence = null", StringComparison.Ordinal)
    && filingHistory.Contains("return (azurePersistence, azurePersistence);", StringComparison.Ordinal),
    "Azure-managed filing history did not select the durable attempt and content stores.");
Assert(filingHistoryView.Contains("VAT filing history", StringComparison.Ordinal)
    && filingHistoryView.Contains("Approved return and audit references", StringComparison.Ordinal)
    && filingHistoryView.Contains("Prepared SHA-256", StringComparison.Ordinal)
    && filingHistoryView.Contains("MaskedVrn", StringComparison.Ordinal)
    && filingHistoryView.Contains("All nine retrieved VAT values exactly match", StringComparison.Ordinal)
    && filingHistoryView.Contains("Do not repeat an uncertain submission", StringComparison.Ordinal)
    && filingHistoryView.Contains("Reconcile with HMRC", StringComparison.Ordinal)
    && filingHistoryView.Split("role=\"status\"", StringSplitOptions.None).Length - 1 >= 4
    && filingHistoryView.Contains("aria-label=\"VAT readback differences\"", StringComparison.Ordinal)
    && filingHistoryView.Contains("aria-label=\"Approved VAT return values\"", StringComparison.Ordinal)
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
    && connectionView.Contains("Validate fraud headers", StringComparison.Ordinal)
    && connectionView.Contains("VAT-CONNECTION-TEMPORARILY-UNAVAILABLE", StringComparison.Ordinal)
    && connectionView.Contains("Retry status", StringComparison.Ordinal)
    && connectionView.Contains("No connection was removed", StringComparison.Ordinal)
    && connectionView.Contains("VatConnectionStatusUnavailable", StringComparison.Ordinal),
    "The VAT connection panel lost required HMRC journey guidance or deployed fraud validation.");
var workflowIdentitySource = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub", "Vat",
    "VatWorkflowIdentity.cs"));
Assert(workflowIdentitySource.Contains("IDbContextFactory<NodeContext>", StringComparison.Ordinal)
    && workflowIdentitySource.Contains("CreateDbContextAsync", StringComparison.Ordinal),
    "The VAT workflow identity boundary can share a circuit-scoped NodeContext across concurrent components.");
var hmrcControllerSource = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Controllers",
    "TaxHubHmrcController.cs"));
Assert(hmrcControllerSource.Contains("FraudPrevention/Validate", StringComparison.Ordinal)
    && hmrcControllerSource.Contains("IVatFraudHeaderValidationService", StringComparison.Ordinal),
    "TCWeb no longer validates fraud headers through its own deployed product composition.");
var subjectEvidenceContract = typeof(SubjectBalanceEvidenceSnapshot);
Assert(subjectEvidenceContract.Namespace == "TradeControl.Tax.UK.Application.DataProvision"
    && subjectEvidenceContract.GetProperties().All(property => new[]
        { "CompaniesHouse", "Hmrc", "Credential", "Declaration", "Submission" }
        .All(prohibited => !property.Name.Contains(prohibited, StringComparison.OrdinalIgnoreCase))),
    "The year-end Subject evidence contract is not authority-neutral.");
Assert(typeof(Subjects).GetMethod(nameof(Subjects.YearEndBalanceSnapshotAsync))?.ReturnType
       == typeof(Task<SubjectBalanceEvidenceSnapshot>),
    "Subjects is no longer the intermediary for the year-end balance evidence contract.");
using (var translationContext = new NodeContext(new DbContextOptionsBuilder<NodeContext>()
           .UseSqlServer("Server=translation-only;Database=translation-only;Integrated Security=True;TrustServerCertificate=True")
           .Options))
{
    var translatedSummary = translationContext.SubjectDatedBalances(new DateTime(2026, 9, 30))
        .GroupBy(_ => 1)
        .Select(group => new
        {
            DebtorCount = group.Count(row => row.PositionCode == (short)SubjectBalancePosition.OwedToUs),
            CreditorCount = group.Count(row => row.PositionCode == (short)SubjectBalancePosition.OwedByUs),
            DebtorTotal = group.Where(row => row.PositionCode == (short)SubjectBalancePosition.OwedToUs)
                .Sum(row => row.HumanBalance),
            CreditorTotal = group.Where(row => row.PositionCode == (short)SubjectBalancePosition.OwedByUs)
                .Sum(row => row.HumanBalance)
        })
        .ToQueryString();
    Assert(translatedSummary.Contains("fnDatedBalances", StringComparison.Ordinal)
        && translatedSummary.Contains("COUNT", StringComparison.OrdinalIgnoreCase)
        && translatedSummary.Contains("SUM", StringComparison.OrdinalIgnoreCase),
        "The SQL Server provider cannot translate the bounded Subject evidence summary query.");
}
var accountsWorkspaceView = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubAccountsWorkspace.razor"));
var yearEndReviewView = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubCompaniesHouseEvidenceReview.razor"));
var filingInputsView = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Hub",
    "Components", "TaxHubCompaniesHouseFilingInputs.razor"));
var taxHubServiceSource = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices", "TaxHub",
    "TaxHubService.cs"));
var cashAssetsServiceSource = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "AppServices",
    "CashAssetsWorkspaceService.cs"));
var cashManagerPage = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Cash", "Manager",
    "Index.cshtml"));
var cashManagerPageModel = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Cash", "Manager",
    "Index.cshtml.cs"));
var cashManagerShell = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Cash", "Manager",
    "CashManagerShell.razor"));
var subjectBrowserShell = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Subject", "Browser",
    "SubjectBrowserShell.razor"));
var subjectBrowserPage = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Subject", "Browser",
    "Index.cshtml"));
Assert(accountsWorkspaceView.Contains("Year-end debtors and creditors", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("Review in Subject Browser", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("Evidence SHA-256", StringComparison.Ordinal)
    && !accountsWorkspaceView.Contains("SubjectBalanceEvidence.Subjects", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("DateTime.DaysInMonth(selectedPeriod.Year, selectedPeriod.Month)", StringComparison.Ordinal)
    && !taxHubServiceSource.Contains("companiesHouse.FilingInputs.PeriodEnd", StringComparison.Ordinal)
    && subjectBrowserShell.Contains("balanceView", StringComparison.Ordinal)
    && subjectBrowserShell.Contains("InitialAsOfDate", StringComparison.Ordinal)
    && subjectBrowserShell.Contains("_mode != SubjectBrowserMode.Balances", StringComparison.Ordinal)
    && subjectBrowserPage.IndexOf("window.subjectBrowser =", StringComparison.Ordinal)
       < subjectBrowserPage.IndexOf("/_framework/blazor.server.js", StringComparison.Ordinal),
    "Tax Hub no longer presents the bounded accounting-period evidence summary or deep-links to historical Subject balances.");
Assert(accountsWorkspaceView.Contains("File Your Accounts", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("File your accounts", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("Start filing journey", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("one continuous journey", StringComparison.Ordinal)
    && !accountsWorkspaceView.Contains("MudTabPanel Text=\"@(IsMobile ? \"Review\" : \"Year-end review\")\"", StringComparison.Ordinal)
    && accountsWorkspaceView.IndexOf("Start filing journey", StringComparison.Ordinal)
       < accountsWorkspaceView.IndexOf("TaxHubCompaniesHouseEvidenceReview", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("CompletionChanged", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("if (_evidenceReviewComplete)", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("EXISTING FILING — DO NOT RESUBMIT", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("Continue the existing Companies House filing", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("CompaniesHouseContinuation.FindAsync", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("Check Companies House status", StringComparison.Ordinal)
    && accountsWorkspaceView.Contains("CompaniesHouseStatus.CheckAsync", StringComparison.Ordinal)
    && yearEndReviewView.Contains("subject-balances/v1", StringComparison.Ordinal)
    && yearEndReviewView.Contains("These are year-end balances, not an aged-debt reconstruction", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Review historical balances", StringComparison.Ordinal)
    && yearEndReviewView.Contains("bank-cash/v1", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Current accounts", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Reserve accounts", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Balance-sheet CASH + BANK", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Continue to fixed assets", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("ROW_NUMBER() OVER", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("account.CashCode IS NULL THEN N'Reserve'", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("SnapshotToken = Convert.ToHexString(SHA256.HashData", StringComparison.Ordinal)
    && yearEndReviewView.Contains("fixed-assets/v2", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Reductions / write-downs", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Post due write-off", StringComparison.Ordinal)
    && yearEndReviewView.Contains("PostScheduledWriteOffAsync", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Future — remains Unposted", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("PaymentStatusCode = 0", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("LIKE N'WRITE OFF%'", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("DueWriteOffCount = dueWriteOffs.Count", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("mapping.MappingRoot IN (N'CA-ASSET', N'CA-DEPREC')", StringComparison.Ordinal)
    && yearEndReviewView.Contains("year-end-evidence-review/v1", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Complete evidence review", StringComparison.Ordinal)
    && yearEndReviewView.Contains("CompletionChanged.InvokeAsync(_reviewCompleted)", StringComparison.Ordinal)
    && !yearEndReviewView.Contains("Evidence.Subjects", StringComparison.Ordinal)
    && filingInputsView.Contains("EvidenceReviewSatisfied", StringComparison.Ordinal)
    && filingInputsView.Contains("Complete the guided year-end evidence review", StringComparison.Ordinal),
    "The Companies House journey is not continuous, duplicates Subject detail, or does not fail closed before filing preparation.");
Assert(cashAssetsServiceSource.Contains("PostScheduledWriteOffAsync", StringComparison.Ordinal)
    && cashAssetsServiceSource.Contains("PaymentStatus.Unposted", StringComparison.Ordinal)
    && cashAssetsServiceSource.Contains("PaidInValue != 0m", StringComparison.Ordinal)
    && cashAssetsServiceSource.Contains("PaidOutValue <= 0m", StringComparison.Ordinal)
    && cashAssetsServiceSource.Contains("StartsWith(\"Write off\"", StringComparison.Ordinal)
    && cashAssetsServiceSource.Contains("Future scheduled write-offs cannot be posted", StringComparison.Ordinal)
    && cashAssetsServiceSource.Contains("CashAccountType.Asset", StringComparison.Ordinal)
    && cashAssetsServiceSource.Contains("outside the mapped statutory fixed-asset population", StringComparison.Ordinal)
    && cashAssetsServiceSource.Contains("You can only post your own scheduled asset write-offs", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Confirm and post", StringComparison.Ordinal)
    && yearEndReviewView.Contains("EvidenceChanged.InvokeAsync", StringComparison.Ordinal),
    "The year-end wizard scheduled-write-off action is missing a required accounting, timing, authority or refresh guard.");
Assert(taxHubServiceSource.Contains("&YearNumber=", StringComparison.Ordinal)
    && cashManagerPageModel.Contains("public short? YearNumber", StringComparison.Ordinal)
    && cashManagerPageModel.Contains("public string? Section", StringComparison.Ordinal)
    && cashManagerPage.Contains("param-InitialYearNumber=\"Model.YearNumber\"", StringComparison.Ordinal)
    && cashManagerPage.Contains("param-InitialSection=\"Model.Section\"", StringComparison.Ordinal)
    && cashManagerShell.Contains("[Parameter] public short? InitialYearNumber", StringComparison.Ordinal)
    && cashManagerShell.Contains("Enum.TryParse<CashManagerSection>(InitialSection", StringComparison.Ordinal)
    && cashManagerShell.Contains("_selectedPeriodStartOn = null;", StringComparison.Ordinal)
    && cashManagerShell.Contains("if (hasRequestedYear)", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Review statement", StringComparison.Ordinal)
    && yearEndReviewView.Contains("Manage schedule", StringComparison.Ordinal)
    && taxHubServiceSource.Contains("&Section=Assets", StringComparison.Ordinal),
    "Tax Hub accounting-evidence drill-throughs do not preserve the reviewed accounting year and all-month statement scope.");
var navigationView = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Pages", "Shared", "_Navigation.cshtml"));
var legacyCompanyPages = Path.Combine(root, "src", "TCWeb", "Pages", "Tax", "Company");
Assert((!Directory.Exists(legacyCompanyPages) || !Directory.EnumerateFiles(legacyCompanyPages).Any())
    && !File.Exists(Path.Combine(root, "src", "TCWeb", "Pages", "Subject", "Reports", "DebtorsAndCreditors.cshtml"))
    && !File.Exists(Path.Combine(root, "src", "TCWeb", "Pages", "Subject", "Reports", "DebtorsAndCreditors.cshtml.cs"))
    && !navigationView.Contains("/Tax/Company", StringComparison.OrdinalIgnoreCase)
    && !navigationView.Contains("/Subject/Reports/DebtorsAndCreditors", StringComparison.OrdinalIgnoreCase),
    "A retired Business Tax or Debtors and Creditors Razor Page remains reachable from the application.");
var startupSource = File.ReadAllText(Path.Combine(root, "src", "TCWeb", "Startup.cs"));
var livenessMiddlewareIndex = startupSource.IndexOf("app.UseHealthChecks(\"/health/live\"", StringComparison.Ordinal);
var tenantMiddlewareIndex = startupSource.IndexOf("app.UseMiddleware<TenantResolutionMiddleware>()", StringComparison.Ordinal);
var authenticationMiddlewareIndex = startupSource.IndexOf("app.UseAuthentication()", StringComparison.Ordinal);
Assert(livenessMiddlewareIndex >= 0
    && tenantMiddlewareIndex > livenessMiddlewareIndex
    && authenticationMiddlewareIndex > livenessMiddlewareIndex,
    "The App Service liveness probe is missing or can construct tenant-scoped authentication services.");
Assert(startupSource.Contains("endpoints.MapHealthChecks(\"/health/ready\"", StringComparison.Ordinal)
    && startupSource.Contains("}).AllowAnonymous();", StringComparison.Ordinal),
    "The tenant-bound App Service readiness probe is missing or protected by interactive authentication.");

Console.WriteLine($"TCWeb Tax Hub boundary tests passed ({assertions} assertions).");

static VatProductHostOptions ValidDevelopmentOptions() => new()
{
    Enabled = true,
    ReviewEnabled = true,
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

    public Task<CompaniesHouseOfficialTestSubmittedDocument> ReadOfficialTestSubmittedDocumentAsync(
        string preparationReference, CancellationToken cancellationToken = default) =>
        Task.FromResult(new CompaniesHouseOfficialTestSubmittedDocument(bytes, "TEST-DIGEST", "S00017",
            new DateTimeOffset(2026, 10, 12, 12, 0, 0, TimeSpan.Zero)));

    public Task<CompaniesHouseApprovalReview> ApproveAsync(string preparationReference,
        string declarationVersion, bool declarationsConfirmed,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

file sealed class StubCompaniesHouseDraftPdfRenderer(byte[] bytes) : ICompaniesHouseDraftPdfRenderer
{
    public byte[] Render(byte[] retainedIxbrl, string sourceSha256) => bytes;
    public byte[] RenderOfficialTestSubmission(byte[] retainedIxbrl, string sourceSha256,
        string submissionNumber, DateTimeOffset submittedAtUtc) => bytes;
}

file sealed class StubCompaniesHouseWorkflowIdentityAccessor(CompaniesHouseWorkflowIdentity identity)
    : ICompaniesHouseWorkflowIdentityAccessor
{
    public Task<CompaniesHouseWorkflowIdentity> GetRequiredAsync(
        CancellationToken cancellationToken = default) => Task.FromResult(identity);
}

file sealed class StubCompaniesHouseWorkflowStore(
    CompaniesHousePreparationRecord preparation,
    CompaniesHouseApprovalRecord approval) : ICompaniesHouseWorkflowStore
{
    public CompaniesHouseConversationRecord? ActiveConversation { get; private set; }
    public int UpsertCount { get; private set; }
    public int TryBeginDispatchCount { get; private set; }
    public int TryBeginStatusCheckCount { get; private set; }

    public void SetState(CompaniesHouseConversationState state)
    {
        if (ActiveConversation is null) throw new InvalidOperationException();
        ActiveConversation = ActiveConversation with { State = state, UpdatedAtUtc = DateTimeOffset.UtcNow };
    }

    public Task AddPreparationAsync(CompaniesHousePreparationRecord value,
        CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<CompaniesHousePreparationRecord?> GetPreparationAsync(string tenantReference,
        string reference, CancellationToken cancellationToken = default) =>
        Task.FromResult<CompaniesHousePreparationRecord?>(
            tenantReference == preparation.TenantReference && reference == preparation.Reference
                ? preparation : null);

    public Task AddApprovalAsync(CompaniesHouseApprovalRecord value,
        CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<CompaniesHouseApprovalRecord?> GetApprovalAsync(string tenantReference,
        string reference, CancellationToken cancellationToken = default) =>
        Task.FromResult<CompaniesHouseApprovalRecord?>(
            tenantReference == approval.TenantReference && reference == approval.Reference
                ? approval : null);

    public Task<CompaniesHouseApprovalRecord?> GetApprovalForPreparationAsync(string tenantReference,
        string preparationReference, CancellationToken cancellationToken = default) =>
        Task.FromResult<CompaniesHouseApprovalRecord?>(approval);

    public Task<CompaniesHouseConversationRecord?> GetActiveConversationAsync(string tenantReference,
        string logicalFilingIdentity, CancellationToken cancellationToken = default) =>
        Task.FromResult(ActiveConversation is not null
                        && ActiveConversation.TenantReference == tenantReference
                        && ActiveConversation.LogicalFilingIdentity == logicalFilingIdentity
                        && ActiveConversation.State is not (CompaniesHouseConversationState.Accepted
                            or CompaniesHouseConversationState.Rejected
                            or CompaniesHouseConversationState.GatewayRejected
                            or CompaniesHouseConversationState.InternalFailure)
            ? ActiveConversation : null);

    public Task UpsertConversationAsync(CompaniesHouseConversationRecord conversation,
        CancellationToken cancellationToken = default)
    {
        UpsertCount++;
        ActiveConversation = conversation;
        return Task.CompletedTask;
    }

    public Task<CompaniesHouseConversationRecord?> GetConversationAsync(string tenantReference,
        string reference, CancellationToken cancellationToken = default) =>
        Task.FromResult(ActiveConversation is not null
                        && ActiveConversation.TenantReference == tenantReference
                        && ActiveConversation.Reference == reference
            ? ActiveConversation : null);

    public Task<bool> TryBeginDispatchAsync(CompaniesHouseConversationRecord conversation,
        CancellationToken cancellationToken = default)
    {
        TryBeginDispatchCount++;
        if (ActiveConversation is null
            || ActiveConversation.State != CompaniesHouseConversationState.Prepared)
            return Task.FromResult(false);
        ActiveConversation = conversation;
        return Task.FromResult(true);
    }

    public Task<bool> TryBeginStatusCheckAsync(CompaniesHouseConversationRecord conversation,
        DateTimeOffset expectedUpdatedAtUtc, CancellationToken cancellationToken = default)
    {
        TryBeginStatusCheckCount++;
        if (ActiveConversation is null
            || ActiveConversation.UpdatedAtUtc != expectedUpdatedAtUtc
            || ActiveConversation.State is not (CompaniesHouseConversationState.Acknowledged
                or CompaniesHouseConversationState.Pending or CompaniesHouseConversationState.Parked)
            || conversation.StatusChecks?.LastOrDefault()?.CheckState
                != CompaniesHouseStatusCheckState.Dispatching)
            return Task.FromResult(false);
        ActiveConversation = conversation;
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<CompaniesHouseConversationRecord>> ListConversationsAsync(
        string tenantReference, string companyIdentitySha256,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CompaniesHouseConversationRecord>>(
            ActiveConversation is null ? [] : [ActiveConversation]);
}

file sealed class StubCompaniesHouseProtectedContentStore(IReadOnlyDictionary<string, byte[]> content)
    : ICompaniesHouseProtectedContentStore
{
    private readonly Dictionary<string, byte[]> _content = new(content);
    public int ReadCount { get; private set; }
    public int WriteCount { get; private set; }

    public Task<CompaniesHouseProtectedContent> WriteAsync(string tenantReference, string contentKind,
        ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        WriteCount++;
        var reference = $"written-{WriteCount}";
        var value = bytes.ToArray();
        _content[reference] = value;
        return Task.FromResult(new CompaniesHouseProtectedContent(reference,
            Convert.ToHexString(SHA256.HashData(value)), value.LongLength, value));
    }

    public Task<CompaniesHouseProtectedContent> ReadAsync(string tenantReference, string reference,
        string expectedSha256, CancellationToken cancellationToken = default)
    {
        ReadCount++;
        var bytes = _content[reference];
        return Task.FromResult(new CompaniesHouseProtectedContent(reference, expectedSha256,
            bytes.LongLength, bytes));
    }
}

file sealed class StubCompaniesHouseAuthorityIdentifierSource(CompaniesHouseAuthorityIdentifiers identifiers)
    : ICompaniesHouseAuthorityIdentifierSource
{
    public Task<CompaniesHouseAuthorityIdentifiers> AllocateAsync(
        CancellationToken cancellationToken = default) => Task.FromResult(identifiers);
}

file sealed class StubCompaniesHouseSubmissionMaterializer : ICompaniesHouseSubmissionMaterializer
{
    public Task<CompaniesHouseMaterializedSubmission> MaterializeAsync(
        CompaniesHouseAuthorityIdentifiers identifiers, CompaniesHouseProtectedContent packagePreview,
        CompaniesHouseProtectedContent accountsDocument, CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes($"<recording submission='{identifiers.SubmissionNumber}' transaction='{identifiers.TransactionId}' />");
        return Task.FromResult(new CompaniesHouseMaterializedSubmission(ImmutableArray.CreateRange(bytes),
            Convert.ToHexString(SHA256.HashData(bytes))));
    }
}

file sealed class StubCompaniesHouseSubmissionExchange(string transactionId, bool fail = false)
    : ICompaniesHouseSubmissionExchange
{
    public int ExchangeCount { get; private set; }

    public Task<CompaniesHouseGatewayResponse> ExchangeAsync(CompaniesHouseMaterializedSubmission request,
        CancellationToken cancellationToken = default)
    {
        ExchangeCount++;
        if (fail) throw new IOException("Injected ambiguous recording failure.");
        var bytes = Encoding.UTF8.GetBytes($$"""
            <?xml version="1.0" encoding="utf-8"?>
            <GovTalkMessage xmlns="http://www.govtalk.gov.uk/CM/envelope">
              <EnvelopeVersion>1.0</EnvelopeVersion>
              <Header><MessageDetails><Class>Accounts</Class><Qualifier>acknowledgement</Qualifier><Function>submit</Function><TransactionID>{{transactionId}}</TransactionID><GatewayTimestamp>2026-10-12T12:00:00+00:00</GatewayTimestamp></MessageDetails></Header>
              <GovTalkDetails/><Body/>
            </GovTalkMessage>
            """);
        return Task.FromResult(new CompaniesHouseGatewayResponse(
            CompaniesHouseGatewayResponseKind.Acknowledgement, ImmutableArray.CreateRange(bytes),
            request.Sha256, Convert.ToHexString(SHA256.HashData(bytes)), "RECORDED — NOT SENT"));
    }
}

file sealed class StubCompaniesHouseStatusTransactionIdSource(params string[] transactionIds)
    : ICompaniesHouseStatusTransactionIdSource
{
    private readonly Queue<string> _transactionIds = new(transactionIds);

    public string Allocate() => _transactionIds.Dequeue();
}

file sealed class StubCompaniesHouseStatusRequestMaterializer : ICompaniesHouseStatusRequestMaterializer
{
    public Task<CompaniesHouseMaterializedSubmission> MaterializeAsync(string submissionNumber,
        string transactionId, CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes(
            $"<status submission='{submissionNumber}' transaction='{transactionId}' />");
        return Task.FromResult(new CompaniesHouseMaterializedSubmission(ImmutableArray.CreateRange(bytes),
            Convert.ToHexString(SHA256.HashData(bytes))));
    }
}

file sealed class StubCompaniesHouseStatusExchange(
    string submissionNumber, params CompaniesHouseSubmissionStatusCode[] statuses)
    : ICompaniesHouseStatusExchange
{
    private readonly Queue<CompaniesHouseSubmissionStatusCode> _statuses = new(statuses);
    public int ExchangeCount { get; private set; }

    public Task<CompaniesHouseGatewayResponse> ExchangeAsync(CompaniesHouseMaterializedSubmission request,
        CancellationToken cancellationToken = default)
    {
        ExchangeCount++;
        var status = _statuses.Dequeue();
        var bytes = Encoding.UTF8.GetBytes($$"""
            <?xml version="1.0" encoding="utf-8"?>
            <GovTalkMessage xmlns="http://www.govtalk.gov.uk/CM/envelope">
              <EnvelopeVersion>1.0</EnvelopeVersion>
              <Header><MessageDetails><Class>GetSubmissionStatus</Class><Qualifier>response</Qualifier><TransactionID>STATUS000{{ExchangeCount}}</TransactionID><GatewayTimestamp>2026-10-12T13:00:00+00:00</GatewayTimestamp></MessageDetails></Header>
              <GovTalkDetails/>
              <Body><GetSubmissionStatus xmlns="http://xmlgw.companieshouse.gov.uk"><Status><SubmissionNumber>{{submissionNumber}}</SubmissionNumber><StatusCode>{{StatusCode(status)}}</StatusCode></Status></GetSubmissionStatus></Body>
            </GovTalkMessage>
            """);
        return Task.FromResult(new CompaniesHouseGatewayResponse(
            CompaniesHouseGatewayResponseKind.SubmissionStatus, ImmutableArray.CreateRange(bytes),
            request.Sha256, Convert.ToHexString(SHA256.HashData(bytes)), "RECORDED — NOT SENT"));
    }

    private static string StatusCode(CompaniesHouseSubmissionStatusCode status) => status switch
    {
        CompaniesHouseSubmissionStatusCode.Accept => "ACCEPT",
        CompaniesHouseSubmissionStatusCode.Reject => "REJECT",
        CompaniesHouseSubmissionStatusCode.Pending => "PENDING",
        CompaniesHouseSubmissionStatusCode.Parked => "PARKED",
        CompaniesHouseSubmissionStatusCode.InternalFailure => "INTERNAL_FAILURE",
        _ => throw new InvalidOperationException()
    };
}
