using Microsoft.Extensions.DependencyInjection;
using TradeControl.Web.AppServices.Execution;
using TradeControl.Web.AppServices.InvoiceRegister;
using TradeControl.Web.AppServices.TaxHub;

namespace TradeControl.Web.AppServices
{
    /// <summary>
    /// Dependency injection registration helpers for Blazor-related features.
    /// Keep Blazor-related service wiring centralized to avoid scattering module-specific registrations in <c>Startup</c>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<ITemplateTreeProvider, TemplateTreeProvider>();
            services.AddScoped<ITemplateInvoicesService, TemplateInvoicesService>();
            services.AddScoped<ITemplateSystemService, TemplateSystemService>();
            services.AddScoped<IInvoiceTypeLookup, InvoiceTypeLookup>();
            services.AddScoped<ITaxConfiguratorService, TaxConfiguratorService>();
            services.AddScoped<ITaxHubService, TaxHubService>();
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
