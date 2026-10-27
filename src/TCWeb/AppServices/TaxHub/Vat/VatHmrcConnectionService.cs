using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.Audit;
using TradeControl.Tax.UK.Adapters.Submission.Configuration;
using TradeControl.Tax.UK.Adapters.Submission.FraudPrevention;
using TradeControl.Tax.UK.Adapters.Submission.OAuth;
using TradeControl.Tax.UK.Adapters.Submission.Rest;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.Hmrc.Vat.v1_0.Obligations;
using TradeControl.Tax.UK.Hmrc.Vat.v1_0.Returns;
using TradeControl.Web.Tenancy;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public interface IVatHmrcConnectionService
{
    Task<VatConnectionStatus> GetStatusAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task<Uri> BeginConnectAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task<VatConnectionStatus> CompleteCallbackAsync(ClaimsPrincipal principal, string state,
        string? code, string? error, CancellationToken cancellationToken = default);
    Task<VatConnectionStatus> CompleteBrokerCallbackAsync(ClaimsPrincipal principal, string ticket,
        CancellationToken cancellationToken = default);
    Task DisconnectAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public sealed record VatFraudHeaderValidationResult(
    int? StatusCode,
    string? ContentType,
    byte[]? Body,
    OAuthReauthorisationReason? ReauthorisationReason = null);

public interface IVatFraudHeaderValidationService
{
    Task<VatFraudHeaderValidationResult> ValidateAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);
}

public sealed record VatAuthoritySubmissionResult(
    PreparedApiOutcome Outcome,
    VatReturnResponse? Receipt,
    VatReturnReconciliationResult? Reconciliation,
    bool ObligationRefreshSucceeded,
    string? ReadbackOutcomeCode = null,
    IReadOnlyList<VatAuthorityError>? Errors = null);

public sealed record VatAuthorityError(string Code, string Message);

public interface IVatAuthorityReturnSubmission
{
    Task<VatAuthoritySubmissionResult> DispatchApprovedAsync(ApprovedVatReturnDispatch approved,
        CancellationToken cancellationToken = default);
    Task<SubmissionReconciliationEvidence> ReconcileApprovedAsync(ApprovedVatReturnDispatch approved,
        string attemptReference, CancellationToken cancellationToken = default);
}

