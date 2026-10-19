using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TCExports.Generator.Contracts;
using TradeControl.Web.AppServices;
using TradeControl.Web.Tenancy;

internal static class CashStatementServiceTests
{
    public static async Task<int> RunAsync()
    {
        var assertions = 0;

        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition)
                throw new InvalidOperationException(message);
        }

        async Task AssertCancelledAsync(Func<Task> action, string message)
        {
            assertions++;
            try
            {
                await action();
            }
            catch (OperationCanceledException)
            {
                return;
            }

            throw new InvalidOperationException(message);
        }

        var validator = new CashStatementGenerationOptionsValidator();
        Assert(validator.Validate(null, new CashStatementGenerationOptions()).Succeeded,
            "Default Cash Statement service limits are invalid.");
        Assert(validator.Validate(null, new CashStatementGenerationOptions
        {
            MaximumConcurrentGenerations = 0,
            MaximumQueuedGenerations = 33,
            QueueTimeoutSeconds = 0,
            ExecutionTimeoutSeconds = 0,
            MaximumOutputBytes = 0
        }).Failed, "Unbounded or unusable Cash Statement service limits were accepted.");

        var authenticatedContext = AuthenticatedContext();
        var tenantContext = new TenantContext();
        tenantContext.Bind(new TenantDescriptor(Guid.NewGuid(), "tenant.test", "TenantA",
            "Server=(tenant);Database=(authorised)"));
        var configuredResolver = new ConfiguredCashStatementSourceResolver(
            new HttpContextAccessor { HttpContext = authenticatedContext }, tenantContext);
        var source = await configuredResolver.ResolveAsync();
        Assert(source.ConnectionString.Contains("Database=(authorised)", StringComparison.Ordinal),
            "The configured single-tenant source was not resolved after authentication.");

        var unauthenticatedResolver = new ConfiguredCashStatementSourceResolver(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, tenantContext);
        await AssertThrowsAsync<UnauthorizedAccessException>(
            () => unauthenticatedResolver.ResolveAsync().AsTask(),
            "An unauthenticated request resolved a Cash Statement source.");

        var disabledOptions = Options.Create(new CashStatementGenerationOptions { OdsEnabled = false });
        using (var gate = new CashStatementGenerationGate(disabledOptions))
        {
            var exporter = new SuccessfulExporter();
            var service = Service(disabledOptions, gate, new FixedSourceResolver(), exporter, authenticatedContext);
            var ods = await service.GenerateAsync(Request("libre"));
            Assert(!ods.IsSuccess && ods.Code == "FEATURE_DISABLED",
                "The ODS feature flag did not reject LibreOffice generation.");
            Assert(exporter.Calls == 0, "Disabled ODS reached the exporter.");

            var excel = await service.GenerateAsync(Request("excel"));
            Assert(excel.IsSuccess && excel.Content is { Length: 4 },
                "Disabling ODS affected Excel generation.");
            Assert(exporter.Calls == 1, "Excel did not reach the exporter exactly once.");
        }

        var defaultOptions = Options.Create(new CashStatementGenerationOptions());
        using (var gate = new CashStatementGenerationGate(defaultOptions))
        {
            var exporter = new SuccessfulExporter();
            var service = Service(defaultOptions, gate, new RejectingSourceResolver(), exporter, authenticatedContext);
            var result = await service.GenerateAsync(Request("excel"));
            Assert(!result.IsSuccess && result.Code == "SOURCE_NOT_AUTHORISED",
                "A tenant-negative source resolution did not fail closed.");
            Assert(exporter.Calls == 0, "A tenant-negative request reached the exporter.");
        }

        var outputOptions = Options.Create(new CashStatementGenerationOptions { MaximumOutputBytes = 3 });
        using (var gate = new CashStatementGenerationGate(outputOptions))
        {
            var service = Service(
                outputOptions,
                gate,
                new FixedSourceResolver(),
                new SuccessfulExporter(),
                authenticatedContext);
            var result = await service.GenerateAsync(Request("excel"));
            Assert(!result.IsSuccess && result.Code == "OUTPUT_LIMIT_EXCEEDED",
                "The configured output-byte limit was not enforced.");
        }

        var timeoutOptions = Options.Create(new CashStatementGenerationOptions
        {
            ExecutionTimeoutSeconds = 1
        });
        using (var gate = new CashStatementGenerationGate(timeoutOptions))
        {
            var service = Service(
                timeoutOptions,
                gate,
                new FixedSourceResolver(),
                new WaitingExporter(),
                authenticatedContext);
            var result = await service.GenerateAsync(Request("excel"));
            Assert(!result.IsSuccess && result.Code == "GENERATION_TIMEOUT",
                "The overall execution limit was not enforced.");
        }

        using (var gate = new CashStatementGenerationGate(defaultOptions))
        {
            var exporter = new SuccessfulExporter();
            var service = Service(
                defaultOptions,
                gate,
                new WaitingSourceResolver(),
                exporter,
                authenticatedContext);
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            await AssertCancelledAsync(
                () => service.GenerateAsync(Request("excel"), cancellation.Token),
                "Caller cancellation was not observed during source acquisition.");
            Assert(exporter.Calls == 0,
                "Cancellation during source acquisition reached the exporter.");
        }

        using (var gate = new CashStatementGenerationGate(defaultOptions))
        {
            var service = Service(
                defaultOptions,
                gate,
                new FixedSourceResolver(),
                new WaitingExporter(),
                authenticatedContext);
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            await AssertCancelledAsync(
                () => service.GenerateAsync(Request("excel"), cancellation.Token),
                "Caller cancellation was converted into an ordinary generation error.");
        }

        var concurrencyOptions = Options.Create(new CashStatementGenerationOptions
        {
            MaximumConcurrentGenerations = 1,
            MaximumQueuedGenerations = 1,
            QueueTimeoutSeconds = 5
        });
        using (var gate = new CashStatementGenerationGate(concurrencyOptions))
        {
            var exporter = new BlockingExporter();
            var service = Service(
                concurrencyOptions,
                gate,
                new FixedSourceResolver(),
                exporter,
                authenticatedContext);
            var first = service.GenerateAsync(Request("excel"));
            await exporter.Started.Task;
            var queued = service.GenerateAsync(Request("libre"));
            await Task.Delay(50);
            var overloaded = await service.GenerateAsync(Request("excel"));
            Assert(!overloaded.IsSuccess && overloaded.Code == "SERVICE_BUSY",
                "Overload did not fail at the bounded queue.");
            exporter.Release.TrySetResult();
            var completed = await first;
            var queuedCompleted = await queued;
            Assert(completed.IsSuccess, "The admitted generation did not complete after overload rejection.");
            Assert(queuedCompleted.IsSuccess, "The bounded queued generation did not complete.");
            Assert(exporter.MaximumObservedConcurrency == 1,
                "The concurrency gate admitted more work than configured.");
        }

        using (var gate = new CashStatementGenerationGate(defaultOptions))
        {
            const string sensitive = "Password=should-never-escape; report-content";
            var service = Service(
                defaultOptions,
                gate,
                new FixedSourceResolver(),
                new ThrowingExporter(sensitive),
                authenticatedContext);
            var result = await service.GenerateAsync(Request("excel"));
            Assert(!result.IsSuccess && result.Code == "EXECUTION_ERROR",
                "An unexpected exporter failure did not return the safe execution code.");
            Assert(!result.Message.Contains(sensitive, StringComparison.Ordinal),
                "An unexpected exporter failure exposed sensitive exception text.");
        }

        Console.WriteLine($"Cash Statement service tests passed ({assertions} assertions).");
        return assertions;
    }

    private static CashStatementGenerationService Service(
        IOptions<CashStatementGenerationOptions> options,
        CashStatementGenerationGate gate,
        ICashStatementSourceResolver sourceResolver,
        ICashStatementExporter exporter,
        HttpContext context) =>
        new(
            options,
            gate,
            sourceResolver,
            exporter,
            new HttpContextAccessor { HttpContext = context },
            NullLogger<CashStatementGenerationService>.Instance);

    private static CashStatementGenerationRequest Request(string format) => new(
        format,
        30,
        IncludeActivePeriods: true,
        IncludeBalanceSheet: true,
        IncludeBankBalances: true,
        IncludeOrderBook: true,
        IncludeTaxAccruals: true,
        IncludeVatDetails: true,
        IncludeReconciliation: true);

    private static DefaultHttpContext AuthenticatedContext()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "cash-statement-test") },
            "test"));
        return context;
    }

    private static async Task AssertThrowsAsync<TException>(Func<Task> action, string message)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private sealed class FixedSourceResolver : ICashStatementSourceResolver
    {
        public ValueTask<CashStatementSource> ResolveAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(new CashStatementSource("Server=(tenant);Database=(authorised)"));
        }
    }

    private sealed class RejectingSourceResolver : ICashStatementSourceResolver
    {
        public ValueTask<CashStatementSource> ResolveAsync(CancellationToken cancellationToken = default) =>
            throw new UnauthorizedAccessException();
    }

    private sealed class WaitingSourceResolver : ICashStatementSourceResolver
    {
        public async ValueTask<CashStatementSource> ResolveAsync(
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable.");
        }
    }

    private sealed class SuccessfulExporter : ICashStatementExporter
    {
        public int Calls { get; private set; }

        public Task<ExportResult> ExportAsync(ExportPayload payload, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Calls++;
            return Task.FromResult(new ExportResult
            {
                Status = "success",
                FileName = payload.Format == "libre" ? "statement.ods" : "statement.xlsx",
                FileContent = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
            });
        }
    }

    private sealed class WaitingExporter : ICashStatementExporter
    {
        public async Task<ExportResult> ExportAsync(
            ExportPayload payload,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable.");
        }
    }

    private sealed class BlockingExporter : ICashStatementExporter
    {
        private int _active;
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int MaximumObservedConcurrency { get; private set; }

        public async Task<ExportResult> ExportAsync(
            ExportPayload payload,
            CancellationToken cancellationToken)
        {
            var active = Interlocked.Increment(ref _active);
            MaximumObservedConcurrency = Math.Max(MaximumObservedConcurrency, active);
            Started.TrySetResult();
            try
            {
                await Release.Task.WaitAsync(cancellationToken);
                return new ExportResult
                {
                    Status = "success",
                    FileName = "statement.xlsx",
                    FileContent = Convert.ToBase64String(new byte[] { 1 })
                };
            }
            finally
            {
                Interlocked.Decrement(ref _active);
            }
        }
    }

    private sealed class ThrowingExporter(string sensitiveMessage) : ICashStatementExporter
    {
        public Task<ExportResult> ExportAsync(ExportPayload payload, CancellationToken cancellationToken) =>
            throw new InvalidOperationException(sensitiveMessage);
    }
}
