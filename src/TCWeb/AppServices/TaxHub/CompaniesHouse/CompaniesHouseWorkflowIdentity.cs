using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradeControl.Web.Data;
using TradeControl.Web.Tenancy;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

internal sealed record CompaniesHouseWorkflowIdentity(
    string TenantReference,
    string AspNetSubjectReference,
    string ActorReference,
    string ReportingSubjectReference);

internal interface ICompaniesHouseWorkflowIdentityAccessor
{
    Task<CompaniesHouseWorkflowIdentity> GetRequiredAsync(CancellationToken cancellationToken = default);
}

internal sealed class CompaniesHouseWorkflowIdentityAccessor(
    IHttpContextAccessor httpContextAccessor,
    NodeContext nodeContext,
    ITenantContext tenantContext) : ICompaniesHouseWorkflowIdentityAccessor
{
    public async Task<CompaniesHouseWorkflowIdentity> GetRequiredAsync(
        CancellationToken cancellationToken = default)
    {
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        var aspNetSubject = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (principal.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(aspNetSubject))
            throw new UnauthorizedAccessException("An authenticated Trade Control subject is required.");

        if (!tenantContext.IsResolved)
            throw new InvalidOperationException("The Companies House workflow tenant is not configured.");
        var tenant = tenantContext.RequiredTenant.TenantId.ToString("D");

        var actor = await nodeContext.GetUserId(aspNetSubject);
        var reportingSubject = await nodeContext.App_tbOptions.AsNoTracking()
            .Select(option => option.SubjectCode)
            .SingleOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(reportingSubject))
            throw new InvalidOperationException("The server-derived Companies House identity is incomplete.");

        return new(tenant, aspNetSubject, actor, reportingSubject);
    }
}
