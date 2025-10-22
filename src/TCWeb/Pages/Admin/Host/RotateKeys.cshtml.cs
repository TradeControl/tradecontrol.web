using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Admin.Host
{
    [Authorize(Roles = "Administrators")]
    public class RotateKeysModel : DI_BasePageModel
    {
        readonly NodeSettings _nodeSettings;

        public RotateKeysModel(NodeContext context) : base(context)
        {
            _nodeSettings = new NodeSettings(NodeContext);
        }

        [BindProperty]
        public bool ConfirmBackup { get; set; }

        public bool Success { get; set; }
        public string ResultMessage { get; set; }

        public void OnGet()
        {
            // show the page
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ConfirmBackup)
            {
                ModelState.AddModelError(nameof(ConfirmBackup), "You must confirm you have backed up the database before rotating keys.");
                return Page();
            }

            // Perform rotation + host password migration
            var ok = await _nodeSettings.RegenerateSymmetricAsync();

            Success = ok;
            ResultMessage = ok ? "Rotation completed successfully. Host passwords re-encrypted where possible."
                               : "Rotation failed. Check server logs for details and DO NOT change further settings.";

            return Page();
        }
    }
}
