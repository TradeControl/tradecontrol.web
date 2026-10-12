using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Application.Preparation;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public sealed record CompaniesHouseStagedSubmission(
    string ConversationReference,
    string ApprovalReference,
    CompaniesHouseConversationState State,
    string DocumentSha256,
    string PackageSha256,
    DateTimeOffset StagedAtUtc);

/// <summary>
/// Opens the durable filing conversation for an immutable approval. Staging is deliberately
/// transport-free: it allocates no authority identity and makes no Companies House request.
/// </summary>
public interface ICompaniesHouseSubmissionStagingService
{
    Task<CompaniesHouseStagedSubmission> StageApprovedAsync(string approvalReference,
        CancellationToken cancellationToken = default);
}

internal sealed class CompaniesHouseSubmissionStagingService(
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    ICompaniesHouseWorkflowStore workflowStore,
    ICompaniesHouseProtectedContentStore contentStore,
    ICompaniesHouseFilingAuthorisationPolicy filingAuthorisationPolicy,
    IHttpContextAccessor httpContextAccessor,
    IOptions<CompaniesHouseProductHostOptions> hostOptions,
    TimeProvider timeProvider) : ICompaniesHouseSubmissionStagingService
{
    public async Task<CompaniesHouseStagedSubmission> StageApprovedAsync(string approvalReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(approvalReference) || approvalReference.Length > 64)
            throw new ArgumentException("A valid approval reference is required.", nameof(approvalReference));
        if (!hostOptions.Value.Enabled)
            throw new InvalidOperationException("Companies House submission staging is unavailable.");

        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        if (!filingAuthorisationPolicy.CanApproveAccounts(principal))
            throw new UnauthorizedAccessException(
                "Companies House submission staging requires Administrator or Manager permission.");

        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var approval = await workflowStore.GetApprovalAsync(identity.TenantReference,
            approvalReference, cancellationToken)
            ?? throw new KeyNotFoundException("The Companies House approval was not found.");
        if (!approval.DeclarationsConfirmed
            || !string.Equals(approval.FilingProfile,
                CompaniesHouseProductPolicy.SupportedFilingProfile, StringComparison.Ordinal))
            throw new InvalidOperationException("The Companies House approval is not eligible for submission staging.");

        var existing = await workflowStore.GetActiveConversationAsync(identity.TenantReference,
            approval.LogicalFilingIdentity, cancellationToken);
        if (existing is not null)
            return EnsureSameApproval(existing, approval);

        var prior = (await workflowStore.ListConversationsAsync(identity.TenantReference,
                approval.CompanyIdentitySha256, cancellationToken))
            .FirstOrDefault(item => string.Equals(item.LogicalFilingIdentity,
                approval.LogicalFilingIdentity, StringComparison.Ordinal));
        if (prior is not null)
            return EnsureSameApproval(prior, approval);

        var preparation = await workflowStore.GetPreparationAsync(identity.TenantReference,
            approval.PreparationReference, cancellationToken)
            ?? throw new InvalidOperationException("The approved Companies House preparation was not found.");
        if (!string.Equals(preparation.DocumentSha256, approval.DocumentSha256, StringComparison.Ordinal)
            || !string.Equals(preparation.PackageSha256, approval.PackageSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("The approval does not match its retained Companies House evidence.");

        _ = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedDocumentReference, approval.DocumentSha256, cancellationToken);
        _ = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedPackageReference, approval.PackageSha256, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var conversation = new CompaniesHouseConversationRecord(
            $"chc1-{Guid.NewGuid():N}", approval.Reference, identity.TenantReference,
            approval.CompanyIdentitySha256, approval.LogicalFilingIdentity,
            null, null, CompaniesHouseConversationState.Prepared,
            approval.DocumentSha256, approval.PackageSha256,
            null, null, null, null, false, false, false, null,
            now, now, CompaniesHouseProductPolicy.RetainUntil(now));

        try
        {
            await workflowStore.UpsertConversationAsync(conversation, cancellationToken);
        }
        catch
        {
            // A competing authorised request may have won the unique active-filing race.
            // Resolve that durable conversation instead of creating or inviting a second send.
            existing = await workflowStore.GetActiveConversationAsync(identity.TenantReference,
                approval.LogicalFilingIdentity, cancellationToken);
            if (existing is null) throw;
            return EnsureSameApproval(existing, approval);
        }

        return ToResult(conversation);
    }

    private static CompaniesHouseStagedSubmission EnsureSameApproval(
        CompaniesHouseConversationRecord conversation, CompaniesHouseApprovalRecord approval)
    {
        if (!string.Equals(conversation.ApprovalReference, approval.Reference, StringComparison.Ordinal)
            || !string.Equals(conversation.DocumentSha256, approval.DocumentSha256, StringComparison.Ordinal)
            || !string.Equals(conversation.PackageSha256, approval.PackageSha256, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "An active Companies House filing conversation already exists for this company and period.");
        return ToResult(conversation);
    }

    private static CompaniesHouseStagedSubmission ToResult(CompaniesHouseConversationRecord conversation) =>
        new(conversation.Reference, conversation.ApprovalReference, conversation.State,
            conversation.DocumentSha256, conversation.PackageSha256, conversation.CreatedAtUtc);
}
