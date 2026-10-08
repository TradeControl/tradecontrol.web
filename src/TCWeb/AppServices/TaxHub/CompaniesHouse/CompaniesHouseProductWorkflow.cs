using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Tax.UK.Application.Preparation;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public enum CompaniesHouseProductAvailability
{
    Disabled,
    SendDisabled,
    Available
}

public sealed record CompaniesHouseProductStatus(CompaniesHouseProductAvailability Availability,
    string SafeReason);
public sealed record CompaniesHousePreparationReference(string Value, string DocumentSha256,
    string PackageSha256, DateTimeOffset ExpiresAtUtc);
public sealed record CompaniesHouseApprovalReference(string Value, string PreparationReference,
    string DeclarationVersion, DateTimeOffset ApprovedAtUtc);
public sealed record CompaniesHouseFilingStatus(string ConversationReference, string? SubmissionNumber,
    CompaniesHouseConversationState State, bool RecoveryRequired, DateTimeOffset UpdatedAtUtc,
    string? SupportReference = null);
public sealed record CompaniesHouseFilingHistoryItem(string ConversationReference, DateOnly PeriodEnd,
    string FilingProfile, string? SubmissionNumber, CompaniesHouseConversationState State,
    string DocumentSha256, string PackageSha256, DateTimeOffset UpdatedAtUtc,
    bool RecoveryRequired, string? SupportReference = null);

/// <summary>
/// API-shaped product boundary for Companies House accounts. Implementations derive tenant,
/// company, principal and actor identity on the server. Callers cannot supply credentials,
/// statutory values, raw iXBRL/GovTalk content or authority responses.
/// </summary>
public interface ICompaniesHouseProductWorkflow
{
    Task<CompaniesHouseProductStatus> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<CompaniesHousePreparationReference> PrepareAccountsAsync(DateOnly periodEnd,
        CancellationToken cancellationToken = default);
    Task<CompaniesHouseApprovalReference> RecordApprovalAsync(string preparationReference,
        string declarationVersion, bool warningsAcknowledged,
        CancellationToken cancellationToken = default);
    Task<CompaniesHouseFilingStatus> SubmitApprovedAsync(string approvalReference,
        CancellationToken cancellationToken = default);
    Task<CompaniesHouseFilingStatus> RefreshStatusAsync(string conversationReference,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompaniesHouseFilingHistoryItem>> GetFilingHistoryAsync(
        CancellationToken cancellationToken = default);
}

internal static class CompaniesHouseProductPolicy
{
    internal const string SupportedFilingProfile = "FRS-105-MICRO-ENTITY-UNAUDITED-FILLETED";
    internal static readonly TimeSpan PreparationLifetime = TimeSpan.FromHours(24);

    // Product evidence is retained for seven years from the last conversation event. This is an
    // operational evidence policy, not a representation of Companies Act accounting-record limits.
    internal static DateOnly RetainUntil(DateTimeOffset lastEventAtUtc) =>
        DateOnly.FromDateTime(lastEventAtUtc.UtcDateTime).AddYears(7);
}

internal sealed record CompaniesHousePreparationRecord(
    string Reference,
    string TenantReference,
    string PrincipalReference,
    string ActorReference,
    string CompanyIdentitySha256,
    string MaskedCompanyNumber,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string FilingProfile,
    string SourceSystem,
    string DatasetKey,
    string SnapshotToken,
    string DocumentSha256,
    string PackageSha256,
    string ProtectedDocumentReference,
    string ProtectedPackageReference,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc);

internal sealed record CompaniesHouseApprovalRecord(
    string Reference,
    string PreparationReference,
    string TenantReference,
    string PrincipalReference,
    string ActorReference,
    string LogicalFilingIdentity,
    string CompanyIdentitySha256,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string FilingProfile,
    string DeclarationVersion,
    string DeclarationSha256,
    string SourceSystem,
    string DatasetKey,
    string SnapshotToken,
    string DocumentSha256,
    string PackageSha256,
    bool WarningsAcknowledged,
    DateTimeOffset ApprovedAtUtc);

internal sealed record CompaniesHouseConversationRecord(
    string Reference,
    string ApprovalReference,
    string TenantReference,
    string CompanyIdentitySha256,
    string LogicalFilingIdentity,
    string? SubmissionNumber,
    string? AuthorityTransactionId,
    CompaniesHouseConversationState State,
    string DocumentSha256,
    string PackageSha256,
    string? LastRequestSha256,
    string? LastResponseSha256,
    string? ProtectedRequestReference,
    string? ProtectedResponseReference,
    bool StatusAcknowledgementRequired,
    bool StatusAcknowledged,
    bool RecoveryRequired,
    string? SupportReference,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateOnly RetainUntil);

/// <summary>
/// Durable persistence port. Implementations must make each mutation atomic, scope every lookup
/// by tenant and logical filing identity, and preserve one active conversation across restarts.
/// Protected references are opaque store handles, never file paths or browser values.
/// </summary>
internal interface ICompaniesHouseWorkflowStore
{
    Task AddPreparationAsync(CompaniesHousePreparationRecord preparation,
        CancellationToken cancellationToken = default);
    Task<CompaniesHousePreparationRecord?> GetPreparationAsync(string tenantReference, string reference,
        CancellationToken cancellationToken = default);
    Task AddApprovalAsync(CompaniesHouseApprovalRecord approval,
        CancellationToken cancellationToken = default);
    Task<CompaniesHouseApprovalRecord?> GetApprovalAsync(string tenantReference, string reference,
        CancellationToken cancellationToken = default);
    Task<CompaniesHouseConversationRecord?> GetActiveConversationAsync(string tenantReference,
        string logicalFilingIdentity, CancellationToken cancellationToken = default);
    Task UpsertConversationAsync(CompaniesHouseConversationRecord conversation,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompaniesHouseConversationRecord>> ListConversationsAsync(string tenantReference,
        string companyIdentitySha256, CancellationToken cancellationToken = default);
}
