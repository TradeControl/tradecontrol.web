using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
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
                    .OrderBy(j => j.JurisdictionName)
                    .Select(j => new TreeNode(
                        $"jurisdiction:{j.JurisdictionCode}",
                        $"{j.JurisdictionCode} - {j.JurisdictionName}",
                        "bi-geo-alt",
                        j.TbTaxTagSources.Any(),
                        false,
                        true,
                        j.TbTaxTagSources.Any(s => s.TbTaxTags.Any(t => t.TbTaxTagMaps.Any(m => !m.IsEnabled)))))
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

        public Task AddCategoryMappingAsync(string sourceCode, string tagCode, string categoryCode)
        {
            return ExecuteMutationAsync(sourceCode, async () => {
                var category = NormalizeCode(categoryCode);
                if (string.IsNullOrWhiteSpace(category))
                    throw new ArgumentException("Category code is required.", nameof(categoryCode));

                var existing = await FindMapAsync(sourceCode, tagCode, NodeEnum.MapTypeCode.Category, category, string.Empty);
                if (existing is null)
                {
                    _nodeContext.Cash_tbTaxTagMaps.Add(new Cash_tbTaxTagMap {
                        TaxSourceCode = NormalizeCode(sourceCode),
                        TagCode = NormalizeCode(tagCode),
                        MapTypeCode = NodeEnum.MapTypeCode.Category,
                        CategoryCode = category,
                        CashCode = string.Empty,
                        IsEnabled = true
                    });
                }
                else
                {
                    existing.IsEnabled = true;
                }
            });
        }

        public Task AddCashCodeMappingAsync(string sourceCode, string tagCode, string cashCode)
        {
            return ExecuteMutationAsync(sourceCode, async () => {
                var code = NormalizeCode(cashCode);
                if (string.IsNullOrWhiteSpace(code))
                    throw new ArgumentException("Cash code is required.", nameof(cashCode));

                var existing = await FindMapAsync(sourceCode, tagCode, NodeEnum.MapTypeCode.CashCode, string.Empty, code);
                if (existing is null)
                {
                    _nodeContext.Cash_tbTaxTagMaps.Add(new Cash_tbTaxTagMap {
                        TaxSourceCode = NormalizeCode(sourceCode),
                        TagCode = NormalizeCode(tagCode),
                        MapTypeCode = NodeEnum.MapTypeCode.CashCode,
                        CategoryCode = string.Empty,
                        CashCode = code,
                        IsEnabled = true
                    });
                }
                else
                {
                    existing.IsEnabled = true;
                }
            });
        }

        public Task RemoveMappingAsync(string sourceCode, string tagCode, NodeEnum.MapTypeCode mapTypeCode, string categoryCode, string cashCode)
        {
            return ExecuteMutationAsync(sourceCode, async () => {
                var map = await FindMapAsync(sourceCode, tagCode, mapTypeCode, NormalizeCode(categoryCode), NormalizeCode(cashCode));
                if (map is not null)
                    _nodeContext.Cash_tbTaxTagMaps.Remove(map);
            });
        }

        public Task ToggleMappingEnabledAsync(string sourceCode, string tagCode, NodeEnum.MapTypeCode mapTypeCode, string categoryCode, string cashCode)
        {
            return ExecuteMutationAsync(sourceCode, async () => {
                var map = await FindMapAsync(sourceCode, tagCode, mapTypeCode, NormalizeCode(categoryCode), NormalizeCode(cashCode));
                if (map is not null)
                    map.IsEnabled = !map.IsEnabled;
            });
        }

        private async Task<IReadOnlyList<TaxConfiguratorValidationRow>> GetValidationIssuesAsync(string taxSourceCode)
        {
            var issues = new List<TaxConfiguratorValidationRow>();

            var connection = _nodeContext.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT IsError, TagCode, TagName, CashCode, CategoryCode, HitCount, Message
                    FROM Cash.fnTaxTagMapValidate(@TaxSourceCode)
                    ORDER BY IsError DESC, TagCode, CashCode, CategoryCode;";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@TaxSourceCode";
                parameter.DbType = DbType.String;
                parameter.Value = NormalizeCode(taxSourceCode);
                command.Parameters.Add(parameter);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    issues.Add(new TaxConfiguratorValidationRow {
                        IsError = !reader.IsDBNull(0) && reader.GetBoolean(0),
                        TagCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                        TagName = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CashCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CategoryCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        HitCount = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                        Message = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                    });
                }
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }

            return issues;
        }

        private async Task<IReadOnlyList<TreeNode>> GetSourceNodesAsync(string jurisdictionCode)
        {
            await _dbGate.WaitAsync();
            try
            {
                var sources = await _nodeContext.Cash_tbTaxTagSources
                    .AsNoTracking()
                    .Where(s => s.JurisdictionCode == jurisdictionCode)
                    .OrderBy(s => s.SourceName)
                    .ThenBy(s => s.TaxSourceCode)
                    .Select(s => new
                    {
                        s.TaxSourceCode,
                        s.SourceName,
                        HasChildren = s.TbTaxTags.Any()
                    })
                    .ToListAsync();

                var nodes = new List<TreeNode>(sources.Count);

                foreach (var source in sources)
                {
                    var validationIssues = await GetValidationIssuesAsync(source.TaxSourceCode);
                    var validationCount = validationIssues.Count;

                    nodes.Add(new TreeNode(
                        $"source:{source.TaxSourceCode}",
                        validationCount > 0
                            ? $"{source.TaxSourceCode} - {source.SourceName} ({validationCount})"
                            : $"{source.TaxSourceCode} - {source.SourceName}",
                        "bi-diagram-3",
                        source.HasChildren,
                        false,
                        true,
                        validationIssues.Any(v => v.IsError)));
                }

                return nodes;
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
                    group tag by new { tagClass.TagClassCode, tagClass.TagClass } into grouped
                    orderby grouped.Key.TagClassCode
                    select new TreeNode(
                        $"class:{sourceCode}:{(byte)grouped.Key.TagClassCode}",
                        $"{grouped.Key.TagClass} ({grouped.Count()})",
                        IconForTagClass(grouped.Key.TagClassCode),
                        grouped.Any(),
                        false,
                        true,
                        false))
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
                        t.TagCode,
                        "bi-tag",
                        false,
                        t.TbTaxTagMaps.Any(),
                        !t.TbTaxTagMaps.Any(m => !m.IsEnabled),
                        false))
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
                        j.UocCode
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
                    })
                    .SingleOrDefaultAsync();

                if (source is null)
                    return null;

                var validationIssues = await GetValidationIssuesAsync(source.TaxSourceCode);

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
                    CanMap = false,
                    ValidationIssues = validationIssues,
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
                        tagClass.TagClassCode,
                        tagClass.TagClass
                    })
                    .SingleOrDefaultAsync();

                if (item is null)
                    return null;

                var tags = await _nodeContext.Cash_tbTaxTags
                    .AsNoTracking()
                    .Where(t => t.TaxSourceCode == sourceCode && t.TagClassCode == tagClassCode)
                    .OrderBy(t => t.DisplayOrder)
                    .ThenBy(t => t.TagCode)
                    .Select(t => new TaxConfiguratorTagRow {
                        TagCode = t.TagCode,
                        TagName = t.TagName,
                        DisplayOrder = t.DisplayOrder,
                        IsEnabled = !t.TbTaxTagMaps.Any(m => !m.IsEnabled),
                        IsMapped = t.TbTaxTagMaps.Any()
                    })
                    .ToListAsync();

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
                    CanMap = false,
                    Tags = tags,
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
                        tagClass.TagClass,
                        source.SourceName,
                        source.JurisdictionCode,
                        jurisdiction.JurisdictionName
                    })
                    .SingleOrDefaultAsync();

                if (item is null)
                    return null;

                IReadOnlyList<TaxConfiguratorLookupRow> categoryOptions = Array.Empty<TaxConfiguratorLookupRow>();
                IReadOnlyList<TaxConfiguratorLookupRow> cashCodeOptions = Array.Empty<TaxConfiguratorLookupRow>();

                if (item.TagClassCode == NodeEnum.TagClassCode.Component)
                {
                    categoryOptions = await GetCategoryOptionsAsync();
                    cashCodeOptions = await GetCashCodeOptionsAsync();
                }

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
                    TagClassCode = ((byte)item.TagClassCode).ToString(),
                    TagClassName = item.TagClass,
                    CanMap = item.TagClassCode == NodeEnum.TagClassCode.Component,
                    DisplayOrder = item.DisplayOrder,
                    CategoryOptions = categoryOptions,
                    CashCodeOptions = cashCodeOptions,
                    Mappings = mappings
                };
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private async Task<IReadOnlyList<TaxConfiguratorLookupRow>> GetCategoryOptionsAsync()
        {
            return await _nodeContext.Set<Cash_tbCategory>()
                .AsNoTracking()
                .Where(c =>
                    c.IsEnabled != 0 &&
                    (c.CategoryTypeCode == (short)NodeEnum.CategoryType.Nominal ||
                     c.CategoryTypeCode == (short)NodeEnum.CategoryType.CashTotal))
                .OrderBy(c => c.Category)
                .Select(c => new TaxConfiguratorLookupRow {
                    Code = c.CategoryCode,
                    Text = c.CategoryCode + " - " + c.Category
                })
                .ToListAsync();
        }

        private async Task<IReadOnlyList<TaxConfiguratorLookupRow>> GetCashCodeOptionsAsync()
        {
            return await _nodeContext.Set<Cash_tbCode>()
                .AsNoTracking()
                .Where(c => c.IsEnabled != 0)
                .OrderBy(c => c.CashDescription)
                .Select(c => new TaxConfiguratorLookupRow {
                    Code = c.CashCode,
                    Text = c.CashCode + " - " + c.CashDescription
                })
                .ToListAsync();
        }

        private async Task<IReadOnlyList<TaxConfiguratorMappingRow>> GetMappingsAsync(string taxSourceCode, string tagCode)
        {
            return await (
                from map in _nodeContext.Cash_tbTaxTagMaps.AsNoTracking()
                join mapType in _nodeContext.Cash_tbTaxTagMapTypes.AsNoTracking()
                    on map.MapTypeCode equals mapType.MapTypeCode
                join category in _nodeContext.Set<Cash_tbCategory>().AsNoTracking()
                    on map.CategoryCode equals category.CategoryCode into categories
                from category in categories.DefaultIfEmpty()
                join cashCode in _nodeContext.Set<Cash_tbCode>().AsNoTracking()
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
        }

        private async Task ExecuteMutationAsync(string taxSourceCode, Func<Task> mutation)
        {
            await _dbGate.WaitAsync();
            try
            {
                await using var transaction = await _nodeContext.Database.BeginTransactionAsync();
                try
                {
                    await mutation();
                    await _nodeContext.SaveChangesAsync();
                    await ValidateTaxSourceAsync(taxSourceCode);
                    await transaction.CommitAsync();
                }
                catch
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch
                    {
                    }

                    throw;
                }
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private Task ValidateTaxSourceAsync(string taxSourceCode)
        {
            return _nodeContext.Database.ExecuteSqlRawAsync(
                "EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = {0}",
                NormalizeCode(taxSourceCode));
        }

        private async Task<Cash_tbTaxTagMap?> FindMapAsync(string sourceCode, string tagCode, NodeEnum.MapTypeCode mapTypeCode, string categoryCode, string cashCode)
        {
            return await _nodeContext.Cash_tbTaxTagMaps
                .SingleOrDefaultAsync(m =>
                    m.TaxSourceCode == NormalizeCode(sourceCode) &&
                    m.TagCode == NormalizeCode(tagCode) &&
                    m.MapTypeCode == mapTypeCode &&
                    m.CategoryCode == categoryCode &&
                    m.CashCode == cashCode);
        }

        private static string NormalizeCode(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
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
            return tagClass switch {
                NodeEnum.TagClassCode.Component => "bi-bullseye",
                NodeEnum.TagClassCode.Derived => "bi-calculator",
                NodeEnum.TagClassCode.Rollup => "bi-collection",
                _ => "bi-layers"
            };
        }
    }
}
