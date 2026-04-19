using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Shared.Tree;

namespace TradeControl.Web.AppServices
{
    public sealed class TaxConfiguratorService : ITaxConfiguratorService
    {
        private readonly NodeContext _nodeContext;
        private readonly SemaphoreSlim _dbGate = new(1, 1);

        public TaxConfiguratorService(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public async Task<IReadOnlyList<TreeNode>> GetRootNodesAsync()
        {
            await _dbGate.WaitAsync();
            try
            {
                return await _nodeContext.App_tbJurisdictions
                    .AsNoTracking()
                    .Where(j => j.IsEnabled)
                    .OrderBy(j => j.JurisdictionName)
                    .Select(j => new TreeNode(
                        $"jurisdiction:{j.JurisdictionCode}",
                        $"{j.JurisdictionCode} - {j.JurisdictionName}",
                        "bi-geo-alt",
                        j.TbTaxTagSources.Any(s => s.IsEnabled)))
                    .ToListAsync();
            }
            finally
            {
                _dbGate.Release();
            }
        }

        public async Task<IReadOnlyList<TreeNode>> GetChildrenAsync(TreeNode node)
        {
            if (TryParseJurisdictionKey(node.Key, out var jurisdictionCode))
                return await GetSourceNodesAsync(jurisdictionCode);

            if (TryParseSourceKey(node.Key, out var sourceCode))
                return await GetTagClassNodesAsync(sourceCode);

            if (TryParseTagClassKey(node.Key, out var sourceCode2, out var tagClassCode))
                return await GetTagNodesAsync(sourceCode2, tagClassCode);

            return Array.Empty<TreeNode>();
        }

        public async Task<TaxConfiguratorNodeDetails?> GetNodeDetailsAsync(TreeNode node)
        {
            if (TryParseJurisdictionKey(node.Key, out var jurisdictionCode))
                return await GetJurisdictionDetailsAsync(node.Key, jurisdictionCode);

            if (TryParseSourceKey(node.Key, out var sourceCode))
                return await GetSourceDetailsAsync(node.Key, sourceCode);

            if (TryParseTagClassKey(node.Key, out var sourceCode2, out var tagClassCode))
                return await GetTagClassDetailsAsync(node.Key, sourceCode2, tagClassCode);

            if (TryParseTagKey(node.Key, out var sourceCode3, out var tagCode))
                return await GetTagDetailsAsync(node.Key, sourceCode3, tagCode);

            return null;
        }

        private async Task<IReadOnlyList<TreeNode>> GetSourceNodesAsync(string jurisdictionCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                return await _nodeContext.Cash_tbTaxTagSources
                    .AsNoTracking()
                    .Where(s => s.JurisdictionCode == jurisdictionCode && s.IsEnabled)
                    .OrderBy(s => s.SourceName)
                    .ThenBy(s => s.TaxSourceCode)
                    .Select(s => new TreeNode(
                        $"source:{s.TaxSourceCode}",
                        $"{s.TaxSourceCode} - {s.SourceName}",
                        "bi-diagram-3",
                        s.TbTaxTags.Any(t => t.IsEnabled)))
                    .ToListAsync();
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<IReadOnlyList<TreeNode>> GetTagClassNodesAsync(string sourceCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                return await (
                    from tag in _nodeContext.Cash_tbTaxTags.AsNoTracking()
                    join tagClass in _nodeContext.Cash_tbTaxTagClasses.AsNoTracking()
                        on tag.TagClassCode equals tagClass.TagClassCode
                    where tag.TaxSourceCode == sourceCode
                    group tagClass by new { tagClass.TagClassCode, tagClass.TagClass } into grouped
                    orderby grouped.Key.TagClassCode
                    select new TreeNode(
                        $"class:{sourceCode}:{(byte)grouped.Key.TagClassCode}",
                        $"{grouped.Key.TagClass} ({grouped.Count()})",
                        IconForTagClass(grouped.Key.TagClassCode),
                        grouped.Any()))
                    .ToListAsync();
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<IReadOnlyList<TreeNode>> GetTagNodesAsync(string sourceCode, NodeEnum.TagClassCode tagClassCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                return await _nodeContext.Cash_tbTaxTags
                    .AsNoTracking()
                    .Where(t => t.TaxSourceCode == sourceCode && t.TagClassCode == tagClassCode)
                    .OrderBy(t => t.DisplayOrder)
                    .ThenBy(t => t.TagName)
                    .Select(t => new TreeNode(
                        $"tag:{t.TaxSourceCode}:{t.TagCode}",
                        $"{t.TagCode} - {t.TagName}",
                        "bi-tag",
                        false,
                        t.TbTaxTagMaps.Any()))
                    .ToListAsync();
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<TaxConfiguratorNodeDetails?> GetJurisdictionDetailsAsync(string key, string jurisdictionCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                var jurisdiction = await _nodeContext.App_tbJurisdictions
                    .AsNoTracking()
                    .Where(j => j.JurisdictionCode == jurisdictionCode)
                    .Select(j => new {
                        j.JurisdictionCode,
                        j.JurisdictionName,
                        j.UocCode,
                        j.IsEnabled
                    })
                    .SingleOrDefaultAsync();

                if (jurisdiction is null)
                    return null;

                return new TaxConfiguratorNodeDetails {
                    Kind = TaxConfiguratorNodeKind.Jurisdiction,
                    Key = key,
                    Code = jurisdiction.JurisdictionCode,
                    Title = jurisdiction.JurisdictionName,
                    Description = null,
                    JurisdictionCode = jurisdiction.JurisdictionCode,
                    JurisdictionName = jurisdiction.JurisdictionName,
                    IsEnabled = jurisdiction.IsEnabled,
                    CanMap = false,
                    Mappings = Array.Empty<TaxConfiguratorMappingRow>()
                };
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<TaxConfiguratorNodeDetails?> GetSourceDetailsAsync(string key, string sourceCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                var source = await (
                    from s in _nodeContext.Cash_tbTaxTagSources.AsNoTracking()
                    join j in _nodeContext.App_tbJurisdictions.AsNoTracking()
                        on s.JurisdictionCode equals j.JurisdictionCode
                    where s.TaxSourceCode == sourceCode
                    select new {
                        s.TaxSourceCode,
                        s.SourceName,
                        s.SourceDescription,
                        s.JurisdictionCode,
                        j.JurisdictionName,
                        s.IsEnabled
                    })
                    .SingleOrDefaultAsync();

                if (source is null)
                    return null;

                return new TaxConfiguratorNodeDetails {
                    Kind = TaxConfiguratorNodeKind.Source,
                    Key = key,
                    Code = source.TaxSourceCode,
                    Title = source.SourceName,
                    Description = source.SourceDescription,
                    ParentCode = source.JurisdictionCode,
                    ParentName = source.JurisdictionName,
                    JurisdictionCode = source.JurisdictionCode,
                    JurisdictionName = source.JurisdictionName,
                    TaxSourceCode = source.TaxSourceCode,
                    TaxSourceName = source.SourceName,
                    IsEnabled = source.IsEnabled,
                    CanMap = false,
                    Mappings = Array.Empty<TaxConfiguratorMappingRow>()
                };
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<TaxConfiguratorNodeDetails?> GetTagClassDetailsAsync(string key, string sourceCode, NodeEnum.TagClassCode tagClassCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                var item = await (
                    from source in _nodeContext.Cash_tbTaxTagSources.AsNoTracking()
                    join jurisdiction in _nodeContext.App_tbJurisdictions.AsNoTracking()
                        on source.JurisdictionCode equals jurisdiction.JurisdictionCode
                    join tagClass in _nodeContext.Cash_tbTaxTagClasses.AsNoTracking()
                        on tagClassCode equals tagClass.TagClassCode
                    where source.TaxSourceCode == sourceCode
                    select new {
                        source.TaxSourceCode,
                        source.SourceName,
                        source.JurisdictionCode,
                        jurisdiction.JurisdictionName,
                        source.IsEnabled,
                        tagClass.TagClassCode,
                        tagClass.TagClass
                    })
                    .SingleOrDefaultAsync();

                if (item is null)
                    return null;

                return new TaxConfiguratorNodeDetails {
                    Kind = TaxConfiguratorNodeKind.TagClass,
                    Key = key,
                    Code = ((byte)item.TagClassCode).ToString(),
                    Title = item.TagClass,
                    Description = null,
                    ParentCode = item.TaxSourceCode,
                    ParentName = item.SourceName,
                    JurisdictionCode = item.JurisdictionCode,
                    JurisdictionName = item.JurisdictionName,
                    TaxSourceCode = item.TaxSourceCode,
                    TaxSourceName = item.SourceName,
                    TagClassCode = ((byte)item.TagClassCode).ToString(),
                    TagClassName = item.TagClass,
                    IsEnabled = item.IsEnabled,
                    CanMap = false,
                    Mappings = Array.Empty<TaxConfiguratorMappingRow>()
                };
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<TaxConfiguratorNodeDetails?> GetTagDetailsAsync(string key, string sourceCode, string tagCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                var item = await (
                    from tag in _nodeContext.Cash_tbTaxTags.AsNoTracking()
                    join tagClass in _nodeContext.Cash_tbTaxTagClasses.AsNoTracking()
                        on tag.TagClassCode equals tagClass.TagClassCode
                    join source in _nodeContext.Cash_tbTaxTagSources.AsNoTracking()
                        on tag.TaxSourceCode equals source.TaxSourceCode
                    join jurisdiction in _nodeContext.App_tbJurisdictions.AsNoTracking()
                        on source.JurisdictionCode equals jurisdiction.JurisdictionCode
                    where tag.TaxSourceCode == sourceCode && tag.TagCode == tagCode
                    select new {
                        tag.TaxSourceCode,
                        tag.TagCode,
                        tag.TagName,
                        tag.TagDescription,
                        tag.TagClassCode,
                        tag.DisplayOrder,
                        tag.IsEnabled,
                        tagClass.TagClass,
                        source.SourceName,
                        source.JurisdictionCode,
                        jurisdiction.JurisdictionName
                    })
                    .SingleOrDefaultAsync();

                if (item is null)
                    return null;

                var mappings = await GetMappingsAsync(item.TaxSourceCode, item.TagCode);

                return new TaxConfiguratorNodeDetails {
                    Kind = TaxConfiguratorNodeKind.Tag,
                    Key = key,
                    Code = item.TagCode,
                    Title = item.TagName,
                    Description = item.TagDescription,
                    ParentCode = item.TaxSourceCode,
                    ParentName = item.SourceName,
                    JurisdictionCode = item.JurisdictionCode,
                    JurisdictionName = item.JurisdictionName,
                    TaxSourceCode = item.TaxSourceCode,
                    TaxSourceName = item.SourceName,
                    TagClassCode = item.TagClassCode.ToString(),
                    TagClassName = item.TagClass,
                    IsEnabled = item.IsEnabled,
                    CanMap = string.Equals(item.TagClass, "Component", StringComparison.OrdinalIgnoreCase),
                    DisplayOrder = item.DisplayOrder,
                    Mappings = mappings
                };
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<IReadOnlyList<TaxConfiguratorMappingRow>> GetMappingsAsync(string taxSourceCode, string tagCode)
        {
            var mappings = await (
                from map in _nodeContext.Cash_tbTaxTagMaps.AsNoTracking()
                join mapType in _nodeContext.Cash_tbTaxTagMapTypes.AsNoTracking()
                    on map.MapTypeCode equals mapType.MapTypeCode
                join category in _nodeContext.Cash_tbCategories.AsNoTracking()
                    on map.CategoryCode equals category.CategoryCode into categories
                from category in categories.DefaultIfEmpty()
                join cashCode in _nodeContext.Cash_Codes.AsNoTracking()
                    on map.CashCode equals cashCode.CashCode into cashCodes
                from cashCode in cashCodes.DefaultIfEmpty()
                where map.TaxSourceCode == taxSourceCode && map.TagCode == tagCode
                orderby map.MapTypeCode, map.CategoryCode, map.CashCode
                select new TaxConfiguratorMappingRow {
                    MapTypeCode = map.MapTypeCode,
                    MapType = mapType.MapType,
                    TargetCode = map.MapTypeCode == NodeEnum.MapTypeCode.Category ? map.CategoryCode : map.CashCode,
                    TargetName = map.MapTypeCode == NodeEnum.MapTypeCode.Category
                        ? (category != null ? category.Category : string.Empty)
                        : (cashCode != null ? cashCode.CashDescription : string.Empty),
                    IsEnabled = map.IsEnabled
                })
                .ToListAsync();

            return mappings;
        }

        private static bool TryParseJurisdictionKey(string key, out string jurisdictionCode)
        {
            jurisdictionCode = string.Empty;

            var parts = key.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !string.Equals(parts[0], "jurisdiction", StringComparison.OrdinalIgnoreCase))
                return false;

            jurisdictionCode = parts[1];
            return !string.IsNullOrWhiteSpace(jurisdictionCode);
        }

        private static bool TryParseSourceKey(string key, out string sourceCode)
        {
            sourceCode = string.Empty;

            var parts = key.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !string.Equals(parts[0], "source", StringComparison.OrdinalIgnoreCase))
                return false;

            sourceCode = parts[1];
            return !string.IsNullOrWhiteSpace(sourceCode);
        }

        private static bool TryParseTagClassKey(string key, out string sourceCode, out NodeEnum.TagClassCode tagClassCode)
        {
            sourceCode = string.Empty;
            tagClassCode = default;

            var parts = key.Split(':', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3 || !string.Equals(parts[0], "class", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!byte.TryParse(parts[2], out var code))
                return false;

            if (!Enum.IsDefined(typeof(NodeEnum.TagClassCode), code))
                return false;

            tagClassCode = (NodeEnum.TagClassCode)code;
            sourceCode = parts[1];
            return !string.IsNullOrWhiteSpace(sourceCode);
        }

        private static bool TryParseTagKey(string key, out string sourceCode, out string tagCode)
        {
            sourceCode = string.Empty;
            tagCode = string.Empty;

            var parts = key.Split(':', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3 || !string.Equals(parts[0], "tag", StringComparison.OrdinalIgnoreCase))
                return false;

            sourceCode = parts[1];
            tagCode = parts[2];
            return !string.IsNullOrWhiteSpace(sourceCode) && !string.IsNullOrWhiteSpace(tagCode);
        }

        private static string IconForTagClass(NodeEnum.TagClassCode tagClass)
        {
            return tagClass switch
            {
                NodeEnum.TagClassCode.Component => "bi-bullseye",
                NodeEnum.TagClassCode.Derived => "bi-calculator",
                NodeEnum.TagClassCode.Rollup => "bi-collection",
                _ => "bi-layers"
            };
        }
    }
}
