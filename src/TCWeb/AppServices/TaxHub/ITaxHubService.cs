using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub
{
    public interface ITaxHubService
    {
        Task<TaxHubResult> GetShellStateAsync();
        Task<TaxHubDashboardModel> GetDashboardAsync();
        Task<IReadOnlyList<TaxHubYearOption>> GetYearsAsync();
        Task<IReadOnlyList<TaxHubPeriodOption>> GetPeriodsAsync(short yearNumber);
        Task<TaxHubPeriodOption?> GetDefaultPeriodAsync();
        Task<TaxHubVatWorkspaceModel> GetVatWorkspaceAsync(short? yearNumber, DateTime? periodStartOn);
        Task<TaxHubAccountsWorkspaceModel> GetAccountsWorkspaceAsync(short? yearNumber, DateTime? periodStartOn);
    }
}
