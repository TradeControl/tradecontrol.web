using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCExports.Generator.CashStatement;
using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize]
    public class TreeDetailsModel : DI_BasePageModel
    {
        private ICategoryTreeChangeControlService CategoryTreeService { get; }

        public TreeDetailsModel(NodeContext context, ICategoryTreeChangeControlService categoryTreeService)
            : base(context)
        {
            CategoryTreeService = categoryTreeService;
        }

        public string NodeType { get; private set; } = "";
        public CategoryDetailsVm Category { get; private set; }
        public CashCodeDetailsVm Code { get; private set; }
        public ExpressionDetailsVm Expression { get; set; }

        public CashCodeListByPolarityVm CashCodes { get; private set; }
        public CategoryTreeWorkspaceContext Workspace { get; private set; }
        public CategoryTreeVersionWorkspaceVm VersionWorkspace { get; private set; }

        public async Task<IActionResult> OnGetAsync(string key, string parentKey = null,
            string versionId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return NotFound();
                }

                Workspace = await CategoryTreeService.GetWorkspaceAsync(versionId,
                    HttpContext.RequestAborted);
                var workspaceVersionId = Workspace.SelectedVersion.VersionId;

                var helper = new CashCodes(NodeContext);

                if (string.Equals(key, CategoryTreeModel.RootNodeKey, StringComparison.Ordinal))
                {
                    NodeType = "versionWorkspace";
                    VersionWorkspace = await BuildVersionWorkspaceAsync();
                    await SetViewData();
                    return Page();
                }

                var isSyntheticCashCodeListKey =
                    string.Equals(key, CategoryTreeModel.DisconnectedNodeKey, StringComparison.Ordinal)
                    || string.Equals(key, "__TYPE_ROOT__", StringComparison.Ordinal);

                // Exclude Expressions synthetic root
                if (isSyntheticCashCodeListKey)
                {
                    NodeType = "cashCodeList";

                    var grouped = await (from map in NodeContext.Cash_tbCategoryCodeMaps.AsNoTracking()
                                         join code in NodeContext.Cash_tbCodes.AsNoTracking()
                                             on map.CashCode equals code.CashCode
                                         join category in NodeContext.Cash_tbCategories.AsNoTracking()
                                             on new { map.VersionId, map.CategoryCode }
                                             equals new { category.VersionId, category.CategoryCode }
                                         where map.VersionId == workspaceVersionId
                                         orderby code.CashCode
                                         select new
                                         {
                                             code.CashCode,
                                             code.CashDescription,
                                             category.Category,
                                             category.CashPolarityCode,
                                             IsCashEnabled = code.IsEnabled != 0,
                                             IsCategoryEnabled = category.IsEnabled != 0
                                         })
                        .ToListAsync();

                    CashCodes = new CashCodeListByPolarityVm {
                        Expense = grouped
                            .Where(x => x.CashPolarityCode == (short)NodeEnum.CashPolarity.Expense)
                            .Select(x => new CashCodeListItemVm {
                                CashCode = x.CashCode,
                                CashDescription = x.CashDescription,
                                Category = x.Category,
                                IsCashEnabled = x.IsCashEnabled,
                                IsCategoryEnabled = x.IsCategoryEnabled
                            }).ToList(),

                        Income = grouped
                            .Where(x => x.CashPolarityCode == (short)NodeEnum.CashPolarity.Income)
                            .Select(x => new CashCodeListItemVm {
                                CashCode = x.CashCode,
                                CashDescription = x.CashDescription,
                                Category = x.Category,
                                IsCashEnabled = x.IsCashEnabled,
                                IsCategoryEnabled = x.IsCategoryEnabled
                            }).ToList(),

                        Neutral = grouped
                            .Where(x => x.CashPolarityCode == (short)NodeEnum.CashPolarity.Neutral)
                            .Select(x => new CashCodeListItemVm {
                                CashCode = x.CashCode,
                                CashDescription = x.CashDescription,
                                Category = x.Category,
                                IsCashEnabled = x.IsCashEnabled,
                                IsCategoryEnabled = x.IsCategoryEnabled
                            }).ToList()
                    };

                    await SetViewData();
                    return Page();
                }

                if (key.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
                {
                    NodeType = "code";
                    var cashCode = key.Substring("code:".Length);

                    var vm = await (from map in NodeContext.Cash_tbCategoryCodeMaps
                                    join code in NodeContext.Cash_tbCodes on map.CashCode equals code.CashCode
                                    join cat in NodeContext.Cash_tbCategories
                                        on new { map.VersionId, map.CategoryCode }
                                        equals new { cat.VersionId, cat.CategoryCode }
                                    join pol in NodeContext.Cash_tbPolaritys on cat.CashPolarityCode equals pol.CashPolarityCode
                                    join typ in NodeContext.Cash_tbTypes on cat.CashTypeCode equals typ.CashTypeCode
                                    where map.VersionId == workspaceVersionId && code.CashCode == cashCode
                                    select new CashCodeDetailsVm {
                                        CashCode = code.CashCode,
                                        CashDescription = code.CashDescription,
                                        CategoryCode = cat.CategoryCode,
                                        Category = cat.Category,
                                        CashPolarity = pol.CashPolarity,
                                        CashType = typ.CashType,
                                        TaxCode = code.TaxCode,
                                        IsEnabled = code.IsEnabled != 0,
                                        IsCategoryEnabled = cat.IsEnabled != 0
                                    }).FirstOrDefaultAsync();

                    if (vm == null)
                    {
                        return NotFound();
                    }

                    vm.VersionId = workspaceVersionId;
                    vm.Namespace = await helper.GetCategoryNamespace(workspaceVersionId,
                        vm.CategoryCode, parentKey);
                    vm.CanEditMaster = Workspace.SelectedVersion.Capabilities.CanEditTree
                        && Workspace.SelectedVersion.Status == NodeEnum.CategoryVersionStatus.Published;

                    Code = vm;
                }
                else if (CategoryTreeModel.IsExpressionKey(key))
                {
                    var categoryCode = key.Substring(CategoryTreeModel.ExpressionKeyPrefix.Length);

                    // Anchor the details lookup on the category because the tree deliberately
                    // includes Expression categories even when their optional detail row is absent.
                    var row = await (from c in NodeContext.Cash_tbCategories.AsNoTracking()
                                     join e in NodeContext.Cash_tbCategoryExps.AsNoTracking()
                                         on c.CategoryCode equals e.CategoryCode into expressionRows
                                     from e in expressionRows.DefaultIfEmpty()
                                     join s in NodeContext.Cash_tbCategoryExpSyntax.AsNoTracking()
                                         on e.SyntaxTypeCode equals s.SyntaxTypeCode into syntaxRows
                                     from s in syntaxRows.DefaultIfEmpty()
                                     where c.VersionId == workspaceVersionId
                                           && c.CategoryCode == categoryCode
                                           && c.CategoryTypeCode == (short)NodeEnum.CategoryType.Expression
                                     select new {
                                         c.CategoryCode,
                                         c.Category,
                                         ExpressionRow = e,
                                         SyntaxType = s == null ? null : s.SyntaxType
                                     }).FirstOrDefaultAsync();

                    if (row == null)
                        return NotFound();

                    // Build the view model after materialisation. In particular, ErrorMessage is
                    // nullable in SQL; projecting it through a CASE expression made SqlClient call
                    // GetString on NULL before C# could supply an empty value.
                    var exp = new ExpressionDetailsVm {
                        VersionId = workspaceVersionId,
                        CategoryCode = row.CategoryCode,
                        Category = row.Category,
                        Expression = row.ExpressionRow?.Expression ?? string.Empty,
                        Format = row.ExpressionRow?.Format ?? string.Empty,
                        IsError = row.ExpressionRow?.IsError ?? false,
                        ErrorMessage = row.ExpressionRow?.ErrorMessage ?? string.Empty,
                        SyntaxType = row.SyntaxType ?? "Not specified"
                    };

                    if (!string.IsNullOrWhiteSpace(exp.Expression))
                    {
                        var displayTranslation = CashStatementExpressionTranslator.ForDisplay(
                            exp.Expression,
                            await LoadExpressionReferencesAsync(workspaceVersionId));
                        exp.Expression = displayTranslation.Expression;
                    }

                    NodeType = "expression";
                    exp.CanEditCategory = Workspace.SelectedVersion.Capabilities.CanEditTree;
                    exp.CanManageMetadata = User.IsInRole(
                        TradeControl.Web.Authorization.Constants.AdministratorsRole);
                    exp.ContextNotice = Workspace.SelectedVersion.IsActive
                        ? "Expression text is current BI metadata and is not versioned with the tree."
                        : $"Expression text is current BI metadata parsed against {workspaceVersionId}; it is not historical content from this tree version.";
                    Expression = exp;
                }
                else
                {
                    NodeType = "category";

                    var vm = await (from c in NodeContext.Cash_tbCategories
                                    join p in NodeContext.Cash_tbPolaritys on c.CashPolarityCode equals p.CashPolarityCode
                                    join t in NodeContext.Cash_tbTypes on c.CashTypeCode equals t.CashTypeCode
                                    join ct in NodeContext.Cash_tbCategoryTypes on c.CategoryTypeCode equals ct.CategoryTypeCode
                                    where c.VersionId == workspaceVersionId && c.CategoryCode == key
                                    select new CategoryDetailsVm {
                                        CategoryCode = c.CategoryCode,
                                        Category = c.Category,
                                        CategoryType = ct.CategoryType,
                                        CashType = t.CashType,
                                        CashPolarity = p.CashPolarity,
                                        DisplayOrder = c.DisplayOrder,
                                        IsEnabled = c.IsEnabled != 0
                                    }).FirstOrDefaultAsync();

                    if (vm == null)
                    {
                        return NotFound();
                    }

                    vm.VersionId = workspaceVersionId;
                    vm.CanEditTree = Workspace.SelectedVersion.Capabilities.CanEditTree;
                    vm.CanCreateCashCode = Workspace.SelectedVersion.Capabilities.CanEditTree
                        && Workspace.SelectedVersion.Status == NodeEnum.CategoryVersionStatus.Published;

                    vm.ChildTotalsCount = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.VersionId == workspaceVersionId && t.ParentCode == vm.CategoryCode)
                        .CountAsync();
                    vm.CodesCount = await NodeContext.Cash_tbCategoryCodeMaps
                        .Where(map => map.VersionId == workspaceVersionId
                            && map.CategoryCode == vm.CategoryCode)
                        .CountAsync();
                    vm.ParentCount = await NodeContext.Cash_tbCategoryTotals
                        .Where(t => t.VersionId == workspaceVersionId && t.ChildCode == vm.CategoryCode)
                        .CountAsync();

                    vm.IsRootNode = vm.ParentCount == 0;

                    var isSyntheticRootCtx = string.Equals(parentKey, CategoryTreeModel.RootNodeKey, StringComparison.Ordinal);
                    var isDisconnectedCtx = string.Equals(parentKey, CategoryTreeModel.DisconnectedNodeKey, StringComparison.Ordinal);
                    var effectiveParentKey = (string.IsNullOrEmpty(parentKey) || isSyntheticRootCtx || isDisconnectedCtx) ? null : parentKey;

                    vm.IsRootContext = (effectiveParentKey == null) && vm.IsRootNode;

                    var roots = await NodeContext.Cash_tbCategoryRoots
                        .Where(root => root.VersionId == workspaceVersionId
                            && root.CategoryCode == vm.CategoryCode)
                        .Select(root => root.RootTypeCode)
                        .ToListAsync();
                    vm.IsProfitRoot = roots.Contains(0);
                    vm.IsVatRoot = roots.Contains(1);

                    var presentOnPrimary = Workspace.SelectedVersion.IsActive
                        && await NodeContext.Cash_vwCategoryPrimaryParents
                            .AnyAsync(v => v.ChildCode == vm.CategoryCode);

                    if (effectiveParentKey == null)
                    {
                        vm.PrimaryKind = vm.IsProfitRoot ? "Profit" : vm.IsVatRoot ? "VAT" : "";
                        vm.IsCategoryInPrimary = presentOnPrimary || !string.IsNullOrEmpty(vm.PrimaryKind);
                        vm.IsContextInPrimary = vm.IsCategoryInPrimary;
                    }
                    else
                    {
                        vm.IsCategoryInPrimary = presentOnPrimary;
                        vm.IsContextInPrimary = Workspace.SelectedVersion.IsActive
                            && await NodeContext.Cash_vwCategoryPrimaryParents
                            .AnyAsync(v => v.ChildCode == vm.CategoryCode && v.ParentCode == effectiveParentKey);

                        vm.PrimaryKind = Workspace.SelectedVersion.IsActive
                            ? await NodeContext.Cash_vwCategoryPrimaryParents
                                .Where(v => v.ChildCode == vm.CategoryCode && v.ParentCode == effectiveParentKey)
                                .Select(v => v.PrimaryKind)
                                .FirstOrDefaultAsync() ?? ""
                            : "";
                    }

                    vm.PrimaryParentCount = Workspace.SelectedVersion.IsActive
                        ? await NodeContext.Cash_vwCategoryPrimaryParents
                            .Where(v => v.ChildCode == vm.CategoryCode)
                            .CountAsync()
                        : 0;

                    vm.Namespace = await helper.GetCategoryNamespace(workspaceVersionId,
                        vm.CategoryCode, parentKey);

                    bool isDisconnectedContext = string.Equals(parentKey, CategoryTreeModel.DisconnectedNodeKey, StringComparison.Ordinal);

                    bool isCashCodeCategory =
                        vm.CodesCount > 0 ||
                        (!string.IsNullOrEmpty(vm.CashType) && vm.CashType.Contains("Code", StringComparison.OrdinalIgnoreCase));

                    vm.IsDisconnectedContext = isDisconnectedContext;
                    vm.IsCashCodeCategory = isCashCodeCategory;

                    Category = vm;
                }

                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        private async Task<IReadOnlyList<CashStatementCategoryReference>> LoadExpressionReferencesAsync(
            string versionId)
        {
            var categories = await NodeContext.Cash_tbCategories
                .AsNoTracking()
                .Where(category => category.VersionId == versionId)
                .Select(category => new CashStatementCategoryReference(
                    category.CategoryCode,
                    category.Category,
                    CashStatementReferenceKind.Category))
                .ToListAsync();
            var cashCodes = await (from map in NodeContext.Cash_tbCategoryCodeMaps.AsNoTracking()
                                   join code in NodeContext.Cash_tbCodes.AsNoTracking()
                                       on map.CashCode equals code.CashCode
                                   where map.VersionId == versionId
                                   select new CashStatementCategoryReference(
                    code.CashCode,
                    code.CashDescription,
                    CashStatementReferenceKind.CashCode))
                .ToListAsync();
            return categories.Concat(cashCodes).ToList();
        }

        private async Task<CategoryTreeVersionWorkspaceVm> BuildVersionWorkspaceAsync()
        {
            var snapshots = Workspace.Versions.ToDictionary(version => version.VersionId,
                _ => new CategoryTreeVersionSnapshotVm(), StringComparer.Ordinal);

            var categoryCounts = await NodeContext.Cash_tbCategories
                .AsNoTracking()
                .GroupBy(category => category.VersionId)
                .Select(group => new { VersionId = group.Key, Count = group.Count() })
                .ToListAsync();
            foreach (var count in categoryCounts)
            {
                if (snapshots.TryGetValue(count.VersionId, out var snapshot))
                    snapshot.CategoryCount = count.Count;
            }

            var edgeCounts = await NodeContext.Cash_tbCategoryTotals
                .AsNoTracking()
                .GroupBy(total => total.VersionId)
                .Select(group => new { VersionId = group.Key, Count = group.Count() })
                .ToListAsync();
            foreach (var count in edgeCounts)
            {
                if (snapshots.TryGetValue(count.VersionId, out var snapshot))
                    snapshot.EdgeCount = count.Count;
            }

            var cashCodeCounts = await NodeContext.Cash_tbCategoryCodeMaps
                .AsNoTracking()
                .GroupBy(map => map.VersionId)
                .Select(group => new { VersionId = group.Key, Count = group.Count() })
                .ToListAsync();
            foreach (var count in cashCodeCounts)
            {
                if (snapshots.TryGetValue(count.VersionId, out var snapshot))
                    snapshot.CashCodeCount = count.Count;
            }

            var roots = await (from root in NodeContext.Cash_tbCategoryRoots.AsNoTracking()
                               join category in NodeContext.Cash_tbCategories.AsNoTracking()
                                   on new { root.VersionId, root.CategoryCode }
                                   equals new { category.VersionId, category.CategoryCode }
                               select new
                               {
                                   root.VersionId,
                                   root.RootTypeCode,
                                   Name = category.Category + " (" + category.CategoryCode + ")"
                               }).ToListAsync();
            foreach (var root in roots)
            {
                if (!snapshots.TryGetValue(root.VersionId, out var snapshot))
                    continue;
                if (root.RootTypeCode == (short)NodeEnum.CategoryRootType.NetProfit)
                    snapshot.ProfitRoot = root.Name;
                else if (root.RootTypeCode == (short)NodeEnum.CategoryRootType.Vat)
                    snapshot.VatRoot = root.Name;
            }

            return new CategoryTreeVersionWorkspaceVm
            {
                Workspace = Workspace,
                Review = await CategoryTreeService.GetReviewAsync(
                    Workspace.SelectedVersion.VersionId, HttpContext.RequestAborted),
                Snapshots = snapshots
            };
        }
    }
}
