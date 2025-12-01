using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Type
{
    [Authorize(Roles = "Administrators")]
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(NodeContext context) : base(context) { }

        [BindProperty]
        public Subject_tbType Subject_tbType { get; set; }

        [BindProperty]
        public string CashPolarity { get; set; }
        public SelectList CashPolaritys { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var orgTypeCode = (await NodeContext.Subject_tbTypes.MaxAsync(t => t.SubjectTypeCode)) + 1;

                Subject_tbType = new()
                {
                    SubjectTypeCode = (short)orgTypeCode,
                    CashPolarityCode = (short)NodeEnum.CashPolarity.Income
                };

                var modes = NodeContext.Cash_tbPolaritys.OrderBy(m => m.CashPolarityCode).Select(m => m.CashPolarity);
                CashPolaritys = new SelectList(await modes.ToListAsync());
                CashPolarity = await NodeContext.Cash_tbPolaritys
                                            .Where(t => t.CashPolarityCode == Subject_tbType.CashPolarityCode)
                                            .Select(t => t.CashPolarity).FirstAsync();
                await SetViewData();
                return Page();
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                Subject_tbType.CashPolarityCode = await NodeContext.Cash_tbPolaritys.Where(t => t.CashPolarity == CashPolarity).Select(t => t.CashPolarityCode).FirstAsync();
                NodeContext.Subject_tbTypes.Add(Subject_tbType);
                await NodeContext.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                throw;
            }
        }
    }
}
