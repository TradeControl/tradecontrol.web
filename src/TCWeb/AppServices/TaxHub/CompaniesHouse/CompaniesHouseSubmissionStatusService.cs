using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.CompaniesHouse.Accounts.Tis6_0;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

internal interface ICompaniesHouseStatusTransactionIdSource
{
    string Allocate();
}

internal sealed class CompaniesHouseStatusTransactionIdSource(TimeProvider timeProvider)
    : ICompaniesHouseStatusTransactionIdSource
{
    public string Allocate() =>
        $"{timeProvider.GetUtcNow():yyyyMMddHHmmssfff}{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}";
}

internal interface ICompaniesHouseStatusRequestMaterializer
{
    Task<CompaniesHouseMaterializedSubmission> MaterializeAsync(string submissionNumber,
        string transactionId, CancellationToken cancellationToken = default);
}

internal interface ICompaniesHouseStatusExchange
{
    Task<CompaniesHouseGatewayResponse> ExchangeAsync(CompaniesHouseMaterializedSubmission request,
        CancellationToken cancellationToken = default);
}

public interface ICompaniesHouseSubmissionStatusService
{
    Task<CompaniesHouseFilingStatus> CheckAsync(string conversationReference,
        CancellationToken cancellationToken = default);
}

internal sealed class CompaniesHouseSubmissionStatusService(
    ICompaniesHouseWorkflowIdentityAccessor identityAccessor,
    ICompaniesHouseWorkflowStore workflowStore,
    ICompaniesHouseProtectedContentStore contentStore,
    ICompaniesHouseFilingAuthorisationPolicy filingAuthorisationPolicy,
    IHttpContextAccessor httpContextAccessor,
    ICompaniesHouseStatusTransactionIdSource transactionIdSource,
    ICompaniesHouseStatusRequestMaterializer materializer,
    ICompaniesHouseStatusExchange exchange,
    TimeProvider timeProvider) : ICompaniesHouseSubmissionStatusService
{
    public async Task<CompaniesHouseFilingStatus> CheckAsync(string conversationReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationReference) || conversationReference.Length > 64)
            throw new ArgumentException("A valid conversation reference is required.", nameof(conversationReference));

        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        if (!filingAuthorisationPolicy.CanApproveAccounts(principal))
            throw new UnauthorizedAccessException(
                "Checking Companies House filing status requires Administrator or Manager permission.");

        var identity = await identityAccessor.GetRequiredAsync(cancellationToken);
        var conversation = await workflowStore.GetConversationAsync(identity.TenantReference,
            conversationReference, cancellationToken)
            ?? throw new KeyNotFoundException("The Companies House filing conversation was not found.");
        var approval = await workflowStore.GetApprovalAsync(identity.TenantReference,
            conversation.ApprovalReference, cancellationToken)
            ?? throw new InvalidOperationException("The filing conversation approval was not found.");
        var preparation = await workflowStore.GetPreparationAsync(identity.TenantReference,
            approval.PreparationReference, cancellationToken)
            ?? throw new InvalidOperationException("The filing conversation preparation was not found.");
        if (!string.Equals(preparation.PrincipalReference, identity.ReportingSubjectReference,
                StringComparison.Ordinal)
            || !string.Equals(preparation.DocumentSha256, conversation.DocumentSha256, StringComparison.Ordinal)
            || !string.Equals(preparation.PackageSha256, conversation.PackageSha256, StringComparison.Ordinal))
            throw new UnauthorizedAccessException(
                "The Companies House filing conversation does not belong to this reporting subject.");

        if (conversation.State is CompaniesHouseConversationState.Accepted
            or CompaniesHouseConversationState.Rejected
            or CompaniesHouseConversationState.GatewayRejected
            or CompaniesHouseConversationState.InternalFailure)
            return ToStatus(conversation);
        if (conversation.State is not (CompaniesHouseConversationState.Acknowledged
                or CompaniesHouseConversationState.Pending or CompaniesHouseConversationState.Parked)
            || conversation.RecoveryRequired || string.IsNullOrWhiteSpace(conversation.SubmissionNumber))
            throw new InvalidOperationException(
                "This Companies House filing is not eligible for a status enquiry.");

        var checks = conversation.StatusChecks?.ToList() ?? [];
        if (checks.LastOrDefault()?.CheckState == CompaniesHouseStatusCheckState.Dispatching)
            return ToStatus(conversation);

        var transactionId = transactionIdSource.Allocate();
        ValidateTransactionId(transactionId);
        var request = await materializer.MaterializeAsync(conversation.SubmissionNumber,
            transactionId, cancellationToken);
        ValidateMaterialized(request);
        var retainedRequest = await contentStore.WriteAsync(identity.TenantReference,
            "gateway-status-request", request.Content.AsMemory(), cancellationToken);
        if (!string.Equals(retainedRequest.Sha256, request.Sha256, StringComparison.Ordinal))
            throw new InvalidOperationException("The Companies House status request changed during retention.");

        var expectedUpdatedAt = conversation.UpdatedAtUtc;
        var startedAt = timeProvider.GetUtcNow();
        checks.Add(new($"chs1-{Guid.NewGuid():N}", transactionId,
            CompaniesHouseStatusCheckState.Dispatching, startedAt, null,
            request.Sha256, retainedRequest.Reference, null, null, null,
            null, null, null, null, null, []));
        var checking = conversation with
        {
            StatusChecks = checks,
            AcknowledgedAtUtc = conversation.AcknowledgedAtUtc ?? conversation.UpdatedAtUtc,
            UpdatedAtUtc = startedAt,
            RetainUntil = CompaniesHouseProductPolicy.RetainUntil(startedAt)
        };
        if (!await workflowStore.TryBeginStatusCheckAsync(checking, expectedUpdatedAt, cancellationToken))
            return ToStatus(await workflowStore.GetConversationAsync(identity.TenantReference,
                conversationReference, cancellationToken)
                ?? throw new InvalidOperationException("The filing conversation disappeared."));

        string? responseSha256 = null;
        string? protectedResponseReference = null;
        try
        {
            var response = await exchange.ExchangeAsync(request, cancellationToken);
            response.Validate();
            if (!string.Equals(response.RequestSha256, request.Sha256, StringComparison.Ordinal))
                throw new InvalidOperationException("The status response belongs to a different request.");
            var retainedResponse = await contentStore.WriteAsync(identity.TenantReference,
                "gateway-status-response", response.Content.AsMemory(), cancellationToken);
            if (!string.Equals(retainedResponse.Sha256, response.ResponseSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("The Companies House status response changed during retention.");
            responseSha256 = retainedResponse.Sha256;
            protectedResponseReference = retainedResponse.Reference;

            if (response.Kind == CompaniesHouseGatewayResponseKind.GatewayError)
            {
                var gateway = new CompaniesHouseEnvelopeSerializer()
                    .ParseGatewayAcknowledgement(response.Content.AsSpan());
                ValidateResponseTransaction(gateway.TransactionId, transactionId);
                var errors = ValidateErrors(gateway.Errors);
                if (errors.Count == 0)
                    throw new InvalidOperationException("The status gateway error contains no error detail.");
                var failed = CompleteCheck(checking, CompaniesHouseStatusCheckState.GatewayError,
                    responseSha256, protectedResponseReference, null, null, null, null, null,
                    errors, null);
                await workflowStore.UpsertConversationAsync(failed, cancellationToken);
                return ToStatus(failed);
            }
            if (response.Kind != CompaniesHouseGatewayResponseKind.SubmissionStatus)
                throw new InvalidOperationException("The gateway did not return a submission status.");

            var serializer = new CompaniesHouseEnvelopeSerializer();
            ValidateResponseTransaction(serializer.ParseGatewayAcknowledgement(
                response.Content.AsSpan()).TransactionId, transactionId);
            var statuses = serializer.ParseSubmissionStatuses(response.Content.AsSpan());
            if (statuses.Count != 1
                || !string.Equals(statuses[0].SubmissionNumber, conversation.SubmissionNumber,
                    StringComparison.Ordinal))
                throw new InvalidOperationException("The status response does not identify this submission uniquely.");
            var status = ValidateStatus(statuses[0], identity.TenantReference,
                conversation.CompanyIdentitySha256);
            var nextState = Map(status.StatusCode);
            var completed = CompleteCheck(checking, CompaniesHouseStatusCheckState.Completed,
                responseSha256, protectedResponseReference, nextState, status.CompanyNumber,
                status.CustomerReference, status.ExaminerTelephone, status.ExaminerComment,
                status.Errors, null) with
            {
                State = nextState,
                Errors = nextState == CompaniesHouseConversationState.Rejected ? status.Errors : [],
                StatusAcknowledgementRequired = false,
                StatusAcknowledged = false
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
                    "gateway-status-unclassified-response", exception.ResponseContent.AsMemory(),
                    CancellationToken.None);
                responseSha256 = retained.Sha256;
                protectedResponseReference = retained.Reference;
            }
            return await RecordRecoveryAsync(checking, responseSha256,
                protectedResponseReference, CancellationToken.None);
        }
        catch
        {
            return await RecordRecoveryAsync(checking, responseSha256,
                protectedResponseReference, CancellationToken.None);
        }
    }

    private async Task<CompaniesHouseFilingStatus> RecordRecoveryAsync(
        CompaniesHouseConversationRecord checking, string? responseSha256,
        string? protectedResponseReference, CancellationToken cancellationToken)
    {
        var supportReference = $"CHS-{checking.Reference[^12..].ToUpperInvariant()}";
        var recovery = CompleteCheck(checking, CompaniesHouseStatusCheckState.RecoveryRequired,
            responseSha256, protectedResponseReference, null, null, null, null, null, [], supportReference);
        await workflowStore.UpsertConversationAsync(recovery, cancellationToken);
        return ToStatus(recovery);
    }

    private CompaniesHouseConversationRecord CompleteCheck(CompaniesHouseConversationRecord conversation,
        CompaniesHouseStatusCheckState checkState, string? responseSha256,
        string? protectedResponseReference, CompaniesHouseConversationState? reportedState,
        string? companyNumber, string? customerReference, string? examinerTelephone,
        string? examinerComment, IReadOnlyList<CompaniesHouseError> errors, string? supportReference)
    {
        var checks = conversation.StatusChecks?.ToList()
            ?? throw new InvalidOperationException("The status check metadata is missing.");
        var current = checks[^1];
        var now = timeProvider.GetUtcNow();
        checks[^1] = current with
        {
            CheckState = checkState,
            CompletedAtUtc = now,
            ResponseSha256 = responseSha256,
            ProtectedResponseReference = protectedResponseReference,
            ReportedState = reportedState,
            CompanyNumber = companyNumber,
            CustomerReference = customerReference,
            ExaminerTelephone = examinerTelephone,
            ExaminerComment = examinerComment,
            SupportReference = supportReference,
            Errors = errors
        };
        return conversation with
        {
            StatusChecks = checks,
            UpdatedAtUtc = now,
            RetainUntil = CompaniesHouseProductPolicy.RetainUntil(now)
        };
    }

    private static CompaniesHouseSubmissionStatus ValidateStatus(CompaniesHouseSubmissionStatus status,
        string tenantReference, string expectedCompanyIdentity)
    {
        var companyNumber = Optional(status.CompanyNumber, 16, nameof(status.CompanyNumber));
        if (companyNumber is not null)
        {
            var normalized = CompaniesHouseReadinessService.NormalizeCompanyNumber(companyNumber);
            if (!string.Equals(CompaniesHouseFilingIdentity.Company(tenantReference, normalized),
                    expectedCompanyIdentity, StringComparison.Ordinal))
                throw new InvalidOperationException("The status response belongs to a different company.");
            companyNumber = CompaniesHouseReadinessService.MaskCompanyNumber(normalized);
        }
        var errors = ValidateErrors(status.Errors);
        if (status.StatusCode == CompaniesHouseSubmissionStatusCode.Reject && errors.Count == 0
            || status.StatusCode != CompaniesHouseSubmissionStatusCode.Reject && errors.Count != 0)
            throw new InvalidOperationException("The status response contains inconsistent rejection evidence.");
        return status with
        {
            CompanyNumber = companyNumber,
            CustomerReference = Optional(status.CustomerReference, 256, nameof(status.CustomerReference)),
            ExaminerTelephone = Optional(status.ExaminerTelephone, 128, nameof(status.ExaminerTelephone)),
            ExaminerComment = Optional(status.ExaminerComment, 1024, nameof(status.ExaminerComment)),
            Errors = errors
        };
    }

    private static IReadOnlyList<CompaniesHouseError> ValidateErrors(
        IReadOnlyList<CompaniesHouseError>? errors)
    {
        if (errors is null || errors.Count > 50)
            throw new InvalidOperationException("The Companies House error evidence is invalid.");
        return errors.Select(error => new CompaniesHouseError(
            Required(error.Code, 64, nameof(error.Code)),
            Required(error.Message, 512, nameof(error.Message)),
            Optional(error.Location, 256, nameof(error.Location)))).ToArray();
    }

    private static string Required(string? value, int maximumLength, string name) =>
        Optional(value, maximumLength, name)
        ?? throw new InvalidOperationException($"The Companies House {name} value is required.");

    private static string? Optional(string? value, int maximumLength, string name)
    {
        if (value is null) return null;
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength || value.Any(char.IsControl))
            throw new InvalidOperationException($"The Companies House {name} value is invalid.");
        return value;
    }

    private static CompaniesHouseConversationState Map(CompaniesHouseSubmissionStatusCode status) => status switch
    {
        CompaniesHouseSubmissionStatusCode.Accept => CompaniesHouseConversationState.Accepted,
        CompaniesHouseSubmissionStatusCode.Reject => CompaniesHouseConversationState.Rejected,
        CompaniesHouseSubmissionStatusCode.Pending => CompaniesHouseConversationState.Pending,
        CompaniesHouseSubmissionStatusCode.Parked => CompaniesHouseConversationState.Parked,
        CompaniesHouseSubmissionStatusCode.InternalFailure => CompaniesHouseConversationState.InternalFailure,
        _ => throw new InvalidOperationException("The Companies House status is unsupported.")
    };

    private static CompaniesHouseFilingStatus ToStatus(CompaniesHouseConversationRecord value) =>
        new(value.Reference, value.SubmissionNumber, value.State, value.RecoveryRequired,
            value.UpdatedAtUtc, value.SupportReference, value.Errors);

    private static void ValidateTransactionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 32
            || value.Any(character => !char.IsAsciiLetterOrDigit(character)))
            throw new InvalidOperationException("The allocated Companies House status transaction identity is invalid.");
    }

    private static void ValidateResponseTransaction(string? actual, string expected)
    {
        if (actual is not null && !string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidOperationException(
                "The Companies House status response transaction identity does not match the request.");
    }

    private static void ValidateMaterialized(CompaniesHouseMaterializedSubmission value)
    {
        if (value.Content.IsDefaultOrEmpty || value.Sha256.Length != 64
            || value.Sha256.Any(character => !Uri.IsHexDigit(character)))
            throw new InvalidOperationException("The materialized Companies House status request is invalid.");
    }
}

internal sealed class DisabledCompaniesHouseSubmissionStatusService : ICompaniesHouseSubmissionStatusService
{
    public Task<CompaniesHouseFilingStatus> CheckAsync(string conversationReference,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Companies House status enquiries are disabled in this environment.");
}
