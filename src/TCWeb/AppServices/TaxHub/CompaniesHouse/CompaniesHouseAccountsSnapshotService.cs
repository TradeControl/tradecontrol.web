using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Tax.UK.Application.Preparation;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

internal sealed record CompaniesHouseApprovedAccountsSnapshot(
    string ApprovalReference,
    string PreparationReference,
    string? ConversationReference,
    CompaniesHouseConversationState? ConversationState,
    string CompanyIdentitySha256,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string FilletedAccountsSha256,
    string FullAccountsSha256,
    string AccountsSnapshotSha256,
    string FullAccountsMediaType,
    byte[] FullAccountsBytes,
    DateTimeOffset? AcceptedAtUtc = null);

internal sealed record CompaniesHouseAcceptedAccountsSnapshot(
    CompaniesHouseApprovedAccountsSnapshot Accounts,
    string ConversationReference,
    DateTimeOffset AcceptedAtUtc);

/// <summary>
/// Supplies the exact full statutory-accounts artifact that is cryptographically paired with an
/// approved Companies House filleted artifact. Corporation Tax orchestration must use this boundary
/// instead of rebuilding accounts from the mutable accounting database.
/// </summary>
internal interface ICompaniesHouseAccountsSnapshotService
{
    Task<CompaniesHouseApprovedAccountsSnapshot> ReadApprovedAsync(string approvalReference,
        CancellationToken cancellationToken = default);
    Task<CompaniesHouseAcceptedAccountsSnapshot> ReadAcceptedForCorporationTaxAsync(
        string approvalReference, CancellationToken cancellationToken = default);
}

internal sealed class CompaniesHouseAccountsSnapshotService(
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    ICompaniesHouseWorkflowStore workflowStore,
    ICompaniesHouseProtectedContentStore contentStore) : ICompaniesHouseAccountsSnapshotService
{
    public async Task<CompaniesHouseApprovedAccountsSnapshot> ReadApprovedAsync(string approvalReference,
        CancellationToken cancellationToken = default) =>
        await ReadAsync(approvalReference, false, cancellationToken);

    public async Task<CompaniesHouseAcceptedAccountsSnapshot> ReadAcceptedForCorporationTaxAsync(
        string approvalReference, CancellationToken cancellationToken = default)
    {
        var accounts = await ReadAsync(approvalReference, true, cancellationToken);
        return new(accounts, accounts.ConversationReference!, accounts.AcceptedAtUtc!.Value);
    }

    private async Task<CompaniesHouseApprovedAccountsSnapshot> ReadAsync(string approvalReference,
        bool requireAcceptedConversation,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(approvalReference) || approvalReference.Length > 64)
            throw new ArgumentException("A valid Companies House approval reference is required.",
                nameof(approvalReference));

        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var approval = await workflowStore.GetApprovalAsync(identity.TenantReference,
            approvalReference, cancellationToken)
            ?? throw new KeyNotFoundException("The Companies House approval was not found.");
        var preparation = await workflowStore.GetPreparationAsync(identity.TenantReference,
            approval.PreparationReference, cancellationToken)
            ?? throw new InvalidOperationException("The approved Companies House preparation was not found.");

        if (!string.Equals(preparation.PrincipalReference, identity.ReportingSubjectReference,
                StringComparison.Ordinal))
            throw new UnauthorizedAccessException(
                "The approved accounts belong to another reporting subject.");
        if (string.IsNullOrWhiteSpace(preparation.ReviewedInputSha256)
            || string.IsNullOrWhiteSpace(preparation.FullAccountsSha256)
            || string.IsNullOrWhiteSpace(preparation.ProtectedFullAccountsReference)
            || string.IsNullOrWhiteSpace(preparation.AccountsSnapshotSha256)
            || string.IsNullOrWhiteSpace(approval.FullAccountsSha256)
            || string.IsNullOrWhiteSpace(approval.AccountsSnapshotSha256))
            throw new InvalidOperationException(
                "The approved accounts predate the immutable CH/CT accounts snapshot and cannot be used for Corporation Tax.");

        if (!string.Equals(approval.DocumentSha256, preparation.DocumentSha256, StringComparison.Ordinal)
            || !string.Equals(approval.FullAccountsSha256, preparation.FullAccountsSha256,
                StringComparison.Ordinal)
            || !string.Equals(approval.AccountsSnapshotSha256, preparation.AccountsSnapshotSha256,
                StringComparison.Ordinal)
            || approval.PeriodStart != preparation.PeriodStart
            || approval.PeriodEnd != preparation.PeriodEnd
            || !string.Equals(approval.CompanyIdentitySha256, preparation.CompanyIdentitySha256,
                StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Companies House approval does not match its retained statutory-accounts snapshot.");

        var expectedSnapshotSha256 = CompaniesHouseAccountsSnapshot.Create(
            preparation.CompanyIdentitySha256, preparation.PeriodStart, preparation.PeriodEnd,
            preparation.FilingProfile, preparation.SnapshotToken, preparation.ReviewedInputSha256,
            preparation.DocumentSha256, preparation.FullAccountsSha256);
        if (!string.Equals(expectedSnapshotSha256, preparation.AccountsSnapshotSha256,
                StringComparison.Ordinal))
            throw new InvalidOperationException("The retained CH/CT accounts snapshot proof is invalid.");

        var conversation = (await workflowStore.ListConversationsAsync(identity.TenantReference,
                approval.CompanyIdentitySha256, cancellationToken))
            .SingleOrDefault(value => string.Equals(value.ApprovalReference, approval.Reference,
                StringComparison.Ordinal));
        if (conversation is not null
            && (!string.Equals(conversation.DocumentSha256, approval.DocumentSha256,
                    StringComparison.Ordinal)
                || !string.Equals(conversation.PackageSha256, approval.PackageSha256,
                    StringComparison.Ordinal)))
            throw new InvalidOperationException(
                "The Companies House filing conversation does not match the approved accounts snapshot.");
        if (requireAcceptedConversation
            && conversation?.State != CompaniesHouseConversationState.Accepted)
            throw new InvalidOperationException(
                "Corporation Tax submission remains locked until Companies House has accepted the linked accounts.");

        var fullAccounts = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedFullAccountsReference, preparation.FullAccountsSha256,
            cancellationToken);

        return new(
            approval.Reference,
            preparation.Reference,
            conversation?.Reference,
            conversation?.State,
            approval.CompanyIdentitySha256,
            approval.PeriodStart,
            approval.PeriodEnd,
            approval.DocumentSha256,
            approval.FullAccountsSha256,
            approval.AccountsSnapshotSha256,
            "application/xhtml+xml",
            fullAccounts.Bytes)
        {
            AcceptedAtUtc = conversation?.State == CompaniesHouseConversationState.Accepted
                ? conversation.UpdatedAtUtc
                : null
        };
    }
}
