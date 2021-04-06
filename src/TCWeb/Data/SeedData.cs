using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TradeControl.Web.Authorization;

namespace TradeControl.Web.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new NodeContext(serviceProvider.GetRequiredService<DbContextOptions<NodeContext>>()))
            {
                await EnsureRole(serviceProvider, Constants.AdministratorsRole);
                await EnsureRole(serviceProvider, Constants.ManagersRole);
            }
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, string role)
        {
            IdentityResult IR = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
                throw new Exception("roleManager null");

            if (!await roleManager.RoleExistsAsync(role))
                IR = await roleManager.CreateAsync(new IdentityRole(role));

            return IR;
        }

    }
}
