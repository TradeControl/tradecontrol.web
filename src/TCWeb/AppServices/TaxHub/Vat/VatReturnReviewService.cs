using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradeControl.Tax.Data;
using TradeControl.Tax.UK.Adapters.Submission.Audit;
using TradeControl.Tax.UK.Adapters.TradeControl.Data;
using TradeControl.Tax.UK.Application.Preparation;
using TradeControl.Tax.UK.Hmrc.Vat.v1_0.Returns;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

public sealed record VatReturnReviewBox(int Number, string Description, string Value);
public sealed record VatReturnReviewFinding(string Severity, string Code, string Message);
public sealed record VatReturnReviewModel(
    string PreparationReference,
    string PeriodKey,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string MaskedVrn,
    IReadOnlyList<VatReturnReviewBox> Boxes,
    IReadOnlyList<VatReturnReviewFinding> Findings,
    string PreparedSha256,
    string SourceSystem,
    string DatasetKey,
    string SnapshotToken,
    DateTimeOffset ExpiresAtUtc,
    string DeclarationVersion,
    string DeclarationText,
    string DeclarationSha256,
    string? ApprovalReference = null);

public interface IVatReturnReviewService
{
    Task<VatReturnReviewModel> PrepareAsync(string periodKey, CancellationToken cancellationToken = default);
    Task<VatReturnReviewModel> ApproveAsync(string preparationReference, string declarationVersion,
        bool warningsAcknowledged,
        CancellationToken cancellationToken = default);
}

public sealed record ApprovedVatReturnDispatch(
    PreparedApiRequest Request,
    VatWorkflowIdentity Identity,
    string ApprovalReference,
    string LogicalSubmissionReference,
    string SubjectPeriodReference,
    string Vrn,
    string PeriodKey,
    DateOnly PeriodStart,
    DateOnly PeriodEnd);

public interface IVatApprovedReturnResolver
{
    Task<ApprovedVatReturnDispatch> ResolveAsync(string approvalReference,
        CancellationToken cancellationToken = default);
}

public sealed class VatReturnReviewException : Exception
{
    public VatReturnReviewException(string safeMessage,
        IReadOnlyList<VatReturnReviewFinding>? findings = null) : base(safeMessage) => Findings = findings ?? [];
    public IReadOnlyList<VatReturnReviewFinding> Findings { get; }
}

public sealed class VatReturnReviewService : IVatReturnReviewService, IVatApprovedReturnResolver
{
    private const string DeclarationVersion = "hmrc-vat-business-2026-09-30";
    private const string DeclarationResource =
        "TradeControl.Web.AppServices.TaxHub.Vat.Declarations.hmrc-vat-business-2026-09-30.txt";
    private static readonly TimeSpan CandidateLifetime = TimeSpan.FromHours(24);
    private static readonly SemaphoreSlim StoreGate = new(1, 1);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly NodeContext _nodeContext;
    private readonly IVatWorkflowIdentityAccessor _identities;
    private readonly IVatObligationWorkspaceService _obligations;
    private readonly IVatFilingAuthorisationPolicy _filingPolicy;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VatReturnReviewService> _logger;
    private readonly VatProductHostOptions _host;
    private readonly TimeProvider _time;
    private readonly FileSubmissionContentStore _content;
    private readonly string _candidatePath;
    private readonly string _approvalPath;
    private readonly string _declarationText;
    private readonly string _declarationSha256;

