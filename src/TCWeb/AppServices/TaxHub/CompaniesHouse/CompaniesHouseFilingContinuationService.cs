using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Tax.UK.Adapters.TradeControl.Data;
using TradeControl.Tax.UK.Adapters.TradeControl.Readers;
using TradeControl.Tax.UK.Application.DataProvision;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.CompaniesHouse.Accounts.Tis6_0;
using TradeControl.Web.Data;
using TradeControl.Web.Tenancy;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public sealed record CompaniesHouseFilingContinuation(
    string ConversationReference,
    string ApprovalReference,
    string PreparationReference,
    string CategoryVersionId,
    string? SubmissionNumber,
    CompaniesHouseConversationState State,
    string DocumentSha256,
    string PackageSha256,
    DateTimeOffset UpdatedAtUtc,
    bool RecoveryRequired,
    string? SupportReference,
    bool StatusAcknowledgementRequired,
    bool StatusAcknowledged,
    IReadOnlyList<CompaniesHouseError> Errors,
    bool StatusCheckInProgress,
    string? LastStatusCheckOutcome,
    string? LastStatusTransactionId,
    DateTimeOffset? LastStatusCheckedAtUtc,
    string? LastStatusRequestSha256,
    string? LastStatusResponseSha256,
    string? ExaminerComment,
    string? StatusCheckSupportReference,
    IReadOnlyList<CompaniesHouseError> StatusCheckErrors,
    bool AcceptanceSimulated,
    DateTimeOffset? AcceptanceSimulatedAtUtc,
    string? AcceptanceSimulatedBy,
    DateOnly AccountsPeriodStart,
    DateOnly AccountsPeriodEnd,
    string AccountsSnapshotSha256,
    string? AmendsConversationReference,
    bool HasOpenAmendment = false);

