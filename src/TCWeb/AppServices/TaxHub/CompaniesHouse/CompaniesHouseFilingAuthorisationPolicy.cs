using System.Security.Claims;
using TradeControl.Web.Authorization;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public interface ICompaniesHouseFilingAuthorisationPolicy
{
    bool CanApproveAccounts(ClaimsPrincipal principal);
}

public sealed class CompaniesHouseFilingAuthorisationPolicy : ICompaniesHouseFilingAuthorisationPolicy
{
    public bool CanApproveAccounts(ClaimsPrincipal principal) =>
        principal?.Identity?.IsAuthenticated == true
        && (principal.IsInRole(Constants.AdministratorsRole)
            || principal.IsInRole(Constants.ManagersRole));
}