    public VatReturnReviewService(NodeContext nodeContext, IVatWorkflowIdentityAccessor identities,
        IVatObligationWorkspaceService obligations, IOptions<VatProductHostOptions> hostOptions,
        TimeProvider timeProvider, IVatFilingAuthorisationPolicy filingPolicy,
        IHttpContextAccessor httpContextAccessor, ILogger<VatReturnReviewService> logger)
    {
        _nodeContext = nodeContext;
        _identities = identities;
        _obligations = obligations;
        _filingPolicy = filingPolicy;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _host = hostOptions.Value;
        _time = timeProvider;
        if (!_host.Enabled || _host.PersistenceMode != VatPersistenceMode.DevelopmentFiles
            || string.IsNullOrWhiteSpace(_host.DevelopmentStoreRoot))
            throw new InvalidOperationException("The VAT return review store is unavailable.");
        var root = Path.GetFullPath(_host.DevelopmentStoreRoot);
        Directory.CreateDirectory(root);
        _candidatePath = Path.Combine(root, "vat-review-candidates.json");
        _approvalPath = Path.Combine(root, "vat-approvals.json");
        _content = new(new(Path.Combine(root, "prepared-content"), Retention: CandidateLifetime));
        _declarationText = ReadDeclaration();
        _declarationSha256 = Sha256(Encoding.UTF8.GetBytes(_declarationText));
    }

