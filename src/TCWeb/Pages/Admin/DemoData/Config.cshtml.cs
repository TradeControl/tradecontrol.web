using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.AppServices.Execution;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Admin.DemoData
{
    [Authorize(Roles = "Administrators")]
    public class ConfigModel : DI_BasePageModel
    {
        [BindProperty]
        public DemoDataRequest DemoData { get; set; }

        public SelectList TemplateNames { get; set; }

        public Dictionary<string, string> TemplateDescriptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, bool> TemplateVatDefaults { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, List<DemoTemplateDataset>> TemplateDatasets { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        IExecutionQueue ExecutionQueue { get; }

        UserManager<TradeControlWebUser> UserManager { get; }

        public ConfigModel(NodeContext context, IExecutionQueue executionQueue, UserManager<TradeControlWebUser> userManager) : base(context)
        {
            ExecutionQueue = executionQueue;
            UserManager = userManager;
        }

        async Task LoadLookupsAsync()
        {
            var templates = await NodeContext.App_tbTemplates
                .OrderBy(t => t.TemplateName)
                .Select(t => new {
                    t.TemplateCode,
                    t.TemplateName,
                    t.TemplateDescription,
                    t.IsVatRegistered
                })
                .ToListAsync();

            var datasets = await (from d in NodeContext.App_tbTemplateDatasets
                                  join t in NodeContext.App_tbTemplates
                                      on d.TemplateCode equals t.TemplateCode
                                  orderby t.TemplateName, d.DatasetTitle
                                  select new {
                                      t.TemplateName,
                                      d.DatasetCode,
                                      d.DatasetTitle,
                                      d.Notes,
                                      d.IsVatRegistered
                                  })
                                  .ToListAsync();

            TemplateNames = new SelectList(templates.Select(t => t.TemplateName).ToList());

            TemplateDescriptions = templates
                .ToDictionary(t => t.TemplateName, t => t.TemplateDescription ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            TemplateVatDefaults = templates
                .ToDictionary(t => t.TemplateName, t => t.IsVatRegistered, StringComparer.OrdinalIgnoreCase);

            TemplateDatasets = templates
                .ToDictionary(t => t.TemplateName, _ => new List<DemoTemplateDataset>(), StringComparer.OrdinalIgnoreCase);

            foreach (var dataset in datasets)
            {
                if (!TemplateDatasets.TryGetValue(dataset.TemplateName, out var items))
                {
                    items = new List<DemoTemplateDataset>();
                    TemplateDatasets[dataset.TemplateName] = items;
                }

                items.Add(new DemoTemplateDataset {
                    DatasetCode = dataset.DatasetCode,
                    DatasetTitle = dataset.DatasetTitle,
                    Notes = dataset.Notes ?? string.Empty,
                    IsVatRegistered = dataset.IsVatRegistered
                });
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadLookupsAsync();

                var defaultTemplate = TemplateNames.FirstOrDefault()?.Text ?? string.Empty;

                DemoData = new DemoDataRequest {
                    TemplateName = defaultTemplate,
                    IsVatRegistered = TemplateVatDefaults.TryGetValue(defaultTemplate, out var isVat) && isVat,
                    DatasetCode = string.Empty
                };

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
            await LoadLookupsAsync();

            if (string.IsNullOrWhiteSpace(DemoData.DatasetCode))
            {
                ModelState.AddModelError("DemoData.DatasetCode", "Select a synthetic dataset scenario.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var dataset = await (from d in NodeContext.App_tbTemplateDatasets
                                 join t in NodeContext.App_tbTemplates
                                     on d.TemplateCode equals t.TemplateCode
                                 where t.TemplateName == DemoData.TemplateName
                                    && d.DatasetCode == DemoData.DatasetCode
                                 select d)
                                 .SingleOrDefaultAsync();

            if (dataset == null)
            {
                ModelState.AddModelError("DemoData.DatasetCode", "The selected synthetic dataset is not valid for this template.");
                return Page();
            }

            if (dataset.IsVatRegistered.HasValue && dataset.IsVatRegistered.Value != DemoData.IsVatRegistered)
            {
                ModelState.AddModelError("DemoData.DatasetCode", "The selected synthetic dataset does not match the current VAT setting.");
                return Page();
            }

            var executionArguments = new SyntheticDatasetExecutionArguments {
                IsCompany = dataset.IsCompany,
                UseStdCompanyTemplate = dataset.UseStdCompanyTemplate,
                IsVatRegistered = dataset.IsVatRegistered,
                MisOrdersPerMonth = dataset.MisOrdersPerMonth,
                MonthsForward = dataset.MonthsForward,
                PriceRatio = dataset.PriceRatio,
                QuantityRatio = dataset.QuantityRatio,
                FloatRatio = dataset.FloatRatio
            };

            string queuedBy = null;
            var user = await UserManager.GetUserAsync(User);

            if (user != null)
            {
                queuedBy = await NodeContext.GetUserId(user.Id);

                if (string.IsNullOrWhiteSpace(queuedBy))
                    queuedBy = null;
            }

            var executionCode = await ExecutionQueue.EnqueueAsync(
                NodeEnum.ExecutionType.SyntheticDataset,
                JsonSerializer.Serialize(executionArguments),
                queuedBy);

            return RedirectToPage("./Status", new {
                executionCode,
                embedded = Request?.Query.ContainsKey("embedded") == true ? Request.Query["embedded"].ToString() : null,
                returnNode = Request?.Query.ContainsKey("returnNode") == true ? Request.Query["returnNode"].ToString() : null
            });
        }
    }

    public class DemoTemplateDataset
    {
        public string DatasetCode { get; set; }

        public string DatasetTitle { get; set; }

        public string Notes { get; set; }

        public bool? IsVatRegistered { get; set; }
    }

    [Keyless]
    public class DemoDataRequest
    {
        [Required]
        [Display(Name = "Configuration Template")]
        public string TemplateName { get; set; }

        [Display(Name = "VAT Registered")]
        public bool IsVatRegistered { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Synthetic Dataset Scenario")]
        public string DatasetCode { get; set; } = string.Empty;
    }
}
