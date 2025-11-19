using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize]
    public class DeleteTotalModel : DI_BasePageModel
    {
        public DeleteTotalModel(NodeContext nodeContext) : base(nodeContext) { }

        [BindProperty(SupportsGet = true)]
        public string ParentKey { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ChildKey { get; set; }

        // Friendly display names
        public string ParentDisplay { get; private set; } = string.Empty;
        public string ChildDisplay { get; private set; } = string.Empty;

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(ParentKey))
            {
                var p = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == ParentKey);
                ParentDisplay = p != null ? p.Category : ParentKey;
            }

            if (!string.IsNullOrWhiteSpace(ChildKey))
            {
                var c = await NodeContext.Cash_tbCategories
                    .FirstOrDefaultAsync(x => x.CategoryCode == ChildKey);
                ChildDisplay = c != null ? c.Category : ChildKey;
            }
        }

    }
}
