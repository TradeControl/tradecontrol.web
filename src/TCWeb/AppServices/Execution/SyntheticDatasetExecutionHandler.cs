using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.AppServices.Execution
{
    public class SyntheticDatasetExecutionHandler : IExecutionHandler
    {
        readonly NodeContext NodeContext;
        readonly IExecutionRuntimeState ExecutionRuntimeState;

        public SyntheticDatasetExecutionHandler(NodeContext nodeContext, IExecutionRuntimeState executionRuntimeState)
        {
            NodeContext = nodeContext;
            ExecutionRuntimeState = executionRuntimeState;
        }

        public NodeEnum.ExecutionType ExecutionType => NodeEnum.ExecutionType.SyntheticDataset;

        public async Task ExecuteAsync(App_tbExecution execution, CancellationToken cancellationToken)
        {
            var arguments = JsonSerializer.Deserialize<SyntheticDatasetExecutionArguments>(execution.Arguments ?? string.Empty);

            if (arguments == null)
                throw new InvalidOperationException("Execution arguments are missing or invalid.");

            execution.ProgressMessage = "Installing synthetic dataset...";
            ExecutionRuntimeState.UpdateProgress(execution.ExecutionCode, execution.ProgressMessage);
            await NodeContext.SaveChangesAsync(cancellationToken);

            await NodeContext.GenerateSyntheticDataset(
                isCompany: arguments.IsCompany,
                useStdCompanyTemplate: arguments.UseStdCompanyTemplate,
                isVatRegistered: arguments.IsVatRegistered,
                misOrdersPerMonth: arguments.MisOrdersPerMonth,
                monthsForward: arguments.MonthsForward,
                priceRatio: arguments.PriceRatio,
                quantityRatio: arguments.QuantityRatio,
                floatRatio: arguments.FloatRatio);

            execution.ProgressMessage = "Synthetic dataset installed.";
            ExecutionRuntimeState.UpdateProgress(execution.ExecutionCode, execution.ProgressMessage);
            await NodeContext.SaveChangesAsync(cancellationToken);
        }
    }
}
