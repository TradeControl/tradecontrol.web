using System;
using TradeControl.Tax.UK.Application.Preparation;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public sealed record VatAuthorityPrincipal
{
    internal VatAuthorityPrincipal(string tenantReference, string principalReference)
    {
        TenantReference = Required(tenantReference);
        PrincipalReference = Required(principalReference);
    }

    public string TenantReference { get; }
    public string PrincipalReference { get; }

    private static string Required(string value) => string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException("A protected HMRC principal reference is incomplete.")
        : value.Trim();
}

public interface IVatAuthorityDispatchContextFactory
{
    AuthorityDispatchContext Create(VatWorkflowIdentity identity, VatAuthorityPrincipal authorityPrincipal,
        string approvalReference, string sealedClientFactsReference,
        string? logicalSubmissionReference = null, string? subjectPeriodReference = null);
}

public sealed class VatAuthorityDispatchContextFactory : IVatAuthorityDispatchContextFactory
{
    public AuthorityDispatchContext Create(VatWorkflowIdentity identity,
        VatAuthorityPrincipal authorityPrincipal, string approvalReference,
        string sealedClientFactsReference, string? logicalSubmissionReference = null,
        string? subjectPeriodReference = null)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(authorityPrincipal);
        if (!string.Equals(identity.TenantReference, authorityPrincipal.TenantReference,
                StringComparison.Ordinal))
            throw new UnauthorizedAccessException("The HMRC principal belongs to another tenant.");

        return new AuthorityDispatchContext(identity.TenantReference,
            authorityPrincipal.PrincipalReference, identity.ActorReference, approvalReference,
            sealedClientFactsReference, logicalSubmissionReference, subjectPeriodReference);
    }
}
