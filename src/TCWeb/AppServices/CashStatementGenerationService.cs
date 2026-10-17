using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TCExports.Generator;
using TCExports.Generator.Contracts;

namespace TradeControl.Web.AppServices;

public sealed class CashStatementGenerationOptions
{
    public const string SectionName = "CashStatementGeneration";

    public bool OdsEnabled { get; set; } = true;
    public int MaximumConcurrentGenerations { get; set; } = 2;
    public int MaximumQueuedGenerations { get; set; } = 2;
    public int QueueTimeoutSeconds { get; set; } = 10;
    public int ExecutionTimeoutSeconds { get; set; } = 300;
    public long MaximumOutputBytes { get; set; } = 64L * 1024L * 1024L;
}

public sealed class CashStatementGenerationOptionsValidator
    : IValidateOptions<CashStatementGenerationOptions>
{
    private const long GeneratorMaximumOutputBytes = 64L * 1024L * 1024L;

    public ValidateOptionsResult Validate(string? name, CashStatementGenerationOptions options)
    {
        var failures = new List<string>();
        if (options.MaximumConcurrentGenerations is < 1 or > 32)
            failures.Add("MaximumConcurrentGenerations must be between 1 and 32.");
        if (options.MaximumQueuedGenerations is < 0 or > 32)
            failures.Add("MaximumQueuedGenerations must be between 0 and 32.");
        if (options.QueueTimeoutSeconds is < 1 or > 600)
            failures.Add("QueueTimeoutSeconds must be between 1 and 600.");
        if (options.ExecutionTimeoutSeconds is < 1 or > 1800)
            failures.Add("ExecutionTimeoutSeconds must be between 1 and 1800.");
        if (options.MaximumOutputBytes is < 1 or > GeneratorMaximumOutputBytes)
            failures.Add($"MaximumOutputBytes must be between 1 and {GeneratorMaximumOutputBytes}.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

public sealed record CashStatementSource(string ConnectionString);

public interface ICashStatementSourceResolver
{
    ValueTask<CashStatementSource> ResolveAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves the node owned by this single-tenant host only after the normal ASP.NET
/// authentication boundary has identified a user. A future shared host can replace
/// this service with a tenant-directory resolver without changing the generator.
/// </summary>
public sealed class ConfiguredCashStatementSourceResolver : ICashStatementSourceResolver
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ConfiguredCashStatementSourceResolver(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public ValueTask<CashStatementSource> ResolveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("An authenticated node user is required.");

        var connectionString = _configuration.GetConnectionString("TCNodeContext");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The Trade Control node source is not configured.");

        return ValueTask.FromResult(new CashStatementSource(connectionString));
    }
}

public sealed record CashStatementGenerationRequest(
    string Format,
    int CommandTimeoutSeconds,
    bool IncludeActivePeriods,
    bool IncludeBalanceSheet,
    bool IncludeBankBalances,
    bool IncludeOrderBook,
    bool IncludeTaxAccruals,
    bool IncludeVatDetails,
    bool IncludeReconciliation);

public sealed record CashStatementGenerationResult(
    bool IsSuccess,
    string Code,
    string Message,
    string? FileName = null,
    string? ContentType = null,
    byte[]? Content = null)
{
    public static CashStatementGenerationResult Failure(string code, string message) =>
        new(false, code, message);

    public static CashStatementGenerationResult Success(
        string fileName,
        string contentType,
        byte[] content) =>
        new(true, "SUCCESS", "Cash Statement generated.", fileName, contentType, content);
}

public interface ICashStatementExporter
{
    Task<ExportResult> ExportAsync(ExportPayload payload, CancellationToken cancellationToken);
}

public sealed class CashStatementExporter : ICashStatementExporter
{
    public Task<ExportResult> ExportAsync(ExportPayload payload, CancellationToken cancellationToken) =>
        ExportRunner.ExportDataAsync(payload, cancellationToken);
}

public sealed class CashStatementGenerationGate : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maximumQueuedGenerations;
    private int _queuedGenerations;

    public CashStatementGenerationGate(IOptions<CashStatementGenerationOptions> options)
    {
        _semaphore = new SemaphoreSlim(
            options.Value.MaximumConcurrentGenerations,
            options.Value.MaximumConcurrentGenerations);
        _maximumQueuedGenerations = options.Value.MaximumQueuedGenerations;
    }

    public async ValueTask<IDisposable?> TryEnterAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_semaphore.Wait(0))
            return new Lease(_semaphore);

        var queued = Interlocked.Increment(ref _queuedGenerations);
        if (queued > _maximumQueuedGenerations)
        {
            Interlocked.Decrement(ref _queuedGenerations);
            return null;
        }

        try
        {
            var entered = await _semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
            return entered ? new Lease(_semaphore) : null;
        }
        finally
        {
            Interlocked.Decrement(ref _queuedGenerations);
        }
    }

    public void Dispose() => _semaphore.Dispose();

    private sealed class Lease : IDisposable
    {
        private SemaphoreSlim? _semaphore;

        public Lease(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose() => Interlocked.Exchange(ref _semaphore, null)?.Release();
    }
}

public interface ICashStatementGenerationService
{
    Task<CashStatementGenerationResult> GenerateAsync(
        CashStatementGenerationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CashStatementGenerationService : ICashStatementGenerationService
{
    private readonly CashStatementGenerationOptions _options;
    private readonly CashStatementGenerationGate _gate;
    private readonly ICashStatementSourceResolver _sourceResolver;
    private readonly ICashStatementExporter _exporter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CashStatementGenerationService> _logger;

    public CashStatementGenerationService(
        IOptions<CashStatementGenerationOptions> options,
        CashStatementGenerationGate gate,
        ICashStatementSourceResolver sourceResolver,
        ICashStatementExporter exporter,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CashStatementGenerationService> logger)
    {
        _options = options.Value;
        _gate = gate;
        _sourceResolver = sourceResolver;
        _exporter = exporter;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<CashStatementGenerationResult> GenerateAsync(
        CashStatementGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var format = request.Format.Trim().ToLowerInvariant();
        if (format is not ("excel" or "libre"))
            return CashStatementGenerationResult.Failure(
                "UNSUPPORTED_FORMAT",
                "Choose Microsoft Excel or LibreOffice Calc.");
        if (format == "libre" && !_options.OdsEnabled)
            return CashStatementGenerationResult.Failure(
                "FEATURE_DISABLED",
                "LibreOffice generation is temporarily unavailable. Microsoft Excel remains available.");

        CashStatementSource source;
        try
        {
            source = await _sourceResolver.ResolveAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException)
        {
            return CashStatementGenerationResult.Failure(
                "SOURCE_NOT_AUTHORISED",
                "The Cash Statement source is not authorised for this request.");
        }
        catch (InvalidOperationException)
        {
            return CashStatementGenerationResult.Failure(
                "SOURCE_UNAVAILABLE",
                "The Cash Statement source is unavailable.");
        }

        using var lease = await _gate.TryEnterAsync(
            TimeSpan.FromSeconds(_options.QueueTimeoutSeconds),
            cancellationToken).ConfigureAwait(false);
        if (lease is null)
        {
            _logger.LogWarning("Cash Statement generation rejected because the bounded queue was full.");
            return CashStatementGenerationResult.Failure(
                "SERVICE_BUSY",
                "The Cash Statement service is busy. Try again shortly.");
        }

        using var execution = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        execution.CancelAfter(TimeSpan.FromSeconds(_options.ExecutionTimeoutSeconds));
        var payload = BuildPayload(source.ConnectionString, request, format);
        var stopwatch = Stopwatch.StartNew();

        ExportResult export;
        try
        {
            export = await _exporter.ExportAsync(payload, execution.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Cash Statement {Format} generation exceeded the configured execution limit.",
                format);
            return CashStatementGenerationResult.Failure(
                "GENERATION_TIMEOUT",
                "Cash Statement generation exceeded the service time limit.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cash Statement {Format} generation was cancelled by the caller.", format);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "Cash Statement {Format} generation failed with {ExceptionType}.",
                format,
                exception.GetType().Name);
            return CashStatementGenerationResult.Failure(
                "EXECUTION_ERROR",
                "Cash Statement generation failed.");
        }
        finally
        {
            stopwatch.Stop();
        }

        if (!string.Equals(export.Status, "success", StringComparison.OrdinalIgnoreCase))
        {
            var code = string.IsNullOrWhiteSpace(export.Code) ? "EXECUTION_ERROR" : export.Code;
            _logger.LogWarning(
                "Cash Statement {Format} generation returned {Code} after {ElapsedMilliseconds} ms.",
                format,
                code,
                stopwatch.ElapsedMilliseconds);
            return CashStatementGenerationResult.Failure(code, SafeFailureMessage(code));
        }

        if (string.IsNullOrWhiteSpace(export.FileContent))
            return CashStatementGenerationResult.Failure(
                "EMPTY_OUTPUT",
                "Cash Statement generation returned no document.");

        byte[] content;
        try
        {
            content = Convert.FromBase64String(export.FileContent);
        }
        catch (FormatException)
        {
            return CashStatementGenerationResult.Failure(
                "INVALID_OUTPUT",
                "Cash Statement generation returned an invalid document.");
        }

        if (content.LongLength > _options.MaximumOutputBytes)
        {
            _logger.LogWarning(
                "Cash Statement {Format} output exceeded the configured byte limit.",
                format);
            return CashStatementGenerationResult.Failure(
                "OUTPUT_LIMIT_EXCEEDED",
                "The Cash Statement exceeds the service output limit.");
        }

        var isLibre = format == "libre";
        var extension = isLibre ? "ods" : "xlsx";
        var fileName = string.IsNullOrWhiteSpace(export.FileName)
            ? $"cash-statement.{extension}"
            : export.FileName;
        var contentType = isLibre
            ? "application/vnd.oasis.opendocument.spreadsheet"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        _logger.LogInformation(
            "Cash Statement {Format} generation completed in {ElapsedMilliseconds} ms with {OutputBytes} bytes.",
            format,
            stopwatch.ElapsedMilliseconds,
            content.LongLength);
        return CashStatementGenerationResult.Success(fileName, contentType, content);
    }

    private ExportPayload BuildPayload(
        string connectionString,
        CashStatementGenerationRequest request,
        string format)
    {
        var userName = _httpContextAccessor.HttpContext?.User.Identity?.Name;
        return new ExportPayload
        {
            SqlConnection = connectionString,
            UserName = string.IsNullOrWhiteSpace(userName) ? "authenticated-user" : userName,
            DocumentType = "cashflow",
            Format = format,
            Params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["commandTimeout"] = request.CommandTimeoutSeconds.ToString(CultureInfo.InvariantCulture),
                ["includeActivePeriods"] = BooleanText(request.IncludeActivePeriods),
                ["includeBalanceSheet"] = BooleanText(request.IncludeBalanceSheet),
                ["includeBankBalances"] = BooleanText(request.IncludeBankBalances),
                ["includeOrderBook"] = BooleanText(request.IncludeOrderBook),
                ["includeTaxAccruals"] = BooleanText(request.IncludeTaxAccruals),
                ["includeVatDetails"] = BooleanText(request.IncludeVatDetails),
                ["includeReconciliation"] = BooleanText(request.IncludeReconciliation)
            }
        };
    }

    private static string SafeFailureMessage(string code) => code switch
    {
        "VALIDATION_ERROR" => "Review the Cash Statement options and try again.",
        "CASH_STATEMENT_INVALID" => "The statement data did not satisfy the reporting contract.",
        "UNSUPPORTED_FORMAT" => "Choose Microsoft Excel or LibreOffice Calc.",
        _ => "Cash Statement generation failed."
    };

    private static string BooleanText(bool value) => value ? "true" : "false";
}
