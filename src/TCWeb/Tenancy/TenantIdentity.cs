using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TradeControl.Web.Areas.Identity.Data;

namespace TradeControl.Web.Tenancy;

public static class TenantIdentity
{
    public const string TenantClaimType = "urn:tradecontrol:tenant";
    public static bool Matches(ClaimsPrincipal? principal, TenantDescriptor tenant) =>
        Guid.TryParse(principal?.FindFirstValue(TenantClaimType), out var claimedTenant)
        && claimedTenant == tenant.TenantId;

    public static async Task ValidateCookieAsync(CookieValidatePrincipalContext context)
    {
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        if (!tenantContext.IsResolved || !Matches(context.Principal, tenantContext.RequiredTenant))
        {
            context.RejectPrincipal();
            return;
        }
        await SecurityStampValidator.ValidatePrincipalAsync(context);
    }
}

public sealed class TenantUserClaimsPrincipalFactory(
    UserManager<TradeControlWebUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options,
    ITenantContext tenantContext)
    : UserClaimsPrincipalFactory<TradeControlWebUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TradeControlWebUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(TenantIdentity.TenantClaimType,
            tenantContext.RequiredTenant.TenantId.ToString("D")));
        return identity;
    }
}
