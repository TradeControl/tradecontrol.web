using Microsoft.Extensions.DependencyInjection;

namespace TradeControl.Web.AppServices
{
    /// <summary>
    /// Dependency injection registration helpers for Blazor-related features.
    /// Keep Blazor-related service wiring centralized to avoid scattering module-specific registrations in <c>Startup</c>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdminManagerServices(this IServiceCollection services)
        {
            services.AddScoped<ITemplateTreeProvider, TemplateTreeProvider>();
            services.AddScoped<IInvoiceTypeLookup, InvoiceTypeLookup>();

            return services;
        }
    }
}
