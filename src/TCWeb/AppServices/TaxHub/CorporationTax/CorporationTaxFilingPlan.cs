using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

internal enum CorporationTaxFilingPlanState
{
    Draft,
    ReviewRequired,
    ReadyForApproval,
    Approved,
    Dispatching,
    SubmissionUncertain,
    AwaitingPoll,
    Accepted,
    Rejected,
    RecoveryRequired
}

/// <summary>
/// Immutable identity for one Corporation Tax return. A long accounts period can therefore
/// produce two independently approved returns while both remain bound to the same accepted
/// Companies House accounts snapshot.
/// </summary>
internal sealed record CorporationTaxFilingPlanIdentity
{
    private const string Version = "TC-CT-FILING-PLAN-1";

    internal CorporationTaxFilingPlanIdentity(string tenantReference,
        string companyIdentitySha256, string accountsSnapshotSha256,
        DateOnly periodStart, DateOnly periodEnd)
    {
        TenantReference = Bounded(tenantReference, 128, nameof(tenantReference));
        CompanyIdentitySha256 = Sha256(companyIdentitySha256, nameof(companyIdentitySha256));
        AccountsSnapshotSha256 = Sha256(accountsSnapshotSha256, nameof(accountsSnapshotSha256));
        if (periodEnd < periodStart || periodEnd > periodStart.AddYears(1).AddDays(-1))
            throw new ArgumentException(
                "A Corporation Tax filing period must be a valid period of at most twelve months.");
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        LogicalIdentity = Hash(string.Join('\n',
            Version,
            TenantReference,
            CompanyIdentitySha256,
            AccountsSnapshotSha256,
            PeriodStart.ToString("yyyy-MM-dd"),
            PeriodEnd.ToString("yyyy-MM-dd")));
    }

    internal string TenantReference { get; }
    internal string CompanyIdentitySha256 { get; }
    internal string AccountsSnapshotSha256 { get; }
    internal DateOnly PeriodStart { get; }
    internal DateOnly PeriodEnd { get; }
    internal string LogicalIdentity { get; }

    private static string Bounded(string? value, int maximum, string parameterName) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= maximum
            && value.All(character => !char.IsControl(character))
                ? value
                : throw new ArgumentException("A bounded filing identity value is required.", parameterName);

    private static string Sha256(string? value, string parameterName) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit)
            ? value.ToUpperInvariant()
            : throw new ArgumentException("A SHA-256 filing identity value is required.", parameterName);

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal sealed record CorporationTaxFilingPlanRecord(
    string Reference,
    CorporationTaxFilingPlanIdentity Identity,
    string AccountsApprovalReference,
    string AccountsConversationReference,
    CorporationTaxFilingPlanState State,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? PreparationReference = null,
    string? ApprovalReference = null,
    string? ConversationReference = null);

/// <summary>
/// Durable continuation port. Implementations must atomically preserve one active filing plan for
/// each logical identity and scope every lookup by tenant. This contract intentionally exposes no
/// submission operation; CT-D preparation and authority gates are introduced in later slices.
/// </summary>
internal interface ICorporationTaxFilingPlanStore
{
    Task<CorporationTaxFilingPlanRecord?> GetAsync(string tenantReference,
        string logicalIdentity, CancellationToken cancellationToken = default);
    Task<bool> TryAddAsync(CorporationTaxFilingPlanRecord plan,
        CancellationToken cancellationToken = default);
}