    public async Task<VatReturnReviewModel> PrepareAsync(string periodKey,
        CancellationToken cancellationToken = default)
    {
        try { return await PrepareCoreAsync(periodKey, cancellationToken); }
        catch (VatReturnReviewException) { throw; }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            var reference = await RecordUnexpectedFailureAsync(exception, "preparation", cancellationToken);
            throw new VatReturnReviewException(
                $"The VAT return could not be prepared safely. Support reference: {reference ?? "unavailable"}.");
        }
    }

    private async Task<VatReturnReviewModel> PrepareCoreAsync(string periodKey,
        CancellationToken cancellationToken)
    {
        var identity = await _identities.GetRequiredAsync(cancellationToken);
        var prepared = await PrepareCurrentAsync(identity, periodKey, cancellationToken);
        if (prepared.Request.HasErrors || prepared.Request.BodyBytes is not { } body
            || string.IsNullOrWhiteSpace(prepared.Request.BodySha256))
            throw new VatReturnReviewException(
                "The VAT return is not ready for review. Resolve its blocking findings first.",
                prepared.Request.Findings.Select(FindingRecord.From).Select(item => item.ToView()).ToArray());

        var reference = Guid.NewGuid().ToString("N");
        var now = _time.GetUtcNow();
        var contentReference = await _content.StoreAsync(identity.TenantReference,
            identity.AspNetSubjectReference, SubmissionContentKind.Payload, body.ToArray(), cancellationToken);
        var source = prepared.Request.SourceEvidence.Single();
        var candidate = new CandidateRecord(reference, identity.TenantReference,
            identity.AspNetSubjectReference, identity.ActorReference, prepared.Obligation.PeriodKey,
            prepared.Obligation.Start, prepared.Obligation.End, prepared.MaskedVrn,
            prepared.VrnSha256, prepared.Request.BodySha256!, contentReference,
            source.SourceSystem, source.DatasetKey, source.SnapshotToken,
            prepared.Request.Findings.Select(FindingRecord.From).ToArray(), now, now.Add(CandidateLifetime));
        await AddCandidateAsync(candidate, cancellationToken);
        return await ReadModelAsync(candidate, null, cancellationToken);
    }

    public async Task<VatReturnReviewModel> ApproveAsync(string preparationReference,
        string declarationVersion, bool warningsAcknowledged,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ApproveCoreAsync(preparationReference, declarationVersion,
                warningsAcknowledged, cancellationToken);
        }
        catch (VatReturnReviewException) { throw; }
        catch (UnauthorizedAccessException) { throw; }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            var reference = await RecordUnexpectedFailureAsync(exception, "approval", cancellationToken);
            throw new VatReturnReviewException(
                $"The VAT return approval could not be recorded safely. Support reference: {reference ?? "unavailable"}.");
        }
    }

    private async Task<VatReturnReviewModel> ApproveCoreAsync(string preparationReference,
        string declarationVersion, bool warningsAcknowledged, CancellationToken cancellationToken)
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("An authenticated Trade Control session is required.");
        if (!_filingPolicy.CanManageHmrcConnection(principal))
            throw new UnauthorizedAccessException("VAT return approval requires Administrator or Manager permission.");
        if (!string.Equals(declarationVersion, DeclarationVersion, StringComparison.Ordinal))
            throw new VatReturnReviewException("The declaration version changed. Review the return again.");
        var identity = await _identities.GetRequiredAsync(cancellationToken);
        var candidate = await GetCandidateAsync(preparationReference, identity, cancellationToken)
            ?? throw new VatReturnReviewException("The VAT return review has expired or is unavailable.");
        if (candidate.ExpiresAtUtc <= _time.GetUtcNow())
            throw new VatReturnReviewException("The VAT return review has expired. Prepare it again.");
        if (candidate.Findings.Any(item => item.Severity.Equals("Warning", StringComparison.OrdinalIgnoreCase))
            && !warningsAcknowledged)
            throw new VatReturnReviewException("Review and acknowledge the preparation warnings before approval.");

        var current = await PrepareCurrentAsync(identity, candidate.PeriodKey, cancellationToken);
        var source = current.Request.SourceEvidence.SingleOrDefault();
        if (current.Request.HasErrors || current.Request.BodySha256 != candidate.PreparedSha256
            || current.VrnSha256 != candidate.VrnSha256
            || source is null || source.DatasetKey != candidate.DatasetKey
            || source.SnapshotToken != candidate.SnapshotToken
            || current.Obligation.Start != candidate.PeriodStart || current.Obligation.End != candidate.PeriodEnd)
            throw new VatReturnReviewException(
                "The VAT source or obligation changed after review. Prepare and review a new return.");

        var approval = await AddApprovalAsync(candidate, identity, warningsAcknowledged, cancellationToken);
        return await ReadModelAsync(candidate, approval.Reference, cancellationToken);
    }

    public async Task<ApprovedVatReturnDispatch> ResolveAsync(string approvalReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(approvalReference) || approvalReference.Length > 64)
            throw new VatReturnReviewException("The VAT approval reference is invalid.");
        var identity = await _identities.GetRequiredAsync(cancellationToken);
        var approval = await GetApprovalAsync(approvalReference, identity, cancellationToken)
            ?? throw new VatReturnReviewException("The VAT approval is unavailable for this user and tenant.");
        var candidate = await GetCandidateAsync(approval.PreparationReference, identity, cancellationToken)
            ?? throw new VatReturnReviewException("The approved VAT preparation is unavailable.");
        if (candidate.ExpiresAtUtc <= _time.GetUtcNow())
            throw new VatReturnReviewException("The approved VAT preparation has expired. Review it again.");
        if (approval.PreparedSha256 != candidate.PreparedSha256
            || approval.DeclarationVersion != DeclarationVersion
            || approval.DeclarationSha256 != _declarationSha256)
            throw new VatReturnReviewException("The retained VAT approval no longer matches its reviewed evidence.");

        var current = await PrepareCurrentAsync(identity, candidate.PeriodKey, cancellationToken);
        EnsureCurrent(candidate, current);
        var bytes = await _content.ReadAsync(candidate.TenantReference, candidate.PrincipalReference,
            candidate.ContentReference, cancellationToken)
            ?? throw new VatReturnReviewException("The exact approved VAT body is unavailable.");
        if (!Sha256(bytes).Equals(candidate.PreparedSha256, StringComparison.Ordinal))
            throw new VatReturnReviewException("The exact approved VAT body failed digest verification.");
        var request = current.Request.WithVerifiedBody(bytes);
        var vrn = ExtractVrn(request.RelativePath);
        return new(request, identity, approval.Reference, approval.LogicalSubmissionIdentity,
            approval.LogicalSubmissionIdentity, vrn, candidate.PeriodKey,
            candidate.PeriodStart, candidate.PeriodEnd);
    }

    private async Task<PreparedCurrent> PrepareCurrentAsync(VatWorkflowIdentity identity, string periodKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(periodKey) || periodKey.Length > 16)
            throw new VatReturnReviewException("The selected HMRC obligation is invalid.");
        var workspace = await _obligations.GetAsync(cancellationToken);
        if (workspace.State != VatObligationWorkspaceState.Ready)
            throw new VatReturnReviewException(workspace.SafeMessage ?? "HMRC obligations are unavailable.");
        var obligation = workspace.Rows.SingleOrDefault(item => item.CanReview
            && string.Equals(item.PeriodKey, periodKey, StringComparison.Ordinal))
            ?? throw new VatReturnReviewException("The selected HMRC obligation is no longer open and matched.");

        var connectionString = _nodeContext.Database.GetConnectionString()
            ?? throw new InvalidOperationException("The Trade Control source is unavailable.");
        var sourceKey = new SourceKey("tcweb-node");
        var connections = new ConnectionFactory();
        var resolver = new SourceConnectionResolver([new(sourceKey, connectionString)]);
        var sourceAdapter = new TradeControlTaxSourceAdapter(connections, resolver);
        var contextAdapter = new TradeControlStatutoryContextAdapter(connections, resolver);
        var preparer = new VatReturnPreparer(sourceAdapter, sourceAdapter, contextAdapter,
            new PreparedApiRequestPipeline());
        var period = new TaxReportingPeriod(obligation.Start, obligation.End, TaxPeriodKind.Vat,
            $"VAT-{obligation.End:yyyy-MM-dd}");
        var request = await preparer.PrepareAsync(new(sourceKey, period, obligation.PeriodKey!, true),
            cancellationToken);
        var vrn = ExtractVrn(request.RelativePath);
        if (vrn.Length != 9 || vrn.Any(character => !char.IsDigit(character)))
            throw new VatReturnReviewException("The prepared VAT registration identity is invalid.");
        return new(request, obligation, $"*****{vrn[^4..]}", Sha256(Encoding.UTF8.GetBytes(vrn)));
    }

    private static string ExtractVrn(string relativePath)
    {
        var vrn = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .SkipWhile(segment => !segment.Equals("vat", StringComparison.OrdinalIgnoreCase))
            .Skip(1).FirstOrDefault() ?? string.Empty;
        if (vrn.Length != 9 || vrn.Any(character => !char.IsDigit(character)))
            throw new VatReturnReviewException("The prepared VAT registration identity is invalid.");
        return vrn;
    }

    private static void EnsureCurrent(CandidateRecord candidate, PreparedCurrent current)
    {
        var source = current.Request.SourceEvidence.SingleOrDefault();
        if (current.Request.HasErrors || current.Request.BodySha256 != candidate.PreparedSha256
            || current.VrnSha256 != candidate.VrnSha256
            || source is null || source.DatasetKey != candidate.DatasetKey
            || source.SnapshotToken != candidate.SnapshotToken
            || current.Obligation.Start != candidate.PeriodStart || current.Obligation.End != candidate.PeriodEnd)
            throw new VatReturnReviewException(
                "The VAT source or obligation changed after approval. Prepare and review a new return.");
    }

    private async Task<VatReturnReviewModel> ReadModelAsync(CandidateRecord candidate,
        string? approvalReference, CancellationToken cancellationToken)
    {
        var bytes = await _content.ReadAsync(candidate.TenantReference, candidate.PrincipalReference,
            candidate.ContentReference, cancellationToken)
            ?? throw new VatReturnReviewException("The exact prepared VAT return is unavailable.");
        if (!Sha256(bytes).Equals(candidate.PreparedSha256, StringComparison.Ordinal))
            throw new InvalidDataException("The exact prepared VAT return failed digest verification.");
        var body = JsonSerializer.Deserialize<VatReturnRequest>(bytes,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("The exact prepared VAT return is invalid.");
        return new(candidate.Reference, candidate.PeriodKey, candidate.PeriodStart, candidate.PeriodEnd,
            candidate.MaskedVrn, ProjectBoxes(body), candidate.Findings.Select(item => item.ToView()).ToArray(),
            candidate.PreparedSha256, candidate.SourceSystem, candidate.DatasetKey, candidate.SnapshotToken,
            candidate.ExpiresAtUtc, DeclarationVersion, _declarationText, _declarationSha256,
            approvalReference);
    }

    internal static IReadOnlyList<VatReturnReviewBox> ProjectBoxes(VatReturnRequest body) =>
    [
        new(1, "VAT due on sales and other outputs", Money(body.VatDueSales)),
        new(2, "VAT due on acquisitions from other EC Member States", Money(body.VatDueAcquisitions)),
        new(3, "Total VAT due", Money(body.TotalVatDue)),
        new(4, "VAT reclaimed in this period", Money(body.VatReclaimedCurrPeriod)),
        new(5, "Net VAT to pay or reclaim", Money(body.NetVatDue)),
        new(6, "Total value of sales excluding VAT", Whole(body.TotalValueSalesExVat)),
        new(7, "Total value of purchases excluding VAT", Whole(body.TotalValuePurchasesExVat)),
        new(8, "Total value of goods supplied to other EC Member States", Whole(body.TotalValueGoodsSuppliedExVat)),
        new(9, "Total acquisitions from other EC Member States", Whole(body.TotalAcquisitionsExVat))
    ];

    private async Task AddCandidateAsync(CandidateRecord candidate, CancellationToken cancellationToken)
    {
        await StoreGate.WaitAsync(cancellationToken);
        try
        {
            var records = await ReadAsync<CandidateRecord>(_candidatePath, cancellationToken);
            records.RemoveAll(item => item.ExpiresAtUtc <= _time.GetUtcNow());
            records.Add(candidate);
            await WriteAsync(_candidatePath, records, cancellationToken);
        }
        finally { StoreGate.Release(); }
    }

    private async Task<CandidateRecord?> GetCandidateAsync(string reference, VatWorkflowIdentity identity,
        CancellationToken cancellationToken)
    {
        await StoreGate.WaitAsync(cancellationToken);
        try
        {
            return (await ReadAsync<CandidateRecord>(_candidatePath, cancellationToken)).SingleOrDefault(item =>
                item.Reference == reference && item.TenantReference == identity.TenantReference
                && item.PrincipalReference == identity.AspNetSubjectReference
                && item.ActorReference == identity.ActorReference);
        }
        finally { StoreGate.Release(); }
    }

    private async Task<ApprovalRecord?> GetApprovalAsync(string reference, VatWorkflowIdentity identity,
        CancellationToken cancellationToken)
    {
        await StoreGate.WaitAsync(cancellationToken);
        try
        {
            return (await ReadAsync<ApprovalRecord>(_approvalPath, cancellationToken)).SingleOrDefault(item =>
                item.Reference == reference && item.TenantReference == identity.TenantReference
                && item.PrincipalReference == identity.AspNetSubjectReference
                && item.ActorReference == identity.ActorReference);
        }
        finally { StoreGate.Release(); }
    }

    private async Task<ApprovalRecord> AddApprovalAsync(CandidateRecord candidate,
        VatWorkflowIdentity identity, bool warningsAcknowledged, CancellationToken cancellationToken)
    {
        await StoreGate.WaitAsync(cancellationToken);
        try
        {
            var approvals = await ReadAsync<ApprovalRecord>(_approvalPath, cancellationToken);
            var existing = approvals.SingleOrDefault(item => item.PreparationReference == candidate.Reference
                && item.TenantReference == identity.TenantReference
                && item.PrincipalReference == identity.AspNetSubjectReference);
            if (existing is not null) return existing;
            var approval = new ApprovalRecord(Guid.NewGuid().ToString("N"), candidate.Reference,
                identity.TenantReference, identity.AspNetSubjectReference, identity.ActorReference,
                Sha256(Encoding.UTF8.GetBytes($"{candidate.VrnSha256}:{candidate.PeriodKey}")),
                candidate.PeriodKey, candidate.PeriodStart, candidate.PeriodEnd, DeclarationVersion,
                _declarationSha256, candidate.PreparedSha256, candidate.SourceSystem,
                candidate.DatasetKey, candidate.SnapshotToken, warningsAcknowledged, _time.GetUtcNow());
            approvals.Add(approval);
            await WriteAsync(_approvalPath, approvals, cancellationToken);
            return approval;
        }
        finally { StoreGate.Release(); }
    }

    private static async Task<List<T>> ReadAsync<T>(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return [];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return await JsonSerializer.DeserializeAsync<List<T>>(stream, Json, cancellationToken) ?? [];
    }

    private static async Task WriteAsync<T>(string path, List<T> records,
        CancellationToken cancellationToken)
    {
        var temporary = path + ".tmp";
        await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None,
            4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
        {
            await JsonSerializer.SerializeAsync(stream, records, Json, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            stream.Flush(true);
        }
        File.Move(temporary, path, true);
    }

    private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
    private static string Whole(decimal value) => value.ToString("0", CultureInfo.InvariantCulture);
    private static string Sha256(ReadOnlySpan<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    private static string ReadDeclaration()
    {
        using var stream = typeof(VatReturnReviewService).Assembly.GetManifestResourceStream(DeclarationResource)
            ?? throw new InvalidOperationException("The versioned HMRC VAT declaration resource is missing.");
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        return reader.ReadToEnd().Trim();
    }

    private async Task<string?> RecordUnexpectedFailureAsync(Exception exception, string operation,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unexpected failure during VAT return {Operation}.", operation);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _nodeContext.ErrorLog(exception);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception logException)
        {
            _logger.LogError(logException,
                "Failed to write a VAT return {Operation} failure to the node Event Log.", operation);
            return null;
        }
    }

    private sealed record PreparedCurrent(PreparedApiRequest Request, VatObligationWorkspaceRow Obligation,
        string MaskedVrn, string VrnSha256);
    private sealed record CandidateRecord(string Reference, string TenantReference, string PrincipalReference,
        string ActorReference, string PeriodKey, DateOnly PeriodStart, DateOnly PeriodEnd, string MaskedVrn,
        string VrnSha256, string PreparedSha256, string ContentReference, string SourceSystem,
        string DatasetKey, string SnapshotToken, IReadOnlyList<FindingRecord> Findings,
        DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc);
    private sealed record ApprovalRecord(string Reference, string PreparationReference, string TenantReference,
        string PrincipalReference, string ActorReference, string LogicalSubmissionIdentity,
        string PeriodKey, DateOnly PeriodStart, DateOnly PeriodEnd, string DeclarationVersion,
        string DeclarationSha256, string PreparedSha256, string SourceSystem, string DatasetKey,
        string SnapshotToken, bool WarningsAcknowledged, DateTimeOffset ApprovedAtUtc);
    private sealed record FindingRecord(string Severity, string Code, string Message)
    {
        public static FindingRecord From(PreparedArtifactFinding finding) =>
            new(finding.Severity.ToString(), finding.Code, SafeMessage(finding));
        public VatReturnReviewFinding ToView() => new(Severity, Code, Message);
        private static string SafeMessage(PreparedArtifactFinding finding) => finding.Code switch
        {
            "VAT-CONTEXT-UNAVAILABLE" => "The effective VAT reporting context is unavailable.",
            "VAT-PERIOD-UNAVAILABLE" => "The calculated VAT period is unavailable.",
            "VAT-READINESS-UNAVAILABLE" => "VAT readiness checks could not be completed.",
            _ => finding.Message
        };
    }
}
