using System.Threading.Tasks;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.Execution
{
    public interface IExecutionQueue
    {
        Task<string> EnqueueAsync(NodeEnum.ExecutionType executionType, string arguments, string queuedBy = null);
    }
}
