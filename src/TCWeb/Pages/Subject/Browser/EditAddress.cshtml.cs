using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Pages.Subject.Browser
{
    public class EditAddressModel : DI_BasePageModel
    {
        public EditAddressModel(NodeContext context) : base(context) { }

        [BindProperty(SupportsGet = true)]
        public string SubjectCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string AddressCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ReturnNode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string IsEmbedded { get; set; } = "0";

        [BindProperty(SupportsGet = true)]
        public string Done { get; set; } = string.Empty;

        [BindProperty]
        public string Address { get; set; } = string.Empty;

        public Subject_tbAddress? CurrentAddress { get; private set; }

        public bool IsEditMode => !string.IsNullOrWhiteSpace(AddressCode);

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(SubjectCode))
                return RedirectToPage("/Subject/Browser/Index");

            if (IsEmbedded == "1" && !string.IsNullOrWhiteSpace(Done))
            {
                await SetViewData();
                return Page();
            }

            if (IsEditMode)
            {
                var subjects = new Subjects(NodeContext, SubjectCode);
                CurrentAddress = await subjects.GetAddressAsync(AddressCode);

                if (CurrentAddress is null)
                    return NotFound();

                Address = CurrentAddress.Address ?? string.Empty;
            }

            await SetViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(SubjectCode))
                return RedirectToPage("/Subject/Browser/Index");

            if (string.IsNullOrWhiteSpace(Address))
            {
                ModelState.AddModelError(nameof(Address), "Address is required.");
                await SetViewData();
                return Page();
            }

            try
            {
                var subjects = new Subjects(NodeContext, SubjectCode);
                SubjectActionResult result;

                if (IsEditMode)
                {
                    result = await subjects.UpdateAddressAsync(AddressCode, Address.Trim());
                }
                else
                {
                    await subjects.AddAddressAsync(Address.Trim());
                    result = SubjectActionResult.Success("Address added.");
                }

                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    await SetViewData();
                    return Page();
                }

                if (IsEmbedded == "1")
                {
                    return RedirectToPage("/Subject/Browser/EditAddress", new
                    {
                        subjectCode = SubjectCode,
                        returnNode = ReturnNode,
                        isEmbedded = "1",
                        done = "save"
                    });
                }

                return RedirectToPage("/Subject/Browser/Index", new
                {
                    mode = "Subject",
                    select = string.IsNullOrWhiteSpace(ReturnNode) ? SubjectCode : ReturnNode
                });
            }
            catch (Exception e)
            {
                await NodeContext.ErrorLog(e);
                ModelState.AddModelError(string.Empty, "Unable to save the address.");
                await SetViewData();
                return Page();
            }
        }
    }
}
