using System.Security.Claims;
using TradeControl.Web.Authorization;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public interface IVatFilingAuthorisationPolicy
{
    bool CanManageHmrcConnection(ClaimsPrincipal principal);
}

public sealed class VatFilingAuthorisationPolicy : IVatFilingAuthorisationPolicy
{
    public bool CanManageHmrcConnection(ClaimsPrincipal principal) =>
        principal?.Identity?.IsAuthenticated == true
        && (principal.IsInRole(Constants.AdministratorsRole)
            || principal.IsInRole(Constants.ManagersRole));
}
