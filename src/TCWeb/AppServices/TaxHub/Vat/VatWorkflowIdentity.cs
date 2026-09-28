using System;
using System.Security.Claims;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public sealed record VatWorkflowIdentity
{
    internal VatWorkflowIdentity(string tenantReference, string aspNetSubjectReference,
        string actorReference, string reportingSubjectReference)
    {
        TenantReference = Required(tenantReference);
        AspNetSubjectReference = Required(aspNetSubjectReference);
        ActorReference = Required(actorReference);
        ReportingSubjectReference = Required(reportingSubjectReference);
    }

    public string TenantReference { get; }
    public string AspNetSubjectReference { get; }
    public string ActorReference { get; }
    public string ReportingSubjectReference { get; }

    private static string Required(string value) => string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException("A server-derived VAT workflow identity is incomplete.")
        : value.Trim();
}

public interface IVatWorkflowIdentityAccessor
{
    Task<VatWorkflowIdentity> GetRequiredAsync(CancellationToken cancellationToken = default);
    Task<VatWorkflowIdentity> GetRequiredAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);
}

public sealed class VatWorkflowIdentityAccessor(
    IHttpContextAccessor httpContextAccessor,
    NodeContext nodeContext,
    IOptions<VatProductHostOptions> hostOptions) : IVatWorkflowIdentityAccessor
{
    public async Task<VatWorkflowIdentity> GetRequiredAsync(CancellationToken cancellationToken = default)
    {
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        return await GetRequiredAsync(principal, cancellationToken);
    }

    public async Task<VatWorkflowIdentity> GetRequiredAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var aspNetSubject = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (principal.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(aspNetSubject))
            throw new UnauthorizedAccessException("An authenticated Trade Control subject is required.");

        var tenant = hostOptions.Value.TenantReference;
        if (string.IsNullOrWhiteSpace(tenant))
            throw new InvalidOperationException("The VAT workflow tenant is not configured.");

        var actor = await nodeContext.GetUserId(aspNetSubject);
        var reportingSubject = await nodeContext.App_tbOptions
            .AsNoTracking()
            .Select(option => option.SubjectCode)
            .SingleOrDefaultAsync(cancellationToken);

        return new VatWorkflowIdentity(tenant, aspNetSubject, actor, reportingSubject);
    }
}
