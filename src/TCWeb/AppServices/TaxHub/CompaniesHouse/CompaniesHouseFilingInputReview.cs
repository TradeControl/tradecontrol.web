using System;
using System.Collections.Generic;
using System.Linq;
using TradeControl.Web.Pages.Tax.Hub.Models;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

public static class CompaniesHouseFilingInputReview
{
    private const decimal AmountLimit = 999_999_999_999_999m;
    private const int RepeatingDisclosureLimit = 100;

    public static IReadOnlyList<string> Validate(TaxHubCompaniesHouseFilingInputDraft draft,
        DateOnly reviewDate)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var errors = new List<string>();

        if (!draft.ApprovedOn.HasValue)
            errors.Add("Enter the date on which the accounts were approved.");
        else
        {
            var approved = DateOnly.FromDateTime(draft.ApprovedOn.Value);
            if (approved < draft.PeriodEnd)
                errors.Add("The approval date cannot precede the accounting period end.");
            if (approved > reviewDate)
                errors.Add("The approval date cannot be in the future.");
        }

        Required(draft.SigningDirectorName, 160, "signing director");
        Required(draft.PrincipalActivity, 500, "principal activity");
        Required(draft.AccountingPolicies, 4_000, "accounting policies");
        if (draft.AverageEmployees is < 0 or > 1_000_000)
            errors.Add("Average employees must be between 0 and 1,000,000.");
        if (!draft.ConfirmsNoMaterialCommitmentsOrContingencies.HasValue)
            errors.Add("Confirm whether the company has commitments or contingencies requiring disclosure.");
        else if (!draft.ConfirmsNoMaterialCommitmentsOrContingencies.Value)
            errors.Add("Commitments or contingencies requiring disclosure are outside the first-release Accounts Mode filing scope.");

        if (draft.DirectorAdvances.Count > RepeatingDisclosureLimit)
            errors.Add($"No more than {RepeatingDisclosureLimit} director-advance disclosures are permitted.");
        for (var index = 0; index < draft.DirectorAdvances.Count; index++)
        {
            var item = draft.DirectorAdvances[index];
            var label = $"Director advance {index + 1}";
            Required(item.DirectorName, 160, $"{label} director name");
            Required(item.Terms, 1_000, $"{label} terms");
            Amount(item.OpeningBalance, $"{label} opening balance");
            Amount(item.Advances, $"{label} advances");
            Amount(item.Repayments, $"{label} repayments");
            Amount(item.ClosingBalance, $"{label} closing balance");
            if (Math.Abs(item.OpeningBalance + item.Advances - item.Repayments - item.ClosingBalance) > 0.01m)
                errors.Add($"{label} does not reconcile: opening plus advances less repayments must equal closing.");
        }

        return errors;

        void Amount(decimal value, string label)
        {
            if (Math.Abs(value) > AmountLimit)
                errors.Add($"{label} exceeds the supported monetary range.");
        }

        void Required(string? value, int maximumLength, string label)
        {
            if (string.IsNullOrWhiteSpace(value))
                errors.Add($"Enter the {label}.");
            else if (value.Length > maximumLength)
                errors.Add($"The {label} exceeds {maximumLength} characters.");
            else if (value.Any(char.IsControl))
                errors.Add($"The {label} contains unsupported control characters.");
        }
    }
}
