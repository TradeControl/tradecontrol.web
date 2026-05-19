using Microsoft.Extensions.DependencyInjection;
using TradeControl.Web.AppServices.Execution;

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
            services.AddScoped<ISubjectBrowserService, SubjectBrowserService>();
            services.AddScoped<ISubjectEnquiryService, SubjectEnquiryService>();
            services.AddScoped<ICashManagerService, CashManagerService>();
            services.AddScoped<ICashNamespaceResolver, CashNamespaceResolver>();
            services.AddScoped<ICashStatementQueryService, CashStatementQueryService>();
            services.AddScoped<ICashStatementPaymentMaintenanceService, CashStatementPaymentMaintenanceService>();
            services.AddScoped<ICashPaymentsWorkspaceService, CashPaymentsWorkspaceService>();
            services.AddScoped<ICashAssetsWorkspaceService, CashAssetsWorkspaceService>();
            services.AddScoped<ICashAccountMaintenanceService, CashAccountMaintenanceService>();

            services.AddSingleton<IExecutionRuntimeState, ExecutionRuntimeState>();
            services.AddScoped<IExecutionQueue, ExecutionQueue>();
            services.AddScoped<IExecutionHandler, SyntheticDatasetExecutionHandler>();
            services.AddHostedService<ExecutionWorker>();

            return services;
        }
    }
}
