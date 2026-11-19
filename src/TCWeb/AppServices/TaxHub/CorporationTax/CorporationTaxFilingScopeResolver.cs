using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.AppServices.TaxHub.CompaniesHouse;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal interface ICorporationTaxFilingScopeResolver
{
    Task<TaxHubCorporationTaxContinuation> ResolveAsync(
        TaxHubCorporationTaxContinuation request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Rehydrates the exact accepted Companies House accounts artifact and turns its accounts period
/// into one or two immutable Corporation Tax filing identities. No mutable transaction-derived accounting value crosses
/// this boundary.
/// </summary>
internal sealed class CorporationTaxFilingScopeResolver(
    ICompaniesHouseAccountsSnapshotService accountsSnapshots,
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor) : ICorporationTaxFilingScopeResolver
{
    public async Task<TaxHubCorporationTaxContinuation> ResolveAsync(
        TaxHubCorporationTaxContinuation request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accepted = await accountsSnapshots.ReadAcceptedForCorporationTaxAsync(
            request.AccountsApprovalReference, cancellationToken);
        var accounts = accepted.Accounts;

        if (!string.Equals(accepted.ConversationReference,
                request.AccountsConversationReference, StringComparison.Ordinal)
            || accounts.PeriodStart != request.AccountsPeriodStart
            || accounts.PeriodEnd != request.AccountsPeriodEnd
            || !string.Equals(accounts.AccountsSnapshotSha256,
                request.AccountsSnapshotSha256, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The selected Companies House filing does not match its accepted accounts snapshot.");

        var workflowIdentity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var periods = CreatePeriods(workflowIdentity.TenantReference,
            accounts.CompanyIdentitySha256, accounts.AccountsSnapshotSha256,
            accounts.PeriodStart, accounts.PeriodEnd);

        return request with
        {
            CompanyIdentitySha256 = accounts.CompanyIdentitySha256,
            FullAccountsSha256 = accounts.FullAccountsSha256,
            AcceptedAtUtc = accepted.AcceptedAtUtc,
            Periods = periods
        };
    }

    internal static IReadOnlyList<TaxHubCorporationTaxPeriod> CreatePeriods(
        string tenantReference, string companyIdentitySha256, string accountsSnapshotSha256,
        DateOnly accountsPeriodStart, DateOnly accountsPeriodEnd)
    {
        var result = new List<TaxHubCorporationTaxPeriod>(2);
        var periodStart = accountsPeriodStart;
        while (periodStart <= accountsPeriodEnd)
        {
            if (result.Count == 2)
                throw new InvalidOperationException(
                    "The accepted accounts period requires more than two Corporation Tax returns and is outside the supported filing scope.");
            var maximumPeriodEnd = periodStart.AddYears(1).AddDays(-1);
            var periodEnd = accountsPeriodEnd < maximumPeriodEnd
                ? accountsPeriodEnd
                : maximumPeriodEnd;
            var identity = new CorporationTaxFilingPlanIdentity(tenantReference,
                companyIdentitySha256, accountsSnapshotSha256, periodStart, periodEnd);
            result.Add(new(periodStart, periodEnd, identity.LogicalIdentity));
            periodStart = periodEnd.AddDays(1);
        }

        return result;
    }
}

internal sealed class DisabledCorporationTaxFilingScopeResolver
    : ICorporationTaxFilingScopeResolver
{
    public Task<TaxHubCorporationTaxContinuation> ResolveAsync(
        TaxHubCorporationTaxContinuation request,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "Corporation Tax filing continuation is unavailable on this host.");
}
