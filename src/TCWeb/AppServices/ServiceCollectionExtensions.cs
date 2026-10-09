using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TradeControl.Web.AppServices.Execution;
using TradeControl.Web.AppServices.InvoiceRegister;
using TradeControl.Web.AppServices.TaxHub;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.AppServices.TaxHub.Vat;

namespace TradeControl.Web.AppServices
{
    /// <summary>
    /// Dependency injection registration helpers for Blazor-related features.
    /// Keep Blazor-related service wiring centralized to avoid scattering module-specific registrations in <c>Startup</c>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ITemplateTreeProvider, TemplateTreeProvider>();
            services.AddScoped<ITemplateInvoicesService, TemplateInvoicesService>();
            services.AddScoped<ITemplateSystemService, TemplateSystemService>();
            services.AddScoped<IInvoiceTypeLookup, InvoiceTypeLookup>();
            services.AddScoped<ITaxConfiguratorService, TaxConfiguratorService>();
            services.AddScoped<ITaxHubService, TaxHubService>();
            services.AddScoped<ICompaniesHouseReadinessService, CompaniesHouseReadinessService>();
            services.AddSingleton<ICompaniesHouseDraftPdfRenderer, CompaniesHouseDraftPdfRenderer>();
            services.AddSingleton<ICompaniesHouseFilingAuthorisationPolicy,
                CompaniesHouseFilingAuthorisationPolicy>();
            services.AddScoped<ICompaniesHouseWorkflowIdentityAccessor,
                CompaniesHouseWorkflowIdentityAccessor>();
            if (configuration.GetValue<CompaniesHousePersistenceMode>(
                    $"{CompaniesHouseProductHostOptions.SectionName}:PersistenceMode")
                == CompaniesHousePersistenceMode.AzureManaged)
            {
                services.AddSingleton(provider =>
                {
                    var options = provider.GetRequiredService<IOptions<CompaniesHouseProductHostOptions>>().Value;
                    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ExcludeInteractiveBrowserCredential = true
                    });
                    var service = new BlobServiceClient(new Uri(options.EvidenceBlobServiceUri!), credential);
                    return service.GetBlobContainerClient(options.EvidenceContainerName);
                });
                services.AddScoped<AzureCompaniesHousePersistence>();
                services.AddScoped<ICompaniesHouseWorkflowStore>(provider =>
                    provider.GetRequiredService<AzureCompaniesHousePersistence>());
                services.AddScoped<ICompaniesHouseProtectedContentStore>(provider =>
                    provider.GetRequiredService<AzureCompaniesHousePersistence>());
                services.AddScoped<ICompaniesHousePersistenceProbe>(provider =>
                    provider.GetRequiredService<AzureCompaniesHousePersistence>());
                services.AddScoped<ICompaniesHousePreparationReviewService,
                    CompaniesHousePreparationReviewService>();
            }
            else
                services.AddScoped<ICompaniesHousePreparationReviewService,
                    DisabledCompaniesHousePreparationReviewService>();
            services.AddHttpContextAccessor();
            services.AddScoped<IVatWorkflowIdentityAccessor, VatWorkflowIdentityAccessor>();
            services.AddSingleton<IVatAuthorityDispatchContextFactory, VatAuthorityDispatchContextFactory>();
            services.AddSingleton<TimeProvider>(TimeProvider.System);
            services.AddSingleton<IVatFraudContextReferenceStore, VatFraudContextReferenceStore>();
            services.AddScoped<VatHmrcConnectionService>();
            services.AddScoped<IVatHmrcConnectionService>(provider =>
                provider.GetRequiredService<VatHmrcConnectionService>());
            services.AddScoped<IVatFraudHeaderValidationService>(provider =>
                provider.GetRequiredService<VatHmrcConnectionService>());
            services.AddScoped<IVatAuthorityObligationSource>(provider =>
                provider.GetRequiredService<VatHmrcConnectionService>());
            services.AddScoped<IVatAuthorityReturnReadbackSource>(provider =>
                provider.GetRequiredService<VatHmrcConnectionService>());
            services.AddScoped<IVatFraudContextCapture, VatFraudContextCapture>();
            services.AddSingleton<VatObligationReconciler>();
            services.AddScoped<IVatObligationWorkspaceService, VatObligationWorkspaceService>();
            services.AddScoped<IVatReturnReviewService, VatReturnReviewService>();
            services.AddScoped<IVatApprovedReturnResolver>(provider =>
                provider.GetRequiredService<IVatReturnReviewService>() as IVatApprovedReturnResolver
                ?? throw new InvalidOperationException("The VAT approval resolver is unavailable."));
            services.AddScoped<IVatApprovalEvidenceSource>(provider =>
                provider.GetRequiredService<IVatReturnReviewService>() as IVatApprovalEvidenceSource
                ?? throw new InvalidOperationException("The VAT approval evidence source is unavailable."));
            services.AddScoped<IVatFilingHistoryService, VatFilingHistoryService>();
            services.AddScoped<IVatAuthorityReturnSubmission>(provider =>
                provider.GetRequiredService<VatHmrcConnectionService>());
            services.AddScoped<IVatReturnSubmissionService, VatReturnSubmissionService>();
            services.AddSingleton<IVatFilingAuthorisationPolicy, VatFilingAuthorisationPolicy>();
            services.AddScoped<ISubjectBrowserService, SubjectBrowserService>();
            services.AddScoped<ISubjectEnquiryService, SubjectEnquiryService>();
            services.AddScoped<ICashManagerService, CashManagerService>();
            services.AddScoped<ICashNamespaceResolver, CashNamespaceResolver>();
            services.AddScoped<ICashStatementQueryService, CashStatementQueryService>();
            services.AddScoped<ICashStatementPaymentMaintenanceService, CashStatementPaymentMaintenanceService>();
            services.AddScoped<ICashPaymentsWorkspaceService, CashPaymentsWorkspaceService>();
            services.AddScoped<ICashAssetsWorkspaceService, CashAssetsWorkspaceService>();
            services.AddScoped<ICashTransfersWorkspaceService, CashTransfersWorkspaceService>();
            services.AddScoped<ICashAccountMaintenanceService, CashAccountMaintenanceService>();
            services.AddScoped<IInvoiceRegisterQueryBuilder, InvoiceRegisterQueryBuilder>();
            services.AddScoped<IInvoiceFormattingService, InvoiceFormattingService>();
            services.AddScoped<IInvoiceRegisterLookupService, InvoiceRegisterLookupService>();
            services.AddScoped<IInvoiceRegisterService, InvoiceRegisterService>();
            services.AddScoped<IInvoiceRegisterWorkflowService, InvoiceRegisterWorkflowService>();

            services.AddSingleton<IExecutionRuntimeState, ExecutionRuntimeState>();
            services.AddScoped<IExecutionQueue, ExecutionQueue>();
            services.AddScoped<IExecutionHandler, SyntheticDatasetExecutionHandler>();
            services.AddHostedService<ExecutionWorker>();

            return services;
        }
    }
}
