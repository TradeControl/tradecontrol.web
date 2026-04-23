using System.Collections.Generic;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Shared.Tree;

namespace TradeControl.Web.AppServices
{
    public interface ITaxConfiguratorService
    {
        Task<IReadOnlyList<TreeNode>> GetRootNodesAsync();
        Task<IReadOnlyList<TreeNode>> GetChildrenAsync(TreeNode node);
        Task<TaxConfiguratorNodeDetails?> GetNodeDetailsAsync(TreeNode node);

        Task AddCategoryMappingAsync(string sourceCode, string tagCode, string categoryCode);
        Task AddCashCodeMappingAsync(string sourceCode, string tagCode, string cashCode);
        Task RemoveMappingAsync(string sourceCode, string tagCode, NodeEnum.MapTypeCode mapTypeCode, string categoryCode, string cashCode);
        Task ToggleMappingEnabledAsync(string sourceCode, string tagCode, NodeEnum.MapTypeCode mapTypeCode, string categoryCode, string cashCode);
    }
}
