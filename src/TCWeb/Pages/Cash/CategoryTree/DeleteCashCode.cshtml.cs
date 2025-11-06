using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    [Authorize]
    public class DeleteCashCodeModel : DI_BasePageModel
    {
        public DeleteCashCodeModel(NodeContext nodeContext) : base(nodeContext) { }

        [BindProperty(SupportsGet = true)]
        public string Key { get; set; } // expected "code:XXXX" or plain code

        public string CashCode { get; private set; } = string.Empty;
        public string CashDescription { get; private set; } = string.Empty;
        public string CategoryCode { get; private set; } = string.Empty;

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Key)) return;

            var codeKey = Key.StartsWith("code:") ? Key.Substring("code:".Length) : Key;
            CashCode = codeKey;

            var code = await NodeContext.Cash_tbCodes
                .FirstOrDefaultAsync(c => c.CashCode == codeKey);

            if (code != null)
            {
                CashDescription = code.CashDescription ?? string.Empty;
                CategoryCode = code.CategoryCode ?? string.Empty;
            }
        }
    }
}