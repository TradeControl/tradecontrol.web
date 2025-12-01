using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize]
    public class DeleteCategoryModel : DI_BasePageModel
    {
        public DeleteCategoryModel(NodeContext nodeContext) : base(nodeContext) { }

        [BindProperty(SupportsGet = true)]
        public string Key { get; set; }

        public string CategoryDisplay { get; private set; } = string.Empty;

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                var cat = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == Key);
                CategoryDisplay = cat != null ? cat.Category : Key;
            }
        }
    }
}