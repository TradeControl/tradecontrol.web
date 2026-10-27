using Microsoft.Extensions.Options;
using TradeControl.Web.Tenancy;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public interface IVatSubmissionGate
{
    bool IsEnabled { get; }
}

public sealed class VatSubmissionGate(
    IOptions<VatProductHostOptions> hostOptions,
    ITenantContext tenantContext) : IVatSubmissionGate
{
    public bool IsEnabled => hostOptions.Value.SubmissionEnabled
        && tenantContext.IsResolved
        && tenantContext.RequiredTenant.VatSubmissionEnabled;
}