public interface ICompaniesHouseFilingContinuationService
{
    Task<CompaniesHouseFilingContinuation?> FindAsync(DateOnly periodStart, DateOnly periodEnd,
        CancellationToken cancellationToken = default);
    Task<CompaniesHouseFilingContinuation?> FindAsync(ClaimsPrincipal principal,
        DateOnly periodStart, DateOnly periodEnd, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompaniesHouseFilingContinuation>> ListAsync(DateOnly identityAsOf,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompaniesHouseFilingContinuation>> ListAsync(ClaimsPrincipal principal,
        DateOnly identityAsOf, CancellationToken cancellationToken = default);
}

internal sealed class CompaniesHouseFilingContinuationService(
    NodeContext nodeContext,
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    ICompaniesHouseWorkflowStore workflowStore) : ICompaniesHouseFilingContinuationService
{
    public async Task<CompaniesHouseFilingContinuation?> FindAsync(DateOnly periodStart, DateOnly periodEnd,
        CancellationToken cancellationToken = default)
        => (await ListAsync(periodEnd, cancellationToken))
            .FirstOrDefault(value => value.AccountsPeriodStart == periodStart
                && value.AccountsPeriodEnd == periodEnd);

    public async Task<CompaniesHouseFilingContinuation?> FindAsync(ClaimsPrincipal principal,
        DateOnly periodStart, DateOnly periodEnd, CancellationToken cancellationToken = default)
        => (await ListAsync(principal, periodEnd, cancellationToken))
            .FirstOrDefault(value => value.AccountsPeriodStart == periodStart
                && value.AccountsPeriodEnd == periodEnd);

    public async Task<IReadOnlyList<CompaniesHouseFilingContinuation>> ListAsync(DateOnly identityAsOf,
        CancellationToken cancellationToken = default)
    {
        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        return await ListAsync(identity, identityAsOf, cancellationToken);
    }

    public async Task<IReadOnlyList<CompaniesHouseFilingContinuation>> ListAsync(
        ClaimsPrincipal principal, DateOnly identityAsOf,
        CancellationToken cancellationToken = default)
    {
        var identity = await identityAccessor.GetRequiredAsync(principal, cancellationToken);
        return await ListAsync(identity, identityAsOf, cancellationToken);
    }

    private async Task<IReadOnlyList<CompaniesHouseFilingContinuation>> ListAsync(
        CompaniesHouseWorkflowIdentity identity, DateOnly identityAsOf,
        CancellationToken cancellationToken)
    {
        var connectionString = nodeContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The statutory accounting source is unavailable.");

        var context = await new TcStatutoryContextReader(new ConnectionFactory(), connectionString)
            .ReadAsync(identityAsOf, cancellationToken);
        var companyNumber = CompaniesHouseReadinessService.NormalizeCompanyNumber(
            context.Identity.CompanyNumber);
        if (companyNumber.Length != 8 || !companyNumber.All(char.IsAsciiLetterOrDigit))
            throw new InvalidOperationException("The Companies House identity cannot be resolved safely.");

        var companyIdentity = CompaniesHouseFilingIdentity.Company(identity.TenantReference,
            companyNumber);
        var result = new List<CompaniesHouseFilingContinuation>();
        foreach (var conversation in await workflowStore.ListConversationsAsync(
                     identity.TenantReference, companyIdentity, cancellationToken))
        {
            var approval = await workflowStore.GetApprovalAsync(identity.TenantReference,
                conversation.ApprovalReference, cancellationToken);
            if (approval is null || !string.Equals(approval.FilingProfile,
                    CompaniesHouseProductPolicy.SupportedFilingProfile, StringComparison.Ordinal))
                continue;
            var preparation = await workflowStore.GetPreparationAsync(identity.TenantReference,
                approval.PreparationReference, cancellationToken)
                ?? throw new InvalidOperationException(
                    "The existing Companies House filing has no retained preparation metadata.");
            if (!string.Equals(preparation.PrincipalReference,
                    identity.ReportingSubjectReference, StringComparison.Ordinal)
                || !string.Equals(preparation.DocumentSha256,
                    conversation.DocumentSha256, StringComparison.Ordinal)
                || !string.Equals(preparation.PackageSha256,
                    conversation.PackageSha256, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The existing Companies House filing does not match its retained evidence.");
            if (string.IsNullOrWhiteSpace(approval.AccountsSnapshotSha256)
                || !string.Equals(approval.AccountsSnapshotSha256,
                    preparation.AccountsSnapshotSha256, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The existing Companies House filing has no valid CT accounts snapshot proof.");

            var statusCheck = conversation.StatusChecks?.LastOrDefault();
            result.Add(new(conversation.Reference, approval.Reference, preparation.Reference,
                preparation.CategoryTree?.VersionId
                    ?? throw new InvalidOperationException(
                        "The existing Companies House filing has no Category Tree statutory evidence."),
                conversation.SubmissionNumber, conversation.State,
                conversation.DocumentSha256, conversation.PackageSha256,
                conversation.UpdatedAtUtc, conversation.RecoveryRequired,
                conversation.SupportReference, conversation.StatusAcknowledgementRequired,
                conversation.StatusAcknowledged, conversation.Errors ?? [],
                statusCheck?.CheckState == CompaniesHouseStatusCheckState.Dispatching,
                statusCheck?.CheckState.ToString(), statusCheck?.AuthorityTransactionId,
                statusCheck?.CompletedAtUtc,
                statusCheck?.RequestSha256, statusCheck?.ResponseSha256,
                statusCheck?.ExaminerComment, statusCheck?.SupportReference,
                statusCheck?.Errors ?? [], conversation.AcceptanceSimulated,
                conversation.AcceptanceSimulatedAtUtc, conversation.AcceptanceSimulatedBy,
                approval.PeriodStart, approval.PeriodEnd, approval.AccountsSnapshotSha256,
                approval.AmendsConversationReference, false));
        }

        var predecessorsWithSuccessors = result
            .Where(value => value.AmendsConversationReference is not null)
            .Select(value => value.AmendsConversationReference!)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var accepted in result.Where(value =>
                     value.State == CompaniesHouseConversationState.Accepted))
        {
            if (await workflowStore.HasOpenAmendmentAsync(identity.TenantReference,
                    accepted.ConversationReference, cancellationToken))
                predecessorsWithSuccessors.Add(accepted.ConversationReference);
        }
        return result
            .Select(value => value with
            {
                HasOpenAmendment = predecessorsWithSuccessors.Contains(value.ConversationReference)
            })
            .OrderByDescending(value => value.AccountsPeriodEnd)
            .ThenByDescending(value => value.UpdatedAtUtc)
            .ToList();
    }
}

internal sealed class DisabledCompaniesHouseFilingContinuationService
    : ICompaniesHouseFilingContinuationService
{
    public Task<CompaniesHouseFilingContinuation?> FindAsync(DateOnly periodStart, DateOnly periodEnd,
        CancellationToken cancellationToken = default) => Task.FromResult<CompaniesHouseFilingContinuation?>(null);
    public Task<CompaniesHouseFilingContinuation?> FindAsync(ClaimsPrincipal principal,
        DateOnly periodStart, DateOnly periodEnd, CancellationToken cancellationToken = default)
        => Task.FromResult<CompaniesHouseFilingContinuation?>(null);
    public Task<IReadOnlyList<CompaniesHouseFilingContinuation>> ListAsync(DateOnly identityAsOf,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CompaniesHouseFilingContinuation>>([]);
    public Task<IReadOnlyList<CompaniesHouseFilingContinuation>> ListAsync(ClaimsPrincipal principal,
        DateOnly identityAsOf, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CompaniesHouseFilingContinuation>>([]);
}
