using System;
using System.Collections.Generic;
using System.Linq;
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
    string PreparationReference,
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
    IReadOnlyList<CompaniesHouseError> StatusCheckErrors);

public interface ICompaniesHouseFilingContinuationService
{
    Task<CompaniesHouseFilingContinuation?> FindAsync(DateOnly periodStart, DateOnly periodEnd,
        CancellationToken cancellationToken = default);
}

internal sealed class CompaniesHouseFilingContinuationService(
    NodeContext nodeContext,
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    ICompaniesHouseWorkflowStore workflowStore,
    IBusinessClock businessClock) : ICompaniesHouseFilingContinuationService
{
    public async Task<CompaniesHouseFilingContinuation?> FindAsync(DateOnly periodStart, DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var connectionString = nodeContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The statutory accounting source is unavailable.");

        var context = await new TcStatutoryContextReader(new ConnectionFactory(), connectionString)
            .ReadAsync(businessClock.Today, cancellationToken);
        var companyNumber = CompaniesHouseReadinessService.NormalizeCompanyNumber(
            context.Identity.CompanyNumber);
        if (companyNumber.Length != 8 || !companyNumber.All(char.IsAsciiLetterOrDigit))
            throw new InvalidOperationException("The Companies House identity cannot be resolved safely.");

        var companyIdentity = CompaniesHouseFilingIdentity.Company(identity.TenantReference,
            companyNumber);
        foreach (var conversation in await workflowStore.ListConversationsAsync(
                     identity.TenantReference, companyIdentity, cancellationToken))
        {
            var approval = await workflowStore.GetApprovalAsync(identity.TenantReference,
                conversation.ApprovalReference, cancellationToken);
            if (approval is null || approval.PeriodStart != periodStart || approval.PeriodEnd != periodEnd
                || !string.Equals(approval.FilingProfile,
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

            var statusCheck = conversation.StatusChecks?.LastOrDefault();
            return new(conversation.Reference, preparation.Reference,
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
                statusCheck?.Errors ?? []);
        }

        return null;
    }
}

internal sealed class DisabledCompaniesHouseFilingContinuationService
    : ICompaniesHouseFilingContinuationService
{
    public Task<CompaniesHouseFilingContinuation?> FindAsync(DateOnly periodStart, DateOnly periodEnd,
        CancellationToken cancellationToken = default) => Task.FromResult<CompaniesHouseFilingContinuation?>(null);
}
