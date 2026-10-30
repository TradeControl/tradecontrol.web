using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradeControl.Web.Data;
using TradeControl.Web.Tenancy;
using Microsoft.Extensions.Options;


namespace TradeControl.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var tenantOptions = host.Services.GetRequiredService<IOptions<TenantHostingOptions>>().Value;
            if (tenantOptions.SeedOnStartup)
            {
                if (tenantOptions.SharedHostEnabled)
                    throw new InvalidOperationException(
                        "Automatic startup seeding is prohibited in shared-host mode.");
                foreach (var tenant in host.Services.GetRequiredService<ITenantDirectory>().ListEnabled())
                {
                    using var scope = host.Services.CreateScope();
                    var services = scope.ServiceProvider;
                    services.GetRequiredService<ITenantContext>().Bind(tenant);
                    try
                    {
                        SeedData.Initialize(services).Wait();
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError("Tenant database seeding failed with {FailureType}.",
                            ex.GetBaseException().GetType().Name);
                    }
                }
            }

             host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options => options.AddServerHeader = false);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