public sealed class VatHmrcConnectionService : IVatHmrcConnectionService, IVatAuthorityObligationSource,
    IVatAuthorityReturnReadbackSource, IVatAuthorityReturnSubmission,
    IVatFraudHeaderValidationService, IDisposable
{
    private readonly IVatWorkflowIdentityAccessor _identities;
    private readonly VatProductHostOptions _host;
    private readonly HmrcOAuthService? _oauth;
    private readonly HmrcOAuthTokenEndpoint? _tokens;
    private readonly FraudHeaderService? _fraud;
    private readonly HmrcPreparedApiRequestGateway? _gateway;
    private readonly HmrcFraudPreventionValidator? _validator;
    private readonly ISubmissionAttemptStore? _attempts;
    private readonly ISubmissionContentStore? _content;
    private readonly IVatFraudContextReferenceStore _fraudReferences;
    private readonly IVatSubmissionGate _submissionGate;
    private readonly ITenantContext? _tenantContext;
    private readonly TimeProvider _clock;
    private readonly OAuthBrokerTicketProtector? _brokerTickets;

    public VatHmrcConnectionService(IVatWorkflowIdentityAccessor identities,
        IOptions<VatProductHostOptions> hostOptions,
        IVatFraudContextReferenceStore fraudReferences,
        IVatSubmissionGate submissionGate,
        AzureVatPersistence? azurePersistence = null,
        ITenantContext? tenantContext = null,
        TimeProvider? clock = null)
    {
        _identities = identities;
        _fraudReferences = fraudReferences;
        _submissionGate = submissionGate;
        _tenantContext = tenantContext;
        _clock = clock ?? TimeProvider.System;
        _host = hostOptions.Value;
        if (!_host.Enabled) return;

        if (_host.PersistenceMode == VatPersistenceMode.AzureManaged)
        {
            if (azurePersistence is null || tenantContext is null || !tenantContext.IsResolved)
                throw new InvalidOperationException("The Azure VAT persistence or tenant context is unavailable.");
            var environment = EnvironmentSelector.Sandbox();
            var secrets = new EnvironmentSecretProvider(HmrcSecretReferences.AppServiceEnvironmentVariables);
            _tokens = new HmrcOAuthTokenEndpoint(environment);
            _validator = new HmrcFraudPreventionValidator(environment);
            var callback = _host.OAuthCallbackMode == VatOAuthCallbackMode.Brokered
                ? _host.OAuthBrokerCallbackUri!
                : new UriBuilder(Uri.UriSchemeHttps, tenantContext.RequiredTenant.CanonicalHost)
                    { Path = _host.OAuthCallbackPath }.Uri;
            if (_host.OAuthCallbackMode == VatOAuthCallbackMode.Brokered)
            {
                var encodedKey = Environment.GetEnvironmentVariable(
                    _host.OAuthBrokerEnvelopeKeyEnvironmentVariable)
                    ?? throw new InvalidOperationException("The OAuth broker envelope key is unavailable.");
                _brokerTickets = OAuthBrokerTicketProtector.FromBase64(encodedKey);
            }
            _oauth = HmrcOAuthService.CreateSandbox(secrets, azurePersistence, _tokens,
                new HmrcOAuthOptions(callback, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5), 18)
                {
                    ApprovedProductCallbackHost = callback.IdnHost
                });
            if (_host.FraudCaptureEnabled)
            {
                _fraud = FraudHeaderService.Create(CreateTopology(_host), CreateVendor(), azurePersistence,
                    sandboxValidatorDiagnostic: _host.AllowIncompleteSandboxFraudHeaders);
                _attempts = azurePersistence;
                _content = azurePersistence;
                _gateway = new HmrcPreparedApiRequestGateway(environment, _oauth, _fraud,
                    _attempts, _content);
            }
            return;
        }

        if (_host.PersistenceMode != VatPersistenceMode.DevelopmentFiles)
            throw new InvalidOperationException("The configured VAT persistence facility is unavailable.");

        var root = Path.GetFullPath(_host.DevelopmentStoreRoot!);
        Directory.CreateDirectory(root);
        var key = LoadOrCreateKey(Path.Combine(root, "oauth-store.key"));
        try
        {
            ISecretProvider secrets = _host.SandboxSecretSource switch
            {
                VatSandboxSecretSource.DevelopmentJsonFile => new JsonFileSecretProvider(
                    _host.DevelopmentClientSettingsPath!, HmrcSecretReferences.LegacyVatSandboxJsonProperties),
                VatSandboxSecretSource.EnvironmentVariables => new EnvironmentSecretProvider(
                    HmrcSecretReferences.AppServiceEnvironmentVariables),
                _ => throw new InvalidOperationException("The sandbox secret source is not supported.")
            };
            var environment = EnvironmentSelector.Sandbox();
            _tokens = new HmrcOAuthTokenEndpoint(environment);
            _validator = new HmrcFraudPreventionValidator(environment);
            var callback = new Uri(_host.PublicOrigin!, _host.OAuthCallbackPath);
            _oauth = HmrcOAuthService.CreateFileBackedSandbox(Path.Combine(root, "oauth-grants.json.enc"),
                key, secrets, _tokens, options: new HmrcOAuthOptions(callback, TimeSpan.FromMinutes(10),
                    TimeSpan.FromMinutes(5), 18));

            if (_host.FraudCaptureEnabled)
            {
                var topology = CreateTopology(_host);
                var fraudKey = LoadOrCreateKey(Path.Combine(root, "fraud-store.key"));
                try
                {
                var vendor = CreateVendor();
                _fraud = _host.AllowIncompleteSandboxFraudHeaders
                    ? FraudHeaderService.CreateFileBackedSandboxReference(
                        Path.Combine(root, "fraud-contexts"), fraudKey, topology, vendor)
                    : FraudHeaderService.CreateFileBacked(
                        Path.Combine(root, "fraud-contexts"), fraudKey, topology, vendor);
                }
                finally { CryptographicOperations.ZeroMemory(fraudKey); }
                _attempts = new FileSubmissionAttemptStore(
                    SubmissionAttemptStoreOptions.SevenYearMetadata(Path.Combine(root, "attempts.json")));
                _content = new FileSubmissionContentStore(new SubmissionContentStoreOptions(
                    Path.Combine(root, "protected-content")));
                _gateway = new HmrcPreparedApiRequestGateway(environment, _oauth, _fraud,
                    _attempts, _content);
            }
        }
        finally { CryptographicOperations.ZeroMemory(key); }
    }

    public async Task<VatConnectionStatus> GetStatusAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (_oauth is null) return new(VatConnectionState.Unavailable, "VAT-HOST-DISABLED");
        var context = await ContextAsync(principal, cancellationToken);
        using var read = await _oauth.GetAccessAsync(context, HmrcOAuthScopes.ReadVat, cancellationToken);
        if (read.Kind != OAuthAccessOutcomeKind.Available)
            return Map(read.ReauthorisationReason);
        using var write = await _oauth.GetAccessAsync(context, HmrcOAuthScopes.WriteVat, cancellationToken);
        return write.Kind == OAuthAccessOutcomeKind.Available
            ? new(VatConnectionState.Connected)
            : Map(write.ReauthorisationReason);
    }

    public async Task<Uri> BeginConnectAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var oauth = _oauth ?? throw new InvalidOperationException("The VAT host is unavailable.");
        var dispatch = await ContextAsync(principal, cancellationToken);
        var start = await oauth.BeginAuthorisationAsync(dispatch,
            HmrcOAuthScopes.VatReadWrite, cancellationToken);
        if (_host.OAuthCallbackMode == VatOAuthCallbackMode.Direct) return start.AuthorisationUri;

        var protector = _brokerTickets
            ?? throw new InvalidOperationException("The OAuth broker ticket service is unavailable.");
        var tenant = _tenantContext?.IsResolved == true
            ? _tenantContext.RequiredTenant
            : throw new InvalidOperationException("The tenant context is unavailable.");
        var query = QueryHelpers.ParseQuery(start.AuthorisationUri.Query);
        if (!query.TryGetValue("state", out var stateValues) || stateValues.Count != 1
            || string.IsNullOrWhiteSpace(stateValues[0]))
            throw new InvalidOperationException("The authority request did not contain a valid state.");
        var now = _clock.GetUtcNow();
        var handoff = new OAuthBrokerHandoff(AuthorityEnvironmentName(), dispatch.TenantReference,
            dispatch.AuthorisationPrincipalReference, dispatch.ActorReference,
            start.AuthorisationUri.AbsoluteUri, stateValues[0]!,
            new UriBuilder(Uri.UriSchemeHttps, tenant.CanonicalHost).Uri.AbsoluteUri,
            now, start.ExpiresAtUtc);
        var ticket = protector.Protect(handoff, OAuthBrokerContract.HandoffPurpose);
        return new Uri(QueryHelpers.AddQueryString(_host.OAuthBrokerStartUri!.AbsoluteUri,
            "handoff", ticket));
    }

    public async Task<VatConnectionStatus> CompleteCallbackAsync(ClaimsPrincipal principal, string state,
        string? code, string? error, CancellationToken cancellationToken = default)
    {
        if (_host.OAuthCallbackMode != VatOAuthCallbackMode.Direct)
            throw new OAuthCallbackValidationException("The direct OAuth callback is disabled for this host.");
        var oauth = _oauth ?? throw new InvalidOperationException("The VAT host is unavailable.");
        using var outcome = await oauth.CompleteCallbackAsync(await ContextAsync(principal, cancellationToken),
            new OAuthCallback(state, code, error), cancellationToken);
        return outcome.Kind == OAuthAccessOutcomeKind.Available
            ? new(VatConnectionState.Connected)
            : Map(outcome.ReauthorisationReason);
    }

    public async Task<VatConnectionStatus> CompleteBrokerCallbackAsync(ClaimsPrincipal principal,
        string ticket, CancellationToken cancellationToken = default)
    {
        if (_host.OAuthCallbackMode != VatOAuthCallbackMode.Brokered)
            throw new OAuthCallbackValidationException("The brokered OAuth callback is disabled for this host.");
        var oauth = _oauth ?? throw new InvalidOperationException("The VAT host is unavailable.");
        var protector = _brokerTickets
            ?? throw new InvalidOperationException("The OAuth broker ticket service is unavailable.");
        OAuthBrokerCompletion completion;
        try { completion = protector.Unprotect<OAuthBrokerCompletion>(ticket, OAuthBrokerContract.CompletionPurpose); }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException)
        {
            _ = exception;
            throw new OAuthCallbackValidationException("The broker completion ticket is invalid.");
        }
        var dispatch = await ContextAsync(principal, cancellationToken);
        var tenant = _tenantContext?.IsResolved == true
            ? _tenantContext.RequiredTenant
            : throw new OAuthCallbackValidationException("The tenant context is unavailable.");
        var expectedOrigin = new UriBuilder(Uri.UriSchemeHttps, tenant.CanonicalHost).Uri.AbsoluteUri;
        var now = _clock.GetUtcNow();
        ValidateBrokerCompletion(completion, AuthorityEnvironmentName(), dispatch.TenantReference,
            dispatch.AuthorisationPrincipalReference, dispatch.ActorReference, expectedOrigin, now);
        using var outcome = await oauth.CompleteCallbackAsync(dispatch,
            new OAuthCallback(completion.State, completion.Code, completion.Error), cancellationToken);
        return outcome.Kind == OAuthAccessOutcomeKind.Available
            ? new(VatConnectionState.Connected)
            : Map(outcome.ReauthorisationReason);
    }

    internal static void ValidateBrokerCompletion(OAuthBrokerCompletion completion,
        string environment, string tenantReference, string principalReference, string actorReference,
        string expectedOrigin, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(completion);
        if (completion.Environment != environment
            || completion.IssuedAtUtc > now + TimeSpan.FromMinutes(1)
            || completion.ExpiresAtUtc <= now
            || completion.ExpiresAtUtc - completion.IssuedAtUtc > OAuthBrokerContract.MaximumCompletionLifetime
            || completion.TenantReference != tenantReference
            || completion.PrincipalReference != principalReference
            || completion.ActorReference != actorReference
            || completion.ReturnOrigin != expectedOrigin)
            throw new OAuthCallbackValidationException("The broker completion ticket does not match this session.");
    }

    public async Task DisconnectAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var oauth = _oauth ?? throw new InvalidOperationException("The VAT host is unavailable.");
        var context = await ContextAsync(principal, cancellationToken);
        await oauth.RevokeAsync(context, HmrcOAuthScopes.ReadVat, cancellationToken);
        await oauth.RevokeAsync(context, HmrcOAuthScopes.WriteVat, cancellationToken);
    }

    public async Task<VatFraudHeaderValidationResult> ValidateAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var oauth = _oauth ?? throw new InvalidOperationException("The HMRC OAuth service is unavailable.");
        var fraud = _fraud ?? throw new InvalidOperationException("Fraud-header capture is unavailable.");
        var validator = _validator ?? throw new InvalidOperationException("The HMRC validator is unavailable.");
        var identity = await _identities.GetRequiredAsync(principal, cancellationToken);
        var oauthContext = new AuthorityDispatchContext(identity.TenantReference,
            identity.AspNetSubjectReference, identity.ActorReference,
            "hmrc-fraud-header-validation", "client-facts-not-required");
        using var access = await oauth.GetAccessAsync(oauthContext, HmrcOAuthScopes.ReadVat, cancellationToken);
        if (access.Kind != OAuthAccessOutcomeKind.Available || access.AccessToken is null)
            return new(null, null, null, access.ReauthorisationReason);
        var sealedFacts = _fraudReferences.GetCurrent(identity, DateTimeOffset.UtcNow);
        if (sealedFacts is null)
            throw new FraudContextRejectedException("Fresh browser and session facts are required.");
        var validationContext = new AuthorityDispatchContext(identity.TenantReference,
            identity.AspNetSubjectReference, identity.ActorReference,
            "hmrc-fraud-header-validation", sealedFacts);
        var headers = await fraud.BuildHeadersAsync(validationContext, cancellationToken);
        var result = await validator.ValidateAsync(headers, access.AccessToken, cancellationToken);
        return new(result.StatusCode, result.ContentType, result.Body);
    }

    public async Task<IReadOnlyList<VatAuthorityObligation>> RetrieveAsync(VatWorkflowIdentity identity,
        string vrn, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (_oauth is null)
            throw new VatAuthorityRequestException("VAT-HOST-DISABLED");
        if (_gateway is null || _content is null)
            throw new VatAuthorityRequestException("VAT-AUTHORITY-TOPOLOGY-UNAVAILABLE");
        var sealedFacts = _fraudReferences.GetCurrent(identity, DateTimeOffset.UtcNow);
        if (sealedFacts is null)
            throw new VatAuthorityRequestException("FRAUD-CONTEXT-REQUIRED");

        var request = new BodylessRequestDescriber(new PreparedApiRequestPipeline())
            .Describe(new DescribeVatObligations(vrn, from, to));
        var context = new AuthorityDispatchContext(identity.TenantReference,
            identity.AspNetSubjectReference, identity.ActorReference, "vat-obligations", sealedFacts);
        var outcome = await _gateway.SendAsync(request, context, cancellationToken);
        if (outcome.Kind != PreparedApiOutcomeKind.Succeeded || outcome.SafeResponseReference is null)
            throw new VatAuthorityRequestException(outcome.OutcomeCode, outcome.AttemptReference);
        var bytes = await _content.ReadAsync(identity.TenantReference, identity.AspNetSubjectReference,
            outcome.SafeResponseReference, cancellationToken)
            ?? throw new VatAuthorityRequestException("HMRC-RESPONSE-EVIDENCE-MISSING", outcome.AttemptReference);
        VatObligationsResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<VatObligationsResponse>(bytes,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            throw new VatAuthorityRequestException("HMRC-RESPONSE-MALFORMED", outcome.AttemptReference);
        }
        if (response is null)
            throw new VatAuthorityRequestException("HMRC-RESPONSE-MALFORMED", outcome.AttemptReference);
        if (response.Obligations.Any(item => item.Status is not "O" and not "F"))
            throw new VatAuthorityRequestException("HMRC-RESPONSE-MALFORMED", outcome.AttemptReference);
        return response.Obligations.Select(item => new VatAuthorityObligation(item.PeriodKey,
                DateOnly.FromDateTime(item.Start), DateOnly.FromDateTime(item.End),
                DateOnly.FromDateTime(item.Due), item.Status,
                item.Received.HasValue ? DateOnly.FromDateTime(item.Received.Value) : null))
            .OrderBy(item => item.Status == "O" ? 0 : 1)
            .ThenByDescending(item => item.End)
            .ToArray();
    }

    public async Task<bool> ExistsAsync(VatWorkflowIdentity identity, string vrn, string periodKey,
        CancellationToken cancellationToken = default)
    {
        if (_gateway is null)
            throw new VatAuthorityRequestException("VAT-AUTHORITY-TOPOLOGY-UNAVAILABLE");
        var sealedFacts = _fraudReferences.GetCurrent(identity, DateTimeOffset.UtcNow);
        if (sealedFacts is null)
            throw new VatAuthorityRequestException("FRAUD-CONTEXT-REQUIRED");
        var request = new BodylessRequestDescriber(new PreparedApiRequestPipeline())
            .Describe(new DescribeVatReturn(vrn, periodKey));
        var context = new AuthorityDispatchContext(identity.TenantReference,
            identity.AspNetSubjectReference, identity.ActorReference, "vat-return-preflight", sealedFacts);
        var outcome = await _gateway.SendAsync(request, context, cancellationToken);
        if (outcome.Kind == PreparedApiOutcomeKind.Succeeded) return true;
        if (outcome.ActualStatusCode == 404) return false;
        throw new VatAuthorityRequestException(outcome.OutcomeCode, outcome.AttemptReference);
    }

    public async Task<VatAuthoritySubmissionResult> DispatchApprovedAsync(ApprovedVatReturnDispatch approved,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approved);
        if (!_submissionGate.IsEnabled)
            return new(new(PreparedApiOutcomeKind.Failed, "VAT-SUBMISSION-LOCKED"),
                null, null, false);
        if (_gateway is null || _content is null)
            return new(new(PreparedApiOutcomeKind.Failed, "VAT-AUTHORITY-TOPOLOGY-UNAVAILABLE"),
                null, null, false);
        var sealedFacts = _fraudReferences.GetCurrent(approved.Identity, DateTimeOffset.UtcNow);
        if (sealedFacts is null)
            return new(new(PreparedApiOutcomeKind.Failed, "FRAUD-CONTEXT-REQUIRED"),
                null, null, false);
        var context = new AuthorityDispatchContext(approved.Identity.TenantReference,
            approved.Identity.AspNetSubjectReference, approved.Identity.ActorReference,
            approved.ApprovalReference, sealedFacts, approved.LogicalSubmissionReference,
            approved.SubjectPeriodReference);
        PreparedApiOutcome outcome;
        try { outcome = await _gateway.SendAsync(approved.Request, context, cancellationToken); }
        catch (ActiveSubmissionAttemptException exception)
        {
            return new(new(PreparedApiOutcomeKind.Unknown, "ACTIVE-SUBMISSION-ATTEMPT",
                AttemptReference: exception.AttemptReference), null, null, false);
        }
        if (outcome.Kind != PreparedApiOutcomeKind.Succeeded || outcome.SafeResponseReference is null)
        {
            var errors = outcome.SafeResponseReference is null
                ? []
                : ParseAuthorityErrors(await _content.ReadAsync(approved.Identity.TenantReference,
                    approved.Identity.AspNetSubjectReference, outcome.SafeResponseReference, cancellationToken));
            return new(outcome, null, null, false, Errors: errors);
        }

        var responseBytes = await _content.ReadAsync(approved.Identity.TenantReference,
            approved.Identity.AspNetSubjectReference, outcome.SafeResponseReference, cancellationToken);
        VatReturnResponse? receipt = null;
        if (responseBytes is not null)
        {
            try
            {
                receipt = JsonSerializer.Deserialize<VatReturnResponse>(responseBytes,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException) { }
        }

        VatReturnReconciliationResult? reconciliation = null;
        string? readbackCode = null;
        try
        {
            var readbackRequest = new BodylessRequestDescriber(new PreparedApiRequestPipeline())
                .Describe(new DescribeVatReturn(approved.Vrn, approved.PeriodKey));
            var readbackContext = new AuthorityDispatchContext(approved.Identity.TenantReference,
                approved.Identity.AspNetSubjectReference, approved.Identity.ActorReference,
                approved.ApprovalReference, sealedFacts);
            var readback = await _gateway.SendAsync(readbackRequest, readbackContext, cancellationToken);
            readbackCode = readback.OutcomeCode;
            if (readback.Kind == PreparedApiOutcomeKind.Succeeded && readback.SafeResponseReference is not null)
            {
                var readbackBytes = await _content.ReadAsync(approved.Identity.TenantReference,
                    approved.Identity.AspNetSubjectReference, readback.SafeResponseReference, cancellationToken);
                if (readbackBytes is not null)
                {
                    reconciliation = VatReturnReconciliation.Compare(approved.Request, readbackBytes);
                    if (_attempts is not null && outcome.AttemptReference is not null)
                        await _attempts.RecordReconciliationAsync(approved.Identity.TenantReference,
                            approved.Identity.AspNetSubjectReference, outcome.AttemptReference,
                            new(reconciliation.Matches, DateTimeOffset.UtcNow, readback.OutcomeCode,
                                reconciliation.Differences.Select(item => new SubmissionReconciliationDifference(
                                    item.Field, item.PreparedValue, item.AuthorityValue)).ToArray()),
                            cancellationToken);
                }
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            readbackCode = "VAT-READBACK-UNAVAILABLE";
        }

        var obligationRefreshed = false;
        try
        {
            var obligations = await RetrieveAsync(approved.Identity, approved.Vrn,
                approved.PeriodStart, approved.PeriodEnd, cancellationToken);
            obligationRefreshed = obligations.Any(item => item.PeriodKey == approved.PeriodKey
                && item.Status.Equals("F", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception exception) when (exception is not OperationCanceledException) { }
        return new(outcome, receipt, reconciliation, obligationRefreshed, readbackCode);
    }

    public async Task<SubmissionReconciliationEvidence> ReconcileApprovedAsync(
        ApprovedVatReturnDispatch approved, string attemptReference,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approved);
        if (_gateway is null || _content is null || _attempts is null)
            throw new VatAuthorityRequestException("VAT-AUTHORITY-TOPOLOGY-UNAVAILABLE", attemptReference);
        var attempt = (await _attempts.ListTenantAsync(approved.Identity.TenantReference, 500,
                cancellationToken)).SingleOrDefault(item => item.AttemptReference == attemptReference)
            ?? throw new VatAuthorityRequestException("VAT-ATTEMPT-NOT-AVAILABLE", attemptReference);
        if (attempt.OperationId != "vat.returns.submit" || attempt.ApprovalReference != approved.ApprovalReference
            || attempt.State is not (SubmissionAttemptState.Succeeded or SubmissionAttemptState.Unknown))
            throw new VatAuthorityRequestException("VAT-ATTEMPT-NOT-RECONCILABLE", attemptReference);
        if (attempt.Reconciliation is not null) return attempt.Reconciliation;
        var sealedFacts = _fraudReferences.GetCurrent(approved.Identity, DateTimeOffset.UtcNow);
        if (sealedFacts is null)
            throw new VatAuthorityRequestException("FRAUD-CONTEXT-REQUIRED", attemptReference);
        var request = new BodylessRequestDescriber(new PreparedApiRequestPipeline())
            .Describe(new DescribeVatReturn(approved.Vrn, approved.PeriodKey));
        var context = new AuthorityDispatchContext(approved.Identity.TenantReference,
            approved.Identity.AspNetSubjectReference, approved.Identity.ActorReference,
            approved.ApprovalReference, sealedFacts);
        var readback = await _gateway.SendAsync(request, context, cancellationToken);
        if (readback.Kind != PreparedApiOutcomeKind.Succeeded || readback.SafeResponseReference is null)
            throw new VatAuthorityRequestException(readback.OutcomeCode,
                readback.AttemptReference ?? attemptReference);
        var bytes = await _content.ReadAsync(approved.Identity.TenantReference,
            approved.Identity.AspNetSubjectReference, readback.SafeResponseReference, cancellationToken)
            ?? throw new VatAuthorityRequestException("HMRC-RESPONSE-EVIDENCE-MISSING", attemptReference);
        var compared = VatReturnReconciliation.Compare(approved.Request, bytes);
        var evidence = new SubmissionReconciliationEvidence(compared.Matches, DateTimeOffset.UtcNow,
            readback.OutcomeCode, compared.Differences.Select(item => new SubmissionReconciliationDifference(
                item.Field, item.PreparedValue, item.AuthorityValue)).ToArray());
        await _attempts.RecordReconciliationAsync(approved.Identity.TenantReference,
            attempt.PrincipalReference, attemptReference, evidence, cancellationToken);
        if (evidence.Matches && attempt.State is SubmissionAttemptState.Unknown or SubmissionAttemptState.Sending)
            await _attempts.RecordOutcomeAsync(approved.Identity.TenantReference,
                attempt.PrincipalReference, attemptReference,
                new(SubmissionAttemptState.Succeeded, "HMRC-SUCCESS-RECONCILED",
                    CorrelationReference: attempt.CorrelationReference,
                    SafeResponseReference: attempt.SafeResponseReference), cancellationToken);
        return evidence;
    }

    private static IReadOnlyList<VatAuthorityError> ParseAuthorityErrors(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0) return [];
        try
        {
            using var document = JsonDocument.Parse(bytes);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return [];
            var root = document.RootElement;
            var errors = new List<VatAuthorityError>();
            Add(root, errors);
            if (root.TryGetProperty("errors", out var items) && items.ValueKind == JsonValueKind.Array)
                foreach (var item in items.EnumerateArray())
                    if (item.ValueKind == JsonValueKind.Object) Add(item, errors);
            return errors.Take(12).ToArray();
        }
        catch (JsonException) { return []; }

        static void Add(JsonElement item, ICollection<VatAuthorityError> target)
        {
            var code = Text(item, "code", 96);
            var message = Text(item, "message", 512);
            if (code is not null && message is not null
                && !target.Any(error => error.Code == code && error.Message == message))
                target.Add(new(code, message));
        }

        static string? Text(JsonElement item, string name, int maximumLength)
        {
            if (!item.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
                return null;
            var value = property.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(value)) return null;
            var safe = new string(value.Where(character => !char.IsControl(character)).ToArray());
            return safe.Length <= maximumLength ? safe : safe[..maximumLength];
        }
    }

    private async Task<AuthorityDispatchContext> ContextAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var identity = await _identities.GetRequiredAsync(principal, cancellationToken);
        return new AuthorityDispatchContext(identity.TenantReference, identity.AspNetSubjectReference,
            identity.ActorReference, "hmrc-connection", "client-facts-not-required");
    }

    private static VatConnectionStatus Map(OAuthReauthorisationReason? reason) => reason switch
    {
        OAuthReauthorisationReason.MissingGrant => new(VatConnectionState.NotConnected),
        OAuthReauthorisationReason.RevokedGrant => new(VatConnectionState.Disconnected),
        _ => new(VatConnectionState.ReauthorisationRequired, reason?.ToString())
    };

    private string AuthorityEnvironmentName() =>
        _host.AuthorityEnvironment == VatAuthorityEnvironment.Sandbox ? "sandbox" : "production";

    private static byte[] LoadOrCreateKey(string path)
    {
        if (File.Exists(path))
        {
            var existing = File.ReadAllBytes(path);
            if (existing.Length != 32) throw new InvalidOperationException("The OAuth store key is invalid.");
            return existing;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var key = RandomNumberGenerator.GetBytes(32);
        try
        {
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            stream.Write(key);
            stream.Flush(true);
            return key;
        }
        catch (IOException) when (File.Exists(path))
        {
            CryptographicOperations.ZeroMemory(key);
            var existing = File.ReadAllBytes(path);
            if (existing.Length != 32) throw new InvalidOperationException("The OAuth store key is invalid.");
            return existing;
        }
    }

    private static FraudDeploymentTopology CreateTopology(VatProductHostOptions host)
    {
        var trustedPeers = host.FraudTrustedProxyAddresses.Select(IPAddress.Parse).ToHashSet();
        var publicHops = host.FraudPublicTlsAddresses.Select(IPAddress.Parse).ToArray();
        return trustedPeers.Count == 0
            ? FraudDeploymentTopology.Direct("tcweb-direct", publicHops.Single())
            : FraudDeploymentTopology.TrustedProxyChain("tcweb-trusted-proxy", trustedPeers, publicHops);
    }

    private static FraudVendorConfiguration CreateVendor() => new("Trade Control",
        new Dictionary<string, string>
        {
            ["tcweb-client"] = "2.0.2",
            ["tcweb-server"] = "2.0.2"
        },
        new Dictionary<string, string>());

    public void Dispose()
    {
        _gateway?.Dispose();
        _validator?.Dispose();
        _fraud?.Dispose();
        _oauth?.Dispose();
        _tokens?.Dispose();
        _brokerTickets?.Dispose();
    }
}
