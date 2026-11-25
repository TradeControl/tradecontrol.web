using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.Audit;
using TradeControl.Tax.UK.Adapters.Submission.TransactionEngine;

namespace TradeControl.Web.AppServices.TaxHub.CorporationTax;

/// <summary>
/// Advances acknowledged Transaction Engine conversations independently of a
/// browser circuit. Each pass performs at most one authority exchange per record.
/// Uncertain submissions are deliberately excluded because DATA_REQUEST recovery
/// requires a freshly resolved live identity and an explicit operator journey.
/// </summary>
internal sealed class CorporationTaxTransactionEngineWorker(
    IServiceScopeFactory scopes,
    IHttpClientFactory httpClientFactory,
    IOptions<CorporationTaxTransactionEngineOptions> options,
    TimeProvider timeProvider,
    ILogger<CorporationTaxTransactionEngineWorker> logger) : BackgroundService
{
    private readonly CorporationTaxTransactionEngineOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.IsEnabled) return;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15), timeProvider);
        do
        {
            try { await AdvanceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Corporation Tax Transaction Engine worker pass failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task AdvanceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ITransactionEngineConversationStore>();
        var content = scope.ServiceProvider.GetRequiredService<ISubmissionContentStore>();
        var protocol = TransactionEngineProtocolOptions.CorporationTaxProduction();
        var conversations = new TransactionEngineConversationCoordinator(store, content,
            protocol, timeProvider);
        var gateway = new TransactionEngineHttpGateway(
            httpClientFactory.CreateClient(CorporationTaxTransactionEngineService.HttpClientName),
            protocol, new(TimeoutSeconds: _options.TimeoutSeconds));
        var exchanges = new TransactionEngineExchangeCoordinator(conversations, gateway);

        foreach (var record in await store.ListActionableAsync(timeProvider.GetUtcNow(), 25,
                     cancellationToken))
        {
            try
            {
                if (record.Stage == TransactionEngineConversationStage.AwaitingPoll)
                    _ = await exchanges.PollOnceAsync(record.TenantReference,
                        record.PrincipalReference, record.ConversationReference,
                        cancellationToken);
                else if (record.Stage is TransactionEngineConversationStage.TerminalPersisted
                             or TransactionEngineConversationStage.DeletePending)
                    _ = await exchanges.DeleteOnceAsync(record.TenantReference,
                        record.PrincipalReference, record.ConversationReference,
                        cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception,
                    "Corporation Tax Transaction Engine action failed at stage {Stage}; durable state was retained.",
                    record.Stage);
            }
        }
    }
}
