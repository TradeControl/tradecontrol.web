/**************************************************************************************
Trade Control
ASP.NET Core Web Interface

Dates: 1 July 2019
Author: IAM

Trade Control by Trade Control Ltd is licensed under GNU General Public License v3.0. 

You may obtain a copy of the License at

	https://www.gnu.org/licenses/gpl-3.0.en.html

***********************************************************************************/

using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

using Wangkanai.Detection.Models;
using TradeControl.Web.Data;
using System.Globalization;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.AppServices;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.AppServices.TaxHub.Vat;

namespace TradeControl.Web
{

    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            // Blazor Server (for Admin/Manager tree host)
            services.AddServerSideBlazor();

            services.AddDetection();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(120);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(_env.WebRootPath));
            services.AddOptions<VatProductHostOptions>()
                .Bind(Configuration.GetSection(VatProductHostOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<VatProductHostOptions>,
                VatProductHostOptionsValidator>();
            services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<VatProductHostOptions>,
                VatProductDevelopmentDefaults>();
            services.AddOptions<CompaniesHouseProductHostOptions>()
                .Bind(Configuration.GetSection(CompaniesHouseProductHostOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<CompaniesHouseProductHostOptions>,
                CompaniesHouseProductHostOptionsValidator>();
            services.AddSingleton<Microsoft.Extensions.Options.IPostConfigureOptions<CompaniesHouseProductHostOptions>,
                CompaniesHouseProductDevelopmentDefaults>();
            services.AddAppServices(Configuration);
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
                .AddCheck<TaxHubReadinessHealthCheck>("trade-control-node", tags: new[] { "ready" });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
                    context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
                    context.Response.Headers.TryAdd("Content-Security-Policy",
                        "base-uri 'self'; object-src 'none'; frame-ancestors 'self'; form-action 'self'");
                    return Task.CompletedTask;
                });
                await next();
            });
            
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Redirect("/favicon.svg", permanent: false);
                    return;
                }
                await next();
            });

            app.UseStaticFiles();

            app.UseDetection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Blazor Server endpoint
                endpoints.MapBlazorHub();

                endpoints.MapRazorPages();

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains("live")
                }).AllowAnonymous();
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains("ready")
                }).AllowAnonymous();
            });            
        }
    }
}
