// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Data;

namespace TradeControl.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<TradeControlWebUser> _userManager;
        private readonly SignInManager<TradeControlWebUser> _signInManager;
        private readonly Profile _profile;

        public IndexModel(
            UserManager<TradeControlWebUser> userManager,
            SignInManager<TradeControlWebUser> signInManager,
            NodeContext _nodeContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _profile = new Profile(_nodeContext);
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList ThemeOptions { get; set; }

        public class InputModel
        {
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Theme")]
            public string ThemeCode { get; set; }
        }

        private async Task LoadAsync(TradeControlWebUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var themes = await _profile.GetThemes();
            var themeCode = await _profile.GetUserThemeCode(user.Id);

            Username = userName;

            ThemeOptions = new SelectList(themes, "ThemeCode", "ThemeName", themeCode == "default" ? string.Empty : themeCode);

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                ThemeCode = themeCode == "default" ? string.Empty : themeCode
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            var selectedThemeCode = string.IsNullOrWhiteSpace(Input.ThemeCode) ? null : Input.ThemeCode;
            var currentThemeCode = await _profile.GetUserThemeCode(user.Id);
            var normalisedCurrentThemeCode = currentThemeCode == "default" ? null : currentThemeCode;

            if (selectedThemeCode != normalisedCurrentThemeCode)
            {
                var validThemeCodes = (await _profile.GetThemes())
                    .Select(t => t.ThemeCode)
                    .ToHashSet();

                if (selectedThemeCode != null && !validThemeCodes.Contains(selectedThemeCode))
                {
                    ModelState.AddModelError("Input.ThemeCode", "The selected theme is invalid.");
                    await LoadAsync(user);
                    return Page();
                }

                var updateThemeResult = await _profile.UpdateUserThemeCode(user.Id, selectedThemeCode);
                if (!updateThemeResult)
                {
                    StatusMessage = "Unexpected error when trying to update theme.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
