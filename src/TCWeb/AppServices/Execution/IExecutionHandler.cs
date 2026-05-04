using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.AppServices.Execution
{
    public interface IExecutionHandler
    {
        NodeEnum.ExecutionType ExecutionType { get; }

        Task ExecuteAsync(App_tbExecution execution, CancellationToken cancellationToken);
    }
}
