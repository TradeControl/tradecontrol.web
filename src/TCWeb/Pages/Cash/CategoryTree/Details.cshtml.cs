using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeControl.Web.Data;
using TradeControl.Web.Pages.Cash.CategoryCode; // for CategoryTreeModel.Root/Disconnected keys

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize]
    public class TreeDetailsModel : DI_BasePageModel
    {
        public TreeDetailsModel(NodeContext context) : base(context)
        {
        }

        public string NodeType { get; private set; } = "";
        public CategoryDetailsVm Category { get; private set; }
        public CashCodeDetailsVm Code { get; private set; }
        public ExpressionDetailsVm Expression { get; set; }

        public CashCodeListByPolarityVm CashCodes { get; private set; }

        public async Task<IActionResult> OnGetAsync(string key, string parentKey = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return NotFound();
                }

                var helper = new CashCodes(NodeContext);

                var isSyntheticCashCodeListKey =
                    string.Equals(key, CategoryTreeModel.RootNodeKey, StringComparison.Ordinal)
                    || string.Equals(key, CategoryTreeModel.DisconnectedNodeKey, StringComparison.Ordinal)
                    || string.Equals(key, "__TYPE_ROOT__", StringComparison.Ordinal);

                // Exclude Expressions synthetic root
                if (isSyntheticCashCodeListKey)
                {
                    NodeType = "cashCodeList";

                    var grouped = await NodeContext.Cash_Codes
                        .OrderBy(t => t.CashCode)
                        .Select(t => new {
                            t.CashCode,
                            t.CashDescription,
                            t.Category,
                            t.CashPolarityCode,
                            t.IsCashEnabled,
                            t.IsCategoryEnabled
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

                    var vm = await (from code in NodeContext.Cash_tbCodes
                                    join cat in NodeContext.Cash_tbCategories on code.CategoryCode equals cat.CategoryCode
                                    join pol in NodeContext.Cash_tbPolaritys on cat.CashPolarityCode equals pol.CashPolarityCode
                                    join typ in NodeContext.Cash_tbTypes on cat.CashTypeCode equals typ.CashTypeCode
                                    where code.CashCode == cashCode
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

                    vm.Namespace = await helper.GetCategoryNamespace(vm.CategoryCode, parentKey);

                    Code = vm;
                }
                else if (CategoryTreeModel.IsExpressionKey(key))
                {
                    var categoryCode = key.Substring(CategoryTreeModel.ExpressionKeyPrefix.Length);

                    var exp = await (from e in NodeContext.Cash_tbCategoryExps
                                     join c in NodeContext.Cash_tbCategories on e.CategoryCode equals c.CategoryCode
                                     join s in NodeContext.Cash_tbCategoryExpSyntax on e.SyntaxTypeCode equals s.SyntaxTypeCode
                                     where e.CategoryCode == categoryCode
                                     select new ExpressionDetailsVm {
                                         CategoryCode = e.CategoryCode,
                                         Category = c.Category,
                                         Expression = e.Expression,
                                         Format = e.Format,
                                         IsError = e.IsError,
                                         ErrorMessage = e.ErrorMessage,
                                         SyntaxType = s.SyntaxType
                                     }).FirstOrDefaultAsync();

                    if (exp == null)
                        return NotFound();

                    NodeType = "expression";
                    Expression = exp;
                }
                else
                {
                    NodeType = "category";

                    var vm = await (from c in NodeContext.Cash_tbCategories
                                    join p in NodeContext.Cash_tbPolaritys on c.CashPolarityCode equals p.CashPolarityCode
                                    join t in NodeContext.Cash_tbTypes on c.CashTypeCode equals t.CashTypeCode
                                    join ct in NodeContext.Cash_tbCategoryTypes on c.CategoryTypeCode equals ct.CategoryTypeCode
                                    where c.CategoryCode == key
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

                    vm.ChildTotalsCount = await NodeContext.Cash_tbCategoryTotals.Where(t => t.ParentCode == vm.CategoryCode).CountAsync();
                    vm.CodesCount = await NodeContext.Cash_tbCodes.Where(cd => cd.CategoryCode == vm.CategoryCode).CountAsync();
                    vm.ParentCount = await NodeContext.Cash_tbCategoryTotals.Where(t => t.ChildCode == vm.CategoryCode).CountAsync();

                    vm.IsRootNode = vm.ParentCount == 0;

                    var isSyntheticRootCtx = string.Equals(parentKey, CategoryTreeModel.RootNodeKey, StringComparison.Ordinal);
                    var isDisconnectedCtx = string.Equals(parentKey, CategoryTreeModel.DisconnectedNodeKey, StringComparison.Ordinal);
                    var effectiveParentKey = (string.IsNullOrEmpty(parentKey) || isSyntheticRootCtx || isDisconnectedCtx) ? null : parentKey;

                    vm.IsRootContext = (effectiveParentKey == null) && vm.IsRootNode;

                    var options = await NodeContext.App_tbOptions.FirstOrDefaultAsync();
                    vm.IsProfitRoot = options != null && string.Equals(options.NetProfitCode, vm.CategoryCode, StringComparison.Ordinal);
                    vm.IsVatRoot = options != null && string.Equals(options.VatCategoryCode, vm.CategoryCode, StringComparison.Ordinal);

                    var presentOnPrimary = await NodeContext.Cash_vwCategoryPrimaryParents
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
                        vm.IsContextInPrimary = await NodeContext.Cash_vwCategoryPrimaryParents
                            .AnyAsync(v => v.ChildCode == vm.CategoryCode && v.ParentCode == effectiveParentKey);

                        vm.PrimaryKind = await NodeContext.Cash_vwCategoryPrimaryParents
                            .Where(v => v.ChildCode == vm.CategoryCode && v.ParentCode == effectiveParentKey)
                            .Select(v => v.PrimaryKind)
                            .FirstOrDefaultAsync() ?? "";
                    }

                    vm.PrimaryParentCount = await NodeContext.Cash_vwCategoryPrimaryParents
                        .Where(v => v.ChildCode == vm.CategoryCode)
                        .CountAsync();

                    vm.Namespace = await helper.GetCategoryNamespace(vm.CategoryCode, parentKey);

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
    }
}
