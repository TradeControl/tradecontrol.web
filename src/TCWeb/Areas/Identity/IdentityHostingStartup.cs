using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;

[assembly: HostingStartup(typeof(TradeControl.Web.Areas.Identity.IdentityHostingStartup))]
namespace TradeControl.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<NodeContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("TCNodeContext")));

                //services.AddPooledDbContextFactory<NodeContext>(options =>
                //    options.UseSqlServer(
                //        context.Configuration.GetConnectionString("TCNodeContext")));

                services.AddDefaultIdentity<TradeControlWebUser>(options => 
                    options.SignIn.RequireConfirmedAccount = true)
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<NodeContext>();

                
                services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                });

                services.AddSingleton<IAuthorizationHandler, AspNetAdminAuthorizationHandler>();
                services.AddSingleton<IAuthorizationHandler, AspNetManagerAuthorizationHandler>();
                services.AddScoped<IAuthorizationHandler, AspNetIsOwnerAuthorizationHandler>();

                var defaultLockoutTimeSpan = context.Configuration.GetSection("Settings")["DefaultLockoutTimeSpan"];

                services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(double.Parse(defaultLockoutTimeSpan));
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                    options.User.RequireUniqueEmail = true;
                });
                

                services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

                    options.LoginPath = "/Identity/Account/Login";
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                    options.SlidingExpiration = true;
                });

                services.AddDistributedMemoryCache();

                string cultureName = context.Configuration.GetSection("Settings")["CultureName"];
                var cultureInfo = new CultureInfo(cultureName);

                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                double sessionTimeSpan = double.Parse(context.Configuration.GetSection("Settings")["SessionTimeSpan"]);

                services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromSeconds(sessionTimeSpan);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                });
            });
        }
    }
}
