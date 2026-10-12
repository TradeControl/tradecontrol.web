using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.CompaniesHouse.Accounts.Tis6_0;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

internal sealed record CompaniesHouseAuthorityIdentifiers(string SubmissionNumber, string TransactionId);

internal interface ICompaniesHouseAuthorityIdentifierSource
{
    Task<CompaniesHouseAuthorityIdentifiers> AllocateAsync(CancellationToken cancellationToken = default);
}

internal sealed record CompaniesHouseMaterializedSubmission(ImmutableArray<byte> Content, string Sha256);

internal interface ICompaniesHouseSubmissionMaterializer
{
    Task<CompaniesHouseMaterializedSubmission> MaterializeAsync(
        CompaniesHouseAuthorityIdentifiers identifiers, CompaniesHouseProtectedContent packagePreview,
        CompaniesHouseProtectedContent accountsDocument, CancellationToken cancellationToken = default);
}

internal interface ICompaniesHouseSubmissionExchange
{
    Task<CompaniesHouseGatewayResponse> ExchangeAsync(CompaniesHouseMaterializedSubmission request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Continues an already-persisted approved filing conversation. The request is protected and the
/// conversation is atomically claimed before the exchange, so an ambiguous result cannot be replayed.
/// This service is not registered in the product host until its transport composition is reviewed.
/// </summary>
public interface ICompaniesHouseSubmissionDispatchService
{
    Task<CompaniesHouseFilingStatus> DispatchAsync(string conversationReference,
        CancellationToken cancellationToken = default);
}

internal sealed class CompaniesHouseSubmissionDispatchService(
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    ICompaniesHouseWorkflowStore workflowStore,
    ICompaniesHouseProtectedContentStore contentStore,
    ICompaniesHouseFilingAuthorisationPolicy filingAuthorisationPolicy,
    IHttpContextAccessor httpContextAccessor,
    ICompaniesHouseAuthorityIdentifierSource identifierSource,
    ICompaniesHouseSubmissionMaterializer materializer,
    ICompaniesHouseSubmissionExchange exchange,
    TimeProvider timeProvider) : ICompaniesHouseSubmissionDispatchService
{
    public async Task<CompaniesHouseFilingStatus> DispatchAsync(string conversationReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationReference) || conversationReference.Length > 64)
            throw new ArgumentException("A valid conversation reference is required.", nameof(conversationReference));

        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        if (!filingAuthorisationPolicy.CanApproveAccounts(principal))
            throw new UnauthorizedAccessException(
                "Companies House submission requires Administrator or Manager permission.");

        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var conversation = await workflowStore.GetConversationAsync(identity.TenantReference,
            conversationReference, cancellationToken)
            ?? throw new KeyNotFoundException("The Companies House filing conversation was not found.");
        if (conversation.State != CompaniesHouseConversationState.Prepared
            || conversation.LastRequestSha256 is not null
            || conversation.RecoveryRequired)
            return ToStatus(conversation);

        var approval = await workflowStore.GetApprovalAsync(identity.TenantReference,
            conversation.ApprovalReference, cancellationToken)
            ?? throw new InvalidOperationException("The filing conversation approval was not found.");
        var preparation = await workflowStore.GetPreparationAsync(identity.TenantReference,
            approval.PreparationReference, cancellationToken)
            ?? throw new InvalidOperationException("The filing conversation preparation was not found.");
        if (!string.Equals(conversation.DocumentSha256, approval.DocumentSha256, StringComparison.Ordinal)
            || !string.Equals(conversation.PackageSha256, approval.PackageSha256, StringComparison.Ordinal)
            || !string.Equals(preparation.DocumentSha256, approval.DocumentSha256, StringComparison.Ordinal)
            || !string.Equals(preparation.PackageSha256, approval.PackageSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("The filing conversation no longer matches its approved evidence.");

        var document = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedDocumentReference, conversation.DocumentSha256, cancellationToken);
        var package = await contentStore.ReadAsync(identity.TenantReference,
            preparation.ProtectedPackageReference, conversation.PackageSha256, cancellationToken);
        var identifiers = await identifierSource.AllocateAsync(cancellationToken);
        ValidateIdentifiers(identifiers);
        var request = await materializer.MaterializeAsync(identifiers, package, document, cancellationToken);
        ValidateMaterialized(request);
        var retainedRequest = await contentStore.WriteAsync(identity.TenantReference,
            "gateway-form-submission-request", request.Content.AsMemory(), cancellationToken);
        if (!string.Equals(retainedRequest.Sha256, request.Sha256, StringComparison.Ordinal))
            throw new InvalidOperationException("The materialized Companies House request changed during retention.");

        var now = timeProvider.GetUtcNow();
        var dispatching = conversation with
        {
            SubmissionNumber = identifiers.SubmissionNumber,
            AuthorityTransactionId = identifiers.TransactionId,
            State = CompaniesHouseConversationState.Dispatching,
            LastRequestSha256 = request.Sha256,
            ProtectedRequestReference = retainedRequest.Reference,
            UpdatedAtUtc = now,
            RetainUntil = CompaniesHouseProductPolicy.RetainUntil(now)
        };
        if (!await workflowStore.TryBeginDispatchAsync(dispatching, cancellationToken))
            return ToStatus(await workflowStore.GetConversationAsync(identity.TenantReference,
                conversationReference, cancellationToken)
                ?? throw new InvalidOperationException("The claimed filing conversation disappeared."));

        string? responseSha256 = null;
        string? protectedResponseReference = null;
        try
        {
            var response = await exchange.ExchangeAsync(request, cancellationToken);
            response.Validate();
            if (!string.Equals(response.RequestSha256, request.Sha256, StringComparison.Ordinal))
                throw new InvalidOperationException("The gateway response belongs to a different request.");
            var retainedResponse = await contentStore.WriteAsync(identity.TenantReference,
                "gateway-form-submission-response", response.Content.AsMemory(), cancellationToken);
            if (!string.Equals(retainedResponse.Sha256, response.ResponseSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("The Companies House response changed during retention.");
            responseSha256 = response.ResponseSha256;
            protectedResponseReference = retainedResponse.Reference;

            var acknowledgement = new CompaniesHouseEnvelopeSerializer()
                .ParseGatewayAcknowledgement(response.Content.AsSpan());
            if (response.Kind is not (CompaniesHouseGatewayResponseKind.Acknowledgement
                    or CompaniesHouseGatewayResponseKind.GatewayError)
                || acknowledgement.TransactionId is not null
                && !string.Equals(acknowledgement.TransactionId, identifiers.TransactionId,
                    StringComparison.Ordinal))
                throw new InvalidOperationException("The gateway acknowledgement does not match this filing request.");
            var errors = acknowledgement.Errors?.ToArray() ?? [];
            var completed = dispatching with
            {
                AuthorityTransactionId = acknowledgement.TransactionId ?? identifiers.TransactionId,
                State = errors.Length == 0
                    ? CompaniesHouseConversationState.Acknowledged
                    : CompaniesHouseConversationState.GatewayRejected,
                LastResponseSha256 = responseSha256,
                ProtectedResponseReference = protectedResponseReference,
                Errors = errors,
                AcknowledgedAtUtc = errors.Length == 0 ? timeProvider.GetUtcNow() : null,
                UpdatedAtUtc = timeProvider.GetUtcNow()
            };
            await workflowStore.UpsertConversationAsync(completed, cancellationToken);
            return ToStatus(completed);
        }
        catch (CompaniesHouseUnclassifiedResponseException exception)
        {
            var actual = Convert.ToHexString(SHA256.HashData(exception.ResponseContent.AsSpan()));
            if (string.Equals(actual, exception.ResponseSha256, StringComparison.Ordinal))
            {
                var retained = await contentStore.WriteAsync(identity.TenantReference,
                    "gateway-unclassified-response", exception.ResponseContent.AsMemory(), CancellationToken.None);
                responseSha256 = retained.Sha256;
                protectedResponseReference = retained.Reference;
            }
            return await RecordRecoveryAsync(dispatching, responseSha256,
                protectedResponseReference, CancellationToken.None);
        }
        catch
        {
            return await RecordRecoveryAsync(dispatching, responseSha256,
                protectedResponseReference, CancellationToken.None);
        }
    }

    private async Task<CompaniesHouseFilingStatus> RecordRecoveryAsync(
        CompaniesHouseConversationRecord dispatching, string? responseSha256,
        string? protectedResponseReference, CancellationToken cancellationToken)
    {
        var recovery = dispatching with
        {
            LastResponseSha256 = responseSha256,
            ProtectedResponseReference = protectedResponseReference,
            RecoveryRequired = true,
            SupportReference = $"CH-{dispatching.Reference[^12..].ToUpperInvariant()}",
            UpdatedAtUtc = timeProvider.GetUtcNow()
        };
        await workflowStore.UpsertConversationAsync(recovery, cancellationToken);
        return ToStatus(recovery);
    }

    private static CompaniesHouseFilingStatus ToStatus(CompaniesHouseConversationRecord value) =>
        new(value.Reference, value.SubmissionNumber, value.State, value.RecoveryRequired,
            value.UpdatedAtUtc, value.SupportReference, value.Errors);

    private static void ValidateIdentifiers(CompaniesHouseAuthorityIdentifiers value)
    {
        if (value.SubmissionNumber.Length != 6 || value.SubmissionNumber.Any(char.IsControl)
            || string.IsNullOrWhiteSpace(value.TransactionId) || value.TransactionId.Length > 32
            || value.TransactionId.Any(character => !char.IsAsciiLetterOrDigit(character)))
            throw new InvalidOperationException("The allocated Companies House authority identities are invalid.");
    }

    private static void ValidateMaterialized(CompaniesHouseMaterializedSubmission value)
    {
        if (value.Content.IsDefaultOrEmpty || value.Sha256.Length != 64
            || value.Sha256.Any(character => !Uri.IsHexDigit(character)))
            throw new InvalidOperationException("The materialized Companies House request is invalid.");
    }
}
