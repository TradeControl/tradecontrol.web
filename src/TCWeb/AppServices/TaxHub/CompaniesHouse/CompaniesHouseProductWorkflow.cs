using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.CompaniesHouse.Accounts.Tis6_0;

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
    string? SupportReference = null, IReadOnlyList<CompaniesHouseError>? Errors = null);
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

internal static class CompaniesHouseFilingIdentity
{
    internal static string Company(string tenantReference, string companyNumber) =>
        Sha256($"{tenantReference}|{companyNumber.Trim().ToUpperInvariant()}");

    internal static string Logical(string tenantReference, string companyIdentitySha256,
        DateOnly periodStart, DateOnly periodEnd, string filingProfile) =>
        Sha256($"{tenantReference}|{companyIdentitySha256}|{periodStart:yyyy-MM-dd}|" +
               $"{periodEnd:yyyy-MM-dd}|{filingProfile}");

    private static string Sha256(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

internal static class CompaniesHouseAccountsSnapshot
{
    private const string Version = "CH-CT-ACCOUNTS-SNAPSHOT-1";

    internal static string Create(string companyIdentitySha256, DateOnly periodStart,
        DateOnly periodEnd, string filingProfile, string sourceSnapshotToken, string reviewedInputSha256,
        string filletedAccountsSha256, string fullAccountsSha256)
    {
        var manifest = string.Join('\n',
            Version,
            companyIdentitySha256,
            periodStart.ToString("yyyy-MM-dd"),
            periodEnd.ToString("yyyy-MM-dd"),
            filingProfile,
            sourceSnapshotToken,
            reviewedInputSha256,
            filletedAccountsSha256,
            fullAccountsSha256);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(manifest)));
    }
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
    DateTimeOffset ExpiresAtUtc,
    short? YearNumber = null,
    DateOnly? SelectedPeriodStart = null,
    string? ReviewedInputSha256 = null,
    string? ProtectedInputReference = null,
    string? FullAccountsSha256 = null,
    string? ProtectedFullAccountsReference = null,
    string? AccountsSnapshotSha256 = null);

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
    bool DeclarationsConfirmed,
    DateTimeOffset ApprovedAtUtc,
    string? FullAccountsSha256 = null,
    string? AccountsSnapshotSha256 = null);

internal enum CompaniesHouseStatusCheckState
{
    Dispatching,
    Completed,
    GatewayError,
    RecoveryRequired
}

internal sealed record CompaniesHouseStatusCheckRecord(
    string Reference,
    string AuthorityTransactionId,
    CompaniesHouseStatusCheckState CheckState,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string RequestSha256,
    string ProtectedRequestReference,
    string? ResponseSha256,
    string? ProtectedResponseReference,
    CompaniesHouseConversationState? ReportedState,
    string? CompanyNumber,
    string? CustomerReference,
    string? ExaminerTelephone,
    string? ExaminerComment,
    string? SupportReference,
    IReadOnlyList<CompaniesHouseError>? Errors = null);

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
    DateOnly RetainUntil,
    IReadOnlyList<CompaniesHouseError>? Errors = null,
    IReadOnlyList<CompaniesHouseStatusCheckRecord>? StatusChecks = null,
    DateTimeOffset? AcknowledgedAtUtc = null);

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
    Task<CompaniesHouseApprovalRecord?> GetApprovalForPreparationAsync(string tenantReference,
        string preparationReference, CancellationToken cancellationToken = default);
    Task<CompaniesHouseConversationRecord?> GetActiveConversationAsync(string tenantReference,
        string logicalFilingIdentity, CancellationToken cancellationToken = default);
    Task<CompaniesHouseConversationRecord?> GetConversationAsync(string tenantReference,
        string reference, CancellationToken cancellationToken = default);
    Task<bool> TryBeginDispatchAsync(CompaniesHouseConversationRecord conversation,
        CancellationToken cancellationToken = default);
    Task<bool> TryBeginStatusCheckAsync(CompaniesHouseConversationRecord conversation,
        DateTimeOffset expectedUpdatedAtUtc, CancellationToken cancellationToken = default);
    Task UpsertConversationAsync(CompaniesHouseConversationRecord conversation,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompaniesHouseConversationRecord>> ListConversationsAsync(string tenantReference,
        string companyIdentitySha256, CancellationToken cancellationToken = default);
}

internal sealed record CompaniesHouseProtectedContent(string Reference, string Sha256, long Length,
    byte[] Bytes);

/// <summary>
/// Exact statutory and gateway bytes are held outside workflow metadata. Implementations must use
/// tenant-scoped opaque handles, verify the expected SHA-256 on every read and refuse replacement.
/// </summary>
internal interface ICompaniesHouseProtectedContentStore
{
    Task<CompaniesHouseProtectedContent> WriteAsync(string tenantReference, string contentKind,
        ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default);
    Task<CompaniesHouseProtectedContent> ReadAsync(string tenantReference, string reference,
        string expectedSha256, CancellationToken cancellationToken = default);
}
