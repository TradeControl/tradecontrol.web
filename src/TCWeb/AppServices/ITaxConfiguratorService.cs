using System.Collections.Generic;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Shared.Tree;

namespace TradeControl.Web.AppServices
{
    public interface ITaxConfiguratorService
    {
        Task<IReadOnlyList<TreeNode>> GetRootNodesAsync();
        Task<IReadOnlyList<TreeNode>> GetChildrenAsync(TreeNode node);
        Task<TaxConfiguratorNodeDetails?> GetNodeDetailsAsync(TreeNode node);
    }
}
